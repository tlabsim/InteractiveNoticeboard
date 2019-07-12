using InteractiveNoticeboard.Data_Structures.API.NewsAPI;
using InteractiveNoticeboard.DB_Manager;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TLABS.WPF.Animations;

namespace InteractiveNoticeboard
{
    /// <summary>
    /// Interaction logic for NewsBoard.xaml
    /// </summary>
    public partial class NewsBoard : UserControl, INotifyPropertyChanged
    {
        public enum NewsModes
        {
            Technology,
            Sports
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        NewsModes _NewsMode = NewsModes.Technology;
        public NewsModes NewsMode
        {
            get
            {
                return _NewsMode;
            }
            set
            {
                _NewsMode = value;

                if(_NewsMode == NewsModes.Technology)
                {
                    txtHeader.Text = "Tech News Today";
                    txtLoadingStateHeader.Text = "Tech News Today";
                    txtLoadingFailedHeader.Text = "Tech News Today";
                }
                else if (_NewsMode == NewsModes.Sports)
                {
                    txtHeader.Text = "Sports News Today";
                    txtLoadingStateHeader.Text = "Sports News Today";
                    txtLoadingFailedHeader.Text = "Sports News Today";
                }
            }
        }

        string API_Key = "ea49a2a2178945418a87f7ba98db1ed8";
        NewsAPIResponse FetchedNews = null;
        int FetchedArticlesCount = 0;
        int CurrentArticleIndex = 0;
        System.Timers.Timer NewsTimer;
        int NewsShuffleInterval = 10000;

        string LocalArticleImagesPath = string.Empty;
        ImageSource DefaultArticleImage;

        Storyboard ArticleSlideShowStoryboard;
        QuadraticEase QEaseOut = new QuadraticEase() { EasingMode = EasingMode.EaseOut };

        public event EventHandler Completed;

        public NewsBoard(NewsModes news_mode = NewsModes.Technology)
        {
            InitializeComponent();

            this.NewsMode = news_mode;
            LoadSettings();
            SetControls();
            GetNews();
            SetUI();
            AddEvents();
        }

        void LoadSettings()
        {
            string overridden_api_key = SettingsManager.GetSettings("NewsViewer", "APIKey");
            if(!string.IsNullOrEmpty(overridden_api_key))
            {
                API_Key = overridden_api_key;
            }
        }

        void SetControls()
        {
            DefaultArticleImage = new BitmapImage(new Uri("pack://application:,,,/InteractiveNoticeboard;component/Resources/tech-news.jpg", UriKind.RelativeOrAbsolute));

            LocalArticleImagesPath = Settings.AppPath.TrimEnd('\\') + @"\Data\TempArticleImages\";
            DirectoryInfo DI = new DirectoryInfo(LocalArticleImagesPath);

            if(!DI.Exists)
            {
                DI.Create();
            }
            else
            {
                //Delete all images more than three days old 
                try
                {
                    var three_days_before = DateTime.Now.Subtract(TimeSpan.FromDays(3));
                    var old_files = DI.GetFiles().ToList().FindAll(f => f.LastWriteTime < three_days_before);
                    old_files.ForEach(f => f.Delete());
                }
                catch { }
            }

            LoadingStateOverlay.Visibility = Visibility.Visible;
        }

        void GetNews()
        {
            string from_date = DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToString("yyyy-mm-dd");            
            string rest_query = string.Empty;

            if(NewsMode == NewsModes.Sports)
            {
                rest_query = string.Format("https://newsapi.org/v2/top-headlines?country=in&category=sport&from={0}&sortBy=publishedAt&apiKey={1}", from_date, API_Key);
            }
            else
            {
                rest_query = string.Format("https://newsapi.org/v2/top-headlines?country=us&category=technology&from={0}&sortBy=publishedAt&apiKey={1}", from_date, API_Key);
            }          
        
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
                            FetchedNews = JsonConvert.DeserializeObject<NewsAPIResponse>(JSON);
                            //Console.WriteLine(result.articles[0].title);
                            break;
                        }
                    }
                }
                catch { }

