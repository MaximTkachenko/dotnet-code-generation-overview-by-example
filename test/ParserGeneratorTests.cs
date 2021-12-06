using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Parsers.Tests
{
    public class ParserGeneratorTests
    {
        [Theory]
        [MemberData(nameof(Factories))]
        public void GetParserUsingArrayIndex_ValidInput_Parsed(IParserFactory parserFactory)
        {
            var p = parserFactory.GetParser<Data>();

            var inst = p.Invoke(new[] { "one", "1994-11-05T13:15:30", "22" });

            inst.Name.Should().Be("one");
            inst.Birthday.Should().Be(new DateTime(1994, 11, 5, 13, 15, 30));
            inst.Number.Should().Be(22);
            inst.PropertyWithInvalidAttr.Should().Be(null);
        }

        [Theory]
        [MemberData(nameof(Factories))]
        public void GetParserUsingArrayIndex_InvalidInput_DefaultValuesNoException(IParserFactory parserFactory)
        {
            var p = parserFactory.GetParser<Data>();

            var inst = p.Invoke(new[] { "one", "2011ss-11-22", "2vv2" });

            inst.Name.Should().Be("one");
            inst.Birthday.Should().Be(DateTime.MinValue);
            inst.Number.Should().Be(default(int));
            inst.PropertyWithInvalidAttr.Should().Be(null);
        }

        public static IEnumerable<object[]> Factories => new List<object[]>
        {
            new object[] { new ExpressionTreeParserFactory() },
            new object[] { new CachedParserFactory(new ExpressionTreeParserFactory()) },
            new object[] { new EmitIlParserFactory() },
            new object[] { new CachedParserFactory(new EmitIlParserFactory()) },
            new object[] { new SigilParserFactory() },
            new object[] { new CachedParserFactory(new SigilParserFactory()) },
            new object[] { RoslynParserInitializer.CreateFactory() },
            new object[] { (IParserFactory)Activator.CreateInstance(Type.GetType("BySourceGenerator.Parser")) },
            new object[] { new ReflectionParserFactory() },
        };
    }
}
