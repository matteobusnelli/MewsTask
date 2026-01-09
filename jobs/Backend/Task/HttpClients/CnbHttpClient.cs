using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ExchangeRateUpdater.Configuration;

namespace ExchangeRateUpdater.HttpClients
{
    // HTTP client implementation for CNB API.
    public class CnbHttpClient : ICnbHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly CnbApiSettings _settings;

        public CnbHttpClient(HttpClient httpClient, IOptions<CnbApiSettings> settings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        // Method's implementation: fetches exchange rate data from the CNB API and returns exchange rate data as a string in pipe-separated format
        public async Task<string> GetExchangeRatesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_settings.BaseUrl);
                // Ensure the HTTP response is success
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new InvalidOperationException("Received empty response from CNB API.");
                }

                return content;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to fetch exchange rates from CNB API.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException($"Request to CNB API timed out after {_settings.TimeoutSeconds} seconds.", ex);
            }
        }
    }
}
