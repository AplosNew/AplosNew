using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Security.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskCloserMasterController : Controller
    {
        private readonly ISqlRepository _sqlRepository;
        public TaskCloserMasterController(SqlRepository R)
        {
            _sqlRepository = R;
        }

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetOpenTask(string fromdate, string todate, string tasktype)
        {
            try
            {
                string str = "";
                if(tasktype == "Task")
                { 
                    if(String.IsNullOrEmpty(fromdate) && String.IsNullOrEmpty(todate))
                    {
                        str = @"select distinct TM.Id, --creater.ResponsiblePersonId as 'AssignedBy_Id', 
                            creater.employeename as 'AssignedBy', TM.TaskTypeGroup TaskType,
                            TM.Taskdescription as Task, TM.TaskDetailDescription as TaskDetail,TM.CurrentStatus as TaskStatus,
                            --AssignMaster.ResponsiblePersonId as 'AssignTo_Id',
                            AssignMaster.employeename as 'AssignedTo',
                            --CheckMaster.ResponsiblePersonId as 'Checkby_Id',
                            CheckMaster.employeename as CheckBy,
                            --Crosscheck.ResponsiblePersonId as 'Crosscheck_Code', 
                            Crosscheck.employeename as CrossCheckBy,
                            --Approve.ResponsiblePersonId as 'ApproveBy_Code', 
                            Approve.employeename as ApproveBy
                            ,TC.CommentText as Comment, TC.AddedBy as CommentBy,  
                            TC.AddedDate as CommentDate
                            ,tm.AddedDate AssignedDate,AssignMaster.DueDate,AssignMaster.commitmentdate, TM.ClosingDate
 
                            from TaskManagerMaster TM
                            left join TaskComments TC on TC.TaskmanagerMasterId = TM.Id
                            left join employeeinformation emp on emp.SystemId = tc.Addedby
                            left join TaskManagerSubTasks TST on TST.TaskmanagerMasterId = TM.Id


                            left join (
                            select ta.duedate, ta.ResponsiblePersonId,ta.TaskManagerMasterId, e.employeename
                              from  TaskAudit TA 
                              left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                              where ta.authorizationtype='CreatedBy'
                            )as Creater on creater.TaskManagerMasterId=TM.ID

                            left join (
                            select ta.ResponsiblePersonId,ta.TaskManagerMasterId, e.employeename
                              from  TaskAudit TA 
                              left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                              where ta.authorizationtype='CheckBy'
                            )as CheckMaster on CheckMaster.TaskManagerMasterId=TM.ID

                            left join (
                            select ta.ResponsiblePersonId,ta.TaskManagerMasterId, e.employeename
                              from  TaskAudit TA 
                              left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                              where ta.authorizationtype='ApproveBy'
                            )as Approve on Approve.TaskManagerMasterId=TM.ID

                            left join (
                            select ta.ResponsiblePersonId,ta.TaskManagerMasterId, e.employeename
                              from  TaskAudit TA 
                              left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                              where ta.authorizationtype='CrossCheckBy'
                            )as Crosscheck on Crosscheck.TaskManagerMasterId=TM.ID

                            left join (
                            select ta.DueDate,ta.ResponsiblePersonId,ta.commitmentdate, ta.TaskManagerMasterId,e.employeename
                              from  TaskAudit TA left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                               where ta.authorizationtype='AssignTo'
                            )as AssignMaster on AssignMaster.TaskManagerMasterId=TM.ID

                            where TM.CurrentStatus <> 'Closed' 
                            ";
                    }
                    else
                    {
                        str = @"select distinct TM.Id, --creater.ResponsiblePersonId as 'AssignedBy_Id', 
                            creater.employeename as 'AssignedBy', TM.TaskTypeGroup TaskType,
                            TM.Taskdescription as Task, TM.TaskDetailDescription as TaskDetail,TM.CurrentStatus as TaskStatus,
                            --AssignMaster.ResponsiblePersonId as 'AssignTo_Id',
                            AssignMaster.employeename as 'AssignedTo',
                            --CheckMaster.ResponsiblePersonId as 'Checkby_Id',
                            CheckMaster.employeename as CheckBy,
                            --Crosscheck.ResponsiblePersonId as 'Crosscheck_Code', 
                            Crosscheck.employeename as CrossCheckBy,
                            --Approve.ResponsiblePersonId as 'ApproveBy_Code', 
                            Approve.employeename as ApproveBy
                            ,TC.CommentText as Comment, TC.AddedBy as CommentBy,  
                            TC.AddedDate as CommentDate
                            ,tm.AddedDate AssignedDate,AssignMaster.DueDate,AssignMaster.commitmentdate, TM.ClosingDate
 
                            from TaskManagerMaster TM
                            left join TaskComments TC on TC.TaskmanagerMasterId = TM.Id
                            left join employeeinformation emp on emp.SystemId = tc.Addedby
                            left join TaskManagerSubTasks TST on TST.TaskmanagerMasterId = TM.Id


                            left join (
                            select ta.duedate, ta.ResponsiblePersonId,ta.TaskManagerMasterId, e.employeename
                              from  TaskAudit TA 
                              left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                              where ta.authorizationtype='CreatedBy'
                            )as Creater on creater.TaskManagerMasterId=TM.ID

                            left join (
                            select ta.ResponsiblePersonId,ta.TaskManagerMasterId, e.employeename
                              from  TaskAudit TA 
                              left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                              where ta.authorizationtype='CheckBy'
                            )as CheckMaster on CheckMaster.TaskManagerMasterId=TM.ID

                            left join (
                            select ta.ResponsiblePersonId,ta.TaskManagerMasterId, e.employeename
                              from  TaskAudit TA 
                              left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                              where ta.authorizationtype='ApproveBy'
                            )as Approve on Approve.TaskManagerMasterId=TM.ID

                            left join (
                            select ta.ResponsiblePersonId,ta.TaskManagerMasterId, e.employeename
                              from  TaskAudit TA 
                              left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                              where ta.authorizationtype='CrossCheckBy'
                            )as Crosscheck on Crosscheck.TaskManagerMasterId=TM.ID

                            left join (
                            select ta.DueDate,ta.ResponsiblePersonId,ta.commitmentdate, ta.TaskManagerMasterId,e.employeename
                              from  TaskAudit TA left join employeeinformation e on e.systemid=ta.ResponsiblePersonId
                               where ta.authorizationtype='AssignTo'
                            )as AssignMaster on AssignMaster.TaskManagerMasterId=TM.ID

                            where TM.CurrentStatus <> 'Closed' AND AssignMaster.DueDate between '" + fromdate+ " 00:00:59' and  '" + todate+ " 12:00:00'";


                    }
                }
                else if (tasktype == "Issue") {
                    if (String.IsNullOrEmpty(fromdate) && String.IsNullOrEmpty(todate))
                    {
                        str = @"select IT.Id, IT.Issue, IT.IssueDetail, IT.IssueType, IT.FinalStatus,FORMAT(IT.IssueDate, 'dd-MMM-yyy') IssueDate, FORMAT(IT.CloseDate, 'dd-MMM-yyy')DueDate ,ABI.EmployeeName AssignBy , ATI.EmployeeName AssignTo ,IT.ObservedBy
                            FROM IssueTransaction IT 
                            LEFT JOIN EmployeeInformation ABI on ABI.SystemId = IT.AssignById
                            left join EmployeeInformation ATI on ATI.SystemId = IT.AssignToId
                            WHERE IT.FinalStatus <> 'ToClose' ";
                    }
                    else {
                        str = @"select IT.Id, IT.Issue, IT.IssueDetail, IT.IssueType, IT.FinalStatus,FORMAT(IT.IssueDate, 'dd-MMM-yyy') IssueDate, FORMAT(IT.CloseDate, 'dd-MMM-yyy')DueDate ,ABI.EmployeeName AssignBy , ATI.EmployeeName AssignTo ,IT.ObservedBy
                            FROM IssueTransaction IT 
                            LEFT JOIN EmployeeInformation ABI on ABI.SystemId = IT.AssignById
                            left join EmployeeInformation ATI on ATI.SystemId = IT.AssignToId
                            WHERE IT.FinalStatus <> 'ToClose' and IT.CloseDate between '" + fromdate + " 00:00:59' and  '" + todate + " 12:00:00'" ;
                    }
                        
                }

                    
                var data = _sqlRepository.GetDataCollection(str);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        #region Task Block
        
        public ActionResult CloseOpenTask(List<Dictionary<string, object>> chkBgtList)
        {
            try
            {
                var id = "";
                foreach (var item in chkBgtList)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableNameChildA = "TaskManagerMaster";


                DataSet dsChildA;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string _Id = "";
                #region CHILD 1
                con.OpenDataSetThroughAdapter("select * from " + TableNameChildA + " where Id In (" + id + ")", out dsChildA, false, "1");

                
                foreach (var item in chkBgtList)
                {
                    DataView dv = new DataView(dsChildA.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {

                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["CurrentStatus"] = "Closed";
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                       
                        dr.EndEdit();
                        
                       
                    }
                    


                }
                #endregion CHILD 1

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChildA);

                return Json(new { Data = chkBgtList, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Task Block

        #region Issue Block
        
        public ActionResult CloseOpenIssue(List<Dictionary<string, object>> chkIssueList)
        {
            try
            {
                var id = "";
                foreach (var item in chkIssueList)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableNameChildA = "IssueTransaction";


                DataSet dsChildA;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string _Id = "";
                #region CHILD 1
                con.OpenDataSetThroughAdapter("select * from " + TableNameChildA + " where Id In (" + id + ")", out dsChildA, false, "1");


                foreach (var item in chkIssueList)
                {
                    DataView dv = new DataView(dsChildA.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {

                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["FinalStatus"] = "ToClose";
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();


                    }



                }
                #endregion CHILD 1

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChildA);

                return Json(new { Data = chkIssueList, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Issue Block

    }

}