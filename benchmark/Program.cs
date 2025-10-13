using BenchmarkDotNet.Running;
using Parsers.Benchmarks;

var mode = args.Length == 1 ? args[0] : "all";
switch (mode)
{
    case "gp":
        BenchmarkRunner.Run<GetParser_Benchmark>();
        break;
    case "pi":
        BenchmarkRunner.Run<ParserInvocation_Benchmark>();
        break;
    default:
        BenchmarkRunner.Run<GetParser_Benchmark>();
        BenchmarkRunner.Run<ParserInvocation_Benchmark>();
        break;
}