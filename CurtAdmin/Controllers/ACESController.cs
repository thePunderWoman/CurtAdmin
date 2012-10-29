/*
 * Author       : Alex Ninneman
 * Created      : January 20, 2011
 * Description  : This controller holds all of the page/AJAX functions for editing with vehicles in the CURT system.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using CurtAdmin.Models;
using Newtonsoft.Json;

namespace CurtAdmin.Controllers {
    public class ACESController : BaseController {

        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "ACES Vehicle Data";
        }

        public ActionResult Index() {
            return View();
        }
        
        public ActionResult Vehicles() {

            List<vcdb_Make> makes = new ACES().GetMakes();
            ViewBag.makes = makes;

            return View();
        }

        public string GetModels(int id) {
            List<vcdb_Model> models = new ACES().GetModels(id);
            return JsonConvert.SerializeObject(models);
        }

        public string GetVehicles(int makeid, int modelid) {
            List<vcdb_Vehicle> vehicles = new List<vcdb_Vehicle>();
            vehicles = new ACES().GetVehicles(makeid, modelid);
            return JsonConvert.SerializeObject(vehicles);
        }

        public string GetVCDBVehicles(int makeid, int modelid) {
            return "";
        }

        public void GenerateReport() {
            
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.pcdbDataContext pcdb = new AAIA.pcdbDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            AAIA.qdbDataContext qdb = new AAIA.qdbDataContext();
            string name = ViewBag.name;
            XDocument report = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

            XElement xdoc = new XElement("ACES",
                            new XAttribute("version", "3.0"),
                            new XElement("Header",
                                new XElement("Company", "CURT Manufacturing"),
                                new XElement("SenderName", name),
                                new XElement("SenderPhone", "877-287-8634"),
                                new XElement("TransferDate", String.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                new XElement("MfrCode", "BKDK"),
                                new XElement("DocumentTitle", "Trailer Hitches"),
                                new XElement("EffectiveDate", String.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                new XElement("SubmissionType", "FULL"),
                                new XElement("VcdbVersionDate", String.Format("{0:yyyy-MM-dd}", vcdb.VCDBVersions.Select(x => x.VersionDate).FirstOrDefault())),
                                new XElement("QdbVersionDate", String.Format("{0:yyyy-MM-dd}", qdb.QDBVersions.Select(x => x.VersionDate).FirstOrDefault())),
                                new XElement("PcdbVersionDate", String.Format("{0:yyyy-MM-dd}", pcdb.PCDBVersions.Select(x => x.VersionDate).FirstOrDefault()))),
                            (from vp in db.vcdb_VehicleParts
                             select new XElement("App",
                                 new XAttribute("action", "A"),
                                 new XAttribute("id", vp.ID),
                                 new XElement("BaseVehicle",
                                     new XAttribute("id", vp.vcdb_Vehicle.BaseVehicle.AAIABaseVehicleID)),
                                 new XElement("Part", vp.PartNumber),
                                 new XElement("MfrLabel", vp.Part.shortDesc),
                                 new XElement("Qty", 1),
                                 new XElement("PartType",
                                     new XAttribute("id", vp.Part.ACESPartTypeID)),
                                 (from n in vp.Notes
                                  select new XElement("Note", n.note1)
                                     ).ToList<XElement>(),
                                 ((vp.vcdb_Vehicle.SubModelID != null) ? new XElement("SubModel", new XAttribute("id", vp.vcdb_Vehicle.Submodel.AAIASubmodelID)) : null),
                                 ((vp.vcdb_Vehicle.ConfigID != null) ? (from ca in vp.vcdb_Vehicle.VehicleConfig.VehicleConfigAttributes
                                                                        select new XElement(((ca.ConfigAttribute.vcdbID == null) ? "Note" : ca.ConfigAttribute.ConfigAttributeType.AcesType.name), ((ca.ConfigAttribute.vcdbID == null) ? ca.ConfigAttribute.value : null),
                                                                            ((ca.ConfigAttribute.vcdbID == null) ? null : new XAttribute("id", ca.ConfigAttribute.vcdbID.ToString())))).ToList<XElement>() : null)
                             )).AsParallel<XElement>().WithDegreeOfParallelism(12),
                            new XElement("Footer", new XElement("RecordCount", db.vcdb_VehicleParts.Count())));
            report.Add(xdoc);
            
            StringWriter wr = new StringWriter();
            report.Save(wr);

            string attachment = "attachment; filename=ACESreport-" + String.Format("{0:yyyyMMddhhmmss}",DateTime.Now) + ".xml";
            HttpContext.Response.Clear();
            HttpContext.Response.ClearHeaders();
            HttpContext.Response.ClearContent();
            HttpContext.Response.AddHeader("content-disposition", attachment);
            HttpContext.Response.ContentType = "text/xml";
            HttpContext.Response.AddHeader("Pragma", "public");
            HttpContext.Response.Write(wr.GetStringBuilder().ToString());
            HttpContext.Response.End();
        }

        public string SearchPartTypes(string keyword = "") {
            return new ACES().SearchPartTypes(keyword);
        }

        public string GetPartTypeByID(int id = 0) {
            return new ACES().GetPartTypeByID(id);
        }
    }

    
}
