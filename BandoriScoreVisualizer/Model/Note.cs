using System.Drawing;
using Newtonsoft.Json;

namespace BandoriScoreVisualizer.Model
{
    public abstract class Note
    {
        [JsonProperty("is_flick")]
        public bool IsFlick { get; set; }
        [JsonProperty("is_skill")]
        public bool IsSkill { get; set; }
        [JsonProperty("track_idx")]
        public int Track { get; set; }
        [JsonProperty("time")]
        public Timing Time { get; set; }

        [JsonIgnore]
        public abstract bool Visible { get; }
        [JsonIgnore]
        public abstract Timing Start { get; }
        [JsonIgnore]
        public abstract Timing End { get; }

        public abstract void Draw(DrawingSettings settings, Graphics g);

        protected static void DrawNote(PointF center, Color color, DrawingSettings settings, Graphics g, bool rectOnly = false)
        {
            var brush = new SolidBrush(color);
            var pen = new Pen(brush, settings.NoteOuterWidth);

            if (!rectOnly)
            {
                var inner = settings.InnerRadius;
                var outer = settings.NoteRadius;
                g.FillEllipse(brush, center.X - inner, center.Y - inner, inner * 2, inner * 2);
                g.DrawEllipse(pen, center.X - outer, center.Y - outer, outer * 2, outer * 2);
            }

            var width = settings.NoteRectWidth;
            var height = settings.NoteRectHeight;
            g.DrawRectangle(pen, center.X - width / 2, center.Y - height / 2, width, height);
        }

        protected static void DrawFlick(PointF center, DrawingSettings settings, Graphics g)
        {
            var brush = Brushes.DeepPink;
            var pen = new Pen(brush, settings.NoteOuterWidth);

            var cx = center.X;
            var cy = center.Y;
            var r = settings.NoteRadius;
            g.DrawPolygon(pen, new[]
            {
                new PointF(cx, cy - r),
                new PointF(cx - r, cy),
                new PointF(cx, cy + r),
                new PointF(cx + r, cy)
            });

            r = settings.InnerRadius;
            g.FillPolygon(brush, new[]
            {
                new PointF(cx, cy - r),
                new PointF(cx - r, cy),
                new PointF(cx, cy + r),
                new PointF(cx + r, cy)
            });
        }
    }
}
