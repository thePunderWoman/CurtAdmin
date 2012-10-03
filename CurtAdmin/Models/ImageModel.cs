using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using CurtAdmin.Models;
using System.IO;

namespace CurtAdmin.Models {
    public class ImageModel {

        public static List<ImageSize> GetPartImages(int partID = 0) {
            List<ImageSize> sizes = new List<ImageSize>();
            try {
                if (partID > 0) {
                    CurtDevDataContext db = new CurtDevDataContext();
                    sizes = (from p in db.PartImageSizes
                             select new ImageSize {
                                 sizeID = p.sizeID,
                                 size = p.size,
                                 images = db.PartImages.Where(x => x.partID.Equals(partID)).Where(x => x.sizeID.Equals(p.sizeID)).OrderBy(x => x.sort).ToList<PartImage>()
                             }).ToList<ImageSize>();
                }
            } catch { }
            return sizes;
        }

        public static string AddImage(int partID = 0, int sizeid = 0, string webfile = "", string localfile = "") {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                JavaScriptSerializer js = new JavaScriptSerializer();
                char sort = 'a';
                try {
                    sort = db.PartImages.Where(x => x.partID.Equals(partID)).Where(x => x.sizeID.Equals(sizeid)).OrderByDescending(x => x.sort).Select(x => x.sort).First<char>();
                    sort = GetNextLetter(sort.ToString());
                } catch {};
                System.Drawing.Image img = System.Drawing.Image.FromFile(localfile);
                PartImage image = new PartImage {
                    partID = partID,
                    sizeID = sizeid,
                    path = webfile,
                    height = img.Height,
                    width = img.Width,
                    sort = sort
                };
                img.Dispose();
                db.PartImages.InsertOnSubmit(image);
                db.SubmitChanges();
                return js.Serialize(image);
            } catch {
                return "error";
            }
        }

        public static void DeleteImage(int imageid = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            PartImage i = db.PartImages.Where(x => x.imageID.Equals(imageid)).First();
            List<string> ids = db.PartImages.Where(x => x.sizeID.Equals(i.sizeID)).Where(x => x.partID.Equals(i.partID)).Where(x => x.imageID != i.imageID).OrderBy(x => x.sort).Select(x => x.imageID.ToString()).ToList();
            db.PartImages.DeleteOnSubmit(i);
            db.SubmitChanges();
            UpdateSort(ids);
        }
        
        public static void UpdateSort(List<string> images) {
            CurtDevDataContext db = new CurtDevDataContext();
            char index = 'a';
            for (int i = 0; i < images.Count; i++ ) {
                PartImage p = db.PartImages.Where(x => x.imageID.Equals(Convert.ToInt32(images[i]))).First();
                if (i != 0) {
                    index = GetNextLetter(index.ToString());
                }
                p.sort = index;
                db.SubmitChanges();
            }
        }

        private static char GetNextLetter(string a) {
            return Convert.ToChar(Base26Sequence().SkipWhile(x => x != a).Skip(1).First());
        }

        private static IEnumerable<string> Base26Sequence() {
            long i = 0L;
            while (true)
                yield return Base26Encode(i++);
        }

        private static char[] base26Chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static string Base26Encode(Int64 value) {
            string returnValue = null;
            do {
                returnValue = base26Chars[value % 26] + returnValue;
                value /= 26;
            } while (value-- != 0);
            return returnValue;
        }
    }

    public class ImageSize : PartImageSize {
        public List<PartImage> images { get; set; }
    }
}