using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using System.Threading;
using System.Web.Mvc;
using Library.Core;
using Library.Accounting.Accounts;
using Library.Data.Sql;
using System;
using Library.Data;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Reflection;
using System.Collections.Generic;

namespace Aplos.Areas.Accounts.Controllers
{
    public class PostInvoiceController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;

        public PostInvoiceController(
             ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }


        public ActionResult Aplos()
        {
            return View();
        }

        

        #region GRN Operation

        [Authorize, HttpGet]
        public JsonResult GetListForInvPayable()
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetGRNListForPostInvoice(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}