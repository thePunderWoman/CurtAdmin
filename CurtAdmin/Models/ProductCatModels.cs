using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;
using System.Data.Linq;

namespace CurtAdmin.Models {
    public class ProductCat {

        /// <summary>
        /// Retrieves a list of DetailedCategories objects
        /// </summary>
        /// <returns>List of DetailedCategories objects</returns>
        public static List<DetailedCategories> GetCategories() {
            List<DetailedCategories> cats = new List<DetailedCategories>();
            CurtDevDataContext db = new CurtDevDataContext();

            cats = (from c in db.Categories
                    select new DetailedCategories { 
                        catID = c.catID,
                        dateAdded = c.dateAdded,
                        parentID = c.parentID,
                        parentCat = (from c2 in db.Categories where c2.catID.Equals(c.parentID) select c2.catTitle).FirstOrDefault<string>(),
                        catTitle = c.catTitle,
                        shortDesc = c.shortDesc,
                        longDesc = c.longDesc,
                        image = c.image,
                        isLifestyle = c.isLifestyle,
                        vehicleSpecific = c.vehicleSpecific,
                        partCount = (from p in db.Parts join cp in db.CatParts on p.partID equals cp.partID where cp.catID.Equals(c.catID) select p).Distinct().Count(),
                        content = (from cb in db.ContentBridges
                                   join co in db.Contents on cb.contentID equals co.contentID
                                   join ct in db.ContentTypes on co.cTypeID equals ct.cTypeID
                                   where cb.catID.Equals(c.catID)
                                   orderby co.cTypeID
                                   select new FullContent {
                                       content_type_id = co.cTypeID,
                                       content_type = ct.type,
                                       contentID = co.contentID,
                                       content = co.text
                                   }).ToList<FullContent>()
                    }).Distinct().OrderBy(x => x.catTitle).ToList<DetailedCategories>();

            return cats;
        }

        /// <summary>
        /// Retrieves a list of DetailedCategories objects
        /// </summary>
        /// <returns>List of DetailedCategories objects</returns>
        public static List<DetailedCategories> GetLifestyles() {
            List<DetailedCategories> cats = new List<DetailedCategories>();
            CurtDevDataContext db = new CurtDevDataContext();

            cats = (from c in db.Categories
                    where c.isLifestyle.Equals(1)
                    orderby c.sort
                    select new DetailedCategories {
                        catID = c.catID,
                        dateAdded = c.dateAdded,
                        parentID = c.parentID,
                        parentCat = (from c2 in db.Categories where c2.catID.Equals(c.parentID) select c2.catTitle).FirstOrDefault<string>(),
                        catTitle = c.catTitle,
                        shortDesc = c.shortDesc,
                        longDesc = c.longDesc,
                        image = c.image,
                        isLifestyle = c.isLifestyle,
                        vehicleSpecific = c.vehicleSpecific,
                        partCount = (from p in db.Parts join cp in db.CatParts on p.partID equals cp.partID where cp.catID.Equals(c.catID) select p).Distinct().Count(),
                        content = (from cb in db.ContentBridges
                                   join co in db.Contents on cb.contentID equals co.contentID
                                   join ct in db.ContentTypes on co.cTypeID equals ct.cTypeID
                                   where cb.catID.Equals(c.catID)
                                   orderby co.cTypeID
                                   select new FullContent {
                                       content_type_id = co.cTypeID,
                                       content_type = ct.type,
                                       contentID = co.contentID,
                                       content = co.text
                                   }).ToList<FullContent>()
                    }).Distinct().OrderBy(x => x.catTitle).ToList<DetailedCategories>();

            return cats;
        }

