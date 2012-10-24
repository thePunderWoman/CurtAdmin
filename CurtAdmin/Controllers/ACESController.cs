/*
 * Author       : Alex Ninneman
 * Created      : January 20, 2011
 * Description  : This controller holds all of the page/AJAX functions for editing with vehicles in the CURT system.
 */

using System;
using System.Collections.Generic;
using System.Linq;
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

            List<vcdb_Make> makes = ACES.GetMakes();
            ViewBag.makes = makes;

            return View();
        }

        public string GetModels(int id) {
            List<vcdb_Model> models = ACES.GetModels(id);
            return JsonConvert.SerializeObject(models);
        }

        public string GetVehicles(int makeid, int modelid) {
            List<vcdb_Vehicle> vehicles = new List<vcdb_Vehicle>();
            vehicles = ACES.GetVehicles(makeid, modelid);
            return JsonConvert.SerializeObject(vehicles);
        }

        public string GetVCDBVehicles(int makeid, int modelid) {
            return "";
        }

        public void GenerateReport() {
            
            CurtDevDataContext db = new CurtDevDataContext();

            List<vcdb_VehiclePart> parts = db.vcdb_VehicleParts.AsParallel<vcdb_VehiclePart>().WithDegreeOfParallelism(12).ToList<vcdb_VehiclePart>();

            string name = ViewBag.name;

            XDocument report = new XDocument();

            XElement xdoc = new XElement("ACES",
                            new XAttribute("version","3.0"));

            XElement header = new XElement("Header",
                                new XElement("Company", "CURT Manufacturing"),
                                new XElement("SenderName", name),
                                new XElement("SenderPhone", "877-287-8634"),
                                new XElement("TransferDate", String.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                new XElement("MfrCode", "BKDK"),
                                new XElement("DocumentTitle", "Trailer Hitches"),
                                new XElement("EffectiveDate", String.Format("{0:yyyy-MM-dd}", DateTime.Now)),
                                new XElement("SubmissionType", "FULL"),
                                new XElement("VcdbVersionDate", "2012-08-31"),
                                new XElement("QdbVersionDate", "2012-08-22"),
                                new XElement("PcdbVersionDate", "2012-09-14"));
            xdoc.Add(header);

            XElement footer = new XElement("Footer",
                                new XElement("RecordCount", parts.Count()));
            int partcount = 1;

            foreach(vcdb_VehiclePart part in parts) {
                xdoc.Add(GetApp(part, partcount++));
            };
            xdoc.Add(footer);

            string attachment = "attachment; filename=report.xml";
            HttpContext.Response.Clear();
            HttpContext.Response.ClearHeaders();
            HttpContext.Response.ClearContent();
            HttpContext.Response.AddHeader("content-disposition", attachment);
            HttpContext.Response.ContentType = "text/xml";
            HttpContext.Response.AddHeader("Pragma", "public");
            HttpContext.Response.Write(xdoc.ToString());
            HttpContext.Response.End();
        }

        private XElement GetApp(vcdb_VehiclePart part, int partcount) {
            XElement application = new XElement("App",
                                    new XAttribute("action", "A"),
                                    new XAttribute("id", partcount),
                                    new XElement("BaseVehicle", 
                                        new XAttribute("id",part.vcdb_Vehicle.BaseVehicle.AAIABaseVehicleID)),
                                    new XElement("Part", part.PartNumber),
                                    new XElement("MfrLabel", part.Part.shortDesc),
                                    new XElement("Qty", 1),
                                    new XElement("PartType",
                                        new XAttribute("id",1212)));
            // add all the notes
            foreach (Note note in part.Notes) {
                XElement n = new XElement("Note", note.note1);
                application.Add(n);
            }

            // add submodel if necessary
            if (part.vcdb_Vehicle.SubModelID != null) {
                XElement submodel = new XElement("SubModel",
                                        new XAttribute("id", part.vcdb_Vehicle.Submodel.AAIASubmodelID));
                application.Add(submodel);
            }

            // add config if necessary
            if (part.vcdb_Vehicle.ConfigID != null) {
                foreach (VehicleConfigAttribute vattr in part.vcdb_Vehicle.VehicleConfig.VehicleConfigAttributes) {
                    if (vattr.ConfigAttribute.ConfigAttributeType.AcesTypeID != null) {
                        XElement attribute = new XElement(vattr.ConfigAttribute.ConfigAttributeType.AcesType.name,
                                                new XAttribute("id", vattr.ConfigAttribute.vcdbID));
                        application.Add(attribute);
                    } else {
                        XElement attribute = new XElement("Note", vattr.ConfigAttribute.value);
                        application.Add(attribute);
                    }
                }
            }
            return application;
        }

    }

    
}
