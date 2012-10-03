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
using System.Net.Mail;
using System.Web.Script.Serialization;

namespace CurtAdmin.Models {

    public class Vehicle {


        /// <summary>
        /// Gets the vehicles.
        /// </summary>
        /// <returns>List of Vehicles</returns>
        /// <remarks></remarks>
        public static List<FullVehicle> GetVehicles() {
            List<FullVehicle> vehicles = new List<FullVehicle>();
            CurtDevDataContext db = new CurtDevDataContext();

            vehicles = (from v in db.Vehicles
                        join y in db.Years on v.yearID equals y.yearID
                        join ma in db.Makes on v.makeID equals ma.makeID
                        join mo in db.Models on v.modelID equals mo.modelID
                        join s in db.Styles on v.styleID equals s.styleID
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
                        }).Distinct().OrderBy(x => x.vehicleID).ToList<FullVehicle>();

            return vehicles;
        }

        public static List<FullVehicle> FindVehicles(string term = "") {
            List<FullVehicle> vehicles = new List<FullVehicle>();
            CurtDevDataContext db = new CurtDevDataContext();
            double query_year = 0;
            Double.TryParse(term, out query_year);
            vehicles = (from v in db.Vehicles
                        join y in db.Years on v.yearID equals y.yearID
                        join ma in db.Makes on v.makeID equals ma.makeID
                        join mo in db.Models on v.modelID equals mo.modelID
                        join s in db.Styles on v.styleID equals s.styleID
                        where s.style1.Contains(term) || mo.model1.Contains(term) || ma.make1.Contains(term) || y.year1.Equals(query_year)
                        orderby v.vehicleID
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
                        }).ToList<FullVehicle>();

