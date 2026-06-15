#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.IssueTracker;
using Library.Model.Setups;
using Library.Model.TaskManagement;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.IssueTracker;
using Library.Service.Setups;
using Library.Service.TaskManagement;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskListController : BaseController
    {
        #region Constructor
        string TableName = "HKP.TaskCategory";
        private readonly ISqlRepository _sqlRepository;

        public TaskListController(
            ISqlRepository R)
        {
            _sqlRepository = R;

        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {

            //Library.Service.TaskScheduler.TaskScheduler schedule = new Library.Service.TaskScheduler.TaskScheduler(_sqlRepository);
            //schedule.ProcessAllPendingSchedule();
            //Library.Planning.OrderManagement.MasterOrder schedule = new Library.Planning.OrderManagement.MasterOrder();
            //schedule.RunTNASchedule();

            //Library.Service.Productions.ProductionBooking.ProductionServices scheduler = new Library.Service.Productions.ProductionBooking.ProductionServices(_sqlRepository);
            //scheduler.UpdateDailyTarget(DateTime.Now.ToString("dd-MMM-yyyy"), dtPlant.Rows[i]["Id"].ToString());


            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetUser()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,ei.EmpPicPath,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType,
                            isnull(D.UserName,'') Designation,DEPT.UserName Department,
                            cp.Chat AS UnreadChat,FORMAT( cp.DateCreated,'dd-MMM-yyyy hh:mm:ss tt') AS UnreadChatDateCreated,
                            CASE WHEN ISNULL(cp.Id,'')='' THEN 0 ELSE 1 END AS UnreadChatCount

                              FROM EmployeeInformation AS ei 
                            INNER JOIN org.Position AS p ON p.Id=MB.PositionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=ei.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON P.DepartmentId=DEPT.Id
                            
                            LEFT OUTER JOIN chat AS cp ON cp.EmployeeId=ei.SystemId AND cp.Id=( 
                            	       SELECT TOP 1 c.Id FROM ChatMaster AS cm
                                    INNER JOIN ChatParticipants AS cp ON cm.Id=cp.ChatMasterId AND cp.EmployeeId='" + identity.EmployeeId + @"'  AND ISNULL(cp.IsRead,0)=0
                                    INNER JOIN Chat AS c ON c.ChatMasterId=cm.Id AND c.EmployeeId=ei.SystemId
                                    WHERE 
                                   
                                    isnull(cm.IsGroupChat,0)=0 
                                    AND (cm.FromId='" + identity.EmployeeId + @"' OR cm.ToId='" + identity.EmployeeId + @"')
                                    AND (cm.FromId=ei.SystemId OR cm.ToId=ei.SystemId)
                                    ORDER BY c.DateCreated DESC
                            )

                            WHERE ISNULL(p.TaskManagementApplicable,0)=1 --AND ei.PlantId=(SELECT plantid FROM EmployeeInformation AS e WHERE e.SystemId='" + identity.EmployeeId + @"')
                            AND ei.EmployeeStatus='active' and systemid<>'" + identity.EmployeeId + @"' order by employeename

                       ";

            string sqlLogin = @"SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,ei.EmpPicPath,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType
                                FROM EmployeeInformation AS ei
                                WHERE systemid='" + identity.EmployeeId + @"' order by employeename";

            var _loginUser = _sqlRepository.GetDataCollection(sqlLogin);
            if (_loginUser[0]["EmpType"].ToString().ToUpper() == "GUEST")
            {
                sql = @"SELECT * FROM (SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,ei.EmpPicPath,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType,
                            isnull(D.UserName,'') Designation,DEPT.UserName Department
                              FROM EmployeeInformation AS ei 
                            INNER JOIN MST.ManpowerBudget MB ON MB.Id=EI.BudgetCode
                            INNER JOIN org.Position AS p ON p.Id=MB.PositionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=ei.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON P.DepartmentId=DEPT.Id
                            WHERE ISNULL(p.TaskManagementApplicable,0)=1 AND ei.GroupId=(SELECT GroupId FROM EmployeeInformation AS e WHERE e.SystemId='" + identity.EmployeeId + @"')
                            AND ei.EmployeeStatus='active' 

                            UNION
                            
                            SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,ei.EmpPicPath,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType,
                            isnull(D.UserName,'') Designation,DEPT.UserName Department
                              FROM EmployeeInformation AS ei 
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=ei.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON ei.DepartmentId=DEPT.Id
                            WHERE  isnull(empType,'')='Guest' AND ei.EmployeeStatus='active' and systemid<>'" + identity.EmployeeId + @"') AS TEMP 

                        LEFT OUTER JOIN chat AS cp ON cp.EmployeeId=TEMP.Id AND cp.Id=( 
                            	          SELECT TOP 1 c.Id FROM ChatMaster AS cm
                                    INNER JOIN ChatParticipants AS cp ON cm.Id=cp.ChatMasterId AND cp.EmployeeId='" + identity.EmployeeId + @"'  AND ISNULL(cp.IsRead,0)=0
                                    INNER JOIN Chat AS c ON c.ChatMasterId=cm.Id AND c.EmployeeId=TEMP.Id
                                    WHERE 
                                   
                                    isnull(cm.IsGroupChat,0)=0 
                                    AND (cm.FromId='" + identity.EmployeeId + @"' OR cm.ToId='" + identity.EmployeeId + @"')
                                    AND (cm.FromId=TEMP.Id OR cm.ToId=TEMP.Id)
                                    ORDER BY c.DateCreated DESC
                            )

                            ORDER BY EmployeeCode";


            }
            else
            {
                sql = @"SELECT * FROM (SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,ei.EmpPicPath,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType,
                            isnull(D.UserName,'') Designation,DEPT.UserName Department
                            FROM EmployeeInformation AS ei 
                            INNER JOIN MST.ManpowerBudget MB ON MB.Id=EI.BudgetCode
                            INNER JOIN org.Position AS p ON p.Id=MB.PositionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=ei.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON P.DepartmentId=DEPT.Id
                            WHERE ISNULL(p.TaskManagementApplicable,0)=1 --AND ei.PlantId=(SELECT plantid FROM EmployeeInformation AS e WHERE e.SystemId='" + identity.EmployeeId + @"')
                            AND ei.EmployeeStatus='active' and systemid<>'" + identity.EmployeeId + @"'

                            UNION
                            
                            SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,ei.EmpPicPath,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType,
                            isnull(D.UserName,'') Designation,DEPT.UserName Department
                              FROM EmployeeInformation AS ei 
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=ei.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON ei.DepartmentId=DEPT.Id
                            WHERE  isnull(empType,'')='Guest' AND ei.EmployeeStatus='active' and systemid<>'" + identity.EmployeeId + @"') AS TEMP 

                        LEFT OUTER JOIN chat AS cp ON cp.EmployeeId=TEMP.Id AND cp.Id=( 
                            	          SELECT TOP 1 c.Id FROM ChatMaster AS cm
                                    INNER JOIN ChatParticipants AS cp ON cm.Id=cp.ChatMasterId AND cp.EmployeeId='" + identity.EmployeeId + @"'  AND ISNULL(cp.IsRead,0)=0
                                    INNER JOIN Chat AS c ON c.ChatMasterId=cm.Id AND c.EmployeeId=TEMP.Id
                                    WHERE 
                                   
                                    isnull(cm.IsGroupChat,0)=0 
                                    AND (cm.FromId='" + identity.EmployeeId + @"' OR cm.ToId='" + identity.EmployeeId + @"')
                                    AND (cm.FromId=TEMP.Id OR cm.ToId=TEMP.Id)
                                    ORDER BY c.DateCreated DESC
                            )

                            ORDER BY EmployeeCode";


            }

            return Json(new { Id = identity.EmployeeId, LoginUser = _loginUser, UserList = _sqlRepository.GetDataCollection(sql) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetIssueDetail(string ToDoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT it.Issue, it.IssueDetail, it.IssueDate, it.IssueType,
                                cby.EmployeeCode AS AssignByEmployeeCode,cby.EmployeeName AssignByEmployeeName,cby.EmpPicPath AS AssignByEmpPicPath,
                                ATO.EmployeeCode AS AssignToEmployeeCode,ATO.EmployeeName AssignToEmployeeName,ATO.EmpPicPath AS AssignToEmpPicPath,

                                UPD.EmployeeCode AS UpdateEmployeeCode,UPD.EmployeeName UpdateEmployeeName,UPD.EmpPicPath AS UpdateEmpPicPath,
                                FUA.EmployeeCode AS FollowUpEmployeeCode,FUA.EmployeeName FollowUpEmployeeName,FUA.EmpPicPath AS FollowUpEmpPicPath,
                                IRA.EmployeeCode AS InternalEmployeeCode,IRA.EmployeeName InternalEmployeeName,IRA.EmpPicPath AS InternalEmpPicPath,
                                ERP.EmployeeCode AS ExternalEmployeeCode,ERP.EmployeeName ExternalEmployeeName,ERP.EmpPicPath AS ExternalEmpPicPath
                                  FROM IssueTransaction AS it
                                LEFT OUTER JOIN EmployeeInformation AS CBY ON cby.SystemId=it.AssignById
                                LEFT OUTER JOIN EmployeeInformation AS ATO ON ATO.SystemId=it.AssignToId
			
                                LEFT OUTER JOIN EmployeeInformation AS UPD ON UPD.SystemId=it.UpdateResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS FUA ON FUA.SystemId=it.FollowUpResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS IRA ON IRA.SystemId=it.InternalResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS ERP ON ERP.SystemId=it.ExternalResponsiblePersonId

                                WHERE it.Id=(SELECT tmm.IssueTransactionId
                                                      FROM TaskManagerMaster AS tmm WHERE tmm.Id='" + ToDoId + @"')";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    data["Id"] = "TC" + _Id;
                    AddNewRow(data, dr);


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }


        [Authorize, HttpGet]
        public ActionResult GetMasterDataForFilter()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            //var _plants = _sqlRepository.GetDataCollection("SELECT * FROM org.Plant AS p WHERE active=1 and p.CompanyGroupId='" + identity.CompanyGroupId + "' ORDER BY p.Sequence");
            //var _Process = _sqlRepository.GetDataCollection("SELECT * FROM hkp.process AS p WHERE active=1 and p.CompanyGroupId='" + identity.CompanyGroupId + "' ORDER BY p.Sequence");
            var _department = _sqlRepository.GetDataCollection("SELECT * FROM org.Department  AS p where active=1 ORDER BY p.Sequence");
            //var _taskAppliedOn = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskAppliedOn AS p ");
            //var _taskCategory = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskCategory AS p where active=1 AND FLAG='" + TaskCategoryFlagEnum.TNA.ToString() + "' ORDER BY p.Sequence");
            //var _taskSubCategory = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskSubCategory AS p where active=1 AND FLAG='" + TaskCategoryFlagEnum.TNA.ToString() + "' ORDER BY p.Sequence");


            return Json(new { Department = _department }, JsonRequestBehavior.AllowGet);


            //return Json(new { Plant = _plants, Process = _Process, Department = _department, TaskAppliedOn = _taskAppliedOn, TaskCategory = _taskCategory, TaskSubCategory = _taskSubCategory }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult SaveAssignToMeDetail(Dictionary<string, object> taskAuditNew)
        {
            //var taskAudit = new TaskAudit();
            bool isUpdateIssueSubTask = false;
            bool isUpdateIssueTransaction = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var employeeId = identity.EmployeeId;

            try
            {



                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where Id='" + taskAuditNew["Id"] + "'", out dsMaster, false, "1");

                if (taskAuditNew["DueDate"] == null)
                    throw new Exception("Due Date cannot be blank");

                if (taskAuditNew["CommitmentDate"] != null)
                {

                    //if (Convert.ToDateTime(taskAuditNew["CommitmentDate"].ToString()) < Convert.ToDateTime(System.DateTime.Now.ToString("dd-MMM-yyy")))
                    //    throw new Exception("Commitment Date cannot be earlier than system date");

                    if (Convert.ToDateTime(Convert.ToDateTime(taskAuditNew["CommitmentDate"].ToString()).ToString("dd-MMM-yyyy")) < Convert.ToDateTime(Convert.ToDateTime(taskAuditNew["DueDate"].ToString()).ToString("dd-MMM-yyyy")))
                        throw new Exception("Commitment Date cannot be earlier than due date");


                }
                if (taskAuditNew["RevisedCommitmentDate"] != null)
                {
                    if (taskAuditNew["CommitmentDate"] == null)
                        throw new Exception("Commitment Date cannot be blank");


                    //if (Convert.ToDateTime(taskAuditNew["RevisedCommitmentDate"].ToString()) < Convert.ToDateTime(System.DateTime.Now.ToString("dd-MMM-yyy"))
                    //    throw new Exception("Revised Commitment Date cannot be earlier than system date");

                    if (Convert.ToDateTime(Convert.ToDateTime(taskAuditNew["RevisedCommitmentDate"].ToString()).ToString("dd-MMM-yyyy")) < Convert.ToDateTime(Convert.ToDateTime(taskAuditNew["CommitmentDate"].ToString()).ToString("dd-MMM-yyyy")))
                        throw new Exception("Revised Commitment Date cannot be earlier than system date");

                }

                #region data update


                EditRow(dsMaster.Tables[0].Rows[0], taskAuditNew);



                #endregion data update

                // Save to Database 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { isUpdateTaskAudit = true, Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { isUpdateTaskAudit = false, Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public ActionResult saveStoryPoint(string StoryPoint, string Id)
        {
            try
            {



                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerMaster  where Id='" + Id + "'", out dsMaster, false, "1");


                dsMaster.Tables[0].Rows[0].BeginEdit();

                dsMaster.Tables[0].Rows[0]["StoryPoint"] = clsStaticInfo.dbl(StoryPoint);

                dsMaster.Tables[0].Rows[0].EndEdit();

                // Save to Database 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { isUpdateTaskAudit = true, Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { isUpdateTaskAudit = false, Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult UpdateToTaskAudit(Dictionary<string, object> taskAuditNew)
        {

            try
            {

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where Id='" + taskAuditNew["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    //bplib.clsGenID genid = new bplib.clsGenID();
                    //genid.GenID(TableName, out _Id);

                    //taskAuditNew["Id"] = "TC" + _Id;
                    //AddNewRow(dsMaster.Tables[0], taskAuditNew);
                }
                else
                {
                    _Id = taskAuditNew["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], taskAuditNew);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { isUpdateTaskAudit = true, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { isUpdateTaskAudit = false, Message = AplosMessage.Updated });
            }

        }

        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost, Authorize]
        public ActionResult DeleteAuthEmployee(string id, string authType)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete FROM TaskAudit WHERE TaskManagerMasterId='" + id + "' AND AuthorizationType='" + authType + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        private void AddNewRow(Dictionary<string, object> sourceData, DataRow dr)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


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


        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    if (item != "Id")
                        dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            try
            {
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;
            }
            catch (Exception)
            {


            }


            dr.EndEdit();
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName);
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpGet, Authorize]
        public ActionResult GetMenu(string taskstatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (taskstatus.ToUpper() == "CLOSED")
                    return Json(new { STAT = GetClosedStatisticsString(), EMPID = identity.EmployeeId }, JsonRequestBehavior.AllowGet);

                return Json(new { STAT = GetStatisticsString(), EMPID = identity.EmployeeId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskAccordingToRresponsiblePersonList(string authorizationType, string flag, string taskstatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                JsonResult jsondata = null;

                if (taskstatus.ToUpper() == "CLOSED")
                {
                    jsondata = Json(
                        new
                        {
                            DATA = GetClosedTaskAccordingToRresponsiblePersonListString(authorizationType, flag),
                            EMPID = identity.EmployeeId
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }
                else
                {
                    jsondata = Json(
                        new
                        {
                            DATA = GetTaskAccordingToRresponsiblePersonListString(authorizationType, flag),
                            EMPID = identity.EmployeeId
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }

                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private double StdHighestTaskPriority = 4.5;
        private List<Dictionary<string, object>> GetTaskAccordingToRresponsiblePersonListString(string authorizationType, string flag)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var logedInUser = identity.EmployeeId;
            string sql = "";

            string fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
            string ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
            sql = @"SELECT distinct ta.Id TaskAuditId, tmm.*,'' AS BuyerName,isnull(tsc.UserName,'') AS TaskCategory ,TSSC.UserName AS TaskSubCategory,'' AS SearchDataTemp
                                ,Tasto.EmpPicPath,NULL AS Auth,Tasto.DepartmentId,d.UserName AS Department,
                                
                                Tasto.EmployeeName AS AssignTo,Tasto.SystemId AS AssignToId,
                                AasBy.EmpPicPath AS EmpPicPathAssignBy,AasBy.EmployeeName AS CreatedBy,AasBy.SystemId AS CreatedById
                                ,isnull(tmm.TaskPriority,0)TaskPriority,FORMAT(ta.AddedDate,'dd-MMM-yyyy hh:mm tt') AS TaskAddedDate,
                                FORMAT( ISNULL(tTo.RevisedCommitmentDate,tTo.CommitmentDate),'dd-MMM-yyyy') AS CommitmentDate,
                                FORMAT( ISNULL(tTo.RevisedCommitmentDate,tTo.CommitmentDate),'dd-MMM-yyyy') AS CommitmentDateFilter,
                                    FORMAT(ta.DueDate,'dd-MMM-yyyy') AS DueDate,FORMAT(ta.DueDate,'dd-MMM-yyyy') AS DueDateFilter
                                ,ta.ResponsiblePersonId,ta.AuthorizationType,ta.Remarks,isnull(Ta.IsRead,0) AS IsRead

                                FROM [TaskManagerMaster] AS tmm
                                LEFT JOIN [IssueTransaction] itr on tmm.IssueTransactionId = itr.Id
                                --LEFT JOIN [HKP].[Buyer] AS b ON itr.BuyerId = b.Id
                                left JOIN  HKP.TaskCategory TSC ON TSC.ID=tmm.TaskCategoryId
                                left JOIN  HKP.TaskSubCategory TSSC ON TSSC.ID=tmm.TaskSubCategoryId
                              
                                LEFT JOIN [TaskAudit] ta ON ta.TaskManagerMasterId = tmm.Id
                                LEFT JOIN [TaskAudit] tTo ON tTo.TaskManagerMasterId = tmm.Id AND tto.AuthorizationType='" + AuthorizationTypeEnum.AssignTo.ToString() + @"'
                                LEFT JOIN [TaskAudit] tBy ON tBy.TaskManagerMasterId = tmm.Id AND tBy.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'


                                INNER JOIN [EmployeeInformation] Tasto ON Tasto.SystemId = tTo.ResponsiblePersonId  
                                INNER JOIN [EmployeeInformation] AasBy ON AasBy.SystemId = tBy.ResponsiblePersonId

                                LEFT OUTER JOIN org.Department AS d ON d.Id=Tasto.DepartmentId
                                INNER JOIN [EmployeeInformation] asto ON asto.SystemId = ta.ResponsiblePersonId 

";
            switch (flag)
            {
                case "Today":
                    sql += @" WHERE isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND CONVERT(DATE, ta.DueDate)='" + DateTime.Now.ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    break;
                case "ThisWeek":
                    sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  CONVERT(DATE, ta.DueDate) Between '" + DateTime.Now.AddDays(1).ToString("dd-MMM-yyyy")
                        + @"' AND '" + DateTime.Now.AddDays(8).ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    break;
                case "OverDue":
                    sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  CONVERT(DATE, ta.DueDate) < '" + DateTime.Now.ToString("dd-MMM-yyyy")
                        + @"' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    break;
                case "MyTasks":
                    sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND tmm.TaskType IN ('ToDo','TNA','Issue')"
                        + @" AND tTo.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + AuthorizationTypeEnum.AssignTo + "' ";
                    break;
                case "HighPriorityTasks":
                    sql += @" where  isnull(ta.isDone,0)=0 AND  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  isnull(tmm.TaskPriority,0)>= " + StdHighestTaskPriority.ToString()
                        + @" AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy
                        + @"' ORDER BY CONVERT(DATETIME,ta.DueDate) ASC,isnull(tmm.TaskPriority,0) DESC ";
                    break;
                default:
                    if (authorizationType == AuthorizationTypeEnum.AssignTo.ToString())
                        sql += @" AND  isnull(ta.isDone,0)=0 AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
							WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "'";
                    else
                    {
                        List<string> TaskTypes = new List<string>();
                        foreach (TaskTypeEnum str in Enum.GetValues(typeof(TaskTypeEnum)))
                            TaskTypes.Add(str.ToString());

                        if (TaskTypes.Contains(authorizationType))
                        {
                            sql += @" AND  isnull(ta.isDone,0)=0 AND tmm.TaskType='" + authorizationType + @"'
							WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "' AND ta.AuthorizationType = '" + AuthorizationTypeEnum.AssignTo.ToString() + "'";

                        }
                        else
                        {
                            if (authorizationType == AuthorizationTypeEnum.CreatedBy.ToString())
                            {
                                sql += @" AND  isnull(ta.isDone,0)=0 AND ta.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
							WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "'";

                            }
                            else
                            {
                                sql += @" AND  isnull(ta.isDone,0)=0 AND ta.AuthorizationType='" + authorizationType + @"'
							WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "' AND ta.AuthorizationType = '" + authorizationType + "'";
                            }
                        }
                    }

                    break;
            }
            //     switch (flag)
            //     {
            //         case "Today":
            //             sql += @" WHERE isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND CONVERT(DATE, ta.DueDate)='" + DateTime.Now.ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
            //             break;
            //         case "ThisWeek":
            //             sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  CONVERT(DATE, ta.DueDate) Between '" + DateTime.Now.AddDays(1).ToString("dd-MMM-yyyy")
            //                 + @"' AND '" + DateTime.Now.AddDays(8).ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
            //             break;
            //         case "OverDue":
            //             sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  CONVERT(DATE, ta.DueDate) < '" + DateTime.Now.ToString("dd-MMM-yyyy")
            //                 + @"' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
            //             break;
            //         case "MyTasks":
            //             sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND tmm.TaskType IN ('ToDo','TNA','Issue')"
            //                 + @" AND tTo.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + AuthorizationTypeEnum.AssignTo + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
            //             break;
            //         case "HighPriorityTasks":
            //             sql += @" where  isnull(ta.isDone,0)=0 AND  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  isnull(tmm.TaskPriority,0)>= " + StdHighestTaskPriority.ToString()
            //                 + @" AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy
            //                 + @"' ORDER BY CONVERT(DATETIME,ta.DueDate) ASC,isnull(tmm.TaskPriority,0) DESC ";
            //             break;
            //         default:
            //             if (authorizationType == AuthorizationTypeEnum.AssignTo.ToString())
            //                 sql += @" AND  isnull(ta.isDone,0)=0 AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
            //WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "'  order by CONVERT(DATETIME,ta.DueDate) ASC ";
            //             else
            //             {
            //                 List<string> TaskTypes = new List<string>();
            //                 foreach (TaskTypeEnum str in Enum.GetValues(typeof(TaskTypeEnum)))
            //                     TaskTypes.Add(str.ToString());

            //                 if (TaskTypes.Contains(authorizationType))
            //                 {
            //                     sql += @" AND  isnull(ta.isDone,0)=0 AND tmm.TaskType='" + authorizationType + @"'
            //WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "' AND ta.AuthorizationType = '" + AuthorizationTypeEnum.AssignTo.ToString() + "'  order by CONVERT(DATETIME,ta.DueDate) ASC ";

            //                 }
            //                 else
            //                 {
            //                     if (authorizationType == AuthorizationTypeEnum.CreatedBy.ToString())
            //                     {
            //                         sql += @" AND  isnull(ta.isDone,0)=0 AND ta.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
            //WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "'  order by CONVERT(DATETIME,ta.DueDate) ASC ";

            //                     }
            //                     else
            //                     {
            //                         sql += @" AND  isnull(ta.isDone,0)=0 AND ta.AuthorizationType='" + authorizationType + @"'
            //WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "' AND ta.AuthorizationType = '" + authorizationType + "'  order by CONVERT(DATETIME,ta.DueDate) ASC ";
            //                     }
            //                 }
            //             }

            //             break;
            //     }

            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql);

            sql = @"SELECT K.*,EI.EmployeeName FROM (
                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id AuditId,isnull(ta.isDone,0) AS isDone,ta.ResponsiblePersonId
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.CheckBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE tm.CurrentStatus<>'" + CurrentStatusEnum.Closed.ToString() + @"'

                        UNION ALL

                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id,isnull(ta.isDone,0) AS isDone,ta.ResponsiblePersonId
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.CrossCheckBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE tm.CurrentStatus<>'" + CurrentStatusEnum.Closed.ToString() + @"'

                        UNION ALL

                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id,isnull(ta.isDone,0) AS isDone,ta.ResponsiblePersonId
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.ApproveBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE tm.CurrentStatus<>'" + CurrentStatusEnum.Closed.ToString() + @"'
                        ) AS K

                        left outer join EmployeeInformation EI on EI.SystemId=K.ResponsiblePersonId
                        WHERE k.TaskManagerMasterId IN (SELECT ta.TaskManagerMasterId
                                                          FROM TaskAudit AS ta WHERE ta.ResponsiblePersonId='" + identity.EmployeeId + @"')
                         ORDER BY K.TaskManagerMasterId,K.AuthType ";

            List<Dictionary<string, object>> Authdata = _sqlRepository.GetDataCollection(sql);
            foreach (Dictionary<string, object> item in data)
            {
                try
                {
                    item["Auth"] = Authdata.Where(ee => ee["TaskManagerMasterId"].ToString() == item["Id"].ToString());
                }
                catch (Exception)
                {


                }
            }

            return data;


        }
        private List<Dictionary<string, object>> GetStatisticsString()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var logedInUser = identity.EmployeeId;

            string fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
            string ToDate = DateTime.Now.ToString("dd-MMM-yyyy");


            string NextWeekfromDate = DateTime.Now.AddDays(1).ToString("dd-MMM-yyyy");
            string NextWeekToDate = DateTime.Now.AddDays(8).ToString("dd-MMM-yyyy");


            string sql = @"SELECT 'Home' AS TaskType,COUNT(*) AS NoOfTasks,0 AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0 AND ta.AuthorizationType='CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'

                                UNION
                                SELECT 'Assigned To Me' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'

                                UNION
                                SELECT 'MyTasks' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ATT.isRead,0)=0 THEN CASE WHEN ((CRB.ResponsiblePersonId=ATT.ResponsiblePersonId) OR CRB.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  
                                FROM TaskManagerMaster AS tmm
                                LEFT JOIN TaskAudit AS CRB ON CRB.TaskManagerMasterId=tmm.Id AND CRB.AuthorizationType='CreatedBy'
                                INNER JOIN TaskAudit AS ATT ON ATT.TaskManagerMasterId=tmm.Id AND ATT.AuthorizationType='AssignTo'
                                WHERE tmm.CurrentStatus<>'Closed'  AND tmm.TaskType IN ('ToDo','TNA','Issue') 
                                AND isnull(ATT.isDone,0)=0  AND ATT.ResponsiblePersonId='" + logedInUser + @"'

                               -- UNION
                               -- SELECT 'HighPriorityTasks' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                               -- INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                               -- LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                               -- WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND tmm.TaskPriority>=" + StdHighestTaskPriority.ToString() + @" AND ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                               --
                               -- UNION
                               -- SELECT 'OverDue' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                               -- INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                               -- LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                               -- WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND CONVERT(DATE,ta.DueDate)<'" + fromDate + @"' AND ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                               --
                               -- UNION
                               -- SELECT 'Today' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                               -- INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                               -- LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                               -- WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND CONVERT(DATE,ta.DueDate)='" + fromDate + @"' AND ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                               --
                               -- UNION
                               -- SELECT 'ThisWeek' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                               -- INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                               -- LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                               -- WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND CONVERT(DATE,ta.DueDate) BETWEEN '" + NextWeekfromDate + @"' AND '" + NextWeekToDate + @"' AND ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'

                                UNION
                                SELECT 'UpdateAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND tmm.TaskType='UpdateAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION
                                SELECT 'FollowUpAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND tmm.TaskType='FollowUpAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION
                                SELECT 'InternalAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND tmm.TaskType='InternalAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION
                                SELECT 'ExternalAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND tmm.TaskType='ExternalAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'

                                UNION
                                SELECT 'CheckBy' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND ta.AuthorizationType='CheckBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'


                                UNION
                                SELECT 'CrossCheckBy' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND ta.AuthorizationType='CrossCheckBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'


                                UNION
                                SELECT 'ApproveBy' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE tmm.CurrentStatus<>'Closed' AND isnull(ta.isDone,0)=0  AND ta.AuthorizationType='ApproveBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'";







            return _sqlRepository.GetDataCollection(sql);


        }


        private List<Dictionary<string, object>> GetClosedTaskAccordingToRresponsiblePersonListString(string authorizationType, string flag)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var logedInUser = identity.EmployeeId;
            string sql = "";

            string fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
            string ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
            sql = @"SELECT ISNULL(ta.isDone,0) AS isDone,tmm.*,'' AS BuyerName,isnull(tsc.UserName,'') AS TaskCategory ,TSSC.UserName AS TaskSubCategory,'' AS SearchDataTemp
                                ,Tasto.EmpPicPath,NULL AS Auth,Tasto.DepartmentId,d.UserName AS Department,
                                
                                Tasto.EmployeeName AS AssignTo,Tasto.SystemId AS AssignToId,
                                AasBy.EmpPicPath AS EmpPicPathAssignBy,AasBy.EmployeeName AS CreatedBy,AasBy.SystemId AS CreatedById,ta.Id AS TaskAuditId
                                ,isnull(tmm.TaskPriority,0)TaskPriority, FORMAT(ta.AddedDate,'dd-MMM-yyyy hh:mm tt') AS TaskAddedDate,
                                FORMAT( ISNULL(tTo.RevisedCommitmentDate,tTo.CommitmentDate),'dd-MMM-yyyy') AS CommitmentDate,
                                    FORMAT(ta.DueDate,'dd-MMM-yyyy') AS DueDate,FORMAT(ta.DueDate,'dd-MMM-yyyy') AS DueDateFilter
                                ,ta.ResponsiblePersonId,ta.AuthorizationType,ta.Remarks,isnull(Ta.IsRead,0) AS IsRead

                                FROM [TaskManagerMaster] AS tmm
                                LEFT JOIN [IssueTransaction] itr on tmm.IssueTransactionId = itr.Id
                                --LEFT JOIN [HKP].[Buyer] AS b ON itr.BuyerId = b.Id
                                left JOIN  HKP.TaskCategory TSC ON TSC.ID=tmm.TaskCategoryId
                                left JOIN  HKP.TaskSubCategory TSSC ON TSSC.ID=tmm.TaskSubCategoryId
                              
                                LEFT JOIN [TaskAudit] ta ON ta.TaskManagerMasterId = tmm.Id
                                LEFT JOIN [TaskAudit] tTo ON tTo.TaskManagerMasterId = tmm.Id AND tto.AuthorizationType='" + AuthorizationTypeEnum.AssignTo.ToString() + @"'
                                LEFT JOIN [TaskAudit] tBy ON tBy.TaskManagerMasterId = tmm.Id AND tBy.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'

                                INNER JOIN [EmployeeInformation] Tasto ON Tasto.SystemId = tTo.ResponsiblePersonId  
                                INNER JOIN [EmployeeInformation] AasBy ON AasBy.SystemId = tBy.ResponsiblePersonId
                                LEFT OUTER JOIN org.Department AS d ON d.Id=Tasto.DepartmentId

                                INNER JOIN [EmployeeInformation] asto ON asto.SystemId = ta.ResponsiblePersonId                

";
            switch (flag)
            {
                case "Today":
                    sql += @" WHERE (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND CONVERT(DATE, ta.DueDate)='" + DateTime.Now.ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by tmm.AddedDate ASC ";
                    break;
                case "ThisWeek":
                    sql += @" WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND  CONVERT(DATE, ta.DueDate) Between '" + DateTime.Now.AddDays(1).ToString("dd-MMM-yyyy")
                        + @"' AND '" + DateTime.Now.AddDays(8).ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by tmm.AddedDate ASC ";
                    break;
                case "OverDue":
                    sql += @" WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND  CONVERT(DATE, ta.DueDate)>= CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND  CONVERT(DATE, ta.DueDate) < '" + DateTime.Now.ToString("dd-MMM-yyyy")
                        + @"' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by tmm.AddedDate ASC ";
                    break;
                case "MyTasks":
                    sql += @" WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND tmm.TaskType IN ('ToDo','TNA','Issue')"
                        + @" AND tTo.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + AuthorizationTypeEnum.AssignTo + "' order by tmm.AddedDate ASC ";
                    break;
                case "HighPriorityTasks":
                    sql += @" where  (isnull(ta.isDone,0)=1 OR  tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND isnull(tmm.TaskPriority,0)>= " + StdHighestTaskPriority.ToString()
                        + @" AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy
                        + @"' ORDER BY tmm.AddedDate DESC,isnull(tmm.TaskPriority,0) DESC ";
                    break;
                default:
                    if (authorizationType == AuthorizationTypeEnum.AssignTo.ToString())
                        sql += @" AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
							WHERE  ( isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "')  AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate())) AND  ta.ResponsiblePersonId ='" + logedInUser + "'  order by tmm.AddedDate ASC ";
                    else
                    {
                        List<string> TaskTypes = new List<string>();
                        foreach (TaskTypeEnum str in Enum.GetValues(typeof(TaskTypeEnum)))
                            TaskTypes.Add(str.ToString());

                        if (TaskTypes.Contains(authorizationType))
                        {
                            sql += @" AND   tmm.TaskType='" + authorizationType + @"'
							WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "')  AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate())) AND  ta.ResponsiblePersonId ='" + logedInUser + "' AND ta.AuthorizationType = '" + AuthorizationTypeEnum.AssignTo.ToString() + "'  order by tmm.AddedDate ASC ";

                        }
                        else
                        {
                            if (authorizationType == AuthorizationTypeEnum.CreatedBy.ToString())
                            {
                                sql += @" AND  ta.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
							WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  ta.ResponsiblePersonId ='" + logedInUser + "'  order by tmm.AddedDate DESC ";

                            }
                            else
                            {
                                sql += @" AND  ta.AuthorizationType='" + authorizationType + @"'
							WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  ta.ResponsiblePersonId ='" + logedInUser + "' AND ta.AuthorizationType = '" + authorizationType + "'  order by tmm.AddedDate ASC ";
                            }
                        }
                    }

                    break;
            }

            //sql += " ORDER BY CONVERT(DATETIME,ta.DueDate) ASC ";
            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql);

            sql = @"SELECT k.*,EI.EmployeeName FROM (
                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id AuditId,isnull(ta.isDone,0) AS isDone,ta.ResponsiblePersonId
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.CheckBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE isnull(ta.IsDone,0)=1  AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate())) 

                        UNION ALL

                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id,isnull(ta.isDone,0) AS isDone,ta.ResponsiblePersonId
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.CrossCheckBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE isnull(ta.IsDone,0)=1  AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate())) 

                        UNION ALL

                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id,isnull(ta.isDone,0) AS isDone,ta.ResponsiblePersonId
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.ApproveBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                       WHERE isnull(ta.IsDone,0)=1  AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate())) 
                        ) AS K
                        left outer join EmployeeInformation EI on EI.SystemId=K.ResponsiblePersonId
                        WHERE k.TaskManagerMasterId IN (SELECT ta.TaskManagerMasterId
                                                          FROM TaskAudit AS ta WHERE ta.ResponsiblePersonId='" + identity.EmployeeId + @"')
                         ORDER BY K.TaskManagerMasterId,K.AuthType ";

            List<Dictionary<string, object>> Authdata = _sqlRepository.GetDataCollection(sql);
            foreach (Dictionary<string, object> item in data)
            {
                try
                {
                    item["Auth"] = Authdata.Where(ee => ee["TaskManagerMasterId"].ToString() == item["Id"].ToString());
                }
                catch (Exception)
                {


                }
            }

            return data;


        }
        private List<Dictionary<string, object>> GetClosedStatisticsString()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var logedInUser = identity.EmployeeId;

            string fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
            string ToDate = DateTime.Now.ToString("dd-MMM-yyyy");


            string NextWeekfromDate = DateTime.Now.AddDays(1).ToString("dd-MMM-yyyy");
            string NextWeekToDate = DateTime.Now.AddDays(8).ToString("dd-MMM-yyyy");


            string sql = @"SELECT 'Home' AS TaskType,COUNT(*) AS NoOfTasks,0 AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND ta.AuthorizationType='CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'

                                UNION
                                SELECT 'Assigned To Me' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION
                                SELECT 'MyTasks' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ATT.isRead,0)=0 THEN CASE WHEN ((CRB.ResponsiblePersonId=ATT.ResponsiblePersonId) OR CRB.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  
                                FROM TaskManagerMaster AS tmm
                                LEFT JOIN TaskAudit AS CRB ON CRB.TaskManagerMasterId=tmm.Id AND CRB.AuthorizationType='CreatedBy'
                                INNER JOIN TaskAudit AS ATT ON ATT.TaskManagerMasterId=tmm.Id AND ATT.AuthorizationType='AssignTo'
                                WHERE (tmm.CurrentStatus='Closed' OR isnull(ATT.isDone,0)=1)  AND CONVERT(DATE, CRB.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND tmm.TaskType IN ('ToDo','TNA','Issue') 
                                AND ATT.ResponsiblePersonId='" + logedInUser + @"' 

                                --UNION
                                --SELECT 'HighPriorityTasks' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                --INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                --LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                -- WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  isnull(ta.isDone,0)=0  AND tmm.TaskPriority>=" + StdHighestTaskPriority.ToString() + @" AND ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                --
                                --UNION
                                --SELECT 'OverDue' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                --INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                --LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                -- WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  isnull(ta.isDone,0)=0  AND CONVERT(DATE,ta.DueDate)<'" + fromDate + @"' AND ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                --
                                --UNION
                                --SELECT 'Today' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                --INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                --LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                -- WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  CONVERT(DATE,ta.DueDate)='" + fromDate + @"' AND ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                --
                                --UNION
                                --SELECT 'ThisWeek' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                --INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                --LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                -- WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  CONVERT(DATE,ta.DueDate) BETWEEN '" + NextWeekfromDate + @"' AND '" + NextWeekToDate + @"' AND ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'

                                UNION
                                SELECT 'UpdateAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  tmm.TaskType='UpdateAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION
                                SELECT 'FollowUpAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  tmm.TaskType='FollowUpAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION
                                SELECT 'InternalAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  tmm.TaskType='InternalAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION
                                SELECT 'ExternalAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  tmm.TaskType='ExternalAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'

                                UNION
                                SELECT 'CheckBy' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND ta.AuthorizationType='CheckBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'


                                UNION
                                SELECT 'CrossCheckBy' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  ta.AuthorizationType='CrossCheckBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'


                                UNION
                                SELECT 'ApproveBy' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  ta.AuthorizationType='ApproveBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'";







            return _sqlRepository.GetDataCollection(sql);


        }


        [HttpGet, Authorize]
        public ActionResult GetTaskManagerSubTasksByResponsiblePersonId(string taskManagerMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var logedInUser = identity.EmployeeId;

            try
            {
                var sql = @"SELECT * FROM [dbo].[TaskManagerSubTasks] WHERE (ResponsiblePersonId ='" + identity.EmployeeId + "' OR isnull(ResponsiblePersonId,'')='') AND TaskManagerMasterId = '" + taskManagerMasterId + "'";


                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSubTaskByTaskManagerMasterId(string taskManagerMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var logedInUser = identity.EmployeeId;

            try
            {
                var sql = @"SELECT convert(bit,CASE WHEN ISNULL(r.Id,'')='' THEN 0 ELSE 1 END) AS hasRemarks, t.* FROM [dbo].[TaskManagerSubTasks] T
                            LEFT OUTER JOIN TaskManagerSubTaskRemarks AS R ON r.TaskManagerSubTasksId=t.Id 
                            AND r.Id=(SELECT TOP 1 Id FROM TaskManagerSubTaskRemarks WHERE TaskManagerSubTasksId=t.Id) WHERE T.TaskManagerMasterId ='" + taskManagerMasterId + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize, HttpPost]
        public ActionResult SearchEmployee(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                      select top 100 * from (  
                        SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
						isnull(D.UserName,'') Designation,
      
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
   
                        WHERE ISNULL(PR.TaskManagementApplicable,0)=1 AND emp.EmployeeStatus='Active' 
                    AND emp.GroupID IN (select GroupID
                                                 from employeeinformation where systemid='" + identity.EmployeeId + @"')
                                               
                                                UNION 
                                                
                                                
                                                
                                              SELECT
                                                 Emp.SystemID AS Id,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                      isnull(D.UserName,'') Designation,
      
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,'' Plant
                              FROM EmployeeInformation AS EMP 
                             LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EMP.BudgetCode
						    LEFT JOIN ORG.Position PR ON PR.Id=MB.PositionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON EMP.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            WHERE  isnull(empType,'')='Guest'  AND EMP.EmployeeStatus='active' AND  emp.GroupID IN (select GroupID
                                                 from employeeinformation where systemid='" + identity.EmployeeId + @"')
                ) AS TEMP where " + strkey + " Order By Id";





            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetSingleEmployee(string Id)
        {

            string sql = @"SELECT * 
                            FROM EmployeeInformation EMP WHERE Systemid='" + Id + "'";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        #region TODO
        [HttpPost, Authorize]
        public ActionResult AddToDo(Dictionary<string, object> ToDo)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerMaster  where Id='" + ToDo["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region Task data update

                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("TO DO", out _Id);

                _Id = "TD" + _Id;
                ToDo["Id"] = _Id;


                DataRow dr = dsMaster.Tables[0].NewRow();
                AddNewRow(ToDo, dr);
                //  

                dr["TaskType"] = TaskTypeEnum.ToDo.ToString();
                dr["TaskTypeGroup"] = TaskTypeEnum.ToDo.ToString();
                dr["TaskDescription"] = ToDo["TaskDescription"];
                try
                {
                    dr["TaskDetailDescription"] = ToDo["TaskDetailDescription"];
                }
                catch (Exception)
                {
                }


                dr["CurrentStatus"] = CurrentStatusEnum.ToStart;
                dr["isOwnTask"] = true;


                dsMaster.Tables[0].Rows.Add(dr);


                #endregion data update


                #region task Authorizations
                string _childId = "";
                genid = new bplib.clsGenID();
                genid.GenID("TO DO AUTH", out _childId);

                DataSet dsAuth;
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where TaskManagerMasterId='" + _Id + "'", out dsAuth, false, "1");



                //Created By
                dr = dsAuth.Tables[0].NewRow();
                AddNewRow(ToDo, dr);

                dr["ID"] = "AU" + _childId;
                dr["TaskManagerMasterId"] = _Id;
                dr["AuthorizationType"] = AuthorizationTypeEnum.CreatedBy;
                dr["ResponsiblePersonId"] = identity.EmployeeId;

                dr["DueDate"] = System.DateTime.Now.ToString();
                dr["IsRead"] = true;


                dsAuth.Tables[0].Rows.Add(dr);

                //Assigned To
                dr = dsAuth.Tables[0].NewRow();
                AddNewRow(ToDo, dr);

                dr["ID"] = "AU" + _childId + "-1";
                dr["TaskManagerMasterId"] = _Id;
                dr["AuthorizationType"] = AuthorizationTypeEnum.AssignTo;
                dr["ResponsiblePersonId"] = identity.EmployeeId;

                dr["DueDate"] = System.DateTime.Now.ToString();
                dr["IsRead"] = true;



                dsAuth.Tables[0].Rows.Add(dr);

                #endregion task Authorizations


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsAuth);

                List<Dictionary<string, object>> _allData = GetTaskAccordingToRresponsiblePersonListString(AuthorizationTypeEnum.CreatedBy.ToString(), AuthorizationTypeEnum.CreatedBy.ToString());

                return Json(new
                {
                    Error = false,
                    TaskSingleData = _allData.Where(ee => ee["Id"].ToString() == _Id).ToList(),
                    TaskData = _allData,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult UpdateToDoSubTasks(Dictionary<string, object> ToDoSubTask)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerSubTasks  where Id='" + ToDoSubTask["Id"] + "'", out dsMaster, false, "1");



                #region Task data update


                EditRow(dsMaster.Tables[0].Rows[0], ToDoSubTask);


                #endregion data update




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        #endregion data update

        [HttpPost, Authorize]
        public ActionResult UpdateToDoAuth(Dictionary<string, object> ToDo, Dictionary<string, Dictionary<string, object>> ToDoEmployee, string TaskManagerMasterId)
        {

            try
            {



                try
                {
                    UpdateAuth(TaskManagerMasterId, ((Dictionary<string, object>)ToDoEmployee[AuthorizationTypeEnum.CreatedBy.ToString()])["EmployeeId"], ToDo["DueDate"].ToString(), AuthorizationTypeEnum.CreatedBy);
                }
                catch { }
                try { UpdateAuth(TaskManagerMasterId, ((Dictionary<string, object>)ToDoEmployee[AuthorizationTypeEnum.AssignTo.ToString()])["EmployeeId"], ToDo["DueDate"].ToString(), AuthorizationTypeEnum.AssignTo); } catch { }
                try
                { UpdateAuth(TaskManagerMasterId, ((Dictionary<string, object>)ToDoEmployee[AuthorizationTypeEnum.CheckBy.ToString()])["EmployeeId"], ToDo["DueDate"].ToString(), AuthorizationTypeEnum.CheckBy); }
                catch { }
                try
                { UpdateAuth(TaskManagerMasterId, ((Dictionary<string, object>)ToDoEmployee[AuthorizationTypeEnum.CrossCheckBy.ToString()])["EmployeeId"], ToDo["DueDate"].ToString(), AuthorizationTypeEnum.CrossCheckBy); }
                catch { }
                try
                { UpdateAuth(TaskManagerMasterId, ((Dictionary<string, object>)ToDoEmployee[AuthorizationTypeEnum.ApproveBy.ToString()])["EmployeeId"], ToDo["DueDate"].ToString(), AuthorizationTypeEnum.ApproveBy); }
                catch { }


                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager();
                con.BeginTransaction();
                con.executeQuery(@"UPDATE TaskManagerMaster SET isOwnTask = CASE WHEN isnull(k.NoOfPeople,0)>1 THEN 0 ELSE 1 END
                                    FROM TaskManagerMaster TM 
                                    INNER JOIN (
                                    SELECT ta.TaskManagerMasterId, COUNT(DISTINCT ta.ResponsiblePersonId) AS NoOfPeople
                                      FROM TaskAudit AS ta WHERE ta.TaskManagerMasterId='" + TaskManagerMasterId + @"'
                                    GROUP BY ta.TaskManagerMasterId) AS K ON tm.Id=k.TaskManagerMasterId
                                    WHERE tm.TaskTypeGroup='ToDo'");

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        private void UpdateAuth(string ToDoId, object EmployeeId, string DueDate, AuthorizationTypeEnum Auth)
        {
            try
            {

                if (EmployeeId == null)
                    return;


                DataTable dt = _sqlRepository.GetDataTable("select * from TaskAudit  where  AuthorizationType NOT IN ('" + AuthorizationTypeEnum.CreatedBy.ToString() + "','" + Auth.ToString() + @"') AND  ResponsiblePersonId='" + EmployeeId + "'  AND TaskManagerMasterId='" + ToDoId + "'");
                if (dt.Rows.Count > 0)
                    throw new Exception("Same employee has already been tagged for this task");


                DataSet dsAuth;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where  AuthorizationType='" + Auth.ToString() + "'  AND TaskManagerMasterId='" + ToDoId + "'", out dsAuth, false, "1");

                while (dsAuth.Tables[0].DefaultView.Count > 0)
                    dsAuth.Tables[0].DefaultView[0].Delete();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string _childId = "";
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("TO DO AUTH", out _childId);

                //Created By
                DataRow dr = dsAuth.Tables[0].NewRow();

                dr["ID"] = "AU" + _childId;
                dr["TaskManagerMasterId"] = ToDoId;
                dr["AuthorizationType"] = Auth;
                dr["ResponsiblePersonId"] = EmployeeId;

                dr["DueDate"] = clsStaticInfo.nullrecorder(DueDate);

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;
                dsAuth.Tables[0].Rows.Add(dr);


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsAuth);

            }
            catch (Exception)
            {


            }

        }

        [HttpPost, Authorize]
        public ActionResult UpdateToDoMaster(Dictionary<string, object> ToDo, string TaskManagerMasterId)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerMaster  where Id='" + TaskManagerMasterId + "'", out dsMaster, false, "1");



                #region Task data update

                DataRow dr = dsMaster.Tables[0].Rows[0];
                //dr.BeginEdit();
                //Id IssueTransactionId  TaskType TaskDescription CurrentStatus AddedBy AddedDate AddedFromIP UpdatedBy UpdatedDate UpdatedFromIP

                try
                {
                    EditRow(dr, ToDo);
                }
                catch (Exception)
                {


                }
                //dr["CurrentStatus"] = ToDo["CurrentStatus"].ToString();
                //dr["TaskDescription"] = ToDo["TaskDescription"].ToString();
                //dr["TaskCategoryId"] = ToDo["TaskCategoryId"].ToString();
                //if (ToDo["TaskPriority"] == null)
                //    dr["TaskPriority"] = 0;
                //else
                //    dr["TaskPriority"] = ToDo["TaskPriority"].ToString();



                //dr.EndEdit();


                DataSet dsAudits;
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TaskAudit AS ta WHERE ta.TaskManagerMasterId='" + TaskManagerMasterId + "'", out dsAudits, false, "1");
                foreach (DataRow item in dsAudits.Tables[0].Rows)
                {
                    item.BeginEdit();
                    item["DueDate"] = ToDo["DueDate"].ToString();

                    item.EndEdit();
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsAudits);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult UpdateToDoMasterStatus(bool closed, string authorizationtype, string TaskManagerMasterId)
        {

            //return null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerMaster  where Id='" + TaskManagerMasterId + "'", out dsMaster, false, "1");


                DataSet dsCreatedBy, dsApproveBy, dsAuthorization;
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where TaskManagerMasterId='" + TaskManagerMasterId + "' AND AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + "'", out dsCreatedBy, false, "1");
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where TaskManagerMasterId='" + TaskManagerMasterId + "' AND AuthorizationType='" + AuthorizationTypeEnum.ApproveBy.ToString() + "'", out dsApproveBy, false, "1");
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where TaskManagerMasterId='" + TaskManagerMasterId + "' AND AuthorizationType='" + authorizationtype + "'", out dsAuthorization, false, "1");



                DataRow dr = dsMaster.Tables[0].Rows[0];

                try
                {
                    dr.BeginEdit();
                    if (closed == true)
                    {
                        if (dsApproveBy.Tables[0].Rows.Count > 0)
                        {
                            if (authorizationtype == AuthorizationTypeEnum.ApproveBy.ToString())
                            {
                                dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                                dr["ClosingDate"] = System.DateTime.Now.ToString();
                                dr["ClosedBy"] = identity.Name;

                            }
                            else
                            {
                                dr["CurrentStatus"] = CurrentStatusEnum.ToClose.ToString();
                                if (authorizationtype == AuthorizationTypeEnum.CreatedBy.ToString())
                                {
                                    dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                                    dr["ClosingDate"] = System.DateTime.Now.ToString();
                                    dr["ClosedBy"] = identity.Name;

                                }
                                else if (authorizationtype == AuthorizationTypeEnum.AssignTo.ToString())
                                {
                                    if (dsCreatedBy.Tables[0].Rows[0]["ResponsiblePersonId"].ToString() == identity.EmployeeId)
                                    {
                                        dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                                        dr["ClosingDate"] = System.DateTime.Now.ToString();
                                        dr["ClosedBy"] = identity.Name;

                                    }
                                    else
                                    {
                                        dr["CurrentStatus"] = CurrentStatusEnum.ToClose.ToString();

                                        DataRow drAuth = dsAuthorization.Tables[0].Rows[0];
                                        drAuth.BeginEdit();
                                        drAuth["isDone"] = true;
                                        drAuth["UpdatedDate"] = System.DateTime.Now.ToString();
                                        drAuth.EndEdit();
                                    }
                                }
                                else
                                {

                                    DataRow drAuth = dsAuthorization.Tables[0].Rows[0];
                                    drAuth.BeginEdit();
                                    drAuth["isDone"] = true;
                                    drAuth["UpdatedDate"] = System.DateTime.Now.ToString();
                                    drAuth.EndEdit();
                                }

                            }

                        }
                        else
                        {

                            if (authorizationtype == AuthorizationTypeEnum.CreatedBy.ToString() || authorizationtype == AuthorizationTypeEnum.AssignTo.ToString())
                            {
                                dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                                dr["ClosingDate"] = System.DateTime.Now.ToString();
                                dr["ClosedBy"] = identity.Name;

                            }
                            else
                            {

                                DataRow drAuth = dsAuthorization.Tables[0].Rows[0];
                                drAuth.BeginEdit();
                                drAuth["isDone"] = true;
                                drAuth["UpdatedDate"] = System.DateTime.Now.ToString();
                                drAuth.EndEdit();
                            }

                        }
                    }
                    else
                    {

                        if (authorizationtype == AuthorizationTypeEnum.CreatedBy.ToString() || authorizationtype == AuthorizationTypeEnum.ApproveBy.ToString())
                        {
                            dr["CurrentStatus"] = CurrentStatusEnum.ToStart.ToString();
                        }
                        else if (authorizationtype == AuthorizationTypeEnum.AssignTo.ToString())
                        {
                            dr["CurrentStatus"] = CurrentStatusEnum.InProgress.ToString();
                        }


                        DataRow drAuth = dsAuthorization.Tables[0].Rows[0];
                        drAuth.BeginEdit();
                        drAuth["isDone"] = false;
                        drAuth["UpdatedDate"] = System.DateTime.Now.ToString();
                        drAuth.EndEdit();



                    }

                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                catch (Exception)
                {


                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsAuthorization);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult UpdateToDoMasterStatusForToDo(bool closed, string authorizationtype, string TaskManagerMasterId)
        {

            //return null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerMaster  where Id='" + TaskManagerMasterId + "'", out dsMaster, false, "1");

                DataSet a, dsAuthorization;
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where TaskManagerMasterId='" + TaskManagerMasterId + "' AND AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + "'", out a, false, "1");
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where TaskManagerMasterId='" + TaskManagerMasterId + "' AND AuthorizationType='" + authorizationtype + "'", out dsAuthorization, false, "1");



                DataRow dr = dsMaster.Tables[0].Rows[0];
                //dr.BeginEdit();
                //Id IssueTransactionId  TaskType TaskDescription CurrentStatus AddedBy AddedDate AddedFromIP UpdatedBy UpdatedDate UpdatedFromIP

                try
                {
                    dr.BeginEdit();
                    if (closed == true)
                    {
                        //if (a.Tables[0].Rows[0]["ResponsiblePersonId"].ToString() == identity.EmployeeId)
                        //{
                        //    dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                        //    dr["ClosingDate"] = System.DateTime.Now.ToString();
                        //       dr["ClosedBy"] = identity.Name;
                        //
                        //}
                        //else
                        //{

                        if (authorizationtype == AuthorizationTypeEnum.CreatedBy.ToString())
                        {
                            dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                            dr["ClosingDate"] = System.DateTime.Now.ToString();
                            dr["ClosedBy"] = identity.Name;

                        }
                        else if (authorizationtype == AuthorizationTypeEnum.AssignTo.ToString())
                        {
                            if (a.Tables[0].Rows[0]["ResponsiblePersonId"].ToString() == identity.EmployeeId)
                            {
                                dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                                dr["ClosingDate"] = System.DateTime.Now.ToString();
                                dr["ClosedBy"] = identity.Name;

                            }
                            else
                            {
                                dr["CurrentStatus"] = CurrentStatusEnum.ToClose.ToString();

                                DataRow drAuth = dsAuthorization.Tables[0].Rows[0];
                                drAuth.BeginEdit();
                                drAuth["isDone"] = true;
                                drAuth["UpdatedDate"] = System.DateTime.Now.ToString();
                                drAuth.EndEdit();
                            }
                        }
                        else
                        {

                            DataRow drAuth = dsAuthorization.Tables[0].Rows[0];
                            drAuth.BeginEdit();
                            drAuth["isDone"] = true;
                            drAuth["UpdatedDate"] = System.DateTime.Now.ToString();
                            drAuth.EndEdit();
                        }

                        //}
                    }
                    else
                    {
                        //if (a.Tables[0].Rows[0]["ResponsiblePersonId"].ToString() == identity.EmployeeId)
                        //{
                        //    dr["CurrentStatus"] = CurrentStatusEnum.ToStart.ToString();
                        //}
                        //else
                        //{
                        //    DataRow drAuth = dsAuthorization.Tables[0].Rows[0];
                        //    drAuth.BeginEdit();
                        //    drAuth["isDone"] = false;
                        //    drAuth.EndEdit();
                        //}

                        if (authorizationtype == AuthorizationTypeEnum.CreatedBy.ToString())
                        {
                            dr["CurrentStatus"] = CurrentStatusEnum.ToStart.ToString();
                        }
                        else if (authorizationtype == AuthorizationTypeEnum.AssignTo.ToString())
                        {
                            dr["CurrentStatus"] = CurrentStatusEnum.InProgress.ToString();
                        }


                        DataRow drAuth = dsAuthorization.Tables[0].Rows[0];
                        drAuth.BeginEdit();
                        drAuth["isDone"] = false;
                        drAuth["UpdatedDate"] = System.DateTime.Now.ToString();
                        drAuth.EndEdit();



                    }

                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                catch (Exception)
                {


                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsAuthorization);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }


        [HttpPost, Authorize]
        public ActionResult DeleteToDoSubTasks(Dictionary<string, object> ToDoSubTask)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerSubTasks  where Id='" + ToDoSubTask["Id"] + "'", out dsMaster, false, "1");



                #region Task data update


                dsMaster.Tables[0].Rows[0].Delete();


                #endregion data update




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult AddToDoComment(Dictionary<string, object> ToDo, string ToDoId)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskComments  where Id='" + ToDo["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region Task data update

                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("TO DO COMMENT", out _Id);

                _Id = "TC" + _Id;
                ToDo["Id"] = _Id;


                DataRow dr = dsMaster.Tables[0].NewRow();
                AddNewRow(ToDo, dr);
                //  
                dr["TaskManagerMasterId"] = ToDoId;

                dr["CreatedById"] = identity.EmployeeId;
                dr["CreatedTime"] = System.DateTime.Now.ToString();

                dsMaster.Tables[0].Rows.Add(dr);


                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                ConnectionManager.clsConnectionManager conNew = new ConnectionManager.clsConnectionManager();
                conNew.BeginTransaction();
                conNew.executeQuery("update TaskAudit set isReadComment=1 where TaskManagerMasterId='" + ToDoId + "' AND ResponsiblePersonId='" + identity.EmployeeId + "'");
                conNew.executeQuery("update TaskAudit set isReadComment=0 where TaskManagerMasterId='" + ToDoId + "' AND ResponsiblePersonId<>'" + identity.EmployeeId + "'");
                conNew.CommitTransaction();


                return Json(new
                {
                    Error = false,
                    CommentsList = _sqlRepository.GetDataCollection(AllComments(ToDoId)),
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public ActionResult UpdateToDoCommentReadStatus(string ToDoId)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



                ConnectionManager.clsConnectionManager conNew = new ConnectionManager.clsConnectionManager();
                conNew.BeginTransaction();
                conNew.executeQuery("update TaskAudit set isReadComment=1 where TaskManagerMasterId='" + ToDoId + "' AND ResponsiblePersonId='" + identity.EmployeeId + "'");
                conNew.CommitTransaction();


                return Json(new { Error = false, Message = "Status Updated" });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult AddToDoSubTask(Dictionary<string, object> ToDo, string ToDoId)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerSubTasks  where Id='" + ToDo["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region Task data update

                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("TO DO SubTask", out _Id);

                _Id = "TC" + _Id;
                ToDo["Id"] = _Id;


                DataRow dr = dsMaster.Tables[0].NewRow();
                AddNewRow(ToDo, dr);
                //  
                dr["TaskManagerMasterId"] = ToDoId;

                dsMaster.Tables[0].Rows.Add(dr);


                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new
                {
                    Error = false,
                    SubTasksList = _sqlRepository.GetDataCollection(AllSubTasks(ToDoId)),
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }


        [HttpPost, Authorize]
        public ActionResult GetAllCreatedByMe(string AuthorizationType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = AllCreatedByMe(AuthorizationType);

                return Json(new { LIST = _sqlRepository.GetDataCollection(sql), EmployeeId = identity.EmployeeId }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }
        string AllCreatedByMe(string AuthorizationType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string s = @"SELECT tm.*,ei.EmployeeCode,ei.EmployeeName,ei.EmpPicPath,
                        FORMAT( ISNULL(ATO.RevisedCommitmentDate,ATO.CommitmentDate),'dd-MMM-yyyy') AS CommitmentDate,
                        FORMAT(ATO.DueDate,'dd-MMM-yyyy') AS DueDate,
                        ATO.ResponsiblePersonId,ACR.ResponsiblePersonId AS CreatedById, ta.AuthorizationType,ta.Remarks
                            FROM [TaskAudit] TA
                            INNER JOIN TaskManagerMaster AS tm ON ta.TaskManagerMasterId=tm.Id
                            left outer join taskAudit ATO on ato.TaskManagerMasterId=tm.Id and ATO.AuthorizationType='" + AuthorizationTypeEnum.AssignTo.ToString() + @"'
                            LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=ATO.ResponsiblePersonId

                            left outer join taskAudit ACR on ACR.TaskManagerMasterId=tm.Id and ACR.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
                            LEFT OUTER JOIN EmployeeInformation AS eiCR ON eiCR.SystemId=ACR.ResponsiblePersonId

                            WHERE TA.AuthorizationType='" + AuthorizationType
                            + @"' AND TA.ResponsiblePersonId='" + identity.EmployeeId + "' AND tm.TaskType='" + TaskTypeEnum.ToDo.ToString()
                            + "' AND tm.CurrentStatus<>'" + CurrentStatusEnum.Closed.ToString() + "' ORDER BY tm.AddedDate DESC";
            return s;
        }
        [Authorize, HttpPost]
        public ActionResult GetMasterData(string ToDoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //var _taskCategory = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskCategory AS p where active=1 AND FLAG='" + TaskCategoryFlagEnum.ToDo.ToString() + "' ORDER BY p.Sequence");
            //var _taskSubCategory = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskSubCategory AS p where active=1 AND FLAG='" + TaskCategoryFlagEnum.ToDo.ToString() + "' ORDER BY p.Sequence");
            Dictionary<string, object> data = _sqlRepository.GetDataCollection("SELECT * FROM TaskManagerMaster AS tmm WHERE id='" + ToDoId + "'")[0];


            string flag = "ToDo";
            try
            {
                if (data["IssueTransactionId"].ToString() != "")
                    flag = "Issue";
            }
            catch (Exception)
            {


            }

            var _taskCategory = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskCategory AS p where active=1 AND FLAG='" + flag + "' ORDER BY p.Sequence");
            var _taskSubCategory = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskSubCategory AS p where active=1 AND FLAG='" + flag + "' ORDER BY p.Sequence");



            return Json(new { TaskCategory = _taskCategory, TaskSubCategory = _taskSubCategory }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetAllComments(string ToDoId)
        {
            try
            {
                string sql = AllComments(ToDoId);

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetAllCommentsForDashboard(string ToDoId)
        {
            try
            {
                string sql = AllCommentsForDashboard(ToDoId);

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetAllUnreadThreads()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from (SELECT distinct tmm.*,'' AS BuyerName,isnull(tsc.UserName,'') AS TaskCategory ,TSSC.UserName AS TaskSubCategory,'' AS SearchDataTemp
                                ,Tasto.EmpPicPath,NULL AS Auth,
                                
                                Tasto.EmployeeName AS AssignTo,Tasto.SystemId AS AssignToId,
                                AasBy.EmpPicPath AS EmpPicPathAssignBy,AasBy.EmployeeName AS CreatedBy,AasBy.SystemId AS CreatedById,ta.Id AS TaskAuditId
                                ,CBY.EmployeeName AS  CommentedBy,
                                FORMAT( ISNULL(tTo.RevisedCommitmentDate,tTo.CommitmentDate),'dd-MMM-yyyy') AS CommitmentDate,
                                    FORMAT(ta.DueDate,'dd-MMM-yyyy') AS DueDate,FORMAT(ta.DueDate,'dd-MMM-yyyy') AS DueDateFilter
                                ,ta.ResponsiblePersonId,ta.AuthorizationType,ta.Remarks,isnull(Ta.IsRead,0) AS IsRead,Tc.CommentText,
                                Format(TC.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS CommentCreatedTime,
                                dense_rank() OVER (PARTITION BY tmm.Id,ta.ResponsiblePersonId ORDER BY ta.AuthorizationType,tc.CreatedTime DESC) AS RNK

                                FROM [TaskManagerMaster] AS tmm
                                INNER JOIN TaskComments AS tc ON tmm.Id=tc.TaskManagerMasterId
                                 INNER JOIN [EmployeeInformation] CBY ON CBY.SystemId = TC.CreatedById

                                left JOIN  HKP.TaskCategory TSC ON TSC.ID=tmm.TaskCategoryId
                                left JOIN  HKP.TaskSubCategory TSSC ON TSSC.ID=tmm.TaskSubCategoryId
                              
                                LEFT JOIN [TaskAudit] ta ON ta.TaskManagerMasterId = tmm.Id
                                LEFT JOIN [TaskAudit] tTo ON tTo.TaskManagerMasterId = tmm.Id AND tto.AuthorizationType='AssignTo'
                                LEFT JOIN [TaskAudit] tBy ON tBy.TaskManagerMasterId = tmm.Id AND tBy.AuthorizationType='CreatedBy'

                                INNER JOIN [EmployeeInformation] Tasto ON Tasto.SystemId = tTo.ResponsiblePersonId  
                                INNER JOIN [EmployeeInformation] AasBy ON AasBy.SystemId = tBy.ResponsiblePersonId

                                INNER JOIN [EmployeeInformation] asto ON asto.SystemId = ta.ResponsiblePersonId
                                WHERE isnull(ta.isReadComment,0)=0 AND ta.ResponsiblePersonId='" + identity.EmployeeId + @"' AND isnull(tmm.CurrentStatus,0)<>'Closed'
                    ) AS K WHERE k.RNK=1";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetAllUnreadTasks()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from (SELECT distinct tmm.*,'' AS BuyerName,isnull(tsc.UserName,'') AS TaskCategory ,TSSC.UserName AS TaskSubCategory,'' AS SearchDataTemp
                                ,Tasto.EmpPicPath,NULL AS Auth,
                                
                                Tasto.EmployeeName AS AssignTo,Tasto.SystemId AS AssignToId,
                                AasBy.EmpPicPath AS EmpPicPathAssignBy,AasBy.EmployeeName AS CreatedBy,AasBy.SystemId AS CreatedById,ta.Id AS TaskAuditId
                                ,FORMAT(ta.AddedDate,'dd-MMM-yyyy hh:mm tt') AS TaskAddedDate,
                                FORMAT( ISNULL(tTo.RevisedCommitmentDate,tTo.CommitmentDate),'dd-MMM-yyyy') AS CommitmentDate,
                                    FORMAT(ta.DueDate,'dd-MMM-yyyy') AS DueDate,FORMAT(ta.DueDate,'dd-MMM-yyyy') AS DueDateFilter
                                ,ta.ResponsiblePersonId,ta.AuthorizationType,ta.Remarks,isnull(Ta.IsRead,0) AS IsRead,
                                dense_rank() OVER (PARTITION BY tmm.Id,ta.ResponsiblePersonId ORDER BY ta.AuthorizationType) AS RNK

                                FROM [TaskManagerMaster] AS tmm
                                left JOIN  HKP.TaskCategory TSC ON TSC.ID=tmm.TaskCategoryId
                                left JOIN  HKP.TaskSubCategory TSSC ON TSSC.ID=tmm.TaskSubCategoryId
                              
                                  INNER JOIN [TaskAudit] ta ON ta.TaskManagerMasterId = tmm.Id  AND ta.ResponsiblePersonId='" + identity.EmployeeId + @"' 
                                LEFT JOIN [TaskAudit] tTo ON tTo.TaskManagerMasterId = tmm.Id AND tto.AuthorizationType='AssignTo'
                                LEFT JOIN [TaskAudit] tBy ON tBy.TaskManagerMasterId = tmm.Id AND tBy.AuthorizationType='CreatedBy'

                                INNER JOIN [EmployeeInformation] Tasto ON Tasto.SystemId = tTo.ResponsiblePersonId  
                                INNER JOIN [EmployeeInformation] AasBy ON AasBy.SystemId = tBy.ResponsiblePersonId

                                INNER JOIN [EmployeeInformation] asto ON asto.SystemId = ta.ResponsiblePersonId
                                WHERE isnull(ta.isRead,0)=0 
                                AND tBy.ResponsiblePersonId<>'" + identity.EmployeeId + @"'
                               
                                AND isnull(tmm.CurrentStatus,'')<>'Closed'
                        ) AS K WHERE k.RNK=1  ORDER BY k.AddedDate DESC";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }

        string AllComments(string todoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager();
            con.BeginTransaction();
            con.executeQuery("update TaskAudit set isReadComment=1 where TaskManagerMasterId='" + todoId + "' AND ResponsiblePersonId='" + identity.EmployeeId + "'");
            con.CommitTransaction();

            return @"SELECT tc.Id, tc.TaskManagerMasterId, tc.CreatedById, format(tc.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS CreatedTime,
                    tc.CommentText, tc.TaskAthorizationType, tc.AddedBy, tc.AddedDate,ei.EmployeeName,ei.EmpPicPath
                    FROM TaskComments AS tc
                    LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=tc.CreatedById
                    WHERE tc.TaskManagerMasterId='" + todoId + "' ORDER BY CONVERT(DATETIME, createdtime) asc";
        }
        string AllCommentsForDashboard(string todoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            return @"SELECT tc.Id, tc.TaskManagerMasterId, tc.CreatedById, format(tc.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS CreatedTime,
                    tc.CommentText, tc.TaskAthorizationType, tc.AddedBy, tc.AddedDate,ei.EmployeeName,ei.EmpPicPath
                    FROM TaskComments AS tc
                    LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=tc.CreatedById
                    WHERE tc.TaskManagerMasterId='" + todoId + "' ORDER BY CONVERT(DATETIME, createdtime) asc";
        }

        [HttpPost, Authorize]
        public ActionResult GetAllSubTasks(string ToDoId)
        {
            try
            {
                string sql = AllSubTasks(ToDoId);

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }
        string AllSubTasks(string todoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string s = @"SELECT convert(bit, CASE WHEN ISNULL(r.Id,'')= '' THEN 0 ELSE 1 END) AS hasRemarks,tc.*,ei.EmployeeName,ei.EmpPicPath--,ta.AuthorizationType
  FROM TaskManagerSubTasks AS tc
LEFT OUTER JOIN TaskManagerSubTaskRemarks AS R ON r.TaskManagerSubTasksId = tc.Id
                        AND r.Id = (SELECT TOP 1 Id FROM TaskManagerSubTaskRemarks WHERE TaskManagerSubTasksId = tc.Id)
LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=tc.ResponsiblePersonId
--LEFT OUTER JOIN TaskAudit AS ta ON tc.TaskManagerMasterId=ta.TaskManagerMasterId and ta.ResponsiblePersonId='" + identity.EmployeeId + @"' AND ta.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
                    WHERE  tc.TaskManagerMasterId='" + todoId + "' order by convert(datetime, TC.AddedDate) ASC";

            return s;
        }

        [HttpPost, Authorize]
        public ActionResult GetAllFiles(string ToDoId)
        {
            try
            {
                string sql = AllFiles(ToDoId);

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }
        string AllFiles(string todoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string s = @"SELECT tc.*,ei.EmployeeName,ei.EmpPicPath,ta.AuthorizationType FROM TaskAttachments AS tc
                    LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=tc.UploadedById
                    LEFT OUTER JOIN TaskAudit AS ta ON tc.TaskManagerMasterId=ta.TaskManagerMasterId and ta.ResponsiblePersonId='" + identity.EmployeeId
                    + @"' AND ta.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
                    WHERE tc.TaskManagerMasterId='" + todoId + "' order by TC.AddedDate ASC";

            return s;
        }

        [HttpPost, Authorize]
        public ActionResult getToDo(string ToDoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {

                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery("update TaskAudit set isRead=1 where TaskManagerMasterId='" + ToDoId + "' AND ResponsiblePersonId='" + identity.EmployeeId + "'");
                connection.executeQuery("update TaskAudit set isReadComment=1 where TaskManagerMasterId='" + ToDoId + "' AND ResponsiblePersonId='" + identity.EmployeeId + "'");
                connection.CommitTransaction();


                Dictionary<string, object> data = _sqlRepository.GetDataCollection("SELECT * FROM TaskManagerMaster AS tmm WHERE id='" + ToDoId + "'")[0];
                Dictionary<string, object> ScheduleData = new Dictionary<string, object>();
                try
                {
                    ScheduleData = _sqlRepository.GetDataCollection("select * from TaskSchedulerMaster where id=(select TaskSchedulerMasterId from TaskManagerMaster where id='" + ToDoId + "')")[0];

                }
                catch (Exception)
                {
                }
                data.Add("Schedule", ScheduleData);
                try
                {
                    List<Dictionary<string, object>> _authdata = _sqlRepository.GetDataCollection(ToDo(ToDoId, AuthorizationTypeEnum.CreatedBy));
                    if (_authdata.Count > 0)
                    {
                        data.Add(AuthorizationTypeEnum.CreatedBy.ToString(), _authdata[0]);
                        data["DueDate"] = _authdata[0]["DueDate"].ToString();
                    }
                    else
                        data.Add(AuthorizationTypeEnum.CreatedBy.ToString(), null);


                    _authdata = _sqlRepository.GetDataCollection(ToDo(ToDoId, AuthorizationTypeEnum.AssignTo));
                    if (_authdata.Count > 0)
                        data.Add(AuthorizationTypeEnum.AssignTo.ToString(), _authdata[0]);
                    else
                        data.Add(AuthorizationTypeEnum.AssignTo.ToString(), null);


                    _authdata = _sqlRepository.GetDataCollection(ToDo(ToDoId, AuthorizationTypeEnum.CheckBy));
                    if (_authdata.Count > 0)
                        data.Add(AuthorizationTypeEnum.CheckBy.ToString(), _authdata[0]);
                    else
                        data.Add(AuthorizationTypeEnum.CheckBy.ToString(), null);


                    _authdata = _sqlRepository.GetDataCollection(ToDo(ToDoId, AuthorizationTypeEnum.CrossCheckBy));
                    if (_authdata.Count > 0)
                        data.Add(AuthorizationTypeEnum.CrossCheckBy.ToString(), _authdata[0]);
                    else
                        data.Add(AuthorizationTypeEnum.CrossCheckBy.ToString(), null);



                    _authdata = _sqlRepository.GetDataCollection(ToDo(ToDoId, AuthorizationTypeEnum.ApproveBy));
                    if (_authdata.Count > 0)
                        data.Add(AuthorizationTypeEnum.ApproveBy.ToString(), _authdata[0]);
                    else
                        data.Add(AuthorizationTypeEnum.ApproveBy.ToString(), null);




                    return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {

                }

                return Json(JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }
        string ToDo(string todoId, AuthorizationTypeEnum auth)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string s = @"SELECT K.* from
                    (select 1 as adummy) a 
                    left outer join (select tc.*,ei.SystemId as EmployeeId,ei.EmployeeCode,ei.EmployeeName,ei.EmpPicPath FROM TaskAudit AS tc
                    LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=tc.ResponsiblePersonId
                    WHERE tc.TaskManagerMasterId='" + todoId + "' AND AuthorizationType='" + auth.ToString() + "' ) AS K ON 1=1";

            return s;
        }



        [HttpPost, Authorize]
        public ActionResult DeleteFile(string FileId)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskAttachments  where Id='" + FileId + "'", out dsMaster, false, "1");



                var destinationPath = Path.Combine(ResourcesPathReader.GetToDoPath(), dsMaster.Tables[0].Rows[0]["FileName"].ToString());
                if (System.IO.File.Exists(destinationPath))
                    System.IO.File.Delete(destinationPath);

                #region Task data update


                dsMaster.Tables[0].Rows[0].Delete();


                #endregion data update




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion TODO

        #region upload product picture
        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                //UploadDefault_data = UploadDefault_data.Replace("\"", "");
                //if (string.IsNullOrEmpty(UploadDefault_data))
                //    throw new Exception("Save the production order first");

                UploadDefault_data = UploadDefault_data.Replace("\\", "");
                DataTable AdditionalData = CustomJsonResult.ToDataTable(UploadDefault_data);


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                foreach (var file in UploadDefault)
                {
                    string _Id = "";
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("TO DO", out _Id);

                    _Id = "T" + _Id;

                    var fileName = Path.GetFileName(AdditionalData.Rows[0]["ToDoId"].ToString() + _Id + new FileInfo(file.FileName).Extension);
                    var destinationPath = Path.Combine(ResourcesPathReader.GetToDoPath(), AdditionalData.Rows[0]["ToDoId"].ToString() + _Id + new FileInfo(file.FileName).Extension);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetToDoPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetToDoPath());
                        }
                        catch (Exception ex)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from TaskAttachments where 1=2";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();

                    if (dsLocal.Tables[0].Rows.Count == 0)
                    {


                        #region Task data update


                        DataRow dr = dsLocal.Tables[0].NewRow();

                        dr["Id"] = _Id;

                        dr["FileName"] = fileName;
                        dr["TaskManagerMasterId"] = AdditionalData.Rows[0]["ToDoId"].ToString();
                        dr["FileDescription"] = AdditionalData.Rows[0]["FileDescription"].ToString();
                        dr["UploadedById"] = identity.EmployeeId;
                        dr["FileOriginalName"] = file.FileName;
                        dr["Extension"] = new FileInfo(file.FileName).Extension;


                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsLocal.Tables[0].Rows.Add(dr);


                        #endregion data update





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
        public ActionResult UpdateFile(string Id, string Description)
        {
            try
            {
                if (string.IsNullOrEmpty(Description))
                    throw new Exception("Please provide file description");

                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                string sql = "select* from TaskAttachments where id='" + Id + "'";
                DataSet dsLocal = null;
                connection.BeginTransaction();
                connection.getDataSet(sql, out dsLocal);
                connection.CommitTransaction();

                if (dsLocal.Tables[0].Rows.Count > 0)
                {


                    #region Task data update


                    DataRow dr = dsLocal.Tables[0].Rows[0];
                    dr.BeginEdit();

                    dr["FileDescription"] = Description;

                    dr.EndEdit();


                    #endregion data update


                    clsStaticInfo info = new clsStaticInfo();
                    info.SaveDataSets(dsLocal);




                }
                return Json(new { Error = false, Message = "File desription updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [Authorize]
        public ActionResult RemoveDefault(string[] fileNames)
        {
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return Content("");
        }

        #endregion upload product picture

        [HttpPost, Authorize]
        public ActionResult CreateTaskSchedule(Dictionary<string, object> taskSchedule, string ToDoId)
        {
            try
            {


                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskSchedulerMaster where id=(select TaskSchedulerMasterId from TaskManagerMaster where id='" + ToDoId + "')", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("Dbo.TaskSchedulerMaster", out _Id);

                    taskSchedule["Id"] = "TS" + _Id;

                    _Id = taskSchedule["Id"].ToString();
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    AddNewRow(taskSchedule, dr);

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {

                    EditRow(dsMaster.Tables[0].Rows[0], taskSchedule);
                    _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                }
                #endregion data update

                DataSet dsToDo;
                con.OpenDataSetThroughAdapter("select * from TaskManagerMaster where id='" + ToDoId + "'", out dsToDo, false, "1");
                dsToDo.Tables[0].Rows[0].BeginEdit();
                dsToDo.Tables[0].Rows[0]["TaskSchedulerMasterId"] = _Id;
                dsToDo.Tables[0].Rows[0].EndEdit();

                // Save to Database 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsToDo);

                return Json(new { TaskSchedule = taskSchedule, Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskSchedule(string ToDoId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT * FROM TaskSchedulerMaster AS tsm WHERE tsm.Id=(SELECT tmm.TaskSchedulerMasterId
                                                         FROM TaskManagerMaster AS tmm WHERE tmm.Id = '" + ToDoId + @"')";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }

        [HttpPost, Authorize]
        public ActionResult getTask(string ToDoId)
        {
            try
            {


                string sql = @"select TM.*, TC.UserName AS TaskCategory, TSC.UserName AS TaskSubCategory, TATO.CommitmentDate, TATO.RevisedCommitmentDate, TABy.DueDate
                ,ETO.EmployeeName AS AssignTo,ETO.EmployeeCode AS EmployeeCodeTo, ETO.EmpPicPath AS EmpPicPathTo, EBy.EmployeeName AS AssignBy,EBy.EmployeeCode ASEmployeeCodeBy,  EBy.EmpPicPath AS EmpPicPathBy
                ,ECheckBy.EmployeeName AS CheckBy,ECheckBy.EmployeeCode AS EmployeeCodeCheckBy ,ECheckBy.EmpPicPath AS EmpPicPathCheckBy,  ECrossCheckBy.EmployeeName AS CrossCheckBy,ECrossCheckBy.EmployeeCode AS EmployeeCodeCrossCheckBy
                ,ECrossCheckBy.EmpPicPath AS EmpPicPathCrossCheckBy, EApproveBy.EmployeeName AS ApproveBy,EApproveBy.EmpPicPath AS EmpPicPathApproveBy, EApproveBy.EmployeeCode AS EmployeeCodeApproveBy
                ,ETO.EmployeeId AS EmployeeIdAssignTo , ECheckBy.EmployeeId AS EmployeeIdCheckBy, ECrossCheckBy.EmployeeId As EmployeeIdCrossCheckBy,EApproveBy.EmployeeId AS EmployeeIdApproveBy
                from TaskManagerMaster  TM
                left join hkp.TaskCategory AS TC ON TC.Id = TM.TaskCategoryId 
                left join hkp.TaskSubCategory AS TSC ON TSC.Id = TM.TaskSubCategoryId
                left outer join TaskAudit TATO on tato.TaskManagerMasterId=TM.Id and tato.AuthorizationType='" + AuthorizationTypeEnum.AssignTo.ToString() + @"'
                left outer join EmployeeInformation ETO on ETo.systemid=tato.ResponsiblePersonId
                left outer join TaskAudit TABy on TABy.TaskManagerMasterId=TM.Id and TABy.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
                left outer join EmployeeInformation EBy on EBy.systemid=TABy.ResponsiblePersonId
                left outer join TaskAudit TACheckBy on TACheckBy.TaskManagerMasterId=TM.Id and TACheckBy.AuthorizationType='" + AuthorizationTypeEnum.CheckBy.ToString() + @"'
                left outer join EmployeeInformation ECheckBy on ECheckBy.systemid=TACheckBy.ResponsiblePersonId
                left outer join TaskAudit TACrossCheckBy on TACrossCheckBy.TaskManagerMasterId=TM.Id and TACrossCheckBy.AuthorizationType='" + AuthorizationTypeEnum.CrossCheckBy.ToString() + @"'
                left outer join EmployeeInformation ECrossCheckBy on ECrossCheckBy.systemid=TACrossCheckBy.ResponsiblePersonId
                left outer join TaskAudit TAApproveBy on TAApproveBy.TaskManagerMasterId=TM.Id and TAApproveBy.AuthorizationType='" + AuthorizationTypeEnum.ApproveBy.ToString() + @"'
                left outer join EmployeeInformation EApproveBy on EApproveBy.systemid=TAApproveBy.ResponsiblePersonId

                 where TM.Id = '" + ToDoId + @"'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
        }



        [HttpPost, Authorize]
        public ActionResult GetScheduledTaskList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT tmm.Id,tmm.TaskSchedulerMasterId, tmm.TaskType, tmm.TaskDescription,tsm.Details,
                                ei.EmployeeName,ei.EmpPicPath
                                FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id AND ta.AuthorizationType='CreatedBy'
                                INNER JOIN TaskAudit AS taTo ON taTo.TaskManagerMasterId=tmm.Id AND taTo.AuthorizationType='AssignTo'
                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=taTo.ResponsiblePersonId
                                INNER JOIN TaskSchedulerMaster AS tsm ON tsm.Id=tmm.TaskSchedulerMasterId

                                WHERE ta.ResponsiblePersonId='" + identity.EmployeeId + "' AND isnull(tmm.IsExpiredSchedule,0)<>1" +
                                " ORDER BY tmm.AddedDate ASC ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = AplosMessage.Updated });
            }
        }

        [HttpPost, Authorize]
        public ActionResult UpdateScheduledTaskList(string taskmasterid)
        {
            try
            {
                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager();
                con.BeginTransaction();
                con.executeQuery("UPDATE TaskManagerMaster SET  LastExecutionDate=NULL,	NextExecutionDate=NULL,	NoOfOccurences=NULL,	IsExpiredSchedule=NULL,TaskSchedulerMasterId=NULL WHERE Id='" + taskmasterid + "' ");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost, Authorize]
        public ActionResult UpdateToDoSubTasksRemarks(string TaskMasterId, string SubTaskId, string Remarks)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerSubTaskRemarks  where 1=2", out dsMaster, false, "1");



                #region Task data update
                string _Id = "";
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("TO DO SUBTASK COMMENT", out _Id);

                DataRow dr = dsMaster.Tables[0].NewRow();
                dr["Id"] = _Id;
                dr["TaskManagerMasterId"] = TaskMasterId;
                dr["TaskManagerSubTasksId"] = SubTaskId;
                dr["RemarksById"] = identity.EmployeeId;
                dr["Remarks"] = Remarks;

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;


                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;



                dsMaster.Tables[0].Rows.Add(dr);
                #endregion data update




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult DeleteToDoSubTasksRemarks(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerSubTaskRemarks  where id='" + Id + "'", out dsMaster, false, "1");



                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Deleted
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetToDoSubTasksRemarks(string SubTaskId)
        {

            try
            {


                return Json(_sqlRepository.GetDataCollection(@"select R.*,ei.EmployeeName,ei.EmpPicPath,format(r.AddedDate,'dd-MMM-yyyy hh:mm:ss tt') RemarksTime from TaskManagerSubTaskRemarks R
                                    INNER JOIN EmployeeInformation AS ei ON ei.SystemId = r.RemarksById
                      where TaskManagerSubTasksId='" + SubTaskId + "' ORDER BY r.AddedDate DESC"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }


        #region CHAT
        [HttpPost, Authorize]
        public ActionResult CreateSingleChatMaster(string ToId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string s = @"select * from ChatMaster  where ((ToId='" + identity.EmployeeId + "' AND FromId='" + ToId + "') OR (FromId='" + identity.EmployeeId + "' AND ToId='" + ToId + "')) AND isnull(IsGroupChat,0)=0";

                var k = _sqlRepository.GetDataCollection(s);
                string _Id = "";
                string singleChat = "";
                if (k.Count > 0)
                {
                    _sqlRepository.ExecuteSqlCommand(@"UPDATE ChatParticipants SET IsRead = 1 WHERE ChatMasterId='" + k[0]["Id"].ToString() + @"' AND EmployeeId='" + identity.EmployeeId + @"'");


                    singleChat = @"SELECT cm.Id AS ChatMasterId, chat.Id, chat.ChatMasterId, chat.EmployeeId,ei.EmployeeName,ei.EmpPicPath, chat.Chat, 
                                   chat.DateCreated, chat.IsActive
                              FROM Chat
                            INNER JOIN EmployeeInformation AS ei ON ei.SystemId=chat.EmployeeId
                            INNER JOIN ChatMaster AS cm ON cm.Id=chat.ChatMasterId
                            WHERE chat.ChatMasterId='" + k[0]["Id"].ToString() + @"'
                            ORDER BY DateCreated ASC";

                    return Json(new
                    {

                        Error = false,
                        ChatId = k[0]["Id"].ToString(),
                        CurrentChat = _sqlRepository.GetDataCollection(singleChat),
                        chatParticipants = _sqlRepository.GetDataCollection("SELECT * FROM ChatParticipants AS cp WHERE cp.ChatMasterId='" + k[0]["Id"].ToString() + "' AND cp.EmployeeId<>'" + identity.EmployeeId + "'"),

                        Message = AplosMessage.Updated
                    });
                }
                #region Task data update
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(s, out dsMaster, false, "1");

                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("TO DO COMMENT", out _Id);

                _Id = "CM" + _Id;

                DataRow dr = dsMaster.Tables[0].NewRow();
                dr["Id"] = _Id;
                dr["FromId"] = identity.EmployeeId;
                dr["ToId"] = ToId;

                dsMaster.Tables[0].Rows.Add(dr);


                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                addParticipants(identity.EmployeeId, _Id, true);
                addParticipants(ToId, _Id, false);

                singleChat = @"SELECT cm.Id AS ChatMasterId, chat.Id, chat.ChatMasterId, chat.EmployeeId,ei.EmployeeName,ei.EmpPicPath, chat.Chat, 
                                   chat.DateCreated, chat.IsActive
                              FROM Chat
                            INNER JOIN EmployeeInformation AS ei ON ei.SystemId=chat.EmployeeId
                            INNER JOIN ChatMaster AS cm ON cm.Id=chat.ChatMasterId
                            WHERE chat.ChatMasterId='" + _Id + @"'
                            ORDER BY DateCreated ASC";

                return Json(new
                {
                    Error = false,
                    ChatList = _sqlRepository.GetDataCollection(s),
                    CurrentChat = _sqlRepository.GetDataCollection(singleChat),
                    chatParticipants = _sqlRepository.GetDataCollection("SELECT * FROM ChatParticipants AS cp WHERE cp.ChatMasterId='" + _Id + "' AND cp.EmployeeId<>'" + identity.EmployeeId + "'"),
                    ChatId = _Id,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }


        private void addParticipants(string EmployeeId, string ChatMasterId, bool isRead)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from ChatParticipants  where EmployeeId='" + EmployeeId + "' AND ChatMasterId='" + ChatMasterId + "'", out dsMaster, false, "1");


                DataRow dr = dsMaster.Tables[0].NewRow();

                dr["ChatMasterId"] = ChatMasterId;
                dr["EmployeeId"] = EmployeeId;
                dr["isRead"] = isRead;

                dsMaster.Tables[0].Rows.Add(dr);




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {


            }
        }
        private void deleteParticipants(string EmployeeId, string ChatMasterId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from ChatParticipants  where EmployeeId='" + EmployeeId + "' AND ChatMasterId='" + ChatMasterId + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                    dsMaster.Tables[0].Rows[0].Delete();


                if (_sqlRepository.GetDataTable(@"SELECT * FROM chat 

                        WHERE ChatMasterId='" + ChatMasterId + @"' AND EmployeeId NOT IN (
                        SELECT fromId FROM ChatMaster AS cm WHERE cm.Id='" + ChatMasterId + @"'
                        UNION
                        SELECT toid FROM ChatMaster AS cm WHERE cm.Id='" + ChatMasterId + @"'
	
                        )").Rows.Count == 0)
                {

                    string s = @"UPDATE ChatMaster SET IsGroupChat = 0 WHERE Id = '" + ChatMasterId + "')";
                    _sqlRepository.ExecuteSqlCommand(s);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {


            }
        }

        [HttpPost, Authorize]
        public ActionResult AddToGroupChat(string EmployeeId, string ChatMasterId)
        {
            try
            {
                addParticipants(EmployeeId, ChatMasterId, false);

                string s = @"UPDATE ChatMaster SET IsGroupChat = 1 WHERE Id = '" + ChatMasterId + "')";

                _sqlRepository.ExecuteSqlCommand(s);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult RemoveFromGroupChat(string EmployeeId, string ChatMasterId)
        {
            try
            {
                deleteParticipants(EmployeeId, ChatMasterId);


                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult CreateChat(string ChatMessage, string ChatMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                _sqlRepository.ExecuteSqlCommand(@"INSERT INTO Chat
                                                    (
	                                                    -- Id -- this column value is auto-generated
	                                                    ChatMasterId,
	                                                    EmployeeId,
	                                                    Chat
                                                    )
                                                    VALUES
                                                    (
	                                                    '" + ChatMasterId + @"',
	                                                    '" + identity.EmployeeId + @"',
	                                                    N'" + ChatMessage + @"'
	                                               
	
                                                    )");

                _sqlRepository.ExecuteSqlCommand(@"UPDATE ChatParticipants SET IsRead = 0 WHERE ChatMasterId='" + ChatMasterId + @"' AND EmployeeId<>'" + identity.EmployeeId + @"'");

                string singleChat = @"SELECT top 1 cm.Id AS ChatMasterId, chat.Id, chat.ChatMasterId, chat.EmployeeId,ei.EmployeeName,ei.EmpPicPath, chat.Chat, 
                                   chat.DateCreated, chat.IsActive
                              FROM Chat
                            INNER JOIN EmployeeInformation AS ei ON ei.SystemId=chat.EmployeeId
                            INNER JOIN ChatMaster AS cm ON cm.Id=chat.ChatMasterId
                            WHERE chat.ChatMasterId='" + ChatMasterId + "' and chat.EmployeeId='" + identity.EmployeeId + @"'
                            ORDER BY DateCreated DESC";
                return Json(new
                {
                    Error = false,
                    SingleChat = _sqlRepository.GetDataCollection(singleChat),
                    Message = AplosMessage.Updated
                }); ;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetAllChat(string ChatMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string singleChat = @"SELECT cm.Id AS ChatMasterId, chat.Id, chat.ChatMasterId, chat.EmployeeId,ei.EmployeeName,ei.EmpPicPath, chat.Chat, chat.IsRead,
                                   chat.DateCreated, chat.IsActive
                              FROM Chat
                            INNER JOIN EmployeeInformation AS ei ON ei.SystemId=chat.EmployeeId
                            INNER JOIN ChatMaster AS cm ON cm.Id=chat.ChatMasterId
                            WHERE ChatMasterId='" + ChatMasterId + "' ";
                return Json(new
                {
                    Error = false,
                    SingleChat = _sqlRepository.GetDataCollection(singleChat),
                    Message = AplosMessage.Updated
                }); ;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }


        [HttpPost, Authorize]
        public ActionResult GetAllChatForList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string singleChat = @"SELECT cp.*,cm.FromId,cm.ToId,isnull(cm.ChatNamem,'') AS ChatName,
                                    eiFrom.EmployeeName FromEmployee,eiFrom.EmpPicPath FromEmpPicPath, 
                                    eiTo.EmployeeName ToEmployee,eiTo.EmpPicPath ToEmpPicPath

                                      FROM ChatMaster AS cm
                                    INNER JOIN ChatParticipants AS cp ON cm.Id=cp.ChatMasterId 
                                    INNER JOIN Chat AS c ON c.ChatMasterId=cm.Id AND cp.EmployeeId=c.EmployeeId
                                    INNER JOIN EmployeeInformation AS eiFrom ON eiFrom.SystemId=CM.FromId
                                    INNER JOIN EmployeeInformation AS eiTo ON eiTo.SystemId=cm.ToId
                                    WHERE cp.EmployeeId='" + identity + "'";
                return Json(new
                {
                    Error = false,
                    AllChats = _sqlRepository.GetDataCollection(singleChat),
                    Message = AplosMessage.Updated
                }); ;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult ReadChatByEmployeeId(string ChatMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                _sqlRepository.ExecuteSqlCommand(@"UPDATE ChatParticipants SET IsRead = 1 WHERE ChatMasterId='" + ChatMasterId + @"' AND EmployeeId='" + identity.EmployeeId + @"'");


                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                }); ;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult ReadChatByThreadId(string ChatMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string singleChat = @"update ChatParticipants set isread=1 where chatMasterId='" + ChatMasterId + "' AND EmployeeId='" + identity.EmployeeId + "'";
                _sqlRepository.ExecuteSqlCommand(singleChat);
                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                }); ;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }



        #endregion CHAT
    }
}