using System;
using System.IO;
using System.Web;

namespace CurtAdmin.Models {
    public class Logger {

        public static string GetTempPath() {
            string path = HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\logs\\";
            return path;
        }

        public static void LogMessageToFile(string msg) {
            if (System.Configuration.ConfigurationManager.AppSettings["logging"] == "true") {
                System.IO.StreamWriter sw = System.IO.File.AppendText(
                    GetTempPath() + "log.txt");
                try {
                    string logLine = System.String.Format(
                        "{0:G}: {1}.", System.DateTime.Now, msg);
                    sw.WriteLine(logLine);
                } finally {
                    sw.Close();
                }
            }
        }
    }
}