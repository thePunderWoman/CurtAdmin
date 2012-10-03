using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CurtAdmin.Models {
    public class FileExtModel {

        /// <summary>
        /// Returns a List of all customers in the database, sorted by name ascending.
        /// </summary>
        /// <returns>List of Customer objects.</returns>
        public static List<FileExtension> GetAll() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<FileExtension> extensions = (from fe in db.FileExts
                                              orderby fe.fileExt1
                                              select new FileExtension {
                                                  fileExtID = fe.fileExtID,
                                                  fileExt1 = fe.fileExt1,
                                                  fileExtIcon = fe.fileExtIcon,
                                                  fileTypeID = fe.fileTypeID,
                                                  FileType = db.FileTypes.Where(x => x.fileTypeID.Equals(fe.fileTypeID)).FirstOrDefault<FileType>()
                                              }).ToList<FileExtension>();
            return extensions;
        }


        public static FileExt Get(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            FileExt extension = db.FileExts.Where(x => x.fileExtID == id).FirstOrDefault<FileExt>();
            return extension;
        }

        public static FileExtension GetByExtension(string ext = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            FileExtension extension = (from f in db.FileExts
                                       where f.fileExt1.ToLower() == ext.ToLower().Trim()
                                       select new FileExtension {
                                           fileExtID = f.fileExtID,
                                           fileExt1 = f.fileExt1,
                                           fileExtIcon = f.fileExtIcon,
                                           fileTypeID = f.fileTypeID,
                                           FileType = db.FileTypes.Where(x => x.fileTypeID.Equals(f.fileTypeID)).FirstOrDefault<FileType>()
                                       }).FirstOrDefault<FileExtension>();
            return extension;
        }

        public static bool Delete(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                FileExt extension = db.FileExts.Where(x => x.fileExtID == id).FirstOrDefault<FileExt>();
                db.FileExts.DeleteOnSubmit(extension);
                db.SubmitChanges();
                return true;
            } catch {
                return false;
            }
        }

    }

    public class FileExtension : FileExt {
        public FileType type { get; set; }
    }
}