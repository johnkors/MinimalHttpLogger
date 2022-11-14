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
        services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, ReplaceLoggingHttpMessageHandlerBuilderFilter>());
        return services;
    }
}

internal class ReplaceLoggingHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly ILoggerFactory _loggerFactory;

    public ReplaceLoggingHttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            next(builder);

            var loggerName = !string.IsNullOrEmpty(builder.Name) ? builder.Name : "Default";
            var innerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.ClientHandler");
            var toRemove = builder.AdditionalHandlers.Where(h => (h is LoggingHttpMessageHandler) || h is LoggingScopeHttpMessageHandler).Select(h => h).ToList();
            foreach (var delegatingHandler in toRemove)
            {
                builder.AdditionalHandlers.Remove(delegatingHandler);
            }
            builder.AdditionalHandlers.Add(new RequestEndOnlyLogger(innerLogger));
        };
    }
}

internal class RequestEndOnlyLogger : DelegatingHandler
{
    private readonly ILogger _logger;

    public RequestEndOnlyLogger(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        var requestUri = request.RequestUri?.ToString(); //SendAsync modifies req uri in case of redirects (?!), so making a local copy
        var stopwatch = ValueStopwatch.StartNew();
        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("{Method} {Uri} - {StatusCode} {StatusCodeLiteral} in {Time}ms", request.Method, requestUri, $"{(int)response.StatusCode}", $"{response.StatusCode}", stopwatch.GetElapsedTime().TotalMilliseconds);
            return response;
        }
        catch(Exception)
        {
            _logger.LogInformation("{Method} {Uri} failed to respond in {Time}ms", request.Method, requestUri, stopwatch.GetElapsedTime().TotalMilliseconds);
            throw;
        }
    }

    internal struct ValueStopwatch
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private long _startTimestamp;

        public bool IsActive => _startTimestamp != 0;

        private ValueStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
        }

        public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

        public TimeSpan GetElapsedTime()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
            }

            long end = Stopwatch.GetTimestamp();
            long timestampDelta = end - _startTimestamp;
            long ticks = (long)(TimestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }
    }
}
