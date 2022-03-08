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
using Library.Service.Helpers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class InvoiceToAcceptancePostController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        private readonly IAutoLoanService _autoLoanService;
        clsInvoiceTagWithLc ep = new clsInvoiceTagWithLc();
		public InvoiceToAcceptancePostController()
		{
			_sqlRepository = new SqlRepository();
		}
		#endregion

		#region -- Pages

		[HttpGet, Authorize]
		public ActionResult InvoiceToAcceptancePost()
		{
			return View("~/Areas/Commercial/Views/InvoiceTaggedWithLC/InvoiceToAcceptancePost.cshtml");
		}

		#endregion

		#region Operation


		#endregion

	}
}