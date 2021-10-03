using System;
using System.Linq;
using System.Reflection;
using Parsers.Common;
using Sigil;

namespace Parsers
{
    public class SigilParserFactory : IParserFactory
    {
        public Func<string[], T> GetParser<T>() where T : new()
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var il = Emit<Func<string[], T>>.NewDynamicMethod($"from_{typeof(string[]).FullName}_to_{typeof(T).FullName}");

            var instance = il.DeclareLocal<T>();
            il.NewObject<T>();
            il.StoreLocal(instance);

            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes(typeof(ArrayIndexAttribute)).ToArray();
                if (attrs.Length == 0) continue;

                int order = ((ArrayIndexAttribute)attrs[0]).Order;
                if (order < 0) continue;

                var label = il.DefineLabel();

                if (prop.PropertyType == typeof(string))
                {
                    il.LoadConstant(order);
                    il.LoadArgument(0);
                    il.LoadLength<string>();
                    il.BranchIfGreaterOrEqual(label);

                    il.LoadLocal(instance);
                    il.LoadArgument(0);
                    il.LoadConstant(order);
                    il.LoadElement<string>();
                    il.CallVirtual(prop.GetSetMethod());

                    il.MarkLabel(label);
                    continue;
                }

                if (!TypeParsers.Parsers.TryGetValue(prop.PropertyType, out var parser))
                {
                    continue;
                }

                il.LoadConstant(order);
                il.LoadArgument(0);
                il.LoadLength<string>();
                il.BranchIfGreaterOrEqual(label);

                var parseResult = il.DeclareLocal(prop.PropertyType);
                
                il.LoadArgument(0);
                il.LoadConstant(order);
                il.LoadElement<string>();
                il.LoadLocalAddress(parseResult);
                il.Call(parser);
                il.BranchIfFalse(label);

                il.LoadLocal(instance);
                il.LoadLocal(parseResult);
                il.CallVirtual(prop.GetSetMethod());

                il.MarkLabel(label);
            }

            il.LoadLocal(instance);
            il.Return();

            return il.CreateDelegate();
        }
    }
}
