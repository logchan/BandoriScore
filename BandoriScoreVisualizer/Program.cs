using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using BandoriScoreVisualizer.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BandoriScoreVisualizer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: BandoriScoreVisualizer <parsed score.json> <output.png>");
                return;
            }

            var jsonFile = args[0];
            var data = new ParsedData();
            try
            {
                // data = JsonConvert.DeserializeObject<ParsedData>(File.ReadAllText(jsonFile));
                var root = JObject.Parse(File.ReadAllText(jsonFile));
                data.MetaData = JsonConvert.DeserializeObject<MetaData>(root["metadata"].ToString());
                var jsonNotes = (JArray) root["notes"];
                var notes = data.Notes;
                foreach (JObject jNote in jsonNotes)
                {
                    var type = jNote["type"].ToString();
                    Note note = null;
                    switch (type)
                    {
                        case "tap":
                            note = JsonConvert.DeserializeObject<TapNote>(jNote.ToString());
                            break;
                        case "slide":
                            note = JsonConvert.DeserializeObject<SlideNote>(jNote.ToString());
                            foreach (var tick in (note as SlideNote).Ticks)
                            {
                                --tick.Time.Bar;
                            }
                            break;
                        case "special":
                            note = JsonConvert.DeserializeObject<SpecialNote>(jNote.ToString());
                            break;
                        default:
                            Console.WriteLine($"Unrecognized note type: {type}");
                            break;
                    }

                    if (note != null)
                    {
                        --note.Time.Bar;
                        notes.Add(note);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot read {jsonFile}: {ex.GetDetails()}");
                return;
            }
            
            Bitmap bm;
            try
            {
                bm = Draw(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while drawing: {ex.GetDetails()}");
                return;
            }

            try
            {
                bm.Save(args[1], ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot save {args[1]}: {ex.GetDetails()}");
            }
        }

        private static readonly Brush BgBrush = new SolidBrush(Color.FromArgb(32, 32, 32));
        private static readonly Pen BarPen = new Pen(Brushes.Gray, 3);
        private static readonly Pen SubbarPen = new Pen(Brushes.Gray, 1);
        private static readonly Pen TrackPen = new Pen(Brushes.Gray, 1);
        private static readonly Font BarNumberFont = new Font("Consolas", 16.0f);
        private static readonly Font MetaFont = new Font("Microsoft YaHei", 16.0f);
        private static readonly Pen SyncPen = new Pen(Brushes.White, 4);

        public static Bitmap Draw(ParsedData data)
        {
            var lastEnd = data.Notes.Max(n => n.End);

            var settings = new DrawingSettings
            {
                NumberOfBars = lastEnd.Bar + 1
            };

            var bm = new Bitmap((int) settings.ImageWidth, (int) settings.ImageHeight);
            var g = Graphics.FromImage(bm);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // draw bg
            g.FillRectangle(BgBrush, new RectangleF(0, 0, settings.ImageWidth, settings.ImageHeight));

            // draw meta
            g.DrawString(data.MetaData.Title, MetaFont, Brushes.White, new PointF(10, 10));
            g.DrawString($"BPM {data.MetaData.Bpm}", MetaFont, Brushes.White, new PointF(10, 40));

            // draw grid
            for (var track = 0; track <= 7; ++track)
            {
                var bottom = settings.GridPosition(track, new Timing(0, 0, 1));
                var top = settings.GridPosition(track, new Timing(lastEnd.Bar, 1, 1));
                g.DrawLine(TrackPen, bottom, top);
            }
            for (var bar = 0; bar <= lastEnd.Bar + 1; ++bar)
            {
                g.DrawLine(BarPen,
                    settings.GridPosition(0, new Timing(bar, 0, 1)),
                    settings.GridPosition(7, new Timing(bar, 0, 1))
                );
                var textPoint = settings.GridPosition(0, new Timing(bar, 0, 1));
                textPoint.X -= 42 + settings.NoteRadius;
                textPoint.Y -= 10;

                g.DrawString((bar + 1).ToString("000"), BarNumberFont, Brushes.White, textPoint);

                if (bar == lastEnd.Bar + 1)
                    continue;
                for (var beat = 1; beat < 4; ++beat)
                {
                    g.DrawLine(SubbarPen,
                        settings.GridPosition(0, new Timing(bar, beat, 4)),
                        settings.GridPosition(7, new Timing(bar, beat, 4))
                    );
                }
            }

            // draw sync lines
            SlideNote activeSlide = null;
            var visibleNotes = (from note in data.Notes
                               where note.Visible
                               select note).ToList();
            for (var i = 0; i < visibleNotes.Count; ++i)
            {
                var curr = visibleNotes[i];
                if (i < visibleNotes.Count - 1)
                {
                    var next = visibleNotes[i + 1];
                    if (curr.Start == next.Start)
                    {
                        g.DrawLine(SyncPen,
                            settings.NotePosition(curr.Track, curr.Start),
                            settings.NotePosition(next.Track, next.Start));
                    }
                }

                if (activeSlide == null)
                {
                    activeSlide = curr as SlideNote;
                }
                else
                {
                    var tap = curr as TapNote;
                    if (tap != null)
                    {
                        if (tap.Time == activeSlide.End)
                        {
                            g.DrawLine(SyncPen,
                                settings.NotePosition(curr.Track, curr.Start),
                                settings.NotePosition(activeSlide.LastTrack, activeSlide.End));
                            activeSlide = null;
                        }
                    }
                    else
                    {
                        var slide = curr as SlideNote;
                        if (slide != null)
                        {
                            if (slide.Start == activeSlide.End)
                            {
                                g.DrawLine(SyncPen,
                                    settings.NotePosition(curr.Track, curr.Start),
                                    settings.NotePosition(activeSlide.LastTrack, activeSlide.End));
                                activeSlide = slide;
                            }
                            else
                            {
                                var comp = slide.End.CompareTo(activeSlide.End);
                                if (comp == 0)
                                {
                                    g.DrawLine(SyncPen,
                                        settings.NotePosition(slide.LastTrack, slide.End),
                                        settings.NotePosition(activeSlide.LastTrack, activeSlide.End));
                                    activeSlide = null;
                                }
                                else if (comp > 0)
                                {
                                    activeSlide = slide;
                                }
                            }
                        }
                    }
                }
            }

            // draw notes
            foreach (var note in data.Notes)
            {
                note.Draw(settings, g);
            }

            return bm;
        }
    }
}
