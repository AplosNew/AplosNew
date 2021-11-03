#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Securites;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using RouteAttribute = System.Web.Http.RouteAttribute;





#endregion Using

namespace Aplos.Controllers
{
	public class MaterialController : ApiController
	{
		#region Constructor

		private readonly IUserService _userService;
		private readonly ISqlRepository _sqlRepository;
		public MaterialController(IUserService userService, ISqlRepository sqlRepository)
		{
			_userService = userService;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor


		// GET api/ptemployees
		[Route("api/Material/Login")]
		public HttpResponseMessage Get(string UserId, string password)
		{
			var employees = @"select Id,EmployeeId from [SEC].[User] where id='" + UserId + "' and EmployeeId='" + password + "'";
			var res = _sqlRepository.GetDataCollection(employees);
			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, res);
			return response;
		}

		#region Requisition Data
		[Route("api/Material/CheckedHoldRejectApproved")]
		public HttpResponseMessage GetData(string Status, string EmployeeId)
		{
			var sql = "";
			//var Status = "For Checking";
			if (Status == "For Checking")
			{
				sql = @"Select
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
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo
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
                        Where MRM.CheckedByStatus <> 'Checked' AND MRM.CheckedByStatus <>'Hold' and MRM.CheckedByStatus <> 'Reject' 
                        AND MRM.CheckedBy='" + EmployeeId + @"'
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
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo
                                Order By RequisitionDate ASC";
			}
			else if (Status == "Hold/Reject")
			{
				sql = @"Select
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
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo
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
                        Where MRM.CheckedByStatus <> 'Checked' AND MRM.CheckedByStatus = 'Hold' OR MRM.CheckedByStatus = 'Reject' 
                        AND MRM.CheckedBy='" + EmployeeId + @"'
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
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo
                                Order By RequisitionDate ASC";
			}
			else if (Status == "Checked")
			{
				sql = @"Select
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
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo
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
                        Where MRM.CheckedByStatus ='Checked' And MRM.AuthorizedBy is not null
                        AND MRM.CheckedBy='" + EmployeeId + @"'
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
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo
                                Order By RequisitionDate ASC";
			}
			else if (Status == "For Approving")
			{
				sql = @"Select
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
                                ,AuthorizedByStatus
                                ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo
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
								--,MRM.AuthorizedByStatus
                                ,Case when  MRM.AuthorizedByStatus <>'Hold' AND MRM.AuthorizedByStatus <>'Reject' AND MRM.AuthorizedByStatus <>'Approval' Then 'For Approving' END AS AuthorizedByStatus
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
                        Where MRM.AuthorizedBy Is not null AND MRM.AuthorizedByStatus <>'Hold' and MRM.AuthorizedByStatus <> 'Reject' and MRM.AuthorizedByStatus <> 'Approval' 
                        AND MRM.AuthorizedBy='" + EmployeeId + @"'
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
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo
                                Order By RequisitionDate ASC";
			}
			else if (Status == "Hold/Reject For Approving")
			{
				sql = @"Select
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
                                ,AuthorizedByStatus
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo
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
								--,MRM.AuthorizedByStatus
                                ,Case when  MRM.AuthorizedByStatus ='Hold' Or MRM.AuthorizedByStatus= 'Reject' AND MRM.AuthorizedByStatus <>'Approval' Then 'Hold/Reject For Approving' END AS AuthorizedByStatus
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
                        Where MRM.AuthorizedBy Is not null AND MRM.AuthorizedByStatus ='Hold' and MRM.AuthorizedByStatus ='Reject' 
                        AND MRM.AuthorizedBy='" + EmployeeId + @"'
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
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo
                                Order By RequisitionDate ASC";
			}
			else if (Status == "Approved")
			{
				sql = @"Select
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
                                ,AuthorizedByStatus
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo
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
								--,MRM.AuthorizedByStatus
                                ,Case when  MRM.AuthorizedByStatus ='Approval' THEN 'Approved' END AS AuthorizedByStatus
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
                        Where MRM.AuthorizedBy Is not null AND MRM.AuthorizedByStatus ='Approval'
                        AND MRM.AuthorizedBy='" + EmployeeId + @"'
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
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo
                                Order By RequisitionDate ASC";
			}
			var res = _sqlRepository.GetDataCollection(sql);
			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, res);
			return response;
		}

