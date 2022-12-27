using Aplos.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Materials.Controllers
{
    public class ScanDataController : BaseController
    {
        #region Constructor
        public ScanDataController()
        {

        }
        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page
    }
}