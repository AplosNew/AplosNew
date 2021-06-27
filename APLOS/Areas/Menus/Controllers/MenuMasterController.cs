using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Menus;
using Library.Service.Menus;
using System.Web.Mvc;

namespace Aplos.Areas.Menus.Controllers
{
    public class MenuMasterController : BaseController
    {
        #region Constructor

        private readonly IMenuMasterService _menuMasterService;
        private readonly IMenuService _menuService;
        private readonly IMenuItemService _menuItemService;

        public MenuMasterController(
            IMenuMasterService menuMasterService
            , IMenuService menuService
            , IMenuItemService menuItemService)
        {
            _menuMasterService = menuMasterService;
            _menuService = menuService;
            _menuItemService = menuItemService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Menus/Views/MenuMaster.cshtml");
        }

        [HttpGet]
        public ActionResult Edit()
        {
            return View("~/Areas/Menus/Views/MenuMasterEdit.cshtml");
        }

        [HttpGet]
        public ActionResult GetMenuItemByMenuItemGroupList(string menuItemGroup, string moduleId, string menuFrameId)
        {
            return Json(_menuMasterService.GetMenuItemList(menuItemGroup, moduleId, menuFrameId).Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetMenuFrameByModuleIdCbo(string moduleId)
        {
            return Json(_menuMasterService.GetMenuFrameByModuleIdCbo(moduleId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMenuMasterAllList(GridParameter parameters)
        {
            return Json(_menuMasterService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMenuMaster(string id)
        {
            return Json(_menuMasterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MenuMaster menuMaster, string[] menuItemIds)
        {
            _menuMasterService.Insert(menuMaster, menuItemIds);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MenuMaster menuMaster)
        {
            _menuMasterService.Update(menuMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _menuMasterService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}