using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Dashboard
{
    public class MachineMapping
    {
        private readonly SqlRepository _sqlRepository;
        public MachineMapping()
        {
            _sqlRepository = new SqlRepository();

        }


        public IEnumerable<object> leftGridData(Dictionary<string, string> parameters)
        {
            try
            {
                var str = @"SELECT  mo.PlantId,c.id AS CompanyId,p.UserName AS PlantName,c.UserName AS CompanyName,COUNT(so.Id) AS NoOfSO,SUM(so.Qty) AS SOQty,
sum(CASE WHEN isnull(pod.Id,'')='' THEN 1 ELSE 0 END)  AS PendingSOForPR,
 sum(CASE WHEN isnull(pbt.Id,'')='' THEN 1 ELSE 0 END)  AS BulletinToAttach,
sum(CASE WHEN so.DeliveryDate<= DATEADD(DAY,45,GETDATE()) AND isnull(pbt.Id,'')='' THEN 1 ELSE 0 END) BulletinToAttachW45Days
FROM trn.MasterOrder AS mo
INNER JOIN trn.MasterOrderItem MOI ON moi.MasterOrderId=mo.Id
JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
JOIN org.Plant AS p ON p.Id=mo.PlantId
JOIN ORg.Company AS c ON c.Id=p.CompanyId
LEFT JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
LEFT JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
LEFT JOIN trn.ProductionBulletinTemplate AS pbt ON pbt.ProductionOrderId=po.Id
WHERE isnull(p.CompanyId,'') IN(" + parameters["CompanyId"] + @") AND
isnull(P.Id,'') IN(" + parameters["PlantId"] + @") AND
isnull(mo.EntityId,'') IN(" + parameters["EntityId"] + @") 
GROUP BY  mo.PlantId,c.id,p.UserName,c.UserName";
                return _sqlRepository.GetDataCollection(str);

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> allFilterLists()
        {
            try
            {
                //var str = @"Select CMP.UserName as Company, CMP.Id as CompanyId,PLN.UserName AS Plant,PLN.Id as PlantId,E.UserName AS Entity,E.Id as EntityId,
                //            sd.UserName as [Shift],
                //            ISNULL (om.SkillId,'') as SkillId,ISNULL( om.UserName,'') Skill, ISNULL(omp.UserName,'') AS Process,
                //            ISNULL(omp.Id,'') as ProcessId, ISNULL(OMOPC.UserName,'') AS Category, ISNULL(OMOPC.Id,'') as CategoryId,
                //            ISNULL(om.[Type],'') AS [Type],ISNULL(om.OperationTypeId,'') as TypeId,
                //            ISNULL(omsg.UserName,'') AS SkillGroup , ISNULL(omsg.Id,'') as SkillGroupId 
                //            from org.Company CMP
                //            join org.Plant PLN on PLN.CompanyId = CMP.Id
                //            JOIN mst.OperationMaster AS om ON om.CompanyGroupId = cmp.CompanyGroupId
                //            JOIN hkp.Process AS omp ON omp.Id=om.ProcessId
                //            JOIN hkp.Skill AS omsk ON omsk.Id=om.SkillId
                //            JOIN HKP.OperationCategory AS OMOPC ON OMOPC.Id=om.OperationCategoryId 
                //            JOIN scs.SkillGrouping AS omsg ON omsg.Id=om.SkillGroupId
                //            join org.Entity E on E.PlantId = PLN.Id
                //            join  ShiftDefination sd on sd.PlantID = PLN.Id";
                var str = @"Select c.id as CompanyId, c.UserName as Company, p.id as PlantId, p.UserName as Plant, e.id as EntityId, e.UserName as Entity
                            , ept.ProcessId , op.UserName as Process from
                            hkp.EntityProcessTag ept 
                            join org.Entity e on e.Id = ept.EntityId
                            join org.Plant p on p.id = e.PlantId
                            join org.Company c on c.Id = p.CompanyId
                            join hkp.Process op on op.Id = ept.ProcessId
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        class Global
        {

            public static DataTable DistinctDates;
        }

        public List<Dictionary<string, object>> DateWiseSkillData(out List<string> DateColumns)
        {
            DataTable dtRequiredManpower = _sqlRepository.GetDataTable(@" Select isnull(req.CompanyId,'') as CompanyId , isnull(req.PlantId,'') as PlantId, isnull(req.EntityId,'') as EntityId ,
Actual.MachineId , Actual.UserName as MachineName,
isnull(req.ProductionDate,'') as ProductionDate, isnull(req.Alloted,0) as Alloted, Actual.Available , ((Actual.Available) -isnull(req.Alloted,0)) as ShortExcess
from
(Select   mm.UserName , mm.Id as MachineId,isnull(Sum(MB.ProductionMachineQty),0) as Available
								from MachineBudget MB
								Left join MSt.MaterialMasterArticle MMA ON MMA.Id=MB.ArticleId
								Left join MSt.MaterialMaster MM ON MM.Id=MMA.MaterialMasterId
								Left join ORG.Plant P ON P.Id=MB.PlantId
								Left join ORG.Entity E ON E.Id=MB.EntityId
								left join ORG.Company C on c.id = P.CompanyId
								Where MM.CompanyGroupId='CG20181'
								group by  mm.UserName , mm.Id) as Actual

	left join (
								SELECT  cmp.Id as CompanyId, po.EntityId , po.PlantId , isnull(mac.Machine,'') as Machine , isnull(mac.MachineID,'') as MachineId, format(p1.ProductionDate,'dd-MMM-yy') as ProductionDate , isnull(sum(bmd.AllotedWorkstation),0) as Alloted
                       from trn.ProductionOrder PO
                    inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                    INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and p1.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
                    LEFT OUTER JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=po.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BT.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.ProductionBulletinTemplateMasterId=BM.Id
					Left outer join org.Plant pln on pln.Id = po.PlantId
					left join ORG.Company cmp on cmp.Id = pln.CompanyId
					inner join (SELECT distinct D.MachineVarientId ,mma.StandardName AS Articles , mm.Id as MachineID , mm.UserName as Machine
							FROM trn.ProductionBulletinTemplateDetail AS D
							JOIN mst.MaterialMasterArticle AS mma ON mma.Id=D.MachineVarientId
							JOIN mst.MaterialMaster AS mm ON mm.Id=mma.MaterialMasterId) as mac on mac.MachineVarientId = bmd.MachineVarientId
				group by mac.Machine , mac.MachineID , p1.ProductionDate , po.EntityId , po.PlantId , cmp.Id
				) as req on req.MachineId = Actual.MachineId
 ");








            DataTable dtDistinctDates = dtRequiredManpower.DefaultView.ToTable(true, "ProductionDate");
            Global.DistinctDates = dtDistinctDates;
           List<DateTime> ltDistinctDates = new List<DateTime>();

            for (int i = 0; i < dtDistinctDates.Rows.Count; i++)
            {
                ltDistinctDates.Add(Convert.ToDateTime(dtDistinctDates.Rows[i]["ProductionDate"].ToString()));
            }

            ltDistinctDates = ltDistinctDates.OrderBy(k => k).ToList();
            DataTable MachineData = new DataTable("Machine");
            MachineData.Columns.Add("MachineId");
            MachineData.Columns.Add("Machine");
            MachineData.Columns.Add("Available");
            MachineData.Columns.Add("Flag");
            MachineData.Columns.Add("Index");

            DateColumns = new List<string>(); ;
            for (int i = 0; i < ltDistinctDates.Count; i++)
            {
                DateColumns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"));
                MachineData.Columns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"), typeof(double));
            }

            string MachineId = "";
            DataRow dr = null;
            int ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region ShortExcess
                if (dtRequiredManpower.Rows[i]["MaterialMasterId"].ToString() != MachineId)
                {
                    dr = MachineData.NewRow();
                    dr["Index"] = ind;
                    dr["MachineId"] = dtRequiredManpower.Rows[i]["MaterialMasterId"];
                    dr["Machine"] = dtRequiredManpower.Rows[i]["machine"];
                    dr["Available"] = dtRequiredManpower.Rows[i]["Total"];
                    dr["Flag"] = "ShortExcess";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = 0;

                    }

                    ind++;
                    MachineData.Rows.Add(dr);

                }

               
                    dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["Total"].ToString());

                
                #endregion
                MachineId = dtRequiredManpower.Rows[i]["MaterialMasterId"].ToString();
            }

            MachineId = "";
            dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region AvailableMP
                if (dtRequiredManpower.Rows[i]["MaterialMasterId"].ToString() != MachineId)
                {
                    dr = MachineData.NewRow();
                    dr["Index"] = ind;
                    dr["MachineId"] = dtRequiredManpower.Rows[i]["MaterialMasterId"];
                    dr["Machine"] = dtRequiredManpower.Rows[i]["machine"];
                    dr["Available"] = dtRequiredManpower.Rows[i]["Total"];
                    dr["Flag"] = "ActualMP";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = 0;

                    }

                    ind++;
                    MachineData.Rows.Add(dr);

                }


                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["Total"].ToString());

                
                #endregion

                MachineId = dtRequiredManpower.Rows[i]["MaterialMasterId"].ToString();
            }


            MachineId = "";
            dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region AllotedMP
                if (dtRequiredManpower.Rows[i]["MaterialMasterId"].ToString() != MachineId)
                {
                    dr = MachineData.NewRow();
                    dr["Index"] = ind;
                    dr["MachineId"] = dtRequiredManpower.Rows[i]["MaterialMasterId"];
                    dr["Machine"] = dtRequiredManpower.Rows[i]["machine"];
                    dr["Available"] = dtRequiredManpower.Rows[i]["Total"];
                    dr["Flag"] = "RequiredManPower";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = 0;

                    }

                    ind++;
                    MachineData.Rows.Add(dr);

                }


                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["Work"].ToString());

                
                #endregion
                MachineId = dtRequiredManpower.Rows[i]["MaterialMasterId"].ToString();
            }

            return Library.Service.Helpers.DataTableExtensions.DataTableToJson(MachineData);
        }


        public List<Dictionary<string, object>> FilterWiseMachineData(out List<string> DateColumns ,  Dictionary<string, string> parameters , string fromDate , string toDate , out List<Dictionary<string, object>> dt)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var str = @"Select isnull(req.CompanyId,'') as CompanyId , isnull(req.PlantId,'') as PlantId, isnull(req.EntityId,'') as EntityId ,
