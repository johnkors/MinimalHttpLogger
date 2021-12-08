using Without;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((c, b) =>
    {
        b.AddSimpleConsole(o => { o.SingleLine = true; });
    })
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();



