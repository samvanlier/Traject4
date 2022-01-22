using System;
using System.Linq;

namespace Traject4
{
    public class Agent
    {
        private readonly Random _random;
        private readonly double _mixFactor = 0.5; // beta 
        public int Id { get; set; }

        public int m_TrajNum { get; set; }
        public Trajectory[] m_T { get; set; }
        public double[] m_Success { get; set; }
        public Trajectory m_Backup { get; set; }
        public int m_ShiftIndex { get; set; }

        public Agent(int trajNum, int id)
        {
            this.Id = id;
            m_TrajNum = trajNum;
            //m_T = Enumerable.Repeat(0, trajNum).Select(i => new Trajectory(i)).ToArray();
            //m_Success = Enumerable.Repeat(Program.testNum / 2.0, trajNum).ToArray();

            m_T = new Trajectory[trajNum];
            m_Success = new double[trajNum];
            for (int i = 0; i < trajNum; i++)
            {
                m_T[i] = new Trajectory(i);
                m_Success[i] = Program.testNum / 2.0;
            }

            _random = new Random();
        }

        public void PrepareShift(int shiftSD)
        {
            m_ShiftIndex = _random.Next(m_TrajNum);
            var trajectory = m_T[m_ShiftIndex];
            //T original
            m_Backup = (Trajectory)trajectory.Clone(); // make a full copy of the object

            trajectory.Shift(shiftSD); // create T shifted
        }

        public Trajectory Say()
        {
            var toSay = m_T[m_ShiftIndex];
            var m_NoiseT = (Trajectory)toSay.Clone();

            // Addnoise must be commented out if noise is added in distance calculation.
            //	m_NoiseT.AddNoise( NoiseLevel);
            m_NoiseT.AddNoise2(Program.NoiseLevel);

            return m_NoiseT;
        }

        public Trajectory Imitate(Trajectory t)
        {
            double bestDist = -1;

            Trajectory best = m_T[0];

            foreach (var item in m_T)
            {
                var dist = t.SimpleDist(item, bestDist);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = item;
                }
            }

            // add noise
            var best_noise = (Trajectory)best.Clone();
            best_noise.AddNoise2(Program.NoiseLevel);

            return best_noise;
        }

        public bool Listen(Trajectory t)
        {
            double bestDist = t.SimpleDist(m_T[0], -1.0);
            int best = 0;
            for (int i = 1; i < m_TrajNum; i++)
            {
                var item = m_T[i];
                var dist = t.SimpleDist(item, bestDist);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }

            return best == m_ShiftIndex;
        }

        internal void AcceptReject(int success)
        {
            var curr = m_Success[m_ShiftIndex];
            if (success < curr) // no improvement, restor original
            {
                m_T[m_ShiftIndex] = (Trajectory)m_Backup.Clone();
                m_Backup = null; // clear backup

                // in pseudo-code, success it not updated when it was not successfull
                return;
            }


            m_T[m_ShiftIndex].Mix(m_Backup, _mixFactor);

            m_Backup = null; // clear backup

            m_Success[m_ShiftIndex] = _mixFactor * success + (1.0 - _mixFactor) * curr;
        }
    }
}
