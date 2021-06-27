using System;
using System.Collections.Generic;
using System.Data;

namespace Library.HumanResource.Payroll.Tax
{
   public class cIncomeTaxCalculation
    {
        string _taxYearId = string.Empty;
        string _plantId = string.Empty;

        public cIncomeTaxCalculation(string _taxYearId, string _plantId)
        {
            this._taxYearId = _taxYearId;
            this._plantId = _plantId;
        }

        public void Calculate()
        {
            try
            {
                List<EmpList> _empLists = _getEmpList();
                cTaxableIncome _taxableIncome = new cTaxableIncome(this._taxYearId);
                _taxableIncome.GetTaxableIncome(ref _empLists);
                //
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<EmpList> _getEmpList() 
        {
            string strSQL;
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            List<EmpList> _empLists = new List<EmpList>();
            try
            {

                strSQL = @" select e.SystemId EmpSystemId,e.EmployeeCode
                                    ,format(e.doj,'dd-MMM-yyyy')DOJ
                                    ,format(y.StartDate,'dd-MMM-yyyy')TaxYearStartDate
                                    ,format(c.CutOffDate,'dd-MMM-yyyy')CutOffDate
                                    ,convert(decimal(18,2), 0) TaxableIncomeFullYear
                                    from EmployeeInformation e
                                    left join scs.CompanyTaxYear t on t.CompanyId=e.CompanyId
                                    left join scs.TaxYear y on t.TaxYearId=y.Id
                                    left join scs.OpeningBalanceCutOffDate c on c.PlantId=e.PlantId and c.ModuleName='HR'
                                    where e.DOJ<=y.EndDate and (e.DOS is null or e.DOS>=y.StartDate)
                                    and e.PlantId='" + this._plantId+@"' and y.id='"+this._taxYearId+@"'
                                    ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    _empLists = dsRef.Tables[0].ToList<EmpList>();
                }
                return _empLists;
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
