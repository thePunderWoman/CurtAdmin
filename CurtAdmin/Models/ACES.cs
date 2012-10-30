using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CurtAdmin.Models {
    public class ACES {
        public List<vcdb_Make> GetMakes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<vcdb_Make> makes = new List<vcdb_Make>();
            makes = db.vcdb_Makes.OrderBy(x => x.MakeName).ToList<vcdb_Make>();
            return makes;
        }

        public List<vcdb_Model> GetModels(int makeid) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<vcdb_Model> models = new List<vcdb_Model>();
            models = (from v in db.vcdb_Vehicles
                      where v.BaseVehicle.MakeID.Equals(makeid)
                      select v.BaseVehicle.vcdb_Model).Distinct().OrderBy(x => x.ModelName).ToList<vcdb_Model>();
            return models;
        }

        public List<vcdb_Vehicle> GetVehicles(int makeid, int modelid) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<vcdb_Vehicle> vehicles = new List<vcdb_Vehicle>();
            vehicles = (from v in db.vcdb_Vehicles
                        where v.BaseVehicle.MakeID.Equals(makeid) && v.BaseVehicle.ModelID.Equals(modelid)
                        select v).Distinct().OrderBy(x => x.BaseVehicle.YearID).ThenBy(x => x.Submodel.SubmodelName).ToList<vcdb_Vehicle>();
            return vehicles;
        }

        public List<AAIA.BaseVehicle> GetVCDBVehicles(int makeid, int modelid) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            vcdb_Make make = db.vcdb_Makes.Where(x => x.ID.Equals(makeid)).First<vcdb_Make>();
            vcdb_Model model = db.vcdb_Models.Where(x => x.ID.Equals(modelid)).First<vcdb_Model>();

            List<AAIA.BaseVehicle> vehicles = new List<AAIA.BaseVehicle>();
            vehicles = (from bv in vcdb.BaseVehicles
                        where bv.MakeID.Equals(make.AAIAMakeID) && bv.ModelID.Equals(model.AAIAModelID)
                        orderby bv.YearID
                        select bv).ToList<AAIA.BaseVehicle>();
            return vehicles;
        }
        
        public List<AAIA.PartTerminology> GetPartTypes() {
            AAIA.pcdbDataContext db = new AAIA.pcdbDataContext();
            List<AAIA.PartTerminology> parttypes = new List<AAIA.PartTerminology>();
            parttypes = db.PartTerminologies.OrderBy(x => x.PartTerminologyName).ToList<AAIA.PartTerminology>();
            return parttypes;
        }

        public string SearchPartTypes(string keyword = "") {
            AAIA.pcdbDataContext db = new AAIA.pcdbDataContext();
            if (keyword != "" && keyword.Length > 2) {
                var types = (from p in db.PartTerminologies
                         where p.PartTerminologyName.Contains(keyword) || p.PartTerminologyID.ToString().Contains(keyword)
                         orderby p.PartTerminologyName
                         select new {
                             id = p.PartTerminologyID,
                             label = p.PartTerminologyName.Trim() + " - " + p.PartTerminologyID,
                             value = p.PartTerminologyID
                         }).ToList();
                return JsonConvert.SerializeObject(types);
            }
            return "[]";
        }

        public string GetPartTypeByID(int id = 0) {
            AAIA.pcdbDataContext db = new AAIA.pcdbDataContext();
            AAIA.PartTerminology typeobj = new AAIA.PartTerminology();
            try {
                typeobj = db.PartTerminologies.Where(x => x.PartTerminologyID.Equals(id)).First<AAIA.PartTerminology>();
            } catch {}
            return JsonConvert.SerializeObject(typeobj);
        }
    }

    public class ACESModel {
        public int ID { get; set; }
        public string name { get; set; }
    }

    public class ACESVehiclePart {
        public int ID { get; set; }
        public int vehicleID { get; set; }
        public int PartNumber { get; set; }
        public string PartDescription { get; set; }
        public int PartTypeID { get; set; }
        public List<Note> Notes { get; set; }
        public ACESVehicle Vehicle { get; set; }
    }

    public class ACESVehicle {
        public int ID { get; set; }
        public int BaseVehicleID { get; set; }
        public int? SubmodelID { get; set; }
        public int? ConfigID { get; set; }
        public ACESConfig Config { get; set; }
    }

    public class ACESConfig {
        public List<ACESAttribute> Notes { get; set; }
        public List<ACESAttribute> attributes { get; set; }
    }

    public class ACESAttribute {
        public string name { get; set; }
        public string value { get; set; }
    }
}