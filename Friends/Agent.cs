using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Friends
{
    /// <summary>
    /// Represents an agent in the simulation.
    /// </summary>
    public class Agent
    {
        private Backup _backup;

        /// <summary>
        /// A unique identifier.
        /// This is also the node number in the network.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// A friends list.
        /// This contains all <see cref="Agent"/>s that are directly connected to this <see cref="Agent"/>.
        /// </summary>
        public ICollection<Agent> Friends { get; set; }

        ///// <summary>
        ///// Ex-friend list.
        ///// These are the <see cref="Agent"/>s which were at some point in time friends with this <see cref="Agent"/>, but had a falling out and are not on speaking terms.
        ///// </summary>
        //public ICollection<Agent> Enemies { get; set; }

        /// <summary>
        /// The trajectories an <see cref="Agent"/> knows.
        /// </summary>
        public Trajectory[] Trajectories { get; private set; }

        /// <summary>
        /// Index of the <see cref="Trajectory"/> that has been shifted.
        /// </summary>
        public int ShiftIndex { get; private set; }

        /// <summary>
        /// An index-array containing the succes values of the <see cref="Trajectories"/> an <see cref="Agent"/> knows.
        /// </summary>
        internal double[] Success { get; set; }

        /// <summary>
        /// Initialize the <see cref="Agent"/> with random <see cref="Trajectories"/>.
        /// </summary>
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

        /// <summary>
        /// Select a random <see cref="Trajectory"/> and apply a shift.
        /// A backup will be made of the <see cref="Trajectory"/> before the shift is applied.
        /// </summary>
        /// <seealso cref="Trajectory.Shift"/>
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

        /// <summary>
        /// Return the shifted <see cref="Trajectory"/> with applied noise.
        /// </summary>
        /// <returns>A noisy version of the shifted <see cref="Trajectory"/></returns>
        /// <seealso cref="Trajectory.AddNoise"/>
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

        /// <summary>
        /// Find the <see cref="Trajectory"/> most similar to <paramref name="t"/>.
        /// </summary>
        /// <param name="t">A <see cref="Trajectory"/> spoken by another <see cref="Agent"/></param>
        /// <returns>The closest <see cref="Trajectory"/> the <see cref="Agent"/> knows given <paramref name="t"/></returns>
        /// <seealso cref="Agent.Listen(Trajectory)"/>
        /// <seealso cref="Agent.Say"/>
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

        /// <summary>
        /// Find the index <see cref="Trajectory"/> most similar to <paramref name="t"/> and check if this is the same as the shifted <see cref="Trajectory"/>.
        /// </summary>
        /// <param name="t">A <see cref="Trajectory"/> spoken by another <see cref="Agent"/></param>
        /// <returns>A <see cref="bool"/> indicating if <paramref name="t"/> matches the shifted <see cref="Trajectory"/></returns>
        /// <seealso cref="Agent.PrepareShift"/>
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

        /// <summary>
        /// Accept or reject the shifted <see cref="Trajectory"/> based on the success <paramref name="counter"/> of this <see cref="Trajectory"/> compared to the success of that <see cref="Trajectory"/> before the shift.
        /// </summary>
        /// <param name="counter">A success counter for the shifted <see cref="Trajectory"/></param>
        /// <see cref="Agent.PrepareShift"/>
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

            Success[ShiftIndex] = Program.BETA * ((double)counter) + (1.0 - Program.BETA) * Success[ShiftIndex];
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
