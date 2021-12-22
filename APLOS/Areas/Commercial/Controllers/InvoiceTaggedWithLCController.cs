#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.MaterialManagement.Material;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Security.Core;
using Library.Service.Finances;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class InvoiceTaggedWithLCController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IAutoLoanService _autoLoanService;
        clsInvoiceTagWithLc ep = new clsInvoiceTagWithLc();
        public InvoiceTaggedWithLCController( ISqlRepository R
           , IAutoLoanService autoLoanService
            )
        {
            _sqlRepository = R;
            _autoLoanService = autoLoanService;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
		}

		#endregion

		#region Operation

		[HttpGet, Authorize]
		public ActionResult GetVendorAvailableInvoiceList()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var jsondata = Json(ep.VendorAvailableInvoiceList(identity.CompanyGroupId,identity.CompanyId), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
        }
		

		#endregion

	}
}