            return vehicles;
        }

        public static List<Year> GetYears() {
            List<Year> years = new List<Year>();
            CurtDevDataContext db = new CurtDevDataContext();
            years = (from v in db.Years
                     select v).Distinct().OrderByDescending(x => x.year1).ToList<Year>();
            return years;
        }

        public static List<Make> GetMakes(int yearID = 0) {
            List<Make> makes = new List<Make>();
            CurtDevDataContext db = new CurtDevDataContext();
            if (yearID > 0) {
                makes = (from m in db.Makes
                         join ym in db.YearMakes on m.makeID equals ym.makeID
                         where ym.yearID.Equals(yearID)
                         select m).Distinct().OrderBy(x => x.make1).ToList<Make>();

            } else {
                makes = (from m in db.Makes
                         select m).Distinct().OrderBy(x => x.make1).ToList<Make>();
            }

            return makes;
        }

        public static List<Model> GetModels(int makeID = 0) {
            List<Model> models = new List<Model>();
            CurtDevDataContext db = new CurtDevDataContext();
            if (makeID > 0) {
                models = (from m in db.Models
                          join mm in db.MakeModels on m.modelID equals mm.modelID
                          where mm.makeID.Equals(makeID)
                          select m).Distinct().OrderBy(x => x.model1).ToList<Model>();
            } else {
                models = (from m in db.Models
                          select m).Distinct().OrderBy(x => x.model1).ToList<Model>();
            }

            return models;
        }

        public static List<Model> GetModelsByYearMake(int yearID = 0, int makeID = 0) {
            List<Model> models = new List<Model>();
            CurtDevDataContext db = new CurtDevDataContext();
            if (yearID > 0 && makeID > 0) {
                models = (from m in db.Models
                             join v in db.Vehicles on m.modelID equals v.modelID
                             join y in db.Years on v.yearID equals y.yearID
                             join ma in db.Makes on v.makeID equals ma.makeID
                             join ms in db.ModelStyles on m.modelID equals ms.modelID
                             where ma.makeID.Equals(makeID) && y.yearID.Equals(yearID)
                             select m).Distinct().OrderBy(x => x.model1).ToList<Model>();
            }
            return models;
        }

        public static List<Style> GetStyles(int modelID = 0) {
            List<Style> styles = new List<Style>();
            CurtDevDataContext db = new CurtDevDataContext();
            if (modelID > 0) {
                styles = (from s in db.Styles
                          join ms in db.ModelStyles on s.styleID equals ms.styleID
                          where ms.modelID.Equals(modelID)
                          select s).Distinct().OrderBy(x => x.style1).ToList<Style>();
            } else {
                styles = (from s in db.Styles
                          select s).Distinct().OrderBy(x => x.style1).ToList<Style>();
            }

            return styles;
        }

        public static List<Style> GetStylesByYearMakeModel(int yearID = 0, int makeID = 0, int modelID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<Style> styles = new List<Style>();

            if (yearID > 0 && makeID > 0 && modelID > 0) {
                styles = (from s in db.Styles
                             join v in db.Vehicles on s.styleID equals v.styleID
                             join y in db.Years on v.yearID equals y.yearID
                             join ma in db.Makes on v.makeID equals ma.makeID
                             join mo in db.Models on v.modelID equals mo.modelID
                             where y.yearID.Equals(yearID) && ma.makeID.Equals(makeID) && mo.modelID.Equals(modelID)
                             select s).Distinct().OrderBy(x => x.style1).ToList<Style>();
            }
            return styles;
        }
        
        public static Vehicles GetVehicle(int vehicleID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            Vehicles vehicle = new Vehicles();
            
            if (vehicleID > 0) {
                vehicle = (from v in db.Vehicles
                           where v.vehicleID.Equals(vehicleID)
                           select v).SingleOrDefault<Vehicles>();
            }
            return vehicle;
        }

        public static int GetVehicleID(int yearID = 0, int makeID = 0, int modelID = 0, int styleID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            Vehicles vehicle = new Vehicles();
            int vehicleID = 0;

            if (yearID > 0 && makeID > 0 && modelID > 0 && styleID > 0) {
                try {
                    vehicleID = (from v in db.Vehicles
                               where v.yearID.Equals(yearID) && v.makeID.Equals(makeID) && v.modelID.Equals(modelID) && v.styleID.Equals(styleID)
                               select v.vehicleID).First();
                } catch { }
            }
            return vehicleID;
        }

        public static FullVehicle GetFullVehicle(int vehicleID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            FullVehicle vehicle = new FullVehicle();

            if (vehicleID > 0) {
                vehicle = (from v in db.Vehicles
                           join y in db.Years on v.yearID equals y.yearID
                           join ma in db.Makes on v.makeID equals ma.makeID
                           join mo in db.Models on v.modelID equals mo.modelID
                           join s in db.Styles on v.styleID equals s.styleID
                           where v.vehicleID.Equals(vehicleID)
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
                           }).FirstOrDefault<FullVehicle>();
            }
            return vehicle;
        }

        public static List<ConvertedPart> GetVehicleParts(int vehicleID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();

            List<ConvertedPart> parts = new List<ConvertedPart>();

            parts = (from p in db.Parts
                        join vp in db.VehicleParts on p.partID equals vp.partID
                        where vp.vehicleID.Equals(vehicleID)
                        select new ConvertedPart {
                            partID = p.partID,
                            status = p.status,
                            dateModified = Convert.ToDateTime(p.dateModified).ToString(),
                            dateAdded = Convert.ToDateTime(p.dateAdded).ToString(),
                            shortDesc = p.shortDesc,
                            oldPartNumber = p.oldPartNumber,
                            priceCode = Convert.ToInt32(p.priceCode),
                            pClass = p.classID,
                            listPrice = String.Format("{0:C}", (from prices in db.Prices
                                                                where prices.partID.Equals(p.partID) && prices.priceType.Equals("List")
                                                                select prices.price1 != null ? prices.price1 : (decimal?)0).SingleOrDefault<decimal?>())
                        }).ToList<ConvertedPart>();
            
            return parts;
        }

        public static List<Price> GetPartPricing(int partID = 0) {
            List<Price> prices = new List<Price>();
            CurtDevDataContext db = new CurtDevDataContext();

            prices = (from p in db.Prices
                      where p.partID.Equals(partID)
                      select p).ToList<Price>();
            return prices;
        }
    }


}
