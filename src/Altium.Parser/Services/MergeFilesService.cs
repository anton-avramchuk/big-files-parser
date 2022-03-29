using System.Diagnostics;
using Altium.Parser.Models;
using Altium.Parser.Options;
using Altium.Parser.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altium.Parser.Services;

public class MergeFilesService: IMergeFilesService
{
    private readonly ILogger<MergeFilesService> _logger;
    private readonly MergeOptions _options;
    private readonly SortOptions _sortOptions;

    public MergeFilesService(ILogger<MergeFilesService> logger, IOptions<MergeOptions> options,IOptions<SortOptions> sortOptions)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (sortOptions == null) throw new ArgumentNullException(nameof(sortOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value;
        _sortOptions = sortOptions.Value;
    }

    public async Task MergeFiles(string path, string[] sortedFiles,string targetFileName , CancellationToken cancellationToken)
    {

        if (File.Exists(targetFileName))
        {
            File.Delete(targetFileName);
        }
        _logger.LogInformation("Merge start at:{0}",DateTime.Now);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var target = File.Create(targetFileName))
        {

            var done = false;
            while (!done)
            {
                var runSize = _options.FilesPerRun;
                var finalRun = sortedFiles.Length <= runSize;

                if (finalRun)
                {
                    await Merge(path, sortedFiles, target, cancellationToken);
                    return;
                }

                // TODO better logic when chunking, we don't want to have 1 chunk of 10 and 1 of 1 for example, better to spread it out.
                var runs = sortedFiles.Chunk(runSize);
                var chunkCounter = 0;
                foreach (var files in runs)
                {
                    var outputFilename =
                        $"{++chunkCounter}{Constants.SortedFileExtension}{Constants.TempFileExtension}";
                    if (files.Length == 1)
                    {
                        File.Move(GetFullPath(path, files.First()),
                            GetFullPath(path, outputFilename.Replace(Constants.TempFileExtension, string.Empty)));
                        continue;
                    }

                    var outputStream = File.OpenWrite(GetFullPath(path, outputFilename));
                    await Merge(path, files, outputStream, cancellationToken);
                    File.Move(GetFullPath(path, outputFilename),
                        GetFullPath(path, outputFilename.Replace(Constants.TempFileExtension, string.Empty)), true);
                }

                sortedFiles = Directory.GetFiles(path, $"*{Constants.SortedFileExtension}")
                    .OrderBy(x =>
                    {
                        var filename = Path.GetFileNameWithoutExtension(x);
                        return int.Parse(filename);
                    })
                    .ToArray();

                if (sortedFiles.Length > 1)
                {
                    continue;
                }

                done = true;
            }
        }
        stopwatch.Stop();
        _logger.LogInformation("Merge finish at:{0}.Total ms {1} ms", DateTime.Now,stopwatch.ElapsedMilliseconds);
    }

    private async Task Merge(
        string path,
        string[] filesToMerge,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        var (streamReaders, rows) = await InitializeStreamReaders(path,filesToMerge);
        var finishedStreamReaders = new List<int>(streamReaders.Length);
        var done = false;
        await using var outputWriter = new StreamWriter(outputStream, bufferSize: _options.OutputBufferSize);

        while (!done)
        {
            rows.Sort((row1, row2) => _sortOptions.Comparer.Compare(row1.Value, row2.Value));
            var valueToWrite = rows[0].Value;
            var streamReaderIndex = rows[0].StreamReader;
            await outputWriter.WriteLineAsync(valueToWrite.AsMemory(), cancellationToken);

            if (streamReaders[streamReaderIndex].EndOfStream)
            {
                var indexToRemove = rows.FindIndex(x => x.StreamReader == streamReaderIndex);
                rows.RemoveAt(indexToRemove);
                finishedStreamReaders.Add(streamReaderIndex);
                done = finishedStreamReaders.Count == streamReaders.Length;
                continue;
            }

            var value = await streamReaders[streamReaderIndex].ReadLineAsync();
            rows[0] = new Row { Value = value, StreamReader = streamReaderIndex };
        }

        CleanupRun(path, streamReaders, filesToMerge);
    }

    private async Task<(StreamReader[] StreamReaders, List<Row> rows)> InitializeStreamReaders(
        string path, IReadOnlyList<string> sortedFiles)
    {
        var streamReaders = new StreamReader[sortedFiles.Count];
        var rows = new List<Row>(sortedFiles.Count);
        for (var i = 0; i < sortedFiles.Count; i++)
        {
            var sortedFilePath = GetFullPath(path, sortedFiles[i]);
            var sortedFileStream = File.OpenRead(sortedFilePath);
            streamReaders[i] = new StreamReader(sortedFileStream, bufferSize: _options.InputBufferSize);
            var value = await streamReaders[i].ReadLineAsync();
            var row = new Row
            {
                Value = value!,
                StreamReader = i
            };
            rows.Add(row);
        }

        return (streamReaders, rows);
    }


    private void CleanupRun(string path, StreamReader[] streamReaders, IReadOnlyList<string> filesToMerge)
    {
        for (var i = 0; i < streamReaders.Length; i++)
        {
            streamReaders[i].Dispose();
            var temporaryFilename = $"{filesToMerge[i]}.removal";
            File.Move(GetFullPath(path, filesToMerge[i]), GetFullPath(path, temporaryFilename));
            File.Delete(GetFullPath(path, temporaryFilename));
        }
    }

    private string GetFullPath(string path, string filename)
    {
        return Path.Combine(path, filename);
    }
}