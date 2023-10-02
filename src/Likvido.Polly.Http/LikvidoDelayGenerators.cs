using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Polly.Retry;

namespace Likvido.Polly.Http
{
    public static class LikvidoDelayGenerators
    {
        public static Func<RetryDelayGeneratorArguments<HttpResponseMessage>, ValueTask<TimeSpan?>> RateLimitAwareDelayGenerator()
        {
            return args =>
            {
                IEnumerable<string> values = null;
                if (args.Outcome.Result?.Headers.TryGetValues("Retry-After", out values) ?? false)
                {
                    var firstRetryAfterValue = values.First();
                    if (int.TryParse(firstRetryAfterValue, out var delayInSeconds))
                    {
                        return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(delayInSeconds));
                    }

                    if (DateTime.TryParse(firstRetryAfterValue, out var retryAfter))
                    {
                        return new ValueTask<TimeSpan?>(retryAfter - DateTime.UtcNow);
                    }
                }

                return new ValueTask<TimeSpan?>((TimeSpan?)null);
            };
        }
    }
}
