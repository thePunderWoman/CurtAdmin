using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Linq;

namespace CurtAdmin.Controllers
{
    public class MiscController : BaseController
    {

        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "Miscellaneous";
        }


        /// <summary>
        /// Load all of the categories in the database so the user can make actions on them.
        /// </summary>
        /// <returns>View of categories</returns>
        public ActionResult Index() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ContentType> types = new List<ContentType>();
            types = db.ContentTypes.OrderBy(x => x.type).ToList<ContentType>();
            ViewBag.types = types;
            return View();
        }

        public string DeleteContentType(int cTypeID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            int count = db.Contents.Where(x => x.cTypeID.Equals(cTypeID)).Count();
            if (count == 0) {
                try {
                    ContentType c = db.ContentTypes.Where(x => x.cTypeID.Equals(cTypeID)).First<ContentType>();
                    db.ContentTypes.DeleteOnSubmit(c);
                    db.SubmitChanges();
                    return "";
                } catch { };
            }
            return "false";
        }

        public string GetContentType(int cTypeID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Newtonsoft.Json.Formatting format = Newtonsoft.Json.Formatting.None;
            ContentType c = new ContentType();
            try {
                c = db.ContentTypes.Where(x => x.cTypeID.Equals(cTypeID)).First<ContentType>();
            } catch { };
            return JsonConvert.SerializeObject(c,format,settings);
        }

        public string UpdateContentType(int cTypeID = 0, bool allowHTML = false) {
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            try {
                ContentType c = db.ContentTypes.Where(x => x.cTypeID.Equals(cTypeID)).First<ContentType>();
                c.allowHTML = allowHTML;
                db.SubmitChanges();
            } catch { };
            ContentType ctype = db.ContentTypes.Where(x => x.cTypeID.Equals(cTypeID)).First<ContentType>();
            return js.Serialize(ctype);
        }

        public string AddContentType(string type = "", bool allowHTML = false) {
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            ContentType c = new ContentType();
            ContentType exists = db.ContentTypes.Where(x => x.type.Equals(type)).FirstOrDefault<ContentType>();
            if (exists == null) {
                c = new ContentType {
                    type = type,
                    allowHTML = allowHTML
                };
                db.ContentTypes.InsertOnSubmit(c);
                db.SubmitChanges();
            }
            return js.Serialize(c);
        }

        public ActionResult VideoTypes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<videoType> types = new List<videoType>();
            types = db.videoTypes.OrderBy(x => x.name).ToList<videoType>();
            ViewBag.types = types;
            return View();
        }

        public string AddVideoType(string name = "", string icon = null) {
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            videoType v = new videoType();
            videoType exists = db.videoTypes.Where(x => x.name.Equals(name)).FirstOrDefault<videoType>();
            if (exists == null) {
                v = new videoType {
                    name = name,
                    icon = (icon.Trim() != "") ? icon : null
                };
                db.videoTypes.InsertOnSubmit(v);
                db.SubmitChanges();
            }
            return js.Serialize(v);
        }

        public string GetVideoType(int vTypeID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            videoType v = new videoType();
            try {
                v = db.videoTypes.Where(x => x.vTypeID.Equals(vTypeID)).First<videoType>();
            } catch { };
            return js.Serialize(v);
        }

        public string UpdateVideoType(int vTypeID = 0, string icon = null) {
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            try {
                videoType v = db.videoTypes.Where(x => x.vTypeID.Equals(vTypeID)).First<videoType>();
                v.icon = (icon.Trim() != "") ? icon : null;
                db.SubmitChanges();
            } catch { };
            videoType vtype = db.videoTypes.Where(x => x.vTypeID.Equals(vTypeID)).First<videoType>();
            return js.Serialize(vtype);
        }

        public string DeleteVideoType(int vTypeID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            int count = db.PartVideos.Where(x => x.vTypeID.Equals(vTypeID)).Count();
            if (count == 0) {
                try {
                    videoType v = db.videoTypes.Where(x => x.vTypeID.Equals(vTypeID)).First<videoType>();
                    db.videoTypes.DeleteOnSubmit(v);
                    db.SubmitChanges();
                    return "";
                } catch { };
            }
            return "false";
        }

        public ActionResult UnitsOfMeasure() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<UnitOfMeasure> measures = db.UnitOfMeasures.OrderBy(x => x.name).ToList<UnitOfMeasure>();
            ViewBag.measures = measures;
            return View();
        }

        public string GetUnit(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            UnitOfMeasure unit = new UnitOfMeasure();
            try {
                unit = db.UnitOfMeasures.Where(x => x.ID.Equals(id)).First<UnitOfMeasure>();
            } catch { };
            return JsonConvert.SerializeObject(unit);
        }

        public string AddUnit(string name = "", string code = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            UnitOfMeasure unit = new UnitOfMeasure();
            UnitOfMeasure exists = db.UnitOfMeasures.Where(x => x.name.Equals(name) || x.code.Equals(code)).FirstOrDefault<UnitOfMeasure>();
            if (exists == null) {
                unit = new UnitOfMeasure {
                    name = name,
                    code = code
                };
                db.UnitOfMeasures.InsertOnSubmit(unit);
                db.SubmitChanges();
            }
            return JsonConvert.SerializeObject(unit);
        }

        public string UpdateUnit(int id = 0, string name = "", string code = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            UnitOfMeasure unit = new UnitOfMeasure();
            unit = db.UnitOfMeasures.Where(x => x.ID.Equals(id)).FirstOrDefault<UnitOfMeasure>();
            if (unit != null) {
                unit.name = name;
                unit.code = code;
                db.SubmitChanges();
            }
            return JsonConvert.SerializeObject(unit);
        }

        public string DeleteUnit(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            int count = db.PartPackages.Where(x => x.dimensionUOM.Equals(id) || x.packageUOM.Equals(id) || x.weightUOM.Equals(id)).Count();
            if (count == 0) {
                try {
                    UnitOfMeasure unit = db.UnitOfMeasures.Where(x => x.ID.Equals(id)).First<UnitOfMeasure>();
                    db.UnitOfMeasures.DeleteOnSubmit(unit);
                    db.SubmitChanges();
                    return "";
                } catch { };
            }
            return "false";
        }

        public ActionResult ImportPolygons(HttpPostedFileBase file) {
            // Verify that the user selected a file
            List<string> errors = new List<string>();
            if (file != null && file.ContentLength > 0) {
                CurtDevDataContext db = new CurtDevDataContext();
                XDocument xdoc = XDocument.Load(file.InputStream);
                foreach (XElement placemark in xdoc.Descendants("Placemark")) {
                    string name = placemark.Element("name").Value.ToString();
                    try {
                        PartStates state = db.PartStates.Where(x => x.state1.ToUpper().Trim().Equals(name.Trim().ToUpper())).First<PartStates>();
                        MapPolygon polygon = new MapPolygon {
                            stateID = state.stateID
                        };
                        db.MapPolygons.InsertOnSubmit(polygon);
                        db.SubmitChanges();

                        List<MapPolygonCoordinate> coordinates = new List<MapPolygonCoordinate>();
                        foreach (XElement coordinate in placemark.Descendants("coordinate")) {
                            MapPolygonCoordinate coord = new MapPolygonCoordinate {
                                MapPolygonID = polygon.ID,
                                latitude = Convert.ToDouble(coordinate.Element("latitude").Value.ToString()),
                                longitude = Convert.ToDouble(coordinate.Element("longitude").Value.ToString())
                            };
                            coordinates.Add(coord);
                        }
                        db.MapPolygonCoordinates.InsertAllOnSubmit(coordinates);
                        db.SubmitChanges();
                    } catch {
                        string e = name + " couldn't be found.";
                        if (!errors.Contains(e)) {
                            errors.Add(e);
                        }
                    }
                }
                int count = (from x in xdoc.Descendants("Placemark")
                             select x).Count();
                ViewBag.message = "file uploaded successfully, " + count + " Placemarks in file.";
                ViewBag.errors = errors;
            }

            return View();
        }

        public ActionResult ImageImport() {
            ImportService importservice = new ImportService();
            ImportProcess status = importservice.checkStatus();
            ViewBag.status = status;
            return View();
        }

        public ActionResult APIAnalytics(int page = 1, int perpage = 100) {

            CurtDevDataContext db = new CurtDevDataContext();
            List<APIAnalytic> data = new APIAnalytic().GetAnalytics(page,perpage);
            new IPtoDNS().CheckAddresses();

            ViewBag.data = data;
            ViewBag.pagenum = page;
            ViewBag.perpage = perpage;

            return View();
        }

    }
}
