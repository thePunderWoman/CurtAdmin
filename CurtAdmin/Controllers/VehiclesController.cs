/*
 * Author       : Alex Ninneman
 * Created      : January 20, 2011
 * Description  : This controller holds all of the page/AJAX functions for editing with vehicles in the CURT system.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class VehiclesController : BaseController
    {

        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "Vehicles";
        }

        /// <summary>
        /// View all vehicles in the database in a tabular format.
        /// </summary>
        /// <returns>View of all Vehicles.</returns>
        public ActionResult Index()
        {

            List<FullVehicle> vehicles = Models.Vehicle.GetVehicles();
            ViewBag.vehicles = vehicles;


            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// This will allow the user to create a new vehicle, or ad/edit any of these fields.
        /// </summary>
        /// <returns>View of years, makes, models, and styles.</returns>
        public ActionResult Add() {

            // Get Years
            List<Year> years = Models.Vehicle.GetYears();
            ViewBag.years = years;

            // Get Makes
            List<Make> makes = Models.Vehicle.GetMakes();
            ViewBag.makes = makes;

            // Get Models
            List<Model> models = Models.Vehicle.GetModels();
            ViewBag.models = models;

            // Get Styles
            List<Style> styles = Models.Vehicle.GetStyles();
            ViewBag.styles = styles;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// This page will handle the submission of a new vehicle.
        /// </summary>
        /// <param name="year">Year of the vehicle</param>
        /// <param name="make">Make of the vehicle</param>
        /// <param name="model">Model of the vehicle</param>
        /// <param name="style">Style of the vehicle</param>
        /// <returns>Redirect on success::::Edit page with errors if we encounter an issue.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Add(int year = 0, int make = 0, int model = 0, int style = 0) {

            ViewBag.year = year;
            ViewBag.make = make;
            ViewBag.model = model;
            ViewBag.style = style;
            List<string> error_messages = new List<string>();

            if (year > 0 && make > 0 && model > 0 && style > 0) {    
                try {
                    CurtDevDataContext db = new CurtDevDataContext();

                    // Make sure we don't already have this vehicle
                    int vCount = (from v in db.Vehicles
                                  where v.yearID.Equals(year) && v.makeID.Equals(make) && v.modelID.Equals(model) && v.styleID.Equals(style)
                                  select v).Count<Vehicles>();

                    try {
                        YearMake ym = db.YearMakes.Where(x => x.yearID.Equals(year)).Where(x => x.makeID.Equals(make)).First<YearMake>();
                    } catch {
                        YearMake ym = new YearMake {
                            yearID = year,
                            makeID = make
                        };
                        db.YearMakes.InsertOnSubmit(ym);
                        db.SubmitChanges();
                    }

                    try {
                        MakeModel mm = db.MakeModels.Where(x => x.makeID.Equals(make)).Where(x => x.modelID.Equals(model)).First<MakeModel>();
                    } catch {
                        MakeModel mm = new MakeModel {
                            makeID = make,
                            modelID = model
                        };
                        db.MakeModels.InsertOnSubmit(mm);
                        db.SubmitChanges();
                    }

                    try {
                        ModelStyle ms = db.ModelStyles.Where(x => x.modelID.Equals(model)).Where(x => x.styleID.Equals(style)).First<ModelStyle>();
                    } catch {
                        ModelStyle ms = new ModelStyle {
                            modelID = model,
                            styleID = style
                        };
                        db.ModelStyles.InsertOnSubmit(ms);
                        db.SubmitChanges();
                    } 
                    
                    if (vCount == 0) {
                        Vehicles new_vehicle = new Vehicles {
                            yearID = year,
                            makeID = make,
                            modelID = model,
                            styleID = style,
                            dateAdded = DateTime.Now
                        };
                        db.Vehicles.InsertOnSubmit(new_vehicle);
                        db.SubmitChanges();
                        return RedirectToAction("Index");
                    } else {
                        error_messages.Add("We already have this vehicle in the database.");
                    }

                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }
            ViewBag.error_messages = error_messages;

            // Get Years
            List<Year> years = Models.Vehicle.GetYears();
            ViewBag.years = years;

            // Get Makes
            List<Make> makes = Models.Vehicle.GetMakes();
            ViewBag.makes = makes;

            // Get Models
            List<Model> models = Models.Vehicle.GetModels();
            ViewBag.models = models;

            // Get Styles
            List<Style> styles = Models.Vehicle.GetStyles();
            ViewBag.styles = styles;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// Find any vehicles that match the given search criteria.
        /// </summary>
        /// <param name="vehicleStr">Search query</param>
        /// <returns>View of all the vehicles that were found.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult FindVehicle(string vehicleStr) {
            List<FullVehicle> vehicles = new List<FullVehicle>();
            string[] query_terms = new string[' '];
            query_terms = vehicleStr.Split(' ');
            foreach (string term in query_terms) {
                if (term.Trim().Length > 0) {
                    List<FullVehicle> returned_vehicles = Models.Vehicle.FindVehicles(term);
                    vehicles.AddRange(returned_vehicles);
                }
            }
            ViewBag.vehicles = vehicles;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View("Index");
        }

        /// <summary>
        /// Edit the fields for a specific vehicle.
        /// </summary>
        /// <param name="vehicleID">ID of a vehicle.</param>
        /// <returns>View that allows you to change all of the information associated with this vehicle.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(int vehicleID) {

            // Get the vehicle
            Vehicles vehicle = Models.Vehicle.GetVehicle(vehicleID);
            ViewBag.vehicle = vehicle;

            // Get the vehicles parts.
            List<ConvertedPart> parts = new List<ConvertedPart>();
            parts = Models.Vehicle.GetVehicleParts(vehicleID);
            ViewBag.parts = parts;

            // Get Years
            List<Year> years = Models.Vehicle.GetYears();
            ViewBag.years = years;

            // Get Makes
            List<Make> makes = Models.Vehicle.GetMakes();
            ViewBag.makes = makes;

            // Get Models
            List<Model> models = Models.Vehicle.GetModels();
            ViewBag.models = models;

            // Get Styles
            List<Style> styles = Models.Vehicle.GetStyles();
            ViewBag.styles = styles;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// Handles the form submission of the edit page.
        /// </summary>
        /// <param name="vehicleID">ID of the vehicle</param>
        /// <param name="year">Year of the vehicle</param>
        /// <param name="make">Make of the vehicle</param>
        /// <param name="model">Model of the vehicle</param>
        /// <param name="style">Style of the vehicle</param>
        /// <returns>Redirects to vehicle list on success::::Displays messages on error.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(int vehicleID, int year, int make, int model, int style) {
            CurtDevDataContext db = new CurtDevDataContext();
            Vehicles vehicle = (from v in db.Vehicles
                                where v.vehicleID.Equals(vehicleID)
                                select v).SingleOrDefault<Vehicles>();
            List<string> error_messages = new List<string>();

            if (vehicleID > 0 && year > 0 && make > 0 && model > 0 && style > 0) {
                try {
                    vehicle.yearID = year;
                    vehicle.makeID = make;
                    vehicle.modelID = model;
                    vehicle.styleID = style;
                    db.SubmitChanges();
                    return RedirectToAction("Index");
                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            } else {
                error_messages.Add("You did not select all fields.");
            }

            ViewBag.vehicle = vehicle;

            // Get Years
            List<Year> years = Models.Vehicle.GetYears();
            ViewBag.years = years;

            // Get Makes
            List<Make> makes = Models.Vehicle.GetMakes();
            ViewBag.makes = makes;

            // Get Models
            List<Model> models = Models.Vehicle.GetModels();
            ViewBag.models = models;

            // Get Styles
            List<Style> styles = Models.Vehicle.GetStyles();
            ViewBag.styles = styles;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /*** AJAX ***/

        /// <summary>
        /// Get the makes for a given year
        /// </summary>
        /// <param name="yearID">ID of a year</param>
        /// <returns>JSON containing makes.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string GetMakes(int yearID = 0) {
            string makes = "";

            List<Make> makeList = Models.Vehicle.GetMakes(yearID);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            makes = ser.Serialize(makeList);

            return makes;
        }

        /// <summary>
        /// Get the models for a given make
        /// </summary>
        /// <param name="makeID">ID of the make</param>
        /// <returns>JSON container Models</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string GetModels(int makeID = 0) {
            string models = "";

            List<Model> modelList = Models.Vehicle.GetModels(makeID);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            models = ser.Serialize(modelList);

            return models;
        }


        /// <summary>
        /// Get the styles for a given model
        /// </summary>
        /// <param name="modelID">ID of a make</param>
        /// <returns>JSON containing styles</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string GetStyles(int modelID = 0) {
            string models = "";

            List<Style> modelList = Models.Vehicle.GetStyles(modelID);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            models = ser.Serialize(modelList);

            return models;
        }

        /// <summary>
        /// Adds a new year to the database.
        /// </summary>
        /// <param name="year">Year to be added.</param>
        /// <returns>Blank string on success:::String containing message on error.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string AddYear(double year) {
            string response = "";
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer ser = new JavaScriptSerializer();

            if (year > 0) {
                // Make sure this year doesn't exist
                int yCount = (from y in db.Years
                              where y.year1.Equals(year)
                              select y).Count<Year>();

                if (yCount == 0) {
                    try {
                        // Add year
                        Year new_year = new Year {
                            year1 = year
                        };
                        db.Years.InsertOnSubmit(new_year);
                        db.SubmitChanges();

                        
                        response = ser.Serialize(new_year);
                    } catch (Exception e) {
                        response = "[{\"error\":\"" + e.Message + "\"}]";
                    }
                } else {
                    response = "[{\"error\":\"This year already exists\"}]";
                }
            }
            return response;
        }

        /// <summary>
        /// Adds a new make to the database.
        /// </summary>
        /// <param name="make">Make to be added</param>
        /// <param name="yearID">ID of the year to associate it with.</param>
        /// <returns>JSON containing either the make's information or an error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string AddMake(string make, int yearID) {
            string response = "";
            make = Uri.UnescapeDataString(make);
            CurtDevDataContext db = new CurtDevDataContext();
            YearMake ym = new YearMake();

            if (make.Length > 0 && yearID > 0) {
                // Make sure this make / year combo doesn't exist
                int mCount = (from m in db.Makes
                              join y in db.YearMakes on m.makeID equals ym.makeID
                              where m.make1.Equals(make) && y.yearID.Equals(yearID)
                              select m).Count<Make>();
                if(mCount == 0) {
                    Make new_make = new Make();
                    try {
                        new_make = db.Makes.Where(x => x.make1.Equals(make)).First<Make>();
                    } catch {
                        new_make = new Make {
                            make1 = make.Trim()
                        };
                        db.Makes.InsertOnSubmit(new_make);
                        db.SubmitChanges();
                    }
                    
                    try {
                        ym = new YearMake {
                            makeID = new_make.makeID,
                            yearID = yearID
                        };
                        db.YearMakes.InsertOnSubmit(ym);
                        db.SubmitChanges();
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        response = ser.Serialize(new_make);
                    } catch (Exception e) {
                        response = "[{\"error\":\""+e.Message+"\"}]";
                    }
                } else {
                    response = "[{\"error\":\"This make already exists for this model year.\"}]";
                }
            }
            return response;

        }


        /// <summary>
        /// Add a given model to the database.
        /// </summary>
        /// <param name="model">Model to be added</param>
        /// <param name="makeID">ID of make to associate this model with.</param>
        /// <returns>JSON containing model info or error message</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string AddModel(string model, int makeID) {
            string response = "";
            model = Uri.UnescapeDataString(model);
            CurtDevDataContext db = new CurtDevDataContext();
            MakeModel mm = new MakeModel();

            if (model.Length > 0 && makeID > 0) {
                // Make sure this model doesn't exist
                int mCount = (from m in db.Models
                              join ma in db.MakeModels on m.modelID equals ma.modelID
                              where m.model1.Equals(model) && ma.makeID.Equals(makeID)
                              select m).Count<Model>();
                if (mCount == 0) {
                    Model new_model = new Model();
                    try {
                        new_model = db.Models.Where(x => x.model1.Equals(model)).First<Model>();
                    } catch {
                        new_model = new Model {
                            model1 = model.Trim()
                        };
                        db.Models.InsertOnSubmit(new_model);
                        db.SubmitChanges();
                    }
                    try {
                        mm = new MakeModel {
                            modelID = new_model.modelID,
                            makeID = makeID
                        };
                        db.MakeModels.InsertOnSubmit(mm);
                        db.SubmitChanges();
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        response = ser.Serialize(new_model);
                    } catch (Exception e) {
                        response = "[{\"error\":\"" + e.Message + "\"}]";
                    }

                } else {
                    response = "[{\"error\":\"This model already exists for this make.\"}]";
                }
            }
            return response;

        }

        /// <summary>
        /// Add a style to the database.
        /// </summary>
        /// <param name="style">Style to be added.</param>
        /// <param name="modelID">ID of the model to associate it with.</param>
        /// <returns>JSON containing added style or error message</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string AddStyle(string style,int modelID) {
            string response = "";
            style = Uri.UnescapeDataString(style);
            CurtDevDataContext db = new CurtDevDataContext();
            ModelStyle ms = new ModelStyle();

            if (style.Length > 0 && modelID > 0) {
                // Make sure this style doesn't exist
                int sCount = (from s in db.Styles
                              join mos in db.ModelStyles on s.styleID equals mos.styleID
                              where s.style1.Equals(style) && mos.modelID.Equals(modelID)
                              select s).Count<Style>();
                if (sCount == 0) {
                    Style new_style = new Style();
                    try {
                        new_style = db.Styles.Where(x => x.style1.Equals(style)).First<Style>();
                    } catch {
                        new_style = new Style {
                            style1 = style.Trim()
                        };
                        db.Styles.InsertOnSubmit(new_style);
                        db.SubmitChanges();
                    }
                    try {
                        ms = new ModelStyle {
                            styleID = new_style.styleID,
                            modelID = modelID
                        };
                        db.ModelStyles.InsertOnSubmit(ms);
                        db.SubmitChanges();
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        response = ser.Serialize(new_style);
                    } catch (Exception e) {
                        response = "[{\"error\":\"" + e.Message + "\"}]";
                    }
                } else {
                    response = "This style already exists for this model.";
                }
            }
            return response;

        }

        /// <summary>
        /// Update the year.
        /// </summary>
        /// <param name="year">New year value.</param>
        /// <param name="yearID">ID of the year to be updated.</param>
        /// <returns>JSON containing year or error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string EditYear(double year, int yearID) {
            string response = "";
            JavaScriptSerializer ser = new JavaScriptSerializer();
            if (year > 0 && yearID > 0) {
                try {
                    CurtDevDataContext db = new CurtDevDataContext();
                    Year y = (from years in db.Years
                              where years.yearID.Equals(yearID)
                              select years).SingleOrDefault<Year>();
                    y.year1 = year;
                    db.SubmitChanges();
                    response = ser.Serialize(y);
                } catch (Exception e) {
                    response = "[{\"error\":\"" + e.Message + "\"}]";
                }
            } else {
                response = "[{\"error\":\"Year was not valid.\"}]";
            }
            return response;
        }

        /// <summary>
        /// Update the make.
        /// </summary>
        /// <param name="make">New make value.</param>
        /// <param name="makeID">ID of the make to be updated.</param>
        /// <returns>JSON containing make or error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string EditMake(string make, int makeID) {
            string response = "";
            JavaScriptSerializer ser = new JavaScriptSerializer();
            if (make.Length > 0 && makeID > 0) {
                try {
                    CurtDevDataContext db = new CurtDevDataContext();
                    Make m = (from makes in db.Makes
                              where makes.makeID.Equals(makeID)
                              select makes).SingleOrDefault<Make>();
                    m.make1 = make.Trim();
                    db.SubmitChanges();
                    response = ser.Serialize(m);
                } catch (Exception e) {
                    response = "[{\"error\":\"" + e.Message + "\"}]";
                }
            } else {
                response = "[{\"error\":\"Year was not valid.\"}]";
            }
            return response;
        }

        /// <summary>
        /// Update the model.
        /// </summary>
        /// <param name="model">New model value.</param>
        /// <param name="modelID">ID of the model to be updated.</param>
        /// <returns>JSON containing model or error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string EditModel(string model, int modelID) {
            string response = "";
            JavaScriptSerializer ser = new JavaScriptSerializer();
            if (model.Length > 0 && modelID > 0) {
                try {
                    CurtDevDataContext db = new CurtDevDataContext();
                    Model m = (from models in db.Models
                              where models.modelID.Equals(modelID)
                              select models).SingleOrDefault<Model>();
                    m.model1 = model.Trim();
                    db.SubmitChanges();
                    response = ser.Serialize(m);
                } catch (Exception e) {
                    response = "[{\"error\":\"" + e.Message + "\"}]";
                }
            } else {
                response = "[{\"error\":\"Year was not valid.\"}]";
            }
            return response;
        }

        /// <summary>
        /// Update the style.
        /// </summary>
        /// <param name="style">New style value.</param>
        /// <param name="styleID">ID of the style to be updated.</param>
        /// <returns>JSON containing the style or error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string EditStyle(string style, int styleID) {
            string response = "";
            JavaScriptSerializer ser = new JavaScriptSerializer();
            if (style.Length > 0 && styleID > 0) {
                try {
                    CurtDevDataContext db = new CurtDevDataContext();
                    Style s = (from styles in db.Styles
                               where styles.styleID.Equals(styleID)
                               select styles).SingleOrDefault<Style>();
                    s.style1 = style.Trim();
                    db.SubmitChanges();
                    response = ser.Serialize(s);
                } catch (Exception e) {
                    response = "[{\"error\":\"" + e.Message + "\"}]";
                }
            } else {
                response = "[{\"error\":\"Year was not valid.\"}]";
            }
            return response;
        }

        /// <summary>
        /// Delete a vehicle
        /// </summary>
        /// <param name="vehicleID">ID of the vehicle to be deleted.</param>
        /// <returns>Blank string on success::::Message on error</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteVehicle(int vehicleID) {
            string response = "";
            if (vehicleID > 0) {
                try {
                    CurtDevDataContext db = new CurtDevDataContext();

                    Vehicles vehicle = (from v in db.Vehicles
                                 where v.vehicleID.Equals(vehicleID)
                                 select v).SingleOrDefault<Vehicles>();
                    int yearID = vehicle.yearID;
                    int makeID = vehicle.makeID;
                    int modelID = vehicle.modelID;
                    int styleID = vehicle.styleID;

                    db.Vehicles.DeleteOnSubmit(vehicle);
                    db.SubmitChanges();

                    // Check if there are any other vehicles with this year
                    int year_vehicles = (from v in db.Vehicles
                                            where v.yearID.Equals(yearID)
                                            select v).Count<Vehicles>();
                    if (year_vehicles == 0) { // Delete all references to this year
                        var year_makes = (from ym in db.YearMakes
                                          where ym.yearID.Equals(yearID)
                                          select ym);
                        db.YearMakes.DeleteAllOnSubmit<YearMake>(year_makes);

                        var years = (from y in db.Years
                                     where y.yearID.Equals(yearID)
                                     select y);
                        db.Years.DeleteAllOnSubmit<Year>(years);
                    }

                    // Check if there are any other vehicles with this make
                    int make_vehicles = (from vm in db.Vehicles
                                         where vm.makeID.Equals(makeID)
                                         select vm).Count<Vehicles>();
                    if (make_vehicles == 0) { // Delete all references to this year
                        var year_makes = (from ym in db.YearMakes
                                          where ym.makeID.Equals(makeID)
                                          select ym);
                        db.YearMakes.DeleteAllOnSubmit<YearMake>(year_makes);

                        var make_models = (from mm in db.MakeModels
                                           where mm.makeID.Equals(makeID)
                                           select mm);
                        db.MakeModels.DeleteAllOnSubmit<MakeModel>(make_models);

                        var makes = (from ma in db.Makes
                                     where ma.makeID.Equals(makeID)
                                     select ma);
                        db.Makes.DeleteAllOnSubmit<Make>(makes);
                    }

                    // Check if there are any other vehicles with this model
                    int model_vehicles = (from vm in db.Vehicles
                                         where vm.modelID.Equals(modelID)
                                         select vm).Count<Vehicles>();
                    if (model_vehicles == 0) { // Delete all references to this year
                        var make_models = (from mm in db.MakeModels
                                          where mm.modelID.Equals(modelID)
                                          select mm);
                        db.MakeModels.DeleteAllOnSubmit<MakeModel>(make_models);

                        var model_styles = (from ms in db.ModelStyles
                                           where ms.modelID.Equals(modelID)
                                           select ms);
                        db.ModelStyles.DeleteAllOnSubmit<ModelStyle>(model_styles);

                        var models = (from m in db.Models
                                     where m.modelID.Equals(modelID)
                                     select m);
                        db.Models.DeleteAllOnSubmit<Model>(models);
                    }

                    // Check if there are any other vehicles with this style
                    int style_vehicles = (from vm in db.Vehicles
                                          where vm.styleID.Equals(styleID)
                                          select vm).Count<Vehicles>();
                    if (style_vehicles == 0) { // Delete all references to this year

                        var model_styles = (from ms in db.ModelStyles
                                            where ms.styleID.Equals(styleID)
                                            select ms);
                        db.ModelStyles.DeleteAllOnSubmit<ModelStyle>(model_styles);

                        var styles = (from s in db.Styles
                                      where s.styleID.Equals(styleID)
                                      select s);
                        db.Styles.DeleteAllOnSubmit<Style>(styles);
                    }
                    db.SubmitChanges();

                } catch (Exception e) {
                    response = "[{\"error\":\"" + e.Message + "\"}]";
                }
            } else {
                response = "[{\"error\":\"Vehicle was not valid.\"}]";
            }
            return response;
        }

        /// <summary>
        /// Delete a year.
        /// </summary>
        /// <param name="yearID">ID of the year to be deleted.</param>
        /// <returns>Success: blank string ::: Error: error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteYear(double yearID) {
            string error = "";
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                Year y = (from years in db.Years
                          where years.yearID.Equals(yearID)
                          select years).SingleOrDefault<Year>();
                db.Years.DeleteOnSubmit(y);

                // Get the vehicles that are tied to this year
                List<Vehicles> vehicles = (from v in db.Vehicles
                                           where v.yearID.Equals(yearID)
                                           select v).ToList<Vehicles>();
                db.Vehicles.DeleteAllOnSubmit<Vehicles>(vehicles);

                // Get the YearMake objects that are tied to this yearID
                List<YearMake> yms = (from ym in db.YearMakes
                                      where ym.yearID.Equals(yearID)
                                      select ym).ToList<YearMake>();
                db.YearMakes.DeleteAllOnSubmit(yms);

                db.SubmitChanges();
            } catch (Exception e) {
                error = e.Message;
            }
            return error;
        }

        /// <summary>
        /// Delete a make.
        /// </summary>
        /// <param name="makeID">ID of the make to be deleted.</param>
        /// <returns>Success: blank string ::: Error: error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteMake(int makeID) {
            string error = "";
            if (makeID > 0) {
                try {
                    CurtDevDataContext db = new CurtDevDataContext();
                    Make make = (from m in db.Makes
                                 where m.makeID.Equals(makeID)
                                 select m).SingleOrDefault<Make>();
                    db.Makes.DeleteOnSubmit(make);

                    // Get the vehicles that have this as a make
                    List<Vehicles> vehicles = (from v in db.Vehicles
                                               where v.makeID.Equals(makeID)
                                               select v).ToList<Vehicles>();
                    db.Vehicles.DeleteAllOnSubmit<Vehicles>(vehicles);

                    // Delete the YearMakes for this makeID
                    List<YearMake> year_makes = (from ym in db.YearMakes
                                                 where ym.makeID.Equals(makeID)
                                                 select ym).ToList<YearMake>();
                    db.YearMakes.DeleteAllOnSubmit<YearMake>(year_makes);

                    // Delete the MakeModels for this makeID
                    List<MakeModel> make_models = (from mm in db.MakeModels
                                                   where mm.makeID.Equals(makeID)
                                                   select mm).ToList<MakeModel>();
                    db.MakeModels.DeleteAllOnSubmit(make_models);

                    db.SubmitChanges();
                } catch (Exception e) {
                    error = e.Message;
                }
            }
            return error;
        }

        /// <summary>
        /// Delete a model.
        /// </summary>
        /// <param name="modelID">ID of the model to be deleted.</param>
        /// <returns>Success: blank string ::: Error: error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteModel(int modelID) {
            string error = "";
            if (modelID > 0) {
                try {
                    CurtDevDataContext db = new CurtDevDataContext();

                    Model model = (from m in db.Models
                                   where m.modelID.Equals(modelID)
                                   select m).SingleOrDefault<Model>();
                    db.Models.DeleteOnSubmit(model);

                    // Get the vehicles that have this as a make
                    List<Vehicles> vehicles = (from v in db.Vehicles
                                               where v.modelID.Equals(modelID)
                                               select v).ToList<Vehicles>();
                    db.Vehicles.DeleteAllOnSubmit<Vehicles>(vehicles);

                    // Delete the MakeModels for this modelID
                    List<MakeModel> make_models = (from ym in db.MakeModels
                                                 where ym.modelID.Equals(modelID)
                                                 select ym).ToList<MakeModel>();
                    db.MakeModels.DeleteAllOnSubmit<MakeModel>(make_models);

                    // Delete the ModelStyles for this modelID
                    List<ModelStyle> model_styles = (from ms in db.ModelStyles
                                                     where ms.modelID.Equals(modelID)
                                                     select ms).ToList<ModelStyle>();
                    db.ModelStyles.DeleteAllOnSubmit<ModelStyle>(model_styles);
                    db.SubmitChanges();

                } catch (Exception e) {
                    error = e.Message;
                }
            }
            return error;
        }

        /// <summary>
        /// Delete a style.
        /// </summary>
        /// <param name="styleID">ID of the style to be deleted.</param>
        /// <returns>Success: blank string ::: Error: error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteStyle(int styleID) {
            string error = "";
            if (styleID > 0) {
                try {
                    CurtDevDataContext db = new CurtDevDataContext();

                    Style style = (from s in db.Styles
                                   where s.styleID.Equals(styleID)
                                   select s).SingleOrDefault<Style>();
                    db.Styles.DeleteOnSubmit(style);

                    // Get the vehicles that have this as a style
                    List<Vehicles> vehicles = (from v in db.Vehicles
                                               where v.styleID.Equals(styleID)
                                               select v).ToList<Vehicles>();
                    db.Vehicles.DeleteAllOnSubmit<Vehicles>(vehicles);

                    // Delete the MakeModels for this modelID
                    List<ModelStyle> model_styles = (from sm in db.ModelStyles
                                                   where sm.styleID.Equals(styleID)
                                                   select sm).ToList<ModelStyle>();
                    db.ModelStyles.DeleteAllOnSubmit<ModelStyle>(model_styles);
                    db.SubmitChanges();

                } catch (Exception e) {
                    error = e.Message;
                }
            }
            return error;
        }

        /// <summary>
        /// Get the parts for a given vehicle.
        /// </summary>
        /// <param name="vehicleID">ID of the vehicle.</param>
        /// <returns>JSON containing the parts.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string GetVehicleParts(int vehicleID) {
            string response = "";
            try {
                List<ConvertedPart> parts = new List<ConvertedPart>();

                parts = Models.Vehicle.GetVehicleParts(vehicleID);
                JavaScriptSerializer ser = new JavaScriptSerializer();
                response = ser.Serialize(parts);
            } catch (Exception e) {
                response = "{\"error\":\""+e.Message+"\"}";
            }
            return response;
        }

        /// <summary>
        /// Get a given vehicles information.
        /// </summary>
        /// <param name="vehicleID">ID of the vehicle</param>
        /// <returns>JSON containing vehicle information.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string GetVehicle(int vehicleID) {
            string response = "";

            CurtDevDataContext db = new CurtDevDataContext();

            if (vehicleID > 0) {
                var v = (from vehicles in db.Vehicles
                         where vehicles.vehicleID.Equals(vehicleID)
                         select vehicles).SingleOrDefault<Vehicles>();
                JavaScriptSerializer ser = new JavaScriptSerializer();
                response = ser.Serialize(v);
            }
            return response;

        }

        
    }
}
