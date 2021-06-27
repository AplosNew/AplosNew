using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Tax
{
    public class clsSalaryStructure
    {
        public void GetSalaryStructureRawData(string EmpSystemIds,string EffectiveDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT mm.EmpInfoSystemID EmpSystemId,m.EffectiveDate,mm.SystemID
                                ,d.SalaryHeadID,d.EntryAmount
                                  FROM 
  
                                  (select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
                                  union
                                  select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
                                  )
                                   mm 
                                  inner join (
                                  select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID from (
                                  select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='"+EffectiveDate+ @"'
                                  union
                                   select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + EffectiveDate + @"'
                                   ) x 
                                   group by EmpInfoSystemID
                                  )m on mm.EffectiveDate=m.EffectiveDate and m.EmpInfoSystemID=mm.EmpInfoSystemID
                                  left join
                                  (select SalaryID,SalaryHeadID,EntryAmount from  SalaryInfoDefine
                                  union
                                  select SalaryID,SalaryHeadID,EntryAmount from SalaryInfoBack
                                  ) d on d.SalaryID=mm.SystemID


                                 where mm.EmpInfoSystemID in (
                                " + EmpSystemIds + @"
                                 )
                                  order by mm.EmpInfoSystemID,EffectiveDate";

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

