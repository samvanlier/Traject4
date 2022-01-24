using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace DTO
{
    public class AgentDTO
    {
        public int Id { get; set; }
        public ICollection<TrajectoryDTO> Trajectories { get; set; }
    }
}
