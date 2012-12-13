using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using CurtAdmin.Models;
using System.Text.RegularExpressions;

namespace CurtAdmin.Models {
    public class ProductModels {

        public static List<ConvertedPart> GetAllParts() {
            List<ConvertedPart> parts = new List<ConvertedPart>();
            CurtDevDataContext db = new CurtDevDataContext();
            parts = (from p in db.Parts
                     select new ConvertedPart {
                         partID = p.partID,
                         status = p.status,
                         dateModified = Convert.ToDateTime(p.dateModified).ToString(),
                         dateAdded = Convert.ToDateTime(p.dateAdded).ToString(),
                         shortDesc = p.shortDesc,
                         oldPartNumber = p.oldPartNumber,
                         priceCode = Convert.ToInt32(p.priceCode),
                         pClass = p.classID,
                         featured = p.featured,
                         listPrice = String.Format("{0:C}", (from prices in db.Prices
                                                             where prices.partID.Equals(p.partID) && prices.priceType.Equals("List")
                                                             select prices.price1 != null ? prices.price1 : (decimal?)0).FirstOrDefault<decimal?>())
                     }).ToList<ConvertedPart>();
            return parts;
        }

        public static List<Class> GetClasses() {
            List<Class> classes = new List<Class>();
            CurtDevDataContext db = new CurtDevDataContext();
            classes = (from c in db.Classes
                       orderby c.class1
                       select c).ToList<Class>();
            return classes;
        }

        public static ConvertedPart GetPart(int partID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();

            ConvertedPart part = new ConvertedPart();
            part = (from p in db.Parts
                    where p.partID.Equals(partID)
                    select new ConvertedPart {
                        partID = p.partID,
                        status = p.status,
                        dateModified = Convert.ToDateTime(p.dateModified).ToString(),
                        dateAdded = Convert.ToDateTime(p.dateAdded).ToString(),
                        shortDesc = p.shortDesc,
                        oldPartNumber = p.oldPartNumber,
                        priceCode = Convert.ToInt32(p.priceCode),
                        pClass = p.classID,
                        featured = p.featured,
                        ACESPartTypeID = p.ACESPartTypeID,
                        listPrice = String.Format("{0:C}", (from prices in db.Prices
                                                            where prices.partID.Equals(p.partID) && prices.priceType.Equals("List")
                                                            select prices.price1 != null ? prices.price1 : (decimal?)0).FirstOrDefault<decimal?>())
                    }).FirstOrDefault<ConvertedPart>();
            return part;
        }

        public static List<ConvertedPart> GetRelatedParts(int partID = 0) {
            List<ConvertedPart> parts = new List<ConvertedPart>();
            CurtDevDataContext db = new CurtDevDataContext();

            parts = (from p in db.Parts
                     join rp in db.RelatedParts on p.partID equals rp.relatedID
                     where rp.partID.Equals(partID)
                     select new ConvertedPart {
                         partID = p.partID,
                         status = p.status,
                         dateModified = Convert.ToDateTime(p.dateModified).ToString(),
                         dateAdded = Convert.ToDateTime(p.dateAdded).ToString(),
                         shortDesc = p.shortDesc,
                         oldPartNumber = p.oldPartNumber,
                         priceCode = Convert.ToInt32(p.priceCode),
                         pClass = p.classID,
                         featured = p.featured,
                         listPrice = String.Format("{0:C}", (from prices in db.Prices
                                                             where prices.partID.Equals(p.partID) && prices.priceType.Equals("List")
                                                             select prices.price1 != null ? prices.price1 : (decimal?)0).FirstOrDefault<decimal?>())
                     }).ToList<ConvertedPart>();
            return parts;
        }

        public static string AddCategoryToPart(int catID = 0, int partID = 0) {
            if (catID == 0 || partID == 0) { return "Invalid parameters."; }

            try {
                CurtDevDataContext db = new CurtDevDataContext();

                // Make sure this category isn't already tied to this product
                int existing = (from cp in db.CatParts
                                where cp.catID.Equals(catID) && cp.partID.Equals(partID)
                                select cp).Count();
                if (existing > 0) { return "The association exists."; }

                CatParts cat_part = new CatParts {
                    catID = catID,
                    partID = partID
                };
                db.CatParts.InsertOnSubmit(cat_part);
                db.SubmitChanges();
                UpdatePart(partID);
            } catch (Exception e) {
                return e.Message;
            }
            return "";
        }

        public static string DeleteCategoryFromPart(int catID = 0, int partID = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<CatParts> cat_part = (from cp in db.CatParts
                                           where cp.catID.Equals(catID) && cp.partID.Equals(partID)
                                           select cp).ToList<CatParts>();
                if (cat_part.Count == 0) { return "Error deleting association."; }

                db.CatParts.DeleteAllOnSubmit<CatParts>(cat_part);
                db.SubmitChanges();
                UpdatePart(partID);
            } catch (Exception e) {
                return e.Message;
            }
            return "";
        }

