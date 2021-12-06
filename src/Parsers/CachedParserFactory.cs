using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Parsers
{
    public class CachedParserFactory : IParserFactory
    {
        private readonly IParserFactory _realParserFactory;
        private readonly ConcurrentDictionary<string, Lazy<object>> _cache;

        public CachedParserFactory(IParserFactory realParserFactory)
        {
            _realParserFactory = realParserFactory;
            _cache = new ConcurrentDictionary<string, Lazy<object>>();
        }

        public Func<string[], T> GetParser<T>() where T : new()
        {
            return (Func<string[], T>)(_cache.GetOrAdd($"aip_{_realParserFactory.GetType().FullName}_{typeof(T).FullName}", 
                new Lazy<object>(() => _realParserFactory.GetParser<T>(), LazyThreadSafetyMode.ExecutionAndPublication)).Value);
        }
    }
}
