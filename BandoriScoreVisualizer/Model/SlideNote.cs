using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace BandoriScoreVisualizer.Model
{
    public class SlideNote : Note
    {
        [JsonProperty("ticks")]
        public List<SlideNoteTick> Ticks { get; } = new List<SlideNoteTick>();

        public override Timing Start => Ticks[0].Time;
        public override Timing End => Ticks[Ticks.Count - 1].Time;
        public int LastTrack => Ticks[Ticks.Count - 1].Track;
        public override bool Visible => true;

        public override void Draw(DrawingSettings settings, Graphics g)
        {
            for (var i = 0; i < Ticks.Count - 1; ++i)
            {
                var tick = Ticks[i];
                var nextTick = Ticks[i + 1];
                var c1 = settings.NotePosition(tick.Track, tick.Time);
                var c2 = settings.NotePosition(nextTick.Track, nextTick.Time);
                g.FillPolygon(new SolidBrush(Color.FromArgb(60, Color.Green)), new[]
                {
                    new PointF(c1.X - settings.NoteRadius, c1.Y),
                    new PointF(c2.X - settings.NoteRadius, c2.Y),
                    new PointF(c2.X + settings.NoteRadius, c2.Y),
                    new PointF(c1.X + settings.NoteRadius, c1.Y)
                });
            }

            for (var i = 0; i < Ticks.Count - 1; ++i)
            {
                var tick = Ticks[i];
                DrawNote(settings.NotePosition(tick.Track, tick.Time), (i == 0 && IsSkill) ? Color.Gold : Color.Green, settings, g, i != 0);
            }

            var last = Ticks[Ticks.Count - 1];
            if (IsFlick)
            {
                DrawFlick(settings.NotePosition(last.Track, last.Time), settings, g);
            }
            else
            {
                DrawNote(settings.NotePosition(last.Track, last.Time), Color.Green, settings, g);
            }
        }
    }
}
