namespace Altium.Parser.Models;

public class SplitResult
{
    public SplitResult(int maxUnsortedRows,IEnumerable<string> files)
    {
        MaxUnsortedRows = maxUnsortedRows;
        Files = files ?? throw new ArgumentNullException(nameof(files));
    }

    public int MaxUnsortedRows { get; }

    public IEnumerable<string> Files { get;  }
}

internal readonly struct Row
{
    public string Value { get; init; }
    public int StreamReader { get; init; }
}