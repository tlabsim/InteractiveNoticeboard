using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TLABS.Extensions;
using TLABS.WPF.Extensions;

namespace InteractiveNoticeboard.Data_Structures
{
    public class Student
    {
        public static string PhotoFolderPath
        {
            get
            {
                return Settings.AppPath.TrimEnd('\\') + @"\Data\StudentPhotos\";
            }
        }

        public int DBIndex { get; set; }
        public string StudentID { get; set; }
        public string Name { get; set; }
        public string AKA { get; set; }
        public string Degree { get; set; }
        public string Session { get; set; }
        public int ClassRoll { get; set; }
        public Gender Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public DateTime? CertificateBirthdate { get; set; }
        public DateTime? ActualBirthDate { get; set; }
        public string Bloodgroup { get; set; }
        public double CGPA { get; set; }
        public string PhotoPath { get; set; }
        public ImageSource Photo { get; set; }

        public Student()
        {
        }

        public static List<Student> GetStudents(string query)
        {
            List<Student> students = new List<Student>();

            using (DataTable dt = DB_Manager.DBClient.ExecuteAdapter(query))
            {
                if (dt != null)
                {
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        Student student = new Student();

                        student.DBIndex = dt.Rows[r]["ID"].GetString().ToInt();
                        student.StudentID = dt.Rows[r]["student_id"].GetString();
                        student.Name = dt.Rows[r]["student_name"].GetString();
                        student.AKA = dt.Rows[r]["aka"].GetString();
                        student.Degree = dt.Rows[r]["degree"].GetString();
                        student.Session = dt.Rows[r]["session"].GetString();
                        student.ClassRoll = dt.Rows[r]["class_roll"].GetString().ToInt(-1);
                        try
                        {
                            student.Gender = (Gender)Enum.Parse(typeof(Gender), dt.Rows[r]["gender"].GetString(), true);
                        }
                        catch
                        {
                            student.Gender = Gender.Male;
                        }
                        student.FatherName = dt.Rows[r]["father_name"].GetString();
                        student.MotherName = dt.Rows[r]["mother_name"].GetString();

                        student.CertificateBirthdate = dt.Rows[r]["certificate_birthday"].GetString().ToDateTime();
                        student.ActualBirthDate = dt.Rows[r]["actual_birthday"].GetString().ToDateTime();

                        try
                        {
                            student.Gender = (Gender)Enum.Parse(typeof(Gender), dt.Rows[r]["gender"].GetString(), true);
                        }
                        catch
                        {
                            student.Gender = Gender.Male;
                        }

                        student.Email = dt.Rows[r]["email"].GetString();
                        student.Phone = dt.Rows[r]["phone"].GetString();

                        student.PhotoPath = dt.Rows[r]["photo"].GetString();
                        if (string.IsNullOrEmpty(student.PhotoPath))
                        {
                            student.PhotoPath = student.StudentID.ToLower() + ".jpg";
                        }
                        if (!string.IsNullOrEmpty(student.PhotoPath))
                        {
                            //Load Photo
                            string full_photo_path = PhotoFolderPath + student.PhotoPath;

                            System.IO.FileInfo fi = new System.IO.FileInfo(full_photo_path);
                            string ext = fi.Extension.ToLower();
                            if (fi.Exists && (ext == ".jpg" || ext == ".png"))
                            {
                                try
                                {
                                    BitmapImage img = new BitmapImage();
                                    img.LoadFromFile(full_photo_path);
                                    student.Photo = img;
                                }
                                catch { }
                            }
                        }
                        if (student.Photo == null)
                        {
                            if (student.Gender == Gender.Male)
                            {
                                student.Photo = new BitmapImage(new Uri("pack://application:,,,/InteractiveNoticeboard;component/Resources/male_silhouette.jpg"));
                            }
                            else
                            {
                                student.Photo = new BitmapImage(new Uri("pack://application:,,,/InteractiveNoticeboard;component/Resources/female_silhouette.png"));
                            }
                        }

                        student.CGPA = dt.Rows[r]["cgpa"].GetString().ToDouble(0.0);
                        students.Add(student);

                        Console.WriteLine("{0}: {1}", student.StudentID, student.Name);
                    }
                }
            }

            return students;
        }

