using System;
using System.Linq;
using System.Reflection;
using Parsers.Common;

namespace Parsers
{
    public class ReflectionParserFactory : IParserFactory
    {
        public Func<string[], T> GetParser<T>() where T : new()
        {
            return ArrayIndexParse<T>;
        }

        private static T ArrayIndexParse<T>(string[] data) where T : new()
        {
            var instance = new T();
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < props.Length; i++)
            {
                var attrs = props[i].GetCustomAttributes(typeof(ArrayIndexAttribute)).ToArray();
                if (attrs.Length == 0) continue;

                int order = ((ArrayIndexAttribute)attrs[0]).Order;
                if (order < 0 || order >= data.Length) continue;

                if (props[i].PropertyType == typeof(string))
                {
                    props[i].SetValue(instance, data[order]);
                    continue;
                }

                if (props[i].PropertyType == typeof(int))
                {
                    if (int.TryParse(data[order], out var intResult))
                    {
                        props[i].SetValue(instance, intResult);
                    }

                    continue;
                }

                if (props[i].PropertyType == typeof(DateTime))
                {
                    if (DateTime.TryParse(data[order], out var dtResult))
                    {
                        props[i].SetValue(instance, dtResult);
                    }
                }
            }
            return instance;
        }
    }
}
