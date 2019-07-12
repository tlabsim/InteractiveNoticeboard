using InteractiveNoticeboard.Data_Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for FeaturedVideoViewer.xaml
    /// </summary>
    public partial class FeaturedVideoViewer : UserControl
    {
        string VideoFolderPath = string.Empty;
        List<FeaturedVideo> FeaturedVideos;
        SubtitlesParser.Classes.Parsers.SrtParser SrtParser;
        List<SubtitlesParser.Classes.SubtitleItem> SubtitleItems;
        int CurrentSubtitleIndex = 0;
        System.Timers.Timer SubtitleTimer;

        public event EventHandler Completed;

        public FeaturedVideoViewer()
        {
            InitializeComponent();

            SetControls();
        }

        void SetControls()
        {
            VideoFolderPath = Settings.AppPath.TrimEnd('\\') + @"\Data\FeaturedVideos\";

            FeaturedVideos = FeaturedVideo.GetAllFeaturedVideos();
        }

        void SetUI()
        {
            if (FeaturedVideos == null || FeaturedVideos.Count == 0) return;

            //Choose the video with least play count
            FeaturedVideos.OrderBy(v => v.PlayCount);

            foreach (var video in FeaturedVideos)
            {
                string full_path = VideoFolderPath + video.Filename;

                FileInfo vfi = new FileInfo(full_path);
                if (vfi.Exists && vfi.Extension.ToLower() == ".mp4")
                {
                    VideoPlayer.Source = new Uri(full_path, UriKind.Absolute);

                    #region Load Subtitle
                    if (SrtParser == null)
                    {
                        SrtParser = new SubtitlesParser.Classes.Parsers.SrtParser();
                    }

                    string subtitle_path = VideoFolderPath + video.SubtitleFilename;
                    FileInfo sfi = new FileInfo(subtitle_path);
                    if (sfi.Exists && sfi.Extension.ToLower() == ".srt")
                    {
                        try
                        {
                            using (FileStream fs = File.OpenRead(subtitle_path))
                            {
                                SubtitleItems = SrtParser.ParseStream(fs, Encoding.Default);

                                if (SubtitleItems != null && SubtitleItems.Count > 0)
                                {
                                    if (SubtitleTimer == null)
                                    {
                                        SubtitleTimer = new System.Timers.Timer();
                                        SubtitleTimer.Interval = 100;
                                        SubtitleTimer.Elapsed += SubtitleTimer_Elapsed;
                                    }

                                    SubtitleTimer.Start();
                                }
                            }
                        }
                        catch { }
                    }
                    #endregion

                    video.IncrementPlayCount();
                    txtVideoTitle.Text = video.Title;
                    VideoPlayer.MediaEnded += VideoPlayer_MediaEnded;
                    VideoPlayer.Play();                    

                    break;
                }
            }
        }

        void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (SubtitleTimer != null) SubtitleTimer.Stop();

            EventHandler eh = this.Completed;

            if(eh != null)
            {
                eh(this, new EventArgs());
            }
        }

        void SubtitleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                    {
                        OnSubtitleTimerElapsed();
                    });
            }
            catch { }
        }

        void OnSubtitleTimerElapsed()
        {
            if (SubtitleItems == null)
            {
                SubtitleTimer.Stop();
                return;
            }

            TimeSpan current_video_pos = VideoPlayer.Position;

            if (CurrentSubtitleIndex >= 0 && CurrentSubtitleIndex < SubtitleItems.Count)
            {
                var cur_sub = SubtitleItems[CurrentSubtitleIndex];
                TimeSpan et = TimeSpan.FromMilliseconds(cur_sub.EndTime);
                if (current_video_pos > et)
                {
                    txtSubtitle.Inlines.Clear();

                    CurrentSubtitleIndex++;
                }
            }

            if (CurrentSubtitleIndex >= 0 && CurrentSubtitleIndex < SubtitleItems.Count)
            {
                var cur_sub = SubtitleItems[CurrentSubtitleIndex];
                TimeSpan st = TimeSpan.FromMilliseconds(cur_sub.StartTime);
                if (current_video_pos > st)
                {
                    txtSubtitle.Inlines.Clear();
                    for (int i = 0; i < cur_sub.Lines.Count; i++)
                    {
                        var l = cur_sub.Lines[i];
                        txtSubtitle.Inlines.Add(new Run() { Text = l });
                        if (i < cur_sub.Lines.Count - 1)
                        {
                            txtSubtitle.Inlines.Add(new LineBreak());
                        }
                    }
                }
            }            
        }

        void AddEvents()
        { }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetUI();
            AddEvents();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                VideoPlayer.Stop();                
            }
            catch { }
            if (SubtitleTimer != null) SubtitleTimer.Stop();
        }

        private void btnSkip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.VideoPlayer.Stop();
            if (SubtitleTimer != null) SubtitleTimer.Stop();

            EventHandler eh = this.Completed;

            if (eh != null)
            {
                eh(this, new EventArgs());
            }
        }
    }
}
