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
                agents[i] = new Agent(trajNum, i);

            File.WriteAllText("./init.json", JsonConvert.SerializeObject(agents));
            var sb = new StringBuilder();
            sb.Append("run;avg\n")

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
                {
                    Console.WriteLine($"index {index}: runAvg={runAvg}");
                    sb.Append($"{index};{runAvg}\n")
                }
                    
            }

            Console.WriteLine("done");

            File.WriteAllText("./out.json", JsonConvert.SerializeObject(agents));
        }
    }

    class PrintAgent{
        public int Id { get; set; }
        public PrintTrajectory[] T { get; set; }

        PrintAgent(Agent a){
            this.Id = a.Id;
            this.T = a.m_T.Select(t => new PrintAgent(t)).ToArray();
        }
    }

    class PrintTrajectory {
        public int Id { get; set; }
        public double[] X { get; set; }
        public double[] Y { get; set; }

        PrintTrajectory(Trajectory t){
            this.Id = t.Id;
            this.X = t.m_X.ToArray();
            this.Y = t.m_Y.ToArray();
        }
    }
}
