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

            pointsOut[i] = trajectory.Points.ElementAt(i) + NormRandPoint(0, sigmaShift);

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

        // c++ versie (werkt een beetje anders dan psuedo code)
        public static Trajectory Shift2(this Trajectory trajectory, double sigmaShift)
        {
            int i = Program.RANDOM.Next(Program.TRAJECTORY_LENGTH);

            var points = trajectory.Points.Select(i => i  *1).ToArray();

            // todo optimize array setting
            points[i] = points[i] + NormRandPoint(0, sigmaShift);

            points[i] = ReFit(points[i]);

            bool shifted = true;
            for (int index = i +1; (index < Program.TRAJECTORY_LENGTH) && shifted; index++)
            {
                var r = ShiftPoint(points[index-1], points[index], out shifted);
                points[index] = r;
            }

            shifted = true;
            for (int index = i-1; (index >= 0) && shifted; index--)
            {
                var r = ShiftPoint(points[index+1], points[index], out shifted);
                points[index] = r;
            }

            return new Trajectory()
            {
                Success = 0.0,
                Points = points,
            };
        }

        // pseodo code (werkt)
        public static Trajectory Shift3(this Trajectory trajectory, double sigmaShift)
        {
            //todo optimize?
            //var nd = new Normal(0, sigmaShift);
            var nd = Program.ShiftNormal;
            var maxDist = Program.MAX_DIST * Program.MAX_DIST;

            var xs = trajectory.Points.Select(p => p.X).ToArray();
            var ys = trajectory.Points.Select(p => p.Y).ToArray();

            int pos = Program.RANDOM.Next(Program.TRAJECTORY_LENGTH);

            xs[pos] = xs[pos] + nd.Sample();
            ys[pos] = ys[pos] + nd.Sample();

            for (int j = pos; j < Program.TRAJECTORY_LENGTH - 1; j++)
            {
                var dist = (xs[j] - xs[j+1]) * (xs[j] - xs[j+1]) + (ys[j] - ys[j+1])*(ys[j] - ys[j+1]);

                //todo optimze
                // sqrt(a) > dist <=> a > dist*dist
                // note: dist > 0
                if (dist > maxDist)
                {
                    dist = Math.Sqrt(dist);
                    xs[j+1] = (Program.MAX_DIST / dist) * (xs[j+1]-xs[j]);
                    ys[j+1] = (Program.MAX_DIST / dist) * (ys[j+1]-ys[j]);
                }
                //else
                //{
                //    xs[j+1] = xs[j+1];
                //    ys[j+1] = ys[j+1];
                //}
            }

            for (int j = pos; j > 0; j--)
            {
                var dist = (xs[j] - xs[j-1]) * (xs[j] - xs[j-1]) + (ys[j] - ys[j-1])*(ys[j] - ys[j-1]);

                //todo optimze
                // sqrt(a) > dist <=> a > dist*dist
                // note: dist > 0
                if (dist > maxDist)
                {
                    dist = Math.Sqrt(dist);
                    xs[j-1] = (Program.MAX_DIST / (dist)) * (xs[j-1]-xs[j]);
                    ys[j-1] = (Program.MAX_DIST / (dist)) * (ys[j-1]-ys[j]);
                }
            }

            var points = xs.Zip(ys, (x, y) => new TrajectoryPoint(x, y)).ToArray();
            return new Trajectory()
            {
                Points = points,
            };
        }

        private static TrajectoryPoint ShiftPoint(TrajectoryPoint goal, TrajectoryPoint curr, out bool shifted)
        {
            double difX = goal.X - curr.X;
            double difY = goal.Y - curr.Y;

            double dist = difX * difX + difY * difY;

            if (dist > 1)
            {
                dist = 1/Math.Sqrt(dist);
                var x = goal.X - (difX * dist);
                var y = goal.Y - (difY*dist);
                shifted = true;
                return new TrajectoryPoint(x, y);
            }
            
            shifted = false;
            return new TrajectoryPoint(curr.X, curr.Y);
        }

        private static TrajectoryPoint ReFit(TrajectoryPoint trajectoryPoint)
        {
            var x = trajectoryPoint.X;
            var y = trajectoryPoint.Y;

            if (x < Trajectory.Min)
                x = Trajectory.Min;
            else if (x > Trajectory.Max)
                x = Trajectory.Max;

            if (y < Trajectory.Min)
                y = Trajectory.Min;
            else if (y > Trajectory.Max)
                y = Trajectory.Max;


            return new TrajectoryPoint(x, y);
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
                Points = t1.Points.Zip(t2.Points, (a, b) => (beta * a) + (1 - beta) * b).ToArray(),
                Success = t1.Success
            };

        public static Trajectory Mix2(this Trajectory oldT, Trajectory newT, double beta)
        {
            var points = new List<TrajectoryPoint>();

            for (int i = 0; i < oldT.Points.Length; i++)
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
                Points = points.ToArray(),
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tPure"></param>
        /// <param name="sigmaNoise"></param>
        /// <returns></returns>
        public static Trajectory AddNoise2(this Trajectory tPure, double sigmaNoise)
        {
            //var points = tPure.Points
            //    .Select(p => p + NormRandPoint(0, sigmaNoise)).ToArray();

            var nb = Program.NoiseNormal;

            var points = tPure.Points
                .Select(p =>
                {
                    var x = nb.Sample();
                    var y = nb.Sample();

                    return p + new TrajectoryPoint(x, y);
                })
                .ToArray();

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

        // pseudo code
        public static Trajectory AddNoise4(this Trajectory tPure, double sigmaNoise)
        {
            var xs = tPure.Points.Select(p => p.X).ToArray();
            var ys = tPure.Points.Select(p => p.Y).ToArray();

            static double S(double stddev) => NormRand(0, stddev);

            double nPoints = Program.TRAJECTORY_LENGTH;

            for (int i = 0; i < xs.Length; i++)
            {
                var pureX = xs[i];
                var pureY = ys[i];

                xs[i] = pureX
                    + (S(nPoints- i) / (nPoints - 1.0))
                    + (S(i) / (nPoints -1.0))
                    + S(sigmaNoise/nPoints);

                ys[i] = pureY
                    + (S(nPoints- i) / (nPoints - 1.0))
                    + (S(i) / (nPoints -1.0))
                    + S(sigmaNoise/nPoints);
            }

            var points = xs.Zip(ys, (x, y) => new TrajectoryPoint(x, y)).ToArray();
            return new Trajectory()
            {
                Points = points,
                Success = tPure.Success,
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
            var normalDist = new Normal(mean, stddev, Program.RANDOM);
 
            return normalDist.Sample();
        }

        private static TrajectoryPoint NormRandPoint(double mean = 0, double stddev = 1)
        {
            var normalDist = new Normal(mean, stddev, Program.RANDOM);
            var x =  normalDist.Sample();
            var y = normalDist.Sample();

            return new TrajectoryPoint(x, y);
        }
    }
}
