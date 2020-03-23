using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetS.Core.Builder;

namespace NetS.Core
{
    /// <summary>
    /// Extension methods for IApplicationHost interface.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Installs handlers for graceful shutdown in the console, starts a application and waits until it terminates. 
        /// </summary>
        /// <param name="app">ApplicationHost to run.</param>
        public static async Task RunAsync(this IApplicationHost app)
        {
            var done = new ManualResetEventSlim(false);
            using (var cts = new CancellationTokenSource())
            {
                Action shutdown = () =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        Console.WriteLine("ApplicationHost is shutting down...");
                        try
                        {
                            cts.Cancel();
                        }
                        catch (ObjectDisposedException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    done.Wait();
                };

                AssemblyLoadContext assemblyLoadContext = AssemblyLoadContext.GetLoadContext(typeof(IApplicationHost).GetTypeInfo().Assembly);
                assemblyLoadContext.Unloading += context => shutdown();

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    shutdown();
                    // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };

                try
                {
                    await app.RunAsync(cts.Token).ConfigureAwait(false);
                }
                finally
                {
                    done.Set();
                }
            }
        }

        /// <summary>
        /// Starts a application, sets up cancellation tokens for its shutdown, and waits until it terminates.
        /// </summary>
        /// <param name="app">ApplicationHost to run.</param>
        /// <param name="cancellationToken">Cancellation token that triggers when the ApplicationHost should be shut down.</param>
        public static async Task RunAsync(this IApplicationHost app, CancellationToken cancellationToken)
        {
            // app.ApplicationLifetime is not initialized yet. Use this temporary variable as to avoid side-effects to application.
            var appLifetime = app.Services.ServiceProvider.GetRequiredService<IApplicationLifeTime>() as ApplicationLifeTime;

            cancellationToken.Register(state =>
                {
                    ((IApplicationLifeTime)state).StopApplication();
                },
                appLifetime);

            var waitForStop = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            appLifetime.ApplicationStopping.Register(obj =>
            {
                var tcs = (TaskCompletionSource<object>)obj;
                tcs.TrySetResult(null);
            }, waitForStop);

            Console.WriteLine();
            Console.WriteLine("ApplicationHost starting, press Ctrl+C to cancel.");
            Console.WriteLine();

            app.Start();

            Console.WriteLine();
            Console.WriteLine("ApplicationHost started, press Ctrl+C to stop.");
            Console.WriteLine();

            await waitForStop.Task.ConfigureAwait(false);

            app.Dispose();

            Console.WriteLine();
            Console.WriteLine("ApplicationHost stopped.");
            Console.WriteLine();
        }
    }
}
