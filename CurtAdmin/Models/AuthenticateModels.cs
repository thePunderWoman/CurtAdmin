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

    public class AuthenticateUser{

        /// <summary>
        /// Generates a new password for a given user and e-mails them the new credentials.
        /// </summary>
        /// <param name="u">User object</param>
        /// <returns>True if e-mail was sent ::: False if we encountered an error.</returns>
        public static Boolean sendNewPass(user u) {

            // Get the user information
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            user thisUser = (from users in doc_db.users
                            where users.userID.Equals(u.userID)
                            select users).FirstOrDefault<user>();

            // Generate the new password
            PasswordGenerator pg = new PasswordGenerator();
            string newPass = pg.Generate();

            // Assign to user
            thisUser.password = newPass;

            try { // Attempt to committ the changes to the database

                // Save the changes
                doc_db.SubmitChanges();

                // Attempt to send e-mail
                try {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient();

                    mail.To.Add(thisUser.email);
                    mail.Subject = "CURT Documentation Account Recovery";

                    mail.IsBodyHtml = true;
                    string htmlBody;

                    htmlBody    =   "<div style='margin-top: 15px;font-family: Arial;font-size: 10pt;'>";
                    htmlBody    +=  "<h4>Dear " + thisUser.fname + " " + thisUser.lname + ",</h4>";
                    htmlBody    +=  "<p>There has been a password change for {"+thisUser.username+"}. You're new credentials for CURT Manufacturing Documentation are: </p>";
                    htmlBody    +=  "<p style='margin:2px 0px'>Username: <strong>" + thisUser.username + "</strong></p>";
                    htmlBody    +=  "<p style='margin:2px 0px'>Password: <strong>" + newPass + "</strong></p>";
                    htmlBody    +=  "______________________________________________________________________";
                    htmlBody += "<p>If you feel this has been sent by mistake, please contact Web Support at <a href='mailto:websupport@curtmfg.com' target='_blank'>websupport@curtmfg.com</a>.</p>";
                    htmlBody    +=  "<br /><span style='color:#999'>Thank you,</span>";
                    htmlBody    +=  "<br /><br /><br />";
                    htmlBody    +=  "<span style='line-height:75px;color:#999'>CURT Documentation Administrator</span>";
                    htmlBody    +=  "</div>";

                    mail.Body = htmlBody;

                    SmtpServer.Send(mail);
                } catch (Exception e) {
                    Console.Write(e.Message);
                    return false;
                }

                return true;
            } catch (ChangeConflictException e) {
                return false;
            }
            
        }

    }

    /// <summary>
    /// The class is used for generating new passwords. I'm not going to go into details for this one. Delare new PasswordGenerator() and call Generate().
    /// </summary>
    public class PasswordGenerator {
        public PasswordGenerator() {
            this.Minimum = DefaultMinimum;
            this.Maximum = DefaultMaximum;
            this.ConsecutiveCharacters = false;
            this.RepeatCharacters = true;
            this.ExcludeSymbols = false;
            this.Exclusions = null;

            rng = new RNGCryptoServiceProvider();
        }

        protected int GetCryptographicRandomNumber(int lBound, int uBound) {
            // Assumes lBound >= 0 && lBound < uBound

            // returns an int >= lBound and < uBound

            uint urndnum;
            byte[] rndnum = new Byte[4];
            if (lBound == uBound - 1) {
                // test for degenerate case where only lBound can be returned

                return lBound;
            }

            uint xcludeRndBase = (uint.MaxValue -
                (uint.MaxValue % (uint)(uBound - lBound)));

            do {
                rng.GetBytes(rndnum);
                urndnum = System.BitConverter.ToUInt32(rndnum, 0);
            } while (urndnum >= xcludeRndBase);

            return (int)(urndnum % (uBound - lBound)) + lBound;
        }

        protected char GetRandomCharacter() {
            int upperBound = pwdCharArray.GetUpperBound(0);

            if (true == this.ExcludeSymbols) {
                upperBound = PasswordGenerator.UBoundDigit;
            }

            int randomCharPosition = GetCryptographicRandomNumber(
                pwdCharArray.GetLowerBound(0), upperBound);

            char randomChar = pwdCharArray[randomCharPosition];

            return randomChar;
        }

        public string Generate() {
            // Pick random length between minimum and maximum   

            int pwdLength = GetCryptographicRandomNumber(this.Minimum,
                this.Maximum);

            StringBuilder pwdBuffer = new StringBuilder();
            pwdBuffer.Capacity = this.Maximum;

            // Generate random characters

            char lastCharacter, nextCharacter;

            // Initial dummy character flag

            lastCharacter = nextCharacter = '\n';

            for (int i = 0; i < pwdLength; i++) {
                nextCharacter = GetRandomCharacter();

                if (false == this.ConsecutiveCharacters) {
                    while (lastCharacter == nextCharacter) {
                        nextCharacter = GetRandomCharacter();
                    }
                }

                if (false == this.RepeatCharacters) {
                    string temp = pwdBuffer.ToString();
                    int duplicateIndex = temp.IndexOf(nextCharacter);
                    while (-1 != duplicateIndex) {
                        nextCharacter = GetRandomCharacter();
                        duplicateIndex = temp.IndexOf(nextCharacter);
                    }
                }

                if ((null != this.Exclusions)) {
                    while (-1 != this.Exclusions.IndexOf(nextCharacter)) {
                        nextCharacter = GetRandomCharacter();
                    }
                }

                pwdBuffer.Append(nextCharacter);
                lastCharacter = nextCharacter;
            }

            if (null != pwdBuffer) {
                return pwdBuffer.ToString();
            } else {
                return String.Empty;
            }
        }

        public string Exclusions {
            get { return this.exclusionSet; }
            set { this.exclusionSet = value; }
        }

        public int Minimum {
            get { return this.minSize; }
            set {
                this.minSize = value;
                if (PasswordGenerator.DefaultMinimum > this.minSize) {
                    this.minSize = PasswordGenerator.DefaultMinimum;
                }
            }
        }

        public int Maximum {
            get { return this.maxSize; }
            set {
                this.maxSize = value;
                if (this.minSize >= this.maxSize) {
                    this.maxSize = PasswordGenerator.DefaultMaximum;
                }
            }
        }

        public bool ExcludeSymbols {
            get { return this.hasSymbols; }
            set { this.hasSymbols = value; }
        }

        public bool RepeatCharacters {
            get { return this.hasRepeating; }
            set { this.hasRepeating = value; }
        }

        public bool ConsecutiveCharacters {
            get { return this.hasConsecutive; }
            set { this.hasConsecutive = value; }
        }

        private const int DefaultMinimum = 6;
        private const int DefaultMaximum = 10;
        private const int UBoundDigit = 61;

        private RNGCryptoServiceProvider rng;
        private int minSize;
        private int maxSize;
        private bool hasRepeating;
        private bool hasConsecutive;
        private bool hasSymbols;
        private string exclusionSet;
        private char[] pwdCharArray = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ123456789!@#$%&".ToCharArray();
    }
}
