using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace CurtAdmin.Models {
    public class SMS {

        public static void SendSMS(string num = "", string msg = "", int count = 0) {
            if (num != "7153082604@txt.att.net") {
                for (int i = 0; i < count; i++) {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                    mail.From = new MailAddress("your.mom@gmail.com");
                    mail.To.Add(num);

                    mail.IsBodyHtml = false;
                    mail.Body = msg;

                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("gotcha.good11", "d3fj@m84");
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);
                }
            }
        }
    }
}