#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Parties.Controllers
{
    public class PDPFunctionController : BaseController
    {
        #region Constructor

        private readonly IPartnerDeterminationProcedureFunctionService _pDPFunctionService;

        public PDPFunctionController(IPartnerDeterminationProcedureFunctionService pDPFunctionService)
        {
            _pDPFunctionService = pDPFunctionService;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet]
        public ActionResult GetVendorPFList(GridParameter parameters, string accountType)
        {
            return Json(_pDPFunctionService.GetVendorPFList(parameters, accountType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCustomerPFList(GridParameter parameters, string accountType)
        {
            return Json(_pDPFunctionService.GetCustomerPFList(parameters, accountType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPDPFunctionListByPDPId(GridParameter parameters, string partnerDeterminationProcedureId)
        {
            return Json(_pDPFunctionService.Query(parameters, partnerDeterminationProcedureId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<PartnerDeterminationProcedureFunction> pDPFunction, string PartnerDeterminationProcedureId)
        {
            _pDPFunctionService.InsertOrUpdateGraph(pDPFunction, PartnerDeterminationProcedureId);
            return Json(new { PDPFunction = pDPFunction, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _pDPFunctionService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}