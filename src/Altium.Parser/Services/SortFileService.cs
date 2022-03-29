using System.Diagnostics;
using Altium.Parser.Options;
using Altium.Parser.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altium.Parser.Services;

public class SortFileService : ISortFileService
{
    private readonly ILogger<SortFileService> _logger;
    private readonly SortOptions _options;

    public SortFileService(IOptions<SortOptions> options, ILogger<SortFileService> logger)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value;
    }

    public async Task<string[]> SortFiles(string outputLocation,
        string[] unsortedFiles, int maxUnsortedRows)
    {
        _logger.LogInformation("Start sort:{0}", DateTime.Now);
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var unsortedRows = new string[maxUnsortedRows];
        var sortedFiles = new string[unsortedFiles.Length];
        double totalFiles = unsortedFiles.Length;
        for (var index = 0; index < unsortedFiles.Length; index++)
        {
            var unsortedFile = unsortedFiles[index];
            var sortedFilename =
                unsortedFile.Replace($"{Constants.UnsortedFileExtension}", $"{Constants.SortedFileExtension}");
            var unsortedFilePath = Path.Combine(outputLocation, unsortedFile);
            var sortedFilePath = Path.Combine(outputLocation, sortedFilename);
            _logger.LogInformation("Sort file: {0}", unsortedFile);
            await SortFile(File.OpenRead(unsortedFilePath), File.OpenWrite(sortedFilePath), unsortedRows);
            File.Delete(unsortedFilePath);
            sortedFiles[index] = sortedFilename;
        }

        stopWatch.Stop();
        _logger.LogInformation("Sort finished:{0}, Total ms :{1} ", DateTime.Now, stopWatch.ElapsedMilliseconds);

        return sortedFiles;
    }

    private async Task SortFile(Stream unsortedFile, Stream target, string[] unsortedRows)
    {
        using var streamReader = new StreamReader(unsortedFile, bufferSize: _options.InputBufferSize);
        var counter = 0;
        while (!streamReader.EndOfStream)
        {
            unsortedRows[counter++] = await streamReader.ReadLineAsync();
        }

        Array.Sort(unsortedRows, _options.Comparer);
        await using var streamWriter = new StreamWriter(target, bufferSize: _options.OutputBufferSize);
        foreach (var row in unsortedRows.Where(x => x != null))
        {
            await streamWriter.WriteLineAsync(row);
        }

        Array.Clear(unsortedRows, 0, unsortedRows.Length);
    }
}