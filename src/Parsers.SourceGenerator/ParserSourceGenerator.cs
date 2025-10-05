using System.Collections.Generic;
using System.Collections.Immutable;
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
    ///
    /// https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md
    /// https://blog.jetbrains.com/dotnet/2023/07/13/debug-source-generators-in-jetbrains-rider/
    /// https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/#creating-a-source-generator-with-the-loggermessage-source-generator-
    /// </summary>
    [Generator]
    public class ParserSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var typesToParse = context.SyntaxProvider
                .CreateSyntaxProvider<object>(
                    predicate: static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                    transform: (ctx, _) =>
                    {
                        var clasDeclaration = (ClassDeclarationSyntax)ctx.Node;
                        
                        var parserOutputTypeSymbol = ctx.SemanticModel.Compilation
                            .GetTypeByMetadataName("Parsers.ParserOutputAttribute");
                        
                        var symbol = ctx.SemanticModel.GetDeclaredSymbol(clasDeclaration) as INamedTypeSymbol;
                        if (symbol == null) return null;
                        
                        return symbol.GetAttributes().Any(a =>
                            SymbolEqualityComparer.Default.Equals(a.AttributeClass, parserOutputTypeSymbol))
                            ? symbol
                            : null;
                    })
                .Where(static m => m is not null)
                .Collect();
            
            context.RegisterSourceOutput(
                context.CompilationProvider.Combine(typesToParse), 
                (k, f ) =>
            {
                var attributeIndexTypeSymbol = f.Left.GetTypeByMetadataName("Parsers.ArrayIndexAttribute");
                GenerateSerializer(k, f.Right, attributeIndexTypeSymbol);
            });
        }
        
        private static void GenerateSerializer(SourceProductionContext context, ImmutableArray<object> classSymbol, INamedTypeSymbol attributeIndexTypeSymbol)
        {
            var typeNames = new List<(string TargetTypeName, string TargetTypeFullName, string TargetTypeParserName)>();
            var builder = new StringBuilder();
            builder.AppendLine(@"
using System;
using Parsers;
namespace BySourceGenerator
{
public class Parser : IParserFactory 
{");
            foreach (var typeSymbol in classSymbol.OfType<INamedTypeSymbol>())
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
                    if (attr == null || attr.ConstructorArguments[0].Value is not int) continue;

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