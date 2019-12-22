using System;
using System.Threading;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Helper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Slipways.DataProvider.Infrastructure;

namespace Slipways.DataProvider.Services
{
    public class CacheLoader : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CacheLoader> _logger;
        private Timer _timer;

        public CacheLoader(
            IServiceProvider services,
            ILogger<CacheLoader> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("CacheLoader service running");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async void DoWork(
            object state)
        {
            try
            {
                using var scope = _services.CreateScope();
                _logger.LogInformation("Reload cache");
                var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
                var rep = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();

                var waters = await rep.Water.SelectAllAsync();
                var o = waters.ToByteArray();
                await cache.RemoveAsync(Cache.Waters);
                await cache.SetAsync(Cache.Waters, o);

                //_ = await rep.Manufacturer.SelectAllAsync();

                //_ = await rep.Station.SelectAllAsync();

                //_ = await rep.Extra.SelectAllAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(6666, $"Error while updating cache", e);
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