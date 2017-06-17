using System.Drawing;

namespace BandoriScoreVisualizer.Model
{
    public class DrawingSettings
    {
        public float NoteMargin { get; set; } = 48;
        public float NoteRadius { get; set; } = 20;
        public float BarHeight { get; set; } = 600;
        public float ImageMargin { get; set; } = 100;
        public int NumberOfBars { get; set; }
        public float NoteOuterWidth { get; set; } = 2;
        public float NoteRectWidth => NoteRadius * 2 + 6;
        public float NoteRectHeight => NoteRadius * 0.2f;

        public float ImageWidth => ImageMargin * 2 + NoteMargin * 6;
        public float ImageHeight => ImageMargin * 2 + BarHeight * NumberOfBars;

        public float InnerRadius => NoteRadius * 0.8f;

        public PointF NotePosition(int track, Timing time)
        {
            var x = ImageMargin + NoteMargin * track;
            var y = ImageHeight - (ImageMargin + BarHeight * time.Bar + BarHeight * time.Beat / time.Denominator);
            return new PointF(x, y);
        }

        public PointF GridPosition(int track, Timing time)
        {
            var point = NotePosition(track, time);
            point.X -= NoteMargin / 2;
            return point;
        }
    }
}
