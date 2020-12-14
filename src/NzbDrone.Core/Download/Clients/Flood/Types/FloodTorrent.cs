using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Flood.Types
{
    public sealed class FloodTorrent
    {
        [JsonProperty(PropertyName = "bytesDone")]
        public long BytesDone { get; set; }

        [JsonProperty(PropertyName = "directory")]
        public string Directory { get; set; }

        [JsonProperty(PropertyName = "eta")]
        public long Eta { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "ratio")]
        public float Ratio { get; set; }

        [JsonProperty(PropertyName = "sizeBytes")]
        public long SizeBytes { get; set; }

        [JsonProperty(PropertyName = "status")]
        public List<string> Status { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; }
    }
}
