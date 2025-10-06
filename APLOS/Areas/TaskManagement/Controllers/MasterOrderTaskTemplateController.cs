using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using System.Web;
using System.IO;
using Library.Service.Helpers;
using OTSBD;
using System.Linq;
using Library.Service.Enums;
using Newtonsoft.Json;

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class MasterOrderTaskTemplateController : BaseController
    {
        //CopyTask Save SaveMaster

        #region Constructor

        private readonly ISqlRepository _sqlRepository;


        public MasterOrderTaskTemplateController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (SELECT TM.*,ei.EmployeeCode,ei.EmployeeName
                          FROM MasterOrderTaskTemplate TM
                            left outer join EmployeeInformation EI on EI.Systemid=TM.EmployeeId) AS TEMP WHERE " + strkey + "";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult SearchEmployee(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                      select top 100 * from (  SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
    
                               WHERE emp.CompanyId='" + identity.CompanyId + @"' AND EMP.EmployeeStatus='Active' OR (emp.EmpType='GUEST' AND emp.EmployeeStatus='Active' AND emp.GroupID='" + identity.CompanyGroupId + @"') ) AS TEMP where " + strkey + " Order By Id";




            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetSingleEmployee(string Id)
        {

            string sql = @"SELECT * 
                            FROM EmployeeInformation EMP WHERE Systemid='" + Id + "'";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult GetMasterData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            var _plants = _sqlRepository.GetDataCollection("SELECT * FROM org.Plant AS p WHERE active=1 and p.CompanyGroupId='" + identity.CompanyGroupId + "' ORDER BY p.Sequence");
            var _Process = _sqlRepository.GetDataCollection("SELECT * FROM hkp.process AS p WHERE active=1 and p.CompanyGroupId='" + identity.CompanyGroupId + "' ORDER BY p.Sequence");
            var _department = _sqlRepository.GetDataCollection("SELECT * FROM org.Department  AS p where active=1 ORDER BY p.Sequence");
            var _taskAppliedOn = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskAppliedOn AS p where active=1 ORDER BY p.Sequence");
            var _taskCategory = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskCategory AS p where active=1 AND FLAG='" + TaskCategoryFlagEnum.TNA.ToString() + "' ORDER BY p.Sequence");



            return Json(new { Plant = _plants, Process = _Process, Department = _department, TaskAppliedOn = _taskAppliedOn, TaskCategory = _taskCategory }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetSelectedTaskList(string TemplateId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT tt.Id, tm.Sequence,p.UserName AS Process,tc.UserName AS TaskCategory,d.UserName AS Department, tm.TaskDescription,tt.TaskDescription AS UserDefineTask, tm.Code,
                                tm.TaskType,tm.Active,TT.predecessor,TT.IsTaskMilestone,TT.IsFirstTask,TT.IsLastTask,TT.IsMandatory,aon.TaskAppliedOnEnum,
                            ei.SystemId AS resourceId,ei.employeename as resourceName,ei.EmpPicPath,isnull(TM.RepeatTask,0) AS RepeatTask,
                            FORMAT( convert(date,startDate),'dd-MMM-yyyy') AS startDate,convert(date,tt.enddate) AS EndDate,0 AS Progress,
                              tt.Duration,tt.MasterOrderId
                          FROM MasterOrderTaskTemplate AS tt
                        LEFT OUTER JOIN  taskmaster TM ON tt.TaskMasterId=tm.Id
                        LEFT OUTER JOIN hkp.TaskAppliedOn AS AON ON aon.Id=tt.TaskAppliedOnId
                        LEFT OUTER JOIN hkp.TaskCategory AS tc ON tm.TaskCategoryId=tc.Id
                        LEFT OUTER JOIN hkp.Process AS p ON p.Id=tm.ProcessId
                        LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=tt.MasterOrderId
						LEFT OUTER JOIN EntityTask AS et ON et.EntityId=mo.EntityId AND et.TaskMasterId=tt.TaskMasterId AND tt.ResponsiblePersonCategory='Entity'
						LEFT OUTER JOIN mst.BuyerMaster AS bm ON isnull(bm.BuyerId,'')=isnull(mo.BuyerId,'')
																AND ISNULL(bm.BuyerDepartmentId,isnull(mo.BuyerDepartmentId,''))=isnull(mo.BuyerDepartmentId,'')
																AND ISNULL(bm.BuyerDivisionId,isnull(mo.BuyerDivisionId,''))=isnull(mo.BuyerDivisionId,'')
						LEFT OUTER JOIN BuyerMasterTask AS bmt ON bmt.BuyerMasterId=bm.Id AND tt.TaskMasterId=bmt.TaskMasterId AND tt.ResponsiblePersonCategory='Buyer' AND bmt.Active=1
						left outer join employeeinformation ei on ei.systemid=isnull(tt.EmployeeId,isnull(bmt.EmpSystemId,et.EmpSystemId))
                        LEFT OUTER JOIN org.Department AS d ON d.Id=tm.DepartmentId 
                WHERE tt.MasterOrderId='" + TemplateId + "' ORDER BY convert(int,isnull(tt.RefTaskTemplateId,999999999)),convert(int,tt.Id)";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;

            return jsondata;
        }
        [Authorize, HttpGet]
        public ActionResult SearchTaskMaster(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (SELECT tm.Id, tm.Sequence,p.UserName AS Process,tc.UserName AS TaskCategory,d.UserName AS Department, tm.TaskDescription, tm.UserDefineTask, tm.Code,
                              aon.UserName AS TaskAppliedOn,  tm.TaskType,tm.Active
                          FROM taskmaster TM
                        LEFT OUTER JOIN hkp.TaskCategory AS tc ON tm.TaskCategoryId=tc.Id
                        LEFT OUTER JOIN hkp.Process AS p ON p.Id=tm.ProcessId
                        LEFT OUTER JOIN org.Department AS d ON d.Id=tm.DepartmentId 
                        LEFT OUTER JOIN hkp.TaskAppliedOn AON ON aon.Id=tm.TaskAppliedOnId
                        LEFT OUTER JOIN TaskMasterPlantAssignment AS tmpa ON tmpa.TaskMasterId=tm.Id AND tmpa.PlantId='" + identity.PlantId + @"') AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion


        [Authorize, HttpPost]
        public ActionResult GenerateTnALog(string MasterOrderId)
        {
            try
            {
                Library.Planning.OrderManagement.MasterOrder mo = new Library.Planning.OrderManagement.MasterOrder();
                mo.GenerateLogForTnA(MasterOrderId, Library.Service.Enums.TaskAppliedOnEnum.MasterOrder);

            }
            catch (Exception ex)
            {

            }
            return null;
        }

        [Authorize, HttpPost]
        public ActionResult CopyTask(string TaskId, string TemplateMasterId)
        {
            //copy with subtasks
            try
            {
                DataTable dtTemp = _sqlRepository.GetDataTable("SELECT * FROM MasterOrderTaskTemplate AS tt WHERE tt.MasterOrderId='" + TemplateMasterId + "' AND tt.TaskMasterId='" + TaskId + "'");
                if (dtTemp.Rows.Count > 0)
                {
                    throw new Exception("You have already added this task in this template");
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsTaskSource, dsSubTaskSource, dsTaskDestination, dsSubTaskDestination;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskMaster where Id='" + TaskId + "'", out dsTaskSource, false, "1");
                con.OpenDataSetThroughAdapter("select * from SubTasks where TaskMasterId='" + TaskId + "'", out dsSubTaskSource, false, "1");


                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplate where 1=2", out dsTaskDestination, false, "1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplateSubTasks where 1=2", out dsSubTaskDestination, false, "1");

                #region Master
                string _TaskMId = "";
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Task Template Task Creation", out _TaskMId);

                _TaskMId = _TaskMId.Replace("-", "");
                DataRow dr = dsTaskDestination.Tables[0].NewRow();


                dr["Id"] = _TaskMId.Replace("-", "");
                dr["TaskDescription"] = dsTaskSource.Tables[0].Rows[0]["UserDefineTask"].ToString();
                dr["MasterOrderId"] = TemplateMasterId;
                dr["TaskMasterId"] = TaskId;

                dr["Active"] = true;
                dr["Remarks"] = dsTaskSource.Tables[0].Rows[0]["Remarks"].ToString();
                dr["Sequence"] = dsTaskSource.Tables[0].Rows[0]["Sequence"].ToString();



                dr["ForNewOrder"] = dsTaskSource.Tables[0].Rows[0]["ForNewOrder"].ToString();
                dr["IsMandatory"] = dsTaskSource.Tables[0].Rows[0]["IsMandatory"].ToString();
                dr["TaskType"] = dsTaskSource.Tables[0].Rows[0]["TaskType"].ToString();
                dr["IsTaskMilestone"] = dsTaskSource.Tables[0].Rows[0]["IsTaskMilestone"].ToString();
                dr["TaskDependentDatesId"] = dsTaskSource.Tables[0].Rows[0]["TaskDependentDatesId"].ToString();
                dr["TaskAppliedOnId"] = dsTaskSource.Tables[0].Rows[0]["TaskAppliedOnId"].ToString();
                dr["WillSendEmail"] = dsTaskSource.Tables[0].Rows[0]["WillSendEmail"].ToString();
                dr["WillSendSMS"] = dsTaskSource.Tables[0].Rows[0]["WillSendSMS"].ToString();
                dr["ResponsiblePersonCategory"] = dsTaskSource.Tables[0].Rows[0]["ResponsiblePersonCategory"].ToString();
                dr["StoryPoint"] = clsStaticInfo.dbl(dsTaskSource.Tables[0].Rows[0]["StoryPoint"].ToString());


                dr["LagDays"] = dsTaskSource.Tables[0].Rows[0]["LagDays"].ToString();
                dr["Duration"] = dsTaskSource.Tables[0].Rows[0]["StandardDays"].ToString();
                dr["startDate"] = new DateTime(2017, 1, 6).ToString("dd-MMM-yyyy");

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;


                dsTaskDestination.Tables[0].Rows.Add(dr);
                #endregion Master

                #region subtasks
                //string subtaskid = "";
                //genid = new bplib.clsGenID();
                //genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Task Template Task Creation", out subtaskid);

                for (int i = 0; i < dsSubTaskSource.Tables[0].Rows.Count; i++)
                {
                    dr = dsSubTaskDestination.Tables[0].NewRow();


                    //dr["Id"] = subtaskid + "-" + (i + 1).ToString();
                    dr["MasterOrderTaskTemplateId"] = _TaskMId;
                    dr["SubTaskDescription"] = dsSubTaskSource.Tables[0].Rows[i]["SubTaskDescription"].ToString();

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsSubTaskDestination.Tables[0].Rows.Add(dr);
                }
                #endregion subtasks


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTaskDestination, dsSubTaskDestination);


                #region Update First Task
                string _sql = @"SELECT tao.Id MainId,tt.TaskAppliedOnId AllUsedId,tt2.TaskAppliedOnId AS CurrentId FROM hkp.TaskAppliedOn AS tao
                                        LEFT OUTER JOIN (SELECT distinct MasterOrderTaskTemplate.TaskAppliedOnId
                                                           FROM  MasterOrderTaskTemplate WHERE MasterOrderId='" + TemplateMasterId + @"') AS tt ON tao.Id=tt.TaskAppliedOnId
                                        LEFT OUTER JOIN MasterOrderTaskTemplate AS tt2 ON tao.Id=tt2.TaskAppliedOnId AND tt2.Id='" + dsTaskDestination.Tables[0].Rows[0]["Id"].ToString() + @"'
                                        WHERE ISNULL(tt.TaskAppliedOnId,'')<>''";


                DataTable dtTable = _sqlRepository.GetDataTable(_sql);
                string _ids = "''";
                bool foundHierarchy = false;
                for (int i = 0; i < dtTable.Rows.Count; i++)
                {

                    if (foundHierarchy == true)
                        _ids += ",'" + dtTable.Rows[i]["AllUsedId"].ToString() + "'";

                    if (dtTable.Rows[i]["CurrentId"].ToString() == dsTaskDestination.Tables[0].Rows[0]["TaskAppliedOnId"].ToString())
                        foundHierarchy = true;
                }

                //update here
                con = new ConnectionManager.DAL.ConManager("1");
                con.BeginTransaction();
                con.ExecuteNonQueryWrapper("UPDATE MasterOrderTaskTemplate SET IsFirstTask = 0 WHERE TaskAppliedOnId IN (" + _ids + ") AND MasterOrderId='" + TemplateMasterId + "' ", true, "1");
                con.CommitTransaction();
                #endregion Update First Task



                return Json(new { Error = false, Message = "Task Added Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult Save(Dictionary<string, object> taskmaster, List<Dictionary<string, object>> subtasks, string plants)
        {
            try
            {
                DataSet dsMaster, dsChild, dsPlants;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskMaster where Id='" + taskmaster["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from SubTasks where TaskMasterId='" + taskmaster["Id"] + "'", out dsChild, false, "1");
                con.OpenDataSetThroughAdapter("select * from TaskMasterPlantAssignment where TaskMasterId='" + taskmaster["Id"] + "'", out dsPlants, false, "1");


                DataRow dr;
                string _TaskMasterId = "";

                #region task master
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Task Master Creation", out _TaskMasterId);
                    _TaskMasterId = _TaskMasterId.Replace("-", "").Substring(2);


                    taskmaster["Id"] = _TaskMasterId;
                    AddNewRow(dsMaster.Tables[0], taskmaster);
                }
                else
                {
                    _TaskMasterId = taskmaster["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], taskmaster);
                }
                #endregion task master

                #region task childs-[sub tasks]
                if (subtasks != null && subtasks.Count > 0)
                {

                    for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                    {
                        List<Dictionary<string, object>> temp = subtasks.Where(ee => ee.ContainsValue((int)clsStaticInfo.dbl(dsChild.Tables[0].Rows[i]["Id"].ToString()))).ToList();
                        if (temp.Count == 0)
                            dsChild.Tables[0].Rows[i].Delete();
                    }

                    for (int i = 0; i < subtasks.Count; i++)
                    {
                        subtasks[i]["TaskMasterId"] = _TaskMasterId;

                        //
                        if (subtasks[i]["Id"] == null)
                        {
                            AddNewRow(dsChild.Tables[0], subtasks[i]);

                        }
                        else
                        {
                            dsChild.Tables[0].DefaultView.RowFilter = "Id=" + (int)clsStaticInfo.dbl(subtasks[i]["Id"].ToString()) + "";
                            if (dsChild.Tables[0].DefaultView.Count > 0)
                                EditRow(dsChild.Tables[0].DefaultView[0].Row, subtasks[i]);

                        }
                    }

                }
                else
                {
                    while (dsChild.Tables[0].DefaultView.Count > 0)
                        dsChild.Tables[0].DefaultView[0].Delete();

                }

                #endregion task childs-[sub tasks]

                #region plant Assignments

                while (dsPlants.Tables[0].DefaultView.Count > 0)
                    dsPlants.Tables[0].DefaultView[0].Delete();

                if (plants != null || string.IsNullOrEmpty(plants) == false)
                {
                    string[] plantids = plants.Split(',');
                    foreach (string item in plantids)
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        dr = dsPlants.Tables[0].NewRow();

                        dr["TaskMasterId"] = _TaskMasterId;
                        dr["PlantId"] = item.Trim();

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsPlants.Tables[0].Rows.Add(dr);
                    }

                }
                #endregion plant Assignments

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild, dsPlants);


                return Json(new { Error = false, Id = _TaskMasterId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult SaveMaster(Dictionary<string, object> taskmaster)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplate where Id='" + taskmaster["Id"] + "'", out dsMaster, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _TaskMasterId = "";

                #region task master
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Task Template Master", out _TaskMasterId);
                    _TaskMasterId = _TaskMasterId.Replace("-", "").Substring(2);


                    taskmaster["Id"] = _TaskMasterId;
                    AddNewRow(dsMaster.Tables[0], taskmaster);
                    dsMaster.Tables[0].Rows[0]["PlantId"] = identity.PlantId;

                }
                else
                {
                    _TaskMasterId = taskmaster["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], taskmaster);
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Id = _TaskMasterId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize, HttpPost]
        public ActionResult UpdateTask(Dictionary<string, object> MasterOrderTaskTemplate)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplate where Id='" + MasterOrderTaskTemplate["Id"] + "'", out dsMaster, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _TaskMasterId = "";

                //if (MasterOrderTaskTemplate["ResponsiblePersonCategory"].ToString() == "Employee")
                //{
                //    if (MasterOrderTaskTemplate["EmployeeId"] == null)
                //        throw new Exception("Select responsible person");
                //}
                //else
                //{
                //    MasterOrderTaskTemplate["EmployeeId"] = null;
                //}

                if ((bool)MasterOrderTaskTemplate["Active"] == false)
                    MasterOrderTaskTemplate["Duration"] = 0;


                if (MasterOrderTaskTemplate["TaskDescription"] == null || MasterOrderTaskTemplate["TaskDescription"].ToString().Trim() == "")
                    throw new Exception("Insert Task Description");


                if (MasterOrderTaskTemplate["IsFirstTask"] == null)
                    MasterOrderTaskTemplate["IsFirstTask"] = false;

                if (MasterOrderTaskTemplate["IsLastTask"] == null)
                    MasterOrderTaskTemplate["IsLastTask"] = false;



                if ((bool)MasterOrderTaskTemplate["IsFirstTask"] == true && (bool)MasterOrderTaskTemplate["IsLastTask"] == true)
                    throw new Exception("Cannot assign both First Task and Last Task for current task");

                if ((bool)MasterOrderTaskTemplate["IsFirstTask"] == true)
                {
                    string _sql = @"SELECT tao.Id MainId,tt.TaskAppliedOnId AllUsedId,tt2.TaskAppliedOnId AS  CurrentId,tao.TaskAppliedOnEnum FROM hkp.TaskAppliedOn AS tao
                                        LEFT OUTER JOIN (SELECT distinct MasterOrderTaskTemplate.TaskAppliedOnId
                                                           FROM  MasterOrderTaskTemplate WHERE MasterOrderId='" + MasterOrderTaskTemplate["MasterOrderId"] + @"') AS tt ON tao.Id=tt.TaskAppliedOnId
                                        LEFT OUTER JOIN MasterOrderTaskTemplate AS tt2 ON tao.Id=tt2.TaskAppliedOnId AND tt2.Id='" + MasterOrderTaskTemplate["Id"] + @"'
                                        WHERE ISNULL(tt.TaskAppliedOnId,'')<>''";

                    DataTable dtTable = _sqlRepository.GetDataTable(_sql);
                    //if (dtTable.Rows[0]["CurrentId"].ToString() != taskTemplate["TaskAppliedOnId"].ToString())
                    //    throw new Exception("Cannot set current task as first task because this task is not on the top of the task hierarchy");
                    dtTable.DefaultView.RowFilter = "isnull(CurrentId,'')<>''";
                    if (dtTable.DefaultView.Count > 0)
                    {
                        if (dtTable.DefaultView[0]["TaskAppliedOnEnum"].ToString() != TaskAppliedOnEnum.MasterOrder.ToString())
                        {
                            throw new Exception("Cannot set current task as first task because this task does not belong to master order");

                        }
                    }
                    //if predecessor exists
                    _sql = @"SELECT tt.* FROM MasterOrderTaskTemplate AS tt 
                            WHERE tt.MasterOrderId='" + MasterOrderTaskTemplate["MasterOrderId"] + "' AND tt.Id='" + MasterOrderTaskTemplate["Id"] + @"' AND isnull(tt.predecessor,'')<>''";
                    dtTable = _sqlRepository.GetDataTable(_sql);
                    if (dtTable.Rows.Count > 0)
                    {
                        throw new Exception("Dependent task cannot be the first task");

                    }

                    //not the only one first task
                    _sql = @"SELECT tt.* FROM MasterOrderTaskTemplate AS tt 
                                WHERE tt.MasterOrderId='" + MasterOrderTaskTemplate["MasterOrderId"] + "' AND tt.Id<>'" + MasterOrderTaskTemplate["Id"] + @"' AND tt.IsFirstTask=1";
                    dtTable = _sqlRepository.GetDataTable(_sql);
                    if (dtTable.Rows.Count > 0)
                    {
                        throw new Exception("Only one task can be first task");

                    }




                }
                if ((bool)MasterOrderTaskTemplate["IsLastTask"] == true)
                { //not the only one last task
                    string _sql = @"SELECT tt.* FROM MasterOrderTaskTemplate AS tt 
                                WHERE tt.MasterOrderId='" + MasterOrderTaskTemplate["MasterOrderId"] + "' AND tt.Id<>'" + MasterOrderTaskTemplate["Id"] + @"' AND tt.IsLastTask=1";
                    DataTable dtTable = _sqlRepository.GetDataTable(_sql);
                    if (dtTable.Rows.Count > 0)
                    {
                        throw new Exception("Only one task can be last task");
                    }
                }
                #region task master
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    _TaskMasterId = MasterOrderTaskTemplate["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], MasterOrderTaskTemplate);
                }



                #endregion task master

                #region Other Tasks For Applied on
                DataSet dsOtherMasterOrderTaskTemplates;
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplate where MasterOrderId='" + MasterOrderTaskTemplate["MasterOrderId"] + @"' AND TaskMasterId='" + MasterOrderTaskTemplate["TaskMasterId"] + "' AND Id<>'" + MasterOrderTaskTemplate["Id"] + "'", out dsOtherMasterOrderTaskTemplates, false, "1");

                for (int i = 0; i < dsOtherMasterOrderTaskTemplates.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = dsOtherMasterOrderTaskTemplates.Tables[0].Rows[i];
                    dr.BeginEdit();
                    //dr["ResponsiblePersonCategory"] = bplib.clsWebLib.RetValidLen(dsMaster.Tables[0].Rows[0]["ResponsiblePersonCategory"].ToString());
                    //dr["EmployeeId"] = bplib.clsWebLib.RetValidLen(dsMaster.Tables[0].Rows[0]["EmployeeId"].ToString());

                    dr.EndEdit();

                }

                #endregion Other Tasks For Applied on

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsOtherMasterOrderTaskTemplates);



                return Json(new { Error = false, Id = _TaskMasterId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize, HttpPost]
        public ActionResult UpdateTaskDuration(string duration, string startDate, string TaskTemplateId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplate where Id='" + TaskTemplateId + "'", out dsMaster, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _TaskMasterId = "";



                #region task master
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    dsMaster.Tables[0].Rows[0].BeginEdit();
                    dsMaster.Tables[0].Rows[0]["Duration"] = clsStaticInfo.dbl(duration);
                    dsMaster.Tables[0].Rows[0]["startDate"] = startDate;
                    dsMaster.Tables[0].Rows[0].EndEdit();
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);



                return Json(new { Error = false, Id = _TaskMasterId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize, HttpPost]
        public ActionResult UpdateFullTaskDuration(List<Dictionary<string, object>> TaskTemplateIds, string MasterOrderId)
        {
            try
            {

                if (TaskTemplateIds == null || TaskTemplateIds.Count == 0)
                    return Json(new { Error = false, Message = "" }, JsonRequestBehavior.AllowGet);


                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplate where MasterOrderId='" + MasterOrderId + "'", out dsMaster, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _TaskMasterId = "";



                #region task master

                for (int D = 0; D < TaskTemplateIds.Count; D++)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + TaskTemplateIds[D]["taskID"].ToString() + "'";
                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["TaskDescription"] = TaskTemplateIds[D]["taskName"].ToString();
                        dr["Duration"] = TaskTemplateIds[D]["duration"].ToString();
                        dr["startDate"] = Convert.ToDateTime(TaskTemplateIds[D]["startDate"].ToString());
                        dr["endDate"] = Convert.ToDateTime(TaskTemplateIds[D]["endDate"].ToString());
                        dr["Active"] = false;
                        if (clsStaticInfo.dbl(TaskTemplateIds[D]["duration"].ToString()) > 0)
                            dr["Active"] = true;

                        if (clsStaticInfo.dbl(TaskTemplateIds[D]["duration"].ToString()) < 0)
                            dr["Duration"] = 0;
                        dr.EndEdit();
                    }
                }


                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);




                return Json(new { Error = false, Id = _TaskMasterId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult SaveSubTasks(string TaskTemplateId, Dictionary<string, object> MasterOrderTaskTemplate)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplateSubTasks where Id='" + MasterOrderTaskTemplate["Id"] + "'", out dsMaster, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _TaskMasterId = "";



                #region task master
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Task Template Sub Task", out _TaskMasterId);
                    _TaskMasterId = _TaskMasterId.Replace("-", "").Substring(2);


                    MasterOrderTaskTemplate["Id"] = _TaskMasterId;
                    AddNewRow(dsMaster.Tables[0], MasterOrderTaskTemplate);
                    dsMaster.Tables[0].Rows[0]["MasterOrderTaskTemplateId"] = TaskTemplateId;
                }
                else
                {
                    _TaskMasterId = MasterOrderTaskTemplate["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], MasterOrderTaskTemplate);
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Id = _TaskMasterId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize, HttpPost]
        public ActionResult SaveMasterOrderTaskTemplateDependency(string TaskTemplateId, Dictionary<string, object> MasterOrderTaskTemplate)
        {
            try
            {
                if (TaskTemplateId == MasterOrderTaskTemplate["PreTaskTemplateId"].ToString())
                    throw new Exception("Same task cannot be a dependent task itself");

                if (MasterOrderTaskTemplate["Criteria"] == null)
                    throw new Exception("Missing Criteria");

                if (MasterOrderTaskTemplate["LagDays"] == null)
                    MasterOrderTaskTemplate["LagDays"] = "0";
                MasterOrderTaskTemplate["LagDays"] = clsStaticInfo.dbl(MasterOrderTaskTemplate["LagDays"].ToString());



                string _sqlValidation = @"SELECT* FROM MasterOrderTaskTemplate AS tt WHERE tt.Id = '" + TaskTemplateId + "'";
                DataTable dtTableValidation = _sqlRepository.GetDataTable(_sqlValidation);
                if (bplib.clsWebLib.GetBoolData(dtTableValidation.Rows[0]["IsFirstTask"].ToString()))
                    throw new Exception("Selected task cannot be depend on other tasks because it has been assigned as first task");

                //DataSet ds;
                //ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("SELECT * FROM MasterOrderTaskTemplateDependency  WHERE PreTaskTemplateId='" + TaskTemplateId + "' AND TaskTemplateId='" + MasterOrderTaskTemplate["PreTaskTemplateId"] + "'", out ds, false, "1");
                //if (ds.Tables[0].Rows.Count > 0)
                //    throw new Exception("Cyclic task dependency will be created. Cannot update data");

                DataSet ds;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(@"SELECT * FROM MasterOrderTaskTemplateDependency AS t WHERE t.TaskTemplateId IN (

                                                SELECT tt.Id

                                                 FROM MasterOrderTaskTemplate AS tt WHERE tt.MasterOrderId IN(
                                                    SELECT tt.MasterOrderId FROM MasterOrderTaskTemplate AS tt WHERE tt.Id = '" + TaskTemplateId + @"'
                                            ))", out ds, false, "1");

                bool isReccursion = Reccursion(MasterOrderTaskTemplate["PreTaskTemplateId"].ToString(), TaskTemplateId, ds.Tables[0]);
                if (isReccursion)
                    throw new Exception("Cyclic task dependency will be created. Cannot update data");



                DataSet dsMaster;
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplateDependency where Id='" + MasterOrderTaskTemplate["Id"] + "'", out dsMaster, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _TaskMasterId = "";



                #region task master
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Task Template Dependency", out _TaskMasterId);
                    _TaskMasterId = _TaskMasterId.Replace("-", "").Substring(2);


                    MasterOrderTaskTemplate["Id"] = _TaskMasterId;
                    AddNewRow(dsMaster.Tables[0], MasterOrderTaskTemplate);
                    dsMaster.Tables[0].Rows[0]["TaskTemplateId"] = TaskTemplateId;


                }
                else
                {
                    _TaskMasterId = MasterOrderTaskTemplate["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], MasterOrderTaskTemplate);
                }
                #endregion task master





                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                updateDependency(TaskTemplateId);



                return Json(new { Error = false, Id = _TaskMasterId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        private bool Reccursion(string Pre, string Cur, DataTable dt)
        {
            dt.DefaultView.RowFilter = "TaskTemplateId=" + Pre + "";
            if (dt.DefaultView.Count == 0)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < dt.DefaultView.Count; i++)
                {
                    if (dt.DefaultView[i]["PreTaskTemplateId"].ToString() == Cur)
                        return true;
                    else
                        return Reccursion(dt.DefaultView[i]["PreTaskTemplateId"].ToString(), Cur, dt);
                }
            }

            return false;

        }
        private void updateDependency(string TaskTemplateId)
        {
            DataSet dsdep, dsPre;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplateDependency where TaskTemplateId='" + TaskTemplateId + "'", out dsdep, false, "1");
            con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplate where Id='" + TaskTemplateId + "'", out dsPre, false, "1");
            string _pre = "";
            for (int i = 0; i < dsdep.Tables[0].Rows.Count; i++)
            {
                string _s = dsdep.Tables[0].Rows[i]["PreTaskTemplateId"].ToString() + dsdep.Tables[0].Rows[i]["Criteria"].ToString();

                if (clsStaticInfo.dbl(dsdep.Tables[0].Rows[i]["LagDays"].ToString()) > 0)
                    _s += "+" + Math.Abs(clsStaticInfo.dbl(dsdep.Tables[0].Rows[i]["LagDays"].ToString()));

                if (clsStaticInfo.dbl(dsdep.Tables[0].Rows[i]["LagDays"].ToString()) < 0)
                    _s += "-" + Math.Abs(clsStaticInfo.dbl(dsdep.Tables[0].Rows[i]["LagDays"].ToString()));

                if (_pre == "")
                    _pre = _s;
                else
                    _pre += "," + _s;

            }
            dsPre.Tables[0].Rows[0].BeginEdit();
            dsPre.Tables[0].Rows[0]["predecessor"] = _pre;
            dsPre.Tables[0].Rows[0].EndEdit();


            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsPre);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSingleMasterOrderTaskTemplate(string Id)
        {
            string _Task = @"SELECT  TT.*,EI.EmployeeCode,EI.EmployeeName,tao.TaskAppliedOnEnum AS TaskDependentOn
                              FROM MasterOrderTaskTemplate TT LEFT OUTER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tt.TaskAppliedOnId
                               left outer join employeeInformation EI on ei.SystemID=TT.EmployeeId where TT.id='" + Id + "'";
            string _SubTasks = "SELECT  * FROM MasterOrderTaskTemplateSubTasks where MasterOrderTaskTemplateId='" + Id + "'";
            string _DependencyList = "SELECT  * FROM MasterOrderTaskTemplateDependency where TaskTemplateId='" + Id + "'";

            return Json(new
            {
                Task = _sqlRepository.GetDataCollection(_Task),
                SubTasks = _sqlRepository.GetDataCollection(_SubTasks),
                DependencyList = _sqlRepository.GetDataCollection(_DependencyList)
            }, JsonRequestBehavior.AllowGet);
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM MasterOrderTaskTemplate");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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

        [HttpGet, Authorize]
        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || id == "null")
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                //con.executeQuery("delete from TaskMasterPlantAssignment where TaskMasterId='" + id + "'");
                //con.executeQuery("delete from SubTasks where TaskMasterId='" + id + "'");
                con.executeQuery("delete from MasterOrderTaskTemplate where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }


        [HttpPost, Authorize]
        public ActionResult DeleteMasterOrderTaskTemplate(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || id == "null")
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from MasterOrderTaskTemplateSubTasks where MasterOrderTaskTemplateId='" + id + "'");
                con.executeQuery("delete from MasterOrderTaskTemplateDependency where TaskTemplateId='" + id + "'");
                con.executeQuery("delete from dbo.TNATasks where TaskTemplateId='" + id + "'");
                con.executeQuery("delete from MasterOrderTaskTemplate where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost, Authorize]
        public ActionResult DeleteSubTask(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || id == "null")
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from MasterOrderTaskTemplateSubTasks where Id='" + id + "'");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost, Authorize]
        public ActionResult DeleteMasterOrderTaskTemplateDependency(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || id == "null")
                    throw new Exception("Select entry first");

                DataSet dsdep;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MasterOrderTaskTemplateDependency where Id='" + id + "'", out dsdep, false, "1");

                string templateid = dsdep.Tables[0].Rows[0]["TaskTemplateId"].ToString();
                dsdep.Tables[0].Rows[0].Delete();

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsdep);

                updateDependency(templateid);

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                string _Task = "SELECT  TT.*,EI.EmployeeCode,EI.EmployeeName FROM MasterOrderTaskTemplate TT left outer join employeeInformation EI on ei.SystemID=TT.EmployeeId where id='" + Id + "'";

                var _master = _sqlRepository.GetDataCollection(_Task);
                //var _subtasks = _sqlRepository.GetDataCollection("select * from SubTasks where TaskMasterId='" + Id + "'");
                //var _plants = _sqlRepository.GetDataCollection("select * from TaskMasterPlantAssignment where TaskMasterId='" + Id + "'");


                //return Json(new { master = _master, subtasks = _subtasks, plants = _plants }, JsonRequestBehavior.AllowGet);
                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


    }
}