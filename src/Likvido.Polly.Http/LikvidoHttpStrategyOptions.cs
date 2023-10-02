using System;
using Polly;

namespace Likvido.Polly.Http
{
    public class LikvidoHttpStrategyOptions
    {
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(2);
        public DelayBackoffType DelayBackoffType { get; set; } = DelayBackoffType.Exponential;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(90);
    }
}
