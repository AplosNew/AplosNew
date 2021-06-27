#region Using
using Aplos.Controllers;
using Library.Core;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    /// <remark>Modified:Belayet Hossain;Date:13-June-2016</remark>
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class UtilityController : BaseController
    {
        #region --Constructor
        private readonly IUtilityService _utilityService;

        public UtilityController(IUtilityService utilityService)
        {
            this._utilityService = utilityService;
        }
        #endregion

        #region dll
        [Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(_utilityService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
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
        public JsonResult GetAutoSequence()
        {
            return Json(_utilityService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]        
        public JsonResult GetCbo()
        {
            CustomIdentity ci = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_utilityService.GetCbo(ci.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string companyGroupId)
        {
            return Json(_utilityService.Query(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }
       
        [HttpPost]
        public JsonResult Create(Utility utility)
        {
            _utilityService.Insert(utility);
            return Json(new { Utility = utility, Sequence = _utilityService.GetAutoSequence(), Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult Edit(Utility utility)
        {
            _utilityService.Update(utility);
            return Json(new { Sequence = _utilityService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _utilityService.Delete(id);
            return Json(new { Sequence = _utilityService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}