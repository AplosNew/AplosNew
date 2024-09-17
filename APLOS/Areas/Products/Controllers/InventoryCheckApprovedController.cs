
using Aplos.Areas.Setups.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Model.Products;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.Service.Logs;
using Library.Service.Products;
using Library.MaterialManagement.Reports;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Setup;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Products.Controllers
{
    public class InventoryCheckApprovedController : Controller
    {

        #region Constructor
        //private readonly InventoryCheckApprovedControllerService _inventoryCheckApprovedControllerService;
        private readonly IMaterialRequsitionMasterServiceService _materialRequsitionMasterServiceService;
        private readonly IRepositoryAsync<NotificationSetting> _notificationSetting;
        private readonly ISqlRepository _sqlRepository;

        public InventoryCheckApprovedController(
             //InventoryCheckApprovedControllerService inventoryCheckApprovedControllerService
             IRepositoryAsync<NotificationSetting> notificationSetting

             ,ISqlRepository sqlRepository
            , IMaterialRequsitionMasterServiceService materialRequsitionMasterServiceService)
        {
            //_inventoryCheckApprovedControllerService = inventoryCheckApprovedControllerService;
            _notificationSetting = notificationSetting;
            _sqlRepository = sqlRepository;
            _materialRequsitionMasterServiceService = materialRequsitionMasterServiceService;
        }

        #endregion Constructor

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize]
        public ActionResult ReqAuthorized() 
        {
            return View();
        }
       
        #endregion Aplos

        #region Requisition 
        [Authorize, HttpGet]
        public JsonResult GetSupervisorCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetSupervisorCboList(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSupervisorCboApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetSupervisorCboApprovedList(), JsonRequestBehavior.AllowGet);
        }
		[Authorize, HttpGet]
		public IEnumerable<object> GetSupervisorCboList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value,E.EmployeeCode +'-'+E.EmployeeName from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where A.ActionStatus='RequisitionCheckedBy' AND E.EmployeeStatus='Active'";// A.PlantId='" + identity.PlantId + "' AND
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
		[Authorize, HttpGet]
		public IEnumerable<object> GetSupervisorCboApprovedList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.systemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where A.ActionStatus='RequisitionApproveBy' AND E.EmployeeStatus='Active'";
                //--A.PlantId = '" + identity.PlantId + "' " +
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetIssueSlipApprovedList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetIssueSlipApprovedListdata(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public IEnumerable<object> GetIssueSlipApprovedListdata() 
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.systemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where A.ActionStatus='IssueSlipApproveBy' AND E.EmployeeStatus='Active'";
                //--A.PlantId = '" + identity.PlantId + "' " +
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        [HttpGet ,Authorize]
        public JsonResult GetListRequisionUnchecked() 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetRequisionUnchecked(), JsonRequestBehavior.AllowGet);
        }     
        public IEnumerable<object> GetRequisionUnchecked() 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"Select
                                 Id
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo, PreparedBy
                        FROM
                        (
                        Select

                                 MRM.Id
                                , MRM.RequisitionDate
                                , MRM.RequisitionType
                                , MRM.RequirmentType
                                , MRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , MRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , MRM.EntityId
                                , MRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , MRM.AddedDate
                                , MRM.AddedFromIP
                                , MRM.UpdatedBy
                                , MRM.UpdatedDate
                                , MRM.UpdatedFromIP
                                , MRM.Remarks
                                , MRM.CheckedBy
                                , MRM.CheckedByStatus
                                , MRM.AuthorizedBy
                                , MRM.AuthorizedByStatus
                                , MRM.IsApproved
                                , A.UserName ActivityName
                                , MM.UserName MaterialName
                                , MRD.TransactionQty
                                , MRD.EstimatedRate
                                , MRD.TotalAmount
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                , EI4.EmployeeName AS AddedBy
                                ,MRM.OrderRefNo
                                ,MRM.ReasonWhyItIsNotPlanEarlier Reason
                                ,MRM.ReqEmpId PreparedBy

                        FROM [TRN].[MaterialRequsitionMaster] MRM
                        Left Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                        Left Join org.Entity E on E.Id = MRM.EntityId
                        LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                        Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 On EI4.SystemId = MRM.ReqEmpId
                        Where MRM.CheckedByStatus <> 'Checked' AND MRM.CheckedByStatus <>'Hold' and MRM.CheckedByStatus <> 'Reject' 
                        AND MRM.CheckedBy='" + identity.EmployeeId + @"'
                        )xyz
                        Group By
                                  Id
                                , RequisitionDate
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo,PreparedBy
                                Order By RequisitionDate ASC";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        [Authorize, HttpGet]

        public JsonResult GetListRequisionHoldReject()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetListRequisionHR(), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetListRequisionHR()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"Select
                                 Id
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,CheckedRejectReason,OrderRefNo,PreparedBy
                        FROM
                        (
                        Select

                                 MRM.Id
                                , MRM.RequisitionDate
                                , MRM.RequisitionType
                                , MRM.RequirmentType
                                , MRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , MRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , MRM.EntityId
                                , MRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , MRM.AddedDate
                                , MRM.AddedFromIP
                                , MRM.UpdatedBy
                                , MRM.UpdatedDate
                                , MRM.UpdatedFromIP
                                , MRM.Remarks
                                --, MRM.CheckedBy
                                ,EI2.EmployeeName CheckedBy
                                ,EI3.EmployeeName AuthorizedBy
                                , MRM.CheckedByStatus
                                --, MRM.AuthorizedBy
                                , MRM.AuthorizedByStatus
                                , MRM.IsApproved
                                , A.UserName ActivityName
                                , MM.UserName MaterialName
                                , MRD.TransactionQty
                                , MRD.EstimatedRate
                                , MRD.TotalAmount
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                , EI4.EmployeeName AS AddedBy
                                ,MRM.ReasonWhyItIsNotPlanEarlier Reason
                                ,MRM.CheckedHoldRejectReason CheckedRejectReason
                                ,MRM.OrderRefNo,MRM.ReqEmpId PreparedBy

                        FROM[TRN].[MaterialRequsitionMaster] MRM
                        Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                        Left Join org.Entity E on E.Id = MRM.EntityId
                        LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                        Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 On EI4.SystemId = MRM.ReqEmpId
                        Where MRM.CheckedbyStatus ='Hold' OR MRM.CheckedbyStatus ='Reject' AND MRM.CheckedByStatus <> 'Checked' --OR MRM.CheckedByStatus is null
                        AND MRM.InActive=0 AND MRM.CheckedBy='" + identity.EmployeeId + @"'
                        )xyz
                        Group By
                                  Id
                                , RequisitionDate
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus,AddedBy,Reason,CheckedRejectReason,OrderRefNo,PreparedBy
                                Order By RequisitionDate ASC";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetListRequisionchecked()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetRequisionchecked(), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetRequisionchecked()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"Select
                                 Id
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo,PreparedBy
                        FROM
                        (
                        Select

                                 MRM.Id
                                , MRM.RequisitionDate
                                , MRM.RequisitionType
                                , MRM.RequirmentType
                                , MRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , MRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , MRM.EntityId
                                , MRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , MRM.AddedDate
                                , MRM.AddedFromIP
                                , MRM.UpdatedBy
                                , MRM.UpdatedDate
                                , MRM.UpdatedFromIP
                                , MRM.Remarks
                                , MRM.CheckedBy
                                , MRM.CheckedByStatus
                                , MRM.AuthorizedBy
                                , MRM.AuthorizedByStatus
                                , MRM.IsApproved
                                , A.UserName ActivityName
                                , MM.UserName MaterialName
                                , MRD.TransactionQty
                                , MRD.EstimatedRate
                                , MRD.TotalAmount
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                 , EI4.EmployeeName AS AddedBy
                                ,MRM.ReasonWhyItIsNotPlanEarlier Reason,MRM.OrderRefNo
                                ,MRM.ReqEmpId PreparedBy

                        FROM[TRN].[MaterialRequsitionMaster] MRM
                        Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                        Left Join org.Entity E on E.Id = MRM.EntityId
                        LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                        Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 On EI4.SystemId = MRM.ReqEmpId
                        Where MRM.CheckedByStatus = 'Checked'
                        AND MRM.InActive=0  AND MRM.CheckedBy='" + identity.EmployeeId + @"'
                        )xyz
                        Group By
                                  Id
                                , RequisitionDate
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo,PreparedBy
                                Order By RequisitionDate ASC";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


    
        [Authorize, HttpGet]
        public JsonResult GetListRequisionUnApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetRequisionUnApproved(), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetRequisionUnApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"SELECT * FROM (Select
                                  Id
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                                ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo,PreparedById,PreparedBy
                        FROM
                        (
                        Select

                                  MRM.Id
                                , MRM.RequisitionDate
                                , MRM.RequisitionType
                                , MRM.RequirmentType
                                , MRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , MRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , MRM.EntityId
                                , MRM.ReasonWhyItIsNotPlanEarlier
                                , EI4.EmployeeName AddedBy
                                , MRM.AddedDate
                                , MRM.AddedFromIP
                                , MRM.UpdatedBy
                                , MRM.UpdatedDate
                                , MRM.UpdatedFromIP
                                , MRM.Remarks
                                , MRM.CheckedBy
                                , MRM.CheckedByStatus
                                , MRM.AuthorizedBy
                                , MRM.AuthorizedByStatus
                                , MRM.IsApproved
                                , A.UserName ActivityName
                                , MM.UserName MaterialName
                                , MRD.TransactionQty
                                , MRD.EstimatedRate
                                , MRD.TotalAmount
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                ,MRM.ReasonWhyItIsNotPlanEarlier Reason,MRM.OrderRefNo,MRM.ReqEmpId PreparedById
                                ,MRM.ReqEmpId PreparedBy
                        FROM[TRN].[MaterialRequsitionMaster] MRM
                        Left  Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                        Left Join org.Entity E on E.Id = MRM.EntityId
                        LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                        Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 ON EI4.SystemId=MRM.ReqEmpId
                        Where MRM.CheckedByStatus ='Checked' 
                        AND MRM.AuthorizedByStatus ='For Approval' 
                        AND MRM.InActive=0 
                        AND MRM.AuthorizedBy='" + identity.EmployeeId + @"'
                        )xyz
                        Group By
                                    Id
                                , RequisitionDate
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo,PreparedById,PreparedBy
                                --Order By RequisitionDate ASC
                        UNION ALL

                        Select
                                    Id
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                 , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                                    ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo,PreparedById,PreparedBy
                        FROM
                        (
                        Select

                                    MRM.Id
                                , MRM.RequisitionDate
                                , MRM.RequisitionType
                                , MRM.RequirmentType
                                , MRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , MRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , MRM.EntityId
                                , MRM.ReasonWhyItIsNotPlanEarlier
                                , EI4.EmployeeName AddedBy
                                , MRM.AddedDate
                                , MRM.AddedFromIP
                                , MRM.UpdatedBy
                                , MRM.UpdatedDate
                                , MRM.UpdatedFromIP
                                , MRM.Remarks
                                , MRM.CheckedBy
                                , MRM.CheckedByStatus
                                , MRM.AuthorizedBy
                                , MRM.AuthorizedByStatus
                                , MRM.IsApproved
                                , A.UserName ActivityName
                                , MM.UserName MaterialName
                                , MRD.TransactionQty
                                , MRD.EstimatedRate
                                , MRD.TotalAmount
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                ,MRM.ReasonWhyItIsNotPlanEarlier Reason,MRM.OrderRefNo,MRM.ReqEmpId PreparedById
                                ,MRM.ReqEmpId PreparedBy
                        FROM[TRN].[MaterialRequsitionMaster] MRM
                        Left Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                        Left Join org.Entity E on E.Id = MRM.EntityId
                        LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                        Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 ON EI4.SystemId=MRM.ReqEmpId
                        Where MRM.CheckedByStatus Is null 
                        AND MRM.AuthorizedByStatus ='For Approval' 
                        AND MRM.InActive=0 
                        AND MRM.AuthorizedBy='" + identity.EmployeeId + @"' 
                        )xyz
                        Group By
                                    Id
                                , RequisitionDate
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo,PreparedById,PreparedBy)x
                                Order By CONVERT(VARCHAR, x.RequisitionDate, 23) desc";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        [Authorize, HttpGet]
        public JsonResult GetListRequisionApprovedHoldReject()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetRequisionAHR(), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetRequisionAHR()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"Select
                                 Id
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,RejectApprovedReason
                                ,OrderRefNo,PreparedById,PreparedBy
                        FROM
                        (
                        Select

                                 MRM.Id
                                , MRM.RequisitionDate
                                , MRM.RequisitionType
                                , MRM.RequirmentType
                                , MRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , MRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , MRM.EntityId
                                , MRM.ReasonWhyItIsNotPlanEarlier
                                , EI4.EmployeeName  AddedBy
                                , MRM.AddedDate
                                , MRM.AddedFromIP
                                , MRM.UpdatedBy
                                , MRM.UpdatedDate
                                , MRM.UpdatedFromIP
                                , MRM.Remarks
                                , MRM.CheckedBy
                                , MRM.CheckedByStatus
                                , MRM.AuthorizedBy
                                , MRM.AuthorizedByStatus
                                , MRM.IsApproved
                                , A.UserName ActivityName
                                , MM.UserName MaterialName
                                , MRD.TransactionQty
                                , MRD.EstimatedRate
                                , MRD.TotalAmount
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                ,MRM.ReasonWhyItIsNotPlanEarlier Reason
                                ,MRM.ApprovedHoldRejectReason RejectApprovedReason
                                ,MRM.OrderRefNo,MRM.ReqEmpId PreparedById,MRM.ReqEmpId PreparedBy
                        FROM[TRN].[MaterialRequsitionMaster] MRM
                        Left Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                        Left Join org.Entity E on E.Id = MRM.EntityId
                        LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                        Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 ON EI4.SystemId=MRM.ReqEmpId
                        Where MRM.AuthorizedByStatus ='Hold' 
						OR MRM.AuthorizedByStatus ='Reject' 
                        AND MRM.CheckedByStatus = 'Checked'
                        AND MRM.AuthorizedBy='" + identity.EmployeeId + @"' AND MRM.InActive=0 
                        )xyz
                        Group By
                                  Id
                                , RequisitionDate
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus,AddedBy,Reason,RejectApprovedReason,OrderRefNo,PreparedById,PreparedBy
                                --Order By RequisitionDate ASC

								UNION ALL
								Select
                                 Id
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,RejectApprovedReason,OrderRefNo,PreparedById,PreparedBy
                        FROM
                        (
                        Select

                                 MRM.Id
                                , MRM.RequisitionDate
                                , MRM.RequisitionType
                                , MRM.RequirmentType
                                , MRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , MRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , MRM.EntityId
                                , MRM.ReasonWhyItIsNotPlanEarlier
                                , EI4.EmployeeName  AddedBy
                                , MRM.AddedDate
                                , MRM.AddedFromIP
                                , MRM.UpdatedBy
                                , MRM.UpdatedDate
                                , MRM.UpdatedFromIP
                                , MRM.Remarks
                                , MRM.CheckedBy
                                , MRM.CheckedByStatus
                                , MRM.AuthorizedBy
                                , MRM.AuthorizedByStatus
                                , MRM.IsApproved
                                , A.UserName ActivityName
                                , MM.UserName MaterialName
                                , MRD.TransactionQty
                                , MRD.EstimatedRate
                                , MRD.TotalAmount
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                ,MRM.ReasonWhyItIsNotPlanEarlier Reason
                                ,MRM.ApprovedHoldRejectReason RejectApprovedReason
                                ,MRM.OrderRefNo,MRM.ReqEmpId PreparedById,MRM.ReqEmpId PreparedBy
                        FROM[TRN].[MaterialRequsitionMaster] MRM
                        Left Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                        Left Join org.Entity E on E.Id = MRM.EntityId
                        LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                        Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 ON EI4.SystemId=MRM.ReqEmpId
                        Where MRM.AuthorizedByStatus ='Hold' 
						--OR MRM.AuthorizedByStatus ='Reject' 
                        AND MRM.CheckedByStatus Is null
                        AND MRM.AuthorizedBy='" + identity.EmployeeId + @"' AND MRM.InActive=0 
                        )xyz
                        Group By
                                  Id
                                , RequisitionDate
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus,AddedBy,Reason,RejectApprovedReason,OrderRefNo,PreparedById,PreparedBy
                               -- Order By RequisitionDate ASC
			
                            UNION ALL
			                            Select
                                            Id
                                        , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                        , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                        , RequisitionType
                                        , RequirmentType
                                        , QualityApprovalResponsiblePersonId
                                        , CheckedBy
                                        , CheckedByStatus
                                        , CheckedByEmp
                                        , AuthorizedBy
                                        , AuthorizedByEmp
                                        , AuthorizedByStatus
                                            ,Sum(TransactionQty) TransactionQty
		                                ,Sum(EstimatedRate) EstimatedRate
		                                ,Sum(TotalAmount) TotalAmount
                                        ,AddedBy,Reason,RejectApprovedReason,OrderRefNo,PreparedById, PreparedBy
                                FROM
                                (
                                Select

                                            MRM.Id
                                        , MRM.RequisitionDate
                                        , MRM.RequisitionType
                                        , MRM.RequirmentType
                                        , MRM.QualityApprovalResponsiblePersonId
                                        , EI.EmployeeName As QualityApproval
                                        , MRM.NeedSpecialAppId
                                        , EI1.EmployeeName NeedSpecialApp
                                        , E.UserName EntityName
                                        , MRM.EntityId
                                        , MRM.ReasonWhyItIsNotPlanEarlier
                                        , EI4.EmployeeName  AddedBy
                                        , MRM.AddedDate
                                        , MRM.AddedFromIP
                                        , MRM.UpdatedBy
                                        , MRM.UpdatedDate
                                        , MRM.UpdatedFromIP
                                        , MRM.Remarks
                                        , MRM.CheckedBy
                                        , MRM.CheckedByStatus
                                        , MRM.AuthorizedBy
                                        , MRM.AuthorizedByStatus
                                        , MRM.IsApproved
                                        , A.UserName ActivityName
                                        , MM.UserName MaterialName
                                        , MRD.TransactionQty
                                        , MRD.EstimatedRate
                                        , MRD.TotalAmount
                                        , EI2.EmployeeName AS CheckedByEmp
                                        , EI3.EmployeeName AS AuthorizedByEmp
                                        ,MRM.ReasonWhyItIsNotPlanEarlier Reason
                                        ,MRM.ApprovedHoldRejectReason RejectApprovedReason
                                        ,MRM.OrderRefNo,MRM.ReqEmpId PreparedById,MRM.ReqEmpId PreparedBy
                                FROM[TRN].[MaterialRequsitionMaster] MRM
                                Left Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                                Left Join org.Entity E on E.Id = MRM.EntityId
                                LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                                Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                                Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                                Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                                Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                                Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                                Left JOIn Dbo.EmployeeInformation EI4 ON EI4.SystemId=MRM.ReqEmpId
                                Where MRM.AuthorizedByStatus ='Reject' 
                                AND MRM.CheckedByStatus Is null
                                AND MRM.AuthorizedBy='" + identity.EmployeeId + @"' AND MRM.InActive=0 
                                )xyz
                                Group By
                                            Id
                                        , RequisitionDate
                                        , RequisitionType
                                        , RequirmentType
                                        , QualityApprovalResponsiblePersonId
                                        , CheckedBy
                                        , CheckedByStatus
                                        , CheckedByEmp
                                        , AuthorizedBy
                                        , AuthorizedByEmp
                                        , AuthorizedByStatus,AddedBy,Reason,RejectApprovedReason,OrderRefNo,PreparedById,PreparedBy
                                        Order By RequisitionDate ASC";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetListRequisionApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetRequisionApproved(), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetRequisionApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                //var _sql = @"Select
                //                 Id
                //                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                //                , RequisitionType
                //                , RequirmentType
                //                , QualityApprovalResponsiblePersonId
                //                , CheckedBy
                //                , CheckedByStatus
                //                , CheckedByEmp
                //                , AuthorizedBy
                //                , AuthorizedByEmp
                //                , AuthorizedByStatus
                //                 ,Sum(TransactionQty) TransactionQty
                //          ,Sum(EstimatedRate) EstimatedRate
                //          ,Sum(TotalAmount) TotalAmount
                //                ,AddedBy,Reason,OrderRefNo,PreparedById
                //        FROM
                //        (
                //        Select

                //                 MRM.Id
                //                , MRM.RequisitionDate
                //                , MRM.RequisitionType
                //                , MRM.RequirmentType
                //                , MRM.QualityApprovalResponsiblePersonId
                //                , EI.EmployeeName As QualityApproval
                //                , MRM.NeedSpecialAppId
                //                , EI1.EmployeeName NeedSpecialApp
                //                , E.UserName EntityName
                //                , MRM.EntityId
                //                , MRM.ReasonWhyItIsNotPlanEarlier
                //                , EI4.EmployeeName  AddedBy
                //                , MRM.AddedDate
                //                , MRM.AddedFromIP
                //                , MRM.UpdatedBy
                //                , MRM.UpdatedDate
                //                , MRM.UpdatedFromIP
                //                , MRM.Remarks
                //                , MRM.CheckedBy
                //                , MRM.CheckedByStatus
                //                , MRM.AuthorizedBy
                //                , MRM.AuthorizedByStatus
                //                , MRM.IsApproved
                //                , A.UserName ActivityName
                //                , MM.UserName MaterialName
                //                , MRD.TransactionQty
                //                , MRD.EstimatedRate
                //                , MRD.TotalAmount
                //                , EI2.EmployeeName AS CheckedByEmp
                //                , EI3.EmployeeName AS AuthorizedByEmp
                //                ,MRM.ReasonWhyItIsNotPlanEarlier Reason,MRM.OrderRefNo,MRM.ReqEmpId PreparedById


                //        FROM[TRN].[MaterialRequsitionMaster] MRM
                //        Left Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                //        Left Join org.Entity E on E.Id = MRM.EntityId
                //        LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                //        Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                //        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                //        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                //        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                //        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                //       Left JOIn Dbo.EmployeeInformation EI4 ON EI4.SystemId=MRM.ReqEmpId
                //        Where MRM.CheckedByStatus='Checked' AND MRM.AuthorizedByStatus = 'Approval'
                //        AND MRM.AuthorizedBy='" + identity.EmployeeId + @"' AND MRM.InActive=0 
                //        )xyz
                //        Group By
                //                  Id
                //                , RequisitionDate
                //                , RequisitionType
                //                , RequirmentType
                //                , QualityApprovalResponsiblePersonId
                //                , CheckedBy
                //                , CheckedByStatus
                //                , CheckedByEmp
                //                , AuthorizedBy
                //                , AuthorizedByEmp
                //                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo,PreparedById
                //                Order By RequisitionDate ASC";

                var _sql = @"Select   Id
                                    , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                    , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                    , RequisitionType
                                    , RequirmentType
                                    , QualityApprovalResponsiblePersonId
                                    , CheckedBy
                                    , CheckedByStatus
                                    , CheckedByEmp
                                    , AuthorizedBy
                                    , AuthorizedByEmp
                                    , AuthorizedByStatus
                                    ,Sum(TransactionQty) TransactionQty
		                            ,Sum(EstimatedRate) EstimatedRate
		                            ,Sum(TotalAmount) TotalAmount
                                    ,AddedBy,Reason,OrderRefNo,PreparedById,PreparedBy
                            FROM
                            (
                            Select

                                        MRM.Id
                                    , MRM.RequisitionDate
                                    , MRM.RequisitionType
                                    , MRM.RequirmentType
                                    , MRM.QualityApprovalResponsiblePersonId
                                    , EI.EmployeeName As QualityApproval
                                    , MRM.NeedSpecialAppId
                                    , EI1.EmployeeName NeedSpecialApp
                                    , E.UserName EntityName
                                    , MRM.EntityId
                                    , MRM.ReasonWhyItIsNotPlanEarlier
                                    , EI4.EmployeeName  AddedBy
                                    , MRM.AddedDate
                                    , MRM.AddedFromIP
                                    , MRM.UpdatedBy
                                    , MRM.UpdatedDate
                                    , MRM.UpdatedFromIP
                                    , MRM.Remarks
                                    , MRM.CheckedBy
                                    , MRM.CheckedByStatus
                                    , MRM.AuthorizedBy
                                    , MRM.AuthorizedByStatus
                                    , MRM.IsApproved
                                    , A.UserName ActivityName
                                    , MM.UserName MaterialName
                                    , MRD.TransactionQty
                                    , MRD.EstimatedRate
                                    , MRD.TotalAmount
                                    , EI2.EmployeeName AS CheckedByEmp
                                    , EI3.EmployeeName AS AuthorizedByEmp
                                    ,MRM.ReasonWhyItIsNotPlanEarlier Reason,MRM.OrderRefNo,MRM.ReqEmpId PreparedById
                                    ,MRM.ReqEmpId PreparedBy


                            FROM[TRN].[MaterialRequsitionMaster] MRM
                            Left Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                            Left Join org.Entity E on E.Id = MRM.EntityId
                            LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                            Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                            Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                            Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                            Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                            Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                            Left JOIn Dbo.EmployeeInformation EI4 ON EI4.SystemId=MRM.ReqEmpId
                            Where MRM.CheckedByStatus='Checked' 
                            AND MRM.AuthorizedByStatus = 'Approved'
                            AND MRM.InActive=0
                            AND MRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            )xyz
                            Group By
                                        Id
                                    , RequisitionDate
                                    , RequisitionType
                                    , RequirmentType
                                    , QualityApprovalResponsiblePersonId
                                    , CheckedBy
                                    , CheckedByStatus
                                    , CheckedByEmp
                                    , AuthorizedBy
                                    , AuthorizedByEmp
                                    , AuthorizedByStatus,AddedBy,Reason,OrderRefNo,PreparedById,PreparedBy
                                    -- Order By RequisitionDate ASC

		                            UNION ALL
		                            Select
                                        Id
                                    , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                    , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate1 
                                    , RequisitionType
                                    , RequirmentType
                                    , QualityApprovalResponsiblePersonId
                                    , CheckedBy
                                    , CheckedByStatus
                                    , CheckedByEmp
                                    , AuthorizedBy
                                    , AuthorizedByEmp
                                    , AuthorizedByStatus
                                        ,Sum(TransactionQty) TransactionQty
		                            ,Sum(EstimatedRate) EstimatedRate
		                            ,Sum(TotalAmount) TotalAmount
                                    ,AddedBy,Reason,OrderRefNo,PreparedById,PreparedBy
                            FROM
                            (
                            Select

                                        MRM.Id
                                    , MRM.RequisitionDate
                                    , MRM.RequisitionType
                                    , MRM.RequirmentType
                                    , MRM.QualityApprovalResponsiblePersonId
                                    , EI.EmployeeName As QualityApproval
                                    , MRM.NeedSpecialAppId
                                    , EI1.EmployeeName NeedSpecialApp
                                    , E.UserName EntityName
                                    , MRM.EntityId
                                    , MRM.ReasonWhyItIsNotPlanEarlier
                                    , EI4.EmployeeName  AddedBy
                                    , MRM.AddedDate
                                    , MRM.AddedFromIP
                                    , MRM.UpdatedBy
                                    , MRM.UpdatedDate
                                    , MRM.UpdatedFromIP
                                    , MRM.Remarks
                                    , MRM.CheckedBy
                                    , MRM.CheckedByStatus
                                    , MRM.AuthorizedBy
                                    , MRM.AuthorizedByStatus
                                    , MRM.IsApproved
                                    , A.UserName ActivityName
                                    , MM.UserName MaterialName
                                    , MRD.TransactionQty
                                    , MRD.EstimatedRate
                                    , MRD.TotalAmount
                                    , EI2.EmployeeName AS CheckedByEmp
                                    , EI3.EmployeeName AS AuthorizedByEmp
                                    ,MRM.ReasonWhyItIsNotPlanEarlier Reason,MRM.OrderRefNo,MRM.ReqEmpId PreparedById,MRM.ReqEmpId PreparedBy


                            FROM[TRN].[MaterialRequsitionMaster] MRM
                            Left Join[TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId = MRM.Id
                            Left Join org.Entity E on E.Id = MRM.EntityId
                            LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                            Left Join MST.MaterialMaster MM on MM.Id = MRD.MaterialMasterId
                            Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = MRM.QualityApprovalResponsiblePersonId
                            Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = MRM.NeedSpecialAppId
                            Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = MRM.CheckedBy
                            Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = MRM.AuthorizedBy
                            Left JOIn Dbo.EmployeeInformation EI4 ON EI4.SystemId=MRM.ReqEmpId
                            Where MRM.CheckedByStatus IS NULL
                            AND MRM.AuthorizedByStatus = 'Approved'
                            AND MRM.InActive=0 
                            AND MRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            )xyz
                            Group By
                                        Id
                                    , RequisitionDate
                                    , RequisitionType
                                    , RequirmentType
                                    , QualityApprovalResponsiblePersonId
                                    , CheckedBy
                                    , CheckedByStatus
                                    , CheckedByEmp
                                    , AuthorizedBy
                                    , AuthorizedByEmp
                                    , AuthorizedByStatus,AddedBy,Reason,OrderRefNo,PreparedById,PreparedBy
                                    Order By RequisitionDate ASC";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        [HttpPost, Authorize]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult ReqChecked(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy,string CheckedHoldRejectReason,string RequisitionType, string RequirmentType, string CheckedBy, string PreparedBY)
        {
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			if (identity.EmployeeId== AuthorizedBy)
			{
				throw new CustomException("Please Select Another Id for To be Approved!");
			}			
            ReqChecked1(PoId, PoValue, CheckedStataus, AuthorizedBy, CheckedHoldRejectReason, RequisitionType, RequirmentType, CheckedBy, PreparedBY);
            return Json(new { Message = "Requisition  Checked" + AplosMessage.Success });
        }
        public void ReqChecked1(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string CheckedHoldRejectReason, string RequisitionType, string RequirmentType, string CheckedBy,string PreparedBY)
        {
            try
            {
                var AuthorizedById = "";
                var AuthorizedByStatus = "";
                PoValue = "0";
                //  var Id = GetPK();
                if (CheckedStataus == "Checked")
                {
                    if (AuthorizedBy == null || AuthorizedBy == "")
                    {
                        throw new CustomException("Select Approved By");
                    }
                    AuthorizedById = AuthorizedBy;
                    AuthorizedByStatus = "For Approval";

                    var DailySendMailRequisition = _notificationSetting.SqlQuery<bool>(@"Select NotificationAfterChecking  from NotificationSetting Where BusinessFlow = 'MaterialRequistion'").FirstOrDefault();
                    if (DailySendMailRequisition == true)
                    {
                        DailySendMailRequisitionApproved(RequisitionType, RequirmentType, CheckedBy, AuthorizedById, PoId, PreparedBY);

                    }

                    else
                    {

                    }

                }
                else
                {
                    AuthorizedById = DBNull.Value.ToString();

                }
                var Status = CheckedStataus;
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql = "Update TRN.MaterialRequsitionMaster set CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "',AuthorizedByStatus='"+ AuthorizedByStatus + "', CheckedHoldRejectReason='" + CheckedHoldRejectReason + "' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.RequisitionApprovalLog(" +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "ReqValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,ReqId) " +
                "values ('" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        [HttpPost, Authorize]
        public JsonResult DailySendMailRequisitionApproved(string RequisitionType, string RequirmentType, string CheckedBy, string ApprovedBy,string PoId,string PreparedBY) 
        {
            _materialRequsitionMasterServiceService.DailySendMailRequisitionApproved("TS", "TS", "10215", RequisitionType, RequirmentType, CheckedBy, ApprovedBy, PoId,PreparedBY);
            return Json(new { Message = AplosMessage.Success });
        }


        [HttpPost, Authorize]
        public JsonResult DailySendMailRequisitionCreator(string RequisitionType, string RequirmentType, string CheckedBy, string ApprovedBy, string PoId, string PreparedBY, string PreparedBYId)
        {
            _materialRequsitionMasterServiceService.DailySendMailRequisitionCreator("TS", "TS", "10215", RequisitionType, RequirmentType, CheckedBy, ApprovedBy, PoId, PreparedBY, PreparedBYId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult ReqApprovedAuth(string PoId, decimal PoValue, string CheckedStataus, string AuthorizedBy,string RejectApprovedReason, string RequisitionType, string RequirmentType, string CheckedBy, string ApprovedBy, string PreparedBY, string PreparedBYId)
        {
            ReqApprovedAuth3(PoId, PoValue, CheckedStataus, AuthorizedBy, RejectApprovedReason, RequisitionType, RequirmentType, CheckedBy, ApprovedBy, PreparedBY, PreparedBYId);
            return Json(new { Message = "Requisition Approved" + AplosMessage.Success });
        }

        private DataTable GetCurrentClanderYear()
        {
            try
            {
                var sql = @"Select NotificationAfterApproval from dbo.NotificationSetting Where BusinessFlow = 'MaterialRequistion'";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ReqApprovedAuth3(string PoId, decimal PoValue, string CheckedStataus, string AuthorizedBy,string RejectApprovedReason, string RequisitionType, string RequirmentType, string CheckedBy, string ApprovedBy, string PreparedBY, string PreparedBYId)
        {
            try
            {
                var IsApproved = 0;
                //bool DailySendMailRequisitionCreat = false;

                //PoValue = 0;
                //  var Id = GetPK();
                if (CheckedStataus == "Approved")
                {
                    IsApproved = 1;
                    //DataTable testId = GetCurrentClanderYear();
                    //if(testId.Rows.Count>0)
                    //{
                    //    if(Convert.ToBoolean(testId.Rows[0]["NotificationAfterApproval"]))
                    //    {
                    //        DailySendMailRequisitionCreator(RequisitionType, RequirmentType, CheckedBy, ApprovedBy, PoId, PreparedBY, PreparedBYId);

                    //    }
                    //}


                    var DailySendMailRequisitionCreat = _notificationSetting.SqlQuery<bool>(@"Select NotificationAfterApproval from NotificationSetting Where BusinessFlow = 'MaterialRequistion'").FirstOrDefault();
                  
                    if (DailySendMailRequisitionCreat)
                    {
                        DailySendMailRequisitionCreator(RequisitionType, RequirmentType, CheckedBy, ApprovedBy, PoId, PreparedBY, PreparedBYId);

                    }
                    else
                    {

                    }
                }


                else
                {
                    IsApproved = 0;

                }
                var Status = CheckedStataus;
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql = "Update TRN.MaterialRequsitionMaster set AuthorizedByStatus='" + Status + "',ApprovedHoldRejectReason='"+ RejectApprovedReason + "' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.RequisitionApprovalLog(" +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "ReqValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,ReqId) " +
                "values ('" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + Convert.ToDecimal(0) + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        [HttpPost, Authorize]
        public JsonResult ReqUnApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            ReqUnApproved2(PoId, PoValue, CheckedStataus, AuthorizedBy);
            return Json(new { Message = "PO Approved" + AplosMessage.Success });
        }
        public void ReqUnApproved2(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            try
            {
                //var AuthorizedById = "";

                PoValue = "0";
                //var Id = GetPK();
                //if (CheckedStataus == "Checked")
                //{
                //    if (AuthorizedBy == null || AuthorizedBy == "")
                //    {
                //        throw new CustomException("Select Approved By");
                //    }
                //    AuthorizedById = AuthorizedBy;

                //}
                //else
                //{
                //    AuthorizedById = null;

                //}
                var Status = "UnApproved";
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql = "Update TRN.MaterialRequsitionMaster set CheckedBy=null,CheckedByStatus=null,AuthorizedBy=null,AuthorizedByStatus=null where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.RequisitionApprovalLog(" +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,ReqId) " +
                "values ('" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #endregion Requisition


        #region


        [Authorize, HttpPost]
        public JsonResult GetMaterialLastPOQty(string materialMasterId, string Id, string Sku1, string Sku2, string Sku3)
        {
           
            return Json(GetMaterialLastPOQtyData(materialMasterId, Id, Sku1, Sku2, Sku3), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetMaterialLastPOQtyData(string MMId, string Id, string Sku1,string Sku2, string Sku3)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                var _sql = @"SELECT    top 1 MM.id MaterialId
		                            ,Isnull(MM.UserName,'') Material
		                            ,ART.Id ArticleId
		                            ,isnull(ART.StandardName,'') Article
		                            ,FCV.Id FCVId
		                            ,ISNULL(FCV.UserName, '') AS Sku1
		                            ,SCV.Id SCVId
		                            ,ISNULL(SCV.UserName, '') AS Sku2
		                            ,TCV.Id TCVId
		                            ,ISNULL(TCV.UserName, '') AS Sku3	
		                            ,IRD.Description	
		                            ,IR.Id
		                            ,REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate 
		                            ,IRD.AddedDate
		                            ,p.StandardName
		                            ,CU.Code
		                            ,IRD.TransactionQty
		                            ,IRD.TransactionRate
		                            ,IRD.TransactionAmount
		                            --,MAX(IR.GRNDate) GRNDate
                            FROM   TRN.PurchaseOrderDetail IRD
                            LEFT JOIN trn.PurchaseOrder IR ON IR.Id=IRD.InventoryReceiveId
                           -- LEFT JOIN TRN.POMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId = MM.Id 
							--AND IM.MaterialMasterId=IRD.InventoryMaterialId AND IM.ArticleId=IRD.ArticleId 
							--AND IM.FirstCharacteristicsId=IRD.FirstCharacteristicsId
							--AND IM.SecondCharacteristicsId=IRD.SecondCharacteristicsId
							--AND IM.ThirdCharacteristicsId=IRD.ThirdCharacteristicsId
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                            Left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN HKP.Party AS P ON P.id=IR.PartyId
                            WHERE  IRD.InventoryMaterialId is not null AND Art.Id is not null --ANd FCV.Id is not null AND SCV.Id is not null AND TCV.Id is not null
                            AND isnull(IRD.InventoryMaterialId,'')='" + MMId + @"' AND isnull(IRD.ArticleId,'')='" + Id + @"' ANd isnull(IRD.FirstCharacteristicsValueId,'')='" + Sku1 + @"'AND isnull(IRD.SecondCharacteristicsValueId,'')='" + Sku2 + @"' AND isnull(IRD.ThirdCharacteristicsValueId,'')='" + Sku3 + @"' 
                            AND IR.AuthorizedByStatus='Approved'
                            Order BY AddedDate DESC";

            return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion

    }


}