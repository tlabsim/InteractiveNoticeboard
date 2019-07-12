using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TLABS.Extensions;

namespace InteractiveNoticeboard.Data_Structures
{
    public class FeaturedVideo
    {
        public int VideoID { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public string SubtitleFilename { get; set; }
        public DateTime? PostDate { get; set; }
        public int PlayCount { get; set; }

        public static List<FeaturedVideo> GetAllFeaturedVideos()
        {
            List<FeaturedVideo> videos = new List<FeaturedVideo>();

            string query = "SELECT * FROM featured_videos ORDER BY play_count ASC";

            try
            {
                using (DataTable dt = DB_Manager.DBClient.ExecuteAdapter(query))
                {
                    if (dt != null)
                    {
                        for (int r = 0; r < dt.Rows.Count; r++)
                        {
                            try
                            {
                                FeaturedVideo video = new FeaturedVideo();
                                video.VideoID = dt.Rows[r]["ID"].GetString().ToInt();
                                video.Title = dt.Rows[r]["title"].GetString().Trim();
                                video.Filename = dt.Rows[r]["filename"].GetString().Trim();
                                video.SubtitleFilename = dt.Rows[r]["subtitle_filename"].GetString().Trim();
                                video.PostDate = dt.Rows[r]["post_date"].GetString().ToDateTime();
                                video.PlayCount = dt.Rows[r]["play_count"].GetString().ToInt(0);

                                videos.Add(video);
                                Console.Write(video);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return videos;
        }

        public bool IncrementPlayCount()
        {
            string query = string.Format("UPDATE featured_videos SET play_count = play_count + 1 WHERE ID = {0}", this.VideoID);
            return DB_Manager.DBClient.ExecuteNonQuery(query) > 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("ID           : {0}", this.VideoID));
            sb.AppendLine(string.Format("Title        : {0}", this.Title));
            sb.AppendLine(string.Format("Path         : {0}", this.Filename));
            sb.AppendLine(string.Format("Subtitle path: {0}", this.SubtitleFilename));
            sb.AppendLine(string.Format("Post date    : {0}", this.PostDate.HasValue ? this.PostDate.Value.ToString() : string.Empty));
            sb.AppendLine(string.Format("Play count   : {0}", this.PlayCount));

            return sb.ToString();
        }
    }
}
