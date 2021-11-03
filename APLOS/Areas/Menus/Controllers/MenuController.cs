using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Menus;
using Library.Service.Menus;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Menus.Controllers
{
    public class MenuController : BaseController
    {
        #region Constructor

        private readonly IMenuService _menuService;
        private readonly IMenuActionService _menuActionService;

        public MenuController(MenuService menuService, IMenuActionService menuActionService)
        {
            _menuService = menuService;
            _menuActionService = menuActionService;
        }

        #endregion Constructor

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Menus/Views/Menu.cshtml");
        }

        #region GetAreaList

        [Authorize]
        public JsonResult GetArea()
        {
            return Json(new SelectList(_menuService.GetArea(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAreaList()
        {
            return Json(new SelectList(_menuService.GetAreaList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        #endregion GetAreaList

        #region GetControllerList

        [Authorize]
        public JsonResult GetController()
        {
            return Json(new SelectList(_menuService.GetController(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetControllerList(string area)
        {
            return Json(new SelectList(_menuService.GetControllerList(area), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        #endregion GetControllerList

        #region GetMenuList

        [Authorize]
        public JsonResult GetMenuList()
        {
            return Json(new SelectList(_menuService.GetMenuList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAllMenuList(GridParameter parameters)
        {
            return Json(_menuService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        #endregion GetMenuList

        #region -- Operations

        [Authorize]
        public JsonResult GeActionListByMenu(string menuId)
        {
            return Json(_menuActionService.GeActionListByMenu(menuId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Menu entity, IEnumerable<MenuAction> actionList)
        {
            _menuService.InsertGraph(entity, actionList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Menu entity, IEnumerable<MenuAction> actionList)
        {
            _menuService.UpdateGraph(entity, actionList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _menuService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteMenuAction(string id)
        {
            _menuActionService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}