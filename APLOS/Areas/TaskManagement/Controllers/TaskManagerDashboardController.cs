#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskManagerDashboardController : BaseController
    {
        private static string Flag = TaskCategoryFlagEnum.TNA.ToString();
        string TableName1 = "dbo.TaskManagerMaster";
        string TableName2 = "dbo.TaskAudit";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public TaskManagerDashboardController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName1 + " WHERE FLAG='" + Flag + "'"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetTaskManagerDashboardList(string fromDate, string ToDate, string TaskTypeGroup)
        {
            string Today = System.DateTime.Now.ToString("dd-MMM-yyyy");
            string TaskTypeGroupSql = "";
            if (TaskTypeGroup != "All")
                TaskTypeGroupSql = "AND TM.TaskTypeGroup='" + TaskTypeGroup + @"'";

            TaskTypeGroupSql += " AND isnull(TM.isOwnTask,0)=0 ";

            //string closedSql = "='Closed'";
            //if (TaskStatus != "Closed")
            //    closedSql = "<>'Closed'";
            fromDate = Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy");
            string sql = @"SELECT K.*,
tc.Total AS TotalCreated,tc.ToDO AS TotalCreatedToDo,tc.Issue AS TotalCreatedIssue,tc.TNA AS TotalCreatedTNA,
TROD.Total AS TotalOverDueUnread,TROD.ToDO AS OverDueUnreadToDo,TROD.Issue AS OverDueUnreadIssue,TROD.TNA AS OverDueUnreadTNA,
TROR.Total AS TotalOverDueRead,TROR.ToDO AS OverDueReadToDo,TROR.Issue AS OverDueReadIssue,TROR.TNA AS OverDueReadTNA,
TTSK.Total AS TodayTask,TTSK.ToDO AS TodayTaskToDo,TTSK.Issue AS TodayTaskIssue,TTSK.TNA AS TodayTaskTNA,
TOCL.Total AS TaskToClose ,TOCL.ToDO AS TaskToCloseToDo,TOCL.Issue AS TaskToCloseIssue,TOCL.TNA AS TaskToCloseTNA

FROM (
	SELECT distinct 
                        P.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,
                        isnull(tm.TaskCategoryId,'')TaskCategoryId,isnull(tm.TaskSubCategoryId,'')AS TaskSubCategoryId,

                        isnull(DTO.UserName,'') AS Department,isnull(EATO.EmployeeName,'') AS AssignToEmployeeName,isnull(EAB.EmployeeName,'') AS AssignByEmployeeName,
                        isnull(tc.UserName,'') AS TaskCategory,isnull(tsc.UserName,'') AS TaskSubCategory--,format(ATO.DueDate,'dd-MMM-yyyy') AS DueDate

                   FROM TaskManagerMaster AS tm
                        LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                        LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'

                        LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                        LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                        LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
                        LEFT OUTER JOIN org.Department AS DTO ON dto.Id=P.DepartmentId

                        LEFT OUTER JOIN hkp.TaskCategory AS tc ON tm.TaskCategoryId=tc.Id
                        LEFT OUTER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=tm.TaskSubCategoryId
                        WHERE isnull(TM.isOwnTask,0)=0 AND convert(date,tm.AddedDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' OR  ATO.DueDate BETWEEN  '" + fromDate + @"' and '" + ToDate + @"'
                        ) AS K
LEFT OUTER JOIN (SELECT 
                                'TotalCreated' AS Particular,
                                P.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,
								isnull(tm.TaskCategoryId,'')TaskCategoryId,isnull(tm.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                COUNT(*) AS Total, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END) AS ToDO, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END) AS Issue,
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
								LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
								LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'
								LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                                LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=eato.DepartmentId
                                WHERE convert(date,tm.AddedDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"'  " + TaskTypeGroupSql + @"
                                         GROUP BY eato.DepartmentId,ATO.ResponsiblePersonId,AB.ResponsiblePersonId,
										tm.TaskCategoryId,tm.TaskSubCategoryId) TC
										ON tc.DepartmentId=k.DepartmentId AND tc.AssignToId=k.AssignToId AND tc.AssignById=k.AssignById
										AND tc.TaskCategoryId=k.TaskCategoryId and tc.TaskSubCategoryId=k.TaskSubCategoryId
LEFT OUTER JOIN (SELECT 
                                'TotalToClose' AS Particular,
                                p.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,
								isnull(tm.TaskCategoryId,'')TaskCategoryId,isnull(tm.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                COUNT(*) AS Total, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END) AS ToDO, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END) AS Issue,
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
								LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
								LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'
								LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                                LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=p.DepartmentId
                                WHERE convert(date,tm.AddedDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"' AND TM.CurrentStatus='ToClose' " + TaskTypeGroupSql + @"
                                         GROUP BY p.DepartmentId,ATO.ResponsiblePersonId,AB.ResponsiblePersonId,
										tm.TaskCategoryId,tm.TaskSubCategoryId) TOCL
										ON TOCL.DepartmentId=k.DepartmentId AND TOCL.AssignToId=k.AssignToId AND TOCL.AssignById=k.AssignById
										AND TOCL.TaskCategoryId=k.TaskCategoryId and TOCL.TaskSubCategoryId=k.TaskSubCategoryId
							
										
LEFT OUTER JOIN (SELECT 
                                'OverdueRead' AS Particular,
                                p.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,
								isnull(tm.TaskCategoryId,'')TaskCategoryId,isnull(tm.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                COUNT(*) AS Total, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END) AS ToDO, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END) AS Issue,
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
								LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
								LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'
								LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                                LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=p.DepartmentId
                                   WHERE convert(date,ATO.DueDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' AND  convert(date,ATO.DueDate) <convert(date,'" + Convert.ToDateTime(Today).ToString("dd-MMM-yyyy") + @"') AND tm.CurrentStatus<>'Closed' AND isnull(ATO.isRead,0)=0  " + TaskTypeGroupSql + @"
										GROUP BY p.DepartmentId,ATO.ResponsiblePersonId,AB.ResponsiblePersonId,
										tm.TaskCategoryId,tm.TaskSubCategoryId) TROD
										ON TROD.DepartmentId=k.DepartmentId AND TROD.AssignToId=k.AssignToId AND TROD.AssignById=k.AssignById
										AND TROD.TaskCategoryId=k.TaskCategoryId and TROD.TaskSubCategoryId=k.TaskSubCategoryId
										
										
LEFT OUTER JOIN (SELECT 
                                'OverdueUnRead' AS Particular,
                                p.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,
								isnull(tm.TaskCategoryId,'')TaskCategoryId,isnull(tm.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                COUNT(*) AS Total, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END) AS ToDO, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END) AS Issue,
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
								LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
								LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'
								LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                                LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=p.DepartmentId
                                   WHERE  convert(date,ATO.DueDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' AND   convert(date,ATO.DueDate) <'" + Convert.ToDateTime(ToDate).ToString("dd-MMM-yyyy") + @"' AND tm.CurrentStatus<>'Closed' AND isnull(ATO.isRead,0)=1  " + TaskTypeGroupSql + @"
										GROUP BY p.DepartmentId,ATO.ResponsiblePersonId,AB.ResponsiblePersonId,
										tm.TaskCategoryId,tm.TaskSubCategoryId) TROR
										ON TROR.DepartmentId=k.DepartmentId AND TROR.AssignToId=k.AssignToId AND TROR.AssignById=k.AssignById
										AND TROR.TaskCategoryId=k.TaskCategoryId and TROR.TaskSubCategoryId=k.TaskSubCategoryId
										
LEFT OUTER JOIN (SELECT 
                                'TODAYTASK' AS Particular,
                                p.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,
								isnull(tm.TaskCategoryId,'')TaskCategoryId,isnull(tm.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                COUNT(*) AS Total, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END) AS ToDO, 
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END) AS Issue,
                                SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
								LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
								LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'
								LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                                LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=p.DepartmentId
                                   WHERE convert(date,ATO.DueDate) BETWEEN  '" + Today + @"' and '" + Today + @"' AND tm.CurrentStatus<>'Closed'  " + TaskTypeGroupSql + @"
										GROUP BY p.DepartmentId,ATO.ResponsiblePersonId,AB.ResponsiblePersonId,
										tm.TaskCategoryId,tm.TaskSubCategoryId) TTSK
										ON TTSK.DepartmentId=k.DepartmentId AND TTSK.AssignToId=k.AssignToId AND TTSK.AssignById=k.AssignById
										AND TTSK.TaskCategoryId=k.TaskCategoryId and TTSK.TaskSubCategoryId=k.TaskSubCategoryId
                WHERE TTSK.Total>0 OR TROR.Total>0 OR TROD.Total>0 OR TOCL.Total>0 OR TC.Total>0";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetMinDueDate()
        {
            return Json(_sqlRepository.GetDataCollection(@"SELECT MIN(DueDate) FromDate,format(getdate(),'dd-MMM-yyyy') AS ToDate  from taskAudit
UNION
SELECT MIN(DueDate) FromDate,format(getdate(),'dd-MMM-yyyy') AS ToDate  from taskAudit Where ISNULL(isDone,0)<>1"), JsonRequestBehavior.AllowGet);
        }

       
        [HttpPost, Authorize]
        public ActionResult GetTaskStatistics(string fromDate, string ToDate, string TaskTypeGroup)
        {
            string Today = DateTime.Now.ToString("dd-MMM-yyyy");
            string TaskTypeGroupSql = "";
            if (TaskTypeGroup != "All")
                TaskTypeGroupSql = "AND TM.TaskTypeGroup='" + TaskTypeGroup + @"'";

            TaskTypeGroupSql += " AND isnull(TM.isOwnTask,0)=0 ";

            string sql = @"
								SELECT 'totalcreatedspline' AS Value,'Total Created (For The Period)' AS Particular,COUNT(*) AS Total, NULL as Sparkline,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
                                WHERE convert(date,tm.AddedDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' " + TaskTypeGroupSql + @"


                                UNION ALL
                                	SELECT 'totalclosedspline' AS Value,'Total Closed (For The Period)' AS Particular,COUNT(*) AS Total, NULL as Sparkline,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
                                WHERE convert(date,tm.ClosingDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' AND tm.CurrentStatus='Closed' " + TaskTypeGroupSql + @"
                                UNION ALL
                                   	SELECT 'totalclosedontimespline' AS Value,'Total Closed-On Time (For The Period)' AS Particular,COUNT(*) AS Total, NULL as Sparkline,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
                                LEFT OUTER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tm.Id AND ta.AuthorizationType='AssignTo'
                                WHERE convert(date,tm.ClosingDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' AND CONVERT(DATE, ta.DueDate)<=convert(date,tm.ClosingDate) 
                                AND tm.CurrentStatus='Closed' " + TaskTypeGroupSql + @"
                               
                                 UNION ALL
                                   	SELECT 'totalcloseddelayedspline' AS Value,'Total Closed-Delayed (For The Period)' AS Particular,COUNT(*) AS Total, NULL as Sparkline,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
                                LEFT OUTER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tm.Id AND ta.AuthorizationType='AssignTo'
                                WHERE convert(date,tm.ClosingDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' AND CONVERT(DATE, ta.DueDate)>convert(date,tm.ClosingDate) 
                                AND tm.CurrentStatus='Closed' " + TaskTypeGroupSql + @"
                               
                              
                                UNION ALL
                                SELECT 
                                'todaytaskspline' AS Value, 'Today Task' AS Particular,COUNT(*) AS Total,  NULL as Sparkline,
                               ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                                   FROM 
                                 TaskManagerMaster AS TM
                                 INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tm.Id AND ta.AuthorizationType='AssignTo'
                                WHERE convert(date,ta.DueDate) BETWEEN '" + Today + @"' AND '" + Today + @"' 
                                AND isnull(tm.CurrentStatus,'')<>'Closed' " + TaskTypeGroupSql + @"

                                UNION ALL
                                SELECT 
                                 'taskoverduereadspline' AS Value,'Task Overdue Read' AS Particular,COUNT(*) AS Total,  NULL as Sparkline,
                               ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                                   FROM 
                                 TaskManagerMaster AS TM
                                 INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tm.Id AND ta.AuthorizationType='AssignTo'
                                WHERE  convert(date,ta.DueDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' AND convert(date,ta.DueDate)<convert(date,'" + Convert.ToDateTime(Today).ToString("dd-MMM-yyyy") + @"') AND isnull(tm.CurrentStatus,'')<>'Closed' 
                                AND isnull(ta.isRead,0)=1 " + TaskTypeGroupSql + @"

                                UNION ALL
                                SELECT 
                                 'taskoverdueunreadspline' AS Value,'Task Overdue Unread' AS Particular,COUNT(*) AS Total,  NULL as Sparkline,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                                  FROM 
                                 TaskManagerMaster AS TM
                                 INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tm.Id AND ta.AuthorizationType='AssignTo'
                                WHERE  convert(date,ta.DueDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' AND convert(date,ta.DueDate)<convert(date,'" + Convert.ToDateTime(Today).ToString("dd-MMM-yyyy") + @"') AND isnull(tm.CurrentStatus,'')<>'Closed' 
                                AND isnull(ta.isRead,0)=0 " + TaskTypeGroupSql + @"

                                UNION ALL
                                SELECT 
                                 'futuretaskspline' AS Value,'Future Task (For The Period)' AS Particular,COUNT(*) AS Total,  NULL as Sparkline,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                                  FROM 
                                 TaskManagerMaster AS TM
                                 INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tm.Id AND ta.AuthorizationType='AssignTo'
                                WHERE convert(date,ta.DueDate) BETWEEN '" + fromDate + @"' AND '" + ToDate +
                                @"' AND convert(date,ta.DueDate)>'" + Convert.ToDateTime(Today).ToString("dd-MMM-yyyy")
                                + @"' AND tm.CurrentStatus<>'Closed' 
                                 " + TaskTypeGroupSql + @"

                               -- UNION ALL

                              --  SELECT 
                              --  'tasktoclosespline' AS Value, 'Task To Close' AS Particular,COUNT(*) AS Total,  NULL as Sparkline,
                              -- ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                              --  ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                              --  ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                              --     FROM 
                              --   TaskManagerMaster AS TM
                              --  WHERE convert(date,tm.AddedDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' 
                                --AND TM.CurrentStatus='ToClose'  " + TaskTypeGroupSql + @"";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetTaskStatisticsForPieChart(string fromDate, string ToDate, string TaskTypeGroup)
        {
            string TaskTypeGroupSql = "";
            if (TaskTypeGroup != "All")
                TaskTypeGroupSql = "AND TM.TaskTypeGroup='" + TaskTypeGroup + @"'";
            TaskTypeGroupSql += " AND isnull(TM.isOwnTask,0)=0 ";

            string sql = @"SELECT 
                                'totalcreatedspline' AS Value,'Total Created' AS Particular,COUNT(*) AS Total, NULL as Sparkline,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='ToDo' THEN 1 ELSE 0 END),0) AS ToDo, 
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='Issue' THEN 1 ELSE 0 END),0) AS Issue,
                                ISNULL(SUM(CASE WHEN isnull(tm.TaskTypeGroup,'')='TNA' THEN 1 ELSE 0 END),0) AS TNA
                                FROM 
                                TaskManagerMaster AS TM
                                WHERE convert(date,tm.AddedDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"'
                               ";
            var created = _sqlRepository.GetDataCollection(sql);
            Dictionary<string, object> TasksStat = new Dictionary<string, object>();
            foreach (TaskCategoryFlagEnum item in Enum.GetValues(typeof(TaskCategoryFlagEnum)))
            {
                string _sql = @"SELECT 
                                '" + item.ToString() + @"' AS Value,'Total Created' AS Particular,COUNT(*) AS Total, NULL as Sparkline,
                                ISNULL(SUM(CASE WHEN isnull(tm.CurrentStatus,'')='ToStart' THEN 1 ELSE 0 END),0) AS ToStart, 
                                ISNULL(SUM(CASE WHEN isnull(tm.CurrentStatus,'')='InProgress' THEN 1 ELSE 0 END),0) AS InProgress, 
                                ISNULL(SUM(CASE WHEN isnull(tm.CurrentStatus,'')='ToClose' THEN 1 ELSE 0 END),0) AS ToClose, 
                                ISNULL(SUM(CASE WHEN isnull(tm.CurrentStatus,'')='Closed' THEN 1 ELSE 0 END),0) AS Closed

                                FROM 
                                TaskManagerMaster AS TM
                                WHERE tm.TaskTypeGroup='" + item.ToString() + @"' AND convert(date,tm.AddedDate) BETWEEN '" + fromDate + @"' AND '" + ToDate + @"' " + TaskTypeGroupSql;

                TasksStat.Add(item.ToString(), _sqlRepository.GetDataCollection(_sql));
            }

            return Json(new { Master = created, Tasks = TasksStat }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetTaskStatisticsBackup(Dictionary<string, object> FilteredData, string fromDate, string ToDate, string taskType)
        {
            string taskTypeWC = "";
            string taskTypeGroupBy = "";
            string taskTypeColumn = "";
            string taskTypeGroupByExtra = "";

            if (string.IsNullOrEmpty(taskType))
            {
                taskTypeWC = "";
                taskTypeGroupBy = "group by tm.currentstatus, AB.DueDate ";
                taskTypeColumn = "";
                //taskTypeColumn = "";
            }
            else
            {
                taskTypeColumn = "TaskType,";
                taskTypeWC = "AND TaskType = '" + taskType + @"'";
                taskTypeGroupBy = "GROUP BY tm.currentstatus, AB.DueDate ,tm.TaskType";
                taskTypeGroupByExtra = " GROUP BY TaskType";
            }
            string sql = @"select  TTYPE, sum(dd.NoOfTasks) NoOfTasks, sum(dd.TotalClosed) TotalClosed, Sum(dd.ToStart) ToStart, Sum(dd.InProgress) InProgress, Sum(dd.ToClose) ToClose from
                (
            SELECT  TT.TTYPE, count(case when isnull(tm.Id,'')<>'' THEN 1 ELSE 0 END) NoOfTasks,
		
                        
                        case when tm.currentstatus = 'Closed' then count(tm.Id)  end TotalClosed,
                        
                        case when tm.currentstatus = 'ToStart' then count(tm.Id)  end ToStart,
                        case when tm.currentstatus = 'InProgress' then count(tm.Id)  end InProgress,
                        case when tm.currentstatus = 'ToClose' then count(tm.Id)  end ToClose
							
                      FROM 
						(select 'ToDo' AS TTYPE UNION select 'Issue' UNION select 'TNA' UNION select 'UpdateAudit' UNION select 'FollowUpAudit' UNION select 'InternalAudit' UNION select 'ExternalAudit') AS TT
						
						left outer join TaskManagerMaster AS tm on tm.TaskType=tt.TTYPE
                        LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                        LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'

                        LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                        LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                        LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
                        LEFT OUTER JOIN org.Department AS DTO ON dto.Id=p.DepartmentId

                        LEFT OUTER JOIN hkp.TaskCategory AS tc ON tm.TaskCategoryId=tc.Id
                        LEFT OUTER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=tm.TaskSubCategoryId

						where 
                        (Convert(date,AB.DueDate) between Convert(date,'" + fromDate + @"') and Convert(date,'" + ToDate + @"')) and 
                        isnull(DTO.Id,'') IN (" + FilteredData["DepartmentId"] + @") AND
						isnull(ATO.ResponsiblePersonId,'') IN (" + FilteredData["AssignToId"] + @") AND
						isnull(AB.ResponsiblePersonId,'') IN (" + FilteredData["AssignById"] + @") AND
						isnull(TC.Id,'') IN (" + FilteredData["TaskCategoryId"] + @") AND
                        --isnull(AB.DueDate,'') IN (" + FilteredData["DueDate"] + @") " + taskTypeWC + @" AND
						isnull(TSC.Id,'') IN (" + FilteredData["TaskSubCategoryId"] + @") group by  TT.TTYPE, tm.currentstatus, AB.DueDate) dd group by TTYPE";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskManagerReports(string tDept, string aToEName, string aByEName, string tCategory, string tSubCategory, string dueDate)
        {
            string sql = @"
                        SELECT* FROM(SELECT distinct
                        p.DepartmentId, ATO.ResponsiblePersonId AS AssignToId, AB.ResponsiblePersonId AS AssignById,
                        isnull(tm.TaskCategoryId,'')TaskCategoryId,isnull(tm.TaskSubCategoryId, '')AS TaskSubCategoryId,

                         isnull(DTO.UserName, '') AS Department, isnull(EATO.EmployeeName, '') AS AssignToEmployeeName, isnull(EAB.EmployeeName, '') AS AssignByEmployeeName,
                              isnull(tc.UserName, '') AS TaskCategory, isnull(tsc.UserName, '') AS TaskSubCategory, format(ATO.DueDate, 'dd-MMM-yyyy') AS DueDate

                         FROM TaskManagerMaster AS tm
                        LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId = tm.Id AND ab.AuthorizationType = 'CreatedBy'
                        LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId = tm.Id AND ATO.AuthorizationType = 'AssignTo'

                        LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId = ab.ResponsiblePersonId
                        LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId = ATO.ResponsiblePersonId
LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
                        LEFT OUTER JOIN org.Department AS DTO ON dto.Id = p.DepartmentId

                        LEFT OUTER JOIN hkp.TaskCategory AS tc ON tm.TaskCategoryId = tc.Id
                        LEFT OUTER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id = tm.TaskSubCategoryId
                        ) AS K where k.DepartmentId IN(tDept)

                        AND K.AssignToId IN(aToEName)
                        AND K.AssignById IN(aByEName)
                        AND K.TaskCategoryId IN(tCategory)
                        AND k.TaskSubCategoryId IN(tSubCategory)
                        AND K.DueDate IN(dueDate) ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public ActionResult GetTaskDetail(Dictionary<string, object> Row, string fromDate, string ToDate, string TaskTypeGroup, string typeflag)
        {
            string Today = System.DateTime.Now.ToString("dd-MMM-yyyy");
            string MainSql = @"
                       SELECT TM.Id, TM.TaskType, TM.TaskDescription, TM.CurrentStatus,FORMAT( ato.DueDate,'dd-MMM-yyyy') AS DueDate,
FORMAT(ISNULL(ATO.RevisedCommitmentDate,ATO.CommitmentDate),'dd-MMM-yyyy') AS  CommitmentDate FROM 
                                TaskManagerMaster AS TM
								LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
								LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'
								LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=p.DepartmentId";


            if (typeflag == "TotalCreated")
            {
                MainSql += @" WHERE convert(date,tm.AddedDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"'
										AND isnull(p.DepartmentId,'')='" + Row["DepartmentId"].ToString() + @"' AND isnull(ATO.ResponsiblePersonId,'')='" + Row["AssignToId"].ToString() + @"' and isnull(AB.ResponsiblePersonId,'')='" + Row["AssignById"].ToString() + @"'
										and isnull(tm.TaskCategoryId,'')='" + Row["TaskCategoryId"].ToString() + @"' and isnull(tm.TaskSubCategoryId,'')='" + Row["TaskSubCategoryId"].ToString() + @"'
										";
            }
            if (typeflag == "TaskToClose")
            {
                MainSql += @" WHERE convert(date,tm.AddedDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"' AND TM.CurrentStatus='ToClose'
										AND isnull(p.DepartmentId,'')='" + Row["DepartmentId"].ToString() + @"' AND isnull(ATO.ResponsiblePersonId,'')='" + Row["AssignToId"].ToString() + @"' and isnull(AB.ResponsiblePersonId,'')='" + Row["AssignById"].ToString() + @"'
										and isnull(tm.TaskCategoryId,'')='" + Row["TaskCategoryId"].ToString() + @"' and isnull(tm.TaskSubCategoryId,'')='" + Row["TaskSubCategoryId"].ToString() + @"'
										";
            }
            if (typeflag == "TotalOverDueUnread")
            {
                MainSql += @" WHERE convert(date,ATO.DueDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"' AND convert(date,ATO.DueDate)<convert(date,'" + Today + @"') AND isnull(tm.CurrentStatus,'')<>'Closed' AND isnull(ATO.isRead,0)=0
										AND isnull(p.DepartmentId,'')='" + Row["DepartmentId"].ToString() + @"' AND isnull(ATO.ResponsiblePersonId,'')='" + Row["AssignToId"].ToString() + @"' and isnull(AB.ResponsiblePersonId,'')='" + Row["AssignById"].ToString() + @"'
										and isnull(tm.TaskCategoryId,'')='" + Row["TaskCategoryId"].ToString() + @"' and isnull(tm.TaskSubCategoryId,'')='" + Row["TaskSubCategoryId"].ToString() + @"'
										";
            }
            if (typeflag == "TotalOverDueRead")
            {
                MainSql += @" WHERE convert(date,ATO.DueDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"' AND convert(date,ATO.DueDate)<convert(date,'" + Today + @"') AND tm.CurrentStatus<>'Closed' AND isnull(ATO.isRead,0)=1
										AND isnull(p.DepartmentId,'')='" + Row["DepartmentId"].ToString() + @"' AND isnull(ATO.ResponsiblePersonId,'')='" + Row["AssignToId"].ToString() + @"' and isnull(AB.ResponsiblePersonId,'')='" + Row["AssignById"].ToString() + @"'
										and isnull(tm.TaskCategoryId,'')='" + Row["TaskCategoryId"].ToString() + @"' and isnull(tm.TaskSubCategoryId,'')='" + Row["TaskSubCategoryId"].ToString() + @"'
										";
            }
            if (typeflag == "TodayTask")
            {
                MainSql += @" WHERE convert(date,ATO.DueDate) BETWEEN  '" + Today + @"' and '" + Today + @"' AND tm.CurrentStatus<>'Closed'
										AND isnull(p.DepartmentId,'')='" + Row["DepartmentId"].ToString() + @"' AND isnull(ATO.ResponsiblePersonId,'')='" + Row["AssignToId"].ToString() + @"' and isnull(AB.ResponsiblePersonId,'')='" + Row["AssignById"].ToString() + @"'
										and isnull(tm.TaskCategoryId,'')='" + Row["TaskCategoryId"].ToString() + @"' and isnull(tm.TaskSubCategoryId,'')='" + Row["TaskSubCategoryId"].ToString() + @"'
										";
            }
            MainSql += " ORDER BY ATO.DueDate ASC  ";

            return Json(_sqlRepository.GetDataCollection(MainSql), JsonRequestBehavior.AllowGet);

        }

     
        [HttpPost, Authorize]
        public ActionResult GetTaskDetailMain(string fromDate, string ToDate, string TaskTypeGroup, string TaskTypeGroupFilter, string typeflag)
        {
            string Today = DateTime.Now.ToString("dd-MMM-yyyy");
            string TaskTypeGroupSql = "";
            if (TaskTypeGroupFilter != "All")
                TaskTypeGroupSql = " AND TM.TaskTypeGroup='" + TaskTypeGroupFilter + @"' ";
            TaskTypeGroupSql += " AND isnull(TM.isOwnTask,0)=0 ";

            string MainSql = @"SELECT TM.Id, isnull(it.Id, TM.Id) AS IdCode,ig.Name AS IssueGroup,
                             format(IT.RequiredDate,'dd-MMM-yyyy') IssueRequiredDate
                            ,format(IT.ExpiryDate, 'dd-MMM-yyyy')IssueExpiryDate
                            ,format(IT.CloseDate,'dd-MMM-yyyy')IssueCloseDate, TM.TaskType, TM.TaskDescription, TM.CurrentStatus,FORMAT( ato.DueDate,'dd-MMM-yyyy') AS DueDate,
                            EATO.EmployeeName AS AssignTo,EABY.EmployeeName AS AssignBy,TC.UserName AS TaskCategory,TSC.UserName AS TaskSubCategory,
                            FORMAT(ISNULL(ATO.RevisedCommitmentDate,ATO.CommitmentDate),'dd-MMM-yyyy') AS  CommitmentDate FROM 
                                TaskManagerMaster AS TM
								LEFT OUTER JOIN (Select distinct TaskManagerMasterId,ResponsiblePersonId from TaskAudit Where AuthorizationType='CreatedBy') AS AB ON ab.TaskManagerMasterId=tm.Id
								LEFT OUTER JOIN (Select distinct TaskManagerMasterId,ResponsiblePersonId,RevisedCommitmentDate,CommitmentDate,DueDate,isRead from TaskAudit Where AuthorizationType='AssignTo') AS ATO ON ATO.TaskManagerMasterId=tm.Id 
								LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
								LEFT OUTER JOIN EmployeeInformation AS EABY ON EABY.SystemId=AB.ResponsiblePersonId
LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=p.DepartmentId
                                LEFT OUTER JOIN hkp.TaskCategory AS tc ON tm.TaskCategoryId = tc.Id
                                LEFT OUTER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id = tm.TaskSubCategoryId
                                LEFT OUTER JOIN IssueTransaction IT ON it.Id=tm.IssueTransactionId
                                left join IssueGroup IG on IG.Id = IT.IssueGroupId";

            if (typeflag == "totalcreatedspline")
            {
                MainSql += @" WHERE convert(date,tm.AddedDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"'
										AND tm.TaskTypeGroup='" + TaskTypeGroup + @"'";
            }
            if (typeflag == "totalclosedspline")
            {
                MainSql += @" WHERE convert(date,tm.ClosingDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"' AND TM.CurrentStatus='Closed'
										AND tm.TaskTypeGroup='" + TaskTypeGroup + @"'";
            }
            if (typeflag == "totalclosedontimespline")
            {
                MainSql += @" WHERE convert(date,tm.ClosingDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"' AND TM.CurrentStatus='Closed'
									AND CONVERT(DATE, ATO.DueDate)<=convert(date,tm.ClosingDate) 	AND tm.TaskTypeGroup='" + TaskTypeGroup + @"'";
            }
            if (typeflag == "totalcloseddelayedspline")
            {
                MainSql += @" WHERE convert(date,tm.ClosingDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"' AND TM.CurrentStatus='Closed'
									AND CONVERT(DATE, ATO.DueDate)>convert(date,tm.ClosingDate) 	AND tm.TaskTypeGroup='" + TaskTypeGroup + @"'";
            }
            if (typeflag == "taskoverdueunreadspline")
            {
                MainSql += @" WHERE convert(date,ATO.DueDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"' AND convert(date,ATO.DueDate)<convert(date,'" + Convert.ToDateTime(Today).ToString("dd-MMM-yyyy") + @"') AND isnull(tm.CurrentStatus,'')<>'Closed' AND isnull(ATO.isRead,0)=0
										AND tm.TaskTypeGroup='" + TaskTypeGroup + @"'";
            }
            if (typeflag == "taskoverduereadspline")
            {
                MainSql += @" WHERE convert(date,ATO.DueDate) BETWEEN  '" + fromDate + @"' and '" + ToDate + @"' AND convert(date,ATO.DueDate)<convert(date,'" + Convert.ToDateTime(Today).ToString("dd-MMM-yyyy") + @"') AND isnull(tm.CurrentStatus,'')<>'Closed' AND isnull(ATO.isRead,0)=1
										AND tm.TaskTypeGroup='" + TaskTypeGroup + @"'";
            }
            if (typeflag == "todaytaskspline")
            {
                MainSql += @" WHERE convert(date,ATO.DueDate) BETWEEN  '" + Today + @"' and '" + Today + @"' AND isnull(tm.CurrentStatus,'')<>'Closed'
										AND tm.TaskTypeGroup='" + TaskTypeGroup + @"'";
            }
            if (typeflag == "futuretaskspline")
            {
                MainSql += @" WHERE convert(date,ATO.DueDate) BETWEEN '" + fromDate + @"' AND '" + ToDate +
                                @"' AND convert(date,ATO.DueDate)>'" + Convert.ToDateTime(Today).ToString("dd-MMM-yyyy") + @"' AND tm.CurrentStatus<>'Closed' 
                                   	AND tm.TaskTypeGroup='" + TaskTypeGroup + @"'";
            }
            MainSql += TaskTypeGroupSql + "  ORDER BY ATO.DueDate ASC  ";


            var jsondata = Json(_sqlRepository.GetDataCollection(MainSql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }


        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName1 + " WHERE FLAG='" + Flag + "') AS TEMP WHERE " + strkey + " order by sequence";



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

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where FLAG='" + Flag + "' AND Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where FLAG='" + Flag + "' AND UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same user name already exists!!!");



                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName1, out _Id);

                    data["Id"] = "TC" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
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

        [Authorize]
        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName1 + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

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



            dr["FLAG"] = Flag;
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

            dr["FLAG"] = Flag;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName1 + " where FLAG='" + Flag + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
    }
}