        public static List<Student> GetAllStudents()
        {
            string query = "SELECT * FROM students";
            return GetStudents(query);
        }

        public static List<Student> GetBirthdayBoysAndGirls()
        {
            DateTime now = DateTime.Now;
            string query = string.Format("SELECT * FROM students WHERE MONTH(actual_birthday) = {0} AND DAY(actual_birthday) = {1}", now.Month, now.Day);
            return GetStudents(query);
        }

        public static int GetStudentCount()
        {
            string query = "SELECT COUNT(*) AS student_count FROM students";
            return DB_Manager.DBClient.ExecuteScalar(query).GetString().ToInt(0);
        }

        public static Student GetStudentInfoFromStudentID(string student_id)
        {
            string query = string.Format("SELECT * FROM students WHERE student_id LIKE '{0}'", student_id);
            return GetStudentInfo(query);
        }

        public static Student GetStudentInfoFromDBIndex(int id)
        {
            string query = string.Format("SELECT * FROM students WHERE ID = {0}", id);
            return GetStudentInfo(query);
        }

        public static Student GetStudentInfo(string query)
        {
            Student student = null;

            using (DataTable dt = DB_Manager.DBClient.ExecuteAdapter(query))
            {
                if (dt != null && dt.Rows.Count >= 1)
                {
                    student = new Student();

                    student.DBIndex = dt.Rows[0]["ID"].GetString().ToInt();
                    student.StudentID = dt.Rows[0]["student_id"].GetString();
                    student.Name = dt.Rows[0]["student_name"].GetString();
                    student.AKA = dt.Rows[0]["aka"].GetString();
                    student.Degree = dt.Rows[0]["degree"].GetString();
                    student.Session = dt.Rows[0]["session"].GetString();
                    student.ClassRoll = dt.Rows[0]["class_roll"].GetString().ToInt(-1);
                    try
                    {
                        student.Gender = (Gender)Enum.Parse(typeof(Gender), dt.Rows[0]["gender"].GetString(), true);
                    }
                    catch
                    {
                        student.Gender = Gender.Male;
                    }
                    student.FatherName = dt.Rows[0]["father_name"].GetString();
                    student.MotherName = dt.Rows[0]["mother_name"].GetString();

                    student.CertificateBirthdate = dt.Rows[0]["certificate_birthday"].GetString().ToDateTime();
                    student.ActualBirthDate = dt.Rows[0]["actual_birthday"].GetString().ToDateTime();

                    try
                    {
                        student.Gender = (Gender)Enum.Parse(typeof(Gender), dt.Rows[0]["gender"].GetString(), true);
                    }
                    catch
                    {
                        student.Gender = Gender.Male;
                    }

                    student.Email = dt.Rows[0]["email"].GetString();
                    student.Phone = dt.Rows[0]["phone"].GetString();

                    student.PhotoPath = dt.Rows[0]["photo"].GetString();
                    if (string.IsNullOrEmpty(student.PhotoPath))
                    {
                        student.PhotoPath = student.StudentID.ToLower() + ".jpg";
                    }
                    if (!string.IsNullOrEmpty(student.PhotoPath))
                    {
                        //Load Photo
                        string full_photo_path = PhotoFolderPath + student.PhotoPath;

                        System.IO.FileInfo fi = new System.IO.FileInfo(full_photo_path);
                        string ext = fi.Extension.ToLower();
                        if (fi.Exists && (ext == ".jpg" || ext == ".png"))
                        {
                            try
                            {
                                BitmapImage img = new BitmapImage();
                                img.LoadFromFile(full_photo_path);
                                student.Photo = img;
                            }
                            catch { }
                        }
                    }
                    if (student.Photo == null)
                    {
                        if (student.Gender == Gender.Male)
                        {
                            student.Photo = new BitmapImage(new Uri("pack://application:,,,/InteractiveNoticeboard;component/Resources/male_silhouette.jpg"));
                        }
                        else
                        {
                            student.Photo = new BitmapImage(new Uri("pack://application:,,,/InteractiveNoticeboard;component/Resources/female_silhouette.png"));
                        }
                    }

                    student.CGPA = dt.Rows[0]["cgpa"].GetString().ToDouble(0.0);
                    Console.WriteLine("{0}: {1}", student.StudentID, student.Name);
                }
            }

            return student;
        }

    }
}
