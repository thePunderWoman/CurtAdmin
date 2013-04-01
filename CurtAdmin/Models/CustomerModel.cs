using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CurtAdmin.Models {
    public class CustomerModel {

        /// <summary>
        /// Returns a List of all customers in the database, sorted by name ascending.
        /// </summary>
        /// <returns>List of Customer objects.</returns>
        public static List<Customer> GetAll() {
            List<Customer> customers = new List<Customer>();
            CurtDevDataContext db = new CurtDevDataContext();

            // Get the customers;
            customers = (from c in db.Customers
                         join dt in db.DealerTypes on c.dealer_type equals dt.dealer_type
                         join dtr in db.DealerTiers on c.tier equals dtr.ID
                         orderby c.name
                         select c).AsParallel<Customer>().WithDegreeOfParallelism(12).ToList<Customer>();

            return customers;
        }

        public static List<Customer> GetCustomersByType(int typeid = 1) {
            List<Customer> customers = new List<Customer>();
            CurtDevDataContext db = new CurtDevDataContext();

            // Get the customers;
            customers = (from c in db.Customers
                         join dt in db.DealerTypes on c.dealer_type equals dt.dealer_type
                         join dtr in db.DealerTiers on c.tier equals dtr.ID
                         where dt.dealer_type.Equals(typeid)
                         orderby c.name
                         select c).ToList<Customer>();

            return customers;
        }

        public static List<Customer> GetCustomersByRep(int id = 0) {
            List<Customer> customers = new List<Customer>();
            CurtDevDataContext db = new CurtDevDataContext();

            // Get the customers;
            customers = (from c in db.Customers
                         join dt in db.DealerTypes on c.dealer_type equals dt.dealer_type
                         join dtr in db.DealerTiers on c.tier equals dtr.ID
                         where c.salesRepID.Equals(id)
                         orderby dt.dealer_type, dtr.tier, c.name
                         select c).ToList<Customer>();

            return customers;
        }
        
        public static Customer Get(int custID) {
            Customer cust = new Customer();
            CurtDevDataContext db = new CurtDevDataContext();

            cust = (from c in db.Customers
                    where c.cust_id.Equals(custID)
                    select c).FirstOrDefault<Customer>();

            return cust;
        }

        public static Customer GetByCustomerID(int custID) {
            Customer cust = new Customer();
            CurtDevDataContext db = new CurtDevDataContext();

            cust = (from c in db.Customers
                    where c.customerID.Equals(custID)
                    select c).FirstOrDefault<Customer>();

            return cust;
        }


        public static List<DealerType> GetDealerTypes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<DealerType> types = new List<DealerType>();

            types = (from dt in db.DealerTypes
                     select dt).ToList<DealerType>();
            return types;
        }

        public static List<DealerTier> GetDealerTiers() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<DealerTier> tiers = new List<DealerTier>();

            tiers = (from dtr in db.DealerTiers
                     select dtr).OrderBy(x => x.sort).ToList<DealerTier>();
            return tiers;
        }

        public static List<MapixCode> GetMapixCodes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<MapixCode> codes = new List<MapixCode>();

            codes = db.MapixCodes.OrderBy(x => x.code).ToList<MapixCode>();
            return codes;
        }

        public static List<SalesRepresentative> GetSalesReps() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<SalesRepresentative> reps = new List<SalesRepresentative>();

            reps = db.SalesRepresentatives.OrderBy(x => x.code).ToList<SalesRepresentative>();
            return reps;
        }

        public static List<FullState> GetStates() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<FullState> states = new List<FullState>();

            states = (from ps in db.PartStates
                      join c in db.Countries on ps.countryID equals c.countryID
                      orderby ps.countryID
                      select new FullState {
                        stateID = ps.stateID,
                        state1 = ps.state1,
                        abbr = ps.abbr,
                        countryID = ps.countryID,
                        country = c
                      }).ToList<FullState>();

            return states;
        }

        public static List<FullCountry> GetCountries() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<FullCountry> countries = new List<FullCountry>();

            countries = (from c in db.Countries
                         select new FullCountry {
                            countryID = c.countryID,
                            name = c.name,
                            abbr = c.abbr,
                            states = db.PartStates.Where(x => x.countryID == c.countryID).OrderBy(x => x.state1).ToList<PartStates>()
                         }).ToList<FullCountry>();

            return countries;

        }

        public static List<CustomerLocation> GetLocations(int custID) {
            List<CustomerLocation> locations = new List<CustomerLocation>();
            CurtDevDataContext db = new CurtDevDataContext();

            locations = (from cl in db.CustomerLocations
                         where cl.cust_id.Equals(custID)
                         select cl).ToList<CustomerLocation>();
            return locations;
        }

        public static CustomerLocation GetLocation(int locationID) {
            CustomerLocation location = new CustomerLocation();
            CurtDevDataContext db = new CurtDevDataContext();

            location = (from cl in db.CustomerLocations
                        where cl.locationID.Equals(locationID)
                        select cl).FirstOrDefault<CustomerLocation>();
            return location;
        }

        public static string DeleteCustomer(int id) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                // Delete any pricing entries for this customer
                List<CustomerPricing> prices = new List<CustomerPricing>();
                prices = (from cp in db.CustomerPricings
                          where cp.cust_id.Equals(id)
                          select cp).ToList<CustomerPricing>();
                db.CustomerPricings.DeleteAllOnSubmit<CustomerPricing>(prices);

                // Delete all locations for this customer
                List<CustomerLocation> locations = new List<CustomerLocation>();
                locations = (from cl in db.CustomerLocations
                             where cl.cust_id.Equals(id)
                             select cl).ToList<CustomerLocation>();
                db.CustomerLocations.DeleteAllOnSubmit<CustomerLocation>(locations);

                // Delete the customer record
                Customer cust = new Customer();
                cust = (from c in db.Customers
                        where c.cust_id.Equals(id)
                        select c).FirstOrDefault<Customer>();
                db.Customers.DeleteOnSubmit(cust);
                db.SubmitChanges();
                return "";
            } catch (Exception e) {
                return "Error while deleting.";
            }
        }

        public static string GetState(int stateID, string field) {
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            switch (field) {
                case "abbr":
                    string abbr = (from ps in db.PartStates
                                   where ps.stateID.Equals(stateID)
                                   select ps.abbr).FirstOrDefault<string>();
                    return abbr;
                case "state":
                    string state = (from ps in db.PartStates
                                    where ps.stateID.Equals(stateID)
                                    select ps.state1).FirstOrDefault<string>();
                    return state;
                default:
                    FullState full_state = (from ps in db.PartStates
                                            join c in db.Countries on ps.countryID equals c.countryID
                                        where ps.stateID.Equals(stateID)
                                        select new FullState {
                                            state1 = ps.state1,
                                            stateID = ps.stateID,
                                            abbr = ps.abbr,
                                            countryID = ps.countryID,
                                            country = c
                                        }).FirstOrDefault<FullState>();
                    return js.Serialize(full_state);
            }
        }

        public static string DeleteLocation(int id) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                CustomerLocation loc = new CustomerLocation();
                loc = (from cl in db.CustomerLocations
                       where cl.locationID.Equals(id)
                       select cl).FirstOrDefault<CustomerLocation>();
                db.CustomerLocations.DeleteOnSubmit(loc);
                db.SubmitChanges();
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }

        public static List<DealerLocation> GetCustomersJSON(string type = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            List<DealerLocation> locations = (from cl in db.CustomerLocations
                                              join c in db.Customers on cl.cust_id equals c.cust_id
                                              join dt in db.DealerTypes on c.dealer_type equals dt.dealer_type
                                              join dtr in db.DealerTiers on c.tier equals dtr.ID
                                              where dt.online == false && dt.type.ToLower().Contains(type.ToLower())
                                              select new DealerLocation {
                                                  state = (from s in db.PartStates
                                                           join co in db.Countries on s.countryID equals co.countryID
                                                           where s.stateID.Equals(cl.stateID)
                                                           select new FullState {
                                                               stateID = s.stateID,
                                                               state1 = s.state1,
                                                               abbr = s.abbr,
                                                               countryID = s.countryID,
                                                               country = co
                                                           }).FirstOrDefault<FullState>(),
                                                  dealerType = new CustomerType { dealer_type = dt.dealer_type, online = dt.online, type = dt.type, tier = dtr.tier, mapicon = db.MapIcons.Where(x => x.tier == dtr.ID).Where(x => x.dealer_type == dt.dealer_type).Select(x => x.mapicon1).FirstOrDefault(), mapiconshadow = db.MapIcons.Where(x => x.tier == dtr.ID).Where(x => x.dealer_type == dt.dealer_type).Select(x => x.mapiconshadow).FirstOrDefault(), show = dt.show },
                                                  locationID = cl.locationID,
                                                  name = cl.name,
                                                  address = cl.address,
                                                  city = cl.city,
                                                  postalCode = cl.postalCode,
                                                  stateID = cl.stateID,
                                                  email = cl.email,
                                                  phone = cl.phone,
                                                  fax = cl.fax,
                                                  latitude = cl.latitude,
                                                  longitude = cl.longitude,
                                                  cust_id = cl.cust_id,
                                                  isprimary = cl.isprimary,
                                                  contact_person = cl.contact_person
                                              }).ToList<DealerLocation>();
            return locations;
        }

        public static List<DealerLocation> GetCustomerLocationsJSON(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<DealerLocation> locations = (from cl in db.CustomerLocations
                                              join c in db.Customers on cl.cust_id equals c.cust_id
                                              join dt in db.DealerTypes on c.dealer_type equals dt.dealer_type
                                              join dtr in db.DealerTiers on c.tier equals dtr.ID
                                              where dt.online == false && cl.cust_id == id
                                              select new DealerLocation {
                                                  state = (from s in db.PartStates
                                                           join co in db.Countries on s.countryID equals co.countryID
                                                           where s.stateID.Equals(cl.stateID)
                                                           select new FullState {
                                                               stateID = s.stateID,
                                                               state1 = s.state1,
                                                               abbr = s.abbr,
                                                               countryID = s.countryID,
                                                               country = co
                                                           }).FirstOrDefault<FullState>(),
                                                  dealerType = new CustomerType { dealer_type = dt.dealer_type, online = dt.online, type = dt.type, tier = dtr.tier, mapicon = db.MapIcons.Where(x => x.tier == dtr.ID).Where(x => x.dealer_type == dt.dealer_type).Select(x => x.mapicon1).FirstOrDefault(), mapiconshadow = db.MapIcons.Where(x => x.tier == dtr.ID).Where(x => x.dealer_type == dt.dealer_type).Select(x => x.mapiconshadow).FirstOrDefault(), show = dt.show },
                                                  locationID = cl.locationID,
                                                  name = cl.name,
                                                  address = cl.address,
                                                  city = cl.city,
                                                  postalCode = cl.postalCode,
                                                  stateID = cl.stateID,
                                                  email = cl.email,
                                                  phone = cl.phone,
                                                  fax = cl.fax,
                                                  latitude = cl.latitude,
                                                  longitude = cl.longitude,
                                                  cust_id = cl.cust_id,
                                                  isprimary = cl.isprimary,
                                                  contact_person = cl.contact_person
                                              }).ToList<DealerLocation>();
            return locations;
        }
    }

    public class FullState {
        public int stateID { get; set; }
        public string state1 { get; set; }
        public string abbr {get; set;}
        public int countryID { get; set; }
        public Country country { get; set; }
    }

    public class CustomerType {
        public int dealer_type { get; set; }
        public string type { get; set; }
        public string tier { get; set; }
        public string mapicon { get; set; }
        public string mapiconshadow { get; set; }
        public bool online { get; set; }
        public bool show { get; set; }
    }

    public class FullCountry : Country {
        public List<PartStates> states { get; set; }
    }

    public class DealerLocation {
        public int locationID { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public int stateID { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string fax { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int cust_id { get; set; }
        public string contact_person { get; set; }
        public bool isprimary { get; set; }
        public FullState state { get; set; }
        public CustomerType dealerType { get; set; }
    }

}