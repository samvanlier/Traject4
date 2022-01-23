using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Friends
{
    public class Agent
    {
        private Backup _backup;

        //public ICollection<Agent> Friends { get; set; }

        public Trajectory[] Trajectories { get; private set; }

        public int ShiftIndex { get; private set; }

        internal double[] Success { get; set; }

        public void Initialize()
        {
            Trajectories = new Trajectory[Program.TRAJECTORY_NUMBER];
            for (int i = 0; i < Program.TRAJECTORY_NUMBER; i++)
            {
                this.Trajectories[i] = new Trajectory();
                this.Trajectories[i].Randomize();
            }

            Success = Enumerable.Repeat(50.0, Program.TRAJECTORY_LENGTH).ToArray();
        }

        public void PrepareShift()
        {
            this.ShiftIndex = Program.RANDOM.Next(Program.TRAJECTORY_NUMBER);

            var trajectoryToShift = Trajectories[ShiftIndex];

            // create hard copy backup!!!
            var xs = new double[Program.TRAJECTORY_LENGTH];
            var ys = new double[Program.TRAJECTORY_LENGTH];

            for (int i = 0; i < Program.TRAJECTORY_LENGTH; i++)
            {
                var point = trajectoryToShift.Points[i];

                xs[i] = point.X;
                ys[i] = point.Y;
            }

            this._backup = new Backup()
            {
                X = xs,
                Y = ys
            };

            trajectoryToShift.Shift();
        }

        public Trajectory Say()
        {
            var toSay = Trajectories[ShiftIndex];

            var noisy = new Trajectory()
            {
                Points = toSay.Points.Select(p => new TrajectoryPoint(p)).ToArray(),
            };
            noisy.AddNoise();

            return noisy;
        }

        public Trajectory Imitate(Trajectory t)
        {
            double bestDist = t.SimpleDist(this.Trajectories[0], -1);

            int bestT = 0;

            for (int i = 0; i < Program.TRAJECTORY_NUMBER; i++)
            {
                var dist = t.SimpleDist(Trajectories[i], bestDist);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = i;
                }
            }

            return this.Trajectories[bestT];
        }

        public bool Listen(Trajectory t)
        {
            double bestDist = t.SimpleDist(Trajectories[0], -1);

            int bestT = 0;

            for (int i = 1; i < Program.TRAJECTORY_NUMBER; i++)
            {
                var dist = t.SimpleDist(Trajectories[i], bestDist);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = i;
                }
            }

            return bestT == ShiftIndex;
        }

        public void AcceptOrReject(int counter)
        {
            if (counter < this.Success[ShiftIndex])
            {
                var t = new Trajectory()
                {
                    Points = BackupToTrajectory()
                };
                _backup = null;

                Trajectories[ShiftIndex] = t;
            }
            else
            {
                var t = new Trajectory()
                {
                    Points = BackupToTrajectory()
                };
                _backup = null;

                var curr = Trajectories[ShiftIndex];
                curr.Mix(t);
            }

            Success[ShiftIndex] = Program.BETA * ((double)counter) + (1.0 * Program.BETA) * Success[ShiftIndex];
        }

        private TrajectoryPoint[] BackupToTrajectory()
        {
            var points = new TrajectoryPoint[Program.TRAJECTORY_LENGTH];
            for (int i = 0; i < Program.TRAJECTORY_LENGTH; i++)
            {
                points[i] = new TrajectoryPoint(this._backup.X[i], this._backup.Y[i]);
            }

            return points;
        }

        class Backup
        {
            public double[] X { get; set; }
            public double[] Y { get; set; }
        }
    }
}
