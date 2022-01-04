using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Traject4
{
    class Program
    {
        public readonly static int SpaceSize = 10; // value of paper
        public readonly static int TrajLength = 20; // value of paper
        //public readonly static int TrajLength = 1;
        public readonly static double NoiseLevel = 0.1; //todo find value in paper

        private readonly static int ShiftSD = 1; // Standard deviation of shift

        static void Main(string[] args)
        {
            var random = new Random(1);

            int agentNum = 10; // value paper
            int trajNum = 4; // value paper

            // number of games
            //int maxIts = 60000; // value paper
            int maxIts = 100;

            // speak testNum times with another agent in one game.
            int testNum = 100;

            // create agents
            var agents = new Agent[agentNum];
            for (int i = 0; i < agentNum; i++)
                agents[i] = new Agent(trajNum, i);

            var init = JsonSerializer.Serialize(agents.Select(a => new PrintAgent(a)).ToArray());
            File.WriteAllText("./init.json", init);
            var sb = new StringBuilder();
            sb.Append("run;avg\n");

            double runAvg = 0;
            double[] success_index = new double[maxIts];
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

                            if (s.Listen(y.Imitate(s.Say()))) // there is some bug here...
                                success++;
                        }

                    }
                }

                runAvg = runAvg * 0.999 + 0.001 * (success * 1.0 / (testNum * (agentNum - 1)));
                agents[shifter].AcceptReject(success);

                //TODO zoek uit hoe Figrure 2 (c) word gemaakt

                double succes_rate = success * 1.0 / (testNum * (agentNum - 1));
                if (succes_rate > 1)
                    Console.WriteLine("help");

                success_index[index] = success * 1.0 / (agentNum * testNum * 1.0);

                if (index % 100 == 0)
                {
                    Console.WriteLine($"index {index}: runAvg={runAvg}");
                    sb.Append($"{index};{runAvg}\n");
                }

            }

            var r = sb.ToString();
            var outJson = JsonSerializer.Serialize(agents.Select(a => new PrintAgent(a)).ToArray());
            File.WriteAllText("./out.json", outJson);
            Console.WriteLine("done");

        }


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
}
