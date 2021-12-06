using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Parsers
{
    public class ExpressionTreeParserFactory : IParserFactory
    {
        public Func<string[], T> GetParser<T>() where T : new()
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            ParameterExpression inputArray = Expression.Parameter(typeof(string[]), "inputArray");
            ParameterExpression instance = Expression.Variable(typeof(T), "instance");

            var block = new List<Expression>
            {
                Expression.Assign(instance, Expression.New(typeof(T).GetConstructors()[0]))
            };
            var variables = new List<ParameterExpression> {instance};

            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes(typeof(ArrayIndexAttribute)).ToArray();
                if (attrs.Length == 0) continue;

                int order = ((ArrayIndexAttribute)attrs[0]).Order;
                if (order < 0) continue;

                var orderConst = Expression.Constant(order);
                var orderCheck = Expression.LessThan(orderConst, Expression.ArrayLength(inputArray));

                if (prop.PropertyType == typeof(string))
                {
                    var stringPropertySet = Expression.Assign(
                        Expression.Property(instance, prop),
                        Expression.ArrayIndex(inputArray, orderConst));

                    block.Add(Expression.IfThen(orderCheck, stringPropertySet));
                    continue;
                }

                if (!TypeParsers.Parsers.TryGetValue(prop.PropertyType, out var parser))
                {
                    continue;
                }

                var parseResult = Expression.Variable(prop.PropertyType, "parseResult");
                var parserCall = Expression.Call(parser, Expression.ArrayIndex(inputArray, orderConst), parseResult);
                var propertySet = Expression.Assign(
                    Expression.Property(instance, prop),
                    parseResult);

                var ifSet = Expression.IfThen(parserCall, propertySet);

                block.Add(Expression.IfThen(orderCheck, ifSet));
                variables.Add(parseResult);
            }

            block.Add(instance);

            return Expression.Lambda<Func<string[], T>>(
                Expression.Block(variables.ToArray(), Expression.Block(block)), 
                inputArray).Compile();
        }
    }
}
