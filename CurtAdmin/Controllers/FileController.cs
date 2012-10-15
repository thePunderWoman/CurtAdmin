using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;
using System.IO;
using System.Net;
using System.Text;

namespace CurtAdmin.Controllers {
    public class FileController : BaseController {

        public ActionResult Index() {
            List<Gallery> galleries = FileGalleryModel.GetAll();
            List<FullFile> files = FileModel.GetImagesByGallery(0);
            Gallery gallery = new Gallery {
                subgalleries = galleries,
                files = files,
                name = "Root",
                fileGalleryID = 0,
                parentID = -1,
                description = ""
            };

            List<FileGallery> breadcrumbs = new List<FileGallery>();
            ViewBag.gallery = gallery;
            ViewBag.breadcrumbs = breadcrumbs;
            return View("gallery");
        }

        public ActionResult Gallery(int id = 0) {
            Gallery gallery = FileGalleryModel.Get(id);
            ViewBag.gallery = gallery;

            List<FileGallery> breadcrumbs = FileGalleryModel.GetBreadcrumbs(id);
            ViewBag.breadcrumbs = breadcrumbs;
            return View();
        }

        public ActionResult CKIndex() {
            ViewBag.CKEditorFuncNum = Request.QueryString["CKEditorFuncNum"];
            ViewBag.CKEditor = Request.QueryString["CKEditor"];
            ViewBag.langCode = Request.QueryString["langCode"];

            List<Gallery> galleries = FileGalleryModel.GetAll();
            List<FullFile> files = FileModel.GetImagesByGallery(0);
            Gallery gallery = new Gallery {
                subgalleries = galleries,
                files = files,
                name = "Root",
                fileGalleryID = 0,
                parentID = -1,
                description = ""
            };

            List<FileGallery> breadcrumbs = new List<FileGallery>();
            ViewBag.gallery = gallery;
            ViewBag.breadcrumbs = breadcrumbs;
            return View("ckgallery");
        }

        public ActionResult CKGallery(int id = 0) {
            ViewBag.CKEditorFuncNum = Request.QueryString["CKEditorFuncNum"];
            ViewBag.CKEditor = Request.QueryString["CKEditor"];
            ViewBag.langCode = Request.QueryString["langCode"];

            Gallery gallery = FileGalleryModel.Get(id);
            ViewBag.gallery = gallery;

            List<FileGallery> breadcrumbs = FileGalleryModel.GetBreadcrumbs(id);
            ViewBag.breadcrumbs = breadcrumbs;
            return View();
        }

        public ActionResult FrameIndex() {
            List<Gallery> galleries = FileGalleryModel.GetAll();
            List<FullFile> files = FileModel.GetImagesByGallery(0);
            Gallery gallery = new Gallery {
                subgalleries = galleries,
                files = files,
                name = "Root",
                fileGalleryID = 0,
                parentID = -1,
                description = ""
            };

            List<FileGallery> breadcrumbs = new List<FileGallery>();
            ViewBag.gallery = gallery;
            ViewBag.breadcrumbs = breadcrumbs;
            return View("FrameGallery");
        }

        public ActionResult FrameGallery(int id = 0) {
            Gallery gallery = FileGalleryModel.Get(id);
            ViewBag.gallery = gallery;

            List<FileGallery> breadcrumbs = FileGalleryModel.GetBreadcrumbs(id);
            ViewBag.breadcrumbs = breadcrumbs;
            return View();
        }

        public string GetGalleryImagesJSON(int id = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<FullFile> images = FileModel.GetImagesByGallery(id);
            return js.Serialize(images);
        }


