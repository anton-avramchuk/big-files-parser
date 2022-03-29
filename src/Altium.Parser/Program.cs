using Altium.Core;
using Altium.Parser.Options;
using Altium.Parser.Services;
using Altium.Parser.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altium.Parser;

class Program
{
    static void Main(string[] args)
    {

        MainAsync(args).GetAwaiter().GetResult();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        services.AddOptions<SplitOptions>().Configure(x => config.Bind("split", x));
        services.AddOptions<SortOptions>().Configure(x => config.Bind("sort", x));

        services.AddSingleton(w => config);
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<ILargeFileSortService, LargeFileSortService>();
        services.AddSingleton<ISortFileService, SortFileService>();
        services.AddSingleton<ISplitFileService, SplitFileService>();
        services.AddSingleton<IMergeFilesService, MergeFilesService>();
    }

    public static async Task MainAsync(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var sortService = serviceProvider.GetRequiredService<ILargeFileSortService>();
        await sortService.Sort(config.GetValue<string>("inputFileName"), config.GetValue<string>("outputFileName"), CancellationToken.None);


    }
}