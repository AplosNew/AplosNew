using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Library.Model.Accounts;
using Library.Service.Core;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Data;
using Syncfusion.XlsIO;
using Library.ViewModel.Organizations;
using Library.Core;
using Library.Data.Repositories;
using Library.Model.Setups;
using Library.ViewModel.Setups;
using System.Web.UI.WebControls;

namespace Library.Service.Extension.Mail
{
    public class TNAService
    {
        SqlRepository _sqlRepository;

        public TNAService()
        {
            _sqlRepository = new SqlRepository();
        }
        public void GetTNAStatusReportsData(out DataTable dtTna, Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {

            MasterOrderDataTablesForGrid(Filter, FilterFields, out dtTna);

            dtTna.Columns.Add("EarlyBy", typeof(int));
            dtTna.Columns.Add("LateBy", typeof(int));
            for (int i = 0; i < dtTna.Rows.Count; i++)
            {
                try
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
                catch (Exception)
                {

                }
            }
        }
        public void MasterOrderDataTablesForGrid(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields, out DataTable MainData)
        {
            string DueDate = "TT.OriginalSequentialEndDate";//or ATO.DueDate
            string FilterText = " WHERE 1=1 ";
            if (FilterFields != null)
            {
                for (int i = 0; i < FilterFields.Count; i++)
                {
                    FilterText += " AND isnull(RTRIM(LTRIM(" + FilterFields[i]["Key"].ToString() + ")),'') IN (" + FilterFields[i]["Value"].ToString().Replace("' ", "'").Replace("', '", "','").Replace(", ", ",") + ")  ";
                }

            }
            string TaskTypeFilter = "";
            if (Filter["ReportLevel"].ToString() != "ALL")
                TaskTypeFilter = "WHERE tao.TaskAppliedOnEnum='" + Filter["ReportLevel"].ToString() + "'";


            if (Filter["ActiveStatus"].ToString() != "All")
            {
                if (Filter["ActiveStatus"].ToString() == "Closed")
                    TaskTypeFilter += " AND TM.CurrentStatus='" + Filter["ActiveStatus"].ToString() + "'";
                else
                    TaskTypeFilter += " AND isnull(TM.CurrentStatus,'')<>'Closed'";
            }
            if (Filter["DateSelection"].ToString() != "WITHOUTDATE")
            {
                if (Filter["DateSelection"].ToString() == "WITHDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";
                    else
                        TaskTypeFilter += " AND " + DueDate + " between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";

                }
                else if (Filter["DateSelection"].ToString() == "WITHOUTDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                    else
                        TaskTypeFilter += " AND " + DueDate + "<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";

                }
            }


            string sql = @"SELECT K.*
                                  FROM (SELECT 
                                TAM.ProcessId,CASE WHEN tm.CurrentStatus='Closed' THEN format(tm.ClosingDate,'dd-MMM-yyyy') ELSE NULL END AS ClosingDate,
                                CASE WHEN tm.CurrentStatus='Closed' THEN isnull(USRCL.FullName,isnull(EACL.EmployeeName,TM.ClosedBy)) ELSE NULL END AS ClosedBy,
                                eato.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,TM.CurrentStatus,
                               mott.Sequence, isnull(TAM.TaskCategoryId,'')TaskCategoryId,isnull(TAM.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                tc.UserName AS Category,tsc.UserName as SubCategory,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat,
                                format(" + DueDate + @",'dd-MMM-yyyy') AS DueDate,
                                format(OriginalSequentialStartDate,'dd-MMM-yyyy') AS OriginalSequentialStartDate,	format(OriginalSequentialEndDate,'dd-MMM-yyyy') AS OriginalSequentialEndDate,
                                format(TempStartDate,'dd-MMM-yyyy') AS TempStartDate,	format(TempEndDate,'dd-MMM-yyyy') AS TempEndDate,
                                concat(TM.TaskType,'/',MO.Dependency) AS TaskType,
                                datediff(day," + DueDate + @",TM.closingDate) AS EarlyOrLateBy,
	                            tm.TaskDescription AS Task,format(ISNULL(ATO.RevisedCommitmentDate,ISNULL(ATO.CommitmentDate,NULL)),'dd-MMM-yyyy') AS CommitmentDate,
								EAB.EmployeeName AS AssignBy,EATO.EmployeeName AS AssignTo,
                                MO.*
                                 FROM TaskManagerMaster AS tm
                                    inner join (" + TNATasks() + @") AS MO on MO.TaskMasterId=tm.Id
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                                LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'

                                LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EACL ON EACL.SystemId=TM.ClosedBy
                                LEFT OUTER JOIN SEC.[USER] AS USRCL ON USRCL.UserId=TM.ClosedBy

                                LEFT OUTER JOIN org.Department AS DTO ON dto.Id=eato.DepartmentId
                                left outer join TaskComments TSK on TSK.TaskManagerMasterId=TM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TM.ID ORDER BY T.CreatedTime DESC)

                              
                                LEFT OUTER JOIN MasterOrderTaskTemplate AS mott ON mott.Id=MO.TaskTemplateId
                                LEFT OUTER JOIN TaskMaster AS TAM ON TAM.Id=mott.TaskMasterId
                                INNER JOIN hkp.TaskCategory AS tc ON TAM.TaskCategoryId=tc.Id AND TC.Active=1
                                INNER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=TAM.TaskSubCategoryId AND TSC.Active=1

                                LEFT OUTER JOIN hkp.Process AS p ON p.Id=TAM.ProcessId
                                INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tam.TaskAppliedOnId
                                    " + TaskTypeFilter + @"
                                ) AS K " + FilterText + @"  ORDER BY Buyer,StyleNo,SONo,PRNo";

            MainData = _sqlRepository.GetDataTable(sql);

        }
        public string TNATasks()
        {
            string sql = @" 	SELECT  'Order' AS Dependency, tt.TaskTemplateId,TMMM.Id AS TaskMasterId, 
                                    	     MO.MasterOrderNo AS MasterOrderId,MO.BuyerId,
                             B.UserName AS Buyer
                            
                            ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                            trn.MasterOrderItem XMOI 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SONo=STUFF((select distinct ','+so.Id from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SOQty=(select sum(SO.Qty) from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId),

                            PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                          
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMMM

                              INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMMM.TNATasksId 
                            LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[MasterOrder] AS MO ON MO.Id = TM.MasterOrderId
                             LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    						
                            UNION

                            SELECT  'Item' AS Dependency, tt.TaskTemplateId, TMM.Id AS TaskMasterId,
                             MO.MasterOrderNo,B.Id, B.UserName AS Buyer
                            ,StyleNo= MOI.BuyerReferenceNo,
                            SONo=STUFF((select distinct ','+so.Id from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SOQty=(select sum(so.Qty) from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId),

                            PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMM

                            LEFT OUTER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                            inner JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = TM.MasterOrderItemId
                            LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                            LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId 
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    

						
                            UNION 

                            SELECT 'Sales Order' AS Dependency, tt.TaskTemplateId, TMM.Id AS TaskMasterId,
                               MO.MasterOrderNo,B.Id, B.UserName AS Buyer
                            ,StyleNo= MOI.BuyerReferenceNo
                            ,SONo=so.Id
                            ,SOQty=SO.Qty
                            ,PRNo=STUFF((select distinct ','+xpod.ProductionOrderId from  trn.ProductionOrderDetail AS xpod
                            where xpod.SalesOrderId = so.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                          
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMM

                              INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                            LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[SalesOrder] AS SO ON SO.Id =  TM.SalesOrderId
                            LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
                            LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                            LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    
                            UNION 

                          

                            SELECT 'Prod. Order' AS Dependency,tt.TaskTemplateId, TMM.Id AS TaskMasterId, 
                               pr.MasterOrderId,PR.BuyerId,pr.Buyer,pr.StyleNo, pr.SONo,PR.SOQty, pr.ProductionOrderId
                            ,Department=bd.UserName,Division=bd2.UserName
				
                                 FROM TaskManagerMaster AS tmm
                                INNER JOIN TNATasks AS TT ON TT.Id=tmm.TNATasksId
                                INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId  AND isnull(t.ProductionOrderId,'')<>''
                                    INNER JOIN trn.ProductionOrder AS po ON PO.Id=t.ProductionOrderId
                                INNER JOIN
                                (
                                			SELECT distinct po.Id AS ProductionOrderId,mo.BuyerDepartmentId,mo.BuyerDivisionId,
                                			b.Id AS BuyerId,b.UserName AS Buyer,
                                			
                                			 MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
											 
											 ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                	
                                			  ,SONo=STUFF((select distinct ','+sox.Id from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                ,SOQty=(select sum(sox.Qty) from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id)
				                                
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
								LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=PR.BuyerDivisionId  ";




            return sql;
        }

        public Dictionary<string, List<DataRow>> GetSqlTaskComments(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {
            string FilterText = " WHERE 1=1 ";
            if (FilterFields != null)
            {
                for (int i = 0; i < FilterFields.Count; i++)
                {
                    FilterText += " AND isnull(" + FilterFields[i]["Key"].ToString() + ",'') IN (" + FilterFields[i]["Value"].ToString() + ")  ";
                }

            }
            string TaskTypeFilter = "";
            if (Filter["ReportLevel"].ToString() != "ALL")
                TaskTypeFilter = "WHERE tao.TaskAppliedOnEnum='" + Filter["ReportLevel"].ToString() + "'";


            if (Filter["ActiveStatus"].ToString() != "All")
            {
                if (Filter["ActiveStatus"].ToString() == "Closed")
                    TaskTypeFilter += " AND TM.CurrentStatus='" + Filter["ActiveStatus"].ToString() + "'";
                else
                    TaskTypeFilter += " AND isnull(TM.CurrentStatus,'')<>'" + Filter["ActiveStatus"].ToString() + "'";
            }
            if (Filter["DateSelection"].ToString() != "WITHOUTDATE")
            {
                if (Filter["DateSelection"].ToString() == "WITHDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";
                    else
                        TaskTypeFilter += " AND ATO.DueDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";

                }
                else if (Filter["DateSelection"].ToString() == "WITHOUTDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                    else
                        TaskTypeFilter += " AND ATO.DueDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";

                }
            }
            string sql = @"SELECT K.*
                                  FROM (SELECT 
                               Tm.ID TaskManagerMasterId, TAM.ProcessId,CASE WHEN tm.CurrentStatus='Closed' THEN format(tm.ClosingDate,'dd-MMM-yyyy') ELSE NULL END AS ClosingDate,
                                eato.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,TM.CurrentStatus,
                                isnull(TAM.TaskCategoryId,'')TaskCategoryId,isnull(TAM.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                tc.UserName AS Category,tsc.UserName as SubCategory,
                                format(ato.DueDate,'dd-MMM-yyyy') AS DueDate,concat(TM.TaskType,'/',MO.Dependency) AS TaskType,
                                datediff(day,ATO.duedate,TM.closingDate) AS EarlyOrLateBy,
                                FORMAT(tcom.CreatedTime,'dd-MMM-yyyy HH:mm:ss tt') AS CreatedTime,ei.EmployeeName AS CommentedBy,
                                    tcom.CommentText,
	                            tm.TaskDescription AS Task,format(ISNULL(ATO.RevisedCommitmentDate,ISNULL(ATO.CommitmentDate,NULL)),'dd-MMM-yyyy') AS CommitmentDate,
								EAB.EmployeeName AS AssignBy,EATO.EmployeeName AS AssignTo,
                                MO.*
                                 FROM TaskManagerMaster AS tm
                                    inner join (" + TNATasks() + @") AS MO on MO.TaskMasterId=tm.Id

                                INNER JOIN TaskComments AS tcom ON tcom.TaskManagerMasterId=tm.Id
                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=tcom.CreatedById

                                LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                                LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'

                                LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId

                                LEFT OUTER JOIN org.Department AS DTO ON dto.Id=eato.DepartmentId

                              
                                LEFT OUTER JOIN MasterOrderTaskTemplate AS mott ON mott.Id=MO.TaskTemplateId
                                LEFT OUTER JOIN TaskMaster AS TAM ON TAM.Id=mott.TaskMasterId
                                INNER JOIN hkp.TaskCategory AS tc ON TAM.TaskCategoryId=tc.Id AND TC.Active=1
                                INNER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=TAM.TaskSubCategoryId AND TSC.Active=1

                                LEFT OUTER JOIN hkp.Process AS p ON p.Id=TAM.ProcessId
                                INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tam.TaskAppliedOnId
                                    " + TaskTypeFilter + @"
                                ) AS K " + FilterText + @"   order by TaskManagerMasterId,convert(datetime,CreatedTime)";




            //string sql = @"select * from (SELECT tcom.TaskManagerMasterId, TMM.CurrentStatus, TSC.UserName as SubCategory,TC.UserName as Category, TMM.TaskType,
            //                    TMM.TaskDescription AS Task, EBy.EmployeeName as AssignBy,ETo.EmployeeName as AssignTo,
            //                    format(TATo.DueDate,'dd-MMM-yyyy') as DueDate, ISNULL(format(TATo.RevisedCommitmentDate,'dd-MMM-yyyy'),
            //                    ISNULL(format(TATo.CommitmentDate,'dd-MMM-yyyy'),format(TATo.DueDate,'dd-MMM-yyyy'))) as CommitmentDate,  NULL MasterOrderNo,
            //                FORMAT(tcom.CreatedTime,'dd-MMM-yyyy HH:mm:ss tt') AS CreatedTime,ei.EmployeeName AS CommentedBy,
            //                        tcom.CommentText,
            //                Buyer=null
            //                ,StyleNo= NULL
            //                ,SONo=NULL
            //                ,PRNo=NULL
            //                ,datediff(day,tato.duedate,TMM.closingDate) AS EarlyOrLateBy,FORMAT(TMM.ClosingDate,'dd-MMM-yyyy') AS ClosingDate
            //                ,Department=NULL,Division=NULL
            //                FROM TaskManagerMaster AS TMM
            //                 INNER JOIN TaskComments AS tcom ON tcom.TaskManagerMasterId=tmm.Id
            //                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=tcom.CreatedById

            //                LEFT OUTER JOIN TaskAudit AS TATo on TATo.TaskManagerMasterId = TMM.Id AND TATo.AuthorizationType = 'AssignTo'
            //                LEFT OUTER JOIN TaskAudit AS TABy on TABy.TaskManagerMasterId = TMM.Id AND TABy.AuthorizationType = 'CreatedBy' 
            //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS ETo ON ETo.SystemId = TATo.ResponsiblePersonId 
            //                LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EBy ON EBy.SystemId = TABy.ResponsiblePersonId 
            //                LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
            //                LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
            //                where isnull(TMM.isOwnTask,0)=0) AS K " + FilterString + " order by TaskManagerMasterId,convert(datetime,CreatedTime)";
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

    }

}
