using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.Service.Administration.Contract;
using Aplos.Properties;


namespace Aplos.Areas.Administration.Controllers
{
    public class GeneralContractReportController : BaseController
    {
        ContractReportService cr = new ContractReportService();
        public ActionResult Aplos()
        {
            return View();
        }


    }
}