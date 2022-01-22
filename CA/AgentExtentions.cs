using System;
using System.Linq;

namespace CA
{
    public static class AgentExtentions
    {

        public static Trajectory FindClosest(this Agent agent, Trajectory spokenTrajectory)
        {
            var tDistances = agent.Trajectories
                .Select(t => Trajectory.Distance(t, spokenTrajectory)).ToList();

            var indexMax = tDistances.IndexOf(tDistances.Min());

            var t = agent.Trajectories.ElementAt(indexMax);

            return new Trajectory()
            {
                Points = t.Points.Select(p => p.Clone()).ToArray(),
                Success = t.Success
            };
        }

        public static Trajectory FindClosest2(this Agent agent, Trajectory spokeTrajectory)
        {
            double bestDist = spokeTrajectory.SimpleDist(agent.Trajectories.ElementAt(0), -1);

            int bestIndex = 0;
            for (int index = 1; index < Program.TRAJ_NUM; index++)
            {
                var dist = spokeTrajectory.SimpleDist(agent.Trajectories.ElementAt(index), bestDist);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = index;
                }
            }

            var bestTrajectory = agent.Trajectories.ElementAt(bestIndex);

            return bestTrajectory;
        }

        public static int FindClosestIndex(this Agent agent, Trajectory spokenTrajectory)
        {
            var tDistances = agent.Trajectories
                .Select(t => Trajectory.Distance(t, spokenTrajectory)).ToList();

            var indexMax = tDistances.IndexOf(tDistances.Min());

            var t = agent.Trajectories.ElementAt(indexMax);

            //return new Trajectory()
            //{
            //    Points = t.Points.Select(p => p.Clone()).ToArray(),
            //    Success = t.Success
            //};

            return indexMax;
        }

        public static int FindClosestIndex2(this Agent agent, Trajectory spokeTrajectory)
        {
            double bestDist = spokeTrajectory.SimpleDist(agent.Trajectories.ElementAt(0), -1);

            int bestIndex = 0;
            for (int index = 1; index < Program.TRAJ_NUM; index++)
            {
                var dist = spokeTrajectory.SimpleDist(agent.Trajectories.ElementAt(index), bestDist);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = index;
                }
            }

            var bestTrajectory = agent.Trajectories.ElementAt(bestIndex);

            //return bestTrajectory.Clone();

            return bestIndex;
        }
    }
}
