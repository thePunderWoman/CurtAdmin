using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models {
    public class ACES {
        public static List<vcdb_Make> GetMakes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<vcdb_Make> makes = new List<vcdb_Make>();
            makes = db.vcdb_Makes.OrderBy(x => x.MakeName).ToList<vcdb_Make>();
            return makes;
        }

        public static List<vcdb_Model> GetModels(int makeid) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<vcdb_Model> models = new List<vcdb_Model>();
            models = (from v in db.vcdb_Vehicles
                      where v.BaseVehicle.MakeID.Equals(makeid)
                      select v.BaseVehicle.vcdb_Model).Distinct().OrderBy(x => x.ModelName).ToList<vcdb_Model>();
            return models;
        }

        public static List<vcdb_Vehicle> GetVehicles(int makeid, int modelid) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<vcdb_Vehicle> vehicles = new List<vcdb_Vehicle>();
            vehicles = (from v in db.vcdb_Vehicles
                        where v.BaseVehicle.MakeID.Equals(makeid) && v.BaseVehicle.ModelID.Equals(modelid)
                        select v).Distinct().OrderBy(x => x.BaseVehicle.YearID).ThenBy(x => x.Submodel.SubmodelName).ToList<vcdb_Vehicle>();
            return vehicles;
        }
    }

    public class ACESModel {
        public int ID { get; set; }
        public string name { get; set; }
    }
}