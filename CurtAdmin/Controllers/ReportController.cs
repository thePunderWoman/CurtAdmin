/*
 * Author       : Alex Ninneman
 * Created      : January 20, 2011
 * Description  : This controller holds all of the page/AJAX functions for editing with vehicles in the CURT system.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using CurtAdmin.Models;
using Newtonsoft.Json;

namespace CurtAdmin.Controllers {
    public class ReportController : BaseController {

        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "Reports";
        }

        public ActionResult Index() {
            return View();
        }

        public void GenerateACES(int customerID = 1) {
            
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.pcdbDataContext pcdb = new AAIA.pcdbDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            AAIA.qdbDataContext qdb = new AAIA.qdbDataContext();
            string name = ViewBag.name;
            XDocument report = new XDocument(new XDeclaration("1.0", "utf-16", "yes"));
            List<int> parts = db.CustomerReportParts.Where(x => x.customerID.Equals(customerID)).Select(x => x.partID).ToList<int>();
            List<XElement> vparts = new List<XElement>();

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
                                new XElement("PcdbVersionDate", String.Format("{0:yyyy-MM-dd}", pcdb.PCDBVersions.Select(x => x.VersionDate).FirstOrDefault()))));
            if (parts.Count == 0) {
                vparts = (from vp in db.vcdb_VehicleParts
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
                          )).AsParallel<XElement>().WithDegreeOfParallelism(12).ToList<XElement>();
            } else {
                vparts = (from vp in db.vcdb_VehicleParts
                          join crp in db.CustomerReportParts on vp.PartNumber equals crp.partID
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
                          )).AsParallel<XElement>().WithDegreeOfParallelism(12).ToList<XElement>();
            }
            XElement footer = new XElement("Footer", new XElement("RecordCount", db.vcdb_VehicleParts.Count()));
            xdoc.Add(vparts);
            xdoc.Add(footer);
            report.Add(xdoc);
            
            StringWriter wr = new StringWriter();
            report.Save(wr);
            
            string attachment = "attachment; filename=ACESreport-" + String.Format("{0:yyyyMMddhhmmss}",DateTime.Now) + ".xml";
            HttpContext.Response.Clear();
            HttpContext.Response.ClearHeaders();
            HttpContext.Response.ClearContent();
            HttpContext.Response.AddHeader("content-disposition", attachment);
            HttpContext.Response.ContentType = "text/xml";
            HttpContext.Response.ContentEncoding = System.Text.Encoding.Unicode;
            HttpContext.Response.AddHeader("Pragma", "public");
            HttpContext.Response.Write(wr.GetStringBuilder().ToString());
            HttpContext.Response.End();
        }

        public void GeneratePIES(string startdate = "", int customerID = 1) {
            DateTime? start = null;
            try {
                start = Convert.ToDateTime(startdate);
            } catch { }
            XDocument xdoc = new XDocument();
            xdoc = new Report().generate(customerID, start);
            StringWriter wr = new StringWriter();
            xdoc.Save(wr);

            string attachment = "attachment; filename=PIESreport-" + String.Format("{0:yyyyMMddhhmmss}", DateTime.Now) + ".xml";
            HttpContext.Response.Clear();
            HttpContext.Response.ClearHeaders();
            HttpContext.Response.ClearContent();
            HttpContext.Response.AddHeader("content-disposition", attachment);
            HttpContext.Response.ContentType = "text/xml";
            HttpContext.Response.ContentEncoding = System.Text.Encoding.Unicode;
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
