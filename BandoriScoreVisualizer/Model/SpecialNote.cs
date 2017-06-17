using System.Drawing;
using Newtonsoft.Json;

namespace BandoriScoreVisualizer.Model
{
    public class SpecialNote : Note
    {
        [JsonProperty("command")]
        public string Command { get; set; }

        public override Timing Start => Time;
        public override Timing End => Time;
        public override bool Visible => false;

        public override void Draw(DrawingSettings settings, Graphics g)
        {
            
        }
    }
}
