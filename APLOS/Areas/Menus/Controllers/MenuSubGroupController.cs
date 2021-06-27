using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Menus;
using Library.Service.Menus;
using System.Web.Mvc;

namespace Aplos.Areas.Menus.Controllers
{
    public class MenuSubGroupController : BaseController
    {
        private readonly IMenuSubGroupService _menuSubGroupService;

        public MenuSubGroupController(IMenuSubGroupService menuSubGroupService)
        {
            _menuSubGroupService = menuSubGroupService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Menus/Views/MenuSubGroup.cshtml");
        }

        [Authorize]
        public JsonResult GetMenuSubGroupCbo()
        {
            return Json(new SelectList(_menuSubGroupService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMenuSubGroupList(GridParameter parameters)
        {
            return Json(_menuSubGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMenuSubGroup(string id)
        {
            return Json(_menuSubGroupService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_menuSubGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MenuSubGroup menuSubGroup)
        {
            _menuSubGroupService.Insert(menuSubGroup);
            return Json(new { MenuSubGroup = menuSubGroup, Sequence = _menuSubGroupService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MenuSubGroup menuSubGroup)
        {
            _menuSubGroupService.Update(menuSubGroup);
            return Json(new { Sequence = _menuSubGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _menuSubGroupService.Delete(id);
            return Json(new { Sequence = _menuSubGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}