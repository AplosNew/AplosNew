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
    public class DefineProcessParameterController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public DefineProcessParameterController(ISqlRepository R)
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

        [Authorize, HttpGet]
        public JsonResult GetGroupList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select [Group] as Value, [Group] as Text from [MST].[ProcessParameterMaster]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSubGroupList(string Group)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select SubGroup as Value, SubGroup as Text from [MST].[ProcessParameterMaster] where [Group]='" + Group + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetUserNameList(string Subgroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value, UserName as Text from [MST].[ProcessParameterMaster] where SubGroup='" + Subgroup + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult LoadDefineProcessParameter()
        {
            string
                sql = @"SELECT DPP.*,PPM.UserName,E.EmployeeName ByWhom,PPM.[Group],PPM.SubGroup  FROM [MST].[DefineProcessParameter] DPP
left join [MST].[ProcessParameterMaster] PPM on PPM.Id=DPP.MasterId
left join EmployeeInformation E on E.SystemId=DPP.ByWhomId";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadDefineProcessParameterEditData(string DefineProcessParameterID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT DPP.*,PPM.UserName,E.EmployeeName ByWhom,PPM.[Group],PPM.SubGroup  FROM [MST].[DefineProcessParameter] DPP
left join [MST].[ProcessParameterMaster] PPM on PPM.Id=DPP.MasterId
left join EmployeeInformation E on E.SystemId=DPP.ByWhomId  where DPP.Id='" + DefineProcessParameterID + @"'";
            return Json(new { defineprocessparameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> DefineProcessParameterData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[DefineProcessParameter] where UserCode='" + DefineProcessParameterData["UserCode"] + "'", out DataSet dsDefineProcessParameterUserCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[DefineProcessParameter] where DefineName='" + DefineProcessParameterData["DefineName"] + "'", out DataSet dsDefineProcessParameterDefineNameValidation, false, "1");
               


                DataSet dsDefineProcessParameter;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[DefineProcessParameter] where Id='" + DefineProcessParameterData["Id"] + "'", out dsDefineProcessParameter, false, "1");
                string _Id = "", Id = string.Empty; ;

                #region data update
                if (dsDefineProcessParameter.Tables[0].Rows.Count == 0)
                {

                    if (dsDefineProcessParameterUserCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Code Already Exist.");
                    }
                    else if (dsDefineProcessParameterDefineNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Define Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("DefineProcessParameter", out _Id);
                        _Id = "DPP" + _Id;
                        DefineProcessParameterData["Id"] = _Id;
                        AddNewRow(dsDefineProcessParameter.Tables[0], DefineProcessParameterData);
                    }
                }
                else
                {
                    _Id = DefineProcessParameterData["Id"].ToString();
                    EditRow(dsDefineProcessParameter.Tables[0].Rows[0], DefineProcessParameterData);
                }
                #endregion data update


                Id = dsDefineProcessParameter.Tables[0].Rows[0]["Id"].ToString();
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDefineProcessParameter);

                return Json(new { Id = Id, Error = false, Data = DefineProcessParameterData, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult DefineProcessParameterDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                //ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                //DataSet EntityCount, AGCount;

                //conRack = new ConnectionManager.DAL.ConManager("1");
                //conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterEntity] where PPID='" + id + "'", out EntityCount, false, "1");
                //conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterActivityGroup] where PPID ='" + id + "'", out AGCount, false, "1");
               

                //if (EntityCount.Tables[0].Rows.Count == 0 && AGCount.Tables[0].Rows.Count == 0 && ProcessCount.Tables[0].Rows.Count == 0 && PositionCodeCount.Tables[0].Rows.Count == 0 && ApprovalPersonCount.Tables[0].Rows.Count == 0)
                //{

                    conC.BeginTransaction();
                    conC.executeQuery("delete from [MST].[DefineProcessParameter] where Id ='" + id + @"'");
                    conC.CommitTransaction();
                //}
                //else
                //{
                //    throw new Exception("Transaction are Exists!");
                //}
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadDefineProcessParameterArticleDetails(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN DPPA.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,DPPA.Id,PM.UserName Product,MM.UserName Material, MMA.StandardName Article,MMA.Id ArticleId,DPPA.Remarks from trn.ProductDefinition PD
left join [MST].[ProductMaster] PM on  PM.Id=PD.ProductMasterId
left join [MST].[MaterialMaster] MM on MM.Id=PD.MaterialMasterId
left join [MST].[MaterialMasterArticle] MMA on MMA.MaterialMasterId=MM.Id
left join [MST].[DefineProcessParameterArticle] DPPA on DPPA.ArticleId=MMA.Id and DPPA.DPPID='" + MasterId + @"'
where PD.Active = 1 order by DPPA.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createDefineProcessParameterArticle(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[DefineProcessParameterArticle]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where DPPID='" + Pid + "'");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and DPPID='" + item["DPPID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "DPA" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadDefineProcessParaProcessDetails(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN DPPP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,DPPP.Id,PPP.PPID MasterId,PPM.UserName,PPP.ProcessId,DPP.Id DPPID,
DPPP.Remarks,P.UserName Process from [MST].[ProcessParameterProcess] PPP
left join hkp.Process P on P.Id=PPP.ProcessId
left join MST.ProcessParameterMaster PPM on PPM.Id=PPP.PPID
left join [MST].[DefineProcessParameter] DPP on DPP.MasterId=PPP.PPID
left join [MST].[DefineProcessParaProcess] DPPP on DPPP.DPPID='"+ MasterId + @"' and DPPP.ProcessId=PPP.ProcessId and DPPP.MasterId=PPM.Id
order by DPPP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createDefineProcessParameterProcess(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[DefineProcessParaProcess]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                
                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and DPPID='" + item["DPPID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "DP" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        
        [Authorize, HttpGet]
        public ActionResult getDefineProcessParameterData(string ProcessParameterId, string ProcessId, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN DPPI.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,DPPI.Id,PPI.SNO,'" + ProcessParameterId + @"' ProcessParameterId,PPI.ParameterId,
PPI.CriticalLevel,PPI.IsAuditable,PPI.ByWhomId,PPI.ProcessId,PPI.ActivityGroup,PPI.Remarks,PPI.Category,PPI.ExceptionDays,PPI.ReportApplicable,
PPI.IsStdApplicable,PPI.OrderSpecific,PPI.General,PPI.UOMId,isnull(DPPI.Max,PPI.Max) Max,isnull(DPPI.Min,PPI.Min) Min,PPI.IsWorkCenter,PPI.IsActive,PPI.ActivityGroup as AGroup,
(select Code from [MST].[ManpowerBudget] where Id=PPI.ByWhomId) as ByWhom,
(select P.UserName from hkp.process P where P.Id=PPI.ProcessId) as Process,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=PPI.UOMId) as UOM,
(select PM.UserName from HKP.ProcessParaMaster PM where PM.Id=PPI.ParameterId) as ParameterName,PPI.Id MasterItemId
from [MST].[ProcessParameterItem] PPI
left join [MST].[DefineProcessParameterItem] DPPI on DPPI.ProcessParameterId='" + ProcessParameterId + @"' and DPPI.MasterItemId=PPI.Id
where PPI.ProcessParameterId=(select Id from MST.ProcessParameterProcess where ProcessId='" + ProcessId + "' and PPID='" + MasterId + "') order by DPPI.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadDefineProcessParameterItemDetails(string ProcessParameterId, string ProcessId, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN DPPI.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,DPPI.Id,PPI.SNO,'" + ProcessParameterId + @"' ProcessParameterId,PPI.ParameterId,
PPI.CriticalLevel,PPI.IsAuditable,PPI.ByWhomId,PPI.ProcessId,PPI.ActivityGroup,PPI.Remarks,PPI.Category,PPI.ExceptionDays,PPI.ReportApplicable,
PPI.IsStdApplicable,PPI.OrderSpecific,PPI.General,PPI.UOMId,isnull(DPPI.Max,PPI.Max) Max,isnull(DPPI.Min,PPI.Min) Min,PPI.IsWorkCenter,PPI.IsActive,PPI.ActivityGroup as AGroup,
(select Code from [MST].[ManpowerBudget] where Id=PPI.ByWhomId) as ByWhom,
(select P.UserName from hkp.process P where P.Id=PPI.ProcessId) as Process,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=PPI.UOMId) as UOM,
(select PM.UserName from HKP.ProcessParaMaster PM where PM.Id=PPI.ParameterId) as ParameterName
from [MST].[ProcessParameterItem] PPI
left join [MST].[DefineProcessParameterItem] DPPI on DPPI.ProcessParameterId='" + ProcessParameterId + @"' and DPPI.MasterItemId=PPI.Id
where PPI.ProcessParameterId=(select Id from MST.ProcessParameterProcess where ProcessId='" + ProcessId + "' and PPID='" + MasterId + "') order by DPPI.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createDefineProcessParameterItem(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[DefineProcessParameterItem]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and ProcessParameterId='" + item["ProcessParameterId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "DPI" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getDefineProcessParameterReasonData(string ParameterId, string MasterParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN DPR.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,DPR.Id,PPR.SNO,isnull(DPR.Remarks,PPR.Remarks) Remarks,PPR.ReasonId,PRM.UserName ReasonName,'" + ParameterId + @"' ParameterId,PPR.IsActive from [MST].[ProcessParameterReason] PPR
left join [HKP].[ProcessParameterReasonMaster] PRM on PRM.Id=PPR.ReasonId
left join [MST].[DefineProcessParameterReason] DPR on DPR.ReasonId=PPR.ReasonId and DPR.ParameterId='" + ParameterId + @"'
where PPR.ParameterId='"+ MasterParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadDefineProcessParameterReasonDetails(string ParameterId, string MasterParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN DPR.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,DPR.Id,PPR.SNO,isnull(DPR.Remarks,PPR.Remarks) Remarks,PPR.ReasonId,PRM.UserName ReasonName,'" + ParameterId + @"' ParameterId,PPR.IsActive from [MST].[ProcessParameterReason] PPR
left join [HKP].[ProcessParameterReasonMaster] PRM on PRM.Id=PPR.ReasonId
left join [MST].[DefineProcessParameterReason] DPR on DPR.ReasonId=PPR.ReasonId and DPR.ParameterId='" + ParameterId + @"'
where PPR.ParameterId='" + MasterParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createDefineProcessParameterReason(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[DefineProcessParameterReason]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and ParameterId='" + item["ParameterId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "DPR" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getDefineParameterCheckPointsData(string ParameterId, string MasterParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN DPC.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,DPC.Id,PCP.SNO,PCP.CheckPoints,isnull(DPC.Remarks,PCP.Remarks) Remarks,'" + ParameterId + @"' ParameterId
from [MST].[ProcessParameterCheckPoints] PCP
left join [MST].[DefineProcessParameterCheckPoints] DPC on DPC.ParameterId='" + ParameterId + @"'
where PCP.ParameterId='" + MasterParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadDefineProcessParameterCheckPointsDetails(string ParameterId, string MasterParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN DPC.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,DPC.Id,PCP.SNO,PCP.CheckPoints,isnull(DPC.Remarks,PCP.Remarks) Remarks,'" + ParameterId + @"' ParameterId
from [MST].[ProcessParameterCheckPoints] PCP
left join [MST].[DefineProcessParameterCheckPoints] DPC on DPC.ParameterId='" + ParameterId + @"'
where PCP.ParameterId='" + MasterParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createDefineProcessParameterCheckPoints(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[DefineProcessParameterCheckPoints]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and ParameterId='" + item["ParameterId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "DP" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadDefineParameterWorkCenterDetails(string ParameterId, string MasterParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN DPW.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,DPW.Id,WM.Id as WorkCenterMasterId, WM.Code ,WM.UserName Workcenter, WC.UserName WorkcenterCategory, WCS.UserName WorkcenterSubCategory, P.UserName Process, E.UserName Entity, WM.Capacity, UOM.UserName UOM 
                            from MST.ProcessParameterWorkCenter PWM
							LEFT JOIN SCS.WorkCenterMaster WM on WM.Id=PWM.WorkCenterMasterId
							LEFT JOIN [MST].[DefineProcessParameterWorkCenter] DPW ON PWM.WorkCenterMasterId=DPW.WorkCenterMasterId and DPW.ParameterId='" + ParameterId + @"'
							LEFT JOIN HKP.WorkCenterCategory WC on WC.Id = WM.WorkCenterCategoryId
                            LEFT JOIN HKP.WorkCenterSubCategory WCS on WCS.Id = WM.WorkCenterSubcategoryId
                            left join HKP.Process P on P.Id = WM.ProcessId
                            left join org.Entity E on E.Id = WM.EntityId
                            LEFT JOIN SCS.UnitOfMeasurement UOM on UOM.Id = WM.UoMId 
                            where WM.Active = 1 and 
                            PWM.ParameterId='" + MasterParameterId + "' order by DPW.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createDefineWorkCenter(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[DefineProcessParameterWorkCenter]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {


                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where ParameterId='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {


                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and ParameterId='" + item["ParameterId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "DPW" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
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

        #endregion -- Operations
    }
}