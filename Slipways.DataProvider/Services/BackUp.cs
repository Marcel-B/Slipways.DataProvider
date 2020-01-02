using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using com.b_velop.Slipways.Data.Contracts;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Security;

namespace com.b_velop.Slipways.DataProvider.Services
{
    public class BackUp : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<BackUp> _logger;
        private Timer _timer;
        private const string BackUpPath = "./backUp/";

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
            _logger.LogInformation("BackUp service running");
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(5),
                TimeSpan.FromHours(2));
            return Task.CompletedTask;
        }

        private async Task BackUpAsync<T>(
            IEnumerable<T> objects,
            string type)
        {
            var fileName = Path.Combine(BackUpPath, $"{type}.json");
            try
            {
                var file = new FileInfo(fileName);
                using var sw = file.CreateText();
                using var str = sw.BaseStream;
                await JsonSerializer.SerializeAsync(str, objects, new JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = true });
                sw.Close();
            }
            catch (ArgumentNullException e)
            {
                _logger?.LogError(6659, $"Error occurred while back up '{type}' in file '{fileName}'", e);
            }
            catch (ArgumentException e)
            {
                _logger?.LogError(6660, $"Error occurred while back up '{type}' in file '{fileName}'", e);
            }
            catch (PathTooLongException e)
            {
                _logger?.LogError(6661, $"Error occurred while back up '{type}' in file '{fileName}'", e);
            }
            catch (NotSupportedException e)
            {
                _logger?.LogError(6662, $"Error occurred while back up '{type}' in file '{fileName}'", e);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger?.LogError(6663, $"Error occurred while back up '{type}' in file '{fileName}' - Unauthorized", e);
            }
            catch (IOException e)
            {
                _logger?.LogError(6664, $"Error occurred while back up '{type}' in file '{fileName}'", e);
            }
            catch (SecurityException e)
            {
                _logger?.LogError(6665, $"Error occurred while back up '{type}' in file '{fileName}'", e);
            }
            catch (Exception e)
            {
                _logger?.LogError(6666, $"Error occurred while back up '{type}' in file '{fileName}'", e);
            }
        }

        private async Task BackUpSlipwaysAsync(
            IRepositoryWrapper wrapper)
        {
            try
            {
                var slipways = await wrapper.Slipway.SelectAllAsync();
                await BackUpAsync(slipways, "slipways");
            }
            catch (Exception e)
            {
                _logger.LogError(6666, "Error occurred while fetch slipways from RepositoryWrapper", e);
            }
        }

        private async Task BackUpStationsAsync(
            IRepositoryWrapper wrapper)
        {
            try
            {
                var slipways = await wrapper.Station.SelectAllAsync();
                await BackUpAsync(slipways, "stations");
            }
            catch (Exception e)
            {
                _logger.LogError(6666, "Error occurred while fetch stations from RepositoryWrapper", e);
            }
        }

        private async Task BackUpServicesAsync(
            IRepositoryWrapper wrapper)
        {
            try
            {
                var slipways = await wrapper.Service.SelectAllAsync();
                await BackUpAsync(slipways, "services");
            }
            catch (Exception e)
            {
                _logger.LogError(6666, "Error occurred while fetch services from RepositoryWrapper", e);
            }
        }

        private async Task BackUpManufacturersAsync(
            IRepositoryWrapper wrapper)
        {
            try
            {
                var slipways = await wrapper.Manufacturer.SelectAllAsync();
                await BackUpAsync(slipways, "manufacturers");
            }
            catch (Exception e)
            {
                _logger.LogError(6666, "Error occurred while fetch manufacturers from RepositoryWrapper", e);
            }
        }

        private async Task BackUpExtrasAsync(
            IRepositoryWrapper wrapper)
        {
            try
            {
                var slipways = await wrapper.Extra.SelectAllAsync();
                await BackUpAsync(slipways, "extras");
            }
            catch (Exception e)
            {
                _logger.LogError(6666, "Error occurred while fetch extras from RepositoryWrapper", e);
            }
        }

        private async Task BackUpSlipwayExtrasAsync(
            IRepositoryWrapper wrapper)
        {
            try
            {
                var slipways = await wrapper.SlipwayExtra.SelectAllAsync();
                await BackUpAsync(slipways, "slipwayExtras");
            }
            catch (Exception e)
            {
                _logger.LogError(6666, "Error occurred while fetch slipwayExtras from RepositoryWrapper", e);
            }
        }

        private async Task BackUpManufacturerServicesAsync(
            IRepositoryWrapper wrapper)
        {
            try
            {
                var slipways = await wrapper.ManufacturerServices.SelectAllAsync();
                await BackUpAsync(slipways, "manufacturerServices");
            }
            catch (Exception e)
            {
                _logger.LogError(6666, "Error occurred while fetch manufacturerServices from RepositoryWrapper", e);
            }
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

                await BackUpSlipwaysAsync(wrapper);
                await BackUpServicesAsync(wrapper);
                await BackUpManufacturersAsync(wrapper);
                await BackUpSlipwayExtrasAsync(wrapper);
                await BackUpExtrasAsync(wrapper);
                await BackUpManufacturerServicesAsync(wrapper);
                await BackUpStationsAsync(wrapper);
            }
            catch (InvalidOperationException e)
            {
                _logger?.LogError(6662, $"Error while backup database", e);
            }
            catch (ArgumentNullException e)
            {
                _logger?.LogError(6663, $"Error while backup database", e);
            }
            catch (ArgumentException e)
            {
                _logger?.LogError(6664, $"Error while backup database", e);
            }
            catch (IOException e)
            {
                _logger?.LogError(6665, $"Error while backup database", e);
            }
            catch (Exception e)
            {
                _logger?.LogError(6666, $"Error while backup database", e);
            }
        }

        public Task StopAsync(
            CancellationToken stoppingToken)
        {
            _logger?.LogInformation("BackUp Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
