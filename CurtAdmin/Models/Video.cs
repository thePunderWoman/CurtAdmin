using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin {
    partial class Video {
        public void Update(Google.YouTube.Video ytvideo) {
            CurtDevDataContext db = new CurtDevDataContext();
            Video v = db.Videos.Where(x => x.videoID.Equals(this.videoID)).FirstOrDefault<Video>();
            v.title = ytvideo.Title;
            v.description = ytvideo.Description;
            v.embed_link = ytvideo.VideoId;
            v.youtubeID = ytvideo.VideoId;
            v.watchpage = ytvideo.WatchPage.ToString();
            foreach (Google.GData.Extensions.MediaRss.MediaThumbnail thumb in ytvideo.Thumbnails) {
                if (thumb.Url.Contains("hqdefault")) {
                    v.screenshot = thumb.Url;
                }
            }
            db.SubmitChanges();
        }
    }
}