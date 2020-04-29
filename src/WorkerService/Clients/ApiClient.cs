using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService.Clients
{
    public class ApiClient
    {
        private HttpClient _client;
        private ILogger<ApiClient> _logger;
        private IConfiguration _configuration;
        private string _apiURL;

        public ApiClient(HttpClient client, ILogger<ApiClient> logger, IConfiguration configuration)
        {
            _client = client;
            _logger = logger;
            _configuration = configuration;
            _apiURL = _configuration.GetSection("UrlApi").Value;
        }

        public async Task<HttpResponseMessage> SendRequest(CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(_apiURL, cancellationToken);
                string result = string.Empty;
                result = await response.Content.ReadAsStringAsync();
                Console.ResetColor();
                if (response.IsSuccessStatusCode)
                {
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    _logger.LogInformation($"Success: [{response.StatusCode}] - {result} ");

                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    _logger.LogWarning($"Warning: [{response.StatusCode}] - {result} ");
                }
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (HttpRequestException)
            {

                throw;
            }         


        }

    }
}
