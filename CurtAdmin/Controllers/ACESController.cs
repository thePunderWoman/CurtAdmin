/*
 * Author       : Alex Ninneman
 * Created      : January 20, 2011
 * Description  : This controller holds all of the page/AJAX functions for editing with vehicles in the CURT system.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CurtAdmin.Models;
using Newtonsoft.Json;

namespace CurtAdmin.Controllers {
    public class ACESController : BaseController {

        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "ACES Vehicle Data";
        }

        public ActionResult Index() {

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

    }

    
}
