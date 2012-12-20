using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;
using OfficeOpenXml;

namespace CurtAdmin.Controllers {
    public class CustomersController : BaseController {
        //
        // GET: /Customers/

        public ActionResult Index(string msg = "") {
            List<SalesRepresentative> salesreps = new SalesRepresentative().GetAll();
            ViewBag.salesreps = salesreps;
            ViewBag.msg = msg;
            return View();
        }

        public ActionResult ViewAll() {
            ViewBag.H2 = "All Customers";
            List<Customer> customers = new List<Customer>();
            customers = CustomerModel.GetAll();
            ViewBag.customers = customers;
            return View("ViewCustomers");
        }

        public ActionResult ViewEcommerce() {
            ViewBag.H2 = "Ecommerce Customers";
            List<Customer> customers = new List<Customer>();
            customers = CustomerModel.GetCustomersByType(1);
            ViewBag.customers = customers;
            return View("ViewCustomers");
        }

        public ActionResult ViewDealerInstaller() {
            ViewBag.H2 = "Ecommerce Customers";
            List<Customer> customers = new List<Customer>();
            customers = CustomerModel.GetCustomersByType(2);
            ViewBag.customers = customers;
            return View("ViewCustomers");
        }

        public ActionResult ViewRetailers() {
            ViewBag.H2 = "Ecommerce Customers";
            List<Customer> customers = new List<Customer>();
            customers = CustomerModel.GetCustomersByType(3);
            ViewBag.customers = customers;
            return View("ViewCustomers");
        }

        public ActionResult ViewOther() {
            ViewBag.H2 = "Ecommerce Customers";
            List<Customer> customers = new List<Customer>();
            customers = CustomerModel.GetCustomersByType(4);
            ViewBag.customers = customers;
            return View("ViewCustomers");
        }

        public ActionResult ViewBySalesRep(int salesRepID = 0) {
            SalesRepresentative rep = new SalesRepresentative().Get(salesRepID);
            ViewBag.H2 = rep.name + "'s Customers";
            List<Customer> customers = new List<Customer>();
            customers = CustomerModel.GetCustomersByRep(salesRepID);
            ViewBag.customers = customers;
            return View("ViewCustomers");
        }




        public ActionResult ViewCustomerUsers()
        {
            // Get a list of all the customer users in the database
            List<CustomerUser> users = CustomerUser.GetAll();
            ViewBag.users = users;

            return View();
        }

        public ActionResult ViewCustomersOwnUsers(int cust_id)
        {
            if (cust_id != 0)
            {
                try
                {
                    Customer customer = CustomerModel.Get(cust_id);
                    List<CustomerUser> users = customer.CustomerUsers.ToList<CustomerUser>();
                    ViewBag.users = users;
                }
                catch (Exception e)
                {
                    ViewBag.error = e.Message;
                }
            }
            else
            {
                ViewBag.error = "That cust_id doesnt exist.";
            }
            return View("ViewCustomerUsers");
        }

        public ActionResult EditCustomerUser(Guid user_id)
        {
            // edits a particular customer user.
            CustomerUser user = new CustomerUser();
            user = user.Get(user_id);
            ViewBag.user = user;

            return View();
        }

        [HttpPost]
        public ActionResult EditCustomerUser(Guid user_id, string name, string email, string customerID, string isActive, string notCustomer, string cust_id )
        {
            ViewBag.error = "";
            ViewBag.msg = "";
            Boolean blnIsActive = false;
            blnIsActive = (isActive == "on") ? true : false;
            Boolean blnNotCustomer = false;
            blnNotCustomer = (notCustomer == "on") ? true : false;
            if (user_id.ToString().Length > 0)
            {
                if (name != "" && email != "" && customerID != "" && cust_id != "") // 0 is an acceptable value for customerID and cust_id since we use that temporarely for non customer customer users.
                {
                    try
                    {
                        // save results
                        CurtDevDataContext db = new CurtDevDataContext();
                        CustomerUser user = db.CustomerUsers.Where(x => x.id.ToString() == user_id.ToString()).FirstOrDefault<CustomerUser>();
                        user.name = name;
                        user.email = email;
                        user.customerID = Convert.ToInt32(customerID);
                        user.active = blnIsActive;
                        user.notCustomer = blnNotCustomer;
                        user.cust_id = Convert.ToInt32(cust_id);

                        db.SubmitChanges();

                        ViewBag.user = user;
                        ViewBag.msg = "Your changes have been saved.";

                    }
                    catch (Exception e)
                    {
                        ViewBag.error = e.Message;
                    }
                }
                else
                {
                    ViewBag.error = "Please make sure none of the fields are left blank. CustomerID and cust_id can be set to 0.";
                }
            }
            else
            {

                ViewBag.error = "Please specify a correct ID.";
            }
            return View();
        }

