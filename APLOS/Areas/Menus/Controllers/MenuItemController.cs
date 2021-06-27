using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Menus;
using Library.Service.Menus;
using System.Web.Mvc;

namespace Aplos.Areas.Menus.Controllers
{
    public class MenuItemController : BaseController
    {
        private readonly IMenuItemService _menuItemService;
        private readonly IMenuService _menuService;

        public MenuItemController(
            IMenuItemService menuItemService,
            IMenuService menuService
            )
        {
            _menuItemService = menuItemService;
            _menuService = menuService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Menus/Views/MenuItem.cshtml");
        }

        [Authorize]
        public JsonResult GetMenuItemCbo()
        {
            return Json(new SelectList(_menuItemService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetMenuItemGroupList()
        {
            return Json(new SelectList(_menuItemService.GetMenuItemGroupList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// For Menu Master
        /// </summary>
        /// <param name="menuItemGroup"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMenuItemByMenuItemGroupList(string menuItemGroup)
        {
            return Json(_menuItemService.Query(menuItemGroup).Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMenuItemList(GridParameter parameters)
        {
            return Json(_menuItemService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMenuItem(string id)
        {
            return Json(_menuItemService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_menuItemService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                _menuItemService.Insert(menuItem);
                return Json(new { MenuItem = menuItem, Sequence = _menuItemService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                _menuItemService.Update(menuItem);
                return Json(new { Sequence = _menuItemService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _menuItemService.Delete(id);
                return Json(new { Sequence = _menuItemService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}