using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using LinqKit;

namespace CurtAdmin.Models {
    public class ACES {
        public List<vcdb_Make> GetMakes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<vcdb_Make> makes = new List<vcdb_Make>();
            makes = (from m in db.vcdb_Makes
                     join bv in db.BaseVehicles on m.ID equals bv.MakeID
                     select m).Distinct().OrderBy(x => x.MakeName).ToList<vcdb_Make>();
            return makes;
        }

        public List<AAIA.Make> GetVCDBMakes() {
            AAIA.VCDBDataContext db = new AAIA.VCDBDataContext();
            List<AAIA.Make> makes = new List<AAIA.Make>();
            List<int> regions = new List<int> {1,2};
            List<int> vtypes = new List<int> { 5, 6, 7 };
            makes = (from m in db.Makes
                     join bv in db.BaseVehicles on m.MakeID equals bv.MakeID
                     where bv.YearID >= 1962 && bv.Vehicles.Any(x => regions.Contains(x.RegionID))
                     && vtypes.Contains(bv.Model.VehicleTypeID)
                     select m).Distinct().OrderBy(x => x.MakeName).ToList<AAIA.Make>();
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

        public List<AAIA.Model> GetVCDBModels(int id = 0) {
            AAIA.VCDBDataContext db = new AAIA.VCDBDataContext();
            List<AAIA.Model> models = new List<AAIA.Model>();
            List<int> regions = new List<int> { 1, 2 };
            List<int> vtypes = new List<int> { 5, 6, 7 };
            models = (from m in db.Models
                      join bv in db.BaseVehicles on m.ModelID equals bv.ModelID
                      where bv.YearID >= 1962 && bv.Vehicles.Any(x => regions.Contains(x.RegionID))
                      && vtypes.Contains(m.VehicleTypeID) && bv.MakeID.Equals(id)
                      select m).Distinct().OrderBy(x => x.ModelName).ToList<AAIA.Model>();
            return models;
        }

        public List<AAIA.BaseVehicle> GetBaseVehicles(int makeid, int modelid) {
            CurtDevDataContext cddb = new CurtDevDataContext();
            List<int> bvids = cddb.BaseVehicles.Where(x => x.vcdb_Make.AAIAMakeID.Equals(makeid) && x.vcdb_Model.AAIAModelID.Equals(modelid) && x.AAIABaseVehicleID != null && x.vcdb_Vehicles.Count > 0).Select(x => (int)x.AAIABaseVehicleID).ToList<int>();
            
            AAIA.VCDBDataContext db = new AAIA.VCDBDataContext();
            List<AAIA.BaseVehicle> basevehicles = new List<AAIA.BaseVehicle>();
            List<int> regions = new List<int> { 1, 2 };
            basevehicles = (from bv in db.BaseVehicles
                            where bv.MakeID.Equals(makeid) && bv.ModelID.Equals(modelid) && bv.YearID >= 1962 && !bvids.Contains(bv.BaseVehicleID)
                            orderby bv.YearID descending
                            select bv).ToList<AAIA.BaseVehicle>();
            return basevehicles;
        }

        public vcdb_Vehicle AddBaseVehicle(int bvid) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            try {
                BaseVehicle bv = new BaseVehicle();
                try {
                    bv = db.BaseVehicles.Where(x => x.AAIABaseVehicleID.Equals(bvid)).First<BaseVehicle>();
                } catch {
                    // The Base Vehicle doesn't exist in CurtDev.  We need to create it first.
                    AAIA.BaseVehicle acesbv = vcdb.BaseVehicles.Where(x => x.BaseVehicleID.Equals(bvid)).First<AAIA.BaseVehicle>();
                    vcdb_Make make = new vcdb_Make();
                    vcdb_Model model = new vcdb_Model();
                    try {
                        make = db.vcdb_Makes.Where(x => x.AAIAMakeID.Equals(acesbv.MakeID)).First<vcdb_Make>();
                    } catch {
                        AAIA.Make vcdbmake = vcdb.BaseVehicles.Where(x => x.BaseVehicleID.Equals(bvid)).Select(x => x.Make).First<AAIA.Make>();
                        make = new vcdb_Make {
                            MakeName = vcdbmake.MakeName.Trim(),
                            AAIAMakeID = vcdbmake.MakeID
                        };
                        db.vcdb_Makes.InsertOnSubmit(make);
                        db.SubmitChanges();
                    }
                    try {
                        model = db.vcdb_Models.Where(x => x.AAIAModelID.Equals(acesbv.ModelID)).First<vcdb_Model>();
                    } catch {
                        AAIA.Model vcdbmodel = vcdb.BaseVehicles.Where(x => x.BaseVehicleID.Equals(bvid)).Select(x => x.Model).First<AAIA.Model>();
                        model = new vcdb_Model {
                            ModelName = vcdbmodel.ModelName.Trim(),
                            AAIAModelID = vcdbmodel.ModelID,
                            VehicleTypeID = vcdbmodel.VehicleTypeID
                        };
                        db.vcdb_Models.InsertOnSubmit(model);
                        db.SubmitChanges();
                    }
                    bv.YearID = acesbv.YearID;
                    bv.MakeID = make.ID;
                    bv.ModelID = model.ID;
                    bv.AAIABaseVehicleID = acesbv.BaseVehicleID;
                    db.BaseVehicles.InsertOnSubmit(bv);
                    db.SubmitChanges();
                }

                //Create a vehicle with just the BaseVehicle

                vehicle = new vcdb_Vehicle {
                    BaseVehicleID = bv.ID
                };
                db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                db.SubmitChanges();
            } catch { }
            return vehicle;
        }

