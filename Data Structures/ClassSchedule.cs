using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TLABS.Extensions;

namespace InteractiveNoticeboard.Data_Structures
{
    public enum EventStates
    {
        Upcoming,
        Running,
        Elapsed
    }

    public class EventSchedule
    {       
        public string Degree { get; set; }
        public string Session { get; set; }
        public string BatchName
        {
            get
            {
                return string.Format("{0} {1} session", Degree, Session);
            }
        }      
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public DateTime DefinitiveStartTime { get; set; }
        public DateTime DefinitiveEndTime { get; set; }
        public TimeSpan DefinitiveDuration
        {
            get
            {
                if (DefinitiveStartTime != null && DefinitiveEndTime != null)
                {
                    return DefinitiveEndTime - DefinitiveStartTime;
                }

                return TimeSpan.FromSeconds(0);
            }
        }
        public TimeSpan TimeToStart
        {
            get
            {
                if (DefinitiveStartTime != null)
                {
                    return DefinitiveStartTime - DateTime.Now;
                }
                return TimeSpan.FromSeconds(int.MaxValue);
            }
        }
        public EventStates EventState
        {
            get
            {
                if (DefinitiveStartTime != null && DefinitiveEndTime != null)
                {
                    DateTime now = DateTime.Now;
                    if (now > DefinitiveEndTime)
                        return EventStates.Elapsed;
                    if (now >= DefinitiveStartTime && now <= DefinitiveEndTime)
                        return EventStates.Running;
                }

                return EventStates.Upcoming;
            }
        }

        public string Venue { get; set; }
        public string Remarks { get; set; }

        public EventSchedule()
        {
        }
    }

    public class ClassSchedule : EventSchedule
    {
        public int DayOfWeek { get; set; }    
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string CourseType { get; set; }
        public string CourseTeacher { get; set; }     

        public ClassSchedule()
        {
        }

