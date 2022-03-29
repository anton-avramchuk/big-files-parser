using Altium.Parser.Models;

namespace Altium.Parser.Services.Abstractions;

public interface ISplitFileService
{
    Task<SplitResult> Split(string outputLocation, Stream sourceStream, CancellationToken cancellationToken);
}