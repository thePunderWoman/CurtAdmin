using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class ACESImportController : BaseController
    {

        public ActionResult Index(int vcount = 0) {
            ViewBag.vcount = vcount;
            return View();
        }

        public ActionResult Import() {
            int vcount = 0;
            CurtDevDataContext db = new CurtDevDataContext();

            ParallelQuery<ACEVehicle> vehicles = (from ace in db.ACESImports
                                                  select new ACEVehicle {
                                                      AppID = ace.AAIAAPPID,
                                                      BaseVehicleID = ace.AAIABaseVehicleID,
                                                      SubmodelID = ace.AAIASubModelID,
                                                      PartNumber = ace.PartNumber
                                                  }).Distinct().AsParallel<ACEVehicle>();

            Parallel.ForEach(vehicles, v => {
                CurtDevDataContext db2 = new CurtDevDataContext();
                VCDB.VCDBDataContext vcdb = new VCDB.VCDBDataContext();
                BaseVehicle bv = db2.BaseVehicles.Where(x => x.AAIABaseVehicleID.Equals(v.BaseVehicleID)).First<BaseVehicle>();
                vcdb_Vehicle vehicle = new vcdb_Vehicle();
                if (v.SubmodelID != null) {
                    Submodel s = db2.Submodels.Where(x => x.AAIASubmodelID.Equals(v.SubmodelID)).First<Submodel>();
                    List<ACEAttribute> attribs = (from att in db2.ACESImports
                                                  where att.AAIABaseVehicleID.Equals(v.BaseVehicleID) && att.AAIASubModelID.Equals(v.SubmodelID) && att.PartNumber.Equals(v.PartNumber)
                                                  && att.AAIAAttributeID != null && att.AAIAAPPID.Equals(v.AppID)
                                                  select new ACEAttribute {
                                                      AttributeID = (int)att.AAIAAttributeID,
                                                      Name = att.AAIAAttribFamilyNm,
                                                      Table = att.AAIATableNm
                                                  }).Distinct().ToList<ACEAttribute>();
                    if (attribs.Count == 0) {
                        // No Configuration needed
                        try {
                            vehicle = db2.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bv.ID) && x.SubModelID.Equals(s.ID)).First<vcdb_Vehicle>();
                            //vehicle exists

                        } catch {
                            //vehicle does not exist
                            vehicle = new vcdb_Vehicle {
                                BaseVehicleID = bv.ID,
                                SubModelID = s.ID
                            };
                            db2.vcdb_Vehicles.InsertOnSubmit(vehicle);
                            db2.SubmitChanges();

                        }

                        // check if a vehicle exists that has no Submodel
                        List<vcdb_Vehicle> removablevehicles = db2.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bv.ID) && x.SubModelID == null).ToList<vcdb_Vehicle>();
                        List<vcdb_VehiclePart> removableparts = new List<vcdb_VehiclePart>();
                        foreach (vcdb_Vehicle rv in removablevehicles) {
                            List<vcdb_VehiclePart> rps = db2.vcdb_VehicleParts.Where(x => x.VehicleID.Equals(rv.ID)).ToList<vcdb_VehiclePart>();
                            removableparts.AddRange(rps);
                        }
                        try {
                            db2.vcdb_VehicleParts.DeleteAllOnSubmit(removableparts);
                            db2.SubmitChanges();
                            db2.vcdb_Vehicles.DeleteAllOnSubmit(removablevehicles);
                            db2.SubmitChanges();
                        } catch { }

                        try {
                            // check for vpart relationship existence
                            vcdb_VehiclePart vpart = db2.vcdb_VehicleParts.Where(x => x.PartNumber.Equals(v.PartNumber) && x.VehicleID.Equals(vehicle.ID)).First<vcdb_VehiclePart>();
                            // do nothing because the relationship exists
                        } catch {
                            try {
                                // add vehicle Part relationship
                                vcdb_VehiclePart vpart = new vcdb_VehiclePart {
                                    VehicleID = vehicle.ID,
                                    PartNumber = v.PartNumber
                                };
                                db2.vcdb_VehicleParts.InsertOnSubmit(vpart);
                                db2.SubmitChanges();
                            } catch { }
                        }
                    } else {
                        // need configurations from attribs

                        // create new vehicle configuration
                        VehicleConfig vconfig = new VehicleConfig();
                        db2.VehicleConfigs.InsertOnSubmit(vconfig);
                        db2.SubmitChanges();

                        List<VehicleConfigAttribute> vconfigattrs = new List<VehicleConfigAttribute>();

                        foreach (ACEAttribute atr in attribs) {
                            ConfigAttribute newattr = new ConfigAttribute();
                            switch (atr.Name) {
                                case "Wheel Base":
                                    // 1
                                    try {
                                        // check if attribute exists already to prevent duplicates
                                        newattr = db2.ConfigAttributes.Where(x => x.vcdbID.Equals(atr.AttributeID) && x.ConfigAttributeTypeID.Equals(1)).First<ConfigAttribute>();
                                    } catch {
                                        // attribute doesn't exist
                                        newattr.vcdbID = atr.AttributeID;
                                        newattr.parentID = 0;
                                        newattr.ConfigAttributeTypeID = 1;
                                        newattr.value = vcdb.WheelBases.Where(x => x.WheelBaseID.Equals(atr.AttributeID)).Select(x => x.WheelBase1).FirstOrDefault();
                                        db2.ConfigAttributes.InsertOnSubmit(newattr);
                                        db2.SubmitChanges();
                                    }
                                    break;
                                case "Body Type":
                                    // 2
                                    try {
                                        // check if attribute exists already to prevent duplicates
                                        newattr = db2.ConfigAttributes.Where(x => x.vcdbID.Equals(atr.AttributeID) && x.ConfigAttributeTypeID.Equals(2)).First<ConfigAttribute>();
                                    } catch {
                                        newattr.vcdbID = atr.AttributeID;
                                        newattr.parentID = 0;
                                        newattr.ConfigAttributeTypeID = 2;
                                        newattr.value = vcdb.BodyTypes.Where(x => x.BodyTypeID.Equals(atr.AttributeID)).Select(x => x.BodyTypeName).FirstOrDefault();
                                        db2.ConfigAttributes.InsertOnSubmit(newattr);
                                        db2.SubmitChanges();
                                    }
                                    break;
                                case "Drive Type":
                                    // 3
                                    try {
                                        // check if attribute exists already to prevent duplicates
                                        newattr = db2.ConfigAttributes.Where(x => x.vcdbID.Equals(atr.AttributeID) && x.ConfigAttributeTypeID.Equals(3)).First<ConfigAttribute>();
                                    } catch {
                                        newattr.vcdbID = atr.AttributeID;
                                        newattr.parentID = 0;
                                        newattr.ConfigAttributeTypeID = 3;
                                        newattr.value = vcdb.DriveTypes.Where(x => x.DriveTypeID.Equals(atr.AttributeID)).Select(x => x.DriveTypeName).FirstOrDefault();
                                        db2.ConfigAttributes.InsertOnSubmit(newattr);
                                        db2.SubmitChanges();
                                    }
                                    break;
                                case "Number of Doors":
                                    // 4
                                    try {
                                        // check if attribute exists already to prevent duplicates
                                        newattr = db2.ConfigAttributes.Where(x => x.vcdbID.Equals(atr.AttributeID) && x.ConfigAttributeTypeID.Equals(4)).First<ConfigAttribute>();
                                    } catch {
                                        newattr.vcdbID = atr.AttributeID;
                                        newattr.parentID = 0;
                                        newattr.ConfigAttributeTypeID = 4;
                                        newattr.value = vcdb.BodyNumDoors.Where(x => x.BodyNumDoorsID.Equals(atr.AttributeID)).Select(x => x.BodyNumDoors).FirstOrDefault();
                                        db2.ConfigAttributes.InsertOnSubmit(newattr);
                                        db2.SubmitChanges();
                                    }
                                    break;
                                case "Bed Length":
                                    // 5
                                    try {
                                        // check if attribute exists already to prevent duplicates
                                        newattr = db2.ConfigAttributes.Where(x => x.vcdbID.Equals(atr.AttributeID) && x.ConfigAttributeTypeID.Equals(5)).First<ConfigAttribute>();
                                    } catch {
                                        newattr.vcdbID = atr.AttributeID;
                                        newattr.parentID = 0;
                                        newattr.ConfigAttributeTypeID = 5;
                                        newattr.value = vcdb.BedLengths.Where(x => x.BedLengthID.Equals(atr.AttributeID)).Select(x => x.BedLength1).FirstOrDefault();
                                        db2.ConfigAttributes.InsertOnSubmit(newattr);
                                        db2.SubmitChanges();
                                    }
                                    break;
                                case "Fuel Type":
                                    // 6
                                    try {
                                        // check if attribute exists already to prevent duplicates
                                        newattr = db2.ConfigAttributes.Where(x => x.vcdbID.Equals(atr.AttributeID) && x.ConfigAttributeTypeID.Equals(6)).First<ConfigAttribute>();
                                    } catch {
                                        newattr.vcdbID = atr.AttributeID;
                                        newattr.parentID = 0;
                                        newattr.ConfigAttributeTypeID = 6;
                                        newattr.value = vcdb.FuelTypes.Where(x => x.FuelTypeID.Equals(atr.AttributeID)).Select(x => x.FuelTypeName).FirstOrDefault();
                                        db2.ConfigAttributes.InsertOnSubmit(newattr);
                                        db2.SubmitChanges();
                                    }
                                    break;
                            }
                            // we have the attribute...now we need to add it to the list
                            VehicleConfigAttribute vconfigattr = new VehicleConfigAttribute {
                                AttributeID = newattr.ID,
                                VehicleConfigID = vconfig.ID
                            };
                            vconfigattrs.Add(vconfigattr);
                        }
                        db2.VehicleConfigAttributes.InsertAllOnSubmit(vconfigattrs);
                        db2.SubmitChanges();

                        // attributes and configuration created, now add vehicle
                        vehicle = new vcdb_Vehicle {
                            BaseVehicleID = bv.ID,
                            SubModelID = s.ID,
                            ConfigID = vconfig.ID
                        };
                        db2.vcdb_Vehicles.InsertOnSubmit(vehicle);
                        db2.SubmitChanges();

                        // check if a vehicle exists that has no Submodel or config
                        List<vcdb_Vehicle> removablevehicles = db2.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bv.ID) && (x.SubModelID == null || x.ConfigID == null)).ToList<vcdb_Vehicle>();
                        List<vcdb_VehiclePart> removableparts = new List<vcdb_VehiclePart>();
                        foreach(vcdb_Vehicle rv in removablevehicles) {
                            List<vcdb_VehiclePart> rps = db2.vcdb_VehicleParts.Where(x => x.VehicleID.Equals(rv.ID)).ToList<vcdb_VehiclePart>();
                            removableparts.AddRange(rps);
                        }
                        try {
                            db2.vcdb_VehicleParts.DeleteAllOnSubmit(removableparts);
                            db2.SubmitChanges();
                            db2.vcdb_Vehicles.DeleteAllOnSubmit(removablevehicles);
                            db2.SubmitChanges();
                        } catch { }

                        // now that we have the vehicle, add the part relationship

                        try {
                            // check for vpart relationship existence
                            vcdb_VehiclePart vpart = db2.vcdb_VehicleParts.Where(x => x.PartNumber.Equals(v.PartNumber) && x.VehicleID.Equals(vehicle.ID)).First<vcdb_VehiclePart>();
                            // do nothing because the relationship exists
                        } catch {
                            try {
                                // add vehicle Part relationship
                                vcdb_VehiclePart vpart = new vcdb_VehiclePart {
                                    VehicleID = vehicle.ID,
                                    PartNumber = v.PartNumber
                                };
                                db2.vcdb_VehicleParts.InsertOnSubmit(vpart);
                                db2.SubmitChanges();
                            } catch { }
                        }

                    }
                } else {
                    try {
                        vehicle = db2.vcdb_Vehicles.Where(x => x.BaseVehicleID.Equals(bv.ID)).First<vcdb_Vehicle>();
                        //vehicle exists

                    } catch {
                        //vehicle does not exist
                        vehicle = new vcdb_Vehicle {
                            BaseVehicleID = bv.ID
                        };
                        db2.vcdb_Vehicles.InsertOnSubmit(vehicle);
                        db2.SubmitChanges();

                    }
                    try {
                        // check for vpart relationship existence
                        vcdb_VehiclePart vpart = db2.vcdb_VehicleParts.Where(x => x.PartNumber.Equals(v.PartNumber) && x.VehicleID.Equals(vehicle.ID)).First<vcdb_VehiclePart>();
                        // do nothing because the relationship exists
                    } catch {
                        try {
                            // add vehicle Part relationship
                            vcdb_VehiclePart vpart = new vcdb_VehiclePart {
                                VehicleID = vehicle.ID,
                                PartNumber = v.PartNumber
                            };
                            db2.vcdb_VehicleParts.InsertOnSubmit(vpart);
                            db2.SubmitChanges();
                        } catch { }
                    }
                }
            });
            vcount = vehicles.Count();

            return RedirectToAction("Index", new { vcount = vcount });
        }

    }

    public class ACEVehicle {
        public int AppID { get; set; }
        public int BaseVehicleID { get; set; }
        public int? SubmodelID  { get; set; }
        public int PartNumber { get; set; }
    }

    public class ACEAttribute {
        public int AttributeID { get; set; }
        public string Name { get; set; }
        public string Table { get; set; }
    }

}
