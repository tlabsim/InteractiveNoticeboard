using System;
using System.Collections.Generic;
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
using InteractiveNoticeboard.Data_Structures;
using TLABS.Extensions;
using System.Windows.Media.Animation;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for NotificationOverlay.xaml
    /// </summary>
    public partial class NotificationOverlay : UserControl
    {
        List<EventSchedule> EventSchedules = new List<EventSchedule>();
        Storyboard NotificationStoryboard;
        QuadraticEase QEaseOut = new QuadraticEase() { EasingMode = EasingMode.EaseOut };
        public event EventHandler Completed;

        public NotificationOverlay()
        {
            InitializeComponent();
        }

        void SetControls()
        { }

        void SetUI()
        { }

        void AddEvents()
        { }

        void ShowUpcomingEventNotifications()
        {
            EventSchedules.Clear();
            stackUpcomingEvents.Children.Clear();

            var now = DateTime.Now;
            int today = now.DayOfWeek.GetHashCode() + 1;
            var upcoming_classes = ClassSchedule.GetAllClassSchedules(today);
            upcoming_classes = upcoming_classes.FindAll(s => s.DefinitiveEndTime > DateTime.Now);
            
            if(upcoming_classes.Count > 5)
            {
                var starting_in_two_hours = upcoming_classes.FindAll(s => s.DefinitiveStartTime > now && (s.DefinitiveStartTime - now).TotalMinutes <= 120);
                if(starting_in_two_hours.Count >= 5)
                {
                    EventSchedules.AddRange(starting_in_two_hours.OrderBy(s => s.DefinitiveStartTime).ToList().GetRange(0, 5));
                }
                else
                {
                    var running_classes = upcoming_classes.FindAll(s => s.EventState == EventStates.Running);
                    if (starting_in_two_hours.Count + running_classes.Count > 5)
                    {
                        //Include only events that is ending shortly
                        int running_events_addable = 5 - starting_in_two_hours.Count;

                        EventSchedules.AddRange(running_classes.OrderBy(s => s.DefinitiveEndTime).ToList().GetRange(running_classes.Count - running_events_addable, running_events_addable));
                        EventSchedules.AddRange(starting_in_two_hours.OrderBy(s => s.DefinitiveStartTime).ToList().GetRange(0, 5));
                    }
                    else
                    {
                        EventSchedules.AddRange(upcoming_classes.OrderBy(s => s.DefinitiveStartTime).ToList().GetRange(0, 5));
                    }
                }
            }

            EventSchedules.AddRange(upcoming_classes.OrderBy(s => s.DefinitiveStartTime).ToList());

            if (upcoming_classes.Count < 5)
            {
                int max_special_events_to_add = 5 - upcoming_classes.Count;
                var upcoming_special_events = SpecialSchedule.GetSpecialSchedules(DateTime.Now, 7);

                if(upcoming_special_events.Count > max_special_events_to_add)
                {
                    EventSchedules.AddRange(upcoming_special_events.OrderBy(s => s.DefinitiveStartTime).ToList().GetRange(0, max_special_events_to_add));
                }
                else
                {
                    EventSchedules.AddRange(upcoming_special_events.OrderBy(s => s.DefinitiveStartTime).ToList());
                }
            }


            int total_events = EventSchedules.Count;

            if (total_events > 0)
            {
                NotificationStoryboard = new Storyboard();
                int duration = total_events <= 3 ? 30 : 40 + (total_events - 3) * 5;
                NotificationStoryboard.Duration = TimeSpan.FromSeconds(duration);

                {
                    DoubleAnimation reveal = new DoubleAnimation(520, 0, TimeSpan.FromMilliseconds(500)) { EasingFunction = QEaseOut };
                    reveal.BeginTime = TimeSpan.FromSeconds(1);
                    Storyboard.SetTarget(reveal, NotificationPanel);
                    Storyboard.SetTargetProperty(reveal, new PropertyPath("(Border.RenderTransform).(TranslateTransform.X)"));
                    NotificationStoryboard.Children.Add(reveal);
                }
                {
                    DoubleAnimation hide = new DoubleAnimation(520, TimeSpan.FromMilliseconds(500)) { EasingFunction = QEaseOut };
                    hide.BeginTime = TimeSpan.FromSeconds(duration - 1);
                    Storyboard.SetTarget(hide, NotificationPanel);
                    Storyboard.SetTargetProperty(hide, new PropertyPath("(Border.RenderTransform).(TranslateTransform.X)"));
                    NotificationStoryboard.Children.Add(hide);
                }

                for (int i = 0; i < total_events; i++)
                {
                    var es = EventSchedules[i];
                    EventNotificationItemControl enic = new EventNotificationItemControl();
                    enic.Margin = new Thickness(0, 0, 0, 5);
                    stackUpcomingEvents.Children.Add(enic);
                    enic.LoadScheduleEvent(es);

                    if (total_events > 3)
                    {
                        switch (i)
                        {
                            case 0:
                                enic.LayoutTransform = new ScaleTransform(1, 1);
                                {
                                    DoubleAnimation fade_out = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) { EasingFunction = QEaseOut };
                                    fade_out.BeginTime = new TimeSpan(0, 0, 0, 25, 0);
                                    Storyboard.SetTarget(fade_out, enic);
                                    Storyboard.SetTargetProperty(fade_out, new PropertyPath("Opacity"));
                                    NotificationStoryboard.Children.Add(fade_out);
                                }
                                {
                                    DoubleAnimation collapse = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) { EasingFunction = QEaseOut };
                                    collapse.BeginTime = new TimeSpan(0, 0, 0, 25, 250);
                                    Storyboard.SetTarget(collapse, enic);
                                    Storyboard.SetTargetProperty(collapse, new PropertyPath("(FrameworkElement.LayoutTransform).(ScaleTransform.ScaleY)"));
                                    NotificationStoryboard.Children.Add(collapse);
                                }                                
                                break;

                            case 2:
                                if (total_events > 4)
                                {
                                    enic.LayoutTransform = new ScaleTransform(1, 1);
                                    {
                                        DoubleAnimation fade_out = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) { EasingFunction = QEaseOut };
                                        fade_out.BeginTime = new TimeSpan(0, 0, 0, 30, 0);
                                        Storyboard.SetTarget(fade_out, enic);
                                        Storyboard.SetTargetProperty(fade_out, new PropertyPath("Opacity"));
                                        NotificationStoryboard.Children.Add(fade_out);
                                    }
                                    {
                                        DoubleAnimation collapse = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) { EasingFunction = QEaseOut };
                                        collapse.BeginTime = new TimeSpan(0, 0, 0, 30, 250);
                                        Storyboard.SetTarget(collapse, enic);
                                        Storyboard.SetTargetProperty(collapse, new PropertyPath("(FrameworkElement.LayoutTransform).(ScaleTransform.ScaleY)"));
                                        NotificationStoryboard.Children.Add(collapse);
                                    }       
                                }
                                break;

                            case 3:
                                enic.LayoutTransform = new ScaleTransform(1, 0);
                                enic.Opacity = 0;
                                {
                                    DoubleAnimation expand = new DoubleAnimation(1, TimeSpan.FromMilliseconds(250)) { EasingFunction = QEaseOut };
                                    expand.BeginTime = new TimeSpan(0, 0, 0, 25, 250);
                                    Storyboard.SetTarget(expand, enic);
                                    Storyboard.SetTargetProperty(expand, new PropertyPath("(FrameworkElement.LayoutTransform).(ScaleTransform.ScaleY)"));
                                    NotificationStoryboard.Children.Add(expand);
                                }       
                                {
                                    DoubleAnimation fade_in = new DoubleAnimation(1, TimeSpan.FromMilliseconds(250)) { EasingFunction = QEaseOut };
                                    fade_in.BeginTime = new TimeSpan(0, 0, 0, 25, 500);
                                    Storyboard.SetTarget(fade_in, enic);
                                    Storyboard.SetTargetProperty(fade_in, new PropertyPath("Opacity"));
                                    NotificationStoryboard.Children.Add(fade_in);
                                }                                
                                break;

                            case 4:
                                enic.LayoutTransform = new ScaleTransform(1, 0);
                                enic.Opacity = 0;
                                {
                                    DoubleAnimation expand = new DoubleAnimation(1, TimeSpan.FromMilliseconds(250)) { EasingFunction = QEaseOut };
                                    expand.BeginTime = new TimeSpan(0, 0, 0, 30, 250);
                                    Storyboard.SetTarget(expand, enic);
                                    Storyboard.SetTargetProperty(expand, new PropertyPath("(FrameworkElement.LayoutTransform).(ScaleTransform.ScaleY)"));
                                    NotificationStoryboard.Children.Add(expand);
                                }       
                                {
                                    DoubleAnimation fade_in = new DoubleAnimation(1, TimeSpan.FromMilliseconds(250)) { EasingFunction = QEaseOut };
                                    fade_in.BeginTime = new TimeSpan(0, 0, 0, 30, 500);
                                    Storyboard.SetTarget(fade_in, enic);
                                    Storyboard.SetTargetProperty(fade_in, new PropertyPath("Opacity"));
                                    NotificationStoryboard.Children.Add(fade_in);
                                }
                                break;
                        }
                    }
                }

                NotificationStoryboard.Completed += NotificationStoryboard_Completed;
                NotificationStoryboard.Begin();
            }
            else
            {
                OnComplete();
            }
        }

        void NotificationStoryboard_Completed(object sender, EventArgs e)
        {
            OnComplete();
        }

        void OnComplete()
        {
            EventHandler eh = this.Completed;
            if(eh != null)
            {
                eh(this, new EventArgs());
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowUpcomingEventNotifications();
        }
    }
}
