[![.NET](https://github.com/MaximTkachenko/dotnet-code-generation-overview-by-example/actions/workflows/dotnet.yml/badge.svg)](https://github.com/MaximTkachenko/dotnet-code-generation-overview-by-example/actions/workflows/dotnet.yml)

Source code for [Dotnet code generation overview by example](https://mtkachenko.me/blog/2021/10/03/dotnet-code-generation.html)

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Core i7-8550U CPU 1.80GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.401
  [Host]     : .NET 5.0.10 (5.0.1021.41214), X64 RyuJIT
  DefaultJob : .NET 5.0.10 (5.0.1021.41214), X64 RyuJIT


```

## Generation of parser
|         Method |         Mean |        Error |       StdDev |     Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|--------------- |-------------:|-------------:|-------------:|----------:|-------:|-------:|----------:|
|         EmitIl |     22.02 μs |     0.495 μs |     1.429 μs |    1.2817 | 0.6409 | 0.0305 |      5 KB |
| ExpressionTree |    683.68 μs |    13.609 μs |    31.268 μs |    2.9297 | 0.9766 |      - |     14 KB |
|          Sigil |    642.63 μs |    12.305 μs |    29.243 μs |  112.3047 |      - |      - |    460 KB |
|         Roslyn | 71,605.64 μs | 2,533.732 μs | 7,350.817 μs | 1000.0000 |      - |      - |  5,826 KB |

## Invocation of parser
|          Method |        Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
|---------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|
|          EmitIl |    374.7 ns |   7.75 ns |  22.36 ns |  1.02 |    0.08 | 0.0095 |      40 B |
|  ExpressionTree |    378.1 ns |   7.56 ns |  20.57 ns |  1.03 |    0.08 | 0.0095 |      40 B |
|      Reflection | 13,625.0 ns | 272.60 ns | 750.81 ns | 37.29 |    2.29 | 0.7782 |   3,256 B |
|           Sigil |    378.9 ns |   7.69 ns |  21.06 ns |  1.03 |    0.07 | 0.0095 |      40 B |
|          Roslyn |    404.2 ns |   7.55 ns |  17.80 ns |  1.10 |    0.07 | 0.0095 |      40 B |
| SourceGenerator |    384.4 ns |   7.79 ns |  21.46 ns |  1.05 |    0.08 | 0.0095 |      40 B |
| ManuallyWritten |    367.8 ns |   7.36 ns |  15.68 ns |  1.00 |    0.00 | 0.0095 |      40 B |
