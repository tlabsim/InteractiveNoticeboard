using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveNoticeboard.Data_Structures.API.NewsAPI
{
    public class ArticleSource
    {
        public string id;
        public string name;
    }

    public class Article
    {
        public ArticleSource source;
        public string author;
        public string title;
        public string description;
        public string url;
        public string urlToImage;
        public DateTime publishedAt;
        public string content;
    }

    public class NewsAPIResponse
    {
        public string status;
        public int totalResults;
        public List<Article> articles;
    }
}
