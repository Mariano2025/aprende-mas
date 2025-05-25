using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting.WindowsServices;
using AprendeMasWindowsService.Service;

namespace MiServicioWindows
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "AprendeMasService";
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<AprendeMasService>();
                    services.AddLogging(logging =>
                    {
                        logging.AddEventLog(settings =>
                        {
                            settings.SourceName = "AprendeMasService";
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