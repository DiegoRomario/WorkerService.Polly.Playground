using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using WorkerService.Clients;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static ApiClient _apiClient;

        public Worker(ILogger<Worker> logger, ApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await RetryPolicy();

                await Task.Delay(500, stoppingToken);
            }
        }

        async Task RetryPolicy()
        {
            var retry = Policy.Handle<Exception>().RetryAsync(100, onRetry: (exception, retryCount) => {
                _logger.LogWarning($"\nRetrying the request. [Retry number: {retryCount}]\n");
            });
            await retry.ExecuteAsync(() => _apiClient.SendRequest());
        }
    }
}
