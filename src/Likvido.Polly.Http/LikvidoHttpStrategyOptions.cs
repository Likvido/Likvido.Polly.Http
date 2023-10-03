using System;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using Polly;

namespace Likvido.Polly.Http
{
    [PublicAPI]
    public class LikvidoHttpStrategyOptions
    {
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(2);
        public DelayBackoffType DelayBackoffType { get; set; } = DelayBackoffType.Exponential;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(90);

        /// <summary>
        /// Use this to add additional status codes to retry on.
        /// </summary>
        public List<HttpStatusCode> ExtraRetryStatusCodes { get; set; } = new List<HttpStatusCode>();
    }
}
