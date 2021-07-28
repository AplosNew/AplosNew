using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class CompanyWiseBankSheetController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public CompanyWiseBankSheetController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
               
        public ActionResult Aplos()
        {
            return View();
        }
                

    }
}