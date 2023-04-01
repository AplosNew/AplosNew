#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class HSNCodeController : BaseController
    {
        #region Constructor

        private readonly IHSNCodeService _hSNCodeService;
        private readonly ISqlRepository _sqlRepository;

        public HSNCodeController(
              IHSNCodeService hSNCodeService, ISqlRepository sqlRepository
            )
        {
            _hSNCodeService = hSNCodeService;
            _sqlRepository = sqlRepository;

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

        [Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json( GetCboData(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetCboData(string companyGroupId)
        {
            try
            {
                string sql = @"SELECT  Id [Value],Code [Text]
                                FROM [HKP].[HSNCode] AS HC WHERE HC.CompanyGroupId='"+ companyGroupId + @"' ORDER BY Code";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hSNCodeService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetHSNCodeUnSelectedList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hSNCodeService.QueryWithUnSelected(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hSNCodeService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(HSNCode model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupId = identity.CompanyGroupId;
            _hSNCodeService.Insert(model);
            return Json(new { HSNCode = model, Sequence = _hSNCodeService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(HSNCode model)
        {
            _hSNCodeService.Update(model);
            return Json(new { Sequence = _hSNCodeService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _hSNCodeService.Delete(id);
            return Json(new { Sequence = _hSNCodeService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}