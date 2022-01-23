using System;
namespace Friends
{
    public readonly struct TrajectoryPoint : IEquatable<TrajectoryPoint>
    {
        public readonly double X;
        public readonly double Y;

        public TrajectoryPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        internal TrajectoryPoint(TrajectoryPoint point) : this(point.X, point.Y)
        { }

        public static TrajectoryPoint operator +(TrajectoryPoint a) => new TrajectoryPoint(a);
        public static TrajectoryPoint operator -(TrajectoryPoint a) => new TrajectoryPoint(-a.X, -a.Y);

        public static TrajectoryPoint operator +(TrajectoryPoint a, TrajectoryPoint b)
            => new TrajectoryPoint(a.X + b.X, a.Y + b.Y);

        public static TrajectoryPoint operator -(TrajectoryPoint a, TrajectoryPoint b)
            => a + (-b);

        public static TrajectoryPoint operator +(TrajectoryPoint a, double val)
            => a + new TrajectoryPoint(val, val);
        public static TrajectoryPoint operator +(double val, TrajectoryPoint a) => a + val;

        public static TrajectoryPoint operator *(TrajectoryPoint a, double val)
            => new TrajectoryPoint(a.X * val, a.Y * val);
        public static TrajectoryPoint operator *(double val, TrajectoryPoint a) => a * val;

        public static TrajectoryPoint operator /(TrajectoryPoint a, double val) => a * (1 / val);
        public static TrajectoryPoint operator /(double val, TrajectoryPoint a) => a / val;

        public TrajectoryPoint Clone() => new TrajectoryPoint(X, Y);

        public bool Equals(TrajectoryPoint other)
        {
            if (this.X == other.X)
                if (this.Y == other.Y)
                    return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return this.Equals((TrajectoryPoint)obj);
        }

        public static bool operator ==(TrajectoryPoint a, TrajectoryPoint b) => a.Equals(b);

        public static bool operator !=(TrajectoryPoint a, TrajectoryPoint b) => !(a == b);


        public static double EuclideanDistance(TrajectoryPoint a, TrajectoryPoint b)
            => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
    }
}
