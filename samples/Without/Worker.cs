namespace Without;

public class Worker : BackgroundService
{
    private readonly HttpClient _client;
    private readonly ILogger<Worker> _logger;

    public Worker(HttpClient client, ILogger<Worker> logger)
    {
        _client = client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _client.GetAsync("https://www.google.com");
            await Task.Delay(10000, stoppingToken);
        }
    }
}
