using System;
using System.Linq;

namespace Traject4
{
    public class Agent
    {
        public int TrajNum { get; private set; }
        public Trajectory[] Trajectories { get; private set; }
        public double[] Success { get; set; }

        public int ShiftIndex { get; private set; }

        private BackUp backup;
        private Trajectory Noisy;

        public void Initialise(int trajNum)
        {
            TrajNum = trajNum;

            Trajectories = new Trajectory[TrajNum];
            for (int i = 0; i < TrajNum; i++)
            {
                Trajectories[i] = new Trajectory();
                Trajectories[i].Randomize();
            }

            Success = Enumerable.Repeat(50.0, Program.TRAJECTORY_LENGTH).ToArray();
        }

        internal class BackUp
        {
            public double[] X { get; set; }
            public double[] Y { get; set; }
        }

        public void PrepareShift(double sigmaShift)
        {
            ShiftIndex = Program.RANDOM.Next(Program.TRAJ_NUM);

            var trajectToShift = Trajectories[ShiftIndex];

            backup = new BackUp()
            {
                X = trajectToShift.X.ToArray(),
                Y = trajectToShift.Y.ToArray(),
            };

            trajectToShift.Shift(sigmaShift);
        }

        public Trajectory Say()
        {
            var toSay = Trajectories[ShiftIndex];

            Noisy = new Trajectory()
            {
                X = toSay.X.ToArray(),
                Y = toSay.Y.ToArray(),
            };

            Noisy.AddNoise(Program.SIGMA_NOISE);

            return Noisy;
        }

        public Trajectory Imitate(Trajectory tSaid)
        {
            double bestDist = tSaid.SimpleDist(Trajectories[0], -1);

            int bestT = 0;

            for (int i = 1; i < Program.TRAJ_NUM; i++)
            {
                var dist = tSaid.SimpleDist(Trajectories[i], bestDist);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = i;
                }
            }

            return Trajectories[bestT];
        }

        public bool Listen(Trajectory tImit)
        {
            double bestDist = tImit.SimpleDist(Trajectories[0], -1);

            int bestT = 0;

            for (int i = 1; i < Program.TRAJ_NUM; i++)
            {
                var dist = tImit.SimpleDist(Trajectories[i], bestDist);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = i;
                }
            }

            return bestT == ShiftIndex;
        }

        internal void AcceptOrReject(int success)
        {
            if (success < this.Success[ShiftIndex]) // no improvement
            {
                var t = new Trajectory()
                {
                    X = backup.X.ToArray(),
                    Y = backup.Y.ToArray()
                };
                backup = null;

                Trajectories[ShiftIndex] = t;
            }
            else
            {
                var t = new Trajectory()
                {
                    X = backup.X.ToArray(),
                    Y = backup.Y.ToArray()
                };
                backup = null;

                var curr = Trajectories[ShiftIndex];

                curr.Mix(t, Program.BETA);
            }
            Success[ShiftIndex] = Program.BETA * ((double)success) + (1.0 - Program.BETA) * Success[ShiftIndex];
        }
    }
}
