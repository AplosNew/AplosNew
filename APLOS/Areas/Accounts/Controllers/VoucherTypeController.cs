using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
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
        private readonly IUnitOfWork _unitOfWork;

        public VoucherTypeController(IVoucherTypeService voucherTypeService, ISqlRepository sqlRepository, IUnitOfWork unitOfWork)
        {
            _voucherTypeService = voucherTypeService;
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
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

        [HttpGet]
        public ActionResult VoucherMaxNumberUpdate()
        {
            return View("~/Areas/Accounts/Views/VoucherMaxNumberUpdate.cshtml");
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

        #region VoucherMaxNumber Update
        [Authorize,HttpGet]
        public JsonResult GetVoucherConfigPeriodCbo()
        {
            return Json(GetVoucherConfigPeriod(), JsonRequestBehavior.AllowGet);
        }
        public List<Dictionary<string, object>> GetVoucherConfigPeriod()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var cmdText = @"SELECT DISTINCT VTN.[Period] [Value],VTN.[Period] [Text]
                                FROM SCS.VoucherTypeNumber VTN
                                ORDER BY VTN.[Period] DESC";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetVoucherMaxNumberCbo(string period,string voucherTypeId)
        {
            return Json(GetVoucherMaxNumber(period, voucherTypeId), JsonRequestBehavior.AllowGet);
        }
        public List<Dictionary<string, object>> GetVoucherMaxNumber(string period, string voucherTypeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var cmdText = @"SELECT  VTN.Id,VTN.MaxNumber 
                                FROM SCS.VoucherTypeNumber VTN
                                LEFT JOIN [SCS].[VoucherTypeConfig] VTC ON VTC.Id=VTN.VoucherTypeConfigId
                                WHERE VTC.PlantId='" + identity.PlantId + @"' AND VTN.[Period]='"+ period + @"' AND VTC.VoucherTypeId='"+ voucherTypeId + @"'
                                ORDER BY VTN.[Period] DESC";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        [HttpPost]
        public ActionResult UpdateMaxNumber(string id, string maxNumber)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var rdBuilder = new System.Text.StringBuilder();
                    var voucherSql = @"UPDATE SCS.VoucherTypeNumber SET MaxNumber="+ maxNumber + " WHERE Id='" + id + "'";
                    rdBuilder.Append(voucherSql);

                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return Json(new { Message = " Successfully Updated" });
            }
            catch (CustomException)
            {
                throw;
            }

            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        #endregion
    }
}