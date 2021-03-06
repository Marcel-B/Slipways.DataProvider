﻿using System;
using System.Threading;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data.Helper;
using com.b_velop.Slipways.Data.Models;
using com.b_velop.Slipways.DataProvider.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace com.b_velop.Slipways.DataProvider.Services
{
    public class CacheLoader : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CacheLoader> _logger;
        //private Timer _timer;

        public CacheLoader(
            IServiceProvider services,
            ILogger<CacheLoader> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async Task StartAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("CacheLoader service running");
            await InitDatabaseAsync();
            //_timer = new Timer(DoWork, null, TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(1));
        }

        private async Task InitDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Add Values to Database");
                using var scope = _services.CreateScope();
                var initializer = scope.ServiceProvider.GetRequiredService<IInitializer>();

                await initializer.InitDatabase<Water>("./initWaters.json", Cache.Waters);
                await initializer.InitDatabase<Extra>("./initExtras.json", Cache.Extras);
                await initializer.InitDatabase<Manufacturer>("./initManufacturers.json", Cache.Manufacturers);
                await initializer.InitDatabase<Slipway>("./initSlipways.json", Cache.Slipways);
                await initializer.InitDatabase<Service>("./initServices.json", Cache.Services);
                await initializer.InitDatabase<SlipwayExtra>("./initSlipwayExtras.json", Cache.SlipwayExtras);
                await initializer.InitDatabase<Station>("./initStations.json", Cache.Stations);
                await initializer.InitDatabase<ManufacturerService>("./initManufacturerServices.json", Cache.ManufacturerServices);

                _logger.LogInformation("Add Values to Database done");
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(6665, $"Error occurred while init database", e);
            }
            catch (Exception e)
            {
                _logger.LogError(6666, $"Unexpected error occurred while init database", e);
            }
        }

        //private async void DoWork(
        //    object state)
        //{
        //    try
        //    {
        //        using var scope = _services.CreateScope();
        //        _logger.LogInformation("Reload cache");
        //        var initializer = scope.ServiceProvider.GetRequiredService<IInitializer>();

        //        await initializer.InitCache<Water>(Cache.Waters);
        //        await initializer.InitCache<SlipwayExtra>(Cache.SlipwayExtras);
        //        await initializer.InitCache<Slipway>(Cache.Slipways);
        //        await initializer.InitCache<Extra>(Cache.Extras);
        //        await initializer.InitCache<Station>(Cache.Stations);
        //        await initializer.InitCache<Manufacturer>(Cache.Manufacturers);
        //        await initializer.InitCache<ManufacturerService>(Cache.ManufacturerServices);
        //        await initializer.InitCache<Service>(Cache.Services);
        //        await initializer.InitCache<Port>(Cache.Ports);

        //        _logger.LogInformation("Reload cache done");
        //    }
        //    catch (InvalidOperationException e)
        //    {
        //        _logger.LogError(6665, $"Error occurred while updating cache", e);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(6666, $"Unexpected error occurred while updating cache", e);
        //    }
        //}

        public Task StopAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("CacheLoader Service is stopping.");
            //_timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //_timer?.Dispose();
        }
    }
}