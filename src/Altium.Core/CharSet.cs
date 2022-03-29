namespace Altium.Core;

public class CharSet
{
    public CharSet(string available)
    {
        AvailableChars = available.Select(x => x).OrderBy(x => x).ToArray();
    }

    public char[] AvailableChars { get; }
}