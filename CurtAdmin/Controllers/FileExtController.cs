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
    public class FileExtController : BaseController {

        public ActionResult Index() {
            ViewBag.extensions = FileExtModel.GetAll();
            return View();
        }

        public ActionResult Types() {
            ViewBag.types = FileTypeModel.GetAll();
            return View();
        }

        public ActionResult AddType() {
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();

            #region Form Submission
            try {
                if (Request.Form.Count > 0) {


                    // Save form values
                    string fileType = (Request.Form["fileType"] != null) ? Request.Form["fileType"] : "";

                    // Validate the form fields
                    if (fileType.Length == 0) throw new Exception("Name is required.");

                    // Create the new customer and save
                    FileType new_type = new FileType {
                        fileType1 = fileType
                    };

                    db.FileTypes.InsertOnSubmit(new_type);
                    db.SubmitChanges();
                    return RedirectToAction("Types");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.error = error;
            return View();
        }

        public ActionResult EditType(int id = 0) {
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();

            #region Form Submission
            try {
                if (Request.Form.Count > 0) {


                    // Save form values
                    string fileType = (Request.Form["fileType"] != null) ? Request.Form["fileType"] : "";

                    // Validate the form fields
                    if (fileType.Length == 0) throw new Exception("Name is required.");

                    // Create the new customer and save
                    FileType mod_type = db.FileTypes.Where(x => x.fileTypeID == id).FirstOrDefault<FileType>();

                    mod_type.fileType1 = fileType;
                    db.SubmitChanges();

                    return RedirectToAction("Types");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.type = FileTypeModel.Get(id);

            ViewBag.error = error;

            return View();
        }

        public ActionResult AddExtension() {
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();

            #region Form Submission
            try {
                if (Request.Form.Count > 0) {


                    // Save form values
                    string extension = (Request.Form["extension"] != null) ? Request.Form["extension"] : "";
                    string icon = (Request.Form["icon"] != null) ? Request.Form["icon"] : "";
                    int typeid = (Request.Form["typeid"] != null) ? Convert.ToInt32(Request.Form["typeid"]) : 0;

                    // Validate the form fields
                    if (extension.Length == 0) throw new Exception("Name is required.");
                    if (typeid == 0) throw new Exception("A File Type is required.");

                    // Create the new customer and save
                    FileExt new_ext = new FileExt {
                        fileExt1 = extension,
                        fileExtIcon = icon,
                        fileTypeID = typeid
                    };


                    db.FileExts.InsertOnSubmit(new_ext);
                    db.SubmitChanges();

                    return RedirectToAction("Index");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.error = error;
            ViewBag.types = FileTypeModel.GetAll();
            return View();
        }

        public ActionResult EditExtension(int id = 0) {
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();

            #region Form Submission
            try {
                if (Request.Form.Count > 0) {


                    // Save form values
                    string extension = (Request.Form["extension"] != null) ? Request.Form["extension"] : "";
                    string icon = (Request.Form["icon"] != null) ? Request.Form["icon"] : "";
                    int typeid = (Request.Form["typeid"] != null) ? Convert.ToInt32(Request.Form["typeid"]) : 0;

                    // Validate the form fields
                    if (extension.Length == 0) throw new Exception("Name is required.");
                    if (typeid == 0) throw new Exception("A File Type is required.");

                    // Create the new customer and save
                    FileExt mod_ext = db.FileExts.Where(x => x.fileExtID == id).FirstOrDefault<FileExt>();

                    mod_ext.fileExt1 = extension;
                    mod_ext.fileExtIcon = icon;
                    mod_ext.fileTypeID = typeid;

                    db.SubmitChanges();

                    return RedirectToAction("Index");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.extension = FileExtModel.Get(id);
            ViewBag.types = FileTypeModel.GetAll();

            ViewBag.error = error;

            return View();
        }

        public ActionResult DeleteExtension(int id = 0) {
            try {
                FileExtModel.Delete(id);
            } catch {}
            return RedirectToAction("Index");
        }

        public ActionResult DeleteType(int id = 0) {
            try {
                bool success = FileTypeModel.Delete(id);
            } catch { }
            return RedirectToAction("Types");
        }
    }
}
