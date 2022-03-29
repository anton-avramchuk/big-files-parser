using Altium.Core;
using Altium.Generator.Config;

namespace Altium.Generator.Services.Abstractions;

public interface IFileGenerator
{
    Task GenerateAsync(CharSet charSet,GeneratorConfig generatorConfig,RepeatingConfig repeatingConfig);
}