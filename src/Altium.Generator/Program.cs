// See https://aka.ms/new-console-template for more information

using Altium.Core;
using Altium.Generator.Config;
using Altium.Generator.Services;
using Altium.Generator.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altium.Generator
{
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
            var generatorConfig = new GeneratorConfig();
            config.Bind("config",generatorConfig);

            var repeatingConfig=new RepeatingConfig();
            config.Bind("repeating",repeatingConfig);

            var charSet = new CharSet(config.GetValue<string>("availableChars"));

            services.AddSingleton(w => generatorConfig);
            services.AddSingleton(w => config);
            services.AddSingleton(w => charSet);
            services.AddSingleton(w => repeatingConfig);
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IFileGenerator, FileGenerator>();
        }

        public static async Task MainAsync(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var charSet = serviceProvider.GetRequiredService<CharSet>();
            var config = serviceProvider.GetRequiredService<GeneratorConfig>();
            var repeatingConfig = serviceProvider.GetRequiredService<RepeatingConfig>();
            var generator= serviceProvider.GetRequiredService<IFileGenerator>();
            await generator.GenerateAsync(charSet,config,repeatingConfig);


        }
    }
}


