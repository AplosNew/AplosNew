using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.HumanResource.Payroll.Tax
{
   public class IncomeTax
    {

        void ProcessIncomeTax(string empids,string month,string year)
        {
            string _effectiveDate = string.Empty;
            string _taxYearId = string.Empty;
            try
            {
                IncomeTaxQuery _itq = new IncomeTaxQuery();
                List<ITaxableEmployee> _emplist = new List<ITaxableEmployee>();
                string[] _empids = empids.Split(',');
                foreach (var emp in _empids)
                {
                    _emplist.Add(new ITaxableEmployee { EmpSystemId = emp.Trim(),Amount=0 });
                }
                //get taxyear+month               
                List<ITPolicy> _iTPolicy = new List<ITPolicy>();

                ///01.earning in the tax year
                ///02.emp and policy wise taxable income in loop
                ///03.emp and policy wise payable tax in loop

                ///01.earning in the tax year
                List<ITaxSalaryHeadEmployee> _ob_list = _itq.getOpeningAmount(empids, _taxYearId);
                List<ITaxSalaryHeadEmployee> _ss_list = _itq.getSalaryStructure(empids, _taxYearId);
                List<ITaxSalaryHeadEmployee> _ps_list = _itq.getProcessedSalary(empids, _taxYearId);
                IncomeTaxCore _itc = new IncomeTaxCore(_ob_list, _ss_list, _ps_list);

                ///02.emp and policy wise taxable income
                for (int i = 0; i < _emplist.Count; i++)
                {
                    string _empid = string.Empty;
                    //yearly taxable income
                    _itc.GetTotalYearlyTaxableIncome(_emplist[i], _iTPolicy);                   
                    //yearly paid
                    _itc.GetTotalYearlyPaid(_emplist[i]);
                    //tax to be paid for the rest period
                    _itc.GetTotalToBePaidTax(_emplist[i]);
                    //---------------------------------------------
                    var final_amount = _emplist[i];
                }//for
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
    }

    public class IncomeTaxQuery
    {
        public List<ITaxSalaryHeadEmployee> getOpeningAmount(string empids, string TaxYearId)
        {
            DataSet ds = null;
            List<ITaxSalaryHeadEmployee> diclist = new List<ITaxSalaryHeadEmployee>();
            try
            {
                _getOpeningAmount(empids, TaxYearId, out ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    diclist = ds.Tables[0].ToList<ITaxSalaryHeadEmployee>();
                }
                return diclist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<ITaxSalaryHeadEmployee> getSalaryStructure(string empids, string TaxYearId)
        {
            DataSet ds = null;
            List<ITaxSalaryHeadEmployee> diclist = new List<ITaxSalaryHeadEmployee>();
            try
            {
                _getSalaryStructure(empids, TaxYearId, out ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    diclist = ds.Tables[0].ToList<ITaxSalaryHeadEmployee>();
                }
                return diclist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<ITaxSalaryHeadEmployee> getProcessedSalary(string empids, string TaxYearId)
        {
            DataSet ds = null;
            List<ITaxSalaryHeadEmployee> diclist = new List<ITaxSalaryHeadEmployee>();
            try
            {
                //string empids, string FromMonth, string FromYear, string ToMonth, string ToYear, string plantid
                //_getProcessedSalary(empids, TaxYearId, out ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    diclist = ds.Tables[0].ToList<ITaxSalaryHeadEmployee>();
                }
                return diclist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void _getOpeningAmount(string empids, string TaxYearId, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT 
                                   [EmpSystemId]      
                                  ,[OpeningTaxableIncomeEarned]
                                  ,[OpeningTaxPaid]
                              FROM [ProfessionalTaxOpeningBalance] where TaxYearId='" + TaxYearId + "' and EmpSystemId in (" + empids + @")";
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
        void _getSalaryStructure(string EmpSystemIds, string EffectiveDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT mm.EmpInfoSystemID,sum(d.EntryAmount) EntryAmount
                                  FROM 
  
                                  (select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
                                  union
                                  select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
                                  )
                                   mm 
                                  inner join (
                                  select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID from (
                                  select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + EffectiveDate + @"'
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
                                    group by mm.EmpInfoSystemID";

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
        void _getProcessedSalary(string empids, string FromMonth, string FromYear, string ToMonth, string ToYear, string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"

                            declare @plantid varchar(10)='" + plantid + @"';
                            declare @monthno int=" + FromMonth + @";
                            declare @yearno int=" + FromYear + @";

                            declare @ToMonth int=" + ToMonth + @";
                            declare @ToYear int=" + ToYear + @";

                            select 

                            TaxableAmount=case 
							
							when

							 sum(case when x.IsFixedTaxGeneral=1 then x.TaxFixedGeneral
                            else x.TaxPercentageGeneral*x.TA_earned/100
                            end)
							>
							 sum(case when x.IsFixedTaxGeneral=1 then x.TaxFixedGeneral
                            else x.TaxPercentageGeneral*x.TA_str/100
                            end)
							then
							 sum(case when x.IsFixedTaxGeneral=1 then x.TaxFixedGeneral
                            else x.TaxPercentageGeneral*x.TA_earned/100
                            end)
							else
							 sum(case when x.IsFixedTaxGeneral=1 then x.TaxFixedGeneral
                            else x.TaxPercentageGeneral*x.TA_str/100
                            end)

							end

                            , sum(case when x.IsFixedTaxGeneral=1 then x.TaxFixedGeneral
                            else x.TaxPercentageGeneral*x.TA_str/100
                            end) as TaxableAmountStr
                            --******************

                            ,x.EmpInfoSystemID,x.TaxPolicyMstID
                            from 
                            (
                            SELECT  h.SalaryHead,c.EntryAmount,c.DefineAmount,c.DisbusmentAmount,c.EmpInfoSystemID,g.TaxPolicyMstID

                            ,TaxableAmount= case when c.EntryAmount =0 then c.DisbusmentAmount
                            else c.EntryAmount
                            end


                             ,TA_earned= c.DisbusmentAmount
							 ,TA_str= c.EntryAmount 

                            ,IsTaxable,	IsFixedTaxGeneral,	TaxFixedGeneral,
                            IsPercentageTaxGeneral,	TaxPercentageGeneral

                            FROM [dbo].[TaxPolicyGeneral] g
                            left join SalaryProcChild c on g.SalaryHeadID=c.SalaryHeadID and c.SlrProcMstSystemID in 
                            (
                            select systemid from SalaryProcMaster                            
                            where (MonthNo>=@monthno and YearNo=@yearno) or (MonthNo<@ToMonth and YearNo=@ToYear)

                            )
                            left join SalaryHead h on h.SalaryHeadID=c.SalaryHeadID
                            inner join EmployeeInformation e on e.systemid=c.EmpInfoSystemID
                            where 
                            g.IsTaxable=1 and e.PlantId=@plantid and e.systemid in (" + empids + @")

                            ) x

                            group by x.EmpInfoSystemID,x.TaxPolicyMstID

                            ";
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

    public class IncomeTaxCore
    {
        List<ITaxSalaryHeadEmployee> _ob = new List<ITaxSalaryHeadEmployee>();
        List<ITaxSalaryHeadEmployee> _ss = new List<ITaxSalaryHeadEmployee>();
        List<ITaxSalaryHeadEmployee> _ps = new List<ITaxSalaryHeadEmployee>();

        public IncomeTaxCore(List<ITaxSalaryHeadEmployee> _ob, List<ITaxSalaryHeadEmployee> _ss, List<ITaxSalaryHeadEmployee> _ps)
        {
            this._ob = _ob;
            this._ss = _ss;
            this._ps = _ps;
        }

        void _getAmount(ITaxSalaryHeadEmployee empIncome, List<ITaxSalaryHeadEmployee> _ob,ref List<ITaxSalaryHeadEmployee> _HeadWiseIncome)
        {
            var eHeadList = _ob.Where(x => x.EmpSystemId == empIncome.EmpSystemId);
            foreach (var eHead in eHeadList)
            {
                foreach (var outOBJ in _HeadWiseIncome)
                {
                    if(outOBJ.SalaryHeadId==eHead.SalaryHeadId)
                    {
                        outOBJ.Amount += eHead.Amount;
                        break;
                    }//if
                }//fore
            }//fore
        }//eof
      

        void GetTotalYearlyIncome(ITaxSalaryHeadEmployee empIncome, List<ITPolicy> _iTPolicy)
        {
            List<ITaxSalaryHeadEmployee> _HeadWiseIncome = new List<ITaxSalaryHeadEmployee>();
            _getAmount(empIncome,_ob,ref _HeadWiseIncome);
            //_getAmount(empIncome,_ss);
            //_getAmount(empIncome,_ps);
        }

        void _getSalaryStructure_Taxable(ITaxableEmployee empIncome)
        {

            
        }
        void _getProcessedSalary_Taxable(ITaxableEmployee empIncome)
        {

        }
        void _getOpeningBalance_Taxable(ITaxableEmployee empIncome)
        {

        }

        public void GetTotalYearlyTaxableIncome(ITaxableEmployee empIncome, List<ITPolicy> _iTPolicy)
        {
            _getOpeningBalance_Taxable(empIncome);
            _getSalaryStructure_Taxable(empIncome);
            _getProcessedSalary_Taxable(empIncome);
        }
        public void GetTotalYearlyPaid(ITaxableEmployee empIncome)
        {
           
        }
        public void GetTotalToBePaidTax(ITaxableEmployee empIncome)
        {

        }
    }

    public class ITPolicy
    {
        public string Id { get; set; }
        public string TaxYearId { get; set; }
           
    }
    public class ITClass
    {
        public string EmpSystemId { get; set; }
        public decimal EarnedAmount { get; set; }
        public decimal StructureAmount { get; set; }
    }
    public class ITaxSalaryHeadEmployee
    {
        private decimal taxableIncome;
        public string EmpSystemId { get; set; }
        public string SalaryHeadId { get; set; }
        public decimal Amount { get; set; }
        public decimal Exemption { get; set; }
        public decimal Rebate { get; set; }
        public decimal TaxableIncome {
            get {
                taxableIncome= Amount- Exemption- Rebate;
                return taxableIncome;
            }
            //get { return name; }
        }

    }
    public class ITaxableEmployee
    {
        public decimal Amount;
        public string EmpSystemId { get; set; }

    }
    //public class ITaxSalaryHeadEmployee: ITaxEmployee
    //{
    //    public string SalaryHeadId { get; set; }      
    //}

  
}
