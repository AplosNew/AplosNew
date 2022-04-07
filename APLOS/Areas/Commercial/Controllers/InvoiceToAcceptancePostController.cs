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

		[HttpGet]
		public ActionResult InvoiceToAcceptancePost()
		{
			return View("~/Areas/Commercial/Views/InvoiceTaggedWithLC/InvoiceToAcceptancePost.cshtml");
		}

		#endregion

		#region Operation
		[HttpGet, Authorize]
		public JsonResult GetVendorAvailableInvoiceListForInvoiceToAcceptancePost(GridParameter parameters, string partyId)
		{
			AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_accountsInvoiceService.GetVendorAvailableInvoiceListForInvoiceToAcceptancePost(parameters, identity.CompanyGroupId, identity.CompanyId, partyId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult InvoiceToAcceptancePostReport(ReportFormat reportFormat, string voucherId)
		{
			AccountsInvoiceReportService _accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var workbook = _accountsInvoiceReportService.GetAcceptancePostReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
			switch (reportFormat)
			{
				case ReportFormat.Pdf:
					return RenderReportAsPdf(workbook, reportFileName);

				case ReportFormat.Excel:
					return RenderReportAsExcel(workbook, reportFileName);

				default:
					return RenderReportAsExcel(workbook, reportFileName);
			}
		}
		#endregion

	}
}