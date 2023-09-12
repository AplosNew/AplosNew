using Library.Service.Extension.Payroll.Tax.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace Library.Service.Extension.Payroll.Tax
{
    public class ProfessionalTax
    {
        public void ProcessPT(string empids,string _plantid, int _monthno, int _yearno)
        {
            DataSet dsTaxPolicy = null;
            string _taxPolicyId = string.Empty;
            string _GenderID = string.Empty;
            string _companyId = string.Empty;
            string _IsGenderSpecific = string.Empty;
            try
            {
                string effective_date = "01-" + _getMonthName(_monthno) + "-" + _yearno;
                _getAllPTPolicy(effective_date,_plantid, out dsTaxPolicy);
                for (int i = 0; i < dsTaxPolicy.Tables[0].Rows.Count; i++)
                {
                    _taxPolicyId = dsTaxPolicy.Tables[0].Rows[i]["SystemId"].ToString();
                    _companyId = dsTaxPolicy.Tables[0].Rows[i]["companyid"].ToString();
                    _GenderID = dsTaxPolicy.Tables[0].Rows[i]["GenderID"].ToString();
                    _IsGenderSpecific = dsTaxPolicy.Tables[0].Rows[i]["IsGenderSpecific"].ToString();
                    if (Convert.ToBoolean(_IsGenderSpecific) == false)
                    {
                        ProcessPT(empids,_plantid, _companyId, _monthno, _yearno, _taxPolicyId);
                        break;//non-gender will b only one
                    }
                    else
                    {
                        ProcessPT(empids,_plantid, _companyId, _monthno, _yearno, _taxPolicyId, _GenderID);
                    }
                }//for
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void ProcessPT(string empids,string _plantid, string _companyId, int _monthno, int _yearno,string _taxPolicyId,string _Gender = "All")
        {
            #region Declaration
            //_monthno = 8;
            //_yearno = 2020;
                DataSet dsOB;
            string _tax_year_id = string.Empty;
            int _Month_tobe_come = 0;
            DataSet dsTaxYear = null;
            DataSet dsTI_emplist = null;
            DataSet dsPTPolicy = null;
            DataSet dsMonthYear;
            DataSet dsYearlyIncome = null;
            DataSet dsDeductedAmount = null;
            //-------------------------------------
            DataSet dsSalaryStructure = null;
            DataSet dsTax_Save = null;
            DataSet dsTaxInSalary_Save = null;
            string _empids = string.Empty;
            string _empids_ss = string.Empty;//for salary structure
            #endregion
           // bool IsMale = false;
            try
            {                
                string _effective_date = "01-"+ _getMonthName(_monthno) + "-"+_yearno;
                List<ProfessionalTaxEmployeeWiseMonthly> _dicEmpPT = new List<ProfessionalTaxEmployeeWiseMonthly>();

                #region ds

                //gt tax policy                
                _getProfTaxPolicy(_taxPolicyId, out dsPTPolicy);     
                
                List<ProfessionalTaxSlab> dicPolicySlab = new List<ProfessionalTaxSlab>();
                if (dsPTPolicy.Tables[0].Rows.Count > 0)
                {
                    dicPolicySlab = dsPTPolicy.Tables[0].ToList<ProfessionalTaxSlab>();
                }

                //get emp wise monthly taxable income
                if (_Gender == "All")
                {
                    _getMonthlyTaxableIncome(empids,_taxPolicyId, _plantid, _monthno, _yearno, out dsTI_emplist);
                }
                else
                {
                    _getMonthlyTaxableIncomeWithGender(empids,_Gender, _taxPolicyId, _plantid, _monthno, _yearno, out dsTI_emplist); 
                }

                List<MonthlyTaxableIncome> dicEmp_MonthlyTaxableIncome = new List<MonthlyTaxableIncome>();
                if (dsTI_emplist.Tables[0].Rows.Count > 0)
                {
                    dicEmp_MonthlyTaxableIncome = dsTI_emplist.Tables[0].ToList<MonthlyTaxableIncome>();
                }
               
                _getEmpIds_for_ss(dicEmp_MonthlyTaxableIncome, out _empids_ss);
                _getTaxYear(_yearno, _monthno, out dsTaxYear);
                if (dsTaxYear.Tables[0].Rows.Count > 0)
                {                   
                    _tax_year_id = dsTaxYear.Tables[0].Rows[0]["TaxYearId"].ToString();
                    _Month_tobe_come=Convert.ToInt32(dsTaxYear.Tables[0].Rows[0]["MonthToCome"].ToString());
                }

                if (_tax_year_id.Length == 0)
                {
                    throw new Exception("Professional Tax Slab is not defined");
                }

                //get emp wise ss to calculate salary amount of future months to come                
                _getSalaryStructure(_empids_ss, _effective_date, out dsSalaryStructure);
                
                
                List<EmpSalaryStructure> dicSalaryStructure = new List<EmpSalaryStructure>();
                if (dsSalaryStructure.Tables[0].Rows.Count > 0)
                {
                    dicSalaryStructure = dsSalaryStructure.Tables[0].ToList<EmpSalaryStructure>();
                }


               
                //get opening balance                
                _getOpeningAmount(empids,_tax_year_id, out dsOB);
                List<OBPTax> dicOB = new List<OBPTax>();
                if (dsOB.Tables[0].Rows.Count > 0)
                {
                    dicOB = dsOB.Tables[0].ToList<OBPTax>();
                }

                _getMonthYear(_tax_year_id, _companyId, out dsMonthYear);
                //get yearly income from processed salary
                _getYearlyIncome(_empids_ss, dsMonthYear, _plantid, out dsYearlyIncome);//TBD
                List<MonthlyTaxableIncome> dicYearlyIncome = new List<MonthlyTaxableIncome>();
                bool IsOk = false;
                List<ProfessionalTaxEmployeeWiseMonthly> dicYearlyPaid = new List<ProfessionalTaxEmployeeWiseMonthly>();

                if (dsYearlyIncome != null)
                {
                    if (dsYearlyIncome.Tables[0].Rows.Count > 0)
                    {
                        dicYearlyIncome = dsYearlyIncome.Tables[0].ToList<MonthlyTaxableIncome>();
                    }

                    //get total yearly income (earned + structure)
                    _totalIncome(dicYearlyIncome, dicSalaryStructure, dicOB, _Month_tobe_come);

                    //get already month wise deducted tax amount
                    _getDeductedAmount(empids, _tax_year_id, _monthno, out dsDeductedAmount);
                    if (dsDeductedAmount.Tables[0].Rows.Count > 0)
                    {
                        dicYearlyPaid = dsDeductedAmount.Tables[0].ToList<ProfessionalTaxEmployeeWiseMonthly>();
                    }
                    _totalPaid(dicYearlyPaid, dicOB);
                    IsOk = true;
                }

                #endregion

                if (IsOk)
                {
                    #region Loop
                    //get emp wise PTax
                    for (int i = 0; i < dicEmp_MonthlyTaxableIncome.Count; i++)
                    {
                        //-----get yearly slab
                        ProfessionalTaxSlab _foundYearlySlab = _getYearlySlabAmount(dicEmp_MonthlyTaxableIncome[i], dicYearlyIncome, dicPolicySlab);
                        //-------------get monthly slab /earned
                        ProfessionalTaxSlab _policyMonthlyEarned = _getMonthlySlabAmount(dicEmp_MonthlyTaxableIncome[i], dicPolicySlab, true);
                        //-------------get yearly already paid tax
                        ProfessionalTaxEmployeeWiseMonthly _yearlyPaid = _getYearlyPaid(dicEmp_MonthlyTaxableIncome[i], dicYearlyPaid);


                        //-------------max amount as per monthly slab
                        decimal _Amount_earned = _policyMonthlyEarned.getMonthlyTaxOnEarnedAmount(_monthno, _foundYearlySlab, _yearlyPaid);
                        //------------Structure amount as per yearly slab
                        decimal _Amount_structure = _foundYearlySlab.getMonthlyTaxOnStructureAmount(_monthno, _yearlyPaid);
                        //-------------set value 
                        if (_Amount_earned > 0)
                        {
                            _dicEmpPT.Add(new ProfessionalTaxEmployeeWiseMonthly { EarnedAmount = _Amount_earned, StructureAmount = _Amount_structure, EmpSystemId = dicEmp_MonthlyTaxableIncome[i].EmpInfoSystemID });
                        }
                    }//for  
                    #endregion

                    _getEmpIds(_dicEmpPT, out _empids);

                    _saveMonthlyTax(_dicEmpPT, _tax_year_id, _monthno, _yearno, _empids, out dsTax_Save);
                    _saveTaxInSalary(_dicEmpPT, _monthno, _yearno, _empids, out dsTaxInSalary_Save);
                    _save(dsTax_Save, dsTaxInSalary_Save); 
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Save
        
        void _saveMonthlyTax(List<ProfessionalTaxEmployeeWiseMonthly> list, string _taxYearId, int _month, int _year, string _empids,out DataSet dsTax)
        {
            dsTax = null;
            string _userName = string.Empty;
            try
            {
                _getTaxMonthly_Save(_taxYearId,_month,_year, _empids,out dsTax);
                if (list.Count() > 0)
                {
                    string sID = string.Empty;
                    clsGenID objGenID = new clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "PRO_TAX_DE_MON", out sID);
                    int _count = 0;
                    foreach (var item in list)
                    {
                        DataView dvTax = new DataView(dsTax.Tables[0]);
                        dvTax.RowFilter = "EmpSystemId='" + item.EmpSystemId + "'";
                        _count++;
                        if (dvTax.Count == 0)
                        {
                           
                            DataRow dr = dsTax.Tables[0].NewRow();
                            dr["Id"] = "PTSAL-" + sID + "-" + _count;

                            dr["EmpSystemId"] = item.EmpSystemId;
                            dr["EarnedAmount"] = item.EarnedAmount;
                            dr["StructureAmount"] = item.StructureAmount;
                            dr["MonthNo"] = _month;
                            dr["YearNo"] = _year;
                            dr["TaxYearId"] = _taxYearId;

                            dr["AddedBy"] = _userName;
                            dr["DateAdded"] = System.DateTime.Now.ToString();
                            dsTax.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dvTax[0].Row;
                            dr.BeginEdit();

                            dr["EmpSystemId"] = item.EmpSystemId;
                            dr["EarnedAmount"] = item.EarnedAmount;
                            dr["StructureAmount"] = item.StructureAmount;
                            dr["MonthNo"] = _month;
                            dr["YearNo"] = _year;
                            dr["TaxYearId"] = _taxYearId;

                            dr["UpdatedBy"] = _userName;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();
                        }
                        dvTax.RowFilter = null;
                    }//for
                }//if
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _saveTaxInSalary(List<ProfessionalTaxEmployeeWiseMonthly> list, int _month, int _year,string _empids, out DataSet dsTaxInSalary)
        {
            dsTaxInSalary = null;
            string _userName = string.Empty;
            //_empids = string.Empty;
            string _childids = string.Empty;
            DataSet dsSalaryHead;
            try
            {
                _getSalaryHead(out dsSalaryHead);
                
                // _getTaxInSalary(_empids, _month, -_year, out dsTaxInSalary);


                _getTaxInSalary(_empids, _month, _year, out dsTaxInSalary);
                _getChildids(dsTaxInSalary, out _childids);
                _getTaxInSalary_Save(_childids, out dsTaxInSalary);
                if (list.Count() > 0)
                {
                    string sID = string.Empty;
                    clsGenID objGenID = new clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "TAX_IN_SAL", out sID);
                    int _count = 0;
                    foreach (var item in list)
                    {
                        //get info from child
                        ProcessedChildInfo pci = new ProcessedChildInfo();
                        DataView dvChild = new DataView(dsTaxInSalary.Tables[0]);
                        dvChild.RowFilter = "EmpInfoSystemID='" + item.EmpSystemId + "' ";
                        if (dvChild.Count > 0)
                        {
                            _getSalaryInfo(dvChild, pci);

                            for (int h = 0; h < dsSalaryHead.Tables[0].Rows.Count; h++)
                            {
                                string _sh = dsSalaryHead.Tables[0].Rows[h]["SalaryHeadId"].ToString();
                                string _HeadCategory = dsSalaryHead.Tables[0].Rows[h]["HeadCategory"].ToString();
                                DataView dvTax = new DataView(dsTaxInSalary.Tables[0]);
                                dvTax.RowFilter = "EmpInfoSystemID='" + item.EmpSystemId + "' and SalaryHeadId='" + _sh + "'";

                                if (dvTax.Count == 0)
                                {
                                    _count++;
                                    DataRow dr = dsTaxInSalary.Tables[0].NewRow();
                                    dr["SystemID"] = "TC" + sID + "_" + _count;

                                    dr["EmpInfoSystemID"] = item.EmpSystemId;
                                    dr["SalaryHeadID"] = _sh;

                                    if (_HeadCategory == "ProfessionalTax")
                                    {
                                        dr["SlrProcMstSystemID"] = pci.SlrProcMstSystemID;
                                        dr["SalaryID"] = pci.SalaryID;
                                        dr["GroupID"] = pci.GroupID;
                                        dr["PlantID"] = pci.PlantID;
                                        dr["PayAbleShSystemID"] = pci.PayAbleShSystemID;

                                        dr["EntryCurrencyID"] = pci.EntryCurrencyID;
                                        dr["EntryAmount"] = 0;
                                        dr["DefineCurrencyID"] = pci.DefineCurrencyID;

                                        dr["DefineAmount"] = 0;
                                        dr["DisbusmentCurrencyID"] = pci.DisbusmentCurrencyID;
                                        dr["DisbusmentAmount"] = item.EarnedAmount * (-1);

                                        dr["AcltExcDisbSlrHDID"] = pci.AcltExcDisbSlrHDID;
                                        dr["AcltExcDisbSlrHDAmt"] = pci.AcltExcDisbSlrHDAmt;
                                    }

                                    dr["IsNetPayEffect"] = 0;
                                    dr["IsApproved"] = 0;
                                    dr["IsDisbursed"] = 0;

                                    dr["AddedBy"] = _userName;
                                    dr["DateAdded"] = System.DateTime.Now.ToString();
                                    dsTaxInSalary.Tables[0].Rows.Add(dr);
                                }
                                else
                                {

                                    DataRow dr = dvTax[0].Row;
                                    dr.BeginEdit();

                                    dr["EmpInfoSystemID"] = item.EmpSystemId;
                                    dr["SalaryHeadID"] = _sh;

                                    if (_HeadCategory == "ProfessionalTax")
                                    {
                                        dr["SlrProcMstSystemID"] = pci.SlrProcMstSystemID;
                                        dr["SalaryID"] = pci.SalaryID;
                                        dr["GroupID"] = pci.GroupID;
                                        dr["PlantID"] = pci.PlantID;
                                        dr["PayAbleShSystemID"] = pci.PayAbleShSystemID;

                                        dr["EntryCurrencyID"] = pci.EntryCurrencyID;
                                        dr["EntryAmount"] = 0;
                                        dr["DefineCurrencyID"] = pci.DefineCurrencyID;

                                        dr["DefineAmount"] = 0;
                                        dr["DisbusmentCurrencyID"] = pci.DisbusmentCurrencyID;
                                        dr["DisbusmentAmount"] = item.EarnedAmount*(-1);

                                        dr["AcltExcDisbSlrHDID"] = pci.AcltExcDisbSlrHDID;
                                        dr["AcltExcDisbSlrHDAmt"] = pci.AcltExcDisbSlrHDAmt;
                                    }
                                    else
                                    {
                                        dr["DisbusmentAmount"] =Convert.ToDecimal(dr["DisbusmentAmount"]) - item.EarnedAmount;

                                        if (clsStaticInfo.dbl(dr["DisbusmentAmount"].ToString()) < 0)
                                            dr["DisbusmentAmount"] = 0;
                                    }

                                    dr["UpdatedBy"] = _userName;
                                    dr["DateUpdated"] = System.DateTime.Now.ToString();
                                    dr.EndEdit();
                                }
                                dvTax.RowFilter = null;
                            }
                        }//net pay found
                    }//for
                }//if
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Func Utility
        void _totalIncome(List<MonthlyTaxableIncome> listEarned, List<EmpSalaryStructure> listStructure, List<OBPTax> listOB, int _monthToCome)
        {
            try
            {
                foreach (var item in listEarned)
                {
                    //Struc
                    var _structureOb = listStructure.Where(r => r.EmpInfoSystemID == item.EmpInfoSystemID).FirstOrDefault();
                    if(_structureOb!=null)
                    {
                    item.TaxableAmount += _structureOb.EntryAmount * _monthToCome;
                    item.TaxableAmountStr += _structureOb.EntryAmount * _monthToCome;
                    }
                    //OB
                    var _ob = listOB.Where(r => r.EmpSystemId == item.EmpInfoSystemID).FirstOrDefault();
                    if (_ob != null)
                    {
                        item.TaxableAmount += _ob.OpeningTaxableIncomeEarned;
                        item.TaxableAmountStr += _ob.OpeningTaxableIncomeEarned;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _totalPaid(List<ProfessionalTaxEmployeeWiseMonthly> listEarned, List<OBPTax> listOB)
        {
            try
            {
                foreach (var item in listEarned)
                {
                    //OB
                    var _ob = listOB.Where(r => r.EmpSystemId == item.EmpSystemId).FirstOrDefault();
                    if (_ob != null)
                    {
                        item.EarnedAmount += _ob.OpeningTaxPaid;
                        item.StructureAmount += _ob.OpeningTaxPaid;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string _getMonthName(int _month)
        {
            string _month_name = string.Empty;
            try
            {
                switch (_month)
                {
                    case 1:
                        _month_name = "Jan";
                        break;
                    case 2:
                        _month_name = "Feb";
                        break;
                    case 3:
                        _month_name = "Mar";
                        break;
                    case 4:
                        _month_name = "Apr";
                        break;

                    case 5:
                        _month_name = "May";
                        break;
                    case 6:
                        _month_name = "Jun";
                        break;
                    case 7:
                        _month_name = "Jul";
                        break;
                    case 8:
                        _month_name = "Aug";
                        break;

                    case 9:
                        _month_name = "Sep";
                        break;
                    case 10:
                        _month_name = "Oct";
                        break;
                    case 11:
                        _month_name = "Nov";
                        break;
                    case 12:
                        _month_name = "Dec";
                        break;

                   default:
                        _month_name = "Jan";
                        break;
                }
                return _month_name;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _getSalaryInfo(DataView dv, ProcessedChildInfo pci)
        {
            try
            {
                if (dv.Count > 0)
                {
                    pci.AcltExcDisbSlrHDAmt = Convert.ToDecimal(dv[0]["AcltExcDisbSlrHDAmt"].ToString());
                    pci.AcltExcDisbSlrHDID = dv[0]["AcltExcDisbSlrHDID"].ToString();
                    pci.DefineAmount = Convert.ToDecimal(dv[0]["DefineAmount"].ToString());
                    pci.DefineCurrencyID = dv[0]["DefineCurrencyID"].ToString();

                    pci.DisbusmentAmount = Convert.ToDecimal(dv[0]["DisbusmentAmount"].ToString());
                    pci.DisbusmentCurrencyID = dv[0]["DisbusmentCurrencyID"].ToString();
                    pci.EntryAmount = Convert.ToDecimal(dv[0]["EntryAmount"].ToString());
                    pci.EntryCurrencyID = dv[0]["EntryCurrencyID"].ToString();

                    pci.GroupID = dv[0]["GroupID"].ToString();
                    pci.PlantID = dv[0]["PlantID"].ToString();
                    pci.PayAbleShSystemID = dv[0]["PayAbleShSystemID"].ToString();
                    pci.SalaryID = dv[0]["SalaryID"].ToString();
                    pci.SlrProcMstSystemID = dv[0]["SlrProcMstSystemID"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _getEmpIds(List<ProfessionalTaxEmployeeWiseMonthly> list, out string empids)
        {
            empids = "''";
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    empids += ",'" + list[i].EmpSystemId + "'";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _getEmpIds_for_ss(List<MonthlyTaxableIncome> list, out string empids)
        {
            empids = "''";
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    empids += ",'" + list[i].EmpInfoSystemID + "'";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _getChildids(DataSet ds, out string childids)
        {
            childids = "''";
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    childids += ",'" + ds.Tables[0].Rows[i]["SystemId"] + "'";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        ProfessionalTaxEmployeeWiseMonthly _getYearlyPaid(MonthlyTaxableIncome mti, List<ProfessionalTaxEmployeeWiseMonthly> dicPTS)
        {
            //decimal mti = Convert.ToDecimal(yearlyTaxableIncome.TaxableAmount);
            var ob= dicPTS.Where(r => r.EmpSystemId == mti.EmpInfoSystemID).FirstOrDefault();
            if (ob == null)
            {
                ob = new ProfessionalTaxEmployeeWiseMonthly();
            }
            return ob;
        }
        ProfessionalTaxSlab _getYearlySlabAmount(MonthlyTaxableIncome empinfo, List<MonthlyTaxableIncome> YearlyIncomeAllEmp, List<ProfessionalTaxSlab> dicPTS)
        {
            ProfessionalTaxSlab ob = null;
               //get emp wise yearly income
               var _YearlyTaxableIncome = YearlyIncomeAllEmp.Where(r => r.EmpInfoSystemID == empinfo.EmpInfoSystemID).FirstOrDefault();
            if (_YearlyTaxableIncome != null)
            {
                decimal yti = _YearlyTaxableIncome.TaxableAmount;
                //get income wise slab
                ob = dicPTS.Where(r => r.TaxPolicyMasterId == empinfo.TaxPolicyMstID && r.YearlyMinValue <= yti && r.YearlyMaxValue >= yti).FirstOrDefault();
            }

            if(ob == null)
            {
                ob = new ProfessionalTaxSlab();
            }
            return ob;
        }
        ProfessionalTaxSlab _getMonthlySlabAmount(MonthlyTaxableIncome monthlyTaxableIncome, List<ProfessionalTaxSlab> dicPTS,bool IsEarned)
        {
            decimal mti = 0;

            if(IsEarned)
            {
            mti=Convert.ToDecimal(monthlyTaxableIncome.TaxableAmount);
            }
            else
            {
                mti = Convert.ToDecimal(monthlyTaxableIncome.TaxableAmountStr);
            }
            var ob= dicPTS.Where(r => r.TaxPolicyMasterId == monthlyTaxableIncome.TaxPolicyMstID && r.MonthlyMinValue <= mti && r.MonthlyMaxValue >= mti).FirstOrDefault();

            if (ob == null)
            {
                ob = new ProfessionalTaxSlab();
            }
            return ob;
        }
        void _getYearlyIncome(string empids,DataSet dsYearMonth, string _plantid, out DataSet dsYearlyIncome)
        {
            dsYearlyIncome = null;
            try
            {
                if (dsYearMonth.Tables[0].Rows.Count > 1)
                {
                    string _fromMonth = dsYearMonth.Tables[0].Rows[0]["MonthNo"].ToString();
                    string _fromYear = dsYearMonth.Tables[0].Rows[0]["YearNo"].ToString();

                    string _toMonth = dsYearMonth.Tables[0].Rows[1]["MonthNo"].ToString();
                    string _toYear = dsYearMonth.Tables[0].Rows[1]["YearNo"].ToString();

                    _getYearlySalary(empids,_fromMonth, _fromYear, _toMonth, _toYear, _plantid, out dsYearlyIncome);
                }
                // _getYearlySalary();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _save(params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
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
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function 
        #endregion

        #region Func SQL
        void _getMonthlyTaxableIncome(string empids, string taxpolicyid, string _plantid, int _monthno, int _yearno, out System.Data.DataSet dsLocal)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
              var   xstrSQL = @"declare @plantid varchar(10)='"+_plantid+@"';
                            declare @monthno int="+_monthno+@";
                            declare @yearno int="+_yearno+ @";

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
                            left join SalaryProcChild c on g.SalaryHeadID=c.SalaryHeadID and c.SlrProcMstSystemID
                                                            in (select systemid from SalaryProcMaster where MonthNo=@monthno and YearNo=@yearno)
                            left join SalaryHead h on h.SalaryHeadID=c.SalaryHeadID
                            inner join EmployeeInformation e on e.systemid=c.EmpInfoSystemID
                            where e.systemid in ("+empids+@") and
                            g.IsTaxable=1 and e.PlantId=@plantid and g.TaxPolicyMstID='" + taxpolicyid+@"'
                            ) x
                            group by x.EmpInfoSystemID,x.TaxPolicyMstID";

                strSQL = @"declare @plantid varchar(10)='" + _plantid + @"';
                            declare @monthno int=" + _monthno + @";
                            declare @yearno int=" + _yearno + @";

							select 
							TaxableAmount=case 
							when CalculationBasis= 'Minimum Amount' and TA_E<TA_S then TA_E 
							when CalculationBasis= 'Minimum Amount' and TA_S<TA_E then TA_S
							
							when CalculationBasis= 'Maximum Amount' and TA_E>TA_S then TA_E
							when CalculationBasis= 'Maximum Amount' and TA_S>TA_E then TA_S

							when CalculationBasis= 'Structured Amount' then TA_S
							when CalculationBasis= 'Structured Amount' then TA_E
							else TA_E  end
							,EmpInfoSystemID
							,TaxPolicyMstID
							,TaxableAmountStr
							
							from
							(

                            select 
							CalculationBasis

							,TA_E=sum(case when x.IsFixedTaxGeneral=1 then x.TaxFixedGeneral
									else x.TaxPercentageGeneral*x.TA_earned/100
									end)

									,TA_S=sum(case when x.IsFixedTaxGeneral=1 then x.TaxFixedGeneral
									else x.TaxPercentageGeneral*x.TA_str/100
									end)

							,TaxableAmount=
									case 
							
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


                            ,x.EmpInfoSystemID,x.TaxPolicyMstID
                            from 
                            (
                            SELECT  m.CalculationBasis,h.SalaryHead,c.EntryAmount,c.DefineAmount,c.DisbusmentAmount,c.EmpInfoSystemID,g.TaxPolicyMstID

                            ,TaxableAmount= case when c.EntryAmount =0 then c.DisbusmentAmount
                            else c.EntryAmount
                            end

							 ,TA_earned= c.DisbusmentAmount
							 ,TA_str= c.EntryAmount

                            ,IsTaxable,	IsFixedTaxGeneral,	TaxFixedGeneral,
                            IsPercentageTaxGeneral,	TaxPercentageGeneral

                            FROM [dbo].[TaxPolicyGeneral] g
							left join TaxPolicyMaster m on g.TaxPolicyMstID=m.systemid
                            left join SalaryProcChild c on g.SalaryHeadID=c.SalaryHeadID and c.SlrProcMstSystemID
                                                            in (select systemid from SalaryProcMaster where MonthNo=@monthno and YearNo=@yearno)
                            left join SalaryHead h on h.SalaryHeadID=c.SalaryHeadID
                            inner join EmployeeInformation e on e.systemid=c.EmpInfoSystemID
                            where e.systemid in (" + empids + @") and
                            g.IsTaxable=1 and e.PlantId=@plantid and g.TaxPolicyMstID='" + taxpolicyid + @"'
                            ) x
                            group by x.EmpInfoSystemID,x.TaxPolicyMstID,x.CalculationBasis
							) xx";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsLocal, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        void _getMonthlyTaxableIncomeWithGender(string empids, string Gender, string taxpolicyid, string _plantid, int _monthno, int _yearno, out System.Data.DataSet dsLocal)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"declare @plantid varchar(10)='" + _plantid + @"';
                            declare @monthno int=" + _monthno + @";
                            declare @yearno int=" + _yearno + @";

                           select 
							TaxableAmount=case 
							when CalculationBasis= 'Minimum Amount' and TA_E<TA_S then TA_E 
							when CalculationBasis= 'Minimum Amount' and TA_S<TA_E then TA_S
							
							when CalculationBasis= 'Maximum Amount' and TA_E>TA_S then TA_E
							when CalculationBasis= 'Maximum Amount' and TA_S>TA_E then TA_S

							when CalculationBasis= 'Structured Amount' then TA_S
							when CalculationBasis= 'Structured Amount' then TA_E
							else TA_E  end
							,EmpInfoSystemID
							,TaxPolicyMstID
							,TaxableAmountStr
							
							from
							(

                            select 
							CalculationBasis

							,TA_E=sum(case when x.IsFixedTaxGeneral=1 then x.TaxFixedGeneral
									else x.TaxPercentageGeneral*x.TA_earned/100
									end)

									,TA_S=sum(case when x.IsFixedTaxGeneral=1 then x.TaxFixedGeneral
									else x.TaxPercentageGeneral*x.TA_str/100
									end)
							,TaxableAmount=case 
							
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


                            ,x.EmpInfoSystemID,x.TaxPolicyMstID
                            from 
                            (
                            SELECT  m.CalculationBasis,h.SalaryHead,c.EntryAmount,c.DefineAmount,c.DisbusmentAmount,c.EmpInfoSystemID,g.TaxPolicyMstID

                            ,TaxableAmount= case when c.EntryAmount =0 then c.DisbusmentAmount
                            else c.EntryAmount
                            end

							 ,TA_earned= c.DisbusmentAmount
							 ,TA_str= c.EntryAmount

                            ,IsTaxable,	IsFixedTaxGeneral,	TaxFixedGeneral,
                            IsPercentageTaxGeneral,	TaxPercentageGeneral

                            FROM [dbo].[TaxPolicyGeneral] g
                            left join TaxPolicyMaster m on g.TaxPolicyMstID=m.systemid
                            left join SalaryProcChild c on g.SalaryHeadID=c.SalaryHeadID and c.SlrProcMstSystemID
                                                            in (select systemid from SalaryProcMaster where MonthNo=@monthno and YearNo=@yearno)
                            left join SalaryHead h on h.SalaryHeadID=c.SalaryHeadID
                            inner join EmployeeInformation e on e.systemid=c.EmpInfoSystemID and e.GenderID='" + Gender + @"'
                            where e.systemid in ("+empids+@") and
                            g.IsTaxable=1 and e.PlantId=@plantid and g.systemid='" + taxpolicyid + @"'
                            ) x
                            group by x.EmpInfoSystemID,x.TaxPolicyMstID,x.CalculationBasis ) xx"; 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsLocal, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        void _getProfTaxPolicy(string _taxPolicyId, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT m.TaxYearID,s.*  FROM [dbo].[TaxPolicyMaster] m
                                left join[dbo].[TaxSlabDefineProfessional] s on m.SystemID=s.taxPolicyMasterId
                                where m.systemid='" + _taxPolicyId + @"'";
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
        void _getAllPTPolicy(string pDate, string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT m.*,pc.companyid  FROM [dbo].[TaxPolicyMaster] m 
                //                        left join TaxPolicyPlantWise p on m.SystemID=p.taxpolicyid
                //                        left join org.plant pc on pc.id=p.plantid
                //                        where p.plantid='" + plantid + "'";
                strSQL = @"SELECT m.*,pc.companyid  FROM [dbo].[TaxPolicyMaster] m 
                                        left join TaxPolicyPlantWise p on m.SystemID=p.taxpolicyid
                                        left join org.plant pc on pc.id=p.plantid
										left join scs.TaxYear t on t.id=m.TaxYearID
                                        where p.plantid='" + plantid + @"'
										and '" + pDate + "' between t.StartDate and t.EndDate";
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

        void x_getAllPTPolicy(string plantid,out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT m.*,pc.companyid  FROM [dbo].[TaxPolicyMaster] m 
                                        left join TaxPolicyPlantWise p on m.SystemID=p.taxpolicyid
                                        left join org.plant pc on pc.id=p.plantid
                                        where p.plantid='" + plantid + "'";
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


        void _getTaxYear(int _year,int _month, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select m.TaxYearId,12-m.PeriodNo MonthToCome
                                    from scs.TaxYear y 
                                    inner join scs.TaxYearPeriod m on y.id=m.TaxYearId
                                    where 
                                     "+_month+@" between datepart(month,m.StartDate) and DATEPART(month,m.EndDate)
                                    and "+_year+@" between datepart(year,m.StartDate) and DATEPART(year,m.EndDate)";
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

        void _getMonthYear(string TaxYearId, string CompanyId, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select TaxYearId,PeriodNo,PeriodName
                                ,DATEPART(MONTH,p.StartDate) MonthNo
                                ,DATEPART(YEAR,p.StartDate) YearNo
                                from scs.TaxYearPeriod p
                                inner join (--1
                                select * from scs.CompanyTaxYearPeriod 
                                where  CompanyTaxYearId in
                                (
                                select id from scs.CompanyTaxYear  where TaxYearId='" + TaxYearId + @"' and CompanyId='" + CompanyId + @"'
                                )
                                ) x on x.TaxYearPeriodId=p.id--1
                                where PeriodNo in (1,12)
                                order by PeriodNo";
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
        void _getDeductedAmount(string empids,string TaxYearId, int _month, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
                                EmpSystemId,sum(EarnedAmount) EarnedAmount, sum(StructureAmount) StructureAmount
                                from ProfessionalTaxDeductionMonthly
                                where 
                                taxyearid='" + TaxYearId + @"' and EmpSystemId in ("+ empids + @") and
                                monthno<>" + _month + @"
                                group by empsystemid";
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
        void _getTaxMonthly_Save(string TaxYearId,int _monthno,int _yearno,string empids, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from ProfessionalTaxDeductionMonthly where TaxYearId='"+ TaxYearId + @"' 
                            and MonthNo="+ _monthno + @" and YearNo="+ _yearno + @" and EmpSystemId in ("+empids+@")";
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
        void _getTaxInSalary_Save(string SalaryProcChildIds, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from SalaryProcChild
                                where 
                                SystemID in (" + SalaryProcChildIds + @")";
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
        void _getTaxInSalary(string empids, int _monthno, int _yearno, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from SalaryProcChild
                                where 
                                EmpInfoSystemID in (" + empids + @")
                                and 
                                SlrProcMstSystemID in (
                                select systemid from SalaryProcMaster where MonthNo=" + _monthno + @" and YearNo=" + _yearno + @"
                                )
                                and SalaryHeadID in (select SalaryHeadID from SalaryHead where HeadCategory in ('ProfessionalTax','Net Payable'))";
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
        void _getSalaryHead(out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from SalaryHead where  HeadCategory in ('ProfessionalTax','Net Payable')";
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
        void _getYearlySalary(string empids,string FromMonth, string FromYear, string ToMonth, string ToYear, string plantid, out System.Data.DataSet dsRef)
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
                            g.IsTaxable=1 and e.PlantId=@plantid and e.systemid in ("+empids+@")

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

        void _getOpeningAmount(string empids,string TaxYearId, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT 
                                   [EmpSystemId]      
                                  ,[OpeningTaxableIncomeEarned]
                                  ,[OpeningTaxPaid]
                              FROM [ProfessionalTaxOpeningBalance] where TaxYearId='"+ TaxYearId + "' and EmpSystemId in ("+empids+@")";
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
        public void _getSalaryStructure(string EmpSystemIds, string EffectiveDate, out DataSet dsRef)
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
        #endregion
    }
}
