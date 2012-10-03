using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models {
    public class TrailerModel {

        public static List<Trailer> GetAll() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<Trailer> trailers = new List<Trailer>();

                trailers = db.Trailers.OrderBy(x => x.TW).ToList<Trailer>();

                return trailers;        
            } catch (Exception e) {
                return new List<Trailer>();
            }
        }

        public static List<Trailer> GetTrailersByLifestyle(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<Trailer> trailers = new List<Trailer>();

                trailers = (from t in db.Trailers
                            join lt in db.Lifestyle_Trailers on t.trailerID equals lt.trailerID
                            where lt.catID.Equals(id)
                            orderby t.TW
                            select t).ToList<Trailer>();

                return trailers;
            } catch (Exception e) {
                return new List<Trailer>();
            }
        }

        public static List<Trailer> GetUnusedTrailersByLifestyle(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<Trailer> trailers = new List<Trailer>();

                List<Trailer> alltrailers = GetAll();
                List<Trailer> exclusions = GetTrailersByLifestyle(id);
                trailers = alltrailers.Except(exclusions, new trailerComparer()).ToList<Trailer>();
                return trailers;
            } catch (Exception e) {
                return new List<Trailer>();
            }
        }

        public static Trailer Get(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                Trailer trailer = new Trailer();
                trailer = db.Trailers.Where(x => x.trailerID == id).FirstOrDefault<Trailer>();

                return trailer;
            } catch (Exception e) {
                return new Trailer();
            }
        }

        public static Trailer Save(List<string> lifestyles, int trailerID = 0, string name = "", string image = "", int tw = 0, int gtw = 0, string hitchClass = "", string shortDesc = "", string message = "") {

            CurtDevDataContext db = new CurtDevDataContext();
            Trailer t = new Trailer();

            // Validate the form fields
            if (name.Length == 0) { throw new Exception("You Must Enter a name."); }
            if (tw == 0) { throw new Exception("You Must Enter a tongue weight."); }
            if (hitchClass.Length == 0) { throw new Exception("You Must choose a hitch class."); }
            if (shortDesc.Length == 0) { throw new Exception("You must enter a short description."); }

            if (trailerID != 0) { // Updating a category
                // Get the category
                t = (from tr in db.Trailers
                       where tr.trailerID.Equals(trailerID)
                       select tr).SingleOrDefault<Trailer>();
                List<Lifestyle_Trailer> lt = db.Lifestyle_Trailers.Where(x => x.trailerID.Equals(t.trailerID)).ToList<Lifestyle_Trailer>();
                db.Lifestyle_Trailers.DeleteAllOnSubmit(lt);
            }

            // Update the fields
            t.name = name;
            t.image = image;
            t.TW = tw;
            t.GTW = gtw;
            t.hitchClass = hitchClass;
            t.shortDesc = shortDesc;
            t.message = message;

            if (trailerID == 0) {
                db.Trailers.InsertOnSubmit(t);
            }

            // Save the changes
            db.SubmitChanges();

            List<Lifestyle_Trailer> new_lts = new List<Lifestyle_Trailer>();

            foreach (string lifestyle in lifestyles) {
                Lifestyle_Trailer new_lt = new Lifestyle_Trailer {
                    trailerID = t.trailerID,
                    catID = Convert.ToInt32(lifestyle)
                };
                new_lts.Add(new_lt);
            }
            db.Lifestyle_Trailers.InsertAllOnSubmit(new_lts);
            db.SubmitChanges();

            return t;
        }

        public static List<string> getTrailerLifestyles(int id = 0) {
            List<string> lifestyles = new List<string>();
            CurtDevDataContext db = new CurtDevDataContext();
            try {
                lifestyles = db.Lifestyle_Trailers.Where(x => x.trailerID.Equals(id)).Select(x => x.catID.ToString()).ToList();
            } catch { }
            return lifestyles;
        }

        public static List<Trailer> AddToLifestyle(string[] trailers, int catID) {
            List<Trailer> t = new List<Trailer>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<Lifestyle_Trailer> lts = new List<Lifestyle_Trailer>();
                foreach (string trailer in trailers) {
                    Lifestyle_Trailer new_lt = new Lifestyle_Trailer {
                        catID = catID,
                        trailerID = Convert.ToInt32(trailer)
                    };
                    lts.Add(new_lt);
                    t.Add(Get(new_lt.trailerID));
                }
                db.Lifestyle_Trailers.InsertAllOnSubmit(lts);
                db.SubmitChanges();

            } catch { }
            return t;
        }

        public static void RemoveFromLifestyle(int trailerid, int catID) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                Lifestyle_Trailer lt = db.Lifestyle_Trailers.Where(x => x.trailerID == trailerid).Where(x => x.catID == catID).First<Lifestyle_Trailer>();
                db.Lifestyle_Trailers.DeleteOnSubmit(lt);
                db.SubmitChanges();
            } catch (Exception e) {
                throw e;
            }
        }

        public static void Delete(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                Trailer t = db.Trailers.Where(x => x.trailerID == id).FirstOrDefault<Trailer>();
                List<Lifestyle_Trailer> lt = db.Lifestyle_Trailers.Where(x => x.trailerID == t.trailerID).ToList<Lifestyle_Trailer>();
                db.Lifestyle_Trailers.DeleteAllOnSubmit(lt);
                db.Trailers.DeleteOnSubmit(t);
                db.SubmitChanges();
            } catch (Exception e) {
                throw e;
            }
        }

    }

    public class trailerComparer : IEqualityComparer<Trailer> {
        public bool Equals(Trailer a, Trailer b) {
            return a.trailerID == b.trailerID;
        }

        public int GetHashCode(Trailer trailer) {
            return trailer.trailerID.GetHashCode();
        }
    }
}