        /// <summary>
        /// Delete a given part and all it's dependent/related data.
        /// </summary>
        /// <param name="partID">ID of the part to be deleted</param>
        /// <returns>Error of type string.</returns>
        public static string DeletePart(int partID = 0) {
            if (partID == 0) { return "partID was invalid."; }
            try {

                CurtDevDataContext db = new CurtDevDataContext();

                // Get the part
                Part part = new Part();
                part = (from p in db.Parts
                        where p.partID.Equals(partID)
                        select p).FirstOrDefault<Part>();
                db.Parts.DeleteOnSubmit(part);

                // Get the part attributes
                List<PartAttribute> attributes = (from pa in db.PartAttributes
                                                  where pa.partID.Equals(partID)
                                                  select pa).ToList<PartAttribute>();
                db.PartAttributes.DeleteAllOnSubmit<PartAttribute>(attributes);

                // Get the related part references
                List<RelatedPart> related = (from rp in db.RelatedParts
                                             where rp.partID.Equals(partID)
                                             select rp).ToList<RelatedPart>();
                db.RelatedParts.DeleteAllOnSubmit<RelatedPart>(related);

                // Get the reviews
                List<Review> reviews = (from r in db.Reviews
                                            where r.partID.Equals(partID)
                                            select r).ToList<Review>();
                db.Reviews.DeleteAllOnSubmit<Review>(reviews);

                // Get the references this vehicle has to parts.
                List<VehiclePart> vehicles = (from vp in db.VehicleParts
                                              where vp.partID.Equals(partID)
                                              select vp).ToList<VehiclePart>();

                foreach (VehiclePart vp in vehicles) {
                    List<VehiclePartAttribute> vpartAttr = (from vpa in db.VehiclePartAttributes
                                                      where vpa.vPartID == vp.vPartID
                                                      select vpa).ToList<VehiclePartAttribute>();
                    db.VehiclePartAttributes.DeleteAllOnSubmit(vpartAttr);
                }

                db.VehicleParts.DeleteAllOnSubmit<VehiclePart>(vehicles);

                // Get the price references
                List<Price> part_prices = (from prices in db.Prices
                                           where prices.partID.Equals(partID)
                                           select prices).ToList<Price>();
                db.Prices.DeleteAllOnSubmit<Price>(part_prices);

                // Get the contents
                List<ContentBridge> bridges = db.ContentBridges.Where(x => x.partID.Equals(partID)).ToList<ContentBridge>();
                List<int> contentids = bridges.Select(x => x.contentID).ToList();
                List<Content> part_contents = db.Contents.Where(x => contentids.Contains(x.contentID)).ToList<Content>();
                db.ContentBridges.DeleteAllOnSubmit<ContentBridge>(bridges);
                db.Contents.DeleteAllOnSubmit<Content>(part_contents);

                // Get the part images
                List<PartImage> part_images = (from images in db.PartImages
                                               where images.partID.Equals(partID)
                                               select images).ToList<PartImage>();
                db.PartImages.DeleteAllOnSubmit<PartImage>(part_images);

                try {
                    // Get the part videos
                    List<PartVideo> part_videos = (from video in db.PartVideos
                                                   where video.partID.Equals(partID)
                                                   select video).ToList<PartVideo>();
                    db.PartVideos.DeleteAllOnSubmit<PartVideo>(part_videos);
                } catch { };

                // Get the CatPart references
                List<CatParts> cat_parts = (from cp in db.CatParts
                                            where cp.partID.Equals(partID)
                                            select cp).ToList<CatParts>();
                db.CatParts.DeleteAllOnSubmit<CatParts>(cat_parts);
                
                // Delete the Index of the part from the search index
                PartIndex partindx = db.PartIndexes.Where(x => x.partID == partID).FirstOrDefault<PartIndex>();
                db.PartIndexes.DeleteOnSubmit(partindx);
                db.SubmitChanges();

            } catch (Exception e) {
                return e.Message;
            }

            return "";
        }

