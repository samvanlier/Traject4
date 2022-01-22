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
    }
}
