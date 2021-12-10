using System;
using System.Linq;

namespace Traject4
{
    public class Agent
    {
        private readonly Random _random;
        private readonly double _mixFactor = 0.5;
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
            m_T = Enumerable.Repeat(0, trajNum).Select(i => new Trajectory(i)).ToArray();
            m_Success = Enumerable.Repeat(50.0, trajNum).ToArray();

            _random = new Random();
        }

        public void PrepareShift(int shiftSD)
        {
            m_ShiftIndex = _random.Next(m_TrajNum);
            var trajectory = m_T[m_ShiftIndex];
            m_Backup = (Trajectory)trajectory.Clone(); // make a full copy of the object

            trajectory.Shift(shiftSD);
        }

        public Trajectory Say()
        {
            var m_NoiseT = (Trajectory)m_T[m_ShiftIndex].Clone();

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

            return best;
        }

        public bool Listen(Trajectory t)
        {
            double bestDist = -1;
            int best = 0;
            for (int i = 0; i < m_TrajNum; i++)
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
            if (success < m_Success[m_ShiftIndex]) // no improvement, restor original
                m_T[m_ShiftIndex] = (Trajectory)m_Backup.Clone();
            else
                m_T[m_ShiftIndex].Mix(m_Backup, _mixFactor);

            m_Backup = null; // clear backup

            m_Success[m_ShiftIndex] = _mixFactor * success + (1 - _mixFactor) * m_Success[m_ShiftIndex];
        }
    }
}
