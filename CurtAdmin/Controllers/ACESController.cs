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

        /*Aces Type methods */

        public ActionResult AcesTypes() {
            List<AcesType> types = new ACES().GetACESTypes();
            ViewBag.types = types;
            return View();
        }

        public ActionResult AddACESType(string error = "") {
            ViewBag.error = error;
            return View("EditACESType");
        }

        public ActionResult EditACESType(int id = 0, string error = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            AcesType type = new ACES().GetACESType(id);
            ViewBag.type = type;
            ViewBag.error = error;
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
                if (id == 0) {
                    return RedirectToAction("AddACESType", new { error = error });
                } else {
                    return RedirectToAction("EditACESType", new { id = id, error = error });
                }
            }
            return RedirectToAction("AcesTypes");
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

        /* Configuration Type Methods */

        public ActionResult ConfigTypes() {
            List<ConfigAttributeType> types = new ACES().GetConfigTypes();
            ViewBag.types = types;
            return View();
        }

        public ActionResult AddConfigurationType(string error = "") {
            ViewBag.error = error;
            List<AcesType> acestypes = new ACES().GetACESTypes();
            ViewBag.acestypes = acestypes;

            return View("EditConfigurationType");
        }

        public ActionResult EditConfigurationType(int id = 0, string error = "") {
            ACES aces = new ACES();
            ConfigAttributeType type = aces.GetConfigType(id);
            ViewBag.type = type;

            List<AcesType> acestypes = aces.GetACESTypes();
            ViewBag.acestypes = acestypes;
            
            ViewBag.error = error;
            return View();
        }
        
        public ActionResult SaveConfigurationType(int id = 0, string name = null, int? acestypeid = null) {
            ConfigAttributeType type = new ConfigAttributeType();
            string error = "";
            try {
                type = new ACES().SaveConfigurationType(id, name, acestypeid);
            } catch (Exception e) {
                error = e.Message;
                if (id == 0) {
                    return RedirectToAction("AddConfigurationType", new { error = error });
                } else {
                    return RedirectToAction("EditConfigurationType", new { id = id, error = error });
                }
            }
            return RedirectToAction("ConfigTypes");
        }

        public ActionResult RemoveConfigurationType(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                if (db.ConfigAttributes.Where(x => x.ConfigAttributeTypeID.Equals(id)).Count() == 0) {
                    ConfigAttributeType t = db.ConfigAttributeTypes.Where(x => x.ID.Equals(id)).First<ConfigAttributeType>();
                    db.ConfigAttributeTypes.DeleteOnSubmit(t);
                    db.SubmitChanges();
                }
            } catch { }
            return RedirectToAction("ConfigTypes");
        }

        /* Configuration Attribute Methods */

        public ActionResult ConfigAttributes() {
            List<ConfigAttribute> attributes = new ACES().GetConfigAttributes();
            ViewBag.attributes = attributes;
            return View();
        }

        public ActionResult AddConfigurationAttribute(string error = "") {
            List<ConfigAttributeType> configtypes = new ACES().GetConfigTypes();
            ViewBag.configtypes = configtypes;
            ViewBag.error = error;
            return View("EditConfigurationAttribute");
        }

        public ActionResult EditConfigurationAttribute(int id = 0, string error = "") {
            ACES aces = new ACES();

            ConfigAttribute attribute = aces.GetConfigAttribute(id);
            ViewBag.attribute = attribute;

            List<ConfigAttributeType> configtypes = aces.GetConfigTypes();
            ViewBag.configtypes = configtypes;
            ViewBag.error = error;
    
            return View();
        }

        public ActionResult SaveConfigurationAttribute(int id = 0, string value = null, int configtypeid = 0) {
            ConfigAttribute attribute = new ConfigAttribute();
            string error = "";
            try {
                attribute = new ACES().SaveConfigurationAttr(id, value, configtypeid);
            } catch (Exception e) {
                error = e.Message;
                if (id == 0) {
                    return RedirectToAction("AddConfigurationAttribute", new { error = error });
                } else {
                    return RedirectToAction("EditConfigurationAttribute", new { id = id, error = error });
                }
            }
            return RedirectToAction("ConfigAttributes");
        }

        public ActionResult RemoveConfigurationAttribute(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                if (db.vcdb_Vehicles.Where(x => x.VehicleConfig.VehicleConfigAttributes.Any(y => y.AttributeID.Equals(id))).Count() == 0) {
                    ConfigAttribute attr = db.ConfigAttributes.Where(x => x.ID.Equals(id)).First<ConfigAttribute>();
                    db.ConfigAttributes.DeleteOnSubmit(attr);
                    db.SubmitChanges();
                }
            } catch { }
            return RedirectToAction("ConfigAttributes");
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
