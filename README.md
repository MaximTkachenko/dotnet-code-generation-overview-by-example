[![.NET](https://github.com/MaximTkachenko/dotnet-code-generation-overview-by-example/actions/workflows/dotnet.yml/badge.svg)](https://github.com/MaximTkachenko/dotnet-code-generation-overview-by-example/actions/workflows/dotnet.yml)

[Dotnet code generation overview by example](https://mtkachenko.me/blog/dotnet/2021/10/03/dotnet-code-generation.html)

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Core i7-8550U CPU 1.80GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.401
  [Host]     : .NET 5.0.10 (5.0.1021.41214), X64 RyuJIT
  DefaultJob : .NET 5.0.10 (5.0.1021.41214), X64 RyuJIT


```
|                           Method |              Mean |             Error |            StdDev |            Median |     Gen 0 |    Gen 1 |   Allocated |
|--------------------------------- |------------------:|------------------:|------------------:|------------------:|----------:|---------:|------------:|
|                 GetParser_EmitIl |     21,674.487 ns |       432.3948 ns |     1,205.3440 ns |     21,405.957 ns |    1.2817 |   0.6409 |     5,472 B |
|         GetParser_ExpressionTree |    728,408.643 ns |    16,212.6425 ns |    47,548.8739 ns |    715,700.293 ns |    2.9297 |   0.9766 |    13,926 B |
|             GetParser_Reflection |          7.664 ns |         0.3028 ns |         0.8831 ns |          7.472 ns |    0.0153 |        - |        64 B |
|                  GetParser_Sigil |    668,290.456 ns |    14,567.7839 ns |    42,031.3875 ns |    657,595.947 ns |  112.3047 |        - |   470,727 B |
|                 GetParser_Roslyn | 60,652,769.231 ns | 1,199,616.1232 ns | 2,804,064.0147 ns | 60,564,942.857 ns | 1000.0000 | 285.7143 | 6,257,459 B |
|          ParserInvocation_EmitIl |        395.822 ns |         8.5046 ns |        25.0761 ns |        390.066 ns |    0.0095 |        - |        40 B |
|  ParserInvocation_ExpressionTree |        385.048 ns |         7.7925 ns |        21.8511 ns |        380.575 ns |    0.0095 |        - |        40 B |
|      ParserInvocation_Reflection |     13,064.371 ns |       277.1491 ns |       812.8303 ns |     12,883.695 ns |    0.7782 |        - |     3,256 B |
|           ParserInvocation_Sigil |        386.315 ns |         7.8140 ns |        18.1102 ns |        383.428 ns |    0.0095 |        - |        40 B |
|          ParserInvocation_Roslyn |        403.248 ns |         8.1247 ns |        21.9656 ns |        399.653 ns |    0.0095 |        - |        40 B |
| ParserInvocation_SourceGenerator |        385.155 ns |         8.6582 ns |        24.8419 ns |        379.948 ns |    0.0095 |        - |        40 B |
|                       NativeCall |        383.488 ns |         7.7245 ns |        21.4046 ns |        380.181 ns |    0.0095 |        - |        40 B |
