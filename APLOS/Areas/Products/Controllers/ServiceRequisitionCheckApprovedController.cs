
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
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
    public class ServiceRequisitionCheckApprovedController : Controller
    {

        #region Constructor
        private readonly IMaterialRequsitionMasterServiceService _materialRequsitionMasterServiceService;
        private readonly ISqlRepository _sqlRepository;
        public ServiceRequisitionCheckApprovedController(
             ISqlRepository sqlRepository
            , IMaterialRequsitionMasterServiceService materialRequsitionMasterServiceService)
        {
            _sqlRepository = sqlRepository;
            _materialRequsitionMasterServiceService = materialRequsitionMasterServiceService;
        }

        #endregion Constructor

        #region Aplos
        [Authorize]
        public ActionResult ServiceReqCheck()
        {
            return View();
        }
        [Authorize]
        public ActionResult ServiceReqApprove() 
        {
            return View();
        }


        #endregion Aplos

        #region Service Requisition 
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
                          where A.ActionStatus='ServiceRequisitionApproveBy' AND E.EmployeeStatus='Active'";
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

        [HttpGet, Authorize]
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
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                          --       ,Sum(TransactionQty) TransactionQty
		                        --,Sum(EstimatedRate) EstimatedRate
		                        ,sum(TotalServiceTranAmount) TotalAmount
								,sum(TotalServiceBooksCurrencyAmount) TotalAmountBC
                                ,AddedBy,Reason
                        FROM
                        (
                        Select

                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                --, A.UserName ActivityName
                                --, MM.UserName MaterialName
                                --, SRD.TransactionQty
                                --, SRD.EstimatedRate
                                 
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                , EI4.EmployeeName AS AddedBy
                                ,SRM.ReasonWhyItIsNotPlanEarlier Reason
								,sum(SRD.TotalServiceTranAmount) TotalServiceTranAmount
								,sum(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                        FROM [TRN].[ServiceRequsitionMaster] SRM
                         Left Join ( select ServiceRequisitionMasterID , sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from  [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID,TotalServiceBooksCurrencyAmount) SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id = SRM.EntityId
                       -- LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                       -- Left Join MST.MaterialMaster MM on MM.Id = SRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = SRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = SRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = SRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = SRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 On EI4.SystemId = SRM.ReqEmpId
                        Where  SRM.CheckedByStatus <>'Hold'
					    and SRM.CheckedByStatus <> 'Reject' 
						 and SRM.CheckedByStatus ='For Checking' 
                        AND SRM.CheckedBy='"+identity.EmployeeId+@"'
						group By
						
                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName 
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName 
                                , E.UserName 
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                --, A.UserName ActivityName
                                --, MM.UserName MaterialName
                                --, SRD.TransactionQty
                                --, SRD.EstimatedRate
                                 
                                , EI2.EmployeeName 
                                , EI3.EmployeeName 
                                , EI4.EmployeeName 
                                ,SRM.ReasonWhyItIsNotPlanEarlier 
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
                                , AuthorizedByStatus,AddedBy,Reason
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
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
		                        ,sum(TotalServiceTranAmount) TotalAmount
								,sum(TotalServiceBooksCurrencyAmount) TotalAmountBC
                                ,AddedBy,Reason,ReasonHR 
                        FROM
                        (
                        Select

                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                , EI4.EmployeeName AS AddedBy
                                ,SRM.ReasonWhyItIsNotPlanEarlier Reason
									,SRM.CheckedHoldRejectReason ReasonHR
								,sum(SRD.TotalServiceTranAmount) TotalServiceTranAmount
								,sum(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                        FROM [TRN].[ServiceRequsitionMaster] SRM
                         Left Join ( select ServiceRequisitionMasterID , sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from  [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID,TotalServiceBooksCurrencyAmount) SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id = SRM.EntityId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = SRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = SRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = SRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = SRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 On EI4.SystemId = SRM.ReqEmpId
                      Where SRM.CheckedbyStatus ='Hold' OR SRM.CheckedbyStatus ='Reject' 
					 AND SRM.CheckedByStatus <> 'Checked' 
                         AND SRM.CheckedBy='" + identity.EmployeeId + @"'
						group By
						
                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName 
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName 
                                , E.UserName 
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                , EI2.EmployeeName 
                                , EI3.EmployeeName 
                                , EI4.EmployeeName 
                                ,SRM.ReasonWhyItIsNotPlanEarlier 
							  ,SRM.CheckedHoldRejectReason 
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
                                , AuthorizedByStatus,AddedBy,Reason,ReasonHR
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
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                          --       ,Sum(TransactionQty) TransactionQty
		                        --,Sum(EstimatedRate) EstimatedRate
		                        ,sum(TotalServiceTranAmount) TotalAmount
								,sum(TotalServiceBooksCurrencyAmount) TotalAmountBC
                                ,AddedBy,Reason
                        FROM
                        (
                        Select

                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                --, A.UserName ActivityName
                                --, MM.UserName MaterialName
                                --, SRD.TransactionQty
                                --, SRD.EstimatedRate
                                 
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                , EI4.EmployeeName AS AddedBy
                                ,SRM.ReasonWhyItIsNotPlanEarlier Reason
								,sum(SRD.TotalServiceTranAmount) TotalServiceTranAmount
								,sum(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                        FROM [TRN].[ServiceRequsitionMaster] SRM
                         Left Join ( select ServiceRequisitionMasterID , sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from  [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID,TotalServiceBooksCurrencyAmount) SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id = SRM.EntityId
                       -- LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                       -- Left Join MST.MaterialMaster MM on MM.Id = SRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = SRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = SRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = SRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = SRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 On EI4.SystemId = SRM.ReqEmpId
                        Where SRM.CheckedByStatus = 'Checked'
                        AND SRM.CheckedBy='" + identity.EmployeeId + @"'
						group By
						
                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName 
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName 
                                , E.UserName 
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                --, A.UserName ActivityName
                                --, MM.UserName MaterialName
                                --, SRD.TransactionQty
                                --, SRD.EstimatedRate
                                 
                                , EI2.EmployeeName 
                                , EI3.EmployeeName 
                                , EI4.EmployeeName 
                                ,SRM.ReasonWhyItIsNotPlanEarlier 
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
                                , AuthorizedByStatus,AddedBy,Reason
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


        [HttpGet, Authorize]
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
                
                var _sql = @"Select
                                 Id
                                , REPLACE(CONVERT(CHAR(11), RequisitionDate, 106),' ','-') AS RequisitionDate 
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
                          --       ,Sum(TransactionQty) TransactionQty
		                        --,Sum(EstimatedRate) EstimatedRate
		                        ,sum(TotalServiceTranAmount) TotalAmount
								,sum(TotalServiceBooksCurrencyAmount) TotalAmountBC
                                ,AddedBy,Reason
                        FROM
                        (
                        Select

                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                --, A.UserName ActivityName
                                --, MM.UserName MaterialName
                                --, SRD.TransactionQty
                                --, SRD.EstimatedRate
                                 
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                , EI4.EmployeeName AS AddedBy
                                ,SRM.ReasonWhyItIsNotPlanEarlier Reason
								,sum(SRD.TotalServiceTranAmount) TotalServiceTranAmount
								,sum(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                        FROM [TRN].[ServiceRequsitionMaster] SRM
                         Left Join ( select ServiceRequisitionMasterID , sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from  [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID,TotalServiceBooksCurrencyAmount) SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id = SRM.EntityId
                       -- LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                       -- Left Join MST.MaterialMaster MM on MM.Id = SRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = SRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = SRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = SRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = SRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 On EI4.SystemId = SRM.ReqEmpId
                      Where SRM.AuthorizedBy='" + identity.EmployeeId + @"'
					    AND  SRM.CheckedByStatus ='Checked' 
					   AND SRM.AuthorizedByStatus ='For Approval'
                       
						group By
						
                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName 
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName 
                                , E.UserName 
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                --, A.UserName ActivityName
                                --, MM.UserName MaterialName
                                --, SRD.TransactionQty
                                --, SRD.EstimatedRate
                                 
                                , EI2.EmployeeName 
                                , EI3.EmployeeName 
                                , EI4.EmployeeName 
                                ,SRM.ReasonWhyItIsNotPlanEarlier 


								UNION ALL

								 Select

                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                --, A.UserName ActivityName
                                --, MM.UserName MaterialName
                                --, SRD.TransactionQty
                                --, SRD.EstimatedRate
                                 
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                , EI4.EmployeeName AS AddedBy
                                ,SRM.ReasonWhyItIsNotPlanEarlier Reason
								,sum(SRD.TotalServiceTranAmount) TotalServiceTranAmount
								,sum(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                        FROM [TRN].[ServiceRequsitionMaster] SRM
                         Left Join ( select ServiceRequisitionMasterID , sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from  [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID,TotalServiceBooksCurrencyAmount) SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id = SRM.EntityId
                       -- LEFT JOin HKp.Activity A On A.Id = MRD.ActivityId
                       -- Left Join MST.MaterialMaster MM on MM.Id = SRD.MaterialMasterId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = SRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = SRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = SRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = SRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 On EI4.SystemId = SRM.ReqEmpId
                      Where SRM.AuthorizedBy='" + identity.EmployeeId + @"'
					   AND SRM.CheckedByStatus is null   
					   AND SRM.AuthorizedByStatus ='For Approval'
                        
						group By
						
                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName 
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName 
                                , E.UserName 
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                --, A.UserName ActivityName
                                --, MM.UserName MaterialName
                                --, SRD.TransactionQty
                                --, SRD.EstimatedRate
                                 
                                , EI2.EmployeeName 
                                , EI3.EmployeeName 
                                , EI4.EmployeeName 
                                ,SRM.ReasonWhyItIsNotPlanEarlier 
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
                                , AuthorizedByStatus,AddedBy,Reason
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
                                , RequisitionType
                                , RequirmentType
                                , QualityApprovalResponsiblePersonId
                                , CheckedBy
                                , CheckedByStatus
                                , CheckedByEmp
                                , AuthorizedBy
                                , AuthorizedByEmp
                                , AuthorizedByStatus
		                        ,sum(TotalServiceTranAmount) TotalAmount
								,sum(TotalServiceBooksCurrencyAmount) TotalAmountBC
                                ,AddedBy,Reason,ReasonAHR
                        FROM
                        (
                        Select

                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName As QualityApproval
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName NeedSpecialApp
                                , E.UserName EntityName
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                , EI2.EmployeeName AS CheckedByEmp
                                , EI3.EmployeeName AS AuthorizedByEmp
                                , EI4.EmployeeName AS AddedBy
                                ,SRM.ReasonWhyItIsNotPlanEarlier Reason
	                            ,SRM.ApprovedHoldRejectReason ReasonAHR
								,sum(SRD.TotalServiceTranAmount) TotalServiceTranAmount
								,sum(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                        FROM [TRN].[ServiceRequsitionMaster] SRM
                         Left Join ( select ServiceRequisitionMasterID , sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from  [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID,TotalServiceBooksCurrencyAmount) SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id = SRM.EntityId
                        Left JOIn Dbo.EmployeeInformation EI On EI.SystemId = SRM.QualityApprovalResponsiblePersonId
                        Left JOIn Dbo.EmployeeInformation EI1 On EI1.SystemId = SRM.NeedSpecialAppId
                        Left JOIn Dbo.EmployeeInformation EI2 On EI2.SystemId = SRM.CheckedBy
                        Left JOIn Dbo.EmployeeInformation EI3 On EI3.SystemId = SRM.AuthorizedBy
                        Left JOIn Dbo.EmployeeInformation EI4 On EI4.SystemId = SRM.ReqEmpId
                      Where SRM.CheckedByStatus='Checked' And SRM.AuthorizedByStatus<>'Approved'
                      And SRM.AuthorizedByStatus='Hold' 
					  OR SRM.AuthorizedByStatus='Reject'  
                    AND SRM.AuthorizedBy='" + identity.EmployeeId + @"'
						group By
						
                                 SRM.Id
                                , SRM.RequisitionDate
                                , SRM.RequisitionType
                                , SRM.RequirmentType
                                , SRM.QualityApprovalResponsiblePersonId
                                , EI.EmployeeName 
                                , SRM.NeedSpecialAppId
                                , EI1.EmployeeName 
                                , E.UserName 
                                , SRM.EntityId
                                , SRM.ReasonWhyItIsNotPlanEarlier
                                --, MRM.AddedBy
                                , SRM.AddedDate
                                , SRM.AddedFromIP
                                , SRM.UpdatedBy
                                , SRM.UpdatedDate
                                , SRM.UpdatedFromIP
                                , SRM.Remarks
                                , SRM.CheckedBy
                                , SRM.CheckedByStatus
                                , SRM.AuthorizedBy
                                , SRM.AuthorizedByStatus
                                , SRM.IsApproved
                                , EI2.EmployeeName 
                                , EI3.EmployeeName 
                                , EI4.EmployeeName 
                                ,SRM.ReasonWhyItIsNotPlanEarlier 
                                ,SRM.ApprovedHoldRejectReason
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
                                , AuthorizedByStatus,AddedBy,Reason,ReasonAHR
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
               
                var _sql = @"SELECT * FROM ( Select 
	                        SRM.Id
	                        ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName EntityName,SRM.ReasonWhyItIsNotPlanEarlier Reason
							,SUM(SRD.TotalServiceTranAmount) TotalAmount
							 ,SUM(SRD.TotalServiceBooksCurrencyAmount) TotalAmountBC
                            ,ei.EmployeeName  AS CheckedByEmp
							,ei1.EmployeeName AS AuthorizedByEmp
							,AuthorizedBy=CASE WHEN SRM.AuthorizedByStatus='Hold' THEN '' 
								  WHEN SRM.AuthorizedByStatus='Reject' THEN ''
								  ELSE ei1.EmployeeName END
					    ,SRM.ApprovedHoldRejectReason
                         FROM [TRN].[ServiceRequsitionMaster] SRM 
					  Left Join (select ServiceRequisitionMasterID,sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID)SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id=SRM.EntityId
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=SRM.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=SRM.AuthorizedBy
                     Where SRM.CheckedByStatus='Checked' 
					  AND SRM.AuthorizedByStatus ='Approved'  
                AND SRM.AuthorizedBy='" + identity.EmployeeId + @"' 
                        group by SRM.Id
						  ,SRM .AuthorizedByStatus
						 ,SRM .CheckedByStatus
	                        ,SRM.RequisitionDate
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName ,SRM.ReasonWhyItIsNotPlanEarlier
                            ,ei.EmployeeName
	                        ,ei1.EmployeeName,SRM.AuthorizedByStatus,SRM.ApprovedHoldRejectReason
                          UNION All  
                            Select 
	                        SRM.Id
	                        ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName EntityName,SRM.ReasonWhyItIsNotPlanEarlier Reason
							,SUM(SRD.TotalServiceTranAmount) TotalAmount
							 ,SUM(SRD.TotalServiceBooksCurrencyAmount) TotalAmountBC
                            ,ei.EmployeeName  AS CheckedByEmp
							,ei1.EmployeeName AS AuthorizedByEmp
							,AuthorizedBy=CASE WHEN SRM.AuthorizedByStatus='Hold' THEN '' 
								  WHEN SRM.AuthorizedByStatus='Reject' THEN ''
								  ELSE ei1.EmployeeName END
					    ,SRM.ApprovedHoldRejectReason
                         FROM [TRN].[ServiceRequsitionMaster] SRM 
					  Left Join (select ServiceRequisitionMasterID,sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID)SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id=SRM.EntityId
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=SRM.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=SRM.AuthorizedBy
                     Where SRM.CheckedByStatus Is null
					  AND SRM.AuthorizedByStatus Is Null
               AND SRM.AuthorizedBy='" + identity.EmployeeId + @"' 
                        group by SRM.Id
						  ,SRM .AuthorizedByStatus
						 ,SRM .CheckedByStatus
	                        ,SRM.RequisitionDate
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName ,SRM.ReasonWhyItIsNotPlanEarlier
                            ,ei.EmployeeName
	                        ,ei1.EmployeeName,SRM.AuthorizedByStatus,SRM.ApprovedHoldRejectReason

							
							UNION All
						Select 	 SRM.Id
	                        ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName EntityName,SRM.ReasonWhyItIsNotPlanEarlier Reason
							,SUM(SRD.TotalServiceTranAmount) TotalAmount
							 ,SUM(SRD.TotalServiceBooksCurrencyAmount) TotalAmountBC
                            ,ei.EmployeeName  AS CheckedByEmp
							,ei1.EmployeeName AS AuthorizedByEmp
							,AuthorizedBy=CASE WHEN SRM.AuthorizedByStatus='Hold' THEN '' 
								  WHEN SRM.AuthorizedByStatus='Reject' THEN ''
								  ELSE ei1.EmployeeName END
					    ,SRM.ApprovedHoldRejectReason
                         FROM [TRN].[ServiceRequsitionMaster] SRM 
					  Left Join (select ServiceRequisitionMasterID,sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID)SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id=SRM.EntityId
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=SRM.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=SRM.AuthorizedBy
                     Where 	 SRM.CheckedByStatus Is null
					 AND SRM.AuthorizedByStatus ='Approved'  
                AND SRM.AuthorizedBy='" + identity.EmployeeId + @"' 
                        group by SRM.Id
						  ,SRM .AuthorizedByStatus
						 ,SRM .CheckedByStatus
	                        ,SRM.RequisitionDate
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName ,SRM.ReasonWhyItIsNotPlanEarlier
                            ,ei.EmployeeName
	                        ,ei1.EmployeeName,SRM.AuthorizedByStatus,SRM.ApprovedHoldRejectReason
                      )X  
					  Order By Id Desc  ";
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
        public JsonResult ReqChecked(string SRMId, string PoValue, string CheckedStataus, string AuthorizedBy,string CheckedHoldRejectReason,string RequisitionType, string RequirmentType, string CheckedBy, string PreparedBY)
        {
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			if (identity.EmployeeId== AuthorizedBy)
			{
				throw new CustomException("Please Select Another Id for To be Approved!");
			}			
            ReqChecked1(SRMId, PoValue, CheckedStataus, AuthorizedBy, CheckedHoldRejectReason, RequisitionType, RequirmentType, CheckedBy, PreparedBY);
            return Json(new { Message = "Service Requisition  Checked" + AplosMessage.Success });
        }
        public void ReqChecked1(string SRMId, string PoValue, string CheckedStataus, string AuthorizedBy, string CheckedHoldRejectReason, string RequisitionType, string RequirmentType, string CheckedBy,string PreparedBY)
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
                    //DailySendMailRequisitionApproved( RequisitionType,  RequirmentType,  CheckedBy, AuthorizedById, SRMId, PreparedBY);

                }
                else
                {
                    AuthorizedById = null;

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
                string _sql = "Update [TRN].[ServiceRequsitionMaster] set CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "',CheckedHoldRejectReason='"+ CheckedHoldRejectReason + "',AuthorizedByStatus='"+ AuthorizedByStatus + "' where id='" + SRMId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.ServiceRequisitionApprovalLog(" +
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
                "'" + ip + "','" + SRMId + "')";
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
        public JsonResult ReqApprovedAuth(string SRMID, string PoValue, string CheckedStataus, string AuthorizedBy,string RejectApprovedReason)
        {
            ReqApprovedAuth3(SRMID, PoValue, CheckedStataus, AuthorizedBy, RejectApprovedReason);
            return Json(new { Message = "PO Approved" + AplosMessage.Success });
        }
	
		public void ReqApprovedAuth3(string SRMID, string PoValue, string CheckedStataus, string AuthorizedBy,string RejectApprovedReason)
        {
            try
            {
                var IsApproved = 0;

                PoValue = "0";
                if (CheckedStataus == "Approved")
                {
                    IsApproved = 1;

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
                string _sql = "Update TRN.ServiceRequsitionMaster set AuthorizedByStatus='" + Status + "',ApprovedHoldRejectReason='"+ RejectApprovedReason + "' where id='" + SRMID + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.ServiceRequisitionApprovalLog(" +
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
                "'" + ip + "','" + SRMID + "')";
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
                            AND IR.AuthorizedByStatus='Approval'
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