                tries++;
            }

            if(FetchedNews == null || FetchedNews.articles == null || FetchedNews.articles.Count == 0)
            {
                LoadingStateOverlay.FadeOut(500);
                LoadFailedOverlay.Opacity = 0;
                LoadFailedOverlay.Visibility = System.Windows.Visibility.Visible;

                Storyboard exit_sb = new Storyboard();
                exit_sb.Duration = TimeSpan.FromMilliseconds(5500);
                {
                    DoubleAnimation fade_in = new DoubleAnimation(1, TimeSpan.FromMilliseconds(500));
                    fade_in.BeginTime = TimeSpan.FromMilliseconds(0);
                    Storyboard.SetTarget(fade_in, LoadFailedOverlay);
                    Storyboard.SetTargetProperty(fade_in, new PropertyPath("Opacity"));
                    exit_sb.Children.Add(fade_in);
                }

                exit_sb.Completed += exit_sb_Completed;
                exit_sb.Begin();
            }
            else
            {
                if(FetchedNews.articles.Count > 20)
                {
                    FetchedNews.articles = FetchedNews.articles.GetRange(0, 20);
                }

                FetchedArticlesCount = FetchedNews.articles.Count;
                CurrentArticleIndex = 0;
                DownloadArticleImages();                
                ShowNews(CurrentArticleIndex);
                LoadingStateOverlay.FadeOut();

                if(NewsTimer == null)
                {
                    NewsTimer = new System.Timers.Timer();
                    NewsTimer.Interval = NewsShuffleInterval;
                    NewsTimer.Elapsed += news_timer_Elapsed;
                }
                NewsTimer.Start();                
            }
        }

        void exit_sb_Completed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                EventHandler eh = this.Completed;
                if (eh != null)
                {

                    eh(this, new EventArgs());
                }
            });
        }

        void news_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                CurrentArticleIndex++;
                if (CurrentArticleIndex >= FetchedArticlesCount)
                {
                    EventHandler eh = this.Completed;
                    if (eh != null)
                    {
                        NewsTimer.Stop();
                        eh(this, new EventArgs());
                        return;
                    }
                    else
                    {
                        CurrentArticleIndex = 0;
                    }
                }

                ShowNews(CurrentArticleIndex);
            });
        }

        void SetUI()
        {
        }

        void AddEvents()
        { }

        void InitAnimationStoryboard()
        {
            if (ArticleSlideShowStoryboard == null)
            {
                ArticleSlideShowStoryboard = new Storyboard();
                {
                    int duration = 500;

                    {
                        DoubleAnimation fadein = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                        fadein.BeginTime = TimeSpan.FromMilliseconds(0);
                        Storyboard.SetTarget(fadein, ArticleInfoPanel);
                        Storyboard.SetTargetProperty(fadein, new PropertyPath("Opacity"));
                        ArticleSlideShowStoryboard.Children.Add(fadein);
                    }
                    {
                        DoubleAnimation scale_x = new DoubleAnimation(50, 0, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                        scale_x.BeginTime = TimeSpan.FromMilliseconds(0);
                        Storyboard.SetTarget(scale_x, ArticleInfoPanel);
                        Storyboard.SetTargetProperty(scale_x, new PropertyPath("(StackPanel.RenderTransform).(TranslateTransform.X)"));
                        ArticleSlideShowStoryboard.Children.Add(scale_x);
                    }

                    {
                        DoubleAnimation fadein = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                        fadein.BeginTime = TimeSpan.FromMilliseconds(0);
                        Storyboard.SetTarget(fadein, imgArticleImage);
                        Storyboard.SetTargetProperty(fadein, new PropertyPath("Opacity"));
                        ArticleSlideShowStoryboard.Children.Add(fadein);
                    }
                    //{
                    //    DoubleAnimation scale_x = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    //    scale_x.BeginTime = TimeSpan.FromMilliseconds(0);
                    //    Storyboard.SetTarget(scale_x, imgArticleImage);
                    //    Storyboard.SetTargetProperty(scale_x, new PropertyPath("(Image.RenderTransform).(ScaleTransform.ScaleX)"));
                    //    ArticleSlideShowStoryboard.Children.Add(scale_x);
                    //}
                    //{
                    //    DoubleAnimation scale_y = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(duration)) { EasingFunction = QEaseOut };
                    //    scale_y.BeginTime = TimeSpan.FromMilliseconds(0);
                    //    Storyboard.SetTarget(scale_y, imgArticleImage);
                    //    Storyboard.SetTargetProperty(scale_y, new PropertyPath("(Image.RenderTransform).(ScaleTransform.ScaleY)"));
                    //    ArticleSlideShowStoryboard.Children.Add(scale_y);
                    //}
                }
            }
        }

        void ShowNews(int index)
        {
            if (FetchedNews != null && FetchedNews.articles != null && FetchedNews.articles.Count > 0 && index >= 0 && index < FetchedNews.articles.Count)
            {
                txtArticleCounter.Text = string.Format("{0:00} of {1:00}", index + 1, FetchedNews.articles.Count);
                var article = FetchedNews.articles[index];

                if (article != null)
                {
                    txtArticleTitle.Text = article.title;
                    runArticlePublishDate.Text = article.publishedAt.ToLocalTime().ToString("F");
                    runArticleSource.Text = string.IsNullOrEmpty(article.author) ? (string.IsNullOrEmpty(article.source.name) ? "" : article.source.name) : (string.IsNullOrEmpty(article.source.name) ? article.author : string.Format("{0} • {1}", article.author, article.source.name));
                    txtArticleDescription.Text = article.description;

                    if (ArticleSlideShowStoryboard == null) InitAnimationStoryboard();
                    ArticleSlideShowStoryboard.Begin();

                    #region Show article image
                    string image_url = article.urlToImage;
                    if (!string.IsNullOrEmpty(image_url))
                    {
                        var hash = GetHashString(image_url);
                        var local_image_path = LocalArticleImagesPath + hash + ".jpg";

                        if (File.Exists(local_image_path))
                        {
                            try
                            {
                                var bmp = new BitmapImage();
                                bmp.BeginInit();
                                bmp.CacheOption = BitmapCacheOption.OnLoad;
                                bmp.UriSource = new Uri(local_image_path, UriKind.Absolute);
                                bmp.EndInit();
                                bmp.Freeze();

                                imgArticleBack.Source = bmp;
                                imgArticleImage.Source = bmp;
                            }
                            catch { }
                        }
                        else
                        {
                            imgArticleBack.Source = DefaultArticleImage;
                            imgArticleImage.Source = DefaultArticleImage;
                            DownloadImage(image_url, local_image_path, true);
                        }
                    }
                    else
                    {
                        imgArticleBack.Source = DefaultArticleImage;
                        imgArticleImage.Source = DefaultArticleImage;
                    }
                    #endregion
                }
            }
            else
            {
                txtArticleCounter.Text = "No articles";
            }
        }

        void DownloadArticleImages()
        {
            if (FetchedNews != null && FetchedNews.articles != null && FetchedNews.articles.Count > 0)
            {
                foreach(var article in FetchedNews.articles)
                {
                    var image_url = article.urlToImage;
                    if (!string.IsNullOrEmpty(image_url))
                    {
                        var hash = GetHashString(image_url);
                        var local_image_path = LocalArticleImagesPath + hash + ".jpg";

                        if (File.Exists(local_image_path)) continue;
                        else
                        {
                            DownloadImage(image_url, local_image_path);
                        }
                    }
                }
            }
        }

        void DownloadImage(string image_url, string save_path, bool show = false)
        {
            var client = new WebClient();
            if (show)
            {
                client.DownloadDataCompleted += ImageDownloadCompleted2;
            }
            else
            {
                client.DownloadDataCompleted += ImageDownloadCompleted;
            }
            client.DownloadDataAsync(new Uri(image_url), save_path);
        }

        private void ImageDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
        {            
            if (!e.Cancelled && e.Error == null)
            {
                string save_path = (string)e.UserState; 
                using (var ms = new MemoryStream(e.Result))
                {
                    using (FileStream fs = new FileStream(save_path, FileMode.Create, FileAccess.Write))
                    {
                        ms.WriteTo(fs);
                    }
                }
            }
        }

        private void ImageDownloadCompleted2(object sender, DownloadDataCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                string save_path = (string)e.UserState;
                using (var ms = new MemoryStream(e.Result))
                {
                    using (FileStream fs = new FileStream(save_path, FileMode.Create, FileAccess.Write))
                    {
                        ms.WriteTo(fs);
                    }

                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();

                    imgArticleBack.Source = bmp;
                    imgArticleImage.Source = bmp;
                }
            }
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(str))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
