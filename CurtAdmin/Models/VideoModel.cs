using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;

namespace CurtAdmin.Models {
    public class VideoModel {

        public static Dictionary<CurtAdmin.Video, Google.YouTube.Video> GetVideos() {
            try {
                // Initiate database
                CurtDevDataContext db = new CurtDevDataContext();
                Dictionary<CurtAdmin.Video, Google.YouTube.Video> videos = new Dictionary<Video, Google.YouTube.Video>();

                // Get the video results from the database
                List<Video> video_records = db.Videos.OrderBy(x => x.sort).ToList<Video>();

                // Loop through the databased video records and build out the YouTube video objects for them
                foreach (Video record in video_records) {
                    if (record.embed_link.Length > 0) {
                        Google.YouTube.Video ytVideo = GetYouTubeVid(record.embed_link);
                        if (ytVideo.VideoId != null) {
                            record.Update(ytVideo);
                            videos.Add(record, ytVideo);
                        } else {
                            Delete(record.videoID);
                        }
                    }
                }
                return videos;
            } catch (Exception) {
                return new Dictionary<CurtAdmin.Video, Google.YouTube.Video>();
            }
        }

        public static CurtAdmin.Video GetRecord(int id = 0) {
            try {
                if(id == 0){ return new Video(); }

                CurtDevDataContext db = new CurtDevDataContext();
                return db.Videos.Where(x => x.videoID == id).FirstOrDefault<Video>();
            } catch (Exception) {
                return new Video();
            }
        }

        public static Google.YouTube.Video GetYTVideo(string ytID = "") {
            try {
                if (ytID == null || ytID.Length == 0) { throw new Exception(); }

                Google.YouTube.Video vid = GetYouTubeVid(ytID);
                return vid;
            } catch (Exception) {
                return new Google.YouTube.Video();
            }
        }


        public static FullVideo Create(Google.YouTube.Video ytVideo = null) {
            if (ytVideo == null) {
                throw new Exception("Invalid link");
            }
            CurtDevDataContext db = new CurtDevDataContext();
            Video new_video = new Video {
                embed_link = ytVideo.VideoId,
                title = ytVideo.Title,
                screenshot = (ytVideo.Thumbnails.Count > 0) ? ytVideo.Thumbnails[2].Url : "/Content/img/noimage.jpg",
                description = ytVideo.Description,
                watchpage = ytVideo.WatchPage.ToString(),
                youtubeID = ytVideo.VideoId,
                dateAdded = DateTime.Now,
                sort = (db.Videos.Count() == 0) ? 1 : db.Videos.OrderByDescending(x => x.sort).Select(x => x.sort).First() + 1
            };
            db.Videos.InsertOnSubmit(new_video);
            db.SubmitChanges();

            FullVideo fullvideo = new FullVideo {
                videoID = new_video.videoID,
                embed_link = new_video.embed_link,
                dateAdded = new_video.dateAdded,
                sort = new_video.sort,
                videoTitle = new_video.title,
                thumb = (ytVideo.Thumbnails.Count > 0) ? ytVideo.Thumbnails[0].Url : "/Content/img/noimage.jpg"
            };

            return fullvideo;
        }

        public static string Delete(int id = 0) {
            try {
                if (id == 0) { throw new Exception("Invalid video ID"); }
                CurtDevDataContext db = new CurtDevDataContext();

                Video record = db.Videos.Where(x => x.videoID == id).FirstOrDefault<Video>();
                db.Videos.DeleteOnSubmit(record);
                db.SubmitChanges();
                List<string> ids = db.Videos.OrderBy(x => x.sort).Select(x => x.videoID.ToString()).ToList();
                Sort(ids);
                return "success";
            } catch (Exception e) {
                return e.Message;
            }
        }

        public static Feed<Google.YouTube.Video> GetCurtVideos(int page = 1) {
            try {
                YouTubeRequestSettings settings = new YouTubeRequestSettings("curtmfg", "AI39si6iCFZ_NutrvZe04i9_m7gFhgmPK1e7LF6-yHMAwB-GDO3vC3eD0R-5lberMQLdglNjH3IWUMe3tJXe9qrFe44n2jAUyg");
                YouTubeRequest req = new YouTubeRequest(settings);

                YouTubeQuery query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
                query.Author = "curtmfg";
                query.Formats.Add(YouTubeQuery.VideoFormat.Embeddable);
                query.OrderBy = "viewCount";
                query.StartIndex = ((page - 1) * 25) + 1;

                // We need to load the feed data for the CURTMfg Youtube Channel
                Feed<Google.YouTube.Video> video_feed = req.Get<Google.YouTube.Video>(query);
                return video_feed;
            } catch (Exception) {
                return null;
            }
        }

        private static Google.YouTube.Video GetYouTubeVid(string id = "") {
            // Initiate video object
            Google.YouTube.Video video = new Google.YouTube.Video();

            try {
                // Initiate YouTube request object
                YouTubeRequestSettings settings = new YouTubeRequestSettings("curtmfg", "AI39si6iCFZ_NutrvZe04i9_m7gFhgmPK1e7LF6-yHMAwB-GDO3vC3eD0R-5lberMQLdglNjH3IWUMe3tJXe9qrFe44n2jAUyg");
                YouTubeRequest req = new YouTubeRequest(settings);

                // Create URI and make request to YouTube
                Uri video_url = new Uri("http://gdata.youtube.com/feeds/api/videos/" + id);
                video = req.Retrieve<Google.YouTube.Video>(video_url);
            } catch { };

            return video;
        }
        
        public static void Sort(List<string> ids) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                int sort = 0;
                foreach (string id in ids) {
                    sort++;
                    Video v = db.Videos.Where(x => x.videoID == Convert.ToInt32(id)).First();
                    v.sort = sort;
                    db.SubmitChanges();
                }
            } catch { }
        }
    }
    public class FullVideo : Video {
        public string videoTitle { get; set; }
        public string thumb { get; set; }
    }
}