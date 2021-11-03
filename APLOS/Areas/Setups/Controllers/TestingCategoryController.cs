#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class TestingCategoryController : BaseController
    {
        #region Constructor

        private readonly ITestingCategoryService _testingCategoryService;

        public TestingCategoryController(
              ITestingCategoryService testingCategoryService
            )
        {
            _testingCategoryService = testingCategoryService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_testingCategoryService.GetCbo(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_testingCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_testingCategoryService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(TestingCategory model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupId = identity.CompanyGroupId;
            _testingCategoryService.Insert(model);
            return Json(new { TestingCategory = model, Sequence = _testingCategoryService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(TestingCategory model)
        {
            _testingCategoryService.Update(model);
            return Json(new { Sequence = _testingCategoryService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _testingCategoryService.Delete(id);
            return Json(new { Sequence = _testingCategoryService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}