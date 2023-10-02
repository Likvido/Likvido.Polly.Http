using System;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace Likvido.Polly.Http
{
    [PublicAPI]
    public static class LikvidoHttpStrategy
    {
        /// <summary>
        /// The default strategy for Likvido HTTP clients.
        /// Will retry transient errors, socket exceptions and rate limit hits.
        /// Will respect the Retry-After header.
        /// Includes an overall timeout of default 90 seconds.
        /// </summary>
        public static ResiliencePipeline<HttpResponseMessage> Default(ILogger logger, LikvidoHttpStrategyOptions options = null)
        {
            if (options == null)
            {
                options = new LikvidoHttpStrategyOptions();
            }

            return new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .HandleTransientHttpError()
                        .HandleSocketException()
                        .HandleRateLimitHit(),
                    MaxRetryAttempts = options.MaxRetryAttempts,
                    Delay = options.Delay,
                    DelayGenerator = LikvidoDelayGenerators.RateLimitAwareDelayGenerator(),
                    BackoffType = options.DelayBackoffType,
                    OnRetry = async args =>
                        await LogRetry(logger, args.Outcome, args.AttemptNumber, args.RetryDelay)
                })
                .AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = options.Timeout,
                    OnTimeout = args =>
                    {
                        logger.LogWarning("Timeout after {Timeout}ms", args.Timeout.TotalMilliseconds);

                        return new ValueTask(Task.CompletedTask);
                    }
                })
                .Build();
        }

        private static async Task LogRetry(
            ILogger logger,
            Outcome<HttpResponseMessage> outcome,
            int attemptNumber,
            TimeSpan retryDelay)
        {
            string response = null;

            try
            {
                var content = outcome.Result?.Content;
                if (content != null)
                {
                    response = await content.ReadAsStringAsync();
                }
            }
            catch
            {
                // ignored
            }

            logger.LogWarning(
                outcome.Exception,
                "Delaying for {Delay}ms," +
                " then making retry {Retry}. Status code: {StatusCode}. Response: {Response}",
                retryDelay.TotalMilliseconds, attemptNumber, outcome.Result?.StatusCode, response);

            outcome.Result?.Dispose();
        }
    }
}
