using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TLABS.Extensions;
using TLABS.WPF.Extensions;
using TLABS.BinPacking;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for NoticeboardControl.xaml
    /// </summary>
    public partial class NoticeboardControl : UserControl
    {
        string NoticeFilesLocation = @"Notices\";
        public List<Notice> Notices;
        Dictionary<Notice, Border> NoticeUIElements = new Dictionary<Notice, Border>();
        int CurrentEnlargedNoticeIndex = -1;        

        double GlobalScaleFactor = 1.0;

        Random RNG = new Random();
        System.Timers.Timer NoticeTimer;
  
        public NoticeboardControl()
        {
            InitializeComponent();
        }

        public void LoadNotices(int max = 10)
        {
            if (this.Notices == null)
            {
                this.Notices = new List<Notice>();
            }
            else
            {
                this.Notices.Clear();
            }

            LoadScannedNotices();
            LoadRTFNotices();

            this.Notices.Sort((n1, n2) => n2.LastWriteTime.CompareTo(n1.LastWriteTime));

            if (this.Notices.Count > max)
            {
                this.Notices = this.Notices.GetRange(0, max);
            }
        }

        void LoadScannedNotices()
        {            
            DirectoryInfo DI = new DirectoryInfo(NoticeFilesLocation);

            if (DI.Exists)
            {
                List<FileInfo> img_files = DI.GetFiles("*.jpg").ToList();

                img_files.ForEach(f =>
                {
                    if (f.Exists)
                    {
                        BitmapImage img = new BitmapImage();
                        img.LoadFromFile(f.FullName);

                        ScannedNotice notice = new ScannedNotice();
                        notice.Image = img;
                        notice.LastWriteTime = f.LastWriteTime;

                        notice.Width = img.Width;
                        notice.Height = img.Height;

                        Notices.Add(notice);
                    }
                });
            }
        }        

        void LoadRTFNotices()
        {
            DirectoryInfo DI = new DirectoryInfo(NoticeFilesLocation);

            if (DI.Exists)
            {
                List<FileInfo> img_files = DI.GetFiles("*.rtf").ToList();

                img_files.ForEach(f =>
                {
                    if (f.Exists)
                    {
                        RichTextNotice notice = new RichTextNotice();
                        notice.FileLocation = f.FullName;
                        notice.LastWriteTime = f.LastWriteTime;

                        notice.Width = 1654;
                        notice.Height = 2338;

                        Notices.Add(notice);
                    }
                });
            }
        }

        public void ArrangeNotices()
        {
            NoticeUIElements.Clear();

            double canvas_width = CanvasNotices.ActualWidth;
            double canvas_height = CanvasNotices.ActualHeight;
            double canvas_aspect_ratio = canvas_width / canvas_height;           
            
            int min_area_needed_in_blocks = Notices.Sum(n => n.AreaInBlocks);
            int min_height = Notices.Max(n => n.HeightInBlocks);
            int min_width = Notices.Max(n => n.WidthInBlocks);

            double d_canvas_block_height = Math.Sqrt((double)min_area_needed_in_blocks / canvas_aspect_ratio);

            if (d_canvas_block_height < min_height) d_canvas_block_height = min_height;

            double d_canvas_block_width = d_canvas_block_height * canvas_aspect_ratio;
            if (d_canvas_block_width < min_width) d_canvas_block_width = min_width;

            int canvas_block_width = (int)Math.Ceiling(d_canvas_block_width);
            int canvas_block_height = (int)Math.Ceiling(d_canvas_block_height);
            double grid_size = canvas_width / canvas_block_width;

            Box2D[] b2ds = Notices.Select(n => new Box2D(0, 0, n.WidthInBlocks, n.HeightInBlocks) { Tag = n }).ToArray();

            TLABS.BinPacking.Algorithms.SubAlgFillOneColumn(canvas_block_width, b2ds);
            GlobalScaleFactor = grid_size / Notice.BlockSize;

            int max_w = b2ds.Max(b => b.Right);
            int max_h = b2ds.Max(b => b.Bottom);

            if (max_h > canvas_block_height)
            {
                double scale = (double)canvas_block_height / (double)(max_h + 1);

                GlobalScaleFactor *= scale;
                grid_size *= scale;
            }

            int wblocks = (int)(canvas_width / grid_size);
            double left_margin = ((wblocks - max_w) / 2.0) * grid_size;

            foreach (Box2D box in b2ds)
            {
                Notice n = box.Tag as Notice;

                Border b = new Border();
                b.Width = (box.Width - 2) * grid_size;
                b.Height = (box.Height - 2) * grid_size;

                if (n is ScannedNotice)
                {
                    b.Background = new ImageBrush() { ImageSource = ((ScannedNotice)n).Image };
                    //b.Background = new SolidColorBrush(Colors.Red);
                }
                else if (n is RichTextNotice)
                {
                    RichTextNotice rtn = n as RichTextNotice;
                    string fileName = rtn.FileLocation;
                    RichTextBox rtb = new RichTextBox();                    
                    TextRange range;

                    if (File.Exists(fileName))
                    {
                        range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                        using (FileStream f_stream = new FileStream(fileName, FileMode.OpenOrCreate))
                        {
                            range.Load(f_stream, DataFormats.Rtf);
                        }
                    }

                    b.Child = rtb;
                    rtb.IsReadOnly = true;
                    rtb.Background = new ImageBrush() { ImageSource = (BitmapImage)this.Resources["PaperTexture"] , TileMode= TileMode.Tile};
                    rtb.Width = n.Width;
                    rtb.Height = n.Height;
                    rtb.Padding = new Thickness(200, 200, 200, 200);
                    rtb.LayoutTransform = new ScaleTransform() { ScaleX = b.Width / rtb.Width, ScaleY = b.Height / rtb.Height };
                }                                    

                double left = left_margin + (box.X + 1) * grid_size;
                double top = (box.Y + 1) * grid_size;
                b.SetValue(Canvas.LeftProperty, left);
                b.SetValue(Canvas.TopProperty, top);

                b.Effect = new DropShadowEffect() { ShadowDepth = 3, Direction = GetShadowDirection(box.X + box.Width / 2, box.Y + box.Height / 2, max_w, max_h), BlurRadius = 5, Opacity=0.5 };

                ScaleTransform st = new ScaleTransform();
                TransformGroup tg = new TransformGroup();
                tg.Children.Add(st);
                b.RenderTransform = tg;
                //b.CacheMode = new BitmapCache() { RenderAtScale = 1 / GlobalScaleFactor , SnapsToDevicePixels=false};

                CanvasNotices.Children.Add(b);

                if (NoticeUIElements.ContainsKey(n))
                {
                    NoticeUIElements[n] = b;
                }
                else
                {
                    NoticeUIElements.Add(n, b);
                }

                #region Add push pin
                Image img_pushpin = GetRandomPushPinImage();

                img_pushpin.Width = 50 * GlobalScaleFactor;
                img_pushpin.Height = 50 * GlobalScaleFactor;

                img_pushpin.SetValue(Canvas.LeftProperty, left - (25 * GlobalScaleFactor) + (b.Width / 2.0));
                img_pushpin.SetValue(Canvas.TopProperty, top);

                CanvasNotices.Children.Add(img_pushpin); 
                #endregion

                this.Wall.CacheMode = new BitmapCache() { RenderAtScale = 1 / GlobalScaleFactor };
            }
        }        

        public void StartSlideShow()
        {
            if (NoticeTimer == null)
            {
                NoticeTimer = new System.Timers.Timer();
                NoticeTimer.Interval = 5000;

                NoticeTimer.Elapsed += (o, eargs) =>
                {
                    CurrentEnlargedNoticeIndex++;

                    if (CurrentEnlargedNoticeIndex >= Notices.Count)
                    {
                        ResetView();
                        CurrentEnlargedNoticeIndex = -1; //Reset
                    }
                    else
                    {
                        Notice n = Notices[CurrentEnlargedNoticeIndex];
                        ShowNotice(n);
                    }
                };
            }

            NoticeTimer.Start();
        }

        #region Helpers

        double GetShadowDirection(double x, double y, double w, double h)
        {
            if (y < 1) y = 1;

            double midw = w / 2;
            double dx = midw - x;
            double dy = y;

            return 270 - RadiansToDegrees(Math.Atan(dx / dy));
        }

        double DegreesToRadians(double d)
        {
            return d * (Math.PI / 180.0);
        }

        double RadiansToDegrees(double r)
        {
            return r * (180.0 / Math.PI);
        }

        Image GetRandomPushPinImage()
        {
            Image img_pushpin = new Image();

            int c = RNG.Next(6);

            switch (c)
            {
                case 1:
                    img_pushpin.Source = (BitmapImage)this.Resources["PushPin_Purple"];
                    break;

                case 2:
                    img_pushpin.Source = (BitmapImage)this.Resources["PushPin_Red"];
                    break;

                case 3:
                    img_pushpin.Source = (BitmapImage)this.Resources["PushPin_Teal"];
                    break;

                case 4:
                    img_pushpin.Source = (BitmapImage)this.Resources["PushPin_Blue"];
                    break;

                default:
                    img_pushpin.Source = (BitmapImage)this.Resources["PushPin_Green"];
                    break;
            }

            return img_pushpin;
        }

        #endregion

        void ShowNotice(Notice n)
        {
            NoticeTimer.Stop();

            this.Dispatcher.Invoke(() =>
            {
                WST.ScaleX = 1;
                WST.ScaleY = 1;

                Border b = NoticeUIElements[n];
                Point wall_mid = new Point(Wall.ActualWidth / 2, Wall.ActualHeight / 2);
                Point b_mid = new Point(b.ActualWidth / 2, b.ActualHeight / 2);
                Point b_loc = b.TranslatePoint(b_mid, Wall);

                //Curs.Margin = new Thickness(b_loc.X - 2, b_loc.Y - 2, 0, 0);

                double aspect_ratio = Viewport.ActualWidth / Viewport.ActualHeight;
                double sx = Wall.ActualWidth / b.ActualWidth;
                double sy = Wall.ActualHeight / b.ActualHeight;
                double s = sx <= sy ? sx : sy;

                double new_left = (wall_mid.X - b_loc.X) * s;
                double new_top = (wall_mid.Y - b_loc.Y) * s;

                Storyboard sb = new Storyboard();
                sb.SetValue(Timeline.DesiredFrameRateProperty, 60);

                int start = 0, duration = 1000;

                {
                    DoubleAnimation move_x = new DoubleAnimation(new_left, TimeSpan.FromMilliseconds(1000));
                    move_x.BeginTime = TimeSpan.FromMilliseconds(start);
                    Storyboard.SetTarget(move_x, Wall);
                    Storyboard.SetTargetProperty(move_x, new PropertyPath(Canvas.LeftProperty));
                    sb.Children.Add(move_x);
                }
                {
                    DoubleAnimation move_y = new DoubleAnimation(new_top, TimeSpan.FromMilliseconds(1000));
                    move_y.BeginTime = TimeSpan.FromMilliseconds(start);
                    Storyboard.SetTarget(move_y, Wall);
                    Storyboard.SetTargetProperty(move_y, new PropertyPath(Canvas.TopProperty));
                    sb.Children.Add(move_y);
                }

                {
                    DoubleAnimation scale_x = new DoubleAnimation(s, TimeSpan.FromMilliseconds(1000));
                    scale_x.BeginTime = TimeSpan.FromMilliseconds(start);
                    Storyboard.SetTarget(scale_x, Wall);
                    Storyboard.SetTargetProperty(scale_x, new PropertyPath("(Grid.RenderTransform).(ScaleTransform.ScaleX)"));
                    sb.Children.Add(scale_x);
                }
                {
                    DoubleAnimation scale_y = new DoubleAnimation(s, TimeSpan.FromMilliseconds(1000));
                    scale_y.BeginTime = TimeSpan.FromMilliseconds(start);
                    Storyboard.SetTarget(scale_y, Wall);
                    Storyboard.SetTargetProperty(scale_y, new PropertyPath("(Grid.RenderTransform).(ScaleTransform.ScaleY)"));
                    sb.Children.Add(scale_y);
                }

                if (sx > sy)
                {
                    //Expand horizontally

                    double w_mid = b.ActualWidth / 2;
                    double h_mid = w_mid / aspect_ratio;
                    b_mid = new Point(w_mid, h_mid);
                    b_loc = b.TranslatePoint(b_mid, Wall);

                    new_left = (wall_mid.X - b_loc.X) * sx;
                    new_top = (wall_mid.Y - b_loc.Y) * sx;

                    start += 5000;
                    duration += 5000;

                    {
                        DoubleAnimation move_x = new DoubleAnimation(new_left, TimeSpan.FromMilliseconds(500));
                        move_x.BeginTime = TimeSpan.FromMilliseconds(start);
                        Storyboard.SetTarget(move_x, Wall);
                        Storyboard.SetTargetProperty(move_x, new PropertyPath(Canvas.LeftProperty));
                        sb.Children.Add(move_x);
                    }
                    {
                        DoubleAnimation move_y = new DoubleAnimation(new_top, TimeSpan.FromMilliseconds(500));
                        move_y.BeginTime = TimeSpan.FromMilliseconds(start);
                        Storyboard.SetTarget(move_y, Wall);
                        Storyboard.SetTargetProperty(move_y, new PropertyPath(Canvas.TopProperty));
                        sb.Children.Add(move_y);
                    }

                    {
                        DoubleAnimation scale_x = new DoubleAnimation(sx, TimeSpan.FromMilliseconds(500));
                        scale_x.BeginTime = TimeSpan.FromMilliseconds(start);
                        Storyboard.SetTarget(scale_x, Wall);
                        Storyboard.SetTargetProperty(scale_x, new PropertyPath("(Grid.RenderTransform).(ScaleTransform.ScaleX)"));
                        sb.Children.Add(scale_x);
                    }
                    {
                        DoubleAnimation scale_y = new DoubleAnimation(sx, TimeSpan.FromMilliseconds(500));
                        scale_y.BeginTime = TimeSpan.FromMilliseconds(start);
                        Storyboard.SetTarget(scale_y, Wall);
                        Storyboard.SetTargetProperty(scale_y, new PropertyPath("(Grid.RenderTransform).(ScaleTransform.ScaleY)"));
                        sb.Children.Add(scale_y);
                    }

                    duration += 500;

                    double old_top = new_top;

                    h_mid = b.ActualHeight - h_mid;
                    b_mid = new Point(w_mid, h_mid);
                    b_loc = b.TranslatePoint(b_mid, Wall);

                    new_top = (wall_mid.Y - b_loc.Y) * sx;
                    double dy = Math.Abs(new_top - old_top);

                    if (dy > 0)
                    {
                        int time_to_scroll = (int)(dy * 50);

                        start += 500;
                        start += 1000;
                        duration += 1000;

                        {
                            DoubleAnimation move_y = new DoubleAnimation(new_top, TimeSpan.FromMilliseconds(time_to_scroll));
                            move_y.BeginTime = TimeSpan.FromMilliseconds(start);
                            Storyboard.SetTarget(move_y, Wall);
                            Storyboard.SetTargetProperty(move_y, new PropertyPath(Canvas.TopProperty));
                            sb.Children.Add(move_y);
                        }

                        duration += time_to_scroll;
                    }
                }
                else if (sx < sy)
                {
                    //Expand vertically

                    double h_mid = b.ActualHeight / 2;
                    double w_mid = h_mid * aspect_ratio;

                    b_mid = new Point(w_mid, h_mid);
                    b_loc = b.TranslatePoint(b_mid, Wall);

                    new_left = (wall_mid.X - b_loc.X) * sy;
                    new_top = (wall_mid.Y - b_loc.Y) * sy;

                    start += 5000;
                    duration += 5000;

                    {
                        DoubleAnimation move_x = new DoubleAnimation(new_left, TimeSpan.FromMilliseconds(500));
                        move_x.BeginTime = TimeSpan.FromMilliseconds(start);
                        Storyboard.SetTarget(move_x, Wall);
                        Storyboard.SetTargetProperty(move_x, new PropertyPath(Canvas.LeftProperty));
                        sb.Children.Add(move_x);
                    }
                    {
                        DoubleAnimation move_y = new DoubleAnimation(new_top, TimeSpan.FromMilliseconds(500));
                        move_y.BeginTime = TimeSpan.FromMilliseconds(start);
                        Storyboard.SetTarget(move_y, Wall);
                        Storyboard.SetTargetProperty(move_y, new PropertyPath(Canvas.TopProperty));
                        sb.Children.Add(move_y);
                    }

                    {
                        DoubleAnimation scale_x = new DoubleAnimation(sy, TimeSpan.FromMilliseconds(500));
                        scale_x.BeginTime = TimeSpan.FromMilliseconds(start);
                        Storyboard.SetTarget(scale_x, Wall);
                        Storyboard.SetTargetProperty(scale_x, new PropertyPath("(Grid.RenderTransform).(ScaleTransform.ScaleX)"));
                        sb.Children.Add(scale_x);
                    }
                    {
                        DoubleAnimation scale_y = new DoubleAnimation(sy, TimeSpan.FromMilliseconds(500));
                        scale_y.BeginTime = TimeSpan.FromMilliseconds(start);
                        Storyboard.SetTarget(scale_y, Wall);
                        Storyboard.SetTargetProperty(scale_y, new PropertyPath("(Grid.RenderTransform).(ScaleTransform.ScaleY)"));
                        sb.Children.Add(scale_y);
                    }

                    duration += 500;

                    double old_left = new_left;

                    w_mid = b.ActualWidth - w_mid;
                    b_mid = new Point(w_mid, h_mid);
                    b_loc = b.TranslatePoint(b_mid, Wall);

                    new_left = (wall_mid.X - b_loc.X) * sy;
                    double dx = Math.Abs(new_left - old_left);

                    if (dx > 0)
                    {
                        int time_to_scroll = (int)(dx * 50);

                        start += 500;
                        start += 1000;
                        duration += 1000;

                        {
                            DoubleAnimation move_x = new DoubleAnimation(new_left, TimeSpan.FromMilliseconds(time_to_scroll));
                            move_x.BeginTime = TimeSpan.FromMilliseconds(start);
                            Storyboard.SetTarget(move_x, Wall);
                            Storyboard.SetTargetProperty(move_x, new PropertyPath(Canvas.LeftProperty));
                            sb.Children.Add(move_x);
                        }

                        duration += time_to_scroll;
                    }
                }

                sb.Duration = TimeSpan.FromMilliseconds(duration);

                sb.Completed += (_o, _e) => { NoticeTimer.Start(); };
                sb.Begin(this, HandoffBehavior.Compose);
            });
        }

        void ResetView()
        {
             NoticeTimer.Stop();

             this.Dispatcher.Invoke(() =>
             {

                 Storyboard sb = new Storyboard();
                 sb.SetValue(Timeline.DesiredFrameRateProperty, 60);

                 int start = 0, duration = 1000;

                 {
                     DoubleAnimation move_x = new DoubleAnimation(0, TimeSpan.FromMilliseconds(1000));
                     move_x.BeginTime = TimeSpan.FromMilliseconds(start);
                     Storyboard.SetTarget(move_x, Wall);
                     Storyboard.SetTargetProperty(move_x, new PropertyPath(Canvas.LeftProperty));
                     sb.Children.Add(move_x);
                 }
                 {
                     DoubleAnimation move_y = new DoubleAnimation(0, TimeSpan.FromMilliseconds(1000));
                     move_y.BeginTime = TimeSpan.FromMilliseconds(start);
                     Storyboard.SetTarget(move_y, Wall);
                     Storyboard.SetTargetProperty(move_y, new PropertyPath(Canvas.TopProperty));
                     sb.Children.Add(move_y);
                 }

                 {
                     DoubleAnimation scale_x = new DoubleAnimation(1, TimeSpan.FromMilliseconds(1000));
                     scale_x.BeginTime = TimeSpan.FromMilliseconds(start);
                     Storyboard.SetTarget(scale_x, Wall);
                     Storyboard.SetTargetProperty(scale_x, new PropertyPath("(Grid.RenderTransform).(ScaleTransform.ScaleX)"));
                     sb.Children.Add(scale_x);
                 }
                 {
                     DoubleAnimation scale_y = new DoubleAnimation(1, TimeSpan.FromMilliseconds(1000));
                     scale_y.BeginTime = TimeSpan.FromMilliseconds(start);
                     Storyboard.SetTarget(scale_y, Wall);
                     Storyboard.SetTargetProperty(scale_y, new PropertyPath("(Grid.RenderTransform).(ScaleTransform.ScaleY)"));
                     sb.Children.Add(scale_y);
                 }

                 sb.Duration = TimeSpan.FromMilliseconds(duration);
                 sb.Completed += (_o, _e) => { NoticeTimer.Start(); };
                 sb.Begin(this, HandoffBehavior.Compose);
             });
        }

        public void Reset()
        {
            this.NoticeTimer.Stop();

            if (this.Notices != null) this.Notices.Clear();
            if (this.NoticeUIElements != null) this.NoticeUIElements.Clear();
            this.CurrentEnlargedNoticeIndex = -1;

            this.Wall.Margin = new Thickness(0, 0, 0, 0);
            WST.ScaleX = 1;
            WST.ScaleY = 1;            
        }

        void ShowStartupAnimation()
        {
            Storyboard sb = new Storyboard();
            sb.SetValue(Timeline.DesiredFrameRateProperty, 60);

            double notice_text_bottom_margin = (this.Wall.ActualHeight / 2 - this.NoticeText.TranslatePoint(new Point(0, 0), this.Wall).Y - 25) * 2;

            {
                DoubleAnimation scale_x = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));
                scale_x.BeginTime = TimeSpan.FromMilliseconds(1000);
                Storyboard.SetTarget(scale_x, NoticeTextDummy);
                Storyboard.SetTargetProperty(scale_x, new PropertyPath("(Image.RenderTransform).(ScaleTransform.ScaleX)"));
                sb.Children.Add(scale_x);
            }
            {
                DoubleAnimation scale_y = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));
                scale_y.BeginTime = TimeSpan.FromMilliseconds(1000);
                Storyboard.SetTarget(scale_y, NoticeTextDummy);
                Storyboard.SetTargetProperty(scale_y, new PropertyPath("(Image.RenderTransform).(ScaleTransform.ScaleY)"));
                sb.Children.Add(scale_y);
            }
            {
                DoubleAnimation fadein = new DoubleAnimation(.75, TimeSpan.FromMilliseconds(500));
                fadein.BeginTime = TimeSpan.FromMilliseconds(1000);
                Storyboard.SetTarget(fadein, NoticeTextDummy);
                Storyboard.SetTargetProperty(fadein, new PropertyPath(UIElement.OpacityProperty));
                sb.Children.Add(fadein);
            }
            {
                ThicknessAnimation move_up = new ThicknessAnimation(new Thickness(0, 0, 0, notice_text_bottom_margin), TimeSpan.FromMilliseconds(200));
                move_up.BeginTime = TimeSpan.FromMilliseconds(3500);
                Storyboard.SetTarget(move_up, NoticeTextDummy);
                Storyboard.SetTargetProperty(move_up, new PropertyPath(FrameworkElement.MarginProperty));
                sb.Children.Add(move_up);
            }
            {
                DoubleAnimation fadeout = new DoubleAnimation(0, TimeSpan.FromMilliseconds(500));
                fadeout.BeginTime = TimeSpan.FromMilliseconds(4000);
                Storyboard.SetTarget(fadeout, NoticeTextDummy);
                Storyboard.SetTargetProperty(fadeout, new PropertyPath(UIElement.OpacityProperty));
                sb.Children.Add(fadeout);
            }
            {
                DoubleAnimation fadein = new DoubleAnimation(1, TimeSpan.FromMilliseconds(500));
                fadein.BeginTime = TimeSpan.FromMilliseconds(4000);
                Storyboard.SetTarget(fadein, NoticeBoard);
                Storyboard.SetTargetProperty(fadein, new PropertyPath(UIElement.OpacityProperty));
                sb.Children.Add(fadein);
            }

            sb.Duration = TimeSpan.FromMilliseconds(5000);
            sb.Completed += (_o, _e) => {
                LoadNotices();
                ArrangeNotices();
                StartSlideShow();                 
            };

            sb.Begin(this, HandoffBehavior.Compose);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowStartupAnimation();

            //LoadNotices();
            //ArrangeNotices();
            //StartSlideShow();            
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Reset();
        }
    }
}
