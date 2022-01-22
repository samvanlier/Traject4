using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace CA
{
    public static class TrajectoryExtentions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sigmaShift"></param>
        /// <returns></returns>
        public static Trajectory Shift(this Trajectory trajectory, double sigmaShift)
        {
            var pointsOut = trajectory.Points.Select(i => i.Clone()).ToArray();

            int i = Program.RANDOM.Next(Program.TRAJECTORY_LENGTH);

            pointsOut[i] = trajectory.Points.ElementAt(i) + NormRand(0, sigmaShift);

            for (int j = i; j < Program.TRAJECTORY_LENGTH - 1; j++)
            {
                var pointOutJ = pointsOut[j];
                var pointInJ1 = trajectory.Points.ElementAt(j + 1);
                //var dist = (pointOutJ - pointInJ1).Norm();
                var dist = pointOutJ.X * pointOutJ.X + pointOutJ.Y * pointOutJ.Y;

                if (dist > Program.MAX_DIST)
                {
                    dist = Math.Sqrt(dist);
                    pointsOut[j + 1] = (Program.MAX_DIST / dist) * (pointInJ1 - pointOutJ);
                }
                else
                    //pointsOut[j + 1] = pointInJ1.Clone();
                    break;
            }

            for (int j = i; j > 0; j--)
            {
                var pointOutJ = pointsOut[j];
                var pointInJ1 = trajectory.Points.ElementAt(j - 1);
                //var dist = (pointOutJ - pointInJ1).Norm();
                var dist = pointOutJ.X * pointOutJ.X + pointOutJ.Y * pointOutJ.Y;

                if (dist > Program.MAX_DIST)
                {
                    dist = Math.Sqrt(dist);
                    pointsOut[j - 1] = (Program.MAX_DIST / dist) * (pointInJ1 - pointOutJ);
                }
                else
                    //pointsOut[j - 1] = pointInJ1.Clone();
                    break;
            }

            return new Trajectory()
            {
                Points = pointsOut,
                Success = trajectory.Success
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public static Trajectory Mix(this Trajectory t1, Trajectory t2, double beta)
            => new Trajectory()
            {
                // todo test mixing
                Points = t1.Points.Zip(t2.Points, (a, b) => (beta * a) + (1 - beta) * b).ToArray(),
                Success = t1.Success
            };

        public static Trajectory Mix2(this Trajectory oldT, Trajectory newT, double beta)
        {
            var points = new List<TrajectoryPoint>();

            for (int i = 0; i < oldT.Points.Count; i++)
            {
                var o = oldT.Points.ElementAt(i);
                var n = newT.Points.ElementAt(i);

                var x = beta * n.X + (1 - beta) * o.X;
                var y = beta * n.Y + (1 - beta) * o.Y;

                var p = new TrajectoryPoint(x, y);
                points.Add(p);
            }

            return new Trajectory()
            {
                Points = points,
                Success = oldT.Success * 1.0
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tPure"></param>
        /// <param name="sigmaNoise"></param>
        /// <returns></returns>
        public static Trajectory AddNoise(this Trajectory tPure, double sigmaNoise)
        {
            var sStart = new TrajectoryPoint(NormRand(0, sigmaNoise), NormRand(0, sigmaNoise));
            var sEnd = new TrajectoryPoint(NormRand(0, sigmaNoise), NormRand(0, sigmaNoise));

            double n1 = Program.TRAJECTORY_LENGTH - 1.0;
            double sigmaPoints = sigmaNoise / Program.TRAJECTORY_LENGTH;

            var points = tPure.Points.Select((point, i) =>
            {
                double alpha = (i - 1.0) / (Program.TRAJECTORY_LENGTH - 1.0);

                return point
                 + (sStart * alpha)
                 + (sEnd * (1.0 - alpha))
                 + NormRand(0, sigmaPoints);
            }).ToArray();

            return new Trajectory()
            {
                Points = points,
                Success = tPure.Success
            };
        }


        public static Trajectory AddNoise2(this Trajectory tPure, double sigmaNoise)
        {
            var points = tPure.Points
                .Select(p => p + NormRand(0, sigmaNoise)).ToArray();

            return new Trajectory()
            {
                Success = tPure.Success,
                Points = points
            };
        }

        public static Trajectory AddNoise3(this Trajectory tPure, double sigmaNoise)
        {
            var start = new TrajectoryPoint(NormRand(0, sigmaNoise), NormRand(0, sigmaNoise));

            var step = new TrajectoryPoint(NormRand(0, sigmaNoise), NormRand(0, sigmaNoise));
            step -= start;
            step /= (Program.TRAJECTORY_LENGTH - 1);

            var sd = sigmaNoise / Program.TRAJECTORY_LENGTH;

            var points = tPure.Points.Select(p =>
            {
                var r = p + new TrajectoryPoint(NormRand(start.X, sd), NormRand(start.Y, sd));
                start += step;
                return r;
            }).ToArray();

            return new Trajectory()
            {
                Success = tPure.Success,
                Points = points
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="stddev"></param>
        /// <returns></returns>
        private static double NormRand(double mean = 0, double stddev = 1)
        {
            var normalDist = new Normal(mean, stddev);
            return normalDist.Sample();
        }
    }
}
