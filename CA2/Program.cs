using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CA2
{
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly static int SPACE_SIZE = 10;

        /// <summary>
        /// In the pseudo-code refert to as N_points
        /// </summary>
        public readonly static int TRAJECTORY_LENGTH = 20; // value of paper

        /// <summary>
        /// 
        /// </summary>
        public readonly static double SIGMA_NOISE = 2; // value in paper = \Sigma_noise

        /// <summary>
        /// 
        /// </summary>
        public readonly static int N_TEST = 100; // play the game testNum of times for the speaker

        /// <summary>
        /// 
        /// </summary>
        public readonly static double SIGMA_SHIFT = 1.0; // Standard deviation of shift

        /// <summary>
        /// 
        /// </summary>
        public static double MAX_DIST = 1;

        /// <summary>
        /// 
        /// </summary>
        public static readonly double BETA = 0.5; // mix factor

        /// <summary>
        /// 
        /// </summary>
        public readonly static int TRAJ_NUM = 4; // value paper

        /// <summary>
        /// 
        /// </summary>
        public readonly static Random RANDOM = new Random(1);

        /// <summary>
        /// 
        /// </summary>
        public readonly static int AGENT_NUM = 10; // value paper

        //internal static double NormRand(double mean = 0.0, double stddev=1.0)
        //{
        //    //todo optimize if works
        //    var normal = new Normal(mean, stddev);
        //    return normal.Sample();
        //}

        private readonly static Normal Noise = new Normal(0, SIGMA_NOISE, RANDOM);
        internal static double RandNoise() => Noise.Sample();

        private readonly static Normal Shift = new Normal(0, SIGMA_SHIFT);
        internal static double RandShift() => Shift.Sample();

        /// <summary>
        /// 
        /// </summary>
        //private static int maxIts = 60000; // value paper
        private readonly static int maxIts = 1000; // value paper

        static void Main(string[] args)
        {
            // TODO port c++ code (again with arrays this time)
            // if this runs well, rebuild to what CA looks like (with points instead of arrays)

            // create agents
            var agents = new Agent[AGENT_NUM];
            for (int i = 0; i < AGENT_NUM; i++)
            {
                agents[i] = new Agent();
                agents[i].Initialise(TRAJ_NUM);
            }

            Console.WriteLine("agents initialized");

            // main loop
            var runAvg = 0.0;
            for (int index = 0; index < maxIts; index++)
            {
                var shifter = RANDOM.Next(AGENT_NUM);
                var initiator = agents[shifter];

                initiator.PrepareShift(SIGMA_SHIFT);

                var success = 0;

                //for (int indey = 0; indey < AGENT_NUM; indey++)
                //{
                //    if (indey != shifter)
                //    {
                //        var imitator = agents[indey];
                //        for (int indez = 0; indey < N_TEST; indez++)
                //        {
                //            Debug.WriteLine($"[{DateTime.Now}] PlayGame({shifter}, {initiator.ShiftIndex}, {indey})");
                //            if (PlayGame(initiator, imitator))
                //                success++;
                //        }

                //    }
                //}

                Parallel.For(0, AGENT_NUM, indey =>
                {
                    if (indey != shifter)
                    {
                        var imitator = agents[indey];
                        for (int indez = 0; indez < N_TEST; indez++)
                        {
                            Debug.WriteLine($"{index}-[{DateTime.Now}] PlayGame({shifter}, {initiator.ShiftIndex}, {indey})");
                            if (PlayGame(initiator, imitator))
                                Interlocked.Increment(ref success);
                        }
                    }
                });

                if (index%100 == 0)
                {
                    Console.WriteLine($"{index}\t" +
                        $"runAvg = {runAvg}");
                }

                runAvg = runAvg * 0.999 + 0.001*((double)success / (N_TEST * (AGENT_NUM - 1.0)));
                initiator.AcceptOrReject(success);
            }

            Console.WriteLine("done");
        }

        private static bool PlayGame(Agent initiator, Agent imitator)
        {
            var tSaid = initiator.Say();
            Trajectory tImit = imitator.Imitate(tSaid);
            return initiator.Listen(tImit);
        }
    }
}
