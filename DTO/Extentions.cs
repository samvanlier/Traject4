using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DTO
{
    public static class Extentions
    {
        public static void SaveAsJson(this ICollection<AgentDTO> dtos, string fileName)
        {
            var outJson = JsonSerializer.Serialize(dtos);
            File.WriteAllText(fileName, outJson);
        }
    }
}
