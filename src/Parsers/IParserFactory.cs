using System;

namespace Parsers
{
    public interface IParserFactory
    {
        Func<string[], T> GetParser<T>() where T : new();
    }
}
