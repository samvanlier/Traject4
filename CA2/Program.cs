using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CA2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var r = new Random();
            int mean = 0;
            int stddev = 1;

            var normalDist = new Normal(mean, stddev);

            var aa = new List<double>();
            var bb = new List<double>();

            for (int i = 0; i < 1000; i++)
            {
                var nd = new Normal(mean, stddev);
                var a = normalDist.Sample();
                var b = nd.Sample();
                Console.WriteLine($"random val: {a} \t" +
                    $"random val new: {b}");

                aa.Add(a);
                bb.Add(b);
            }


            Console.WriteLine($"a average: {aa.Average()}");
            Console.WriteLine($"b average: {bb.Average()}");
        }
    }
}