        /// <summary>
        /// Gets all the categories that the given part falls into
        /// </summary>
        /// <param name="partID">ID of the part</param>
        /// <returns>List of DetailedCategories object</returns>
        public static List<DetailedCategories> GetPartsCategories(int partID = 0) {
            List<DetailedCategories> cats = new List<DetailedCategories>();
            CurtDevDataContext db = new CurtDevDataContext();

            cats = (from c in db.Categories
                    join cp in db.CatParts on c.catID equals cp.catID
                    where cp.partID.Equals(partID)
                    select new DetailedCategories {
                        catID = c.catID,
                        dateAdded = c.dateAdded,
                        parentID = c.parentID,
                        parentCat = (from c2 in db.Categories where c2.catID.Equals(c.parentID) select c2.catTitle).FirstOrDefault<string>(),
                        catTitle = c.catTitle,
                        shortDesc = c.shortDesc,
                        longDesc = c.longDesc,
                        image = c.image,
                        isLifestyle = c.isLifestyle,
                        partCount = (from p in db.Parts where p.partID.Equals(cp.partID) select p).Count(),
                        content = (from cb in db.ContentBridges 
                                   join co in db.Contents on cb.contentID equals co.contentID
                                   join ct in db.ContentTypes on co.cTypeID equals ct.cTypeID
                                   where cb.catID.Equals(c.catID)
                                   orderby co.cTypeID
                                   select new FullContent {
                                       content_type_id = co.cTypeID,
                                       content_type = ct.type,
                                       contentID = co.contentID,
                                       content = co.text
                                   }).ToList<FullContent>()
                    }).ToList<DetailedCategories>();
            return cats;
        }


        /// <summary>
        /// Get the parts of a given category
        /// </summary>
        /// <param name="catID">ID of the category</param>
        /// <returns>List of ConvertedPart object</returns>
        public static List<ConvertedPart> GetCategoryItems(int catID = 0) {
            List<ConvertedPart> parts = new List<ConvertedPart>();
            CurtDevDataContext db = new CurtDevDataContext();

            parts = (from p in db.Parts
                     join cp in db.CatParts on p.partID equals cp.partID
                     where cp.catID.Equals(catID)
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
                     }).Distinct().ToList<ConvertedPart>();

            return parts;
        }

        /// <summary>
        /// Get a given category's information
        /// </summary>
        /// <param name="catID">ID of the category</param>
        /// <returns>DetailedCategories object</returns>
        public static DetailedCategories GetCategory(int catID = 0) {
            DetailedCategories cat = new DetailedCategories();
            CurtDevDataContext db = new CurtDevDataContext();

            cat = (from c in db.Categories
                   join cp in db.CatParts on c.catID equals cp.catID into CParts from cp in CParts.DefaultIfEmpty()
                   where c.catID.Equals(catID)
                   select new DetailedCategories {
                       catID = c.catID,
                       dateAdded = c.dateAdded,
                       parentID = c.parentID,
                       parentCat = (from c2 in db.Categories where c2.catID.Equals(c.parentID) select c2.catTitle).FirstOrDefault<string>(),
                       catTitle = c.catTitle,
                       shortDesc = c.shortDesc,
                       longDesc = c.longDesc,
                       image = c.image,
                       isLifestyle = c.isLifestyle,
                       vehicleSpecific = c.vehicleSpecific,
                       partCount = (from p in db.Parts where p.partID.Equals(cp.partID) select p).Count(),
                       content = (from cb in db.ContentBridges 
                                  join co in db.Contents on cb.contentID equals co.contentID
                                  join ct in db.ContentTypes on co.cTypeID equals ct.cTypeID
                                  where cb.catID.Equals(c.catID)
                                  orderby co.cTypeID
                                  select new FullContent {
                                      content_type_id = co.cTypeID,
                                      content_type = ct.type,
                                      contentID = co.contentID,
                                      content = co.text
                                  }).ToList<FullContent>()

                   }).FirstOrDefault<DetailedCategories>();
            return cat;
        }

