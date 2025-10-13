using System;

namespace Parsers;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ParserOutputAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Property)]
public sealed class ArrayIndexAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}