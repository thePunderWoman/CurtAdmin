/*
 * Author       : Alex Ninneman
 * Created      : January 20, 2011
 * Description  : This controller will handle the search actions throughout the site.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Data.Linq.SqlClient;
using System.Web.Script.Serialization;

namespace CurtAdmin.Controllers
{
    public class SearchController : BaseController
    {
        /// <summary>
        /// This is just a generic index function for this controller. We won't be using it.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// This function will serve as an AJAX response function to populate our dropdown menu for search results.
        /// </summary>
        /// <param name="search_term">The search query.</param>
        /// <returns>List of SearchObjects.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string Search(string search_term) {

            string searchResults = "";
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            CurtDevDataContext dev_db  = new CurtDevDataContext();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<SearchObject> users = new List<SearchObject>();
            List<SearchObject> cat_results = new List<SearchObject>();

            // Get the users based on the search term
            string[] names = search_term.Split(' ');
            string fname = names[0];
            string lname = "";
            if (names.Length > 1) {
                lname = names[1];
                users = (from u in doc_db.users
                             where u.fname.Contains(fname) &&
                             u.lname.Contains(lname)
                             orderby u.lname
                             select new SearchObject{
                                 id = u.userID.ToString(),
                                 term = u.fname + " " + u.lname,
                                 type = "user"
                             }).ToList<SearchObject>();
                //searchResults = serializer.Serialize(users);
            } else {
                users = (from u in doc_db.users
                             where u.fname.Contains(fname) || u.lname.Contains(fname)
                             orderby u.lname
                             select new SearchObject{
                                 id = u.userID.ToString(),
                                 term = u.fname + " " + u.lname,
                                 type = "user"
                             }).ToList<SearchObject>();
            }


            // Search the category names
            string[] cat_terms = search_term.Split(' ');
            if (cat_terms.Length > 0) {
                List<SearchObject> cats = new List<SearchObject>();
                foreach (string cat_term in cat_terms) {
                    cats = (from c in doc_db.docCategories
                            where c.catName.Contains(cat_term)
                            select new SearchObject {
                                id = c.catID.ToString(),
                                term = c.catName,
                                type = "category"
                            }).ToList<SearchObject>();
                    cat_results = cat_results.Union(cats).ToList<SearchObject>();
                }
            }

            // Search part ids
            string partID = search_term;
            List<SearchObject> parts = new List<SearchObject>();
            parts = (from p in dev_db.Parts
                        where p.partID.ToString().Contains(partID)
                        select new SearchObject {
                            id = p.partID.ToString(),
                            term = p.partID.ToString(),
                            type = "part"
                        }).ToList<SearchObject>();
            cat_results = cat_results.Union(parts).ToList<SearchObject>();

            List<SearchObject> cat_user_results = users.Union(cat_results).ToList<SearchObject>();
            searchResults = serializer.Serialize(cat_user_results);
            return searchResults;
        }

        /// <summary>
        /// Store a search result
        /// </summary>
        /// <remarks></remarks>
        public class SearchObject {
            public string id { get; set; }
            public string term { get; set; }
            public string type { get; set; }
        }

    }
}
