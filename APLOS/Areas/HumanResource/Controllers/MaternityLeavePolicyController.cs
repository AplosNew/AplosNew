using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class MaternityLeavePolicyController : BaseController
    {
        #region Constructor

        private readonly IMaternityLeavePolicyService _maternityLeavePolicyService;

        public MaternityLeavePolicyController(
              IMaternityLeavePolicyService maternityLeavePolicyService
            )
        {
            _maternityLeavePolicyService = maternityLeavePolicyService;
        }

        #endregion Constructor

        #region -- Pages
       [Authorize]
        public ActionResult MaternityLeavePolicyNew()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpGet]
        public ActionResult GetList(string plantId)
        {
            return Json(_maternityLeavePolicyService.Query(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MaternityLeavePolicy maternityLeavePolicy)
        {
           
            _maternityLeavePolicyService.Insert(maternityLeavePolicy);
            return Json(new { MaternityLeavePolicy = maternityLeavePolicy, Message = AplosMessage.Success });
        }
      

        [HttpPost]
        public JsonResult Edit(MaternityLeavePolicy maternityLeavePolicy)
        {
            _maternityLeavePolicyService.Update(maternityLeavePolicy);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            _maternityLeavePolicyService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations  
    }
}