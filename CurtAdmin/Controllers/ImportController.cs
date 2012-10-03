using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Threading;

namespace CurtAdmin.Controllers {
    public class ImportController : AsyncController {

        private List<int> _parts;

        public ActionResult Start(string partlist = "", int partcount = 0) {
            ImportService importService = new ImportService();
            ImportProcess currentProcess = importService.checkStatus();
            if (currentProcess.endTime != null) {
                ThreadPool.QueueUserWorkItem(o => ImportAsync(partlist,partcount));
            }
            ViewBag.status = currentProcess;
            return View();
        }

        public string StartAjax(string partlist = "", int partcount = 0) {
            ImportService importService = new ImportService();
            ImportProcess currentProcess = importService.checkStatus();
            if (currentProcess.endTime != null) {
                ThreadPool.QueueUserWorkItem(o => ImportAsync(partlist, partcount));
                return "Started";
            }
            return "Running";
        }

        public string CheckStatus() {
            ImportService importService = new ImportService();
            ImportProcess currentProcess = importService.checkStatus();
            return Newtonsoft.Json.JsonConvert.SerializeObject(currentProcess);
        }
        
        public void ImportAsync(string partlist = "", int partcount = 0) {
            ImportService importService = new ImportService();
            List<int> importedparts = new List<int>();
            List<int> localpartlist = new List<int>();
            if (partlist != "") {
                List<int> suppliedparts = new List<int>();
                string[] suppliedliststring = partlist.Split(',');
                foreach (string part in suppliedliststring) {
                    suppliedparts.Add(Convert.ToInt32(part));
                }
                this._parts = suppliedparts;
            } else {
                this._parts = importService.GetPartList(partcount);
            }
            localpartlist.AddRange(this._parts);
            if (importService.StartImport(_parts.Count)) {
                AsyncManager.OutstandingOperations.Increment(_parts.Count);
                foreach (int partID in localpartlist) {
                    importService.ImportImagesCompleted += (sender, e) => {
                        AsyncManager.OutstandingOperations.Decrement();
                        importedparts.Add(e.partID);
                        _parts.Remove(e.partID);
                        if (_parts.Count == 0) {
                            importService.FinishImport();
                        }
                        AsyncManager.Parameters["importedParts"] = importedparts;
                    };
                    importService.ImportImagesAsync(Server, partID, Guid.NewGuid());
                }
            }
        }

        public ActionResult ImportCompleted(List<int> importedParts = null) {
            List<int> parts = (importedParts != null) ? importedParts : new List<int>();
            ImportService importService = new ImportService();
            if (parts.Count == _parts.Count) {
                importService.FinishImport();
            }
            ViewBag.status = importService.checkStatus();
            ViewBag.parts = parts;
            return View();
        }

    }

}
