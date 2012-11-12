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

        public List<BaseVehicle> GetVehicles(int makeid, int modelid) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<BaseVehicle> vehicles = new List<BaseVehicle>();
            vehicles = (from bv in db.BaseVehicles
                        where bv.MakeID.Equals(makeid) && bv.ModelID.Equals(modelid)
                        select bv).Distinct().OrderBy(x => x.YearID).ToList<BaseVehicle>();
            return vehicles;
        }

        public List<ACESBaseVehicle> GetVCDBVehicles(int makeid, int modelid) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            vcdb_Make make = db.vcdb_Makes.Where(x => x.ID.Equals(makeid)).First<vcdb_Make>();
            vcdb_Model model = db.vcdb_Models.Where(x => x.ID.Equals(modelid)).First<vcdb_Model>();
            List<int> regions = new List<int> {1,2};

            List<ACESBaseVehicle> vehicles = new List<ACESBaseVehicle>();
            vehicles = (from bv in vcdb.BaseVehicles
                        where bv.MakeID.Equals(make.AAIAMakeID) && bv.ModelID.Equals(model.AAIAModelID)
                        && bv.Vehicles.Any(x => regions.Contains(x.RegionID))
                        select new ACESBaseVehicle {
                            BaseVehicleID = bv.BaseVehicleID,
                            Year = bv.YearID,
                            Make = bv.Make,
                            Model = bv.Model,
                            Vehicles = (from v in bv.Vehicles
                                        where regions.Contains(v.RegionID)
                                        select new ACESVehicle {
                                            Submodel = v.Submodel,
                                            Region = v.Region,
                                            Configs = v.VehicleConfigs.Distinct().OrderBy(x => x.BodyStyleConfig.BodyTypeID).ToList<AAIA.VehicleConfig>()
                                        }).Distinct().OrderBy(x => x.Region.RegionID).ThenBy(x => x.Submodel.SubmodelID).ToList<ACESVehicle>()
                        }).OrderBy(x => x.Year).ToList<ACESBaseVehicle>();
            return vehicles;
        }

        internal List<AAIA.VehicleConfig> getVehicleConfigs(int BaseVehicleID, int SubmodelID) {
            AAIA.VCDBDataContext db = new AAIA.VCDBDataContext();
            List<int> regions = new List<int> {1,2};
            List<AAIA.VehicleConfig> configs = db.VehicleConfigs.Where(x => x.Vehicle.BaseVehicleID.Equals(BaseVehicleID) && x.Vehicle.SubmodelID.Equals(SubmodelID) && regions.Contains(x.Vehicle.RegionID)).Distinct().OrderBy(x => x.BodyStyleConfig.BodyTypeID).ToList<AAIA.VehicleConfig>();
            return configs;
        }

        public List<ConfigAttributeType> GetConfigAttributeTypes() {
            List<ConfigAttributeType> configs = new List<ConfigAttributeType>();
            CurtDevDataContext db = new CurtDevDataContext();
            configs = db.ConfigAttributeTypes.OrderBy(x => x.sort).ToList<ConfigAttributeType>();
            return configs;
        }

        public List<BaseVehicle> GetVehiclesByPart(int partid) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<BaseVehicle> vehicles = new List<BaseVehicle>();
            vehicles = (from vp in db.vcdb_VehicleParts
                        where vp.PartNumber.Equals(partid)
                        select vp.vcdb_Vehicle.BaseVehicle).Distinct().OrderBy(x => x.YearID).ToList<BaseVehicle>();
            return vehicles;
        }

        public List<AAIA.PartTerminology> GetPartTypes() {
            AAIA.pcdbDataContext db = new AAIA.pcdbDataContext();
            List<AAIA.PartTerminology> parttypes = new List<AAIA.PartTerminology>();
            parttypes = db.PartTerminologies.OrderBy(x => x.PartTerminologyName).ToList<AAIA.PartTerminology>();
            return parttypes;
        }

        public List<AcesType> GetACESTypes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<AcesType> types = new List<AcesType>();
            types = db.AcesTypes.OrderBy(x => x.name).ToList<AcesType>();
            foreach (AcesType t in types) {
                t.count = t.ConfigAttributeTypes.Count;
            }
            return types;
        }

        public List<ConfigAttributeType> GetConfigTypes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ConfigAttributeType> types = new List<ConfigAttributeType>();
            types = db.ConfigAttributeTypes.OrderBy(x => x.name).ToList<ConfigAttributeType>();
            return types;
        }

        public List<ConfigAttribute> GetConfigAttributes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ConfigAttribute> attributes = new List<ConfigAttribute>();
            attributes = db.ConfigAttributes.OrderBy(x => x.value).ToList<ConfigAttribute>();
            return attributes;
        }

        public AcesType SaveACESType(int id = 0, string name = null) {
            AcesType type = new AcesType();
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                type = db.AcesTypes.Where(x => x.ID.Equals(id)).First<AcesType>();
                if (name != null) {
                    try {
                        AcesType t = db.AcesTypes.Where(x => x.name.Trim().Equals(name.Trim()) && !x.ID.Equals(id)).First<AcesType>();
                    } catch {
                        type.name = name.Trim();
                        db.SubmitChanges();
                    }
                }
            } catch {
                if (name != null && name.Trim() != "") {
                    try {
                        AcesType t = db.AcesTypes.Where(x => x.name.Trim().Equals(name.Trim())).First<AcesType>();
                    } catch {
                        type.name = name.Trim();
                        db.AcesTypes.InsertOnSubmit(type);
                        db.SubmitChanges();
                    }
                } else {
                    throw new Exception("You must enter a name.");
                }
            }
            return type;
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

    /*public class AAIAVehicleComparer : IEqualityComparer<ACESVehicle> {
        bool IEqualityComparer<ACESVehicle>.Equals(ACESVehicle x, ACESVehicle y) {
            // Check whether the compared objects reference the same data.
            if (x.BaseVehicle.BaseVehicleID.Equals(y.BaseVehicle.BaseVehicleID) && x.Submodel.SubmodelID.Equals(y.Submodel.SubmodelID)) {
                return true;
            } else {
                return false;
            }

        }

        int IEqualityComparer<ACESVehicle>.GetHashCode(ACESVehicle obj) {
            return obj.BaseVehicle.GetHashCode();
        }
    }

    public class AAIAVehicleConfigComparer : IEqualityComparer<AAIA.VehicleConfig> {
        bool IEqualityComparer<AAIA.VehicleConfig>.Equals(AAIA.VehicleConfig x, AAIA.VehicleConfig y) {
            // Check whether the compared objects reference the same data.
            if (x.BedConfig.BedLengthID.Equals(y.BedConfig.BedLengthID) && x.BedConfig.BedTypeID.Equals(y.BedConfig.BedTypeID)
                && x.BodyStyleConfig.BodyTypeID.Equals(y.BodyStyleConfig.BodyTypeID) && x.BodyStyleConfig.BodyNumDoorsID.Equals(y.BodyStyleConfig.BodyNumDoorsID)
                && x.BrakeConfig.BrakeABSID.Equals(y.BrakeConfig.BrakeABSID)  && x.BrakeConfig.BrakeSystemID.Equals(y.BrakeConfig.BrakeSystemID)
                && x.BrakeConfig.FrontBrakeTypeID.Equals(y.BrakeConfig.FrontBrakeTypeID) && x.BrakeConfig.RearBrakeTypeID.Equals(y.BrakeConfig.RearBrakeTypeID)
                && x.DriveTypeID.Equals(y.DriveTypeID) && x.WheelbaseID.Equals(y.WheelbaseID) && x.MfrBodyCodeID.Equals(y.MfrBodyCodeID) 
                && x.EngineConfig.EngineBaseID.Equals(y.EngineConfig.EngineBaseID) && x.EngineConfig.FuelDeliveryConfigID.Equals(y.EngineConfig.FuelDeliveryConfigID)
                && x.EngineConfig.FuelTypeID.Equals(y.EngineConfig.FuelTypeID) && x.EngineConfig.ValvesID.Equals(y.EngineConfig.ValvesID)
                && x.EngineConfig.IgnitionSystemTypeID.Equals(y.EngineConfig.IgnitionSystemTypeID) && x.EngineConfig.EngineVINID.Equals(y.EngineConfig.EngineVINID)
                && x.EngineConfig.EngineVersionID.Equals(y.EngineConfig.EngineVersionID) && x.EngineConfig.EngineMfrID.Equals(y.EngineConfig.EngineMfrID)
                && x.EngineConfig.EngineDesignationID.Equals(y.EngineConfig.EngineDesignationID) && x.EngineConfig.CylinderHeadTypeID.Equals(y.EngineConfig.CylinderHeadTypeID)
                && x.EngineConfig.AspirationID.Equals(y.EngineConfig.AspirationID) && x.SpringTypeConfig.RearSpringTypeID.Equals(y.SpringTypeConfig.RearSpringTypeID)
                && x.SpringTypeConfig.FrontSpringTypeID.Equals(y.SpringTypeConfig.FrontSpringTypeID) && x.SteeringConfig.SteeringSystemID.Equals(y.SteeringConfig.SteeringSystemID)
                && x.SteeringConfig.SteeringTypeID.Equals(y.SteeringConfig.SteeringTypeID) && x.SpringTypeConfigID.Equals(y.SpringTypeConfigID) && x.SteeringConfigID.Equals(y.SteeringConfigID)
                && x.Transmission.TransmissionElecControlledID.Equals(y.Transmission.TransmissionElecControlledID)
                && x.Transmission.TransmissionMfrCodeID.Equals(y.Transmission.TransmissionMfrCodeID)
                && x.Transmission.TransmissionMfrID.Equals(y.Transmission.TransmissionMfrID) && x.Transmission.TransmissionBase.TransmissionControlTypeID.Equals(y.Transmission.TransmissionBase.TransmissionControlTypeID)
                && x.Transmission.TransmissionBase.TransmissionNumSpeedsID.Equals(y.Transmission.TransmissionBase.TransmissionNumSpeedsID)
                && x.Transmission.TransmissionBase.TransmissionTypeID.Equals(x.Transmission.TransmissionBase.TransmissionTypeID)
                ) {
                return true;
            } else {
                return false;
            }

        }

        int IEqualityComparer<AAIA.VehicleConfig>.GetHashCode(AAIA.VehicleConfig obj) {
            return obj.BedConfig.GetHashCode();
        }
    }*/
    public class ACESModel {
        public int ID { get; set; }
        public string name { get; set; }
    }

    public class ACESBaseVehicle {
        public int BaseVehicleID { get; set; }
        public int Year { get; set; }
        public AAIA.Make Make { get; set; }
        public AAIA.Model Model { get; set; }
        public List<ACESVehicle> Vehicles { get; set; }
    }

    public class ACESVehicle {
        public AAIA.Submodel Submodel { get; set; }
        public AAIA.Region Region { get; set; }
        public List<AAIA.VehicleConfig> Configs { get; set; }
    }

}