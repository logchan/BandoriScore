using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
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
                var root = JObject.Parse(File.ReadAllText(jsonFile));
                data.MetaData = JsonConvert.DeserializeObject<MetaData>(root["metadata"].ToString());
                var jsonNotes = (JArray)root["notes"];
                var notes = data.Notes;

                foreach (var token in jsonNotes)
                {
                    if (!(token is JObject))
                        continue;
                    var jNote = (JObject)token;
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

            var appSettings = ConfigurationManager.AppSettings;
            var fontFile = appSettings["font"];
            Font metaFont = null;
            if (!String.IsNullOrEmpty(fontFile))
            {
                try
                {
                    var fontCollection = new PrivateFontCollection();
                    fontCollection.AddFontFile(fontFile);
                    metaFont = new Font(fontCollection.Families[0], 16.0f);
                    Console.WriteLine($"Use font: {metaFont.Name} ({metaFont.Height})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot read {fontFile}: {ex.GetDetails()}");
                    return;
                }
            }

            Bitmap bm;
            try
            {
                bm = Draw(data, metaFont);
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

        private static Graphics CreateGraphics(Image image)
        {
            var g = Graphics.FromImage(image);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            return g;
        }

        public static Bitmap Draw(ParsedData data, Font metaFont)
        {
            var lastEnd = data.Notes.Max(n => n.End);

            var settings = new DrawingSettings
            {
                NumberOfBars = lastEnd.Bar + 1
            };

            if (metaFont != null)
            {
                settings.MetaFont = metaFont;
            }

            var bm = new Bitmap((int)settings.ImageWidth, (int)settings.ImageHeight);
            var g = CreateGraphics(bm);

            // draw grid
            for (var track = 0; track <= 7; ++track)
            {
                var bottom = settings.GridPosition(track, new Timing(0, 0, 1));
                var top = settings.GridPosition(track, new Timing(lastEnd.Bar, 1, 1));
                g.DrawLine((track == 0 || track == 7) ? settings.BarPen : settings.TrackPen, bottom, top);
            }

            for (var bar = 0; bar <= lastEnd.Bar + 1; ++bar)
            {
                g.DrawLine(settings.BarPen,
                    settings.GridPosition(0, new Timing(bar, 0, 1)),
                    settings.GridPosition(7, new Timing(bar, 0, 1))
                );
                var textPoint = settings.GridPosition(0, new Timing(bar, 0, 1));
                textPoint.X -= 2;
                g.DrawString((bar + 1).ToString("000"), settings.BarNumberFont, Brushes.White, textPoint, 
                    new StringFormat
                    {
                        Alignment = StringAlignment.Far,
                        LineAlignment = StringAlignment.Center
                    });

                if (bar == lastEnd.Bar + 1)
                    continue;
                for (var beat = 1; beat < 4; ++beat)
                {
                    g.DrawLine(settings.SubbarPen,
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
                        g.DrawLine(settings.SyncPen,
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
                            g.DrawLine(settings.SyncPen,
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
                                g.DrawLine(settings.SyncPen,
                                    settings.NotePosition(curr.Track, curr.Start),
                                    settings.NotePosition(activeSlide.LastTrack, activeSlide.End));
                                activeSlide = slide;
                            }
                            else
                            {
                                var comp = slide.End.CompareTo(activeSlide.End);
                                if (comp == 0)
                                {
                                    g.DrawLine(settings.SyncPen,
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

            var fbm = new Bitmap((int)settings.FinalImageWidth, (int)settings.FinalImageHeight);
            var fg = CreateGraphics(fbm);

            // draw bg
            fg.FillRectangle(settings.BgBrush, new RectangleF(0, 0, settings.FinalImageWidth, settings.FinalImageHeight));

            // draw meta
            fg.DrawString($"{data.MetaData.Title} [{data.MetaData.Difficulty.ToUpper()}] ★{data.MetaData.Level} Combo {data.MetaData.Combo}", settings.MetaFont, Brushes.White, new PointF(10, 10));
            fg.DrawString($"BPM {data.MetaData.Bpm}", settings.MetaFont, Brushes.White, new PointF(10, settings.MetaFont.Height + 12));

            var col = 0;
            for (var start = 0; start < settings.NumberOfBars; start += settings.BarChunkSize, col++)
            {
                var end = Math.Min(start + settings.BarChunkSize, settings.NumberOfBars);
                var chunkSize = end - start;
                var sourceTop = settings.ImageHeight - settings.ImageMargin - end * settings.BarHeight - settings.BarHeight / 8;
                var sourceHeight = chunkSize * settings.BarHeight + settings.BarHeight / 4;
                var targetTop = settings.ImageMargin + (settings.BarChunkSize - chunkSize) * settings.BarHeight;
                var targetLeft = col * settings.ColumnWidth;
                fg.DrawImage(
                    bm,
                    targetLeft, targetTop,
                    new RectangleF(
                        0, sourceTop,
                        settings.ImageWidth, sourceHeight
                    ),
                    GraphicsUnit.Pixel
                );
                if(start > 0)
                {
                    fg.DrawLine(settings.BarPen,
                        targetLeft + settings.ImageMargin - settings.NoteMargin / 2,
                        settings.FinalImageHeight - settings.ImageMargin,
                        targetLeft + settings.ColumnWidth + settings.NoteMargin / 2,
                        settings.FinalImageHeight - settings.ImageMargin
                    );
                }
                if(start + settings.BarChunkSize < settings.NumberOfBars)
                {
                    fg.DrawLine(settings.BarPen,
                        targetLeft + settings.ImageMargin - settings.NoteMargin / 2,
                        settings.ImageMargin,
                        targetLeft + settings.ColumnWidth + settings.NoteMargin / 2,
                        settings.ImageMargin
                    );
                }
            }

            return fbm;
        }
    }
}
