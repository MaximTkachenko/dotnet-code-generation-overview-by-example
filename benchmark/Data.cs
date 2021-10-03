using System;
using Parsers.Common;

namespace Parsers.Benchmarks
{
    [ParserOutput]
    public class Data
    {
        [ArrayIndex(0)]
        public string Name { get; set; }

        [ArrayIndex(2)]
        public int Number { get; set; }

        [ArrayIndex(1)]
        public DateTime Birthday { get; set; }
    }
}