        public ActionResult CKUpload() {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string AddFile() {
            JavaScriptSerializer js = new JavaScriptSerializer();
            try {
                string filename = HttpContext.Request.Headers["X-File-Name"];
                bool preserve = Convert.ToBoolean(HttpContext.Request.Headers["X-Preserve-FileName"]);
                string galleryidstr = HttpContext.Request.Headers["X-Gallery-ID"];
                int galleryid = (galleryidstr == "") ? 0 : Convert.ToInt32(galleryidstr);
                int height = 0;
                int width = 0;
                int filesize = 0;
                string path = Path.Combine(Path.GetTempPath(), filename);
                FileStream uploadstream = System.IO.File.Create(path);
                Request.InputStream.CopyTo(uploadstream);
                uploadstream.Close();
                
                FileInfo fi = new FileInfo(path);
                filesize = Convert.ToInt32(fi.Length);
                string ext = Path.GetExtension(filename);

                FileExtension extension = FileExtModel.GetByExtension(ext);
                if (extension.fileExtID == 0) {
                    fi.Delete();
                    throw new Exception("File Type Does Not Exist.");
                }

                if (extension.FileType.fileType1.ToLower() == "image") {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(path);
                    height = img.Height;
                    width = img.Width;
                    img.Dispose();
                }

                string newfile = Guid.NewGuid() + ext;
                if (preserve) {
                    newfile = filename;
                }
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(GetServerPath() + "/webfiles/" + newfile);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.Credentials = new NetworkCredential("eCommerceFTP", "3GaJPaAZ");
                
                // Copy the contents of the file to the request stream.
                byte[] fileContents = new byte[fi.Length];
                FileStream filestream = fi.OpenRead();
                filestream.Read(fileContents, 0, fileContents.Length);
                filestream.Close();
                request.ContentLength = fileContents.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                response.Close();
                string newpath = "https://www.curtmfg.com/assets/" + newfile;

                File i = new File();
                
                CurtDevDataContext db = new CurtDevDataContext();
                try {
                    i = db.Files.Where(x => x.path == newpath).First<File>();
                    i.createdDate = DateTime.Now;
                    i.height = height;
                    i.width = width;
                    i.size = filesize;
                    db.SubmitChanges();
                } catch {
                    i = new File {
                        name = filename,
                        path = newpath,
                        createdDate = DateTime.Now,
                        height = height,
                        width = width,
                        size = filesize,
                        fileGalleryID = galleryid,
                        fileExtID = extension.fileExtID
                    };

                    db.Files.InsertOnSubmit(i);
                    db.SubmitChanges();
                }

                fi.Delete();

                FullFile f = FileModel.Get(i.fileID);

                return js.Serialize(f);

            } catch (Exception e) {
                return "error";
            }
        }

        public string check(int id = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            FullFile f = FileModel.Get(id);
            return js.Serialize(f);
        }

        public FileGallery AddGallery(string name = "", int parentid = 0) {
                CurtDevDataContext db = new CurtDevDataContext();
                FileGallery gallery = new FileGallery {
                    name = name,
                    description = "",
                    parentID = parentid
                };
                db.FileGalleries.InsertOnSubmit(gallery);
                db.SubmitChanges();
                return gallery;
        }

        public string AddGalleryAjax(string name = "", int parentid = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                JavaScriptSerializer js = new JavaScriptSerializer();
                FileGallery gallery = new FileGallery {
                    name = name,
                    description = "",
                    parentID = parentid
                };
                db.FileGalleries.InsertOnSubmit(gallery);
                db.SubmitChanges();
                return js.Serialize(gallery);
            } catch {
                return "";
            }
        }

        public string RenameAjax(string name = "", int galleryid = 0, int fileid = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                JavaScriptSerializer js = new JavaScriptSerializer();
                if (galleryid != 0) {
                    FileGallery gallery = db.FileGalleries.Where(x => x.fileGalleryID.Equals(galleryid)).FirstOrDefault<FileGallery>();
                    gallery.name = name;
                    db.SubmitChanges();
                    return js.Serialize(gallery);
                } else {
                    File file = db.Files.Where(x => x.fileID.Equals(fileid)).FirstOrDefault<File>();
                    file.name = name;
                    db.SubmitChanges();
                    return js.Serialize(file);
                }
            } catch {
                return "";
            }
        }

        public string RefreshFileAjax(int fileid = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            FullFile file = FileModel.Get(fileid);
            try {
                File f = db.Files.Where(x => x.fileID == fileid).First<File>();
                string localpath = Server.MapPath(file.path.Replace("http://www.curtmfg.com", ""));
                FileInfo fi = new FileInfo(localpath);
                if (file.extension.FileType.fileType1.ToLower() == "image") {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(localpath);
                    f.height = img.Height;
                    f.width = img.Width;
                    img.Dispose();
                }
                f.size = Convert.ToInt32(fi.Length);
                db.SubmitChanges();
                file = FileModel.Get(fileid);
            } catch {}
            return js.Serialize(file);
        }

        public ActionResult DeleteGallery(int id = 0) {
            int parentid = 0;
            try {
                parentid = FileGalleryModel.Delete(GetServerPath(), id);
            } catch { };
            return RedirectToAction("Gallery", new { id = parentid });

        }
        
        public string DeleteGalleryAjax(int id = 0) {
            try {
                FileGalleryModel.Delete(GetServerPath(), id);
                return "true";
            } catch {
                return "false";
            };
        }

        public string DeleteFileAjax(int id = 0) {
            try {
                FileModel.Delete(GetServerPath(), id);
                return "true";
            } catch {
                return "false";
            };
        }

        public string GetServerPath() {
            string serverpath = "ftp://10.10.90.82";
            if (Request.Url.Host == "localhost") {
                serverpath = "ftp://216.17.90.82";
            }
            return serverpath;
        }
    }
}
