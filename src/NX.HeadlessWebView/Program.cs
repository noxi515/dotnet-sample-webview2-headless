using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NX.HeadlessWebView;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<WebViewHost>();
    })
    .RunConsoleAsync();
