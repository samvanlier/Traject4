using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Graph
{
    /// <summary>
    /// Represents a simple graph
    /// </summary>
    public abstract class Graph
    {
        /// <summary>
        /// An upper triangular matrix
        /// </summary>
        internal bool[,] Matrix { get; private set; }
        /// <summary>
        /// The number of nodes in the graph
        /// </summary>
        public int NumberOfNodes { get; protected set; }
        /// <summary>
        /// The number of edges in the graph
        /// </summary>
        public int NumberOfEdges { get; protected set; }

        /// <summary>
        /// A node-indexed array that contains the degree of the node at the index (aka the number of edges connected to that node)
        /// </summary>
        public ushort[] NodeDegrees { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph"/>.
        /// </summary>
        /// <param name="numberOfNodes">The number of nodes that are in the graph.</param>
        public Graph(int numberOfNodes)
        {
            NumberOfNodes = numberOfNodes;
            Matrix = new bool[numberOfNodes, numberOfNodes];

            NodeDegrees = Enumerable.Repeat((ushort)0, numberOfNodes).ToArray();
        }

        /// <summary>
        /// Connect two nodes with an edge (because this is a simple graph, the edge as no direction).
        ///
        /// Note that this method is <see cref="MethodImplOptions.Synchronized"/>.
        /// </summary>
        /// <param name="a">A node number to connect</param>
        /// <param name="b">Another node number to connect</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connect(int a, int b)
        {
            if (a == b)
                throw new ArgumentException("You are not allowed to connect a node to itself in this repressentation.");

            if (IsConnected(a, b)) // don't need to connect if already connected
                return;

            int i = Math.Min(a, b);
            int j = Math.Max(a, b);
            Matrix[i, j] = true;
            NumberOfEdges++;
            NodeDegrees[i] += 1;
            NodeDegrees[j] += 1;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Disconnect(int a, int b)
        {
            if (!IsConnected(a, b)) // don't need to disconnect if already disconnected
                return;

            int i = Math.Min(a, b);
            int j = Math.Max(a, b);
            Matrix[i, j] = false;

            NumberOfEdges--;
            NodeDegrees[i] -= 1;
            NodeDegrees[j] -= 1;
        }

        /// <summary>
        /// Check if two nodes are connected.
        /// </summary>
        /// <param name="a">A node</param>
        /// <param name="b">A node</param>
        /// <returns>A <see cref="bool"/> indicating if node <paramref name="a"/> and node <paramref name="b"/> are connected</returns>
        public bool IsConnected(int a, int b)
            => Matrix[a, b] || Matrix[b, a];

        /// <summary>
        /// Get the graph as a upper triangular matrix.
        /// </summary>
        /// <returns></returns>
        public int[,] GetMatrixForm() => Matrix.Clone() as int[,];

        public static int MaxNumberOfEdges(int nodes)
            => (int)Math.Ceiling(nodes * (nodes - 1.0) / 2.0);

        public string PrintMatrix()
            => Matrix.Print2DArray();
    }
}
