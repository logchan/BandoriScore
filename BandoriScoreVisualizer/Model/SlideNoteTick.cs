using Newtonsoft.Json;

namespace BandoriScoreVisualizer.Model
{
    public class SlideNoteTick
    {
        [JsonProperty("time")]
        public Timing Time { get; set; }
        [JsonProperty("track_idx")]
        public int Track { get; set; }
    }
}
