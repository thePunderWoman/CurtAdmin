using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models {
    public class UserManagement {

        ///////////// Users \\\\\\\\\\\

        public static List<CustomerUser> getAllUsersByCust(int cust_ID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<CustomerUser> listOfChildUsers = db.CustomerUsers.Where(x => x.cust_id == cust_ID && x.isSudo == false).ToList<CustomerUser>();
            return listOfChildUsers;
        }

        public static CustomerUser getUserByEmail(string email) {
            CurtDevDataContext db = new CurtDevDataContext();
            CustomerUser user = db.CustomerUsers.Where(x => x.email == email).FirstOrDefault<CustomerUser>();
            return user;
        }

        public static bool isChildUser(string sudoEmail, string childEmail) {
            bool isChildUser = false;

            CurtDevDataContext db = new CurtDevDataContext();

            // get sudo's cust_ID
            CustomerUser sudoUser = db.CustomerUsers.Where(x => x.email == sudoEmail).FirstOrDefault<CustomerUser>();
            if (sudoUser != null) {
                CustomerUser childUser = db.CustomerUsers.Where(x => x.email == childEmail).FirstOrDefault<CustomerUser>();
                if (childUser != null) {
                    if (sudoUser.cust_id == childUser.cust_id) {
                        return true;
                    }
                }
            }
            return isChildUser;
        }

        ///////////// API \\\\\\\\\\\\
        /// <returns></returns>
        public static List<ApiAccess> getAPIModules() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ApiAccess> listOfModules = db.ApiAccesses.ToList<ApiAccess>();
            return listOfModules;
        }
        public static CustomerUser findOwnerOfKey(string key) {
            CurtDevDataContext db = new CurtDevDataContext();
            CustomerUser user = db.ApiKeys.Where(x => x.api_key.ToString() == key).Select(x => x.CustomerUser).FirstOrDefault<CustomerUser>();
            return user;
        }
        public static ApiKey getAPIKey(string key) {
            CurtDevDataContext db = new CurtDevDataContext();
            ApiKey APIKey = db.ApiKeys.Where(x => x.api_key.ToString() == key && x.ApiKeyType.type == "Custom").FirstOrDefault<ApiKey>();
            return APIKey;
        }

        public static List<ApiKey> getAPIKeys(string email) {
            CurtDevDataContext db = new CurtDevDataContext();
            // grab user from email
            CustomerUser user = getUserByEmail(email);
            List<ApiKey> listOfKeys = user.ApiKeys.Where(x => x.ApiKeyType.type != "Authentication").ToList<ApiKey>();
            return listOfKeys;
        }

        public static List<ApiModule> getAllAPIModules() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ApiModule> listOfAllModules = db.ApiModules.ToList<ApiModule>();
            return listOfAllModules;
        }

        public static List<ApiModule> getSelectedModules(string keyID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ApiModule> modules = new List<ApiModule>();

            modules = (from m in db.ApiModules
                       join um in db.ApiAccesses on m.id equals um.module_id
                       where um.key_id.Equals(keyID)
                       orderby m.name
                       select m).ToList<ApiModule>();

            return modules;
        }

        public static void generateCustomAPIKey(Guid userID, List<string> selectedModuleIDs) {
            CurtDevDataContext db = new CurtDevDataContext();
            // create API Key with custom type
            ApiKey customKey = new ApiKey();
            customKey.id = Guid.NewGuid();
            customKey.api_key = Guid.NewGuid();
            customKey.type_id = db.ApiKeyTypes.Where(x => x.type == "Custom").Select(x => x.id).FirstOrDefault<Guid>();
            customKey.user_id = userID;
            customKey.date_added = DateTime.Now;

            db.ApiKeys.InsertOnSubmit(customKey);
            db.SubmitChanges();

            // create record for each selected module ID in the APIAcess table
            List<ApiAccess> listOfNewAPIAccesses = new List<ApiAccess>();
            foreach (string modID in selectedModuleIDs) {
                ApiAccess apiAccess = new ApiAccess();
                apiAccess.id = Guid.NewGuid();
                apiAccess.key_id = customKey.id;
                apiAccess.module_id = new Guid(modID);
                listOfNewAPIAccesses.Add(apiAccess);
            }
            db.ApiAccesses.InsertAllOnSubmit(listOfNewAPIAccesses);
            db.SubmitChanges();

            // submit changes
        }

        public static void generateGenericKey(Guid userID, string keyType) {
            CurtDevDataContext db = new CurtDevDataContext();

            // before creating a new genericKey (private or public), check to see if one already exists, if one does, dont create a new one.
            ApiKey keyCheck = db.ApiKeys.Where(x => x.user_id == userID && x.ApiKeyType.type == keyType).FirstOrDefault<ApiKey>();
            if (keyCheck != null) {
                // one already exists so throw exception stating one already exists
                throw new Exception("A " + keyType + " Key already exists. Each user should only have one of these types of keys.");
            } else {
                // create API Key
                ApiKey customKey = new ApiKey();
                customKey.id = Guid.NewGuid();
                customKey.api_key = Guid.NewGuid();
                customKey.type_id = db.ApiKeyTypes.Where(x => x.type == keyType).Select(x => x.id).FirstOrDefault<Guid>();
                customKey.user_id = userID;
                customKey.date_added = DateTime.Now;

                db.ApiKeys.InsertOnSubmit(customKey);
                db.SubmitChanges();
            }
        }

        public static void deleteKey(Guid userID, Guid idFieldOfAPIKey) {
            // delete a key as long as it is not an authentication type key
            CurtDevDataContext db = new CurtDevDataContext();

            ApiKey APIKeyToRemove = db.ApiKeys.Where(x => x.id == idFieldOfAPIKey && x.user_id == userID && x.ApiKeyType.type != "Authentication").FirstOrDefault<ApiKey>();
            if (APIKeyToRemove != null) {
                // remove each record in the access table with the key_id = idFieldOfAPIKey.
                List<ApiAccess> keysRefToRemove = db.ApiAccesses.Where(x => x.key_id == APIKeyToRemove.id).ToList<ApiAccess>();
                db.ApiAccesses.DeleteAllOnSubmit(keysRefToRemove);
                db.ApiKeys.DeleteOnSubmit(APIKeyToRemove);
                db.SubmitChanges();
            } else {
                throw new Exception("Could not delete key: Key not found.");
            }
        }

        public static void saveCustomAPIKey(Guid keyID, List<string> selectedModuleIDs) {

            // remove old APIKey Permission records
            CurtDevDataContext db = new CurtDevDataContext();

            List<ApiAccess> listOfAccesesToDelete = db.ApiAccesses.Where(x => x.key_id == keyID).ToList<ApiAccess>();
            db.ApiAccesses.DeleteAllOnSubmit(listOfAccesesToDelete);
            db.SubmitChanges();

            // create record for each selected module ID in the APIAcess table
            List<ApiAccess> listOfNewAPIAccesses = new List<ApiAccess>();
            foreach (string modID in selectedModuleIDs) {
                ApiAccess apiAccess = new ApiAccess();
                apiAccess.id = Guid.NewGuid();
                apiAccess.key_id = keyID;
                apiAccess.module_id = new Guid(modID);
                listOfNewAPIAccesses.Add(apiAccess);
            }
            db.ApiAccesses.InsertAllOnSubmit(listOfNewAPIAccesses);
            db.SubmitChanges();
        }

        public static bool hasPublic(string email) {
            // get list of users keys
            List<ApiKey> APIKeys = getAPIKeys(email);
            foreach (ApiKey key in APIKeys) {
                if (key.ApiKeyType.type == "Public") {
                    return true;
                }
            }
            return false;
        }

        public static bool hasPrivate(string email) {
            // get list of users keys
            List<ApiKey> APIKeys = getAPIKeys(email);
            foreach (ApiKey key in APIKeys) {
                if (key.ApiKeyType.type == "Private") {
                    return true;
                }
            }
            return false;
        }

        ////////// Web Permissions \\\\\\\\\\\

        public static List<AuthArea> getAllAuthAreas(string domainID) {

            CurtDevDataContext db = new CurtDevDataContext();
            List<AuthArea> allAreas = db.AuthAreas.Where(x => x.DomainID == new Guid(domainID)).ToList<AuthArea>();
            return allAreas;
        }

        public static List<AuthArea> getSelectedAreas(Guid userID, string domainID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<AuthArea> areas = new List<AuthArea>();
            areas = db.AuthAreas.Where(x => x.AuthAccess.userID == userID && x.DomainID.Equals(new Guid(domainID))).ToList<AuthArea>();
            return areas;
        }


        public static List<AuthDomain> getAllAuthDomains() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<AuthDomain> allDomains = db.AuthDomains.ToList<AuthDomain>();
            return allDomains;
        }

        public static List<AuthDomain> getSelectedDomains(Guid userID) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<AuthDomain> domains = new List<AuthDomain>();

            //domains = (from d in db.AuthDomains
             //          join aa in db.AuthAccesses on d.id equals aa.DomainID
             //          where aa.userID.Equals(userID) && aa.AreaID.Equals(Guid.Empty)
             //          orderby d.name
             //          select d).ToList<AuthDomain>();
            return domains;
        }

        public static string toggleWebPermissions(Guid userID, string domainID, string areaID) {
            CurtDevDataContext db = new CurtDevDataContext();
            if (areaID == "") {
                // domain level permission
                // attempt to find  authAccess record, if one is found, delete it, if one is not found, create one.
                AuthAccess authRecord = db.AuthAccesses.Where(x => x.userID == userID && x.AreaID.Equals(Guid.Empty)).FirstOrDefault<AuthAccess>();
                if (authRecord != null) {
                    // record exists so delete it.
                    db.AuthAccesses.DeleteOnSubmit(authRecord);
                    db.SubmitChanges();
                } else {
                    // record doesnt exist so create one.
                    AuthAccess newAuthAccess = new AuthAccess();
                    newAuthAccess.id = Guid.NewGuid();
                    newAuthAccess.userID = userID;
                    newAuthAccess.AreaID = Guid.Empty;
                    db.AuthAccesses.InsertOnSubmit(newAuthAccess);
                    db.SubmitChanges();
                }

            } else {
                // area level permission

                // attempt to find  authAccess record, if one is found, delete it, if one is not found, create one.
                AuthAccess authRecord = db.AuthAccesses.Where(x => x.userID == userID && x.AreaID.Equals(new Guid(areaID))).FirstOrDefault<AuthAccess>();
                if (authRecord != null) {
                    // record exists so delete it.
                    db.AuthAccesses.DeleteOnSubmit(authRecord);
                    db.SubmitChanges();
                } else {
                    // record doesnt exist so create one.
                    AuthAccess newAuthAccess = new AuthAccess();
                    newAuthAccess.id = Guid.NewGuid();
                    newAuthAccess.userID = userID;
                    newAuthAccess.AreaID = new Guid(areaID);
                    db.AuthAccesses.InsertOnSubmit(newAuthAccess);
                    db.SubmitChanges();
                }
            }

            return "";
        }

        public static AuthDomain getDomainByID(string domainID) {
            CurtDevDataContext db = new CurtDevDataContext();
            return db.AuthDomains.Where(x => x.id.Equals(new Guid(domainID))).FirstOrDefault<AuthDomain>();
        }

        public static AuthDomain getDomainByURL(string url) {
            CurtDevDataContext db = new CurtDevDataContext();
            return db.AuthDomains.Where(x => x.url.Equals(url)).FirstOrDefault<AuthDomain>();
        }

    }


}

