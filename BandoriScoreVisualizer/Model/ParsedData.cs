using System.Collections.Generic;
using Newtonsoft.Json;

namespace BandoriScoreVisualizer.Model
{
    public class ParsedData
    {
        [JsonProperty("metadata")]
        public MetaData MetaData { get; set; }

        [JsonProperty("notes")]
        public List<Note> Notes { get; } = new List<Note>();
    }
}