Actual.MachineId , Actual.UserName as MachineName,
isnull(req.ProductionDate,'') as ProductionDate, isnull(req.Allotted,0) as Allotted, Actual.Available , ((Actual.Available) -isnull(req.Allotted,0)) as ShortExcess
from
(Select   mm.UserName , mm.Id as MachineId,isnull(Sum(MB.ProductionMachineQty),0) as Available
								from MachineBudget MB
								Left join MSt.MaterialMasterArticle MMA ON MMA.Id=MB.ArticleId
								Left join MSt.MaterialMaster MM ON MM.Id=MMA.MaterialMasterId
								Left join ORG.Plant P ON P.Id=MB.PlantId
								Left join ORG.Entity E ON E.Id=MB.EntityId
								left join ORG.Company C on c.id = P.CompanyId
								Where MM.CompanyGroupId='"+identity.CompanyGroupId+@"' AND
                                isnull(C.Id,'') IN(" + parameters["CompanyId"] + @") AND
                                isnull(P.Id,'') IN("+parameters["PlantId"]+ @") AND
                                isnull(E.Id,'') IN(" + parameters["EntityId"] + @") 
								group by  mm.UserName , mm.Id) as Actual

	left join (
								SELECT  cmp.Id as CompanyId, po.EntityId , po.PlantId , isnull(mac.Machine,'') as Machine , isnull(mac.MachineID,'') as MachineId, format(p1.ProductionDate,'dd-MMM-yy') as ProductionDate , isnull(sum(bmd.AllotedWorkstation),0) as Allotted
                       from trn.ProductionOrder PO
                    inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                    INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID 
                    AND p1.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
                    AND p1.ID=(SELECT TOP 1 TT.ID FROM ProductionPlanningType1 TT WHERE tt.WorkCenterMasterId=p1.WorkCenterMasterId AND tt.ProductionDate=p1.ProductionDate)
                    LEFT OUTER JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=po.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BT.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.ProductionBulletinTemplateMasterId=BM.Id
					Left outer join org.Plant pln on pln.Id = po.PlantId
					left join ORG.Company cmp on cmp.Id = pln.CompanyId
					inner join (SELECT distinct D.MachineVarientId ,mma.StandardName AS Articles , mm.Id as MachineID , mm.UserName as Machine
							FROM trn.ProductionBulletinTemplateDetail AS D
							JOIN mst.MaterialMasterArticle AS mma ON mma.Id=D.MachineVarientId
							JOIN mst.MaterialMaster AS mm ON mm.Id=mma.MaterialMasterId) as mac on mac.MachineVarientId = bmd.MachineVarientId
                    where isnull(cmp.Id,'') IN(" + parameters["CompanyId"] + @") AND
                                isnull(pln.Id,'') IN(" + parameters["PlantId"] + @") AND
                                isnull(po.EntityId,'') IN(" + parameters["EntityId"] + @") AND
                                isnull(p1.ProcessID , '' ) IN("+parameters["ProcessId"] +@") 
				group by mac.Machine , mac.MachineID , p1.ProductionDate , po.EntityId , po.PlantId , cmp.Id
				) as req on req.MachineId = Actual.MachineId
                     
                     
                      WHERE 
                          CAST(ProductionDate as DATE) between '" + fromDate + @"' and  '" + toDate + @"'

 

            ORDER BY  Actual.MachineId";

            DataTable dtRequiredManpower = _sqlRepository.GetDataTable(str);




            DataTable dtDistinctDates = dtRequiredManpower.DefaultView.ToTable(true, "ProductionDate");
            List<DateTime> ltDistinctDates = new List<DateTime>();

            for (int i = 0; i < dtDistinctDates.Rows.Count; i++)
            {
                ltDistinctDates.Add(Convert.ToDateTime(dtDistinctDates.Rows[i]["ProductionDate"].ToString()));
            }

            ltDistinctDates = ltDistinctDates.OrderBy(k => k).ToList();
            DataTable MachineData = new DataTable("Machine");
            MachineData.Columns.Add("MachineId");
            MachineData.Columns.Add("Machine");
            MachineData.Columns.Add("Available");
            MachineData.Columns.Add("Flag");
            MachineData.Columns.Add("Index");

            DateColumns = new List<string>(); ;
            for (int i = 0; i < ltDistinctDates.Count; i++)
            {
                DateColumns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"));
                MachineData.Columns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"), typeof(double));
            }

            string MachineId = "";
            DataRow dr = null;
            int ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region ShortExcess
                if (dtRequiredManpower.Rows[i]["MachineId"].ToString() != MachineId)
                {
                    dr = MachineData.NewRow();
                    dr["Index"] = ind;
                    dr["MachineId"] = dtRequiredManpower.Rows[i]["MachineId"];
                    dr["Machine"] = dtRequiredManpower.Rows[i]["MachineName"];
                    dr["Available"] = dtRequiredManpower.Rows[i]["Available"];
                    string ava = dtRequiredManpower.Rows[i]["Available"].ToString();
                    dr["Flag"] = "ShortExcess";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = ava;

                    }

                    ind++;
                    MachineData.Rows.Add(dr);
                   
                }

                    dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["ShortExcess"].ToString());

                
                #endregion
                MachineId = dtRequiredManpower.Rows[i]["MachineId"].ToString();
            }

             MachineId = "";
             dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region Available
                if (dtRequiredManpower.Rows[i]["MachineId"].ToString() != MachineId)
                {
                    dr = MachineData.NewRow();
                    dr["Index"] = ind;
                    dr["MachineId"] = dtRequiredManpower.Rows[i]["MachineId"];
                    dr["Machine"] = dtRequiredManpower.Rows[i]["MachineName"];
                    dr["Available"] = dtRequiredManpower.Rows[i]["Available"];
                    string a = dtRequiredManpower.Rows[i]["Available"].ToString();
                    dr["Flag"] = "Available";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = a;

                    }

                    ind++;
                    MachineData.Rows.Add(dr);

                }

                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["Available"].ToString());


                #endregion
                MachineId = dtRequiredManpower.Rows[i]["MachineId"].ToString();
            }


             MachineId = "";
             dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region Allotted
                if (dtRequiredManpower.Rows[i]["MachineId"].ToString() != MachineId)
                {
                    dr = MachineData.NewRow();
                    dr["Index"] = ind;
                    dr["MachineId"] = dtRequiredManpower.Rows[i]["MachineId"];
                    dr["Machine"] = dtRequiredManpower.Rows[i]["MachineName"];
                    dr["Available"] = dtRequiredManpower.Rows[i]["Available"];
                    dr["Flag"] = "Allotted";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = 0;

                    }

                    ind++;
                    MachineData.Rows.Add(dr);

                }

                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["Allotted"].ToString());


                #endregion
                MachineId = dtRequiredManpower.Rows[i]["MachineId"].ToString();
            }
            DataTable ds = CompactTable(MachineData, DateColumns);
            dt = Library.Service.Helpers.DataTableExtensions.DataTableToJson(ds);

            return Library.Service.Helpers.DataTableExtensions.DataTableToJson(MachineData);
        }

        private DataTable CompactTable(DataTable dtData, List<string> Columns)
        {


            try
            {
                DataTable dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Available" }).Select(x =>
                {

                    DataRow row = dtData.NewRow();
                    row["Flag"] = "Total Available";
                    for (int i = 0; i < Columns.Count; i++)
                    {

                        row[Columns[i]] = x.Sum(r => r["Flag"].ToString() == "Available" ? Convert.ToDouble(r[Columns[i]]) : 0);
                    }
                    return row;

                }).CopyToDataTable();

                DataTable dtTemp = dtCompact.DefaultView.ToTable();

                dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Requirement" }).Select(x =>
                {

                    DataRow row = dtData.NewRow();
                    row["Flag"] = "Total Requirement";
                    for (int i = 0; i < Columns.Count; i++)
                    {

                        row[Columns[i]] = x.Sum(r => r["Flag"].ToString() == "Allotted" ? OTSBD.clsStaticInfo.dbl(Convert.ToDouble(r[Columns[i]])) : 0);
                    }
                    return row;

                }).CopyToDataTable();

                dtTemp.Merge(dtCompact.DefaultView.ToTable());

                dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Excess" }).Select(x =>
                {

                    DataRow row = dtData.NewRow();
                    row["Flag"] = "Total Excess";
                    for (int i = 0; i < Columns.Count; i++)
                    {

                        row[Columns[i]] = x.Sum(r => OTSBD.clsStaticInfo.dbl(r[Columns[i]]) > 0 && r["Flag"].ToString() == "ShortExcess" ? Convert.ToDouble(r[Columns[i]]) : 0);
                    }
                    return row;

                }).CopyToDataTable();

                dtTemp.Merge(dtCompact.DefaultView.ToTable());

                dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Short" }).Select(x =>
                {

                    DataRow row = dtData.NewRow();
                    row["Flag"] = "Total Short";
                    for (int i = 0; i < Columns.Count; i++)
                    {

                        row[Columns[i]] = x.Sum(r => OTSBD.clsStaticInfo.dbl(r[Columns[i]]) < 0 && r["Flag"].ToString() == "ShortExcess" ? Convert.ToDouble(r[Columns[i]]) : 0);
                    }
                    return row;

                }).CopyToDataTable();

                dtTemp.Merge(dtCompact.DefaultView.ToTable());


                return dtTemp;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public List<Dictionary<string, object>> FilterWiseArticleData(out List<string> DateColumns, Dictionary<string, string> parameters, string fromDate, string toDate, out List<Dictionary<string, object>> dt)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var str = @"Select isnull(req.CompanyId,'') as CompanyId , isnull(req.PlantId,'') as PlantId, isnull(req.EntityId,'') as EntityId ,
