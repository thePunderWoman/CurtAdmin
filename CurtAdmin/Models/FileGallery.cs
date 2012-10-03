using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models {
    public class FileGalleryModel {

        public static List<Gallery> GetAll() {
            List<Gallery> galleries = new List<Gallery>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                galleries = GetSubGalleries(0);
            } catch (Exception e) { }

            return galleries;
        }

        private static List<Gallery> GetSubGalleries(int parentID = 0) {
            List<Gallery> galleries = new List<Gallery>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                galleries = (from IG in db.FileGalleries
                             where IG.parentID.Equals(parentID)
                             orderby IG.name
                             select new Gallery {
                                 fileGalleryID = IG.fileGalleryID,
                                 name = IG.name,
                                 description = IG.description,
                                 parentID = IG.parentID,
                                 files = FileModel.GetImagesByGallery(IG.fileGalleryID),
                                 subgalleries = GetSubGalleries(IG.fileGalleryID)
                             }).ToList<Gallery>();
            } catch (Exception e) { }

            return galleries;
        }
        
        public static Gallery Get(int id = 0) {
            Gallery gallery = new Gallery();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                gallery = (from IG in db.FileGalleries
                           where IG.fileGalleryID.Equals(id)
                           orderby IG.name
                           select new Gallery {
                               fileGalleryID = IG.fileGalleryID,
                               name = IG.name,
                               description = IG.description,
                               parentID = IG.parentID,
                               files = FileModel.GetImagesByGallery(IG.fileGalleryID),
                               subgalleries = GetSubGalleries(IG.fileGalleryID)
                           }).First<Gallery>();

            } catch (Exception e) { }
            return gallery;
        }

        public static List<FileGallery> GetBreadcrumbs(int id = 0) {
            List<FileGallery> breadcrumbs = new List<FileGallery>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                FileGallery gallery = db.FileGalleries.Where(x => x.fileGalleryID == id).First();
                int parentid = gallery.parentID;

                while (parentid != 0) {
                    gallery = db.FileGalleries.Where(x => x.fileGalleryID == parentid).First();
                    breadcrumbs.Add(gallery);
                    parentid = gallery.parentID;
                }

            } catch { };
            return breadcrumbs;
        }

        public static int Delete(string serverpath, int id = 0) {
            int parentid = 0;
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                FileGallery ig = db.FileGalleries.Where(x => x.fileGalleryID == id).FirstOrDefault<FileGallery>();
                parentid = ig.parentID;
                List<File> images = db.Files.Where(x => x.fileGalleryID.Equals(ig.fileGalleryID)).ToList<File>();
                foreach (File image in images) {
                    FileModel.Delete(serverpath, image.fileID);
                }
                List<FileGallery> igs = db.FileGalleries.Where(x => x.parentID == ig.fileGalleryID).ToList<FileGallery>();
                foreach (FileGallery i in igs) {
                    Delete(serverpath, i.fileGalleryID);
                }
                db.FileGalleries.DeleteOnSubmit(ig);
                db.SubmitChanges();
            } catch (Exception e) {
                throw e;
            }
            return parentid;
        }

    }

    public class Gallery : FileGallery {
        public List<FullFile> files { get; set; }
        public List<Gallery> subgalleries { get; set; }
    }
}