using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.IO;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CurtAdmin.Controllers {
    public class EditorController : BaseController {
        //
        // GET: /Editor/

        private string[] allowed_filetypes = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        public ActionResult Index() {
            ViewBag.CKEditorFuncNum = Request.QueryString["CKEditorFuncNum"];
            ViewBag.CKEditor = Request.QueryString["CKEditor"];
            ViewBag.langCode = Request.QueryString["langCode"];

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://216.17.90.82/webfiles");
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential("eCommerceFTP", "3GaJPaAZ");

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.ASCII);
            string Datastring = sr.ReadToEnd();
            response.Close();

            string[] fileNames = Regex.Split(Datastring, "\r\n");
            List<string> images = new List<string>();
            foreach (string filename in fileNames) {
                string image = filename;
                images.Add(image);
            }
            ViewBag.images = images;
            return View();
        }

        public ActionResult UploadForm() {

            return View();
        }

        public string GetImagesJSON() {
            var dirPath = Path.Combine(Server.MapPath("/Content/img/uploaded"));
            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }

            string[] fileNames = Directory.GetFiles(dirPath);
            List<object> images = new List<object>();
            List<FileInfo> imagefiles = new List<FileInfo>();
            foreach (string filename in fileNames) {
                FileInfo image = new FileInfo(filename);
                imagefiles.Add(image);
            }
            // Show recently uploaded images first
            imagefiles.Sort((x,y) => y.LastWriteTime.CompareTo(x.LastWriteTime));

            foreach (FileInfo imagefile in imagefiles) {
                var imageobj = new {
                    fullname = "http://" + Request.Url.Host + ((Request.Url.Host.Contains("localhost")) ? ":" + Request.Url.Port.ToString() : "") + "/Content/img/uploaded/" + imagefile.Name,
                    filename = imagefile.Name,
                    created = String.Format("{0:M/d/yyyy h:mm tt}",imagefile.LastWriteTime)
                };
                images.Add(imageobj);
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(images);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImageUpload(HttpPostedFileBase upload, string CKEditorFuncNum, string CKEditor, string langCode) {
            string url; // url to return
            string message; // message to display

            // path of the image
            string path = "/Content/img/uploaded/" + upload.FileName;

            url = Request.Url.GetLeftPart(UriPartial.Authority) + path;


            try {
                if (upload != null && upload.ContentLength > 0) {
                    try {

                        string file_path = Path.Combine(HttpContext.Server.MapPath("/Content/img/uploaded"), Path.GetFileName(upload.FileName));

                        // Make sure the file type is allowed
                        string file_ext = Path.GetExtension(file_path);
                        if (!allowed_filetypes.Contains(file_ext.ToLower())) {
                            throw new Exception("File type (" + file_ext + ") not allowed.");
                        }

                        // Save new image
                        upload.SaveAs(file_path);
                        string newfile = Guid.NewGuid() + Path.GetExtension(upload.FileName);

                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://216.17.90.82/webfiles/" + newfile);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.Credentials = new NetworkCredential("eCommerceFTP", "3GaJPaAZ");
                        request.UsePassive = true;

                        // Copy the contents of the file to the request stream.
                        StreamReader sourceStream = new StreamReader(file_path);
                        byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                        sourceStream.Close();
                        request.ContentLength = fileContents.Length;

                        Stream requestStream = request.GetRequestStream();
                        requestStream.Write(fileContents, 0, fileContents.Length);
                        requestStream.Close();

                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                        response.Close();

                        FileInfo fi = new FileInfo(file_path);
                        fi.Delete();
                        
                        message = "File uploaded.";
                    } catch (Exception e) {
                        throw e;
                    }
                } else {
                    throw new Exception("Invalid file data.");
                }
            } catch (Exception e) {
                message = e.Message;
            }

            string output = @"<html><body><script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + url + "\",\"" + message + "\");</script></body></html>";
            return Content(output);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadNoEditor(HttpPostedFileBase upload) {
            string url; // url to return
            string message; // message to display

            // path of the image
            string path = "/Content/img/uploaded/" + upload.FileName;

            url = Request.Url.GetLeftPart(UriPartial.Authority) + path;


            try {
                if (upload != null && upload.ContentLength > 0) {
                    try {

                        string file_path = Path.Combine(HttpContext.Server.MapPath("/Content/img/uploaded"), Path.GetFileName(upload.FileName));

                        // Make sure the file type is allowed
                        string file_ext = Path.GetExtension(file_path);
                        if (!allowed_filetypes.Contains(file_ext.ToLower())) {
                            throw new Exception("File type (" + file_ext + ") not allowed.");
                        }

                        // Save new image
                        upload.SaveAs(file_path);
                        string newfile = Guid.NewGuid() + Path.GetExtension(upload.FileName);
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://216.17.90.82/webfiles/" + newfile);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.Credentials = new NetworkCredential("eCommerceFTP", "3GaJPaAZ");

                        // Copy the contents of the file to the request stream.
                        StreamReader sourceStream = new StreamReader(file_path);
                        byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                        sourceStream.Close();
                        request.ContentLength = fileContents.Length;

                        Stream requestStream = request.GetRequestStream();
                        requestStream.Write(fileContents, 0, fileContents.Length);
                        requestStream.Close();

                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                        response.Close();

                        FileInfo fi = new FileInfo(file_path);
                        fi.Delete();
                        
                        string fullname = "http://www.curtmfg.com/uploads/" + newfile;

                        string output = @"<html><body><script>window.parent.selectUploadedPhoto('" + fullname + "');</script></body></html>";
                        return Content(output);
                    } catch (Exception e) {
                        throw e;
                    }
                } else {
                    throw new Exception("Invalid file data.");
                }
            } catch (Exception e) {
                string output = @"<html><body><script>window.parent.uploadError('" + e.Message + "');</script></body></html>";
                return Content(output);
            }
        }
    }
}
