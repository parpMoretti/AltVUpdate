using Newtonsoft.Json;

namespace AltVUpdate
{
    public partial class Update
    {
        [JsonProperty("latestBuildNumber")]
        public long LatestBuildNumber { get; set; }

        [JsonProperty("hashList")]
        public HashList HashList { get; set; }
    }

    public partial class HashList
    {
        [JsonProperty("altv-server.exe")]
        public string AltvServerExe { get; set; }

        [JsonProperty("data/vehmodels.bin")]
        public string DataVehmodelsBin { get; set; }

        [JsonProperty("data/vehmods.bin")]
        public string DataVehmodsBin { get; set; }
    }
}