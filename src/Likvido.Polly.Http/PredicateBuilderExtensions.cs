using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Polly;

namespace Likvido.Polly.Http
{
    public static class PredicateBuilderExtensions
    {
        public static PredicateBuilder<T> HandleTransientHttpError<T>(this PredicateBuilder<T> predicateBuilder) 
            where T : HttpResponseMessage
        {
            return predicateBuilder
                .Handle<HttpRequestException>()
                .HandleResult(response =>
                    (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout);
        }

        public static PredicateBuilder<T> HandleSocketException<T>(this PredicateBuilder<T> predicateBuilder) 
            where T : HttpResponseMessage
        {
            return predicateBuilder
                .Handle<SocketException>()
                .HandleInner<SocketException>();
        }

        public static PredicateBuilder<T> HandleRateLimitHit<T>(this PredicateBuilder<T> predicateBuilder) 
            where T : HttpResponseMessage
        {
            return predicateBuilder.HandleResult(response => (int)response.StatusCode == 429);
        }
    }
}
