namespace Altium.Parser.Comparers;

public class StringComparer : Comparer<string>,IComparer<string>
{
    public override int Compare(string? x, string? y)
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;

        var xIndex = x.IndexOf(".");
        var xLeft = x.Substring(0, xIndex);
        var xRight= x.Substring(xIndex + 1);

        var yIndex = y.IndexOf(".");
        var yLeft = y.Substring(0, yIndex);
        var yRight = y.Substring(yIndex + 1);

        var defaultComparer = Comparer<string>.Default;
        var result= defaultComparer.Compare(xRight, yRight);
        if (result == 0)
        {
            return defaultComparer.Compare(xLeft, yLeft);
        }

        return result;
    }
}