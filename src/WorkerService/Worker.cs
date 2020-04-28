using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
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
                await CircuitBreakerPolicy();
                await Task.Delay(500, stoppingToken);
            }
        }

        async Task RetryPolicy()
        {
            var retry = Policy.Handle<Exception>().RetryAsync(100, onRetry: (exception, retryCount) =>
            {
                _logger.LogWarning($"\nRetrying the request. [Retry number: {retryCount}]\n");
            });
            await retry.ExecuteAsync(() => _apiClient.SendRequest());
        }

        async Task CircuitBreakerPolicy()
        {
            var breaker = Policy
                 .Handle<HttpRequestException>()
                 .CircuitBreakerAsync(2, TimeSpan.FromSeconds(10),
                 (exception, timespan, context) => { Console.WriteLine("OnBreak"); },
                 context => { Console.WriteLine("OnReset"); });

            await breaker.ExecuteAsync(() => _apiClient.SendRequest());
        }

    }
}
