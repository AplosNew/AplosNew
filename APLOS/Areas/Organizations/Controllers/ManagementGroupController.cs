#region Using

using Aplos.Controllers;
using Library.Service.Organizations;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class ManagementGroupController : BaseController
    {
        #region Constructor

        private readonly IManagementGroupService _managementgroupService;

        public ManagementGroupController(IManagementGroupService managementgroupService)
        {
            _managementgroupService = managementgroupService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult GetList()
        {
            return Json(_managementgroupService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        #region ddl

        [Authorize]
        public JsonResult GetManagementGroupCbo()
        {
            return Json(new SelectList(_managementgroupService.GetList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        #endregion ddl

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetManagement()
        {
            return Json(_managementgroupService.Query());
        }
    }
}