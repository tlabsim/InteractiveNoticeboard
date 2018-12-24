using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace InteractiveNoticeboard
{
    public enum NoticeTypes
    {
        Scanned,
        RichText
    }


    public class Notice
    {
        public NoticeTypes NoticeType
        {
            get;
            protected set;
        }

        public static double BlockSize = 25;  // in points, square
        public static double Margin = 25;     // in points
         
        public double Width { get; set; }
        public double Height { get; set; }
        public double Area
        {
            get
            {
                return Width * Height;
            }
        }

        public int WidthInBlocks
        {
            get
            {
                return (int)Math.Ceiling((Width + Margin) / (double)BlockSize);
            }
        }
        public int HeightInBlocks
        {
            get
            {
                return (int)Math.Ceiling((Height + Margin) / (double)BlockSize);
            }
        }

        public int AreaInBlocks
        {
            get
            {
                return WidthInBlocks * HeightInBlocks;
            }
        }

        public double BlockX { get; set; }
        public double BlockY { get; set; }

        public DateTime LastWriteTime { get; set; }
    }

    public class ScannedNotice : Notice
    {
        BitmapImage _Image = null;
        public BitmapImage Image
        {
            get
            {
                return _Image;
            }
            set
            {
                _Image = value;
            }
        }

        public ScannedNotice()
        {
            this.NoticeType = NoticeTypes.Scanned;
        }        
    }

    public class RichTextNotice : Notice
    {
        string _RichText = string.Empty;

        public string RichText
        {
            get
            {
                return _RichText;
            }
            set
            {
                _RichText = value;
            }
        }

        string _FileLocation = string.Empty;
        public string FileLocation
        {
            get
            {
                return _FileLocation;
            }
            set
            {
                _FileLocation = value;
            }
        }

        public RichTextNotice()
        {
            this.NoticeType = NoticeTypes.RichText;
        }

        public void Load()
        {

        }
    }
}
