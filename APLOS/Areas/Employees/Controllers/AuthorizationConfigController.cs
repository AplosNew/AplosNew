#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Employees;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class AuthorizationConfigController : BaseController
    {
        #region Constructor
        /// <summary>   The AuthorizationConfigService service. </summary>
        private readonly IAuthorizationConfigService _authorizationConfigService;

        public AuthorizationConfigController(IAuthorizationConfigService authorizationConfigService)
        {
            _authorizationConfigService = authorizationConfigService;
        }
        #endregion

        #region -- Pages
      
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        

        [HttpGet, Authorize]
        public ActionResult GetList(string actionStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_authorizationConfigService.Query(identity.CompanyId, identity.PlantId, actionStatus), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllEmployeeData()
        {
            JsonResult json = Json(_authorizationConfigService.GetAllEmployeeData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost]
        public JsonResult Create(AuthorizationConfig authorizationConfig)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            authorizationConfig.PlantId = identity.PlantId;
            _authorizationConfigService.Insert(authorizationConfig);
            return Json(new { AuthorizationConfig= authorizationConfig,  Message = AplosMessage.Insert });
        }
        
        public ActionResult Delete(string id)
        {
            _authorizationConfigService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo(string status)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_authorizationConfigService.GetCbo(status, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion


    }
}