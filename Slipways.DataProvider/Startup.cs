using System;
using com.b_velop.Slipways.Data;
using com.b_velop.Slipways.Data.Contracts;
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

namespace Slipways.DataProvider
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = "cache";
            //    options.InstanceName = "Slipways";
            //});
            var connectionString = $"Server={server},{port};Database={database};User Id={user};Password={pw}";
            services.AddScoped<ISecretProvider, SecretProvider>();
            services.AddScoped<IExtraRepository, ExtraRepository>();
            services.AddScoped<IManufacturerRepository, ManufacturerRepository> ();
            services.AddScoped<IManufacturerServicesRepository, ManufacturerServicesRepository> ();
            services.AddScoped<IPortRepository, PortRepository> ();
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper> ();
            services.AddScoped<IServiceRepository, ServiceRepository> ();
            services.AddScoped<ISlipwayExtraRepository, SlipwayExtraRepository> ();
            services.AddScoped<ISlipwayRepository, SlipwayRepository> ();
            services.AddScoped<IStationRepository, StationRepository> ();
            services.AddScoped <IWaterRepository, WaterRepository> ();

            services.AddDbContext<SlipwaysContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime lifetime
            //,
            //IDistributedCache cache
            )
        {
            //lifetime.ApplicationStarted.Register(() =>
            //{
            //    cache.SetString("test", "Hello from space");
            //});
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    //lifetime.ApplicationStarted.Register(async () =>
                    //{
                    //    var value = cache.GetString("test");
                    //    await context.Response.WriteAsync(value);
                    //});
                });
            });
        }
    }
}
