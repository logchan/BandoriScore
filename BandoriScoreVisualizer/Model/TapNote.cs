using System.Drawing;

namespace BandoriScoreVisualizer.Model
{
    public class TapNote : Note
    {
        public override Timing Start => Time;
        public override Timing End => Time;

        public override bool Visible => true;

        public override void Draw(DrawingSettings settings, Graphics g)
        {
            if (IsFlick)
            {
                DrawFlick(settings.NotePosition(Track, Time), settings, g);
            }
            else
            {
                DrawNote(settings.NotePosition(Track, Time), IsSkill ? Color.Gold : Color.White, settings, g);
            }
        }
    }
}
