using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class EncashmentController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IEncashmentService _EncashmentService;
        public EncashmentController(
               IEncashmentService EncashmentService,
            ISqlRepository sqlRepository
            )
        {
            _EncashmentService = EncashmentService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult EarnLeaveReport()
        {
            return View();
        }
        public ActionResult ELReport()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        #region Encashment Report

        [HttpGet]
        public ActionResult GetEncashReport(ReportFormat reportFormat, string YearNo)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _EncashmentService.GetEncashReport(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, YearNo);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Encashment Report";
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
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            
        }

        [HttpGet]
        public ActionResult GetEarnLeaveReport(ReportFormat reportFormat, string YearNo, bool isDetail, bool isActive,bool isSeperated)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _EncashmentService.GetEarnLeaveReport(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, YearNo.Trim(), isDetail,isActive,isSeperated);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Earn Leave Report";
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
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion  Encashment Report

        #endregion -- Operations  
    }
}