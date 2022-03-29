namespace Altium.Parser.Services.Abstractions;

public interface IMergeFilesService
{
    Task MergeFiles(string path, string[] sortedFiles, string targetFileName, CancellationToken cancellationToken);
}