        public static List<ClassSchedule> GetAllClassSchedules(int day_of_week = -1)
        {
            List<ClassSchedule> schedules = new List<ClassSchedule>();

            string query = string.Empty;
            if(day_of_week > 0)
            {
                query = string.Format("SELECT * FROM class_schedules WHERE day_of_week = '{0}'", day_of_week);
            }
            else
            {
                query = "SELECT * FROM class_schedules ORDER BY day_of_week ASC";
            }

            try
            {
                int todays_day_of_week = DateTime.Now.DayOfWeek.GetHashCode() + 1;
                using (DataTable dt = DB_Manager.DBClient.ExecuteAdapter(query))
                {
                    if (dt != null)
                    {
                        for (int r = 0; r < dt.Rows.Count; r++)
                        {
                            try
                            {
                                ClassSchedule cs = new ClassSchedule();                                
                                cs.DayOfWeek = dt.Rows[r]["day_of_week"].GetString().ToInt();
                                cs.Degree = dt.Rows[r]["degree"].GetString().Trim();
                                cs.Session = dt.Rows[r]["session"].GetString().Trim();
                                cs.CourseCode = dt.Rows[r]["course_code"].GetString().Trim();
                                cs.CourseTitle = dt.Rows[r]["course_title"].GetString().Trim();
                                cs.CourseType = dt.Rows[r]["course_type"].GetString();
                                cs.CourseTeacher = dt.Rows[r]["course_teacher"].GetString().Trim();
                                cs.StartTime = TimeSpan.Parse(dt.Rows[r]["start_time"].GetString());
                                cs.EndTime = TimeSpan.Parse(dt.Rows[r]["end_time"].GetString());
                                cs.Venue = dt.Rows[r]["venue"].GetString().Trim();
                                cs.Remarks = dt.Rows[r]["remarks"].GetString();

                                int days_to_go = 0;                                
                                days_to_go = todays_day_of_week  - cs.DayOfWeek;
                                if (days_to_go < 0) days_to_go += 7;
                                cs.DefinitiveStartTime = DateTime.Now.Date.AddDays(days_to_go).Add(cs.StartTime);
                                cs.DefinitiveEndTime = DateTime.Now.Date.AddDays(days_to_go).Add(cs.EndTime);

                                schedules.Add(cs);
                                Console.Write(cs);
                            }
                            catch (Exception ex) 
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return schedules;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Day of week   : {0}", this.DayOfWeek));
            sb.AppendLine(string.Format("Degree        : {0}", this.Degree));
            sb.AppendLine(string.Format("Session       : {0}", this.Session));
            sb.AppendLine(string.Format("Course code   : {0}", this.CourseCode));
            sb.AppendLine(string.Format("Course title  : {0}", this.CourseTitle));
            sb.AppendLine(string.Format("Course teacher: {0}", this.CourseTeacher));
            sb.AppendLine(string.Format("Start time    : {0}", this.StartTime));
            sb.AppendLine(string.Format("End time      : {0}", this.EndTime));
            sb.AppendLine(string.Format("Venue         : {0}", this.Venue));
            sb.AppendLine(string.Format("Remarks       : {0}", this.Remarks));

            return sb.ToString();
        }
    }

    public class SpecialSchedule: EventSchedule
    {
        public DateTime EventDate { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
        public string EventType { get; set; }

        public static List<SpecialSchedule> GetSpecialSchedules(DateTime date, int for_days = 1)
        {
            List<SpecialSchedule> schedules = new List<SpecialSchedule>();

            string query = string.Empty;
            if (for_days > 1)
            {
                DateTime to_date = date.AddDays(for_days - 1);
                query = string.Format("SELECT * FROM special_schedules WHERE event_date >= #{0}/{1:00}/{2:00}# AND event_date <= #{3}/{4:00}/{5:00}#", date.Year, date.Month, date.Day, to_date.Year, to_date.Month, to_date.Day);
            }
            else
            {
                query = string.Format("SELECT * FROM special_schedules WHERE event_date = #{0}/{1:00}/{2:00}#", date.Year, date.Month, date.Day);
            }

            try
            {
                using (DataTable dt = DB_Manager.DBClient.ExecuteAdapter(query))
                {
                    if (dt != null)
                    {
                        for (int r = 0; r < dt.Rows.Count; r++)
                        {
                            try
                            {
                                SpecialSchedule sc = new SpecialSchedule();
                                sc.EventDate = DateTime.Parse(dt.Rows[r]["event_date"].GetString()).Date;

                                sc.Degree = dt.Rows[r]["degree"].GetString().Trim();
                                sc.Session = dt.Rows[r]["session"].GetString().Trim();
                                sc.EventTitle = dt.Rows[r]["event_title"].GetString().Trim();
                                sc.EventDescription = dt.Rows[r]["event_description"].GetString().Trim();
                                sc.EventType = dt.Rows[r]["event_type"].GetString().Trim(); 
                                sc.StartTime = TimeSpan.Parse(dt.Rows[r]["start_time"].GetString());
                                sc.StartTime = new TimeSpan(sc.StartTime.Hours, sc.StartTime.Minutes, 0);
                                sc.EndTime = TimeSpan.Parse(dt.Rows[r]["end_time"].GetString());
                                sc.EndTime = new TimeSpan(sc.EndTime.Hours, sc.EndTime.Minutes, 0);
                                sc.DefinitiveStartTime = sc.EventDate.Add(sc.StartTime);
                                sc.DefinitiveEndTime = sc.EventDate.Add(sc.EndTime);                                
                                sc.Venue = dt.Rows[r]["venue"].GetString().Trim();
                                sc.Remarks = dt.Rows[r]["remarks"].GetString();

                                schedules.Add(sc);
                                Console.Write(sc);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return schedules;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Date   : {0}", this.EventDate.Date));
            sb.AppendLine(string.Format("Degree        : {0}", this.Degree));
            sb.AppendLine(string.Format("Session       : {0}", this.Session));
            sb.AppendLine(string.Format("Event title   : {0}", this.EventTitle));
            sb.AppendLine(string.Format("Description   : {0}", this.EventDescription));
            sb.AppendLine(string.Format("Start time    : {0}", this.StartTime));
            sb.AppendLine(string.Format("End time      : {0}", this.EndTime));
            sb.AppendLine(string.Format("Venue         : {0}", this.Venue));
            sb.AppendLine(string.Format("Remarks       : {0}", this.Remarks));

            return sb.ToString();
        }
    }
}
