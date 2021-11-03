#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Setups;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LegalSalaryStructureController : BaseController
    {
        #region -- Constructor

        private readonly ILegalSalaryStructureService _legalSalaryGradeService;

        public LegalSalaryStructureController(ILegalSalaryStructureService legalSalaryGradeService)
        {
            _legalSalaryGradeService = legalSalaryGradeService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult LegalSalaryReportPage()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string legalSalaryGradeId)
        {
            return Json(_legalSalaryGradeService.Query(parameters, legalSalaryGradeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHeadList(string legalSalaryGradeId)
        {
            return Json(_legalSalaryGradeService.GetHeadList(legalSalaryGradeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHeadEdit(string id)
        {
            return Json(_legalSalaryGradeService.GetHeadEdit(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(LegalSalaryStructure entity, IEnumerable<LegalSalaryStructureValue> values)
        {
            _legalSalaryGradeService.InsertOrUpdateGraph(entity, values);
            return Json(new { entity.Id, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _legalSalaryGradeService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

        #region Report

        public ActionResult LegalSalaryReport(string effectiveDate, string plantId)
        {
            string fileName = "Legal Salary Report " + DateTime.Now.ToString("ddMMMyyyy") + "";
            IWorkbook workbook = _legalSalaryGradeService.GetLegalSalaryReport(effectiveDate, plantId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        #endregion Report
    }
}