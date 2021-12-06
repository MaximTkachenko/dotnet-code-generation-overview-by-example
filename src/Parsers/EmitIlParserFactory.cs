using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Parsers
{
    public class EmitIlParserFactory : IParserFactory
    {
        public Func<string[], T> GetParser<T>() where T : new()
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var dm = new DynamicMethod($"from_{typeof(string[]).FullName}_to_{typeof(T).FullName}", 
                typeof(T), new [] { typeof(string[]) }, typeof(EmitIlParserFactory).Module);
            var il = dm.GetILGenerator();

            var instance = il.DeclareLocal(typeof(T));
            il.Emit(OpCodes.Newobj, typeof(T).GetConstructors()[0]);
            il.Emit(OpCodes.Stloc, instance);

            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes(typeof(ArrayIndexAttribute)).ToArray();
                if (attrs.Length == 0) continue;

                int order = ((ArrayIndexAttribute)attrs[0]).Order;
                if (order < 0) continue;

                var label = il.DefineLabel();

                if (prop.PropertyType == typeof(string))
                {
                    il.Emit(OpCodes.Ldc_I4, order);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldlen);
                    il.Emit(OpCodes.Bge_S, label);

                    il.Emit(OpCodes.Ldloc, instance);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4, order);
                    il.Emit(OpCodes.Ldelem_Ref);
                    il.Emit(OpCodes.Callvirt, prop.GetSetMethod());

                    il.MarkLabel(label);
                    continue;
                }

                if (!TypeParsers.Parsers.TryGetValue(prop.PropertyType, out var parser))
                {
                    continue;
                }

                il.Emit(OpCodes.Ldc_I4, order);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Bge_S, label);

                var parseResult = il.DeclareLocal(prop.PropertyType);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, order);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Ldloca, parseResult);
                il.EmitCall(OpCodes.Call, parser, null);
                il.Emit(OpCodes.Brfalse_S, label);

                il.Emit(OpCodes.Ldloc, instance);
                il.Emit(OpCodes.Ldloc, parseResult);
                il.Emit(OpCodes.Callvirt, prop.GetSetMethod());

                il.MarkLabel(label);
            }

            il.Emit(OpCodes.Ldloc, instance);
            il.Emit(OpCodes.Ret);

            return (Func<string[], T>)dm.CreateDelegate(typeof(Func<string[], T>));
        }
    }
}
