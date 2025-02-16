using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseMinimalHttpLogger(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor
            .Singleton<IHttpMessageHandlerBuilderFilter, ReplaceLoggingHttpMessageHandlerBuilderFilter>());
        return services;
    }
}

internal class ReplaceLoggingHttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory)
    : IHttpMessageHandlerBuilderFilter
{
    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            next(builder);

            var loggerName = !string.IsNullOrEmpty(builder.Name) ? builder.Name : "Default";
            var innerLogger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.ClientHandler");
            var toRemove = builder.AdditionalHandlers
                .Where(h => h is LoggingHttpMessageHandler or LoggingScopeHttpMessageHandler)
                .ToList();
            foreach (var delegatingHandler in toRemove)
            {
                builder.AdditionalHandlers.Remove(delegatingHandler);
            }

            builder.AdditionalHandlers.Add(new RequestEndOnlyLogger(innerLogger));
        };
    }
}

internal class RequestEndOnlyLogger(ILogger logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var
            requestUri =
                request.RequestUri
                    ?.ToString(); //SendAsync modifies req uri in case of redirects (?!), so making a local copy
        var stopwatch = ValueStopwatch.StartNew();
        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("{Method} {Uri} - {StatusCode} {StatusCodeLiteral} in {Time}ms", request.Method,
                requestUri, $"{(int)response.StatusCode}", $"{response.StatusCode}",
                stopwatch.GetElapsedTime().TotalMilliseconds);
            return response;
        }
        catch (Exception)
        {
            logger.LogInformation("{Method} {Uri} failed to respond in {Time}ms", request.Method, requestUri,
                stopwatch.GetElapsedTime().TotalMilliseconds);
            throw;
        }
    }

    internal readonly struct ValueStopwatch
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private readonly long _startTimestamp;

        public bool IsActive => _startTimestamp != 0;

        private ValueStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
        }

        public static ValueStopwatch StartNew()
        {
            return new ValueStopwatch(Stopwatch.GetTimestamp());
        }

        public TimeSpan GetElapsedTime()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException(
                    "An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
            }

            var end = Stopwatch.GetTimestamp();
            var timestampDelta = end - _startTimestamp;
            var ticks = (long)(TimestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }
    }
}
