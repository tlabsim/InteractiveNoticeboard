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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TLABS.WPF.Animations;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public object UIContext
        {
            get;
            set;
        }

        System.Timers.Timer NotificationTimer;
        int notification_interval_in_minutes = 5;

        List<string> ShowOrder = new List<string>() { "Intro", "NoticeBoard", "TechNews", "WeatherReport", "ClassSchedules", "Teachers", "FeaturedVideo", "NoticeBoard", "SportsNews", "WeatherReport", "ClassSchedules", "Teachers", "SpecialEventBanners" };
        List<string> NotificationsAllowedOn = new List<string>() { "NoticeBoard", "TechNews", "SportsNews", "Teachers", "FeaturedVideo", "WeatherReport", "SpecialEventBanners" };
        int CurrentlyShowingIndex = 0;

        FrameworkElement LastControl = null;

        public MainWindow()
        {
            Session.Window = this;

            InitializeComponent();
            this.DataContext = this;
            this.UIContext = Settings.UISettings;

            SetControls();
            //SetUI();
            //AddEvents();
        }

        void SetControls()
        {
            NotificationTimer = new System.Timers.Timer();
            NotificationTimer.Interval = GetNextNotificationTime();
            NotificationTimer.Elapsed += NotificationTimer_Elapsed;
            NotificationTimer.Start();
        }

        double GetNextNotificationTime()
        {
            DateTime now = DateTime.Now;
            int mins = (int)Math.Ceiling((now - now.Date).TotalMinutes);
            int rem = mins % notification_interval_in_minutes;
            if (rem > 0)
            {
                mins = mins + notification_interval_in_minutes - rem;
            }
            DateTime next = now.Date.Add(TimeSpan.FromMinutes(mins));
            var interval = (next - now).TotalMilliseconds;
            return interval;
        }

        void NotificationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                    {
                        OnNotificationTimerElapsed();
                    });
            }
            catch { }
        }

        void OnNotificationTimerElapsed()
        {   
            string currently_showing = ShowOrder[CurrentlyShowingIndex];
            if (NotificationsAllowedOn.Contains(currently_showing))
            {
                NotificationOverlay notification_overlay = new NotificationOverlay();
                //notification_overlay.SetValue(Panel.ZIndexProperty, 10000);
                notification_overlay.Completed += notification_overlay_Completed;
                NotificationPanel.Children.Add(notification_overlay);
            }

            NotificationTimer.Stop();
            NotificationTimer.Interval = GetNextNotificationTime();
            NotificationTimer.Stop();
        }

        void notification_overlay_Completed(object sender, EventArgs e)
        {
            NotificationOverlay notification_overlay = sender as NotificationOverlay;
            NotificationPanel.Children.Remove(notification_overlay);
        }

        void SetUI()
        { 
        }

        void StartShow()
        {
            ContainerPanel.Children.Clear();
            if (ShowOrder.Count == 0) return;
            if (CurrentlyShowingIndex >= ShowOrder.Count) CurrentlyShowingIndex = 0;
            string currently_showing = ShowOrder[CurrentlyShowingIndex];

            try
            {
                switch (currently_showing)
                {
                    case "Intro":
                        IntroAnimationControl iac = new IntroAnimationControl();
                        LastControl = iac;
                        iac.Completed += ShowCompleted;
                        iac.Opacity = 0;
                        ContainerPanel.Children.Add(iac);
                        iac.CacheMode = new BitmapCache();
                        FadeInControl(iac);
                        break;

                    case "NoticeBoard":
                        NoticeboardControl nbc = new NoticeboardControl();
                        LastControl = nbc;
                        nbc.Completed += ShowCompleted;
                        //nbc.Opacity = 0;
                        ContainerPanel.Children.Add(nbc);
                        //nbc.FadeIn(500);
                        break;

                    case "TechNews":
                        NewsBoard nb_tech = new NewsBoard(NewsBoard.NewsModes.Technology);
                        LastControl = nb_tech;
                        nb_tech.Completed += ShowCompleted;
                        nb_tech.Opacity = 0;
                        ContainerPanel.Children.Add(nb_tech);
                        FadeInControl(nb_tech);
                        break;

                    case "SportsNews":
                        NewsBoard nb_sports = new NewsBoard(NewsBoard.NewsModes.Sports);
                        LastControl = nb_sports;
                        nb_sports.Completed += ShowCompleted;
                        nb_sports.Opacity = 0;
                        ContainerPanel.Children.Add(nb_sports);
                        FadeInControl(nb_sports);
                        break;

                    case "ClassSchedules":
                        ClassScheduleViewerContainer csvc = new ClassScheduleViewerContainer();
                        LastControl = csvc;
                        csvc.Completed += ShowCompleted;
                        csvc.Opacity = 0;
                        ContainerPanel.Children.Add(csvc);
                        FadeInControl(csvc);
                        break;

                    case "Teachers":
                        TeacherProfileControl tpc = new TeacherProfileControl();
                        LastControl = tpc;
                        tpc.Completed += ShowCompleted;
                        tpc.Opacity = 0;
                        ContainerPanel.Children.Add(tpc);
                        FadeInControl(tpc);
                        break;

                    case "FeaturedVideo":
                        FeaturedVideoViewer fvv = new FeaturedVideoViewer();
                        LastControl = fvv;
                        fvv.Completed += ShowCompleted;
                        fvv.Opacity = 0;
                        ContainerPanel.Children.Add(fvv);
                        FadeInControl(fvv);
                        break;

                    case "SpecialEventBanners":
                        SpecialEventBannerSlideShow sebss = new SpecialEventBannerSlideShow();
                        LastControl = sebss;
                        sebss.Completed += ShowCompleted;
                        sebss.Opacity = 0;
                        ContainerPanel.Children.Add(sebss);
                        FadeInControl(sebss);
                        break;

                    case "WeatherReport":
                        WeatherReport wr = new WeatherReport();
                        LastControl = wr;
                        wr.Completed += ShowCompleted;
                        wr.Opacity = 0;
                        ContainerPanel.Children.Add(wr);
                        FadeInControl(wr);
                        break;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void FadeInControl(FrameworkElement fe)
        {
            this.Dispatcher.Invoke(() =>
            {

                fe.CacheMode = new BitmapCache();
                DoubleAnimation fade_in = new DoubleAnimation(1, TimeSpan.FromMilliseconds(500));
                fade_in.Completed += (o, e) =>
                {
                    if (LastControl != null)
                    {
                        LastControl.CacheMode = null;
                    }
                };
                fe.BeginAnimation(FrameworkElement.OpacityProperty, fade_in);
            });

            this.Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.ContextIdle, null); //Force to render
        }
        
        void ShowCompleted(object sender, EventArgs e)
        {
            if (LastControl != null)
            {
                LastControl.CacheMode = new BitmapCache();
                DoubleAnimation fade_out = new DoubleAnimation(0, TimeSpan.FromMilliseconds(1000));
                fade_out.Completed += fade_out_Completed;
                LastControl.BeginAnimation(FrameworkElement.OpacityProperty, fade_out);
            }
            else
            {
                CurrentlyShowingIndex++;
                StartShow();
            }

            GC.Collect();
        }

        void fade_out_Completed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                System.Threading.Thread.Sleep(100);
                CurrentlyShowingIndex++;
                StartShow();
            });
        }        

        void AddEvents()
        { }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartShow();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if(this.NotificationTimer != null)
            {
                NotificationTimer.Stop();
            }
        }

        private void btnMinimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void btnClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.Close();
                Application.Current.Shutdown();
            }
            catch { }
        }
    }
}
