namespace With;

public class Worker : BackgroundService
{
    private readonly HttpClient _client;
    private readonly ILogger<Worker> _logger;

    public Worker(HttpClient client, ILogger<Worker> logger)
    {
        _client = client;
        _client.Timeout = TimeSpan.FromSeconds(10);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _client.GetAsync("https://httpstat.us/200?sleep=3000", stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Timeout!");
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}
