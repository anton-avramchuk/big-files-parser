namespace Altium.Parser.Services.Abstractions;

public interface ISortFileService
{
    Task<string[]> SortFiles(string outputLocation,
        string[] unsortedFiles,int maxUnsortedRows);
}