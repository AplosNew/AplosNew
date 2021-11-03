using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class RestController : BaseController
    {
        #region Constructor

        private readonly IRestService _restService;
        private readonly IRestDetailsService _restDetailsService;

        public RestController(
              IRestService restService
            , IRestDetailsService restDetailsService
            )
        {
            _restService = restService;
            _restDetailsService = restDetailsService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_restService.Query(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetRestDetailsData(string restId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_restService.GetRestDetailsData(restId,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllEmployeeList(GridParameter parameters, string sectionId, string subSectionId, string departmentId, bool isOTEntitle,string AttendanceRestDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_restService.GetAllEmployee(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, sectionId, subSectionId, departmentId, isOTEntitle, AttendanceRestDate), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AttendanceRest rest, IEnumerable<AttendanceRestDetail> restDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _restService.Insert(rest, identity.PlantId, restDetails);
            return Json(new { Rest = rest, Message = AplosMessage.Success });
        }


        public ActionResult Delete(string id)
        {
            _restService.DeleteMaster(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public ActionResult DeleteDetail(string id)
        {
            _restDetailsService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}