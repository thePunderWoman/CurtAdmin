using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Configuration;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Mail;

namespace CurtAdmin.Controllers {
    public class HomeController : BaseController {

        public ActionResult Index() {

            ViewBag.Message = "Welcome "+ Session["username"];

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            int userId = 0;
            if (Session["userID"] != null && Session["userID"].ToString().Length > 0) {
                userId = Convert.ToInt32(Session["userID"]);
            } else {
                HttpContext.Response.Redirect("~/Authenticate");
            }


            // Get the modules for this admin user
            List<module> modules = Users.GetUserModules(userId);

            ViewBag.Modules = modules;
            
            return View();
        }

        public string UpdateParts() {
            CurtDevDataContext db2 = new CurtDevDataContext();
            List<PartChange2012> changes = new List<PartChange2012>();
            Dictionary<DateTime, List<PartChange2012>> results = new Dictionary<DateTime, List<PartChange2012>>();
            List<int> failedList = new List<int>();
            changes = db2.PartChange2012s.OrderBy(x => x.partID).ToList();
            failedList = processChanges(changes);
            results.Add(DateTime.Now, changes);
            while (failedList.Count > 0) {
                changes = new List<PartChange2012>();
                changes = db2.PartChange2012s.Where(x => failedList.Contains(x.partID)).OrderBy(x => x.partID).ToList();
                failedList = processChanges(changes);
            }
            
            return JsonConvert.SerializeObject(results);
        }

        private List<int> processChanges(List<PartChange2012> changes) {
            List<int> failedParts = new List<int>();
            Parallel.ForEach(changes, change => {
                CurtDevDataContext db = new CurtDevDataContext();
                Part oldPart = db.Parts.Where(x => x.partID.Equals(change.partID)).FirstOrDefault();
                List<string> errors = new List<string>();
                if (oldPart != null && oldPart.partID != null && oldPart.partID > 0) {
                    try {
                        Part newPart = new Part();
                        try {
                            newPart = db.Parts.Where(x => x.partID.Equals(change.newPartID)).First();
                            newPart.priceCode = change.newPriceCode;
                            newPart.ACESPartTypeID = oldPart.ACESPartTypeID;
                            newPart.classID = oldPart.classID;
                            newPart.dateAdded = oldPart.dateAdded;
                            newPart.dateModified = DateTime.Now;
                            newPart.oldPartNumber = oldPart.oldPartNumber;
                            newPart.featured = oldPart.featured;
                            newPart.shortDesc = oldPart.shortDesc;
                            newPart.status = oldPart.status;
                            db.SubmitChanges();
                        } catch {
                            newPart = new Part {
                                partID = change.newPartID,
                                priceCode = change.newPriceCode,
                                ACESPartTypeID = oldPart.ACESPartTypeID,
                                classID = oldPart.classID,
                                dateAdded = oldPart.dateAdded,
                                dateModified = DateTime.Now,
                                oldPartNumber = oldPart.oldPartNumber,
                                featured = oldPart.featured,
                                shortDesc = oldPart.shortDesc,
                                status = oldPart.status
                            };
                            db.Parts.InsertOnSubmit(newPart);
                            db.SubmitChanges();
                        }

                        try {
                            foreach (PartImage img in oldPart.PartImages) {
                                img.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (PartAttribute attr in oldPart.PartAttributes) {
                                attr.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (CatParts cp in oldPart.CatParts) {
                                cp.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (ContentBridge cb in oldPart.ContentBridges) {
                                cb.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (PartPackage pp in oldPart.PartPackages) {
                                pp.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (PartVideo pv in oldPart.PartVideos) {
                                pv.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (Price pr in oldPart.Prices) {
                                pr.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (RelatedPart rp in oldPart.RelatedParts) {
                                rp.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (Review review in oldPart.Reviews) {
                                review.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (vcdb_VehiclePart vvp in oldPart.vcdb_VehicleParts) {
                                vvp.PartNumber = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            foreach (VehiclePart vp in oldPart.VehicleParts) {
                                vp.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            List<CartIntegration> integrations = db.CartIntegrations.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                            foreach (CartIntegration integration in integrations) {
                                integration.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            List<CustomerPricing> pricing = db.CustomerPricings.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                            foreach (CustomerPricing price in pricing) {
                                price.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            List<CustomerReportPart> reportParts = db.CustomerReportParts.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                            foreach (CustomerReportPart reportPart in reportParts) {
                                reportPart.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            List<PartIndex> indeces = db.PartIndexes.Where(x => x.partID.Equals(oldPart.partID)).ToList<PartIndex>();
                            foreach (PartIndex ix in indeces) {
                                ix.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            List<KioskOrderItem> koitems = db.KioskOrderItems.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                            foreach (KioskOrderItem koitem in koitems) {
                                koitem.partID = newPart.partID;
                            }
                        } catch (Exception e) {
                            errors.Add(e.Message);
                        }

                        try {
                            db.SubmitChanges();
                        } catch {
                            errors.Add("Error on Submit Changes");
                        }

                        try {
                            db.Parts.DeleteOnSubmit(oldPart);
                            db.SubmitChanges();
                        } catch {
                            errors.Add("Error Deleting Part");
                        }
                        if (errors.Count > 0) {
                            throw new Exception();
                        }
                    } catch (Exception e) {
                        failedParts.Add(change.partID);
                        string emsg = "<p>Error during change for part # " + change.partID + ".</p><ul>";
                        foreach (string error in errors) {
                            emsg += "<li>" + error + "</li>";
                        }
                        emsg += "<li>" + e.Message + "</li>";
                        emsg += "</ul>";
                        sendMail(emsg);
                    }
                }

            });
            return failedParts;
        }

        private void sendMail(string message = "") {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient();

            mail.To.Add("websupport@curtmfg.com");
            mail.Subject = "Automated Part Number Change Error";

            mail.IsBodyHtml = true;
            string htmlBody;

            htmlBody = "<h4>Error on Part Change process</h4>";
            htmlBody += "<p>There has been an error during the change process:</p>";
            htmlBody += message;

            mail.Body = htmlBody;

            SmtpServer.Send(mail);
        }
    }
}