        /// <summary>
        /// Removes the given Customer user from the database.
        /// </summary>
        /// <param name="userID">Primary Key to identify the user.</param>
        /// <returns>Blank string on success::::Error message if an issue is encountered.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string RemoveCustomerUser(Guid userID)
        {
            CustomerUser u = new CustomerUser().Get(userID);
            u.Delete();
            return "";
        }

        /// <summary>
        /// Update the entered user's record to be either active or inactive.
        /// </summary>
        /// <param name="userID">Primary Key of user.</param>
        /// <returns>String representing the success/errors encountered.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string SetCustomerUserStatus(Guid userID)
        {
            CustomerUser u = new CustomerUser().Get(userID);
            u.activate();
            return "";
        }

        public ActionResult ViewWebProperties()
        {
            CurtDevDataContext db = new CurtDevDataContext();
            List<WebProperty> webProperties = new List<WebProperty>();
            webProperties = db.WebProperties.ToList<WebProperty>();
            ViewBag.webProperties = webProperties;
            
            return View();
        }
        public ActionResult ViewUserWebProperties(Guid userID)
        {
            CurtDevDataContext db = new CurtDevDataContext();
            List<WebProperty> webProperties = new List<WebProperty>();
            CustomerUser user = new CustomerUser();
            user = user.Get(userID);

            List<int> listOfWebPropIDs = db.CustUserWebProperties.Where(x => x.userID.Equals(userID)).Select(x => x.webPropID).ToList<int>();

            foreach (int webPropID in listOfWebPropIDs)
            {
                WebProperty webProp = db.WebProperties.Where(x => x.id == webPropID).FirstOrDefault<WebProperty>();
                if (webProp != null)
                {
                    webProperties.Add(webProp);
                }
            }// end foreach
            ViewBag.user = user;
            ViewBag.webProperties = webProperties;
            return View();
        }

        [HttpGet]
        public string SetWebPropertyStatus(int record_id)
        {
            try 
	        {	        
		         CurtDevDataContext db = new CurtDevDataContext();

                    WebProperty wp = db.WebProperties.Where(x => x.id == record_id).FirstOrDefault<WebProperty>();
                    if (wp.isEnabled == false)
                    {
                        wp.isEnabled = true;
                        wp.isEnabledDate = DateTime.Now;
                        try
                        {
                            string subject = "The following Web Property has PROVISIONAL AUTHORIZATION to use the CURT Authorized Dealer badge.";
                            string htmlBody = "<p>The following Web Property has PROVISIONAL AUTHORIZATION to use the CURT Authorized Dealer badge. This provisional authorization is subject to a CURT review, within 90 days, to determine if the web property adheres to all of the requirements of the CURT Authorized Dealer Policy. Please use this time to add the badge, review the Policy, and make adjustments to your web property as needed. Final approval is subject to complete compliance with all of the Policy criteria. Please note that each web property that you control, will be evaluated separately for authorization. You may only use the badge on the property for which it was authorized</p>";
                            htmlBody += "<hr />";
                            htmlBody += "<span>Name: <strong>" + wp.name + "</strong></span><br />";
                            htmlBody += "<span>Website Address: <strong>" + wp.url + "</strong></span><br />";
                            if (wp.typeID != 0)
                            {
                                htmlBody += "<span>Type: <strong>" + wp.WebPropertyTypes.type + "</strong></span><br />";
                            }
                            htmlBody += "<span>Seller ID: <strong>" + wp.sellerID + "</strong></span><br />";
                            htmlBody += "<hr /><br />";
                            htmlBody += "";
                            htmlBody += "<p>Please generate your badge code for your Web Property by clicking the link below:<br /><a href='http://dealers.curtmfg.com/AuthorizedDealer/WebProperties'>http://dealers.curtmfg.com/AuthorizedDealer/WebProperties</a></p>";
                            htmlBody += "<p>Once you have your Authorized Dealer Badge, please place it on your website, to build trust with your customers and reinforce your brand.</p>";
                            htmlBody += "<p><strong>Please respond to this email address when your web property is ready for the second final review.</strong> In that email please provide:</p>";
                            htmlBody += "<ul><ol>1. The url of the page on your web property that contains the badge.</ol><ol>2. The url of the page on your web property that contains the hyperlink to www.curtmfg.com.</ol></ul>";
                            htmlBody += "<p>If you are experiencing any problems adding the badge to your website, please contact dealers@curtmfg.com and we will be happy to help you.</p>";
                            helpers.SendEmail(wp.CustUserWebProperty.CustomerUser.email, subject, htmlBody, true);
                        }
                        catch (Exception e)
                        {
                            return e.Message;
                        }

                    }
                    else
                    {
                        wp.isEnabled = false;
                        db.SubmitChanges();
                        return "The Web Property has been updated.";
                    }
                    db.SubmitChanges();
                    // email user of property to let them know that it has been updated
                    return "";
	        }
	        catch (Exception e)
	        {
                return "Could not update record: " + e.Message;
	        }
        }

