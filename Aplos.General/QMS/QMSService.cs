using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using Library.Service.Enums;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using OTSBD;
using Syncfusion.XlsIO;
using System.IO;
using Library.Service.EmployeeServices;

namespace Library.General.QMS
{
    public class QMSService
    {

        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public QMSService()
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

                                UNION ALL
                                SELECT 'Assigned To Me' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  ta.AuthorizationType<>'CreatedBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION ALL
                                SELECT 'MyTasks' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ATT.isRead,0)=0 THEN CASE WHEN ((CRB.ResponsiblePersonId=ATT.ResponsiblePersonId) OR CRB.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  
                                FROM TaskManagerMaster AS tmm
                                LEFT JOIN TaskAudit AS CRB ON CRB.TaskManagerMasterId=tmm.Id AND CRB.AuthorizationType='CreatedBy'
                                INNER JOIN TaskAudit AS ATT ON ATT.TaskManagerMasterId=tmm.Id AND ATT.AuthorizationType='AssignTo'
                                WHERE (tmm.CurrentStatus='Closed' OR isnull(ATT.isDone,0)=1)  AND CONVERT(DATE, CRB.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND tmm.TaskType IN ('ToDo','TNA','Issue') 
                                AND ATT.ResponsiblePersonId='" + logedInUser + @"' 
                             
                                UNION ALL
                                SELECT 'UpdateAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  tmm.TaskType='UpdateAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION ALL
                                SELECT 'FollowUpAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  tmm.TaskType='FollowUpAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION ALL
                                SELECT 'InternalAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  tmm.TaskType='InternalAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'
                                UNION ALL
                                SELECT 'ExternalAudit' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                 WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  tmm.TaskType='ExternalAudit' AND ta.AuthorizationType='AssignTo' AND ta.ResponsiblePersonId='" + logedInUser + @"'

                                UNION ALL
                                SELECT 'CheckBy' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND ta.AuthorizationType='CheckBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'


                                UNION ALL
                                SELECT 'CrossCheckBy' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  ta.AuthorizationType='CrossCheckBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'


                                UNION ALL
                                SELECT 'ApproveBy' AS TaskType,COUNT(*) AS NoOfTasks,SUM(CASE WHEN ISNULL(ta.isRead,0)=0 THEN CASE WHEN ((TA.ResponsiblePersonId=TTO.ResponsiblePersonId) OR TTO.ResponsiblePersonId='" + logedInUser + @"') THEN 0 ELSE 1 END ELSE 0 END) AS Unread  FROM TaskManagerMaster AS tmm
                                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                                LEFT JOIN TaskAudit AS TTO ON TTO.TaskManagerMasterId=tmm.Id AND TTO.AuthorizationType='CreatedBy'
                                WHERE (tmm.CurrentStatus='Closed'  OR isnull(ta.isDone,0)=1) AND CONVERT(DATE, ta.DueDate) BETWEEN CONVERT(DATE, DATEADD(MONTH,-3,getdate())) AND CONVERT(DATE, DATEADD(MONTH,3,getdate()))  AND  ta.AuthorizationType='ApproveBy' AND ta.ResponsiblePersonId='" + logedInUser + @"'";




            return _sqlRepository.GetDataCollection(sql);


        }      

       
        
    }
}
