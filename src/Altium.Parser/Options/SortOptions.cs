using Altium.Parser.Comparers;

namespace Altium.Parser.Options;

public class SortOptions
{
    public int InputBufferSize { get; set; }
    public int OutputBufferSize { get; set; }

    public IComparer<string> Comparer { get; init; } = Comparer<string>.Default;
}