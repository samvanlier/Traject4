using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using ScottPlot;

namespace CA
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
        //public readonly static int TRAJECTORY_LENGTH = 1;

        /// <summary>
        /// 
        /// </summary>
        public readonly static double SIGMA_NOISE = 2; // value in paper = \Sigma_noise
        //public readonly static double SIGMA_NOISE = 1;

        /// <summary>
        /// 
        /// </summary>
        public readonly static int N_TEST = 100; // play the game testNum of times for the speaker

        /// <summary>
        /// 
        /// </summary>
        public readonly static int SIGMA_SHIFT = 1; // Standard deviation of shift

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

        private static int agentNum = 10; // value paper
        private static int maxIts = 60000; // value paper
        //private static int maxIts = 250000; // value C++ code

        public static Normal ShiftNormal { get; private set; }
        public static Normal NoiseNormal { get; private set; }

        static void Main(string[] args)
        {
            SetNormals();
            Console.WriteLine("Start simulation");
            Stopwatch stopwatch = Stopwatch.StartNew();

            var folder = @"/Users/sam/Google Drive/evo_of_speech/";

            Console.WriteLine("Create agents");
            var agents = Enumerable.Range(0, agentNum)
                .AsParallel()
                .AsOrdered()
                .Select(i =>
                {
                    var a = new Agent()
                    {
                        Id = i
                    };

                    a.Trajectories = Enumerable.Range(0, TRAJ_NUM)
                        .Select(_ =>
                        {
                            var t = new Trajectory()
                            {
                                Success = 0.5,
                            };

                            t.Randomize();

                            return t;
                        })
                        .ToArray();

                    return a;
                })
                .ToList();

            var init = JsonSerializer.Serialize(agents.Select(a => new PrintAgent(a)).ToArray());
            File.WriteAllText($"./init.json", init);
            var sb = new StringBuilder();
            sb.Append("run;avg\n");

            double runAvg = 0;
            double[] success_index = new double[maxIts];
            double[] runner = new double[maxIts];
            int replaced = 0;

            Console.WriteLine($"start running for {maxIts} iterations");

            for (int i = 0; i < maxIts; i++)
            {
                var initatorIndex = RANDOM.Next(agentNum);
                var initiator = agents.ElementAt(initatorIndex);

                var tOriginalIndex = RANDOM.Next(TRAJ_NUM);
                var tOriginal = initiator.Trajectories.ElementAt(tOriginalIndex).Clone();

                //var tShifted = tOriginal.Shift2(SIGMA_SHIFT);
                var tShifted = tOriginal.Shift3(SIGMA_SHIFT); //TODO check shift
                tShifted.Success = 0.0;
                initiator.Replace(tOriginalIndex, tShifted);

                int success = 0;

                #region C++ codebase

                Parallel.For(0, agentNum, indey =>
                {
                    if (indey != initatorIndex)
                    {
                        var imitator = agents.ElementAt(indey);

                        for (int j = 0; j < N_TEST; j++)
                        {
                            Debug.WriteLine($"[{DateTime.Now} iter {i}]" +
                                $"\t" +
                                $"PlayGame({initiator.Id}, {tOriginalIndex}, {imitator.Id})");
                            if (PlayGame(initiator, tShifted, imitator))
                                Interlocked.Increment(ref success);
                        }
                    }
                });

                #endregion

                #region psuedo code

                //Parallel.For(0, N_TEST, j =>
                //{
                //    int imitatorIndex = RANDOM.Next(agentNum);

                //    while (imitatorIndex == initatorIndex)
                //        imitatorIndex = RANDOM.Next(agentNum);

                //    var imitator = agents.ElementAt(imitatorIndex);

                //    if (PlayGame(initiator, tShifted, imitator))
                //        Interlocked.Increment(ref success);
                //});

                #endregion


                if ((double)success / (N_TEST * (agentNum - 1)) < tOriginal.Success)
                    initiator.Replace(tOriginalIndex, tOriginal);
                else
                {
                    var tNew = tOriginal.Mix(tShifted, BETA);
                    double og = BETA * tOriginal.Success;
                    double sr = (double)success / (N_TEST * (agentNum - 1));
                    double b = sr * (1.0 - BETA);

                    tNew.Success = og + b;

                    initiator.Replace(tOriginalIndex, tNew);
                    replaced++;
                }


                //success_index[i] = (double)success / N_TEST;
                // runAvg = runAvg * 0.999 + 0.001 * (success * 1.0 / N_TEST);

                runAvg = runAvg * 0.999 + 0.001 * ((double)success / (N_TEST * (agentNum - 1.0)));


                //runAvg = success_index.Sum() / (i + 1);
                runner[i] = runAvg;

                if (i % 100 == 0)
                {
                    Console.WriteLine($"index {i}: runAvg={runAvg}");
                    sb.Append($"{i};{runAvg}\n");
                }
            }

            stopwatch.Stop();

            TimeSpan t = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            string elapsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            Console.WriteLine($"run time: {elapsedTime}");
            Console.WriteLine("All finnished");

            Console.WriteLine($"The number of replacemants was {replaced}");

            var r = sb.ToString();
            File.WriteAllText("./out.csv", r);

            var outJson = JsonSerializer.Serialize(agents.Select(a => new PrintAgent(a)).ToArray());
            File.WriteAllText($"./out.json", outJson);

            Console.WriteLine("plot chart:");

            var plt = new Plot();
            plt.AddSignal(success_index);
            plt.Title("Success");
            plt.SaveFig("./success.png");

            plt = new Plot();
            plt.AddSignal(runner);
            plt.Title("Runner");
            plt.SaveFig("./runner.png");

            Console.WriteLine("plotting done.");
        }

        public static bool PlayGame(Agent initiator, Trajectory tInit, Agent imitator)
        {
            // todo optimize with index instead of Trajectories

            var tSaid = tInit.AddNoise2(SIGMA_NOISE);
            var tImit = imitator.FindClosest(tSaid);
            //var tResp = tImit.AddNoise2(SIGMA_NOISE); // no noise added in C++ version
            var tResp = tImit;
            var tSucc = initiator.FindClosest(tResp);

            return tSucc.Equals(tInit);
        }

        private static void SetNormals()
        {
            ShiftNormal = new Normal(0, SIGMA_SHIFT);
            NoiseNormal = new Normal(0, SIGMA_NOISE);
        }
    }

    public class PrintAgent
    {
        public int Id { get; set; }
        public PrintTrajectory[] T { get; set; }

        public PrintAgent(Agent a)
        {
            this.Id = a.Id;
            this.T = a.Trajectories.Select(t => new PrintTrajectory(t)).ToArray();
        }
    }

    public class PrintTrajectory
    {
        public double[] X { get; set; }
        public double[] Y { get; set; }

        public PrintTrajectory(Trajectory t)
        {
            this.X = new double[Program.TRAJECTORY_LENGTH];
            this.Y = new double[Program.TRAJECTORY_LENGTH];

            for (int i = 0; i < Program.TRAJECTORY_LENGTH; i++)
            {
                var p = t.Points.ElementAt(i);
                X[i] = p.X;
                Y[i] = p.Y;
            }
        }
    }
}
