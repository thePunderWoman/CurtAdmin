using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;


namespace CurtAdmin.Models
{
    public class helpers
    {
        // helpers class is used to extend functionality to the CURT Admin project by including methods that are commonly used

        public static void SendEmail(string email, string subject, string htmlBody, bool isBodyHTML = false)
        {
            try
            {
                SmtpClient SmtpServer = new SmtpClient();
                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.Subject = subject;
                mail.IsBodyHtml = isBodyHTML;
                mail.Body = htmlBody;
                SmtpServer.Send(mail);
            }
            catch (Exception e)
            {
                throwException("Error Sending Email" + e.Message);
            }
        }


        public static void throwException(string msg)
        {
            // plan to add more functionality later.
            throw new Exception(msg);
        }





    }
}