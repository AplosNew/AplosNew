using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using Library.Service.Enums;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using OTSBD;

namespace Library.General.TaskScheduler
{
    public class TasksService
    {

        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public TasksService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        private double StdHighestTaskPriority = 4.5;

        // For Menu
        private List<Dictionary<string, object>> GetClosedStatisticsString(string logedInUser)
        {
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

        private List<Dictionary<string, object>> GetStatisticsString(string logedInUser)
        {
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

        public IEnumerable<object> GetMenu(string taskstatus, string EmpId)
        {
            try
            {

                if (taskstatus.ToUpper() == "CLOSED")
                    return GetClosedStatisticsString(EmpId);

                else
                    return GetStatisticsString(EmpId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // For Tasks Data
        private List<Dictionary<string, object>> GetTaskAccordingToRresponsiblePersonListString(string logedInUser, string authorizationType, string flag)
        {
            string sql = "";

            string fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
            string ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
            sql = @"SELECT tmm.*,'' AS BuyerName,isnull(tsc.UserName,'') AS TaskCategory ,TSSC.UserName AS TaskSubCategory,'' AS SearchDataTemp
                                ,Tasto.EmpPicPath,NULL AS Auth,Tasto.DepartmentId,d.UserName AS Department,
                                
                                Tasto.EmployeeName AS AssignTo,Tasto.SystemId AS AssignToId,
                                AasBy.EmpPicPath AS EmpPicPathAssignBy,AasBy.EmployeeName AS CreatedBy,AasBy.SystemId AS CreatedById,ta.Id AS TaskAuditId
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
                    sql += @" WHERE isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND CONVERT(DATE, ta.DueDate)='" + DateTime.Now.ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    break;
                case "ThisWeek":
                    sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  CONVERT(DATE, ta.DueDate) Between '" + DateTime.Now.AddDays(1).ToString("dd-MMM-yyyy")
                        + @"' AND '" + DateTime.Now.AddDays(8).ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    break;

                case "FutureTasks":
                    sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  CONVERT(DATE, ta.DueDate) >'" + DateTime.Now.AddDays(8).ToString("dd-MMM-yyyy")
                        + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";

                    break;
                case "OverDue":
                    sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  CONVERT(DATE, ta.DueDate) < '" + DateTime.Now.ToString("dd-MMM-yyyy")
                        + @"' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    break;

                case "Unread":
                    sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND isnull(ta.isRead,1)=0 And ta.AuthorizationType='" + authorizationType + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    break;
                case "MyTasks":
                    sql += @" WHERE  isnull(ta.isDone,0)=0 AND tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND tmm.TaskType IN ('ToDo','TNA','Issue')"
                        + @" AND tTo.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + AuthorizationTypeEnum.AssignTo + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    break;
                case "HighPriorityTasks":
                    sql += @" where  isnull(ta.isDone,0)=0 AND  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  isnull(tmm.TaskPriority,0)>= " + StdHighestTaskPriority.ToString()
                        + @" AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType + @"' ORDER BY CONVERT(DATETIME,ta.DueDate) ASC,isnull(tmm.TaskPriority,0) DESC ";
                    break;

                case "ToClose":
                    sql += @" WHERE isnull(ta.isDone,0)=1 AND tmm.currentstatus='ToClose' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy + "' order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    break;

                default:
                    if (authorizationType == AuthorizationTypeEnum.AssignTo.ToString())
                        sql += @" AND  isnull(ta.isDone,0)=0 AND ta.AuthorizationType='" + authorizationType + @"'
							WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "'  order by CONVERT(DATETIME,ta.DueDate) ASC ";
                    else
                    {
                        List<string> TaskTypes = new List<string>();
                        foreach (TaskTypeEnum str in Enum.GetValues(typeof(TaskTypeEnum)))
                            TaskTypes.Add(str.ToString());

                        if (TaskTypes.Contains(authorizationType))
                        {
                            sql += @" AND  isnull(ta.isDone,0)=0 AND tmm.TaskType='" + authorizationType + @"'
							WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "' AND ta.AuthorizationType = '" + AuthorizationTypeEnum.AssignTo.ToString() + "'  order by CONVERT(DATETIME,ta.DueDate) ASC ";

                        }
                        else
                        {
                            if (authorizationType == AuthorizationTypeEnum.CreatedBy.ToString())
                            {
                                sql += @" AND  isnull(ta.isDone,0)=0 AND ta.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + @"'
							WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "'  order by CONVERT(DATETIME,ta.DueDate) ASC ";

                            }
                            else
                            {
                                sql += @" AND  isnull(ta.isDone,0)=0 AND ta.AuthorizationType='" + authorizationType + @"'
							WHERE  tmm.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "' AND  ta.ResponsiblePersonId ='" + logedInUser + "' AND ta.AuthorizationType = '" + authorizationType + "'  order by CONVERT(DATETIME,ta.DueDate) ASC ";
                            }
                        }
                    }

                    break;
            }

            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql);

            sql = @"SELECT K.*,EI.EmployeeName FROM (
                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id AuditId,isnull(ta.isDone,0) AS isDone,
                            ta.ResponsiblePersonId,ta.isRead,tm.currentstatus,ta.AuthorizationType,ta.DueDate
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.CheckBy.ToString() + @"' AS AuthType) 
                         AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE tm.CurrentStatus<>'" + CurrentStatusEnum.Closed.ToString() + @"'

                        UNION ALL

                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id,isnull(ta.isDone,0) AS isDone,
                        ta.ResponsiblePersonId,ta.isRead,tm.currentstatus,ta.AuthorizationType,ta.DueDate
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.CrossCheckBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE tm.CurrentStatus<>'" + CurrentStatusEnum.Closed.ToString() + @"'

                        UNION ALL

                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id,isnull(ta.isDone,0) AS isDone,ta.ResponsiblePersonId
                       ,ta.isRead,tm.currentstatus,ta.AuthorizationType,ta.DueDate
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.ApproveBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE tm.CurrentStatus<>'" + CurrentStatusEnum.Closed.ToString() + @"'
                        ) AS K

                        left outer join EmployeeInformation EI on EI.SystemId=K.ResponsiblePersonId
                        WHERE k.TaskManagerMasterId IN (SELECT ta.TaskManagerMasterId
                                                          FROM TaskAudit AS ta WHERE ta.ResponsiblePersonId='" + logedInUser + @"')";
            //ORDER BY K.TaskManagerMasterId,K.AuthType";

            switch (flag)
            {
                case "NonAction":
                    sql += @" AND (isnull(k.isDone,0)=0 
                      AND k.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "'" +
                        " AND k.ResponsiblePersonId='" + logedInUser + "'" +
                        " AND k.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "'" +
                        "AND k.AuthorizationType='" + authorizationType + "')" +
                        "order by CONVERT(DATETIME,k.DueDate) ASC ";
                    break;

                case "NotRead":
                    sql += @"--AND (isnull(k.isDone,0)=0 
                    AND k.currentstatus<>'" + CurrentStatusEnum.Closed.ToString() + "'" +
                    " AND isnull(k.isRead,1)=0 And k.AuthorizationType='" + authorizationType + "' " +
                    "AND k.ResponsiblePersonId='" + logedInUser + "'" +
                    " AND k.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' " +
                    "order by CONVERT(DATETIME,k.DueDate) ASC ";
                    break;
            }
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

        private List<Dictionary<string, object>> GetClosedTaskAccordingToRresponsiblePersonListString(string logedInUser, string authorizationType, string flag)
        {
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
                    sql += @" WHERE (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND CONVERT(DATE, ta.DueDate)='" + DateTime.Now.ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType + "' order by tmm.AddedDate ASC ";
                    break;
                case "ThisWeek":
                    sql += @" WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND  CONVERT(DATE, ta.DueDate) Between '" + DateTime.Now.AddDays(1).ToString("dd-MMM-yyyy")
                        + @"' AND '" + DateTime.Now.AddDays(8).ToString("dd-MMM-yyyy") + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType + "' order by tmm.AddedDate ASC ";
                    break;
                case "OverDue":
                    sql += @" WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND  CONVERT(DATE, ta.DueDate)>= CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND  CONVERT(DATE, ta.DueDate) < '" + DateTime.Now.ToString("dd-MMM-yyyy")
                        + @"' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType + "' order by tmm.AddedDate ASC ";
                    break;

                case "Unread":
                    sql += @" WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "' AND isnull(ta.isRead,1)=0) And ta.AuthorizationType='" + authorizationType + "'AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' order by tmm.AddedDate ASC ";
                    break;

                case "FutureTasks":
                    sql += @" WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND  CONVERT(DATE, ta.DueDate)>= CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, ta.DueDate) > '" + DateTime.Now.AddDays(8).ToString("dd-MMM-yyyy")
                       + "' AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType + "' order by tmm.AddedDate ASC ";
                    break;

                case "MyTasks":
                    sql += @" WHERE  (isnull(ta.isDone,0)=1 OR tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND tmm.TaskType IN ('ToDo','TNA','Issue')"
                        + @" AND tTo.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + AuthorizationTypeEnum.AssignTo + "' order by tmm.AddedDate ASC ";
                    break;

                case "HighPriorityTasks":
                    sql += @" where  (isnull(ta.isDone,0)=1 OR  tmm.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "') AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND isnull(tmm.TaskPriority,0)>= " + StdHighestTaskPriority.ToString()
                        + @" AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + authorizationType
                        + @"' ORDER BY tmm.AddedDate DESC,isnull(tmm.TaskPriority,0) DESC ";
                    break;

                case "ToClose":
                    sql += @" WHERE (isnull(ta.isDone,0)=1 OR tmm.currentstatus='ToClose') AND ta.ResponsiblePersonId='" + logedInUser + "' AND ta.AuthorizationType='" + AuthorizationTypeEnum.CreatedBy + "' order by tmm.AddedDate ASC ";
                    break;

                default:
                    if (authorizationType == AuthorizationTypeEnum.AssignTo.ToString())
                        sql += @" AND ta.AuthorizationType='" + authorizationType + @"'
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

            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql);

            sql = @"SELECT k.*,EI.EmployeeName FROM (
                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id AuditId,isnull(ta.isDone,0) AS isDone,
                       ta.ResponsiblePersonId,ta.isRead,tm.currentstatus,ta.AuthorizationType,ta.DueDate,ta.AddedDate
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.CheckBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE isnull(ta.IsDone,0)=1  AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate())) 

                        UNION ALL

                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id,isnull(ta.isDone,0) AS isDone,ta.ResponsiblePersonId,
                        ta.isRead,tm.currentstatus,ta.AuthorizationType,ta.DueDate,ta.AddedDate
                          FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.CrossCheckBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                        WHERE isnull(ta.IsDone,0)=1  AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate())) 

