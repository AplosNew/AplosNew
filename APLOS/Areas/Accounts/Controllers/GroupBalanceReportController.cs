using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Advances;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Model.Vouchers;
using Library.Service.Advances;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Accounts.Controllers
{
    public class GroupBalanceReportController : BaseController
    {
        //private readonly IVoucherService _voucharService;
        //private readonly IVoucherReportService _voucharReportService;
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly ISqlRepository _sqlRepository;
        //private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        //private readonly IAdvanceService _advanceService;
        //private readonly IEmployeePayableService _employeePayableService;
        //private readonly IPKGeneratorService _pkGeneratorService;
        //private readonly IFiscalYearService _fiscalYearService;
        //private readonly ICompanyFiscalYearService _companyFiscalYearService;
        //private readonly ICompanyTaxYearService _companyTaxYearService;
        //private readonly AccountVoucherReportService _accountVoucherReportService;
        //private readonly IRepositoryAsync<EmployeeSalaryAdvance> _employeeSalaryAdvanceRepository;
        private readonly GroupBalanceReportService _groupBalanceReportService;
        private readonly AccountVoucherReportService _accountVoucherReportService;


        public GroupBalanceReportController(
            GroupBalanceReportService groupBalanceReportService
            ,AccountVoucherReportService accountVoucherReportService
            )
        {
           _groupBalanceReportService = groupBalanceReportService;
           _accountVoucherReportService = accountVoucherReportService;
        }


        public ActionResult Aplos()
        {
            return View();
        }

        //General ledger report
        [HttpGet, Authorize]
        public ActionResult GetGroupBalanceReport(ReportFormat reportFormat, string glId, string budgetMasterId, string fromDate, string toDate, bool isActivity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = null;
            //string activityId = null;
            if (budgetMasterId=="" || budgetMasterId == "null")
            {
                budgetMasterId = null;
            }

            if (isActivity == true && budgetMasterId == null)
            {
                workbook = _groupBalanceReportService.GetGeneralLedgerGroupWithBudgetActivityReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, fromDate, toDate);
            }
            else  
            {
                workbook = _groupBalanceReportService.GetGeneralLedgerGroupReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, fromDate, toDate);
            }
            
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Group Ledger Report";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }

        }

    }


}