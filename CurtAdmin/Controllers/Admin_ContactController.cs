using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.IO;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace CurtAdmin.Controllers {
    public class Admin_ContactController : AdminBaseController {
        //
        // GET: /Contact/

        public ActionResult Index()
        {
            ViewBag.contacts = ContactModel.GetAll();
            return View();
        }

        public ActionResult ViewContact(int id = 0) {
            ViewBag.contact = ContactModel.GetContact(id);
            return View();
        }

        public ActionResult Receivers() {
            ViewBag.receivers = ContactModel.GetReceivers();
            return View();
        }

        public ActionResult Types() {
            ViewBag.types = ContactModel.GetTypes();
            return View();
        }

        public ActionResult AddReceiver() {
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();

            #region Form Submission
            try {
                if (Request.Form.Count > 0) {


                    // Save form values
                    string first_name = (Request.Form["first_name"] != null) ? Request.Form["first_name"] : "";
                    string last_name = (Request.Form["last_name"] != null) ? Request.Form["last_name"] : "";
                    string email = (Request.Form["email"] != null) ? Request.Form["email"] : "";
                    List<string> types = (Request.Form["types"] != null) ? Request.Form["types"].Split(',').ToList<string>() : new List<string>();

                    // Validate the form fields
                    if (email.Length == 0) throw new Exception("Email is required.");
                    if (types.Count() == 0) throw new Exception("At least one Receiver Type is required.");

                    // Create the new customer and save
                    ContactReceiver new_receiver = new ContactReceiver {
                        first_name = first_name,
                        last_name = last_name,
                        email = email
                    };

                    db.ContactReceivers.InsertOnSubmit(new_receiver);
                    db.SubmitChanges();
                    if (types.Count() > 0) {
                        foreach (string type in types) {
                            if (type != "") {
                                try {
                                    ContactReceiver_ContactType rectype = new ContactReceiver_ContactType {
                                        contactReceiverID = new_receiver.contactReceiverID,
                                        contactTypeID = Convert.ToInt32(type)
                                    };
                                    db.ContactReceiver_ContactTypes.InsertOnSubmit(rectype);
                                } catch { }
                            }
                        }
                    }
                    db.SubmitChanges();
                    Response.Redirect("/Admin_Contact/Receivers");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.types = ContactModel.GetTypes();

            ViewBag.error = error;

            return View();
        }

        public ActionResult EditReceiver(int id = 0) {
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();

            #region Form Submission
            try {
                if (Request.Form.Count > 0) {


                    // Save form values
                    string first_name = (Request.Form["first_name"] != null) ? Request.Form["first_name"] : "";
                    string last_name = (Request.Form["last_name"] != null) ? Request.Form["last_name"] : "";
                    string email = (Request.Form["email"] != null) ? Request.Form["email"] : "";
                    List<string> types = (Request.Form["types"] != null) ? Request.Form["types"].Split(',').ToList<string>() : new List<string>();
                    // Validate the form fields
                    if (email.Length == 0) throw new Exception("Email is required.");
                    if (types.Count() == 0) throw new Exception("At least one Receiver Type is required.");

                    // Create the new customer and save
                    ContactReceiver mod_receiver = db.ContactReceivers.Where(x => x.contactReceiverID == id).FirstOrDefault<ContactReceiver>();
                    
                    mod_receiver.first_name = first_name;
                    mod_receiver.last_name = last_name;
                    mod_receiver.email = email;
                    db.SubmitChanges();

                    List<ContactReceiver_ContactType> rectypes = db.ContactReceiver_ContactTypes.Where(x => x.contactReceiverID == mod_receiver.contactReceiverID).ToList<ContactReceiver_ContactType>();
                    db.ContactReceiver_ContactTypes.DeleteAllOnSubmit<ContactReceiver_ContactType>(rectypes);
                    db.SubmitChanges();

                    if (types.Count() > 0) {
                        foreach (string type in types) {
                            if (type != "") {
                                try {
                                    ContactReceiver_ContactType rectype = new ContactReceiver_ContactType {
                                        contactReceiverID = mod_receiver.contactReceiverID,
                                        contactTypeID = Convert.ToInt32(type)
                                    };
                                    db.ContactReceiver_ContactTypes.InsertOnSubmit(rectype);
                                } catch { }
                            }
                        }
                    }

                    db.SubmitChanges();
                    Response.Redirect("/Admin_Contact/Receivers");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.receiver = ContactModel.GetReceiver(id);
            ViewBag.types = ContactModel.GetTypes();

            ViewBag.error = error;

            return View();
        }

        public ActionResult AddType() {
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();

            #region Form Submission
            try {
                if (Request.Form.Count > 0) {


                    // Save form values
                    string name = (Request.Form["name"] != null) ? Request.Form["name"] : "";

                    // Validate the form fields
                    if (name.Length == 0) throw new Exception("name is required.");

                    // Create the new customer and save
                    ContactType new_type = new ContactType {
                        name = name
                    };

                    db.ContactTypes.InsertOnSubmit(new_type);
                    db.SubmitChanges();
                    Response.Redirect("/Admin_Contact/Types");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.error = error;

            return View();
        }

        public void DeleteReceiver(int id = 0) {
            try {
                ContactModel.DeleteReceiver(id);
                Response.Redirect("/Admin_Contact/Receivers");
            } catch {
                Response.Redirect("/Admin_Contact/Receivers");
            }
        }

        public void DeleteType(int id = 0) {
            try {
                ContactModel.DeleteType(id);
                Response.Redirect("/Admin_Contact/Types");
            } catch {
                Response.Redirect("/Admin_Contact/Types");
            }
        }

        public void Export() {
            try {
                List<Contact> contacts = ContactModel.GetAll();
                ExcelPackage pck = new ExcelPackage();
                var ws = pck.Workbook.Worksheets.Add("Contacts");
                ws.Cells[1,1].Value = "First Name";
                ws.Cells[1,1].Style.Font.Bold = true;
                ws.Column(1).Width = 15;
                ws.Cells[1,2].Value = "Last Name";
                ws.Cells[1,2].Style.Font.Bold = true;
                ws.Column(2).Width = 15;
                ws.Cells[1,3].Value = "Email Address";
                ws.Cells[1,3].Style.Font.Bold = true;
                ws.Column(3).Width = 25;
                ws.Cells[1,4].Value = "Created Date";
                ws.Cells[1,4].Style.Font.Bold = true;
                ws.Column(4).Width = 18;
                ws.Cells[1,5].Value = "Type";
                ws.Cells[1,5].Style.Font.Bold = true;
                ws.Column(5).Width = 25;
                ws.Cells[1,6].Value = "Subject";
                ws.Cells[1,6].Style.Font.Bold = true;
                ws.Column(6).Width = 32;
                ws.Cells[1,7].Value = "Message";
                ws.Cells[1,7].Style.Font.Bold = true;
                ws.Column(7).Width = 80;
                int counter = 1;
                foreach (Contact contact in contacts) {
                    counter++;
                    ws.Cells[counter,1].Value = contact.first_name;
                    ws.Cells[counter,2].Value = contact.last_name;
                    ws.Cells[counter,3].Value = contact.email;
                    ws.Cells[counter,4].Value = contact.createdDate;
                    ws.Cells[counter,4].Style.Numberformat.Format = "m/d/yyyy h:mm AM/PM";
                    ws.Cells[counter,5].Value = contact.type;
                    ws.Cells[counter,6].Value = contact.subject;
                    ws.Cells[counter,7].Value = contact.message;
                }
                pck.SaveAs(Response.OutputStream);
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=contacts.xlsx");
            } catch { }
        }
    }
}
