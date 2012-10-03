using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using CurtAdmin.Models;
using System.IO;
using Newtonsoft.Json;

namespace CurtAdmin.Models {
    public class PartVideoModel {

        public static List<PartVideo> GetPartVideos(int partID = 0) {
            List<PartVideo> videos = new List<PartVideo>();
            try {
                if (partID > 0) {
                    CurtDevDataContext db = new CurtDevDataContext();
                    videos = (from p in db.PartVideos
                              where p.partID.Equals(partID)
                              select p).ToList<PartVideo>();
                }
            } catch { }
            return videos;
        }

        public static PartVideo Get(int videoID = 0) {
            PartVideo video = new PartVideo();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                video = (from p in db.PartVideos
                         where p.pVideoID.Equals(videoID)
                            select p).First<PartVideo>();
            } catch { }
            return video;
        }

        public static string Save(int pVideoID = 0, int partID = 0, int vTypeID = 0, string video = "", bool isPrimary = false) {
            CurtDevDataContext db = new CurtDevDataContext();
            PartVideo v = new PartVideo();
            try {
                if(pVideoID > 0) {
                    v = db.PartVideos.Where(x => x.pVideoID == pVideoID).First<PartVideo>();
                    v.video = video;
                    v.isPrimary = isPrimary;
                    v.vTypeID = vTypeID;
                    v.partID = partID;
                    db.SubmitChanges();
                } else {
                    v = new PartVideo {
                        isPrimary = isPrimary,
                        vTypeID = vTypeID,
                        video = video,
                        partID = partID
                    };
                    db.PartVideos.InsertOnSubmit(v);
                    db.SubmitChanges();
                }
            } catch {
                return "error";
            }
            return JsonConvert.SerializeObject(Get(v.pVideoID));
        }

        public static void DeleteVideo(int videoID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            PartVideo v = db.PartVideos.Where(x => x.pVideoID.Equals(videoID)).First();
            db.PartVideos.DeleteOnSubmit(v);
            db.SubmitChanges();
        }
    }

}