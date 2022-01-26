using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph
{
    public static class Extentsions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static double AvgDegree(this Graph g)
            => g.NodeDegrees.Average();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="nodeNumber"></param>
        /// <returns></returns>
        public static ICollection<int> GetConnectedNodes(this Graph g, int nodeNumber)
        {
            var colConnected = g.Matrix.GetColumn(nodeNumber)
                .Select((node, index) => new { index, node }) // pair nodes with their index
                .Where(el => el.node) // select the nodes that are connect
                .Select(el => el.index) // get the connect node ids.
                .ToArray();

            var rowConnected = g.Matrix.GetRow(nodeNumber)
                .Select((node, index) => new { index, node }) // pair nodes with their index
                .Where(el => el.node) // select the nodes that are connect
                .Select(el => el.index) // get the connect node ids.
                .ToArray();

            return colConnected.Concat(rowConnected).Distinct().Where(index => index != nodeNumber).ToArray();
        }

        private static double Average(this IEnumerable<ushort> array)
            => array.Select(i => (double)i).Average();
    }
}
