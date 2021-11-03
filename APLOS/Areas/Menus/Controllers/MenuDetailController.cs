using Aplos.Controllers;
using Library.Service.Menus;
using System.Web.Mvc;

namespace Aplos.Areas.Menus.Controllers
{
    public class MenuDetailController : BaseController
    {
        private readonly IMenuService _menuService;
        private readonly IMenuDetailService _menuDetailService;

        public MenuDetailController(
            IMenuService menuService,
            IMenuDetailService menuDetailService)
        {
            _menuService = menuService;
            _menuDetailService = menuDetailService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}