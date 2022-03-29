namespace Altium.Parser.Services.Abstractions;

public interface ILargeFileSortService
{
    Task Sort(string inputFileName,string outputFileName, CancellationToken cancellationToken);
}