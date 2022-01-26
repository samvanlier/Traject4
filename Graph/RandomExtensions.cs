using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph
{
    public static class RandomExtentions
    {
        /// <summary>
        /// Pick a random element from <paramref name="choices"/> with a given distribution <paramref name="p"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rnd"></param>
        /// <param name="choices"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static T Choice<T>(this Random rnd, IEnumerable<T> choices, IEnumerable<double> p = null)
        {
            int length = choices.Count();

            if (p == null) // pick uniform random
                return choices.ElementAt(rnd.Next(length));

            if (p.Count() != length)
                throw new ArgumentException($"The length of choices and p have to be equal. choices.Count()={length}, p.Count()={p.Count()}");

            var cumP = new List<double>();
            double last = 0;
            var t = p.ToArray();
            foreach (var cur in p)
            {
                last += cur;
                cumP.Add(last);
            }

            double pick = rnd.NextDouble();

            for (int i = 0; i < length; i++)
            {
                if (pick < cumP[i])
                    return choices.ElementAt(i);
            }

            throw new Exception("no pick found");
        }

        /// <summary>
        /// Create a random <see cref="int"/> for 0 to <paramref name="n"/>, given a probability distribution <paramref name="p"/>
        /// </summary>
        /// <param name="rnd"></param>
        /// <param name="n"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static int Choice(this Random rnd, int n, IEnumerable<double> p = null)
            => rnd.Choice(Enumerable.Range(0, n), p);


        private static bool Equals3DigitPrecision(this double left, double right)
            => Math.Abs(left - right) < 0.001;
    }
}
