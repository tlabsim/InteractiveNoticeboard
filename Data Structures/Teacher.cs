using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TLABS.Extensions;
using TLABS.WPF.Extensions;

namespace InteractiveNoticeboard.Data_Structures
{  
    public class Teacher
    {
        static string PhotoFolderPath
        {
            get
            {
                return Settings.AppPath.TrimEnd('\\') + @"\Data\TeacherPhotos\";
            }
        }


        public int TeacherID { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }        
        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(Title) ? Name : string.Format("{0} {1}", Title, Name);
            }
        }
        public string Designation { get; set; }        
        public Gender Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ResearchInterest { get; set; }
        public string About { get; set; }
        public string PhotoPath { get; set; }
        public ImageSource Photo { get; set; }

        public Teacher()
        {

        }

        public static List<Teacher> GetAllTeachers()
        {
            List<Teacher> teachers = new List<Teacher>();

            string query = "SELECT * FROM teachers ORDER BY rank ASC";

            using (DataTable dt = DB_Manager.DBClient.ExecuteAdapter(query))
            {
                if (dt != null)
                {
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        Teacher teacher = new Teacher();

                        teacher.TeacherID = dt.Rows[r]["ID"].GetString().ToInt();
                        teacher.Name = dt.Rows[r]["teacher_name"].GetString();
                        teacher.Title = dt.Rows[r]["title"].GetString();
                        teacher.Designation = dt.Rows[r]["designation"].GetString();
                        try
                        {
                            teacher.Gender = (Gender)Enum.Parse(typeof(Gender), dt.Rows[r]["gender"].GetString(), true);
                        }
                        catch 
                        {
                            teacher.Gender = Gender.Male;
                        }
                        teacher.Email = dt.Rows[r]["email"].GetString();
                        teacher.Phone = dt.Rows[r]["phone"].GetString();
                        teacher.ResearchInterest = dt.Rows[r]["research_interests"].GetString();
                        teacher.About = dt.Rows[r]["about"].GetString();                       
                        teacher.PhotoPath = dt.Rows[r]["photo"].GetString();

                        if (!string.IsNullOrEmpty(teacher.PhotoPath))
                        {
                            //Load Photo
                            string full_photo_path = PhotoFolderPath + teacher.PhotoPath;

                            System.IO.FileInfo fi = new System.IO.FileInfo(full_photo_path);
                            string ext = fi.Extension.ToLower();
                            if (fi.Exists && (ext == ".jpg" || ext == ".png"))
                            {
                                try
                                {
                                    BitmapImage img = new BitmapImage();
                                    img.LoadFromFile(full_photo_path);
                                    teacher.Photo = img;
                                }
                                catch { }
                            }
                        }
                        if(teacher.Photo == null)
                        {
                            if(teacher.Gender == Gender.Male)
                            {
                                teacher.Photo = new BitmapImage(new Uri("pack://application:,,,/InteractiveNoticeboard;component/Resources/male_silhouette.jpg"));
                            }
                            else
                            {
                                teacher.Photo = new BitmapImage(new Uri("pack://application:,,,/InteractiveNoticeboard;component/Resources/female_silhouette.png"));
                            }
                        }

                        teachers.Add(teacher);

                        Console.WriteLine("{0}: {1}", teacher.TeacherID, teacher.FullName);
                    }
                }
            }

            return teachers;
        }

        public static int GetTeacherCount()
        {
            string query = "SELECT COUNT(*) AS teacher_count FROM teachers";
            return DB_Manager.DBClient.ExecuteScalar(query).GetString().ToInt(0);
        }
    }
}
