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

namespace Aplos.Areas.Productions.Controllers
{
    public class QualityActionConfirmationController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public QualityActionConfirmationController(ISqlRepository R)
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

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select distinct EI.SystemId,EI.EmployeeName, mb.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    ,EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection  from 
TRN.QualityControlDetails QCD
left join dbo.EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
where EI.EmployeeStatus='Active' and EI.EmployeeCode is not null and QCD.Status='Close' and QCD.ResponsiblePersonId is not null";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetActionBy()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, mb.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult LoadQualityActionUpdateHeader(string FromDate, string ToDate, string ResponsiblePersonId)
        {
            string FilterDate = string.Empty;
            string ResponsiblePerson = string.Empty;

            if (FromDate != null && ToDate != null && FromDate != "undefined" && ToDate != "undefined")
            {
                FilterDate = " and convert(Date,QCD.AddedDate) between '"+ FromDate + "' and '" + ToDate + "'";
            }

            if (ResponsiblePersonId != "null" && ResponsiblePersonId != "undefined")
            {
                ResponsiblePerson = " and ResponsiblePersonId = '" + ResponsiblePersonId + "'";
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct QC.Id as HeaderId,format(QC.AddedDate,'dd-MMM-yyyy') as Date,DATEDIFF(Hour,QC.AddedDate,GETDATE()) PendingTime,E.Id EntityId,E.UserName Entity,P.Id ProcessId,P.UserName Process,
QC.IssueId,QMM.UserName Issue,EI.SystemId CheckedById,EI.EmployeeName CheckedBy,QC.ProductionOrderId PONo,QC.LotNumber,
Article=STUFF((select distinct ','+MA.StandardName from trn.ProductionOrderDetail Pod 
left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId=so.Id
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where Pod.ProductionOrderId=QC.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PS.UserName POStatus from TRN.QualityControlDetails QCD
left join TRN.QualityControl QC on QC.Id=QCD.QCId
left join ORG.Entity E on E.Id=QC.EntityId
left join hkp.Process P on P.Id=QC.ProcessId
left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
left join EmployeeInformation EI on EI.SystemId=QC.ProductionInchargeId
left join TRN.ProductionOrder PO on PO.Id=QC.ProductionOrderId
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
where QCD.Status in ('Close') and PS.UserName in ('Running','To Close') and QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1) " + FilterDate + @" " + ResponsiblePerson + @" order by DATEDIFF(Hour,QC.AddedDate,GETDATE()) desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadQualityActionUpdateParameterListGetDetails(string HeaderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select QCD.Id ParameterId,PM.UserName Parameter,QCD.Status,UOM.UserName UOM,QCD.Value,QMP.Max,QMP.Min,WC.UserName WorkCenter,QGD.GradeName,
QAD.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,QCD.Remarks,QCD.ItemId,format(QCD.AddedDate,'dd-MMM-yyyy') as AddedDate,format(QCD.AddedDate,'hh:mm tt') as AddedTime  from TRN.QualityControlDetails QCD
left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
left join hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId
left join SCS.WorkCenterMaster WC on WC.Id=QCD.WorkCenterId
left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
left join MST.QualityActionToBeTakenDetails QAD on QAD.Id=QCD.ActionToBeTaken
left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
where QCD.Status in ('Close') and QCD.GradeId in (select Id from MST.QualityGradeDetails where ActionApplicable=1)
and QCD.QCId='" + HeaderId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadQualityActionTakenListGetDetails(string ParameterId, string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select QAT.Id,isnull(QAT.SNO,QPR.SNO) SNO,QPR.Id ReasonId,isnull(QRM.UserName,QAT.ReasonName) ReasonName,QAT.ActionTaken,QAT.ActionById,EI.EmployeeName ActionBy,QAT.Remarks
from [TRN].[QualityActionTakenUpdate]  QAT
left join [MST].[QualityManagementParameterReason] QPR on QPR.Id=QAT.ReasonId and QPR.IsActive=1
left join [HKP].[QualityManagementReasonMaster] QRM on QRM.Id=QPR.ReasonId
left join EmployeeInformation EI on EI.SystemId=QAT.ActionById
where QAT.ParameterId='" + ParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetReasonNameLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,ReasonName as Text from [MST].[QualityManagementParameterReason]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        
        [HttpPost]
        public ActionResult createActionTaken(List<Dictionary<string, object>> DataList, string PId, string Status, string ConfirmationRemarks)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[QualityActionTakenUpdate]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and ParameterId='" + PId + "'", out dsProdBooked, false, "1");

                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("QualityActionTakenUpdate", out _Id);
                            _Id = "QAT" + _Id;
                            item["Id"] = _Id;
                            item["ParameterId"] = PId;
                            if(item["ReasonId"] != null)
                            {
                                item["ReasonName"] = "NULL";
                            }
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            _Id = item["Id"].ToString();
                            item["ParameterId"] = PId;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("Update TRN.QualityControlDetails set Status='" + Status + "',ConfirmBy='"+ identity.UserId + "',ConfirmationRemarks='"+ ConfirmationRemarks +"' where Id='" + PId + @"'");
                    conC.CommitTransaction();
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
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

        [Authorize, HttpPost]
        public ActionResult SaveDefault(IEnumerable<System.Web.HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the order first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetMSADocumentPath(), fileName);

                    var directory = ResourcesPathReader.GetMSADocumentPath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetMSADocumentPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetMSADocumentPath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM [TRN].[MachineAssetPlannedDetails] WHERE Id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id)
        {
            try
            {
                return Json(_sqlRepository.GetDataCollection("select * from [TRN].[MachineAssetPlannedDetails]  where Id='" + Id + "'"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion -- Operations
    }
}