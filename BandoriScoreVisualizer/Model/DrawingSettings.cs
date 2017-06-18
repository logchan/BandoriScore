using System;
using System.Drawing;

namespace BandoriScoreVisualizer.Model
{
    public class DrawingSettings
    {
        public float NoteMargin { get; set; } = 48;
        public float NoteRadius { get; set; } = 20;
        public float BarHeight { get; set; } = 600;
        public float ImageMargin { get; set; } = 120;
        public int NumberOfBars { get; set; }
        public float NoteOuterWidth { get; set; } = 2;

        public Brush BgBrush { get; set; } = new SolidBrush(Color.FromArgb(32, 32, 32));
        public Pen BarPen { get; set; } = new Pen(Brushes.Gray, 3);
        public Pen SubbarPen { get; set; } = new Pen(Brushes.Gray, 1);
        public Pen TrackPen { get; set; } = new Pen(Brushes.Gray, 1);
        public Font BarNumberFont { get; set; } = new Font("Consolas", 16.0f);
        public Font MetaFont { get; set; } = new Font("Microsoft YaHei", 16.0f);
        public Pen SyncPen { get; set; } = new Pen(Brushes.White, 4);

        public float NoteRectWidth => NoteRadius * 2 + 6;
        public float NoteRectHeight => NoteRadius * 0.2f;

        public float ImageWidth => ImageMargin * 2 + NoteMargin * 6;
        public float ImageHeight => ImageMargin * 2 + BarHeight * NumberOfBars;

        public float InnerRadius => NoteRadius * 0.8f;

        public int BarChunkSize { get; set; } = 8;
        public int NumberOfColumns => ((NumberOfBars - 1) / BarChunkSize) + 1;
        public float FinalImageWidth => (ImageMargin + NoteMargin * 6) * NumberOfColumns + ImageMargin;
        public float FinalImageHeight => ImageMargin * 2 + BarHeight * (Math.Min(NumberOfBars, BarChunkSize) + 1.0f / 4);
        public float ColumnWidth => ImageWidth - ImageMargin;


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
