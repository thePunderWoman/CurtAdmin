using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;

namespace CurtAdmin.Models {
    public class ImportService {

        public bool StartImport() {
            CurtDevDataContext db = new CurtDevDataContext();
            ImportProcess currentProcess = db.ImportProcesses.Where(x => x.endTime.Equals(null)).FirstOrDefault<ImportProcess>();
            int count = db.Parts.Count();
            if (currentProcess != null && currentProcess.ID > 0) {
                return false;
            } else {
                ImportProcess newProcess = new ImportProcess {
                    startTime = DateTime.Now,
                    partCount = count,
                    currentCount = 0
                };
                db.ImportProcesses.InsertOnSubmit(newProcess);
                db.SubmitChanges();
                return true;
            }
        }

        public void IncrementCount() {
            CurtDevDataContext db = new CurtDevDataContext();
            ImportProcess currentProcess = db.ImportProcesses.Where(x => x.endTime.Equals(null)).FirstOrDefault<ImportProcess>();
            if (currentProcess != null && currentProcess.ID > 0) {
                currentProcess.currentCount = currentProcess.currentCount + 1;
                db.SubmitChanges();
            }
        }
        
        public ImportProcess checkStatus() {
            CurtDevDataContext db = new CurtDevDataContext();
            ImportProcess currentProcess = db.ImportProcesses.Where(x => x.endTime.Equals(null)).FirstOrDefault<ImportProcess>();
            if (currentProcess == null || currentProcess.ID == 0) {
                currentProcess = db.ImportProcesses.OrderByDescending(x => x.endTime).FirstOrDefault<ImportProcess>();
            }
            currentProcess.average = getAverageTime();
            return currentProcess;
        }

        public int getAverageTime() {
            List<int> milliseconds = new List<int>();
            CurtDevDataContext db = new CurtDevDataContext();

            List<ImportProcess> processes = db.ImportProcesses.Where(x => !x.endTime.Equals(null)).ToList<ImportProcess>();
            foreach (ImportProcess process in processes) {
                double t = ((DateTime)process.endTime - process.startTime).TotalMilliseconds;
                milliseconds.Add(Convert.ToInt32(t / process.partCount));
            }
            return Convert.ToInt32(milliseconds.Average());
        }

        public List<int> GetImportList() {
            CurtDevDataContext db = new CurtDevDataContext();
            ImportProcess process = checkStatus();
            int startpoint = 0;
            if (process != null && process.ID != 0) {
                startpoint = process.currentCount;
            }
            List<int> ids = db.Parts.OrderBy(x => x.partID).Select(x => x.partID).Skip(startpoint).Take(10).ToList();
            return ids;
        }

        public List<int> GetPartList(int partcount = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            List<int> parts = new List<int>();
            if (partcount == 0) {
                parts = db.Parts.Select(x => x.partID).ToList<int>();
            } else {
                parts = db.Parts.Select(x => x.partID).Take(partcount).ToList<int>();
            }
            return parts;
        }

        public void ImportImagesAsync(HttpServerUtilityBase server, int partid, object userState) {
            string directory = server.MapPath("/masterlibrary/" + partid.ToString() + "/images/");
            try {
                string[] filenames = Directory.GetFiles(@directory);
                List<string> files = new List<string>();
                List<PartImage> newimgs = new List<PartImage>();
                foreach (string file in filenames) {
                    files.Add(file.Substring(file.IndexOf("masterlibrary") - 1));
                }

                CurtDevDataContext db = new CurtDevDataContext();

                List<PartImage> oldimages = db.PartImages.Where(x => x.partID == partid).ToList<PartImage>();
                db.PartImages.DeleteAllOnSubmit(oldimages);
                db.SubmitChanges();

                List<PartImageSize> sizes = db.PartImageSizes.ToList<PartImageSize>();

                foreach (PartImageSize size in sizes) {
                    foreach (string file in files) {
                        if (file.IndexOf(size.dimensions) != -1) {
                            try {
                                int period = file.IndexOf(".");
                                int start = file.IndexOf("_", file.IndexOf(size.dimensions)) + 1;
                                string sort = file.Substring(start, (period - start));
                                Image i = Image.FromFile(server.MapPath(file));

                                PartImage img = new PartImage {
                                    sizeID = size.sizeID,
                                    sort = Convert.ToChar(sort),
                                    path = "https://www.curtmfg.com" + file.Replace('\\', '/'),
                                    partID = partid,
                                    height = i.Height,
                                    width = i.Width
                                };
                                newimgs.Add(img);
                            } catch { }
                        }
                    }
                }
                db.PartImages.InsertAllOnSubmit(newimgs);
                db.SubmitChanges();

            } catch { };
            IncrementCount();
            /*
            * These are some junk response objects for testing
            * Evetually we will populate this will slightly less abstract data. In the end, it's really meaningless unless we need to update the status or something similar
            */
            object sender = new object();
            ImportImagesCompletedEventArgs args = new ImportImagesCompletedEventArgs(new Exception("Hit end"), false, userState, partid);

            // Invoke the completion of this Async action
            ImportImagesCompleted.Invoke(sender, args);

        }

        public string ImportImages(HttpServerUtilityBase server, int partid = 0) {
            try {
                string directory = server.MapPath("/masterlibrary/" + partid.ToString() + "/images/");
                string[] filenames = Directory.GetFiles(@directory);
                List<string> files = new List<string>();
                List<PartImage> newimgs = new List<PartImage>();
                foreach (string file in filenames) {
                    files.Add(file.Substring(file.IndexOf("masterlibrary") - 1));
                }

                CurtDevDataContext db = new CurtDevDataContext();

                List<PartImage> oldimages = db.PartImages.Where(x => x.partID == partid).ToList<PartImage>();
                db.PartImages.DeleteAllOnSubmit(oldimages);
                db.SubmitChanges();

                List<PartImageSize> sizes = db.PartImageSizes.ToList<PartImageSize>();

                foreach (PartImageSize size in sizes) {
                    foreach (string file in files) {
                        if (file.IndexOf(size.dimensions) != -1) {
                            try {
                                int period = file.IndexOf(".");
                                int start = file.IndexOf("_", file.IndexOf(size.dimensions)) + 1;
                                string sort = file.Substring(start, (period - start));
                                Image i = Image.FromFile(server.MapPath(file));

                                PartImage img = new PartImage {
                                    sizeID = size.sizeID,
                                    sort = Convert.ToChar(sort),
                                    path = "https://www.curtmfg.com" + file.Replace('\\', '/'),
                                    partID = partid,
                                    height = i.Height,
                                    width = i.Width
                                };
                                newimgs.Add(img);
                            } catch { }
                        }
                    }
                }
                db.PartImages.InsertAllOnSubmit(newimgs);
                db.SubmitChanges();
            } catch { };
            return "done";
        }

        public event ImportImagesCompletedEventHandler ImportImagesCompleted;

        public void FinishImport() {
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                ImportProcess currentProcess = db.ImportProcesses.Where(x => x.endTime.Equals(null)).FirstOrDefault<ImportProcess>();
                currentProcess.endTime = DateTime.Now;
                db.SubmitChanges();
            } catch { };
        }

    }

    public delegate void ImportImagesCompletedEventHandler(object sender, ImportImagesCompletedEventArgs e);

    public class ImportImagesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {

        public int partID { get; private set; }

        public ImportImagesCompletedEventArgs(Exception error, bool cancelled, object userState, int partnumber)
            : base(error, cancelled, userState) {
            this.partID = partnumber;
        }
    }

}