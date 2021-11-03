using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Planning.PlanningType1
{
    public class ProductionPlanningReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public ProductionPlanningReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }

        public IEnumerable<object> GetSnapShotNames(string SnapShotType)
        {
            try
            {
                string _sql = "";

                if (SnapShotType == "SnapShotType1")
                {
                   
                   _sql = @"select m.ID as SnapId,m.SnapshotName,FORMAT(m.SnapshotDate,'dd-MMM-yyyy') SnapshotDate,
                 FORMAT(min(c.ProductionDate), 'dd-MMM-yyyy') MinDate, FORMAT(max(c.ProductionDate), 'dd-MMM-yyyy') MaxDate
                from 
                dbo.ProductionPlanningSnapshotMasterType1 m
                 left join dbo.ProductionPlanningSnapshotType1 c on m.ID = c.ProductionPlanningSnapshotMasterType1
                 group by m.SnapshotName,m.SnapshotDate,m.ID order by SnapshotDate asc";
                }
              
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetSnapShotData(string From,string To, string SnapShotType,string SnapId)
        {
            try
            {
                var Snap = string.Empty;
                string _sql = "";
                if (SnapId == null)
                {
                    Snap = "";
                }
                else
                {
                    Snap = @"AND pp.ID = '" + SnapId + @"'";
                }
                if (SnapShotType == "SnapShotType1")
                {
                    
                _sql = @"select distinct pp.Id as MasterId,pp.SnapshotName,FORMAT(pp.SnapshotDate, 'dd-MMM-yyyy') as SnapshotDate,pp.SnapshotDesc,tp.ProductionOrderID,
                tp.WorkCenterMasterId,w.UserName as WorkCenter,
                tp.MaterialMasterId,tp.EntityID,e.UserName as Entity,pl.Id as PlantId,pl.UserName as Plant,cm.Id as CompanyId,cm.UserName as Company,tp.ProcessID,
                pr.UserName as Process,XP.UserName as Customer,xp.Id as CustomerId,tp.ProductionDate,tp.Quantity,tp.ProductionHours,
                tp.isBuildUp,tp.isStyleChange,tp.BlockNo
                from ProductionPlanningSnapshotMasterType1 pp
                left join ProductionPlanningSnapshotType1 tp on tp.ProductionPlanningSnapshotMasterType1=pp.ID
                left join org.Entity e on e.Id=tp.EntityID
                left join org.Plant pl on pl.Id=e.PlantId
                left join org.Company cm on cm.Id=pl.CompanyId
                left join hkp.Process pr on pr.Id=tp.ProcessID
                left join scs.WorkCenterMaster w on w.Id=tp.WorkCenterMasterId
                left join trn.ProductionOrderDetail AS Xpod ON xpod.ProductionOrderId=tp.ProductionOrderID
                JOIN  trn.SalesOrder AS Xso ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId			                                                    
                where ProductionDate between '" + From + "' and '" + To + @"'"+Snap+"";
                }
                else
                {
                 
                    TimeSpan ts = Convert.ToDateTime(To).Subtract(Convert.ToDateTime(From));
                    if (Math.Abs(ts.Days) > 120)
                    {
                        throw new Exception("Please choose a date range between 120 days");
                    }
                _sql = @"select distinct pp.Id as MasterId,pp.SnapshotName,FORMAT(pp.SnapshotDate, 'dd-MMM-yyyy') as SnapshotDate,pp.SnapshotDesc,tp.ProductionOrderID,
                tp.WorkCenterMasterId,w.UserName as WorkCenter,
                tp.MaterialMasterId,tp.EntityID,e.UserName as Entity,pl.Id as PlantId,pl.UserName as Plant,cm.Id as CompanyId,cm.UserName as Company,tp.ProcessID,
                pr.UserName as Process,XP.UserName as Customer,xp.Id as CustomerId,tp.ProductionDate,tp.Quantity,tp.ProductionHours,
                tp.isBuildUp,tp.isStyleChange,tp.BlockNo
                from ProductionPlanningSnapshot2MasterType1 pp
                left join ProductionPlanningSnapshot2Type1 tp on tp.ProductionPlanningSnapshot2MasterType1Id=pp.ID
                left join org.Entity e on e.Id=tp.EntityID
                left join org.Plant pl on pl.Id=e.PlantId
                left join org.Company cm on cm.Id=pl.CompanyId
                left join hkp.Process pr on pr.Id=tp.ProcessID
                left join scs.WorkCenterMaster w on w.Id=tp.WorkCenterMasterId
                left join trn.ProductionOrderDetail AS Xpod ON xpod.ProductionOrderId=tp.ProductionOrderID
                JOIN  trn.SalesOrder AS Xso ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId			                                                    
                where ProductionDate between '" + From + "' and '" + To + "'";

                }

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetReportData(string From, string To, string SnapShotType,
            string CompanyId, string SnapDate, string PlantId, string EntityId, string ProcessId, string SnapName, string WkCenterId,
            string CustomerId, string POId)
        {
            try
            {               

                string _sql = "";
                if (SnapShotType == "SnapShotType1")
                {
                    _sql = @"select pp.Id as MasterId,pp.SnapshotName,FORMAT(pp.SnapshotDate, 'dd-MMM-yyyy') as SnapshotDate,pp.SnapshotDesc,tp.ProductionOrderID,
                tp.WorkCenterMasterId,w.UserName as WorkCenter,
                tp.MaterialMasterId,tp.EntityID,e.UserName as Entity,pl.Id as PlantId,pl.UserName as Plant,cm.Id as CompanyId,
                cm.UserName as Company,tp.ProcessID,
                pr.UserName as Process,XP.UserName as Customer,xp.Id as CustomerId,FORMAT(tp.ProductionDate, 'dd-MMM-yyyy') as ProductionDate,tp.Quantity,
                tp.ProductionHours,
                tp.isBuildUp,tp.isStyleChange,tp.BlockNo
                from ProductionPlanningSnapshotMasterType1 pp
                left join ProductionPlanningSnapshotType1 tp on tp.ProductionPlanningSnapshotMasterType1=pp.ID
                left join org.Entity e on e.Id=tp.EntityID
                left join org.Plant pl on pl.Id=e.PlantId
                left join org.Company cm on cm.Id=pl.CompanyId
                left join hkp.Process pr on pr.Id=tp.ProcessID
                left join scs.WorkCenterMaster w on w.Id=tp.WorkCenterMasterId
                left join trn.ProductionOrderDetail AS Xpod ON xpod.ProductionOrderId=tp.ProductionOrderID
                JOIN  trn.SalesOrder AS Xso ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId			                                                    
                where ProductionDate between '" + From + @"' and '" + To + @"'
				and isnull(e.PlantId ,'') IN(" + PlantId + @")              
                         AND isnull(tp.EntityID, '') IN(" + EntityId + @")  AND
                          isnull(tp.ProcessID, '') IN(" + ProcessId + @") AND
                          isnull(pl.CompanyId, '') IN(" + CompanyId + @") AND
                          isnull(pp.SnapshotDate, '') IN(" + SnapDate + @") AND
                          isnull(pp.SnapshotName, '') IN(" + SnapName + @") AND
                          isnull(tp.WorkCenterMasterId, '') IN(" + WkCenterId + @") AND
                          isnull(tp.ProductionOrderID, '') IN(" + POId + @") AND
                          isnull(xp.Id, '') IN(" + CustomerId + @")";
                   
                }
                else
                {
                    _sql = @"select pp.Id as MasterId,pp.SnapshotName,FORMAT(pp.SnapshotDate, 'dd-MMM-yyyy') as SnapshotDate,pp.SnapshotDesc,tp.ProductionOrderID,
                tp.WorkCenterMasterId,w.UserName as WorkCenter,
                tp.MaterialMasterId,tp.EntityID,e.UserName as Entity,pl.Id as PlantId,pl.UserName as Plant,cm.Id as CompanyId,
                cm.UserName as Company,tp.ProcessID,
                pr.UserName as Process,XP.UserName as Customer,xp.Id as CustomerId,FORMAT(tp.ProductionDate, 'dd-MMM-yyyy') as ProductionDate,tp.Quantity,
                tp.ProductionHours,
                tp.isBuildUp,tp.isStyleChange,tp.BlockNo
                from ProductionPlanningSnapshot2MasterType1 pp
                left join ProductionPlanningSnapshot2Type1 tp on tp.ProductionPlanningSnapshot2MasterType1Id=pp.ID
                left join org.Entity e on e.Id=tp.EntityID
                left join org.Plant pl on pl.Id=e.PlantId
                left join org.Company cm on cm.Id=pl.CompanyId
                left join hkp.Process pr on pr.Id=tp.ProcessID
                left join scs.WorkCenterMaster w on w.Id=tp.WorkCenterMasterId
                left join trn.ProductionOrderDetail AS Xpod ON xpod.ProductionOrderId=tp.ProductionOrderID
                JOIN  trn.SalesOrder AS Xso ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId			                                                    
                where ProductionDate between '" + From + @"' and '" + To + @"'
				and isnull(e.PlantId ,'') IN(" + PlantId + @")              
                         AND isnull(tp.EntityID, '') IN(" + EntityId + @")  AND
                          isnull(tp.ProcessID, '') IN(" + ProcessId + @") AND
                          isnull(pl.CompanyId, '') IN(" + CompanyId + @") AND
                          isnull(pp.SnapshotDate, '') IN(" + SnapDate + @") AND
                          isnull(pp.SnapshotName, '') IN(" + SnapName + @") AND
                          isnull(tp.WorkCenterMasterId, '') IN(" + WkCenterId + @") AND
                          isnull(tp.ProductionOrderID, '') IN(" + POId + @") AND
                          isnull(xp.Id, '') IN(" + CustomerId + @")";
                   
                }

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }    
}
