using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;

namespace CurtAdmin.Controllers
{
    public class ProductController : BaseController
    {
        //
        // GET: /Product/

        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "Products";
        }

        public ActionResult Index(){
            // Get all the categories
            List<DetailedCategories> cats = ProductCat.GetCategories();
            ViewBag.cats = cats;

            // Get the number of unkown parts
            int unknown = ProductCat.GetUncategorizedParts().Count;
            ViewBag.unknown = unknown;

            return View();
        }

        public ActionResult Add() {

            // Get the product classes
            ViewBag.classes = ProductModels.GetClasses();

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Add(int partID = 0, string shortDesc = "", int status = 0, string oldPartNumber = "", int priceCode = 0, int classID = 0, string btnSubmit = "", string btnContinue = "", string upc = "", bool featured = false, int? ACESPartTypeID = null) {

            CurtDevDataContext db = new CurtDevDataContext();
            List<string> error_messages = new List<string>();
            Part new_part = new Part{
                    partID = partID,
                    shortDesc = shortDesc,
                    status = status,
                    oldPartNumber = oldPartNumber,
                    priceCode = priceCode,
                    classID = classID,
                    dateAdded = DateTime.Now,
                    dateModified = DateTime.Now,
                    featured = featured,
                    ACESPartTypeID = ACESPartTypeID
            };



            // Validate the partID and shortDesc fields
            if(partID == 0){ error_messages.Add("You must enter a part number."); }
            if(shortDesc.Length == 0){ error_messages.Add("You must enter a short description."); }
            if (upc.Trim().Length == 0) { error_messages.Add("You must enter a UPC."); }


            int existing_part = 0;
            // Make sure we don't have a product with this partID
            existing_part = (from p in db.Parts
                             where p.partID.Equals(partID)
                             select p).Count();
            if (existing_part != 0) { error_messages.Add("This part number exists."); }

            if (error_messages.Count == 0) { // No errors, add the part
                try {

                    db.Parts.InsertOnSubmit(new_part);
                    db.SubmitChanges();

                    ProductModels.SaveAttribute(0,new_part.partID, "UPC", upc);

                    db.indexPart(new_part.partID);

                    if (btnContinue != "") { // Redirect to add more part information
                        return RedirectToAction("edit", new { partID = new_part.partID }); 
                    } else { // Redirect to product index page
                        return RedirectToAction("index");
                    }
                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }
                
            ViewBag.error_messages = error_messages;

            // Get all the parts in the database :: This will allow the user to create related parts
            ViewBag.parts = ProductModels.GetAllParts();

            // Get the product classes
            ViewBag.classes = ProductModels.GetClasses();

            return View();
        }

        public string Clone(int partID = 0, int newPartID = 0, string upc = "", bool categories = false, bool relatedParts = false, bool attributes = false, bool content = false, bool vehicles = false, bool prices = false) {

            CurtDevDataContext db = new CurtDevDataContext();
            List<string> messages = new List<string>();
            // Validate the partID and shortDesc fields
            if (partID == 0) { messages.Add("You must enter a part number."); }
            if (newPartID == 0) { messages.Add("You must enter a part number."); }
            if (upc.Trim().Length == 0) { messages.Add("You must enter a UPC."); }

            int existing_part = 0;
            // Make sure we don't have a product with this partID
            existing_part = (from p in db.Parts
                             where p.partID.Equals(newPartID)
                             select p).Count();
            if (existing_part != 0) { messages.Add("This part number exists."); }

            #region clone part
            if (messages.Count == 0) { // No errors, add the part
                try {
                    ConvertedPart cp = ProductModels.GetPart(partID);
                    Part new_part = new Part {
                        partID = newPartID,
                        classID = cp.pClass,
                        dateAdded = DateTime.Now,
                        dateModified = DateTime.Now,
                        featured = cp.featured,
                        oldPartNumber = cp.oldPartNumber,
                        priceCode = cp.priceCode,
                        shortDesc = cp.shortDesc,
                        status = cp.status,
                        ACESPartTypeID = cp.ACESPartTypeID
                    };
                    db.Parts.InsertOnSubmit(new_part);
                    db.SubmitChanges();
                    messages.Add("Part Added Successfully");

                    try {
                        ProductModels.SaveAttribute(0,new_part.partID, "UPC", upc);
                        messages.Add("UPC Added Successfully");
                    } catch (Exception e) {
                        messages.Add(e.Message);
                    }

                    #region clone categories
                    if (categories) {
                        try {
                            List<CatParts> new_catparts = new List<CatParts>();
                            List<int> catparts = db.CatParts.Where(x => x.partID == partID).Select(x => x.catID).Distinct().ToList();
                            foreach (int catpart in catparts) {
                                CatParts ncp = new CatParts {
                                    catID = catpart,
                                    partID = new_part.partID
                                };
                                new_catparts.Add(ncp);
                            }
                            db.CatParts.InsertAllOnSubmit(new_catparts);
                            db.SubmitChanges();
                            messages.Add("Categories Cloned Successfully");
                        } catch {
                            messages.Add("There was a problem cloning the categories.");
                        }
                    }
                    #endregion

                    #region clone Related Parts
                    if (relatedParts) {
                        try {
                            List<RelatedPart> new_relparts = new List<RelatedPart>();
                            List<RelatedPart> relparts = db.RelatedParts.Where(x => x.partID == partID).ToList<RelatedPart>();
                            foreach (RelatedPart relpart in relparts) {
                                RelatedPart nrp = new RelatedPart {
                                    relatedID = relpart.relatedID,
                                    rTypeID = relpart.rTypeID,
                                    partID = new_part.partID
                                };
                                new_relparts.Add(nrp);
                            }
                            db.RelatedParts.InsertAllOnSubmit(new_relparts);
                            db.SubmitChanges();
                            messages.Add("Related Parts Cloned Successfully");
                        } catch {
                            messages.Add("There was a problem cloning the related parts.");
                        }
                    }
                    #endregion

                    #region clone Attributes
                    if (attributes) {
                        try {
                            List<PartAttribute> new_attrs = new List<PartAttribute>();
                            List<PartAttribute> attributelist = db.PartAttributes.Where(x => x.partID == partID).Where(x => x.field.ToLower() != "upc").ToList<PartAttribute>();
                            foreach (PartAttribute attribute in attributelist) {
                                PartAttribute attr = new PartAttribute {
                                    value = attribute.value,
                                    field = attribute.field,
                                    partID = new_part.partID,
                                    sort = attribute.sort
                                };
                                new_attrs.Add(attr);
                            }
                            db.PartAttributes.InsertAllOnSubmit(new_attrs);
                            db.SubmitChanges();
                            messages.Add("Attributes Cloned Successfully");
                        } catch {
                            messages.Add("There was a problem cloning the attributes.");
                        }
                    }
                    #endregion

                    #region clone Content
                    if (content) {
                        try {
                            List<ContentBridge> new_content = new List<ContentBridge>();
                            List<ContentBridge> contents = (from cb in db.ContentBridges
                                                            where cb.partID == partID
                                                            select cb).ToList<ContentBridge>();
                            foreach (ContentBridge cont in contents) {
                                Content c = db.Contents.Where(x => x.contentID.Equals(cont.contentID)).FirstOrDefault();
                                Content new_c = new Content {
                                    cTypeID = c.cTypeID,
                                    text = c.text
                                };
                                db.Contents.InsertOnSubmit(new_c);
                                db.SubmitChanges();
                                ContentBridge cb = new ContentBridge {
                                        partID = new_part.partID,
                                        contentID = new_c.contentID
                                };
                                db.ContentBridges.InsertOnSubmit(cb);
                                db.SubmitChanges();
                            }
                            messages.Add("Contents Cloned Successfully");
                        } catch {
                            messages.Add("There was a problem cloning the contents.");
                        }
                    }
                    #endregion

                    #region clone Vehicles
                    if (vehicles) {
                        try {
                            List<VehiclePart> vehiclelist = db.VehicleParts.Where(x => x.partID == partID).ToList<VehiclePart>();
                            foreach (VehiclePart vp in vehiclelist) {
                                VehiclePart vehiclepart = new VehiclePart {
                                    partID = new_part.partID,
                                    vehicleID = vp.vehicleID,
                                    drilling = vp.drilling,
                                    installTime = vp.installTime,
                                    exposed = vp.exposed
                                };
                                db.VehicleParts.InsertOnSubmit(vehiclepart);
                                db.SubmitChanges();

                                List<VehiclePartAttribute> new_vpattr = new List<VehiclePartAttribute>();
                                List<VehiclePartAttribute> vpattrs = db.VehiclePartAttributes.Where(x => x.vPartID == vp.vPartID).ToList<VehiclePartAttribute>();
                                foreach (VehiclePartAttribute vpa in vpattrs) {
                                    VehiclePartAttribute new_vpa = new VehiclePartAttribute {
                                        vPartID = vehiclepart.vPartID,
                                        value = vpa.value,
                                        field = vpa.field,
                                        sort = vpa.sort
                                    };
                                    new_vpattr.Add(new_vpa);
                                };
                                db.VehiclePartAttributes.InsertAllOnSubmit(new_vpattr);
                                db.SubmitChanges();
                                messages.Add("Vehicles Cloned Successfully");
                            }
                        } catch {
                            messages.Add("There was a problem cloning the vehicles.");
                        }
                    }
                    #endregion

                    #region clone Prices
                    if (prices) {
                        try {
                            List<Price> new_prices = new List<Price>();
                            List<Price> pricelist = db.Prices.Where(x => x.partID == partID).ToList<Price>();
                            foreach (Price prc in pricelist) {
                                Price price = new Price {
                                    priceType = prc.priceType,
                                    price1 = prc.price1,
                                    partID = new_part.partID,
                                    enforced = prc.enforced
                                };
                                new_prices.Add(price);
                            }
                            db.Prices.InsertAllOnSubmit(new_prices);
                            db.SubmitChanges();
                            messages.Add("Prices Cloned Successfully");
                        } catch {
                            messages.Add("There was a problem cloning the prices.");
                        }
                    }
                    #endregion

                    ImportImages(new_part.partID);

                    db.indexPart(new_part.partID);
                    messages.Add("Part Cloned Successfully.");

                } catch (Exception e) {
                    messages.Add(e.Message);
                }
            } else {
                messages.Add("Part Clone Failed.");
            }
            #endregion

            return Newtonsoft.Json.JsonConvert.SerializeObject(messages);
        }
        
        public ActionResult Edit(int partID = 0) {

            // Get the part
            ConvertedPart part = new ConvertedPart();
            part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            // Get the product classes
            ViewBag.classes = ProductModels.GetClasses();
            ViewBag.UPC = ProductModels.GetAttribute(part.partID, "UPC");

            ViewBag.PartTypes = new ACES().GetPartTypes();

            ViewBag.active_tab = "info";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(int partID = 0, string shortDesc = "", int status = 0, string oldPartNumber = "", int priceCode = 0, int classID = 0, string btnSubmit = "", string btnContinue = "", string upc = "", bool featured = false, int? ACESPartTypeID = null) {

            CurtDevDataContext db = new CurtDevDataContext();
            List<string> error_messages = new List<string>();
            Part part = new Part();

            part = (from p in db.Parts
                    where p.partID.Equals(partID)
                    select p).FirstOrDefault<Part>();
            part.partID = partID;
            part.shortDesc = shortDesc;
            part.status = status;
            if (status == 900) {
                // find eMAP pricing and set enforced = 0
                Price eMapPrice = db.Prices.Where(x => x.priceType == "eMap" && x.partID == partID).FirstOrDefault<Price>();
                if (eMapPrice != null) {
                    eMapPrice.enforced = false;
                    eMapPrice.dateModified = DateTime.Now;
                    db.SubmitChanges();
                }
            }
            part.oldPartNumber = oldPartNumber;
            part.priceCode = priceCode;
            part.classID = classID;
            part.dateModified = DateTime.Now;
            part.featured = featured;
            part.ACESPartTypeID = ACESPartTypeID;

            // Validate the partID and shortDesc fields
            if (partID == 0) { error_messages.Add("You must enter a part number."); }
            if (shortDesc.Length == 0) { error_messages.Add("You must enter a short description."); }
            if (upc.Trim().Length == 0) { error_messages.Add("You must enter a UPC."); }

            if (error_messages.Count == 0) { // No errors, add the part
                try {
                    db.SubmitChanges();

                    if (ProductModels.HasAttribute(partID, "UPC")) {
                        ProductModels.UpdateAttributeByField(partID, "UPC", upc);
                    } else {
                        ProductModels.SaveAttribute(0,partID, "UPC", upc);
                    }
                    
                    db.indexPart(part.partID);

                    if(btnSubmit != ""){ // Redirect to product index page
                        return RedirectToAction("index");
                    }
                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }
            
            // Save the error messages into the bag
            ViewBag.error_messages = error_messages;

            // Get the ConvertedPart version of this part object
            ViewBag.part = ProductModels.GetPart(part.partID);
            ViewBag.UPC = ProductModels.GetAttribute(part.partID, "UPC");

            // Get the product classes
            ViewBag.classes = ProductModels.GetClasses();

            ViewBag.PartTypes = new ACES().GetPartTypes();

            ViewBag.active_tab = "info";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditCategories(int partID = 0) {

            // Get the part
            ViewBag.part = ProductModels.GetPart(partID);

            // Get all of the categories
            ViewBag.cats = ProductCat.GetCategories();

            // Get this items categories
            ViewBag.part_cats = ProductCat.GetPartsCategories(partID);

            ViewBag.active_tab = "categories";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditRelated(int partID = 0) {

            // Get the related parts for this part
            List<ConvertedPart> related_parts = ProductModels.GetRelatedParts(partID);
            ViewBag.related_parts = related_parts;

            // Get all of the parts
            List<ConvertedPart> parts = ProductModels.GetAllParts();
            ViewBag.parts = parts;

            // Get the part we're working with
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            ViewBag.active_tab = "related";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditIncluded(int partID = 0) {

            // Get the related parts for this part
            List<ConvertedPart> included_parts = ProductModels.GetIncludedParts(partID);
            ViewBag.included_parts = included_parts;

            // Get all of the parts
            List<ConvertedPart> parts = ProductModels.GetAllParts();
            ViewBag.parts = parts;

            // Get the part we're working with
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            ViewBag.active_tab = "included";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditGroups(int partID = 0) {

            // Get the related parts for this part
            List<PartGroup> partGroups = ProductModels.GetPartGroups(partID);
            ViewBag.partGroups = partGroups;

            // Get the part we're working with
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            ViewBag.active_tab = "groups";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditReviews(int partID = 0) {

            // Get the reviews for this product
            List<ReviewDetail> reviews = new List<ReviewDetail>();
            reviews = ReviewModel.GetReviews(partID);
            ViewBag.reviews = reviews;

            // Get the part
            ConvertedPart part = new ConvertedPart();
            part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            ViewBag.active_tab = "reviews";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditAttributes(int partID = 0) {
            // Get the part
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            // Get the attributes for this part
            List<PartAttribute> attributes = new List<PartAttribute>();
            attributes = ProductModels.GetAttributes(partID);
            ViewBag.attributes = attributes;

            // Get the distinct attribute fields
            List<string> fields = ProductModels.GetDistinctAttributeFields();
            ViewBag.fields = fields;

            ViewBag.active_tab = "attributes";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditContent(int partID = 0) {

            // Get the part
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            // Get the content
            List<FullContent> part_content = new List<FullContent>();
            part_content = ProductModels.GetPartContent(partID);
            ViewBag.part_content = part_content;

            // Get the content types
            CurtDevDataContext db = new CurtDevDataContext();
            List<ContentType> content_types = new List<ContentType>();
            content_types = (from ct in db.ContentTypes
                             select ct).Distinct().OrderBy(x => x.type).ToList<ContentType>();
            ViewBag.content_types = content_types;

            ViewBag.active_tab = "content";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditImages(int partID = 0) {

            // Get the part
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            // Get the content
            List<ImageSize> sizes = ImageModel.GetPartImages(partID);
            ViewBag.sizes = sizes;

            ViewBag.active_tab = "images";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditVideos(int partID = 0) {

            // Get the video types
            CurtDevDataContext db = new CurtDevDataContext();
            List<videoType> types = db.videoTypes.OrderBy(x => x.name).ToList<videoType>();
            ViewBag.video_types = types;

            // Get the part
            ConvertedPart part = new ConvertedPart();
            part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            // Get the content
            List<PartVideo> videos = PartVideoModel.GetPartVideos(partID);
            ViewBag.videos = videos;

            ViewBag.active_tab = "videos";
            return View();
        }

        public string ImportImages(int partid = 0) {
            HttpServerUtilityBase server = Server;
            ImportService importservice = new ImportService();
            string complete = importservice.ImportImages(server, partid);

            return complete;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditVehicles(int partID = 0) {
            // Get the part
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            CurtDevDataContext db = new CurtDevDataContext();
            // Get the distinct drilling values
            List<string> drilling_value = db.VehicleParts.Select(v => v.drilling).Where(y => y != "").Distinct().OrderBy(x => x).ToList<string>();
            ViewBag.drilling_value = drilling_value;

            // Get the distinct drilling values
            List<string> exposed_value = db.VehicleParts.Select(v => v.exposed).Where(y => y != "").Distinct().OrderBy(x => x).ToList<string>();
            ViewBag.exposed_value = exposed_value;

            // Get the vehicles that are tied to this part
            List<FullVehicle> part_vehicles = new List<FullVehicle>();
            part_vehicles = ProductModels.GetVehicles(partID);
            ViewBag.part_vehicles = part_vehicles;

            // Get the distinct attribute fields
            List<string> fields = ProductModels.GetDistinctVehiclePartAttributeFields();
            ViewBag.fields = fields;

            List<Year> years = CurtAdmin.Models.Vehicle.GetYears();
            ViewBag.years = years;

            ViewBag.active_tab = "vehicles";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditACESVehicles(int partID = 0) {
            // Get the part
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            // Get the vehicles that are tied to this part
            List<ACESBaseVehicle> part_vehicles = new List<ACESBaseVehicle>();
            part_vehicles = new ACES().GetVehiclesByPart(partID);
            ViewBag.part_vehicles = part_vehicles;

            List<vcdb_Make> makes = new ACES().GetMakes();
            ViewBag.makes = makes;

            ViewBag.active_tab = "acesvehicles";
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string GetPartVehicles(int partID = 0) {
            List<ACESBaseVehicle> part_vehicles = new List<ACESBaseVehicle>();
            part_vehicles = new ACES().GetVehiclesByPart(partID);
            return JsonConvert.SerializeObject(part_vehicles);
        }
        
        public ActionResult EditPricing(int partID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();

            // Get the part
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            // Get the distinct price types
            List<string> price_types = db.Prices.Select(p => p.priceType).Distinct().OrderBy(x => x).ToList<string>();
            ViewBag.price_types = price_types;

            // Get the prices for this part
            List<Price> prices = ProductModels.GetPrices(partID);
            ViewBag.prices = prices;

            ViewBag.active_tab = "pricing";
            return View();
        }

        public ActionResult EditPackages(int partID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();

            // Get the part
            ConvertedPart part = ProductModels.GetPart(partID);
            ViewBag.part = part;

            List<UnitOfMeasure> units = db.UnitOfMeasures.OrderBy(x => x.name).ToList<UnitOfMeasure>();
            ViewBag.units = units;

            // Get the prices for this part
            List<PartPackage> packages = ProductModels.GetPackages(partID);
            ViewBag.packages = packages;

            ViewBag.active_tab = "packages";
            return View();
        }

        public ActionResult ViewAll() {
            List<ConvertedPart> parts = ProductModels.GetAllParts();
            ViewBag.parts = parts;

            return View();

        }

        /*** AJAX ****/

        /// <summary>
        /// Delete a given part from the database
        /// </summary>
        /// <param name="partID">ID the part</param>
        /// <returns>String containing error message</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string DeletePart(int partID = 0) {
            return ProductModels.DeletePart(partID);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string AddCategory(int catID = 0, int partID = 0) {
            return ProductModels.AddCategoryToPart(catID, partID);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteCategory(int catID = 0, int partID = 0) {
            return ProductModels.DeleteCategoryFromPart(catID, partID);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string AddRelated(int partID = 0, int relatedID = 0) {
            return ProductModels.AddRelated(partID, relatedID);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteRelated(int partID = 0, int relatedID = 0) {
            return ProductModels.DeleteRelated(partID, relatedID);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string AddIncluded(int partID = 0, int includedID = 0) {
            return ProductModels.AddIncluded(partID, includedID);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteIncluded(int partID = 0, int includedID = 0) {
            return ProductModels.DeleteIncluded(partID, includedID);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string AddReview(int partID = 0, int rating = 0, string subject = "", string review_text = "", string name = "", string email = "", int reviewID = 0) {
            return ReviewModel.Add(partID, rating, subject, review_text, name, email, reviewID);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string AddVehicle(int partID = 0, int vehicleID = 0){
            string response = ProductModels.AddVehicle(partID, vehicleID);
            if (response.Length == 0) {
                // Get the vehicle
                FullVehicle vehicle = Models.Vehicle.GetFullVehicle(vehicleID);
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(vehicle);
            }
            else {
                return response;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string AddVehicleByFilter(int partID = 0, int yearID = 0, int makeID = 0, int modelID = 0, int styleID = 0) {
            int vehicleID = CurtAdmin.Models.Vehicle.GetVehicleID(yearID, makeID, modelID, styleID);
            string response = ProductModels.AddVehicle(partID, vehicleID);
            if (response.Length == 0) {
                // Get the vehicle
                FullVehicle vehicle = Models.Vehicle.GetFullVehicle(vehicleID);
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(vehicle);
            } else {
                return response;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string UpdateVehicle(int partID = 0, int vehicleID = 0, string drilling = "", string exposed = "", int installTime = 0) {
            try {
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(ProductModels.UpdateVehicle(partID, vehicleID, drilling, exposed, installTime));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        /// <summary>
        /// Get a part video from a videoID
        /// </summary>
        /// <param name="pVideoID">ID of the video</param>
        /// <returns>JSON container Models</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string GetPartVideo(int pVideoID = 0) {
            PartVideo video = PartVideoModel.Get(pVideoID);
            return JsonConvert.SerializeObject(video);
        }

        /// <summary>
        /// Get the models for a given make and year
        /// </summary>
        /// <param name="makeID">ID of the make</param>
        /// <returns>JSON container Models</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string GetModels(int yearID = 0, int makeID = 0) {
            string models = "";

            List<Model> modelList = Models.Vehicle.GetModelsByYearMake(yearID, makeID);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            models = ser.Serialize(modelList);

            return models;
        }


        /// <summary>
        /// Get the styles for a given model, make, and year
        /// </summary>
        /// <param name="modelID">ID of a make</param>
        /// <returns>JSON containing styles</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string GetStyles(int yearID = 0, int makeID = 0, int modelID = 0) {
            string models = "";

            List<Style> modelList = Models.Vehicle.GetStylesByYearMakeModel(yearID, makeID, modelID);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            models = ser.Serialize(modelList);

            return models;
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        public string UpdateVehiclePartAttribute(int vpAttrID = 0, string field = "", string value = "") {
            try {
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(ProductModels.UpdateVehiclePartAttribute(vpAttrID, field, value));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string AddVehiclePartAttribute(int vPartID = 0, string field = "", string value = "") {
            try {
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(ProductModels.AddVehiclePartAttribute(vPartID, field, value));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteVehiclePartAttribute(int vpAttrID = 0) {
            return ProductModels.DeleteVehiclePartAttribute(vpAttrID);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteVehicle(int vehicleID = 0, int partID = 0) {
            return ProductModels.DeleteVehiclePart(vehicleID, partID);
        }

        public string getAllVehicles() {
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<FullVehicle> vehicles = new List<FullVehicle>();
            vehicles = Models.Vehicle.GetVehicles();
            return js.Serialize(vehicles);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string GetVehiclePart(int vehicleID = 0, int partID = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            try {
                return js.Serialize(ProductModels.GetVehiclePart(vehicleID, partID));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        public string GetCarryOverData(int vehicleID = 0, int partID = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            try {
                return js.Serialize(ProductModels.GetLatestVehiclePart(vehicleID, partID));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        public string GetAllPartOptions(int partID = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            try {
                return js.Serialize(ProductModels.getAllCarryOverParts(partID));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        public string CarryOverPart(int vehicleID = 0, int partID = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            try {
                return js.Serialize(ProductModels.CarryOverPart(vehicleID, partID));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string GetVehiclePartAttribute(int vpAttrID = 0) {
            try {
                return JsonConvert.SerializeObject(ProductModels.GetVehiclePartAttribute(vpAttrID));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string GetGroup(int groupID) {
            try {
                return JsonConvert.SerializeObject(ProductModels.GetGroup(groupID));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string SaveGroup(int partID, string name, int groupID = 0) {
            try {
                return JsonConvert.SerializeObject(ProductModels.SaveGroup(partID, name, groupID));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteGroup(int groupID = 0) {
            try {
                return ProductModels.DeleteGroup(groupID);
            } catch (Exception e) {
                return e.Message;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string AddGroupPart(int groupID, int partID) {
            try {
                return JsonConvert.SerializeObject(ProductModels.AddGroupPart(groupID, partID));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        public string updateGroupSort() {
            List<string> partgroupparts = Request.QueryString["parts[]"].Split(',').ToList<string>();
            ProductModels.UpdateGroupSort(partgroupparts);
            return "";
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string RemovePartFromGroup(int id) {
            try {
                ProductModels.RemovePartFromGroup(id);
                return "{\"success\":true}";
            } catch {
                return "{\"success\":false}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get), ValidateInput(false)]
        public string SaveContent(int contentID = 0, int partID = 0, string content = "", int contentType = 0) {
            try {
                return Newtonsoft.Json.JsonConvert.SerializeObject(ProductModels.SaveContent(contentID, partID, content, contentType));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteContent(int partID = 0, int contentID = 0) {
            try {
                return ProductModels.DeleteContent(partID, contentID);
            } catch (Exception e) {
                return e.Message;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string GetFullContent(int contentID = 0) {
            try {
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(ProductModels.GetFullContent(contentID));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string SaveAttribute(int attrID = 0, int partID = 0, string field = "", string value = "") {
            try {
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(ProductModels.SaveAttribute(attrID, partID, field, value));
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteAttribute(int attrID = 0) {
            try {
                ProductModels.DeleteAttribute(attrID);
                return "Attribute removed.";
            } catch (Exception e) {
                return e.Message;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string SavePrice(int partID = 0, int priceID = 0, decimal price = 0, string price_type = "", bool enforced = true) {
            try {
                Price saved_price = new Price();
                saved_price = ProductModels.SavePrice(priceID, price, price_type, partID, enforced);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                Newtonsoft.Json.Formatting format = Newtonsoft.Json.Formatting.None;
                return JsonConvert.SerializeObject(saved_price, format, settings);
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string DeletePrice(int priceID = 0) {
            try {
                if (ProductModels.DeletePrice(priceID)) {
                    return "";
                } else {
                    return "Failed to remove price.";
                }
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string SavePackage(int packageID = 0, int partID = 0, double weight = 0, double height = 0, double width = 0, double length = 0, int qty = 1, int weightUnit = 0, int heightUnit = 0, int widthUnit = 0, int lengthUnit = 0, int qtyUnit = 0) {
            try {
                PartPackage package = new PartPackage();
                package = ProductModels.SavePackage(packageID, partID, weight, height, width, length, qty, weightUnit, heightUnit, qtyUnit);

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                Newtonsoft.Json.Formatting format = Newtonsoft.Json.Formatting.None;
                return JsonConvert.SerializeObject(package, format, settings);
            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string GetPackage(int packageID) {
            PartPackage package = ProductModels.GetPackage(packageID);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Newtonsoft.Json.Formatting format = Newtonsoft.Json.Formatting.None;
            return JsonConvert.SerializeObject(package, format, settings);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string DeletePackage(int packageID = 0) {
            try {
                return ProductModels.DeletePackage(packageID);
            } catch (Exception e) {
                return e.Message;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string AddImage(int partID = 0, int size = 0, string file = "") {
            string fullpath = file;
            if (file.Trim() != "" && partID > 0 && size > 0) {
                try {
                    fullpath = file.Replace("http://docs.curthitch.biz", "");
                    fullpath = Server.MapPath(fullpath.Replace("http://www.curtmfg.com", ""));
                } catch { };
                ProductModels.UpdatePart(partID);
                return ImageModel.AddImage(partID, size, file, fullpath);
            } else {
                return "error";
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string DeleteImage(int imageid = 0) {
            try {
                ImageModel.DeleteImage(imageid);
                return "success";
            } catch { return "error"; };
        }

        public string CarryOver(int vPartID = 0) {
            return "";
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public void updateSort() {
            List<string> partimage = Request.QueryString["partimage[]"].Split(',').ToList<string>();
            ImageModel.UpdateSort(partimage);
        }

        public string updateAttributeSort() {
            List<string> partattributes = Request.QueryString["attr[]"].Split(',').ToList<string>();
            ProductModels.UpdateAttributeSort(partattributes);
            return "";
        }

        public string updateVehicleAttributeSort() {
            List<string> vpartattributes = Request.QueryString["attribute[]"].Split(',').ToList<string>();
            ProductModels.updateVehicleAttributeSort(vpartattributes);
            return "";
        }

        public string GetVideo(int pVideoID = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            if (pVideoID > 0) {
                return js.Serialize(PartVideoModel.Get(pVideoID));
            } else {
                return "{\"error\":\"Video Not Found\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string SaveVideo(int partID = 0, int pVideoID = 0, int videoType = 0, bool isPrimary = false, string video = "") {
            if (video.Trim() != "" && partID > 0 && videoType > 0) {
                return PartVideoModel.Save(pVideoID,partID,videoType,video,isPrimary);
            } else {
                return "{\"error\":\"Video Not Saved\"}";
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string DeleteVideo(int videoID = 0) {
            try {
                PartVideoModel.DeleteVideo(videoID);
                return "success";
            } catch { return "error"; };
        }

    }
}
