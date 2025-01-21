using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.HumanResource.Parameter;

namespace Aplos.Areas.QMS.Controllers
{
    public class SQCMasterController : Controller
    {
        #region Constructor


        ParameterService ps = new ParameterService();
        private readonly ISqlRepository _sqlRepository;

        public SQCMasterController(ISqlRepository R)
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
        public ActionResult GetPositionCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select P.Id,P.Code,P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection
from ORG.Position P	
LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
left outer join MST.DesignationMaster DM ON DM.DesignationId=P.DesignationId
where P.Active = 1";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcessList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.Process";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetProcess()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id , Code, UserName as Process from HKP.Process";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSubProcessList(string Pid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select SP.Id as Value,P.UserName as Text from HKP.SubProcess SP
left join HKP.Process P ON  P.Id=SP.ProcessId
where P.Id='" + Pid + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetResponsiblePerson()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select EMP.SystemId EmpSystemId, EMP.EmployeeCode, EMP.EmployeeName, FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, EC.UserName EmployeeCategory, DP.UserName Department
                               ,SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation, LDSG.UserName LegalDesignation, UN.UserName as Entity
                                from EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                                where EMP.EmployeeStatus = 'Active'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            try
            {
                return Json(ps.GetSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize, HttpGet]
        public JsonResult GetSQReasonNameLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from [HKP].[SQReasonMaster]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createSQParaMaster(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = ps.SQSave(data), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetSQParameterMasterSequence()
        {
            try
            {
                return Json(ps.GetSQParaMasterSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                string ret = ps.SQParameterDelete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetSQParameterMasterSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        [Authorize, HttpPost]
        public JsonResult GetParameterItemList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select PM.Id, PM.Code,PM.StandardName,PM.UserName  from [HKP].[SQParameterMaster] PM where PM.IsActive = 1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetParameterProcessList(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select ProcessId as Value,P.UserName as Text from MST.SQProcess SQP 
left join hkp.Process P on P.id=SQP.ProcessId
where SQCID='" + ScheduleId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetParameterProcessAGList(string ScheduleId, string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select QMA.ActivityGroupName from MST.SQProcess SQP
left join MST.SQActivityGroup QMA on QMA.id=SQP.ActivityGroupId
where SQP.SQCID='" + ScheduleId + "' and SQP.ProcessId='" + ProcessId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public decimal GetItemAutoSequence(string scheduleId)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM [MST].[QualityManagementParameterItem] where QMID='" + scheduleId + "'");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["SNO"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetUOM()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select UM.Id UOMId, UM.Code,UM.StandardName, UM.UserName UOM from scs.UnitOfMeasurement UM where UM.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> ScheduleData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQCMaster] where ScheduleCode='" + ScheduleData["ScheduleCode"] + "'", out DataSet dsQualityManagmentMasterCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQCMaster] where StandaredName='" + ScheduleData["StandaredName"] + "'", out DataSet dsQualityManagmentMasterSNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQCMaster] where UserName='" + ScheduleData["UserName"] + "'", out DataSet dsQualityManagmentMasterUNameValidation, false, "1");
                

                DataSet dsQualityManagmentMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQCMaster] where Id='" + ScheduleData["Id"] + "'", out dsQualityManagmentMaster, false, "1");
                string _Id = "", Id = string.Empty; ;

                #region data update
                if (dsQualityManagmentMaster.Tables[0].Rows.Count == 0)
                {
                    
                    if (dsQualityManagmentMasterCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Schedule Code Already Exist.");
                    }
                    else if (dsQualityManagmentMasterSNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsQualityManagmentMasterUNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("SQCMaster", out _Id);
                        _Id = "SQ" + _Id;
                        ScheduleData["Id"] = _Id;
                        AddNewRow(dsQualityManagmentMaster.Tables[0], ScheduleData);
                    }
                }
                else
                {
                    _Id = ScheduleData["Id"].ToString();
                    EditRow(dsQualityManagmentMaster.Tables[0].Rows[0], ScheduleData);
                }
                #endregion data update


                Id = dsQualityManagmentMaster.Tables[0].Rows[0]["Id"].ToString();
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagmentMaster);

                return Json(new { Id = Id, Error = false, Data = ScheduleData, Message = AplosMessage.Insert });
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

        [Authorize, HttpGet]
        public ActionResult LoadSQCMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=QM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode
                            FROM [MST].[SQCMaster] QM";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSQCMasterEditData(string ScheduleID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=QM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode
                            FROM [MST].[SQCMaster] QM where QM.Id='" + ScheduleID + @"'";
            return Json(new { schedule = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult ScheduleDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                DataSet EntityCount, AGCount, ItemCount, ProcessCount, MachineCount, ProductCount, WorkCenterCount;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQCEntity] where SQCID='" + id + "'", out EntityCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQActivityGroup] where SQCID ='" + id + "'", out AGCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQParameterItem] where SQCID ='" + id + "'", out ItemCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQProcess] where SQCID ='" + id + "'", out ProcessCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQMachine] where SQCID ='" + id + "'", out MachineCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQProduct] where SQCID ='" + id + "'", out ProductCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQWorkCenter] where SQCID ='" + id + "'", out WorkCenterCount, false, "1");

                if (EntityCount.Tables[0].Rows.Count == 0 && AGCount.Tables[0].Rows.Count == 0 && ItemCount.Tables[0].Rows.Count == 0 && ProcessCount.Tables[0].Rows.Count == 0 && MachineCount.Tables[0].Rows.Count == 0 && ProductCount.Tables[0].Rows.Count == 0 && WorkCenterCount.Tables[0].Rows.Count == 0)
                {

                    conC.BeginTransaction();
                    conC.executeQuery("delete from [MST].[SQCMaster] where Id ='" + id + @"'");
                    conC.CommitTransaction();
                }
                else
                {
                    throw new Exception("Transaction are Exists!");
                }
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetActivityGroupList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,ActivityGroupName as Text from [MST].[SQActivityGroup]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadEntityDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN SQE.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,SQE.Id,E.Id EntityId,E.EntityType,E.UserName Entity,E.Code,SQE.Remarks 
                            from ORG.Entity E
							LEFT JOIN [MST].[SQCEntity] SQE ON SQE.EntityId=E.Id and SQE.SQCID='" + ScheduleId + @"'
                            where E.Active = 1 order by SQE.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN SQP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,SQP.Id,P.Id ProcessId,P.UserName Process,P.Code,SAG.Id as ActivityGroupId,SAG.ActivityGroupName,SQP.Remarks
                            from hkp.Process P
							LEFT JOIN [MST].[SQProcess] SQP ON SQP.ProcessId=P.Id and SQP.SQCID='" + ScheduleId + @"'
							LEFT JOIN  MST.SQActivityGroup SAG ON SAG.Id=SQP.ActivityGroupId
                            where P.Active = 1 order by SQP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSQMachineDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN SQM.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,SQM.Id,MM.Id MachineMasterId,MM.UserName Machine, MC.UserName Category, MSC.UserName SubCategory
                            from MST.MachineMaster MM
							LEFT JOIN [MST].[SQMachine] SQM ON SQM.MachineMasterId=MM.Id and SQM.SQCID='" + ScheduleId + @"'
							left join HKP.MachineCategory MC on MC.Id = MM.MachineCategoryId
                            left join HKP.MachineSubCategory MSC on MSC.Id = MM.MachineSubCategoryId
                            where MM.Active = 1 order by SQM.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSQProductDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN SQP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,SQP.Id,PM.Id ProductMasterId,PM.Code, PM.StandardName Product,PG.UserName ProductCategory, PSC.UserName ProductSubCategory
                            from MST.ProductMaster PM
							LEFT JOIN [MST].[SQProduct] SQP ON SQP.ProductMasterId=PM.Id and SQP.SQCID='" + ScheduleId + @"'
							LEFT JOIN HKP.ProductCategory PG on PG.Id = PM.ProductCategoryId
                            LEFT JOIN HKP.ProductSubCategory PSC on PSC.Id = PM.ProductSubCategoryId
                            where PM.Active = 1 order by SQP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSQWorkCenterDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN SQW.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,SQW.Id,WM.Id as WorkCenterMasterId, WM.Code ,WM.UserName Workcenter, WC.UserName WorkcenterCategory, WCS.UserName WorkcenterSubCategory, P.UserName Process, E.UserName Entity, WM.Capacity, UOM.UserName UOM 
                            from SCS.WorkCenterMaster WM
							LEFT JOIN [MST].[SQWorkCenter] SQW ON SQW.WorkCenterMasterId=WM.Id and SQW.SQCID='" + ScheduleId + @"'
							LEFT JOIN HKP.WorkCenterCategory WC on WC.Id = WM.WorkCenterCategoryId
                            LEFT JOIN HKP.WorkCenterSubCategory WCS on WCS.Id = WM.WorkCenterSubcategoryId
                            left join HKP.Process P on P.Id = WM.ProcessId
                            left join org.Entity E on E.Id = WM.EntityId
                            LEFT JOIN SCS.UnitOfMeasurement UOM on UOM.Id = WM.UoMId 
                            where WM.Active = 1 and WM.EntityId in (select EntityId from [MST].[SQCEntity] where SQCID='" + ScheduleId + @"') and WM.ProcessId in (select ProcessId from MST.SQProcess where SQCID='" + ScheduleId + @"') order by SQW.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSQPositionCodeDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN SPC.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,SPC.Id,P.Id PositionCodeId,P.Code PositionCode,P.UserName Position,
D.UserName Division,DEP.UserName Department,S.UserName Section,SS.UserName SUbSection,P.Activity,DEG.UserName Designation,PRO.UserName Process,
P.UserReportGroup,SPC.Remarks 
                            from ORG.Position P
							LEFT JOIN ORG.Division D on D.Id=P.DivisionId
							LEFT JOIN ORG.Department DEP on DEP.Id=P.DepartmentId
							LEFT JOIN ORG.Section S on S.Id=P.SectionId
							LEFT JOIN ORG.SubSection SS on SS.Id=P.SubSectionId
							LEFT JOIN hkp.Designation DEG on DEG.Id=P.DesignationId
							LEFT JOIN hkp.Process PRO on PRO.Id=P.ProcessId
							LEFT JOIN [MST].[SQPositionCode] SPC ON SPC.PositionCodeId=P.Id and SPC.SQCID='" + ScheduleId + @"'
                            where P.Active = 1 order by SPC.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createEntity(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQCEntity]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where SQCID='" + Pid + "'");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");
                
                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and SQCID='" + item["SQCID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SQE" + _Id;
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

        [HttpPost]
        public ActionResult createPositionCode(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQPositionCode]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where SQCID='" + Pid + "'");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and SQCID='" + item["SQCID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SPC" + _Id;
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
        public ActionResult LoadSQActivityGroupDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from [MST].[SQActivityGroup] where SQCID ='" + ScheduleId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadActivityGroupEditData(string AGId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[SQActivityGroup] AG where AG.Id='" + AGId + @"'";
            return Json(new { activitygroup = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createActivityGroup(Dictionary<string, object> ActivityGroupData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQActivityGroup] where ActivityGroupName='" + ActivityGroupData["ActivityGroupName"] + "'", out DataSet dsSQAGValidation, false, "1");

                DataSet dsSQAG;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQActivityGroup] where Id='" + ActivityGroupData["Id"] + "'", out dsSQAG, false, "1");
                string _Id = "";

                #region data update
                if (dsSQAG.Tables[0].Rows.Count == 0)
                {
                    if (dsSQAGValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Activity Group Already Exist.");
                    }
                    else
                    { 
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SQAG", out _Id);
                    _Id = "SAG" + _Id;
                    ActivityGroupData["Id"] = _Id;
                    ActivityGroupData["SQCID"] = Pid;
                    AddNewRow(dsSQAG.Tables[0], ActivityGroupData);
                    }
                }
                else
                {
                    _Id = ActivityGroupData["Id"].ToString();
                    ActivityGroupData["SQCID"] = Pid;
                    EditRow(dsSQAG.Tables[0].Rows[0], ActivityGroupData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSQAG);

                return Json(new { Error = false, Data = ActivityGroupData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult ActivityGroupDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[SQActivityGroup] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult CheckPointDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[SQParameterCheckPoints] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult ReasonDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[SQParameterReason] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult createProcess(List<Dictionary<string, object>> DataList,string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQProcess]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where SQCID='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and SQCID='" + item["SQCID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SQP" + _Id;
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

        [HttpPost]
        public ActionResult ItemDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from MST.SQParameterItem where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult FrequencyDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [HKP].[SQFrequency] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult ReasonMasterDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [HKP].[SQReasonMaster] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult AuthorizedPersonDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [HKP].[SQQualityManagementAuthorizedPerson] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult DefectMasterDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[SQCDefectMaster] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult AQLMasterDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[SQCAQLMaster] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadSQFrequencyList(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select FV.Id,FV.QA,FV.Quality,FV.Management,FD.Id as FrequencyId,FD.UserName
from [HKP].[SQFrequency] FD
left join [MST].[SQParameterFrequencyValue] FV ON FV.FrequencyId=FD.Id and FV.ParameterId='" + ParameterId + @"' order by FD.SNO";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSQDefectList(string ParameterId, string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select DM.*,CAST (CASE WHEN PD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PD.Id,DM.Id as DefectId from MST.SQCDefectMaster DM  
left join MST.SQParameterDefect PD on PD.DefectId=DM.Id and PD.ParameterId='" + ParameterId + @"'
where DM.ProcessId='"+ ProcessId + "' order by DM.SNO";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadDefectCategoryList(string DefectId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select * from MST.SQCDefectMaster DM
where DM.Id='" + DefectId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetBudgetCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MP.Id ManPowerBudgetId, MP.Code, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from MST.ManpowerBudget MP
                            left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MP.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetByWhomeBudgetCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MP.Id ManPowerBudgetId, MP.Code, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from MST.ManpowerBudget MP
                            left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MP.Active = 1 and EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
       
      
        [Authorize, HttpGet]
        public ActionResult LoadItemEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,
(select Code from [MST].[ManpowerBudget] where Id=SQP.ByWhomId) as ByWhom,
(select P.UserName from hkp.process P where P.Id=SQP.ProcessId) as Process,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=SQP.UOMId) as UOM,
(select PM.UserName from HKP.SQParameterMaster PM where PM.Id=SQP.ParameterId) as ParameterName,
SQP.ActivityGroup as AGroup
FROM MST.SQParameterItem SQP where Id ='" + ItemId + "'";
            return Json(new { item = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadSQParameterEditData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[SQParameterCheckPoints] where Id='" + ParameterId + @"'";
            return Json(new { Parameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSQReasonEditData(string ReasonId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[SQParameterReason] where Id='" + ReasonId + @"'";
            return Json(new { Reason = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,
(select Code from [MST].[ManpowerBudget] where Id=SQP.ByWhomId) as ByWhom,
(select P.UserName from hkp.process P where P.Id=SQP.ProcessId) as Process,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=SQP.UOMId) as UOM,
(select PM.UserName from HKP.SQParameterMaster PM where PM.Id=SQP.ParameterId) as ParameterName,
SQP.ActivityGroup as AGroup
FROM MST.SQParameterItem SQP where SQCID ='" + ScheduleId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getSQParameterData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM [MST].[SQParameterCheckPoints] where ParameterId ='" + ParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getSQReasonData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT SPR.*,SQR.UserName ReasonName FROM [MST].[SQParameterReason] SPR
left join [HKP].[SQReasonMaster] SQR on SQR.Id=SPR.ReasonId
where SPR.ParameterId ='" + ParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getSQFrequency()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM [HKP].[SQFrequency] order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getSQReasonMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM [HKP].[SQReasonMaster] order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getDefectMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,(select UserName from hkp.Process where id=DM.ProcessId) DefectProcess FROM [MST].[SQCDefectMaster] DM  order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getAQLMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * FROM [MST].[SQCAQLMaster] order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getSQFrequencyData(string FrequencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * FROM [HKP].[SQFrequency] SQF where SQF.Id='" + FrequencyId + @"' order by SQF.SNO";
            return Json(new { frequency = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getSQReasonMasterData(string ReasonId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * FROM [HKP].[SQReasonMaster] SQR where SQR.Id='" + ReasonId + @"' order by SQR.SNO";
            return Json(new { ReasonMaster = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getDefectMasterData(string DefectId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,(select UserName from hkp.Process where id=DM.ProcessId) DefectProcess FROM [MST].[SQCDefectMaster] DM where DM.Id='" + DefectId + @"' order by SNO";
            return Json(new { DefectMaster = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getAQLMasterData(string AQLId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * FROM [MST].[SQCAQLMaster] AQM where AQM.Id='" + AQLId + @"' order by AQM.SNO";
            return Json(new { AQLMaster = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadBudgetCodeDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MPB.Id,MPB.SNO,MPB.PersonBudgetCodeId,MPB.[Group],MPB.AddedBy,MPB.AddedDate,MPB.AddedFromIP,MPB.UpdatedBy,MPB.UpdatedDate,MPB.UpdatedFromIP,MP.Id ManPowerBudgetId, MP.Code as PersonBudgetCode, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from [TRN].[SkillManagementPersonBudgetCode] MPB
                            left join MST.ManpowerBudget MP on MP.Id=MPB.PersonBudgetCodeId and MP.Active = 1
						    left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MPB.SMID='" + ScheduleId + "' order by MPB.SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateItem(Dictionary<string, object> ItemData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQParameterItem] where Id<>'" + ItemData["Id"] + "'", out DataSet dsSQParameterItemValidation, false, "1");

                DataSet dsSQParameterItem;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQParameterItem] where Id='" + ItemData["Id"] + "'", out dsSQParameterItem, false, "1");
                string _Id = "";

                #region data update
                if (dsSQParameterItem.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SQParameterItem", out _Id);
                    _Id = "SPI" + _Id;
                    ItemData["Id"] = _Id;
                    ItemData["SQCID"] = Pid;
                    AddNewRow(dsSQParameterItem.Tables[0], ItemData);
                }
                else
                {
                    _Id = ItemData["Id"].ToString();
                    ItemData["SQCID"] = Pid;
                    EditRow(dsSQParameterItem.Tables[0].Rows[0], ItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSQParameterItem);

                return Json(new { Error = false, Data = ItemData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost]
        public JsonResult CreateParameter(Dictionary<string, object> ParameterData, string Pid)
        {
            try
            {
               
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQParameterCheckPoints] where CheckPoints='" + ParameterData["CheckPoints"] + "' and ParameterId='" + Pid + "'", out DataSet dsSQParameterCheckPointsValidation, false, "1");

                DataSet dsSQParameterCheckPoints;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQParameterCheckPoints] where Id='" + ParameterData["Id"] + "'", out dsSQParameterCheckPoints, false, "1");
                string _Id = "";

                #region data update
                if (ParameterData["SNO"] == null)
                {
                    throw new Exception("SNO is required");
                }
                else
                {
                    if (ParameterData["CheckPoints"] == null)
                    {
                        throw new Exception("CheckPoints is required");
                    }
                    else
                    {
                        if (dsSQParameterCheckPoints.Tables[0].Rows.Count == 0)
                        {
                            if (dsSQParameterCheckPointsValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("CheckPoints Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("SQParameterCheckPoints", out _Id);
                                _Id = "SCP" + _Id;
                                ParameterData["Id"] = _Id;
                                ParameterData["ParameterId"] = Pid;
                                AddNewRow(dsSQParameterCheckPoints.Tables[0], ParameterData);
                            }
                        }
                        else
                        {
                            _Id = ParameterData["Id"].ToString();
                            ParameterData["ParameterId"] = Pid;
                            EditRow(dsSQParameterCheckPoints.Tables[0].Rows[0], ParameterData);
                        }
                    }
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSQParameterCheckPoints);

                return Json(new { Error = false, Data = ParameterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult CreateReason(Dictionary<string, object> ReasonData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQParameterReason] where ReasonId='" + ReasonData["ReasonId"] + "' and ParameterId='" + Pid + "'", out DataSet dsSQParameterReasonValidation, false, "1");

                DataSet dsSQParameterReason;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQParameterReason] where Id='" + ReasonData["Id"] + "'", out dsSQParameterReason, false, "1");
                string _Id = "";

                #region data update
                if (ReasonData["SNO"] == null)
                {
                    throw new Exception("SNO is required");
                }
                else
                {
                    if (ReasonData["ReasonId"] == null)
                    {
                        throw new Exception("Reason is required");
                    }
                    else
                    {
                        if (dsSQParameterReason.Tables[0].Rows.Count == 0)
                        {
                            if (dsSQParameterReasonValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("Reason Name Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("SQParameterReason", out _Id);
                                _Id = "SPR" + _Id;
                                ReasonData["Id"] = _Id;
                                ReasonData["ParameterId"] = Pid;
                                AddNewRow(dsSQParameterReason.Tables[0], ReasonData);
                            }
                        }
                        else
                        {
                            _Id = ReasonData["Id"].ToString();
                            ReasonData["ParameterId"] = Pid;
                            EditRow(dsSQParameterReason.Tables[0].Rows[0], ReasonData);
                        }
                    }
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSQParameterReason);

                return Json(new { Error = false, Data = ReasonData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult createFrequency(Dictionary<string, object> FrequencyData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[SQFrequency] where Id<>'" + FrequencyData["Id"] + "'", out DataSet dsSQFrequencyValidation, false, "1");

                DataSet dsSQFrequency;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[SQFrequency] where Id='" + FrequencyData["Id"] + "'", out dsSQFrequency, false, "1");
                string _Id = "";

                #region data update
                if (dsSQFrequency.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SQFrequency", out _Id);
                    _Id = "SQF" + _Id;
                    FrequencyData["Id"] = _Id;
                    AddNewRow(dsSQFrequency.Tables[0], FrequencyData);
                }
                else
                {
                    _Id = FrequencyData["Id"].ToString();
                    EditRow(dsSQFrequency.Tables[0].Rows[0], FrequencyData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSQFrequency);

                return Json(new { Error = false, Data = FrequencyData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult createReasonMaster(Dictionary<string, object> ReasonMasterData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[SQReasonMaster] where UserName='" + ReasonMasterData["UserName"] + "'", out DataSet dsSQReasonMasterReasonValidation, false, "1");

                DataSet dsSQReasonMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[SQReasonMaster] where Id='" + ReasonMasterData["Id"] + "'", out dsSQReasonMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsSQReasonMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SQReasonMaster", out _Id);
                    _Id = "SQR" + _Id;
                    ReasonMasterData["Id"] = _Id;
                    AddNewRow(dsSQReasonMaster.Tables[0], ReasonMasterData);
                }
                else
                {
                    _Id = ReasonMasterData["Id"].ToString();
                    EditRow(dsSQReasonMaster.Tables[0].Rows[0], ReasonMasterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSQReasonMaster);

                return Json(new { Error = false, Data = ReasonMasterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult createDefectMaster(Dictionary<string, object> DefectMasterData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQCDefectMaster] where Defect='" + DefectMasterData["Defect"] + "'", out DataSet dsSQCDefectMasterValidation, false, "1");

                DataSet dsSQCDefectMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQCDefectMaster] where Id='" + DefectMasterData["Id"] + "'", out dsSQCDefectMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsSQCDefectMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SQCDefectMaster", out _Id);
                    _Id = "SDM" + _Id;
                    DefectMasterData["Id"] = _Id;
                    AddNewRow(dsSQCDefectMaster.Tables[0], DefectMasterData);
                }
                else
                {
                    _Id = DefectMasterData["Id"].ToString();
                    EditRow(dsSQCDefectMaster.Tables[0].Rows[0], DefectMasterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSQCDefectMaster);

                return Json(new { Error = false, Data = DefectMasterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult createAQLMaster(Dictionary<string, object> AQLMasterData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                DataSet dsSQCAQLMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SQCAQLMaster] where Id='" + AQLMasterData["Id"] + "'", out dsSQCAQLMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsSQCAQLMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SQCAQLMaster", out _Id);
                    _Id = "AQL" + _Id;
                    AQLMasterData["Id"] = _Id;
                    AddNewRow(dsSQCAQLMaster.Tables[0], AQLMasterData);
                }
                else
                {
                    _Id = AQLMasterData["Id"].ToString();
                    EditRow(dsSQCAQLMaster.Tables[0].Rows[0], AQLMasterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSQCAQLMaster);

                return Json(new { Error = false, Data = AQLMasterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost]
        public JsonResult createFrequencyValue(List<Dictionary<string, object>> ParameterFrequencyData, string ParameterId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQParameterFrequencyValue]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (ParameterFrequencyData != null)
                {
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("delete from " + TableName + " where ParameterId='" + ParameterId + "'");
                    conC.CommitTransaction();

                    foreach (var item in ParameterFrequencyData)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SPF" + _Id;
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
                else
                {
                    throw new Exception("Please select atleast one value and proceed.");
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult createDefect(List<Dictionary<string, object>> ParameterDefectData, string ParameterId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQParameterDefect]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (ParameterDefectData != null)
                {
                    foreach (var item in ParameterDefectData)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SPD" + _Id;
                            item["ParameterId"] = ParameterId;
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
                else
                {
                    throw new Exception("Please select atleast one value and proceed.");
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult createDefectCategory(List<Dictionary<string, object>> DefectCategoryData, string DefectId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQCDefectMaster]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (DefectCategoryData != null)
                {
                    foreach (var item in DefectCategoryData)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SDM" + _Id;
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
                else
                {
                    throw new Exception("Please select atleast one value and proceed.");
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public ActionResult createMachine(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQMachine]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                

                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where SQCID='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and SQCID='" + item["SQCID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SQM" + _Id;
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

        [HttpPost]
        public ActionResult createProduct(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQProduct]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {


                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where SQCID='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and SQCID='" + item["SQCID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SQP" + _Id;
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

        [HttpPost]
        public ActionResult createWorkCenter(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQWorkCenter]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {


                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where SQCID='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {
                    

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and SQCID='" + item["SQCID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SQW" + _Id;
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
        public ActionResult LoadSQParameterResponsiblePersonDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PRP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PRP.Id,EMP.SystemId ResponsiblePersonId, EMP.EmployeeCode, EMP.EmployeeName, FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, EC.UserName EmployeeCategory, DP.UserName Department
                               ,SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation, LDSG.UserName LegalDesignation, UN.UserName as Entity
                                from EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
								LEFT JOIN [MST].[SQParameterResponsiblePerson] PRP ON PRP.ResponsiblePersonId=EMP.SystemId
                                where EMP.EmployeeStatus = 'Active' order by PRP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createParameterResponsiblePerson(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQParameterResponsiblePerson]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + "");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SPR" + _Id;
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
        public ActionResult LoadSQParameterApprovalResponsiblePersonDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN ARP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,ARP.Id,EMP.SystemId ApprovalResponsiblePersonId, EMP.EmployeeCode, EMP.EmployeeName, FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, EC.UserName EmployeeCategory, DP.UserName Department
                               ,SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation, LDSG.UserName LegalDesignation, UN.UserName as Entity
                                from EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
								LEFT JOIN [MST].[SQParameterApprovalResponsiblePerson] ARP ON ARP.ApprovalResponsiblePersonId=EMP.SystemId
                                where EMP.EmployeeStatus = 'Active' order by ARP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createParameterApprovalResponsiblePerson(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQParameterApprovalResponsiblePerson]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + "");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SAR" + _Id;
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
        public ActionResult LoadSQQualityActionResponsiblePersonDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QAR.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QAR.Id,EMP.SystemId SQQualityActionResponsiblePersonId, EMP.EmployeeCode, EMP.EmployeeName, FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, EC.UserName EmployeeCategory, DP.UserName Department
                               ,SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation, LDSG.UserName LegalDesignation, UN.UserName as Entity
                                from EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
								LEFT JOIN [MST].[SQQualityActionResponsiblePerson] QAR ON QAR.SQQualityActionResponsiblePersonId=EMP.SystemId
                                where EMP.EmployeeStatus = 'Active' order by QAR.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createQualityActionResponsiblePerson(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[SQQualityActionResponsiblePerson]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + "");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "SQA" + _Id;
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
        public ActionResult getSQAuthorizedPerson()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT SQA.*,(select EmployeeName from EmployeeInformation where SystemId=SQA.AuthorizedResPersonId) AuthorizedResPerson  FROM [HKP].[SQQualityManagementAuthorizedPerson] SQA order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getSQAuthorizedPersonData(string AuthorizedId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select SQA.*,(select EmployeeName from EmployeeInformation where SystemId=SQA.AuthorizedResPersonId) AuthorizedResPerson FROM [HKP].[SQQualityManagementAuthorizedPerson] SQA where SQA.Id='" + AuthorizedId + @"' order by SQA.SNO";
            return Json(new { AuthorizedPerson = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createAuthorizedPerson(Dictionary<string, object> AuthorizedPersonData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[SQQualityManagementAuthorizedPerson] where AuthorizedResPersonId='" + AuthorizedPersonData["AuthorizedResPersonId"] + "'", out DataSet dsSQQualityManagementAuthorizedPersonValidation, false, "1");

                DataSet dsSQQualityManagementAuthorizedPerson;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[SQQualityManagementAuthorizedPerson] where Id='" + AuthorizedPersonData["Id"] + "'", out dsSQQualityManagementAuthorizedPerson, false, "1");
                string _Id = "";

                #region data update
                if (dsSQQualityManagementAuthorizedPerson.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SQQualityManagementAuthorizedPerson", out _Id);
                    _Id = "A" + _Id;
                    AuthorizedPersonData["Id"] = _Id;
                    AddNewRow(dsSQQualityManagementAuthorizedPerson.Tables[0], AuthorizedPersonData);
                }
                else
                {
                    _Id = AuthorizedPersonData["Id"].ToString();
                    EditRow(dsSQQualityManagementAuthorizedPerson.Tables[0].Rows[0], AuthorizedPersonData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSQQualityManagementAuthorizedPerson);

                return Json(new { Error = false, Data = AuthorizedPersonData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public ActionResult GetAuthorizedResPerson()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}