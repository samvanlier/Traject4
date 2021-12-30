using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph
{
    public static class Extentsions
    {
        public static double AvgDegree(this Graph g)
            => g.NodeDegrees.Average();

        private static double Average(this IEnumerable<ushort> array)
            => array.Select(i => (double)i).Average();
    }
}
