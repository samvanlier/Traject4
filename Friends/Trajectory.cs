using System;
using System.Linq;

namespace Friends
{
    /// <summary>
    /// This is an abstract representation of a series of sounds.
    /// More details about this can be found in the <a href="https://journals.sagepub.com/doi/abs/10.1177/1059712309345789">original work</a> by de Boer and Zuidema.
    /// </summary>
    public class Trajectory
    {
        private double Min { get => -Program.SPACE_SIZE / 2.0; }
        private double Max { get => +Program.SPACE_SIZE / 2.0; }

        /// <summary>
        /// An array that makes up a trajectory.
        /// </summary>
        public TrajectoryPoint[] Points { get; set; }

        /// <summary>
        /// Cleanup the existing <see cref="Points"/> and create a new set of <see cref="TrajectoryPoint"/>s that are randomized.
        /// </summary>
        public void Randomize()
        {
            Points = null;
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

        /// <summary>
        /// Mix this <see cref="Trajectory"/> with another <see cref="Trajectory"/>.
        /// </summary>
        /// <param name="oldTrajectory">A <see cref="Trajectory"/> to mix with this <see cref="Trajectory"/></param>
        /// <remarks>
        /// the mix is done with a mix factor <see cref="Program.BETA"/>.
        /// </remarks>
        /// <see cref="Program.BETA"/>
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

        /// <summary>
        /// Shift the <see cref="Trajectory.Points"/> using some shift noise (<see cref="Program.SIGMA_SHIFT"/>). 
        /// </summary>
        internal void Shift()
        {
            int pos = Program.RANDOM.Next(Program.TRAJECTORY_LENGTH);

            var x = Points[pos].X + Program.RandShift();
            var y = Points[pos].Y + Program.RandShift();
            Points[pos] = new TrajectoryPoint(x, y);

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

        /// <summary>
        /// Refit the point at <paramref name="pos"/> so that it fits within the boundaries set by <see cref="Program.SPACE_SIZE"/>.
        /// </summary>
        /// <param name="pos">The index of the point to refit</param>
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
            if (dist > Program.MAX_DIST * Program.MAX_DIST)
            {
                dist = Program.MAX_DIST / Math.Sqrt(dist);
                var xToSet = x - difx * dist;
                var yToSet = y - dify * dist;
                Points[indexToSet] = new TrajectoryPoint(xToSet, yToSet);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculate the distance between this <see cref="Trajectory"/> an another.
        /// </summary>
        /// <param name="t">A <see cref="Trajectory"/> to calculate the distance from</param>
        /// <param name="best">
        /// A threshold to use as cut-off when searching for the best distance.
        /// if <paramref name="best"/> is <c>-1</c> or less, it calculates the full distance instead of cutting short if the calculated distance is alread greater than <paramref name="best"/>.
        /// </param>
        /// <returns>The distance between this <see cref="Trajectory"/> and <paramref name="t"/></returns>
        /// <remarks>
        /// The cut-off threshold is designed to optimize search.
        /// </remarks>
        internal double SimpleDist(Trajectory t, double best = -1)
        {
            double totD = TrajectoryPoint.EuclideanDistance(t.Points[0], this.Points[0]);

            if (best <= -1)
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

        /// <summary>
        /// Add noise to the <see cref="Trajectory.Points"/>.
        /// </summary>
        /// <remarks>
        /// The noise is Guassian noise with a mean = <c>0</c> and a standard deviation = <see cref="Program.SIGMA_NOISE"/>.
        /// For every point, a new value is pulled from the Gaussion noise function.
        /// </remarks>
        /// <seealso cref="Program.SIGMA_NOISE"/>
        /// <seealso cref="Program.RandNoise"/>
        internal void AddNoise()
        {
            for (int i = 0; i < Program.TRAJECTORY_LENGTH; i++)
            {
                var x = Points[i].X + Program.RandNoise();
                var y = Points[i].Y + Program.RandNoise();

                Points[i] = new TrajectoryPoint(x, y);
            }
        }

    }
}