        /// <summary>
        /// Saves the given category information.
        /// </summary>
        /// <param name="catID">ID of the category</param>
        /// <param name="catTitle">Category title</param>
        /// <param name="parentID">ID of the parent category</param>
        /// <param name="image">Category images</param>
        /// <param name="isLifestyle">Lifestyle category?</param>
        /// <param name="shortDesc">Short Description</param>
        /// <param name="longDesc">Long Description</param>
        /// <returns>List of errors.</returns>
        public static Categories SaveCategory(int catID = 0, string catTitle = "", int parentID = 0, string image = "", int isLifestyle = 0, string shortDesc = "", string longDesc = "", bool vehicleSpecific = false) {

            CurtDevDataContext db = new CurtDevDataContext();
            Categories cat = new Categories();

            // Validate the form fields
            if (catTitle.Trim().Length == 0) { throw new Exception("You Must Enter a category Title."); }
            if (shortDesc.Length == 0) { throw new Exception("You must enter a short description."); }

            if (catID != 0) { // Updating a category
                // Get the category
                cat = (from c in db.Categories
                                    where c.catID.Equals(catID)
                                    select c).SingleOrDefault<Categories>();
            }

            // Update the fields
            cat.catTitle = catTitle.Trim();
            cat.dateAdded = DateTime.Now;
            cat.parentID = parentID;
            cat.image = image;
            cat.isLifestyle = isLifestyle;
            cat.shortDesc = shortDesc.Trim();
            cat.longDesc = longDesc;
            cat.vehicleSpecific = vehicleSpecific;

            if (catID == 0) {
                db.Categories.InsertOnSubmit(cat);
            }

            // Save the changes
            db.SubmitChanges();

            return cat;
        }

        public static List<ConvertedPart> GetUncategorizedParts() {
            CurtDevDataContext db = new CurtDevDataContext();

            List<ConvertedPart> unknown_parts = (from p in db.Parts
                              join cp in db.CatParts on p.partID equals cp.partID into PartJoin from cp in PartJoin.DefaultIfEmpty()
                              where cp.catID.Equals(null)
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
                                  listPrice = String.Format("{0:C}",(from prices in db.Prices
                                                             where prices.partID.Equals(p.partID) && prices.priceType.Equals("List")
                                                             select prices.price1 != null ? prices.price1 : (decimal?)0).FirstOrDefault<decimal?>())
                              }).ToList<ConvertedPart>();

            return unknown_parts;
        }

        public static decimal GetPrice(int partID = 0, string priceType = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            decimal listPrice = 0;

            if (partID > 0 && priceType != "") {
                Price p = (from prices in db.Prices
                           where prices.partID.Equals(partID) && prices.priceType.Equals(priceType)
                           select prices).FirstOrDefault<Price>();
                if (p != null && p.price1 > 0) {
                    listPrice = p.price1;
                }
            }

            return listPrice;
        }

        /// <summary>
        /// Delete the given category from the database and break relationship to any parts.
        /// </summary>
        /// <param name="catID">ID of the category</param>
        /// <returns>Error message (string)</returns>
        public static string DeleteCategory(int catID = 0) {
            if (catID == 0) { throw new Exception("There was error while processing. Category data is invalid."); }

            try {
                CurtDevDataContext db = new CurtDevDataContext();

                Categories cat = (from c in db.Categories
                                  where c.catID.Equals(catID)
                                  select c).FirstOrDefault<Categories>();
                db.Categories.DeleteOnSubmit(cat);

                List<ContentBridge> bridges = db.ContentBridges.Where(x => x.catID.Equals(catID)).ToList<ContentBridge>();
                List<int> contentids = bridges.Select(x => x.contentID).ToList();
                List<Content> contents = db.Contents.Where(x => contentids.Contains(x.contentID)).ToList<Content>();
                db.ContentBridges.DeleteAllOnSubmit(bridges);
                db.Contents.DeleteAllOnSubmit(contents);

                // Remove the CatPart relationships for this category
                List<CatParts> cp = (from cat_parts in db.CatParts
                                     where cat_parts.catID.Equals(catID)
                                     select cat_parts).ToList<CatParts>();
                db.CatParts.DeleteAllOnSubmit<CatParts>(cp);
                db.SubmitChanges();
            } catch (Exception e) {
                return e.Message;
            }

            return "";
        }

    }
}