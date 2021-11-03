using Library.Service.Helpers;
using Library.Service.Organizations;
using Library.Service.Securites;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    [AllowAnonymous]
    public class ResourcesPathReaderController : BaseController
    {
        #region Constructor

        private readonly IUserService _userService;
        private readonly ICompanyGroupService _companyGroupService;

        public ResourcesPathReaderController(
            IUserService userService
            , ICompanyGroupService companyGroupService)
        {
            _userService = userService;
            _companyGroupService = companyGroupService;
        }

        #endregion Constructor

        /// <summary>
        /// User picture
        /// </summary>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult GetUserPicUrl()
        {
            return Json(ResourcesPathReader.GetUserPicUrl(), JsonRequestBehavior.AllowGet);
        }
    }
}