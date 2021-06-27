#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class BusinessProcessController : BaseController
    {
        #region Constructor

        private readonly IBusinessProcessService _brandService;

        public BusinessProcessController(IBusinessProcessService brandService)
        {
            _brandService = brandService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string companyGroupId)
        {
            return Json(_brandService.Query(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBusinessProcessList(string materialMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_brandService.GetBusinessProcessList(identity.CompanyGroupId, materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BusinessProcess BusinessProcess)
        {
            _brandService.Insert(BusinessProcess);
            return Json(new { BusinessProcess, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BusinessProcess BusinessProcess)
        {
            _brandService.Update(BusinessProcess);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _brandService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}