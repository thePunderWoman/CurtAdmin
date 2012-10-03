using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CurtAdmin.Models {
    public class ContactModel {

        /// <summary>
        /// Returns a List of all customers in the database, sorted by name ascending.
        /// </summary>
        /// <returns>List of Customer objects.</returns>
        public static List<Contact> GetAll() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<Contact> contacts = db.Contacts.OrderByDescending(x => x.createdDate).ToList<Contact>();

            return contacts;
        }


        public static Contact GetContact(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            Contact contact = db.Contacts.Where(x => x.contactID == id).First<Contact>();
            return contact;
        }

        public static ContactReceiverWithTypes GetReceiver(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            ContactReceiverWithTypes receiver = (from cr in db.ContactReceivers
                                                 where cr.contactReceiverID.Equals(id)
                                                 select new ContactReceiverWithTypes {
                                                     contactReceiverID = cr.contactReceiverID,
                                                     first_name = cr.first_name,
                                                     last_name = cr.last_name,
                                                     email = cr.email,
                                                     types = (from ct in db.ContactTypes
                                                              join crt in db.ContactReceiver_ContactTypes on ct.contactTypeID equals crt.contactTypeID
                                                              where crt.contactReceiverID.Equals(cr.contactReceiverID)
                                                              orderby ct.name
                                                              select ct).ToList<ContactType>()
                                                 }).FirstOrDefault<ContactReceiverWithTypes>();
            return receiver;
        }

        public static List<ContactReceiverWithTypes> GetReceivers() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ContactReceiverWithTypes> receivers = (from cr in db.ContactReceivers
                                                        select new ContactReceiverWithTypes {
                                                            contactReceiverID = cr.contactReceiverID,
                                                            first_name = cr.first_name,
                                                            last_name = cr.last_name,
                                                            email = cr.email,
                                                            types = (from ct in db.ContactTypes
                                                                     join crt in db.ContactReceiver_ContactTypes on ct.contactTypeID equals crt.contactTypeID
                                                                     where crt.contactReceiverID.Equals(cr.contactReceiverID)
                                                                     orderby ct.name
                                                                     select ct).ToList<ContactType>()
                                                        }).ToList<ContactReceiverWithTypes>();
            return receivers;
        }

        public static List<ContactType> GetTypes() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<ContactType> types = db.ContactTypes.ToList<ContactType>();
            return types;
        }

        public static void DeleteReceiver(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            IEnumerable<ContactReceiver_ContactType> types = db.ContactReceiver_ContactTypes.Where(x => x.contactReceiverID == id).ToList<ContactReceiver_ContactType>();
            db.ContactReceiver_ContactTypes.DeleteAllOnSubmit<ContactReceiver_ContactType>(types);
            db.SubmitChanges();
            ContactReceiver receiver = db.ContactReceivers.Where(x => x.contactReceiverID == id).Single<ContactReceiver>();
            db.ContactReceivers.DeleteOnSubmit(receiver);
            db.SubmitChanges();
        }

        public static void DeleteType(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            IEnumerable<ContactReceiver_ContactType> types = db.ContactReceiver_ContactTypes.Where(x => x.contactTypeID == id).ToList<ContactReceiver_ContactType>();
            db.ContactReceiver_ContactTypes.DeleteAllOnSubmit(types);
            db.SubmitChanges();
            ContactType type = db.ContactTypes.Where(x => x.contactTypeID == id).Single<ContactType>();
            db.ContactTypes.DeleteOnSubmit(type);
            db.SubmitChanges();
        }
    }

    public class ContactReceiverWithTypes : ContactReceiver {
        public List<ContactType> types { get; set; }
    }

}