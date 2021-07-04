#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Model.Setups;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Inventory;
using Library.Service.Logs;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.JobWork.Controllers
{
    public class JWTransformationPurchaseOrderController : BaseController
    {
        string TableName = "JWTransformationPurchaseOrder";
        //authentication for
        //GetList Create Delete
        Library.MaterialManagement.JobWork.JobWorkCommon JobWorkCommon = null;
        Library.General.Conversions.UOMConversion conversion = new Library.General.Conversions.UOMConversion();
        #region Constructor
        private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<POService> _inventoryServiceRepository;
        public JWTransformationPurchaseOrderController(ISqlRepository R, IPurchaseOrderService inventoryReveiveService, IRepositoryAsync<POService> inventoryServiceRepository)
        {
            _inventoryServiceRepository = inventoryServiceRepository;
            _sqlRepository = R;
            _inventoryReveiveService = inventoryReveiveService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetPOTypeList(string POTypeStatus)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(JobWorkCommon.GetPOTypeList(identity.PlantId, POTypeStatus), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId, string PODate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();

            return Json(JobWorkCommon.GetJWServiceTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId, PODate), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id AS Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from JWActivity where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult GetList(string column, string value)
        {



            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetList(column, value), null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetJWPOChildList(string jwpoId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWPOChildList(jwpoId), null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetJwPoDetailByProduct(string jwpoDetailId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJwPoDetailByProduct(jwpoDetailId), null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetJwTransPoDetailInputMaterial(string jwpoDetailId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJwTransPoDetailInputMaterial(jwpoDetailId), null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();

            return Json(JobWorkCommon.GetSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetJobWorkActivityList()
        {
            string strSql = @"SELECT * FROM JWActivity WHERE Type = '" + JobWorkType.Transformation.ToString() + "'";
            return Json(_sqlRepository.GetDataCollection(strSql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCurrencyList()
        {
            string strSql = @"SELECT C.Id CurrencyCode, C.Code AS Currency 
                                FROM scs.Currency C";
            return Json(_sqlRepository.GetDataCollection(strSql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            try
            {
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                data = JobWorkCommon.Create(data, CheckedByStatusForNoti, ApprovedByStatusForNoti);
                return Json(new { Data = data, Message = AplosMessage.Success + " PO no <b>" + data["Id"] + "</b>" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult detailcreate(List<Dictionary<string, object>> data, string JWPurchaseOrderId,string JWActivityId,string OrderSpecific,string type)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                data = JobWorkCommon.detailcreate(data,  JWPurchaseOrderId, JWActivityId,identity.Name,identity.IPAddress,OrderSpecific,type);
                return Json(new { Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveTaxList(List<Dictionary<string, object>> data, List<Dictionary<string, object>> TaxList)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                data = JobWorkCommon.SaveTaxList(data, TaxList, identity.Name, identity.IPAddress);
                return Json(new { Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [HttpPost, Authorize]
        public JsonResult ServiceChargeCreate(Dictionary<string, object> data, List<Dictionary<string, object>> TaxList)
        {
            try
            {
                if (Convert.ToBoolean(_inventoryServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM JWTransformationPurchaseOrderService WHERE JWTransformationPurchaseOrderId='" + data["JWTransformationPurchaseOrderId"] + "' AND ServiceMasterId='" + data["ServiceMasterId"] + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                    throw new CustomException("This service already taken."); ;

                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                data = JobWorkCommon.ServiceChargeCreate(data, TaxList);
                return Json(new { Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetJWTransformationPurchaseOrderServiceList(string jwpoId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
           
            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWTransformationPurchaseOrderServiceList(jwpoId), null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetJWItemMAList(string ActivityId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWItemMAList(ActivityId), null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                JobWorkCommon.Delete(id);

                return Json(new { Error = false, Sequence = JobWorkCommon.GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        [Authorize]
        public ActionResult DeleteDetail(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                JobWorkCommon.DeleteDetail(id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
       



       
        [Authorize, HttpPost]
        public ActionResult GetJWItemList(string column, string value)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWItemList(column, value), null), JsonRequestBehavior.AllowGet);
        }
        
        [Authorize, HttpGet]
        public JsonResult GetServiceChargeList(string jwpoId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetServiceChargeList(jwpoId), null), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetPODetailServiceChargeList(string jwpoId, string jwpodId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetPODetailServiceChargeList(jwpoId, jwpodId), null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public IEnumerable<object> GetServiceTaxList(string serviceId)
        {
            try
            {
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                return _sqlRepository.GetDataCollection(JobWorkCommon.GetServiceTaxList(serviceId));
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetPODetailTaxList(string jwPOId, string jwPoDetailId)
        {
            try
            {
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetPODetailTaxList(jwPOId, jwPoDetailId)), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #region Order Specific JW PO

        [Authorize, HttpGet]
        public JsonResult GetBOQItems(string ContractId, string VendorId, string IsOwnVendor, string JWPOId, string JWPODId, string jwActivityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                return Json(JobWorkCommon.GetBOQItems(ContractId, VendorId, IsOwnVendor, JWPOId, JWPODId, jwActivityId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetBOQItemsForUpdate(string ContractId, string VendorId, string IsOwnVendor, string JWPOId, string JWPODId, string jwActivityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                return Json(JobWorkCommon.GetBOQItemsForUpdate(ContractId, VendorId, IsOwnVendor, JWPOId, JWPODId, jwActivityId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
       [Authorize, HttpPost]
        public JsonResult ConverttedBOQUOMData(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            data["RequiredQtyPO"] = conversion.Convert(data["MaterialMasterId"].ToString(), data["FromPoUomId"].ToString(), data["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(data["RequiredQtyPOOrginal"].ToString())).ToString("F2"); ;
            data["OtherPOQty"] = conversion.Convert(data["MaterialMasterId"].ToString(), data["FromPoUomId"].ToString(), data["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(data["OtherPOQtyOrginal"].ToString())).ToString("F2"); ;
            return Json(new { data, Message = AplosMessage.Success });
        }
        #endregion

        [Authorize, HttpPost]
        public JsonResult GetJWPODTChildMaterials(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                return Json(JobWorkCommon.GetJWPODTChildMaterials(data), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetJWPODTChildMaterialsSummary(string JWPODId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                return Json(JobWorkCommon.GetJWPODTChildMaterialsSummary(JWPODId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetJWPOChildListAll()
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWPOChildListAll(), null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetJWPOActivityService(string JWPODId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.JobWorkCommon();
                return Json(JobWorkCommon.GetJWPOActivityService(JWPODId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }





}