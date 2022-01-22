using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CA
{
    public class Agent
    {
        public int Id { get; set; }
        public ICollection<Trajectory> Trajectories { get; set; }

        public void Replace(int originalIndex, Trajectory trajectory)
        {
            //todo optimize?
            var array = Trajectories.ToArray();
            array[originalIndex] = trajectory.Clone();

            this.Trajectories = array;
        }
    }
}
