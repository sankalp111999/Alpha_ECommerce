using eCommerce.SharedLibrary.Logs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Application.Services;
using Polly;
using Polly.Retry;

namespace OrderApi.Application.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration config)
        {
            // Register HttpClient service
            //Create Dependency Injection
            //Creates named HttpClient for OrderService with pre-configured ProductAPI URL + 1s timeout.
            services.AddHttpClient<IOrderService,OrderService>(options =>
            {
               options.BaseAddress = new Uri(config["ApiGateway:BaseAddress"]!);
                options.Timeout = TimeSpan.FromSeconds(1);
            });




            //Create Retry Strategy
            // Defines retry policy - if ProductAPI times out → wait 500ms → retry (3x max).
            var retryStrategy = new RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder().Handle<TaskCanceledException>(),   //// Triggers on timeout
                BackoffType = DelayBackoffType.Constant,  // Constant delay
                UseJitter = true,    // Add jitter to avoid thundering herd problem
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),      // Wait for 500ms between retries
                OnRetry = args =>
                {
                    string message = $"Retrying {args.AttemptNumber} Outcome{args.Outcome}";
                    LogException.LogToConsole(message);
                    LogException.LogToDebugger(message);
                    return ValueTask.CompletedTask;
                }
            };




            //UseRetry Strategy
            services.AddResiliencePipeline("my-retry-pipeline", builder =>                  //// Named pipeline for later injection
            {
                builder.AddRetry(retryStrategy);
            });

            return services;
        }
    }
}