req.MachineId , req.Machine as MachineName, req.MachineVarientId as ArticleId , req.Articles,
isnull(req.ProductionDate,'') as ProductionDate, isnull(req.Allotted,0) as Allotted, isnull(Actual.Available,0) as Available , (isnull(Actual.Available,0) -isnull(req.Allotted,0)) as ShortExcess
from
(SELECT  cmp.Id as CompanyId, po.EntityId , po.PlantId , isnull(mac.Machine,'') as Machine , isnull(mac.MachineID,'') as MachineId, bmd.MachineVarientId, mac.Articles,
format(p1.ProductionDate,'dd-MMM-yy') as ProductionDate , isnull(sum(bmd.AllotedWorkstation),0) as Allotted
                       from trn.ProductionOrder PO
                    inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                    INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID 
                    AND p1.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
                    AND p1.ID=(SELECT TOP 1 TT.ID FROM ProductionPlanningType1 TT WHERE tt.WorkCenterMasterId=p1.WorkCenterMasterId AND tt.ProductionDate=p1.ProductionDate)
                    LEFT OUTER JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=po.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BT.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.ProductionBulletinTemplateMasterId=BM.Id
					Left outer join org.Plant pln on pln.Id = po.PlantId
					left join ORG.Company cmp on cmp.Id = pln.CompanyId
					inner join (SELECT distinct D.MachineVarientId ,mma.StandardName AS Articles , mm.Id as MachineID , mm.UserName as Machine
							FROM trn.ProductionBulletinTemplateDetail AS D
							JOIN mst.MaterialMasterArticle AS mma ON mma.Id=D.MachineVarientId
							JOIN mst.MaterialMaster AS mm ON mm.Id=mma.MaterialMasterId) as mac on mac.MachineVarientId = bmd.MachineVarientId
                    where isnull(cmp.Id,'') IN(" + parameters["CompanyId"] + @") AND
                                isnull(pln.Id,'') IN(" + parameters["PlantId"] + @") AND
                                isnull(po.EntityId,'') IN(" + parameters["EntityId"] + @") AND
                                isnull(p1.ProcessID , '' ) IN(" + parameters["ProcessId"] + @") 
				group by mac.Machine , mac.MachineID , p1.ProductionDate , po.EntityId , po.PlantId , cmp.Id , bmd.MachineVarientId, mac.Articles 
								) as req

	left join (Select   mm.UserName , mm.Id as MachineId, mma.Id , mma.StandardName as ArticleName,isnull(Sum(MB.ProductionMachineQty),0) as Available
								from MachineBudget MB
								Left join MSt.MaterialMasterArticle MMA ON MMA.Id=MB.ArticleId
								Left join MSt.MaterialMaster MM ON MM.Id=MMA.MaterialMasterId
								Left join ORG.Plant P ON P.Id=MB.PlantId
								Left join ORG.Entity E ON E.Id=MB.EntityId
								left join ORG.Company C on c.id = P.CompanyId
								Where MM.CompanyGroupId='"+ identity.CompanyGroupId + @"' AND
                               isnull(C.Id,'') IN(" + parameters["CompanyId"] + @") AND
                                isnull(P.Id,'') IN(" + parameters["PlantId"] + @") AND
                                isnull(E.Id,'') IN(" + parameters["EntityId"] + @") 
								group by  mm.UserName , mm.Id , mma.Id , mma.StandardName
						
				) as Actual on req.MachineVarientId = Actual.Id
                     
                     
                      WHERE 
                          CAST(ProductionDate as DATE) between '"+fromDate+@"' and  '"+toDate+@"'
						  

 

            ORDER BY req.MachineVarientId";

            DataTable dtRequiredManpower = _sqlRepository.GetDataTable(str);




            DataTable dtDistinctDates = dtRequiredManpower.DefaultView.ToTable(true, "ProductionDate");
            List<DateTime> ltDistinctDates = new List<DateTime>();

            for (int i = 0; i < dtDistinctDates.Rows.Count; i++)
            {
                ltDistinctDates.Add(Convert.ToDateTime(dtDistinctDates.Rows[i]["ProductionDate"].ToString()));
            }

            ltDistinctDates = ltDistinctDates.OrderBy(k => k).ToList();
            DataTable MachineData = new DataTable("Machine");
            MachineData.Columns.Add("ArticleId");
            MachineData.Columns.Add("MachineName");
            MachineData.Columns.Add("Articles");
            MachineData.Columns.Add("Available");
            MachineData.Columns.Add("Flag");
            MachineData.Columns.Add("Index");

            DateColumns = new List<string>(); ;
            for (int i = 0; i < ltDistinctDates.Count; i++)
            {
                DateColumns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"));
                MachineData.Columns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"), typeof(double));
            }

            string ArticleId = "";
            DataRow dr = null;
            int ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region ShortExcess
                if (dtRequiredManpower.Rows[i]["ArticleId"].ToString() != ArticleId)
                {
                    dr = MachineData.NewRow();
                    dr["Index"] = ind;
                    dr["ArticleId"] = dtRequiredManpower.Rows[i]["ArticleId"];
                    dr["MachineName"] = dtRequiredManpower.Rows[i]["MachineName"];
                    dr["Articles"] = dtRequiredManpower.Rows[i]["Articles"];
                    dr["Available"] = dtRequiredManpower.Rows[i]["Available"];
                    string ava = dtRequiredManpower.Rows[i]["Available"].ToString();
                    dr["Flag"] = "ShortExcess";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = ava;

                    }

                    ind++;
                    MachineData.Rows.Add(dr);

                }

                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["ShortExcess"].ToString());


                #endregion
                ArticleId = dtRequiredManpower.Rows[i]["ArticleId"].ToString();
            }

            ArticleId = "";
            dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region Available
                if (dtRequiredManpower.Rows[i]["ArticleId"].ToString() != ArticleId)
                {
                    dr = MachineData.NewRow();
                    dr["Index"] = ind;
                    dr["ArticleId"] = dtRequiredManpower.Rows[i]["ArticleId"];
                    dr["MachineName"] = dtRequiredManpower.Rows[i]["MachineName"];
                    dr["Articles"] = dtRequiredManpower.Rows[i]["Articles"];
                    dr["Available"] = dtRequiredManpower.Rows[i]["Available"];
                    string a = dtRequiredManpower.Rows[i]["Available"].ToString();
                    dr["Flag"] = "Available";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = a;

                    }

                    ind++;
                    MachineData.Rows.Add(dr);

                }

                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["Available"].ToString());


                #endregion
                ArticleId = dtRequiredManpower.Rows[i]["ArticleId"].ToString();
            }


            ArticleId = "";
            dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region Allotted
                if (dtRequiredManpower.Rows[i]["ArticleId"].ToString() != ArticleId)
                {
                    dr = MachineData.NewRow();
                    dr["Index"] = ind;
                    dr["ArticleId"] = dtRequiredManpower.Rows[i]["ArticleId"];
                    dr["MachineName"] = dtRequiredManpower.Rows[i]["MachineName"];
                    dr["Articles"] = dtRequiredManpower.Rows[i]["Articles"];
                    dr["Available"] = dtRequiredManpower.Rows[i]["Available"];
                    dr["Flag"] = "Allotted";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = 0;

                    }

                    ind++;
                    MachineData.Rows.Add(dr);

                }

                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["Allotted"].ToString());


                #endregion
                ArticleId = dtRequiredManpower.Rows[i]["ArticleId"].ToString();
            }
            DataTable ds = CompactTableArticle(MachineData, DateColumns);
            dt = Library.Service.Helpers.DataTableExtensions.DataTableToJson(ds);

            return Library.Service.Helpers.DataTableExtensions.DataTableToJson(MachineData);
        }


        private DataTable CompactTableArticle(DataTable dtData, List<string> Columns)
        {

            try
            {

                DataTable dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Available" }).Select(x =>
                {

                    DataRow row = dtData.NewRow();
                    row["Flag"] = "Total Available";
                    for (int i = 0; i < Columns.Count; i++)
                    {

                        row[Columns[i]] = x.Sum(r => r["Flag"].ToString() == "Available" ? Convert.ToDouble(r[Columns[i]]) : 0);
                    }
                    return row;

                }).CopyToDataTable();

                DataTable dtTemp = dtCompact.DefaultView.ToTable();

                dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Requirement" }).Select(x =>
                {

                    DataRow row = dtData.NewRow();
                    row["Flag"] = "Total Requirement";
                    for (int i = 0; i < Columns.Count; i++)
                    {

                        row[Columns[i]] = x.Sum(r => r["Flag"].ToString() == "Allotted" ? OTSBD.clsStaticInfo.dbl(Convert.ToDouble(r[Columns[i]])) : 0);
                    }
                    return row;

                }).CopyToDataTable();

                dtTemp.Merge(dtCompact.DefaultView.ToTable());

                dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Excess" }).Select(x =>
                {

                    DataRow row = dtData.NewRow();
                    row["Flag"] = "Total Excess";
                    for (int i = 0; i < Columns.Count; i++)
                    {

                        row[Columns[i]] = x.Sum(r => OTSBD.clsStaticInfo.dbl(r[Columns[i]]) > 0 && r["Flag"].ToString() == "ShortExcess" ? Convert.ToDouble(r[Columns[i]]) : 0);
                    }
                    return row;

                }).CopyToDataTable();

                dtTemp.Merge(dtCompact.DefaultView.ToTable());

                dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Short" }).Select(x =>
                {

                    DataRow row = dtData.NewRow();
                    row["Flag"] = "Total Short";
                    for (int i = 0; i < Columns.Count; i++)
                    {

                        row[Columns[i]] = x.Sum(r => OTSBD.clsStaticInfo.dbl(r[Columns[i]]) < 0 && r["Flag"].ToString() == "ShortExcess" ? Convert.ToDouble(r[Columns[i]]) : 0);
                    }
                    return row;

                }).CopyToDataTable();

                dtTemp.Merge(dtCompact.DefaultView.ToTable());


                return dtTemp;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //**************
        ///The OnClick Data From Dates API ///
        //**************
        public IEnumerable<object> allotedWorkCenter(Dictionary<string, string> parameters, string machineId , string date)
        {
            try
            {
                var str = @"SELECT   mac.Machine as Machine , wc.UserName as WorkCenter ,mac.MachineID as MachineId, format(p1.ProductionDate,'dd-MMM-yy') as ProductionDate , isnull(sum(bmd.AllotedWorkstation),0) as Allotted
                            ,buyer=STUFF((select distinct ','+XB.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                            where po.Id=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            Article=STUFF((select distinct ','+Xmm.StandardName from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            inner join mst.materialMasterArticle Xmm on Xmm.id = XMOI.ArticleId
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=po.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
							po.Id as PRNO
                       from trn.ProductionOrder PO
                    inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                    INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and p1.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
                    LEFT OUTER JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=po.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BT.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.ProductionBulletinTemplateMasterId=BM.Id
					Left outer join org.Plant pln on pln.Id = po.PlantId
					left join ORG.Company cmp on cmp.Id = pln.CompanyId
					left join scs.WorkCenterMaster wc on wc.Id = p1.WorkCenterMasterId
					inner join (SELECT distinct D.MachineVarientId ,mma.StandardName AS Articles , mm.Id as MachineID , mm.UserName as Machine
							FROM trn.ProductionBulletinTemplateDetail AS D
							JOIN mst.MaterialMasterArticle AS mma ON mma.Id=D.MachineVarientId
							JOIN mst.MaterialMaster AS mm ON mm.Id=mma.MaterialMasterId) as mac on mac.MachineVarientId = bmd.MachineVarientId
                    where isnull(cmp.Id,'') IN(" + parameters["CompanyId"]+ @") AND
                                isnull(pln.Id,'') IN(" + parameters["PlantId"] + @") AND
                                isnull(po.EntityId,'') IN("+parameters["EntityId"]+ @") and
                                isnull(p1.ProcessID , '' ) IN(" + parameters["ProcessId"] + @") 
								and p1.ProductionDate = '" + date+@"'
								And mac.MachineID = '"+machineId+@"'
				group by mac.Machine , mac.MachineID , wc.UserName , p1.ProductionDate  , po.Id
                ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        public IEnumerable<object> allotedArticleWorkCenter(Dictionary<string, string> parameters, string machineVId, string date)
        {
            try
            {
                var str = @"SELECT   bmd.MachineVarientId, mac.Articles, wc.UserName as WorkCenter,
                            format(p1.ProductionDate,'dd-MMM-yy') as ProductionDate , sum(bmd.AllotedWorkstation) as Allotted,
                            buyer=STUFF((select distinct ','+XB.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                            where po.Id=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            Article=STUFF((select distinct ','+Xmm.StandardName from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            inner join mst.materialMasterArticle Xmm on Xmm.id = XMOI.ArticleId
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=po.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
							po.Id as PRNO
                       from trn.ProductionOrder PO
                    inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                    INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and p1.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
                    LEFT OUTER JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=po.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BT.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.ProductionBulletinTemplateMasterId=BM.Id
					Left outer join org.Plant pln on pln.Id = po.PlantId
					left join ORG.Company cmp on cmp.Id = pln.CompanyId
					left join scs.WorkCenterMaster wc on wc.Id = p1.WorkCenterMasterId
					inner join (SELECT distinct D.MachineVarientId ,mma.StandardName AS Articles , mm.Id as MachineID , mm.UserName as Machine
							FROM trn.ProductionBulletinTemplateDetail AS D
							JOIN mst.MaterialMasterArticle AS mma ON mma.Id=D.MachineVarientId
							JOIN mst.MaterialMaster AS mm ON mm.Id=mma.MaterialMasterId) as mac on mac.MachineVarientId = bmd.MachineVarientId
                    where isnull(cmp.Id,'') IN(" + parameters["CompanyId"]+@") AND      
                                isnull(pln.Id,'') IN("+parameters["PlantId"]+@") AND          
                                isnull(po.EntityId,'') IN("+parameters["EntityId"]+ @")  and     
                                isnull(p1.ProcessID , '' ) IN(" + parameters["ProcessId"] + @") and
								bmd.MachineVarientId = '" + machineVId+@"' and p1.ProductionDate = '"+date+@"'        
								group by bmd.MachineVarientId, mac.Articles, wc.UserName , p1.ProductionDate ,po.Id    
                ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
