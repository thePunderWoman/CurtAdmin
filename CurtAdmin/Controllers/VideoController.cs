using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace CurtAdmin.Controllers {
    public class VideoController : BaseController {
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
            Google.YouTube.Video ytVideo = new Google.YouTube.Video();
            ytVideo = VideoModel.GetYTVideo(ytID);
            FullVideo record = VideoModel.Create(ytVideo);

            return JsonConvert.SerializeObject(record);
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
