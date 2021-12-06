using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Parsers.SourceGenerator
{
    /// <summary>
    /// http://dontcodetired.com/blog/post/C-Source-Generators-Less-Boilerplate-Code-More-Productivity
    /// https://github.com/amis92/csharp-source-generators
    /// </summary>
    [Generator]
    public class ParserSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            //uncomment to debug
            //System.Diagnostics.Debugger.Launch();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            var parserOutputTypeSymbol = compilation.GetTypeByMetadataName("Parsers.ParserOutputAttribute");
            var attributeIndexTypeSymbol = compilation.GetTypeByMetadataName("Parsers.ArrayIndexAttribute");
            var typesToParse = new List<ITypeSymbol>();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                typesToParse.AddRange(syntaxTree.GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(x => semanticModel.GetDeclaredSymbol(x))
                    .OfType<ITypeSymbol>()
                    .Where(x => x.GetAttributes().Select(a => a.AttributeClass)
                        .Any(b => b == parserOutputTypeSymbol)));
            }

            var typeNames = new List<(string TargetTypeName, string TargetTypeFullName, string TargetTypeParserName)>();
            var builder = new StringBuilder();
            builder.AppendLine(@"
using System;
using Parsers;
namespace BySourceGenerator
{
public class Parser : IParserFactory 
{");
            foreach (var typeSymbol in typesToParse)
            {
                var targetTypeName = typeSymbol.Name;
                var targetTypeFullName = GetFullName(typeSymbol);
                var targetTypeParserName = targetTypeName + "Parser";
                typeNames.Add((targetTypeName, targetTypeFullName, targetTypeParserName));
                builder.AppendLine($"private static T {targetTypeParserName}<T>(string[] input)");

                builder.Append($@"
{{
    var {targetTypeName}Instance = new {targetTypeFullName}();");

                var props = typeSymbol.GetMembers().OfType<IPropertySymbol>();
                foreach (var prop in props)
                {
                    var attr = prop.GetAttributes().FirstOrDefault(x => x.AttributeClass == attributeIndexTypeSymbol);
                    if (attr == null || !(attr.ConstructorArguments[0].Value is int)) continue;

                    int order = (int) attr.ConstructorArguments[0].Value;
                    if (order < 0) continue;

                    if (GetFullName(prop.Type) == "System.String")
                    {
                        builder.Append($@"
if({order} < input.Length)
{{
    {targetTypeName}Instance.{prop.Name} = input[{order}];
}}
");
                    }

                    if (GetFullName(prop.Type) == "System.Int32")
                    {
                        builder.Append($@"
if({order} < input.Length && int.TryParse(input[{order}], out var parsed{prop.Name}))
{{
    {targetTypeName}Instance.{prop.Name} = parsed{prop.Name};
}}
");
                    }

                    if (GetFullName(prop.Type) == "System.DateTime")
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

            builder.AppendLine("}}");

            var src = builder.ToString();
            context.AddSource(
                "ParserGeneratedBySourceGenerator.cs",
                SourceText.From(src, Encoding.UTF8)
            );
        }

        private static string GetFullName(ITypeSymbol typeSymbol) =>
            $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";
    }
}