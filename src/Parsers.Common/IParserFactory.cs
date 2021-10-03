using System;

namespace Parsers.Common
{
    public interface IParserFactory
    {
        Func<string[], T> GetParser<T>() where T : new();
    }
}
