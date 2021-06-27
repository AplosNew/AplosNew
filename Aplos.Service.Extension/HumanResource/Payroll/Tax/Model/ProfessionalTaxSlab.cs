using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.Payroll.Tax.Model
{
    public class ProfessionalTaxSlab
    {
        public string Id { get; set; }
        public string TaxPolicyMasterId { get; set; }
        public decimal YearlyMinValue { get; set; }
        public decimal YearlyMaxValue { get; set; }
        public decimal YearlyTaxAmount { get; set; }
        public decimal MonthlyMinValue { get; set; }
        public decimal MonthlyMaxValue { get; set; }
        public decimal MonthlyTaxAmount { get; set; }
        public decimal AdjustingAmount { get; set; }
        public int MonthOfAdjustment { get; set; }
        public decimal SeqenceNo { get; set; }
        public string PlantID { get; set; }

        public decimal getMonthlyTaxOnEarnedAmount(int monthno, ProfessionalTaxSlab yearlySlabValue, ProfessionalTaxEmployeeWiseMonthly Prev_PTax)
        {
            decimal _monthly_amount = 0;
            try
            {
                if (this.MonthOfAdjustment == monthno)
                {
                    //if (this.AdjustingAmount > 0)
                    //{
                    //    _monthly_amount = this.AdjustingAmount;
                    //}
                    //else
                    //{
                        decimal YearlyLimit = 0;
                        if (yearlySlabValue == null)
                        {
                            YearlyLimit = 0;
                        }
                        else
                        {
                            YearlyLimit = yearlySlabValue.YearlyTaxAmount;
                        }
                        if (Prev_PTax != null)
                        {
                            _monthly_amount = YearlyLimit - Prev_PTax.EarnedAmount;
                        }
                        else
                        {
                            _monthly_amount = YearlyLimit;
                        }
                    //}
                }
                else
                {
                    _monthly_amount = this.MonthlyTaxAmount;
                }
                return _monthly_amount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public decimal getMonthlyTaxOnStructureAmount(int monthno, ProfessionalTaxEmployeeWiseMonthly Prev_PTax)
        {
            decimal _monthly_amount = 0;
            try
            {
                if (this.MonthOfAdjustment == monthno)
                {
                    //if (this.AdjustingAmount > 0)
                    //{
                    //    _monthly_amount = this.AdjustingAmount;
                    //}
                    //else
                    //{
                        if (Prev_PTax != null)
                        {
                            _monthly_amount = this.YearlyTaxAmount - Prev_PTax.StructureAmount;
                        }
                        else
                        {
                            _monthly_amount = this.YearlyTaxAmount;
                        }
                    //}
                }
                else
                {
                    _monthly_amount = this.MonthlyTaxAmount;
                }
                return _monthly_amount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class EmpSalaryStructure
    {
        public string EmpInfoSystemID { get; set; }
        //public string SystemID { get; set; }
        //public string SalaryHeadID { get; set; }
        public decimal EntryAmount { get; set; }
    }

}
