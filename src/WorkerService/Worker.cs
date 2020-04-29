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
using Polly.Timeout;
using WorkerService.Clients;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static ApiClient _apiClient;
        private CancellationTokenSource _cts;
        public Worker(ILogger<Worker> logger, ApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
            _cts = new CancellationTokenSource();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //await RetryPolicy();
                await TimeoutPolicy();
                await Task.Delay(5000, stoppingToken);
            }
        }

        async Task RetryPolicy()
        {
            var retry = Policy.Handle<Exception>().RetryAsync(100, onRetry: (exception, retryCount) =>
            {
                _logger.LogWarning($"\nRetrying the request. [Retry number: {retryCount}]\n");
            });
            await retry.ExecuteAsync(() => _apiClient.SendRequest(_cts.Token));
        }

        async Task TimeoutPolicy()
        {
            AsyncTimeoutPolicy timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Pessimistic, onTimeoutAsync: (context, timespan, task) =>
            {
                _logger.LogError($"{context.PolicyKey} at {context.OperationKey}: execution timed out after {timespan.TotalSeconds} seconds.");
                _cts.Cancel();
                _cts = new CancellationTokenSource();
                ExecuteAsync(_cts.Token).GetAwaiter();
                return Task.CompletedTask;
            });
            HttpResponseMessage httpResponse = await timeoutPolicy
                .ExecuteAsync(async ct => await _apiClient.SendRequest(_cts.Token), _cts.Token);

        }

    }
}
