using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data;
using com.b_velop.Slipways.Data.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Slipways.DataProvider.Infrastructure
{
    public class Initializer
    {
        private SlipwaysContext _context;
        private IDistributedCache _cache;

        public Initializer(
            SlipwaysContext context,
            IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task Init<T>(
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
            await InitCache<T>(name);
        }

        public async Task InitCache<T>(
            string name) where T: class, IEntity
        {
            Console.WriteLine($"Init cache for {name}");
            var all = await _context.Set<T>().ToListAsync();
            var asBytes = all.ToByteArray();
            await _cache.SetAsync(name, asBytes);
        }
    }
}