		[Route("api/Material/RequisitionApproveBy")]
		public HttpResponseMessage RequisitionApproveBy()
		{
            var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A
                          Inner JOin dbo.EmployeeInformation E On E.systemId= A.EmployeeId
                          where A.ActionStatus='RequisitionApproveBy'";

			var res = _sqlRepository.GetDataCollection(sql);
			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, res);
			return response;
		}
        [Route("api/Material/GetAllReqdataDetailsById")]
        public HttpResponseMessage GetAllReqdataDetailsById(string RequisitionId) 
        {
           
            var sql = @"SELECT IM.Id
                        --,IM.Id AS MaterialReqqusitionMasterId
                         ,IM.MaterialReqqusitionMasterId AS Id
                         ,IR.Id MaterialReqqusitionMasterId
                        , MGM.UserName AS MaterialGroupName
                        , IM.MaterialMasterId, MM.UserName AS MaterialName
                        , IM.ArticleId, ART.StandardName AS ArticleName
                        , IM.FirstCharacteristicsId, FC.UserName 
                        , IM.FirstCharacteristicsValueId , FCV.UserName AS SKU1
                        , IM.SecondCharacteristicsId, SC.UserName 
                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SKU2
                        , IM.ThirdCharacteristicsId, TC.UserName 
                        , IM.ThirdCharacteristicsValueId , TCV.UserName AS SKU3
                        , ROUND(IM.TransactionQty,2) TransactionQty
                        , IM.TransactionUoMId
                        , TUoM.UserName AS TransactionUoM
                        , ROUND(IM.EstimatedRate,2) EstimatedRate 
                        , CU.Code AS CurrencyName
                        , ROUND((IM.TransactionQty * IM.EstimatedRate),2) AS TotalAmount   
                        ,IM.MaterialDetail
                        ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate
                        ,Act.Id As Activity
                        ,Act.UserName As ActivityName
                        ,IM.BudgetType
                        ,IM.Reason
                        ,IM.Remarks
                        ,IM.FutureReqApp
                        --,BudgetMasterId
                        --,GLGeneralInfoId
                        ,IM.MaterialDetail
                        ,IM.PORcvQty PORaisedQty
						,Balance=(ROUND(IM.TransactionQty,2)-IM.PORcvQty)
						,RequisitionStatus=CASE WHEN IM.POQtyStatus=1 THEN 'Closed' ELSE 'Not Closed' END
                        --,isnull(pod.GRNRcvQty,0) GRNRcvQty
						--,pod.Id POdetailId
                          ,IR.OrderRefNo
                        FROM TRN.MaterialRequsitionDetails AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
                        LEFT JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
                        LEFT JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
                        LEFT JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
                        --Left join trn.PurchaseOrderDetail pod on pod.RequisitionDetailId=im.Id
                        --JOIN [HKP].Budget
                        --JOIN [HKP].Gl
                       WHERE IM.MaterialReqqusitionMasterId='"+ RequisitionId + "'";

            var res = _sqlRepository.GetDataCollection(sql);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, res);
            return response;
        }
        [Route("api/Material/GetDataForReqStatus")]
        public HttpResponseMessage GetDataForReqStatus(string RequisitionId)
        {
            

            var sql = @"Select   Id
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
                                 ,Sum(TransactionQty) TransactionQty
		                        ,Sum(EstimatedRate) EstimatedRate
		                        ,Sum(TotalAmount) TotalAmount
                                ,AddedBy,Reason,OrderRefNo
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
                        Where MRM.CheckedByStatus <> 'Checked' AND MRM.CheckedByStatus <>'Hold' and MRM.CheckedByStatus <> 'Reject' 
                        AND MRM.Id='" + RequisitionId + @"'
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
                                , AuthorizedByStatus,AddedBy,Reason,OrderRefNo
                                Order By RequisitionDate ASC";
            var res = _sqlRepository.GetDataCollection(sql);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, res);
            return response;
        }
        //private string GetPK()
        //{
        //    string sID = string.Empty;
        //    bplib.clsGenID objGenID = new bplib.clsGenID();
        //    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(MaterialRequsitionMaster), out sID);
        //    return sID;
        //}
        //public void PoApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        //{
        //    try
        //    {
        //        var AuthorizedById = "";

        //        PoValue = "0";
        //        var Id = GetPK();
        //        if (CheckedStataus == "Checked")
        //        {
        //            if (AuthorizedBy == null || AuthorizedBy == "")
        //            {
        //                throw new CustomException("Select Approved By");
        //            }
        //            AuthorizedById = AuthorizedBy;

        //        }
        //        else
        //        {
        //            AuthorizedById = null;

        //        }
        //        var Status = CheckedStataus;
        //        var UpdatedBy = "";
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        var ip = identity.IPAddress;
        //        var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
        //        var AddedBy = identity.Name;
        //        var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
        //        var CompanyGroupId = identity.CompanyGroupId;
        //        var CompanyId = identity.CompanyId;
        //        var PlantId = identity.PlantId;
        //        string _sql = "Update TRN.PurchaseOrder set IsApproved='0',CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "' where id='" + PoId + "'";
        //        _sqlRepository.ExecuteSqlCommand(_sql);
        //        string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
        //        "CompanyGroupId," +
        //        "CompanyId," +
        //        "PlantId," +
        //        "ApprovedBy," +
        //        "Date," +
        //        "POValue," +
        //        "Status," +
        //        "AddedBy," +
        //        "AddedDate," +
        //        "AddedFromIp," +
        //        "UpdatedBy," +
        //        "UpdatedDate," +
        //        "UpdatedFromIp,POID) " +
        //        "values ('" + Id + "'," +
        //        "'" + CompanyGroupId + "'," +
        //        "'" + CompanyId + "'," +
        //        "'" + PlantId + "'," +
        //        "'" + AddedBy + "'," +
        //        "'" + AddedDate + "'," +
        //        "'" + PoValue + "'," +
        //        "'" + Status + "'," +
        //        "'" + AddedBy + "'," +
        //        "'" + AddedDate + "'," +
        //        "'" + ip + "'," +
        //        "'" + UpdatedBy + "'," +
        //        "'" + updatedDate + "', " +
        //        "'" + ip + "','" + PoId + "')";
        //        _sqlRepository.ExecuteSqlCommand(_sql1);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //    }
        //}

        #endregion
    }
}