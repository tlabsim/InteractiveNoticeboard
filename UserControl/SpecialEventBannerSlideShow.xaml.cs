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
using TLABS.WPF.Animations;
using TLABS.WPF.Extensions;
using System.IO;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for SpecialEventBannerSlideShow.xaml
    /// </summary>
    public partial class SpecialEventBannerSlideShow : UserControl
    {
        string SpecialEventBannersLocation = string.Empty;

        System.Timers.Timer SlideshowTimer;

        Image Next;
        List<BitmapImage> SlideshowImages;
        int CurrentShowingImageIndex = 0;

        public event EventHandler Completed;

        public SpecialEventBannerSlideShow()
        {
            InitializeComponent();

            SetControls();
        }

        void SetControls()
        {
            SlideshowImages = new List<BitmapImage>();

            SpecialEventBannersLocation = Settings.AppPath.TrimEnd('\\') + @"\SpecialEventBanners\";
            DirectoryInfo DI = new DirectoryInfo(SpecialEventBannersLocation);
            if (!DI.Exists)
            {
                DI.Create();
            }

            if(SlideshowTimer == null)
            {
                SlideshowTimer = new System.Timers.Timer();
                SlideshowTimer.Interval = 10000;
                SlideshowTimer.Elapsed += SlideshowTimer_Elapsed;
            }

            Next = Image1;
        }

        void SlideshowTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
                {
                    OnSlideshowTimerElapsed();
                });
        }

        void OnSlideshowTimerElapsed()
        {
            if (CurrentShowingImageIndex >= SlideshowImages.Count)
            {
                EventHandler eh = this.Completed;
                if (eh != null)
                {
                    if (SlideshowTimer != null) SlideshowTimer.Stop();

                    eh(this, new EventArgs());
                    return;
                }
                else
                {
                    CurrentShowingImageIndex = 0;
                }
            }

            ShowCurrentImage();
        }

        void OnComplete()
        {
            EventHandler eh = this.Completed;
            if (eh != null)
            {
                eh(this, new EventArgs());
            }
        }

        void ShowCurrentImage()
        {
            if (SlideshowImages.Count == 0 || CurrentShowingImageIndex >= SlideshowImages.Count) return;

            var img = SlideshowImages[CurrentShowingImageIndex];

            Next.Source = img;

            if (Next == Image1)
            {
                Image1.FadeIn(500);
                Image2.FadeOut(500);

                Next = Image2;
            }
            else
            {
                Image2.FadeIn(500);
                Image1.FadeOut(500);

                Next = Image1;
            }

            CurrentShowingImageIndex++;
        }

        void LoadSlideshowImages()
        {
            LoadBirthdayImages();
            LoadOtherImages();
        }

        void LoadBirthdayImages()
        {
            DateTime now = DateTime.Now;

            string birthday_greetings_folder = SpecialEventBannersLocation.TrimEnd('\\') + @"\BirthdayGreetings\";
            DirectoryInfo bgfi = new DirectoryInfo(birthday_greetings_folder);
            if (!bgfi.Exists)
            {
                bgfi.Create();
            }

            var folders = bgfi.GetDirectories();
            for (int i = 0; i < folders.Length; i++)
            {
                var f = folders[i];

                DateTime dt;
                if (DateTime.TryParse(f.Name, out dt))
                {
                    if (dt.Date == now.Date)
                    {
                        var subfolders = f.GetDirectories();

                        for (int j = 0; j < subfolders.Length; j++)
                        {
                            var sf = subfolders[j];

                            var std = Student.GetStudentInfoFromStudentID(sf.Name);
                            if (std != null)
                            {
                                //Get photos
                                var photos = sf.EnumerateFiles()
                                            .Where(file => file.Name.ToLower().EndsWith("jpg") || file.Name.ToLower().EndsWith("png"))
                                            .ToList();

                                foreach (var photo in photos)
                                {
                                    try
                                    {
                                        BitmapImage img = new BitmapImage();
                                        img.LoadFromFile(photo.FullName);
                                        SlideshowImages.Add(img);
                                    }
                                    catch { }
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        void LoadOtherImages()
        {
            DateTime now = DateTime.Now;

            //first get todays images
            DirectoryInfo sebl = new DirectoryInfo(SpecialEventBannersLocation);
            if (!sebl.Exists)
            {
                return;
            }

            var folders = sebl.GetDirectories();
            for (int i = 0; i < folders.Length; i++)
            {
                var f = folders[i];

                DateTime dt;
                if (DateTime.TryParse(f.Name, out dt))
                {
                    if (dt.Date == now.Date)
                    {
                        //Get today's photos
                        var photos_for_today =
                            f.EnumerateFiles()
                            .Where(file => file.Name.ToLower().EndsWith("jpg") || file.Name.ToLower().EndsWith("png"))
                            .ToList();

                        foreach (var photo in photos_for_today)
                        {
                            try
                            {
                                BitmapImage img = new BitmapImage();
                                img.LoadFromFile(photo.FullName);
                                SlideshowImages.Add(img);
                            }
                            catch { }
                        }
                        break;
                    }
                }
            }

            //Get other photos
            var other_photos =
                sebl.EnumerateFiles()
                .Where(file => file.Name.ToLower().EndsWith("jpg") || file.Name.ToLower().EndsWith("png"))
                .ToList();

            foreach (var photo in other_photos)
            {
                try
                {
                    BitmapImage img = new BitmapImage();
                    img.LoadFromFile(photo.FullName);
                    SlideshowImages.Add(img);
                }
                catch { }
            }
        }

        bool IsDateString(string str)
        {
            DateTime dt;
            return DateTime.TryParse(str, out dt);
        }
     
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSlideshowImages();

            if(SlideshowImages.Count == 0)
            {
                if (SlideshowTimer != null) SlideshowTimer.Stop();

                OnComplete();
                return;
            }

            ShowCurrentImage();
            SlideshowTimer.Start();
        }

        
    }
}
