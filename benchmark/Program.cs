using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Parsers.Common;

namespace Parsers.Benchmarks
{
    class Program
    {
        static void Main() => BenchmarkRunner.Run<Test>();
    }

    [MemoryDiagnoser]
    public class Test
    {
        private EmitIlParserFactory _emitIlParserFactory;
        private ExpressionTreeParserFactory _expressionTreeParserFactory;
        private SigilParserFactory _sigilParserFactory;

        private Func<string[], Data> _emitIlParser;
        private Func<string[], Data> _expressionTreeParser;
        private Func<string[], Data> _reflectionParser;
        private Func<string[], Data> _sigilParser;
        private Func<string[], Data> _roslynParser;
        private Func<string[], Data> _sourceGeneratorParser;

        private static readonly string[] Input = {"one", "1994-11-05T13:15:30", "22"};

        [GlobalSetup]
        public void GlobalSetup()
        {
            _emitIlParserFactory = new EmitIlParserFactory();
            _expressionTreeParserFactory = new ExpressionTreeParserFactory();
            _sigilParserFactory = new SigilParserFactory();

            _emitIlParser = new EmitIlParserFactory().GetParser<Data>();
            _expressionTreeParser = new ExpressionTreeParserFactory().GetParser<Data>();
            _reflectionParser = new ReflectionParserFactory().GetParser<Data>();
            _sigilParser = new SigilParserFactory().GetParser<Data>();
            _roslynParser = RoslynParserInitializer.CreateFactory().GetParser<Data>();
            _sourceGeneratorParser = ((IParserFactory) Activator.CreateInstance(Type.GetType("BySourceGenerator.Parser"))).GetParser<Data>();
        }

        [Benchmark]
        public Func<string[], Data> GetParser_EmitIl()
        {
            return _emitIlParserFactory.GetParser<Data>();
        }

        [Benchmark]
        public Func<string[], Data> GetParser_ExpressionTree()
        {
            return _expressionTreeParserFactory.GetParser<Data>();
        }

        [Benchmark]
        public Func<string[], Data> GetParser_Sigil()
        {
            return _sigilParserFactory.GetParser<Data>();
        }

        [Benchmark]
        public Func<string[], Data> GetParser_Roslyn()
        {
            return RoslynParserInitializer.CreateFactory().GetParser<Data>();
        }

        [Benchmark]
        public Data ParserInvocation_EmitIl()
        {
            return _emitIlParser.Invoke(Input);
        }

        [Benchmark]
        public Data ParserInvocation_ExpressionTree()
        {
            return _expressionTreeParser.Invoke(Input);
        }

        [Benchmark]
        public Data ParserInvocation_Reflection()
        {
            return _reflectionParser.Invoke(Input);
        }

        [Benchmark]
        public Data ParserInvocation_Sigil()
        {
            return _sigilParser.Invoke(Input);
        }

        [Benchmark]
        public Data ParserInvocation_Roslyn()
        {
            return _roslynParser.Invoke(Input);
        }
        
        [Benchmark]
        public Data ParserInvocation_SourceGenerator()
        {
            return _sourceGeneratorParser.Invoke(Input);
        }

        [Benchmark(Baseline = true)]
        public Data NativeCall()
        {
            var data = new Data();
            if (0 < Input.Length)
            {
                data.Name = Input[0];
            }
            if (1 < Input.Length && DateTime.TryParse(Input[1], out var bd))
            {
                data.Birthday = bd;
            }
            if (2 < Input.Length && int.TryParse(Input[2], out var n))
            {
                data.Number = n;
            }
            return data;
        }
    }
}