        public static string AddRelated(int partID = 0, int relatedID = 0) {
            try {
                if (partID > 0 && relatedID > 0) {
                    CurtDevDataContext db = new CurtDevDataContext();
                    RelatedPart related_part = new RelatedPart {
                        partID = partID,
                        relatedID = relatedID
                    };
                    db.RelatedParts.InsertOnSubmit(related_part);
                    db.SubmitChanges();

                    // get the related parts information
                    ConvertedPart part = new ConvertedPart();
                    part = GetPart(relatedID);
                    UpdatePart(partID);
                    // Serialize and return
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    return js.Serialize(part);
                } else {
                    return "{\"error\":\"Invalid data.\"}";
                }
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        public static string DeleteRelated(int partID = 0, int relatedID = 0) {
            try {
                if (partID > 0 && relatedID > 0) {
                    CurtDevDataContext db = new CurtDevDataContext();
                    RelatedPart rp = (from r in db.RelatedParts
                                      where r.relatedID.Equals(relatedID) && r.partID.Equals(partID)
                                      select r).FirstOrDefault<RelatedPart>();
                    db.RelatedParts.DeleteOnSubmit(rp);
                    db.SubmitChanges();
                    UpdatePart(partID);
                    // Get the parts information
                    ConvertedPart part = GetPart(relatedID);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    return js.Serialize(part);
                } else {
                    return "{\"error\":\"Invalid data.\"}";
                }
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        public static List<FullVehicle> GetVehicles(int partID = 0) {
            List<FullVehicle> vehicles = new List<FullVehicle>();
            CurtDevDataContext db = new CurtDevDataContext();
            vehicles = (from v in db.Vehicles
                        join y in db.Years on v.yearID equals y.yearID
                        join ma in db.Makes on v.makeID equals ma.makeID
                        join mo in db.Models on v.modelID equals mo.modelID
                        join s in db.Styles on v.styleID equals s.styleID
                        join vp in db.VehicleParts on v.vehicleID equals vp.vehicleID
                        where vp.partID.Equals(partID)
                        select new FullVehicle {
                            vehicleID = v.vehicleID,
                            yearID = v.yearID,
                            makeID = v.makeID,
                            modelID = v.modelID,
                            styleID = v.styleID,
                            aaiaID = s.aaiaID,
                            year = y.year1,
                            make = ma.make1,
                            model = mo.model1,
                            style = s.style1
                        }).Distinct().OrderBy(x => x.makeID).ThenBy(x => x.modelID).ThenBy(x => x.styleID).ThenBy(x => x.yearID).ToList<FullVehicle>();

            return vehicles;
        }

        public static string AddVehicle(int partID = 0, int vehicleID = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                if (partID > 0 && vehicleID > 0) {
                    List<int> options = getAllCarryOverParts(partID);
                    foreach (int partnum in options) {
                        // Check to make sure a record doesn't already exist for this part and vehicle
                        int existing = (from vp in db.VehicleParts
                                        where vp.partID.Equals(partnum) && vp.vehicleID.Equals(vehicleID)
                                        select vp).Count();
                        if (existing == 0) {
                            VehiclePart vp = new VehiclePart {
                                vehicleID = vehicleID,
                                partID = partnum,
                                exposed = "",
                                drilling = "",
                                installTime = 0
                            };
                            db.VehicleParts.InsertOnSubmit(vp);
                            db.SubmitChanges();
                            db.indexPart(partnum);
                        }
                    }
                    UpdatePart(partID);
                } else {
                    throw new Exception("Invalid partID or vehicleID");
                }
                return "";
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        public static VehiclePart UpdateVehicle(int partID = 0, int vehicleID = 0, string drilling = "", string exposed = "", int installTime = 0) {
            // Variable initializers
            CurtDevDataContext db = new CurtDevDataContext();
            VehiclePart vp = new VehiclePart();

            // Get the VehiclePart that we want to work with
            vp = (from v in db.VehicleParts
                  where v.vehicleID.Equals(vehicleID) && v.partID.Equals(partID)
                  select v).FirstOrDefault<VehiclePart>();
            if (vp == null) { throw new Exception("VehiclePart not found."); }

            // Update fields and commit to database
            vp.drilling = drilling;
            vp.exposed = exposed;
            vp.installTime = installTime;
            db.SubmitChanges();
            db.indexPart(partID);

            return vp;
        }

        public static string DeleteVehiclePart(int vehicleID = 0, int partID = 0) {
            try {
                if (vehicleID == 0 || partID == 0) {
                    throw new Exception("Invalid data.");
                }

                CurtDevDataContext db = new CurtDevDataContext();
                VehiclePart vp = new VehiclePart();
                List<VehiclePartAttribute> attributes = new List<VehiclePartAttribute>();
                vp = (from v in db.VehicleParts
                      where v.vehicleID.Equals(vehicleID) && v.partID.Equals(partID)
                      select v).FirstOrDefault<VehiclePart>();
                attributes = (from vpa in db.VehiclePartAttributes
                                join v in db.VehicleParts on vpa.vPartID equals v.vPartID
                                where v.vehicleID == vehicleID && v.partID == partID
                                select vpa).ToList<VehiclePartAttribute>();
                db.VehiclePartAttributes.DeleteAllOnSubmit(attributes);
                db.SubmitChanges();    
                db.VehicleParts.DeleteOnSubmit(vp);
                db.SubmitChanges();
                db.indexPart(partID);
                UpdatePart(partID);
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }

        public static FullVehiclePart GetVehiclePart(int vehicleID = 0, int partID = 0) {
            try {
                FullVehiclePart vp = new FullVehiclePart();
                CurtDevDataContext db = new CurtDevDataContext();
                vp = (from v in db.VehicleParts
                      where v.vehicleID.Equals(vehicleID) && v.partID.Equals(partID)
                      select new FullVehiclePart {
                          vPartID = v.vPartID,
                          vehicleID = v.vehicleID,
                          partID = v.partID,
                          drilling = v.drilling,
                          exposed = v.exposed,
                          installTime = v.installTime,
                          attributes = db.VehiclePartAttributes.Where(x => x.vPartID.Equals(v.vPartID)).OrderBy(x => x.sort).ToList<VehiclePartAttribute>()
                      }).FirstOrDefault<FullVehiclePart>();
                return vp;
            } catch (Exception e) {
                throw new Exception(e.Message);
            }
        }

        public static CarryOverInfo GetLatestVehiclePart(int vehicleID = 0, int partID = 0) {
            try {
                CarryOverInfo application = new CarryOverInfo();
                CurtDevDataContext db = new CurtDevDataContext();
                FullVehicle vehicle = Vehicle.GetFullVehicle(vehicleID);
                application = (from vp in db.VehicleParts
                      join v in db.Vehicles on vp.vehicleID equals v.vehicleID
                      join y in db.Years on v.yearID equals y.yearID
                      join m in db.Makes on v.makeID equals m.makeID
                      join mo in db.Models on v.modelID equals mo.modelID
                      join s in db.Styles on v.styleID equals s.styleID
                      where vp.partID.Equals(partID) && s.styleID.Equals(vehicle.styleID) && m.makeID.Equals(vehicle.makeID) && mo.modelID.Equals(vehicle.modelID)
                      orderby y.year1 descending
                      select new CarryOverInfo {
                          year = y.year1,
                          yearID = y.yearID,
                          make = m.make1,
                          makeID = m.makeID,
                          model = mo.model1,
                          modelID = mo.modelID,
                          style = s.style1,
                          styleID = s.styleID,
                          vehicleID = v.vehicleID,
                          partids = getAllCarryOverParts(partID)
                      }).First<CarryOverInfo>();
                return application;
            } catch (Exception e) {
                throw new Exception(e.Message);
            }
        }

        public static List<int> getAllCarryOverParts(int partID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<int> partids = new List<int>();
            try {
                if (partID.ToString().Length > 5) {
                    partID = Convert.ToInt32(partID.ToString().Substring(0, 5));
                }
                partids = db.Parts.Where(x => x.partID.ToString().StartsWith(partID.ToString())).Select(x => x.partID).ToList();
            } catch {partids.Add(partID);};

            return partids;
        }

        public static FullVehicle CarryOverPart(int vehicleID = 0, int partID = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                CarryOverInfo latest = GetLatestVehiclePart(vehicleID, partID);
                VehiclePart latestvp = db.VehicleParts.Where(x => x.vehicleID == vehicleID).Where(x => x.partID == partID).First<VehiclePart>();
                List<VehiclePartAttribute> vpas = db.VehiclePartAttributes.Where(x => x.vPartID == latestvp.vPartID).ToList<VehiclePartAttribute>();
                List<VehiclePartAttribute> newvpas = new List<VehiclePartAttribute>();
                double year = latest.year + 1;
                int yearID = 0;

                try {
                    yearID = db.Years.Where(x => x.year1 == year).Select(x => x.yearID).First();
                } catch {
                    Year y = new Year {
                        year1 = year
                    };
                    db.Years.InsertOnSubmit(y);
                    db.SubmitChanges();
                    yearID = y.yearID;
                };

                try {
                    YearMake ym = db.YearMakes.Where(x => x.yearID.Equals(yearID)).Where(x => x.makeID.Equals(latest.makeID)).First<YearMake>();
                } catch {
                    YearMake ym = new YearMake {
                        yearID = yearID,
                        makeID = latest.makeID
                    };
                    db.YearMakes.InsertOnSubmit(ym);
                    db.SubmitChanges();
                }

                int vID = Vehicle.GetVehicleID(yearID, latest.makeID, latest.modelID, latest.styleID);

                if (vID == 0) {
                    Vehicles v = new Vehicles {
                        yearID = yearID,
                        makeID = latest.makeID,
                        modelID = latest.modelID,
                        styleID = latest.styleID,
                        dateAdded = DateTime.Now
                    };
                    db.Vehicles.InsertOnSubmit(v);
                    db.SubmitChanges();
                    vID = v.vehicleID;
                }
                List<VehiclePart> parts = new List<VehiclePart>();

                foreach (int partnum in latest.partids) {
                    if (db.VehicleParts.Where(x => x.partID.Equals(partnum)).Where(x => x.vehicleID.Equals(vID)).Count() == 0) {
                        VehiclePart vp = new VehiclePart {
                            vehicleID = vID,
                            partID = partnum,
                            drilling = latestvp.drilling,
                            exposed = latestvp.exposed,
                            installTime = latestvp.installTime
                        };
                        parts.Add(vp);
                    }
                }

                db.VehicleParts.InsertAllOnSubmit(parts);
                db.SubmitChanges();

                foreach (VehiclePart part in parts) {
                    newvpas = new List<VehiclePartAttribute>();
                    foreach (VehiclePartAttribute vpa in vpas) {
                        VehiclePartAttribute newvpa = new VehiclePartAttribute {
                            vPartID = part.vPartID,
                            field = vpa.field,
                            value = vpa.value
                        };
                        newvpas.Add(newvpa);
                    };
                    db.VehiclePartAttributes.InsertAllOnSubmit(newvpas);
                    db.SubmitChanges();
                };

                UpdatePart(partID);
                return Vehicle.GetFullVehicle(vID);

            } catch (Exception e) {
                throw new Exception(e.Message);
            }
        }
        
        public static List<FullContent> GetPartContent(int partID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<FullContent> content = new List<FullContent>();
            content = (from cb in db.ContentBridges
                       join c in db.Contents on cb.contentID equals c.contentID
                       join ct in db.ContentTypes on c.cTypeID equals ct.cTypeID
                       where cb.partID.Equals(partID)
                       select new FullContent{
                            contentID = c.contentID,
                            content_type = ct.type,
                            content = c.text,
                            content_type_id = c.cTypeID
                       }).Distinct().OrderBy(x => x.content_type).ToList<FullContent>();
            return content;
        }

        public static FullContent GetFullContent(int contentID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            FullContent content = new FullContent();
            content = (from c in db.Contents
                       where c.contentID.Equals(contentID)
                       select new FullContent {
                           contentID = c.contentID,
                           content_type = c.ContentType.type,
                           content = c.text,
                           content_type_id = c.cTypeID
                       }).FirstOrDefault<FullContent>();
            return content;
        }

        public static FullContent SaveContent(int contentID = 0, int partID = 0, string content = "", int contentType = 0) {
            if (partID <= 0) { throw new Exception("Part ID invalid."); }
            if (content.Length == 0) { throw new Exception("Content is blank."); }
            if (contentType <= 0) { throw new Exception("Content type not defined."); }
            CurtDevDataContext db = new CurtDevDataContext();
            Content c = new Content();

            if (contentID == 0) {
                Content existing = (from co in db.Contents
                                    join cob in db.ContentBridges on co.contentID equals cob.contentID
                                    where cob.partID.Equals(partID) && co.ContentType.cTypeID.Equals(contentType) && co.text.Equals(content.Trim())
                                    select co).FirstOrDefault<Content>();
                if (existing != null && existing.contentID > 0) {
                    throw new Exception("This content already exists.");
                } else {
                    c = new Content {
                        text = content,
                        cTypeID = contentType,
                    };
                    db.Contents.InsertOnSubmit(c);
                }
                db.SubmitChanges();

                ContentBridge cb = new ContentBridge {
                    partID = partID,
                    contentID = c.contentID
                };
                db.ContentBridges.InsertOnSubmit(cb);
            } else {
                c = (from contents in db.Contents
                     where contents.contentID.Equals(contentID)
                     select contents).FirstOrDefault<Content>();
                c.text = content;
                c.cTypeID = contentType;
            }
            db.SubmitChanges();
            UpdatePart(partID);
            return GetFullContent(c.contentID);
        }

        public static string DeleteContent(int partID = 0, int contentID = 0) {
            if (contentID <= 0) { throw new Exception("Content ID invalid."); }
            CurtDevDataContext db = new CurtDevDataContext();
            ContentBridge cb = db.ContentBridges.Where(x => x.partID.Equals(partID)).Where(x => x.contentID.Equals(contentID)).FirstOrDefault();
            db.ContentBridges.DeleteOnSubmit(cb);
            db.SubmitChanges();
            if (db.ContentBridges.Where(x => x.contentID.Equals(contentID)).Count() == 0) {
                Content c = db.Contents.Where(x => x.contentID.Equals(contentID)).FirstOrDefault(); ;
                db.Contents.DeleteOnSubmit(c);
                db.SubmitChanges();
            }
            UpdatePart(partID);
            return "";
        }

        // Start Part Attribute Methods
        internal static List<string> GetDistinctAttributeFields() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<string> fields = new List<string>();
            fields = (from a in db.PartAttributes
                      select a.field).Distinct().OrderBy(x => x).ToList<string>();
            return fields;
        }

        internal static List<PartAttribute> GetAttributes(int partID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<PartAttribute> attributes = new List<PartAttribute>();
            attributes = (from a in db.PartAttributes
                          where a.partID.Equals(partID)
                          orderby a.sort
                          select a).ToList<PartAttribute>();
            return attributes;
        }

        internal static string GetAttribute(int partID = 0, string key = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            string attribute = "";
            attribute = (from a in db.PartAttributes
                          where a.partID.Equals(partID) && a.field.ToLower().Equals(key.Trim().ToLower())
                          select a.value).FirstOrDefault();
            return attribute;
        }

        internal static void UpdateAttributeByField(int partID = 0, string key = "", string value = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            PartAttribute attribute = (from a in db.PartAttributes
                                         where a.partID.Equals(partID) && a.field.ToLower().Equals(key.Trim().ToLower())
                                         select a).FirstOrDefault <PartAttribute>();
            attribute.value = value;
            db.SubmitChanges();
            db.indexPart(partID);
            UpdatePart(partID);
        }

        internal static bool HasAttribute(int partID = 0, string key = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                PartAttribute attribute = (from a in db.PartAttributes
                                           where a.partID.Equals(partID) && a.field.ToLower().Equals(key.Trim().ToLower())
                                           select a).First<PartAttribute>();
                return true;
            } catch {
                return false;
            }
        }

        internal static List<string> getAttributeIDs(int partID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<string> attrs = db.PartAttributes.Where(x => x.partID.Equals(partID)).OrderBy(x => x.sort).Select(x => x.pAttrID.ToString()).ToList();
            return attrs;
        }

        public static void UpdateAttributeSort(List<string> attributes) {
            CurtDevDataContext db = new CurtDevDataContext();
            for (int i = 0; i < attributes.Count; i++) {
                PartAttribute a = db.PartAttributes.Where(x => x.pAttrID.Equals(Convert.ToInt32(attributes[i]))).First();
                UpdatePart(a.partID);
                a.sort = i + 1;
                db.SubmitChanges();
            }
        }

        internal static List<string> getVehiclePartAttributeIDs(int vPartID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<string> attrs = db.VehiclePartAttributes.Where(x => x.vPartID.Equals(vPartID)).OrderBy(x => x.sort).Select(x => x.vpAttrID.ToString()).ToList();
            return attrs;
        }

        public static void updateVehicleAttributeSort(List<string> attributes) {
            CurtDevDataContext db = new CurtDevDataContext();
            for (int i = 0; i < attributes.Count; i++) {
                VehiclePartAttribute vpa = db.VehiclePartAttributes.Where(x => x.vpAttrID.Equals(Convert.ToInt32(attributes[i]))).First();
                vpa.sort = i + 1;
                db.SubmitChanges();
                UpdatePart(vpa.VehiclePart.partID);
            }
        }

        /// <summary>
        /// Checks to see if a UPC code already exists
        /// </summary>
        /// <param name="value">UPC code</param>
        /// <returns>Boolean</returns>
        private static bool ValidateUPC(string value = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            string upc = value;

            int exists = (from pa in db.PartAttributes
                          where pa.value.Equals(value) && pa.field.ToUpper().Equals("UPC")
                          select pa).Count();
            if (exists == 0) {
                return true;
            }
            return false;
        }

        public static PartAttribute SaveAttribute(int attrID = 0, int partID = 0, string field = "", string value = "") {
            // Validate fields
            if (partID <= 0) { throw new Exception("Part ID is invalid."); }
            if (field.Length <= 0) { throw new Exception("Field is invalid."); }
            if (value.Length <= 0) { throw new Exception("Value is invalid."); }
            if (field.ToUpper() == "UPC") { // Validate UPC
                if (!ValidateUPC(value)) {
                    throw new Exception("UPC exists.");
                }
            }

            // Declare objects
            CurtDevDataContext db = new CurtDevDataContext();
            PartAttribute pa = new PartAttribute();

            if (attrID == 0) {
                // Add New Attribute
                bool exists = db.PartAttributes.Where(x => x.partID == partID).Where(x => x.field == field).Count() > 0;
                if (exists) {
                    throw new Exception("This attribute already exists!");
                }

                pa = new PartAttribute {
                    partID = partID,
                    field = field.Trim(),
                    value = value.Trim(),
                    sort = (db.PartAttributes.Where(x => x.partID.Equals(partID)).OrderByDescending(x => x.sort).Select(x => x.sort).FirstOrDefault() + 1)
                };

                // Commit to databse
                db.PartAttributes.InsertOnSubmit(pa);
            } else {
                // Update Existing Attribute
                // Retrive PartAttribute
                pa = (from p in db.PartAttributes
                      where p.pAttrID.Equals(attrID)
                      select p).FirstOrDefault<PartAttribute>();

                // Update values
                pa.partID = partID;
                pa.field = field.Trim();
                pa.value = value.Trim();
            }
            // Commit to databse
            db.SubmitChanges();
            UpdateAttributeSort(getAttributeIDs(partID));
            db.indexPart(partID);
            UpdatePart(partID);
            // Return PartAttribute
            return pa;
        }

        public static void DeleteAttribute(int attrID = 0) {
            // Validate
            if (attrID <= 0) { throw new Exception("Attribute ID is invalid."); }

            // Variable Declaration
            CurtDevDataContext db = new CurtDevDataContext();
            PartAttribute pa = new PartAttribute();

            pa = (from p in db.PartAttributes
                  where p.pAttrID.Equals(attrID)
                  select p).FirstOrDefault<PartAttribute>();
            int pid = pa.partID;
            db.PartAttributes.DeleteOnSubmit(pa);
            db.SubmitChanges();
            UpdateAttributeSort(getAttributeIDs(pid));
            db.indexPart(pid);
            UpdatePart(pid);
        }

        // End Part Attribute Methods

        // Start Vehicle Part Attribute Methods
        internal static List<string> GetDistinctVehiclePartAttributeFields() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<string> fields = new List<string>();
            fields = (from a in db.VehiclePartAttributes
                      select a.field).Distinct().OrderBy(x => x).ToList<string>();
            return fields;
        }

        internal static List<VehiclePartAttribute> GetVehiclePartAttributes(int vPartID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<VehiclePartAttribute> attributes = new List<VehiclePartAttribute>();
            attributes = (from a in db.VehiclePartAttributes
                          where a.vPartID.Equals(vPartID)
                          select a).OrderBy(x => x.field).ToList<VehiclePartAttribute>();
            return attributes;
        }

        internal static VehiclePartAttribute GetVehiclePartAttribute(int vpAttrID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            VehiclePartAttribute attribute = new VehiclePartAttribute();
            attribute = (from a in db.VehiclePartAttributes
                         where a.vpAttrID.Equals(vpAttrID)
                          select a).FirstOrDefault<VehiclePartAttribute>();
            return attribute;
        }

        internal static VehiclePartAttribute UpdateVehiclePartAttribute(int vpAttrID = 0, string key = "", string value = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            VehiclePart v = (from vp in db.VehicleParts
                             join vpa in db.VehiclePartAttributes on vp.vPartID equals vpa.vPartID
                             where vpa.vpAttrID == vpAttrID
                             select vp).FirstOrDefault<VehiclePart>();

            VehiclePartAttribute attribute = (from a in db.VehiclePartAttributes
                                         where a.vpAttrID.Equals(vpAttrID)
                                       select a).FirstOrDefault<VehiclePartAttribute>();
            attribute.field = key;
            attribute.value = value;
            if (key.ToLower().Contains("install")) {
                Match match = Regex.Match(value, @"\d+");
                if (match.Success) {
                    string result = match.Groups[0].Value;
                    int duration = Convert.ToInt32(result);
                    v.installTime = duration;
                }
            }

            if (key.ToLower().Contains("visibility")) {
                v.exposed = value;
            };

            if (key.ToLower().Contains("drill")) {
                if (value.ToLower() == "no") {
                    v.drilling = "No drilling required";
                } else {
                    v.drilling = "Drilling required";
                }
            };
            UpdatePart(v.partID);
            db.SubmitChanges();
            return attribute;
            //db.indexPart(partID);
        }

        internal static bool HasVehiclePartAttribute(int vPartID = 0, string key = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                VehiclePartAttribute attribute = (from a in db.VehiclePartAttributes
                                                  where a.vPartID.Equals(vPartID) && a.field.ToLower().Equals(key.Trim().ToLower())
                                           select a).First<VehiclePartAttribute>();
                return true;
            } catch {
                return false;
            }
        }

        public static VehiclePartAttribute AddVehiclePartAttribute(int vPartID = 0, string field = "", string value = "") {
            // Validate fields
            if (vPartID <= 0) { throw new Exception("Vehicle Part ID is invalid."); }
            if (field.Length <= 0) { throw new Exception("Field is invalid."); }
            if (value.Length <= 0) { throw new Exception("Value is invalid."); }

            // Declare objects
            CurtDevDataContext db = new CurtDevDataContext();
            VehiclePartAttribute vpa = new VehiclePartAttribute {
                vPartID = vPartID,
                field = field,
                value = value,
                sort = (db.VehiclePartAttributes.Where(x => x.vPartID.Equals(vPartID)).OrderByDescending(x => x.sort).Select(x => x.sort).FirstOrDefault() + 1)
            };

            VehiclePart v = (from vp in db.VehicleParts
                             where vp.vPartID == vPartID
                             select vp).FirstOrDefault<VehiclePart>();

            if (field.ToLower().Contains("install")) {
                Match match = Regex.Match(value, @"\d+");
                if (match.Success) {
                    string result = match.Groups[0].Value;
                    int duration = Convert.ToInt32(result);
                    v.installTime = duration;
                }
            }

            if (field.ToLower().Contains("visibility")) {
                v.exposed = value;
            };

            if (field.ToLower().Contains("drill")) {
                if (value.ToLower() == "no") {
                    v.drilling = "No drilling required";
                } else {
                    v.drilling = "Drilling required";
                }
            };

            // Commit to database
            db.VehiclePartAttributes.InsertOnSubmit(vpa);
            db.SubmitChanges();
            updateVehicleAttributeSort(getVehiclePartAttributeIDs(vPartID));
            UpdatePart(vpa.VehiclePart.partID);
            //db.indexPart(partID);
            // Return PartAttribute
            return vpa;
        }

        public static string DeleteVehiclePartAttribute(int attrID = 0) {
            try {
                // Validate
                if (attrID <= 0) { throw new Exception("Attribute ID is invalid."); }

                // Variable Declaration
                CurtDevDataContext db = new CurtDevDataContext();
                VehiclePartAttribute vpa = new VehiclePartAttribute();

                vpa = (from p in db.VehiclePartAttributes
                      where p.vpAttrID.Equals(attrID)
                      select p).FirstOrDefault<VehiclePartAttribute>();
                int pid = vpa.vPartID;
                db.VehiclePartAttributes.DeleteOnSubmit(vpa);
                db.SubmitChanges();
                updateVehicleAttributeSort(getVehiclePartAttributeIDs(pid));
                db.indexPart(vpa.VehiclePart.partID);
                UpdatePart(vpa.VehiclePart.partID);
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }

        // End Vehicle Part Attribute Methods
        
        public static List<Price> GetPrices(int partID) {
            List<Price> prices = new List<Price>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                prices = (from pr in db.Prices
                          where pr.partID.Equals(partID)
                          select pr).ToList<Price>();
            } catch (Exception e) { }
            return prices;
        }



        public static Price SavePrice(int priceID, decimal price, string price_type, int partID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            Price price_obj = new Price();

            // Validate the price information
            if (price == 0) throw new Exception("Price wasn't valid.");
            if (price_type.Length == 0) throw new Exception("Price type was not detected.");

            if (priceID == 0) {
                Price existing_price = db.Prices.Where(x => x.partID.Equals(partID) && x.priceType.ToLower().Equals(price_type.Trim().ToLower())).FirstOrDefault<Price>();
                if (existing_price != null && existing_price.priceID > 0) {
                    existing_price.price1 = price;
                    db.SubmitChanges();
                    price_obj = existing_price;
                } else {
                    // Create new price object
                    price_obj = new Price {
                        partID = partID,
                        price1 = price,
                        priceType = price_type
                    };

                    // Validate the price object
                    if (price_obj.partID > 0 && price_obj.price1 > 0 && price_obj.priceType.Length > 0) {
                        db.Prices.InsertOnSubmit(price_obj);
                        db.SubmitChanges();
                    } else { // Throw exceptions
                        if (price_obj.priceType.Length == 0) throw new Exception("Price type is invalid.");
                        if (price_obj.price1 == 0) throw new Exception("Invalid price.");
                        if (price_obj.partID == 0) throw new Exception("Invalid Part ID.");
                    }
                }
            } else {
                // Get the price record that we will update
                price_obj = (from p in db.Prices
                                 where p.priceID.Equals(priceID)
                                 select p).FirstOrDefault<Price>();

                // Make sure we found the price
                if (price_obj.partID > 0) {

                    // Update the price
                    price_obj.price1 = price;
                    price_obj.priceType = price_type;
                    db.SubmitChanges();
                } else {
                    throw new Exception("Failed to update price.");
                }
            }
            Price ser_price = new Price {
                partID = price_obj.partID,
                price1 = price_obj.price1,
                priceID = price_obj.priceID,
                priceType = price_obj.priceType
            };
            UpdatePart(partID);
            return ser_price;
        }

        public static bool DeletePrice(int priceID) {
            CurtDevDataContext db = new CurtDevDataContext();
            Price price = new Price();

            price = (from p in db.Prices
                     where p.priceID.Equals(priceID)
                     select p).FirstOrDefault<Price>();
            UpdatePart(price.partID);
            if (price.partID > 0) {
                db.Prices.DeleteOnSubmit(price);
                db.SubmitChanges();
                return true;
            } else {
                return false;
            }
        }

        public static List<PartPackage> GetPackages(int partID) {
            List<PartPackage> packages = new List<PartPackage>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                packages = (from pp in db.PartPackages
                            where pp.partID.Equals(partID)
                            select pp).ToList<PartPackage>();
            } catch (Exception e) { }
            return packages;
        }

