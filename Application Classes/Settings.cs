using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TLABS.Extensions;

namespace InteractiveNoticeboard
{
    public class Settings
    {
        public static CultureInfo AppCulture = CultureInfo.CreateSpecificCulture("en-US");
        public static string ApplicationName = "Interactive Noticeboard System";
        public static string AppVersionInfo = "INS v1.0.0.1";
        public static string Developer = "TLABS";

        public static string AppPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }

        public static string AppDataPath
        {
            get
            {
                string folderBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dir = string.Format(@"{0}\TLABS\INS", folderBase);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }

        public static UISettings UISettings = new UISettings();

        static Settings()
        {
        }
    }

    public class UISettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        static UISettings()
        {
        }

        public UISettings()
        {
            _DefaultFontSize = RegistryHelper.GetSettings("UI", "DefaultFontSize").ToDouble(13.0);
        }

        double _DefaultFontSize = 13;

        public double DefaultFontSize
        {
            get
            {
                return _DefaultFontSize;
            }
            set
            {
                _DefaultFontSize = value;
                NotifyPropertyChanged("DefaultFontSize");
                NotifyPropertyChanged("TinyFontSize");
                NotifyPropertyChanged("SmallFontSize");
                NotifyPropertyChanged("BigFontSize");
                NotifyPropertyChanged("LargeFontSize");
                NotifyPropertyChanged("ExtraLargeFontSize");
                NotifyPropertyChanged("HugeFontSize");
            }
        }

        public double TinyFontSize
        {
            get
            {
                return _DefaultFontSize - 4;
            }
        }

        public double SmallFontSize
        {
            get
            {
                return _DefaultFontSize - 2;
            }
        }

        public double BigFontSize
        {
            get
            {
                return _DefaultFontSize + 1;
            }
        }

        public double LargeFontSize
        {
            get
            {
                return _DefaultFontSize + 3;
            }
        }

        public double ExtraLargeFontSize
        {
            get
            {
                return _DefaultFontSize + 5;
            }
        }

        public double HugeFontSize
        {
            get
            {
                return _DefaultFontSize + 7;
            }
        }

        public void icon_IncreaseFontSize()
        {
            if (DefaultFontSize < 20)
            {
                DefaultFontSize += 1;
            }
        }

        public void icon_DecreaseFontSize()
        {
            if (DefaultFontSize > 8)
            {
                DefaultFontSize -= 1;
            }
        }

        public void icon_ResetFontSize()
        {
            DefaultFontSize = RegistryHelper.GetSettings("UI", "DefaultFontSize").ToDouble(13.0);
        }
    }
}
