using System;
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

        public List<ACESMake> GetAllMakes() {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<ACESMake> makes = (from m in db.vcdb_Makes
                                    select new ACESMake { 
                                        ID = m.ID,
                                        name = m.MakeName,
                                        AAIAID = m.AAIAMakeID
                                    }).Distinct().ToList();
            List<int> regions = new List<int> { 1, 2 };
            List<int> vtypes = new List<int> { 5, 6, 7 };
            List<ACESMake> vcdbmakes = (from m in vcdb.Makes
                                        join bv in vcdb.BaseVehicles on m.MakeID equals bv.MakeID
                                        where bv.Vehicles.Any(x => regions.Contains(x.RegionID))
                                        && vtypes.Contains(bv.Model.VehicleTypeID)
                                        select new ACESMake {
                                            ID = 0,
                                            name = m.MakeName,
                                            AAIAID = m.MakeID
                                        }).Distinct().ToList();
            makes.AddRange(vcdbmakes);
            List<ACESMake> allmakes = makes.Distinct(new ACESMakeComparer()).OrderBy(x => x.name).ToList();
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

        public vcdb_Make AddMake(string name) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Make make = new vcdb_Make();
            try {
                make = db.vcdb_Makes.Where(x => x.MakeName.ToLower().Trim().Equals(name.ToLower().Trim())).First();
            } catch {
                AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
                AAIA.Make vcdbMake = vcdb.Makes.Where(x => x.MakeName.ToLower().Trim().Equals(name.ToLower().Trim())).FirstOrDefault();
                make = new vcdb_Make {
                    MakeName = name,
                    AAIAMakeID = (vcdbMake != null && vcdbMake.MakeID > 0) ? vcdbMake.MakeID : (int?)null,
                };
                db.vcdb_Makes.InsertOnSubmit(make);
                db.SubmitChanges();
            }
            return make;
        }

        public vcdb_Make UpdateMake(int id, string name) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            vcdb_Make make = new vcdb_Make();
            // get make
            make = db.vcdb_Makes.Where(x => x.ID.Equals(id)).First();
            try {
                // check if the make exists in the vcdb
                AAIA.Make vcdbMake = vcdb.Makes.Where(x => x.MakeName.Trim().ToLower().Equals(name.Trim().ToLower())).First();
                try {
                    // if it exists, check to see if a make is already in curtdev with the matching AAIA ID
                    vcdb_Make existingMake = db.vcdb_Makes.Where(x => x.AAIAMakeID.Equals(vcdbMake.MakeID)).First();
                    // get all the base vehicles with the present make
                    List<BaseVehicle> baseVehicles = db.BaseVehicles.Where(x => x.MakeID.Equals(make.ID)).ToList();
                    foreach (BaseVehicle bv in baseVehicles) {
                        // update the Base Vehicles to use the existing make that's correct
                        bv.MakeID = existingMake.ID;
                    }
                    db.SubmitChanges();
                    // delete the duplicate
                    db.vcdb_Makes.DeleteOnSubmit(make);
                } catch {
                    // there is no existing make.  Update the make with the new vcdb correct values
                    make.MakeName = vcdbMake.MakeName.Trim();
                    make.AAIAMakeID = vcdbMake.MakeID;
                }
            } catch {
                // no vcdb match...just update the name
                make.MakeName = name.Trim();
            }
            db.SubmitChanges();
            return make;
        }

        public void RemoveMake(string make) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<int> idlist = make.Split('|').Select(n => int.Parse(n)).ToList();
            int mID = idlist[0];
            int aaiaID = idlist[1];
            vcdb_Make m = db.vcdb_Makes.Where(x => x.ID.Equals(mID) || x.AAIAMakeID.Equals(aaiaID)).First();
            db.vcdb_Makes.DeleteOnSubmit(m);
            db.SubmitChanges();
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

        public List<ACESMake> GetAllModels() {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<ACESMake> models = (from m in db.vcdb_Models
                                    select new ACESMake {
                                        ID = m.ID,
                                        name = m.ModelName,
                                        AAIAID = m.AAIAModelID
                                    }).Distinct().ToList();
            List<int> regions = new List<int> { 1, 2 };
            List<int> vtypes = new List<int> { 5, 6, 7 };
            List<ACESMake> vcdbmodels = (from m in vcdb.Models
                                        join bv in vcdb.BaseVehicles on m.ModelID equals bv.ModelID
                                        where bv.Vehicles.Any(x => regions.Contains(x.RegionID))
                                        && vtypes.Contains(bv.Model.VehicleTypeID)
                                        select new ACESMake {
                                            ID = 0,
                                            name = m.ModelName,
                                            AAIAID = m.ModelID
                                        }).Distinct().ToList();
            models.AddRange(vcdbmodels);
            List<ACESMake> allmodels = models.Distinct(new ACESMakeComparer()).OrderBy(x => x.name).ToList();
            return allmodels;
        }

        public vcdb_Model AddModel(string name) {
            CurtDevDataContext db = new CurtDevDataContext();
            vcdb_Model model = new vcdb_Model();
            try {
                model = db.vcdb_Models.Where(x => x.ModelName.ToLower().Trim().Equals(name.ToLower().Trim())).First();
            } catch {
                AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
                AAIA.Model vcdbModel = vcdb.Models.Where(x => x.ModelName.ToLower().Trim().Equals(name.ToLower().Trim())).FirstOrDefault();
                model = new vcdb_Model {
                    ModelName = name,
                    AAIAModelID = (vcdbModel != null && vcdbModel.ModelID > 0) ? vcdbModel.ModelID : (int?)null,
                };
                db.vcdb_Models.InsertOnSubmit(model);
                db.SubmitChanges();
            }
            return model;
        }

        public vcdb_Model UpdateModel(int id, string name) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            vcdb_Model model = new vcdb_Model();
            // get make
            model = db.vcdb_Models.Where(x => x.ID.Equals(id)).First();
            try {
                // check if the make exists in the vcdb
                AAIA.Model vcdbModel = vcdb.Models.Where(x => x.ModelName.Trim().ToLower().Equals(name.Trim().ToLower())).First();
                try {
                    // if it exists, check to see if a make is already in curtdev with the matching AAIA ID
                    vcdb_Model existingModel = db.vcdb_Models.Where(x => x.AAIAModelID.Equals(vcdbModel.ModelID)).First();
                    // get all the base vehicles with the present make
                    List<BaseVehicle> baseVehicles = db.BaseVehicles.Where(x => x.ModelID.Equals(model.ID)).ToList();
                    foreach (BaseVehicle bv in baseVehicles) {
                        // update the Base Vehicles to use the existing make that's correct
                        bv.ModelID = existingModel.ID;
                    }
                    db.SubmitChanges();
                    // delete the duplicate
                    db.vcdb_Models.DeleteOnSubmit(model);
                } catch {
                    // there is no existing make.  Update the make with the new vcdb correct values
                    model.ModelName = vcdbModel.ModelName.Trim();
                    model.AAIAModelID = vcdbModel.ModelID;
                }
            } catch {
                // no vcdb match...just update the name
                model.ModelName = name.Trim();
            }
            db.SubmitChanges();
            return model;
        }

        public void RemoveModel(string model) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<int> idlist = model.Split('|').Select(n => int.Parse(n)).ToList();
            int mID = idlist[0];
            int aaiaID = idlist[1];
            vcdb_Model m = db.vcdb_Models.Where(x => x.ID.Equals(mID) || x.AAIAModelID.Equals(aaiaID)).First();
            db.vcdb_Models.DeleteOnSubmit(m);
            db.SubmitChanges();
        }

        public Submodel AddSubmodel(string name) {
            CurtDevDataContext db = new CurtDevDataContext();
            Submodel submodel = new Submodel();
            try {
                submodel = db.Submodels.Where(x => x.SubmodelName.ToLower().Trim().Equals(name.ToLower().Trim())).First();
            } catch {
                AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
                AAIA.Submodel vcdbSubmodel = vcdb.Submodels.Where(x => x.SubmodelName.ToLower().Trim().Equals(name.ToLower().Trim())).FirstOrDefault();
                submodel = new Submodel {
                    SubmodelName = name,
                    AAIASubmodelID = (vcdbSubmodel != null && vcdbSubmodel.SubmodelID > 0) ? vcdbSubmodel.SubmodelID : (int?)null,
                };
                db.Submodels.InsertOnSubmit(submodel);
                db.SubmitChanges();
            }
            return submodel;
        }

        public Submodel UpdateSubmodel(int id, string name) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            Submodel submodel = new Submodel();
            // get make
            submodel = db.Submodels.Where(x => x.ID.Equals(id)).First();
            try {
                // check if the make exists in the vcdb
                AAIA.Submodel vcdbSubmodel = vcdb.Submodels.Where(x => x.SubmodelName.Trim().ToLower().Equals(name.Trim().ToLower())).First();
                try {
                    // if it exists, check to see if a make is already in curtdev with the matching AAIA ID
                    Submodel existingSubmodel = db.Submodels.Where(x => x.AAIASubmodelID.Equals(vcdbSubmodel.SubmodelID)).First();
                    // get all the base vehicles with the present make
                    List<vcdb_Vehicle> vehicles = db.vcdb_Vehicles.Where(x => x.SubModelID.Equals(submodel.ID)).ToList();
                    foreach (vcdb_Vehicle v in vehicles) {
                        // update the Base Vehicles to use the existing make that's correct
                        v.SubModelID = existingSubmodel.ID;
                    }
                    db.SubmitChanges();
                    // delete the duplicate
                    db.Submodels.DeleteOnSubmit(submodel);
                } catch {
                    // there is no existing make.  Update the make with the new vcdb correct values
                    submodel.SubmodelName = vcdbSubmodel.SubmodelName.Trim();
                    submodel.AAIASubmodelID = vcdbSubmodel.SubmodelID;
                }
            } catch {
                // no vcdb match...just update the name
                submodel.SubmodelName = name.Trim();
            }
            db.SubmitChanges();
            return submodel;
        }

        public void RemoveSubmodel(string submodel) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<int> idlist = submodel.Split('|').Select(n => int.Parse(n)).ToList();
            int sID = idlist[0];
            int aaiaID = idlist[1];
            Submodel s = db.Submodels.Where(x => x.ID.Equals(sID) || x.AAIASubmodelID.Equals(aaiaID)).First();
            db.Submodels.DeleteOnSubmit(s);
            db.SubmitChanges();
        }

        public List<ACESMake> GetAllSubmodels() {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<ACESMake> submodels = (from s in db.Submodels
                                        select new ACESMake {
                                            ID = s.ID,
                                            name = s.SubmodelName,
                                            AAIAID = s.AAIASubmodelID
                                        }).Distinct().ToList();
            List<int> regions = new List<int> { 1, 2 };
            List<int> vtypes = new List<int> { 5, 6, 7 };
            List<ACESMake> vcdbsubmodels = (from s in vcdb.Submodels
                                            join v in vcdb.Vehicles on s.SubmodelID equals v.SubmodelID
                                            where regions.Contains(v.RegionID)
                                            && vtypes.Contains(v.BaseVehicle.Model.VehicleTypeID)
                                            select new ACESMake {
                                                ID = 0,
                                                name = s.SubmodelName,
                                                AAIAID = s.SubmodelID
                                            }).Distinct().ToList();
            submodels.AddRange(vcdbsubmodels);
            List<ACESMake> allsubmodels = submodels.Distinct(new ACESMakeComparer()).OrderBy(x => x.name).ToList();
            return allsubmodels;
        }

        public vcdb_Vehicle AddNonVCDBVehicle(int year, string make, string model, string submodel = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            vcdb_Vehicle vehicle = new vcdb_Vehicle();
            List<int> idlist = make.Split('|').Select(n => int.Parse(n)).ToList();
            int maID = idlist[0];
            int maAaiaID = idlist[1];

            idlist = model.Split('|').Select(n => int.Parse(n)).ToList();
            int moID = idlist[0];
            int moAaiaID = idlist[1];

            int sID = 0;
            int sAaiaID = 0;
            if (submodel != "") {
                idlist = submodel.Split('|').Select(n => int.Parse(n)).ToList();
                sID = idlist[0];
                sAaiaID = idlist[1];
            }

            if (maID == 0) {
                AAIA.Make vcdbMake = vcdb.Makes.Where(x => x.MakeID.Equals(maAaiaID)).First();
                vcdb_Make cdmake = new vcdb_Make {
                    MakeName = vcdbMake.MakeName.Trim(),
                    AAIAMakeID = vcdbMake.MakeID
                };
                db.vcdb_Makes.InsertOnSubmit(cdmake);
                db.SubmitChanges();
                maID = cdmake.ID;
            }

            if (moID == 0) {
                AAIA.Model vcdbModel = vcdb.Models.Where(x => x.ModelID.Equals(moAaiaID)).First();
                vcdb_Model cdmodel = new vcdb_Model {
                    ModelName = vcdbModel.ModelName.Trim(),
                    AAIAModelID = vcdbModel.ModelID,
                    VehicleTypeID = vcdbModel.VehicleTypeID
                };
                db.vcdb_Models.InsertOnSubmit(cdmodel);
                db.SubmitChanges();
                moID = cdmodel.ID;
            }

            BaseVehicle bv = new BaseVehicle();
            try {
                bv = db.BaseVehicles.Where(x => x.YearID.Equals(year) && x.MakeID.Equals(maID) && x.ModelID.Equals(moID)).First();
            } catch {
                AAIA.BaseVehicle aaiabv = new AAIA.BaseVehicle();
                aaiabv = vcdb.BaseVehicles.Where(x => x.YearID.Equals(year) && x.MakeID.Equals(maAaiaID) && x.ModelID.Equals(moAaiaID)).FirstOrDefault();
                bv = new BaseVehicle {
                    YearID = year,
                    MakeID = maID,
                    ModelID = moID,
                    AAIABaseVehicleID = (aaiabv != null && aaiabv.BaseVehicleID > 0) ? aaiabv.BaseVehicleID : (int?)null
                };
                db.BaseVehicles.InsertOnSubmit(bv);
                db.SubmitChanges();
            }
            int? subID = null;
            if (sID > 0) {
                subID = sID;
            } else if (sAaiaID > 0) {
                AAIA.Submodel vcdbSubmodel = vcdb.Submodels.Where(x => x.SubmodelID.Equals(sAaiaID)).First();
                Submodel cdsubmodel = new Submodel {
                    SubmodelName = vcdbSubmodel.SubmodelName.Trim(),
                    AAIASubmodelID = vcdbSubmodel.SubmodelID
                };
                db.Submodels.InsertOnSubmit(cdsubmodel);
                db.SubmitChanges();
                subID = cdsubmodel.ID;
            }
            try {
                vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bv.ID) && x.SubModelID.Equals(subID) && x.ConfigID.Equals(null)).First();
            } catch {
                vehicle = new vcdb_Vehicle {
                    BaseVehicleID = bv.ID,
                    SubModelID = subID
                };
                db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                db.SubmitChanges();
            }
            return vehicle;
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
                var predicate = PredicateBuilder.True<AAIA.Vehicle>();
                predicate = predicate.And(p => vehicleIDs.Contains(p.VehicleID));
                foreach (ConfigAttribute ca in configs) {
                    switch (ca.ConfigAttributeType.AcesType.name) {
                        case "WheelBase":
                            predicate = predicate.And(p => p.VehicleToWheelbases.Any(y => y.WheelbaseID.Equals(ca.vcdbID)));
                            break;
                        case "BodyType":
                            predicate = predicate.And(p => p.VehicleToBodyStyleConfigs.Any(y => y.BodyStyleConfig.BodyTypeID.Equals(ca.vcdbID)));
                            break;
                        case "DriveType":
                            predicate = predicate.And(p => p.VehicleToDriveTypes.Any(y => y.DriveTypeID.Equals(ca.vcdbID)));
                            break;
                        case "BodyNumDoors":
                            predicate = predicate.And(p => p.VehicleToBodyStyleConfigs.Any(y => y.BodyStyleConfig.BodyNumDoorsID.Equals(ca.vcdbID)));
                            break;
                        case "BedLength":
                            predicate = predicate.And(p => p.VehicleToBedConfigs.Any(y => y.BedConfig.BedLengthID.Equals(ca.vcdbID)));
                            break;
                        case "FuelType":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.FuelTypeID.Equals(ca.vcdbID)));
                            break;
                        case "EngineBase":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.EngineBaseID.Equals(ca.vcdbID)));
                            break;
                        case "Aspiration":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.AspirationID.Equals(ca.vcdbID)));
                            break;
                        case "BedType":
                            predicate = predicate.And(p => p.VehicleToBedConfigs.Any(y => y.BedConfig.BedTypeID.Equals(ca.vcdbID)));
                            break;
                        case "BrakeABS":
                            predicate = predicate.And(p => p.VehicleToBrakeConfigs.Any(y => y.BrakeConfig.BrakeABSID.Equals(ca.vcdbID)));
                            break;
                        case "BrakeSystem":
                            predicate = predicate.And(p => p.VehicleToBrakeConfigs.Any(y => y.BrakeConfig.BrakeSystemID.Equals(ca.vcdbID)));
                            break;
                        case "CylinderHeadType":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.CylinderHeadTypeID.Equals(ca.vcdbID)));
                            break;
                        case "EngineDesignation":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.EngineDesignationID.Equals(ca.vcdbID)));
                            break;
                        case "EngineMfr":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.EngineMfrID.Equals(ca.vcdbID)));
                            break;
                        case "EngineVersion":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.EngineVersionID.Equals(ca.vcdbID)));
                            break;
                        case "EngineVIN":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.EngineVINID.Equals(ca.vcdbID)));
                            break;
                        case "FrontBrakeType":
                            predicate = predicate.And(p => p.VehicleToBrakeConfigs.Any(y => y.BrakeConfig.FrontBrakeTypeID.Equals(ca.vcdbID)));
                            break;
                        case "FrontSpringType":
                            predicate = predicate.And(p => p.VehicleToSpringTypeConfigs.Any(y => y.SpringTypeConfig.FrontSpringTypeID.Equals(ca.vcdbID)));
                            break;
                        case "FuelDeliverySubType":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.FuelDeliveryConfig.FuelDeliverySubTypeID.Equals(ca.vcdbID)));
                            break;
                        case "FuelDeliveryType":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.FuelDeliveryConfig.FuelDeliveryTypeID.Equals(ca.vcdbID)));
                            break;
                        case "FuelSystemControlType":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.FuelDeliveryConfig.FuelSystemControlTypeID.Equals(ca.vcdbID)));
                            break;
                        case "FuelSystemDesign":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.FuelDeliveryConfig.FuelSystemDesignID.Equals(ca.vcdbID)));
                            break;
                        case "IgnitionSystemType":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.IgnitionSystemTypeID.Equals(ca.vcdbID)));
                            break;
                        case "MfrBodyCode":
                            predicate = predicate.And(p => p.VehicleToMfrBodyCodes.Any(y => y.MfrBodyCode.MfrBodyCodeID.Equals(ca.vcdbID)));
                            break;
                        case "PowerOutput":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.PowerOutputID.Equals(ca.vcdbID)));
                            break;
                        case "RearBrakeType":
                            predicate = predicate.And(p => p.VehicleToBrakeConfigs.Any(y => y.BrakeConfig.RearBrakeTypeID.Equals(ca.vcdbID)));
                            break;
                        case "RearSpringType":
                            predicate = predicate.And(p => p.VehicleToSpringTypeConfigs.Any(y => y.SpringTypeConfig.RearSpringTypeID.Equals(ca.vcdbID)));
                            break;
                        case "SteeringSystem":
                            predicate = predicate.And(p => p.VehicleToSteeringConfigs.Any(y => y.SteeringConfig.SteeringSystemID.Equals(ca.vcdbID)));
                            break;
                        case "SteeringType":
                            predicate = predicate.And(p => p.VehicleToSteeringConfigs.Any(y => y.SteeringConfig.SteeringTypeID.Equals(ca.vcdbID)));
                            break;
                        case "TransElecControlled":
                            predicate = predicate.And(p => p.VehicleToTransmissions.Any(y => y.Transmission.ElecControlled.ElecControlledID.Equals(ca.vcdbID)));
                            break;
                        case "Transmission":
                            predicate = predicate.And(p => p.VehicleToTransmissions.Any(y => y.Transmission.TransmissionID.Equals(ca.vcdbID)));
                            break;
                        case "TransmissionBase":
                            predicate = predicate.And(p => p.VehicleToTransmissions.Any(y => y.Transmission.TransmissionBaseID.Equals(ca.vcdbID)));
                            break;
                        case "TransmissionControlType":
                            predicate = predicate.And(p => p.VehicleToTransmissions.Any(y => y.Transmission.TransmissionBase.TransmissionControlTypeID.Equals(ca.vcdbID)));
                            break;
                        case "TransmissionMfrCode":
                            predicate = predicate.And(p => p.VehicleToTransmissions.Any(y => y.Transmission.TransmissionMfrCodeID.Equals(ca.vcdbID)));
                            break;
                        case "TransmissionNumSpeeds":
                            predicate = predicate.And(p => p.VehicleToTransmissions.Any(y => y.Transmission.TransmissionBase.TransmissionNumSpeedsID.Equals(ca.vcdbID)));
                            break;
                        case "TransmissionType":
                            predicate = predicate.And(p => p.VehicleToTransmissions.Any(y => y.Transmission.TransmissionBase.TransmissionTypeID.Equals(ca.vcdbID)));
                            break;
                        case "ValvesPerEngine":
                            predicate = predicate.And(p => p.VehicleToEngineConfigs.Any(y => y.EngineConfig.ValvesID.Equals(ca.vcdbID)));
                            break;
                        default:
                            isValid = false;
                            break;
                    }
                    if (isValid) {
                        // run query
                        List<AAIA.Vehicle> vconfigs = vcdb.Vehicles.Where(predicate).ToList<AAIA.Vehicle>();
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
                                            BedConfigs = v.VehicleToBedConfigs.ToList(),
                                            BodyStyleConfigs = v.VehicleToBodyStyleConfigs.ToList(),
                                            BrakeConfigs = v.VehicleToBrakeConfigs.ToList(),
                                            DriveTypes = v.VehicleToDriveTypes.ToList(),
                                            EngineConfigs = v.VehicleToEngineConfigs.ToList(),
                                            MfrBodyCodes = v.VehicleToMfrBodyCodes.ToList(),
                                            SpringTypeConfigs = v.VehicleToSpringTypeConfigs.ToList(),
                                            SteeringConfigs = v.VehicleToSteeringConfigs.ToList(),
                                            Transmissions = v.VehicleToTransmissions.ToList(),
                                            Wheelbases = v.VehicleToWheelbases.ToList(),
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

        internal ACESVehicleDetails getVehicleConfigs(int BaseVehicleID, int SubmodelID) {
            ACESVehicleDetails vdetails = new ACESVehicleDetails();
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            try {
                BaseVehicle bv = db.BaseVehicles.Where(x => x.ID.Equals(BaseVehicleID)).First<BaseVehicle>();
                Submodel submodel = db.Submodels.Where(x => x.ID.Equals(SubmodelID)).First<Submodel>();
                List<ConfigAttributeType> attrtypes = db.ConfigAttributeTypes.Where(x => x.AcesTypeID != null).OrderBy(x => x.sort).ToList<ConfigAttributeType>();
                List<AAIA.Vehicle> vehicles = vcdb.Vehicles.Where(x => x.BaseVehicleID.Equals(bv.AAIABaseVehicleID) && x.SubmodelID.Equals(submodel.AAIASubmodelID)).Distinct().ToList<AAIA.Vehicle>();
                foreach (ConfigAttributeType type in attrtypes) {
                    switch (type.name) {
                        case "Body Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToBodyStyleConfigs.Select(y => y.BodyStyleConfig.BodyTypeID)).Distinct().Count();
                            break;
                        case "Number of Doors":
                            type.count = vehicles.SelectMany(x => x.VehicleToBodyStyleConfigs.Select(y => y.BodyStyleConfig.BodyNumDoorsID)).Distinct().Count();
                            break;
                        case "Drive Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToDriveTypes.Select(y => y.DriveTypeID)).Distinct().Count();
                            break;
                        case "Bed Length":
                            type.count = vehicles.SelectMany(x => x.VehicleToBedConfigs.Select(y => y.BedConfig.BedLengthID)).Distinct().Count();
                            break;
                        case "Wheel Base":
                            type.count = vehicles.SelectMany(x => x.VehicleToWheelbases.Select(y => y.WheelbaseID)).Distinct().Count();
                            break;
                        case "Engine":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.EngineBaseID)).Distinct().Count();
                            break;
                        case "Fuel Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.FuelTypeID)).Distinct().Count();
                            break;
                        case "Aspiration":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.AspirationID)).Distinct().Count();
                            break;
                        case "Bed Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToBedConfigs.Select(y => y.BedConfig.BedTypeID)).Distinct().Count();
                            break;
                        case "Brake ABS":
                            type.count = vehicles.SelectMany(x => x.VehicleToBrakeConfigs.Select(y => y.BrakeConfig.BrakeABSID)).Distinct().Count();
                            break;
                        case "Brake System":
                            type.count = vehicles.SelectMany(x => x.VehicleToBrakeConfigs.Select(y => y.BrakeConfig.BrakeSystemID)).Distinct().Count();
                            break;
                        case "Cylinder Head Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.CylinderHeadTypeID)).Distinct().Count();
                            break;
                        case "Engine Designation":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.EngineDesignationID)).Distinct().Count();
                            break;
                        case "Engine Manufacturer":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.EngineMfrID)).Distinct().Count();
                            break;
                        case "Engine Version":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.EngineVersionID)).Distinct().Count();
                            break;
                        case "Engine VIN":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.EngineVINID)).Distinct().Count();
                            break;
                        case "Front Brake Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToBrakeConfigs.Select(y => y.BrakeConfig.FrontBrakeTypeID)).Distinct().Count();
                            break;
                        case "Front Spring Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToSpringTypeConfigs.Select(y => y.SpringTypeConfig.FrontSpringTypeID)).Distinct().Count();
                            break;
                        case "Fuel Delivery Sub-Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.FuelDeliveryConfig.FuelDeliverySubTypeID)).Distinct().Count();
                            break;
                        case "Fuel Delivery Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.FuelDeliveryConfig.FuelDeliveryTypeID)).Distinct().Count();
                            break;
                        case "Fuel System Control Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.FuelDeliveryConfig.FuelSystemControlTypeID)).Distinct().Count();
                            break;
                        case "Fuel System Design":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.FuelDeliveryConfig.FuelSystemDesignID)).Distinct().Count();
                            break;
                        case "Ignition System Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.IgnitionSystemTypeID)).Distinct().Count();
                            break;
                        case "Manufacturer Body Code":
                            type.count = vehicles.SelectMany(x => x.VehicleToMfrBodyCodes.Select(y => y.MfrBodyCodeID)).Distinct().Count();
                            break;
                        case "Power Output":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.PowerOutputID)).Distinct().Count();
                            break;
                        case "Rear Brake Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToBrakeConfigs.Select(y => y.BrakeConfig.RearBrakeTypeID)).Distinct().Count();
                            break;
                        case "Rear Spring Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToSpringTypeConfigs.Select(y => y.SpringTypeConfig.RearSpringTypeID)).Distinct().Count();
                            break;
                        case "Steering System":
                            type.count = vehicles.SelectMany(x => x.VehicleToSteeringConfigs.Select(y => y.SteeringConfig.SteeringSystemID)).Distinct().Count();
                            break;
                        case "Steering Type":
                            type.count = vehicles.SelectMany(x => x.VehicleToSteeringConfigs.Select(y => y.SteeringConfig.SteeringTypeID)).Distinct().Count();
                            break;
                        case "Tranmission Electronic Controlled":
                            type.count = vehicles.SelectMany(x => x.VehicleToTransmissions.Select(y => y.Transmission.TransmissionElecControlledID)).Distinct().Count();
                            break;
                        case "Transmission":
                            type.count = vehicles.SelectMany(x => x.VehicleToTransmissions.Select(y => y.Transmission.TransmissionID)).Distinct().Count();
                            break;
                        case "Transmission Base":
                            type.count = vehicles.SelectMany(x => x.VehicleToTransmissions.Select(y => y.Transmission.TransmissionBaseID)).Distinct().Count();
                            break;
                        /*case "Transmission Control Type":
                            type.count = configs.Select(x => x.Transmission.TransmissionBase.TransmissionControlTypeID).Distinct().Count();
                            break;*/
                        case "Tranmission Manufacturer Code":
                            type.count = vehicles.SelectMany(x => x.VehicleToTransmissions.Select(y => y.Transmission.TransmissionMfrCodeID)).Distinct().Count();
                            break;
                        /*case "Transmission Number of Speeds":
                            type.count = configs.Select(x => x.Transmission.TransmissionBase.TransmissionNumSpeedsID).Distinct().Count();
                            break;*/
                        /*case "Transmission Type":
                            type.count = configs.Select(x => x.Transmission.TransmissionBase.TransmissionTypeID).Distinct().Count();
                            break;*/
                        case "Valves Per Engine":
                            type.count = vehicles.SelectMany(x => x.VehicleToEngineConfigs.Select(y => y.EngineConfig.ValvesID)).Distinct().Count();
                            break;
                        default:
                            type.count = 0;
                            break;
                    }
                }
                vdetails = ConvertConfig(vehicles, attrtypes);
            } catch (Exception e) {
                string x = e.Message;
            }
            return vdetails;
        }

        internal ACESVehicleDetails getVehicleConfigs(int vehicleID) {
            ACESVehicleDetails vdetails = new ACESVehicleDetails();
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            try {
                // get vehicle by ID
                vcdb_Vehicle v = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
                // get all the vehicle configs for the basevehicle and submodel
                vdetails = getVehicleConfigs(v.BaseVehicleID, (int)v.SubModelID);

                List<VehicleConfigAttribute> vattrs = new List<VehicleConfigAttribute>();
                if (v.ConfigID != null) {
                    // get the ACES ConfigAttributes for the config of the vehicle if it exists
                    vattrs = v.VehicleConfig.VehicleConfigAttributes.Where(x => x.ConfigAttribute.ConfigAttributeType.AcesTypeID != null).ToList();
                }
                if (vattrs.Count > 0) {
                    // loop through each of the ACES Vehicle Configs
                    foreach (ACESVehicleConfigType config in vdetails.configs) {
                        // loop through each of the ConfigAttributes to check for matches
                        foreach (VehicleConfigAttribute attr in vattrs) {
                            if (config.type.ID.Equals(attr.ConfigAttribute.ConfigAttributeTypeID) && config.attributes.Any(x => x.vcdbID.Equals(attr.ConfigAttribute.vcdbID))) {
                                // if there exists a ConfigAttribute that matches on type and vcdbID, make sure it's not shown
                                config.type.count = 0;
                            }
                        }
                    }
                }
            } catch { }
            return vdetails;
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

        private ACESVehicleDetails ConvertConfig(List<AAIA.Vehicle> vehicles, List<ConfigAttributeType> types) {
            ACESVehicleDetails vdetails = new ACESVehicleDetails();
            List<ACESVehicleConfigType> results = new List<ACESVehicleConfigType>();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            foreach (ConfigAttributeType type in types) {
                ACESVehicleConfigType ct = new ACESVehicleConfigType();
                ct.type = type;
                List<ConfigAttribute> attribs = new List<ConfigAttribute>();
                switch (type.name) {
                    case "Body Type":
                        foreach (AAIA.BodyType v in vehicles.SelectMany(y => y.VehicleToBodyStyleConfigs.Select(x => x.BodyStyleConfig.BodyType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.BodyTypeID;
                            ca.value = v.BodyTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Number of Doors":
                        foreach (AAIA.BodyNumDoor v in vehicles.SelectMany(y => y.VehicleToBodyStyleConfigs.Select(x => x.BodyStyleConfig.BodyNumDoor)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.BodyNumDoorsID;
                            ca.value = v.BodyNumDoors.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Drive Type":
                        foreach (AAIA.DriveType v in vehicles.SelectMany(y => y.VehicleToDriveTypes.Select(x => x.DriveType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.DriveTypeID;
                            ca.value = v.DriveTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Bed Length":
                        foreach (AAIA.BedLength v in vehicles.SelectMany(y => y.VehicleToBedConfigs.Select(x => x.BedConfig.BedLength)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.BedLengthID;
                            ca.value = v.BedLength1.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Wheel Base":
                        foreach (AAIA.WheelBase v in vehicles.SelectMany(y => y.VehicleToWheelbases.Select(x => x.WheelBase)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.WheelBaseID;
                            ca.value = v.WheelBase1.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Engine":
                        foreach (AAIA.EngineConfig v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.EngineBaseID;
                            ca.value = v.EngineBase.Liter.Trim() + "L " + v.EngineBase.BlockType.Trim() + v.EngineBase.Cylinders.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Fuel Type":
                        foreach (AAIA.FuelType v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.FuelType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.FuelTypeID;
                            ca.value = v.FuelTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Aspiration":
                        foreach (AAIA.Aspiration a in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.Aspiration)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = a.AspirationID;
                            ca.value = a.AspirationName.Trim();
                            if (!attribs.Contains(ca)) {
                                attribs.Add(ca);
                            }
                        }
                        break;
                    case "Bed Type":
                        foreach (AAIA.BedType v in vehicles.SelectMany(y => y.VehicleToBedConfigs.Select(x => x.BedConfig.BedType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.BedTypeID;
                            ca.value = v.BedTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Brake ABS":
                        foreach (AAIA.BrakeAB v in vehicles.SelectMany(y => y.VehicleToBrakeConfigs.Select(x => x.BrakeConfig.BrakeAB)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.BrakeABSID;
                            ca.value = v.BrakeABSName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Brake System":
                        foreach (AAIA.BrakeSystem v in vehicles.SelectMany(y => y.VehicleToBrakeConfigs.Select(x => x.BrakeConfig.BrakeSystem)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.BrakeSystemID;
                            ca.value = v.BrakeSystemName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Cylinder Head Type":
                        foreach (AAIA.CylinderHeadType v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.CylinderHeadType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.CylinderHeadTypeID;
                            ca.value = v.CylinderHeadTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Engine Designation":
                        foreach (AAIA.EngineDesignation v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.EngineDesignation)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.EngineDesignationID;
                            ca.value = v.EngineDesignationName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Engine Manufacturer":
                        foreach (AAIA.Mfr v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.Mfr)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.MfrID;
                            ca.value = v.MfrName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Engine Version":
                        foreach (AAIA.EngineVersion v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.EngineVersion)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.EngineVersionID;
                            ca.value = v.EngineVersion1.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Engine VIN":
                        foreach (AAIA.EngineVIN v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.EngineVIN)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.EngineVINID;
                            ca.value = v.EngineVINName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Front Brake Type":
                        foreach (AAIA.BrakeType v in vehicles.SelectMany(y => y.VehicleToBrakeConfigs.Select(x => x.BrakeConfig.FrontBrakeType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.BrakeTypeID;
                            ca.value = v.BrakeTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Front Spring Type":
                        foreach (AAIA.SpringType v in vehicles.SelectMany(y => y.VehicleToSpringTypeConfigs.Select(x => x.SpringTypeConfig.FrontSpringType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.SpringTypeID;
                            ca.value = v.SpringTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Fuel Delivery Sub-Type":
                        foreach (AAIA.FuelDeliverySubType v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.FuelDeliveryConfig.FuelDeliverySubType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.FuelDeliverySubTypeID;
                            ca.value = v.FuelDeliverySubTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Fuel Delivery Type":
                        foreach (AAIA.FuelDeliveryType v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.FuelDeliveryConfig.FuelDeliveryType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.FuelDeliveryTypeID;
                            ca.value = v.FuelDeliveryTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Fuel System Control Type":
                        foreach (AAIA.FuelSystemControlType v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.FuelDeliveryConfig.FuelSystemControlType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.FuelSystemControlTypeID;
                            ca.value = v.FuelSystemControlTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Fuel System Design":
                        foreach (AAIA.FuelSystemDesign v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.FuelDeliveryConfig.FuelSystemDesign)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.FuelSystemDesignID;
                            ca.value = v.FuelSystemDesignName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Ignition System Type":
                        foreach (AAIA.IgnitionSystemType v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.IgnitionSystemType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.IgnitionSystemTypeID;
                            ca.value = v.IgnitionSystemTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Manufacturer Body Code":
                        foreach (AAIA.MfrBodyCode v in vehicles.SelectMany(y => y.VehicleToMfrBodyCodes.Select(x => x.MfrBodyCode)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.MfrBodyCodeID;
                            ca.value = v.MfrBodyCodeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Power Output":
                        foreach (int v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.PowerOutputID)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v;
                            ca.value = vcdb.PowerOutputs.Where(x => x.PowerOutputID.Equals(v)).Select(x => x.HorsePower).First().Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Rear Brake Type":
                        foreach (AAIA.BrakeType v in vehicles.SelectMany(y => y.VehicleToBrakeConfigs.Select(x => x.BrakeConfig.RearBrakeType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.BrakeTypeID;
                            ca.value = v.BrakeTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Rear Spring Type":
                        foreach (AAIA.SpringType v in vehicles.SelectMany(y => y.VehicleToSpringTypeConfigs.Select(x => x.SpringTypeConfig.RearSpringType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.SpringTypeID;
                            ca.value = v.SpringTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Steering System":
                        foreach (AAIA.SteeringSystem v in vehicles.SelectMany(y => y.VehicleToSteeringConfigs.Select(x => x.SteeringConfig.SteeringSystem)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.SteeringSystemID;
                            ca.value = v.SteeringSystemName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Steering Type":
                        foreach (AAIA.SteeringType v in vehicles.SelectMany(y => y.VehicleToSteeringConfigs.Select(x => x.SteeringConfig.SteeringType)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.SteeringTypeID;
                            ca.value = v.SteeringTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Tranmission Electronic Controlled":
                        foreach (AAIA.ElecControlled v in vehicles.SelectMany(y => y.VehicleToTransmissions.Select(x => x.Transmission.ElecControlled)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.ElecControlledID;
                            ca.value = v.ElecControlled1.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Transmission Base":
                        foreach (AAIA.TransmissionBase v in vehicles.SelectMany(y => y.VehicleToTransmissions.Select(x => x.Transmission.TransmissionBase)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.TransmissionBaseID;
                            ca.value = v.TransmissionNumSpeed.TransmissionNumSpeeds.Trim() + "sp " + v.TransmissionControlType.TransmissionControlTypeName.Trim() + " " + v.TransmissionType.TransmissionTypeName.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Tranmission Manufacturer Code":
                        foreach (AAIA.TransmissionMfrCode v in vehicles.SelectMany(y => y.VehicleToTransmissions.Select(x => x.Transmission.TransmissionMfrCode)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.TransmissionMfrCodeID;
                            ca.value = v.TransmissionMfrCode1.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    case "Valves Per Engine":
                        foreach (AAIA.Valve v in vehicles.SelectMany(y => y.VehicleToEngineConfigs.Select(x => x.EngineConfig.Valve)).Distinct().ToList()) {
                            ConfigAttribute ca = new ConfigAttribute();
                            ca.ConfigAttributeType = type;
                            ca.vcdbID = v.ValvesID;
                            ca.value = v.ValvesPerEngine.Trim();
                            attribs.Add(ca);
                        }
                        break;
                    default:
                        type.count = 0;
                        break;
                }
                ct.attributes = attribs;
                results.Add(ct);
            }
            vdetails.configs = results.OrderBy(x => x.type.sort).ToList();
            return vdetails;
        }

        public ACESBaseVehicle addConfig(int BaseVehicleID, int SubmodelID, List<int> configids) {
            CurtDevDataContext db = new CurtDevDataContext();
            ACESVehicleDetails vdetails = new ACESVehicleDetails();
            List<vcdb_Vehicle> newVehicles = new List<vcdb_Vehicle>();
            vdetails = new ACES().getVehicleConfigs(BaseVehicleID, SubmodelID);
            //Dictionary<int, List<ConfigAttribute>> attributeSet = new Dictionary<int, List<ConfigAttribute>>();
            List<List<ConfigAttribute>> attributeSet = new List<List<ConfigAttribute>>();
            foreach (int id in configids) {
                List<ConfigAttribute> attributes = vdetails.configs.SelectMany(x => x.attributes.Where(y => y.ConfigAttributeTypeID.Equals(id))).ToList<ConfigAttribute>();
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
            vcdb_Vehicle vehicle = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID)).First();
            List<ConfigAttributeType> types = new List<ConfigAttributeType>();
            if (vehicle.BaseVehicle.AAIABaseVehicleID == null || vehicle.Submodel.AAIASubmodelID == null) {
                List<ConfigAttributeType> alltypes = db.ConfigAttributeTypes.OrderBy(x => x.name).ToList();
                List<VehicleConfigAttribute> vehicleattr = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID) && x.ConfigID != null).SelectMany(x => x.VehicleConfig.VehicleConfigAttributes).Distinct().ToList();
                List<ConfigAttributeType> vehicletypes = vehicleattr.Select(x => x.ConfigAttribute.ConfigAttributeType).Distinct().ToList<ConfigAttributeType>();
                types = alltypes.Except(vehicletypes).OrderBy(x => x.name).ToList<ConfigAttributeType>();
            } else {
                List<ConfigAttributeType> alltypes = db.ConfigAttributeTypes.Where(x => x.AcesTypeID == null).OrderBy(x => x.name).ToList();
                List<VehicleConfigAttribute> vehicleattr = db.vcdb_Vehicles.Where(x => x.ID.Equals(vehicleID) && x.ConfigID != null).SelectMany(x => x.VehicleConfig.VehicleConfigAttributes).Distinct().ToList();
                List<ConfigAttributeType> vehicletypes = vehicleattr.Where(x => x.ConfigAttribute.ConfigAttributeType.AcesTypeID == null).Select(x => x.ConfigAttribute.ConfigAttributeType).Distinct().ToList<ConfigAttributeType>();
                types = alltypes.Except(vehicletypes).OrderBy(x => x.name).ToList<ConfigAttributeType>();
            }
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
            List<ConfigAttribute> attributelist = new List<ConfigAttribute>();
            if(vehicle.ConfigID != null) {
                attributelist = vehicle.VehicleConfig.VehicleConfigAttributes.Where(x => x.AttributeID != attributeID).Select(x => x.ConfigAttribute).ToList<ConfigAttribute>();
            }
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

        public string SearchMakes(string keyword = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            if (keyword != "" && keyword.Length >= 1) {
                List<Autocomplete> makes = (from m in db.vcdb_Makes
                                            where m.MakeName.Contains(keyword)
                                            select new Autocomplete {
                                                id = 1,
                                                label = m.MakeName.Trim(),
                                                value = m.MakeName.Trim()
                                            }).ToList();
                List<Autocomplete> vcdbMakes = (from m in vcdb.Makes
                                                where m.MakeName.Contains(keyword)
                                                select new Autocomplete {
                                                    id = 1,
                                                    label = m.MakeName.Trim(),
                                                    value = m.MakeName.Trim()
                                                }).ToList();
                makes.AddRange(vcdbMakes);
                List<Autocomplete> allmakes = makes.Distinct().ToList();
                return JsonConvert.SerializeObject(allmakes);
            }
            return "[]";
        }

        public string SearchModels(string keyword = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            if (keyword != "" && keyword.Length >= 1) {
                List<Autocomplete> models = (from m in db.vcdb_Models
                                             where m.ModelName.Contains(keyword)
                                             orderby m.ModelName
                                             select new Autocomplete {
                                                 id = 1,
                                                 label = m.ModelName.Trim(),
                                                 value = m.ModelName.Trim()
                                             }).ToList();
                List<Autocomplete> vcdbModels = (from m in vcdb.Models
                                                 where m.ModelName.Contains(keyword)
                                                 select new Autocomplete {
                                                     id = 1,
                                                     label = m.ModelName.Trim(),
                                                     value = m.ModelName.Trim()
                                                 }).ToList();
                models.AddRange(vcdbModels);
                List<Autocomplete> allmodels = models.Distinct().ToList();
                return JsonConvert.SerializeObject(models);
            }
            return "[]";
        }

        public string SearchSubmodels(string keyword = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            if (keyword != "" && keyword.Length >= 1) {
                List<Autocomplete> submodels = (from s in db.Submodels
                                                where s.SubmodelName.Contains(keyword)
                                                select new Autocomplete {
                                                    id = 1,
                                                    label = s.SubmodelName,
                                                    value = s.SubmodelName
                                                }).ToList();
                List<Autocomplete> vcdbSubmodels = (from s in vcdb.Submodels
                                                    where s.SubmodelName.Contains(keyword)
                                                    select new Autocomplete {
                                                        id = 1,
                                                        label = s.SubmodelName.Trim(),
                                                        value = s.SubmodelName.Trim()
                                                    }).ToList();
                submodels.AddRange(vcdbSubmodels);
                List<Autocomplete> allsubmodels = submodels.Distinct().ToList();
                return JsonConvert.SerializeObject(allsubmodels);
            }
            return "[]";
        }

        public int RunCheck() {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<vcdb_Vehicle> vehicles = new List<vcdb_Vehicle>();
            List<vcdb_Make> makes = db.vcdb_Makes.Where(x => x.AAIAMakeID.Equals(null)).ToList();
            foreach(vcdb_Make make in makes) {
                try {
                    AAIA.Make amake = vcdb.Makes.Where(x => x.MakeName.Trim().ToLower().Equals(make.MakeName.Trim().ToLower())).First();
                    make.MakeName = amake.MakeName.Trim();
                    make.AAIAMakeID = amake.MakeID;
                } catch {}
            }

            List<vcdb_Model> models = db.vcdb_Models.Where(x => x.AAIAModelID.Equals(null)).ToList();
            foreach(vcdb_Model model in models) {
                try {
                    AAIA.Model amodel = vcdb.Models.Where(x => x.ModelName.Trim().ToLower().Equals(model.ModelName.Trim().ToLower())).First();
                    model.ModelName = amodel.ModelName.Trim();
                    model.AAIAModelID = amodel.ModelID;
                    model.VehicleTypeID = amodel.VehicleTypeID;
                } catch {}
            }

            List<Submodel> submodels = db.Submodels.Where(x => x.AAIASubmodelID.Equals(null)).ToList();
            foreach(Submodel submodel in submodels) {
                try {
                    AAIA.Submodel asubmodel = vcdb.Submodels.Where(x => x.SubmodelName.Trim().ToLower().Equals(submodel.SubmodelName.Trim().ToLower())).First();
                    submodel.SubmodelName = asubmodel.SubmodelName.Trim();
                    submodel.AAIASubmodelID = asubmodel.SubmodelID;
                } catch {}
            }
            db.SubmitChanges();

            int updateCount = 0;
            List<BaseVehicle> baseVehicles = db.BaseVehicles.Where(x => x.AAIABaseVehicleID.Equals(null)).ToList();
            foreach (BaseVehicle baseVehicle in baseVehicles) {
                try {
                    AAIA.BaseVehicle bv = vcdb.BaseVehicles.Where(x => x.YearID.Equals(baseVehicle.YearID) && x.MakeID.Equals(baseVehicle.vcdb_Make.AAIAMakeID) && x.ModelID.Equals(baseVehicle.vcdb_Model.AAIAModelID)).First();
                    baseVehicle.AAIABaseVehicleID = bv.BaseVehicleID;
                    updateCount++;
                } catch { }
            }
            db.SubmitChanges();
            return updateCount;
        }

        public List<Vehicles> MapPart(int id) {
            CurtDevDataContext db = new CurtDevDataContext();
            AAIA.VCDBDataContext vcdb = new AAIA.VCDBDataContext();
            List<VehiclePart> vehicleParts = new List<VehiclePart>();
            vehicleParts = db.VehicleParts.Where(x => x.partID.Equals(id)).ToList();
            List<Vehicles> unmapped = new List<Vehicles>();
            foreach (VehiclePart vpart in vehicleParts) {
                try {
                    int yearID = Convert.ToInt32(vpart.Vehicles.Year.year1);
                    vcdb_Year year = db.vcdb_Years.Where(x => x.YearID.Equals(yearID)).First();

                    string makename = vpart.Vehicles.Make.make1;
                    AAIA.Make aaiamake = vcdb.Makes.Where(x => x.MakeName.Trim().ToLower().Equals(makename.Trim().ToLower())).First();
                    vcdb_Make make = db.vcdb_Makes.Where(x => x.AAIAMakeID.Equals(aaiamake.MakeID)).First();

                    string modelname = vpart.Vehicles.Model.model1;
                    AAIA.Model aaiamodel = vcdb.Models.Where(x => x.ModelName.Trim().ToLower().Equals(modelname.Trim().ToLower())).First();
                    vcdb_Model model = db.vcdb_Models.Where(x => x.AAIAModelID.Equals(aaiamodel.ModelID)).First();

                    // get the basevehicle
                    BaseVehicle bv = db.BaseVehicles.Where(x => x.YearID.Equals(year.YearID) && x.MakeID.Equals(make.ID) && x.ModelID.Equals(model.ID)).FirstOrDefault();
                    if (bv == null || bv.ID == 0) {
                        AAIA.BaseVehicle aaiabv = vcdb.BaseVehicles.Where(x => x.YearID.Equals(year.YearID) && x.MakeID.Equals(make.AAIAMakeID) && x.ModelID.Equals(model.AAIAModelID)).First();
                        bv = new BaseVehicle {
                            AAIABaseVehicleID = aaiabv.BaseVehicleID,
                            YearID = year.YearID,
                            MakeID = make.ID,
                            ModelID = model.ID
                        };
                        db.BaseVehicles.InsertOnSubmit(bv);
                        db.SubmitChanges();
                    }
                    
                    // get the submodel if it exists
                    Submodel submodel = new Submodel();
                    string stylename = vpart.Vehicles.Style.style1;
                    AAIA.Submodel aaiasubmodel = vcdb.Vehicles.Where(x => x.BaseVehicleID.Equals(bv.AAIABaseVehicleID)).Select(x => x.Submodel).Where(x => x.SubmodelName.Trim().ToLower().Equals(stylename.Trim().ToLower())).FirstOrDefault();
                    if (aaiasubmodel != null && aaiasubmodel.SubmodelID > 0) {
                        submodel = db.Submodels.Where(x => x.AAIASubmodelID.Equals(aaiasubmodel.SubmodelID)).FirstOrDefault();
                        if(submodel == null || submodel.ID == 0) {
                            // submodel is not yet in the data
                            submodel = new Submodel {
                                AAIASubmodelID = aaiasubmodel.SubmodelID,
                                SubmodelName = aaiasubmodel.SubmodelName.Trim(),
                            };
                            db.Submodels.InsertOnSubmit(submodel);
                            db.SubmitChanges();
                        }
                    }

                    // get the vehicle if it exists
                    vcdb_Vehicle vehicle = db.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bv.ID) && x.SubModelID.Equals(((submodel == null || submodel.ID == 0) ? null : (int?)submodel.ID)) && x.ConfigID.Equals(null)).FirstOrDefault();
                    if(vehicle == null || vehicle.ID == 0) {
                        vehicle = new vcdb_Vehicle {
                            BaseVehicleID = bv.ID,
                            SubModelID = (submodel == null || submodel.ID == 0) ? null : (int?)submodel.ID
                        };
                        db.vcdb_Vehicles.InsertOnSubmit(vehicle);
                        db.SubmitChanges();
                    }

                    vcdb_VehiclePart vp = new vcdb_VehiclePart();
                    vp = db.vcdb_VehicleParts.Where(x => x.VehicleID.Equals(vehicle.ID) && x.PartNumber.Equals(vpart.partID)).FirstOrDefault();
                    if(vp == null || vp.ID == 0) {
                        vp = new vcdb_VehiclePart {
                            PartNumber = vpart.partID,
                            VehicleID = vehicle.ID
                        };
                        db.vcdb_VehicleParts.InsertOnSubmit(vp);
                        db.SubmitChanges();
                    }

                    List<Note> notes = new List<Note>();
                    foreach (VehiclePartAttribute vpa in vpart.VehiclePartAttributes) {
                        Note note = new Note {
                            note1 = vpa.field + ": " + vpa.value,
                            vehiclePartID = vp.ID
                        };
                        if (!vp.Notes.Any(x => x.note1.Equals(note.note1))) {
                            notes.Add(note);
                        }
                    }
                    if (notes.Count > 0) {
                        db.Notes.InsertAllOnSubmit(notes);
                        db.SubmitChanges();
                    }

                } catch {
                    unmapped.Add(vpart.Vehicles);
                }
            }
            return unmapped;
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

    public class ACESMakeComparer : IEqualityComparer<ACESMake> {
        bool IEqualityComparer<ACESMake>.Equals(ACESMake x, ACESMake y) {
            // Check whether the compared objects reference the same data.
            if (x.name.Trim().Equals(y.name.Trim()) && x.AAIAID.Equals(y.AAIAID)) {
                return true;
            } else {
                return false;
            }

        }

        int IEqualityComparer<ACESMake>.GetHashCode(ACESMake obj) {
            return obj.name.Trim().GetHashCode();
        }
    }


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
        public List<AAIA.VehicleToBedConfig> BedConfigs { get; set; }
        public List<AAIA.VehicleToBodyStyleConfig> BodyStyleConfigs { get; set; }
        public List<AAIA.VehicleToBrakeConfig> BrakeConfigs { get; set; }
        public List<AAIA.VehicleToDriveType> DriveTypes { get; set; }
        public List<AAIA.VehicleToEngineConfig> EngineConfigs { get; set; }
        public List<AAIA.VehicleToMfrBodyCode> MfrBodyCodes { get; set; }
        public List<AAIA.VehicleToSpringTypeConfig> SpringTypeConfigs { get; set; }
        public List<AAIA.VehicleToSteeringConfig> SteeringConfigs { get; set; }
        public List<AAIA.VehicleToTransmission> Transmissions { get; set; }
        public List<AAIA.VehicleToWheelbase> Wheelbases { get; set; }
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

    public class ACESVehicleConfig {
        public List<ConfigAttribute> attributes { get; set; }
    }

    public class ACESVehicleConfigType {
        public ConfigAttributeType type { get; set; }
        public List<ConfigAttribute> attributes { get; set; }
    }
    public class ACESVehicleDetails {
        public List<ACESVehicleConfigType> configs { get; set; }

        /*public void clearDuplicates() {
            List<ACESVehicleConfig> paredlist = new List<ACESVehicleConfig>();
            List<ACESVehicleConfigType> typelist = configs.Where(x => x.type.count > 1).ToList();
            List<int> typeids = typelist.Select(x => x.type.ID).ToList();
            for (int i = 0; i < this.configs.Count; i++) {
                bool exists = false;
                foreach (ACESVehicleConfig config in paredlist) {
                    List<ConfigAttribute> attribs = config.attributes.Where(x => typeids.Contains(x.ConfigAttributeTypeID)).OrderBy(x => x.ConfigAttributeType.sort).ToList();
                    List<ConfigAttribute> masterattribs = this.configs[i].attributes.Where(x => typeids.Contains(x.ConfigAttributeTypeID)).OrderBy(x => x.ConfigAttributeType.sort).ToList();
                    int exceptCount = attribs.Except(masterattribs, new ConfigAttributeComparer()).Count();
                    if (exceptCount == 0) {
                        exists = true;
                    }
                }
                if (!exists) {
                    paredlist.Add(this.configs[i]);
                }
            }
            this.configs = paredlist;
        }*/
    }

    public class Autocomplete {
        public int id { get; set; }
        public string label { get; set; }
        public string value { get; set; }
    }
}