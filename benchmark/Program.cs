using BenchmarkDotNet.Running;

namespace Parsers.Benchmarks
{
    class Program
    {
        private const string AllMode = "all";
        private const string GetParserMode = "gp";
        private const string ParserInvocationMode = "pi";

        static void Main(string[] args)
        {
            var mode = args.Length == 1 ? args[0] : AllMode;

            switch (mode)
            {
                case GetParserMode:
                    BenchmarkRunner.Run<GetParser_Benchmark>();
                    break;
                case ParserInvocationMode:
                    BenchmarkRunner.Run<ParserInvocation_Benchmark>();
                    break;
                default:
                    BenchmarkRunner.Run<GetParser_Benchmark>();
                    BenchmarkRunner.Run<ParserInvocation_Benchmark>();
                    break;
            }
        }
    }
}
