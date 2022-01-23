using System;
namespace CA2
{
    public class Trajectory
    {
        public double Min { get => -Program.SPACE_SIZE / 2.0; }
        public double Max { get => +Program.SPACE_SIZE / 2.0; }

        public double[] X { get; set; }
        public double[] Y { get; set; }

        public void Randomize()
        {
            double scale = this.Max - this.Min;

            CheckNull();

            X[0] = scale * (0.1 * Program.RANDOM.NextDouble() + 0.45) + Min;
            Y[0] = scale * (0.1 * Program.RANDOM.NextDouble() + 0.45) + Min;

            for (int i = 0; i < Program.TRAJECTORY_LENGTH -1; i++)
            {
                do
                {
                    double angle = 2.0 * Math.PI * Program.RANDOM.NextDouble() - Math.PI;
                    X[i + 1] = X[i] + Math.Cos(angle);
                    Y[i + 1] = Y[i] + Math.Sin(angle);
                }
                while (NoFit(X[i + 1], Y[i + 1]));
            }
        }

        private bool NoFit(double x, double y)
            => (x < Min) || (x > Max) || (y < Min) || (y > Max);

        private void CheckNull()
        {
            if (X == null || X.Length == 0)
                X = new double[Program.TRAJECTORY_LENGTH];
            if (Y == null || Y.Length == 0)
                Y = new double[Program.TRAJECTORY_LENGTH];
        }

        internal void Shift(double sigmaShift)
        {
            int pos = Program.RANDOM.Next(Program.TRAJECTORY_LENGTH);

            X[pos] = X[pos] + Program.RandShift();
            Y[pos] = Y[pos] + Program.RandShift();

            ReFit(pos);

            bool shifted = true;
            for (int i = pos+1; (i < Program.TRAJECTORY_LENGTH) && shifted; i++)
            {
                shifted = ShiftPoint(X[i-1], Y[i-1], i);
            }


            shifted = true;
            for (int i = pos-1; (i >= 0) && shifted; i--)
            {
                shifted = ShiftPoint(X[i + 1], Y[i + 1], i);
            }
        }

        internal double SimpleDist(Trajectory t, double best)
        {
            double totD = Dist(t.X[0], t.Y[0], X[0], Y[0]);

            if (best == -1)
            {
                for (int i = 1; i < Program.TRAJECTORY_LENGTH; i++)
                    totD += Dist(t.X[i], t.Y[i], X[i], Y[i]);
            }
            else
            {
                for (int i = 1; (i < Program.TRAJECTORY_LENGTH) && (totD < best); i++)
                    totD += Dist(t.X[i], t.Y[i], X[i], Y[i]);
            }

            return totD;
        }

        private static double Dist(double x1, double y1, double x2, double y2)
            => Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        

        /// <summary>
        /// Is <c>AddNoise2</c> in the C++ version
        /// </summary>
        /// <param name="noiseLevel"></param>
        internal void AddNoise(double noiseLevel)
        {
            for (int i = 0; i < Program.TRAJECTORY_LENGTH; i++)
            {
                X[i] = X[i] + Program.RandNoise();
                Y[i] = Y[i] + Program.RandNoise();
            }
        }

        private bool ShiftPoint(double x, double y, int indexToSet)
        {
            double difx = x - X[indexToSet];
            double dify = y - Y[indexToSet];

            var dist = (difx * difx) + (dify * dify);
            if (dist > Program.MAX_DIST) // dist > 1^2
            {
                dist = 1 / Math.Sqrt(dist);
                X[indexToSet] = x - difx * dist;
                Y[indexToSet] = y - dify * dist;
                return true;
            }

            return false;
        }

        internal void Mix(Trajectory old, double beta)
        {
            for (int i = 0; i < Program.TRAJECTORY_LENGTH; i++)
            {
                X[i] = beta * X[i] + (1.0 - beta) * old.X[i];
                Y[i] = beta * X[i] + (1.0 - beta) * old.Y[i];
            }
        }

        private void ReFit(int pos)
        {
            if (X[pos] < Min)
                X[pos] = Min;
            else if (X[pos] > Max)
                X[pos] = Max;

            if (Y[pos] < Min)
                Y[pos] = Min;
            else if (Y[pos] > Max)
                Y[pos] = Max;
        }
    }
}
