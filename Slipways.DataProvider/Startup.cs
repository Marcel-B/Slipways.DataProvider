using System;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Models;
using com.b_velop.Slipways.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slipways.DataProvider.Infrastructure;
using Slipways.DataProvider.Services;
using StackExchange.Redis;

namespace Slipways.DataProvider
{
    public class Startup
    {
        public void ConfigureServices(
            IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddHostedService<BackUp>();
            var secretProvider = new SecretProvider();

            var server = Environment.GetEnvironmentVariable("SERVER");
            var database = Environment.GetEnvironmentVariable("DATABASE");
            var user = Environment.GetEnvironmentVariable("USER");
            var port = Environment.GetEnvironmentVariable("PORT");
            var pw = secretProvider.GetSecret("dev_slipway_db");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "cache";
                options.InstanceName = "Slipways";
                var co = new ConfigurationOptions();
                co.ResolveDns = true;
                options.ConfigurationOptions = co;
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

            services.AddDbContext<SlipwaysContext>(options =>
            {
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Slipways.DataProvider"));
            });
        }

        public async void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IDistributedCache cache)
        {
            await InitializeDatabase(app, cache);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                            await context.Response.WriteAsync("Hello"));
            });
        }

        private async Task InitializeDatabase(
            IApplicationBuilder app,
            IDistributedCache cache)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<SlipwaysContext>();
            context.Database.Migrate();
            var initializer = new Initializer(context, cache);
            await initializer.Init<Water>("./initWaters.json", "Waters");
            await initializer.Init<Extra>("./initExtras.json", "Extras");
            await initializer.Init<Manufacturer>("./initManufacturers.json", "Manufacturers");
            await initializer.Init<Slipway>("./initSlipways.json", "Slipways");
            await initializer.Init<Service>("./initServices.json", "Services");
            await initializer.Init<SlipwayExtra>("./initSlipwayExtras.json", "SlipwayExtras");
            await initializer.Init<ManufacturerService>("./initManufacturerServices.json", "ManufacturerServices");
            context.SaveChanges();
        }
    }
}
