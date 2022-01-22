using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace CA
{

    public class Trajectory
    {
        public ICollection<TrajectoryPoint> Points { get; set; }
        public double Success { get; set; }

        public static double Min
        {
            get => -(double)Program.SPACE_SIZE/2.0;
        }
        public static double Max
        {
            get => (double)Program.SPACE_SIZE/2.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static double Distance(Trajectory t1, Trajectory t2)
        {
            double dist = t1.Points.Zip(t2.Points, (a, b) => a - b)
                .Select(p => p.Norm()).Sum();

            return dist;
        }

        internal double SimpleDist(Trajectory t, double best)
        {
            double totD = Points.ElementAt(0).Dist(t.Points.ElementAt(0));

            if (best == -1)
            {
                for (int i = 1; i < Program.TRAJECTORY_LENGTH; i++)
                {
                    totD += Points.ElementAt(i).Dist(t.Points.ElementAt(i));  
                }
            } else
            {
                for (int i = 1; (i< Program.TRAJECTORY_LENGTH) && (totD < best); i++)
                {
                    totD += Points.ElementAt(i).Dist(t.Points.ElementAt(i));
                }
            }

            return totD;
        }

        public override bool Equals(object obj)
        {
            var t = (Trajectory)obj;

            for (int i = 0; i < Program.TRAJECTORY_LENGTH; i++)
            {
                var thisP = Points.ElementAt(i);
                var other = t.Points.ElementAt(i);

                if (thisP != other)
                    return false;
            }

            return true;
        }

        internal Trajectory Clone()
            => new Trajectory()
            {
                Success = this.Success,
                Points = this.Points.Select(t => t * 1.0).ToArray()
            };

        //todo add random to function parameters
        public void Randomize()
        {
            var points = new TrajectoryPoint[Program.TRAJECTORY_LENGTH];

            double min = -Program.TRAJECTORY_LENGTH / 2.0;
            double max = Program.TRAJECTORY_LENGTH / 2.0;

            double scale = max - min;

            var x = scale * (0.1 * Program.RANDOM.NextDouble() + 0.45) + min;
            var y = scale * (0.1 * Program.RANDOM.NextDouble() + 0.45) + min;

            points[0] = new TrajectoryPoint(x, y);

            for (int i = 0; i < Program.TRAJECTORY_LENGTH - 1; i++)
            {
                var p = points[i];
                do
                {
                    var angle = 2 * Math.PI * Program.RANDOM.NextDouble() - Math.PI;
                    x = p.X + Math.Cos(angle);
                    y = p.Y + Math.Sin(angle);

                    points[i + 1] = new TrajectoryPoint(x, y);

                } while (NoFit(x, y));
            }

            this.Points = points;
        }

        private bool NoFit(double x, double y)
        {
            double min = -Program.SPACE_SIZE / 2.0;
            double max = Program.SPACE_SIZE / 2.0;

            return (x < min) || (x > max) || (y < min) || (y > max);
        }
    }
}
