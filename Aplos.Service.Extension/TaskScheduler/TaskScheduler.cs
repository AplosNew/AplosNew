#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.TaskScheduler;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace Library.Service.Extension.TaskScheduler
{
    public class TaskScheduler
    {
        #region Constructor

        private readonly SqlRepository _sqlRepository;

        public TaskScheduler()
        {
            _sqlRepository =new SqlRepository();
        }
        private DataSet getDataset(string sql)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

            return dsMaster;
        }
        #endregion Constructor

        public void GetDataSourceMasterOrderNew(string MasterOrderId,out DataTable dtData, out DataTable dtRelations, out DataTable dtTaskDelayedEndDate, out DataTable dtCalendar)
        {
            string sql = "";
            _sqlRepository.ExecuteSqlCommand(@"UPDATE MasterOrderTaskTemplate SET TaskDependentDatesId = tm.TaskDependentDatesId
                                                    FROM MasterOrderTaskTemplate AS mott 
                                                    INNER JOIN TaskMaster AS tm ON tm.Id=mott.TaskMasterId
                                                    WHERE ISNULL(mott.TaskDependentDatesId,'')=''
                                                
                                                UPDATE TaskTemplate SET TaskDependentDatesId = tm.TaskDependentDatesId
                                                    FROM TaskTemplate AS mott 
                                                    INNER JOIN TaskMaster AS tm ON tm.Id=mott.TaskMasterId
                                                    WHERE ISNULL(mott.TaskDependentDatesId,'')=''");
            sql = @"SELECT d.PreTaskTemplateId, isnull(d.TaskTemplateId,tt.Id) AS TaskTemplateId, d.Criteria,  
                                    isnull(d.LagDays,0) AS LagDays,isnull(tt.LagDays,0) AS OwnLagDays,tt.Id, tt.TaskMasterId,'' AS DependentDate,
                                --CASE WHEN ISNULL(d.Criteria,'')='' THEN isnull(tt.LagDays,0) ELSE isnull(d.LagDays,0) END AS LagDays,
                                                            '' AS TempStartDate,'' AS TempEndDate,'' AS ActualStartDate,'' AS ActualEndDate,'' AS SequentialStartDate,'' AS SequentialEndDate,'' AS OriginalSequentialStartDate,'' AS OriginalSequentialEndDate,
                                                                        tt.TaskDescription,convert(bit,1) AS HasActualDate,convert(bit,1) AS HasPredecessorActualDate,'NO' AS isPredecessorDelayed,'NO' AS isCurrentDelayed,
                                                                            convert(INT,(isnull(ei.SystemId,'0'))) AS resourceId,ei.employeename as resourceName,ei.EmpPicPath,
                                                                    tt.[Active], tt.Sequence,  CASE WHEN ISNULL(rpt.IsRepeat,0)=1 AND ISNULL(tt.ForNewOrder,0)=1 THEN 0 
                                                                    ELSE tt.Duration END AS Duration, tt.startDate, tt.endDate,isnull(tm.ConsiderOffDays,0) AS ConsiderOffDays,
                                                                    EI.SystemId AS EmployeeId, tt.ForNewOrder, tt.IsMandatory, tt.TaskType,
                                                                    tt.IsTaskMilestone, tt.TaskDependentDatesId, tt.TaskAppliedOnId,aon.TaskAppliedOnEnum,tdd.DependentDatesEnum,
                                                                    tt.ResponsiblePersonCategory, tt.IsFirstTask, tt.IsLastTask
                                                            FROM MasterOrderTaskTemplate AS tt 
                                                            LEFT OUTER JOIN MasterOrderTaskTemplateDependency AS D ON d.TaskTemplateId=tt.Id AND d.Id=(SELECT TOP 1 Id FROM MasterOrderTaskTemplateDependency WHERE TaskTemplateId=tt.Id)
                                                            LEFT OUTER JOIN TaskMaster AS tm ON tm.Id=tt.TaskMasterId
                                                            LEFT OUTER JOIN hkp.TaskAppliedOn AS AON ON aon.Id=tt.TaskAppliedOnId
                                                            LEFT OUTER JOIN hkp.TaskDependentDates AS tdd ON tdd.Id=tt.TaskDependentDatesId  
                                                            LEFT OUTER JOIN trn.MasterOrder AS mo ON mo.Id=tt.MasterOrderId
                                                            LEFT OUTER JOIN (SELECT TOP 1 moi.MasterOrderId,IsRepeat FROM trn.MasterOrderItem AS moi 
                                                                            WHERE moi.MasterOrderId='" + MasterOrderId + @"' AND ISNULL(moi.IsRepeat,0)=0) as RPT ON RPT.MasterOrderId=MO.Id
                                                            LEFT OUTER JOIN EntityTask AS et ON et.EntityId=mo.EntityId AND et.TaskMasterId=tt.TaskMasterId AND tt.ResponsiblePersonCategory='Entity'
                                                            LEFT OUTER JOIN mst.BuyerMaster AS bm ON isnull(bm.BuyerId,'')=isnull(mo.BuyerId,'')
										                                                            AND ISNULL(bm.BuyerDepartmentId,isnull(mo.BuyerDepartmentId,''))=isnull(mo.BuyerDepartmentId,'')
										                                                            AND ISNULL(bm.BuyerDivisionId,isnull(mo.BuyerDivisionId,''))=isnull(mo.BuyerDivisionId,'')
										
                                                            LEFT OUTER JOIN BuyerMasterTask AS bmt ON bmt.BuyerMasterId=bm.Id AND tt.TaskMasterId=bmt.TaskMasterId AND tt.ResponsiblePersonCategory='Buyer' AND bmt.Active=1
                                                            LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=ISNULL(tt.EmployeeId,ISNULL(bmt.EmpSystemId,et.EmpSystemId))
                                                            WHERE tt.MasterOrderId='" + MasterOrderId + @"' ORDER BY convert(int,isnull(tt.RefTaskTemplateId,999999999)),convert(int,tt.Id)";

             dtData = _sqlRepository.GetDataTable(sql);
            sql = @"SELECT isnull(D.Id,tt.Id) AS Id,  d.PreTaskTemplateId,convert(bit,1) AS HasActualDate,'NO' AS isCurrentDelayed,
                                                                    isnull(d.TaskTemplateId,tt.Id) AS TaskTemplateId, d.Criteria, isnull(tm.ConsiderOffDays,0) AS ConsiderOffDays,
                                                                    --CASE WHEN ISNULL(d.Criteria,'')='' THEN isnull(tt.LagDays,0) ELSE isnull(d.LagDays,0) END AS LagDays,
                                                                    isnull(d.LagDays,0) AS LagDays,isnull(tt.LagDays,0) AS OwnLagDays,
                                                                      CASE WHEN ISNULL(rpt.IsRepeat,0)=1 AND ISNULL(tt.ForNewOrder,0)=1 THEN 0 
                                                                            ELSE tt.Duration END AS Duration,'' AS TempStartDate,'' AS TempEndDate,'' AS DependentDate,
                                                                    '' AS ActualStartDate,'' AS ActualEndDate,'' AS SequentialStartDate,'' AS SequentialEndDate
                                                                    ,aon.TaskAppliedOnEnum,tdd.DependentDatesEnum
                                                                    FROM MasterOrderTaskTemplate AS tt 
                                                                    LEFT JOIN MasterOrderTaskTemplateDependency AS D ON d.TaskTemplateId=tt.Id 
                                                                    LEFT OUTER JOIN (SELECT TOP 1 moi.MasterOrderId,IsRepeat FROM trn.MasterOrderItem AS moi 
                                                                            WHERE moi.MasterOrderId='" + MasterOrderId + @"' AND ISNULL(moi.IsRepeat,0)=0) as RPT ON RPT.MasterOrderId=tt.MasterOrderId
                                                         
                                                                    LEFT OUTER JOIN TaskMaster AS tm ON tm.Id=tt.TaskMasterId
                                                                    LEFT OUTER JOIN hkp.TaskAppliedOn AS AON ON aon.Id=tt.TaskAppliedOnId
                                                                    LEFT OUTER JOIN hkp.TaskDependentDates AS tdd ON tdd.Id=tt.TaskDependentDatesId     
                                                         WHERE tt.MasterOrderId='" + MasterOrderId + @"'
                                                        ORDER BY convert(int,isnull(tt.RefTaskTemplateId,999999999)),convert(int,d.TaskTemplateId)";
            dtRelations = _sqlRepository.GetDataTable(sql);


            dtTaskDelayedEndDate = _sqlRepository.GetDataTable(@"SELECT MT.Id,
                                                    FORMAT(GETDATE(),'dd-MMM-yyyy') AS TaskNewEndDate FROM TaskManagerMaster TM
                                                    INNER JOIN TNATasks AS t ON tm.TNATasksId=t.Id
                                                    INNER JOIN MasterOrderTaskTemplate AS MT ON mt.Id=t.TaskTemplateId
                                                    INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tm.Id AND ta.AuthorizationType='AssignTo'

                                                    WHERE mt.MasterOrderId='" + MasterOrderId + @"' AND convert(date,GETDATE())>=convert(date,ta.DueDate) AND tm.CurrentStatus<>'CLOSED'");


            DataTable dtCal = _sqlRepository.GetDataTable(@"select XMO.AddedDate from trn.MasterOrder XMO WHERE XMO.Id='" + MasterOrderId + "'");
            DateTime dtCreationDate = Convert.ToDateTime(dtCal.Rows[0]["AddedDate"].ToString());

             sql = @"SELECT * FROM PlantCalendar AS pc
                                                                    WHERE PC.PlantId=(select PlantId from TRN.MasterOrder where Id='" + MasterOrderId + @"') AND convert(date,WorkingDate) between '" + dtCreationDate.AddMonths(-3).ToString("dd-MMM-yyyy") + @"' and '" + dtCreationDate.AddMonths(36).ToString("dd-MMM-yyyy") + @"'
                                                                    ORDER BY pc.WorkingDate";
            dtCalendar = _sqlRepository.GetDataTable(sql);

        }
    }
}