using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Payrolls.OT
{
    public class xclsOTCalculation
    {
        public void LoadSalaryStructure(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                            select m.*,d.EntryAmount Amount,d.DefineAmount,d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType from
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
