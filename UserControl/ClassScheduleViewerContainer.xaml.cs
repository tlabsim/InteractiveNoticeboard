using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for ClassScheduleViewerContainer.xaml
    /// </summary>   
    
    public partial class ClassScheduleViewerContainer : UserControl
    {       
        QuadraticEase QEaseOut = new QuadraticEase() { EasingMode = EasingMode.EaseOut };

        ClassScheduleViewer TodaysScheduleViewer, NextDaysScheduleViewer;
        ClassScheduleViewer.ShowModes CurrentShowing = ClassScheduleViewer.ShowModes.Today;

        bool Repeat = false;
        public event EventHandler Completed;

        public ClassScheduleViewerContainer()
        {
            InitializeComponent();

            SetControls();
        }

        void SetControls()
        {
            TodaysScheduleViewer = new ClassScheduleViewer(ClassScheduleViewer.ShowModes.Today);
            NextDaysScheduleViewer = new ClassScheduleViewer(ClassScheduleViewer.ShowModes.NextDay);

            NextDaysScheduleViewer.SetValue(Panel.ZIndexProperty, 100);
            NextDaysScheduleViewer.RenderTransform = new TranslateTransform() { X = 3840 };

            ScheduleViewerContainerPanel.Children.Add(TodaysScheduleViewer);
            ScheduleViewerContainerPanel.Children.Add(NextDaysScheduleViewer);
        }

        void SetUI()
        {
            TodaysScheduleViewer.Completed += TodaysScheduleViewer_Completed;
            NextDaysScheduleViewer.Completed += NextDaysScheduleViewer_Completed;

            CurrentShowing = ClassScheduleViewer.ShowModes.Today;
            TodaysScheduleViewer.Loaded += (o, e) =>
            {
                TodaysScheduleViewer.StartCompletionTimer();
            };
        }
        
        void TodaysScheduleViewer_Completed(object sender, EventArgs e)
        {
            TodaysScheduleViewer.SetValue(Panel.ZIndexProperty, 0);
            NextDaysScheduleViewer.SetValue(Panel.ZIndexProperty, 100);

            double width = ScheduleViewerContainerPanel.ActualWidth;

            NextDaysScheduleViewer.CacheMode = new BitmapCache();

            Storyboard sb = new Storyboard();
            {
                DoubleAnimation slide_left = new DoubleAnimation(width, 0, TimeSpan.FromMilliseconds(500)) { EasingFunction = QEaseOut };
                Storyboard.SetTarget(slide_left, NextDaysScheduleViewer);
                Storyboard.SetTargetProperty(slide_left, new PropertyPath("(FrameworkElement.RenderTransform).(TranslateTransform.X)"));
                sb.Children.Add(slide_left);
            }
            sb.Completed += (_o, _e) => 
            { 
                NextDaysScheduleViewer.CacheMode = null;
                NextDaysScheduleViewer.StartCompletionTimer();
            };
            sb.Begin();

            CurrentShowing = ClassScheduleViewer.ShowModes.NextDay;
        }

        void NextDaysScheduleViewer_Completed(object sender, EventArgs e)
        {
            EventHandler eh = this.Completed;
            if (eh != null)
            {
                eh(this, new EventArgs());
            }
        }

        void AddEvents()
        { }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetUI();
            AddEvents();
        }
    }
}
