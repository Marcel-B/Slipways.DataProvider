using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Prometheus;

namespace Slipways.DataProvider
{
    public class Program
    {
        public static MetricPusher PushGateway;
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var job = env == "Production" ? "SlipwaysDataProvider" : "DevSlipwaysDataProvider";
            PushGateway = new MetricPusher(new MetricPusherOptions
            {
                Endpoint = "https://push.qaybe.de/metrics",
                Job = job,
                Instance = job
            });

            PushGateway.Start();
            string file;
            if (env == "Production")
                file = "nlog.config";
            else
                file = "dev-nlog.config";

            var logger = NLogBuilder.ConfigureNLog(file).GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
            .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .UseNLog();
    }
}
