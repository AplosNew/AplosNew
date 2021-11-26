#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Securites;
using Library.Service.Securites;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    /// <summary>
    /// Credential Policy Controller.
    /// </summary>
    public class CredentialPolicyController : BaseController
    {
        #region Constructor

        private readonly ICredentialPolicyService _credentialPolicyService;

        public CredentialPolicyController(ICredentialPolicyService CredentialPolicyService)
        {
            _credentialPolicyService = CredentialPolicyService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetSearchGridData(GridParameter parameters)
        {
            return new CustomJsonResult { Data = _credentialPolicyService.GetSearchData(parameters) };
        }

        [HttpGet]
        public ActionResult GetllCredentialPolicy()
        {
            return Json(_credentialPolicyService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCredentialPolicyList(GridParameter parameters)
        {
            return Json(_credentialPolicyService.GetSearchData(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCredentialPolicy()
        {
            // Policy is company group wise single and one entry.
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // Make credential policy company group wise.
            var id = identity.CompanyGroupId + "1";
            return Json(_credentialPolicyService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CredentialPolicy credentialPolicy)
        {
            if (!ModelState.IsValid) throw new CustomException(Resources.RequiredFieldMessage);
            _credentialPolicyService.Insert(credentialPolicy);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(CredentialPolicy credentialPolicy)
        {
            if (!ModelState.IsValid) throw new CustomException(Resources.RequiredFieldMessage);
            _credentialPolicyService.Update(credentialPolicy);
            return Json(new { Message = AplosMessage.Updated });
        }
    }
}