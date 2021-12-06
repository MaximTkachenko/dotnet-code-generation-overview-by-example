using System;

namespace Parsers
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ParserOutputAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ArrayIndexAttribute : Attribute
    {
        public ArrayIndexAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }
}