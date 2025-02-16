using Without;

AppContext.SetSwitch("System.Net.Http.DisableUriRedaction", true);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((_, b) =>
    {
        b.AddSimpleConsole(o => { o.SingleLine = true; });
    })
    .ConfigureServices(services =>
    {
        services.Configure<HostOptions>(hostOptions =>
        {
            hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });
        services.AddHttpClient();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
