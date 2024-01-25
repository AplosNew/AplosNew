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
    public class QualityManagementMasterController : Controller
    {
        #region Constructor


        ParameterService ps = new ParameterService();
        private readonly ISqlRepository _sqlRepository;

        public QualityManagementMasterController(ISqlRepository R)
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
        public JsonResult GetReasonNameLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from [HKP].[QualityManagementReasonMaster]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createParaMaster(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = ps.Save(data), Message = AplosMessage.Success });
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
                string ret = ps.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetAutoSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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

            var sql = @"select PM.Id, PM.Code,PM.StandardName,PM.UserName  from HKP.ParameterMaster PM where PM.IsActive = 1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetParameterProcessList(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select ProcessId as Value,P.UserName as Text from MST.QualityManagementProcess QMP 
left join hkp.Process P on P.id=QMP.ProcessId
where QMID='"+ ScheduleId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetParameterProcessAGList(string ScheduleId, string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select QMA.ActivityGroupName from MST.QualityManagementProcess QMP
left join MST.QualityManagementActivityGroup QMA on QMA.id=QMP.ActivityGroupId
where QMP.QMID='" + ScheduleId + "' and QMP.ProcessId='" + ProcessId + "'";

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
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementMaster] where ScheduleCode='" + ScheduleData["ScheduleCode"] + "'", out DataSet dsQualityManagmentMasterCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementMaster] where StandaredName='" + ScheduleData["StandaredName"] + "'", out DataSet dsQualityManagmentMasterSNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementMaster] where UserName='" + ScheduleData["UserName"] + "'", out DataSet dsQualityManagmentMasterUNameValidation, false, "1");
                

                DataSet dsQualityManagmentMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementMaster] where Id='" + ScheduleData["Id"] + "'", out dsQualityManagmentMaster, false, "1");
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
                        genid.GenID("QualityManagmentMaster", out _Id);
                        _Id = "QM" + _Id;
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
        public ActionResult LoadQualityManagementMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=QM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode
                            FROM [MST].[QualityManagementMaster] QM";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadQualityManagementEditData(string ScheduleID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=QM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode
                            FROM [MST].[QualityManagementMaster] QM where QM.Id='" + ScheduleID + @"'";
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
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementEntity] where QMID='" + id + "'", out EntityCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementActivityGroup] where QMID ='" + id + "'", out AGCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementParameterItem] where QMID ='" + id + "'", out ItemCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementProcess] where QMID ='" + id + "'", out ProcessCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementMachine] where QMID ='" + id + "'", out MachineCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementProduct] where QMID ='" + id + "'", out ProductCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementWorkCenter] where QMID ='" + id + "'", out WorkCenterCount, false, "1");

                if (EntityCount.Tables[0].Rows.Count == 0 && AGCount.Tables[0].Rows.Count == 0 && ItemCount.Tables[0].Rows.Count == 0 && ProcessCount.Tables[0].Rows.Count == 0 && MachineCount.Tables[0].Rows.Count == 0 && ProductCount.Tables[0].Rows.Count == 0 && WorkCenterCount.Tables[0].Rows.Count == 0)
                {

                    conC.BeginTransaction();
                    conC.executeQuery("delete from [MST].[QualityManagementMaster] where Id ='" + id + @"'");
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

            var sql = @"select Id as Value,ActivityGroupName as Text from MST.QualityManagementActivityGroup";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadEntityDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QME.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QME.Id,E.Id EntityId,E.EntityType,E.UserName Entity,E.Code,QME.Remarks 
                            from ORG.Entity E
							LEFT JOIN [MST].[QualityManagementEntity] QME ON QME.EntityId=E.Id and QME.QMID='" + ScheduleId + @"'
                            where E.Active = 1 order by QME.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QMP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QMP.Id,P.Id ProcessId,P.UserName Process,P.Code,QAG.Id as ActivityGroupId,QAG.ActivityGroupName,QMP.Remarks
                            from hkp.Process P
							LEFT JOIN [MST].[QualityManagementProcess] QMP ON QMP.ProcessId=P.Id and QMP.QMID='" + ScheduleId + @"'
							LEFT JOIN  MST.QualityManagementActivityGroup QAG ON QAG.Id=QMP.ActivityGroupId
                            where P.Active = 1 order by QMP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMachineDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QMM.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QMM.Id,MM.Id MachineMasterId,MM.UserName Machine, MC.UserName Category, MSC.UserName SubCategory
                            from MST.MachineMaster MM
							LEFT JOIN [MST].[QualityManagementMachine] QMM ON QMM.MachineMasterId=MM.Id and QMM.QMID='"+ ScheduleId + @"'
							left join HKP.MachineCategory MC on MC.Id = MM.MachineCategoryId
                            left join HKP.MachineSubCategory MSC on MSC.Id = MM.MachineSubCategoryId
                            where MM.Active = 1 order by QMM.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProductDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QMP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QMP.Id,PM.Id ProductMasterId,PM.Code, PM.StandardName Product,PG.UserName ProductCategory, PSC.UserName ProductSubCategory
                            from MST.ProductMaster PM
							LEFT JOIN [MST].[QualityManagementProduct] QMP ON QMP.ProductMasterId=PM.Id and QMP.QMID='" + ScheduleId + @"'
							LEFT JOIN HKP.ProductCategory PG on PG.Id = PM.ProductCategoryId
                            LEFT JOIN HKP.ProductSubCategory PSC on PSC.Id = PM.ProductSubCategoryId
                            where PM.Active = 1 order by QMP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadWorkCenterDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QMW.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QMW.Id,WM.Id as WorkCenterMasterId, WM.Code ,WM.UserName Workcenter, WC.UserName WorkcenterCategory, WCS.UserName WorkcenterSubCategory, P.UserName Process, E.UserName Entity, WM.Capacity, UOM.UserName UOM 
                            from SCS.WorkCenterMaster WM
							LEFT JOIN [MST].[QualityManagementWorkCenter] QMW ON QMW.WorkCenterMasterId=WM.Id and QMW.QMID='" + ScheduleId + @"'
							LEFT JOIN HKP.WorkCenterCategory WC on WC.Id = WM.WorkCenterCategoryId
                            LEFT JOIN HKP.WorkCenterSubCategory WCS on WCS.Id = WM.WorkCenterSubcategoryId
                            left join HKP.Process P on P.Id = WM.ProcessId
                            left join org.Entity E on E.Id = WM.EntityId
                            LEFT JOIN SCS.UnitOfMeasurement UOM on UOM.Id = WM.UoMId 
                            where WM.Active = 1 and WM.EntityId in (select EntityId from MST.QualityManagementEntity where QMID='" + ScheduleId + @"') and WM.ProcessId in (select ProcessId from MST.QualityManagementProcess where QMID='" + ScheduleId + @"') order by QMW.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadPositionCodeDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QPC.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QPC.Id,P.Id PositionCodeId,P.Code PositionCode,P.UserName Position,
D.UserName Division,DEP.UserName Department,S.UserName Section,SS.UserName SUbSection,P.Activity,DEG.UserName Designation,PRO.UserName Process,
P.UserReportGroup,QPC.Remarks 
                            from ORG.Position P
							LEFT JOIN ORG.Division D on D.Id=P.DivisionId
							LEFT JOIN ORG.Department DEP on DEP.Id=P.DepartmentId
							LEFT JOIN ORG.Section S on S.Id=P.SectionId
							LEFT JOIN ORG.SubSection SS on SS.Id=P.SubSectionId
							LEFT JOIN hkp.Designation DEG on DEG.Id=P.DesignationId
							LEFT JOIN hkp.Process PRO on PRO.Id=P.ProcessId
							LEFT JOIN [MST].[QualityManagementPositionCode] QPC ON QPC.PositionCodeId=P.Id and QPC.QMID='" + ScheduleId + @"'
                            where P.Active = 1 order by QPC.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createEntity(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[QualityManagementEntity]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where QMID='" + Pid + "'");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");
                
                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QMID='" + item["QMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "QME" + _Id;
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
            string TableName = "[MST].[QualityManagementPositionCode]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where QMID='" + Pid + "'");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QMID='" + item["QMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "QPC" + _Id;
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
        public ActionResult LoadQMActivityGroupDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from [MST].[QualityManagementActivityGroup] where QMID ='" + ScheduleId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadActivityGroupEditData(string AGId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[QualityManagementActivityGroup] AG where AG.Id='" + AGId + @"'";
            return Json(new { activitygroup = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createActivityGroup(Dictionary<string, object> ActivityGroupData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementActivityGroup] where ActivityGroupName='" + ActivityGroupData["ActivityGroupName"] + "'", out DataSet dsQualityManagementAGValidation, false, "1");

                DataSet dsQualityManagementAG;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementActivityGroup] where Id='" + ActivityGroupData["Id"] + "'", out dsQualityManagementAG, false, "1");
                string _Id = "";

                #region data update
                if (dsQualityManagementAG.Tables[0].Rows.Count == 0)
                {
                    if (dsQualityManagementAGValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Activity Group Already Exist.");
                    }
                    else
                    { 
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("QualityManagementAG", out _Id);
                    _Id = "QAG" + _Id;
                    ActivityGroupData["Id"] = _Id;
                    ActivityGroupData["QMID"] = Pid;
                    AddNewRow(dsQualityManagementAG.Tables[0], ActivityGroupData);
                    }
                }
                else
                {
                    _Id = ActivityGroupData["Id"].ToString();
                    ActivityGroupData["QMID"] = Pid;
                    EditRow(dsQualityManagementAG.Tables[0].Rows[0], ActivityGroupData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagementAG);

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
                conC.executeQuery("delete from [MST].[QualityManagementActivityGroup] where Id ='" + id + @"'");
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
                conC.executeQuery("delete from QualityManagementParameterCheckPoints where Id ='" + id + @"'");
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
                conC.executeQuery("delete from [MST].[QualityManagementParameterReason] where Id ='" + id + @"'");
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
            string TableName = "[MST].[QualityManagementProcess]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where QMID='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QMID='" + item["QMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "QMP" + _Id;
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
                conC.executeQuery("delete from [MST].[QualityManagementParameterItem] where Id ='" + id + @"'");
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
                conC.executeQuery("delete from [HKP].[QualityManagementFrequency] where Id ='" + id + @"'");
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
                conC.executeQuery("delete from [HKP].[QualityManagementReasonMaster] where Id ='" + id + @"'");
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
                conC.executeQuery("delete from [HKP].[QualityManagementAuthorizedPerson] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadFrequencyList(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select FV.Id,FV.QA,FV.Quality,FV.Management,FD.Id as FrequencyId,FD.UserName
from [HKP].[QualityManagementFrequency] FD
left join [MST].[QualityManagmentParameterFrequencyValue] FV ON FV.FrequencyId=FD.Id and FV.ParameterId='" + ParameterId + @"' order by FD.SNO";

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
(select Code from [MST].[ManpowerBudget] where Id=QMP.ByWhomId) as ByWhom,
(select P.UserName from hkp.process P where P.Id=QMP.ProcessId) as Process,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=QMP.UOMId) as UOM,
(select PM.UserName from HKP.ParameterMaster PM where PM.Id=QMP.ParameterId) as ParameterName,
QMP.ActivityGroup as AGroup
FROM [MST].[QualityManagementParameterItem] QMP where Id ='" + ItemId + "'";
            return Json(new { item = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadParameterEditData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from QualityManagementParameterCheckPoints where Id='" + ParameterId + @"'";
            return Json(new { Parameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadReasonEditData(string ReasonId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[QualityManagementParameterReason] where Id='" + ReasonId + @"'";
            return Json(new { Reason = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,
(select Code from [MST].[ManpowerBudget] where Id=QMP.ByWhomId) as ByWhom,
(select P.UserName from hkp.process P where P.Id=QMP.ProcessId) as Process,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=QMP.UOMId) as UOM,
(select PM.UserName from HKP.ParameterMaster PM where PM.Id=QMP.ParameterId) as ParameterName,
QMP.ActivityGroup as AGroup
FROM [MST].[QualityManagementParameterItem] QMP where QMID ='" + ScheduleId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getParameterData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM QualityManagementParameterCheckPoints where ParameterId ='" + ParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getReasonData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT QPR.*,QRM.UserName ReasonName FROM [MST].[QualityManagementParameterReason] QPR
left join [HKP].[QualityManagementReasonMaster] QRM on QRM.Id=QPR.ReasonId
where QPR.ParameterId ='" + ParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getFrequency()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM [HKP].[QualityManagementFrequency] order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getReasonMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM [HKP].[QualityManagementReasonMaster] order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getFrequencyData(string FrequencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * FROM [HKP].[QualityManagementFrequency] QMF where QMF.Id='" + FrequencyId + @"' order by SNO";
            return Json(new { frequency = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getReasonMasterData(string ReasonId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * FROM [HKP].[QualityManagementReasonMaster] QMR where QMR.Id='" + ReasonId + @"' order by SNO";
            return Json(new { ReasonMaster = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
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
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementParameterItem] where Id<>'" + ItemData["Id"] + "'", out DataSet dsQualityManagementParameterItemValidation, false, "1");

                DataSet dsQualityManagementParameterItem;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementParameterItem] where Id='" + ItemData["Id"] + "'", out dsQualityManagementParameterItem, false, "1");
                string _Id = "";

                #region data update
                if (dsQualityManagementParameterItem.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("QualityManagementParameterItem", out _Id);
                    _Id = "QMP" + _Id;
                    ItemData["Id"] = _Id;
                    ItemData["QMID"] = Pid;
                    AddNewRow(dsQualityManagementParameterItem.Tables[0], ItemData);
                }
                else
                {
                    _Id = ItemData["Id"].ToString();
                    ItemData["QMID"] = Pid;
                    EditRow(dsQualityManagementParameterItem.Tables[0].Rows[0], ItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagementParameterItem);

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
                conRack.OpenDataSetThroughAdapter("select * from QualityManagementParameterCheckPoints where CheckPoints='" + ParameterData["CheckPoints"] + "' and ParameterId='" + Pid + "'", out DataSet dsQualityManagementParameterCheckPointsValidation, false, "1");

                DataSet dsQualityManagementParameterCheckPoints;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from QualityManagementParameterCheckPoints where Id='" + ParameterData["Id"] + "'", out dsQualityManagementParameterCheckPoints, false, "1");
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
                        if (dsQualityManagementParameterCheckPoints.Tables[0].Rows.Count == 0)
                        {
                            if (dsQualityManagementParameterCheckPointsValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("CheckPoints Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("QualityManagementParameterCheckPoints", out _Id);
                                _Id = "SIP" + _Id;
                                ParameterData["Id"] = _Id;
                                ParameterData["ParameterId"] = Pid;
                                AddNewRow(dsQualityManagementParameterCheckPoints.Tables[0], ParameterData);
                            }
                        }
                        else
                        {
                            _Id = ParameterData["Id"].ToString();
                            ParameterData["ParameterId"] = Pid;
                            EditRow(dsQualityManagementParameterCheckPoints.Tables[0].Rows[0], ParameterData);
                        }
                    }
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagementParameterCheckPoints);

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
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementParameterReason] where ReasonId='" + ReasonData["ReasonId"] + "' and ParameterId='" + Pid + "'", out DataSet dsQualityManagementParameterReasonValidation, false, "1");

                DataSet dsQualityManagementParameterReason;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityManagementParameterReason] where Id='" + ReasonData["Id"] + "'", out dsQualityManagementParameterReason, false, "1");
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
                        if (dsQualityManagementParameterReason.Tables[0].Rows.Count == 0)
                        {
                            if (dsQualityManagementParameterReasonValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("Reason Name Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("QualityManagementParameterReason", out _Id);
                                _Id = "QPR" + _Id;
                                ReasonData["Id"] = _Id;
                                ReasonData["ParameterId"] = Pid;
                                AddNewRow(dsQualityManagementParameterReason.Tables[0], ReasonData);
                            }
                        }
                        else
                        {
                            _Id = ReasonData["Id"].ToString();
                            ReasonData["ParameterId"] = Pid;
                            EditRow(dsQualityManagementParameterReason.Tables[0].Rows[0], ReasonData);
                        }
                    }
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagementParameterReason);

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
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[QualityManagementFrequency] where Id<>'" + FrequencyData["Id"] + "'", out DataSet dsQualityManagementFrequencyValidation, false, "1");

                DataSet dsQualityManagementFrequency;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[QualityManagementFrequency] where Id='" + FrequencyData["Id"] + "'", out dsQualityManagementFrequency, false, "1");
                string _Id = "";

                #region data update
                if (dsQualityManagementFrequency.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("QualityManagementFrequency", out _Id);
                    _Id = "QMF" + _Id;
                    FrequencyData["Id"] = _Id;
                    AddNewRow(dsQualityManagementFrequency.Tables[0], FrequencyData);
                }
                else
                {
                    _Id = FrequencyData["Id"].ToString();
                    EditRow(dsQualityManagementFrequency.Tables[0].Rows[0], FrequencyData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagementFrequency);

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
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[QualityManagementReasonMaster] where UserName='" + ReasonMasterData["UserName"] + "'", out DataSet dsQualityManagementReasonMasterReasonValidation, false, "1");

                DataSet dsQualityManagementReasonMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[QualityManagementReasonMaster] where Id='" + ReasonMasterData["Id"] + "'", out dsQualityManagementReasonMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsQualityManagementReasonMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("QualityManagementReasonMaster", out _Id);
                    _Id = "QMR" + _Id;
                    ReasonMasterData["Id"] = _Id;
                    AddNewRow(dsQualityManagementReasonMaster.Tables[0], ReasonMasterData);
                }
                else
                {
                    _Id = ReasonMasterData["Id"].ToString();
                    EditRow(dsQualityManagementReasonMaster.Tables[0].Rows[0], ReasonMasterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagementReasonMaster);

                return Json(new { Error = false, Data = ReasonMasterData, Message = AplosMessage.Insert });

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
            string TableName = "[MST].[QualityManagmentParameterFrequencyValue]";
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
                            item["Id"] = "PFV" + _Id;
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
            string TableName = "[MST].[QualityManagementMachine]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                

                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where QMID='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QMID='" + item["QMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "QMM" + _Id;
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
            string TableName = "[MST].[QualityManagementProduct]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {


                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where QMID='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QMID='" + item["QMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "QMP" + _Id;
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
            string TableName = "[MST].[QualityManagementWorkCenter]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {


                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where QMID='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {
                    

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QMID='" + item["QMID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "QMW" + _Id;
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
        public ActionResult LoadParameterResponsiblePersonDetails()
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
								LEFT JOIN [MST].[ParameterResponsiblePerson] PRP ON PRP.ResponsiblePersonId=EMP.SystemId
                                where EMP.EmployeeStatus = 'Active' order by PRP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createParameterResponsiblePerson(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[ParameterResponsiblePerson]";
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
                            item["Id"] = "PRP" + _Id;
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
        public ActionResult LoadParameterApprovalResponsiblePersonDetails()
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
								LEFT JOIN [MST].[ParameterApprovalResponsiblePerson] ARP ON ARP.ApprovalResponsiblePersonId=EMP.SystemId
                                where EMP.EmployeeStatus = 'Active' order by ARP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createParameterApprovalResponsiblePerson(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[ParameterApprovalResponsiblePerson]";
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
                            item["Id"] = "ARP" + _Id;
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
        public ActionResult LoadQualityActionResponsiblePersonDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QAR.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QAR.Id,EMP.SystemId QualityActionResponsiblePersonId, EMP.EmployeeCode, EMP.EmployeeName, FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, EC.UserName EmployeeCategory, DP.UserName Department
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
								LEFT JOIN [MST].[QualityActionResponsiblePerson] QAR ON QAR.QualityActionResponsiblePersonId=EMP.SystemId
                                where EMP.EmployeeStatus = 'Active' order by QAR.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createQualityActionResponsiblePerson(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[QualityActionResponsiblePerson]";
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
                            item["Id"] = "QAR" + _Id;
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
        public ActionResult getAuthorizedPerson()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT QAP.*,(select EmployeeName from EmployeeInformation where SystemId=QAP.AuthorizedResPersonId) AuthorizedResPerson  FROM [HKP].[QualityManagementAuthorizedPerson] QAP order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getAuthorizedPersonData(string AuthorizedId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select QAP.*,(select EmployeeName from EmployeeInformation where SystemId=QAP.AuthorizedResPersonId) AuthorizedResPerson FROM [HKP].[QualityManagementAuthorizedPerson] QAP where QAP.Id='" + AuthorizedId + @"' order by SNO";
            return Json(new { AuthorizedPerson = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createAuthorizedPerson(Dictionary<string, object> AuthorizedPersonData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[QualityManagementAuthorizedPerson] where AuthorizedResPersonId='" + AuthorizedPersonData["AuthorizedResPersonId"] + "'", out DataSet dsQualityManagementAuthorizedResPersonValidation, false, "1");

                DataSet dsQualityManagementAuthorizedPerson;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[QualityManagementAuthorizedPerson] where Id='" + AuthorizedPersonData["Id"] + "'", out dsQualityManagementAuthorizedPerson, false, "1");
                string _Id = "";

                #region data update
                if (dsQualityManagementAuthorizedPerson.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("QualityManagementAuthorizedPerson", out _Id);
                    _Id = "A" + _Id;
                    AuthorizedPersonData["Id"] = _Id;
                    AddNewRow(dsQualityManagementAuthorizedPerson.Tables[0], AuthorizedPersonData);
                }
                else
                {
                    _Id = AuthorizedPersonData["Id"].ToString();
                    EditRow(dsQualityManagementAuthorizedPerson.Tables[0].Rows[0], AuthorizedPersonData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityManagementAuthorizedPerson);

                return Json(new { Error = false, Data = AuthorizedPersonData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public ActionResult GetAuthorizedPerson()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadCPSDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CPS.Id,ROW_NUMBER() Over (order by QMP.Id) SLNo,QMP.Id QMPId,QMP.QMID,QMM.UserName IssueName,
QMP.ParameterId,PM.UserName ParameterName,QMP.UOMId,UM.UserName UOM,CPS.Sequence from MST.QualityManagementParameterItem QMP 
left join MST.QualityManagementMaster QMM on QMM.Id=QMP.QMID
left join HKP.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UM on UM.Id=QMP.UOMId
left join [MST].[QualityManagementCPSequence] CPS on CPS.QMPId=QMP.Id
where CustomerParameter=1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult createCPSequence(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[QualityManagementCPSequence]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        objCon.OpenDataSetThroughAdapter("select * from " + TableName + " where Sequence='" + item["Sequence"] + "'", out DataSet dsQualityManagmentSequenceValidation, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            if (dsQualityManagmentSequenceValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("Sequence No Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID(TableName, out _Id);
                                item["Id"] = "CPS" + _Id;
                                AddNewRow(dsProdBooked.Tables[0], item);
                            }
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
        #endregion -- Operations
    }
}