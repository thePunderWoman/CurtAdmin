/*
 * Author       : Alex Ninneman
 * Created      : January 15, 2011
 * Description  : This model will hold the functionality for working with the modules for this application.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;
using System.Data.Linq;

namespace CurtAdmin.Models {
    public class Mods {

        /// <summary>
        /// Gets module by name.
        /// </summary>
        /// <param name="module1">The module1.</param>
        /// <returns>module</returns>
        /// <remarks></remarks>
        public static module GetModuleByName(string module1){
            module mod = new module();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            mod = (from m in doc_db.modules
                 where m.module1.Equals(module1)
                 select m).FirstOrDefault<module>();

            return mod;
        }

    }
}