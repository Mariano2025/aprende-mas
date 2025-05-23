using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace MiServicioWindows
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "MiServicioWindows";
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                    services.AddLogging(logging =>
                    {
                        logging.AddEventLog(settings =>
                        {
                            settings.SourceName = "MiServicioWindows";
                            settings.LogName = "Application";
                        });
                        logging.AddConsole();
                    });
                })
                .Build()
                .Run();
        }
    }
}