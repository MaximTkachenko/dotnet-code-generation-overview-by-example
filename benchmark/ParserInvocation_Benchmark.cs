using System;
using BenchmarkDotNet.Attributes;
using Parsers.Common;

namespace Parsers.Benchmarks
{
    // ReSharper disable once InconsistentNaming
    [MemoryDiagnoser]
    public class ParserInvocation_Benchmark
    {
        private Func<string[], Data> _emitIlParser;
        private Func<string[], Data> _expressionTreeParser;
        private Func<string[], Data> _reflectionParser;
        private Func<string[], Data> _sigilParser;
        private Func<string[], Data> _roslynParser;
        private Func<string[], Data> _sourceGeneratorParser; 
        
        private static readonly string[] Input = { "one", "1994-11-05T13:15:30", "22" };

        [GlobalSetup]
        public void GlobalSetup()
        {
            _emitIlParser = new EmitIlParserFactory().GetParser<Data>();
            _expressionTreeParser = new ExpressionTreeParserFactory().GetParser<Data>();
            _reflectionParser = new ReflectionParserFactory().GetParser<Data>();
            _sigilParser = new SigilParserFactory().GetParser<Data>();
            _roslynParser = RoslynParserInitializer.CreateFactory().GetParser<Data>();
            // ReSharper disable once PossibleNullReferenceException
            _sourceGeneratorParser = ((IParserFactory)Activator.CreateInstance(Type.GetType("BySourceGenerator.Parser"))).GetParser<Data>();
        }

        [Benchmark]
        public Data EmitIl()
        {
            return _emitIlParser.Invoke(Input);
        }

        [Benchmark]
        public Data ExpressionTree()
        {
            return _expressionTreeParser.Invoke(Input);
        }

        [Benchmark]
        public Data Reflection()
        {
            return _reflectionParser.Invoke(Input);
        }

        [Benchmark]
        public Data Sigil()
        {
            return _sigilParser.Invoke(Input);
        }

        [Benchmark]
        public Data Roslyn()
        {
            return _roslynParser.Invoke(Input);
        }

        [Benchmark]
        public Data SourceGenerator()
        {
            return _sourceGeneratorParser.Invoke(Input);
        }

        [Benchmark(Baseline = true)]
        public Data ManuallyWritten()
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
