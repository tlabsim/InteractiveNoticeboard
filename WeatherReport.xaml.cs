using InteractiveNoticeboard.Data_Structures.API.OpenWeatherMap;
using InteractiveNoticeboard.DB_Manager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TLABS.WPF.Animations;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for WeatherReport.xaml
    /// </summary>
    public partial class WeatherReport : UserControl
    {
        string BackgroundVideoPath = string.Empty;
        Random RNG = new Random();

        string API_Key = "97d532e7da4be7ee99be7c13d98aa879";
        WeatherData CurrentWeatherData;

        System.Timers.Timer TimeoutTimer;
        public event EventHandler Completed;
        QuadraticEase QEaseOut = new QuadraticEase() { EasingMode = EasingMode.EaseOut };

        List<string> ValidIcons = new List<string> { "01d", "01n", "02d", "02n", "03d", "03n", "04d", "04n", "09d", "09n", "10d", "10n", "11d", "11n", "13d", "13n", "50d", "50n" };

        public WeatherReport()
        {
            InitializeComponent();

            BackgroundVideoPath = Settings.AppPath.TrimEnd('\\') + @"\Data\WeatherBackgroundVideos\";
            LoadSettings();
            SetControls();
        }

        void LoadSettings()
        {
            string overridden_api_key = SettingsManager.GetSettings("WeatherReport", "APIKey");
            if (!string.IsNullOrEmpty(overridden_api_key))
            {
                API_Key = overridden_api_key;
            }
        }

        void SetControls()
        {
            if (TimeoutTimer == null)
            {
                TimeoutTimer = new System.Timers.Timer();
                TimeoutTimer.Interval = 30000;
                TimeoutTimer.Elapsed += TimeoutTimer_Elapsed;
            }

            TimeoutTimer.Start();
        }

        void TimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
                {
                    OnTimeoutTimerElapsed();
                });
        }

        void OnTimeoutTimerElapsed()
        {
            EventHandler eh = this.Completed;

            if (eh != null)
            {
                BackgroundVideoPlayer.Stop();
                if (TimeoutTimer != null) TimeoutTimer.Stop();
                eh(this, new EventArgs());
            }
        }

        void LoadBackgroundVideo()
        {
            DirectoryInfo DI = new DirectoryInfo(BackgroundVideoPath);

            if (!DI.Exists) DI.Create();

            var files = DI.GetFiles("*.mp4");

            if(files.Length > 0)
            {
                int c = files.Length;
                int r = RNG.Next(c);
                var selected_vid = files[r];

                if(selected_vid.Exists && selected_vid.Extension.ToLower() == ".mp4") //redundant check
                {
                    try
                    {
                        BackgroundVideoPlayer.Source = new Uri(selected_vid.FullName, UriKind.Absolute);
                        BackgroundVideoPlayer.MediaEnded += (_o, _e) => { BackgroundVideoPlayer.Stop(); BackgroundVideoPlayer.Play(); };
                        BackgroundVideoPlayer.Play();
                    }
                    catch
                    {
                    }
                }
            }
        }

        void LoadWeather()
        {
            string rest_query = string.Format("https://api.openweathermap.org/data/2.5/weather?lat=22.79&lon=91.1&APPID={0}", API_Key);

            int tries = 0;
            while (tries < 3)
            {
                Console.WriteLine("Try {0}", tries + 1);

                try
                {
                    WebRequest request = WebRequest.Create(rest_query);
                    request.Method = "GET";
                    request.Timeout = 10000;

                    var response = (HttpWebResponse)request.GetResponse();
                    //Console.WriteLine(response.StatusDescription);
                    var response_stream = response.GetResponseStream();
                    if (response_stream != null)
                    {
                        using (var stream_reader = new StreamReader(response_stream))
                        {
                            string JSON = stream_reader.ReadToEnd();
                            CurrentWeatherData = JsonConvert.DeserializeObject<WeatherData>(JSON);
                            //Console.WriteLine(CurrentWeatherData.main.temp);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                tries++;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBackgroundVideo();

            CurrentWeatherData = null;
            LoadWeather();

            if (CurrentWeatherData != null)
            {
                try
                {
                    txtCurrentTemp.Text = Math.Round(CurrentWeatherData.main.temp - 273.15).ToString();
                    if (CurrentWeatherData.weather.Count > 0)
                    {
                        txtWeatherSummary.Text = CurrentWeatherData.weather[0].description;
                        if (ValidIcons.Contains(CurrentWeatherData.weather[0].icon))
                        {
                            string icon_path = string.Format("pack://application:,,,/InteractiveNoticeboard;component/Resources/WeatherIcons/{0}.png", CurrentWeatherData.weather[0].icon);
                            imgIcon.Source = new BitmapImage(new Uri(icon_path));
                        }
                        else
                        {
                            imgIcon.Source = null;
                        }
                    }
                    else
                    {
                        txtWeatherSummary.Text = "---";
                        imgIcon.Source = null;
                    }

                    txtUpdatedOn.Text = string.Format("as of {0}", EpochToDateTime(CurrentWeatherData.dt).ToString(@"hh:mm tt"));

                    runCloudCover.Text = string.Format("{0}%", CurrentWeatherData.clouds.all);
                    runHumidity.Text = string.Format("{0}%", CurrentWeatherData.main.humidity);
                    runBarometer.Text = string.Format("{0}mb", CurrentWeatherData.main.pressure);
                    runWindSpeed.Text = string.Format("{0} km/h", CurrentWeatherData.wind.speed);
                    pathWindDirection.RenderTransform = new RotateTransform() { Angle = CurrentWeatherData.wind.deg };

                    Storyboard sb = new Storyboard();
                    {
                        DoubleAnimation fade_in = new DoubleAnimation(1, TimeSpan.FromMilliseconds(500)) { EasingFunction = QEaseOut };
                        fade_in.BeginTime = TimeSpan.FromMilliseconds(3000);
                        Storyboard.SetTarget(fade_in, WeatherReportContents);
                        Storyboard.SetTargetProperty(fade_in, new PropertyPath("Opacity"));
                        sb.Children.Add(fade_in);
                    }
                    {
                        DoubleAnimation fade_in = new DoubleAnimation(100, 0, TimeSpan.FromMilliseconds(500)) { EasingFunction = QEaseOut };
                        fade_in.BeginTime = TimeSpan.FromMilliseconds(3000);
                        Storyboard.SetTarget(fade_in, WeatherReportContents);
                        Storyboard.SetTargetProperty(fade_in, new PropertyPath("(StackPanel.RenderTransform).(TranslateTransform.Y)"));
                        sb.Children.Add(fade_in);
                    }
                    WeatherReportContents.CacheMode = new BitmapCache();
                    sb.Begin();
                }
                catch { }
            }
            else
            {
                WeatherReportContainer.Visibility = System.Windows.Visibility.Collapsed;
                FailedNotificationPanel.FadeIn();
            }
        }

        DateTime EpochToDateTime(int epoch)
        {
            return new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(epoch) + TimeSpan.FromHours(6); //Adjusted for Bangladesh time
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            BackgroundVideoPlayer.Stop();
            if (TimeoutTimer != null) TimeoutTimer.Stop();
        }


    }
}
