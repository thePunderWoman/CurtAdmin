﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using LinqKit;

namespace CurtAdmin.Models {
    public class ACES {
        public List<int> GetYears() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<int> years = new List<int>();
            years = db.vcdb_Years.OrderByDescending(x => x.YearID).Select(x => x.YearID).ToList();
            return years;
        }

        public vcdb_Year AddYear(int year) {
            vcdb_Year y = new vcdb_Year();
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                y = db.vcdb_Years.Where(x => x.YearID.Equals(year)).First();
            } catch {
                y = new vcdb_Year {
                    YearID = year
                };
                db.vcdb_Years.InsertOnSubmit(y);
                db.SubmitChanges();
            }

            return y;
        }

        public void RemoveYear(int year) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Year y = db.vcdb_Years.Where(x => x.YearID.Equals(year)).First();
            db.vcdb_Years.DeleteOnSubmit(y);
            db.SubmitChanges();
        }

        public List<ACESMake> GetMakesByYear(int yearID) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<ACESMake> makes = (from m in db.vcdb_Makes
                                    join bv in db.BaseVehicles on m.ID equals bv.MakeID
                                    where bv.YearID.Equals(yearID)
                                    select new ACESMake { 
                                        ID = m.ID,
                                        name = m.MakeName,
                                        AAIAID = m.AAIAMakeID
                                    }).Distinct().ToList();
            List<int> makeids = makes.Where(x => x.AAIAID != null).Select(x => (int)x.AAIAID).ToList();
            List<int> regions = new List<int> { 1, 2 };
            List<int> vtypes = new List<int> { 5, 6, 7 };
            List<ACESMake> vcdbmakes = (from m in vcdb.Makes
                                        join bv in vcdb.BaseVehicles on m.MakeID equals bv.MakeID
                                        where bv.YearID.Equals(yearID) && bv.Vehicles.Any(x => regions.Contains(x.RegionID))
                                        && vtypes.Contains(bv.Model.VehicleTypeID) && !makeids.Contains(m.MakeID)
                                        select new ACESMake {
                                            ID = 0,
                                            name = m.MakeName,
                                            AAIAID = m.MakeID
                                        }).Distinct().ToList();
            makes.AddRange(vcdbmakes);
            List<ACESMake> allmakes = makes.OrderBy(x => x.name).ToList();
            return allmakes;
        }

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

        public List<ACESMake> GetModelsByMake(int yearID, string makeID) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<int> idlist = makeID.Split('|').Select(n => int.Parse(n)).ToList();
            int mID = idlist[0];
            int aaiaID = idlist[1];
            List<ACESMake> models = (from m in db.vcdb_Models
                                    join bv in db.BaseVehicles on m.ID equals bv.ModelID
                                    where bv.YearID.Equals(yearID) && (bv.MakeID.Equals(mID) || bv.vcdb_Make.AAIAMakeID.Equals(aaiaID))
                                    select new ACESMake {
                                        ID = m.ID,
                                        name = m.ModelName,
                                        AAIAID = m.AAIAModelID
                                    }).Distinct().ToList();
            List<int> modelids = models.Where(x => x.AAIAID != null).Select(x => (int)x.AAIAID).ToList();
            List<int> regions = new List<int> { 1, 2 };
            List<int> vtypes = new List<int> { 5, 6, 7 };
            List<ACESMake> vcdbmodels = (from m in vcdb.Models
                                        join bv in vcdb.BaseVehicles on m.ModelID equals bv.ModelID
                                        where bv.YearID.Equals(yearID) && bv.MakeID.Equals(aaiaID) && bv.Vehicles.Any(x => regions.Contains(x.RegionID))
                                        && vtypes.Contains(bv.Model.VehicleTypeID) && !modelids.Contains(m.ModelID)
                                        select new ACESMake {
                                            ID = 0,
                                            name = m.ModelName,
                                            AAIAID = m.ModelID
                                        }).Distinct().ToList();
            models.AddRange(vcdbmodels);
            List<ACESMake> allmodels = models.OrderBy(x => x.name).ToList();
            return allmodels;
        }

        public List<ACESMake> GetSubmodelsByModel(int yearID, string makeID, string modelID) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<int> idlist = makeID.Split('|').Select(n => int.Parse(n)).ToList();
            int maID = idlist[0];
            int maAaiaID = idlist[1];
            idlist = modelID.Split('|').Select(n => int.Parse(n)).ToList();
            int moID = idlist[0];
            int moAaiaID = idlist[1];
            List<ACESMake> submodels = (from s in db.Submodels
                                        join v in db.vcdb_Vehicles on s.ID equals v.SubModelID
                                        where v.BaseVehicle.YearID.Equals(yearID) && (v.BaseVehicle.MakeID.Equals(maID) || v.BaseVehicle.vcdb_Make.AAIAMakeID.Equals(maAaiaID))
                                        && (v.BaseVehicle.ModelID.Equals(moID) || v.BaseVehicle.vcdb_Model.AAIAModelID.Equals(moAaiaID))
                                        select new ACESMake {
                                            ID = s.ID,
                                            name = s.SubmodelName,
                                            AAIAID = s.AAIASubmodelID
                                        }).Distinct().ToList();
            List<int> submodelids = submodels.Where(x => x.AAIAID != null).Select(x => (int)x.AAIAID).ToList();
            List<int> regions = new List<int> { 1, 2 };
            List<int> vtypes = new List<int> { 5, 6, 7 };
            List<ACESMake> vcdbsubmodels = (from s in vcdb.Submodels
                                            join v in vcdb.Vehicles on s.SubmodelID equals v.SubmodelID
                                            where v.BaseVehicle.YearID.Equals(yearID) && v.BaseVehicle.MakeID.Equals(maAaiaID) && v.BaseVehicle.ModelID.Equals(moAaiaID)
                                            && regions.Contains(v.RegionID)
                                            && vtypes.Contains(v.BaseVehicle.Model.VehicleTypeID) && !submodelids.Contains(s.SubmodelID)
                                            select new ACESMake {
                                                ID = 0,
                                                name = s.SubmodelName,
                                                AAIAID = s.SubmodelID
                                            }).Distinct().ToList();
            submodels.AddRange(vcdbsubmodels);
            List<ACESMake> allsubmodels = submodels.OrderBy(x => x.name).ToList();
            return allsubmodels;
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
                List<VehicleConfigAttribute> vattrs = deleteables.SelectMany(x => x.VehicleConfigAttributes).Distinct().ToList();
                db.VehicleConfigAttributes.DeleteAllOnSubmit(vattrs);
                db.SubmitChanges();

                db.VehicleConfigs.DeleteAllOnSubmit(deleteables);
            }
            
            BaseVehicle bv = db.BaseVehicles.Where(x => x.ID.Equals(bvid)).First<BaseVehicle>();
            db.BaseVehicles.DeleteOnSubmit(bv);
            db.SubmitChanges();
        }

        public vcdb_Vehicle AddVCDBSubmodel(int bvid, int submodelID) {
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
                try {
                    vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bv.ID) && x.SubModelID.Equals(submodel.ID) && x.ConfigID == null).First();
                } catch {
                    vehicle = new vcdb_Vehicle {
                        BaseVehicleID = bv.ID,
                        SubModelID = submodel.ID
                    };
                    db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                    db.SubmitChanges();
                }
            } catch { }
            return vehicle;
        }

        public vcdb_Vehicle AddSubmodel(int bvid, int submodelID) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            try {
                BaseVehicle bv = db.BaseVehicles.Where(x => x.ID.Equals(bvid)).First<BaseVehicle>();
                Submodel submodel = db.Submodels.Where(x => x.ID.Equals(submodelID)).First<Submodel>();

                //Create a vehicle with the BaseVehicle and Submodel
                try {
                    vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bv.ID) && x.SubModelID.Equals(submodel.ID) && x.ConfigID == null).First();
                } catch {
                    vehicle = new vcdb_Vehicle {
                        BaseVehicleID = bv.ID,
                        SubModelID = submodel.ID
                    };
                    db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                    db.SubmitChanges();
                }
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
                List<VehicleConfigAttribute> vattrs = deleteables.SelectMany(x => x.VehicleConfigAttributes).Distinct().ToList();
                db.VehicleConfigAttributes.DeleteAllOnSubmit(vattrs);
                db.SubmitChanges();

                db.VehicleConfigs.DeleteAllOnSubmit(deleteables);
                db.SubmitChanges();
            }
        }

        public vcdb_Vehicle GetVehicle(int vehicleID) {
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            CurtDevDataContext db = new CurtDevDataContext();
            vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).FirstOrDefault<vcdb_Vehicle>();
            return vehicle;
        }

        public void RemoveVehicle(int vehicleID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<Note> notes = db.Notes.Where(x => x.vcdb_VehiclePart.vcdb_Vehicle.ID.Equals(vehicleID)).ToList<Note>();
            db.Notes.DeleteAllOnSubmit(notes);
            db.SubmitChanges();

            List<vcdb_VehiclePart> vehicleParts = db.vcdb_VehicleParts.Where(x => x.vcdb_Vehicle.ID.Equals(vehicleID)).ToList<vcdb_VehiclePart>();
            db.vcdb_VehicleParts.DeleteAllOnSubmit(vehicleParts);
            db.SubmitChanges();

            vcdb_Vehicle vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First<vcdb_Vehicle>();
            int configID = vehicle.ConfigID ?? 0;
            db.vcdb_Vehicles.DeleteOnSubmit(vehicle);
            db.SubmitChanges();

            if (configID != 0) {
                VehicleConfig config = db.VehicleConfigs.Where(x => x.ID.Equals(configID)).First<VehicleConfig>();
                if (config.vcdb_Vehicles.Count == 0) {
                    List<VehicleConfigAttribute> vattrs = db.VehicleConfigAttributes.Where(x => x.VehicleConfigID.Equals(configID)).ToList();
                    db.VehicleConfigAttributes.DeleteAllOnSubmit(vattrs);
                    db.SubmitChanges();

                    db.VehicleConfigs.DeleteOnSubmit(config);
                    db.SubmitChanges();
                }
            }
        }

        public List<ACESBaseVehicle> GetVehicles(int makeid, int modelid, int partID = 0) {
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
                            vehiclePart = bv.vcdb_Vehicles.Where(x => x.SubModelID.Equals(null) && x.ConfigID.Equals(null)).Select(x => x.vcdb_VehicleParts.Where(y => y.PartNumber.Equals(partID)).FirstOrDefault()).FirstOrDefault(),
                            Submodels = (from v in bv.vcdb_Vehicles
                                         where v.SubModelID != null
                                         group v by v.Submodel into s
                                         select new ACESSubmodel {
                                             SubmodelID = s.Key.ID,
                                             submodel = s.Key,
                                             vehiclePart = bv.vcdb_Vehicles.Where(x => x.SubModelID.Equals(s.Key.ID) && x.ConfigID.Equals(null)).Select(x => x.vcdb_VehicleParts.Where(y => y.PartNumber.Equals(partID)).FirstOrDefault()).FirstOrDefault(),
                                             vehicles = (from ve in bv.vcdb_Vehicles
                                                         where ve.SubModelID.Equals(s.Key.ID)
                                                         select new ACESVehicle {
                                                             ID = ve.ID,
                                                             configs = ve.VehicleConfig.VehicleConfigAttributes.Select(x => x.ConfigAttribute).OrderBy(x => x.ConfigAttributeType.name).ToList<ConfigAttribute>(),
                                                             vehiclePart = ve.vcdb_VehicleParts.Where(x => x.PartNumber.Equals(partID)).FirstOrDefault()
                                                         }).OrderBy(x => x.configs.Count).ToList<ACESVehicle>(),
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
                        if (v.configs.Any(x => x.vcdbID == null) || abv.AAIABaseVehicleID == null || sm.submodel.AAIASubmodelID == null) {
                            v.vcdb = false;
                        } else {
                            v.vcdb = ValidateVehicleToVCDB((int)abv.AAIABaseVehicleID, (int)sm.submodel.AAIASubmodelID, v.configs);
                        }
                    }
                }
            }
            return vehicles;
        }

        public List<ACESBaseVehicle> GetVehiclesByPart(int partID) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<ACESBaseVehicle> vehicles = new List<ACESBaseVehicle>();
            vehicles = (from bv in db.BaseVehicles
                        where bv.vcdb_Vehicles.Any(x => x.vcdb_VehicleParts.Any(y => y.PartNumber.Equals(partID)))
                        select new ACESBaseVehicle {
                            ID = bv.ID,
                            AAIABaseVehicleID = bv.AAIABaseVehicleID,
                            YearID = bv.YearID,
                            Make = bv.vcdb_Make,
                            Model = bv.vcdb_Model,
                            vehiclePart = bv.vcdb_Vehicles.Where(x => x.SubModelID.Equals(null) && x.ConfigID.Equals(null)).Select(x => x.vcdb_VehicleParts.Where(y => y.PartNumber.Equals(partID)).FirstOrDefault()).FirstOrDefault(),
                            Submodels = (from v in bv.vcdb_Vehicles
                                         where v.SubModelID != null
                                         group v by v.Submodel into s
                                         select new ACESSubmodel {
                                             SubmodelID = s.Key.ID,
                                             submodel = s.Key,
                                             vehiclePart = s.Where(x => x.SubModelID.Equals(s.Key.ID) && x.ConfigID.Equals(null)).Select(x => x.vcdb_VehicleParts.Where(y => y.PartNumber.Equals(partID)).FirstOrDefault()).FirstOrDefault(),
                                             vehicles = (from ve in bv.vcdb_Vehicles
                                                         where ve.SubModelID.Equals(s.Key.ID)
                                                         select new ACESVehicle {
                                                             ID = ve.ID,
                                                             configs = ve.VehicleConfig.VehicleConfigAttributes.Select(x => x.ConfigAttribute).OrderBy(x => x.ConfigAttributeType.name).ToList<ConfigAttribute>(),
                                                             vehiclePart = ve.vcdb_VehicleParts.Where(x => x.PartNumber.Equals(partID)).FirstOrDefault(),
                                                         }).OrderBy(x => x.configs.Count).ToList<ACESVehicle>(),
                                             configlist = (from c in bv.vcdb_Vehicles
                                                           join vc in db.VehicleConfigAttributes on c.ConfigID equals vc.VehicleConfigID
                                                           where c.SubModelID.Equals(s.Key.ID) && c.vcdb_VehicleParts.Where(x => x.PartNumber.Equals(partID)).Count() > 0
                                                           select vc.ConfigAttribute.ConfigAttributeType).Distinct().OrderBy(x => x.name).ToList<ConfigAttributeType>()
                                         }).OrderBy(x => x.submodel.SubmodelName).ToList<ACESSubmodel>(),
                        }).Distinct().OrderByDescending(x => x.YearID).ToList<ACESBaseVehicle>();
            foreach (ACESBaseVehicle abv in vehicles) {
                foreach (ACESSubmodel sm in abv.Submodels) {
                    sm.vcdb = vcdb.Vehicles.Where(x => x.BaseVehicleID.Equals(abv.AAIABaseVehicleID) && x.SubmodelID.Equals(sm.submodel.AAIASubmodelID)).ToList<AAIA.Vehicle>().Count > 0;
                    foreach (ACESVehicle v in sm.vehicles) {
                        if (v.configs.Any(x => x.vcdbID == null) || abv.AAIABaseVehicleID == null || sm.submodel.AAIASubmodelID == null) {
                            v.vcdb = false;
                        } else {
                            v.vcdb = ValidateVehicleToVCDB((int)abv.AAIABaseVehicleID, (int)sm.submodel.AAIASubmodelID, v.configs);
                        }
                    }
                }
            }
            return vehicles;
        }
        
        public ACESBaseVehicle GetVehicle(int basevehicleID, int submodelID) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            ACESBaseVehicle vehicle = new ACESBaseVehicle();
            vehicle = (from bv in db.BaseVehicles
                        where bv.ID.Equals(basevehicleID)
                        select new ACESBaseVehicle {
                            ID = bv.ID,
                            AAIABaseVehicleID = bv.AAIABaseVehicleID,
                            YearID = bv.YearID,
                            Make = bv.vcdb_Make,
                            Model = bv.vcdb_Model,
                            Submodels = (from v in bv.vcdb_Vehicles
                                         where v.SubModelID.Equals(submodelID)
                                         group v by v.Submodel into s
                                         select new ACESSubmodel {
                                             SubmodelID = s.Key.ID,
                                             submodel = s.Key,
                                             vehicles = (from ve in bv.vcdb_Vehicles
                                                         where ve.SubModelID.Equals(s.Key.ID)
                                                         select new ACESVehicle {
                                                             ID = ve.ID,
                                                             configs = ve.VehicleConfig.VehicleConfigAttributes.Select(x => x.ConfigAttribute).OrderBy(x => x.ConfigAttributeType.name).ToList<ConfigAttribute>()
                                                         }).OrderBy(x => x.configs.Count).ToList<ACESVehicle>(),
                                             configlist = (from c in bv.vcdb_Vehicles
                                                           join vc in db.VehicleConfigAttributes on c.ConfigID equals vc.VehicleConfigID
                                                           where c.SubModelID.Equals(s.Key.ID)
                                                           select vc.ConfigAttribute.ConfigAttributeType).Distinct().OrderBy(x => x.name).ToList<ConfigAttributeType>()
                                         }).OrderBy(x => x.submodel.SubmodelName).ToList<ACESSubmodel>(),
                        }).First<ACESBaseVehicle>();
            foreach (ACESSubmodel sm in vehicle.Submodels) {
                sm.vcdb = vcdb.Vehicles.Where(x => x.BaseVehicleID.Equals(vehicle.AAIABaseVehicleID) && x.SubmodelID.Equals(sm.submodel.AAIASubmodelID)).ToList<AAIA.Vehicle>().Count > 0;
                foreach (ACESVehicle v in sm.vehicles) {
                    if (v.configs.Any(x => x.vcdbID == null) || vehicle.AAIABaseVehicleID == null || sm.submodel.AAIASubmodelID == null) {
                        v.vcdb = false;
                    } else {
                        v.vcdb = ValidateVehicleToVCDB((int)vehicle.AAIABaseVehicleID, (int)sm.submodel.AAIASubmodelID, v.configs);
                    }
                }
            }
            return vehicle;
        }
        
        public bool ValidateVehicleToVCDB(int BaseVehicleID, int submodelID, List<ConfigAttribute> configs, bool AAIAVals = true) {
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            bool isValid = true;
            if (!AAIAVals) {
                CurtDevDataContext db = new CurtDevDataContext();
                try {
                    BaseVehicleID = db.BaseVehicles.Where(x => x.ID.Equals(BaseVehicleID) && x.AAIABaseVehicleID != null).Select(x => (int)x.AAIABaseVehicleID).First();
                    submodelID = db.Submodels.Where(x => x.ID.Equals(submodelID) && x.AAIASubmodelID != null).Select(x => (int)x.AAIASubmodelID).First();
                } catch {
                    isValid = false;
                }
            }
            if (isValid) {
                List<int> vehicleIDs = vcdb.Vehicles.Where(x => x.SubmodelID.Equals(submodelID) && x.BaseVehicleID.Equals(BaseVehicleID)).Select(x => x.VehicleID).ToList<int>();
                var predicate = PredicateBuilder.True<AAIA.VehicleConfig>();
                predicate = predicate.And(p => vehicleIDs.Contains(p.VehicleID));
                foreach (ConfigAttribute ca in configs) {
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
                        case "Aspiration":
                            predicate = predicate.And(p => p.EngineConfig.AspirationID.Equals(ca.vcdbID));
                            break;
                        case "BedType":
                            predicate = predicate.And(p => p.BedConfig.BedTypeID.Equals(ca.vcdbID));
                            break;
                        case "BrakeABS":
                            predicate = predicate.And(p => p.BrakeConfig.BrakeABSID.Equals(ca.vcdbID));
                            break;
                        case "BrakeSystem":
                            predicate = predicate.And(p => p.BrakeConfig.BrakeSystemID.Equals(ca.vcdbID));
                            break;
                        case "CylinderHeadType":
                            predicate = predicate.And(p => p.EngineConfig.CylinderHeadTypeID.Equals(ca.vcdbID));
                            break;
                        case "EngineDesignation":
                            predicate = predicate.And(p => p.EngineConfig.EngineDesignationID.Equals(ca.vcdbID));
                            break;
                        case "EngineMfr":
                            predicate = predicate.And(p => p.EngineConfig.EngineMfrID.Equals(ca.vcdbID));
                            break;
                        case "EngineVersion":
                            predicate = predicate.And(p => p.EngineConfig.EngineVersionID.Equals(ca.vcdbID));
                            break;
                        case "EngineVIN":
                            predicate = predicate.And(p => p.EngineConfig.EngineVINID.Equals(ca.vcdbID));
                            break;
                        case "FrontBrakeType":
                            predicate = predicate.And(p => p.BrakeConfig.FrontBrakeTypeID.Equals(ca.vcdbID));
                            break;
                        case "FrontSpringType":
                            predicate = predicate.And(p => p.SpringTypeConfig.FrontSpringTypeID.Equals(ca.vcdbID));
                            break;
                        case "FuelDeliverySubType":
                            predicate = predicate.And(p => p.EngineConfig.FuelDeliveryConfig.FuelDeliverySubTypeID.Equals(ca.vcdbID));
                            break;
                        case "FuelDeliveryType":
                            predicate = predicate.And(p => p.EngineConfig.FuelDeliveryConfig.FuelDeliveryTypeID.Equals(ca.vcdbID));
                            break;
                        case "FuelSystemControlType":
                            predicate = predicate.And(p => p.EngineConfig.FuelDeliveryConfig.FuelSystemControlTypeID.Equals(ca.vcdbID));
                            break;
                        case "FuelSystemDesign":
                            predicate = predicate.And(p => p.EngineConfig.FuelDeliveryConfig.FuelSystemDesignID.Equals(ca.vcdbID));
                            break;
                        case "IgnitionSystemType":
                            predicate = predicate.And(p => p.EngineConfig.IgnitionSystemTypeID.Equals(ca.vcdbID));
                            break;
                        case "MfrBodyCode":
                            predicate = predicate.And(p => p.MfrBodyCodeID.Equals(ca.vcdbID));
                            break;
                        case "PowerOutput":
                            predicate = predicate.And(p => p.EngineConfig.PowerOutputID.Equals(ca.vcdbID));
                            break;
                        case "RearBrakeType":
                            predicate = predicate.And(p => p.BrakeConfig.RearBrakeTypeID.Equals(ca.vcdbID));
                            break;
                        case "RearSpringType":
                            predicate = predicate.And(p => p.SpringTypeConfig.RearSpringTypeID.Equals(ca.vcdbID));
                            break;
                        case "SteeringSystem":
                            predicate = predicate.And(p => p.SteeringConfig.SteeringSystemID.Equals(ca.vcdbID));
                            break;
                        case "SteeringType":
                            predicate = predicate.And(p => p.SteeringConfig.SteeringTypeID.Equals(ca.vcdbID));
                            break;
                        case "TransElecControlled":
                            predicate = predicate.And(p => p.Transmission.ElecControlled.ElecControlledID.Equals(ca.vcdbID));
                            break;
                        case "Transmission":
                            predicate = predicate.And(p => p.TransmissionID.Equals(ca.vcdbID));
                            break;
                        case "TransmissionBase":
                            predicate = predicate.And(p => p.Transmission.TransmissionBaseID.Equals(ca.vcdbID));
                            break;
                        case "TransmissionControlType":
                            predicate = predicate.And(p => p.Transmission.TransmissionBase.TransmissionControlTypeID.Equals(ca.vcdbID));
                            break;
                        case "TransmissionMfrCode":
                            predicate = predicate.And(p => p.Transmission.TransmissionMfrCodeID.Equals(ca.vcdbID));
                            break;
                        case "TransmissionNumSpeeds":
                            predicate = predicate.And(p => p.Transmission.TransmissionBase.TransmissionNumSpeedsID.Equals(ca.vcdbID));
                            break;
                        case "TransmissionType":
                            predicate = predicate.And(p => p.Transmission.TransmissionBase.TransmissionTypeID.Equals(ca.vcdbID));
                            break;
                        case "ValvesPerEngine":
                            predicate = predicate.And(p => p.EngineConfig.ValvesID.Equals(ca.vcdbID));
                            break;
                        default:
                            isValid = false;
                            break;
                    }
                    if (isValid) {
                        // run query
                        List<AAIA.VehicleConfig> vconfigs = vcdb.VehicleConfigs.Where(predicate).ToList<AAIA.VehicleConfig>();
                        isValid = vconfigs.Count > 0;
                    }
                }
            }
            return isValid;
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

        internal ACESConfigs getVehicleConfigs(int BaseVehicleID, int SubmodelID) {
            ACESConfigs acesconfigs = new ACESConfigs();
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            try {
                BaseVehicle bv = db.BaseVehicles.Where(x => x.ID.Equals(BaseVehicleID)).First<BaseVehicle>();
                Submodel submodel = db.Submodels.Where(x => x.ID.Equals(SubmodelID)).First<Submodel>();
                List<ConfigAttributeType> attrtypes = db.ConfigAttributeTypes.Where(x => x.AcesTypeID != null).OrderBy(x => x.sort).ToList<ConfigAttributeType>();
                List<AAIA.VehicleConfig> configs = vcdb.VehicleConfigs.Where(x => x.Vehicle.BaseVehicleID.Equals(bv.AAIABaseVehicleID) && x.Vehicle.SubmodelID.Equals(submodel.AAIASubmodelID)).Distinct().OrderBy(x => x.BodyStyleConfig.BodyTypeID).ToList<AAIA.VehicleConfig>();
                List<ACESVehicleConfig> vehicleconfigs = new List<ACESVehicleConfig>();
                foreach (ConfigAttributeType type in attrtypes) {
                    switch (type.name) {
                        case "Body Type":
                            type.count = configs.Select(x => x.BodyStyleConfig.BodyTypeID).Distinct().Count();
                            break;
                        case "Number of Doors":
                            type.count = configs.Select(x => x.BodyStyleConfig.BodyNumDoorsID).Distinct().Count();
                            break;
                        case "Drive Type":
                            type.count = configs.Select(x => x.DriveTypeID).Distinct().Count();
                            break;
                        case "Bed Length":
                            type.count = configs.Select(x => x.BedConfig.BedLengthID).Distinct().Count();
                            break;
                        case "Wheel Base":
                            type.count = configs.Select(x => x.WheelbaseID).Distinct().Count();
                            break;
                        case "Engine":
                            type.count = configs.Select(x => x.EngineConfig.EngineBaseID).Distinct().Count();
                            break;
                        case "Fuel Type":
                            type.count = configs.Select(x => x.EngineConfig.FuelTypeID).Distinct().Count();
                            break;
                        case "Aspiration":
                            type.count = configs.Select(x => x.EngineConfig.AspirationID).Distinct().Count();
                            break;
                        case "Bed Type":
                            type.count = configs.Select(x => x.BedConfig.BedTypeID).Distinct().Count();
                            break;
                        case "Brake ABS":
                            type.count = configs.Select(x => x.BrakeConfig.BrakeABSID).Distinct().Count();
                            break;
                        case "Brake System":
                            type.count = configs.Select(x => x.BrakeConfig.BrakeSystemID).Distinct().Count();
                            break;
                        case "Cylinder Head Type":
                            type.count = configs.Select(x => x.EngineConfig.CylinderHeadTypeID).Distinct().Count();
                            break;
                        case "Engine Designation":
                            type.count = configs.Select(x => x.EngineConfig.EngineDesignationID).Distinct().Count();
                            break;
                        case "Engine Manufacturer":
                            type.count = configs.Select(x => x.EngineConfig.EngineMfrID).Distinct().Count();
                            break;
                        case "Engine Version":
                            type.count = configs.Select(x => x.EngineConfig.EngineVersionID).Distinct().Count();
                            break;
                        case "Engine VIN":
                            type.count = configs.Select(x => x.EngineConfig.EngineVINID).Distinct().Count();
                            break;
                        case "Front Brake Type":
                            type.count = configs.Select(x => x.BrakeConfig.FrontBrakeTypeID).Distinct().Count();
                            break;
                        case "Front Spring Type":
                            type.count = configs.Select(x => x.SpringTypeConfig.FrontSpringTypeID).Distinct().Count();
                            break;
                        case "Fuel Delivery Sub-Type":
                            type.count = configs.Select(x => x.EngineConfig.FuelDeliveryConfig.FuelDeliverySubTypeID).Distinct().Count();
                            break;
                        case "Fuel Delivery Type":
                            type.count = configs.Select(x => x.EngineConfig.FuelDeliveryConfig.FuelDeliveryTypeID).Distinct().Count();
                            break;
                        case "Fuel System Control Type":
                            type.count = configs.Select(x => x.EngineConfig.FuelDeliveryConfig.FuelSystemControlTypeID).Distinct().Count();
                            break;
                        case "Fuel System Design":
                            type.count = configs.Select(x => x.EngineConfig.FuelDeliveryConfig.FuelSystemDesignID).Distinct().Count();
                            break;
                        case "Ignition System Type":
                            type.count = configs.Select(x => x.EngineConfig.IgnitionSystemTypeID).Distinct().Count();
                            break;
                        case "Manufacturer Body Code":
                            type.count = configs.Select(x => x.MfrBodyCodeID).Distinct().Count();
                            break;
                        case "Power Output":
                            type.count = configs.Select(x => x.EngineConfig.PowerOutputID).Distinct().Count();
                            break;
                        case "Rear Brake Type":
                            type.count = configs.Select(x => x.BrakeConfig.RearBrakeTypeID).Distinct().Count();
                            break;
                        case "Rear Spring Type":
                            type.count = configs.Select(x => x.SpringTypeConfig.RearSpringTypeID).Distinct().Count();
                            break;
                        case "Steering System":
                            type.count = configs.Select(x => x.SteeringConfig.SteeringSystemID).Distinct().Count();
                            break;
                        case "Steering Type":
                            type.count = configs.Select(x => x.SteeringConfig.SteeringTypeID).Distinct().Count();
                            break;
                        case "Tranmission Electronic Controlled":
                            type.count = configs.Select(x => x.Transmission.TransmissionElecControlledID).Distinct().Count();
                            break;
                        case "Transmission":
                            type.count = configs.Select(x => x.TransmissionID).Distinct().Count();
                            break;
                        case "Transmission Base":
                            type.count = configs.Select(x => x.Transmission.TransmissionBaseID).Distinct().Count();
                            break;
                        /*case "Transmission Control Type":
                            type.count = configs.Select(x => x.Transmission.TransmissionBase.TransmissionControlTypeID).Distinct().Count();
                            break;*/
                        case "Tranmission Manufacturer Code":
                            type.count = configs.Select(x => x.Transmission.TransmissionMfrCodeID).Distinct().Count();
                            break;
                        /*case "Transmission Number of Speeds":
                            type.count = configs.Select(x => x.Transmission.TransmissionBase.TransmissionNumSpeedsID).Distinct().Count();
                            break;*/
                        /*case "Transmission Type":
                            type.count = configs.Select(x => x.Transmission.TransmissionBase.TransmissionTypeID).Distinct().Count();
                            break;*/
                        case "Valves Per Engine":
                            type.count = configs.Select(x => x.EngineConfig.ValvesID).Distinct().Count();
                            break;
                        default:
                            type.count = 0;
                            break;
                    }
                    acesconfigs.types = attrtypes;
                }
                foreach (AAIA.VehicleConfig c in configs) {
                    vehicleconfigs.Add(ConvertConfig(c, attrtypes));
                }
                acesconfigs.configs = vehicleconfigs;
            } catch { }
            return acesconfigs;
        }

        internal ACESConfigs getVehicleConfigs(int vehicleID) {
            ACESConfigs acesconfigs = new ACESConfigs();
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            try {
                // get vehicle by ID
                vcdb_Vehicle v = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
                // get all the vehicle configs for the basevehicle and submodel
                ACESConfigs fullacesconfigs = getVehicleConfigs(v.BaseVehicleID, (int)v.SubModelID);
                // set the full list of types
                acesconfigs.types = fullacesconfigs.types;
                // initialize configs as a new list
                acesconfigs.configs = new List<ACESVehicleConfig>();
                List<VehicleConfigAttribute> vattrs = new List<VehicleConfigAttribute>();
                if (v.ConfigID != null) {
                    // get the ACES ConfigAttributes for the config of the vehicle if it exists
                    vattrs = v.VehicleConfig.VehicleConfigAttributes.Where(x => x.ConfigAttribute.ConfigAttributeType.AcesTypeID != null).ToList();
                }
                if (vattrs.Count > 0) {
                    // loop through each of the ACES Vehicle Configs
                    foreach (ACESVehicleConfig config in fullacesconfigs.configs) {
                        bool add = true;
                        // loop through each of the ConfigAttributes to check for matches
                        foreach (VehicleConfigAttribute attr in vattrs) {
                            if (config.attributes.Any(x => x.ConfigAttributeTypeID.Equals(attr.ConfigAttribute.ConfigAttributeTypeID) && x.vcdbID.Equals(attr.ConfigAttribute.vcdbID))) {
                                // if there exists a ConfigAttribute that matches on type and vcdbID, make sure it's not shown
                                ConfigAttributeType t = acesconfigs.types.Where(x => x.ID.Equals(attr.ConfigAttribute.ConfigAttributeTypeID)).First();
                                t.count = 0;
                            } else {
                                add = false;
                            }
                        }
                        if (add) {
                            acesconfigs.configs.Add(config);
                        }
                    }
                    acesconfigs.clearDuplicates();
                } else {
                    acesconfigs = fullacesconfigs;
                }
            } catch { }
            return acesconfigs;
        }

        public List<ConfigAttributeType> GetConfigAttributeTypes() {
            List<ConfigAttributeType> configs = new List<ConfigAttributeType>();
            CurtDevDataContext db = new CurtDevDataContext();
            configs = db.ConfigAttributeTypes.OrderBy(x => x.sort).ToList<ConfigAttributeType>();
            return configs;
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

        private ACESVehicleConfig ConvertConfig(AAIA.VehicleConfig config, List<ConfigAttributeType> types) {
            ACESVehicleConfig vconfig = new ACESVehicleConfig();
            vconfig.attributes = new List<ConfigAttribute>();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            foreach (ConfigAttributeType type in types) {
                ConfigAttribute ca = new ConfigAttribute();
                ca.ConfigAttributeType = type;
                switch (type.name) {
                    case "Body Type":
                        ca.vcdbID = config.BodyStyleConfig.BodyTypeID;
                        ca.value = config.BodyStyleConfig.BodyType.BodyTypeName.Trim();
                        break;
                    case "Number of Doors":
                        ca.vcdbID = config.BodyStyleConfig.BodyNumDoorsID;
                        ca.value = config.BodyStyleConfig.BodyNumDoor.BodyNumDoors.Trim();
                        break;
                    case "Drive Type":
                        ca.vcdbID = config.DriveTypeID;
                        ca.value = config.DriveType.DriveTypeName.Trim();
                        break;
                    case "Bed Length":
                        ca.vcdbID = config.BedConfig.BedLengthID;
                        ca.value = config.BedConfig.BedLength.BedLength1.Trim();
                        break;
                    case "Wheel Base":
                        ca.vcdbID = config.WheelbaseID;
                        ca.value = config.WheelBase.WheelBase1.Trim();
                        break;
                    case "Engine":
                        ca.vcdbID = config.EngineConfig.EngineBaseID;
                        ca.value = config.EngineConfig.EngineBase.Liter.Trim() + "L " + config.EngineConfig.EngineBase.BlockType.Trim() + config.EngineConfig.EngineBase.Cylinders.Trim();
                        break;
                    case "Fuel Type":
                        ca.vcdbID = config.EngineConfig.FuelTypeID;
                        ca.value = config.EngineConfig.FuelType.FuelTypeName.Trim();
                        break;
                    case "Aspiration":
                        ca.vcdbID = config.EngineConfig.AspirationID;
                        ca.value = config.EngineConfig.Aspiration.AspirationName.Trim();
                        break;
                    case "Bed Type":
                        ca.vcdbID = config.BedConfig.BedTypeID;
                        ca.value = config.BedConfig.BedType.BedTypeName.Trim();
                        break;
                    case "Brake ABS":
                        ca.vcdbID = config.BrakeConfig.BrakeABSID;
                        ca.value = config.BrakeConfig.BrakeAB.BrakeABSName.Trim();
                        break;
                    case "Brake System":
                        ca.vcdbID = config.BrakeConfig.BrakeABSID;
                        ca.value = config.BrakeConfig.BrakeAB.BrakeABSName.Trim();
                        break;
                    case "Cylinder Head Type":
                        ca.vcdbID = config.EngineConfig.CylinderHeadTypeID;
                        ca.value = config.EngineConfig.CylinderHeadType.CylinderHeadTypeName.Trim();
                        break;
                    case "Engine Designation":
                        ca.vcdbID = config.EngineConfig.EngineDesignationID;
                        ca.value = config.EngineConfig.EngineDesignation.EngineDesignationName.Trim();
                        break;
                    case "Engine Manufacturer":
                        ca.vcdbID = config.EngineConfig.EngineMfrID;
                        ca.value = config.EngineConfig.Mfr.MfrName.Trim();
                        break;
                    case "Engine Version":
                        ca.vcdbID = config.EngineConfig.EngineVersionID;
                        ca.value = config.EngineConfig.EngineVersion.EngineVersion1.Trim();
                        break;
                    case "Engine VIN":
                        ca.vcdbID = config.EngineConfig.EngineVINID;
                        ca.value = config.EngineConfig.EngineVIN.EngineVINName.Trim();
                        break;
                    case "Front Brake Type":
                        ca.vcdbID = config.BrakeConfig.FrontBrakeTypeID;
                        ca.value = config.BrakeConfig.FrontBrakeType.BrakeTypeName.Trim();
                        break;
                    case "Front Spring Type":
                        ca.vcdbID = config.SpringTypeConfig.FrontSpringTypeID;
                        ca.value = config.SpringTypeConfig.FrontSpringType.SpringTypeName.Trim();
                        break;
                    case "Fuel Delivery Sub-Type":
                        ca.vcdbID = config.EngineConfig.FuelDeliveryConfig.FuelDeliverySubTypeID;
                        ca.value = config.EngineConfig.FuelDeliveryConfig.FuelDeliverySubType.FuelDeliverySubTypeName.Trim();
                        break;
                    case "Fuel Delivery Type":
                        ca.vcdbID = config.EngineConfig.FuelDeliveryConfig.FuelDeliveryTypeID;
                        ca.value = config.EngineConfig.FuelDeliveryConfig.FuelDeliveryType.FuelDeliveryTypeName.Trim();
                        break;
                    case "Fuel System Control Type":
                        ca.vcdbID = config.EngineConfig.FuelDeliveryConfig.FuelSystemControlTypeID;
                        ca.value = config.EngineConfig.FuelDeliveryConfig.FuelSystemControlType.FuelSystemControlTypeName.Trim();
                        break;
                    case "Fuel System Design":
                        ca.vcdbID = config.EngineConfig.FuelDeliveryConfig.FuelSystemDesignID;
                        ca.value = config.EngineConfig.FuelDeliveryConfig.FuelSystemDesign.FuelSystemDesignName.Trim();
                        break;
                    case "Ignition System Type":
                        ca.vcdbID = config.EngineConfig.IgnitionSystemTypeID;
                        ca.value = config.EngineConfig.IgnitionSystemType.IgnitionSystemTypeName.Trim();
                        break;
                    case "Manufacturer Body Code":
                        ca.vcdbID = config.MfrBodyCodeID;
                        ca.value = config.MfrBodyCode.MfrBodyCodeName.Trim();
                        break;
                    case "Power Output":
                        ca.vcdbID = config.EngineConfig.PowerOutputID;
                        ca.value = vcdb.PowerOutputs.Where(x => x.PowerOutputID.Equals(config.EngineConfig.PowerOutputID)).Select(x => x.HorsePower).First().Trim();
                        break;
                    case "Rear Brake Type":
                        ca.vcdbID = config.BrakeConfig.RearBrakeTypeID;
                        ca.value = config.BrakeConfig.RearBrakeType.BrakeTypeName.Trim();
                        break;
                    case "Rear Spring Type":
                        ca.vcdbID = config.SpringTypeConfig.RearSpringTypeID;
                        ca.value = config.SpringTypeConfig.RearSpringType.SpringTypeName.Trim();
                        break;
                    case "Steering System":
                        ca.vcdbID = config.SteeringConfig.SteeringSystemID;
                        ca.value = config.SteeringConfig.SteeringSystem.SteeringSystemName.Trim();
                        break;
                    case "Steering Type":
                        ca.vcdbID = config.SteeringConfig.SteeringTypeID;
                        ca.value = config.SteeringConfig.SteeringType.SteeringTypeName.Trim();
                        break;
                    case "Tranmission Electronic Controlled":
                        ca.vcdbID = config.Transmission.TransmissionElecControlledID;
                        ca.value = config.Transmission.ElecControlled.ElecControlled1.Trim();
                        break;
                    /*case "Transmission":
                        ca.vcdbID = config.TransmissionID;
                        ca.value = "transmission";
                        break;*/
                    case "Transmission Base":
                        ca.vcdbID = config.Transmission.TransmissionBaseID;
                        ca.value = config.Transmission.TransmissionBase.TransmissionNumSpeed.TransmissionNumSpeeds.Trim() + "sp " + config.Transmission.TransmissionBase.TransmissionControlType.TransmissionControlTypeName.Trim() + " " + config.Transmission.TransmissionBase.TransmissionType.TransmissionTypeName.Trim();
                        break;
                    /*case "Transmission Control Type":
                        ca.vcdbID = config.Transmission.TransmissionBase.TransmissionControlTypeID;
                        ca.value = config.Transmission.TransmissionBase.TransmissionControlType.TransmissionControlTypeName.Trim();
                        break;*/
                    case "Tranmission Manufacturer Code":
                        ca.vcdbID = config.Transmission.TransmissionMfrCodeID;
                        ca.value = config.Transmission.TransmissionMfrCode.TransmissionMfrCode1.Trim();
                        break;
                    /*case "Transmission Number of Speeds":
                        ca.vcdbID = config.Transmission.TransmissionBase.TransmissionNumSpeedsID;
                        ca.value = config.Transmission.TransmissionBase.TransmissionNumSpeed.TransmissionNumSpeeds.Trim();
                        break;*/
                    /*case "Transmission Type":
                        ca.vcdbID = config.Transmission.TransmissionBase.TransmissionTypeID;
                        ca.value = config.Transmission.TransmissionBase.TransmissionType.TransmissionTypeName.Trim();
                        break;*/
                    case "Valves Per Engine":
                        ca.vcdbID = config.EngineConfig.ValvesID;
                        ca.value = config.EngineConfig.Valve.ValvesPerEngine.Trim();
                        break;
                    default:
                        type.count = 0;
                        break;
                }
                vconfig.attributes.Add(ca);

            }
            return vconfig;
        }

        public ACESBaseVehicle addConfig(int BaseVehicleID, int SubmodelID, List<int> configids) {
            CurtDevDataContext db = new CurtDevDataContext();
            ACESConfigs acesconfigs = new ACESConfigs();
            List<vcdb_Vehicle> newVehicles = new List<vcdb_Vehicle>();
            acesconfigs = new ACES().getVehicleConfigs(BaseVehicleID, SubmodelID);
            //Dictionary<int, List<ConfigAttribute>> attributeSet = new Dictionary<int, List<ConfigAttribute>>();
            List<List<ConfigAttribute>> attributeSet = new List<List<ConfigAttribute>>();
            foreach (int id in configids) {
                List<ConfigAttribute> attributes = acesconfigs.configs.SelectMany(x => x.attributes.Where(y => y.ConfigAttributeTypeID.Equals(id))).ToList<ConfigAttribute>().Distinct(new ConfigAttributeComparer()).ToList<ConfigAttribute>();
                attributeSet.Add(attributes);
            }
            List<ACESVehicleConfig> vehicleConfigs = new List<ACESVehicleConfig>();
            vehicleConfigs = buildConfig(attributeSet, vehicleConfigs,0);
            List<ACESVehicleConfig> validConfigs = new List<ACESVehicleConfig>();
            foreach (ACESVehicleConfig config in vehicleConfigs) {
                if(ValidateVehicleToVCDB(BaseVehicleID,SubmodelID,config.attributes,false)) {
                    validConfigs.Add(config);
                }
            }
            foreach (ACESVehicleConfig config in validConfigs) {
                config.attributes = getOrCreateAttributes(config.attributes);
                VehicleConfig vconfig = new VehicleConfig();
                // Add Config
                try {
                    List<int> attrIDs = config.attributes.Select(x => x.ID).ToList();
                    List<VehicleConfig> configlist = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(BaseVehicleID) && x.SubModelID.Equals(SubmodelID) && x.ConfigID != null).Select(x => x.VehicleConfig).ToList<VehicleConfig>();
                    foreach (VehicleConfig cl in configlist) {
                        List<int> vattrIDs = cl.VehicleConfigAttributes.Select(x => x.AttributeID).ToList();
                        if (attrIDs.Except(vattrIDs).Count() == 0 && vattrIDs.Except(attrIDs).Count() == 0) {
                            vconfig = cl;
                        }
                    }
                    if (vconfig == null || vconfig.ID == 0) {
                        throw new Exception("No Vehicle");
                    }
                } catch {
                    db.VehicleConfigs.InsertOnSubmit(vconfig);
                    db.SubmitChanges();
                    List<VehicleConfigAttribute> vcas = new List<VehicleConfigAttribute>();
                    foreach (ConfigAttribute ca in config.attributes) {
                        VehicleConfigAttribute vca = new VehicleConfigAttribute {
                            AttributeID = ca.ID,
                            VehicleConfigID = vconfig.ID
                        };
                        vcas.Add(vca);
                    }
                    db.VehicleConfigAttributes.InsertAllOnSubmit(vcas);
                    db.SubmitChanges();
                }
                // Add Vehicle
                vcdb_Vehicle vehicle = new vcdb_Vehicle();
                try {
                    vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(BaseVehicleID) && x.SubModelID.Equals(SubmodelID) && x.ConfigID.Equals(vconfig.ID)).First();
                } catch {
                    vehicle = new vcdb_Vehicle {
                        BaseVehicleID = BaseVehicleID,
                        SubModelID = SubmodelID,
                        ConfigID = vconfig.ID
                    };
                    db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                    db.SubmitChanges();
                    newVehicles.Add(vehicle);
                }
            }
            return GetVehicle(BaseVehicleID,SubmodelID);
        }

        public List<ACESVehicleConfig> buildConfig(List<List<ConfigAttribute>> attributeSet, List<ACESVehicleConfig> configs, int level, ACESVehicleConfig config = null) {
            foreach (ConfigAttribute attribute in attributeSet[level]) {
                ACESVehicleConfig newconfig = new ACESVehicleConfig();
                newconfig.attributes = new List<ConfigAttribute>();
                newconfig.attributes.Add(attribute);
                if (config != null) {
                    newconfig.attributes.AddRange(config.attributes);
                } else {
                    config = new ACESVehicleConfig();
                    config.attributes = new List<ConfigAttribute>();
                }
                if (level == (attributeSet.Count - 1)) {
                    configs.Add(newconfig);
                } else {
                    configs = buildConfig(attributeSet, configs, (level + 1), newconfig);
                }
            }
            return configs;
        }

        public List<ConfigAttribute> getOrCreateAttributes(List<ConfigAttribute> attributes) {
            CurtDevDataContext db = new CurtDevDataContext();
            foreach (ConfigAttribute attr in attributes) {
                try {
                    attr.ID = db.ConfigAttributes.Where(x => x.ConfigAttributeTypeID.Equals(attr.ConfigAttributeTypeID) && x.vcdbID.Equals(attr.vcdbID) && x.value.Equals(attr.value)).Select(x => x.ID).First();
                } catch {
                    ConfigAttribute newattr = new ConfigAttribute {
                        ConfigAttributeTypeID = attr.ConfigAttributeTypeID,
                        value = attr.value,
                        vcdbID = attr.vcdbID,
                        parentID = 0
                    };
                    db.ConfigAttributes.InsertOnSubmit(newattr);
                    db.SubmitChanges();
                    attr.ID = newattr.ID;
                }
            }
            return attributes;
        }

        public ACESBaseVehicle addCustomConfigToVehicle(int vehicleID, int attributeID) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
            if (vehicle.ConfigID == null) {
                // no config yet
                VehicleConfig config = new VehicleConfig();
                db.VehicleConfigs.InsertOnSubmit(config);
                db.SubmitChanges();

                VehicleConfigAttribute vca = new VehicleConfigAttribute {
                    AttributeID = attributeID,
                    VehicleConfigID = config.ID
                };
                db.VehicleConfigAttributes.InsertOnSubmit(vca);
                vehicle.ConfigID = config.ID;
                db.SubmitChanges();
            } else {
                // config exists
                VehicleConfig config = vehicle.VehicleConfig;
                if (config.vcdb_Vehicles.Count == 1) {
                    // Safe to change
                    VehicleConfigAttribute vca = new VehicleConfigAttribute {
                        AttributeID = attributeID,
                        VehicleConfigID = config.ID
                    };
                    db.VehicleConfigAttributes.InsertOnSubmit(vca);
                    db.SubmitChanges();
                } else {
                    // Config is used by more than one vehicle
                    VehicleConfig newConfig = new VehicleConfig();
                    db.VehicleConfigs.InsertOnSubmit(newConfig);
                    db.SubmitChanges();

                    List<VehicleConfigAttribute> newAttributes = new List<VehicleConfigAttribute>();
                    foreach (VehicleConfigAttribute attr in config.VehicleConfigAttributes) {
                        VehicleConfigAttribute vca = new VehicleConfigAttribute {
                            AttributeID = attr.AttributeID,
                            VehicleConfigID = newConfig.ID
                        };
                        newAttributes.Add(vca);
                    }
                    VehicleConfigAttribute newAttribute = new VehicleConfigAttribute {
                        AttributeID = attributeID,
                        VehicleConfigID = newConfig.ID
                    };
                    newAttributes.Add(newAttribute);
                    db.VehicleConfigAttributes.InsertAllOnSubmit(newAttributes);
                    vehicle.ConfigID = newConfig.ID;
                    db.SubmitChanges();
                }
            }
            return GetVehicle(vehicle.BaseVehicleID, (int)vehicle.SubModelID);
        }

        public ACESBaseVehicle addCustomConfig(int vehicleID, int attributeID) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
            vcdb_Vehicle newVehicle = new vcdb_Vehicle {
                BaseVehicleID = vehicle.BaseVehicleID,
                SubModelID = vehicle.SubModelID
            };
            if (vehicle.ConfigID == null) {
                // no config yet
                VehicleConfig config = new VehicleConfig();
                db.VehicleConfigs.InsertOnSubmit(config);
                db.SubmitChanges();

                VehicleConfigAttribute vca = new VehicleConfigAttribute {
                    AttributeID = attributeID,
                    VehicleConfigID = config.ID
                };
                db.VehicleConfigAttributes.InsertOnSubmit(vca);
                db.SubmitChanges();
                newVehicle.ConfigID = config.ID;
            } else {
                // config exists
                VehicleConfig config = vehicle.VehicleConfig;

                VehicleConfig newConfig = new VehicleConfig();
                db.VehicleConfigs.InsertOnSubmit(newConfig);
                db.SubmitChanges();

                List<VehicleConfigAttribute> newAttributes = new List<VehicleConfigAttribute>();
                foreach (VehicleConfigAttribute attr in config.VehicleConfigAttributes) {
                    VehicleConfigAttribute vca = new VehicleConfigAttribute {
                        AttributeID = attr.AttributeID,
                        VehicleConfigID = newConfig.ID
                    };
                    newAttributes.Add(vca);
                }
                VehicleConfigAttribute newAttribute = new VehicleConfigAttribute {
                    AttributeID = attributeID,
                    VehicleConfigID = newConfig.ID
                };
                newAttributes.Add(newAttribute);
                db.VehicleConfigAttributes.InsertAllOnSubmit(newAttributes);
                
                newVehicle.ConfigID = newConfig.ID;
            }
            db.vcdb_Vehicles.InsertOnSubmit(newVehicle);
            db.SubmitChanges();
            return GetVehicle(newVehicle.BaseVehicleID, (int)newVehicle.SubModelID);
        }

        public List<ConfigAttributeType> getNonACESConfigurationTypes(int vehicleID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ConfigAttributeType> alltypes = db.ConfigAttributeTypes.Where(x => x.AcesTypeID == null).OrderBy(x => x.name).ToList();
            List<VehicleConfigAttribute> vehicleattr = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID) && x.ConfigID != null).SelectMany(x => x.VehicleConfig.VehicleConfigAttributes).Distinct().ToList();
            List<ConfigAttributeType> vehicletypes = vehicleattr.Where(x => x.ConfigAttribute.ConfigAttributeType.AcesTypeID == null).Select(x => x.ConfigAttribute.ConfigAttributeType).Distinct().ToList<ConfigAttributeType>();
            List<ConfigAttributeType> types = alltypes.Except(vehicletypes).OrderBy(x => x.name).ToList<ConfigAttributeType>();
            return types;
        }

        public List<ConfigAttribute> GetAttributesByType(int typeID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ConfigAttribute> attributes = db.ConfigAttributes.Where(x => x.ConfigAttributeTypeID.Equals(typeID)).OrderBy(x => x.value).ToList();
            return attributes;
        }

        public int checkVehicleExists(int vehicleID, int attributeID, string method = "remove") {
            int duplicateID = 0;
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            vehicle = new ACES().GetVehicle(vehicleID);
            List<ConfigAttribute> attributelist = attributelist = vehicle.VehicleConfig.VehicleConfigAttributes.Where(x => x.AttributeID != attributeID).Select(x => x.ConfigAttribute).ToList<ConfigAttribute>();
            if (method != "remove") {
                ConfigAttribute ca = db.ConfigAttributes.Where(x => x.ID.Equals(attributeID)).First();
                attributelist.Add(ca);
            }
            List<vcdb_Vehicle> vehicles = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(vehicle.BaseVehicleID) && x.SubModelID.Equals(vehicle.SubModelID)).ToList<vcdb_Vehicle>();
            if (attributelist.Count == 0) {
                // check for a vehicle with no configuration
                try {
                    duplicateID = vehicles.Where(x => x.ConfigID == null).Select(x => x.ID).First();
                } catch { }
            } else {
                foreach (vcdb_Vehicle v in vehicles) {
                    if (v.ConfigID != null) {
                        List<int> attrIDs = attributelist.Select(x => x.ID).ToList();
                        List<int> vattrIDs = v.VehicleConfig.VehicleConfigAttributes.Select(x => x.AttributeID).ToList();
                        if (attrIDs.Count == vattrIDs.Count && attrIDs.Except(vattrIDs).Count().Equals(0)) {
                            duplicateID = v.ID;
                        }
                    }
                }
            }
            return duplicateID;
        }

        public int checkVehicleExists(int vehicleID, int vcdbID, int typeID, string value) {
            int duplicateID = 0;
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            vehicle = new ACES().GetVehicle(vehicleID);
            try {
                // get attribute based on info supplied
                ConfigAttribute ca = db.ConfigAttributes.Where(x => x.vcdbID.Equals(vcdbID) && x.ConfigAttributeTypeID.Equals(typeID)).First();
                List<ConfigAttribute> attributelist = attributelist = vehicle.VehicleConfig.VehicleConfigAttributes.Where(x => x.AttributeID != ca.ID).Select(x => x.ConfigAttribute).ToList<ConfigAttribute>();
                attributelist.Add(ca);
                List<vcdb_Vehicle> vehicles = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(vehicle.BaseVehicleID) && x.SubModelID.Equals(vehicle.SubModelID)).ToList<vcdb_Vehicle>();
                foreach (vcdb_Vehicle v in vehicles) {
                    if (v.ConfigID != null) {
                        List<int> attrIDs = attributelist.Select(x => x.ID).ToList();
                        List<int> vattrIDs = v.VehicleConfig.VehicleConfigAttributes.Select(x => x.AttributeID).ToList();
                        if (attrIDs.Count == vattrIDs.Count && attrIDs.Except(vattrIDs).Count().Equals(0)) {
                            duplicateID = v.ID;
                        }
                    }
                }
            } catch {
                // attribute doesn't exist therefore no duplicate exists
            }
            return duplicateID;
        }

        public ACESBaseVehicle mergeVehicles(int targetID, int currentID, bool deleteCurrent = true) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle targetVehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(targetID)).First();
            vcdb_Vehicle currentVehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(currentID)).First();
            List<vcdb_VehiclePart> currentParts = currentVehicle.vcdb_VehicleParts.ToList();
            List<vcdb_VehiclePart> targetParts = targetVehicle.vcdb_VehicleParts.ToList();
            List<vcdb_VehiclePart> partsToAdd = currentParts.Except(targetParts).ToList();
            foreach (vcdb_VehiclePart part in partsToAdd) {
                vcdb_VehiclePart newPart = new vcdb_VehiclePart {
                    PartNumber = part.PartNumber,
                    VehicleID = targetID
                };
                db.vcdb_VehicleParts.InsertOnSubmit(newPart);
                db.SubmitChanges();

                List<Note> notes = new List<Note>();
                foreach (Note note in part.Notes) {
                    Note newNote = new Note {
                        note1 = note.note1,
                        vehiclePartID = newPart.ID
                    };
                    notes.Add(newNote);
                }
                db.Notes.InsertAllOnSubmit(notes);
                db.SubmitChanges();
            }
            if (deleteCurrent) {
                RemoveVehicle(currentID);
            }

            return GetVehicle(targetVehicle.BaseVehicleID, (int)targetVehicle.SubModelID);
        }

        public ACESBaseVehicle addAttributeToVehicle(int vehicleID, int vcdbID, int typeID, string value) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
            ConfigAttribute ca = new ConfigAttribute();
            try {
                ca = db.ConfigAttributes.Where(x => x.vcdbID.Equals(vcdbID) && x.ConfigAttributeTypeID.Equals(typeID)).First();
            } catch {
                ca = new ConfigAttribute {
                    ConfigAttributeTypeID = typeID,
                    vcdbID = vcdbID,
                    value = value,
                    parentID = 0
                };
                db.ConfigAttributes.InsertOnSubmit(ca);
                db.SubmitChanges();
            }

            if (vehicle.ConfigID == null) {
                VehicleConfig config = new VehicleConfig();
                db.VehicleConfigs.InsertOnSubmit(config);
                db.SubmitChanges();

                VehicleConfigAttribute vca = new VehicleConfigAttribute {
                    AttributeID = ca.ID,
                    VehicleConfigID = config.ID
                };
                db.VehicleConfigAttributes.InsertOnSubmit(vca);
                vehicle.ConfigID = config.ID;
                db.SubmitChanges();
            } else {
                // config exists
                VehicleConfig config = vehicle.VehicleConfig;
                if (config.vcdb_Vehicles.Count == 1) {
                    // Safe to change
                    VehicleConfigAttribute vca = new VehicleConfigAttribute {
                        AttributeID = ca.ID,
                        VehicleConfigID = config.ID
                    };
                    db.VehicleConfigAttributes.InsertOnSubmit(vca);
                    db.SubmitChanges();
                } else {
                    // Config is used by more than one vehicle
                    VehicleConfig newConfig = new VehicleConfig();
                    db.VehicleConfigs.InsertOnSubmit(newConfig);
                    db.SubmitChanges();

                    List<VehicleConfigAttribute> newAttributes = new List<VehicleConfigAttribute>();
                    foreach (VehicleConfigAttribute attr in config.VehicleConfigAttributes) {
                        VehicleConfigAttribute vca = new VehicleConfigAttribute {
                            AttributeID = attr.AttributeID,
                            VehicleConfigID = newConfig.ID
                        };
                        newAttributes.Add(vca);
                    }
                    VehicleConfigAttribute newAttribute = new VehicleConfigAttribute {
                        AttributeID = ca.ID,
                        VehicleConfigID = newConfig.ID
                    };
                    newAttributes.Add(newAttribute);
                    db.VehicleConfigAttributes.InsertAllOnSubmit(newAttributes);
                    vehicle.ConfigID = newConfig.ID;
                    db.SubmitChanges();
                }
            }
            return GetVehicle(vehicle.BaseVehicleID, (int)vehicle.SubModelID);
        }

        public ACESBaseVehicle addAttribute(int vehicleID, int vcdbID, int typeID, string value) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
            vcdb_Vehicle newVehicle = new vcdb_Vehicle {
                BaseVehicleID = vehicle.BaseVehicleID,
                SubModelID = vehicle.SubModelID
            };
            ConfigAttribute ca = new ConfigAttribute();
            try {
                ca = db.ConfigAttributes.Where(x => x.vcdbID.Equals(vcdbID) && x.ConfigAttributeTypeID.Equals(typeID)).First();
            } catch {
                ca = new ConfigAttribute {
                    ConfigAttributeTypeID = typeID,
                    vcdbID = vcdbID,
                    value = value,
                    parentID = 0
                };
                db.ConfigAttributes.InsertOnSubmit(ca);
                db.SubmitChanges();
            }

            if (vehicle.ConfigID == null) {
                VehicleConfig config = new VehicleConfig();
                db.VehicleConfigs.InsertOnSubmit(config);
                db.SubmitChanges();

                VehicleConfigAttribute vca = new VehicleConfigAttribute {
                    AttributeID = ca.ID,
                    VehicleConfigID = config.ID
                };
                db.VehicleConfigAttributes.InsertOnSubmit(vca);
                newVehicle.ConfigID = config.ID;
            } else {
                // config exists
                VehicleConfig config = vehicle.VehicleConfig;

                VehicleConfig newConfig = new VehicleConfig();
                db.VehicleConfigs.InsertOnSubmit(newConfig);
                db.SubmitChanges();

                List<VehicleConfigAttribute> newAttributes = new List<VehicleConfigAttribute>();
                foreach (VehicleConfigAttribute attr in config.VehicleConfigAttributes) {
                    VehicleConfigAttribute vca = new VehicleConfigAttribute {
                        AttributeID = attr.AttributeID,
                        VehicleConfigID = newConfig.ID
                    };
                    newAttributes.Add(vca);
                }
                VehicleConfigAttribute newAttribute = new VehicleConfigAttribute {
                    AttributeID = ca.ID,
                    VehicleConfigID = newConfig.ID
                };
                newAttributes.Add(newAttribute);
                db.VehicleConfigAttributes.InsertAllOnSubmit(newAttributes);
                
                newVehicle.ConfigID = newConfig.ID;
            }
            db.vcdb_Vehicles.InsertOnSubmit(newVehicle);
            db.SubmitChanges();
            return GetVehicle(vehicle.BaseVehicleID, (int)vehicle.SubModelID);
        }

        public ACESBaseVehicle removeAttribute(int vehicleID, int attributeID) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
            VehicleConfigAttribute vca = vehicle.VehicleConfig.VehicleConfigAttributes.Where(x => x.AttributeID.Equals(attributeID)).First();
            int caID = vca.ConfigAttribute.ID;
            db.VehicleConfigAttributes.DeleteOnSubmit(vca);
            db.SubmitChanges();
            ConfigAttribute ca = db.ConfigAttributes.Where(x => x.ID.Equals(caID)).First();
            if (ca.VehicleConfigAttributes.Count == 0) {
                db.ConfigAttributes.DeleteOnSubmit(ca);
                db.SubmitChanges();
            }
            return GetVehicle(vehicle.BaseVehicleID, (int)vehicle.SubModelID);
        }

        public List<vcdb_VehiclePart> GetVehicleParts(int vehicleID) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
            List<vcdb_VehiclePart> parts = vehicle.vcdb_VehicleParts.ToList();
            return parts;
        }

        public List<vcdb_VehiclePart> GetVehicleParts(int baseVehicleID, int submodelID) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            List<vcdb_VehiclePart> parts = new List<vcdb_VehiclePart>();
            try {
                if (submodelID == 0) {
                    vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(baseVehicleID) && x.SubModelID == null).First();
                } else {
                    vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(baseVehicleID) && x.SubModelID.Equals(submodelID) && x.ConfigID == null).First();
                }
                parts = vehicle.vcdb_VehicleParts.ToList();
            } catch { }
            return parts;
        }

        public void RemoveVehiclePart(int vPartID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<Note> notes = db.Notes.Where(x => x.vcdb_VehiclePart.ID.Equals(vPartID)).ToList<Note>();
            db.Notes.DeleteAllOnSubmit(notes);
            db.SubmitChanges();

            vcdb_VehiclePart vehiclePart = db.vcdb_VehicleParts.Where(x => x.ID.Equals(vPartID)).First();
            db.vcdb_VehicleParts.DeleteOnSubmit(vehiclePart);
            db.SubmitChanges();
        }

        public List<vcdb_VehiclePart> AddVehiclePart(int vehicleID = 0, int baseVehicleID = 0, int submodelID = 0, int partID = 0, string partOrVehicle = "vehicle") {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            List<vcdb_VehiclePart> vParts = new List<vcdb_VehiclePart>();
            if (vehicleID != 0) {
                vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
            } else {
                if (submodelID == 0) {
                    try {
                        vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(baseVehicleID) && x.SubModelID.Equals(null)).First();
                    } catch {
                        // no vehicle exists -- create it
                        vehicle = new vcdb_Vehicle {
                            BaseVehicleID = baseVehicleID
                        };
                        db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                        db.SubmitChanges();
                    }
                } else {
                    try {
                        vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(baseVehicleID) && x.SubModelID.Equals(submodelID) && x.ConfigID.Equals(null)).First();
                    } catch {
                        // no vehicle exists -- create it
                        vehicle = new vcdb_Vehicle {
                            BaseVehicleID = baseVehicleID,
                            SubModelID = submodelID
                        };
                        db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                        db.SubmitChanges();
                    }
                }
            }
            // have vehicle -- add part if part exists
            Part part = new Part();
            try {
                part = db.Parts.Where(x => x.partID.Equals(partID)).First();
            } catch {
                throw new Exception("Part doesn't exist");
            }
            try {
                // check if vehicle part relationship already exists
                vcdb_VehiclePart vPart = db.vcdb_VehicleParts.Where(x => x.PartNumber.Equals(part.partID) && x.VehicleID.Equals(vehicle.ID)).First();
            } catch {
                // relationship doesn't exist yet - add it
                vcdb_VehiclePart vPart = new vcdb_VehiclePart {
                    VehicleID = vehicle.ID,
                    PartNumber = part.partID
                };
                db.vcdb_VehicleParts.InsertOnSubmit(vPart);
                db.SubmitChanges();

                List<Note> notes = new List<Note>();
                // Select a distinct set of the notes for each vehicle and add them to the vpart
                if (partOrVehicle == "vehicle") {
                    // lean notes on the vehicle side
                    notes = db.vcdb_VehicleParts.Where(x => x.VehicleID.Equals(vehicle.ID)).SelectMany(x => x.Notes).ToList().Distinct(new NoteComparer()).ToList();
                } else {
                    // lean notes on the part side
                    notes = db.vcdb_VehicleParts.Where(x => x.PartNumber.Equals(partID)).SelectMany(x => x.Notes).ToList().Distinct(new NoteComparer()).ToList();
                }
                List<Note> newNotes = new List<Note>();
                foreach (Note n in notes) {
                    Note newNote = new Note {
                        note1 = n.note1,
                        vehiclePartID = vPart.ID
                    };
                    newNotes.Add(newNote);
                }
                if (newNotes.Count > 0) {
                    db.Notes.InsertAllOnSubmit(newNotes);
                    db.SubmitChanges();
                }
                if (partOrVehicle != "vehicle") {
                    vPart.Notes.AddRange(newNotes);
                    vParts.Add(vPart);
                }
            }
            if (partOrVehicle == "vehicle") {
                vParts = db.vcdb_VehicleParts.Where(x => x.VehicleID.Equals(vehicle.ID)).ToList();
            }
            return vParts;
        }

        public void PopulatePartsFromBaseVehicle(int vehicleID = 0, int baseVehicleID = 0, int submodelID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<vcdb_VehiclePart> vParts = new List<vcdb_VehiclePart>();
            List<vcdb_Vehicle> vehicles = new List<vcdb_Vehicle>();
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            if (vehicleID != 0) {
                vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
                vehicles = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).SelectMany(x => x.BaseVehicle.vcdb_Vehicles).ToList();
            } else {
                if (submodelID == 0) {
                    try {
                        vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(baseVehicleID) && x.SubModelID.Equals(null)).First();
                    } catch {
                        vehicle = new vcdb_Vehicle {
                            BaseVehicleID = baseVehicleID
                        };
                        db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                        db.SubmitChanges();
                    }
                } else {
                    try {
                        vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(baseVehicleID) && x.SubModelID.Equals(submodelID) && x.ConfigID.Equals(null)).First();
                    } catch {
                        vehicle = new vcdb_Vehicle {
                            BaseVehicleID = baseVehicleID,
                            SubModelID = submodelID
                        };
                        db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                        db.SubmitChanges();
                    }
                }
                vehicles = db.BaseVehicles.Where(x => x.ID.Equals(baseVehicleID)).SelectMany(x => x.vcdb_Vehicles).ToList();
            }
            if (vehicles.Count > 0 && vehicle != null && vehicle.ID > 0) {
                vParts = vehicles.SelectMany(x => x.vcdb_VehicleParts).ToList().Distinct(new VehiclePartComparer()).ToList();
                
                foreach (vcdb_VehiclePart vpart in vParts) {
                    vcdb_VehiclePart vp = new vcdb_VehiclePart {
                        VehicleID = vehicle.ID,
                        PartNumber = vpart.PartNumber
                    };
                    db.vcdb_VehicleParts.InsertOnSubmit(vp);
                    db.SubmitChanges();

                    List<Note> notes = new List<Note>();
                    foreach (Note note in vpart.Notes) {
                        Note n = new Note {
                            vehiclePartID = vp.ID,
                            note1 = note.note1
                        };
                        notes.Add(n);
                    }
                    db.Notes.InsertAllOnSubmit(notes);
                    db.SubmitChanges();
                }
            }

        }

        public List<Note> getNotes(int vPartID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<Note> notes = new List<Note>();
            try {
                notes = db.Notes.Where(x => x.vehiclePartID.Equals(vPartID)).ToList();
            } catch { }
            return notes;
        }

        public void RemoveNote(int noteID) {
            CurtDevDataContext db = new CurtDevDataContext();
            Note note = db.Notes.Where(x => x.ID.Equals(noteID)).First<Note>();
            db.Notes.DeleteOnSubmit(note);
            db.SubmitChanges();
        }

        public void AddNote(int vPartID, string note) {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                Note n = db.Notes.Where(x => x.vehiclePartID.Equals(vPartID) && x.note1.ToLower().Trim().Equals(note.ToLower().Trim())).First();
            } catch {
                Note n = new Note {
                    vehiclePartID = vPartID,
                    note1 = note.Trim()
                };
                db.Notes.InsertOnSubmit(n);
                db.SubmitChanges();
            }
        }

        public string SearchNotes(string keyword = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            if (keyword != "" && keyword.Length >= 1) {
                var notes = (from n in db.Notes
                             where n.note1.Contains(keyword)
                             orderby n.note1
                             select new {
                                 id = 1,
                                 label = n.note1,
                                 value = n.note1
                             }).ToList().Distinct().ToList();
                return JsonConvert.SerializeObject(notes);
            }
            return "[]";
        }

    }

    public class ConfigAttributeComparer : IEqualityComparer<ConfigAttribute> {
        bool IEqualityComparer<ConfigAttribute>.Equals(ConfigAttribute x, ConfigAttribute y) {
            // Check whether the compared objects reference the same data.
            if (x.ConfigAttributeTypeID.Equals(y.ConfigAttributeTypeID) && x.vcdbID.Equals(y.vcdbID) && x.value.Equals(y.value)) {
                return true;
            } else {
                return false;
            }

        }

        int IEqualityComparer<ConfigAttribute>.GetHashCode(ConfigAttribute obj) {
            return obj.vcdbID.GetHashCode();
        }
    }

    public class NoteComparer : IEqualityComparer<Note> {
        bool IEqualityComparer<Note>.Equals(Note x, Note y) {
            // Check whether the compared objects reference the same data.
            if (x.note1.Trim().Equals(y.note1.Trim())) {
                return true;
            } else {
                return false;
            }

        }

        int IEqualityComparer<Note>.GetHashCode(Note obj) {
            return obj.note1.GetHashCode();
        }
    }

    public class VehiclePartComparer : IEqualityComparer<vcdb_VehiclePart> {
        bool IEqualityComparer<vcdb_VehiclePart>.Equals(vcdb_VehiclePart x, vcdb_VehiclePart y) {
            // Check whether the compared objects reference the same data.
            if (x.PartNumber.Equals(y.PartNumber)) {
                return true;
            } else {
                return false;
            }

        }

        int IEqualityComparer<vcdb_VehiclePart>.GetHashCode(vcdb_VehiclePart obj) {
            return obj.PartNumber.GetHashCode();
        }
    }

    public class BaseVehicleComparer : IEqualityComparer<ACESBaseVehicle> {
        bool IEqualityComparer<ACESBaseVehicle>.Equals(ACESBaseVehicle x, ACESBaseVehicle y) {
            // Check whether the compared objects reference the same data.
            if (x.ID.Equals(y.ID)) {
                return true;
            } else {
                return false;
            }

        }

        int IEqualityComparer<ACESBaseVehicle>.GetHashCode(ACESBaseVehicle obj) {
            return obj.ID.GetHashCode();
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

    public class ACESMake {
        public int ID { get; set; }
        public string name { get; set; }
        public int? AAIAID { get; set; }
    }

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
        public vcdb_VehiclePart vehiclePart { get; set; }
        public List<ACESSubmodel> Submodels { get; set; }
    }

    public class ACESSubmodel {
        public int? SubmodelID { get; set; }
        public Submodel submodel { get; set; }
        public List<ConfigAttributeType> configlist { get; set; }
        public List<ACESVehicle> vehicles { get; set; }
        public bool vcdb { get; set; }
        public vcdb_VehiclePart vehiclePart { get; set; }
    }

    public class ACESVehicle {
        public int ID { get; set; }
        public List<ConfigAttribute> configs { get; set; }
        public bool vcdb { get; set; }
        public vcdb_VehiclePart vehiclePart { get; set; }
    }

    public class ACESConfigs {
        public List<ConfigAttributeType> types { get; set; }
        public List<ACESVehicleConfig> configs { get; set; }

        public void clearDuplicates() {
            List<ACESVehicleConfig> paredlist = new List<ACESVehicleConfig>();
            List<ConfigAttributeType> typelist = types.Where(x => x.count > 1).ToList();
            List<int> typeids = typelist.Select(x => x.ID).ToList();
            for (int i = 0; i < this.configs.Count; i++) {
                bool exists = false;
                foreach (ACESVehicleConfig config in paredlist) {
                    List<ConfigAttribute> attribs = config.attributes.Where(x => typeids.Contains(x.ConfigAttributeTypeID)).OrderBy(x => x.ConfigAttributeType.sort).ToList();
                    List<ConfigAttribute> masterattribs = this.configs[i].attributes.Where(x => typeids.Contains(x.ConfigAttributeTypeID)).OrderBy(x => x.ConfigAttributeType.sort).ToList();
                    int exceptCount = attribs.Except(masterattribs,new ConfigAttributeComparer()).Count();
                    if (exceptCount == 0) {
                        exists = true;
                    }
                }
                if (!exists) {
                    paredlist.Add(this.configs[i]);
                }
            }
            this.configs = paredlist;
        }
    }

    public class ACESVehicleConfig {
        public List<ConfigAttribute> attributes { get; set; }
    }
}