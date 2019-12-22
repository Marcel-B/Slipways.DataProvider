using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
using Newtonsoft.Json;
using Slipways.DataProvider.Infrastructure;
using Slipways.DataProvider.Services;

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

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime lifetime,
            IDistributedCache cache)
        {
            InitializeDatabase(app, cache);
            lifetime.ApplicationStarted.Register(async () =>
            {
                cache.SetString("test", "Hello from space");
                var bf = new BinaryFormatter();
                var slipway = new Slipway
                {
                    Id = Guid.NewGuid(),
                    Name = "Test",
                    City = "Krefeld"
                };
                using var ms = new MemoryStream();
                bf.Serialize(ms, slipway);
                var result = ms.ToArray();
                await cache.SetAsync("slipway", result);
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    lifetime.ApplicationStarted.Register(async () =>
                    {
                        var bytes = await cache.GetAsync("slipway");
                        using (var memStream = new MemoryStream())
                        {
                            var binForm = new BinaryFormatter();
                            memStream.Write(bytes, 0, bytes.Length);
                            memStream.Seek(0, SeekOrigin.Begin);
                            var obj = binForm.Deserialize(memStream) as Slipway;
                            System.Console.WriteLine(obj.Name);
                            var abc = JsonConvert.SerializeObject(obj);
                            Console.WriteLine(abc);
                            await context.Response.WriteAsync(abc);
                        }
                        //var value = cache.GetString("test");
                    });
                });
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
