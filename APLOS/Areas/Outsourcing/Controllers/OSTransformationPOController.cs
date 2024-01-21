#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.MaterialManagement.Inventory;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using Library.Core;
using Library.Model.Parties;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Outsourcing.Controllers
{
    public class OSTransformationPOController : BaseController
    {
        string TableName = "OSTransformationPO";
        //authentication for
        //GetList Create Delete
        Library.MaterialManagement.JobWork.OSCommon JobWorkCommon = null;
        Library.General.Conversions.UOMConversion conversion = new Library.General.Conversions.UOMConversion();
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public OSTransformationPOController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetPOTypeList(string POTypeStatus)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(JobWorkCommon.GetPOTypeList(identity.PlantId, POTypeStatus), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId, string PODate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

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



            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetList(column, value), null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetJWPOChildList(string jwpoId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWPOChildList(jwpoId), null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetJwPoDetailByProduct(string jwpoDetailId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJwPoDetailByProduct(jwpoDetailId), null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetJwTransPoDetailInputMaterial(string jwpoDetailId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJwTransPoDetailInputMaterial(jwpoDetailId), null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

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
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                data = JobWorkCommon.Create(data, CheckedByStatusForNoti, ApprovedByStatusForNoti);
                return Json(new { Data = data, Message = AplosMessage.Success + " PO no <b>" + data["Id"] + "</b>" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult detailcreate(List<Dictionary<string, object>> data, string JWPurchaseOrderId, string JWActivityId, string OrderSpecific, string type, List<Dictionary<string, object>> taxCategoryList, string JWPOToCurrencyRate, string JWPOIsNonCreditable, string JWPODate, string JWPOType)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                data = JobWorkCommon.detailcreate(data, JWPurchaseOrderId, JWActivityId, identity.Name, identity.IPAddress, OrderSpecific, type, taxCategoryList, JWPOToCurrencyRate, JWPOIsNonCreditable, JWPODate, JWPOType);
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
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
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
                //if (Convert.ToBoolean(_inventoryServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM OSTransformationPOService WHERE OSTransformationPOId='" + data["OSTransformationPOId"] + "' AND ServiceMasterId='" + data["ServiceMasterId"] + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                //    throw new CustomException("This service already taken."); ;

                DataTable dt = _sqlRepository.GetDataTable(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM OSTransformationPOService WHERE OSTransformationPOId='" + data["OSTransformationPOId"] + "' AND ServiceMasterId='" + data["ServiceMasterId"] + "') AS A) SELECT 1 AS RET ELSE SELECT 0 AS RET RETURN");
                if (bplib.clsWebLib.GetBoolData(dt.Rows[0]["RET"].ToString()))
                    throw new CustomException("This service already taken.");

                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
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
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWTransformationPurchaseOrderServiceList(jwpoId), null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetJWItemMAList(string ActivityId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWItemMAList(ActivityId), null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                JobWorkCommon.Delete(id);

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        [HttpPost, Authorize]
        public ActionResult DeleteDetail(string id, string OrderSpecific)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                JobWorkCommon.DeleteDetail(id, OrderSpecific);
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
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWItemList(column, value), null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceChargeList(string jwpoId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetServiceChargeList(jwpoId), null), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetPODetailServiceChargeList(string jwpoId, string jwpodId)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetPODetailServiceChargeList(jwpoId, jwpodId), null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public IEnumerable<object> GetServiceTaxList(string serviceId)
        {
            try
            {
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
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
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
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
        public JsonResult GetBOQItems(string ContractId, string VendorId, string IsOwnVendor, string JWPOId, string JWPODId, string jwActivityId, string POType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.GetBOQItems(ContractId, VendorId, IsOwnVendor, JWPOId, JWPODId, jwActivityId, POType), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetBOQItemsForUpdate(string ContractId, string VendorId, string IsOwnVendor, string JWPOId, string JWPODId, string jwActivityId, string MaterialId, string ArticleId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.GetBOQItemsForUpdate(ContractId, VendorId, IsOwnVendor, JWPOId, JWPODId, jwActivityId, MaterialId, ArticleId), JsonRequestBehavior.AllowGet);
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
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
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
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
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
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

            return Json(_sqlRepository.GetDataCollection(JobWorkCommon.GetJWPOChildListAll(), null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetJWPOActivityService(string JWPODId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.GetJWPOActivityService(JWPODId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public ActionResult LoadAllEmpDetails(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                   --AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from dbo.JobWorkValueAddedContractChild where JobWorkValueAddedContractMasterId='" + Id + @"')
                  order by EMP.EmployeeCode";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetAllEntity(string PlantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.GetAllEntity(PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpGet, Authorize]
        public JsonResult NotificationSetting()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='OutSource' and plantId='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [Authorize, HttpGet]
        public JsonResult GetMaterialfromJW(string JobWorkItemId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.GetMaterialfromJW(JobWorkItemId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult GetListForHoldRejectApproved(string ApproveRejectHold)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.GetListForHoldRejectApproved(identity.PlantId, ApproveRejectHold), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpPost]
        public JsonResult LoadInputArticle(string MaterialMstId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.LoadInputArticle(MaterialMstId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult GetJWMaterialStorage(string JWLocId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.GetJWMaterialStorage(JWLocId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        #region Job Work Purchase Order Report 
        [HttpGet, Authorize]
        public ActionResult GePurchaseOrderReport(string purchaseOrderId, string POType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
            //    return Json(JobWorkCommon.GePurchaseOrderReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, purchaseOrderId), JsonRequestBehavior.AllowGet);

            JobWorkCommon.GePurchaseOrderReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, purchaseOrderId, POType);

            return null;

        }
        #endregion

        // DOCUMENT ATTACH

        #region Documents Upload
        [HttpPost, Authorize]
        public JsonResult PODocCreate(FormCollection form, string POId)
        {
            var PODocumentMap = new JavaScriptSerializer().Deserialize<PODocumentMap>(form["PODocumentMap"]);

            var directory = ResourcesPathReader.GetJobWorkPurchaseOrderPath();
            var path = Path.Combine(directory);

            if (PODocumentMap.UserFilename.IsNotNull())
            {
                ResourcesPathReader.IsValidFileExtention(Path.GetExtension(PODocumentMap.UserFilename));
            }

            var fileId = "";
            var fileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PODocumentMap.CompanyGroupId = identity.CompanyGroupId;


            //_inventoryReveiveService.InsertPODocMap(PODocumentMap, POId, out string Id);
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
            JobWorkCommon.InsertPODocMap(PODocumentMap, POId, out string Id);

            var file = Request.Files["file"];

            if (PODocumentMap.UserFilename.IsNotNull())
            {

                if (System.IO.File.Exists(path + PODocumentMap.POId))
                    System.IO.File.Delete(path + Id + Path.GetExtension(PODocumentMap.UserFilename));
                file.SaveAs(path + Id + Path.GetExtension(PODocumentMap.UserFilename));
            }
            return Json(new { PODocumentMap = PODocumentMap, Message = AplosMessage.Insert });
        }

        public JsonResult PODocumentMapData(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.PODocumentMapData(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //public JsonResult PODocumentMapData(string POID)
        //{
        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        Library.MaterialManagement.InventoryManagements.PurchaseOrderService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderService();
        //        return Json(obj.PODocumentMapData(POID), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}



        [Authorize, HttpPost]
        public ActionResult POImageDelete(string Id)
        {
            var fileId = "";
            var fileName = "";
            try
            {
                //   Library.MaterialManagement.InventoryManagements.PurchaseOrderService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderService();

                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();

                var directory = ResourcesPathReader.GetJobWorkPurchaseOrderPath();
                var path = Path.Combine(directory);
                var data = GetFile(Id);
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                    !string.IsNullOrEmpty(data["UserFilename"].ToString()))
                        fileId = data["Id"].ToString();
                    fileName = data["UserFilename"].ToString();
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }
                JobWorkCommon.GRNImageDelete(Id);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }
        public Dictionary<string, object> GetFile(string systemId)
        {
            try
            {
                var sql = @"Select Id, UserFilename From dbo.JWPODocumentMap Where Id='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //[HttpGet, Authorize]
        //public JsonResult PODocumentMapDataAll(string POID)
        //{
        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        Library.MaterialManagement.InventoryManagements.PurchaseOrderService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderService();
        //        return Json(obj.PODocumentMapDataAll(POID), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [HttpGet, Authorize]
        public JsonResult PODocumentMapDataAll(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.PODocumentMapDataAll(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        [HttpGet, Authorize]
        public JsonResult getMatInputListBOQData(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.getMatInputListBOQData(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpPost]
        public ActionResult DelMaterialInputBOQ(string Id)
        {

            try
            {

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                JobWorkCommon.DelMaterialInputBOQ(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult GetSalesOrderData(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.GetSalesOrderData(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public JsonResult LoadAllSKU(string MaterialMstId, string assignment, string charId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
                return Json(JobWorkCommon.LoadAllSKU(MaterialMstId, assignment, charId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetProductionOredrList(string entityid, string column, string value)
        {
            JobWorkCommon = new Library.MaterialManagement.JobWork.OSCommon();
            var jsondata = Json(JobWorkCommon.GetProductionOredrList(entityid, column, value), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

    }
}