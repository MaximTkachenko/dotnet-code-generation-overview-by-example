[![.NET](https://github.com/MaximTkachenko/dotnet-code-generation-overview-by-example/actions/workflows/dotnet.yml/badge.svg)](https://github.com/MaximTkachenko/dotnet-code-generation-overview-by-example/actions/workflows/dotnet.yml)

Source code for [Dotnet code generation overview by example](https://mtkachenko.me/blog/2021/10/03/dotnet-code-generation.html)

``` ini

BenchmarkDotNet v0.15.4, macOS 26.0.1 (25A362) [Darwin 25.0.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 9.0.9 (9.0.9, 9.0.925.41916), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 9.0.9 (9.0.9, 9.0.925.41916), Arm64 RyuJIT armv8.0-a


```

## Generation of parser
| Method         | Mean          | Error       | StdDev      | Gen0      | Gen1     | Gen2     | Allocated  |
|--------------- |--------------:|------------:|------------:|----------:|---------:|---------:|-----------:|
| EmitIl         |      3.370 us |   0.0283 us |   0.0251 us |    0.4196 |   0.2060 |   0.0305 |    3.48 KB |
| ExpressionTree |    131.847 us |   0.4957 us |   0.4394 us |    1.2207 |   0.4883 |        - |    11.4 KB |
| Sigil          |    130.371 us |   1.2540 us |   1.1730 us |   52.7344 |  13.6719 |        - |   433.2 KB |
| Roslyn         | 14,522.046 us | 263.8589 us | 246.8138 us | 1031.2500 | 343.7500 | 125.0000 | 7757.56 KB |

## Invocation of parser
| Method          | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| EmitIl          |    75.14 ns |  0.352 ns |  0.294 ns |  1.01 |    0.00 | 0.0048 |      40 B |        1.00 |
| ExpressionTree  |    76.12 ns |  1.306 ns |  1.222 ns |  1.02 |    0.02 | 0.0048 |      40 B |        1.00 |
| Reflection      | 1,084.40 ns | 21.704 ns | 25.837 ns | 14.52 |    0.34 | 0.0992 |     832 B |       20.80 |
| Sigil           |    75.51 ns |  0.319 ns |  0.249 ns |  1.01 |    0.00 | 0.0048 |      40 B |        1.00 |
| Roslyn          |    82.45 ns |  0.117 ns |  0.110 ns |  1.10 |    0.00 | 0.0048 |      40 B |        1.00 |
| SourceGenerator |    75.59 ns |  0.092 ns |  0.086 ns |  1.01 |    0.00 | 0.0048 |      40 B |        1.00 |
| ManuallyWritten |    74.66 ns |  0.142 ns |  0.133 ns |  1.00 |    0.00 | 0.0048 |      40 B |        1.00 |