                        UNION ALL

                        SELECT c.AuthType,TM.Id TaskManagerMasterId, ta.Id,isnull(ta.isDone,0) AS isDone,
                        ta.ResponsiblePersonId,ta.isRead,tm.currentstatus,ta.AuthorizationType,ta.DueDate,ta.AddedDate
                           FROM TaskManagerMaster TM
                        LEFT OUTER JOIN  (SELECT '" + AuthorizationTypeEnum.ApproveBy.ToString() + @"' AS AuthType) AS C ON 1=1
                        LEFT OUTER JOIN TaskAudit AS ta ON ta.authorizationType=c.AuthType AND tm.Id=ta.TaskManagerMasterId
                       WHERE isnull(ta.IsDone,0)=1  AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate())) 
                        ) AS K
                        left outer join EmployeeInformation EI on EI.SystemId=K.ResponsiblePersonId
                        WHERE k.TaskManagerMasterId IN (SELECT ta.TaskManagerMasterId
                                                          FROM TaskAudit AS ta WHERE ta.ResponsiblePersonId='" + logedInUser + @"')";
            //    ORDER BY K.TaskManagerMasterId,K.AuthType ";

            switch (flag)
            {
                case "NonAction":
                    sql += @" AND (isnull(k.isDone,0)=0 
                      AND k.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "'" +
                        " AND k.ResponsiblePersonId='" + logedInUser + "'" +
                        " AND k.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "'" +
                        "AND k.AuthorizationType='" + authorizationType + "')" +
                        "order by k.AddedDate ASC ";
                    break;

                case "NotRead":
                    sql += @" AND k.currentstatus='" + CurrentStatusEnum.Closed.ToString() + "'" +
                    " AND isnull(k.isRead,1)=0 And k.AuthorizationType='" + authorizationType + "' " +
                    "AND k.ResponsiblePersonId='" + logedInUser + "'" +
                    " AND k.AuthorizationType<>'" + AuthorizationTypeEnum.CreatedBy + "' " +
                    "order by k.AddedDate ASC ";
                    break;
            }

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

        public IEnumerable<object> GetTaskAccordingToRresponsiblePersonList(string EmpId, string authorizationType, string flag, string taskstatus)
        {
            try
            {

                if (taskstatus.ToUpper() == "CLOSED")
                    return GetClosedTaskAccordingToRresponsiblePersonListString(EmpId, authorizationType, flag);
                else
                    return GetTaskAccordingToRresponsiblePersonListString(EmpId, authorizationType, flag);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetUser(string EmpId)
        {

            try
            {
                var sqlLogin = @"SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType
                                FROM EmployeeInformation AS ei
                                WHERE systemid='" + EmpId + @"' order by employeename";

                var _loginUser = _sqlRepository.GetDataCollection(sqlLogin);
                if (_loginUser[0]["EmpType"].ToString().ToUpper() == "GUEST")
                {
                    var sql = @"SELECT * FROM (SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType,
                            isnull(D.UserName,'') Designation,DEPT.UserName Department
                              FROM EmployeeInformation AS ei 
                            INNER JOIN org.Position AS p ON p.Id=ei.PositionID
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=ei.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON P.DepartmentId=DEPT.Id
                            WHERE ISNULL(p.TaskManagementApplicable,0)=1 AND ei.GroupId=(SELECT GroupId FROM EmployeeInformation AS e WHERE e.SystemId='" + EmpId + @"')
                            AND ei.EmployeeStatus='active' 

                            UNION
                            
                            SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType,
                            isnull(D.UserName,'') Designation,DEPT.UserName Department
                              FROM EmployeeInformation AS ei 
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=ei.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON ei.DepartmentId=DEPT.Id
                            WHERE  isnull(empType,'')='Guest' AND ei.EmployeeStatus='active' and systemid<>'" + EmpId + @"') AS TEMP 

                        LEFT OUTER JOIN chat AS cp ON cp.EmployeeId=TEMP.Id AND cp.Id=( 
                            	          SELECT TOP 1 c.Id FROM ChatMaster AS cm
                                    INNER JOIN ChatParticipants AS cp ON cm.Id=cp.ChatMasterId AND cp.EmployeeId='" + EmpId + @"'  AND ISNULL(cp.IsRead,0)=0
                                    INNER JOIN Chat AS c ON c.ChatMasterId=cm.Id AND c.EmployeeId=TEMP.Id
                                    WHERE 
                                   
                                    isnull(cm.IsGroupChat,0)=0 
                                    AND (cm.FromId='" + EmpId + @"' OR cm.ToId='" + EmpId + @"')
                                    AND (cm.FromId=TEMP.Id OR cm.ToId=TEMP.Id)
                                    ORDER BY c.DateCreated DESC
                            )

                            ORDER BY EmployeeCode";
                    return _sqlRepository.GetDataCollection(sql, null);

                }
                else
                {
                    var sql = @"SELECT * FROM (SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType,
                            isnull(D.UserName,'') Designation,DEPT.UserName Department
                              FROM EmployeeInformation AS ei 
                            INNER JOIN org.Position AS p ON p.Id=ei.PositionID
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=ei.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON P.DepartmentId=DEPT.Id
                            WHERE ISNULL(p.TaskManagementApplicable,0)=1 --AND ei.PlantId=(SELECT plantid FROM EmployeeInformation AS e WHERE e.SystemId='" + EmpId + @"')
                            AND ei.EmployeeStatus='active' and systemid<>'" + EmpId + @"'

                            UNION
                            
                            SELECT ei.SystemId AS Id,ei.EmployeeCode,ei.EmployeeName,convert(bit,0) as IsConnected,isnull(ei.EmpType,'') AS EmpType,
                            isnull(D.UserName,'') Designation,DEPT.UserName Department
                              FROM EmployeeInformation AS ei 
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=ei.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON ei.DepartmentId=DEPT.Id
                            WHERE  isnull(empType,'')='Guest' AND ei.EmployeeStatus='active' and systemid<>'" + EmpId + @"') AS TEMP 

                        LEFT OUTER JOIN chat AS cp ON cp.EmployeeId=TEMP.Id AND cp.Id=( 
                            	          SELECT TOP 1 c.Id FROM ChatMaster AS cm
                                    INNER JOIN ChatParticipants AS cp ON cm.Id=cp.ChatMasterId AND cp.EmployeeId='" + EmpId + @"'  AND ISNULL(cp.IsRead,0)=0
                                    INNER JOIN Chat AS c ON c.ChatMasterId=cm.Id AND c.EmployeeId=TEMP.Id
                                    WHERE 
                                   
                                    isnull(cm.IsGroupChat,0)=0 
                                    AND (cm.FromId='" + EmpId + @"' OR cm.ToId='" + EmpId + @"')
                                    AND (cm.FromId=TEMP.Id OR cm.ToId=TEMP.Id)
                                    ORDER BY c.DateCreated DESC
                            )

                            ORDER BY EmployeeCode";
                    return _sqlRepository.GetDataCollection(sql, null);

                }


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetTaskCat()
        {
            try
            {
                var sql = @"select tsk.UserName as Text,Id as Value from hkp.TaskCategory tsk where Flag='ToDo'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetTaskSubCat()
        {
            try
            {
                var sql = @"select tsk.UserName as Text,Id as Value from hkp.TaskSubCategory tsk where Flag='ToDo'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetTaskData(string MasterId)
        {
            try
            {
                var sql = @"select tak.Id,tak.TaskType,tak.TaskDescription,tak.CurrentStatus,tak.AddedBy as CreatedBy,
                    tak.TaskPriority,tak.TaskDetailDescription,tc.UserName as TaskCategory,
                    tak.TaskCategoryId,tak.StoryPoint,tak.TaskSubCategoryId,sb.UserName as TaskSubCategory,au.Id as AuditId,
                    AuthorizationType,ResponsiblePersonId as AssignedId,ei.EmployeeName as AssignedName,ei.EmployeeCode as AssignedCode,CommitmentDate,
                    RevisedCommitmentDate, DueDate,au.AddedBy as CreatedBy,
                    isDone,au.TakenForNotification,isRead,isReadComment,tak.TaskTypeGroup from dbo.TaskManagerMaster tak 
                    left join dbo.TaskAudit au on au.TaskManagerMasterId=tak.Id
                    left join hkp.TaskCategory tc on tc.Id=tak.TaskCategoryId 
                    left join hkp.TaskSubCategory sb on sb.Id=tak.TaskSubCategoryId
                    left join dbo.EmployeeInformation ei on ei.SystemId =au.ResponsiblePersonId

                    where tak.Id='" + MasterId + "' and au.AuthorizationType='AssignTo'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetCheckBy(string MasterId)
        {
            try
            {

                var sql = @"select au.Id as AuditId,AuthorizationType,au.TaskManagerMasterId,
            ResponsiblePersonId as AssignedId,
            ei.EmployeeName as AssignedName,ei.EmployeeCode as AssignedCode,CommitmentDate,
            RevisedCommitmentDate, DueDate,au.AddedBy as CreatedBy,
            isDone,au.TakenForNotification,isRead,isReadComment from 
            dbo.TaskManagerMaster tak left join dbo.TaskAudit au on au.TaskManagerMasterId=tak.Id
            left join dbo.EmployeeInformation ei on ei.SystemId =au.ResponsiblePersonId
            where tak.Id='" + MasterId + "' and au.AuthorizationType='CheckBy'";

                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetApproveBy(string MasterId)
        {
            try
            {

                var sql = @"select au.Id as AuditId,AuthorizationType,au.TaskManagerMasterId,ResponsiblePersonId as AssignedId,
            ei.EmployeeName as AssignedName,ei.EmployeeCode as AssignedCode,
            CommitmentDate,RevisedCommitmentDate, DueDate,au.AddedBy as CreatedBy,isDone,au.TakenForNotification,isRead,isReadComment from 
            dbo.TaskManagerMaster tak left join dbo.TaskAudit au 
            on au.TaskManagerMasterId=tak.Id
            left join dbo.EmployeeInformation ei on ei.SystemId =au.ResponsiblePersonId
            where tak.Id='" + MasterId + "' and au.AuthorizationType='ApproveBy'";

                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetCrossCheckBy(string MasterId)
        {
            try
            {
                var sql = @"select au.Id as AuditId,AuthorizationType,au.TaskManagerMasterId,ResponsiblePersonId as AssignedId,
                ei.EmployeeName as AssignedName,ei.EmployeeCode as AssignedCode,CommitmentDate,RevisedCommitmentDate, DueDate,
                au.AddedBy as CreatedBy,isDone,au.TakenForNotification,isRead,isReadComment from 
                dbo.TaskManagerMaster tak left join dbo.TaskAudit au on au.TaskManagerMasterId=tak.Id
                left join dbo.EmployeeInformation ei on ei.SystemId =au.ResponsiblePersonId
                where tak.Id='" + MasterId + "' and au.AuthorizationType='CrossCheckBy'";

                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetCreatedBy(string MasterId)
        {
            try
            {
                var sql = @"select au.Id as AuditId,AuthorizationType,au.TaskManagerMasterId,ResponsiblePersonId as AssignedId,
                ei.EmployeeName as AssignedName,ei.EmployeeCode as AssignedCode,CommitmentDate,RevisedCommitmentDate, DueDate,
                au.AddedBy as CreatedBy,isDone,au.TakenForNotification,isRead,isReadComment from 
                dbo.TaskManagerMaster tak left join dbo.TaskAudit au on au.TaskManagerMasterId=tak.Id
                left join dbo.EmployeeInformation ei on ei.SystemId =au.ResponsiblePersonId
                where tak.Id='" + MasterId + "' and au.AuthorizationType='CreatedBy'";

                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetEmpName(string EmpId)
        {
            try
            {
                var sql = @"select emp.EmployeeName as EmpName,emp.SystemId as EmployeeId,emp.EmployeeCode from dbo.EmployeeInformation emp where isnull(SystemId,'')='" + EmpId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetComments(string Md)
        {
            try
            {
                var sql = @"select Id,TaskManagerMasterId,CommentText,CreatedTime from dbo.TaskComments where TaskManagerMasterId='" + Md + "' Order By CreatedTime";
                var Sql = @"select Id,TaskManagerMasterId,sub.TaskDetail as Text from dbo.TaskManagerSubTasks sub where TaskManagerMasterId='" + Md + "' order by AddedDate ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSubTasks(string Md)
        {
            try
            {
                var Sql = @"select Id,TaskManagerMasterId,sub.TaskDetail as Text from dbo.TaskManagerSubTasks sub where TaskManagerMasterId='" + Md + "' order by AddedDate ";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CommentsCreate(string MId, IEnumerable<TaskCommentsData> DataToSavey)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.TaskComments";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (DataToSavey.Count() == 0)
                    return "";

                List<TaskCommentsData> items = DataToSavey.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";

                foreach (TaskCommentsData item in DataToSavey)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "TC" + _Id;
                        dr["TaskManagerMasterId"] = MId;
                        dr["CommentText"] = item.CommentText;
                        dr["CreatedById"] = item.CreatedById;
                        dr["CreatedTime"] = System.DateTime.Now.ToString();
                        dr["TaskAthorizationType"] = item.TaskAthorizationType;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dsMaster.Tables[0].Rows.Add(dr);

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                }
                return "true";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string TaskAuditCreate(string MId, IEnumerable<TaskAuditData> DataToSavex)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.TaskAudit";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSavex.Count() == 0)
                    return "";

                foreach (TaskAuditData item in DataToSavex)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + item.Id + "'", out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = "AU" + _Id;
                        dr["TaskManagerMasterId"] = MId;
                        dr["AuthorizationType"] = item.AuthorizationType;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["Remarks"] = DBNull.Value;
                        dr["CommitmentDate"] = DBNull.Value;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["DueDate"] = item.DueDate;
                        dr["RevisedCommitmentDate"] = DBNull.Value;
                        dr["isDone"] = item.isDone;
                        dr["isRead"] = item.isRead;
                        dr["isReadComment"] = item.isReadComment;
                        dr["TakenForNotification"] = item.TakenForNotification;
                        dsMaster.Tables[0].Rows.Add(dr);

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);

                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["TaskManagerMasterId"] = MId;
                        dr["AuthorizationType"] = item.AuthorizationType;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["Remarks"] = DBNull.Value;
                        dr["CommitmentDate"] = DBNull.Value;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["DueDate"] = item.DueDate;
                        dr["RevisedCommitmentDate"] = DBNull.Value;
                        dr["isDone"] = item.isDone;
                        dr["isRead"] = item.isRead;
                        dr["isReadComment"] = item.isReadComment;
                        dr["TakenForNotification"] = item.TakenForNotification;
                        dr.EndEdit();

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);

                    }
                }
                return "true";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string TaskAuditUpdate(string MId, IEnumerable<TaskAuditData> DataToSavex)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.TaskAudit";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSavex.Count() == 0)
                    return "";

                foreach (TaskAuditData item in DataToSavex)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + item.Id + "'", out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["TaskManagerMasterId"] = MId;
                        dr["AuthorizationType"] = item.AuthorizationType;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["Remarks"] = DBNull.Value;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["DueDate"] = item.DueDate;
                        dr["isDone"] = item.isDone;
                        dr["isRead"] = item.isRead;
                        dr["isReadComment"] = item.isReadComment;
                        dr["TakenForNotification"] = item.TakenForNotification;
                        if (item.CommitmentDate != null)
                        {
                            dr["CommitmentDate"] = item.CommitmentDate;
                            dr["RevisedCommitmentDate"] = DBNull.Value;
                        }
                        if (item.RevisedCommitmentDate != null)
                        {
                            dr["RevisedCommitmentDate"] = item.RevisedCommitmentDate;

                        }
                        dr.EndEdit();

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);

                    }
                }
                return "true";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string TaskCreate(IEnumerable<TaskMasterData> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.TaskManagerMaster";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<TaskMasterData> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";

                foreach (TaskMasterData item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "TD" + _Id;
                        dr["TaskType"] = item.TaskType;
                        dr["TaskDescription"] = item.TaskDescription;
                        dr["CurrentStatus"] = item.CurrentStatus;
                        dr["TaskPriority"] = item.TaskPriority;
                        dr["TaskCategoryId"] = item.TaskCategoryId;
                        dr["TaskDetailDescription"] = item.TaskDetailDescription;
                        dr["TaskSubCategoryId"] = item.TaskSubCategoryId;
                        dr["TaskSchedulerMasterId"] = item.TaskSchedulerMasterId;
                        dr["IssueTransactionId"] = item.IssueTransactionId;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["ClosingDate"] = DBNull.Value;
                        dr["LastExecutionDate"] = DBNull.Value;
                        dr["NextExecutionDate"] = DBNull.Value;
                        dr["NoOfOccurences"] = item.NoOfOccurences;
                        dr["IsExpiredSchedule"] = item.IsExpiredSchedule;
                        dr["ParentTaskManagerMasterId"] = item.ParentTaskManagerMasterId;
                        dr["TaskTypeGroup"] = item.TaskTypeGroup;
                        dr["TNATasksId"] = item.TNATasksId;
                        dr["TakenForNotification"] = item.TakenForNotification;
                        dr["StoryPoint"] = item.StoryPoint;
                        dr["isOwnTask"] = item.isOwnTask;
                        dr["ClosedBy"] = item.ClosedBy;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["TaskType"] = item.TaskType;
                        dr["TaskDescription"] = item.TaskDescription;
                        dr["CurrentStatus"] = item.CurrentStatus;
                        dr["TaskPriority"] = item.TaskPriority;
                        dr["TaskCategoryId"] = item.TaskCategoryId;
                        dr["TaskDetailDescription"] = item.TaskDetailDescription;
                        dr["TaskSubCategoryId"] = item.TaskSubCategoryId;
                        dr["TaskSchedulerMasterId"] = item.TaskSchedulerMasterId;
                        dr["IssueTransactionId"] = item.IssueTransactionId;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["ClosingDate"] = DBNull.Value;
                        dr["LastExecutionDate"] = DBNull.Value;
                        dr["NextExecutionDate"] = DBNull.Value;
                        dr["NoOfOccurences"] = item.NoOfOccurences;
                        dr["IsExpiredSchedule"] = item.IsExpiredSchedule;
                        dr["ParentTaskManagerMasterId"] = item.ParentTaskManagerMasterId;
                        dr["TaskTypeGroup"] = item.TaskTypeGroup;
                        dr["TNATasksId"] = item.TNATasksId;
                        dr["TakenForNotification"] = item.TakenForNotification;
                        dr["StoryPoint"] = item.StoryPoint;
                        dr["isOwnTask"] = item.isOwnTask;
                        dr["ClosedBy"] = item.ClosedBy;

                        dr.EndEdit();
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MasterId;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string SubTaskCreate(string MId, IEnumerable<TaskSubTasksData> DataToSavez)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.TaskManagerSubTasks";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (DataToSavez.Count() == 0)
                    return "";

                List<TaskSubTasksData> items = DataToSavez.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";


                foreach (TaskSubTasksData item in DataToSavez)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "TC" + _Id;
                        dr["TaskManagerMasterId"] = MId;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["TaskDetail"] = item.TaskDetail;
                        dr["IsDone"] = item.IsDone;
                        dr["Remarks"] = item.Remarks;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dsMaster.Tables[0].Rows.Add(dr);

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                }
                return "true";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public IEnumerable<object> GetUnreadTasks(string EmpId)
        {
            try
            {
                var sql = @"select * from (SELECT distinct tmm.*,'' AS BuyerName,isnull(tsc.UserName,'') AS TaskCategory ,TSSC.UserName AS TaskSubCategory,'' AS SearchDataTemp
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
                              
                                  INNER JOIN [TaskAudit] ta ON ta.TaskManagerMasterId = tmm.Id  AND ta.ResponsiblePersonId='" + EmpId + @"' 
                                LEFT JOIN [TaskAudit] tTo ON tTo.TaskManagerMasterId = tmm.Id AND tto.AuthorizationType='AssignTo'
                                LEFT JOIN [TaskAudit] tBy ON tBy.TaskManagerMasterId = tmm.Id AND tBy.AuthorizationType='CreatedBy'

                                INNER JOIN [EmployeeInformation] Tasto ON Tasto.SystemId = tTo.ResponsiblePersonId  
                                INNER JOIN [EmployeeInformation] AasBy ON AasBy.SystemId = tBy.ResponsiblePersonId

                                INNER JOIN [EmployeeInformation] asto ON asto.SystemId = ta.ResponsiblePersonId
                                WHERE isnull(ta.isRead,0)=0 
                                AND tBy.ResponsiblePersonId<>'" + EmpId + @"'
                               
                                AND isnull(tmm.CurrentStatus,'')<>'Closed'
                        ) AS K WHERE k.RNK=1  ORDER BY k.AddedDate DESC";
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetUnreadComments(string EmpId)
        {
            try
            {
                var sql = @"select * from (SELECT distinct tmm.*,'' AS BuyerName,isnull(tsc.UserName,'') AS TaskCategory ,TSSC.UserName AS TaskSubCategory,'' AS SearchDataTemp
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
                                WHERE isnull(ta.isReadComment,0)=0 AND ta.ResponsiblePersonId='" + EmpId + @"' AND isnull(tmm.CurrentStatus,0)<>'Closed'
                    ) AS K WHERE k.RNK=1";
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        
        public IEnumerable<object> GetEmpDetails(string Name)
        {
            try
            {
                var sql = @"select emp.SystemId as EmpId,emp.EmployeeName,emp.EmployeeCode,dx.StandardName as Department,
                    emp.EmpType,d.StandardName as Designation
                                from dbo.EmployeeInformation emp left join hkp.LegalDesignation d
                    on d.Id=emp.LegalDesignationId left join org.Department dx on dx.Id=emp.DepartmentId
                                where EmployeeName='" + Name + "'";
                return _sqlRepository.GetDataCollection(sql, null);



            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string UpdateToDoMasterStatusForToDo(string MId, IEnumerable<TaskModelData> DataToSavea)
        {



            try
            {
                string TaskManagerMasterId = MId;
                List<TaskModelData> items = DataToSavea.ToList();
                bool closed = items[0].closed;
                string EmpId = items[0].EmpId;
                string Ip = items[0].Ip;
                string authorizationtype = items[0].authorizationtype;



                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskManagerMaster  where Id='" + TaskManagerMasterId + "'", out dsMaster, false, "1");



                DataSet a, dsAuthorization;
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where TaskManagerMasterId='" + TaskManagerMasterId + "' AND AuthorizationType='" + AuthorizationTypeEnum.CreatedBy.ToString() + "'", out a, false, "1");
                con.OpenDataSetThroughAdapter("select * from TaskAudit  where TaskManagerMasterId='" + TaskManagerMasterId + "' AND AuthorizationType='" + authorizationtype + "'", out dsAuthorization, false, "1");





                DataRow dr = dsMaster.Tables[0].Rows[0];

                try
                {
                    dr.BeginEdit();
                    if (closed == true)
                    {

                        if (authorizationtype == AuthorizationTypeEnum.CreatedBy.ToString())
                        {
                            dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                            dr["ClosingDate"] = System.DateTime.Now.ToString();
                            dr["ClosedBy"] = EmpId;



                        }
                        else if (authorizationtype == AuthorizationTypeEnum.AssignTo.ToString())
                        {
                            if (a.Tables[0].Rows[0]["ResponsiblePersonId"].ToString() == EmpId)
                            {
                                dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                                dr["ClosingDate"] = System.DateTime.Now.ToString();
                                dr["ClosedBy"] = EmpId;



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
                    else
                    {




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
                    dr["UpdatedFromIP"] = Ip;
                    dr.EndEdit();
                }
                catch (Exception)
                {




                }




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsAuthorization);



                return "true";



            }
            catch (Exception ex)
            {



                return ex.ToString();
            }



        }

        public string UpdateToDoMasterStatus(string MId, IEnumerable<TaskModelData> DataToSaveb)
        {
            try
            {
                string TaskManagerMasterId = MId; List<TaskModelData> items = DataToSaveb.ToList();
                bool closed = items[0].closed; string EmpId = items[0].EmpId;
                string Ip = items[0].Ip; string authorizationtype = items[0].authorizationtype;



                DataSet dsMaster; ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
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
                                dr["ClosedBy"] = EmpId;



                            }
                            else
                            {
                                dr["CurrentStatus"] = CurrentStatusEnum.ToClose.ToString();
                                if (authorizationtype == AuthorizationTypeEnum.CreatedBy.ToString())
                                {
                                    dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                                    dr["ClosingDate"] = System.DateTime.Now.ToString();
                                    dr["ClosedBy"] = EmpId;



                                }
                                else if (authorizationtype == AuthorizationTypeEnum.AssignTo.ToString())
                                {
                                    if (dsCreatedBy.Tables[0].Rows[0]["ResponsiblePersonId"].ToString() == EmpId)
                                    {
                                        dr["CurrentStatus"] = CurrentStatusEnum.Closed.ToString();
                                        dr["ClosingDate"] = System.DateTime.Now.ToString();
                                        dr["ClosedBy"] = EmpId;



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
                                dr["ClosedBy"] = EmpId;



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
                    dr["UpdatedFromIP"] = Ip;
                    dr.EndEdit();
                }
                catch (Exception)
                {

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsAuthorization);

                return "true";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }



        }
    }
}

public class TaskModelData
{
    public string Ip { get; set; }
    public string EmpId { get; set; }
    public string authorizationtype { get; set; }
    public bool closed { get; set; }



}
public class TaskMasterData
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string ClosedBy{get;set;}
        public string TaskDescription { get; set; }
        public string TaskType { get; set; }
        public string CurrentStatus { get; set; }
        public decimal TaskPriority { get; set; }
        public string TaskCategoryId { get; set; }
        public string TaskDetailDescription { get; set; }
        public string TaskSubCategoryId { get; set; }        
        public string TaskSchedulerMasterId { get; set; }
        public string  IssueTransactionId { get; set; }
        public string LastExecutionDate { get; set; }
        public string NextExecutionDate { get; set; }
        public decimal NoOfOccurences { get; set; }
        public string IsExpiredSchedule { get; set; }
        public string ParentTaskManagerMasterId { get; set; }
        public string TaskTypeGroup { get; set; }
        public string ClosingDate { get; set; }
        public string TNATasksId { get; set; }
        public string TakenForNotification { get; set; }
        public decimal StoryPoint { get; set; }
        public string isOwnTask { get; set; }
      
        #endregion Scalar Properties

        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string AddedFromIP { get; set; }

        #endregion Audit Properties

    }

    public class TaskCommentsData
    {
        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string AddedFromIP { get; set; }

        #endregion Audit Properties

        #region Scalar Properties

        public string Id { get; set; }
        public string TaskManagerMasterId { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string CommentText { get; set; }
        public string TaskAthorizationType { get; set; }
    
        #endregion Scalar Properties
    }

    public class TaskSubTasksData
    {
        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string AddedFromIP { get; set; }

        #endregion Audit Properties

        #region Scalar Properties

        public string Id { get; set; }
        public string TaskManagerMasterId { get; set; }
        public string ResponsiblePersonId { get; set; }     
        public string TaskDetail { get; set; }
        public string IsDone { get; set; }
        public string Remarks { get; set; }

        #endregion Scalar Properties
    }

    public class TaskAuditData
    {
        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string AddedFromIP { get; set; }

        #endregion Audit Properties

        #region Scalar Properties

        public string Id { get; set; }
        public string TaskManagerMasterId { get; set; }
        public string AuthorizationType { get; set; }
        public DateTime? CommitmentDate { get; set; }
        public DateTime? RevisedCommitmentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string Remarks { get; set; }
        public string isDone { get; set; }
        public string isRead { get; set; }
        public string isReadComment { get; set; }
        public string TakenForNotification { get; set; }

        #endregion Scalar Properties
    }



