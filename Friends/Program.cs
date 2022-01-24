using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DTO;
using MathNet.Numerics.Distributions;
using ScottPlot;

namespace Friends
{
    public class Program
    {

        public static readonly int TRAJECTORY_LENGTH = 20;
        public static readonly int TRAJECTORY_NUMBER = 4;
        public static readonly int AGENT_NUM = 10;
        public static readonly int MAX_ITERATIONS = 60000;
        public static readonly int N_TEST = 100;

        public static readonly double MAX_DIST = 1.0; // is r in the paper
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

        private static Stopwatch stopwatch;
        private static DirectoryInfo output_folder;

        static void Main(string[] args)
        {
            Console.WriteLine("Evolution of speech: simulation");
            Console.WriteLine("==============================================");
            PrepOutputFolder();

            stopwatch = new Stopwatch(); // monitor time

            RunSimulationClassic();

            TimeSpan t = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            string elapsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            Console.WriteLine($"run time: {elapsedTime}");
            Console.WriteLine("All finnished");
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

        private static void RunSimulationClassic()
        {
            IList<Agent> agents = CreateAgents();

            SaveToFile(agents, "init.json");

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
                    if (imitatorIndex != initiatorIndex)
                    {
                        var imitator = agents[imitatorIndex];
                        for (int indez = 0; indez < N_TEST; indez++)
                        {
                            if (PlayGame(initiator, imitator))
                                success++;
                        }
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
            Console.WriteLine("done running simulations");
            stopwatch.Stop();

            Console.WriteLine("print output files:");
            // print agents to json files.
            SaveToFile(agents, "out.json");
            SaveToFile(runner, "runner.csv");
            PlotToFile(runner, "success.png");
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
                                X = t.Points.Select(p => p.X).ToArray(),
                                Y = t.Points.Select(p => p.Y).ToArray(),
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
