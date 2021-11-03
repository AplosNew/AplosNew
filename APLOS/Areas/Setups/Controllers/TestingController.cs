using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class TestingController : BaseController
    {
        #region -- Constructor

        private readonly ITestingService _testingService;

        public TestingController(ITestingService testingService)
        {
            this._testingService = testingService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters, string testingCategoryId)
        {
            return Json(_testingService.Query(parameters, testingCategoryId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTestingData(GridParameter parameters, string testingCategoryId, string testingStandardId)
        {
            return Json(_testingService.GetTestingData(parameters, testingCategoryId, testingStandardId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo(string testingCategoryId)
        {
            return Json(_testingService.GetCbo(testingCategoryId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_testingService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTesting()
        {
            return Json(_testingService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Testing testing)
        {
            _testingService.Insert(testing);
            return Json(new { Testing = testing, Sequence = _testingService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Testing testing)
        {
            _testingService.Update(testing);
            return Json(new { Sequence = _testingService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _testingService.Delete(id);
            return Json(new { Sequence = _testingService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}