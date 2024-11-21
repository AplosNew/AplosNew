#region Using
using Aplos.Controllers;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;
using Library.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessSetController : BaseController
    {
        #region --Constructor
        private readonly IProcessSetService _processSetService;
        private readonly IProcessSetDetailService _processSetDetailService;

        public ProcessSetController(
             IProcessSetService processSetService
           , IProcessSetDetailService processSetDetailService
            )
        {
            _processSetService = processSetService;
            _processSetDetailService = processSetDetailService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult ProcessSetReportPage()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string companyId, string entityId)
        {
            return Json(_processSetService.Query(parameters, companyId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDataList(GridParameter parameters, string companyGroupId)
        {
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_processSetService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetListByCompany(GridParameter parameters, string companyId, string entityId)
        {
            return Json(_processSetService.QueryByCompany(parameters, companyId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProcessSetListByCompany(GridParameter parameters, string companyId)
        {
            return Json(_processSetService.GetProcessSetListByCompany(parameters, companyId), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Process set detail by process set id.
        /// </summary>
        /// <param name="processSetId"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult GetProcessSetDetailList(string processSetId)
        {
            return Json(_processSetDetailService.Query(processSetId), JsonRequestBehavior.AllowGet);
        }
		/// <summary>
		/// in material master
		/// </summary>
		/// <param name="processSetId"></param>
		/// <returns></returns>
		[HttpGet, Authorize]
		public JsonResult GetProcessSetList(string processSetId,string entityId)
		{
			return Json(_processSetDetailService.GetProcessSetList(processSetId, entityId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
        public JsonResult Create(ProcessSet processSet, IEnumerable<ProcessSetDetail> processSetDetail)
        {
            _processSetService.InsertGraph(processSet, processSetDetail);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ProcessSet processSet, IEnumerable<ProcessSetDetail> processSetDetail)
        {
            _processSetService.UpdateGraph(processSet, processSetDetail);
            return Json(new { Message = AplosMessage.Updated });
        }
        public ActionResult Delete(string id)
        {
            _processSetService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion

        #region Report
        public ActionResult ProcessSetReport(string companyId, string entityId, string process)
        {
            string fileName;
            if (process == "Process")
            {
                fileName = "Process Report " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            }
            else
            {
                fileName = "Process Set Report " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            }
            IWorkbook workbook = _processSetService.GetProcessSetReport(companyId, entityId, process);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
        #endregion
    }
}