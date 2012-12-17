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
            changes = db2.PartChange2012s.Where(x => x.partID < 13000).OrderBy(x => x.partID).ToList();
            Parallel.ForEach(changes, change => {
                CurtDevDataContext db = new CurtDevDataContext();
                Part oldPart = db.Parts.Where(x => x.partID.Equals(change.partID)).FirstOrDefault();
                if (oldPart != null && oldPart.partID != null && oldPart.partID > 0) {
                    try {
                        Part newPart = new Part {
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

                        foreach (PartImage img in oldPart.PartImages) {
                            img.partID = newPart.partID;
                        }

                        foreach (PartAttribute attr in oldPart.PartAttributes) {
                            attr.partID = newPart.partID;
                        }

                        foreach (CatParts cp in oldPart.CatParts) {
                            cp.partID = newPart.partID;
                        }

                        foreach (ContentBridge cb in oldPart.ContentBridges) {
                            cb.partID = newPart.partID;
                        }

                        foreach (PartPackage pp in oldPart.PartPackages) {
                            pp.partID = newPart.partID;
                        }

                        foreach (PartVideo pv in oldPart.PartVideos) {
                            pv.partID = newPart.partID;
                        }

                        foreach (Price pr in oldPart.Prices) {
                            pr.partID = newPart.partID;
                        }

                        foreach (RelatedPart rp in oldPart.RelatedParts) {
                            rp.partID = newPart.partID;
                        }

                        foreach (Review review in oldPart.Reviews) {
                            review.partID = newPart.partID;
                        }

                        foreach (vcdb_VehiclePart vvp in oldPart.vcdb_VehicleParts) {
                            vvp.PartNumber = newPart.partID;
                        }

                        foreach (VehiclePart vp in oldPart.VehicleParts) {
                            vp.partID = newPart.partID;
                        }

                        List<CartIntegration> integrations = db.CartIntegrations.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                        foreach (CartIntegration integration in integrations) {
                            integration.partID = newPart.partID;
                        }

                        List<CustomerPricing> pricing = db.CustomerPricings.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                        foreach (CustomerPricing price in pricing) {
                            price.partID = newPart.partID;
                        }

                        List<CustomerReportPart> reportParts = db.CustomerReportParts.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                        foreach (CustomerReportPart reportPart in reportParts) {
                            reportPart.partID = newPart.partID;
                        }

                        List<PartIndex> indeces = db.PartIndexes.Where(x => x.partID.Equals(oldPart.partID)).ToList<PartIndex>();
                        foreach (PartIndex ix in indeces) {
                            ix.partID = newPart.partID;
                        }

                        List<KioskOrderItem> koitems = db.KioskOrderItems.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                        foreach (KioskOrderItem koitem in koitems) {
                            koitem.partID = newPart.partID;
                        }

                        db.SubmitChanges();

                        db.Parts.DeleteOnSubmit(oldPart);
                        db.SubmitChanges();
                    } catch {
                        sendMail("Error during change for part # " + change.partID + ".");
                    }
                }

            });

            changes = new List<PartChange2012>();
            changes = db2.PartChange2012s.Where(x => x.partID > 13000).OrderBy(x => x.partID).ToList();
            Parallel.ForEach(changes, change => {
                CurtDevDataContext db = new CurtDevDataContext();
                Part oldPart = db.Parts.Where(x => x.partID.Equals(change.partID)).FirstOrDefault();
                if (oldPart != null && oldPart.partID != null && oldPart.partID > 0) {
                    try {
                        Part newPart = new Part {
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

                        foreach (PartImage img in oldPart.PartImages) {
                            img.partID = newPart.partID;
                        }

                        foreach (PartAttribute attr in oldPart.PartAttributes) {
                            attr.partID = newPart.partID;
                        }

                        foreach (CatParts cp in oldPart.CatParts) {
                            cp.partID = newPart.partID;
                        }

                        foreach (ContentBridge cb in oldPart.ContentBridges) {
                            cb.partID = newPart.partID;
                        }

                        foreach (PartPackage pp in oldPart.PartPackages) {
                            pp.partID = newPart.partID;
                        }

                        foreach (PartVideo pv in oldPart.PartVideos) {
                            pv.partID = newPart.partID;
                        }

                        foreach (Price pr in oldPart.Prices) {
                            pr.partID = newPart.partID;
                        }

                        foreach (RelatedPart rp in oldPart.RelatedParts) {
                            rp.partID = newPart.partID;
                        }

                        foreach (Review review in oldPart.Reviews) {
                            review.partID = newPart.partID;
                        }

                        foreach (vcdb_VehiclePart vvp in oldPart.vcdb_VehicleParts) {
                            vvp.PartNumber = newPart.partID;
                        }

                        foreach (VehiclePart vp in oldPart.VehicleParts) {
                            vp.partID = newPart.partID;
                        }

                        List<CartIntegration> integrations = db.CartIntegrations.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                        foreach (CartIntegration integration in integrations) {
                            integration.partID = newPart.partID;
                        }

                        List<CustomerPricing> pricing = db.CustomerPricings.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                        foreach (CustomerPricing price in pricing) {
                            price.partID = newPart.partID;
                        }

                        List<CustomerReportPart> reportParts = db.CustomerReportParts.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                        foreach (CustomerReportPart reportPart in reportParts) {
                            reportPart.partID = newPart.partID;
                        }

                        List<PartIndex> indeces = db.PartIndexes.Where(x => x.partID.Equals(oldPart.partID)).ToList<PartIndex>();
                        foreach (PartIndex ix in indeces) {
                            ix.partID = newPart.partID;
                        }

                        List<KioskOrderItem> koitems = db.KioskOrderItems.Where(x => x.partID.Equals(oldPart.partID)).ToList();
                        foreach (KioskOrderItem koitem in koitems) {
                            koitem.partID = newPart.partID;
                        }

                        db.SubmitChanges();

                        db.Parts.DeleteOnSubmit(oldPart);
                        db.SubmitChanges();
                    } catch {
                        sendMail("Error during change for part # " + change.partID + ".");
                    }
                }

            }); 
            return JsonConvert.SerializeObject(changes);
        }

        private void sendMail(string message = "") {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient();

            mail.To.Add("websupport@curtmfg.com");
            mail.Subject = "CURT Documentation Account Recovery";

            mail.IsBodyHtml = true;
            string htmlBody;

            htmlBody = "<h4>Error on Part Change process</h4>";
            htmlBody += "<p>There has been an error during the change process:</p>";
            htmlBody += "<p>" + message + "</p>";

            mail.Body = htmlBody;

            SmtpServer.Send(mail);
        }
    }
}
