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

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for EventNotificationItemControl.xaml
    /// </summary>
    public partial class EventNotificationItemControl : UserControl
    {
        System.Timers.Timer EventClock;
        EventSchedule CurrentEventSchedule;

        public EventNotificationItemControl()
        {
            InitializeComponent();
        }

        public void LoadScheduleEvent(EventSchedule event_schedule)
        {
            if (event_schedule == null) return;

            runEventFor.Text = string.Format("{0} {1} session", event_schedule.Degree, event_schedule.Session);

            if(event_schedule.DefinitiveStartTime.Date == event_schedule.DefinitiveEndTime.Date)
            {
                txtEventPeriod.Text = string.Format("{0} {1} - {2}", event_schedule.DefinitiveStartTime.Date.ToShortDateString(), TimespanToString(event_schedule.StartTime), TimespanToString(event_schedule.EndTime));
            }
            else
            {
                txtEventPeriod.Text = string.Format("{0} {1} - {2} {3}", event_schedule.DefinitiveStartTime.Date.ToShortDateString(), TimespanToString(event_schedule.StartTime), event_schedule.DefinitiveEndTime.Date.ToShortDateString(), TimespanToString(event_schedule.EndTime));
            }
            txtEventDuration.Text = TimespanToString2(event_schedule.DefinitiveDuration);

            if (event_schedule.EventState == EventStates.Running)
            {
                runEventStartsIn.Text = string.Format("Started {0} ago...", DateTime.Now - event_schedule.DefinitiveStartTime);
            }
            else if (event_schedule.EventState == EventStates.Elapsed)
            {
                runEventStartsIn.Text = string.Format("Ended {0} ago...", DateTime.Now - event_schedule.DefinitiveEndTime);
            }
            else
            {
                runEventStartsIn.Text = string.Format("In {0}...", TimespanToString2(event_schedule.TimeToStart));
            }

            if (event_schedule is ClassSchedule)
            {
                var class_schedule = (ClassSchedule)event_schedule;

                txtEventType.Text = class_schedule.CourseType == "Lab" ? "Lab" : "Class";
                switch (class_schedule.CourseType)
                {
                    case "Lab":
                        EventTypeBorder.Background = new SolidColorBrush(Color.FromArgb(38, 0, 112, 192));
                        EventTypeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(128, 0, 112, 192));
                        break;
                    case "Other":
                        EventTypeBorder.Background = new SolidColorBrush(Color.FromArgb(38, 146, 208, 80));
                        EventTypeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(128, 146, 208, 80));
                        break;
                    case "Theory":
                    default:
                        EventTypeBorder.Background = new SolidColorBrush(Color.FromArgb(38, 0, 176, 80));
                        EventTypeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(128, 0, 176, 80));
                        break;
                }               

                txtEventTitle.Text = string.Format("{0}: {1}", class_schedule.CourseCode, class_schedule.CourseTitle);
                txtEventDescription.Text = string.IsNullOrEmpty(class_schedule.CourseTeacher) ? "Teacher not specified" : "Conducted by " + class_schedule.CourseTeacher;
                txtOtherDescription.Text = string.IsNullOrEmpty(class_schedule.Venue) ? "Venue not specified" : class_schedule.Venue;
            }
            else if (event_schedule is SpecialSchedule)
            {
                var special_schedule = (SpecialSchedule)event_schedule;

                txtEventType.Text = special_schedule.EventType;
                switch (special_schedule.EventType)
                {
                    case "Examination":
                        EventTypeBorder.Background = new SolidColorBrush(Color.FromArgb(38, 192, 0, 0));
                        EventTypeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(128, 192, 0, 0));
                        break;
                    case "Seminar":
                        EventTypeBorder.Background = new SolidColorBrush(Color.FromArgb(38, 204, 0, 102));
                        EventTypeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(128, 204, 0, 102));
                        break;
                    case "Workshop":
                        EventTypeBorder.Background = new SolidColorBrush(Color.FromArgb(38, 112, 48, 160));
                        EventTypeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(128, 112, 48, 160));
                        break;
                    case "Other":
                    default:
                        EventTypeBorder.Background = new SolidColorBrush(Color.FromArgb(38, 128, 128, 128));
                        EventTypeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128));
                        break;
                }

                txtEventTitle.Text = special_schedule.EventTitle;
                txtEventDescription.Text = special_schedule.EventDescription;
                txtOtherDescription.Text = string.IsNullOrEmpty(special_schedule.Venue) ? "Venue not specified" : special_schedule.Venue;
            }

            CurrentEventSchedule = event_schedule;
            if(EventClock == null)
            {
                EventClock = new System.Timers.Timer();
                EventClock.Interval = 1000;
                EventClock.Elapsed += EventClock_Elapsed;
            }
            EventClock.Start();
        }

        void EventClock_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                    {
                        OnEventClockElapsed();
                    });
            }
            catch { }
        }

        void OnEventClockElapsed()
        {
            if (CurrentEventSchedule == null) return;

            if (CurrentEventSchedule.EventState == EventStates.Running)
            {
                runEventStartsIn.Text = string.Format("Started {0} ago...", TimespanToString2(DateTime.Now - CurrentEventSchedule.DefinitiveStartTime));
            }
            else if (CurrentEventSchedule.EventState == EventStates.Elapsed)
            {
                runEventStartsIn.Text = string.Format("Ended {0} ago...", TimespanToString2(DateTime.Now - CurrentEventSchedule.DefinitiveEndTime));
            }
            else
            {
                runEventStartsIn.Text = string.Format("In {0}...", TimespanToString2(CurrentEventSchedule.TimeToStart));
            }
        }
               
        string TimespanToString(TimeSpan ts)
        {
            int hours = ts.Hours, minutes = ts.Minutes;
            string m = hours >= 12 ? "PM" : "AM";
            hours = hours >= 12 ? hours - 12 : hours;
            hours = hours == 0 ? 12 : hours;

            return string.Format("{0}:{1:00} {2}", hours, minutes, m);
        }

        string TimespanToString2(TimeSpan ts)
        {
            int days = ts.Days, hours = ts.Hours, minutes = ts.Minutes, seconds = ts.Seconds;

            if (days > 0)
            {
                if (hours > 0)
                {
                    return string.Format("{0} day{1} {2} hour{3}", days, days > 1 ? "s" : "", hours, hours > 1 ? "s" : "");
                }
                else
                {
                    return string.Format("{0} day{1}", days, days > 1 ? "s" : "");
                }
            }
            else if (hours >= 1)
            {
                return string.Format("{0}:{1:00} hour{2}", hours, minutes, hours > 1 ? "s" : "");
            }
            else if (minutes >= 1)
            {
                return string.Format("{0}:{1:00} minute{2}", minutes, seconds, minutes > 1 ? "s" : "");
            }
            else
            {
                return string.Format("{0} second{1}", seconds, seconds > 1 ? "s" : "");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if(this.EventClock != null)
            {
                this.EventClock.Stop();                
            }
        }
    }
}
