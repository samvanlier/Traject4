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

            Points[pos] = Points[pos] + RandShift();

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

        internal void AddNoise()
        {
            Points = Points.Select(p => p + this.RandNoise()).ToArray();
        }

        private TrajectoryPoint RandNoise()
        {
            var x = Program.RandNoise();
            var y = Program.RandNoise();

            return new TrajectoryPoint(x, y);
        }

        private TrajectoryPoint RandShift()
        {
            var x = Program.RandShift();
            var y = Program.RandShift();

            return new TrajectoryPoint(x, y);
        }

        private void ReFit(int pos)
        {
            // check x value
            if (Points[pos].X < Min)
                Points[pos] = new TrajectoryPoint(Min, Points[pos].Y);
            else if (Points[pos].X > Max)
                Points[pos] = new TrajectoryPoint(Max, Points[pos].Y);

            //check y value
            if (Points[pos].Y < Min)
                Points[pos] = new TrajectoryPoint(Points[pos].X, Min);
            else if (Points[pos].Y > Max)
                Points[pos] = new TrajectoryPoint(Points[pos].X, Max);
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


        internal static double Distance(Trajectory t1, Trajectory t2)
            => t1.Points.Zip(t2.Points, (a, b) => TrajectoryPoint.EuclideanDistance(a, b)).Sum();

    }
}
