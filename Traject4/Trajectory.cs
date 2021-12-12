using System;
using MathNet.Numerics.Distributions;

namespace Traject4
{
    public class Trajectory : ICloneable
    {
        private readonly Random _random;

        public int Id { get; set; }
        public double[] m_X { get; set; }
        public double[] m_Y { get; set; }
        public double m_MinX { get; set; }
        public double m_MinY { get; set; }
        public double m_MaxX { get; set; }
        public double m_MaxY { get; set; }

        public Trajectory(int id)
        {
            Id = id;
            m_MinX = m_MinY = -Program.SpaceSize / 2;
            m_MaxX = m_MaxY = Program.SpaceSize / 2;
            _random = new Random();

            // init arrays
            m_X = new double[Program.TrajLength];
            m_Y = new double[Program.TrajLength];

            Randomize();
        }

        private void Randomize()
        {
            double xscale = m_MaxX - m_MinX;
            double yscale = m_MaxY - m_MinY;

            // note: randomization only takes place in the middle
            m_X[0] = xscale * (0.1 * _random.NextDouble() + 0.45) + m_MinX;
            m_Y[0] = yscale * (0.1 * _random.NextDouble() + 0.45) + m_MinY;

            // except if one uses the below statements:
            //m_X[0] = xscale * _random.NextDouble() + m_MinX;
            //m_Y[0] = yscale *  _random.NextDouble() + m_MinY;

            for (int i = 0; i < Program.TrajLength - 1; i++)
            {
                do
                {
                    var angle = 2 * Math.PI * _random.NextDouble() - Math.PI;
                    m_X[i + 1] = m_X[i] + Math.Cos(angle);
                    m_Y[i + 1] = m_Y[i] + Math.Sin(angle);
                }
                while (NoFit(m_X[i + 1], m_Y[i + 1]));
            }
        }

        internal double SimpleDist(Trajectory t, double best)
        {
            double totD = Dist(t.m_X[0], t.m_Y[0], this.m_X[0], this.m_Y[0]);

            if (best == -1)
                for (int i = 1; i < Program.TrajLength; i++)
                    totD += Dist(t.m_X[i], t.m_Y[i], this.m_X[i], this.m_Y[i]);
            else
                for (int i = 1; (i < Program.TrajLength) && (totD < best); i++)
                    totD += Dist(t.m_X[i], t.m_Y[i], this.m_X[i], this.m_Y[i]);

            return totD;
        }

        /// <summary>
        /// Calculate Euclidean distance
        /// </summary>
        private double Dist(double x1, double y1, double x2, double y2)
            => Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

        internal void Mix(Trajectory old, double alpha)
        {
            for (int i = 0; i < Program.TrajLength; i++)
            {
                var oldX = old.m_X[i];
                var oldY = old.m_Y[i];
                var curX = m_X[i];
                var curY = m_Y[i];

                m_X[i] = alpha * curX + (1 - alpha) * oldX;
                m_Y[i] = alpha * curY + (1 - alpha) * oldY;

            }
        }

        internal void AddNoise2(double noise)
        {
            for (int i = 0; i < Program.TrajLength; i++)
            {
                m_X[i] += NormRand(0, noise);
                m_Y[i] += NormRand(0, noise);
            }
        }

        internal void Shift(int amount)
        {
            int pos = _random.Next(Program.TrajLength); // select random position on the trajectory

            // Find random shift of this point that keeps it within the boundaries
            m_X[pos] = m_X[pos] + NormRand(0, amount);
            m_Y[pos] = m_Y[pos] + NormRand(0, amount);

            ReFit(pos); // make sure points stay inside the boundaries

            // Shift points later on the trajectory
            bool shifted = true;
            for (int i = pos + 1; (i < Program.TrajLength) && shifted; i++)
            {
                var x = m_X[i - 1];
                var y = m_Y[i - 1];
                shifted = ShiftPoint(x, y, i);
            }

            // Shift points earlier on the trajectory
            shifted = true;
            for (int i = pos - 1; (i >= 0) && shifted; i--)
            {
                var x = m_X[i + 1];
                var y = m_Y[i + 1];
                shifted = ShiftPoint(x, y, i);
            }
        }

        private bool ShiftPoint(double goalX, double goalY, int position)
        {
            double difX = goalX - m_X[position];
            double difY = goalY - m_Y[position];

            double dist = difX * difX + difY * difY;
            if (dist > 1)
            {
                dist = 1 / Math.Sqrt(dist);
                m_X[position] = goalX - difX * dist;
                m_Y[position] = goalY - difY * dist;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Refit point so that it doesn't move out-of-bounds
        /// </summary>
        /// <param name="position">position of point in the trajectory</param>
        private void ReFit(int position)
        {
            if (m_X[position] < m_MinX)
                m_X[position] = m_MinX;
            else if (m_X[position] > m_MaxX)
                m_X[position] = m_MaxX;

            if (m_Y[position] < m_MinY)
                m_Y[position] = m_MinY;
            else if (m_Y[position] > m_MaxY)
                m_Y[position] = m_MaxY;

        }

        /// <summary>
        /// Get a random number from a normal distribution
        /// </summary>
        /// <param name="mean">The mean value</param>
        /// <param name="stddev">The standard deviation</param>
        /// <returns>a <see cref="double"/> pulled from a normal distribution</returns>
        private double NormRand(double mean = 0, double stddev = 1)
        {
            var normalDist = new Normal(mean, stddev);
            return normalDist.Sample();
        }

        private bool NoFit(double x, double y)
            => (x < m_MinX) || (x > m_MaxX) || (y < m_MinY) || (y > m_MaxY);

        /// <summary>
        /// Create a deep copy of the object
        /// </summary>
        /// <returns>A clone of the current <see cref="Trajectory"/></returns>
        public object Clone()
        {
            Trajectory t = (Trajectory)this.MemberwiseClone();
            t.m_X = (double[])m_X.Clone();
            t.m_Y = (double[])m_Y.Clone();
            return t;
        }
    }
}
