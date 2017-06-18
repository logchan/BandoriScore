using System;

namespace BandoriScoreVisualizer.Model
{
    public class MetaData
    {
        public string Title { get; set; } = String.Empty;
        public string Difficulty { get; set; } = String.Empty;
        public int Level { get; set; }
        public int Combo { get; set; }
        public int Bpm { get; set; }
    }
}
