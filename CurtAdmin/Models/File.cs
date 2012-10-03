using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace CurtAdmin.Models {
    public class FileModel {

        public static List<FullFile> GetAll() {
            List<FullFile> images = new List<FullFile>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                images = (from f in db.Files
                          orderby f.name, f.createdDate descending
                          select new FullFile {
                              fileID = f.fileID,
                              name = f.name,
                              path = f.path,
                              height = f.height,
                              width = f.width,
                              size = f.size,
                              createdDate = f.createdDate,
                              fileGalleryID = f.fileGalleryID,
                              fileExtID = f.fileExtID,
                              extension = (from fe in db.FileExts
                                              where fe.fileExtID.Equals(f.fileExtID)
                                              select new FileExtension {
                                                  fileExtID = fe.fileExtID,
                                                  fileExt1 = fe.fileExt1,
                                                  fileExtIcon = fe.fileExtIcon,
                                                  fileTypeID = fe.fileTypeID,
                                                  FileType = db.FileTypes.Where(x => x.fileTypeID.Equals(fe.fileTypeID)).FirstOrDefault<FileType>()
                                              }).First<FileExtension>()
                          }).ToList<FullFile>();
            } catch (Exception e) { }

            return images;
        }

        public static List<FullFile> GetImagesByGallery(int fileGalleryID = 0) {
            List<FullFile> images = new List<FullFile>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                images = (from f in db.Files
                          where f.fileGalleryID.Equals(fileGalleryID)
                          orderby f.name, f.createdDate descending
                          select new FullFile {
                              fileID = f.fileID,
                              name = f.name,
                              path = f.path,
                              height = f.height,
                              width = f.width,
                              size = f.size,
                              createdDate = f.createdDate,
                              created = String.Format("{0:ddd, dd MMM yyyy HH:mm:ss}",f.createdDate),
                              fileGalleryID = f.fileGalleryID,
                              fileExtID = f.fileExtID,
                              extension = (from fe in db.FileExts
                                           where fe.fileExtID.Equals(f.fileExtID)
                                           select new FileExtension {
                                               fileExtID = fe.fileExtID,
                                               fileExt1 = fe.fileExt1,
                                               fileExtIcon = fe.fileExtIcon,
                                               fileTypeID = fe.fileTypeID,
                                               FileType = db.FileTypes.Where(x => x.fileTypeID.Equals(fe.fileTypeID)).FirstOrDefault<FileType>()
                                           }).First<FileExtension>()
                          }).ToList<FullFile>();
            } catch (Exception e) { }

            return images;
        }

        public static FullFile Get(int id = 0) {
            FullFile file = new FullFile();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                file = (from f in db.Files
                        where f.fileID.Equals(id)
                        orderby f.createdDate descending
                        select new FullFile {
                            fileID = f.fileID,
                            name = f.name,
                            path = f.path,
                            height = f.height,
                            width = f.width,
                            size = f.size,
                            createdDate = f.createdDate,
                            created = String.Format("{0:ddd, dd MMM yyyy HH:mm:ss}", f.createdDate),
                            fileGalleryID = f.fileGalleryID,
                            fileExtID = f.fileExtID,
                            extension = (from fe in db.FileExts
                                         where fe.fileExtID.Equals(f.fileExtID)
                                         select new FileExtension {
                                             fileExtID = fe.fileExtID,
                                             fileExt1 = fe.fileExt1,
                                             fileExtIcon = fe.fileExtIcon,
                                             fileTypeID = fe.fileTypeID,
                                             FileType = db.FileTypes.Where(x => x.fileTypeID.Equals(fe.fileTypeID)).FirstOrDefault<FileType>()
                                         }).First<FileExtension>()
                        }).FirstOrDefault<FullFile>();

            } catch (Exception e) { }
            return file;
        }

        public static void Delete(string serverpath, int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                File file = db.Files.Where(x => x.fileID.Equals(id)).FirstOrDefault<File>();
                // delete file on FTP
                try {
                    string filepath = file.path.Replace("http://docs.curthitch.biz/uploads/", "");
                    filepath = file.path.Replace("http://www.curtmfg.com/uploads/", "");
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverpath + "/webfiles/" + filepath);
                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                    request.Credentials = new NetworkCredential("eCommerceFTP", "3GaJPaAZ");
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                } catch { };
                db.Files.DeleteOnSubmit(file);
                db.SubmitChanges();
            } catch (Exception e) {
                throw e;
            }
        }

    }

    public class FullFile : File {
        public FileExtension extension { get; set; }
        public string created { get; set; }
    }
}