using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin {
    partial class CustomerUser {

        public static List<CustomerUser> GetAll() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<CustomerUser> users = new List<CustomerUser>();

                users = db.CustomerUsers.ToList<CustomerUser>();
                return users;
            } catch (Exception e) {
                return new List<CustomerUser>();
            }
        }

        public CustomerUser Get(Guid id) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                CustomerUser u = new CustomerUser();

                u = db.CustomerUsers.Where(x => x.id.Equals(id)).FirstOrDefault<CustomerUser>();
                return u;
            } catch (Exception e) {
                return new CustomerUser();
            }
        }

        public void Delete() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                CustomerUser u = db.CustomerUsers.Where(x => x.id == this.id).FirstOrDefault<CustomerUser>();
                db.CustomerUsers.DeleteOnSubmit(u);
                db.SubmitChanges();
            } catch (Exception e) {
                throw e;
            }
        }

        public List<WebProperty> getWebProperties() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();

                List<WebProperty> webProperties = new List<WebProperty>();
                List<int> listOfWebPropIDs = db.CustUserWebProperties.Where(x => x.userID.Equals(this.id)).Select(x => x.webPropID).ToList<int>();
                foreach (int webPropID in listOfWebPropIDs) {
                    WebProperty webProp = db.WebProperties.Where(x => x.id == webPropID).FirstOrDefault<WebProperty>();
                    if (webProp != null) {
                        webProperties.Add(webProp);
                    }
                }
                return webProperties;


            } catch (Exception e) {
                
                throw e;
            }
        }

        /// <summary>
        /// This will update the active/isactive status of the user.
        /// </summary>
        /// <param name="userID">ID of the user.</param>
        /// <returns>True/False</returns>
        public Boolean activate() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                CustomerUser u = db.CustomerUsers.Where(x => x.id == this.id).FirstOrDefault<CustomerUser>();
                u.active = (u.active) ? false : true;
                db.SubmitChanges();
                return true;
            } catch {
                return false;
            }
        }
    }
}