        public static PartPackage GetPackage(int packageID) {
            PartPackage package = new PartPackage();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                package = (from pp in db.PartPackages
                           where pp.ID.Equals(packageID)
                           select pp).FirstOrDefault<PartPackage>();
            } catch (Exception e) { }
            return package;
        }

        public static PartPackage SavePackage(int packageID = 0, int partID = 0, double weight = 0, double height = 0, double width = 0, double length = 0, int qty = 1, int weightUnit = 0, int dimensionUnit = 0, int qtyUnit = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            PartPackage package = new PartPackage();

            // Validate the price information
            if (partID == 0) throw new Exception("Part ID is invalid.");
            if (weight == 0) throw new Exception("Weight is required.");
            if (qty <= 0) throw new Exception("Quantity is required.");
            if (weightUnit == 0) throw new Exception("Weight Unit is required.");
            if (dimensionUnit == 0) throw new Exception("Dimensional Unit is required.");
            if (qtyUnit == 0) throw new Exception("Package Unit is required.");

            if (packageID == 0) {
                // Create new price object
                package = new PartPackage {
                    partID = partID,
                    weight = weight,
                    width = (width > 0) ? width : (double?)null,
                    height = (height > 0) ? height : (double?)null,
                    length = (length > 0) ? length : (double?)null,
                    quantity = qty,
                    weightUOM = weightUnit,
                    dimensionUOM = dimensionUnit,
                    packageUOM = qtyUnit
                };

                db.PartPackages.InsertOnSubmit(package);
                db.SubmitChanges();
            } else {
                // Get the price record that we will update
                package = (from p in db.PartPackages
                           where p.ID.Equals(packageID)
                           select p).FirstOrDefault<PartPackage>();

                // Make sure we found the price
                if (package.partID > 0) {
                    package.weight = weight;
                    package.width = (width > 0) ? width : (double?)null;
                    package.height = (height > 0) ? height : (double?)null;
                    package.length = (length > 0) ? length : (double?)null;
                    package.quantity = qty;
                    package.weightUOM = weightUnit;
                    package.dimensionUOM = dimensionUnit;
                    package.packageUOM = qtyUnit;
                    db.SubmitChanges();
                } else {
                    throw new Exception("Failed to update package.");
                }
            }
            UpdatePart(partID);
            return package;
        }

        public static string DeletePackage(int packageID) {
            CurtDevDataContext db = new CurtDevDataContext();
            PartPackage package = new PartPackage();
            try {
                package = (from p in db.PartPackages
                           where p.ID.Equals(packageID)
                           select p).First<PartPackage>();
                UpdatePart(package.partID);
                db.PartPackages.DeleteOnSubmit(package);
                db.SubmitChanges();
            } catch { }
            return "";
        }

        public static void UpdatePart(int partID) {
            CurtDevDataContext db = new CurtDevDataContext();
            Part part = db.Parts.Where(x => x.partID.Equals(partID)).First();
            part.dateModified = DateTime.Now;
            db.SubmitChanges();
        }
    }

    public class FullVehiclePart : VehiclePart {
        public List<VehiclePartAttribute> attributes { get; set; }
    }
}