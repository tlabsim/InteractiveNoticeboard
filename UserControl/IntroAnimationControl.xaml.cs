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

using TLABS.Extensions;

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
                new Typer.Line("Department of", 40, "#FF3050A0"),
                new Typer.Line("Computer Science and Telcommunication Engineering", 50, "#FF204090"),
            };

            Typer typer = new Typer();
            typer.TextBlock = txtWelcomeMessage;

            Session.Window.MediaPlayer.Source = new Uri(Settings.AppPath + "/Media/Sounds/typewriter-key.mp3", UriKind.RelativeOrAbsolute);
            Session.Window.MediaPlayer.Volume = 0.05;

            typer.Typing += (o, e) =>
                {
                    if (PlaySound)
                    {
                        Session.Window.MediaPlayer.Stop();
                        Session.Window.MediaPlayer.Play();
                    }
                };

            typer.Type(lines);
        }

        void AnimateCSTE()
        {
            CAC.SetupCanvas();
            CAC.StartTextStream();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //TypeWelcomeMessage();
            AnimateCSTE();
        }
    }
}