namespace CurtAdmin {

    public partial class AuthArea {

        public List<AuthArea> getBreadCrumbs() {
            CurtDevDataContext db = new CurtDevDataContext();

            List<AuthArea> areas = new List<AuthArea>();

            areas.Add(db.AuthAreas.Where(x => x.id == this.id).FirstOrDefault<AuthArea>());
            bool moreAreas = (areas[0].parentAreaID == Guid.Empty) ? false : true; // check to see if the inital area has a parent or not.
            while (moreAreas) {
                // grab the last area in the areas array and grab its parent area
                areas.Add(db.AuthAreas.Where(x => x.id == areas[areas.Count() - 1].parentAreaID).FirstOrDefault<AuthArea>());
                // if the last area (the previous parent area) area is not null then continue
                if (areas[areas.Count() - 1].parentAreaID == Guid.Empty) {
                    moreAreas = false;
                }
            }
            // reverse the order for ease of display ( /Parent/Child )
            areas.Reverse();
            return areas;
        }

        public string getBreadCrumbsPath() {
            List<AuthArea> bc = getBreadCrumbs();
            string display = this.AuthDomain.url + "/";
            foreach (AuthArea a in bc) {
                if (a.path != "") {
                    display += a.path + "/";
                } else {
                    display += a.path;
                }
            }
            return display;
        }

    }
}