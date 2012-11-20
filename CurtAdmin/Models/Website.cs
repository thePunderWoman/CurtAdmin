using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin {
    partial class Website {

        public List<Website> GetAll() {
            CurtDevDataContext db = new CurtDevDataContext();
            List<Website> websites = db.Websites.ToList<Website>();
            return websites;
        }

        public Website Get(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            Website website = db.Websites.Where(x => x.ID.Equals(id)).FirstOrDefault<Website>();
            return website;
        }
    }
}