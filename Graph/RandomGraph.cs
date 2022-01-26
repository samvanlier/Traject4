using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Graph
{
    public class RandomGraph : Graph
    {
        private readonly Random _random;

        /// <summary>
        /// Create a new random simple graph using the <a href="https://en.wikipedia.org/wiki/Erd%C5%91s%E2%80%93R%C3%A9nyi_model">Erdős–Rényi model</a>.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="edges"></param>
        /// <param name="random"></param>
        public RandomGraph(int nodes, int edges, Random random, bool isFullyConnected = false) : base(nodes)
        {
            // checks
            if (nodes < 2)
                throw new ArgumentException("The number of nodes has to be 2 or more");

            int check = MaxNumberOfEdges(nodes);

            if (edges > check)
                throw new ArgumentException($"The number of edges is larger than the maximum number of edges (= {check}). " +
                    $"The max number of edges is n*(n-1)/2, where n is the number of nodes.");

            // init values
            _random = random;
            // populate matrix
            double split = (double)edges / check;

            if (!isFullyConnected)
            {
                if (split < 0.5)
                {
                    Debug.WriteLine("Use the regular Erdős–Rényi model");
                    Enumerable
                        .Range(0, edges)
                        .AsParallel()
                        //.WithDegreeOfParallelism(1) // single threaded (for debug)
                        .ForAll(i =>
                        {
                            Debug.WriteLine($"[{i}] Thread ID:{Thread.CurrentThread.ManagedThreadId}");

                            bool connected = false;

                            while (!connected)
                            {
                                int a = _random.Next(nodes - 1);
                                int b = _random.Next(nodes - 1);

                                if (a != b && !IsConnected(a, b))
                                {
                                    Connect(a, b);
                                    connected = true;
                                }

                            }
                        });
                }
                else
                {
                    Debug.WriteLine("Use the reverse Erdős–Rényi model");
                    // create fully connected graph
                    Enumerable.Range(0, nodes)
                        .AsParallel()
                        .ForAll(i =>
                        {
                            for (int j = i; j < nodes; j++)
                            {
                                if (i != j)
                                    Connect(i, j);
                            }
                        });

                    while (NumberOfEdges != edges)
                    {
                        int a = _random.Next(nodes - 1);
                        int b = _random.Next(nodes - 1);

                        if (a != b && IsConnected(a, b))
                            Disconnect(a, b);
                    }
                }
            }
            else
            {
                if (split < 0.5)
                {
                    Debug.WriteLine("Use the regular Erdős–Rényi model");

                    for (int i = 0; i < nodes - 1; i++)
                    {
                        var degree = NodeDegrees[i + 1];
                        if (degree != 0)
                            continue;
                        Connect(i, i + 1);
                    }

                    Enumerable
                        .Range(0, edges - this.NumberOfEdges)
                        .AsParallel()
                        //.WithDegreeOfParallelism(1) // single threaded (for debug)
                        .ForAll(i =>
                        {
                            Debug.WriteLine($"[{i}] Thread ID:{Thread.CurrentThread.ManagedThreadId}");

                            bool connected = false;

                            while (!connected)
                            {
                                int a = _random.Next(nodes - 1);
                                int b = _random.Next(nodes - 1);

                                if (a != b && !IsConnected(a, b))
                                {
                                    Connect(a, b);
                                    connected = true;
                                }
                            }
                        });

                    // check unconnected nodes
                    var unconnectedNodes = NodeDegrees
                        .Select((index, degree) => new { index, degree })
                        .Where(node => node.degree == 0)
                        .Select(node => node.index)
                        .ToArray();

                }
                else
                {
                    Debug.WriteLine("Use the reverse Erdős–Rényi model");
                    // create fully connected graph
                    Enumerable.Range(0, nodes)
                        .AsParallel()
                        .ForAll(i =>
                        {
                            for (int j = i; j < nodes; j++)
                            {
                                if (i != j)
                                    Connect(i, j);
                            }
                        });

                    while (NumberOfEdges != edges)
                    {
                        int a = _random.Next(nodes - 1);
                        int b = _random.Next(nodes - 1);

                        if (a != b && IsConnected(a, b) && NodeDegrees[a] > 1 && NodeDegrees[b] > 1)
                            Disconnect(a, b);
                    }
                }
            }
        }
    }
}