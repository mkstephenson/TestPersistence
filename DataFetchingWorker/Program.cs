using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataFetchingWorker
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
              services.AddHttpClient();
              services.AddHostedService<Worker>();
              services.AddApplicationInsightsTelemetryWorkerService(hostContext.Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
            });
  }
}
