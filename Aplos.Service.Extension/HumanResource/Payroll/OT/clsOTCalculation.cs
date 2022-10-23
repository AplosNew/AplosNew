using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Library.Service.Payrolls.OT
{
    public class clsOTCalculation
    {
        public void LoadSalaryStructure(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                            select m.*,d.EntryAmount Amount,d.DefineAmount,d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType 
                            ,LD.UserName OldLegalDesignation, LG.Code OldGradeCode from
                                (---m
                            select max(ed) ed,EmpInfoSystemID from
                            (
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid='" + sPlantID + @"' group by EmpInfoSystemID
												                            union 
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid='" + sPlantID + @"'  group by EmpInfoSystemID
                            ) x 
                            group by EmpInfoSystemID
                            ) ---m
                            mx
                            left join (
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid='" + sPlantID + @"'
                            union
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid='" + sPlantID + @"'
                            )
                             m on m.EmpInfoSystemID=mx.EmpInfoSystemID and m.EffectiveDate=mx.ed
                            left join (
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoDefine
                            union
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoBack
                            )
                            d on d.SalaryID=m.SystemID
                            left join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
                            LEFT JOIN IncrementHistory IH on IH.ToSalaryId=d.SalaryID
                            
                            LEFT JOIN Hkp.LegalDesignation LD ON LD.Id = ih. FromLegalDesignationId
                            LEFT JOIN MST.LegalSalaryGradeDesignation LGD ON LGD.LegalDesignationId = ih.FromLegalDesignationId AND LGD.PlantId='" + sPlantID + @"'
                            
                            LEFT JOIN scs.LegalSalaryGrade LG ON LG.Id = LGD.LegalSalaryGradeId
                            

                            order by m.EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

       
        public void LoadSalaryStructureWithGradeLD(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                            select m.*,d.EntryAmount Amount,d.DefineAmount,d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType 
                            ,LD.UserName OldLegalDesignation, LG.Code OldGradeCode from
                                (---m
                            select max(ed) ed,EmpInfoSystemID from
                            (
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and EffectiveDate>='" + sFromDate + @"' and plantid='" + sPlantID + @"' group by EmpInfoSystemID
												                            union 
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and EffectiveDate>='" + sFromDate + @"' and plantid='" + sPlantID + @"'  group by EmpInfoSystemID
                            ) x 
                            group by EmpInfoSystemID
                            ) ---m
                            mx
                            left join (
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and EffectiveDate>='" + sFromDate + @"' and plantid='" + sPlantID + @"'
                            union
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and EffectiveDate>='" + sFromDate + @"' and plantid='" + sPlantID + @"'
                            )
                             m on m.EmpInfoSystemID=mx.EmpInfoSystemID and m.EffectiveDate=mx.ed
                            left join (
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoDefine
                            union
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoBack
                            )
                            d on d.SalaryID=m.SystemID
                            left join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
                            LEFT JOIN IncrementHistory IH on IH.ToSalaryId=d.SalaryID
                            
                            LEFT JOIN Hkp.LegalDesignation LD ON LD.Id = ih. FromLegalDesignationId
                            LEFT JOIN MST.LegalSalaryGradeDesignation LGD ON LGD.LegalDesignationId = ih.FromLegalDesignationId
                            
                            LEFT JOIN scs.LegalSalaryGrade LG ON LG.Id = LGD.LegalSalaryGradeId
                            order by m.EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void LoadSalaryStructureNew(string sPlantID, string sFromDate, string sToDate,string HeadCategory, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT distinct NEW.SystemID,NEW.RankEmp,NEW.EmpInfoSystemID,NEW.SalaryRuleMasterSystemID,NEW.EffectiveDate,d.EntryAmount Amount,d.DefineAmount,d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType 
                            ,LD.UserName OldLegalDesignation, LG.Code OldGradeCode from 
                            
                            (
                            	SELECT 
								dense_rank() OVER (PARTITION BY NI.EmpInfoSystemID ORDER BY NI.EffectiveDate DESC,SystemId) AS RankEmp,NI.*
								 FROM 
								(
								SELECT sidm.SystemID, sidm.EmpInfoSystemID, sidm.SalaryRuleMasterSystemID, sidm.EffectiveDate FROM SalaryInfoDefineMaster AS sidm WHERE sidm.IsApproved=1 AND sidm.PlantID='" + sPlantID + @"' AND sidm.EffectiveDate BETWEEN '"+sFromDate+@"' AND '"+sToDate+@"'
								UNION ALL
								SELECT sidm.SystemID, sidm.EmpInfoSystemID, sidm.SalaryRuleMasterSystemID, sidm.EffectiveDate FROM SalaryInfoBackMaster AS sidm WHERE sidm.IsApproved=1 AND sidm.PlantID='"+ sPlantID + @"'  AND sidm.EffectiveDate BETWEEN '"+sFromDate+@"' AND '"+sToDate+ @"'
								) AS NI
                            	
                            ) AS NEW 

                INNER JOIN 
                (
                SELECT sid1.SystemID,sid1.EntryAmount,sid1.DefineAmount,sid1.SalaryID,sid1.SalaryHeadID,sid1.EntryCurrencyID FROM SalaryInfoDefine AS sid1
                UNION ALL
                SELECT sid1.SystemID,sid1.EntryAmount,sid1.DefineAmount,sid1.SalaryID,sid1.SalaryHeadID,sid1.EntryCurrencyID FROM SalaryInfoBack AS sid1 
                ) AS D ON D.SalaryId=NEW.SystemID
                 left join SalaryHead h on h.SalaryHeadID=D.SalaryHeadID
                LEFT JOIN IncrementHistory IH on IH.ToSalaryId=D.SalaryID
                           
                LEFT JOIN Hkp.LegalDesignation LD ON LD.Id = ih. FromLegalDesignationId
                LEFT JOIN MST.LegalSalaryGradeDesignation LGD ON LGD.LegalDesignationId = ih.FromLegalDesignationId and LGD.PlantId='" + sPlantID + @"'
                            
                LEFT JOIN scs.LegalSalaryGrade LG ON LG.Id = LGD.LegalSalaryGradeId

                where NEW.RankEmp=1 AND h.HeadCategory='" + HeadCategory+@"' 
                ORDER BY new.EmpInfoSystemID ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function


        public void LoadSalaryStructureOld(string sPlantID, string sFromDate, string sToDate, string HeadCategory, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT DISTINCT xx.SystemID,xx.EmpInfoSystemID,xx.SalaryIncrementSystemID,xx.SalaryRuleMasterSystemID,xx.EffectiveDate,xx.IsApproved,xx.Amount,xx.DefineAmount,xx.SalaryHeadID
 ,xx.EntryCurrencyID,xx.SalaryHead,xx.HeadCategory,xx.HeadType,xx.OldLegalDesignation,xx.OldGradeCode FROM (
SELECT  OLD.*,d.EntryAmount Amount,d.DefineAmount,d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType 
                            ,LD.UserName OldLegalDesignation, LG.Code OldGradeCode  FROM (

				SELECT dense_rank() OVER (PARTITION BY m.EmpInfoSystemID ORDER BY m.EffectiveDate DESC) AS OLDRANK, M.*
											   from (select LastInc.EmpInfoSystemID,LastInc.EffectiveDate
								   from (SELECT 
				dense_rank() OVER (PARTITION BY NI.EmpInfoSystemID ORDER BY NI.EffectiveDate DESC) AS RankEmp,
				NI.*
				 FROM (
						SELECT sidm.SystemID, sidm.EmpInfoSystemID, sidm.SalaryRuleMasterSystemID, sidm.EffectiveDate FROM SalaryInfoDefineMaster AS sidm WHERE sidm.IsApproved=1 AND sidm.PlantID='" + sPlantID + @"' AND sidm.EffectiveDate BETWEEN '" + sFromDate+@"' AND '"+sToDate+ @"'
								UNION ALL
								SELECT sidm.SystemID, sidm.EmpInfoSystemID, sidm.SalaryRuleMasterSystemID, sidm.EffectiveDate FROM SalaryInfoBackMaster AS sidm WHERE sidm.IsApproved=1 AND sidm.PlantID='"+sPlantID+@"'  AND sidm.EffectiveDate BETWEEN '" + sFromDate + @"' AND '" + sFromDate + @"'
					) AS NI
			
					) AS LastInc 
				where LastInc.RankEmp=1
				) AS NEW
				JOIN (
				SELECT sidm.EmpInfoSystemID, sidm.SystemID, sidm.SalaryIncrementSystemID,sidm.SalaryRuleMasterSystemID, sidm.EffectiveDate, sidm.IsApproved FROM SalaryInfoDefineMaster AS sidm 
				UNION ALL
				SELECT  sidm.EmpInfoSystemID, sidm.SystemID, sidm.SalaryIncrementSystemID,sidm.SalaryRuleMasterSystemID, sidm.EffectiveDate, sidm.IsApproved FROM SalaryInfoBackMaster AS sidm
				) AS M ON m.EmpInfoSystemID=NEW.EmpInfoSystemID AND convert(date,m.EffectiveDate)<convert(date,new.EffectiveDate)
) AS OLD


INNER JOIN 
(
SELECT sid1.SystemID,sid1.EntryAmount,sid1.DefineAmount,sid1.SalaryID,sid1.SalaryHeadID,sid1.EntryCurrencyID FROM SalaryInfoDefine AS sid1 
UNION ALL
SELECT sid1.SystemID,sid1.EntryAmount,sid1.DefineAmount,sid1.SalaryID,sid1.SalaryHeadID,sid1.EntryCurrencyID FROM SalaryInfoBack AS sid1 
) AS D ON D.SalaryId=OLD.SystemID
 left join SalaryHead h on h.SalaryHeadID=D.SalaryHeadID
LEFT JOIN IncrementHistory IH on IH.ToSalaryId=D.SalaryID
                           
LEFT JOIN Hkp.LegalDesignation LD ON LD.Id = ih. FromLegalDesignationId
LEFT JOIN MST.LegalSalaryGradeDesignation LGD ON LGD.LegalDesignationId = ih.FromLegalDesignationId and LGD.PlantId='"+ sPlantID + @"'
                            
LEFT JOIN scs.LegalSalaryGrade LG ON LG.Id = LGD.LegalSalaryGradeId and LGD.PlantId='" + sPlantID + @"'

where OLD.OLDRANK=1 AND h.HeadCategory='" + HeadCategory+ @"' and LG.Code is not null) as xx
ORDER BY xx.EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function


        public void LoadOldSalaryStructure(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                           select m.*,d.EntryAmount Amount,d.DefineAmount,
d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType from
(---m
select max(ed) ed,EmpInfoSystemID from
(

select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='"+sToDate+@"' and plantid='"+sPlantID+ @"' group by EmpInfoSystemID
) x
group by EmpInfoSystemID
) ---m
mx
Inner join (

select SystemID,EmpInfoSystemID,EffectiveDate from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid='" + sPlantID + @"'
)
m on m.EmpInfoSystemID=mx.EmpInfoSystemID and m.EffectiveDate=mx.ed
left join (

select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoBack
)
d on d.SalaryID=m.SystemID
left join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
order by m.EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function




        public void LoadOverTimePolicy(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;//
            try
            {
                strSQL = @"
                                        SELECT e.SystemId,E.EmployeeCode,e.GivenDesignationId,dc.IsOTEntitled
											,onw.FormulaDesID FormulaDesIDN,onw.IsFixed IsFixedN,onw.IsFormula IsFormulaN,onw.FixedValue FixedValueN
											,ow.FormulaDesID FormulaDesIDW,ow.IsFixed IsFixedW,ow.IsFormula IsFormulaW,ow.FixedValue FixedValueW
											,oh.FormulaDesID FormulaDesIDH,oh.IsFixed IsFixedH,oh.IsFormula IsFormulaH,oh.FixedValue FixedValueH


                                    FROM dbo.EmployeeInformation E                                                

												left join mst.DesignationMaster dml on dml.DesignationId=e.GivenDesignationId
												inner join (select DesignationMasterId,OverTimePmtPolicyMasterID,IsOTEntitled 
                                                            from scs.DesignationMasterConfiguration where PlantId='" + sPlantID + @"' and IsOTEntitled=1) dc 
                                                            on dc.DesignationMasterId=dml.Id
												left join OverTimePmtPolicyMaster otpm on otpm.ID=dc.OverTimePmtPolicyMasterID and otpm.PlantID='" + sPlantID + @"'
												left join OverTimePmtPolicyDetails oH on oh.OverTimePmtPolicyID=otpm.ID and oh.OverTimeDayType='Holiday'
												left join OverTimePmtPolicyDetails oW on ow.OverTimePmtPolicyID=otpm.ID and ow.OverTimeDayType='Week Off'
												left join OverTimePmtPolicyDetails oNW on oNW.OverTimePmtPolicyID=otpm.ID and onw.OverTimeDayType='Working Day'

												where (e.DOJ<='" + sToDate + @"') and (dos is null or e.DOS>='" + sFromDate + @"') and e.PlantId='" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
    }
}
