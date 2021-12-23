#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.ViewModel.Invoices;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Vouchers;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class PIInvoiceController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public PIInvoiceController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        #endregion
    }


}