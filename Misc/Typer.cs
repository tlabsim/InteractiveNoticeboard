using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Timers;
using System.Windows.Media.Animation;
using System.Media;

namespace InteractiveNoticeboard
{
    public class Typer
    {
        public class TypingEventArgs : EventArgs
        {
            public char Char { get; set; }
        }

        public delegate void TypingEventHandler(object sender, TypingEventArgs eargs);

        public class Line
        {
            public string Text { get; set; }
            public double FontSize { get; set; }
            public SolidColorBrush Foreground { get; set; }

            public Line()
            {
                Text = string.Empty;
                FontSize = 12;
                Foreground = new SolidColorBrush(Colors.Black);
            }

            public Line(string text)
            {
                this.Text = text;
            }

            public Line(string text, double fontsize)
            {
                this.Text = text;
                this.FontSize = fontsize;
            }

            public Line(string text, double fontsize, SolidColorBrush foreground)
            {
                this.Text = text;
                this.FontSize = fontsize;
                this.Foreground = foreground;
            }

            public Line(string text, double fontsize, string foreground)
            {
                this.Text = text;
                this.FontSize = fontsize;
                try
                {
                    this.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(foreground));
                }
                catch { }
            }
        }

        static Random RNG = new Random();

        public TextBlock TextBlock { get; set; }
        public int TypeSpeed = 100;
        int PreWaitTicks = 10;        

        public event EventHandler TypingStarted;
        public event TypingEventHandler Typing;
        public event EventHandler TypingEnded;


        public Typer()
        {

        }

        public void Type(params Line[] lines)
        {
            if (TextBlock != null && lines != null && lines.Length > 0)
            {
                TextBlock.Inlines.Clear();

                int lines_count = lines.Length;
                int current_line_index = 0;
                int i = 0;
                Line current_line = lines[current_line_index];
                int current_line_length = current_line.Text.Length;
                                
                //Add Cursor
                InlineUIContainer cursor_container = new InlineUIContainer() { BaselineAlignment = BaselineAlignment.Baseline };
                Rectangle rect_cursor = new Rectangle() { RadiusX = 2, RadiusY = 2, Width = current_line.FontSize / 2, Height = current_line.FontSize / 10, Fill = current_line.Foreground };
                rect_cursor.Loaded += rect_cursor_Loaded;
                cursor_container.Child= rect_cursor;

                this.TextBlock.Inlines.Add(cursor_container);

                Run r = new Run();
                this.TextBlock.Inlines.InsertBefore(cursor_container, r);

                Timer type_timer = new Timer();
                type_timer.Interval = TypeSpeed;

                int pwt = this.PreWaitTicks;

                EventHandler ts = this.TypingStarted;
                TypingEventHandler teh = this.Typing;
                EventHandler te = this.TypingEnded;

                if (ts != null)
                {
                    ts(this, new EventArgs());
                }

                type_timer.Elapsed += (o, e) =>
                    {
                        try
                        {
                            if (pwt > 0)
                            {
                                pwt--;
                                return;
                            }

                            this.TextBlock.Dispatcher.Invoke(() =>
                            {
                                if (i >= current_line_length)
                                {
                                    if (current_line_index == lines_count - 1)
                                    {
                                        //finish
                                        type_timer.Stop();
                                        type_timer.Dispose();
                                       
                                        if (te != null)
                                        {
                                            te(this, new EventArgs());
                                        }

                                        return;
                                    }
                                    else
                                    {
                                        i = 0;
                                        current_line_index++;
                                        current_line = lines[current_line_index];
                                        current_line_length = current_line.Text.Length;

                                        if (teh != null)
                                        {
                                            teh(this, new TypingEventArgs() { Char = '\n' });
                                        }

                                        LineBreak lb = new LineBreak();
                                        this.TextBlock.Inlines.InsertAfter(r, lb);                                        

                                        rect_cursor.Width = current_line.FontSize / 2;
                                        rect_cursor.Height = current_line.FontSize / 10;
                                        rect_cursor.Fill = current_line.Foreground;

                                        r = new Run();
                                        r.FontSize = current_line.FontSize;
                                        r.Foreground = current_line.Foreground;
                                        this.TextBlock.Inlines.InsertAfter(lb, r);
                                    }
                                }

                                char char_to_type = current_line.Text[i++];
                                
                                if(teh!=null)
                                {
                                    teh(this, new TypingEventArgs() { Char = char_to_type });
                                }

                                r.Text = r.Text + char_to_type;                                

                            }, System.Windows.Threading.DispatcherPriority.Background);
                        }
                        catch
                        {
                            type_timer.Dispose();
                        }

                        type_timer.Interval = (TypeSpeed - TypeSpeed / 4) + RNG.Next(TypeSpeed / 2);
                    };

                type_timer.Start();
            }
        }

        void rect_cursor_Loaded(object sender, RoutedEventArgs e)
        {
            double SpeedRatio = 1.0;

            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                System.Windows.Media.Animation.QuadraticEase QEaseOut = new System.Windows.Media.Animation.QuadraticEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };
                System.Windows.Media.Animation.Storyboard SB = new System.Windows.Media.Animation.Storyboard();
                SB.RepeatBehavior = RepeatBehavior.Forever;
                SB.Duration = System.TimeSpan.FromMilliseconds(500 * SpeedRatio);

                System.Windows.Media.Animation.DoubleAnimation fadeout = new System.Windows.Media.Animation.DoubleAnimation(0, System.TimeSpan.FromMilliseconds(100 * SpeedRatio));
                fadeout.BeginTime = new System.TimeSpan(0, 0, 0, 0, 0);
                fadeout.EasingFunction = QEaseOut;
                System.Windows.Media.Animation.Storyboard.SetTarget(fadeout, fe);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeout, new PropertyPath(TextBlock.OpacityProperty));
                SB.Children.Add(fadeout);

                System.Windows.Media.Animation.DoubleAnimation fadein = new System.Windows.Media.Animation.DoubleAnimation(1, System.TimeSpan.FromMilliseconds(100 * SpeedRatio));
                fadein.BeginTime = new System.TimeSpan(0, 0, 0, 0, 200);
                fadein.EasingFunction = QEaseOut;
                System.Windows.Media.Animation.Storyboard.SetTarget(fadein, fe);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadein, new PropertyPath(TextBlock.OpacityProperty));
                SB.Children.Add(fadein);

                SB.Begin();
            }
        }

        public static void Type(TextBlock TextBlock, int TypeSpeed, int PreWaitTicks, params Line[] lines)
        {

        }
    }
}
