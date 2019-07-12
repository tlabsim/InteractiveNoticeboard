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
using TLABS.Extensions;
using TLABS.WPF.Animations;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for IntroAnimationControl.xaml
    /// </summary>
    public partial class IntroAnimationControl : UserControl
    {
        public object UIContext
        {
            get;
            set;
        }

        public bool PlaySound = true;

        System.Timers.Timer TimeoutTimer;
        public event EventHandler Completed;

        public IntroAnimationControl()
        {
            InitializeComponent();

            this.DataContext = this;
            this.UIContext = Settings.UISettings;

            SetControls();
            SetUI();
            AddEvents();
        }

        void SetControls()
        {
            TimeoutTimer = new System.Timers.Timer();
            TimeoutTimer.Interval = 60000;
            TimeoutTimer.Elapsed += TimeoutTimer_Elapsed;
            TimeoutTimer.Start();
        }

        void TimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if(TimeoutTimer != null)
                {
                    TimeoutTimer.Stop();
                }

                this.Dispatcher.Invoke(() =>
                    {
                        OnComplete();
                    });
            }
            catch { }
        }

        void OnComplete()
        {
            EventHandler eh = this.Completed;

            if(eh != null)
            {
                eh(this, new EventArgs());
            }
        }

        void SetUI()
        {            
        }

        void AddEvents()
        {
        }

        void TypeWelcomeMessage()
        {
            Typer.Line[] lines = 
            {
                new Typer.Line("Welcome to", 30, "#FF464646"),
                new Typer.Line("Department of", 35, "#FF3050A0"),
                new Typer.Line("Computer Science and Telcommunication Engineering", 40, "#FF204090"),
            };

            Typer typer = new Typer();
            typer.TextBlock = txtWelcomeMessage;

            Session.Window.MediaPlayer.Source = new Uri(Settings.AppPath + "/Media/Sounds/typewriter-key.mp3", UriKind.RelativeOrAbsolute);
            Session.Window.MediaPlayer.Volume = 0.2;

            typer.Typing += (o, e) =>
                {
                    if (PlaySound)
                    {
                        Session.Window.MediaPlayer.Stop();
                        Session.Window.MediaPlayer.Play();
                    }
                };

            typer.TypingEnded += typer_TypingEnded;
            typer.Type(lines);
        }

        void typer_TypingEnded(object sender, EventArgs e)
        {
            AnimateCSTE();
        }

        void AnimateCSTE()
        {          
            CAC.SetupCanvas();
            CAC.StartTextStream();

            CAC.CacheMode = new BitmapCache();
            DoubleAnimation fade_in = new DoubleAnimation(1, TimeSpan.FromMilliseconds(1000));
            CAC.BeginAnimation(UIElement.OpacityProperty, fade_in);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TypeWelcomeMessage();
            //AnimateCSTE();
        }
    }
}
