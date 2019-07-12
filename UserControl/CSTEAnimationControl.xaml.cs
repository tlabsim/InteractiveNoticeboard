using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InteractiveNoticeboard.Data_Structures;
using TLABS.Extensions;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for CSTEAnimationControl.xaml
    /// </summary>
    public partial class CSTEAnimationControl : UserControl
    {
        public class MStream
        {
            public int Column = 0;
            public int Start = 0;
            public int Current = 0;
            public int Max = 0;
            public int PreviousBlockIndex = -1;
            public int CurrentBlockIndex = 0;
            public int CurrentCharIndex = 0;
            public string Text = string.Empty;
        }

        public class LogoBlockRange
        {
            public int Start = 0;
            public int End = 0;
        }

        Random RNG = new Random();

        List<SolidColorBrush> NormalColorPalette = new List<SolidColorBrush>()
        {
            new SolidColorBrush(Color.FromRgb(50, 200, 50)),
            new SolidColorBrush(Color.FromRgb(50, 210, 50)),
            new SolidColorBrush(Color.FromRgb(50, 220, 50)),
            new SolidColorBrush(Color.FromRgb(50, 230, 50)),
            new SolidColorBrush(Color.FromRgb(60, 200, 60)),
            new SolidColorBrush(Color.FromRgb(60, 210, 60)),
            new SolidColorBrush(Color.FromRgb(60, 220, 60)),
            new SolidColorBrush(Color.FromRgb(60, 230, 60))
        };

        List<SolidColorBrush> LogoColorPalette = new List<SolidColorBrush>() 
        {            
            new SolidColorBrush(Color.FromArgb(60, 100, 180, 20)),
             new SolidColorBrush(Color.FromArgb(60, 100, 190, 20)),
             new SolidColorBrush(Color.FromArgb(60, 100, 200, 20)),
             new SolidColorBrush(Color.FromArgb(60, 100, 180, 30)),
             new SolidColorBrush(Color.FromArgb(60, 100, 190, 30)),
             new SolidColorBrush(Color.FromArgb(60, 100, 200, 30))
        };

        double block_width = 16;
        double block_height = 16;
        double text_size = 12;

        int horizontal_blocks = 0;
        int vertical_blocks = 0;

        double canvas_width = 0;
        double canvas_height = 0;

        int mid_h_blocks = 72;
        int mid_v_blocks = 15;
        int side_h_blocks = 0;
        int side_v_blocks = 0;

        List<Canvas> ContainerCanvases = new List<Canvas>();
        List<Border> Blocks = new List<Border>();
        Border[] BlocksArray;
        int block_count = 0;

        List<MStream> Streams = new List<MStream>();
        //int MaxStreams = 20;

        public List<LogoBlockRange> LogoBlockRanges = new List<LogoBlockRange>();

        List<string> StreamTexts = new List<string>()
        {
            "Interactive Noticeboard System",
            "Department of Computer Science and Telecommunication Engineering",
            "Noakhali Science and Technology University",            
        };        

        string AlphaNumerics = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public CSTEAnimationControl()
        {
            InitializeComponent();

            SetControls();
        }

        void SetControls()
        {
            var teachers = Teacher.GetAllTeachers();
            foreach(var teacher in teachers)
            {
                StreamTexts.Add(string.Format("{0} | {1}", teacher.Name, teacher.Email));
            }
        }

        public void SetupCanvas()
        {
            double screen_width = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screen_height = System.Windows.SystemParameters.PrimaryScreenHeight;

            horizontal_blocks = (int)Math.Round(screen_width / block_width);
            vertical_blocks = (int)Math.Round(screen_height / block_height);

            if (horizontal_blocks % 2 == 1)
            {
                horizontal_blocks += 1;
            }

            if (vertical_blocks % 2 == 1)
            {
                vertical_blocks += 1;
            }

            if (horizontal_blocks < 72) horizontal_blocks = 72;
            if (vertical_blocks < 20) vertical_blocks = 20;

            canvas_width = horizontal_blocks * block_width;
            canvas_height = vertical_blocks * block_height;

            CSTEAnimationCanvas.Width = canvas_width;
            CSTEAnimationCanvas.Height = canvas_height;

            side_h_blocks = (horizontal_blocks - mid_h_blocks) / 2;
            side_v_blocks = (vertical_blocks - mid_v_blocks) / 2;

            ContainerCanvases.Clear();
            Blocks.Clear();
            block_count = 0;

            //Make 3D
            double[] transform_vertor = new double[horizontal_blocks];
            for (int h = 0; h < horizontal_blocks; h++)
            {
                double tr = (RNG.NextDouble() + RNG.NextDouble() - 1) / 1.0;
                double abstr = Math.Abs(tr);

                transform_vertor[h] = tr;

                Canvas c = new Canvas();
                c.CacheMode = new BitmapCache();
                c.Width = block_width;
                c.Height = vertical_blocks * block_height;

                c.SetValue(Canvas.LeftProperty, h * block_width);
                c.SetValue(Canvas.TopProperty, 0.0);

                c.RenderTransformOrigin = new Point(0.5, 0.5);
                c.RenderTransform = new ScaleTransform() { ScaleX = 1 + tr, ScaleY = 1 + tr };
                c.Effect = new BlurEffect() { Radius = abstr * 5 };

                CSTEAnimationCanvas.Children.Add(c);
                c.CacheMode = new BitmapCache();
                ContainerCanvases.Add(c);
            }

            for (int v = 0; v < vertical_blocks; v++)
            {
                for (int h = 0; h < horizontal_blocks; h++)
                {
                    Border b = new Border();
                    b.Width = block_width;
                    b.Height = block_height;
                    b.Background = new SolidColorBrush(Colors.Transparent);

                    b.SetValue(Canvas.LeftProperty, 0.0);
                    b.SetValue(Canvas.TopProperty, v * block_height);

                    //b.Opacity = 0;

                    TextBlock tb = new TextBlock();
                    tb.FontSize = text_size;
                    tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tb.Foreground = GetANormalColor();             

                    b.Child = tb;                                        

                    ContainerCanvases[h].Children.Add(b);

                    Blocks.Add(b);
                    block_count++;
                }
            }

            BlocksArray = Blocks.ToArray();

            LogoBlockRanges.Clear();

            int cbi = 0;
            int ci = 0;
            int ri = 0;

            //C
            ci = side_h_blocks;
            ri = side_v_blocks;
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });

            //S
            ci = side_h_blocks + 18;
            ri = side_v_blocks;
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 12, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 12, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 12, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });

            //T
            ci = side_h_blocks + 36;
            ri = side_v_blocks;
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi + 6, End = cbi + 9 });

            //E
            ci = side_h_blocks + 54;
            ri = side_v_blocks;
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 3 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });
            cbi = ri++ * horizontal_blocks + ci;
            LogoBlockRanges.Add(new LogoBlockRange() { Start = cbi, End = cbi + 15 });

        }

        public void StartTextStream()
        {
            //Initial streams
            Streams.Clear();

            int initial_streams = 50; // 5 + RNG.Next(5);
            for (int i = 0; i < initial_streams; i++)
            {
                Streams.Add(GetATextStream());
            }

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 50;

            Border b, pb;
            TextBlock tb, ptb;

            timer.Elapsed += (o, e) =>
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        for (int s = 0; s < Streams.Count;s++ )
                        {
                            MStream stream = Streams[s];
                            if (stream.Current <= stream.Max && stream.CurrentCharIndex < stream.Text.Length)
                            {
                                b = BlocksArray[stream.CurrentBlockIndex];
                                tb = b.Child as TextBlock;
                                tb.Text = stream.Text[stream.CurrentCharIndex++].ToString();
                                tb.Foreground = new SolidColorBrush(Color.FromRgb(150, 255, 150));
                                tb.FontWeight = FontWeights.Bold;

                                if (LogoBlockRanges.Exists(br => stream.CurrentBlockIndex >= br.Start && stream.CurrentBlockIndex < br.End))
                                {
                                    b.Background = GetALogoColor();
                                }

                                if(stream.PreviousBlockIndex>=0)
                                {
                                    pb = BlocksArray[stream.PreviousBlockIndex];
                                    ptb = pb.Child as TextBlock;
                                    ptb.Foreground = GetANormalColor();
                                    ptb.FontWeight = FontWeights.Normal;
                                }

                                stream.Current++;
                                stream.PreviousBlockIndex = stream.CurrentBlockIndex;
                                stream.CurrentBlockIndex += horizontal_blocks;
                            }
                            else
                            {
                                Streams.Remove(stream);
                                MStream ns = GetATextStream();
                                ns.PreviousBlockIndex = stream.PreviousBlockIndex;
                                Streams.Insert(s, ns);
                            }
                        }                       
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
                catch { }
            };

            timer.Start();
        }

        public MStream GetATextStream()
        {
            MStream stream = new MStream();
            stream.Column = RNG.Next(horizontal_blocks);
            stream.Start = RNG.Next(vertical_blocks - 5);
            stream.Current = stream.Start;
            stream.CurrentBlockIndex = stream.Current * horizontal_blocks + stream.Column;
            stream.Max = vertical_blocks - 1;

            int ti = RNG.Next(StreamTexts.Count + 1);
            if (ti == StreamTexts.Count)
            {
                stream.Text = GetARandomString() + GetRandomLengthWhitespaces();
            }
            else
            {
                stream.Text = StreamTexts[ti] + GetRandomLengthWhitespaces();
            }

            return stream;
        }

        public string GetARandomString()
        {
            string rs = string.Empty;
            int rsl = 10 + RNG.Next(10);            

            for (int l = 0; l < rsl; l++)
            {
                rs += AlphaNumerics[RNG.Next(62)].ToString();
            }

            return rs;
        }

        public string GetRandomLengthWhitespaces(int max_length = 20)
        {
            return new String(' ', 5 + RNG.Next(max_length - 5));
        }

        SolidColorBrush GetANormalColor()
        {
            int c = NormalColorPalette.Count;
            if (c > 0)
            {
                return NormalColorPalette[RNG.Next(c)];
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(128, 128, 128));
            }
        }

        SolidColorBrush GetALogoColor()
        {
            int c = LogoColorPalette.Count;
            if (c > 0)
            {
                return LogoColorPalette[RNG.Next(c)];
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(30, 50, 150));
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {           
        }
    }
}
