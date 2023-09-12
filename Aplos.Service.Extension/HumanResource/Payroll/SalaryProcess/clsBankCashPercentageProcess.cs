using Library.Crosscutting.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.HumanResource.Payroll.SalaryProcess
{
   public class clsBankCashPercentageProcess
    {
        
        public void DeleteEmployeeWiseBankCashAmount(string _fromDate, string _empids)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {                
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM EmployeeWiseBankCashAmount  
                                                where YearNo=Year('" + _fromDate+ @"') and MonthNo=Month('" + _fromDate + @"')
                                                and empsystemid in (" + _empids + ")", true, "1");

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {                
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                }
                throw (ex);
            }
            finally
            {               
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetPaymentModeWiseEmp(string _empids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select PaymentMode,systemid,employeeCode from EmployeeInformation where 
                                systemid in ("+_empids+ ") and PaymentMode='Bank'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public bool GetBankCashPercentageSetting(string empid, DataSet dsPmode)
        {
            bool result = false;
            try
            {
                DataView dv = new DataView(dsPmode.Tables[0]);
                dv.RowFilter = "systemid='" + empid + "'";
                if(dv.Count>0)
                {
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
               
            }
        }//End Function

        public void GetBankCashPercentageSetting(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select HeadLabel,FormulaDes,FormulaDesID from BankCashPercentageSettinng where PlantId='" + PlantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmployeeWiseBankCashAmount(string _empids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select * from EmployeeWiseBankCashAmount where  empsystemid in ("+ _empids + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void Save(params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();//
                //objCon.ExecuteNonQueryWrapper("DELETE FROM SalaryProcChild WHERE MonthNo = " + intMonthNo + " AND YearNo = " + intYearNo + " AND IsDisbursed = 0 AND (" + strEmp + ")", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception ex2)
                {
                    throw ex2;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetFormula(DataSet dsSetting, out string _bank_formula, out string _cash_formula)
        {
            _bank_formula = string.Empty;
            _cash_formula = string.Empty;
            try
            {
                for (int i = 0; i < dsSetting.Tables[0].Rows.Count; i++)
                {
                    if (dsSetting.Tables[0].Rows[i]["HeadLabel"].ToString().ToUpper() == "BANK")
                    {
                        _bank_formula = dsSetting.Tables[0].Rows[i]["FormulaDesID"].ToString();
                    }

                    if (dsSetting.Tables[0].Rows[i]["HeadLabel"].ToString().ToUpper() == "CASH")
                    {
                        _cash_formula = dsSetting.Tables[0].Rows[i]["FormulaDesID"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
    }
}


