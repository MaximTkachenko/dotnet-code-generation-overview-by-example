using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Parsers;

public class CachedParserFactory(IParserFactory realParserFactory) : IParserFactory
{
    private readonly ConcurrentDictionary<string, Lazy<object>> _cache = new();

    public Func<string[], T> GetParser<T>() where T : new()
    {
        return (Func<string[], T>)(_cache.GetOrAdd($"aip_{realParserFactory.GetType().FullName}_{typeof(T).FullName}", 
            new Lazy<object>(realParserFactory.GetParser<T>, LazyThreadSafetyMode.ExecutionAndPublication)).Value);
    }
}