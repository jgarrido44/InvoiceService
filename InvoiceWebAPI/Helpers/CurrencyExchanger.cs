using InvoiceWebAPI.Controllers;
using InvoiceWebAPI.Helpers;
using Newtonsoft.Json;

namespace InvoiceWebAPI
{
    public class CurrencyExchanger
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoiceController> _logger;
        private readonly HttpClient _httpClient = new();

        public CurrencyExchanger(IConfiguration configuration, ILogger<InvoiceController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<decimal> GetExchangeRateAsync(string baseCurrency, string targetCurrency)
        {
            try
            {
                string apiUrl = _configuration.GetValue<string>("CurrencyApiUrl");
                string apiKey = _configuration.GetValue<string>("CurrencyApiKey");

                string endpointUrl = $"{apiUrl}/{apiKey}/pair/{baseCurrency}/{targetCurrency}";

                HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to retrieve exchange rate. Watch for typos in currency code.");
                }

                string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var exchangeRateData = JsonConvert.DeserializeObject<Currency>(responseContent) ??
                    throw new Exception("Failed to deserialize currency cbject");

                _logger.LogInformation("Conversion was successful");

                return exchangeRateData.conversion_rate;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
