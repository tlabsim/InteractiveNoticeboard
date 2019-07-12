using InteractiveNoticeboard.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for ClassScheduleViewer.xaml
    /// </summary>
    public partial class ClassScheduleViewer : UserControl
    {
        public enum ShowModes
        {
            Today,
            NextDay
        }

        public ShowModes ShowMode = ShowModes.Today;

        int DayOfWeek;
        DateTime ScheduleDate;          

        List<Batch> CurrentBatches;
        List<ClassSchedule> Schedules;
        List<SpecialSchedule> SpecialSchedules;

        SolidColorBrush
            DarkColor1 = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            DarkColor2 = new SolidColorBrush(Color.FromArgb(255, 16, 16, 16)),
            DarkColor3 = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32)),
            DarkColor4 = new SolidColorBrush(Color.FromArgb(255, 48, 48, 48)),
            DarkColor5 = new SolidColorBrush(Color.FromArgb(255, 64, 64, 64)),
            GreyColor1 = new SolidColorBrush(Color.FromArgb(255, 64, 64, 64)),
            GreyColor2 = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80)),
            GreyColor3 = new SolidColorBrush(Color.FromArgb(255, 96, 96, 96)),
            GreyColor4 = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127)),
            LightColor1 = new SolidColorBrush(Color.FromArgb(255, 192, 192, 192)),
            LightColor2 = new SolidColorBrush(Color.FromArgb(255, 208, 208, 208)),
            LightColor3 = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220)),
            LightColor4 = new SolidColorBrush(Color.FromArgb(255, 232, 232, 232)),
            LightColor5 = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)),
            LightColor6 = new SolidColorBrush(Color.FromArgb(255, 248, 248, 248));

        static TimeSpan MinTime, MaxTime;
        static double RowHeaderWidth, RowContentWidth, TotalHeight, PPM, TopMargin, BottomMargin;

        Border ElapsedTimeBorder;
        Path ElapsedTimePointer_Arrow;
        Border ElapsedTimePointer_Box;
        TextBlock ElapsedTimePointer_TextBlock;

        System.Timers.Timer StateUpdaterTimer;
        System.Timers.Timer CompletionTimer;
        public int TimeToComplete = 60000;
        public event EventHandler Completed;

        public ClassScheduleViewer(ShowModes show_mode = ShowModes.Today)
        {
            InitializeComponent();

            ShowMode = show_mode;
            SetControls();            
        }

        void SetControls()
        {            
        }

        void SetUI()
        {
            DayOfWeek = DateTime.Now.DayOfWeek.GetHashCode() + 1;
            ScheduleDate = DateTime.Now;

            if (ShowMode == ShowModes.NextDay)
            {
                DayOfWeek++;
                if (DayOfWeek > 5)
                {
                    int days_skipped = 8 - DayOfWeek;
                    DayOfWeek = 1;
                    ScheduleDate = DateTime.Now.AddDays(days_skipped);
                }
                else
                {
                    ScheduleDate = DateTime.Now.AddDays(1);
                }

                txtHeader.Text = "Next Day's Schedules";
            }
            else
            {
                //txtHeader.Text = "Today's Class Schedules";
            }

            txtScheduleDate.Text = ScheduleDate.ToString("dddd, dd MMMM yyyy");

            CurrentBatches = Batch.GetBatches(true);
            if (CurrentBatches.Count > 8) CurrentBatches = CurrentBatches.GetRange(0, 8);

            Schedules = ClassSchedule.GetAllClassSchedules(DayOfWeek);
            Schedules = Schedules.FindAll(s =>
                {
                    return CurrentBatches.Exists(b => b.Degree.ToLower() == s.Degree.ToLower() && b.Session == s.Session);
                });

            SpecialSchedules = SpecialSchedule.GetSpecialSchedules(ScheduleDate);
            SpecialSchedules = SpecialSchedules.FindAll(s =>
            {
                return CurrentBatches.Exists(b => b.Degree.ToLower() == s.Degree.ToLower() && b.Session == s.Session);
            });

            if (Schedules.Count == 0 && SpecialSchedules.Count == 0)
            {
                cv.Visibility = System.Windows.Visibility.Collapsed;
                txtNoSchedules.Visibility = System.Windows.Visibility.Visible;
                if (ShowMode == ShowModes.NextDay)
                {
                    runNoSchedules.Text = "No classes scheduled.";
                }
                else
                {
                    //runNoSchedules.Text = "No classes scheduled today.";
                }
                
                TimeToComplete = 30000;
            }
            else
            {
                cv.Visibility = System.Windows.Visibility.Visible;
                txtNoSchedules.Visibility = System.Windows.Visibility.Collapsed;

                cv.Children.Clear();

                int batch_count = CurrentBatches.Count;
                double
                    cv_width = cv.ActualWidth,
                    cv_height = cv.ActualHeight,
                    row_header_width = 200.0,
                    row_content_width = cv_width - row_header_width - 40,
                    top_margin = 0,
                    bottom_margin = 30,
                    timeline_height = 30,
                    row_height = (cv_height - bottom_margin - timeline_height) / (double)batch_count,
                    x = 0,
                    y = 0;

                row_height = row_height > 100 ? 100 : row_height;
                double total_height = (row_height * batch_count) + timeline_height;
                top_margin = (cv_height - bottom_margin - total_height) / 2.0;
                top_margin = top_margin > 50 ? 50 : top_margin;

                TimeSpan
                    earliest = new TimeSpan(8, 0, 0),   // 8 AM
                    latest__ = new TimeSpan(17, 0, 0),  // 5 PM
                    min_time = Schedules.Count > 0 ? Schedules.Min(s => s.StartTime) : earliest,
                    max_time = Schedules.Count > 0 ? Schedules.Max(s => s.EndTime) : latest__,
                    sp_min_t = SpecialSchedules.Count > 0 ? SpecialSchedules.Min(s => s.StartTime) : min_time,
                    sp_max_t = SpecialSchedules.Count > 0 ? SpecialSchedules.Max(s => s.EndTime) : max_time;

                if (sp_min_t < min_time) min_time = sp_min_t;
                if (sp_max_t > max_time) max_time = sp_max_t;
                if (min_time > earliest) min_time = earliest;
                if (max_time < latest__) max_time = latest__;

                int
                    hours = (int)(max_time - min_time).TotalHours,
                    minutes = (int)(max_time - min_time).TotalMinutes,
                    _lms = (int)min_time.TotalMinutes % 30,
                    lms = _lms > 0 ? (30 - _lms) : 0,
                    lme = (int)max_time.TotalMinutes % 30,
                    divs = ((minutes - (lms + lme)) / 30) + 1;

                double ppm = row_content_width / minutes,
                       divw = ppm * 30.0;

                //Timeline
                x = row_header_width + (lms * ppm);
                y = top_margin;

                TimeSpan cur = min_time.Add(TimeSpan.FromMinutes(lms));

                for (int i = 0; i < divs; i++)
                {
                    Rectangle v_line = new Rectangle()
                    {
                        Height = total_height,
                        Width = cur.Minutes == 0 ? 2 : 1,
                        Stroke = LightColor3,
                        Fill = LightColor3
                    };

                    v_line.SetValue(Canvas.LeftProperty, x);
                    v_line.SetValue(Canvas.TopProperty, y);
                    cv.Children.Add(v_line);

                    TextBlock tb = new TextBlock()
                    {
                        Foreground = DarkColor5,
                        FontSize = 15,
                        Text = cur.Minutes == 0 ? TimespanToString(cur) : "",
                        FontFamily = new FontFamily("Consolas")
                    };
                    tb.SetValue(Canvas.LeftProperty, x + 5);
                    tb.SetValue(Canvas.TopProperty, y + 5);
                    cv.Children.Add(tb);
                    cur = cur.Add(TimeSpan.FromMinutes(30));

                    x += divw;
                }

                //Headers
                x = 0;
                y = top_margin + timeline_height;
                for (int i = 0; i < batch_count; i++)
                {
                    Batch batch = CurrentBatches[i];

                    Border b = new Border();
                    b.Width = row_header_width - 2;
                    b.Height = row_height - 2;
                    b.Padding = new Thickness(10);
                    b.CornerRadius = new CornerRadius(5);
                    b.Background = LightColor4;
                    b.BorderBrush = LightColor1;
                    b.BorderThickness = new Thickness(1);
                    b.SetValue(Canvas.LeftProperty, x);
                    b.SetValue(Canvas.TopProperty, y + 1);

                    TextBlock tb = new TextBlock()
                    {
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        FontFamily = new FontFamily("Asap")
                    };

                    tb.Inlines.Add(new Run() { Text = batch.Degree, FontSize = 15, Foreground = GreyColor3 });
                    tb.Inlines.Add(new LineBreak());
                    tb.Inlines.Add(new Run() { Text = string.Format("{0} session", batch.Session), FontFamily = new FontFamily("Lora"), FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(180, 30, 60)) });
                    tb.Inlines.Add(new LineBreak());
                    tb.Inlines.Add(new Run() { Text = batch.CurrentTerm, FontSize = 15, Foreground = GreyColor3 });

                    b.Child = tb;
                    cv.Children.Add(b);

                    Border batch_timeline_back = new Border();
                    batch_timeline_back.Width = row_content_width;
                    batch_timeline_back.Height = row_height - 2;
                    batch_timeline_back.Padding = new Thickness(0);
                    batch_timeline_back.CornerRadius = new CornerRadius(5);
                    batch_timeline_back.Background = new SolidColorBrush(Color.FromArgb(200, 240, 240, 240));
                    batch_timeline_back.BorderBrush = LightColor3;
                    batch_timeline_back.BorderThickness = new Thickness(1);
                    batch_timeline_back.SetValue(Canvas.LeftProperty, x + row_header_width);
                    batch_timeline_back.SetValue(Canvas.TopProperty, y + 1);
                    cv.Children.Add(batch_timeline_back);

                    //Batch class schedules
                    var batch_class_schedules = Schedules.FindAll(s => s.Degree.ToLower() == batch.Degree.ToLower() && s.Session == batch.Session);
                    var special_schedules = SpecialSchedules.FindAll(s => s.Degree.ToLower() == batch.Degree.ToLower() && s.Session == batch.Session);

                    foreach (var class_schedule in batch_class_schedules)
                    {
                        if (DoesOverlap(class_schedule, special_schedules)) continue;

                        double scx = row_header_width + (int)(class_schedule.StartTime - min_time).TotalMinutes * ppm;
                        double scw = (int)(class_schedule.EndTime - class_schedule.StartTime).TotalMinutes * ppm;

                        Border scb = new Border();
                        scb.Width = scw;
                        scb.Height = row_height - 2;
                        scb.Padding = new Thickness(5);
                        scb.CornerRadius = new CornerRadius(5);
                        switch (class_schedule.CourseType)
                        {
                            case "Lab":
                                scb.Background = new SolidColorBrush(Color.FromArgb(24, 0, 112, 192));
                                scb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 112, 192));
                                break;
                            case "Other":
                                scb.Background = new SolidColorBrush(Color.FromArgb(24, 146, 208, 80));
                                scb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 146, 208, 80));
                                break;
                            case "Theory":
                            default:
                                scb.Background = new SolidColorBrush(Color.FromArgb(24, 0, 176, 80));
                                scb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 176, 80));
                                break;
                        }

                        scb.BorderThickness = new Thickness(1);
                        scb.SetValue(Canvas.LeftProperty, scx);
                        scb.SetValue(Canvas.TopProperty, y + 1);

                        TextBlock sctb = new TextBlock()
                        {
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center,
                            FontFamily = new FontFamily("Asap"),
                            TextTrimming = TextTrimming.CharacterEllipsis
                        };

                        string time_and_venue = string.IsNullOrEmpty(class_schedule.Venue) ? string.Format("{0} - {1}", class_schedule.StartTime.ToString(@"hh\:mm"), class_schedule.EndTime.ToString(@"hh\:mm")) : string.Format("{0} - {1} • {2}", class_schedule.StartTime.ToString(@"hh\:mm"), class_schedule.EndTime.ToString(@"hh\:mm"), class_schedule.Venue);
                        sctb.Inlines.Add(new Run() { Text = time_and_venue, FontSize = 14, Foreground = GreyColor2 });
                        sctb.Inlines.Add(new LineBreak());
                        sctb.Inlines.Add(new Run() { Text = class_schedule.CourseCode, FontFamily = new FontFamily("Arial"), FontWeight = FontWeights.Bold, FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(32, 24, 24)) });
                        sctb.Inlines.Add(new LineBreak());
                        sctb.Inlines.Add(new Run() { Text = class_schedule.CourseTitle, FontFamily = new FontFamily("Arial"), FontSize = 14, Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)) });
                        sctb.Inlines.Add(new LineBreak());
                        sctb.Inlines.Add(new Run() { Text = class_schedule.CourseTeacher, FontFamily = new FontFamily("Arial"), FontWeight = FontWeights.Bold, FontSize = 14, Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)) });

                        scb.Child = sctb;
                        cv.Children.Add(scb);
                    }

                    foreach (var special_schedule in special_schedules)
                    {
                        double scx = row_header_width + (int)(special_schedule.StartTime - min_time).TotalMinutes * ppm;
                        double scw = (int)(special_schedule.EndTime - special_schedule.StartTime).TotalMinutes * ppm;

                        Border sscb = new Border();
                        sscb.Width = scw;
                        sscb.Height = row_height - 2;
                        sscb.Padding = new Thickness(5);
                        sscb.CornerRadius = new CornerRadius(5);
                        switch (special_schedule.EventType)
                        {
                            case "Examination":
                                sscb.Background = new SolidColorBrush(Color.FromArgb(24, 192, 0, 0));
                                sscb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 192, 0, 0));
                                break;
                            case "Seminar":
                                sscb.Background = new SolidColorBrush(Color.FromArgb(24, 204, 0, 102));
                                sscb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 204, 0, 102));
                                break;
                            case "Workshop":
                                sscb.Background = new SolidColorBrush(Color.FromArgb(24, 112, 48, 160));
                                sscb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 112, 48, 160));
                                break;
                            case "Other":
                            default:
                                sscb.Background = new SolidColorBrush(Color.FromArgb(24, 128, 128, 128));
                                sscb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
                                break;
                        }

                        sscb.BorderThickness = new Thickness(1);
                        sscb.SetValue(Canvas.LeftProperty, scx);
                        sscb.SetValue(Canvas.TopProperty, y + 1);

                        TextBlock ssctb = new TextBlock()
                        {
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center,
                            FontFamily = new FontFamily("Asap"),
                            TextTrimming = TextTrimming.CharacterEllipsis
                        };

                        string time_and_venue = string.IsNullOrEmpty(special_schedule.Venue) ? string.Format("{0} - {1}", special_schedule.StartTime.ToString(@"hh\:mm"), special_schedule.EndTime.ToString(@"hh\:mm")) : string.Format("{0} - {1} • {2}", special_schedule.StartTime.ToString(@"hh\:mm"), special_schedule.EndTime.ToString(@"hh\:mm"), special_schedule.Venue);
                        ssctb.Inlines.Add(new Run() { Text = time_and_venue, FontSize = 14, Foreground = GreyColor2 });
                        ssctb.Inlines.Add(new LineBreak());
                        ssctb.Inlines.Add(new Run() { Text = special_schedule.EventTitle, FontFamily = new FontFamily("Arial"), FontWeight = FontWeights.Bold, FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(32, 24, 24)) });
                        ssctb.Inlines.Add(new LineBreak());
                        ssctb.Inlines.Add(new Run() { Text = special_schedule.EventDescription, FontFamily = new FontFamily("Arial"), FontSize = 14, Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)) });
                        ssctb.Inlines.Add(new LineBreak());
                        ssctb.Inlines.Add(new Run() { Text = special_schedule.Remarks, FontFamily = new FontFamily("Arial"), FontWeight = FontWeights.Normal, FontSize = 14, Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)) });

                        sscb.Child = ssctb;
                        cv.Children.Add(sscb);
                    }

                    y += row_height;
                }

                MinTime = min_time;
                MaxTime = max_time;
                RowHeaderWidth = row_header_width;
                RowContentWidth = row_content_width;
                TotalHeight = total_height;
                PPM = ppm;
                TopMargin = top_margin;
                BottomMargin = bottom_margin;

                if (ShowMode == ShowModes.Today)
                {
                    #region Elapsed time visualization
                    TimeSpan now = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                    if (now > min_time)
                    {
                        ElapsedTimeBorder = new Border()
                        {
                            //Background = new SolidColorBrush(Color.FromArgb(64, 255, 255, 255)),
                            Background = (ImageBrush)this.Resources["ElapsedTimeBorderBackground"],
                            BorderBrush = new SolidColorBrush(Color.FromArgb(128, 200, 40, 80)),
                            BorderThickness = new Thickness(0, 0, 1, 0),
                            Height = row_height * batch_count
                        };
                        ElapsedTimeBorder.SetValue(Canvas.LeftProperty, row_header_width);
                        ElapsedTimeBorder.SetValue(Canvas.TopProperty, top_margin + timeline_height);

                        #region Animation
                        Storyboard etb_sb = new Storyboard();
                        etb_sb.RepeatBehavior = RepeatBehavior.Forever;
                        {
                            DoubleAnimation slide_x = new DoubleAnimation(0, 25, TimeSpan.FromMilliseconds(1000));
                            slide_x.BeginTime = TimeSpan.FromMilliseconds(0);
                            Storyboard.SetTarget(slide_x, ElapsedTimeBorder);
                            Storyboard.SetTargetProperty(slide_x, new PropertyPath("(Border.Background).(ImageBrush.Transform).(TranslateTransform.X)"));
                            etb_sb.Children.Add(slide_x);
                        }
                        etb_sb.Begin();
                        #endregion

                        ElapsedTimePointer_Arrow = new Path();
                        ElapsedTimePointer_Arrow.Data = Geometry.Parse("M0,10 7,0, 14,10Z");
                        ElapsedTimePointer_Arrow.Fill = new SolidColorBrush(Color.FromArgb(128, 200, 40, 80));

                        ElapsedTimePointer_Box = new Border()
                        {
                            Width = 60,
                            Height = 24,
                            CornerRadius = new CornerRadius(12),
                            Background = new SolidColorBrush(Colors.Transparent),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(128, 200, 40, 80)),
                            BorderThickness = new Thickness(1)
                        };

                        ElapsedTimePointer_TextBlock = new TextBlock()
                        {
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center,
                            FontFamily = new FontFamily("Consolas"),
                            FontSize = 14,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 40, 80)),
                        };
                        ElapsedTimePointer_Box.Child = ElapsedTimePointer_TextBlock;

                        cv.Children.Add(ElapsedTimeBorder);
                        cv.Children.Add(ElapsedTimePointer_Arrow);
                        cv.Children.Add(ElapsedTimePointer_Box);

                        if (now > max_time)
                        {
                            now = max_time;
                            TheDayIsOver.Visibility = System.Windows.Visibility.Visible;
                            if (cv.Effect == null)
                            {
                                cv.Effect = new BlurEffect() { Radius = 10 };
                            }
                        }
                        else
                        {
                            TheDayIsOver.Visibility = System.Windows.Visibility.Collapsed;
                            cv.Effect = null;
                        }

                        double width = (now - min_time).TotalMinutes * ppm;
                        ElapsedTimeBorder.Width = width;

                        ElapsedTimePointer_TextBlock.Text = now.ToString(@"hh\:mm");

                        ElapsedTimePointer_Arrow.SetValue(Canvas.LeftProperty, row_header_width + width - 7.5);
                        ElapsedTimePointer_Arrow.SetValue(Canvas.TopProperty, top_margin + total_height);

                        ElapsedTimePointer_Box.SetValue(Canvas.LeftProperty, row_header_width + width - 30);
                        ElapsedTimePointer_Box.SetValue(Canvas.TopProperty, top_margin + total_height + 10);

                        if (StateUpdaterTimer == null)
                        {
                            StateUpdaterTimer = new System.Timers.Timer();
                            StateUpdaterTimer.Interval = 60000;
                            StateUpdaterTimer.Elapsed += StateUpdaterTimer_Elapsed;
                        }
                        StateUpdaterTimer.Start();
                    }
                    #endregion
                }

                TimeToComplete = 60000;
            }
        }

        public void StartCompletionTimer()
        {
            if (CompletionTimer == null)
            {
                CompletionTimer = new System.Timers.Timer();                
                CompletionTimer.Elapsed += CompletionTimer_Elapsed;
            }

            CompletionTimer.Interval = TimeToComplete;
            CompletionTimer.Start();
        }

        void CompletionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                EventHandler eh = this.Completed;

                if (eh != null)
                {
                    CompletionTimer.Stop();
                    eh(this, new EventArgs());
                }
            });
        }

        void StateUpdaterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                OnStateUpdaterTimerElapsed();
            });          
        }

        void OnStateUpdaterTimerElapsed()
        {
            if (ElapsedTimeBorder == null) return;

            TimeSpan now = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
            if (now > MinTime)
            {
                if (now > MaxTime)
                {
                    now = MaxTime;
                    TheDayIsOver.Visibility = System.Windows.Visibility.Visible;
                    if (cv.Effect == null)
                    {
                        cv.Effect = new BlurEffect() { Radius = 10 };
                    }
                }
                else
                {
                    TheDayIsOver.Visibility = System.Windows.Visibility.Collapsed;
                    cv.Effect = null;
                }

                double width = (now - MinTime).TotalMinutes * PPM;
                ElapsedTimeBorder.Width = width;

                if (ElapsedTimePointer_Arrow != null && ElapsedTimePointer_Box != null && ElapsedTimePointer_TextBlock != null)
                {
                    ElapsedTimePointer_TextBlock.Text = now.ToString(@"hh\:mm");

                    ElapsedTimePointer_Arrow.SetValue(Canvas.LeftProperty, RowHeaderWidth + width - 7);
                    ElapsedTimePointer_Arrow.SetValue(Canvas.TopProperty, TopMargin + TotalHeight);

                    ElapsedTimePointer_Box.SetValue(Canvas.LeftProperty, RowHeaderWidth + width - 30);
                    ElapsedTimePointer_Box.SetValue(Canvas.TopProperty, TopMargin + TotalHeight + 10);
                }
            }
        }

        void AddEvents()
        { }

        void ShowClassShedules()
        {
            cv.Children.Clear();
            //Step 1: Group by Degree and session
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetUI();
            AddEvents();
        }

        string TimespanToString(TimeSpan ts)
        {
            if(ts.Minutes > 0)
            {
                return ts.ToString(@"hh\:mm");
            }
            else
            {
                if (ts.Hours == 0) return "12 AM";
                else if (ts.Hours == 12) return "12 PM";
                else if (ts.Hours > 12) return string.Format("{0} PM", ts.Hours - 12);
                else return string.Format("{0} AM", ts.Hours);
            }
        }

        bool DoesOverlap(ClassSchedule cs, List<SpecialSchedule> special_schedules)
        {
            bool overlaps = false;

            overlaps = special_schedules.Exists(ss =>
                {
                    return 
                        cs.StartTime < ss.StartTime && cs.EndTime > ss.StartTime ||
                        cs.StartTime > ss.StartTime && cs.EndTime < ss.EndTime ||
                        cs.StartTime < ss.EndTime && cs.EndTime > ss.EndTime;
                });

            return overlaps;
        }
    }
}
