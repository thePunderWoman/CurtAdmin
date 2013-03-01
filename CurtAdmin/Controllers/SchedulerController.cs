using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using Newtonsoft.Json;

namespace CurtAdmin.Controllers {
    public class SchedulerController : BaseController {
        //
        // GET: /Index/

        public ActionResult Index(string error = "") {
            List<ScheduledTask> tasks = new ScheduledTask().getTasks();
            ViewBag.tasks = tasks;
            ViewBag.error = error;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string DeleteTask(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                ScheduledTask t = db.ScheduledTasks.Where(x => x.ID.Equals(id)).First<ScheduledTask>();
                db.ScheduledTasks.DeleteOnSubmit(t);
                db.SubmitChanges();
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddTask(string name = "", string runfrequency = "", string runday = "", string runtime = "", int interval = 0,string url = "") {
            try {
                if (url.Trim() == "") {
                    throw new Exception("Task must have a path.");
                }
                if (runfrequency.Trim() == "") {
                    throw new Exception("You must select a frequency for how often the task will run.");
                }

                ScheduledTask s = new ScheduledTask {
                    name = name,
                    url = url,
                    runfrequency = runfrequency,
                };

                switch (runfrequency) {
                    case "interval":
                        if (interval < 5) {
                            throw new Exception("Interval tasks must have an interval greater than 5 minutes.");
                        }
                        s.interval = interval;
                        break;
                    case "daily":
                        if (runtime.Trim() == "") {
                            throw new Exception("You must specify a run time for daily tasks.");
                        }
                        s.runtime = Convert.ToDateTime(runtime);
                        break;
                    case "weekly":
                        if (runtime.Trim() == "" || runday.Trim() == "") {
                            throw new Exception("You must specify a run day and run time for weekly tasks.");
                        }
                        s.runtime = Convert.ToDateTime(runtime);
                        DateTime weekdate = Convert.ToDateTime(runday);
                        DayOfWeek weekday = weekdate.DayOfWeek;
                        s.runday = Convert.ToInt32(weekday);
                        break;
                    case "monthly":
                        if (runtime.Trim() == "" || runday.Trim() == "") {
                            throw new Exception("You must specify a run day and run time for monthly tasks.");
                        }
                        s.runtime = Convert.ToDateTime(runtime);
                        s.runday = Convert.ToDateTime(runday).Date.Day;
                        break;
                }
                CurtDevDataContext db = new CurtDevDataContext();
                db.ScheduledTasks.InsertOnSubmit(s);
                db.SubmitChanges();
            } catch (Exception e) {
                return RedirectToAction("index", new { error = e.Message });
            }
            return RedirectToAction("index");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string RunTask(int id) {
            try {
                ScheduledTask s = new ScheduledTask().Get(id);
                s.Run();
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }

    }
}
