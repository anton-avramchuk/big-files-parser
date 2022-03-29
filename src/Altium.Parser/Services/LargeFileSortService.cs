using System.Diagnostics;
using Altium.Parser.Models;
using Altium.Parser.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Altium.Parser.Services;

public class LargeFileSortService : ILargeFileSortService
{
    private readonly ILogger<LargeFileSortService> _logger;
    private readonly ISplitFileService _splitFileService;
    private readonly ISortFileService _sortFileService;
    private readonly IMergeFilesService _mergeFilesService;

    public LargeFileSortService(ILogger<LargeFileSortService> logger, ISplitFileService splitFileService, ISortFileService sortFileService,IMergeFilesService mergeFilesService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _splitFileService = splitFileService ?? throw new ArgumentNullException(nameof(splitFileService));
        _sortFileService = sortFileService ?? throw new ArgumentNullException(nameof(sortFileService));
        _mergeFilesService = mergeFilesService ?? throw new ArgumentNullException(nameof(mergeFilesService));
    }

    public async Task Sort(string inputFileName, string outputFileName, CancellationToken cancellationToken)
    {
        if (inputFileName == null) throw new ArgumentNullException(nameof(inputFileName));
        if (outputFileName == null) throw new ArgumentNullException(nameof(outputFileName));
        _logger.LogInformation("Start:{0}", DateTime.Now);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        if (File.Exists(inputFileName))
        {
            var path =
                new FileInfo(inputFileName).Directory.FullName;
            var splitResult = await SplitFile(inputFileName,path, cancellationToken);
            var sortedFiles =
                await _sortFileService.SortFiles(path, splitResult.Files.ToArray(), splitResult.MaxUnsortedRows);
            await _mergeFilesService.MergeFiles(path, sortedFiles, outputFileName, cancellationToken);
        }

        stopwatch.Stop();
        _logger.LogInformation("Finish {0}, TotalTime:{1} ms", DateTime.Now, stopwatch.ElapsedMilliseconds);
    }

    private async Task<SplitResult> SplitFile(string inputFileName,string filePath,
        CancellationToken cancellationToken)
    {

        using (FileStream fileStream = File.Open(inputFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (BufferedStream bufferedStream = new BufferedStream(fileStream))
            {
                return await _splitFileService.Split(filePath,
                    bufferedStream,
                    cancellationToken);

            }
        }

        
    }


}