using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Text;

namespace CurtAdmin.Controllers
{
    public class APIController : Controller
    {
        /* Version 2.0 of CURT Manufacturing eCommerce Data API */

        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// Get all the years in the database
        /// </summary>
        /// <param name="dataType">Response Format [XML,JSON,JSONP]</param>
        /// <param name="callback">If dataType equals JSONP, we use callback string as function name</param>
        [AcceptVerbs(HttpVerbs.Get)]
        public void GetYear(string dataType = "", string callback = ""){
            Logger.LogMessageToFile("Getting Year, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") {
                string yearJSON = APIModel.GetYearsJSON();
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + yearJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(yearJSON);
                }
                Response.End();
            } else {
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetYearsXML());
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Year, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        /// <summary>
        /// Get all the makes, or if a year is passed in, get all the makes for that year
        /// </summary>
        /// <param name="year">Year to retrieve makes for. [Optional]</param>
        /// <param name="dataType">Response Format [XML,JSON,JSONP]</param>
        /// <param name="callback">If dataType equals JSONP, we will use callback string as function name.</param>
        [AcceptVerbs(HttpVerbs.Get)]
        public void GetMake(double year = 0,string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Get Make, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") {
                string makeJSON = APIModel.GetMakesJSON(year);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + makeJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(makeJSON);
                }
                Response.End();
            } else {
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetMakesXML(year));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Make, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        /// <summary>
        /// Get all the models, or if a year and make are passed in, get the all models for that year/make
        /// </summary>
        /// <param name="year">Year to retrieve models for. [Optional]</param>
        /// <param name="make">Make to retrieve models for. [Optional]</param>
        /// <param name="dataType">Response Format [XML,JSON,JSONP]</param>
        /// <param name="callback">If dataType equals JSONP, we will use callback string as function name.</param>
        [AcceptVerbs(HttpVerbs.Get)]
        public void GetModel(double year = 0, string make = "", string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Model, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") {
                string modelJSON = APIModel.GetModelsJSON(year,make);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + modelJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(modelJSON);
                }
                Response.End();
            } else {
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetModelsXML(year,make));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Model, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        /// <summary>
        /// Get all the styles, or if a year, make, model are passed in, get the models for that year/make/model.
        /// </summary>
        /// <param name="year">Year to retrieve styles for. [Optional]</param>
        /// <param name="make">Make to retrieve styles for. [Optional]</param>
        /// <param name="model">Model to retrieve styles for. [Optional]</param>
        /// <param name="dataType">Response Format. [XML,JSON,JSONP]</param>
        /// <param name="callback">If dataType equals JSONP, we will use callback string as function name.</param>
        [AcceptVerbs(HttpVerbs.Get)]
        public void GetStyle(double year = 0, string make = "", string model = "", string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Style, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") {
                string styleJSON = APIModel.GetStylesJSON(year, make, model);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + styleJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(styleJSON);
                }
                Response.End();
            } else {
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetStylesXML(year, make, model));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Style, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        /// <summary>
        /// Get all the vehicles records, or if a year, make, model, and style are passed in, get the vehicles for that year/make/model/style.
        /// </summary>
        /// <param name="year">Year of the vehicle. [Optional]</param>
        /// <param name="make">Make of the vehicle. [Optional]</param>
        /// <param name="model">Model of the vehicle. [Optional]</param>
        /// <param name="style">Style of the vehicle. [Optional]</param>
        /// <param name="dataType">Response Format. [XML,JSON,JSONP]</param>
        /// <param name="callback">If dataType equals JSONP, we will use callback string as function name.</param>
        [AcceptVerbs(HttpVerbs.Get)]
        public void GetVehicle(double year = 0, string make = "", string model = "", string style = "", string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Vehicle, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") {
                string vehicleJSON = APIModel.GetVehiclesJSON(year, make, model, style);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + vehicleJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(vehicleJSON);
                }
                Response.End();
            } else {
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetVehiclesXML(year, make, model, style));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Vehicle, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        /// <summary>
        /// Returns the vehicles that fit the given part ID
        /// </summary>
        /// <param name="partID">ID of the part to find vehicles for.</param>
        /// <param name="dataType">Return data type</param>
        /// <param name="callback">If data type is JSONP, wrap in function</param>
        public void GetPartVehicles(int partID = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Vehicle Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string vehicleJSON = APIModel.GetVehiclesByPartJSON(partID);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + vehicleJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(vehicleJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetVehiclesByPartXML(partID));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Vehicle Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        /// <summary>
        /// Get the parts for either the given vehicleID OR the year/make/model/style.
        /// </summary>
        /// <param name="vehicleID">ID of the vehicle to retrieve parts for. [Optional]</param>
        /// <param name="year">Year of the vehicle to retrieve parts for. [Optional]</param>
        /// <param name="make">Make of the vehicle to retrieve parts for. [Optional]</param>
        /// <param name="model">Model of the vehicle to retrieve parts for. [Optional]</param>
        /// <param name="style">Style of the vehicle to retrieve parts for. [Optional]</param>
        /// <param name="integrated">Integrate with customer part number. [Optional]</param>
        /// <param name="cust_id">Customer ID. [Optional]</param>
        /// <param name="dataType">Response Format. [XML,JSON,JSONP]</param>
        /// <param name="callback">IF dataType equals JSONP, we will use callback string as functio name.</param>
        [AcceptVerbs(HttpVerbs.Get)]
        public void GetParts(int vehicleID = 0, double year = 0, string make = "", string model = "", string style = "", string catName = "", bool integrated = false, int cust_id = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") {
                string partJSON = "";
                if (vehicleID != 0) {
                    if (catName.Length > 0){
                        partJSON = APIModel.GetPartsByVehicleIDWithFilterJSON(vehicleID,catName, integrated, cust_id);
                    } else{
                        partJSON = APIModel.GetPartsByVehicleIDJSON(vehicleID, integrated, cust_id);
                    }
                } else {
                    if (catName.Length > 0) {
                        partJSON = APIModel.GetPartsWithFilterJSON(year, make, model, style, catName, integrated, cust_id);
                    } else {
                        partJSON = APIModel.GetPartsJSON(year, make, model, style, integrated, cust_id);
                    }
                }
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + partJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(partJSON);
                }
                Response.End();
            } else {
                Response.ContentType = "text/xml";
                if (vehicleID != 0) {
                    if (catName.Length > 0) {
                        Response.Write(APIModel.GetPartsByVehicleIDWithFilterXML(vehicleID,catName, integrated, cust_id));
                    } else {
                        Response.Write(APIModel.GetPartsByVehicleIDXML(vehicleID, integrated, cust_id));
                    }
                } else {
                    if (catName.Length > 0) {
                        Response.Write(APIModel.GetPartsWithFilterXML(year, make, model, style,catName, integrated, cust_id));
                    } else {
                        Response.Write(APIModel.GetPartsXML(year, make, model, style, integrated, cust_id));
                    }
                }
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        /// <summary>
        /// Retrieve part information for a given partID
        /// </summary>
        /// <param name="partID">ID of the part to retrieve</param>
        /// <param name="dataType">Response Format [XML,JSON,JSONP]</param>
        /// <param name="callback">If the dataType is equal to JSONP we will use the callback string as the returned function name.</param>
        [AcceptVerbs(HttpVerbs.Get)]
        public void GetPart(int partID = 0, int vehicleID = 0, double year = 0, string make = "", string model = "", string style = "", bool integrated = false, int cust_id = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Part, Requester: " + Request.ServerVariables["HTTP_REFERER"]);

            // Validate the partID
            if (partID == 0) {
                Response.ContentType = "application/json";
                Response.Write("Invalid partID");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string partJSON = "";
                if (partID > 0) {
                    if (vehicleID > 0) {
                        partJSON = APIModel.GetPartByVehicleIDJSON(partID, vehicleID, integrated, cust_id);
                    } else if (year > 0 && make != "" && model != "" && style != "") {
                        partJSON = APIModel.GetPartWithFilterJSON(partID, year, make, model, style, integrated, cust_id);
                    } else {
                        partJSON = APIModel.GetPartJSON(partID, integrated, cust_id);
                    }
                }
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + partJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(partJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                if (partID > 0) {
                    if (vehicleID > 0) {
                        Response.Write(APIModel.GetPartByVehicleIDXML(partID, vehicleID, integrated, cust_id));
                    } else if (year > 0 && make != "" && model != "" && style != "") {
                        Response.Write(APIModel.GetPartWithFilterXML(partID, year, make, model, style, integrated, cust_id));
                    } else {
                        Response.Write(APIModel.GetPartXML(partID, integrated, cust_id));
                    }
                }
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Part, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public void GetAllParts(string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting All Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);

            if (dataType.ToUpper() == "JSON") { // Display JSON
                string partJSON = "";
                partJSON = APIModel.GetAllPartsJSON();
                Response.ContentType = "application/json";
                Response.Write(partJSON);
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetAllPartsXML());
                Response.End();
            }
            Logger.LogMessageToFile("Retreived All Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public void GetAllPartID(string dataType = "", string callback = "") {
            if (dataType.ToUpper().Equals("JSON")) {
                Response.ContentType = "application/json";
                Response.Write(APIModel.GetAllPartIDJSON());
                Response.End();
            } else if (dataType.ToUpper().Equals("JSONP")) {
                Response.ContentType = "application/x-javascript";
                Response.Write(callback + "(" + APIModel.GetAllPartIDJSON() + ")");
                Response.End();
            } else {
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetAllPartIDXML());
                Response.End();
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public void GetRelatedParts(int partID = 0, string dataType = "", string callback = "", bool widget = false, bool integrated = false, int cust_id = 0) {
            Logger.LogMessageToFile("Getting Related Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            // Validate the partID
            if (partID == 0) {
                Response.ContentType = "application/json";
                Response.Write("Invalid partID");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string partJSON = "";
                if (partID > 0) {
                    partJSON = APIModel.GetRelatedJSON(partID,integrated, cust_id);
                }
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    if (!widget) {
                        Response.Write(callback + "(" + partJSON + ")");
                    }else{
                        Response.Write(callback + "(" + partID + "," + partJSON + ")");
                    }
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(partJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                if (partID > 0) {
                    Response.Write(APIModel.GetRelatedXML(partID, integrated, cust_id));
                }
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Related Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public void GetLatestParts(int count = 5, string dataType = "", string callback = "", bool integrated = false, int cust_id = 0) {
            Logger.LogMessageToFile("Getting Latest Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string partsJSON = "";
                partsJSON = APIModel.GetLatestPartsJSON(integrated, cust_id, 6);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + partsJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(partsJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetLatestPartsXML(integrated, cust_id, 6));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Latest Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetSPGridData(int partID = 0, string dataType = "",string callback = "") {
            // Validate the partID
            if (partID == 0) {
                Response.ContentType = "application/json";
                Response.Write("Invalid partID");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string gridJSON = "";
                if (partID > 0) {
                    gridJSON = APIModel.GetSPGridJSON(partID);
                }
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + gridJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(gridJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                if (partID > 0) {
                    Response.Write(APIModel.GetSPGridXML(partID));
                }
                Response.End();
            }
        }

        public void GetPartsByDateModified(string date = "", string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Parts By Date Modified, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            // Validate the partID
            if (date.Length == 0) {
                Response.ContentType = "application/json";
                Response.Write("You must enter a valid Date");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string gridJSON = APIModel.GetPartsByDateModifiedJSON(date);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + gridJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(gridJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetPartsByDateModifiedXML(date));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Parts By Date Modified, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetAttributes(int partID = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Attributes, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            // Validate the partID
            if (partID == 0) {
                Response.ContentType = "application/json";
                Response.Write("Invalid Part #");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string attrJSON = APIModel.GetAttributesJSON(partID);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + attrJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(attrJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetAttributesXML(partID));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Attributes, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetReviewsByPart(int partID = 0, int page = 1, int perPage = 10, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Reviews By Part, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            // Validate the partID
            if (partID == 0) {
                Response.ContentType = "application/json";
                Response.Write("Invalid Part #");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string attrJSON = APIModel.GetReviewsByPartJSON(partID,page,perPage);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + attrJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(attrJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetReviewsByPartXML(partID,page,perPage));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Reviews By Part, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void SubmitReview(int partID = 0, int cust_id = 0, string name = "", string email = "", int rating = 0, string subject = "", string review_text = "") {
            Logger.LogMessageToFile("Submitting Review, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            // Validate the data
            if (partID == 0 || cust_id == 0 || subject == "" || rating == 0 || review_text == "") {
                Response.ContentType = "application/json";
                Response.Write("Invalid Data Submitted");
                Response.Write("partID, cust_id, subject, rating, and review_text are required");
                Response.End();
            } else {
                APIModel.SubmitReview(partID, cust_id, name, email, rating, subject, review_text);
                Response.ContentType = "application/json";
                Response.Write("success");
                Response.End();
            }
            Logger.LogMessageToFile("Submitted Review, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetParentCategories(string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Parent Categories, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string catJSON = APIModel.GetParentCategoriesJSON();
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + catJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(catJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetParentCategoriesXML());
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Parent Categories, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetCategories(int parentID = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Categories, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string catJSON = APIModel.GetCategoriesJSON(parentID);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + catJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(catJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetCategoriesXML(parentID));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Categories, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetCategory(int catID = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Category, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (catID == 0) {
                Response.ContentType = "text/plain";
                Response.Write("Invalid category ID.");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string catJSON = APIModel.GetCategoryJSON(catID);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + catJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(catJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetCategoryXML(catID));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Category, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetCategoryByName(string catName = "", string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Category By Name, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (catName.Length == 0) {
                Response.ContentType = "text/plain";
                Response.Write("Invalid category name.");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string catJSON = APIModel.GetCategoryByNameJSON(catName);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + catJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(catJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetCategoryByNameXML(catName));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Category By Name, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetCategoryBreadCrumbs(int catId = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Category Breadcrumbs, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (catId == 0) {
                Response.ContentType = "text/plain";
                Response.Write("Invalid category id.");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string catJSON = APIModel.GetCategoryBreadCrumbsJSON(catId);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + catJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(catJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetCategoryBreadCrumbsXML(catId));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Category Breadcrumbs, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetCategoryParts(int catID = 0, int cust_id = 0, bool integrated = false, string dataType = "", string callback = "", int page = 1, int perpage = 0) {
            Logger.LogMessageToFile("Getting Category Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string partsJSON = APIModel.GetCategoryPartsJSON(catID, cust_id, integrated, page, perpage);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + partsJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(partsJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetCategoryPartsXML(catID, cust_id, integrated, page, perpage));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Category Parts, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetCategoryPartsByName(string catName = "", int cust_id = 0, bool integrated = false, string dataType = "", string callback = "", int page = 1, int perpage = 0){
            Logger.LogMessageToFile("Getting Category Parts By Name, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (catName.Length == 0)
            {
                Response.ContentType = "text/plain";
                Response.Write("Invalid category.");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP")
            { // Display JSON
                string partsJSON = APIModel.GetCategoryPartsByNameJSON(catName, cust_id, integrated, page, perpage);
                if (dataType.ToUpper() == "JSONP")
                {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + partsJSON + ")");
                }
                else
                {
                    Response.ContentType = "application/json";
                    Response.Write(partsJSON);
                }
                Response.End();
            }
            else
            { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetCategoryPartsByNameXML(catName, cust_id, integrated, page, perpage));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Category Parts By Name, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetLifestyles(string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Lifestyles, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string catJSON = APIModel.GetLifestylesJSON();
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + catJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(catJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetLifestylesXML());
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Lifestyles, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetLifestyle(int lifestyleid = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Lifestyle, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string catJSON = APIModel.GetLifestyleJSON(lifestyleid);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + catJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(catJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetLifestyleXML(lifestyleid));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Lifestyle, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetConnector(int vehicleID = 0, double year = 0, string make = "", string model = "", string style = "", bool integrated = false, int cust_id = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Connector, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (vehicleID == 0) {
                // We're going to parse out the vehicle record using year make model style...
                if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                    string connectorJSON = APIModel.GetConnectorBySpecJSON(year,make,model,style,integrated,cust_id);
                    if (dataType.ToUpper() == "JSONP") {
                        Response.ContentType = "application/x-javascript";
                        Response.Write(callback + "(" + connectorJSON + ")");
                    } else {
                        Response.ContentType = "application/json";
                        Response.Write(connectorJSON);
                    }
                    Response.End();
                } else { // Display XML
                    Response.ContentType = "text/xml";
                    Response.Write(APIModel.GetConnectorBySpecXML(year,make,model,style, integrated, cust_id));
                    Response.End();
                }
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string connectorJSON = APIModel.GetConnectorJSON(vehicleID, integrated, cust_id);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + connectorJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(connectorJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetConnectorXML(vehicleID,integrated, cust_id));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Connector, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void Search(string search_term = "", string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Search, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (search_term.Length == 0) {
                Response.ContentType = "text/plain";
                Response.Write("");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string searchJSON = APIModel.GetSearch_JSON(search_term);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + searchJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(searchJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetSearch_XML(search_term));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Search, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void PowerSearch(string search_term = "", bool integrated = false, int customerID = 0, string dataType = "", string callback = "") {
            Logger.LogMessageToFile("Getting Power Search, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            if (search_term.Length == 0) {
                Response.ContentType = "text/plain";
                Response.Write("");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string searchJSON = APIModel.GetPowerSearch_JSON(search_term, integrated, customerID);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + searchJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(searchJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetPowerSearch_XML(search_term, integrated, customerID));
                Response.End();
            }
            Logger.LogMessageToFile("Retreived Power Search, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetDefaultPartImages(bool integrated = false, int cust_id = 0) {
            Logger.LogMessageToFile("Getting Default Part Images, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            Response.ContentType = "text/xml";
            Response.Write(APIModel.GetPartDefaultImages_XML(integrated,cust_id));
            Response.End();
            Logger.LogMessageToFile("Retreived Default Part Images, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetPartImagesByIndex(char index = 'a', bool integrated = false, int cust_id = 0) {
            Logger.LogMessageToFile("Getting Part Images By Index, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            Response.ContentType = "text/xml";
            Response.Write(APIModel.GetPartImagesByIndex_XML(index, integrated, cust_id));
            Response.End();
            Logger.LogMessageToFile("Retreived Part Images By Index, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetPartImages(int partID = 0, bool integrated = false, int cust_id = 0) {
            Logger.LogMessageToFile("Getting Part Images, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            Response.ContentType = "text/xml";
            Response.Write(APIModel.GetPartImages_XML(partID, integrated, cust_id));
            Response.End();
            Logger.LogMessageToFile("Retreived Part Images, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        public void GetPartImage(int partID = 0, char index = 'a', string size = "Grande") {
            Logger.LogMessageToFile("Getting Part Image, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
            Response.ContentType = "text";
            Response.Write(APIModel.GetPartImage(partID, index, size));
            Response.End();
            Logger.LogMessageToFile("Retreived Part Image, Requester: " + Request.ServerVariables["HTTP_REFERER"]);
        }

        /****** Kiosk API Calls *********/

        public void GetKioskHitches(int vehicleID = 0, int acctID = 0) {
            string partJSON = "";
            partJSON = APIModel.GetKioskHitches(vehicleID, acctID);
            Response.ContentType = "application/json";
            Response.Write(partJSON);
            Response.End();
        }

        public void GetRelatedKiosk(int partID = 0, int acctID = 0) {
            string partJSON = "";
            partJSON = APIModel.GetRelatedKiosk(partID, acctID);
            Response.ContentType = "application/json";
            Response.Write(partJSON);
            Response.End();
        }

        public void GetConnectorKiosk(int vehicleID = 0, int acctID = 0) {
            string partJSON = "";
            partJSON = APIModel.GetConnectorKiosk(vehicleID, acctID);
            Response.ContentType = "application/json";
            Response.Write(partJSON);
            Response.End();
        }

        public void GetOpenAccessories(int vehicleID = 0, int acctID = 0) {
            string partJSON = "";
            partJSON = APIModel.GetOpenAccessories(vehicleID, acctID);
            Response.ContentType = "application/json";
            Response.Write(partJSON);
            Response.End();
        }

        public void CheckAccount(int acctID = 0) {
            int resp = APIModel.CheckAccount(acctID);
            if (resp == 1) {
                Response.StatusCode = 200;
            } else {
                Response.StatusCode = 400;
                Response.TrySkipIisCustomErrors = true;
            }
        }

        public void GetColor(int partID = 0) {
            Response.ContentType = "application/json";
            Response.Write(APIModel.GetCategoryColor_Kiosk(partID));
            Response.End();
        }

        /****** End Kiosk Calls *********/


        /********** eLocal Calls *********/

        public void Search_eLocal(string search_term = "", string dataType = "", string callback = "") {
            if (search_term.Length == 0) {
                Response.ContentType = "text/plain";
                Response.Write("");
                Response.End();
            }

            if (dataType.ToUpper() == "JSON" || dataType.ToUpper() == "JSONP") { // Display JSON
                string searchJSON = APIModel.GetSearch_eLocalJSON(search_term);
                if (dataType.ToUpper() == "JSONP") {
                    Response.ContentType = "application/x-javascript";
                    Response.Write(callback + "(" + searchJSON + ")");
                } else {
                    Response.ContentType = "application/json";
                    Response.Write(searchJSON);
                }
                Response.End();
            } else { // Display XML
                Response.ContentType = "text/xml";
                Response.Write(APIModel.GetSearch_eLocalXML(search_term));
                Response.End();
            }
        }

        public void Auth_eLocal(int i = 0, string s = "") {
            Response.ContentType = "text/plain";
            Response.Write(APIModel.CheckAuth_eLocal(i, s));
            Response.End();
        }

        public void Auth_Login(string e, string p, string u) {
            Response.ContentType = "text/plain";
            Response.Write(APIModel.DoLogin_eLocal(e,p,u));
            Response.End();
        }

        public void Setup_eLocal(string e, string p, string url) {
            Response.ContentType = "text/plain";
            Response.Write(APIModel.Setup_eLocal(e, p, url));
            Response.End();
        }

        public void GetPricing_eLocal(int acctID) {
            Response.ContentType = "application/json";
            Response.Write(APIModel.GetPricing_eLocal(acctID));
            Response.End();
        }

        public void GetCustomerID(string email) {
            Response.ContentType = "text/plain";
            Response.Write(APIModel.GetCustomerID(email));
            Response.End();
        }

        /********* End eLocal ************/

        /**** Theisens Integration *****/

        public void TheisensProduct(string date_modified = "") {
            Response.ContentType = "text/xml";
            Response.Write(APIModel.GetTheisensByDate(date_modified));
            Response.End();
        }

        /***** End Theisens *****/

        /*** Hitch Widget Tracker ****/

        public void AddDeployment(string url = "") {
            Response.ContentType = "text/plain";
            Response.Write(APIModel.AddDeployment(url));
            Response.End();
        }

        /***** End Tracker **********/

        /**** Image Submission *********/
        public void AddImage(int partID = 0, char sort = ' ', string path = "", int height = 0, int width = 0, string size = "") {
            try {
                APIModel.AddImage(partID, sort, path, height, width, size);
                Response.StatusCode = 200;
            } catch (Exception) {
                Response.StatusCode = 500;
            }
        }
        /****** End Image ****************/

    } // End Class
} // End Namespace
