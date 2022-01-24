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
    /// <summary>
    /// The console application 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The number of points a <see cref="Trajectory"/> can have.
        /// The original value from the original paper is <c>20</c>.
        /// </summary>
        public static readonly int TRAJECTORY_LENGTH = 20;
        /// <summary>
        /// The number of <see cref="Trajectory">trajectories</see> an <see cref="Agent"/> has.
        /// The original value from the original paper is <c>4</c>.
        /// </summary>
        public static readonly int TRAJECTORY_NUMBER = 4;
        /// <summary>
        /// The number of <see cref="Agent"/>s that are present in the simulation.
        /// The original value from the original paper is <c>10</c>
        /// </summary>
        public static readonly int AGENT_NUM = 10;
        /// <summary>
        /// The number of Imitation Games two agents play.
        /// The original value form the original paper is <c>100</c>.
        /// </summary>
        /// <remarks>
        /// For more information about the Imitation Game, see https://journals.sagepub.com/doi/abs/10.1177/1059712309345789.
        /// </remarks>
        public static readonly int N_TEST = 100;
        /// <summary>
        /// The number of runs a similulation has.
        /// The original value form the original paper is <c>60000</c>.
        /// </summary>
        public static readonly int MAX_ITERATIONS = 60000;

        /// <summary>
        /// The maximum distance between two neighbouring <see cref="TrajectoryPoint"/>s on the same <see cref="Trajectory"/>.
        /// The original value form the original paper is <c>1</c>. 
        /// </summary>
        /// <remarks>
        /// Two neighbouring <see cref="TrajectoryPoint"/>s can have a distance between them with a lenght from 0 to <see cref="MAX_DIST"/>.
        /// </remarks>
        public static readonly double MAX_DIST = 1.0; // is r in the paper
        /// <summary>
        /// The space size dictates the size of the X-axis and Y-axis in which <see cref="TrajectoryPoint"/>s can reside.
        /// The bounderies for both X and Y are (-<see cref="SPACE_SIZE"/>/2, <see cref="SPACE_SIZE"/>/2), creating a square with all sides equal to <see cref="SPACE_SIZE"/>.
        /// The original value form the original paper is <c>10</c>. 
        /// </summary>
        public static readonly double SPACE_SIZE = 10.0;
        /// <summary>
        /// The standard deviation of Gaussian that is used to shift <see cref="TrajectoryPoint"/>s.
        /// The original value form the original paper is <c>1</c>.
        /// </summary>
        /// <seealso cref="Shift"/>
        /// <seealso cref="RandShift"/>
        /// <seealso cref="Agent.PrepareShift"/>
        /// <seealso cref="Trajectory.Shift"/>
        public static readonly double SIGMA_SHIFT = 1.0;
        /// <summary>
        /// The standard deviation for the Gaussian that is used to add noise to a <see cref="TrajectoryPoint"/>.
        /// The original value form the original paper is <c>2</c>.
        /// </summary>
        /// <seealso cref="Noise"/>
        /// <seealso cref="RandNoise"/>
        /// <seealso cref="Agent.Say"/>
        /// <seealso cref="Trajectory.AddNoise"/>
        public static readonly double SIGMA_NOISE = 2.0;
        /// <summary>
        /// The mix factor that is used to mix two <see cref="Trajectory"/> objects.
        /// The original value from the original paper is <c>0.5</c>.
        /// </summary>
        /// <seealso cref="Agent.AcceptOrReject(int)"/>
        /// <seealso cref="Trajectory.Mix(Trajectory)"/>
        public static readonly double BETA = 0.5;

        /// <summary>
        /// A <see cref="Random"/> that is used by all fucntions that incroporate randomness.
        /// </summary>
        /// <remarks>
        /// This is mainly done for debug reasons (otherwise it is imposseble to unit test)
        /// </remarks>
        public static readonly Random RANDOM = new Random(1);

        // normal distributions
        /// <summary>
        /// The Gaussian noise function, used to add noise to a <see cref="Trajectory"/>.
        /// The mean value is <c>1</c> and the standard deviation is <see cref="SIGMA_NOISE"/>
        /// </summary>
        /// <seealso cref="RandNoise"/>
        /// <seealso cref="Agent.Say"/>
        /// <seealso cref="Trajectory.AddNoise"/>
        private readonly static Normal Noise = new Normal(0, SIGMA_NOISE, RANDOM);
        /// <summary>
        /// The Gaussian noise function, used to add shift to a <see cref="Trajectory"/>.
        /// The mean value is <c>1</c> and the standard deviation is <see cref="SIGMA_SHIFT"/>
        /// </summary>
        /// <seealso cref="RandShift"/>
        /// <seealso cref="Agent.PrepareShift"/>
        /// <seealso cref="Trajectory.Shift"/>
        private readonly static Normal Shift = new Normal(0, SIGMA_SHIFT, RANDOM);

        /// <summary>
        /// Sample a value from the Gaussian distribution defined in <see cref="Noise"/>.
        /// </summary>
        /// <returns>A value from the defined Gaussian distribution</returns>
        internal static double RandNoise() => Noise.Sample();
        /// <summary>
        /// Sample a value from the Gaussian distribution defined in <see cref="Shift"/>.
        /// </summary>
        /// <returns>A value from the defined Gaussian distribution</returns>
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


        /// <summary>
        /// Run the simulation in the classic style of the original paper.
        /// </summary>
        /// <seealso cref="PlayGame(Agent, Agent)"/>
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


        /// <summary>
        /// Play an imitation game.
        /// </summary>
        /// <param name="initiator">
        /// An <see cref="Agent"/> that says the shifted <see cref="Trajectory"/> with added noise and validates the result of the <paramref name="imitator"/>
        /// </param>
        /// <param name="imitator">
        /// An <see cref="Agent"/> that tries to imitate the spoken <see cref="Trajectory"/> of the <paramref name="initiator"/>
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> indicating if the <paramref name="imitator"/> has successfully imitated the spoken <see cref="Trajectory"/> of the <paramref name="initiator"/>.
        /// </returns>
        /// <seealso cref="RunSimulationClassic"/>
        private static bool PlayGame(Agent initiator, Agent imitator)
        {
            var tSaid = initiator.Say();
            var tImit = imitator.Imitate(tSaid);
            return initiator.Listen(tImit);
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
    }
}
