using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Dashboard
{
    public class SkillMapping
    {
        private readonly SqlRepository _sqlRepository;
        public SkillMapping()
        {
            _sqlRepository = new SqlRepository();

        }


        public IEnumerable<object> leftGridData()
        {
            try
            {
                var str = @"SELECT mo.PlantId,c.id AS CompanyId,p.UserName AS PlantName,c.UserName AS CompanyName,COUNT(so.Id) AS NoOfSO,SUM(so.Qty) AS SOQty,
sum(CASE WHEN isnull(pod.Id,'')='' THEN 1 ELSE 0 END)  AS PendingSOForPR,
 sum(CASE WHEN isnull(pbt.Id,'')='' THEN 1 ELSE 0 END)  AS BulletinToAttach,
sum(CASE WHEN so.DeliveryDate<= DATEADD(DAY,45,GETDATE()) AND isnull(pbt.Id,'')='' THEN 1 ELSE 0 END) BulletinToAttachW45Days
from trn.MasterOrder as mo
left outer join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
left outer join trn.SalesOrder so on moi.Id = so.MasterOrderItemId
left outer join org.Plant p on p.id = mo.PlantId
left outer join org.Company c on c.Id = p.CompanyId
left join trn.ProductionOrderDetail as pod on pod.SalesOrderId = so.Id
left join trn.ProductionOrder as po on po.Id = pod.ProductionOrderId
left join trn.ProductionBulletinTemplate as pbt on pbt.ProductionOrderId = po.Id
group by mo.PlantId , c.Id , p.UserName , c.UserName";
                return _sqlRepository.GetDataCollection(str);

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> allFilterLists(string PlantId)
        {
            try
            {
                var str = @"
                            Select distinct c.Id as CompanyId,c.UserName as Company,e.Id as EntityId, e.UserName as Entity,
                            p.Id as PlantId,p.UserName as Plant,s.ShiftDefinationDescription as [Shift],s.SystemID as ShiftId,
                            ISNULL (om.SkillId,'') as SkillId,ISNULL( om.UserName,'') as Skill, ISNULL(omp.UserName,'') AS Process,
                            ISNULL(omp.Id,'') as ProcessId, ISNULL(skc.UserName,'') AS Category, ISNULL(skc.Id,'') as CategoryId,
                            ISNULL(om.[Type],'') AS [Type],ISNULL(om.OperationTypeId,'') as TypeId,
                            ISNULL(omsg.UserName,'') AS SkillGroup , ISNULL(omsg.Id,'') as SkillGroupId 
                            from mst.OperationMaster AS om 
                            JOIN hkp.Process AS omp ON omp.Id=om.ProcessId
                            JOIN hkp.Skill AS omsk ON omsk.Id=om.SkillId
							JOIN hkp.SkillCategory as skc on skc.Id = omsk.SkillCategoryId
                            JOIN scs.SkillGrouping AS omsg ON omsg.Id=om.SkillGroupId
							join hkp.EntityProcessTag ep on ep.ProcessId = omp.Id
							join org.Entity e on e.Id = ep.EntityId
							join org.Plant p on p.Id = e.PlantId
							left join(select distinct sd.SystemID,sd.ShiftDefinationDescription,WC.EntityId from SCS.WorkCenterMaster WC
							left join [dbo].[WorkCenterWiseShift] WS ON WS.WorkCenterMasterId=WC.Id 
							join ShiftDefination sd on sd.SystemID=WS.ShiftDefinationID
							) S ON S.EntityId=E.Id
							join org.Company c on c.Id = p.CompanyId
                            Where p.Id='" + PlantId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<Dictionary<string, object>> DateWiseSkillData(out List<string> DateColumns)
        {
            DataTable dtRequiredManpower = _sqlRepository.GetDataTable(@" select  isnull(emp.Skill1,0) as Skill1, isnull(emp.Skill2,0) as Skill2, isnull(emp.Skill3,0) as Skill3
,DT.*,isnull(emp.CompanyId,'') as CompanyId,convert(DECIMAL(18,2),isnull(emp.Skill1,0)-ISNULL(dt.AllotedManpower,0)) AS ShortExcess
  from (SELECT om.Id AS OperationMasterId,omsk.Id AS SkillId, om.SkillGroupId , om.OperationTypeId , om.Code as SkillCode,omsk.SkillCategoryId,skc.UserName as SkillCat,
                FORMAT(p1.ProductionDate,'dd-MMM-yy') AS ProductionDate, om.UserName AS Skill ,omsk.UserName AS UserName,skc.[Sequence],
                 convert(DECIMAL(18,2),SUM(bmd.RequiredManPower)) AS RequiredManPower ,SUM(bmd.AllotedManpower) as AllotedManpower,  PO.PlantId , PO.EntityId ,omp.Id as ProcessId, 
				om.OperationCategoryId as CategoryId 
                       from trn.ProductionOrder PO
                    inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                    INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and p1.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
                    LEFT OUTER JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=po.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BT.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.ProductionBulletinTemplateMasterId=BM.Id
        
        
                    JOIN mst.OperationMaster AS om ON om.Id=bmd.SkillMasterId       
                    LEFT OUTER JOIN hkp.Process AS omp ON omp.Id=om.ProcessId
                    LEFT OUTER JOIN hkp.Skill AS omsk ON omsk.Id=om.SkillId
					LEFT OUTER JOIN hkp.SkillCategory AS skc ON skc.Id = omsk.SkillCategoryId
                    LEFT OUTER JOIN HKP.OperationCategory AS OMOPC ON OMOPC.Id=om.OperationCategoryId 
                    LEFT OUTER JOIN scs.SkillGrouping AS omsg ON omsg.Id=om.SkillGroupId       


					left outer join (Select EOP.Code
											
											from EmployeeInformation E
											LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
											LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
											LEFT JOIN EmployeeOperation EO ON EO.EmpSystemId=E.SystemId
											LEFT JOIN MST.OperationMaster EOP ON EOP.Id=EO.OperationMasterId
											LEFT JOIN (
											SELECT ES.EmpSystemID, S.ShiftDefinationDescription
											FROM EmpDateWiseShiftAssign ES
											LEFT JOIN ShiftDefination S ON S.SystemID = ES.ShiftSystemID
											WHERE ES.WorkDate =FORMAT(GETDATE(), 'dd-MMM-yyyy') 
                                            AND isnull(S.ShiftDefinationDescription,'') IN()
											) AD ON AD.EmpSystemID=E.SystemId
											where E.EmployeeStatus='Active' AND E.EmpType<>'Guest' 
											group by EOP.Code,C.id 
											) as shi on shi.Code = om.Code
                    GROUP BY om.Id,omsk.Id, p1.ProductionDate,om.UserName,omsk.UserName , omsk.SkillCategoryId , PO.PlantId , PO.EntityId,omp.Id,om.OperationCategoryId , om.SkillGroupId ,  om.OperationTypeId , om.Code , skc.UserName , skc.[Sequence]
					
                     ) AS DT
                     
                    

					 LEFT JOIN (Select emp.Id as CompanyId,emp.Code,
								sum(emp.Skill1) as Skill1 , sum(emp.Skill2) as Skill2 , sum(emp.Skill3) as Skill3 
								from
								(Select C.Id,EOP.Code,  sum( case when EO.Sequence='1.00' then 1 else 0 end)  as Skill1 , 
											sum( case when EO.Sequence='2.00' then 1 else 0 end) as Skill2 , 
											sum( case when EO.Sequence='3.00' then 1 else 0 end) as Skill3
											from EmployeeInformation E
											LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
											LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
											LEFT JOIN EmployeeOperation EO ON EO.EmpSystemId=E.SystemId
											LEFT JOIN MST.OperationMaster EOP ON EOP.Id=EO.OperationMasterId
											 JOIN (
											SELECT ES.EmpSystemID, S.ShiftDefinationDescription
											FROM EmpDateWiseShiftAssign ES
											LEFT JOIN ShiftDefination S ON S.SystemID = ES.ShiftSystemID
											WHERE ES.WorkDate =FORMAT(GETDATE(), 'dd-MMM-yyyy')
											 AND isnull(S.ShiftDefinationDescription,'') IN()
											) AD ON AD.EmpSystemID=E.SystemId
											where E.EmployeeStatus='Active' AND E.EmpType<>'Guest'
											group by EOP.Code,C.id  ) as emp
											group by emp.Id , emp.Code 
											) as emp on emp.Code = DT.SkillCode
                      WHERE 
                         1=1
            ORDER BY  DT.[Sequence]
 ");








            DataTable dtDistinctDates = dtRequiredManpower.DefaultView.ToTable(true, "ProductionDate");
            List<DateTime> ltDistinctDates = new List<DateTime>();

            for (int i = 0; i < dtDistinctDates.Rows.Count; i++)
            {
                ltDistinctDates.Add(Convert.ToDateTime(dtDistinctDates.Rows[i]["ProductionDate"].ToString()));
            }

            ltDistinctDates = ltDistinctDates.OrderBy(k => k).ToList();
            DataTable SkillData = new DataTable("SKILL");
            SkillData.Columns.Add("SkillId");
            SkillData.Columns.Add("Skill");
            SkillData.Columns.Add("SkillCategory");
            SkillData.Columns.Add("SkillCode");
            SkillData.Columns.Add("Skill1");
            SkillData.Columns.Add("Skill2");
            SkillData.Columns.Add("Skill3");
            SkillData.Columns.Add("Flag");
            SkillData.Columns.Add("Index");

            DateColumns = new List<string>(); ;
            for (int i = 0; i < ltDistinctDates.Count; i++)
            {
                DateColumns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"));
                SkillData.Columns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"), typeof(double));
            }

            string SkillId = "";
            DataRow dr = null;
            int ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region ShortExcess
                if (dtRequiredManpower.Rows[i]["SkillCode"].ToString() != SkillId)
                {
                    dr = SkillData.NewRow();
                    dr["Index"] = ind;
                    dr["SkillId"] = dtRequiredManpower.Rows[i]["SkillId"];
                    dr["Skill"] = dtRequiredManpower.Rows[i]["Skill"];
                    dr["SkillCategory"] = dtRequiredManpower.Rows[i]["SkillCat"];
                    dr["SkillCode"] = dtRequiredManpower.Rows[i]["SkillCode"];
                    dr["Skill1"] = dtRequiredManpower.Rows[i]["Skill1"];
                    dr["Skill2"] = dtRequiredManpower.Rows[i]["Skill2"];
                    dr["Skill3"] = dtRequiredManpower.Rows[i]["Skill3"];
                    dr["Flag"] = "ShortExcess";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = 0;

                    }

                    ind++;
                    SkillData.Rows.Add(dr);

                }


                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["ShortExcess"].ToString());


                #endregion
                SkillId = dtRequiredManpower.Rows[i]["SkillCode"].ToString();
            }

            SkillId = "";
            dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region AvailableMP
                if (dtRequiredManpower.Rows[i]["SkillCode"].ToString() != SkillId)
                {
                    dr = SkillData.NewRow();
                    dr["Index"] = ind;
                    dr["SkillId"] = dtRequiredManpower.Rows[i]["SkillId"];
                    dr["Skill"] = dtRequiredManpower.Rows[i]["Skill"];
                    dr["SkillCategory"] = dtRequiredManpower.Rows[i]["SkillCat"];
                    dr["SkillCode"] = dtRequiredManpower.Rows[i]["SkillCode"];
                    dr["Flag"] = "ActualMP";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = 0;

                    }
                    ind++;
                    SkillData.Rows.Add(dr);
                }


                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["Skill1"].ToString());


                #endregion

                SkillId = dtRequiredManpower.Rows[i]["SkillCode"].ToString();
            }


            SkillId = "";
            dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                #region AllotedMP
                if (dtRequiredManpower.Rows[i]["SkillCode"].ToString() != SkillId)
                {
                    dr = SkillData.NewRow();
                    dr["Index"] = ind;
                    dr["SkillId"] = dtRequiredManpower.Rows[i]["SkillId"];
                    dr["Skill"] = dtRequiredManpower.Rows[i]["Skill"];
                    dr["SkillCategory"] = dtRequiredManpower.Rows[i]["SkillCat"];
                    dr["SkillCode"] = dtRequiredManpower.Rows[i]["SkillCode"];
                    dr["Flag"] = "RequiredManPower";
                    for (int j = 0; j < ltDistinctDates.Count; j++)
                    {

                        dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = 0;

                    }

                    ind++;
                    SkillData.Rows.Add(dr);
                }


                dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["AllotedManPower"].ToString());


                #endregion
                SkillId = dtRequiredManpower.Rows[i]["SkillCode"].ToString();
            }

            return Library.Service.Helpers.DataTableExtensions.DataTableToJson(SkillData);
        }


        public List<Dictionary<string, object>> FilterWiseSkillData(out List<string> DateColumns, Dictionary<string, string> parameters, string fromDate, string toDate, out List<Dictionary<string, object>> dt)
        {

            var str = @"select  isnull(emp.Skill1,0) as Skill1, isnull(emp.Skill2,0) as Skill2, isnull(emp.Skill3,0) as Skill3,skc.[Sequence],omp.Id as ProcessId, 
om.Id AS OperationMasterId,om.SkillId, om.SkillGroupId , om.OperationTypeId , isnull(om.Code,'') as SkillCode,om.UserName AS Skill,sg.UserName SkillGroup ,om.OperationCategoryId as CategoryId,
skc.UserName as SkillCat,omsk.SkillCategoryId,omsk.UserName AS UserName,
isnull(DT.ProductionDate,'" + fromDate + @"') as ProductionDate , isnull(dt.SkillIdTemp,'') as SkillIdTemp, isnull(sum(dt.RequiredManPower),0) as RequiredManPower,
isnull(sum(dt.AllotedManpower),0) as AllotedManpower , 
isnull(emp.CompanyId,'') as CompanyId,convert(DECIMAL(18,2),ISNULL(sum(dt.AllotedManpower),0))-isnull(sum(dt.RequiredManPower),0) AS ShortExcess
FROM
 hkp.Skill AS omsk 
LEFT OUTER JOIN hkp.SkillCategory AS skc ON skc.Id = omsk.SkillCategoryId
LEFT OUTER JOIN SCS.SkillGrouping AS sg ON sg.Id = omsk.SkillGroupId
left JOIN mst.OperationMaster AS om ON om.SkillId=omsk.Id       
LEFT OUTER JOIN hkp.Process AS omp ON omp.Id=om.ProcessId

LEFT JOIN (SELECT 
                isnull(FORMAT(p1.ProductionDate,'dd-MMM-yy'),'Unused') AS ProductionDate, om.SkillId AS SkillIdTemp,
                 convert(DECIMAL(18,2),SUM(bmd.RequiredManPower)) AS RequiredManPower ,SUM(bmd.AllotedManpower) as AllotedManpower
			
                        from trn.ProductionOrder PO
                    inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                    INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID 
                    AND p1.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
                    AND p1.ID=(SELECT TOP 1 TT.ID FROM ProductionPlanningType1 TT WHERE tt.WorkCenterMasterId=p1.WorkCenterMasterId AND tt.ProductionDate=p1.ProductionDate)
                    LEFT OUTER JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=po.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BT.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.ProductionBulletinTemplateMasterId=BM.Id
        
                  
                    JOIN mst.OperationMaster AS om ON om.Id=bmd.SkillMasterId       
                    LEFT OUTER JOIN hkp.Process AS omp ON omp.Id=om.ProcessId      


					left outer join (Select EOP.Id as operationId
											
											from EmployeeInformation E
											LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
											LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
											LEFT JOIN EmployeeOperation EO ON EO.EmpSystemId=E.SystemId
											LEFT JOIN MST.OperationMaster EOP ON EOP.Id=EO.OperationMasterId
											LEFT JOIN (
											SELECT ES.EmpSystemID, S.ShiftDefinationDescription
											FROM EmpDateWiseShiftAssign ES
											LEFT JOIN ShiftDefination S ON S.SystemID = ES.ShiftSystemID
											WHERE ES.WorkDate =FORMAT(GETDATE(), 'dd-MMM-yyyy') 
                                            AND isnull(S.SystemId,'') IN(" + parameters["ShiftId"] + @")
											) AD ON AD.EmpSystemID=E.SystemId
											where E.EmployeeStatus='Active' AND E.EmpType<>'Guest' 
											group by EOP.Id,C.id 
											) as shi on shi.operationId = om.Id
                          where CAST(p1.ProductionDate as DATE) between '" + fromDate + @"' and  '" + toDate + @"' AND
                            isnull(po.PlantId ,'') IN(" + parameters["PlantId"] + @") AND
					      isnull(po.EntityId,'') IN(" + parameters["EntityId"] + @")  AND
					      isnull(omp.Id,'') IN(" + parameters["ProcessId"] + @")  AND
					      isnull(om.OperationTypeId,'') IN(" + parameters["TypeId"] + @") AND
						  isnull(om.SkillId,'') IN(" + parameters["SkillId"] + @")  AND
					      ISNULL(om.SkillGroupId,'') IN(" + parameters["SkillGroupId"] + @") 
                     GROUP BY om.Id,om.SkillId, p1.ProductionDate,om.UserName, PO.PlantId , PO.EntityId,omp.Id,om.OperationCategoryId , 
                    om.SkillGroupId ,  om.OperationTypeId , om.Code
					
                     ) AS DT ON omsk.Id=DT.SkillIdTemp
                     
                    

					 LEFT JOIN (Select emp.Id as CompanyId,emp.OperationMasterId,
								sum(emp.Skill1) as Skill1 , sum(emp.Skill2) as Skill2 , sum(emp.Skill3) as Skill3 
								from
								(Select C.Id,EOP.Id AS OperationMasterId,  sum( case when EO.Sequence='1.00' then 1 else 0 end)  as Skill1 , 
											sum( case when EO.Sequence='2.00' then 1 else 0 end) as Skill2 , 
											sum( case when EO.Sequence='3.00' then 1 else 0 end) as Skill3
											from EmployeeInformation E
											LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
											LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
											LEFT JOIN EmployeeOperation EO ON EO.EmpSystemId=E.SystemId
											LEFT JOIN MST.OperationMaster EOP ON EOP.Id=EO.OperationMasterId
											LEFT JOIN (
											SELECT ES.EmpSystemID, S.ShiftDefinationDescription
											FROM EmpDateWiseShiftAssign ES
											LEFT JOIN ShiftDefination S ON S.SystemID = ES.ShiftSystemID
											WHERE ES.WorkDate =FORMAT(GETDATE(), 'dd-MMM-yyyy')
											 AND isnull(S.SystemId,'') IN(" + parameters["ShiftId"] + @")
											) AD ON AD.EmpSystemID=E.SystemId
											where E.EmployeeStatus='Active' AND E.EmpType<>'Guest'
											group by EOP.Id,C.id  ) as emp
											group by emp.Id , emp.OperationMasterId 
											) as emp on emp.OperationMasterId = om.Id

					     where isnull(omsk.SkillCategoryId,'')  IN(" + parameters["CategoryId"] + @")  
group by emp.Skill1 , emp.Skill2 , emp.Skill3 ,skc.[Sequence] , omp.Id , om.Id , om.SkillId , om.SkillGroupId , om.OperationTypeId,om.Code,
om.UserName,sg.UserName,om.OperationCategoryId ,skc.UserName , omsk.SkillCategoryId , omsk.UserName , dt.ProductionDate , dt.SkillIdTemp , emp.CompanyId
            ORDER BY  skc.[Sequence]";

            DataTable dtRequiredManpower = _sqlRepository.GetDataTable(str);




            DataTable dtDistinctDates = dtRequiredManpower.DefaultView.ToTable(true, "ProductionDate");
            List<DateTime> ltDistinctDates = new List<DateTime>();

            for (int i = 0; i < dtDistinctDates.Rows.Count; i++)
            {
                ltDistinctDates.Add(Convert.ToDateTime(dtDistinctDates.Rows[i]["ProductionDate"].ToString()));
            }

            ltDistinctDates = ltDistinctDates.OrderBy(k => k).ToList();
            DataTable SkillData = new DataTable("SKILL");
            SkillData.Columns.Add("SkillId");
            SkillData.Columns.Add("RowCaption");
            SkillData.Columns.Add("Skill");
            SkillData.Columns.Add("SkillGroup");
            SkillData.Columns.Add("SkillCategory");
            SkillData.Columns.Add("SkillCode");
            SkillData.Columns.Add("Skill1");
            SkillData.Columns.Add("Skill2");
            SkillData.Columns.Add("Skill3");
            SkillData.Columns.Add("Flag");
            SkillData.Columns.Add("Index");

            DateColumns = new List<string>(); ;
            for (int i = 0; i < ltDistinctDates.Count; i++)
            {
                DateColumns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"));
                SkillData.Columns.Add(ltDistinctDates[i].ToString("dd-MMM-yy"), typeof(double));
            }

            string SkillId = "";
            DataRow dr = null;
            int ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                if (dtRequiredManpower.Rows[i]["SkillCode"].ToString() != "")
                {


                    #region ShortExcess
                    if (dtRequiredManpower.Rows[i]["SkillCode"].ToString() != SkillId)
                    {
                        dr = SkillData.NewRow();
                        dr["Index"] = ind;
                        dr["SkillId"] = dtRequiredManpower.Rows[i]["SkillId"];
                        dr["Skill"] = dtRequiredManpower.Rows[i]["Skill"];
                        dr["SkillGroup"] = dtRequiredManpower.Rows[i]["SkillGroup"];
                        dr["SkillCategory"] = dtRequiredManpower.Rows[i]["SkillCat"];
                        dr["SkillCode"] = dtRequiredManpower.Rows[i]["SkillCode"];
                        dr["Skill1"] = dtRequiredManpower.Rows[i]["Skill1"];
                        string skill = dtRequiredManpower.Rows[i]["Skill1"].ToString();
                        dr["Skill2"] = dtRequiredManpower.Rows[i]["Skill2"];
                        dr["Skill3"] = dtRequiredManpower.Rows[i]["Skill3"];
                        dr["Flag"] = "ShortExcess";
                        for (int j = 0; j < ltDistinctDates.Count; j++)
                        {

                            dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = skill;

                        }

                        ind++;
                        SkillData.Rows.Add(dr);

                    }

                    dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["ShortExcess"].ToString());

                    #endregion
                    SkillId = dtRequiredManpower.Rows[i]["SkillCode"].ToString();
                }
            }

            SkillId = "";
            dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                if (dtRequiredManpower.Rows[i]["SkillCode"].ToString() != "")
                {
                    #region ActualMP
                    if (dtRequiredManpower.Rows[i]["SkillCode"].ToString() != SkillId)
                    {
                        dr = SkillData.NewRow();
                        dr["Index"] = ind;
                        dr["SkillId"] = dtRequiredManpower.Rows[i]["SkillId"];
                        dr["Skill"] = dtRequiredManpower.Rows[i]["Skill"];
                        dr["SkillGroup"] = dtRequiredManpower.Rows[i]["SkillGroup"];
                        dr["SkillCategory"] = dtRequiredManpower.Rows[i]["SkillCat"];
                        dr["SkillCode"] = dtRequiredManpower.Rows[i]["SkillCode"];
                        string skilled = dtRequiredManpower.Rows[i]["Skill1"].ToString();
                        dr["Flag"] = "ActualMP";
                        for (int j = 0; j < ltDistinctDates.Count; j++)
                        {

                            dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = skilled;

                        }
                        ind++;
                        SkillData.Rows.Add(dr);
                    }

                    dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["Skill1"].ToString());

                    #endregion

                    SkillId = dtRequiredManpower.Rows[i]["SkillCode"].ToString();
                }
            }


            SkillId = "";
            dr = null;
            ind = 0;
            for (int i = 0; i < dtRequiredManpower.Rows.Count; i++)
            {
                if (dtRequiredManpower.Rows[i]["SkillCode"].ToString() != "")
                {
                    #region RequiredMP
                    if (dtRequiredManpower.Rows[i]["SkillCode"].ToString() != SkillId)
                    {
                        dr = SkillData.NewRow();
                        dr["Index"] = ind;
                        dr["SkillId"] = dtRequiredManpower.Rows[i]["SkillId"];
                        dr["Skill"] = dtRequiredManpower.Rows[i]["Skill"];
                        dr["SkillGroup"] = dtRequiredManpower.Rows[i]["SkillGroup"];
                        dr["SkillCategory"] = dtRequiredManpower.Rows[i]["SkillCat"];
                        dr["SkillCode"] = dtRequiredManpower.Rows[i]["SkillCode"];
                        dr["Flag"] = "RequiredManPower";
                        for (int j = 0; j < ltDistinctDates.Count; j++)
                        {

                            dr[ltDistinctDates[j].ToString("dd-MMM-yy")] = 0;

                        }

                        ind++;
                        SkillData.Rows.Add(dr);
                    }


                    dr[dtRequiredManpower.Rows[i]["ProductionDate"].ToString()] = OTSBD.clsStaticInfo.dbl(dtRequiredManpower.Rows[i]["AllotedManPower"].ToString());

                    #endregion
                    SkillId = dtRequiredManpower.Rows[i]["SkillCode"].ToString();
                }
            }

            DataTable ds = CompactTable(SkillData, DateColumns);
            dt = Library.Service.Helpers.DataTableExtensions.DataTableToJson(ds);

            return Library.Service.Helpers.DataTableExtensions.DataTableToJson(SkillData);
        }

        private DataTable CompactTable(DataTable dtData, List<string> Columns)
        {



            DataTable dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Available" }).Select(x => {

                DataRow row = dtData.NewRow();
                row["RowCaption"] = "Total Available";
                for (int i = 0; i < Columns.Count; i++)
                {

                    row[Columns[i]] = x.Sum(r => r["Flag"].ToString() == "ActualMP" ? Convert.ToDouble(r[Columns[i]]) : 0);
                }
                return row;

            }).CopyToDataTable();

            DataTable dtTemp = dtCompact.DefaultView.ToTable();

            dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Requirement" }).Select(x => {

                DataRow row = dtData.NewRow();
                row["RowCaption"] = "Total Requirement";
                for (int i = 0; i < Columns.Count; i++)
                {

                    row[Columns[i]] = x.Sum(r => r["Flag"].ToString() == "RequiredManPower" ? OTSBD.clsStaticInfo.dbl(Convert.ToDouble(r[Columns[i]])) : 0);
                }
                return row;

            }).CopyToDataTable();

            dtTemp.Merge(dtCompact.DefaultView.ToTable());

            dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Excess" }).Select(x => {

                DataRow row = dtData.NewRow();
                row["RowCaption"] = "Total Excess";
                for (int i = 0; i < Columns.Count; i++)
                {

                    row[Columns[i]] = x.Sum(r => OTSBD.clsStaticInfo.dbl(r[Columns[i]]) > 0 && r["Flag"].ToString() == "ShortExcess" ? Convert.ToDouble(r[Columns[i]]) : 0);
                }
                return row;

            }).CopyToDataTable();

            dtTemp.Merge(dtCompact.DefaultView.ToTable());

            dtCompact = dtData.AsEnumerable().GroupBy(x => new { Flag = "Total Short" }).Select(x => {

                DataRow row = dtData.NewRow();
                row["RowCaption"] = "Total Short";
                for (int i = 0; i < Columns.Count; i++)
                {

                    row[Columns[i]] = x.Sum(r => OTSBD.clsStaticInfo.dbl(r[Columns[i]]) < 0 && r["Flag"].ToString() == "ShortExcess" ? Convert.ToDouble(r[Columns[i]]) : 0);
                }
                return row;

            }).CopyToDataTable();

            dtTemp.Merge(dtCompact.DefaultView.ToTable());


            return dtTemp;
        }
        public IEnumerable<object> allotedWorkCenter(Dictionary<string, string> parameters, string skillId, string date)
        {
            try
            {
                var str = @"SELECT 
                FORMAT(p1.ProductionDate,'dd-MMM-yy') AS ProductionDate, om.SkillId AS SkillIdTemp, om.Code as SkillCode, p1.WorkCenterMasterId,
                 sum(bmd.AllotedManpower) as Alloted , wc.UserName , 
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
                    INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID 
                    AND p1.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
                    AND p1.ID=(SELECT TOP 1 TT.ID FROM ProductionPlanningType1 TT WHERE tt.WorkCenterMasterId=p1.WorkCenterMasterId AND tt.ProductionDate=p1.ProductionDate)
                    LEFT OUTER JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=po.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BT.Id
                    LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.ProductionBulletinTemplateMasterId=BM.Id
        
                  
                    JOIN mst.OperationMaster AS om ON om.Id=bmd.SkillMasterId       
                    LEFT OUTER JOIN hkp.Process AS omp ON omp.Id=om.ProcessId      
					left join scs.WorkCenterMaster wc on wc.Id = po.EntityId

					left outer join (Select EOP.Id as operationId
											
											from EmployeeInformation E
											LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
											LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
											LEFT JOIN EmployeeOperation EO ON EO.EmpSystemId=E.SystemId
											LEFT JOIN MST.OperationMaster EOP ON EOP.Id=EO.OperationMasterId
											LEFT JOIN (
											SELECT ES.EmpSystemID, S.ShiftDefinationDescription
											FROM EmpDateWiseShiftAssign ES
											LEFT JOIN ShiftDefination S ON S.SystemID = ES.ShiftSystemID
											WHERE ES.WorkDate =FORMAT(GETDATE(), 'dd-MMM-yyyy') 
                                            AND isnull(S.SystemId,'') IN(" + parameters["ShiftId"] + @")
											) AD ON AD.EmpSystemID=E.SystemId
											where E.EmployeeStatus='Active' AND E.EmpType<>'Guest' 
											group by EOP.Id,C.id 
											) as shi on shi.operationId = om.Id
                          where CAST(p1.ProductionDate as DATE) = '" + date + @"' AND
                          isnull(po.PlantId ,'') IN(" + parameters["PlantId"] + @") AND
					      isnull(po.EntityId,'') IN(" + parameters["EntityId"] + @")   AND
					      isnull(omp.Id,'') IN(" + parameters["ProcessId"] + @")   AND
					      isnull(om.OperationTypeId,'') IN(" + parameters["TypeId"] + @")  AND
						  isnull(om.SkillId,'') ='" + skillId + @"' and 
                          ISNULL(om.SkillGroupId,'') IN(" + parameters["SkillGroupId"] + @")
                    GROUP BY p1.ProductionDate , om.SkillId , wc.UserName ,po.id , om.Code , p1.WorkCenterMasterId
                ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> skillWiseEmployees(string code, string shifts, string seq)
        {
            try
            {
                var str = @"Select C.Id,EOP.Code, E.EmployeeCode , E.EmployeeName 
											
											from EmployeeInformation E
											LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
											LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
											LEFT JOIN EmployeeOperation EO ON EO.EmpSystemId=E.SystemId
											LEFT JOIN MST.OperationMaster EOP ON EOP.Id=EO.OperationMasterId
											LEFT JOIN (
											SELECT ES.EmpSystemID, S.ShiftDefinationDescription
											FROM EmpDateWiseShiftAssign ES
											LEFT JOIN ShiftDefination S ON S.SystemID = ES.ShiftSystemID
                                            
											WHERE ES.WorkDate =FORMAT(GETDATE(), 'dd-MMM-yyyy')
                                            AND isnull(S.SystemId,'') IN(" + shifts + @")
											) AD ON AD.EmpSystemID=E.SystemId
											where E.EmployeeStatus='Active' AND E.EmpType<>'Guest' and eop.code = '" + code + @"' and eo.Sequence = '" + seq + @"'
											order by E.EmployeeName";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
