using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Menus;
using Library.Service.Menus;
using System.Web.Mvc;

namespace Aplos.Areas.Menus.Controllers
{
    public class MenuGroupController : BaseController
    {
        private readonly IMenuGroupService _menuGroupService;

        public MenuGroupController(IMenuGroupService menuGroupService)
        {
            _menuGroupService = menuGroupService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Menus/Views/MenuGroup.cshtml");
        }

        [Authorize]
        public JsonResult GetMenuGroupCbo()
        {
            return Json(new SelectList(_menuGroupService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMenuGroupList(GridParameter parameters)
        {
            return Json(_menuGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMenuGroup(string id)
        {
            return Json(_menuGroupService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_menuGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MenuGroup menuGroup)
        {
            _menuGroupService.Insert(menuGroup);
            return Json(new { MenuGroup = menuGroup, Sequence = _menuGroupService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MenuGroup menuGroup)
        {
            _menuGroupService.Update(menuGroup);
            return Json(new { Sequence = _menuGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _menuGroupService.Delete(id);
            return Json(new { Sequence = _menuGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}