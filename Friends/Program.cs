using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MathNet.Numerics.Distributions;

namespace Friends
{
    public class Program
    {

        public static readonly int TRAJECTORY_LENGTH = 20;
        public static readonly int TRAJECTORY_NUMBER = 4;
        public static readonly int AGENT_NUM = 10;
        public static readonly int MAX_ITERATIONS = 60000;
        public static readonly int N_TEST = 100;

        public static readonly double MAX_DIST = 10.0; // is r in the paper
        public static readonly double SPACE_SIZE = 10.0;
        public static readonly double SIGMA_SHIFT = 1.0;
        public static readonly double SIGMA_NOISE = 2.0;
        public static readonly double BETA = 0.5;

        public static readonly Random RANDOM = new Random(1);


        // normal distributions
        private readonly static Normal Noise = new Normal(0, SIGMA_NOISE, RANDOM);
        private readonly static Normal Shift = new Normal(0, SIGMA_SHIFT, RANDOM);

        internal static double RandNoise() => Noise.Sample();
        internal static double RandShift() => Shift.Sample();

        static void Main(string[] args)
        {
            Console.WriteLine("Evolution of speech: simulation");
            Console.WriteLine("==============================================");

            Stopwatch stopwatch = new Stopwatch(); // monitor time

            RunSimulationClassic();

        }

        private static void RunSimulationClassic()
        {
            IList<Agent> agents = CreateAgents();

            Console.WriteLine("Start simulation:");

            var runner = new double[MAX_ITERATIONS];
            var runAvg = 0.0;

            for (int runId = 0; runId < MAX_ITERATIONS; runId++)
            {
                // select agent to speak
                var initiatorIndex = RANDOM.Next(AGENT_NUM);
                var initiator = agents[initiatorIndex];

                // shift a trajectory
                initiator.PrepareShift();

                var success = 0; // success counter

                for (int imitatorIndex = 0; imitatorIndex < AGENT_NUM; imitatorIndex++)
                {
                    if (imitatorIndex == initiatorIndex)
                        continue; // skip... don't need to talk to youself.

                    var imitator = agents[imitatorIndex];

                    for (int i = 0; i < N_TEST; i++)
                    {
                        if (PlayGame(initiator, imitator))
                            success++;
                    }
                }

                if (runId % 100 == 0)
                {
                    Console.WriteLine($"{runId}\t" +
                        $"runAvg = {runAvg}");
                }

                runAvg = runAvg * 0.999 + 0.001 * ((double)success / (N_TEST * (AGENT_NUM - 1.0)));
                runner[runId] = runAvg;
                initiator.AcceptOrReject(success);
            }

            Console.WriteLine($"{MAX_ITERATIONS}\t" +
                        $"runAvg = {runAvg}");
            Console.WriteLine("done");
        }

        private static IList<Agent> CreateAgents()
        {
            Console.WriteLine("Create agents");

            var agents = new Agent[AGENT_NUM];
            for (int i = 0; i < AGENT_NUM; i++)
            {
                agents[i] = new Agent();
                agents[i].Initialize();
            }

            Console.WriteLine("Agents created");

            return agents;
        }

        private static bool PlayGame(Agent initiator, Agent imitator)
        {
            var tSaid = initiator.Say();
            var tImit = imitator.Imitate(tSaid);
            return initiator.Listen(tImit);
        }
    }
}
