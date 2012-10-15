using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;
using System.Security.Cryptography;
using System.Data.Linq;
using System.Net.Mail;

namespace CurtAdmin.Models {

    public class Users {

        public static string error_message { get; set; }


        /// <summary>
        /// Get all users in the database
        /// </summary>
        /// <returns>List of users</returns>
        public static List<user> GetAllUsers() {
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            List<user> users = new List<user>();

            users = (from u in doc_db.users
                        orderby u.lname
                        select u).ToList<user>();

            return users;
        }

        /// <summary>
        /// If you pass in the module type it will return the modules of that type, or if left blank it will return all modules
        /// </summary>
        /// <param name="moduleType">string value containing either 'user' or 'admin'</param>
        /// <returns>List of modules</returns>
        public static List<module> GetAllModules() {
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            List<module> modules = new List<module>();

            modules = (from m in doc_db.modules
                        orderby m.module1
                        select m).ToList<module>();

            return modules;
        }


        /// <summary>
        /// Get the modules applied to a given user
        /// </summary>
        /// <param name="userID">integer designating the user to get modules for.</param>
        /// <returns>List of modules</returns>
        public static List<module> GetUserModules(int userID) {
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            List<module> modules = new List<module>();

            if (userID > 0) { // Make sure we have a valid userID
                
                modules = (from m in doc_db.modules
                           join um in doc_db.user_modules on m.moduleID equals um.moduleID
                           where um.userID.Equals(userID)
                           orderby m.module1
                           select m).ToList<module>();
            }

            return modules;
        }

        /// <summary>
        /// Gets a user record based on the userID that is passed.
        /// </summary>
        /// <param name="userID">Integer</param>
        /// <returns>user object</returns>
        public static user GetUser(int userID) {
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            user u = (from users in doc_db.users
                      where users.userID.Equals(userID)
                      select users).FirstOrDefault<user>();
            return u;
        }

        /// <summary>
        /// This function will send an email to the given user letting them know that their account has been created and is active.
        /// </summary>
        /// <param name="userID">ID of the user.</param>
        public static void AlertNewUser(int userID) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            // Get the user's infromation
            user thisUser = (from u in doc_db.users
                             where u.userID.Equals(userID)
                             select u).FirstOrDefault<user>();

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient();

            mail.To.Add(thisUser.email);
            mail.Subject = "CURT Documentation Account Activation";

            mail.IsBodyHtml = true;
            string htmlBody;

            htmlBody = "<div style='margin-top: 15px;font-family: Arial;font-size: 10pt;'>";
            htmlBody += "<h4>Dear " + thisUser.fname + " " + thisUser.lname + ",</h4>";
            htmlBody += "<p>A new account has been created with the e-mail {" + thisUser.email +"}. The credentials are: </p>";
            htmlBody += "<p style='margin:2px 0px'>Username: <strong>" + thisUser.username + "</strong></p>";
            htmlBody += "<p style='margin:2px 0px'>Password: <strong>" + thisUser.password + "</strong></p>";
            htmlBody += "<p>You may log into your account at <a href='http://admin.curtmfg.com'>http://admin.curtmfg.com</a></p>";
            htmlBody += "______________________________________________________________________";
            htmlBody += "<p>If you feel this has been sent by mistake, please contact Web Support at <a href='mailto:websupport@curtmfg.com' target='_blank'>websupport@curtmfg.com</a>.</p>";
            htmlBody += "<br /><span style='color:#999'>Thank you,</span>";
            htmlBody += "<br /><br /><br />";
            htmlBody += "<span style='line-height:75px;color:#999'>CURT Administration</span>";
            htmlBody += "</div>";

            mail.Body = htmlBody;

            SmtpServer.Send(mail);
        }

        /// <summary>
        /// Alert the sales rep that a new user has signed up. This will let them know that they need to log in and chose to make the user active or not.
        /// </summary>
        /// <param name="u">User that has signed up.</param>
        public static void AlertRep(user u) {
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Get the users state
            State state = (from s in doc_db.States
                          where s.stateID.Equals(u.stateID)
                          select s).FirstOrDefault<State>();

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient();

            mail.To.Add("websupport@curtmfg.com");
            mail.Subject = "CURT Administration Account Sign Up";

            mail.IsBodyHtml = true;
            string htmlBody;

            htmlBody = "<div style='margin-top: 15px;font-family: Arial;font-size: 10pt;'>";
                htmlBody += "<div style='border-bottom: 2px solid #999'>";
                    htmlBody += "<p>A new account has been created with the e-mail {" + u.email + "}. </p>";
                    htmlBody += "<p style='margin:2px 0px'>Name: <strong>" + u.fname + " " + u.lname + "</strong></p>";
                    htmlBody += "<p style='margin:2px 0px'>Phone: <strong>" + u.phone + "</strong></p>";
                    htmlBody += "<p style='margin:2px 0px'>Address: <strong>" + u.address + " " + u.city +", "+ state.abbr + "</strong></p>";
                    htmlBody += "<p style='margin:2px 0px'>Comments: <strong>" + u.comments + "</strong></p><br />";
                    htmlBody += "<p style='margin:2px 0px'>Please login to the admin section of the CURT Administration and activate the account.</p>";
                htmlBody += "</div>";
                htmlBody += "<p>If you feel this has been sent by mistake, please contact Web Support at <a href='mailto:websupport@curtmfg.com' target='_blank'>websupport@curtmfg.com</a>.</p>";
                htmlBody += "<br /><span style='color:#999'>Thank you,</span>";
                htmlBody += "<br /><br /><br />";
                htmlBody += "<span style='line-height:75px;color:#999'>CURT Administration</span>";
            htmlBody += "</div>";

            mail.Body = htmlBody;

            SmtpServer.Send(mail);
        }

        /*** AJAX Functions ***/

        /// <summary>
        /// Delete a given user from the system.
        /// </summary>
        /// <param name="userID">ID of the user.</param>
        /// <returns>True/False</returns>
        public static Boolean DeleteUser(int userID) {

            try {
                DocsLinqDataContext doc_db = new DocsLinqDataContext();
                user u = (from users in doc_db.users
                          where users.userID.Equals(userID)
                          select users).FirstOrDefault<user>();
                doc_db.users.DeleteOnSubmit(u);
                doc_db.SubmitChanges();
                return true;
            } catch (Exception e) {
                error_message = e.Message;
                return false;

            }
        }

        /// <summary>
        /// This will update the active/isactive status of the user.
        /// </summary>
        /// <param name="userID">ID of the user.</param>
        /// <returns>True/False</returns>
        public static Boolean Set_isActive(int userID) {
            try {
                DocsLinqDataContext doc_db = new DocsLinqDataContext();
                user u = (from users in doc_db.users
                          where users.userID.Equals(userID)
                          select users).FirstOrDefault<user>();

                int old_status = u.isActive;
                u.isActive = (u.isActive == 0)?1:0;
                int new_status = u.isActive;

                if (new_status == 1 && old_status == 0) {
                    AlertNewUser(userID);
                }

                doc_db.SubmitChanges();
                return true;
            } catch (Exception e) {
                error_message = e.Message;
                return false;
            }
        }
    }
}
