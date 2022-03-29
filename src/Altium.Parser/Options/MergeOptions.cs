namespace Altium.Parser.Options;

public class MergeOptions
{
    
    public int FilesPerRun { get; set; } = 10;
    
    public int InputBufferSize { get; set; } = 65536;
    
    public int OutputBufferSize { get; set; } = 65536;
}