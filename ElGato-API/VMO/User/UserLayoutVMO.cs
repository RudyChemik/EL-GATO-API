using ElGato_API.Models.User;
using System.Text.Json.Serialization;

namespace ElGato_API.VMO.User
{
    public class UserLayoutVMO
    {
        public bool Animations { get; set; } = true;
        public List<ChartStackVMO> ChartStack { get; set; }
    }

    public class ChartStackVMO
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChartType ChartType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChartDataType ChartDataType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Period Period { get; set; }

        public string Name { get; set; }
    }
}
