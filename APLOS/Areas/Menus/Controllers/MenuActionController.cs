using Aplos.Controllers;
using Library.Core;
using Library.Model.Menus;
using Library.Service.Menus;
using System;
using System.Web.Mvc;

namespace Aplos.Areas.Menus.Controllers
{
    public class MenuActionController : BaseController
    {
        private readonly IMenuActionService _menuActionService;

        public MenuActionController(IMenuActionService menuActionService)
        {
            _menuActionService = menuActionService;
        }

        [Authorize]
        public JsonResult GetMenuActionList(string menuId)
        {
            return Json(new SelectList(_menuActionService.GetMenuActionList(menuId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Menus/Views/CompanyGroupMenuMaster.cshtml");
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new MenuAction { Active = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JavaScriptResult Create(MenuAction menuActionVM)
        {
            try
            {
                return JavaScript($"ShowResult('{"Data saved successfully."}','{"success"}','{"redirect"}')");
            }
            catch (Exception ex)
            {
                return JavaScript($"ShowResult(\'{ex.Message}\','{"failure"}')");
            }
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            try
            {
                return View();
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public JavaScriptResult Edit(MenuAction menuActionVM)
        {
            try
            {
                return JavaScript($"ShowResult('{"Data saved successfully."}','{"success"}','{"redirect"}')");
            }
            catch (Exception ex)
            {
                return JavaScript($"ShowResult(\'{ex.Message}\','{"failure"}')");
            }
        }

        [Authorize]
        public JsonResult GetAllMenuActionList(GridParameter parameters)
        {
            return Json(_menuActionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
    }
}