﻿using System;
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
            List<SalesRepresentative> salesreps = SalesRepModel.GetAll();
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
            SalesRepresentative rep = SalesRepModel.Get(salesRepID);
            ViewBag.H2 = rep.name + "'s Customers";
            List<Customer> customers = new List<Customer>();
            customers = CustomerModel.GetCustomersByRep(salesRepID);
            ViewBag.customers = customers;
            return View("ViewCustomers");
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
                    string searchURL = (Request.Form["searchURL"] != null) ? Request.Form["searchURL"] : "";
                    string logo = (Request.Form["logo"] != null && Request.Form["logo"].Trim() != "") ? Request.Form["logo"] : null;
                    int dealer_type = (Request.Form["dealer_type"] != null) ? Convert.ToInt32(Request.Form["dealer_type"]) : 0;
                    int customerID = (Request.Form["customerID"] != null && Request.Form["customerID"] != "") ? Convert.ToInt32(Request.Form["customerID"]) : 0;
                    int salesRepID = (Request.Form["salesRepID"] != null && Request.Form["salesRepID"] != "") ? Convert.ToInt32(Request.Form["salesRepID"]) : 0;
                    int mapixCodeID = (Request.Form["mapixCodeID"] != null && Request.Form["mapixCodeID"] != "") ? Convert.ToInt32(Request.Form["mapixCodeID"]) : 0;
                    int parentID = (Request.Form["parentID"] != null && Request.Form["parentID"] != "") ? Convert.ToInt32(Request.Form["parentID"]) : 0;
                    int tier = (Request.Form["tier"] != null && Request.Form["tier"] != "") ? Convert.ToInt32(Request.Form["tier"]) : 1;
                    bool isDummy = (Request.Form["isDummy"] != null && Request.Form["isDummy"] != "") ? Convert.ToBoolean(Request.Form["isDummy"]) : false;
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
                        searchURL = searchURL,
                        logo = logo,
                        mCodeID = mapixCodeID,
                        salesRepID = salesRepID,
                        dealer_type = dealer_type,
                        isDummy = isDummy,
                        tier = tier
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
                    string searchURL = (Request.Form["searchURL"] != null) ? Request.Form["searchURL"] : "";
                    string logo = (Request.Form["logo"] != null && Request.Form["logo"].Trim() != "") ? Request.Form["logo"] : null;
                    int dealer_type = (Request.Form["dealer_type"] != null) ? Convert.ToInt32(Request.Form["dealer_type"]) : 0;
                    int customerID = (Request.Form["customerID"] != null && Request.Form["customerID"] != "") ? Convert.ToInt32(Request.Form["customerID"]) : 0;
                    int salesRepID = (Request.Form["salesRepID"] != null && Request.Form["salesRepID"] != "") ? Convert.ToInt32(Request.Form["salesRepID"]) : 0;
                    int mapixCodeID = (Request.Form["mapixCodeID"] != null && Request.Form["mapixCodeID"] != "") ? Convert.ToInt32(Request.Form["mapixCodeID"]) : 0;
                    int parentID = (Request.Form["parentID"] != null && Request.Form["parentID"] != "") ? Convert.ToInt32(Request.Form["parentID"]) : 0;
                    int tier = (Request.Form["tier"] != null && Request.Form["tier"] != "") ? Convert.ToInt32(Request.Form["tier"]) : 1;
                    bool isDummy = (Request.Form["isDummy"] != null && Request.Form["isDummy"] != "") ? Convert.ToBoolean(Request.Form["isDummy"]) : false;

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
                    cust.searchURL = searchURL;
                    cust.logo = logo;
                    cust.dealer_type = dealer_type;
                    cust.mCodeID = mapixCodeID;
                    cust.salesRepID = salesRepID;
                    cust.latitude = location.latitude;
                    cust.longitude = location.longitude;
                    cust.isDummy = isDummy;
                    cust.tier = tier;

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