using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TestAgainstTraject4
    {
        private Traject4.Agent _t4Agent;
        private Friends.Agent _friendsAgent;

        [TestInitialize]
        public void Initialize()
        {
            _t4Agent = new Traject4.Agent();
            _t4Agent.Initialise(Traject4.Program.TRAJ_NUM);

            _friendsAgent = new Friends.Agent();
            _friendsAgent.Initialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _t4Agent = null;
            _friendsAgent = null;
        }

        [TestMethod]
        public void TestTrajectoryInit()
        {
            var t4Traject = new Traject4.Trajectory();
            t4Traject.Randomize();

            var friendsTraject = new Friends.Trajectory();
            friendsTraject.Randomize();

            for (int i = 0; i < friendsTraject.Points.Length; i++)
            {
                var point = friendsTraject.Points[i];
                var x = t4Traject.X[i];
                var y = t4Traject.Y[i];

                Assert.AreEqual(x, point.X);
                Assert.AreEqual(y, point.Y);
            }
        }

        [TestMethod]
        public void TestAgentInit()
        {
            _t4Agent = new Traject4.Agent();
            _t4Agent.Initialise(Traject4.Program.TRAJ_NUM);

            _friendsAgent = new Friends.Agent();
            _friendsAgent.Initialize();

            for (int i = 0; i < Traject4.Program.TRAJ_NUM; i++)
            {
                var t4 = _t4Agent.Trajectories[i];
                var friends = _friendsAgent.Trajectories[i];

                for (int j = 0; j < Traject4.Program.TRAJECTORY_LENGTH; j++)
                {
                    var point = friends.Points[j];
                    var x = t4.X[j];
                    var y = t4.Y[j];

                    Assert.AreEqual(x, point.X);
                    Assert.AreEqual(y, point.Y);
                }
            }
        }

        [TestMethod]
        public void TestShift()
        {
            _t4Agent.PrepareShift(Traject4.Program.SIGMA_SHIFT);
            _friendsAgent.PrepareShift();

            for (int i = 0; i < Traject4.Program.TRAJ_NUM; i++)
            {
                var t4 = _t4Agent.Trajectories[i];
                var friends = _friendsAgent.Trajectories[i];

                for (int j = 0; j < Traject4.Program.TRAJECTORY_LENGTH; j++)
                {
                    var point = friends.Points[j];
                    var x = t4.X[j];
                    var y = t4.Y[j];

                    Assert.AreEqual(x, point.X);
                    Assert.AreEqual(y, point.Y);
                }
            }
        }

        [TestMethod]
        public void TestSay()
        {
            _t4Agent.PrepareShift(Traject4.Program.SIGMA_SHIFT);
            _friendsAgent.PrepareShift();

            var n1 = _t4Agent.Say();
            var n2 = _friendsAgent.Say();

            for (int i = 0; i < n2.Points.Length; i++)
            {
                var point = n2.Points[i];
                var x = n1.X[i];
                var y = n1.Y[i];

                Assert.AreEqual(x, point.X);
                Assert.AreEqual(y, point.Y);
            }
        }

        [TestMethod]
        public void TestImitate()
        {
            var t4Traject = new Traject4.Trajectory();
            t4Traject.Randomize();
            var t4Imitation = _t4Agent.Imitate(t4Traject);

            var friendsTraject = new Friends.Trajectory();
            friendsTraject.Randomize();
            var friendsImitation = _friendsAgent.Imitate(friendsTraject);

            for (int i = 0; i < friendsImitation.Points.Length; i++)
            {
                var point = friendsImitation.Points[i];
                var x = t4Imitation.X[i];
                var y = t4Imitation.Y[i];

                Assert.AreEqual(x, point.X);
                Assert.AreEqual(y, point.Y);
            }

        }

        [TestMethod]
        public void TestListen()
        {
            var t4Traject = new Traject4.Trajectory();
            t4Traject.Randomize();
            var t4Imitation = _t4Agent.Listen(t4Traject);

            var friendsTraject = new Friends.Trajectory();
            friendsTraject.Randomize();
            var friendsImitation = _friendsAgent.Listen(friendsTraject);

            Assert.AreEqual(t4Imitation, friendsImitation);
        }

        [TestMethod]
        public void TestMix()
        {
            var t4Traject = new Traject4.Trajectory();
            t4Traject.Randomize();

            var friendsTraject = new Friends.Trajectory();
            friendsTraject.Randomize();

            for (int i = 0; i < friendsTraject.Points.Length; i++)
            {
                var point = friendsTraject.Points[i];
                var x = t4Traject.X[i];
                var y = t4Traject.Y[i];

                Assert.AreEqual(x, point.X);
                Assert.AreEqual(y, point.Y);
            }


            _t4Agent.Trajectories[0].Mix(t4Traject, 0.5);
            _friendsAgent.Trajectories[0].Mix(friendsTraject);

            //for (int i = 0; i < Traject4.Program.TRAJ_NUM; i++)
            //{
            //    var t4 = _t4Agent.Trajectories[i];
            //    var friends = _friendsAgent.Trajectories[i];

            //    for (int j = 0; j < Traject4.Program.TRAJECTORY_LENGTH; j++)
            //    {
            //        var point = friends.Points[j];
            //        var x = t4.X[j];
            //        var y = t4.Y[j];

            //        Assert.AreEqual(x, point.X);
            //        Assert.AreEqual(y, point.Y);
            //    }
            //}
        }
    }
}
