using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;

namespace CurtAdmin.Controllers {
    public class Admin_VideoController : AdminBaseController {
        //
        // GET: /Video/

        public ActionResult Index(int page = 1) {

            // Retrieve the videos
            Dictionary<CurtAdmin.Video, Google.YouTube.Video> videos = VideoModel.GetVideos();
            ViewBag.videos = videos;

            Google.GData.Client.Feed<Google.YouTube.Video> allvideos = VideoModel.GetCurtVideos(page);
            ViewBag.allvideos = allvideos;
            ViewBag.page = page;

            return View();
        }

        public string AddVideo(string ytID = "") {
            FullVideo record = VideoModel.Create(ytID);
            Google.YouTube.Video ytVideo = new Google.YouTube.Video();
            ytVideo = VideoModel.GetYTVideo(ytID);
            record.videoTitle = ytVideo.Title;
            record.thumb = (ytVideo.Thumbnails.Count > 0) ? ytVideo.Thumbnails[0].Url : "/Content/img/noimage.jpg";

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(record);
        }
        

        public string Delete(int id = 0) {
            return VideoModel.Delete(id);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public void updateSort() {
            List<string> videos = Request.QueryString["video[]"].Split(',').ToList<string>();
            VideoModel.Sort(videos);
        }

    }
}
