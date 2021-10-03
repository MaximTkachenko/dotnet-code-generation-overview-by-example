using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Parsers.Common;

namespace Parsers
{
    /// <summary>
    /// https://www.tugberkugurlu.com/archive/compiling-c-sharp-code-into-memory-and-executing-it-with-roslyn
    /// https://gunnarpeipman.com/using-roslyn-to-build-object-to-object-mapper/amp/
    /// https://www.youtube.com/watch?v=052xutD86uI
    /// </summary>
    public static class RoslynParserInitializer
    {
        public static IParserFactory CreateFactory()
        {
            var targetTypes =
                (from a in AppDomain.CurrentDomain.GetAssemblies()
                 from t in a.GetTypes()
                 let attributes = t.GetCustomAttributes(typeof(ParserOutputAttribute), true)
                 where attributes != null && attributes.Length > 0
                 select t).ToArray();

            var typeNames = new List<(string TargetTypeName, string TargetTypeFullName, string TargetTypeParserName)>();
            var builder = new StringBuilder();
            builder.AppendLine(@"
using System;
using Parsers.Common;

public class RoslynGeneratedParserFactory : IParserFactory 
{");
            foreach (var targetType in targetTypes)
            {
                var targetTypeName = targetType.Name;
                var targetTypeFullName = targetType.FullName;
                var targetTypeParserName = targetTypeName + "Parser";
                typeNames.Add((targetTypeName, targetTypeFullName, targetTypeParserName));
                builder.AppendLine($"private static T {targetTypeParserName}<T>(string[] input)");

                builder.Append($@"
{{
    var {targetTypeName}Instance = new {targetTypeFullName}();");

                var props = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var prop in props)
                {
                    var attrs = prop.GetCustomAttributes(typeof(ArrayIndexAttribute)).ToArray();
                    if (attrs.Length == 0) continue;

                    int order = ((ArrayIndexAttribute)attrs[0]).Order;
                    if (order < 0) continue;

                    if (prop.PropertyType == typeof(string))
                    {
                        builder.Append($@"
if({order} < input.Length)
{{
    {targetTypeName}Instance.{prop.Name} = input[{order}];
}}
");
                    }

                    if (prop.PropertyType == typeof(int))
                    {
                        builder.Append($@"
if({order} < input.Length && int.TryParse(input[{order}], out var parsed{prop.Name}))
{{
    {targetTypeName}Instance.{prop.Name} = parsed{prop.Name};
}}
");
                    }

                    if (prop.PropertyType == typeof(DateTime))
                    {
                        builder.Append($@"
if({order} < input.Length && DateTime.TryParse(input[{order}], out var parsed{prop.Name}))
{{
    {targetTypeName}Instance.{prop.Name} = parsed{prop.Name};
}}
");
                    }
                }

                builder.Append($@"
    object obj = {targetTypeName}Instance;
    return (T)obj;
}}");
            }

            builder.AppendLine("public Func<string[], T> GetParser<T>() where T : new() {");
            foreach (var typeName in typeNames)
            {
                builder.Append($@"
if (typeof(T) == typeof({typeName.TargetTypeFullName}))
{{
    return {typeName.TargetTypeParserName}<T>;
}}
");
            }
            builder.AppendLine("throw new NotSupportedException();}");

            builder.AppendLine("}");

            var syntaxTree = CSharpSyntaxTree.ParseText(builder.ToString());

            string assemblyName = Path.GetRandomFileName();
            var refPaths = new List<string> {
                typeof(Object).GetTypeInfo().Assembly.Location,
                typeof(Enumerable).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll"),
                typeof(RoslynParserInitializer).GetTypeInfo().Assembly.Location,
                typeof(IParserFactory).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(GCSettings).GetTypeInfo().Assembly.Location), "netstandard.dll"),
            };
            refPaths.AddRange(targetTypes.Select(x => x.Assembly.Location));

            var references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
            
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    throw new Exception(string.Join(",", result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage())));
                }
                ms.Seek(0, SeekOrigin.Begin);

                var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                var factoryType = assembly.GetType("RoslynGeneratedParserFactory");
                if (factoryType == null) throw new NullReferenceException("Roslyn generated parser type not found");
                return (IParserFactory)Activator.CreateInstance(factoryType);
            }
        }
    }
}
