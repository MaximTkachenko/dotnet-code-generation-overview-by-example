using System;
using System.Collections.Generic;
using System.Reflection;

namespace Parsers
{
    internal static class TypeParsers
    {
        public static readonly Dictionary<Type, MethodInfo> Parsers = new Dictionary<Type, MethodInfo>
        {
            { typeof(int), typeof(int).GetMethod("TryParse", new[] {typeof(string), typeof(int).MakeByRefType()}) },
            { typeof(DateTime), typeof(DateTime).GetMethod("TryParse", new[] {typeof(string), typeof(DateTime).MakeByRefType()}) }
        };
    }
}
