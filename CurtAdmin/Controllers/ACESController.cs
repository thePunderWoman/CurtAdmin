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

        public ActionResult Vehicles() {
            List<vcdb_Make> makes = new ACES().GetMakes();
            ViewBag.makes = makes;
            return View();
        }

        public ActionResult AcesTypes() {
            List<AcesType> types = new ACES().GetACESTypes();
            ViewBag.types = types;
            return View();
        }

        public ActionResult ConfigTypes() {
            List<ConfigAttributeType> types = new ACES().GetConfigTypes();
            ViewBag.types = types;
            return View();
        }

        public ActionResult ConfigAttributes() {
            List<ConfigAttribute> attributes = new ACES().GetConfigAttributes();
            ViewBag.attributes = attributes;
            return View();
        }

        public ActionResult SaveACESType(int id = 0, string name = null) {
            CurtDevDataContext db = new CurtDevDataContext();
            AcesType type = new AcesType();
            string error = "";
            try {
                type = new ACES().SaveACESType(id, name);
            } catch (Exception e) {
                error = e.Message;
            }
            if (type != null && id != type.ID) {
                return RedirectToAction("SaveACESType", new { id = type.ID });
            }
            ViewBag.type = type;
            ViewBag.error = error;
            return View();
        }

        public ActionResult RemoveACESType(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                AcesType t = db.AcesTypes.Where(x => x.ID.Equals(id)).First<AcesType>();
                db.AcesTypes.DeleteOnSubmit(t);
                db.SubmitChanges();
            } catch { }
            return RedirectToAction("AcesTypes");
        }
        
        public string GetModels(int id) {
            List<vcdb_Model> models = new ACES().GetModels(id);
            return JsonConvert.SerializeObject(models);
        }

        public string GetVehicles(int makeid, int modelid) {
            List<BaseVehicle> vehicles = new List<BaseVehicle>();
            vehicles = new ACES().GetVehicles(makeid, modelid);
            return JsonConvert.SerializeObject(vehicles);
        }

        public string GetVCDBVehicles(int makeid, int modelid) {
            List<ACESBaseVehicle> vehicles = new List<ACESBaseVehicle>();
            vehicles = new ACES().GetVCDBVehicles(makeid, modelid);
            return JsonConvert.SerializeObject(vehicles);
        }

        public string SearchPartTypes(string keyword = "") {
            return new ACES().SearchPartTypes(keyword);
        }

        public string GetPartTypeByID(int id = 0) {
            return new ACES().GetPartTypeByID(id);
        }
    }

    
}
