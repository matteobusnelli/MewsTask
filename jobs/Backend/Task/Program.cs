using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;
using ExchangeRateUpdater.Configuration;
using ExchangeRateUpdater.HttpClients;
using ExchangeRateUpdater.Parsers;

namespace ExchangeRateUpdater
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Setup dependency injection container
            // The DI container automatically resolves and injects dependencies based on constructor parameters.
            // When a service is requested (e.g., ExchangeRateProvider), the container:
            // 1. Inspects the constructor to see what dependencies are needed
            // 2. Creates instances of those dependencies (ICnbHttpClient, ICnbDataParser)
            // 3. Calls the constructor with the created instances
            // This eliminates manual object creation and enables loose coupling.
            var services = new ServiceCollection();

            // Register configuration
            var cnbSettings = configuration.GetSection("CnbApi").Get<CnbApiSettings>();
            services.Configure<CnbApiSettings>(configuration.GetSection("CnbApi"));

            // Register HttpClient with Polly retry policy
            services.AddHttpClient<ICnbHttpClient, CnbHttpClient>()
                .AddTransientHttpErrorPolicy(builder =>
                    builder.WaitAndRetryAsync(
                        retryCount: cnbSettings?.RetryCount ?? 3,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(cnbSettings?.RetryDelaySeconds ?? 2)));

            // Register services
            services.AddTransient<ICnbDataParser, CnbDataParser>();
            services.AddTransient<ExchangeRateProvider>();

            // Build service provider container
            var serviceProvider = services.BuildServiceProvider();

            // Execute the application
            var currencies = new[]
            {
                new Currency("USD"),
                new Currency("EUR"),
                new Currency("CZK"),
                new Currency("JPY"),
                new Currency("KES"),
                new Currency("RUB"),
                new Currency("THB"),
                new Currency("TRY"),
                new Currency("XYZ")
            };

            try
            {
                var provider = serviceProvider.GetRequiredService<ExchangeRateProvider>();
                var rates = await provider.GetExchangeRatesAsync(currencies);

                Console.WriteLine($"Successfully retrieved {rates.Count()} exchange rates:");
                foreach (var rate in rates)
                {
                    Console.WriteLine(rate.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not retrieve exchange rates: '{e.Message}'.");
            }

            Console.ReadLine();
        }
    }
}
