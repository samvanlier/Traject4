using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DTO;
using MathNet.Numerics.Distributions;
using ScottPlot;

namespace Traject4
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
        public readonly static double SIGMA_NOISE = 2.0; // value in paper = \Sigma_noise

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
        public readonly static Random RANDOM = new Random();

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

        private readonly static Normal Shift = new Normal(0, SIGMA_SHIFT, RANDOM);
        internal static double RandShift() => Shift.Sample();

        /// <summary>
        /// 
        /// </summary>
        private static int maxIts = 60000; // value paper
        //private readonly static int maxIts = 1000; // value paper
        private static DirectoryInfo output_folder;

        static void Main(string[] args)
        {
            PrepOutputFolder();

            // create agents
            var agents = new Agent[AGENT_NUM];
            for (int i = 0; i < AGENT_NUM; i++)
            {
                agents[i] = new Agent();
                agents[i].Initialise(TRAJ_NUM);
            }

            Console.WriteLine("agents initialized");
            SaveToFile(agents, "init.json");


            // main loop
            var runner = new double[maxIts];
            var runAvg = 0.0;
            for (int index = 0; index < maxIts; index++)
            {
                var shifter = RANDOM.Next(AGENT_NUM);
                var initiator = agents[shifter];

                initiator.PrepareShift(SIGMA_SHIFT);

                var success = 0;

                for (int indey = 0; indey < AGENT_NUM; indey++)
                {
                    if (indey != shifter)
                    {
                        var imitator = agents[indey];
                        for (int indez = 0; indez < N_TEST; indez++)
                        {
                            //Debug.WriteLine($"{index}-[{DateTime.Now}] PlayGame({shifter}, {initiator.ShiftIndex}, {indey})");
                            if (PlayGame(initiator, imitator))
                                success++;
                        }

                    }
                }

                if (index % 100 == 0)
                {
                    Console.WriteLine($"{index}\t" +
                        $"runAvg = {runAvg}");
                }

                runAvg = runAvg * 0.999 + 0.001 * ((double)success / (N_TEST * (AGENT_NUM - 1.0)));
                runner[index] = runAvg;
                initiator.AcceptOrReject(success);
            }
            Console.WriteLine($"{maxIts}\t" +
                        $"runAvg = {runAvg}");

            SaveToFile(agents, "out.json");
            SaveToFile(runner, "runner.csv");
            PlotToFile(runner, "success.png");

            Console.WriteLine("done");
        }

        private static bool PlayGame(Agent initiator, Agent imitator)
        {
            var tSaid = initiator.Say();
            Trajectory tImit = imitator.Imitate(tSaid);
            return initiator.Listen(tImit);
        }

        private static void PrepOutputFolder()
        {
            var curr = Directory.GetCurrentDirectory();
            var folder = Directory.GetParent(curr).Parent.Parent;

            var sub = folder.GetDirectories();
            if (!sub.Select(x => x.Name).Contains("output"))
                output_folder = folder.CreateSubdirectory("output");
            else
                output_folder = sub.Where(x => x.Name == "output").First();

            var now = DateTime.Now;
            var u = now.ToFileTimeUtc();
            var f = $"{now.Date.Day}_{now.Date.Month}_{now.ToFileTimeUtc()}";
            output_folder = output_folder.CreateSubdirectory(f);
        }

        private static void SaveToFile(double[] runner, string fileName)
        {
            var file = Path.Combine(output_folder.FullName, fileName);
            StringBuilder sb = new StringBuilder();
            sb.Append("index;runAvg");
            sb.Append("\n");

            var s = string.Join("\n", runner.Select((r, i) => $"{i};{r}"));
            sb.Append(s);

            var r = sb.ToString();
            File.WriteAllText(fileName, r);
        }

        private static void SaveToFile(IList<Agent> agents, string fileName)
        {
            var dtos = agents
                .Select((a, i) =>
                {
                    var trajectories = a.Trajectories
                        .Select((t, i) =>
                            new TrajectoryDTO()
                            {
                                Id = i,
                                X = t.X,
                                Y = t.Y,
                            })
                        .ToArray();

                    return new AgentDTO()
                    {
                        Id = i,
                        Trajectories = trajectories
                    };
                })
                .ToArray();

            var file = Path.Combine(output_folder.FullName, fileName);

            dtos.SaveAsJson(file);
        }

        private static void PlotToFile(double[] runner, string fileName)
        {
            var file = Path.Combine(output_folder.FullName, fileName);
            Console.WriteLine("plot chart:");

            var plt = new Plot();
            plt.AddSignal(runner);
            plt.Title("Average success over time");
            plt.XLabel("iteration");
            plt.YLabel("success");
            plt.SaveFig(file);

            Console.WriteLine("plotting done.");
        }
    }
}
