using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using com.b_velop.Slipways.Data.Contracts;
using System.IO;
using System.Text.Json;

namespace Slipways.DataProvider.Services
{
    public class BackUp : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<BackUp> _logger;
        private Timer _timer;

        public BackUp(
            IServiceProvider services,
            ILogger<BackUp> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("CacheLoader service running");

            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(5),
                TimeSpan.FromHours(2));
            return Task.CompletedTask;
        }

        private async void DoWork(
            object state)
        {
            try
            {
                using var scope = _services.CreateScope();
                var directory = new DirectoryInfo("backUp");

                if (!directory.Exists)
                    directory.Create();

                _logger.LogInformation("Start BackUp Database");
                var wrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                var slipways = await wrapper.Slipway.SelectAllAsync();
                var file = new FileInfo("./backUp/slipways.json");
                using var sw = file.CreateText();
                using var str = sw.BaseStream;
                await JsonSerializer.SerializeAsync(str, slipways, new JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = true});
                sw.Close();
            }
            catch(ArgumentNullException e)
            {
                _logger.LogError(6666, $"Error while backup database", e);
            }
            catch (Exception e)
            {
                _logger.LogError(6666, $"Error while backup database", e);
            }
        }

        public Task StopAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("CacheLoader Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
