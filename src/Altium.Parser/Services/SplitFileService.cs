using System.Reflection.Metadata.Ecma335;
using Altium.Parser.Models;
using Altium.Parser.Options;
using Altium.Parser.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altium.Parser.Services;

public class SplitFileService : ISplitFileService
{
    private readonly ILogger<SplitFileService> _logger;
    private readonly SplitOptions _splitOptions;
    public SplitFileService(IOptions<SplitOptions> splitOptions, ILogger<SplitFileService> logger)
    {
        if (splitOptions == null) throw new ArgumentNullException(nameof(splitOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _splitOptions = splitOptions.Value;
    }

    public async Task<SplitResult> Split(string outputLocation, Stream sourceStream, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start split");
        var fileSize = _splitOptions.FileSize;
        var buffer = new byte[fileSize];
        var extraBuffer = new List<byte>();
        var fileNames = new List<string>();
        var newLineSeparator = '\n';
        var maxUnsortedRows = 0;
        await using (sourceStream)
        {
            var currentFile = 0L;
            while (sourceStream.Position < sourceStream.Length)
            {
                var totalRows = 0;
                var runBytesRead = 0;
                while (runBytesRead < fileSize)
                {
                    var value = sourceStream.ReadByte();
                    if (value == -1)
                    {
                        break;
                    }

                    var @byte = (byte)value;
                    buffer[runBytesRead] = @byte;
                    runBytesRead++;
                    if (@byte == newLineSeparator)
                    {
                        // Count amount of rows, used for allocating a large enough array later on when sorting
                        totalRows++;
                    }
                }

                var extraByte = buffer[fileSize - 1];

                while (extraByte != newLineSeparator)
                {
                    var flag = sourceStream.ReadByte();
                    if (flag == -1)
                    {
                        break;
                    }
                    extraByte = (byte)flag;
                    extraBuffer.Add(extraByte);
                }

                var fileName = $"{++currentFile}{Constants.UnsortedFileExtension}";
                await using var unsortedFile = File.Create(Path.Combine(outputLocation, fileName));
                await unsortedFile.WriteAsync(buffer, 0, runBytesRead, cancellationToken);
                if (extraBuffer.Count > 0)
                {
                    await unsortedFile.WriteAsync(extraBuffer.ToArray(), 0, extraBuffer.Count, cancellationToken);
                }

                if (totalRows > maxUnsortedRows)
                {
                    // Used for allocating a large enough array later on when sorting.
                    maxUnsortedRows = totalRows + 1;
                }

                fileNames.Add(fileName);
                _logger.LogInformation("Split file:{0}, totalRows:{1},maxUnsortedRows:{2} ", fileName, totalRows, maxUnsortedRows);
                extraBuffer.Clear();
            }
            _logger.LogInformation("Finish split");
            return new SplitResult(maxUnsortedRows, fileNames);
        }
    }
}