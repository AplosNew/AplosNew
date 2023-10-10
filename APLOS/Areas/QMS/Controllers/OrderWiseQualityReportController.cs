using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.QMS.Controllers
{
    public class OrderWiseQualityReportController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public OrderWiseQualityReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult LoadOrderWiseQualityReport(string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string
                sql = @"select (case when Z.RejectValue > 0  then 'Reject'
when Z.FailValue > 0  then 'Fail'
when Z.EntryMissing > 0  then 'Pending'
else 'Pass' end) QualityStatus,
Date=(select top 1 Format(AddedDate,'dd-MMM-yyyy') from TRN.QualityControl where IssueId=Z.IssueId),
POStatus=(select UserName from hkp.ProductionStatus where Id=(select ProductionStatusId from TRN.ProductionOrder where Id=Z.PONo)),
MOLineItemNo= STUFF((select distinct ','+ XMOI.Id from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
where Z.PONo=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
Article=STUFF((select distinct ','+MA.StandardName from
											MST.MaterialMasterArticle MA
											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=xp.Id
											where Z.PONo=Xpod.ProductionOrderId for xml path('') ), 1, 1, '')
,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where Z.PONo=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
Z.PONo,Z.LotNumber,
Z.Entity,Z.IssueId,
Sum(Convert(Int,Z.PassValue)) Pass,Sum(Convert(Int,Z.FailValue)) Fail,Sum(Convert(Int,Z.RejectValue)) Reject,Sum(Z.EntryMissing) MissingEntry,
Reverse(stuff(Reverse((select OWC.Grade +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.MOLineItemNo=Z.MOLineItemNo and OWC.PONo=Z.PONo and OWC.LotNo=Z.LotNumber for xml PATH(''))),1,2,'')) Grade,
Reverse(stuff(Reverse((select format(OWC.AddedDate,'dd-MMM-yyyy') + '-' + OWC.Comment +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.MOLineItemNo=Z.MOLineItemNo and OWC.PONo=Z.PONo and OWC.LotNo=Z.LotNumber for xml PATH(''))),1,2,'')) CommentDetails
from (select QCData.QCDate,M.MOLineItemNo,M.POStatus,M.ProductionOrderId PONo,M.LotNumber,M.Article,M.Customer, 
M.IssueId,M.IssueName,M.ParameterSequence,M.ParameterId,M.ParameterName,M.UOM,QCData.Value,QCData.GradeName,QCData.ParameterRemark,QCData.ActionToBeTakenName,QCData.ResponsiblePerson,QCData.PassValue,QCData.FailValue,QCData.RejectValue,
QCData.HeaderId,QCData.ChildId,QCData.QCDDate,(Case When (QCData.Value is null or QCData.Value = '0') then 1 else 0 end) EntryMissing,M.Process,M.Entity from (Select  P.*,CP.IssueName,CP.IssueId,CP.ParameterId,CP.ParameterName,CP.UOM,CP.ParameterSequence,CP.Process from (select Distinct PS.ProductionOrderId,PS.LotNumber,E.UserName Entity,
MOLineItemNo= STUFF((select distinct ','+ XMOI.Id from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
where PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),  
Article=STUFF((select distinct ','+MA.StandardName from
											MST.MaterialMasterArticle MA
											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=xp.Id
											where PS.ProductionOrderId=Xpod.ProductionOrderId for xml path('') ), 1, 1, '')
,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
1 PlanSet,PST.UserName POStatus,PS.ProcessId
from TRN.ProductionSummary PS
left join trn.ProductionOrder PO on PO.Id=PS.ProductionOrderId
left join hkp.ProductionStatus PST on PST.Id=PO.ProductionStatusId
left join org.Entity E on E.Id=PS.EntityId
where PS.AddedDate between '" + FromDate + "' and '" + ToDate + @"') P
inner Join (select QMM.UserName IssueName,QMP.QMID IssueId,QMP.Id ParameterId,PM.UserName ParameterName,QMP.SNO ParameterSequence,UOM.UserName UOM,1 as PlanSet,PR.UserName Process,QMP.ProcessId
 from MST.QualityManagementParameterItem QMP
 left join MST.QualityManagementMaster QMM on QMM.Id=QMP.QMID
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId
 left join hkp.Process PR on  PR.Id=QMP.ProcessId
 where CustomerParameter = 1) CP on CP.PlanSet=P.PlanSet and CP.ProcessId=P.ProcessId) M
 left join (select QC.IssueId,QMM.UserName IssueName,QCD.ItemId ParameterId,PM.UserName ParameterName,QC.LotNumber,QC.ProductionOrderId,
 QCD.Value,QGD.GradeName,QCD.Remarks ParameterRemark,QAT.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,
 format(QC.AddedDate,'dd-MMM-yyyy') QCDate,format(QCD.AddedDate,'dd-MMM-yyyy') QCDDate,QC.Id HeaderId,QCD.Id ChildId,QGD.IsPassValue PassValue,QGD.IsFailValue FailValue,QGD.IsRejectValue RejectValue
 from TRN.QualityControlDetails QCD
 left join TRN.QualityControl QC on QC.Id=QCD.QCId
 left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
 left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
 left join MST.QualityActionToBeTakenDetails QAT on QAT.Id=QCD.ActionToBeTaken
 left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
 where QCD.ItemId in (select Id from MST.QualityManagementParameterItem where CustomerParameter = 1)) QCData on QCData.IssueId=M.IssueId and QCData.ParameterId=M.ParameterId
 and QCData.ProductionOrderId=M.ProductionOrderId and QCData.LotNumber=M.LotNumber) Z Group By Z.PONo,Z.LotNumber,Z.IssueId,Z.IssueName,Z.EntryMissing,Z.Entity,Z.MOLineItemNo,Z.RejectValue,Z.FailValue,Z.PassValue order by  (case 
when Z.RejectValue > 0  then 'A'
when Z.FailValue > 0  then 'B'
when Z.EntryMissing > 0  then 'C'
else 'D' end) ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getOrderWiseParameterData(string FromDate, string ToDate,string IssueId, string ProductionOrderId, string LotNumber)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string
                sql = @"select QCData.QCDate,M.MOLineItemNo,M.POStatus,M.ProductionOrderId PONo,M.LotNumber,M.Article,M.Customer, 
M.IssueId,M.IssueName,M.ParameterSequence,M.ParameterId,M.ParameterName,M.UOM,QCData.Value,QCData.GradeName,QCData.ParameterRemark,QCData.ActionToBeTakenName,QCData.ResponsiblePerson,QCData.PassValue,QCData.FailValue,QCData.RejectValue,
QCData.HeaderId,QCData.ChildId,QCData.QCDDate,(Case When (QCData.Value is null or QCData.Value = '0') then 1 else 0 end) EntryMissing,M.Process,M.Entity,Reverse(stuff(Reverse((select OWC.Grade +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.MOLineItemNo=M.MOLineItemNo and OWC.PONo=M.ProductionOrderId and OWC.LotNo=M.LotNumber for xml PATH(''))),1,2,'')) Grade,
Reverse(stuff(Reverse((select format(OWC.AddedDate,'dd-MMM-yyyy') + '-' + OWC.Comment +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.MOLineItemNo=M.MOLineItemNo and OWC.PONo=M.ProductionOrderId and OWC.LotNo=M.LotNumber for xml PATH(''))),1,2,'')) CommentDetails,
Reverse(stuff(Reverse((select isnull(RD.MinRequirement,'') + '/' + isnull(RD.MaxRequirement,'') +', ' from TRN.UCPRequirementDetails RD																			
where RD.ParameterId=QCData.ChildId for xml PATH(''))),1,2,'')) MinMaxRequirement,
Reverse(stuff(Reverse((select isnull(SD.MinStandard,'') + '/' + isnull(SD.MaxStandard,'') +', ' from TRN.UCPMaxMinStandardDetails SD																			
where SD.ParameterId=QCData.ChildId for xml PATH(''))),1,2,'')) MinMaxStandard,
QCData.ActionTaken,QCData.ActionBy,QCData.ConfirmRemarks,QCData.QAURemarks,QCData.ReasonName
from (Select  P.*,CP.IssueName,CP.IssueId,CP.ParameterId,CP.ParameterName,CP.UOM,CP.ParameterSequence,CP.Process from (select Distinct PS.ProductionOrderId,PS.LotNumber,E.UserName Entity,
MOLineItemNo= STUFF((select distinct ','+ XMOI.Id from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
where PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),  
Article=STUFF((select distinct ','+MA.StandardName from
											MST.MaterialMasterArticle MA
											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=xp.Id
											where PS.ProductionOrderId=Xpod.ProductionOrderId for xml path('') ), 1, 1, '')
,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
1 PlanSet,PST.UserName POStatus
from TRN.ProductionSummary PS
left join trn.ProductionOrder PO on PO.Id=PS.ProductionOrderId
left join hkp.ProductionStatus PST on PST.Id=PO.ProductionStatusId
left join org.Entity E on E.Id=PS.EntityId
where PS.AddedDate between '" + FromDate + "' and '" + ToDate + @"') P
Left Join (select QMM.UserName IssueName,QMP.QMID IssueId,QMP.Id ParameterId,PM.UserName ParameterName,QMP.SNO ParameterSequence,UOM.UserName UOM,1 as PlanSet,PR.UserName Process
 from MST.QualityManagementParameterItem QMP
 left join MST.QualityManagementMaster QMM on QMM.Id=QMP.QMID
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId
 left join hkp.Process PR on  PR.Id=QMP.ProcessId
 where CustomerParameter = 1) CP on CP.PlanSet=P.PlanSet) M
 left join (select QC.IssueId,QMM.UserName IssueName,QCD.ItemId ParameterId,PM.UserName ParameterName,QC.LotNumber,QC.ProductionOrderId,
 QCD.Value,QGD.GradeName,QCD.Remarks ParameterRemark,QAT.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,
 format(QC.AddedDate,'dd-MMM-yyyy') QCDate,format(QCD.AddedDate,'dd-MMM-yyyy') QCDDate,QC.Id HeaderId,QCD.Id ChildId,QGD.IsPassValue PassValue,QGD.IsFailValue FailValue,QGD.IsRejectValue RejectValue,
 QAU.ActionTaken,QAE.EmployeeName ActionBy,QAU.Remarks QAURemarks,isnull(QAU.ReasonName,(select UserName from [HKP].[QualityManagementReasonMaster] where Id=(select ReasonId from [MST].[QualityManagementParameterReason] where Id=QAU.ReasonId))) ReasonName,QAU.ConfirmRemarks
 from TRN.QualityControlDetails QCD
 left join TRN.QualityControl QC on QC.Id=QCD.QCId
 left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
 left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
 left join MST.QualityActionToBeTakenDetails QAT on QAT.Id=QCD.ActionToBeTaken
 left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
 left join TRN.QualityActionTakenUpdate QAU on QAU.ParameterId=QCD.Id
 left join EmployeeInformation QAE on QAE.SystemId=QAU.ActionById
 where QCD.ItemId in (select Id from MST.QualityManagementParameterItem where CustomerParameter = 1)) QCData on QCData.IssueId=M.IssueId and QCData.ParameterId=M.ParameterId
 and QCData.ProductionOrderId=M.ProductionOrderId and QCData.LotNumber=M.LotNumber
where M.IssueId='" + IssueId+"' and M.ProductionOrderId='"+ProductionOrderId+"' and M.LotNumber='"+LotNumber+"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getCommentEntryData(string MOLineItemNo, string PONo, string LotNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,isnull(MOLineItemNo,'" + MOLineItemNo + "') MOLineItemNo,isnull(PONo,'" + PONo + "') PONo,isnull(LotNo,'" + LotNo + "') LotNo,Comment,ByWhomId," +
                "(select EmployeeName from EmployeeInformation Where SystemId=(select AuthorizedResPersonId from [HKP].[QualityManagementAuthorizedPerson] where Id=ByWhomId)) ByWhom,Grade " +
                "from [MST].[OrderWiseQualityComment] where MOLineItemNo='"+ MOLineItemNo + "' and PONo='"+ PONo + "' and LotNo='"+ LotNo + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetByWhomLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,(select EmployeeName from  employeeinformation where SystemId=AuthorizedResPersonId) as Text from [HKP].[QualityManagementAuthorizedPerson]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createComments(Dictionary<string, object> CommentsData, string MOItem, string POId, string LotNumber)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[OrderWiseQualityComment] where Comment='" + CommentsData["Comment"] + "'", out DataSet dsOrderWiseQualityCommentValidation, false, "1");

                DataSet dsOrderWiseQualityComment;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[OrderWiseQualityComment] where Id='" + CommentsData["Id"] + "'", out dsOrderWiseQualityComment, false, "1");
                string _Id = "";

                #region data update
                if (dsOrderWiseQualityComment.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("OrderWiseQualityComment", out _Id);
                    _Id = "OWC" + _Id;
                    CommentsData["Id"] = _Id;
                    CommentsData["MOLineItemNo"] = MOItem;
                    CommentsData["PONo"] = POId;
                    CommentsData["LotNo"] = LotNumber;
                    AddNewRow(dsOrderWiseQualityComment.Tables[0], CommentsData);
                }
                else
                {
                    _Id = CommentsData["Id"].ToString();
                    EditRow(dsOrderWiseQualityComment.Tables[0].Rows[0], CommentsData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsOrderWiseQualityComment);

                return Json(new { Error = false, Data = CommentsData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public ActionResult getCommentData(string MOId, string POId, string LotNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select OWC.*,(select EmployeeName from EmployeeInformation Where SystemId=(select AuthorizedResPersonId from [HKP].[QualityManagementAuthorizedPerson] where Id=ByWhomId)) ByWhom from [MST].[OrderWiseQualityComment] OWC
where OWC.MOLineItemNo ='" + MOId + "' and OWC.PONo='" + POId + "' and OWC.LotNo='" + LotNo + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadCommentEntryEditData(string CommentId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select OWC.*,(select EmployeeName from EmployeeInformation Where SystemId=(select AuthorizedResPersonId from [HKP].[QualityManagementAuthorizedPerson] where Id=ByWhomId)) ByWhom from [MST].[OrderWiseQualityComment] OWC where Id='" + CommentId + @"'";
            return Json(new { comment = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CommentsDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[OrderWiseQualityComment] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [Authorize, HttpGet]
        public JsonResult GetParameterApprovalPersonLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,(select EmployeeName from  employeeinformation where SystemId=ResponsiblePersonId) as Text from [MST].[ProcessParameterApprovalResponsiblePerson]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createCustomerUpdatePara(Dictionary<string, object> CustomerUpdateParaData,string ApprovalStatus)
        {
            try
            {
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[CustomerUpdateParameter] where LineItemNo='" + CustomerUpdateParaData["LineItemNo"] + "'", out DataSet dsCustomerUpdateParaItemLineNoValidation, false, "1");

                DataSet dsCustomerUpdatePara;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[CustomerUpdateParameter] where Id='" + CustomerUpdateParaData["Id"] + "'", out dsCustomerUpdatePara, false, "1");
                string _Id = "";

                #region data update
                if (CustomerUpdateParaData["LineItemNo"] == null)
                {
                    throw new Exception("LineItemNo is required");
                }
                else
                {
                    if (CustomerUpdateParaData["EmployeeId"] == null)
                    {
                        throw new Exception("Employee is required");
                    }
                    else
                    {
                        if (dsCustomerUpdatePara.Tables[0].Rows.Count == 0)
                        {
                            if (dsCustomerUpdateParaItemLineNoValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("LineItemNo Name Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[CustomerUpdateParameter]", out _Id);
                                CustomerUpdateParaData["Id"] = _Id;
                                CustomerUpdateParaData["ApprovalStatus"] = ApprovalStatus;
                                AddNewRow(dsCustomerUpdatePara.Tables[0], CustomerUpdateParaData);
                                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                                conC.BeginTransaction();
                                conC.executeQuery("Update TRN.MasterOrderItem set CustomerParameterId='" + CustomerUpdateParaData["Id"] + "' where Id='" + CustomerUpdateParaData["LineItemNo"] + @"'");
                                conC.CommitTransaction();
                            }
                        }
                        else
                        {
                            _Id = CustomerUpdateParaData["Id"].ToString();
                            CustomerUpdateParaData["ApprovalStatus"] = ApprovalStatus;
                            EditRow(dsCustomerUpdatePara.Tables[0].Rows[0], CustomerUpdateParaData);
                        }
                    }
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsCustomerUpdatePara);

                return Json(new { Error = false, Data = CustomerUpdateParaData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }


        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion -- Operations
    }
}