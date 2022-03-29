using System.Diagnostics;
using System.Text;
using Altium.Core;
using Altium.Generator.Config;
using Altium.Generator.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Altium.Generator.Services;

public class FileGenerator : IFileGenerator
{
    private readonly ILogger<FileGenerator> _logger;
    private Random _random = new Random();

    public FileGenerator(ILogger<FileGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task GenerateAsync(CharSet charSet, GeneratorConfig generatorConfig, RepeatingConfig repeatingConfig)
    {
        if (charSet == null) throw new ArgumentNullException(nameof(charSet));
        await GenerateFileAsync(generatorConfig.FileName, generatorConfig.FileSize, generatorConfig.LineSize,
            generatorConfig.MaxNumber, charSet, repeatingConfig.RepeatingElements, repeatingConfig.RepeatingElementsFrequency);
    }

    private async Task GenerateFileAsync(string fileName, long fileSize, int lineSize, int maxNumber, CharSet charSet,
        string[] repeatingElements, int repeatingElementsFrequency)
    {
        _logger.LogInformation("Start at:{0}", DateTime.Now);
        var sw = new Stopwatch();
        sw.Start();
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));
        if (charSet == null) throw new ArgumentNullException(nameof(charSet));
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        using (var stream = File.CreateText(fileName))
        {
            long size = 0;
            var index = 0;
            while (size < fileSize)
            {
                string str;
                if (index != 0 && repeatingElementsFrequency != 0 && repeatingElements != null && index % repeatingElementsFrequency == 0)
                {
                    var elementIndex = _random.Next(0, repeatingElements.Length);
                    str = $"{_random.Next(maxNumber)}.{repeatingElements[elementIndex]}";
                    //_logger.LogInformation("Frequensy string {0}", str);
                }
                else
                {
                    str = GenerateString(maxNumber, lineSize, charSet.AvailableChars);
                }


                size += Encoding.UTF8.GetByteCount(str);
                await stream.WriteLineAsync(str);
                index++;
            }
        }
        sw.Stop();
        _logger.LogInformation("End at:{0}.Total ms: {1}", DateTime.Now, sw.ElapsedMilliseconds);

    }



    private string GenerateString(int maxNumber, int lineSize, char[] chars)
    {
        var number = $"{_random.Next(maxNumber)}.";
        var stringSize = _random.Next(1,lineSize);
        var sb = new StringBuilder(number.Length + stringSize);
        var charSize = chars.Length - 1;
        sb.Append(number);
        for (int i = 0; i < stringSize; i++)
        {
            var charIndex = _random.Next(0, charSize);
            sb.Append(chars[charIndex]);
        }

        return sb.ToString();
    }
}