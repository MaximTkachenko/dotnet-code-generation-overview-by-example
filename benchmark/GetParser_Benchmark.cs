using System;
using BenchmarkDotNet.Attributes;

namespace Parsers.Benchmarks
{
    // ReSharper disable once InconsistentNaming
    [MemoryDiagnoser]
    public class GetParser_Benchmark
    {
        private EmitIlParserFactory _emitIlParserFactory;
        private ExpressionTreeParserFactory _expressionTreeParserFactory;
        private SigilParserFactory _sigilParserFactory;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _emitIlParserFactory = new EmitIlParserFactory();
            _expressionTreeParserFactory = new ExpressionTreeParserFactory();
            _sigilParserFactory = new SigilParserFactory();
        }

        [Benchmark]
        public Func<string[], Data> EmitIl()
        {
            return _emitIlParserFactory.GetParser<Data>();
        }

        [Benchmark]
        public Func<string[], Data> ExpressionTree()
        {
            return _expressionTreeParserFactory.GetParser<Data>();
        }

        [Benchmark]
        public Func<string[], Data> Sigil()
        {
            return _sigilParserFactory.GetParser<Data>();
        }

        [Benchmark]
        public Func<string[], Data> Roslyn()
        {
            return RoslynParserInitializer.CreateFactory().GetParser<Data>();
        }
    }
}