        [HttpGet]
        public string WPSetIsFinalApproved(int record_id)
        {
            try
            {
                CurtDevDataContext db = new CurtDevDataContext();

                WebProperty wp = db.WebProperties.Where(x => x.id == record_id).FirstOrDefault<WebProperty>();
                if (wp.isFinalApproved == false)
                {
                    wp.isFinalApproved = true;
                }
                else
                {
                    wp.isFinalApproved = false;
                    wp.isEnabledDate = null;
                    db.SubmitChanges();
                    return "The Web Property has been updated.";
                }
                db.SubmitChanges();
                return "";
            }
            catch (Exception e)
            {
                return "Could not update record: " + e.Message;
            }

        }


        [HttpGet]
        public string WPSetIsDenied(int record_id)
        {
            try
            {
                CurtDevDataContext db = new CurtDevDataContext();

                WebProperty wp = db.WebProperties.Where(x => x.id == record_id).FirstOrDefault<WebProperty>();
                if (wp.isDenied == false)
                {
                    wp.isDenied = true;
                }
                else
                {
                    wp.isDenied = false;
                    db.SubmitChanges();
                    return "The Web Property has been updated.";
                }
                db.SubmitChanges();
                return "";
            }
            catch (Exception e)
            {
                return "Could not update record: " + e.Message;
            }

        }



