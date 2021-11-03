using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Setups;
using Library.Service.Setups;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
	public class TestingStandardController : BaseController
	{
		#region -- Constructor

		private readonly ITestingStandardService _testingStandardService;
		private readonly ITestingStandardDetailService _testingStandardDetailService;
		private readonly ITestingStandardBuyerService _testingStandardBuyerService;

		public TestingStandardController(ITestingStandardService testingStandardService
			, ITestingStandardDetailService testingStandardDetailService
			, ITestingStandardBuyerService testingStandardBuyerService)
		{
			this._testingStandardService = testingStandardService;
			this._testingStandardDetailService = testingStandardDetailService;
			this._testingStandardBuyerService = testingStandardBuyerService;
		}

		#endregion -- Constructor

		#region Pages

		[Authorize]
		public ActionResult Aplos()
		{
			return View();
		}

		[Authorize]
		public ActionResult TestingStandardReportPage()
		{
			return View();
		}

		#endregion Pages

		#region -- Operations

		[Authorize, HttpGet]
		public JsonResult GetList(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_testingStandardService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetCbo(string companyGroupId)
		{
			return Json(_testingStandardService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
		}


        [HttpGet, Authorize]
        public JsonResult GetCboWithBuyer(string companyGroupId)
        {
            return Json(_testingStandardService.GetCboWithBuyer(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
		public JsonResult GetTestingStandard()
		{
			return Json(_testingStandardService.Query().Select(), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetTestingStandardDetailWithTSId(GridParameter parameters, string testingStandardId)
		{
			return Json(_testingStandardDetailService.QueryForTestingStandardDetailWithTSId(parameters, testingStandardId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetTestingStandardDetail(string testingStandardId)
		{
			return Json(_testingStandardDetailService.QueryForTestingStandardDetail(testingStandardId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetTestingStandardById(GridParameter parameters, string id)
		{
			return Json(_testingStandardService.FindById(parameters, id), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(TestingStandard testingStandard, IEnumerable<TestingStandardDetail> testingStandardDetail, IEnumerable<TestingStandardBuyer> testingStandardBuyer)
		{
			string testingStandardId = _testingStandardService.InsertAndUpdate(testingStandard, testingStandardDetail, testingStandardBuyer);
			return Json(new { TestingStandardId = testingStandardId, Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult Edit(TestingStandard testingStandard)
		{
			_testingStandardService.Update(testingStandard);
			return Json(new { Message = AplosMessage.Updated });
		}

		[HttpPost]
		public JsonResult Delete(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				_testingStandardService.DeleteGraph(id);
				return Json(new { Message = AplosMessage.Deleted });
			}
			else
				throw new CustomException(Resources.IdNotFound);
		}

		[HttpPost]
		public JsonResult DeleteTestingStandardDetail(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				_testingStandardDetailService.Delete(id);
				return Json(new { Message = AplosMessage.Deleted });
			}
			else
				throw new CustomException(Resources.IdNotFound);
		}

		[HttpPost]
		public JsonResult DeleteTestingStandardBuyer(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				_testingStandardBuyerService.Delete(id);
				return Json(new { Message = AplosMessage.Deleted });
			}
			else
				throw new CustomException(Resources.IdNotFound);
		}

		[Authorize, HttpGet]
		public JsonResult GetTestingStandardBuyer(string testingStandardId)
		{
			return Json(_testingStandardBuyerService.QueryForTestingStandardBuyer(testingStandardId), JsonRequestBehavior.AllowGet);
		}

		#endregion -- Operations

		#region Report

		public ActionResult TestingStandardReport(string testing)
		{
			string fileName = "Testing Standard Report " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
			IWorkbook workbook = _testingStandardService.GetTestingStandardReport(testing);
			workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
			return null;
		}

		#endregion Report
	}
}