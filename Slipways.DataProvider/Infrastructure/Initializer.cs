using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Extensions;
using com.b_velop.Slipways.DataProvider.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace com.b_velop.Slipways.DataProvider.Infrastructure
{
    public class Initializer : IInitializer
    {
        private SlipwaysContext _context;
        private ILogger<Initializer> _logger;
        private IDistributedCache _cache;

        public Initializer(
            SlipwaysContext context,
            ILogger<Initializer> logger,
            IDistributedCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task InitDatabase<T>(
            string path,
            string name) where T : class, IEntity
        {
            var json = File.ReadAllText(path);
            var objects = JsonConvert.DeserializeObject<IEnumerable<T>>(json);
            var all = await _context.Set<T>().ToListAsync();
            var targets = new List<T>();
            foreach (var obj in objects)
            {
                var value = all.FirstOrDefault(_ => _.Id == obj.Id);
                if (value == null)
                    targets.Add(obj);
            }
            await _context.Set<T>().AddRangeAsync(targets);
            _context.SaveChanges();
        }

        public async Task InitCache<T>(
            string name) where T : class, IEntity
        {
            _logger.LogInformation($"Init cache for {name}");
            var all = await _context.Set<T>().ToListAsync();
            var asBytes = all.ToByteArray();
            await _cache.SetAsync(name, asBytes, new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(5)
            });
        }
    }
}