        public ActionResult Add() {
            string error = "";

            #region Form Submission
            try {
                if (Request.Form["btnSubmit"] != null) {

                    CurtDevDataContext db = new CurtDevDataContext();

                    // Save form values
                    string name = (Request.Form["name"] != null) ? Request.Form["name"] : "";
                    string email = (Request.Form["email"] != null) ? Request.Form["email"] : "";
                    string phone = (Request.Form["phone"] != null) ? Request.Form["phone"] : "";
                    string fax = (Request.Form["fax"] != null) ? Request.Form["fax"] : "";
                    string address = (Request.Form["address"] != null) ? Request.Form["address"] : "";
                    string address2 = (Request.Form["address2"] != null) ? Request.Form["address2"] : "";
                    string city = (Request.Form["city"] != null) ? Request.Form["city"] : "";
                    int stateID = (Request.Form["state"] != null) ? Convert.ToInt32(Request.Form["state"]) : 0;
                    string postalCode = (Request.Form["postal_code"] != null) ? Request.Form["postal_code"] : "";
                    string contact = (Request.Form["contact_person"] != null) ? Request.Form["contact_person"] : "";
                    string website = (Request.Form["website"] != null) ? Request.Form["website"] : "";
                    string eLocalURL = (Request.Form["eLocalURL"] != null) ? Request.Form["eLocalURL"] : "";
                    string searchURL = (Request.Form["searchURL"] != null) ? Request.Form["searchURL"] : "";
                    string logo = (Request.Form["logo"] != null && Request.Form["logo"].Trim() != "") ? Request.Form["logo"] : null;
                    int dealer_type = (Request.Form["dealer_type"] != null) ? Convert.ToInt32(Request.Form["dealer_type"]) : 0;
                    int customerID = (Request.Form["customerID"] != null && Request.Form["customerID"] != "") ? Convert.ToInt32(Request.Form["customerID"]) : 0;
                    int salesRepID = (Request.Form["salesRepID"] != null && Request.Form["salesRepID"] != "") ? Convert.ToInt32(Request.Form["salesRepID"]) : 0;
                    int mapixCodeID = (Request.Form["mapixCodeID"] != null && Request.Form["mapixCodeID"] != "") ? Convert.ToInt32(Request.Form["mapixCodeID"]) : 0;
                    int parentID = (Request.Form["parentID"] != null && Request.Form["parentID"] != "") ? Convert.ToInt32(Request.Form["parentID"]) : 0;
                    int tier = (Request.Form["tier"] != null && Request.Form["tier"] != "") ? Convert.ToInt32(Request.Form["tier"]) : 1;
                    bool isDummy = (Request.Form["isDummy"] != null && Request.Form["isDummy"] != "") ? Convert.ToBoolean(Request.Form["isDummy"]) : false;
                    bool showWebsite = (Request.Form["showWebsite"] != null && Request.Form["showWebsite"] != "") ? Convert.ToBoolean(Request.Form["showWebsite"]) : false;
                    LatLng location = new LatLng();

                    // Validate the form fields
                    if (name.Length == 0) throw new Exception("Name is required.");
                    if (customerID == 0 && parentID == 0) throw new Exception("Either a Customer ID or a Parent Customer is required.");
                    if (dealer_type == 0) throw new Exception("A Dealer Type is required.");

                    if (address != "" && city != "" && stateID != 0) {
                        GoogleMaps map = new GoogleMaps(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsKey"]);
                        location = map.GetLatLng(address, city, stateID);
                    }

                    // Create the new customer and save
                    Customer new_cust = new Customer {
                        name = name,
                        email = email,
                        phone = phone,
                        fax = fax,
                        address = address,
                        address2 = address2,
                        city = city,
                        stateID = stateID,
                        postal_code = postalCode,
                        contact_person = contact,
                        website = website,
                        eLocalURL = eLocalURL,
                        searchURL = searchURL,
                        logo = logo,
                        mCodeID = mapixCodeID,
                        salesRepID = salesRepID,
                        dealer_type = dealer_type,
                        isDummy = isDummy,
                        tier = tier,
                        showWebsite = showWebsite
                    };

                    if (location.latitude != null && location.longitude != null) {
                        new_cust.latitude = location.latitude;
                        new_cust.longitude = location.longitude;
                    }

                    if (customerID != 0) {
                        new_cust.customerID = customerID;
                    }

                    if (parentID != 0) {
                        new_cust.parentID = parentID;
                    }

                    db.Customers.InsertOnSubmit(new_cust);
                    db.SubmitChanges();
                    return RedirectToAction("Add");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            // Get the dealer tiers
            List<DealerTier> dealer_tiers = CustomerModel.GetDealerTiers();
            ViewBag.dealer_tiers = dealer_tiers;

            // Get the dealer types
            List<DealerType> dealer_types = CustomerModel.GetDealerTypes().Reverse<DealerType>().ToList<DealerType>();
            ViewBag.dealer_types = dealer_types;

            // Get the mapix codes
            List<MapixCode> mapix_codes = CustomerModel.GetMapixCodes();
            ViewBag.mapix_codes = mapix_codes;

            // Get the mapix codes
            List<SalesRepresentative> sales_reps = CustomerModel.GetSalesReps();
            ViewBag.sales_reps = sales_reps;

            // Get the states
            List<FullCountry> countries = CustomerModel.GetCountries();
            ViewBag.countries = countries;

            // Get the customer list
            List<Customer> customers = CustomerModel.GetAll();
            ViewBag.customers = customers;

            ViewBag.error = error;
            return View();
        }

        public ActionResult Edit(int id = 0) {
            string error = "";

            #region Form Submission
            try {
                if (Request.Form["btnSubmit"] != null) {

                    CurtDevDataContext db = new CurtDevDataContext();

                    // Save form values
                    string name = (Request.Form["name"] != null) ? Request.Form["name"] : "";
                    string email = (Request.Form["email"] != null) ? Request.Form["email"] : "";
                    string phone = (Request.Form["phone"] != null) ? Request.Form["phone"] : "";
                    string fax = (Request.Form["fax"] != null) ? Request.Form["fax"] : "";
                    string address = (Request.Form["address"] != null) ? Request.Form["address"] : "";
                    string address2 = (Request.Form["address2"] != null) ? Request.Form["address2"] : "";
                    string city = (Request.Form["city"] != null) ? Request.Form["city"] : "";
                    int stateID = (Request.Form["state"] != null) ? Convert.ToInt32(Request.Form["state"]) : 0;
                    string postalCode = (Request.Form["postal_code"] != null) ? Request.Form["postal_code"] : "";
                    string contact = (Request.Form["contact_person"] != null) ? Request.Form["contact_person"] : "";
                    string website = (Request.Form["website"] != null) ? Request.Form["website"] : "";
                    string eLocalURL = (Request.Form["eLocalURL"] != null) ? Request.Form["eLocalURL"] : "";
                    string searchURL = (Request.Form["searchURL"] != null) ? Request.Form["searchURL"] : "";
                    string logo = (Request.Form["logo"] != null && Request.Form["logo"].Trim() != "") ? Request.Form["logo"] : null;
                    int dealer_type = (Request.Form["dealer_type"] != null) ? Convert.ToInt32(Request.Form["dealer_type"]) : 0;
                    int customerID = (Request.Form["customerID"] != null && Request.Form["customerID"] != "") ? Convert.ToInt32(Request.Form["customerID"]) : 0;
                    int salesRepID = (Request.Form["salesRepID"] != null && Request.Form["salesRepID"] != "") ? Convert.ToInt32(Request.Form["salesRepID"]) : 0;
                    int mapixCodeID = (Request.Form["mapixCodeID"] != null && Request.Form["mapixCodeID"] != "") ? Convert.ToInt32(Request.Form["mapixCodeID"]) : 0;
                    int parentID = (Request.Form["parentID"] != null && Request.Form["parentID"] != "") ? Convert.ToInt32(Request.Form["parentID"]) : 0;
                    int tier = (Request.Form["tier"] != null && Request.Form["tier"] != "") ? Convert.ToInt32(Request.Form["tier"]) : 1;
                    bool isDummy = (Request.Form["isDummy"] != null && Request.Form["isDummy"] != "") ? Convert.ToBoolean(Request.Form["isDummy"]) : false;
                    bool showWebsite = (Request.Form["showWebsite"] != null && Request.Form["showWebsite"] != "") ? Convert.ToBoolean(Request.Form["showWebsite"]) : false;

                    // Validate the form fields
                    if (name.Length == 0) throw new Exception("Name is required.");
                    if (customerID == 0 && parentID == 0) throw new Exception("Either a Customer ID or a Parent Customer is required.");
                    if (dealer_type == 0) throw new Exception("A Dealer Type is required.");

                    Customer cust = (from c in db.Customers
                                     where c.cust_id.Equals(id)
                                     select c).FirstOrDefault<Customer>();

                    // Get the geographic position of this customer
                    GoogleMaps map = new GoogleMaps(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsKey"]);
                    LatLng location = map.GetLatLng(address, city, stateID);

                    // Save new values
                    cust.name = name;
                    cust.email = email;
                    cust.phone = phone;
                    cust.fax = fax;
                    cust.address = address;
                    cust.address2 = address2;
                    cust.city = city;
                    cust.stateID = stateID;
                    cust.postal_code = postalCode;
                    cust.contact_person = contact;
                    cust.website = website;
                    cust.eLocalURL = eLocalURL;
                    cust.searchURL = searchURL;
                    cust.logo = logo;
                    cust.dealer_type = dealer_type;
                    cust.mCodeID = mapixCodeID;
                    cust.salesRepID = salesRepID;
                    cust.latitude = location.latitude;
                    cust.longitude = location.longitude;
                    cust.isDummy = isDummy;
                    cust.tier = tier;
                    cust.showWebsite = showWebsite;

                    if (customerID != 0) {
                        cust.customerID = customerID;
                    }

                    if (parentID != 0) {
                        cust.parentID = parentID;
                    }

                    db.SubmitChanges();
                    return RedirectToAction("Index");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            // Get the customer for this id
            Customer customer = CustomerModel.Get(id);
            ViewBag.customer = customer;

            // Get the dealer tiers
            List<DealerTier> dealer_tiers = CustomerModel.GetDealerTiers();
            ViewBag.dealer_tiers = dealer_tiers;

            // Get the dealer types
            List<DealerType> dealer_types = CustomerModel.GetDealerTypes();
            ViewBag.dealer_types = dealer_types;

            // Get the mapix codes
            List<MapixCode> mapix_codes = CustomerModel.GetMapixCodes();
            ViewBag.mapix_codes = mapix_codes;

            // Get the mapix codes
            List<SalesRepresentative> sales_reps = CustomerModel.GetSalesReps();
            ViewBag.sales_reps = sales_reps;

            // Get the states
            List<FullCountry> countries = CustomerModel.GetCountries();
            ViewBag.countries = countries;

            // Get the customer list
            List<Customer> customers = CustomerModel.GetAll();
            ViewBag.customers = customers;

            ViewBag.error = error;
            return View();
        }

        public ActionResult Locations(int id = 0) {

            // Get the customer information
            Customer customer = CustomerModel.Get(id);
            ViewBag.customer = customer;

            // Get the locations for this customers stores
            List<CustomerLocation> locations = CustomerModel.GetLocations(id);
            ViewBag.locations = locations;

            return View();
        }

        /// <summary>
        /// Add a new customer location
        /// </summary>
        /// <param name="id">ID of the customer we're adding the location for.</param>
        /// <returns>View</returns>
        public ActionResult AddLocation(int id = 0) {
            string error = "";

            #region Form Submission
            try {
                if (Request.Form["btnSubmit"] != null) {

                    CurtDevDataContext db = new CurtDevDataContext();

                    // Save form values
                    string name = (Request.Form["name"] != null) ? Request.Form["name"] : "";
                    string email = (Request.Form["email"] != null) ? Request.Form["email"] : "";
                    string phone = (Request.Form["phone"] != null) ? Request.Form["phone"] : "";
                    string fax = (Request.Form["fax"] != null) ? Request.Form["fax"] : "";
                    string address = (Request.Form["address"] != null) ? Request.Form["address"] : "";
                    string city = (Request.Form["city"] != null) ? Request.Form["city"] : "";
                    string postalCode = (Request.Form["postalCode"] != null) ? Request.Form["postalCode"] : "";
                    int stateID = (Request.Form["state"] != null) ? Convert.ToInt32(Request.Form["state"]) : 0;

                    // Validate the form fields
                    if (name.Length == 0) throw new Exception("Name is required.");
                    if (phone.Length == 0) throw new Exception("Phone is required.");
                    if (address.Length == 0) throw new Exception("Address is required.");
                    if (city.Length == 0) throw new Exception("City is required.");
                    if (stateID == 0) throw new Exception("State is required.");
                    if (postalCode.Length == 0) throw new Exception("Postal Code is required.");

                    GoogleMaps map = new GoogleMaps(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsKey"]);
                    LatLng location = map.GetLatLng(address, city, stateID);

                    // Create the new customer and save
                    CustomerLocation new_location = new CustomerLocation {
                        name = name,
                        email = email,
                        phone = phone,
                        fax = fax,
                        address = address,
                        city = city,
                        stateID = stateID,
                        postalCode = postalCode,
                        latitude = Convert.ToDouble(location.latitude),
                        longitude = Convert.ToDouble(location.longitude),
                        cust_id = id
                    };

                    db.CustomerLocations.InsertOnSubmit(new_location);
                    db.SubmitChanges();
                    return RedirectToAction("Locations", new { id = id });
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            // Get the customer that we're adding the location to
            Customer customer = CustomerModel.Get(id);
            ViewBag.customer = customer;

            // Get the states
            List<FullCountry> countries = CustomerModel.GetCountries();
            ViewBag.countries = countries;

            ViewBag.error = error;
            return View();
        }

        /// <summary>
        /// Edit the information about a given location
        /// </summary>
        /// <param name="id">ID of location</param>
        /// <returns>View</returns>
        public ActionResult EditLocation(int id = 0) {
            string error = "";

            #region Form Submission
            try {
                if (Request.Form["btnSubmit"] != null) {

                    CurtDevDataContext db = new CurtDevDataContext();

                    // Save form values
                    string name = (Request.Form["name"] != null) ? Request.Form["name"] : "";
                    string email = (Request.Form["email"] != null) ? Request.Form["email"] : "";
                    string phone = (Request.Form["phone"] != null) ? Request.Form["phone"] : "";
                    string fax = (Request.Form["fax"] != null) ? Request.Form["fax"] : "";
                    string address = (Request.Form["address"] != null) ? Request.Form["address"] : "";
                    string city = (Request.Form["city"] != null) ? Request.Form["city"] : "";
                    string postalCode = (Request.Form["postalCode"] != null) ? Request.Form["postalCode"] : "";
                    double latitude = (Request.Form["latitude"] != null) ? Convert.ToDouble(Request.Form["latitude"]) : 0;
                    double longitude = (Request.Form["longitude"] != null) ? Convert.ToDouble(Request.Form["longitude"]) : 0;
                    int stateID = (Request.Form["state"] != null) ? Convert.ToInt32(Request.Form["state"]) : 0;

                    // Validate the form fields
                    if (name.Length == 0) throw new Exception("Name is required.");
                    if (phone.Length == 0) throw new Exception("Phone is required.");
                    if (address.Length == 0) throw new Exception("Address is required.");
                    if (city.Length == 0) throw new Exception("City is required.");
                    if (stateID == 0) throw new Exception("State is required.");
                    if (postalCode.Length == 0) throw new Exception("Postal Code is required.");


                    // Get the location
                    CustomerLocation updated_loc = (from cl in db.CustomerLocations
                                                    where cl.locationID.Equals(id)
                                                    select cl).FirstOrDefault<CustomerLocation>();

                    GoogleMaps map = new GoogleMaps(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsKey"]);
                    LatLng geo = map.GetLatLng(address, city, stateID);
                    if (geo.latitude != "0") {
                        updated_loc.latitude = Convert.ToDouble(geo.latitude);
                        updated_loc.longitude = Convert.ToDouble(geo.longitude);
                    } else {
                        updated_loc.latitude = latitude;
                        updated_loc.longitude = longitude;
                    }

                    // Create the new customer and save
                    updated_loc.name = name;
                    updated_loc.email = email;
                    updated_loc.phone = phone;
                    updated_loc.fax = fax;
                    updated_loc.address = address;
                    updated_loc.city = city;
                    updated_loc.postalCode = postalCode;
                    updated_loc.stateID = stateID;

                    db.SubmitChanges();
                    return RedirectToAction("Locations", new { id = updated_loc.cust_id });
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            // Get the information for this location
            CustomerLocation location = CustomerModel.GetLocation(id);
            ViewBag.location = location;

            // Get the customer record that we're adding the location to
            Customer customer = CustomerModel.Get(location.cust_id);
            ViewBag.customer = customer;

            // Get the states
            List<FullCountry> countries = CustomerModel.GetCountries();
            ViewBag.countries = countries;

            ViewBag.error = error;
            return View();
        }

        public ActionResult MassUpload(string msg = "", List<string> errors = null, string path = "") {
            ViewBag.msg = msg;
            ViewBag.path = path;
            ViewBag.errors = errors;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public dynamic MassLoad(HttpPostedFileBase dealers) {
            PartStates state = new PartStates();
            List<string> errors = new List<string>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                string dir_path = Server.MapPath("/Content/locator_csv");
                GoogleMaps map = new GoogleMaps(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsKey"]);

                DirectoryInfo dir_info = new DirectoryInfo(dir_path);
                int file_count = dir_info.GetFiles().Count();
                string file_path = Server.MapPath("/Content/locator_csv/" + (file_count + 1) + ".xlsx");
                dealers.SaveAs(file_path);
                FileInfo existingFile = new FileInfo(file_path);
                using (ExcelPackage package = new ExcelPackage(existingFile)) {
                    // get the first worksheet in the workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int row = 2; //The first row with data
                    while (worksheet.Cells[row, 2].Value != null) {
                        // col 1 = Customer ID Code
                        // col 2 = Customer Name
                        // col 3 = Website
                        // col 4 = Location Name
                        // col 5 = Contact First Name
                        // col 6 = Contact Last Name
                        // col 7 = phone
                        // col 8 = street
                        // col 9 = city
                        // col 10 = state
                        // col 11 = zip
                        // col 12 = sales rep
                        // col 13 = Dealer Tier
                        // col 14 = Dealer Type
                        // col 15 = Parent curt acct # (meaning a third party client)

                        try {
                            int acctnum = (worksheet.Cells[row, 1].Value != null) ? Convert.ToInt32(worksheet.Cells[row, 1].Value) : 0;

                            string rep = (worksheet.Cells[row, 12].Value != null) ? worksheet.Cells[row, 12].Value.ToString() : "";
                            string dtier = worksheet.Cells[row, 13].Value.ToString().Trim().ToLower();
                            string dtype = worksheet.Cells[row, 14].Value.ToString().Trim().ToLower();
                            string customername = (worksheet.Cells[row, 2].Value != null) ? worksheet.Cells[row, 2].Value.ToString() : "";
                            string locationname = (worksheet.Cells[row, 4].Value != null) ? worksheet.Cells[row, 4].Value.ToString() : "";
                            string contact = ((worksheet.Cells[row, 5].Value != null) ? worksheet.Cells[row, 5].Value.ToString() : "") + ((worksheet.Cells[row, 6].Value != null) ? " " + worksheet.Cells[row, 6].Value.ToString() : "");
                            string website = (worksheet.Cells[row, 3].Value != null) ? worksheet.Cells[row, 3].Value.ToString().Trim() : null;
                            string statestr = (worksheet.Cells[row, 10].Value != null) ? worksheet.Cells[row, 10].Value.ToString() : "";
                            string postalCode = (worksheet.Cells[row, 11].Value != null) ? worksheet.Cells[row, 11].Value.ToString() : "";
                            string address = (worksheet.Cells[row, 8].Value != null) ? worksheet.Cells[row, 8].Value.ToString() : "";
                            string city = (worksheet.Cells[row, 9].Value != null) ? worksheet.Cells[row, 9].Value.ToString() : "";
                            string phone = (worksheet.Cells[row, 7].Value != null) ? worksheet.Cells[row, 7].Value.ToString() : "";
                            string parentacct = (worksheet.Cells[row, 15].Value != null) ? worksheet.Cells[row, 15].Value.ToString() : "";
                            int stateid = db.PartStates.Where(x => x.abbr.ToLower().Trim() == statestr.ToLower().Trim()).Select(x => x.stateID).FirstOrDefault();

                            LatLng geo = map.GetLatLng(address, city, stateid);

                            double latitude = Convert.ToDouble(geo.latitude);
                            double longitude = Convert.ToDouble(geo.longitude);

                            SalesRepresentative salesrep = db.SalesRepresentatives.Where(x => x.name.ToLower().Trim().Equals(rep.Trim().ToLower())).FirstOrDefault<SalesRepresentative>();
                            DealerType type = db.DealerTypes.Where(x => x.type.Trim().ToLower() == dtype).First();
                            DealerTier tier = db.DealerTiers.Where(x => x.tier.Trim().ToLower() == dtier).First();
                            Customer customer = new Customer();
                            if (acctnum != 0) {
                                customer = (from c in db.Customers
                                            join d in db.DealerTypes on c.dealer_type equals d.dealer_type
                                            where c.customerID.Equals(acctnum) && d.online.Equals(type.online)
                                            select c).FirstOrDefault<Customer>();
                            } else {
                                customer = (from c in db.Customers
                                            join d in db.DealerTypes on c.dealer_type equals d.dealer_type
                                            where c.name.ToLower().Equals(customername.Trim().ToLower()) && d.online.Equals(type.online) && c.parentID.Equals(Convert.ToInt32(parentacct))
                                            select c).FirstOrDefault<Customer>();
                            }
                            if (customer == null) {
                                // new customer
                                Customer new_customer = new Customer {
                                    name = customername,
                                    contact_person = contact,
                                    tier = tier.ID,
                                    dealer_type = type.dealer_type,
                                };

                                if (salesrep != null && salesrep.salesRepID != 0) {
                                    new_customer.salesRepID = salesrep.salesRepID;
                                }

                                if (acctnum != 0) {
                                    new_customer.customerID = acctnum;
                                }

                                if (parentacct.Trim() != "") {
                                    new_customer.parentID = Convert.ToInt32(parentacct);
                                }

                                if (website != "") {
                                    new_customer.website = website;
                                }

                                db.Customers.InsertOnSubmit(new_customer);
                                db.SubmitChanges();

                                if (address != "" && city != "" && postalCode != "") {
                                    CustomerLocation location = new CustomerLocation {
                                        cust_id = new_customer.cust_id,
                                        name = locationname,
                                        address = address,
                                        city = city,
                                        postalCode = postalCode,
                                        phone = phone,
                                        contact_person = contact,
                                        latitude = latitude,
                                        longitude = longitude,
                                        isprimary = true
                                    };

                                    if (stateid != 0) {
                                        location.stateID = stateid;
                                    }

                                    db.CustomerLocations.InsertOnSubmit(location);
                                }
                                db.SubmitChanges();

                            } else {
                                if (parentacct.Trim() != "") {
                                    customer.parentID = Convert.ToInt32(parentacct);
                                }
                                // prior customer
                                // check for location
                                customer.dealer_type = type.dealer_type;
                                customer.tier = tier.ID;
                                if (salesrep != null && salesrep.salesRepID != 0) {
                                    customer.salesRepID = salesrep.salesRepID;
                                }
                                CustomerLocation location = db.CustomerLocations.Where(x => x.address.ToLower() == address.ToLower() && x.postalCode.Trim() == postalCode.Trim() && x.cust_id.Equals(customer.cust_id)).FirstOrDefault<CustomerLocation>();
                                if (location == null) {
                                    CustomerLocation new_location = new CustomerLocation {
                                        cust_id = customer.cust_id,
                                        name = locationname,
                                        address = address,
                                        city = city,
                                        postalCode = postalCode,
                                        phone = phone,
                                        latitude = latitude,
                                        longitude = longitude,
                                        contact_person = contact
                                    };

                                    if (stateid != 0) {
                                        new_location.stateID = stateid;
                                    }

                                    db.CustomerLocations.InsertOnSubmit(new_location);
                                    db.SubmitChanges();
                                } else {
                                    // update location
                                    location.contact_person = contact;
                                    location.name = locationname;
                                    location.city = city;
                                    location.phone = phone;
                                    location.postalCode = postalCode;
                                    location.latitude = latitude;
                                    location.longitude = longitude;
                                    db.SubmitChanges();
                                }
                            }
                        } catch (Exception e) {
                            errors.Add(row + ": " + e.Message);
                        }
                        row++;
                    }

                }
                if (errors.Count == 0) {
                    return RedirectToAction("Index", new { msg = "Customer Import Successful" });
                } else {
                    return RedirectToAction("MassUpload", new { msg = "There were some errors", errors = errors });
                }
            } catch (Exception e) {
                return RedirectToAction("MassUpload", new { msg = e.Message });
            }
        }

        public ActionResult PopulateLocations(int id = 0) {
            if (id > 0) {
                try {
                    CurtDevDataContext db = new CurtDevDataContext();
                    GoogleMaps map = new GoogleMaps(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsKey"]);
                    List<CustomerLocation> locations = db.CustomerLocations.Where(x => x.cust_id.Equals(id) && x.latitude.Equals(0) && x.longitude.Equals(0)).ToList<CustomerLocation>();
                    foreach (CustomerLocation l in locations) {
                        LatLng geo = map.GetLatLng(l.address, l.city, l.stateID);
                        if (geo != null) {
                            l.latitude = Convert.ToDouble(geo.latitude);
                            l.longitude = Convert.ToDouble(geo.longitude);
                        }
                    }
                    db.SubmitChanges();
                } catch {}
            }
            return RedirectToAction("Locations", new { id = id });
        }

        /* AJAX */


        public string GetCustomersJSON() {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(CustomerModel.GetCustomersJSON());
        }

        public string CustomersJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(CustomerModel.GetAll());
        }

        public string GetCustomerLocationsJSON(int id = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(CustomerModel.GetCustomerLocationsJSON(id));
        }

        public string GetLocations(int id = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(CustomerModel.GetLocations(id));
        }

        public string Delete(int id = 0) {
            return CustomerModel.DeleteCustomer(id);
        }

        public string DeleteLocation(int id = 0) {
            return CustomerModel.DeleteLocation(id);
        }

    }
}
