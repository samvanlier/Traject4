using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using ToolBox.Bridge;
using ToolBox.Notification;
using ToolBox.Platform;
using System.Diagnostics;

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

        private static double Average(this IEnumerable<ushort> array)
            => array.Select(i => (double)i).Average();

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

        private struct Pair : IEquatable<Pair>
        {
            public int A;
            public int B;

            public Pair(int a, int b)
            {
                A = Math.Min(a, b);
                B = Math.Max(a, b);
            }

            public override bool Equals(object obj)
            {
                return obj is Pair pair && Equals(pair);
            }

            public bool Equals(Pair other)
            {
                return A == other.A && B == other.B ||
                       A == other.B && B == other.A;
            }

            public override int GetHashCode()
            {
                var a = Math.Min(A, B);
                var b = Math.Max(A, B);
                return HashCode.Combine(a, b);
            }

            public static bool operator ==(Pair left, Pair right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Pair left, Pair right)
            {
                return !(left == right);
            }
        }

        public static void SaveToPdf(this Graph g, string fileName)
        {
            var pairs = new HashSet<Pair>();

            for (int a = 0; a < g.NumberOfNodes; a++)
            {
                for (int b = 0; b < g.NumberOfNodes; b++)
                {
                    if (g.IsConnected(a, b))
                        pairs.Add(new Pair(a, b));
                }
            }

            var edges = pairs.Select(p => new Edge<int>(p.A, p.B)).ToList();

            var dotGraph = edges.ToUndirectedGraph<int, Edge<int>>().ToGraphviz();

            // convert a graph to SVG or pdf
            string command = $"echo '{dotGraph}' | dot -Tpdf > {fileName}";

            var notificationSystem = NotificationSystem.Default;
            string os = OS.GetCurrent();
            IBridgeSystem bridgeSystem = BridgeSystem.Bash;

            switch (os)
            {
                case "win":
                    bridgeSystem = BridgeSystem.Bat;
                    break;
                case "mac":
                case "gnu":
                    bridgeSystem = BridgeSystem.Bash;
                    break;
            }
            var shell = new ShellConfigurator(bridgeSystem, notificationSystem);

            Response result = shell.Term(command);

            if (result.code == 0)
                Console.WriteLine($"File is printed");
            else
                Console.WriteLine($"{result.stderr}");
        }
    }
}