        public void RemoveBaseVehicle(int bvid) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<Note> notes = db.Notes.Where(x => x.vcdb_VehiclePart.vcdb_Vehicle.BaseVehicleID.Equals(bvid)).ToList<Note>();
            db.Notes.DeleteAllOnSubmit(notes);
            db.SubmitChanges();

            List<vcdb_VehiclePart> vehicleParts = db.vcdb_VehicleParts.Where(x => x.vcdb_Vehicle.BaseVehicleID.Equals(bvid)).ToList<vcdb_VehiclePart>();
            db.vcdb_VehicleParts.DeleteAllOnSubmit(vehicleParts);
            db.SubmitChanges();

            List<vcdb_Vehicle> vehicles = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bvid)).ToList<vcdb_Vehicle>();
            List<VehicleConfig> configs = vehicles.Where(x => x.ConfigID != null).Select(x => x.VehicleConfig).ToList<VehicleConfig>();
            db.vcdb_Vehicles.DeleteAllOnSubmit(vehicles);
            db.SubmitChanges();

            List<VehicleConfig> deleteables = new List<VehicleConfig>();
            foreach (VehicleConfig config in configs) {
                if (db.VehicleConfigs.Where(x => x.ID.Equals(config.ID)).Select(x => x.vcdb_Vehicles).Count() == 0) {
                    deleteables.Add(config);
                }
            }
            if (deleteables.Count > 0) {
                db.VehicleConfigs.DeleteAllOnSubmit(deleteables);
            }
            
            BaseVehicle bv = db.BaseVehicles.Where(x => x.ID.Equals(bvid)).First<BaseVehicle>();
            db.BaseVehicles.DeleteOnSubmit(bv);
            db.SubmitChanges();
        }

        public vcdb_Vehicle AddSubmodel(int bvid, int submodelID) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            try {
                BaseVehicle bv = new BaseVehicle();
                try {
                    bv = db.BaseVehicles.Where(x => x.AAIABaseVehicleID.Equals(bvid)).First<BaseVehicle>();
                } catch {
                    vcdb_Vehicle newvehicle = AddBaseVehicle(bvid);
                    bv = newvehicle.BaseVehicle;
                }

                Submodel submodel = new Submodel();
                try {
                    submodel = db.Submodels.Where(x => x.AAIASubmodelID.Equals(submodelID)).First<Submodel>();
                } catch {
                    AAIA.Submodel vcdbsubmodel = vcdb.Submodels.Where(x => x.SubmodelID.Equals(submodelID)).First<AAIA.Submodel>();
                    submodel = new Submodel {
                        AAIASubmodelID = vcdbsubmodel.SubmodelID,
                        SubmodelName = vcdbsubmodel.SubmodelName
                    };
                    db.Submodels.InsertOnSubmit(submodel);
                    db.SubmitChanges();
                }

                //Create a vehicle with the BaseVehicle and Submodel

                vehicle = new vcdb_Vehicle {
                    BaseVehicleID = bv.ID,
                    SubModelID = submodel.ID
                };
                db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                db.SubmitChanges();
            } catch { }
            return vehicle;
        }

        public void RemoveSubmodel(int bvid, int submodelID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<Note> notes = db.Notes.Where(x => x.vcdb_VehiclePart.vcdb_Vehicle.BaseVehicleID.Equals(bvid) && x.vcdb_VehiclePart.vcdb_Vehicle.SubModelID.Equals(submodelID)).ToList<Note>();
            db.Notes.DeleteAllOnSubmit(notes);
            db.SubmitChanges();

            List<vcdb_VehiclePart> vehicleParts = db.vcdb_VehicleParts.Where(x => x.vcdb_Vehicle.BaseVehicleID.Equals(bvid) && x.vcdb_Vehicle.SubModelID.Equals(submodelID)).ToList<vcdb_VehiclePart>();
            db.vcdb_VehicleParts.DeleteAllOnSubmit(vehicleParts);
            db.SubmitChanges();

            List<vcdb_Vehicle> vehicles = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bvid) && x.SubModelID.Equals(submodelID)).ToList<vcdb_Vehicle>();
            List<VehicleConfig> configs = vehicles.Where(x => x.ConfigID != null).Select(x => x.VehicleConfig).ToList<VehicleConfig>();
            db.vcdb_Vehicles.DeleteAllOnSubmit(vehicles);
            db.SubmitChanges();

            List<VehicleConfig> deleteables = new List<VehicleConfig>();
            foreach (VehicleConfig config in configs) {
                if (db.VehicleConfigs.Where(x => x.ID.Equals(config.ID)).Select(x => x.vcdb_Vehicles).Count() == 0) {
                    deleteables.Add(config);
                }
            }
            if (deleteables.Count > 0) {
                db.VehicleConfigs.DeleteAllOnSubmit(deleteables);
                db.SubmitChanges();
            }
        }
        
        public List<ACESBaseVehicle> GetVehicles(int makeid, int modelid) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<ACESBaseVehicle> vehicles = new List<ACESBaseVehicle>();
            vehicles = (from bv in db.BaseVehicles
                        where bv.MakeID.Equals(makeid) && bv.ModelID.Equals(modelid) && bv.vcdb_Vehicles.Count > 0
                        select new ACESBaseVehicle {
                            ID = bv.ID,
                            AAIABaseVehicleID = bv.AAIABaseVehicleID,
                            YearID = bv.YearID,
                            Make = bv.vcdb_Make,
                            Model = bv.vcdb_Model,
                            Submodels = (from v in bv.vcdb_Vehicles
                                         where v.SubModelID != null
                                         group v by v.Submodel into s
                                         select new ACESSubmodel {
                                             SubmodelID = s.Key.ID,
                                             submodel = s.Key,
                                             vehicles = (from ve in bv.vcdb_Vehicles
                                                         where ve.SubModelID.Equals(s.Key.ID)
                                                         select new ACESVehicle {
                                                             ID = ve.ID,
                                                             configs = ve.VehicleConfig.VehicleConfigAttributes.Select(x => x.ConfigAttribute).OrderBy(x => x.ConfigAttributeType.name).ToList<ConfigAttribute>()
                                                         }).ToList<ACESVehicle>(),
                                             configlist = (from c in bv.vcdb_Vehicles
                                                           join vc in db.VehicleConfigAttributes on c.ConfigID equals vc.VehicleConfigID
                                                           where c.SubModelID.Equals(s.Key.ID)
                                                           select vc.ConfigAttribute.ConfigAttributeType).Distinct().OrderBy(x => x.name).ToList<ConfigAttributeType>()
                                         }).OrderBy(x => x.submodel.SubmodelName).ToList<ACESSubmodel>(),
                        }).Distinct().OrderByDescending(x => x.YearID).ToList<ACESBaseVehicle>();
            foreach (ACESBaseVehicle abv in vehicles) {
                foreach (ACESSubmodel sm in abv.Submodels) {
                    sm.vcdb = vcdb.Vehicles.Where(x => x.BaseVehicleID.Equals(abv.AAIABaseVehicleID) && x.SubmodelID.Equals(sm.submodel.AAIASubmodelID)).ToList<AAIA.Vehicle>().Count > 0;
                    foreach (ACESVehicle v in sm.vehicles) {
                        if (v.configs.Any(x => x.vcdbID == null)) {
                            v.vcdb = false;
                        } else {
                            v.vcdb = true;
                            List<int> vehicleIDs = vcdb.Vehicles.Where(x => x.SubmodelID.Equals(sm.submodel.AAIASubmodelID) && x.BaseVehicleID.Equals(abv.AAIABaseVehicleID)).Select(x => x.VehicleID).ToList<int>();
                            var predicate = PredicateBuilder.True<AAIA.VehicleConfig>();
                            predicate = predicate.And(p => vehicleIDs.Contains(p.VehicleID));
                            foreach (ConfigAttribute ca in v.configs) {
                                switch (ca.ConfigAttributeType.AcesType.name) {
                                    case "WheelBase":
                                        predicate = predicate.And(p => p.WheelbaseID.Equals(ca.vcdbID));
                                        break;
                                    case "BodyType":
                                        predicate = predicate.And(p => p.BodyStyleConfig.BodyTypeID.Equals(ca.vcdbID));
                                        break;
                                    case "DriveType":
                                        predicate = predicate.And(p => p.DriveTypeID.Equals(ca.vcdbID));
                                        break;
                                    case "BodyNumDoors":
                                        predicate = predicate.And(p => p.BodyStyleConfig.BodyNumDoorsID.Equals(ca.vcdbID));
                                        break;
                                    case "BedLength":
                                        predicate = predicate.And(p => p.BedConfig.BedLengthID.Equals(ca.vcdbID));
                                        break;
                                    case "FuelType":
                                        predicate = predicate.And(p => p.EngineConfig.FuelTypeID.Equals(ca.vcdbID));
                                        break;
                                    case "EngineBase":
                                        predicate = predicate.And(p => p.EngineConfig.EngineBaseID.Equals(ca.vcdbID));
                                        break;
                                    default:
                                        v.vcdb = false;
                                        break;
                                }
                                if (v.vcdb) {
                                    // run query
                                    List<AAIA.VehicleConfig> vconfigs = vcdb.VehicleConfigs.Where(predicate).ToList<AAIA.VehicleConfig>();
                                    v.vcdb = vconfigs.Count > 0;
                                }
                            }
                        }
                    }
                }
            }
            return vehicles;
        }

        public List<VCDBBaseVehicle> GetVCDBVehicles(int makeid, int modelid) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            vcdb_Make make = db.vcdb_Makes.Where(x => x.ID.Equals(makeid)).First<vcdb_Make>();
            vcdb_Model model = db.vcdb_Models.Where(x => x.ID.Equals(modelid)).First<vcdb_Model>();
            List<int> regions = new List<int> {1,2};
            List<BaseVehicle> basevehicles = (from bv in db.BaseVehicles
                                              where bv.MakeID.Equals(makeid) && bv.ModelID.Equals(modelid) && bv.vcdb_Vehicles.Count > 0 && bv.AAIABaseVehicleID != null
                                              select bv).Distinct().ToList<BaseVehicle>();
            List<VCDBBaseVehicle> vehicles = new List<VCDBBaseVehicle>();
            vehicles = (from bv in vcdb.BaseVehicles
                        where bv.MakeID.Equals(make.AAIAMakeID) && bv.ModelID.Equals(model.AAIAModelID)
                        && bv.Vehicles.Any(x => regions.Contains(x.RegionID))
                        select new VCDBBaseVehicle {
                            BaseVehicleID = bv.BaseVehicleID,
                            Year = bv.YearID,
                            Make = bv.Make,
                            Model = bv.Model,
                            Vehicles = (from v in bv.Vehicles
                                        where regions.Contains(v.RegionID)
                                        select new VCDBVehicle {
                                            Submodel = v.Submodel,
                                            Region = v.Region,
                                            Configs = v.VehicleConfigs.Distinct().OrderBy(x => x.BodyStyleConfig.BodyTypeID).ToList<AAIA.VehicleConfig>()
                                        }).Distinct().OrderBy(x => x.Region.RegionID).ThenBy(x => x.Submodel.SubmodelID).ToList<VCDBVehicle>()
                        }).OrderByDescending(x => x.Year).ToList<VCDBBaseVehicle>();
            
            // linq won't let me do this comparison in the query itself. Iteration is the only solution...F#*&ing Linq
            foreach (VCDBBaseVehicle abv in vehicles) {
                abv.exists = basevehicles.Any(x => x.AAIABaseVehicleID.Equals(abv.BaseVehicleID) && x.vcdb_Vehicles.Count > 0);
                foreach (VCDBVehicle vehicle in abv.Vehicles) {
                    vehicle.exists = basevehicles.Where(x => x.AAIABaseVehicleID.Equals(abv.BaseVehicleID)).Any(x => x.vcdb_Vehicles.Any(y => y.SubModelID != null && y.Submodel.AAIASubmodelID.Equals(vehicle.Submodel.SubmodelID)));
                }
            }
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

        public AcesType GetACESType(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            AcesType type = new AcesType();
            type = db.AcesTypes.Where(x => x.ID.Equals(id)).FirstOrDefault<AcesType>();
            type.count = type.ConfigAttributeTypes.Count;
            return type;
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

        public List<ConfigAttributeType> GetConfigTypes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ConfigAttributeType> types = new List<ConfigAttributeType>();
            types = db.ConfigAttributeTypes.OrderBy(x => x.name).ToList<ConfigAttributeType>();
            foreach (ConfigAttributeType t in types) {
                t.count = t.ConfigAttributes.Count;
            }
            return types;
        }

        public ConfigAttributeType GetConfigType(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            ConfigAttributeType type = new ConfigAttributeType();
            type = db.ConfigAttributeTypes.Where(x => x.ID.Equals(id)).FirstOrDefault<ConfigAttributeType>();
            type.count = type.ConfigAttributes.Count;
            return type;
        }

        public ConfigAttributeType SaveConfigurationType(int id = 0, string name = null, int? acestypeid = null) {
            ConfigAttributeType type = new ConfigAttributeType();
            CurtDevDataContext db = new CurtDevDataContext();
            if (String.IsNullOrWhiteSpace(name)) {
                throw new Exception("You must enter a name.");
            }
            try {
                type = db.ConfigAttributeTypes.Where(x => x.ID.Equals(id)).First<ConfigAttributeType>();
                if (name != null) {
                    try {
                        ConfigAttributeType t = db.ConfigAttributeTypes.Where(x => x.name.Trim().Equals(name.Trim()) && !x.ID.Equals(id)).First<ConfigAttributeType>();
                    } catch {
                        type.name = name.Trim();
                        type.AcesTypeID = acestypeid;
                        db.SubmitChanges();
                    }
                }
            } catch {
                try {
                    ConfigAttributeType t = db.ConfigAttributeTypes.Where(x => x.name.Trim().Equals(name.Trim())).First<ConfigAttributeType>();
                } catch {
                    type.name = name.Trim();
                    type.AcesTypeID = acestypeid;
                    type.sort = db.ConfigAttributeTypes.OrderByDescending(x => x.sort).Select(x => x.sort).FirstOrDefault<int>() + 1;
                    db.ConfigAttributeTypes.InsertOnSubmit(type);
                    db.SubmitChanges();
                }
            }
            return type;
        }

        public List<ConfigAttribute> GetConfigAttributes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ConfigAttribute> attributes = new List<ConfigAttribute>();
            attributes = db.ConfigAttributes.OrderBy(x => x.value).ToList<ConfigAttribute>();
            foreach (ConfigAttribute attr in attributes) {
                attr.count = db.vcdb_Vehicles.Where(x => x.VehicleConfig.VehicleConfigAttributes.Any(y => y.AttributeID.Equals(attr.ID))).Distinct().Count();
            }
            return attributes;
        }

        public ConfigAttribute GetConfigAttribute(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            ConfigAttribute attribute = new ConfigAttribute();
            attribute = db.ConfigAttributes.Where(x => x.ID.Equals(id)).FirstOrDefault<ConfigAttribute>();
            attribute.count = db.vcdb_Vehicles.Where(x => x.VehicleConfig.VehicleConfigAttributes.Any(y => y.AttributeID.Equals(attribute.ID))).Distinct().Count();
            return attribute;
        }

        public ConfigAttribute SaveConfigurationAttr(int id = 0, string value = null, int configtypeid = 0, int? vcdbID = null) {
            ConfigAttribute attr = new ConfigAttribute();
            CurtDevDataContext db = new CurtDevDataContext();
            if (configtypeid == 0) {
                throw new Exception("You must choose a configuration type");
            }
            if (String.IsNullOrWhiteSpace(value)) {
                throw new Exception("You must enter a name.");
            }
            try {
                attr = db.ConfigAttributes.Where(x => x.ID.Equals(id)).First<ConfigAttribute>();
                try {
                    ConfigAttribute a = db.ConfigAttributes.Where(x => x.value.Trim().Equals(value.Trim()) && x.vcdbID.Equals(vcdbID) && !x.ID.Equals(id)).First<ConfigAttribute>();
                } catch {
                    attr.value = value.Trim();
                    attr.ConfigAttributeTypeID = configtypeid;
                    attr.vcdbID = vcdbID;
                    db.SubmitChanges();
                }
            } catch {
                try {
                    ConfigAttribute a = db.ConfigAttributes.Where(x => x.value.Trim().Equals(value.Trim()) && x.vcdbID.Equals(vcdbID)).First<ConfigAttribute>();
                } catch {
                    attr.value = value.Trim();
                    attr.ConfigAttributeTypeID = configtypeid;
                    attr.vcdbID = vcdbID;
                    db.ConfigAttributes.InsertOnSubmit(attr);
                    db.SubmitChanges();
                }
            }
            return attr;
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

    public class VCDBBaseVehicle {
        public int BaseVehicleID { get; set; }
        public int Year { get; set; }
        public AAIA.Make Make { get; set; }
        public AAIA.Model Model { get; set; }
        public List<VCDBVehicle> Vehicles { get; set; }
        public bool exists { get; set; }
    }

    public class VCDBVehicle {
        public AAIA.Submodel Submodel { get; set; }
        public AAIA.Region Region { get; set; }
        public List<AAIA.VehicleConfig> Configs { get; set; }
        public bool exists { get; set; }
    }

    public class ACESBaseVehicle {
        public int ID { get; set; }
        public int? AAIABaseVehicleID { get; set; }
        public int YearID { get; set; }
        public vcdb_Make Make { get; set; }
        public vcdb_Model Model { get; set; }
        public List<ACESSubmodel> Submodels { get; set; }
    }

    public class ACESSubmodel {
        public int? SubmodelID { get; set; }
        public Submodel submodel { get; set; }
        public List<ConfigAttributeType> configlist { get; set; }
        public List<ACESVehicle> vehicles { get; set; }
        public bool vcdb { get; set; }
    }

    public class ACESVehicle {
        public int ID { get; set; }
        public List<ConfigAttribute> configs { get; set; }
        public bool vcdb { get; set; }
    }

}