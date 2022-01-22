using CA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tests
{
    [TestClass]
    public class TrajectoryTests
    {
        private Agent _agentA;
        private Agent _agentB;

        private Trajectory _trajectoryA;
        private Trajectory _trajectoryB;

        [TestInitialize]
        public void Init()
        {
            _agentA = new Agent();
            _agentB = new Agent();

            _agentA.Trajectories = CreateTrajectories();
            _agentB.Trajectories = CreateTrajectories();

            _trajectoryA = new Trajectory()
            {
                Success = 0.5
            };
            _trajectoryA.Randomize();

            _trajectoryB = new Trajectory()
            {
                Success = 0.5
            };
            _trajectoryB.Randomize();
        }

        private ICollection<Trajectory> CreateTrajectories()
        {
           return  Enumerable.Range(0, 4)
                        .Select(_ =>
                        {
                            var t = new Trajectory()
                            {
                                Success = 0.5,
                            };

                            t.Randomize();

                            return t;
                        })
                        .ToArray();
        }

        [TestCleanup]
        public void Clean()
        {
            _agentA = null;
            _agentB = null;
            _trajectoryA = null;
            _trajectoryB = null;
        }

        [TestMethod]

        public void TestFindSameIndex()
        {
            var random = Program.RANDOM;
            var id = random.Next(4);
            var rT = _agentA.Trajectories.ElementAt(id);

            var res = _agentA.FindClosestIndex(rT);
            var res2 = _agentA.FindClosestIndex2(rT);

            Assert.AreEqual(id, res);
            Assert.AreEqual(id, res2);
        }

        [TestMethod]
        public void TestFindSameTraject()
        {
            var random = Program.RANDOM;
            var id = random.Next(4);
            var rT = _agentA.Trajectories.ElementAt(id);

            var res = _agentA.FindClosest(rT);
            var res2 = _agentA.FindClosest2(rT);

            for (int i = 0; i < res.Points.Count; i++)
            {
                Assert.AreEqual(rT.Points.ElementAt(i), res.Points.ElementAt(i));
            }

            for (int i = 0; i < res.Points.Count; i++)
            {
                Assert.AreEqual(rT.Points.ElementAt(i), res2.Points.ElementAt(i));
            }
        }

        [TestMethod]
        public void TestFindClosest()
        {
            var random = Program.RANDOM;
            var id = random.Next(4);
            var rT = _agentA.Trajectories.ElementAt(id);

            var res = _agentB.FindClosest(rT);
            var res2 = _agentB.FindClosest2(rT);

            for (int i = 0; i < res.Points.Count; i++)
            {
                Assert.AreEqual(res2.Points.ElementAt(i), res.Points.ElementAt(i));
            }

            //for (int i = 0; i < res.Points.Count; i++)
            //{
            //    Assert.AreEqual(rT.Points.ElementAt(i), res2.Points.ElementAt(i));
            //}
        }

        [TestMethod]
        public void TestShift()
        {
            double sigmaShift = 1.0;

            var points = _trajectoryA.Points;

            var res1 = _trajectoryA.Shift(sigmaShift).Points; // werkt niet
            var res2 = _trajectoryA.Shift2(sigmaShift).Points; // c++ versie
            var res3 = _trajectoryA.Shift3(sigmaShift).Points;  // psuedo code versie

            for (int i = 0; i < points.Count; i++)
            {
                var point = points.ElementAt(i);
                var res1po = res1.ElementAt(i);
                var res2po = res2.ElementAt(i);
                var res3po = res3.ElementAt(i);
                Debug.WriteLine($"{i} point = ({point.X}, {point.Y})");
                Debug.WriteLine($"{i} point = ({res1po.X}, {res1po.Y})");
                Debug.WriteLine($"{i} point = ({res2po.X}, {res2po.Y})");
                Debug.WriteLine($"{i} point = ({res3po.X}, {res3po.Y})");
                Debug.WriteLine("===================================================");
            }

            Assert.AreNotEqual(points, res1);
        }

        [TestMethod]
        public void TestZip()
        {
            double[] xs = { 0.5, 0.2, 1.2, 4.56, 12.365, 02 };
            double[] ys = { 1.7, 5.3, 0.2, 1.02, 0, 2.7 };

            List<TrajectoryPoint> points = new List<TrajectoryPoint>()
            {
                new TrajectoryPoint(xs[0], ys[0]),
                new TrajectoryPoint(xs[1], ys[1]),
                new TrajectoryPoint(xs[2], ys[2]),
                new TrajectoryPoint(xs[3], ys[3]),
                new TrajectoryPoint(xs[4], ys[4]),
                new TrajectoryPoint(xs[5], ys[5]),
            };

            var res = xs.Zip(ys, (x, y) => new TrajectoryPoint(x, y)).ToArray();

            for (int i = 0; i < points.Count; i++)
            {
                var p = points.ElementAt(i);
                Assert.AreEqual(p, res[i]);
            }
        }

        [TestMethod]
        public void TestAddNoise()
        {
            double sigmaNoise = 2.0;
            var points = _trajectoryA.Points;

            var res1 = _trajectoryA.AddNoise(sigmaNoise).Points;
            var res2 = _trajectoryA.AddNoise2(sigmaNoise).Points;
            var res3 = _trajectoryA.AddNoise3(sigmaNoise).Points;
            var res4 = _trajectoryA.AddNoise4(sigmaNoise).Points;

            for (int i = 0; i < points.Count; i++)
            {
                var point = points.ElementAt(i);
                var res1po = res1.ElementAt(i);
                var res2po = res2.ElementAt(i);
                var res3po = res3.ElementAt(i);
                var res4po = res4.ElementAt(i);

                Debug.WriteLine($"{i} point = ({point.X}, {point.Y})");
                Debug.WriteLine($"{i} point = ({res1po.X}, {res1po.Y})");
                Debug.WriteLine($"{i} point = ({res2po.X}, {res2po.Y})");
                Debug.WriteLine($"{i} point = ({res3po.X}, {res3po.Y})");
                Debug.WriteLine($"{i} point = ({res4po.X}, {res4po.Y})");
                Debug.WriteLine("===================================================");
            }

            Assert.AreNotEqual(points, res1);
        }

        [TestMethod]
        public void TestMix()
        {
            double beta = 0.5;
            var points = _trajectoryA.Points;

            var res1 = _trajectoryA.Mix(_trajectoryB, beta).Points;
            var res2 = _trajectoryA.Mix2(_trajectoryB, beta).Points;


            Assert.AreNotEqual(points, res1);
            Assert.AreNotEqual(points, res2);

            for (int i = 0; i < points.Count; i++)
            {
                var point = points.ElementAt(i);
                var res1po = res1.ElementAt(i);
                var res2po = res2.ElementAt(i);

                Debug.WriteLine($"{i} point = ({point.X}, {point.Y})");
                Debug.WriteLine($"{i} point = ({res1po.X}, {res1po.Y})");
                Debug.WriteLine($"{i} point = ({res2po.X}, {res2po.Y})");
                Debug.WriteLine("===================================================");
                Assert.AreEqual(res1po, res2po);
            }
        }

        [TestMethod]
        public void TestShift3Performance()
        {
            var stopwatch = Stopwatch.StartNew();

            var runs = 10000000;
            for (int i = 0; i < runs; i++)
                _trajectoryA.Shift3(1);

            stopwatch.Stop();

            TimeSpan t = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            string elapsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            Debug.WriteLine($"run time: {elapsedTime}ms");
            Console.WriteLine($"run time: {elapsedTime}ms");
        }

        [TestMethod]
        public void TestShift2Performance()
        {
            var stopwatch = Stopwatch.StartNew();

            var runs = 10000000;
            for (int i = 0; i < runs; i++)
                _trajectoryA.Shift2(1);

            stopwatch.Stop();

            TimeSpan t = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            string elapsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            Debug.WriteLine($"run time: {elapsedTime}ms");
            Console.WriteLine($"run time: {elapsedTime}ms");
        }

        [TestMethod]
        public void TestMixPerformance()
        {
            var stopwatch = Stopwatch.StartNew();

            var runs = 10000000;

            double beta = 0.5;

            for (int i = 0; i < runs; i++)
                _trajectoryA.Mix(_trajectoryB, beta);

            stopwatch.Stop();

            TimeSpan t = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            string elapsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            Debug.WriteLine($"run time: {elapsedTime}ms");
            Console.WriteLine($"run time: {elapsedTime}ms");
        }
    }
}
