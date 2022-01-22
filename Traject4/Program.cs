using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ScottPlot;

namespace Traject4
{
    //regular main (Original Code): runtime of 14min
    class Program
    {
        public readonly static int SpaceSize = 10; // value of paper
        /// <summary>
        /// In the pseudo-code refert to as N_points
        /// </summary>
        public readonly static int TrajLength = 20; // value of paper
        //public readonly static int TrajLength = 1;
        public readonly static double NoiseLevel = 2; //todo find value in paper = \Sigma_noise in paper
        public readonly static int testNum = 100; // play the game testNum of times for the speaker

        private readonly static int ShiftSD = 1; // Standard deviation of shift


        #region Main program

        static void Main(string[] args)
        {

            // target folder
            var folder = @"/Users/sam/Google Drive/evo_of_speech/";


            Stopwatch stopwatch = Stopwatch.StartNew();

            var random = new Random(); // TODO remove seed

            int agentNum = 10; // value paper
            int trajNum = 4; // value paper

            // number of games
            //int maxIts = 60000; // value paper
            int maxIts = 1000000;

            // create agents
            var agents = new Agent[agentNum];
            for (int i = 0; i < agentNum; i++)
                agents[i] = new Agent(trajNum, i);

            var init = JsonSerializer.Serialize(agents.Select(a => new PrintAgent(a)).ToArray());
            File.WriteAllText($"{folder}init.json", init);
            var sb = new StringBuilder();
            sb.Append("run;avg\n");

            double runAvg = 0;
            double[] success_index = new double[maxIts];
            double[] runner = new double[maxIts];
            for (int index = 0; index < maxIts; index++)
            {
                int shifter = random.Next(agentNum); // initiator
                agents[shifter].PrepareShift(ShiftSD);

                int success = 0; // max value = testNum

                //for (int indey = 0; indey < agentNum; indey++)
                //{
                //    // Generate random other player and make
                //    // sure it is not equal to the original player
                //    if (indey != shifter)
                //    {
                //        for (int indez = 0; indez < testNum; indez++)
                //        {
                //            var s = agents[shifter];
                //            var y = agents[indey];

                //            if (s.Listen(y.Imitate(s.Say())))
                //                success++;
                //        }

                //    }
                //}

                for (int i = 0; i < testNum; i++)
                {
                    int indey = random.Next(agentNum);

                    while (indey == shifter) // make sure agent is not talking to himself
                        indey = random.Next(agentNum);

                    var s = agents[shifter];
                    var y = agents[indey];

                    // play game!
                    if (s.Listen(y.Imitate(s.Say())))
                        success++;
                }

                // check why runAvg decreases
                runAvg = runAvg * 0.999 + 0.001 * (success * 1.0 / testNum);
                runner[index] = runAvg;
                agents[shifter].AcceptReject(success);

                //TODO zoek uit hoe Figrure 2 (c) word gemaakt
                success_index[index] = success;

                if (index % 100 == 0)
                {
                    Console.WriteLine($"index {index}: runAvg={runAvg}");
                    sb.Append($"{index};{runAvg}\n");
                }

            }

            stopwatch.Stop();

            var r = sb.ToString();
            File.WriteAllText("./out.csv", r);

            var outJson = JsonSerializer.Serialize(agents.Select(a => new PrintAgent(a)).ToArray());
            File.WriteAllText($"{folder}out.json", outJson);
            Console.WriteLine("done running");

            TimeSpan t = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            string elapsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            Console.WriteLine($"run time: {elapsedTime}ms");
            Console.WriteLine("All finnished");

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
        #endregion


        #region test get graph
        //static void Main()
        //{
        //    Random _random = new Random(1);
        //    int n = 100; // nodes
        //    int k = 4500; // edges

        //    var g = new RandomGraph(n, k, _random);
        //    g.PrintMatrix();

        //    if (g.NumberOfEdges != k)
        //        Console.WriteLine("help");
        //    Console.WriteLine("done");
        //}
        #endregion
    }

    public class PrintAgent
    {
        public int Id { get; set; }
        public PrintTrajectory[] T { get; set; }

        public PrintAgent(Agent a)
        {
            this.Id = a.Id;
            this.T = a.m_T.Select(t => new PrintTrajectory(t)).ToArray();
        }
    }

    public class PrintTrajectory
    {
        public int Id { get; set; }
        public double[] X { get; set; }
        public double[] Y { get; set; }

        public PrintTrajectory(Trajectory t)
        {
            this.Id = t.Id;
            this.X = t.m_X.ToArray();
            this.Y = t.m_Y.ToArray();
        }
    }

    // class Program
    // {
    //     public static INotificationSystem _notificationSystem { get; set; }
    //     public static IBridgeSystem _bridgeSystem { get; set; }
    //     public static ShellConfigurator _shell { get; set; }

    //     static void Main(string[] args)
    //     {
    //         // create a graph
    //         var edges = new[] { new Edge<int>(1, 2), new Edge<int>(0, 1), new Edge<int>(1, 0) };
    //         var graph = edges.ToBidirectionalGraph<int, Edge<int>>();

    //         string dotGraph = graph.ToGraphviz();

    //         Console.WriteLine(dotGraph);

    //         // convert a graph to SVG or pdf
    //         string command = $"echo '{dotGraph}' | dot -Tpdf > ~/Desktop/output.pdf";

    //         _notificationSystem = NotificationSystem.Default;
    //         string os = OS.GetCurrent();
    //         switch (os)
    //         {
    //             case "win":
    //                 _bridgeSystem = BridgeSystem.Bat;
    //                 break;
    //             case "mac":
    //             case "gnu":
    //                 _bridgeSystem = BridgeSystem.Bash;
    //                 break;
    //         }
    //         _shell = new ShellConfigurator(_bridgeSystem, _notificationSystem);

    //         Response result = _shell.Term(command);
    //         //_shell.Result(result.stdout, "Not Installed");


    //         Console.WriteLine($"{result.code}");
    //         if (result.code == 0)
    //             Console.WriteLine($"command worked");
    //         else
    //             Console.WriteLine($"command failed");

    //         Console.WriteLine("done");

    //     }
    // }

}
