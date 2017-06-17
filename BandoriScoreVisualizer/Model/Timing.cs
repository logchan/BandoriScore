using System;
using Newtonsoft.Json;

namespace BandoriScoreVisualizer.Model
{
    public class Timing : IComparable<Timing>
    {
        public Timing()
        {
            
        }

        public Timing(int bar, int beat, int denominator)
        {
            Bar = bar;
            Beat = beat;
            Denominator = denominator;
        }

        [JsonProperty("bar_idx")]
        public int Bar { get; set; }
        [JsonProperty("beat_idx")]
        public int Beat { get; set; }
        [JsonProperty("denominator")]
        public int Denominator { get; set; }

        public int CompareTo(Timing other)
        {
            if (Bar != other.Bar)
            {
                return Bar.CompareTo(other.Bar);
            }

            if (Denominator == other.Denominator)
            {
                return Beat.CompareTo(other.Beat);
            }

            return (Beat * other.Denominator).CompareTo(other.Beat * Denominator);
        }

        public static bool operator ==(Timing a, Timing b)
        {
            return a.CompareTo(b) == 0;
        }

        public static bool operator !=(Timing a, Timing b)
        {
            return !(a == b);
        }
    }
}
