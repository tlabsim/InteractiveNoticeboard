using InteractiveNoticeboard.Data_Structures;
using InteractiveNoticeboard.DB_Manager;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using TLABS.Extensions;


namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for TeacherProfileControl.xaml
    /// </summary>
    public partial class TeacherProfileControl : UserControl
    {
        List<Teacher> Teachers;
        int CurrentShowingTeacherIndex = -1;
        
        Border CurrentTeacherPhotoContainer, NextTeacherPhotoContainer;
        Border CurrentTeacherImageBlurred, NextTeacherImageBlurred;

        ImageSource MaleSilhouette, FemaleSilhouette;

        System.Timers.Timer TeacherShuffleTimer;
        QuadraticEase QEaseOut = new QuadraticEase() { EasingMode = EasingMode.EaseOut };

        public event EventHandler Completed;
        int ShuffleInterval = 10000;
        bool Repeat = false;

        public TeacherProfileControl()
        {
            InitializeComponent();

            SetControls();
            SetUI();
            AddEvents();
        }

        void SetControls()
        {     
            MaleSilhouette = new BitmapImage(new Uri("pack://application:,,,/InteractiveNoticeboard;component/Resources/male_silhouette.jpg"));
            FemaleSilhouette = new BitmapImage(new Uri("pack://application:,,,/InteractiveNoticeboard;component/Resources/female_silhouette.png"));

            CurrentTeacherPhotoContainer = TeacherPhotoContainer1;
            NextTeacherPhotoContainer = TeacherPhotoContainer2;

            CurrentTeacherImageBlurred = TeacherImageBlurred1;
            NextTeacherImageBlurred = TeacherImageBlurred2;

            if (!SettingsManager.HasSettings("TeachersProfileViewer", "SlideshowInterval"))
            {
                ShuffleInterval = 15000;
                SettingsManager.SetSettings("TeachersProfileViewer", "SlideshowInterval", 15000);
            }
            else
            {
                ShuffleInterval = SettingsManager.GetSettings("TeachersProfileViewer", "SlideshowInterval").ToInt(15000);
                if (ShuffleInterval < 5000) ShuffleInterval = 5000;
                if (ShuffleInterval > 60000) ShuffleInterval = 60000;
            }
        }

        void SetUI()
        {
            Teachers = Teacher.GetAllTeachers();

            if(Teachers.Count > 0)
            {
                CurrentShowingTeacherIndex = 0;
                Teacher current_teacher = Teachers[CurrentShowingTeacherIndex];
                CurrentTeacherPhotoContainer.Background = new ImageBrush(GetTeacherPhoto(current_teacher));        
                CurrentTeacherImageBlurred.Background = new VisualBrush() { Viewport = new Rect(-.1, -.1, 1.2, 1.2), Stretch = Stretch.UniformToFill, Visual = new Image() { Source = GetTeacherPhoto(current_teacher), Effect = new BlurEffect() { Radius = 20 } } };

                txtCurrentTeacherName.Text = current_teacher.FullName;
                txtCurrentTeacherDesignation.Text = current_teacher.Designation;
                txtCurrentTeacherEmail.Text = current_teacher.Email;
                txtCurrentTeacherPhone.Text = current_teacher.Phone;
                txtCurrentTeacherResearchInterest.Text = current_teacher.ResearchInterest;
                if (!string.IsNullOrEmpty(current_teacher.About))
                {
                    txtCurrentTeacherAbout.Visibility = System.Windows.Visibility.Visible;
                    runCurrentTeacherAbout.Text = current_teacher.About;
                }
                else
                {
                    txtCurrentTeacherAbout.Visibility = System.Windows.Visibility.Collapsed;
                }
               

                if (Teachers.Count > 1)
                {
                    if (TeacherShuffleTimer == null)
                    {
                        TeacherShuffleTimer = new System.Timers.Timer();
                        TeacherShuffleTimer.Interval = ShuffleInterval;
                        TeacherShuffleTimer.Elapsed += TeacherShuffleTimer_Elapsed;
                    }

                    TeacherShuffleTimer.Start();
                }
                else
                {
                    if (TeacherShuffleTimer != null) TeacherShuffleTimer.Stop();
                }
            }
            else
            {
                NoTeacherOverlay.Visibility = Visibility.Visible;               
            }
        }

        void AddEvents()
        {
        }

        void TeacherShuffleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
                {
                    ShuffleTeachers();
                });            
        }

        void ShuffleTeachers()
        {
            int next_teacher_index = CurrentShowingTeacherIndex + 1;            
            if (next_teacher_index >= Teachers.Count)
            {
                if(!Repeat)
                {
                    TeacherShuffleTimer.Stop();

                    EventHandler eh = this.Completed;
                    if(eh != null)
                    {
                        eh(this, new EventArgs());
                    }

                    return;
                }

                next_teacher_index = 0;
            }

            Teacher next_teacher = Teachers[next_teacher_index];
            if (next_teacher != null)
            {
                NextTeacherPhotoContainer.Background = new ImageBrush(GetTeacherPhoto(next_teacher));
                //NextTeacherImageBlurred.Background = new ImageBrush(GetTeacherPhoto(next_teacher)) { Stretch = Stretch.UniformToFill };                
                double w = NextTeacherImageBlurred.ActualWidth, h = NextTeacherImageBlurred.ActualHeight;
                NextTeacherImageBlurred.Background = new VisualBrush() { Viewport = new Rect(-.1, -.1, 1.2, 1.2), Stretch = System.Windows.Media.Stretch.UniformToFill, Visual = new Image() { Source = GetTeacherPhoto(next_teacher), Effect = new BlurEffect() { Radius = 20 } } };

                txtNextTeacherName.Text = next_teacher.FullName;
                txtNextTeacherDesignation.Text = next_teacher.Designation;
                txtNextTeacherEmail.Text = next_teacher.Email;
                txtNextTeacherPhone.Text = next_teacher.Phone;
                txtNextTeacherResearchInterest.Text = next_teacher.ResearchInterest;
                if (!string.IsNullOrEmpty(next_teacher.About))
                {
                    txtNextTeacherAbout.Visibility = System.Windows.Visibility.Visible;
                    runNextTeacherAbout.Text = next_teacher.About;
                }
                else
                {
                    txtNextTeacherAbout.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            CurrentShowingTeacherIndex = next_teacher_index; 

            Storyboard sb_shuffle = new Storyboard();
            {
                int duration = 500;

                #region Next
                {
                    DoubleAnimation fadein = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    fadein.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(fadein, NextTeacherPhotoContainer);
                    Storyboard.SetTargetProperty(fadein, new PropertyPath("Opacity"));
                    sb_shuffle.Children.Add(fadein);
                }
                {
                    ThicknessAnimation slide_up = new ThicknessAnimation(new Thickness(0, 375, 0, 0), new Thickness(0, 125, 0, 0), TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    slide_up.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(slide_up, NextTeacherPhotoContainer);
                    Storyboard.SetTargetProperty(slide_up, new PropertyPath("Margin"));
                    sb_shuffle.Children.Add(slide_up);
                }
                {
                    DoubleAnimation scale_x = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    scale_x.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(scale_x, NextTeacherPhotoContainer);
                    Storyboard.SetTargetProperty(scale_x, new PropertyPath("(Border.RenderTransform).(ScaleTransform.ScaleX)"));
                    sb_shuffle.Children.Add(scale_x);
                }
                {
                    DoubleAnimation scale_y = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    scale_y.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(scale_y, NextTeacherPhotoContainer);
                    Storyboard.SetTargetProperty(scale_y, new PropertyPath("(Border.RenderTransform).(ScaleTransform.ScaleY)"));
                    sb_shuffle.Children.Add(scale_y);
                }
                {
                    DoubleAnimation fadein = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(0)) { EasingFunction = QEaseOut };
                    fadein.BeginTime = TimeSpan.FromMilliseconds(500);
                    Storyboard.SetTarget(fadein, NextTeacherInfoPanel);
                    Storyboard.SetTargetProperty(fadein, new PropertyPath("Opacity"));
                    sb_shuffle.Children.Add(fadein);
                }
                {
                    DoubleAnimation fadein = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500)) { EasingFunction = QEaseOut };
                    fadein.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(fadein, NextTeacherImageBlurred);
                    Storyboard.SetTargetProperty(fadein, new PropertyPath("Opacity"));
                    sb_shuffle.Children.Add(fadein);
                }
                #endregion

                #region Current
                {
                    DoubleAnimation fadeout = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    fadeout.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(fadeout, CurrentTeacherPhotoContainer);
                    Storyboard.SetTargetProperty(fadeout, new PropertyPath("Opacity"));
                    sb_shuffle.Children.Add(fadeout);
                }
                {
                    ThicknessAnimation slide_up = new ThicknessAnimation(new Thickness(0, 125, 0, 0), new Thickness(0, -125, 0, 0), TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    slide_up.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(slide_up, CurrentTeacherPhotoContainer);
                    Storyboard.SetTargetProperty(slide_up, new PropertyPath("Margin"));
                    sb_shuffle.Children.Add(slide_up);
                }
                {
                    DoubleAnimation scale_x = new DoubleAnimation(1, 0.5, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    scale_x.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(scale_x, CurrentTeacherPhotoContainer);
                    Storyboard.SetTargetProperty(scale_x, new PropertyPath("(Border.RenderTransform).(ScaleTransform.ScaleX)"));
                    sb_shuffle.Children.Add(scale_x);
                }
                {
                    DoubleAnimation scale_y = new DoubleAnimation(1, 0.5, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    scale_y.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(scale_y, CurrentTeacherPhotoContainer);
                    Storyboard.SetTargetProperty(scale_y, new PropertyPath("(Border.RenderTransform).(ScaleTransform.ScaleY)"));
                    sb_shuffle.Children.Add(scale_y);
                }
                {
                    DoubleAnimation fadeout = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(0)) { EasingFunction = QEaseOut };
                    fadeout.BeginTime = TimeSpan.FromMilliseconds(500);
                    Storyboard.SetTarget(fadeout, CurrentTeacherInfoPanel);
                    Storyboard.SetTargetProperty(fadeout, new PropertyPath("Opacity"));
                    sb_shuffle.Children.Add(fadeout);
                }
                {
                    DoubleAnimation fadeout = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500)) { EasingFunction = QEaseOut };
                    fadeout.BeginTime = TimeSpan.FromMilliseconds(00);
                    Storyboard.SetTarget(fadeout, CurrentTeacherImageBlurred);
                    Storyboard.SetTargetProperty(fadeout, new PropertyPath("Opacity"));
                    sb_shuffle.Children.Add(fadeout);
                }
                #endregion

                //sb_shuffle.FillBehavior = FillBehavior.Stop;
                sb_shuffle.Completed += (_o, _e) =>
                {                
                    if (CurrentTeacherPhotoContainer == TeacherPhotoContainer1)
                    {
                        CurrentTeacherPhotoContainer = TeacherPhotoContainer2;
                        NextTeacherPhotoContainer = TeacherPhotoContainer1;
                    }
                    else
                    {
                        CurrentTeacherPhotoContainer = TeacherPhotoContainer1;
                        NextTeacherPhotoContainer = TeacherPhotoContainer2;
                    }

                    if (CurrentTeacherImageBlurred == TeacherImageBlurred1)
                    {
                        CurrentTeacherImageBlurred = TeacherImageBlurred2;
                        NextTeacherImageBlurred = TeacherImageBlurred1;
                    }
                    else
                    {
                        CurrentTeacherImageBlurred = TeacherImageBlurred1;
                        NextTeacherImageBlurred = TeacherImageBlurred2;
                    }
                  
                    NextTeacherPhotoContainer.Margin = new Thickness(0, 375, 0, 0);
                    
                    Teacher current_teacher = Teachers[CurrentShowingTeacherIndex];
                    txtCurrentTeacherName.Text = current_teacher.FullName;
                    txtCurrentTeacherDesignation.Text = current_teacher.Designation;
                    txtCurrentTeacherEmail.Text = current_teacher.Email;
                    txtCurrentTeacherPhone.Text = current_teacher.Phone;
                    txtCurrentTeacherResearchInterest.Text = current_teacher.ResearchInterest;
                    if (!string.IsNullOrEmpty(current_teacher.About))
                    {
                        txtCurrentTeacherAbout.Visibility = System.Windows.Visibility.Visible;
                        runCurrentTeacherAbout.Text = current_teacher.About;
                    }
                    else
                    {
                        txtCurrentTeacherAbout.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    CurrentTeacherInfoPanel.Opacity = 1;
                    NextTeacherInfoPanel.Opacity = 0;
                };
            }
            sb_shuffle.Begin();
        }

        ImageSource GetTeacherPhoto(Teacher teacher)
        {
            if (teacher == null) return null;
            if (teacher.Photo != null) return teacher.Photo;

            if (teacher.Gender == Gender.Female) return FemaleSilhouette;
            else return MaleSilhouette;
        }
    }
}
