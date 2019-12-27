using System;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Helper;
using com.b_velop.Slipways.Data.Models;
using com.b_velop.Slipways.Data.Repositories;
using com.b_velop.Slipways.DataProvider.Contracts;
using com.b_velop.Slipways.DataProvider.Infrastructure;
using com.b_velop.Slipways.DataProvider.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace com.b_velop.Slipways.DataProvider
{
    public class Startup
    {
        public void ConfigureServices(
            IServiceCollection services)
        {
            services.AddHostedService<BackUp>();
            var secretProvider = new SecretProvider();

            var server = Environment.GetEnvironmentVariable("SERVER");
            var database = Environment.GetEnvironmentVariable("DATABASE");
            var user = Environment.GetEnvironmentVariable("USER");
            var port = Environment.GetEnvironmentVariable("PORT");
            var cache = Environment.GetEnvironmentVariable("CACHE");

            var pw = secretProvider.GetSecret(server);

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cache;
                options.InstanceName = "Slipways";
            });

            var connectionString = $"Server={server},{port};Database={database};User Id={user};Password={pw}";

            services.AddScoped<ISecretProvider, SecretProvider>();
            services.AddScoped<IExtraRepository, ExtraRepository>();
            services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
            services.AddScoped<IManufacturerServicesRepository, ManufacturerServicesRepository>();
            services.AddScoped<IPortRepository, PortRepository>();
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<ISlipwayExtraRepository, SlipwayExtraRepository>();
            services.AddScoped<ISlipwayRepository, SlipwayRepository>();
            services.AddScoped<IStationRepository, StationRepository>();
            services.AddScoped<IWaterRepository, WaterRepository>();
            services.AddScoped<IInitializer, Initializer>();

            services.AddDbContext<SlipwaysContext>(options =>
            {
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Slipways.DataProvider"));
            });

            services.AddControllers();
        }

        public async void Configure(
            ILogger<Startup> logger,
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpMetrics();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });

            await InitializeDatabase(logger, app);
        }

        private async Task InitializeDatabase(
            ILogger<Startup> logger,
            IApplicationBuilder app)
        {
            logger.LogInformation($"Start initializing Cache");
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<SlipwaysContext>();
            var initializer = serviceScope.ServiceProvider.GetRequiredService<IInitializer>();

            context.Database.Migrate();
            
            await initializer.Init<Water>("./initWaters.json", Cache.Waters);
            await initializer.Init<Extra>("./initExtras.json", Cache.Extras);
            await initializer.Init<Manufacturer>("./initManufacturers.json", Cache.Manufacturers);
            await initializer.Init<Slipway>("./initSlipways.json", Cache.Slipways);
            await initializer.Init<Service>("./initServices.json", Cache.Services);
            await initializer.Init<SlipwayExtra>("./initSlipwayExtras.json", Cache.SlipwayExtras);
            await initializer.Init<Station>("./initStations.json", Cache.Stations);

            // TODO - Stations
            await initializer.Init<ManufacturerService>("./initManufacturerServices.json", Cache.ManufacturerServices);
            context.SaveChanges();
            logger.LogInformation($"Initializing Cache ready");
        }
    }
}
