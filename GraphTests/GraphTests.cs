using System;
using System.Linq;
using Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GraphTests
{
    [TestClass]
    public class GraphTests
    {
        private Random _random;

        [TestInitialize]
        public void Init()
        {
            _random = new Random(1);
        }

        [TestCleanup]
        public void Clean()
        {
            _random = null;
        }

        [TestMethod]
        public void TestCreatingRandomGraph()
        {
            int n = 10; // nodes
            int k = 40; // edges

            var g = new RandomGraph(n, k, _random);

            Assert.AreEqual(k, g.NumberOfEdges);
            // test with handshake (https://en.wikipedia.org/wiki/Handshaking_lemma#Definitions_and_statement)
            int sumDegree = g.NodeDegrees.Select(i => (int)i).Sum();
            Assert.AreEqual(k * 2, sumDegree);
        }
    }
}
