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

        /*public ActionResult Start(string partlist = "", int partcount = 0) {
            ImportService importService = new ImportService();
            ImportProcess currentProcess = importService.checkStatus();
            if (currentProcess.endTime != null) {
                ThreadPool.QueueUserWorkItem(o => ImportAsync(partlist,partcount));
            }
            ViewBag.status = currentProcess;
            return View();
        }*/

        public string StartAjax() {
            ImportService importService = new ImportService();
            ImportProcess currentProcess = importService.checkStatus();
            if (currentProcess.endTime != null) {
                importService.StartImport();
                ThreadPool.QueueUserWorkItem(o => ImportAsync());

                return "Started";
            }
            return "Running";
        }

        public string CheckStatus() {
            ImportService importService = new ImportService();
            ImportProcess currentProcess = importService.checkStatus();
            return Newtonsoft.Json.JsonConvert.SerializeObject(currentProcess);
        }

        public void ContinueProcessing() {
            ThreadPool.QueueUserWorkItem(o => ImportAsync());
        }

        public void ImportAsync() {
            ImportService importService = new ImportService();
            List<int> importedparts = importService.GetImportList();
            List<int> localpartlist = new List<int>();
            localpartlist.AddRange(importedparts);
            if (importedparts.Count > 0) {
                AsyncManager.OutstandingOperations.Increment(localpartlist.Count);
                int count = AsyncManager.OutstandingOperations.Count;
                foreach (int partID in localpartlist) {
                    importService.ImportImagesCompleted += (sender, e) => {
                        bool removed = importedparts.Remove(e.partID);
                        if (removed) {
                            AsyncManager.OutstandingOperations.Decrement();
                        }
                        count = AsyncManager.OutstandingOperations.Count;
                        if (count == 0 && removed) {
                            ImportCompleted();
                        }
                    };
                    importService.ImportImagesAsync(Server, partID, Guid.NewGuid());
                }
            }
        }

        public void ImportCompleted() {
            ImportService importService = new ImportService();
            ImportProcess process = importService.checkStatus();
            if (process.currentCount == process.partCount) {
                importService.FinishImport();
            } else {
                ThreadPool.QueueUserWorkItem(o => ImportAsync());
            }
        }

    }

}
