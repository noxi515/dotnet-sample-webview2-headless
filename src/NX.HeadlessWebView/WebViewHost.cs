using System.Windows.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Web.WebView2.Core;

namespace NX.HeadlessWebView;

public class WebViewHost : IHostedService
{
    private static readonly IntPtr HWND_MESSAGE = new(-3);

    private readonly CoreWebView2Controller _controller;
    private readonly Dispatcher _dispatcher;

    public WebViewHost()
    {
        var dispatcherSource = new TaskCompletionSource<Dispatcher>();
        var controllerSource = new TaskCompletionSource<CoreWebView2Controller>();

        // UI用のスレッドを作成してWebView2Controllerの生成
        var uiThread = new Thread(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            dispatcherSource.SetResult(dispatcher);

            dispatcher.Invoke(async () =>
            {
                // WebView2Controllerの生成
                var env = await CoreWebView2Environment.CreateAsync();
                var controller = await env.CreateCoreWebView2ControllerAsync(HWND_MESSAGE);
                controllerSource.SetResult(controller);
            });
            Dispatcher.Run();
        });

        // UIスレッドとして実行
        uiThread.SetApartmentState(ApartmentState.STA);
        uiThread.Start();

        // 作成したDispatcherとWebView2Controllerをフィールドに保持
        _dispatcher = dispatcherSource.Task.Result;
        _controller = controllerSource.Task.Result;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // WebView2Controllerの破棄（しているつもりなのだが、エラーが発生しているのか終了しない）
        _dispatcher.Invoke(() =>
        {
            _controller.Close();
        });

        return Task.CompletedTask;
    }
}
