using System;
using MathNet.Numerics.Distributions;

namespace Traject4
{
    class Program
    {
        public readonly static int SpaceSize = 10;
        public readonly static int TrajLength = 20;
        public readonly static double NoiseLevel = 0.1; //todo find value in paper

        private readonly static int ShiftSD = 1; // Standard deviation of shift

        static void Main(string[] args)
        {
            var random = new Random();

            int agentNum = 10;
            int trajNum = 2; //todo find correct number in paper
            int maxIts = 250000;
            int testNum = 100;

            // create agents
            var agents = new Agent[agentNum];
            for (int i = 0; i < agentNum; i++)
                agents[i] = new Agent(trajNum);

            double runAvg = 0;
            for (int index = 0; index < maxIts; index++)
            {
                int shifter = random.Next(agentNum);
                agents[shifter].PrepareShift(ShiftSD);

                int success = 0;
                for (int indey = 0; indey < agentNum; indey++)
                {
                    // Generate random other player and make
                    // sure it is not equal to the original player
                    if (indey != shifter)
                    {
                        for (int indez = 0; indez < testNum; indez++)
                        {
                            var s = agents[shifter];
                            var y = agents[indey];

                            if (s.Listen(y.Imitate(s.Say())))
                                success++;
                        }

                    }
                }

                runAvg = runAvg * 0.999 + 0.001 * ((double)success / (testNum * (agentNum - 1)));
                agents[shifter].AcceptReject(success);

                if (index % 100 == 0)
                    Console.WriteLine($"index {index}: runAvg={runAvg}");
            }

            Console.WriteLine("done");

        }
    }
}
