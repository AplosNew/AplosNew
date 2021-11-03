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
using Library.Model.Enums;

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TNAStatusReportsController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;


        public TNAStatusReportsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        //private string GetSql(Dictionary<string, string> filterString)
        //{
        //    string FilterString = "";

        //    string FromDate = filterString["FromDate"];
        //    string ToDate = filterString["ToDate"];
        //    string ActiveStatus = filterString["ActiveStatus"];
        //    string Durration = filterString["Durration"];
        //    string status = filterString["Status"];
        //    string Today = DateTime.Now.ToString("dd-MMM-yyyy");
        //    string FirstDayOfEndNextWeek = DateTime.Now.AddDays(8).ToString();

        //    if (ActiveStatus == "Active")
        //        FilterString = " WHERE CurrentStatus<>'Closed'";
        //    else
        //        FilterString = " WHERE CurrentStatus='Closed'";

        //    FilterString += " AND Convert(date,DueDate) Between Convert(date,'"+FromDate+"') AND Convert(date, '"+ToDate+"')";

        //    if(Durration == "OverDue")
        //    {
        //        FilterString += " AND DueDate < '" + Today + "'";
        //    }
        //    else if (Durration == "ToDay")
        //    {
        //        FilterString += " AND DueDate = '" + Today + "'";
        //    }
        //    else if (Durration == "NextWeek")
        //    {
        //        FilterString += " AND DueDate > '" + Today + "'" + " AND  DueDate <= '" + FirstDayOfEndNextWeek + "'";
        //    }
        //    else if (Durration == "Future")
        //    {
        //        FilterString += " AND DueDate >= '" + FirstDayOfEndNextWeek  + "'";
        //    }

        //    if (status == "ToDo")
        //    {
        //        return @"select * from (SELECT TMM.CurrentStatus,  TSC.UserName AS TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate,ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  NULL MasterOrderNo,
        //                    Buyer=null
        //                    ,StyleNo= NULL
        //                    ,SONo=NULL
        //                    ,PRNo=NULL
        //                    FROM TaskManagerMaster AS TMM
        //                    LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo'
        //                    LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
        //                    where  TMM.TaskType = 'ToDo'  and isnull(TMM.isOwnTask,0)=0) AS K " + FilterString;
        //    }
        //    else if (status == "Issue")
        //    {
        //        return @"SELECT * FROM (SELECT  TMM.CurrentStatus,TSC.UserName AS TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate
        //                    ,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  NULL MasterOrderNo,
        //                    Buyer=STUFF((select distinct ',' + XB.UserName AS Buyer from IssueTransaction AS XIT 
        //                    INNER JOIN IssueBuyer AS XIB ON XIB.IssueTransactionId = XIT.Id
        //                    LEFT OUTER JOIN [HKP].[Buyer] AS XB ON XB.Id = XIB.BuyerId
        //                    where XIT.Id=IT.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        //                    ,StyleNo= NULL
        //                    ,SONo=NULL
        //                    ,PRNo=NULL
        //                    FROM TaskManagerMaster AS TMM
        //                    LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                    LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                    INNER JOIN IssueTransaction AS IT ON IT.Id = TMM.IssueTransactionId
        //                    LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId) AS K " + FilterString;
        //    }
        //    else if (status == "TNA")
        //    {
        //        return @" SELECT * FROM(
        //                    (SELECT  TMMM.CurrentStatus,TSC.UserName AS TaskType, TMMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
        //                    ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
        //                    trn.MasterOrderItem XMOI 
        //                    where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

        //                    SONo=STUFF((select distinct ','+so.Id from 
        //                    trn.MasterOrderItem XMOI 
        //                    INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
        //                    where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

        //                    PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
        //                    trn.MasterOrderItem XMOI 
        //                    INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
        //                    INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
        //                    where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        //                    FROM TaskManagerMaster AS TMMM
        //                    LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                    LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMMM.TaskSubCategoryId
        //                    INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMMM.TNATasksId 
        //                    LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
        //                    inner JOIN [TRN].[MasterOrder] AS MO ON MO.Id = TM.MasterOrderId
        //                    LEFT OUTER JOIN IssueTransaction AS IT ON IT.Id = TMMM.IssueTransactionId
        //                    LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId) 

        //                    UNION

        //                    (SELECT  TMM.CurrentStatus,TSC.UserName AS TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
        //                    ,StyleNo= MOI.BuyerReferenceNo,
        //                    SONo=STUFF((select distinct ','+so.Id from 
        //                    trn.MasterOrderItem XMOI 
        //                    INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
        //                    where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

        //                    PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
        //                    trn.MasterOrderItem XMOI 
        //                    INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
        //                    INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
        //                    where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        //                    FROM TaskManagerMaster AS TMM
        //                    LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                    LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
        //                    LEFT OUTER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
        //                    inner JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
        //                    inner JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = TM.MasterOrderItemId
        //                    LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
        //                    LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId ) 

        //                    UNION 

        //                    (SELECT  TMM.CurrentStatus, TSC.UserName AS TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate
        //                    , ISNULL(TATo.RevisedCommitmentDate,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
        //                    ,StyleNo= MOI.BuyerReferenceNo
        //                    ,SONo=so.Id

        //                    ,PRNo=STUFF((select distinct ','+xpod.ProductionOrderId from  trn.ProductionOrderDetail AS xpod
        //                    where xpod.SalesOrderId = so.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

        //                    FROM TaskManagerMaster AS TMM
        //                    LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                    LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
        //                    INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
        //                    LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId

        //                    inner JOIN [TRN].[SalesOrder] AS SO ON SO.Id =  TM.SalesOrderId
        //                    LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
        //                    LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
        //                    LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId)
        //                    UNION 
        //                    (SELECT TMM.CurrentStatus, TSC.UserName AS TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate
        //                    ,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
        //                    ,StyleNo= MOI.BuyerReferenceNo
        //                    ,SONo=so.Id
        //                    ,PRNo=PO.Id
        //                    FROM TaskManagerMaster AS TMM
        //                    LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                    LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                    LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
        //                    inner JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
        //                    LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
        //                    inner join [TRN].[ProductionOrder] AS PO ON PO.Id = TM.ProductionOrderId
        //                    LEFT OUTER JOIN [TRN].[SalesOrder] AS SO ON SO.Id =  TM.SalesOrderId
        //                    LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
        //                    LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
        //                    LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId)
        //                    ) AS K " + FilterString;
        //    }
        //    else
        //    {
        //        return @"SELECT * FROM (
        //                (SELECT  TMMM.CurrentStatus, TMMM.TaskType, TMMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
        //                ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
        //                trn.MasterOrderItem XMOI 
        //                where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

        //                SONo=STUFF((select distinct ','+so.Id from 
        //                trn.MasterOrderItem XMOI 
        //                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
        //                where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

        //                PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
        //                trn.MasterOrderItem XMOI 
        //                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
        //                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
        //                where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        //                FROM TaskManagerMaster AS TMMM
        //                LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMMM.TaskSubCategoryId
        //                INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMMM.TNATasksId 
        //                LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
        //                inner JOIN [TRN].[MasterOrder] AS MO ON MO.Id = TM.MasterOrderId
        //                LEFT OUTER JOIN IssueTransaction AS IT ON IT.Id = TMMM.IssueTransactionId
        //                LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId) 

        //                UNION 

        //                (SELECT TMM.CurrentStatus, TMM.TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
        //                ,StyleNo= MOI.BuyerReferenceNo,

        //                SONo=STUFF((select distinct ','+so.Id from 
        //                trn.MasterOrderItem XMOI 
        //                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
        //                where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

        //                PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
        //                trn.MasterOrderItem XMOI 
        //                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
        //                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
        //                where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        //                FROM TaskManagerMaster AS TMM
        //                LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                LEFT OUTER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
        //                inner JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
        //                inner JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = TM.MasterOrderItemId
        //                LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
        //                LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId)
        //                UNION 
        //                (SELECT TMM.CurrentStatus, TMM.TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate
        //                , ISNULL(TATo.RevisedCommitmentDate,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
        //                ,StyleNo= MOI.BuyerReferenceNo
        //                ,SONo=so.Id

        //                ,PRNo=STUFF((select distinct ','+xpod.ProductionOrderId from  trn.ProductionOrderDetail AS xpod
        //                where xpod.SalesOrderId = so.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

        //                FROM TaskManagerMaster AS TMM
        //                LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
        //                LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId

        //                inner JOIN [TRN].[SalesOrder] AS SO ON SO.Id =  TM.SalesOrderId
        //                LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
        //                LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
        //                LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId)

        //                UNION 

        //                (SELECT TMM.CurrentStatus, TMM.TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate
        //                ,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
        //                ,StyleNo= MOI.BuyerReferenceNo
        //                ,SONo=so.Id
        //                ,PRNo=PO.Id
        //                FROM TaskManagerMaster AS TMM
        //                LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                inner JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
        //                LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
        //                inner join [TRN].[ProductionOrder] AS PO ON PO.Id = TM.ProductionOrderId
        //                LEFT OUTER JOIN [TRN].[SalesOrder] AS SO ON SO.Id =  TM.SalesOrderId
        //                LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
        //                LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
        //                LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId)

        //                union

        //                (SELECT TMM.CurrentStatus, TMM.TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate
        //                ,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  NULL MasterOrderNo,
        //                Buyer=null
        //                ,StyleNo= NULL
        //                ,SONo=NULL
        //                ,PRNo=NULL
        //                FROM TaskManagerMaster AS TMM
        //                LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo'
        //                LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                where  TMM.TaskType = 'ToDo'  and isnull(TMM.isOwnTask,0)=0)

        //                union 

        //                (SELECT TMM.CurrentStatus, TSC.UserName AS TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, TATo.DueDate as DueDate, ISNULL(TATo.RevisedCommitmentDate
        //                ,ISNULL(TATo.CommitmentDate,TATo.DueDate)) as CommitmentDate  ,  NULL MasterOrderNo,
        //                Buyer=STUFF((select distinct ',' + XB.UserName AS Buyer from IssueTransaction AS XIT 
        //                INNER JOIN IssueBuyer AS XIB ON XIB.IssueTransactionId = XIT.Id
        //                LEFT OUTER JOIN [HKP].[Buyer] AS XB ON XB.Id = XIB.BuyerId
        //                where XIT.Id=IT.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
        //                ,StyleNo= NULL
        //                ,SONo=NULL
        //                ,PRNo=NULL
        //                FROM TaskManagerMaster AS TMM
        //                LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
        //                LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'AssignBy' 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
        //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
        //                LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
        //                INNER JOIN IssueTransaction AS IT ON IT.Id = TMM.IssueTransactionId) 
        //                ) AS K " + FilterString; 
        //    }

        //}

        private string GetSql(Dictionary<string, string> filterString)
        {
            string FilterString = "";

            string FromDate = filterString["FromDate"];
            string ToDate = filterString["ToDate"];
            string ActiveStatus = filterString["ActiveStatus"];
            string status = filterString["Status"];
            string Today = DateTime.Now.ToString("dd-MMM-yyyy");
            string FirstDayOfEndNextWeek = DateTime.Now.AddDays(8).ToString();

            FilterString = "WHERE 1=1 ";
            if (ActiveStatus == "Active")
            {
                FilterString += " AND isnull(CurrentStatus,'')<>'Closed'";
                FilterString += " AND Convert(date,DueDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "')";

            }
            else if (ActiveStatus == "Closed")
            {
                FilterString += " AND isnull(CurrentStatus,'')='Closed'";
                FilterString += " AND Convert(date,ClosingDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "')";

            }
            else
            {
                FilterString += " AND ( (Convert(date,DueDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "') AND isnull(CurrentStatus,'')<>'Closed')";
                FilterString += " OR (Convert(date,ClosingDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "') AND isnull(CurrentStatus,'')='Closed'))";

            }




            if (status == "ToDo")
            {
                return @"select * from (SELECT TMM.Id AS TaskMasterId, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, TMM.TaskType,
TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat,
format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),
ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  NULL MasterOrderNo,
                            
                            Buyer=null
                            ,StyleNo= NULL
                            ,SONo=NULL
                            ,PRNo=NULL
                            ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=NULL,Division=NULL
                            FROM TaskManagerMaster AS TMM
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)
                            
                            LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo'
                            LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                            LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                            LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
                            where  TMM.TaskTypeGroup = 'ToDo' and isnull(TMM.isOwnTask,0)=0) AS K " + FilterString + " order by DueDate";
            }
            else if (status == "Issue")
            {
                return @"SELECT * FROM (SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus,TSC.UserName as SubCategory,TC.UserName as Category,TMM.TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  NULL MasterOrderNo,
                            
                            Buyer=STUFF((select distinct ',' + XB.UserName from IssueTransaction AS XIT 
                            INNER JOIN IssueBuyer AS XIB ON XIB.IssueTransactionId = XIT.Id
                            LEFT OUTER JOIN [HKP].[Buyer] AS XB ON XB.Id = XIB.BuyerId
                            where XIT.Id=IT.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,StyleNo= NULL
                            ,SONo=NULL
                            ,PRNo=NULL
                            ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=NULL,Division=NULL
                            FROM TaskManagerMaster AS TMM
                         left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)
                           
                            LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                            LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                            INNER JOIN IssueTransaction AS IT ON IT.Id = TMM.IssueTransactionId
                            LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                            LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId) AS K " + FilterString + " order by DueDate";
            }
            else if (status == "TNA")
            {
                return @" SELECT * FROM(SELECT TMMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMMM.CurrentStatus,TSC.UserName as SubCategory,TC.UserName as Category, concat(TMMM.TaskTypeGroup,'/',TM.TNAAppliedOn) as TaskType, TMMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  MO.MasterOrderNo,
                             B.UserName AS Buyer
                            
                            ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                            trn.MasterOrderItem XMOI 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SONo=STUFF((select distinct ','+so.Id from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,datediff(day,tato.duedate,TMMM.closingDate) AS EarlyOrLateBy,FORMAT(TMMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMMM
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMMM.ID ORDER BY T.CreatedTime DESC)

                            LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                            LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                            LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMMM.TaskSubCategoryId
                            LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMMM.TaskCategoryId
                            INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMMM.TNATasksId 
                            LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[MasterOrder] AS MO ON MO.Id = TM.MasterOrderId
                            LEFT OUTER JOIN IssueTransaction AS IT ON IT.Id = TMMM.IssueTransactionId
                            LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    						
                            UNION

                            SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, concat(TMM.TaskTypeGroup,'/',TM.TNAAppliedOn) as TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
                            ,StyleNo= MOI.BuyerReferenceNo,
                            SONo=STUFF((select distinct ','+so.Id from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMM
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)

                            LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                            LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                            LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                            LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
                            LEFT OUTER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                            inner JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = TM.MasterOrderItemId
                            LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                            LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId 
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    

						
                            UNION 

                            SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, concat(TMM.TaskTypeGroup,'/',TM.TNAAppliedOn) as TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate
                            , ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
                            ,StyleNo= MOI.BuyerReferenceNo
                            ,SONo=so.Id

                            ,PRNo=STUFF((select distinct ','+xpod.ProductionOrderId from  trn.ProductionOrderDetail AS xpod
                            where xpod.SalesOrderId = so.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMM
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)

                            LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                            LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId
                            INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                            LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                            LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
                            inner JOIN [TRN].[SalesOrder] AS SO ON SO.Id =  TM.SalesOrderId
                            LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
                            LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                            LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    
                            UNION 

                            --SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category,  concat(TMM.TaskTypeGroup,'/',TM.TNAAppliedOn) as TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo,format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
                            --,StyleNo= MOI.BuyerReferenceNo
                            --,SONo=so.Id
                            --,PRNo=POD.Id
                            --,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            --,Department=bd.UserName,Division=bd2.UserName
                            --FROM TaskManagerMaster AS TMM
                            --LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                            --LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                            --LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                            --LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                            --inner JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                            --LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            --inner join [TRN].[ProductionOrderDetail] AS POD ON POD.ProductionOrderId = TM.ProductionOrderId 
							--and pod.Id=(select top 1 Id from  [TRN].[ProductionOrderDetail] XD where  XD.ProductionOrderId=TM.ProductionOrderId)
                            --LEFT OUTER JOIN [TRN].[SalesOrder] AS SO ON so.Id=pod.SalesOrderId
                            --LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
                            --LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                            --LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            --LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                            --LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
                            --LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    
                            


 SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category,
   concat(TMM.TaskTypeGroup,'/',T.TNAAppliedOn) as TaskType,
    TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo,
    format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, 
    ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  , 
    pr.MasterOrderId,pr.Buyer,pr.StyleNo, pr.SONo, pr.ProductionOrderId,
    datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
    ,Department=bd.UserName,Division=bd2.UserName
				
                                 FROM TaskManagerMaster AS tmm
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)

                                INNER JOIN TNATasks AS TT ON TT.Id=tmm.TNATasksId
                                INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId  AND isnull(t.ProductionOrderId,'')<>''
                                 LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
								LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
								LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
								LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId
                                 INNER JOIN trn.ProductionOrder AS po ON PO.Id=t.ProductionOrderId
                                INNER JOIN
                                (
                                			SELECT distinct po.Id AS ProductionOrderId,mo.BuyerDepartmentId,mo.BuyerDivisionId,
                                			b.Id AS BuyerId,b.UserName AS Buyer,
                                			
                                			 MasterOrderId=STUFF((select distinct ', '+XMOI.MasterOrderId from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
											 
											 ,StyleNo=STUFF((select distinct ', '+XMOI.BuyerReferenceNo from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                	
                                			  ,SONo=STUFF((select distinct ', '+sox.Id from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				                                
														 FROM trn.ProductionOrder PO
										INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=po.Id AND pod.Id=(SELECT TOP 1 Id FROM trn.ProductionOrderDetail AS px WHERE px.ProductionOrderId=po.Id)
                                		INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                		inner join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
										INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
										LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                ) AS PR ON pr.ProductionOrderId=po.Id
                                
                                LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
								LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
								LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=PR.BuyerDepartmentId   
								LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=PR.BuyerDivisionId  

                        ) AS K " + FilterString + " order by DueDate";
            }
            else
            {
                return @"SELECT * FROM (
                        (SELECT TMMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, concat(TMMM.TaskTypeGroup,'/',TM.TNAAppliedOn) as TaskType, TMMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  MO.MasterOrderNo, 
                         B.UserName AS Buyer
                        ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                        trn.MasterOrderItem XMOI 
                        where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                        SONo=STUFF((select distinct ','+so.Id from 
                        trn.MasterOrderItem XMOI 
                        INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                        where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                        PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                        trn.MasterOrderItem XMOI 
                        INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                        where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        ,datediff(day,tato.duedate,TMMM.closingDate) AS EarlyOrLateBy,FORMAT(TMMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=bd.UserName,Division=bd2.UserName
                        FROM TaskManagerMaster AS TMMM
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMMM.ID ORDER BY T.CreatedTime DESC)

                        LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                        LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                        LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMMM.TaskSubCategoryId
                        LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMMM.TaskCategoryId
                        INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMMM.TNATasksId 
                        LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                        inner JOIN [TRN].[MasterOrder] AS MO ON MO.Id = TM.MasterOrderId
                        LEFT OUTER JOIN IssueTransaction AS IT ON IT.Id = TMMM.IssueTransactionId
                        LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                        LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId     

)
                                              

                        UNION 

                        (SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, concat(TMM.TaskTypeGroup,'/',TM.TNAAppliedOn) as TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  MO.MasterOrderNo, B.UserName AS Buyer
                        ,StyleNo= MOI.BuyerReferenceNo,
                        SONo=STUFF((select distinct ','+so.Id from 
                        trn.MasterOrderItem XMOI 
                        INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                        where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                        PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                        trn.MasterOrderItem XMOI 
                        INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                        where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=bd.UserName,Division=bd2.UserName
                        FROM TaskManagerMaster AS TMM
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)

                        LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                        LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                        LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                        LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
                        LEFT OUTER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                        inner JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                        inner JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = TM.MasterOrderItemId
                        LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                        LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    
                       )
UNION 

                        (SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, concat(TMM.TaskTypeGroup,'/',TM.TNAAppliedOn) as TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate
                        , ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  MO.MasterOrderNo,
                        B.UserName AS Buyer
                        ,StyleNo= MOI.BuyerReferenceNo
                        ,SONo=so.Id

                        ,PRNo=STUFF((select distinct ','+xpod.ProductionOrderId from  trn.ProductionOrderDetail AS xpod
                        where xpod.SalesOrderId = so.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=bd.UserName,Division=bd2.UserName
                        FROM TaskManagerMaster AS TMM
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)

                        LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                        LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                        INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                        LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                        LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                        LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
                        inner JOIN [TRN].[SalesOrder] AS SO ON SO.Id =  TM.SalesOrderId
                        LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
                        LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                        LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    
                       
                        )
                        UNION 

                        
             SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category,
               concat(TMM.TaskTypeGroup,'/',T.TNAAppliedOn) as TaskType,
                TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo,
                format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, 
                ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  , 
                pr.MasterOrderId,pr.Buyer,pr.StyleNo, pr.SONo, pr.ProductionOrderId,
                datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                ,Department=bd.UserName,Division=bd2.UserName
				
                                 FROM TaskManagerMaster AS tmm
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)

                                INNER JOIN TNATasks AS TT ON TT.Id=tmm.TNATasksId
                                INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId  AND isnull(t.ProductionOrderId,'')<>''
                                 LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
								LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
								LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
								LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId
                                 INNER JOIN trn.ProductionOrder AS po ON PO.Id=t.ProductionOrderId
                                INNER JOIN
                                (
                                			SELECT distinct po.Id AS ProductionOrderId,mo.BuyerDepartmentId,mo.BuyerDivisionId,
                                			b.Id AS BuyerId,b.UserName AS Buyer,
                                			
                                			 MasterOrderId=STUFF((select distinct ', '+XMOI.MasterOrderId from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
											 
											 ,StyleNo=STUFF((select distinct ', '+XMOI.BuyerReferenceNo from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                	
                                			  ,SONo=STUFF((select distinct ', '+sox.Id from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				                                
														 FROM trn.ProductionOrder PO
										INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=po.Id AND pod.Id=(SELECT TOP 1 Id FROM trn.ProductionOrderDetail AS px WHERE px.ProductionOrderId=po.Id)
                                		INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                		inner join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
										INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
										LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                ) AS PR ON pr.ProductionOrderId=po.Id
                                
                                LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
								LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
								LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=PR.BuyerDepartmentId   
								LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=PR.BuyerDivisionId  
                        union

                        (SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, TMM.TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  NULL MasterOrderNo,
                        Buyer=null
                        ,StyleNo= NULL
                        ,SONo=NULL
                        ,PRNo=NULL
                        ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=NULL,Division=NULL
                        FROM TaskManagerMaster AS TMM
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)

                        LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo'
                        LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                        LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                        LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
                        where  TMM.TaskTypeGroup = 'ToDo'  and isnull(TMM.isOwnTask,0)=0)

                        union 

                        (SELECT TMM.Id AS TaskMasterId,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, TMM.TaskType, TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo, format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate  ,  NULL MasterOrderNo,
                        Buyer=STUFF((select distinct ',' + XB.UserName from IssueTransaction AS XIT 
                        INNER JOIN IssueBuyer AS XIB ON XIB.IssueTransactionId = XIT.Id
                        LEFT OUTER JOIN [HKP].[Buyer] AS XB ON XB.Id = XIB.BuyerId
                        where XIT.Id=IT.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        ,StyleNo= NULL
                        ,SONo=NULL
                        ,PRNo=NULL
                        ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=NULL,Division=NULL
                        FROM TaskManagerMaster AS TMM
                        left outer join TaskComments TSK on TSK.TaskManagerMasterId=TMM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TMM.ID ORDER BY T.CreatedTime DESC)

                        LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo' 
                        LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                        LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                        LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                        LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId

                        INNER JOIN IssueTransaction AS IT ON IT.Id = TMM.IssueTransactionId) 
                        ) AS K " + FilterString + " order by DueDate";
            }

        }
        private Dictionary<string, List<DataRow>> GetSqlTaskComments(Dictionary<string, string> filterString)
        {
            string FilterString = "";

            string FromDate = filterString["FromDate"];
            string ToDate = filterString["ToDate"];
            string ActiveStatus = filterString["ActiveStatus"];
            string status = filterString["Status"];
            string Today = DateTime.Now.ToString("dd-MMM-yyyy");
            string FirstDayOfEndNextWeek = DateTime.Now.AddDays(8).ToString();

            FilterString = "WHERE 1=1 ";
            if (ActiveStatus == "Active")
            {
                FilterString += " AND isnull(CurrentStatus,'')<>'Closed'";
                FilterString += " AND Convert(date,DueDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "')";

            }
            else if (ActiveStatus == "Closed")
            {
                FilterString += " AND isnull(CurrentStatus,'')='Closed'";
                FilterString += " AND Convert(date,ClosingDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "')";

            }
            else
            {
                FilterString += " AND ( (Convert(date,DueDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "') AND isnull(CurrentStatus,'')<>'Closed')";
                FilterString += " OR (Convert(date,ClosingDate) Between Convert(date,'" + FromDate + "') AND Convert(date, '" + ToDate + "') AND isnull(CurrentStatus,'')='Closed'))";

            }



            string sql = @"select * from (SELECT tcom.TaskManagerMasterId, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, TMM.TaskType,
                                TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo,
                                format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),
                                ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate,  NULL MasterOrderNo,
                            FORMAT(tcom.CreatedTime,'dd-MMM-yyyy HH:mm:ss tt') AS CreatedTime,ei.EmployeeName AS CommentedBy,
                                    tcom.CommentText,
                            Buyer=null
                            ,StyleNo= NULL
                            ,SONo=NULL
                            ,PRNo=NULL
                            ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
                            ,Department=NULL,Division=NULL
                            FROM TaskManagerMaster AS TMM
                             INNER JOIN TaskComments AS tcom ON tcom.TaskManagerMasterId=tmm.Id
                            INNER JOIN EmployeeInformation AS ei ON ei.SystemId=tcom.CreatedById

                            LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo'
                            LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
                            LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
                            LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
                            where isnull(TMM.isOwnTask,0)=0) AS K " + FilterString + " order by TaskManagerMasterId,convert(datetime,CreatedTime)";
            DataTable dt = _sqlRepository.GetDataTable(sql);

            Dictionary<string, List<DataRow>> dicComments = new Dictionary<string, List<DataRow>>();
            List<DataRow> Data = new List<DataRow>();
            string id = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (id != dt.Rows[i]["TaskManagerMasterId"].ToString())
                {
                    Data = new List<DataRow>();
                    dicComments.Add(dt.Rows[i]["TaskManagerMasterId"].ToString(), Data);
                }
                Data.Add(dt.Rows[i]);

                id = dt.Rows[i]["TaskManagerMasterId"].ToString();
            }

            return dicComments;
        }

        private void GetTNAStatusReportsData(out DataTable dtTna, Dictionary<string, string> filterString)
        {
            string sql = GetSql(filterString);
            dtTna = _sqlRepository.GetDataTable(sql);
            dtTna.Columns.Add("EarlyBy", typeof(int));
            dtTna.Columns.Add("LateBy", typeof(int));
            for (int i = 0; i < dtTna.Rows.Count; i++)
            {
                if (dtTna.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED" && dtTna.Rows[i]["ClosingDate"].ToString() != "")
                {

                    DateTime dtDueDate = Convert.ToDateTime(dtTna.Rows[i]["DueDate"].ToString());
                    DateTime dtClosingDate = Convert.ToDateTime(dtTna.Rows[i]["ClosingDate"].ToString());
                    if (dtClosingDate < dtDueDate)
                        dtTna.Rows[i]["EarlyBy"] = Math.Abs(clsStaticInfo.dateDiff(dtClosingDate.ToString("dd-MMM-yyyy"), dtDueDate.ToString("dd-MMM-yyyy")));
                    if (dtClosingDate > dtDueDate)
                        dtTna.Rows[i]["LateBy"] = Math.Abs(clsStaticInfo.dateDiff(dtDueDate.ToString("dd-MMM-yyyy"), dtClosingDate.ToString("dd-MMM-yyyy")));
                }
            }
        }
        public IWorkbook GetTNAStatusReport(string CompanyGroupId, string CompanyId, string PlantId, string PlantName, string EmployeeId, string UserName, Dictionary<string, string> filterString)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();

            DataTable dtTNA = null;

            DataSet dsCmp = null;

            DataSet dsFactory = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string FactoryAddress = string.Empty;
            string OTConsiderOn = string.Empty;
            #endregion

            try
            {
                objRpt = new clsReport();


                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);

                #region Get Data Query
                GetTNAStatusReportsData(out dtTNA, filterString);
                if (dtTNA.Rows.Count == 0)
                    throw new Exception("No data found");

                Dictionary<string, List<DataRow>> dicComments = GetSqlTaskComments(filterString);
                #endregion

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                var isl = 0;
                var SLNo = 1;

                int colTaskType = 0;
                int colTask = 0;
                int colAssignBy = 0;
                int colAssignTo = 0;
                int colDueDate = 0;
                int colCommitmentDate = 0;
                int colMasterOrderNo = 0;
                int colStyleNo = 0;
                int colSONo = 0;
                int colPRNo = 0;
                int colSubCategory = 0;
                int colCategory = 0;
                int colEarlyBy = 0;
                int colLateBy = 0;
                int colClosingDate = 0;

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);

                objRpt.SelectedPlant(PlantId, out dsFactory);

                workbook = application.Workbooks.Create(1);

                #region Task List

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;
                xlsCol += 1;
                colDueDate = xlsCol;
                sheet1.Range[xlsRow, colDueDate].Text = "Due Date";
                sheet1.Range[xlsRow, colDueDate].ColumnWidth = 12;
                xlsCol += 1;
                colCommitmentDate = xlsCol;
                sheet1.Range[xlsRow, colCommitmentDate].Text = "Commitment Date";
                sheet1.Range[xlsRow, colCommitmentDate].ColumnWidth = 12;


                xlsCol += 1;
                colClosingDate = xlsCol;
                sheet1.Range[xlsRow, colClosingDate].Text = "Closing Date";
                sheet1.Range[xlsRow, colClosingDate].ColumnWidth = 12;

                xlsCol += 1;
                colEarlyBy = xlsCol;
                sheet1.Range[xlsRow, colEarlyBy].Text = "Early By";
                sheet1.Range[xlsRow, colEarlyBy].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colEarlyBy].ColumnWidth = 9;

                xlsCol += 1;
                colLateBy = xlsCol;
                sheet1.Range[xlsRow, colLateBy].Text = "Late By";
                sheet1.Range[xlsRow, colEarlyBy].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colLateBy].ColumnWidth = 9;
                xlsCol += 1;
                int colCurrentStatus = xlsCol;
                sheet1.Range[xlsRow, colCurrentStatus].Text = "Current Status";
                sheet1.Range[xlsRow, colCurrentStatus].ColumnWidth = 12;

                xlsCol += 1;
                colTaskType = xlsCol;
                sheet1.Range[xlsRow, colTaskType].Text = "Task Type";
                sheet1.Range[xlsRow, colTaskType].ColumnWidth = 10;

                xlsCol += 1;
                colTask = xlsCol;
                sheet1.Range[xlsRow, colTask].Text = "Task";
                sheet1.Range[xlsRow, colTask].ColumnWidth = 70;

                xlsCol += 1;
                colAssignTo = xlsCol;
                sheet1.Range[xlsRow, colAssignTo].Text = "Assigned To";
                sheet1.Range[xlsRow, colAssignTo].ColumnWidth = 25;
                xlsCol += 1;
                int colLastChat = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Last Activity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                colCategory = xlsCol;
                sheet1.Range[xlsRow, colCategory].Text = "Category";
                sheet1.Range[xlsRow, colCategory].ColumnWidth = 14;

                xlsCol += 1;
                colSubCategory = xlsCol;
                sheet1.Range[xlsRow, colSubCategory].Text = "Sub Category";
                sheet1.Range[xlsRow, colSubCategory].ColumnWidth = 14;



                xlsCol += 1;
                colAssignBy = xlsCol;
                sheet1.Range[xlsRow, colAssignBy].Text = "Assigned By";
                sheet1.Range[xlsRow, colAssignBy].ColumnWidth = 25;




               

                xlsCol += 1;
                int colBuyer = xlsCol;
                sheet1.Range[xlsRow, colBuyer].Text = "Buyer";
                sheet1.Range[xlsRow, colBuyer].ColumnWidth = 12;

                xlsCol += 1;
                int colDepartment = xlsCol;
                sheet1.Range[xlsRow, colDepartment].Text = "Department";
                sheet1.Range[xlsRow, colDepartment].ColumnWidth = 12;

                xlsCol += 1;
                int colDivision = xlsCol;
                sheet1.Range[xlsRow, colDivision].Text = "Division";
                sheet1.Range[xlsRow, colDivision].ColumnWidth = 12;

                xlsCol += 1;
                colMasterOrderNo = xlsCol;
                sheet1.Range[xlsRow, colMasterOrderNo].Text = "Master Order No";
                sheet1.Range[xlsRow, colMasterOrderNo].ColumnWidth = 16;

                xlsCol += 1;
                colStyleNo = xlsCol;
                sheet1.Range[xlsRow, colStyleNo].Text = "Line Item";
                sheet1.Range[xlsRow, colStyleNo].ColumnWidth = 9;

                xlsCol += 1;
                colSONo = xlsCol;
                sheet1.Range[xlsRow, colSONo].Text = "SO No";
                sheet1.Range[xlsRow, colSONo].ColumnWidth = 9;

                xlsCol += 1;
                colPRNo = xlsCol;
                sheet1.Range[xlsRow, colPRNo].Text = "PR No";
                sheet1.Range[xlsRow, colPRNo].ColumnWidth = 9;


                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Size = 8f;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Italic = true;
                fontRegular.Size = 6f;

                int StartRow = xlsRow;
                #region ----------------------Data-----------------------
                for (int i = 0; i < dtTNA.Rows.Count; i++)
                {
                   
                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, colTaskType].Text = dtTNA.Rows[i]["TaskType"].ToString();
                    sheet1.Range[xlsRow, colTask].Text = dtTNA.Rows[i]["Task"].ToString();
                    sheet1.Range[xlsRow, colAssignBy].Text = dtTNA.Rows[i]["AssignBy"].ToString();
                    sheet1.Range[xlsRow, colAssignTo].Text = dtTNA.Rows[i]["AssignTo"].ToString();
                    sheet1.Range[xlsRow, colDueDate].Text = dtTNA.Rows[i]["DueDate"].ToString();
                    sheet1.Range[xlsRow, colCommitmentDate].Text = dtTNA.Rows[i]["CommitmentDate"].ToString();
                    sheet1.Range[xlsRow, colLastChat].Text = dtTNA.Rows[i]["LastChat"].ToString();


                    sheet1.Range[xlsRow, colClosingDate].Text = dtTNA.Rows[i]["ClosingDate"].ToString();
                    sheet1.Range[xlsRow, colBuyer].Text = dtTNA.Rows[i]["Buyer"].ToString();
                    sheet1.Range[xlsRow, colDepartment].Text = dtTNA.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, colDivision].Text = dtTNA.Rows[i]["Division"].ToString();
                    sheet1.Range[xlsRow, colCurrentStatus].Text = dtTNA.Rows[i]["CurrentStatus"].ToString();


                    sheet1.Range[xlsRow, colMasterOrderNo].Text = dtTNA.Rows[i]["MasterOrderNo"].ToString();
                    sheet1.Range[xlsRow, colStyleNo].Text = dtTNA.Rows[i]["StyleNo"].ToString();
                    sheet1.Range[xlsRow, colSONo].Text = dtTNA.Rows[i]["SONo"].ToString();
                    sheet1.Range[xlsRow, colPRNo].Text = dtTNA.Rows[i]["PRNo"].ToString();
                    sheet1.Range[xlsRow, colSubCategory].Text = dtTNA.Rows[i]["SubCategory"].ToString();
                    sheet1.Range[xlsRow, colCategory].Text = dtTNA.Rows[i]["Category"].ToString();

                    double earlyOrLate = clsStaticInfo.dbl(dtTNA.Rows[i]["EarlyOrLateBy"].ToString());

                    double earlyBy = 0;
                    double lateBy = 0;
                    if (earlyOrLate < 0)
                    {
                        earlyBy = Math.Abs(earlyOrLate);
                    }
                    else if (earlyOrLate > 0)
                    {
                        lateBy = Math.Abs(earlyOrLate);
                    }



                    //today's task
                    DateTime DueDate = Convert.ToDateTime(dtTNA.Rows[i]["DueDate"].ToString());
                    DateTime CurrentDate = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy"));
                    if (DueDate == CurrentDate)
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#E6F0FF");


                    //overdue
                    if (DueDate < CurrentDate)
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#FFF4E6");

                    //overdue
                    if (DueDate > CurrentDate)
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#F5FFE6");




                    if (dtTNA.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED")
                    {
                        DateTime ClosingDate = Convert.ToDateTime(dtTNA.Rows[i]["ClosingDate"].ToString());
                        //late closed
                        if (DueDate < ClosingDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#52b3d9");



                        //early closed
                        if (DueDate >= ClosingDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#2ecc71");

                    }


                    #region Comments
                   
                    if (dicComments.ContainsKey(dtTNA.Rows[i]["TaskMasterId"].ToString()))
                    {
                        IRange range = sheet1[xlsRow, colTask];
                        ICommentShape shape = range.AddComment();

                        for (int COMM = 0; COMM < dicComments[dtTNA.Rows[i]["TaskMasterId"].ToString()].Count; COMM++)
                        {
                            DataRow drTempComment = dicComments[dtTNA.Rows[i]["TaskMasterId"].ToString()][COMM];
                            shape.RichText.Append(drTempComment["CommentedBy"].ToString() + " says :" + drTempComment["CommentText"].ToString(), fontCaption);
                            shape.RichText.Append(" " + drTempComment["CreatedTime"].ToString() + Environment.NewLine + Environment.NewLine, fontRegular);
                            shape.IsTextLocked = false;
                            shape.AutoSize = false;

                            shape.Height += 30;
                            shape.Width = 300;
                        }

                    }

                    #endregion Comments

                    sheet1.Range[xlsRow, colEarlyBy].Number = earlyBy;
                    sheet1.Range[xlsRow, colLateBy].Number = lateBy;

                    xlsRow++;
                    SLNo++;
                }
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                sheet1.Range[StartRow, 1, xlsRow - 1, endXlsCol].CellStyle.Font.Size = 8f;
                sheet1.AutoFilters.FilterRange = sheet1.Range[StartRow - 1, 1, xlsRow, endXlsCol];
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************
                xlsRow = 1;
                FactoryName = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Task List: ";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment
                sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange.WrapText = true;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + UserName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Task List";
                #endregion Page Setup

                #endregion  ManualOutTime



                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Worker Late Status
        [HttpPost, Authorize]
        public ActionResult GetTNAStatusReports(ReportFormat reportFormat, Dictionary<string, string> filterString)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = GetTNAStatusReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, identity.EmployeeId, identity.Name, filterString);

                workbook.Version = ExcelVersion.Excel2013;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "Task Status Reports.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        # endregion WORKER Late Status

        [HttpPost, Authorize]
        public ActionResult GetTaskList(Dictionary<string, string> filterString)
        {

            string sql = GetSql(filterString);
            DataTable dtTna = _sqlRepository.GetDataTable(sql);
            dtTna.Columns.Add("EarlyBy", typeof(int));
            dtTna.Columns.Add("LateBy", typeof(int));
            for (int i = 0; i < dtTna.Rows.Count; i++)
            {
                if (dtTna.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED" && dtTna.Rows[i]["ClosingDate"].ToString() != "")
                {
                    try
                    {
                        DateTime dtDueDate = Convert.ToDateTime(dtTna.Rows[i]["DueDate"].ToString());
                        DateTime dtClosingDate = Convert.ToDateTime(dtTna.Rows[i]["ClosingDate"].ToString());
                        if (dtClosingDate < dtDueDate)
                            dtTna.Rows[i]["EarlyBy"] = Math.Abs(clsStaticInfo.dateDiff(dtClosingDate.ToString("dd-MMM-yyyy"), dtDueDate.ToString("dd-MMM-yyyy")));
                        if (dtClosingDate > dtDueDate)
                            dtTna.Rows[i]["LateBy"] = Math.Abs(clsStaticInfo.dateDiff(dtDueDate.ToString("dd-MMM-yyyy"), dtClosingDate.ToString("dd-MMM-yyyy")));
                    }
                    catch (Exception ex)
                    {

                    }
                  
                }
            }

            var jsondata = Json(CustomJsonResultService.DataTableToJson(dtTna), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

    }
}