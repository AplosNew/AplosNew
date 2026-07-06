using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Vouchers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherTypeController : BaseController
    {
        private readonly IVoucherTypeService _voucherTypeService;
        private readonly ISqlRepository _sqlRepository;
        public VoucherTypeController(IVoucherTypeService voucherTypeService, ISqlRepository sqlRepository)
        {
            _voucherTypeService = voucherTypeService;
            _sqlRepository = sqlRepository;
        }

        [HttpGet]
        public ActionResult VoucherType()
        {
            return View("~/Areas/Accounts/Views/VoucherType.cshtml");
        }

        [HttpGet]
        public ActionResult VoucherTypeAdditionalInfo()
        {
            return View("~/Areas/Accounts/Views/VoucherTypeAdditionalInfo.cshtml");
        }

        [HttpGet]
        public ActionResult VoucherTypeMatrix()
        {
            return View("~/Areas/Accounts/Views/VoucherTypeMatrix.cshtml");
        }

        [HttpGet]
        public ActionResult VoucherTypeConfig()
        {
            return View("~/Areas/Accounts/Views/VoucherTypeConfig.cshtml");
        }

        [Authorize]
        public JsonResult GetVoucherTypeCbo()
        {
            return Json(GetCbo(), JsonRequestBehavior.AllowGet);
        }
        public List<Dictionary<string, object>> GetCbo()
        {
            try
            {
                var cmdText = @"SELECT UserName [Text],Id Value FROM [SCS].[VoucherType] where Active=1";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        [HttpGet]
        public ActionResult GetVoucherTypeList(GridParameter parameters)
        {
            return Json(_voucherTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetVoucherType(string id)
        {
            return Json(_voucherTypeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_voucherTypeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(VoucherType voucherType)
        {
            if (ModelState.IsValid)
            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //voucherType.CompanyGroupId = identity.CompanyGroupId;
                _voucherTypeService.Insert(voucherType);
                return Json(new { VoucherType = voucherType, Sequence = _voucherTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(VoucherType voucherType)
        {
            if (ModelState.IsValid)
            {
                _voucherTypeService.Update(voucherType);
                return Json(new { Sequence = _voucherTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _voucherTypeService.Delete(id);
                return Json(new { Sequence = _voucherTypeService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [Authorize, HttpGet]
        public ActionResult GetVoucherTypeAdditionalinfo()
        {
            return Json(GetVoucherTypeAdditionalinfoData(), JsonRequestBehavior.AllowGet);
        }

        public List<Dictionary<string, object>> GetVoucherTypeAdditionalinfoData()
        {
            try
            {
                var sql = @"SELECT * FROM [SCS].VoucherType";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        [HttpPost]
        public JsonResult UpdateVoucherTypeAdditionalInfo(string voucherTypeAdditionalinfoList)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<VoucherType> gLCompanyInfoList = JsonConvert.DeserializeObject<List<VoucherType>>(voucherTypeAdditionalinfoList, settings);
            _voucherTypeService.UpdateVoucherTypeAdditionalInfo(gLCompanyInfoList);
            return Json(new { GLGeneralInfo = gLCompanyInfoList, Message = AplosMessage.Success });
        }
    }
}