using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CurtAdmin.Models {
    public class FileTypeModel {

        /// <summary>
        /// Returns a List of all customers in the database, sorted by name ascending.
        /// </summary>
        /// <returns>List of Customer objects.</returns>
        public static List<FileType> GetAll() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<FileType> types = db.FileTypes.OrderBy(x => x.fileType1).ToList<FileType>();

            return types;
        }

        public static FileType Get(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            FileType type = db.FileTypes.Where(x => x.fileTypeID == id).FirstOrDefault<FileType>();
            return type;
        }

        public static bool Delete(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                int extensions = db.FileExts.Where(x => x.fileTypeID == id).Count();
                if (extensions == 0) {
                    FileType type = db.FileTypes.Where(x => x.fileTypeID == id).FirstOrDefault<FileType>();
                    db.FileTypes.DeleteOnSubmit(type);
                    db.SubmitChanges();
                } else {
                    throw new Exception("File Type has many extensions. Delete those first.");
                }
                return true;
            } catch {
                return false;
            }
        }

    }

}