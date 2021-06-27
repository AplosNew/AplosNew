using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Tax
{
  public class cTaxableIncome
    {
        string _taxYearId = string.Empty;
        public cTaxableIncome(string _taxYearId)
        {
            this._taxYearId = _taxYearId;
        }
        public void GetTaxableIncome(ref List<EmpList> _listEmpList)
        {
            string _emps = "''";
            try
            {
                foreach (var item in _listEmpList)
                {
                    _emps += ",'" + item.EmpSystemId + "'";
                }

                List<IncomeTaxPolicyVM> _headWiseYearlyTaxableIncome=  GetHeadWiseAmount(_emps);
                foreach (var item in _listEmpList)
                {
                    var _heads_of_single_emp = _headWiseYearlyTaxableIncome.Where(r => r.EmpSystemId == item.EmpSystemId).ToList<IncomeTaxPolicyVM>();
                    var _yearlyTaxableIncome = _heads_of_single_emp.Sum(d => d.TaxableIncome);
                    item.TaxableIncomeFullYear = _yearlyTaxableIncome;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<IncomeTaxPolicyVM> GetHeadWiseAmount(string _emps)
        {
            string strSQL;
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            List<IncomeTaxPolicyVM> incomeTaxPolicyVMs = new List<IncomeTaxPolicyVM>();
            try
            {

                strSQL = @" select e.SystemId empsystemid,e.EmployeeCode                        
                        ,format(e.doj,'dd-MMM-yyyy')DOJ
                        ,format(e.dos,'dd-MMM-yyyy')DOS
                        ,m.SystemID,m.Description,m.TaxYearID,m.PlantID
                        ,g.SalaryHeadID
                        ,g.SalaryHeadID,h.SalaryHead
                        ,g.IsTaxable,g.IsFixedTaxGeneral						
						,convert(decimal(18,2), isnull(g.TaxFixedGeneral,0)) TaxFixedGeneral
						,convert(decimal(18,2), isnull(g.TaxPercentageGeneral,0)) TaxPercentageGeneral
						,convert(decimal(18,2), isnull(g.TaxMaxExmpAmt,0)) TaxMaxExmpAmt
						,convert(decimal(18,2), isnull(g.PercentageExmAmtOtherSlrHd,0)) PercentageExmAmtOtherSlrHd
						,convert(decimal(18,2), 0) EntryAmountOfOtherSHead

						,convert(decimal(18,2), isnull(ss.EntryAmount,0)) EntryAmount
                        ,convert(decimal(18,2), 0) ExmBaseOnActual
                        , convert(decimal(18,2), 0) TaxableIncome
                        ,12 TaxPeriodCount

						,g.IsPercentageTaxGeneral
						,g.IsExemption,g.IsExmWhichEverLess,g.IsMaxExmpAmt
						,g.IsExmBaseOnActual,g.IsExmBaseOnOtherSlrHd,g.ExmSalaryHeadID

                        from
                        EmployeeInformation e
                        left join TaxPolicyMaster m on m.PlantID=e.PlantId
                        left join TaxPolicyGeneral g on m.SystemID=g.TaxPolicyMstID
                        left join SalaryHead h on h.SalaryHeadID=g.SalaryHeadID
                        left join scs.TaxYear y on y.id=m.TaxYearID

                        left join (
                        SELECT n.SystemId EmpSystemId ,n.EmployeeCode,mm.EffectiveDate
                        ,d.SalaryHeadID,h.SalaryHead,d.EntryAmount
	                          FROM EmployeeInformation n
	                          inner join 
	                          (select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
	                          union
	                          select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
	                          )
	                          mm on mm.EmpInfoSystemID=n.SystemId
	                          inner join (
	                          select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID   
	                          from (
	                          select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='29-Jan-2020'
	                          union
	                           select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='29-Jan-2020'
	                           ) x 
	                           group by EmpInfoSystemID
	                          )m on mm.EffectiveDate=m.EffectiveDate and m.EmpInfoSystemID=mm.EmpInfoSystemID
	                          left join SalaryInfoDefine d on d.SalaryID=mm.SystemID
	                          left join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID

                          --where  n.PlantId=''
                        ) ss on ss.EmpSystemId=e.SystemId and g.SalaryHeadID=ss.SalaryHeadID
                        where y.Id='" + this._taxYearId + @"' and e.systemid in ("+ _emps + @")
                        and e.doj<=y.EndDate and (doj is null or e.dos>y.StartDate)
                         ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if(dsRef.Tables[0].Rows.Count>0)
                {
                    incomeTaxPolicyVMs = dsRef.Tables[0].ToList<IncomeTaxPolicyVM>();
                }
                return incomeTaxPolicyVMs;
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

    public class EmpList
    {
        public string EmpSystemId { get; set; }
        public string EmployeeCode { get; set; }
        public string DOJ { get; set; }
        public string TaxYearStartDate { get; set; }
        public string CutOffDate { get; set; }
        public decimal TaxableIncomeFullYear { get; set; }
    }
    public class EmpListHeadWiseAmount
    {
        public string EmpSystemId { get; set; }
        public string SalaryHeadId { get; set; }
        public string StructureAmount { get; set; }
        public string FormulaDes { get; set; }
        public string FormulaIds { get; set; }
        public string TaxableAmount { get; set; }
    }
    public class IncomeTaxPolicyVM
    {
        public string EmpSystemId { get; set; }
        public string EmployeeCode { get; set; }
        public string SalaryHeadId { get; set; }

        public string doj { get; set; }
        public string dos { get; set; }
        public string Description { get; set; }

        public string TaxYearID { get; set; }
        public string PlantID { get; set; }
        public string SalaryHeadID { get; set; }
        public string SalaryHead { get; set; }

        public bool IsTaxable { get; set; }
        public bool IsFixedTaxGeneral { get; set; }
        public decimal TaxFixedGeneral { get; set; }
        public bool IsPercentageTaxGeneral { get; set; }

        public decimal TaxPercentageGeneral { get; set; }
        public bool IsExemption { get; set; }
        public bool IsExmWhichEverLess { get; set; }

        public bool IsMaxExmpAmt { get; set; }
        public decimal TaxMaxExmpAmt { get; set; }
        public bool IsExmBaseOnActual { get; set; }
        public decimal ExmBaseOnActual { get; set; }

        public bool IsExmBaseOnOtherSlrHd { get; set; }
        public string ExmSalaryHeadID { get; set; }
        public decimal PercentageExmAmtOtherSlrHd { get; set; }
        public decimal EntryAmount { get; set; }
        public decimal EntryAmountOfOtherSHead { get; set; }
        public int TaxPeriodCount { get; set; }
        private decimal taxableIncome;

        public decimal TaxableIncome
        {
            get { return _taxableIncome(); }
            set { taxableIncome = value; } 
        }

        decimal _taxableIncome()
        {
            decimal _return=0;
            decimal _tempReturn = 0;
            try
            {
                if(this.IsTaxable)
                {
                    if(this.IsPercentageTaxGeneral)
                    {
                        _tempReturn = (this.EntryAmount * this.TaxPercentageGeneral / 100)* TaxPeriodCount;
                    }
                    else//fixed
                    {
                        _tempReturn = this.TaxFixedGeneral;
                    }
                    //---------------------------------------------------------------------
                    if(this.IsExemption)
                    {
                        if (this.IsMaxExmpAmt)
                        {
                            if (this.IsExmWhichEverLess)
                            {
                                if (this.TaxMaxExmpAmt < _tempReturn)
                                {
                                    _tempReturn = this.TaxMaxExmpAmt;
                                }
                            }
                            else
                            {
                                _tempReturn -= this.TaxMaxExmpAmt;
                                if (_tempReturn < 0)
                                {
                                    _tempReturn = 0;
                                }
                            }
                        }

                        if(this.IsExmBaseOnActual)
                        {
                            if (this.IsExmWhichEverLess)
                            {
                                if (this.ExmBaseOnActual < _tempReturn)
                                {
                                    _tempReturn = this.ExmBaseOnActual;
                                }
                            }
                        }

                        if(this.IsExmBaseOnOtherSlrHd)
                        {
                            if(this.IsExmWhichEverLess)
                            {
                                decimal _osha = (this.EntryAmountOfOtherSHead * this.PercentageExmAmtOtherSlrHd / 100)*this.TaxPeriodCount;
                                if(_osha < _tempReturn)
                                {
                                    _tempReturn = _osha;
                                }
                            }
                        }
                    }//IsExemption
                }//IsTaxable
                _return = _tempReturn;
                return _return;
            }
            catch (Exception ex)
            {
                throw ex;
            }           
        }
    }
}
