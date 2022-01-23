using System;
using System.Linq;

namespace Friends
{
    public class Trajectory
    {
        public double Min { get => -Program.SPACE_SIZE / 2.0; }
        public double Max { get => +Program.SPACE_SIZE / 2.0; }

        public TrajectoryPoint[] Points { get; set; }


        public void Randomize()
        {
            double scale = Max - Min;

            if (Points == null || Points.Length == 0)
                Points = new TrajectoryPoint[Program.TRAJECTORY_LENGTH];

            double x = scale * (0.1 * Program.RANDOM.NextDouble() + 0.45) + Min;
            double y = scale * (0.1 * Program.RANDOM.NextDouble() + 0.45) + Min;

            Points[0] = new TrajectoryPoint(x, y);

            for (int i = 0; i < Program.TRAJECTORY_LENGTH - 1; i++)
            {
                do
                {
                    double angle = 2.0 * Math.PI * Program.RANDOM.NextDouble() - Math.PI;

                    var curr = Points[i];

                    x = curr.X + Math.Cos(angle);
                    y = curr.Y + Math.Sin(angle);

                    Points[i + 1] = new TrajectoryPoint(x, y);
                }
                while (NoFit(x, y));
            }
        }

        public void Mix(Trajectory oldTrajectory)
        {
            for (int i = 0; i < Program.TRAJECTORY_LENGTH; i++)
            {
                var curr = Points[i];
                var old = oldTrajectory.Points[i];

                var x = Program.BETA * curr.X + (1.0 - Program.BETA) * old.X;
                var y = Program.BETA * curr.Y + (1.0 - Program.BETA) * old.Y;

                this.Points[i] = new TrajectoryPoint(x, y);
            }
        }

        private bool NoFit(double x, double y)
            => (x < Min) || (x > Max) || (y < Min) || (y > Max);

        internal void Shift()
        {
            int pos = Program.RANDOM.Next(Program.TRAJECTORY_LENGTH);

            var x = Points[pos].X + Program.RandShift();
            var y = Points[pos].Y + Program.RandShift();
            Points[pos] = new TrajectoryPoint(x, y);
            //Points[pos] = Points[pos] + RandShift();

            ReFit(pos);

            bool shifted = true;
            for (int i = pos + 1; (i < Program.TRAJECTORY_LENGTH) && shifted; i++)
            {
                var p = Points[i - 1];
                shifted = ShiftPoint(p.X, p.Y, i);
            }

            shifted = true;
            for (int i = pos - 1; (i >= 0) && shifted; i--)
            {
                var p = Points[i + 1];
                shifted = ShiftPoint(p.X, p.Y, i);
            }
        }

        internal double SimpleDist(Trajectory t, double best)
        {
            double totD = TrajectoryPoint.EuclideanDistance(t.Points[0], this.Points[0]);

            if (best == -1)
            {
                for (int i = 1; i < Program.TRAJECTORY_LENGTH; i++)
                    totD += TrajectoryPoint.EuclideanDistance(t.Points[i], this.Points[i]);
            }
            else
            {
                for (int i = 1; (i < Program.TRAJECTORY_LENGTH) && (totD < best); i++)
                    totD += TrajectoryPoint.EuclideanDistance(t.Points[i], this.Points[i]);
            }

            return totD;
        }

        internal void AddNoise()
        {
            for (int i = 0; i < Program.TRAJECTORY_LENGTH; i++)
            {
                var x = Points[i].X + Program.RandNoise();
                var y = Points[i].Y + Program.RandNoise();

                Points[i] = new TrajectoryPoint(x, y);
            }
        }

        private void ReFit(int pos)
        {
            var x = Points[pos].X;
            var y = Points[pos].Y;

            if (x < Min)
                x = Min;
            else if (x > Max)
                x = Max;

            if (y < Min)
                y = Min;
            else if (y > Max)
                y = Max;

            Points[pos] = new TrajectoryPoint(x, y);
        }

        private bool ShiftPoint(double x, double y, int indexToSet)
        {
            double difx = x - Points[indexToSet].X;
            double dify = y - Points[indexToSet].Y;

            // sqrt(a) >= b
            // <=>
            // a >= b*b
            var dist = (difx * difx) + (dify * dify);
            if (dist >= Program.MAX_DIST * Program.MAX_DIST)
            {
                dist = Program.MAX_DIST / Math.Sqrt(dist);
                var xToSet = x - difx * dist;
                var yToSet = y - dify * dist;
                Points[indexToSet] = new TrajectoryPoint(xToSet, yToSet);
                return true;
            }

            return false;
        }

        //internal static double Distance(Trajectory t1, Trajectory t2)
        //    => t1.Points.Zip(t2.Points, (a, b) => TrajectoryPoint.EuclideanDistance(a, b)).Sum();

    }
}
