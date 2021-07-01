using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Setting
{
    public class BonusProcess
    {
        ISqlRepository _sqlRepository;
        public BonusProcess()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> GetHead(string strHdCat)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT SH.SalaryHead, SH.SalaryHeadID FROM SalaryHead SH where HeadCategory = '" + strHdCat + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetMasterDetails(string sBonusPolicyMstSystemID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT BPX.SystemID, BPX.BPMSystemID, m.EntitleFrm,  BPX.MinServLen, BPX.MaxServLen, 
	                              BPX.IsFixed, BPX.FixedAmount, BPX.IsPercentage, BPX.PerctSalaryHeadID, PrctSH.SalaryHead PerctSalaryHead, BPX.BonusPercentage, 
	                              BPX.IsProportionate, BPX.DivisionFactor, BPX.MinBonusAmt,m.ServiceLengthType
                           FROM BonusPolicyDetail BPX
						   LEFT JOIN BonusPolicyMaster m on m.SystemID=BPX.BPMSystemID
				           LEFT JOIN SalaryHead PrctSH ON BPX.PerctSalaryHeadID = PrctSH.SalaryHeadID
                           WHERE BPX.BPMSystemID = '" + sBonusPolicyMstSystemID + @"'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> BonusPolicyMasterInfo(string sPlantID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT TPM.SystemID PolicyId, TPM.PolicyName FROM BonusPolicyMaster TPM
                            left join BonusPolicyPlantWise p on p.BonusPolicyID = TPM.SystemID
                            where p.PlantId= '" + sPlantID + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function


        public void SetBonusProcess(List<BonusProcessLocal> process, string MasterID, BonusProcessModel Bonus)
        {
            #region Declare Variable

            DataSet dsBnsMst = null;
            DataTable dtBnsMst = null;
            DataView dvBnsMst = null;
            DataRow drBnsMst = null;

            DataSet dsBnsDtl = null;
            DataTable dtBnsDtl = null;
            DataView dvBnsDtl = null;
            DataRow drBnsDtl = null;

            DataSet dsGrdEmp = null;
            DataSet dsMaster = null;
            DataSet dsEmpLocal = null;

            DataView dvGrdEmp = null;

            DataSet dsGrdBnsDtl = null;
            DataTable dtGrdBnsDtl = null;
            DataView dvGrdBnsDtl = null;


            DataSet dsSlrInfo = null;
            DataTable dtSlrInfo = null;
            DataView dvSlrInfo = null;

            #endregion Declare Variable

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsBonusProcess objBnsProc;
            objBnsProc = new clsBonusProcess();

            clsSalaryProc objSlrProc = null;
            objSlrProc = new clsSalaryProc();

            bplib.clsGenID objGenID = new bplib.clsGenID();

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();


            try
            {
                #region CHECK EDIT/UPDATE ACCESS

                //var ob = new clsStaticInfo(); ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);

                #endregion CHECK EDIT/UPDATE ACCESS

                #region Data Set Load from Data Grid
                DataSet dsLocal = null;
                LoadGrdBonusPolicy(MasterID.Trim(), out dsGrdBnsDtl);
                dtGrdBnsDtl = dsGrdBnsDtl.Tables[0];
                dvGrdBnsDtl = new DataView();
                dvGrdEmp = new DataView();

                #endregion Data Set Load from Data Grid

                #region Validation

                if (string.IsNullOrEmpty(Bonus.Work) == true || bplib.clsWebLib.IsDateOK(Bonus.Work.Trim()) == false)
                {
                    Exception ex = new Exception("Please define bonus process cut off date..! (allowed format is  dd-MMM-yyyy ex: '01-Jan-2014')");
                    throw (ex);
                }
                if (string.IsNullOrEmpty(MasterID.Trim()) == true)
                {

                    Exception ex = new Exception("Please select bonus policy...");
                    throw (ex);
                }
                if (string.IsNullOrEmpty(MasterID.Trim()) == true)
                {
                    Exception ex = new Exception("Please select bonus disbust salary head...");
                    throw (ex);
                }

                if (dsGrdBnsDtl.Tables[0].Rows.Count == 0)
                {
                    Exception ex = new Exception("Please define bonus policy details...");
                    throw (ex);
                }

                if (process.Count == 0)
                {
                    Exception ex = new Exception("No Employee found in this bonus policy...");
                    throw (ex);
                }

                #endregion Validation

                #region Employee SystemID Collection From Grid Data Set

                string sEmpInfoSysID = "";
                string sEmpSysID = "";
                clsBonusProcess objBonusPoli = null;
                objBonusPoli = new clsBonusProcess();
                bool IsDefault = false;

                objBonusPoli.GetBonusPolicyMaster(MasterID.ToString().Trim(), identity.PlantId.ToString().Trim(), out dsMaster);

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    //lblBnsPolicySystemID.Text = "" + dsLocal.Tables[0].Rows[0]["SystemID"].ToString().Trim();

                    //txtBonusPolicyName.Text = "" + dsLocal.Tables[0].Rows[0]["PolicyName"].ToString().Trim();
                    IsDefault = Convert.ToBoolean(dsMaster.Tables[0].Rows[0]["DefaultPolicy"]);

                }

                ///by monir for the time being 190516
                //if (IsDefault == true)
                //{
                //    objBonusPoli.LoadEmpForNotConfiguredDefault(identity.PlantId.ToString().Trim(), MasterID.ToString().Trim(), Bonus.Work.Trim(), out dsGrdEmp);
                //}
                //else
                //{
                objBonusPoli.LoadEmployeeInGrdForDefaultBonusProcess(identity.PlantId.ToString().Trim(), MasterID.ToString().Trim(), Bonus.Work.Trim(), out dsGrdEmp);
                //}


                for (int i = 0; i < dsGrdEmp.Tables[0].Rows.Count; i++)
                {
                    if (Convert.ToBoolean(dsGrdEmp.Tables[0].Rows[i]["IsSelectBonusProc"].ToString().Trim()) == true)
                    {
                        if (sEmpInfoSysID == "")
                        {
                            sEmpInfoSysID = "EmpInfoSystemID = '" + dsGrdEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim() + "'";
                            sEmpSysID = "EmpSystemID = '" + dsGrdEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim() + "'";
                        }
                        else
                        {
                            sEmpInfoSysID += " OR EmpInfoSystemID = '" + dsGrdEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim() + "'";
                            sEmpSysID += " OR EmpSystemID = '" + dsGrdEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim() + "'";
                        }
                    }
                }

                #endregion 

                #region Data SET

                objSlrProc.LoadEmpSlrDefForSlrProcess(identity.PlantId, sEmpInfoSysID, Bonus.Work.Trim(), Bonus.Work.Trim(), out dsSlrInfo);
                dtSlrInfo = dsSlrInfo.Tables[0];
                dvSlrInfo = new DataView();

                objBnsProc.GetBonusPaymentActualMaster(identity.PlantId.Trim(), Bonus.Work.Trim(), out dsBnsMst);
                dtBnsMst = dsBnsMst.Tables[0];
                dvBnsMst = new DataView();

                objBnsProc.GetBonusPaymentActual(sEmpSysID.Trim(), Bonus.Work.Trim(), out dsBnsDtl);
                dtBnsDtl = dsBnsDtl.Tables[0];
                dvBnsDtl = new DataView();

                //objBnsProc.GetBonusPolMstTagEmp(sEmpSysID.Trim(), out dsEmpBnsPolTag);
                //dtEmpBnsPolTag = dsEmpBnsPolTag.Tables[0];
                //dvEmpBnsPolTag = new DataView();
                DataSet dsCurrency = null;
                GetLocalCurrency(identity.PlantId.Trim(), out dsCurrency);
                string _currencyId = string.Empty;
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    //lblLocalCurrency.Text = "" + dsLocal.Tables[0].Rows[0]["Currency"].ToString().Trim();
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }

                #endregion Data SET

                #region Vertual Table

                DataTable dt = new DataTable();
                dt.TableName = "TempTable";
                dt.Columns.Add("IsSelectBonusProc", typeof(bool));                  //0
                dt.Columns.Add("EmpSystemID");                                      //1
                dt.Columns.Add("EmployeeCode");                                     //2
                dt.Columns.Add("EmployeeName");                                     //3                
                dt.Columns.Add("DOJ");                                              //6
                dt.Columns.Add("ServiceLength_Day");                                    //7
                dt.Columns.Add("ServiceLength_Month");                                    //7
                dt.Columns.Add("DOC");                                              //8
                dt.Columns.Add("ConfirmServiceLength_Day");                             //9
                dt.Columns.Add("ConfirmServiceLength_Month");                             //9
                dt.Columns.Add("CalculaServiceLength");                             //10
                dt.Columns.Add("ServiceLengthType");                             //10
                dt.Columns.Add("DesignationGroup");                                 //11
                dt.Columns.Add("SalaryHeadID");                                     //12
                dt.Columns.Add("SalaryHead");                                       //13
                dt.Columns.Add("SalaryAmount");                                     //14
                dt.Columns.Add("BonusAmount");                                      //15
                dt.Columns.Add("EntryCurrencyID");                                  //16
                dt.Columns.Add("DefineCurrencyID");                                 //17
                dt.Columns.Add("DisbustCurrencyID");                                //18
                dt.Columns.Add("AmtDefinationCurrencyID");                          //19
                dt.Columns.Add("AmtDefinationRate");
                //20
                dt.Columns.Add("IsFixed", typeof(bool));                                //20
                dt.Columns.Add("IsPercentage", typeof(bool));                                //20
                dt.Columns.Add("IsProportionate", typeof(bool));                                //20
                dt.Columns.Add("BonusPercentage");                                //20
                dt.Columns.Add("DivisionFactor");                                //20


                #endregion Vertual Table

                #region ---- dont UnderStant this part --

                StringCollection strCurrency = new StringCollection();
                string _LocalCurrencyID = string.Empty;
                string strCurrencyID = null;
                //strCurrencyID = "";
                string lblForeignCurrencyID = string.Empty;
                string lblUseFrgCurID = string.Empty;
                string txtForeignCurRate = string.Empty;
                string lblForeignCurrency = string.Empty;
                string lblLocalCurRate = "1";
                LoadLocalCurrency(identity.PlantId, out _currencyId, out _LocalCurrencyID);
                objSlrProc.LoadSalaryRuleInfo(identity.PlantId, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < dsLocal.Tables[0].Rows.Count; j++)
                    {
                        if (strCurrency.Contains(dsLocal.Tables[0].Rows[j]["AmtEntryCurrency"].ToString().Trim()) == false)
                        {
                            strCurrency.Add(dsLocal.Tables[0].Rows[j]["AmtEntryCurrency"].ToString().Trim());
                        }
                        if (strCurrency.Contains(dsLocal.Tables[0].Rows[j]["AmtDefinitionCurrency"].ToString().Trim()) == false)
                        {
                            strCurrency.Add(dsLocal.Tables[0].Rows[j]["AmtDefinitionCurrency"].ToString().Trim());
                        }
                        if (strCurrency.Contains(dsLocal.Tables[0].Rows[j]["AmtDisbusmentCurrency"].ToString().Trim()) == false)
                        {
                            strCurrency.Add(dsLocal.Tables[0].Rows[j]["AmtDisbusmentCurrency"].ToString().Trim());
                        }
                    }

                    for (int c = 0; c < strCurrency.Count; c++)
                    {
                        if (_LocalCurrencyID.Trim() != strCurrency[c].ToString())
                        {
                            strCurrencyID = strCurrency[c].ToString();
                            lblForeignCurrencyID = strCurrency[c].ToString();
                            lblUseFrgCurID = strCurrency[c].ToString();
                        }
                    }

                    dsLocal = null;
                    objSlrProc.GetEntityCurrencyRateInfo(strCurrencyID, identity.PlantId, Bonus.Work.Trim(), out dsLocal);
                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        if (dsLocal.Tables[0].Rows[0]["ToCurrencyCode"].ToString().Trim() == _LocalCurrencyID.Trim())
                        {
                            lblForeignCurrencyID = dsLocal.Tables[0].Rows[0]["FromCurrencyCode"].ToString().Trim();
                            lblForeignCurrency = dsLocal.Tables[0].Rows[0]["FromCurrencyDesc"].ToString().Trim();
                            txtForeignCurRate = dsLocal.Tables[0].Rows[0]["ToCurrencyBuying"].ToString().Trim();
                        }
                    }
                    else
                    {
                        lblForeignCurrencyID = _LocalCurrencyID.Trim();
                        lblForeignCurrency = _currencyId.Trim();
                        txtForeignCurRate = lblLocalCurRate.Trim();
                    }
                }
                #endregion

                int iBonusProcEmpCnt = 0;
                for (int i = 0; i < dsGrdEmp.Tables[0].Rows.Count; i++)
                {
                    string sSalaryHeadID = "";
                    string sSalaryHead = "";
                    string sSalaryAmount = "0";
                    string sServiceLength = "0";
                    string sBonusAmount = "0";
                    string sMinBonusAmt = "0";
                    string sEntryCurrencyID = "";
                    string sDefineCurrencyID = "";
                    string sDisbustCurrencyID = "";
                    string sAmtDefinationCurrencyID = "";
                    string AmtDefinationRate = "1";

                    bool _IsFixed = false;
                    bool _IsPercentage = false;
                    bool _IsProportionate = false;
                    decimal _BonusPercentage = 0;
                    decimal _DivisionFactor = 0;

                    int _service_length_doj = 0;
                    int _service_length_doc = 0;
                    string _ServiceLengthType = string.Empty;
                    int sl_d = 0;
                    int sl_m = 0;

                    int slc_d = 0;
                    int slc_m = 0;

                    var s = dsGrdEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                    if (s == "1903257")
                    {

                    }
                    if (Convert.ToBoolean(dsGrdEmp.Tables[0].Rows[i]["IsSelectBonusProc"].ToString().Trim()) == true)
                    {
                        iBonusProcEmpCnt++;
                        dvGrdBnsDtl.Table = dtGrdBnsDtl;
                        for (int j = 0; j < dvGrdBnsDtl.Count; j++)
                        {
                            if ("DOJ" == dvGrdBnsDtl[j]["EntitleFrm"].ToString().Trim())
                            {
                                #region Service Length is Less Then Max Service Length
                                sl_d = Convert.ToInt32(dsGrdEmp.Tables[0].Rows[i]["ServiceLength_Day"].ToString().Trim());
                                sl_m = Convert.ToInt32(dsGrdEmp.Tables[0].Rows[i]["ServiceLength_Month"].ToString().Trim());
                                string ServiceLengthType = dvGrdBnsDtl[j]["ServiceLengthType"].ToString().Trim();
                                _service_length_doj = 0;
                                if (ServiceLengthType.ToUpper() == "MONTH")
                                {
                                    _service_length_doj = sl_m;
                                }
                                else
                                {
                                    _service_length_doj = sl_d;
                                }

                                if (_service_length_doj <= Convert.ToInt32(dvGrdBnsDtl[j]["MaxServLen"].ToString().Trim()))
                                {
                                    if (_service_length_doj >= Convert.ToInt32(dvGrdBnsDtl[j]["MinServLen"].ToString().Trim()))
                                    {
                                        sServiceLength = _service_length_doj.ToString();// dsGrdEmp.Tables[0].Rows[i]["ServiceLength"].ToString().Trim();
                                        sMinBonusAmt = dvGrdBnsDtl[j]["MinBonusAmt"].ToString().Trim();
                                        _ServiceLengthType = ServiceLengthType;

                                        if (Convert.ToBoolean(dvGrdBnsDtl[j]["IsFixed"].ToString().Trim()) == true)
                                        {
                                            sBonusAmount = Convert.ToDecimal(dvGrdBnsDtl[j]["FixedAmount"].ToString().Trim()).ToString("###0;(###0)");
                                        }
                                        else
                                        {
                                            var empid = dsGrdEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                                            var shid = dvGrdBnsDtl[j]["PerctSalaryHeadID"].ToString().Trim();
                                            dvSlrInfo.Table = dtSlrInfo;
                                            dvSlrInfo.RowFilter = "EmpInfoSystemID = '" + empid + "' AND SalaryHeadID = '" + shid + "'";
                                            if (dvSlrInfo.Count > 0)
                                            {
                                                sSalaryHeadID = dvSlrInfo[0]["SalaryHeadID"].ToString().Trim();
                                                sSalaryHead = dvSlrInfo[0]["SalaryHead"].ToString().Trim();
                                                sSalaryAmount = dvSlrInfo[0]["DefineAmount"].ToString().Trim();
                                                if (Convert.ToDouble(bplib.clsWebLib.GetNumData(sSalaryAmount)) < 0)
                                                {
                                                    sSalaryAmount = "0";
                                                }

                                                sEntryCurrencyID = dvSlrInfo[0]["EntryCurrencyID"].ToString().Trim();
                                                sDefineCurrencyID = dvSlrInfo[0]["DefineCurrencyID"].ToString().Trim();
                                                sDisbustCurrencyID = dvSlrInfo[0]["DisbusmentCurrencyID"].ToString().Trim();
                                                sAmtDefinationCurrencyID = dvSlrInfo[0]["DisbusmentCurrencyID"].ToString().Trim();
                                                if (sDisbustCurrencyID.Trim() == _currencyId.Trim())
                                                { AmtDefinationRate = txtForeignCurRate.Trim(); }
                                                else
                                                { AmtDefinationRate = lblLocalCurRate.Trim(); }
                                            }

                                            if (Convert.ToBoolean(dvGrdBnsDtl[j]["IsPercentage"].ToString().Trim()) == true)
                                            {
                                                sBonusAmount = ((Convert.ToDecimal(sSalaryAmount) * Convert.ToDecimal(dvGrdBnsDtl[j]["BonusPercentage"].ToString().Trim())) / 100).ToString("###0;(###0)");
                                            }
                                            else if (Convert.ToBoolean(dvGrdBnsDtl[j]["IsProportionate"].ToString().Trim()) == true)
                                            {

                                                try
                                                {
                                                    var bp = dvGrdBnsDtl[j]["BonusPercentage"].ToString().Trim();
                                                    var df = dvGrdBnsDtl[j]["DivisionFactor"].ToString().Trim();
                                                    sBonusAmount = ((((Convert.ToDecimal(sSalaryAmount) * Convert.ToDecimal(bp)) / 100) / Convert.ToDecimal(df)) * Convert.ToDecimal(sServiceLength)).ToString("###0;(###0)");
                                                }
                                                catch (Exception exx)
                                                {
                                                    throw exx;
                                                }
                                            }
                                        }

                                        if (Convert.ToDecimal(sBonusAmount) > 0 && Convert.ToDecimal(sBonusAmount) < Convert.ToDecimal(sMinBonusAmt))
                                        {
                                            sBonusAmount = sMinBonusAmt;
                                        }

                                        _BonusPercentage = Convert.ToDecimal(dvGrdBnsDtl[j]["BonusPercentage"].ToString().Trim());
                                        _IsFixed = Convert.ToBoolean(dvGrdBnsDtl[j]["IsFixed"].ToString().Trim());
                                        _IsPercentage = Convert.ToBoolean(dvGrdBnsDtl[j]["IsPercentage"].ToString().Trim());
                                        _IsProportionate = Convert.ToBoolean(dvGrdBnsDtl[j]["IsProportionate"].ToString().Trim());
                                        if (_IsProportionate)
                                        {
                                            _DivisionFactor = Convert.ToDecimal(dvGrdBnsDtl[j]["DivisionFactor"].ToString().Trim());
                                        }
                                        else
                                        {
                                            _DivisionFactor = 1;
                                        }
                                        break;
                                    }
                                }

                                #endregion Service Length is Less Then Max Service Length                               
                            }
                            else if ("DOC" == dvGrdBnsDtl[j]["EntitleFrm"].ToString().Trim())
                            {
                                #region Service Length is Less Then Max Service Length

                                slc_d = Convert.ToInt32(dsGrdEmp.Tables[0].Rows[i]["ConfirmServiceLength"].ToString().Trim());
                                slc_m = Convert.ToInt32(dsGrdEmp.Tables[0].Rows[i]["ConfirmServiceLength"].ToString().Trim());
                                string ServiceLengthType = dvGrdBnsDtl[j]["ServiceLengthType"].ToString().Trim();
                                _service_length_doc = 0;
                                if (ServiceLengthType.ToUpper() == "MONTH")
                                {
                                    _service_length_doc = sl_m;
                                }
                                else
                                {
                                    _service_length_doc = sl_d;
                                }

                                if (_service_length_doc <= Convert.ToInt32(dvGrdBnsDtl[j]["MaxServLen"].ToString().Trim()))
                                {
                                    if (_service_length_doc >= Convert.ToInt32(dvGrdBnsDtl[j]["MinServLen"].ToString().Trim()))
                                    {
                                        sServiceLength = _service_length_doc.ToString();// dsGrdEmp.Tables[0].Rows[i]["ConfirmServiceLength"].ToString().Trim();
                                        sMinBonusAmt = dvGrdBnsDtl[j]["MinBonusAmt"].ToString().Trim();
                                        _ServiceLengthType = ServiceLengthType;

                                        if (Convert.ToBoolean(dvGrdBnsDtl[j]["IsFixed"].ToString().Trim()) == true)
                                        {
                                            sBonusAmount = Convert.ToDecimal(dvGrdBnsDtl[j]["FixedAmount"].ToString().Trim()).ToString("###0;(###0)");
                                        }
                                        else
                                        {
                                            dvSlrInfo.Table = dtSlrInfo;
                                            dvSlrInfo.RowFilter = "EmpInfoSystemID = '" + dsGrdEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim() + "' AND SalaryHeadID = '" + dvGrdBnsDtl[j]["PerctSalaryHeadID"].ToString().Trim() + "'";
                                            if (dvSlrInfo.Count > 0)
                                            {
                                                sSalaryHeadID = dvSlrInfo[0]["SalaryHeadID"].ToString().Trim();
                                                sSalaryHead = dvSlrInfo[0]["SalaryHead"].ToString().Trim();
                                                sSalaryAmount = dvSlrInfo[0]["DefineAmount"].ToString().Trim();
                                                if (Convert.ToDouble(bplib.clsWebLib.GetNumData(sSalaryAmount)) < 0)
                                                {
                                                    sSalaryAmount = "0";
                                                }
                                                sEntryCurrencyID = dvSlrInfo[0]["EntryCurrencyID"].ToString().Trim();
                                                sDefineCurrencyID = dvSlrInfo[0]["DefineCurrencyID"].ToString().Trim();
                                                sDisbustCurrencyID = dvSlrInfo[0]["DisbusmentCurrencyID"].ToString().Trim();
                                                sAmtDefinationCurrencyID = dvSlrInfo[0]["DisbusmentCurrencyID"].ToString().Trim();
                                                if (sDisbustCurrencyID.Trim() == lblForeignCurrencyID.Trim())
                                                { AmtDefinationRate = txtForeignCurRate.Trim(); }
                                                else
                                                { AmtDefinationRate = lblLocalCurRate.Trim(); }
                                            }

                                            if (Convert.ToBoolean(dvGrdBnsDtl[j]["IsPercentage"].ToString().Trim()) == true)
                                            {
                                                sBonusAmount = ((Convert.ToDecimal(sSalaryAmount) * Convert.ToDecimal(dvGrdBnsDtl[j]["BonusPercentage"].ToString().Trim())) / 100).ToString("###0;(###0)");
                                            }
                                            else if (Convert.ToBoolean(dvGrdBnsDtl[j]["IsProportionate"].ToString().Trim()) == true)
                                            {
                                                sBonusAmount = ((((Convert.ToDecimal(sSalaryAmount) * Convert.ToDecimal(dvGrdBnsDtl[j]["BonusPercentage"].ToString().Trim())) / 100) / Convert.ToDecimal(dvGrdBnsDtl[j]["DivisionFactor"].ToString().Trim())) * Convert.ToDecimal(sServiceLength)).ToString("###0;(###0)");
                                            }
                                        }
                                        if (Convert.ToDecimal(sBonusAmount) > 0 && Convert.ToDecimal(sBonusAmount) < Convert.ToDecimal(sMinBonusAmt))
                                        {
                                            sBonusAmount = sMinBonusAmt;
                                        }
                                        _BonusPercentage = Convert.ToDecimal(dvGrdBnsDtl[j]["BonusPercentage"].ToString().Trim());
                                        _IsFixed = Convert.ToBoolean(dvGrdBnsDtl[j]["IsFixed"].ToString().Trim());
                                        _IsPercentage = Convert.ToBoolean(dvGrdBnsDtl[j]["IsPercentage"].ToString().Trim());
                                        _IsProportionate = Convert.ToBoolean(dvGrdBnsDtl[j]["IsProportionate"].ToString().Trim());
                                        if (_IsProportionate)
                                        {
                                            _DivisionFactor = Convert.ToDecimal(dvGrdBnsDtl[j]["DivisionFactor"].ToString().Trim());
                                        }
                                        else
                                        {
                                            _DivisionFactor = 1;
                                        }

                                        break;
                                    }
                                }

                                #endregion Service Length is Less Then Max Service Length                               
                            }
                        }//for
                         //}

                        #region Insert New Row In Vertual Table 

                        if (((_service_length_doc > 0 || _service_length_doj > 0) && string.IsNullOrEmpty(sEntryCurrencyID) == false) || _IsFixed)
                        {
                            DataRow dtRow = dt.NewRow();
                            dtRow["IsSelectBonusProc"] = Convert.ToBoolean(dsGrdEmp.Tables[0].Rows[i]["IsSelectBonusProc"].ToString().Trim());
                            dtRow["EmpSystemID"] = dsGrdEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                            dtRow["EmployeeCode"] = dsGrdEmp.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim();
                            dtRow["EmployeeName"] = dsGrdEmp.Tables[0].Rows[i]["EmployeeName"].ToString().Trim();
                            dtRow["DOJ"] = dsGrdEmp.Tables[0].Rows[i]["DOJ"].ToString().Trim();
                            dtRow["ServiceLength_Month"] = sl_m;
                            dtRow["ServiceLength_Day"] = sl_d;
                            dtRow["DOC"] = dsGrdEmp.Tables[0].Rows[i]["DOC"].ToString().Trim();
                            dtRow["ConfirmServiceLength_Month"] = slc_m;
                            dtRow["ConfirmServiceLength_Day"] = slc_d;
                            dtRow["CalculaServiceLength"] = bplib.clsWebLib.GetNumData(sServiceLength.Trim());
                            dtRow["DesignationGroup"] = dsGrdEmp.Tables[0].Rows[i]["DesignationGroup"].ToString().Trim();
                            dtRow["SalaryHeadID"] = sSalaryHeadID.Trim();
                            dtRow["SalaryHead"] = sSalaryHead.Trim();
                            dtRow["SalaryAmount"] = bplib.clsWebLib.GetNumData(sSalaryAmount);
                            dtRow["BonusAmount"] = sBonusAmount;
                            dtRow["EntryCurrencyID"] = sEntryCurrencyID;
                            dtRow["DefineCurrencyID"] = sDefineCurrencyID;
                            dtRow["DisbustCurrencyID"] = sDisbustCurrencyID;
                            dtRow["AmtDefinationCurrencyID"] = sAmtDefinationCurrencyID;
                            dtRow["AmtDefinationRate"] = AmtDefinationRate;
                            dtRow["IsFixed"] = _IsFixed;
                            dtRow["IsPercentage"] = _IsPercentage;
                            dtRow["IsProportionate"] = _IsProportionate;
                            dtRow["BonusPercentage"] = _BonusPercentage;
                            dtRow["DivisionFactor"] = _DivisionFactor;
                            dtRow["ServiceLengthType"] = _ServiceLengthType;


                            dt.Rows.Add(dtRow);
                        }//if

                        #endregion Insert New Row In Vertual Table
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    string strTDMCode;

                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "BONUS_ACT", out strTDMCode);
                    strTDMCode = "BNSAT" + strTDMCode;

                    string BonusPolicyMaster = "";
                    bool bIsApproved = false;
                    string sSalaryHeadID = "";
                    string sEntryCurrencyID = "";
                    string sDefineCurrencyID = "";
                    string sDisbustCurrencyID = "";
                    string sAmtDefinationCurrencyID = "";
                    string AmtDefinationRate = "1";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (sSalaryHeadID.Length == 0)
                        {
                            sSalaryHeadID = dt.Rows[i]["SalaryHeadID"].ToString().Trim();
                        }
                        if (sEntryCurrencyID.Length == 0)
                        {
                            sEntryCurrencyID = dt.Rows[i]["EntryCurrencyID"].ToString().Trim();
                        }

                        if (sDefineCurrencyID.Length == 0)
                        {
                            sDefineCurrencyID = dt.Rows[i]["DefineCurrencyID"].ToString().Trim();
                        }
                        if (sDisbustCurrencyID.Length == 0)
                        {
                            sDisbustCurrencyID = dt.Rows[i]["DisbustCurrencyID"].ToString().Trim();
                        }
                        if (sAmtDefinationCurrencyID.Length == 0)
                        {
                            sAmtDefinationCurrencyID = dt.Rows[i]["AmtDefinationCurrencyID"].ToString().Trim();
                        }
                        AmtDefinationRate = dt.Rows[i]["AmtDefinationRate"].ToString().Trim();
                        if (Convert.ToBoolean(dt.Rows[i]["IsSelectBonusProc"].ToString().Trim()) == true)
                        {
                            bIsApproved = false;
                            dvBnsDtl.Table = dtBnsDtl;
                            dvBnsDtl.RowFilter = "EmpSystemID = '" + dt.Rows[i]["EmpSystemID"].ToString().Trim() + "'";
                            if (dvBnsDtl.Count > 0)
                            {
                                bIsApproved = Convert.ToBoolean(dvBnsDtl[0].Row["IsApproved"].ToString());
                                if (bIsApproved == false)
                                {
                                    BonusPolicyMaster = dvBnsDtl[0].Row["BnsMstSystemID"].ToString();
                                    drBnsDtl = dvBnsDtl[0].Row;
                                    drBnsDtl.Delete();
                                }
                            }

                            if (bIsApproved == false)
                            {
                                drBnsDtl = dtBnsDtl.NewRow();
                                drBnsDtl["SystemID"] = strTDMCode + "-" + (i + 1).ToString();
                                drBnsDtl["BnsMstSystemID"] = strTDMCode;
                                drBnsDtl["EmpSystemID"] = dt.Rows[i]["EmpSystemID"].ToString().Trim();
                                drBnsDtl["ServiceLenght"] = dt.Rows[i]["CalculaServiceLength"].ToString().Trim();
                                drBnsDtl["ServiceLengthType"] = dt.Rows[i]["ServiceLengthType"].ToString().Trim();

                                drBnsDtl["SalaryAmount"] = dt.Rows[i]["SalaryAmount"].ToString().Trim();
                                drBnsDtl["BonusAmount"] = dt.Rows[i]["BonusAmount"].ToString().Trim();

                                drBnsDtl["BonusPercentage"] = dt.Rows[i]["BonusPercentage"].ToString().Trim();
                                drBnsDtl["IsPercentage"] = dt.Rows[i]["IsPercentage"].ToString().Trim();
                                drBnsDtl["IsFixed"] = dt.Rows[i]["IsFixed"].ToString().Trim();
                                drBnsDtl["IsProportionate"] = dt.Rows[i]["IsProportionate"].ToString().Trim();
                                drBnsDtl["DivisionFactor"] = dt.Rows[i]["DivisionFactor"].ToString().Trim();
                                dtBnsDtl.Rows.Add(drBnsDtl);
                            }
                        }
                    }

                    dvBnsMst.Table = dtBnsMst;
                    dvBnsMst.RowFilter = "SystemID = '" + strTDMCode + "'";
                    if (dvBnsMst.Count == 0)
                    {
                        if (sEntryCurrencyID.Length == 0)
                        {
                            sEntryCurrencyID = _currencyId;
                            sDefineCurrencyID = _currencyId;
                            sDisbustCurrencyID = _currencyId;
                            sAmtDefinationCurrencyID = _currencyId;
                        }
                        drBnsMst = dtBnsMst.NewRow();
                        drBnsMst["SystemID"] = strTDMCode;
                        drBnsMst["BonusSystemID"] = Bonus.BPolicy.Trim();
                        if (sSalaryHeadID.Trim().Length == 0)
                        {
                            drBnsMst["SalaryHeadID"] = DBNull.Value;
                        }
                        else
                        {
                            drBnsMst["SalaryHeadID"] = sSalaryHeadID.Trim();
                        }
                        drBnsMst["EntryCurrencyID"] = sEntryCurrencyID.Trim();
                        drBnsMst["DefineCurrencyID"] = sDefineCurrencyID.Trim();
                        drBnsMst["DisbustCurrencyID"] = sDisbustCurrencyID.Trim();
                        drBnsMst["AmtDefinationCurrencyID"] = sAmtDefinationCurrencyID.Trim();
                        drBnsMst["AmtDefinationRate"] = AmtDefinationRate;
                        drBnsMst["DisbustSalaryHeadID"] = Bonus.DSalaryHead.Trim();
                        drBnsMst["EffectiveDate"] = Bonus.Work.Trim();
                        drBnsMst["Remarks"] = Bonus.Remark;
                        drBnsMst["BonusType"] = Bonus.BType.Trim();
                        drBnsMst["PlantID"] = bplib.clsWebLib.RetValidLen(identity.PlantId.ToString().Trim());
                        drBnsMst["AddedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                        drBnsMst["DateAdded"] = DateTime.Now;
                        drBnsMst["UpdatedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                        drBnsMst["DateUpdated"] = DateTime.Now;
                        dtBnsMst.Rows.Add(drBnsMst);
                    }

                    objStatic.SaveDataSets(/*dsEmpBnsPolTag,*/dsBnsMst, dsBnsDtl);
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //clean variable
            }
        }//End Function 

        public void LoadEmpSlrDefForSlrProcess(string sPlantID, string sEmpInfo, string sFromDate, string sToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM 
		                            (
                                     SELECT SD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                                            SD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate,	
                                            SD.EntryCurrencyID, ECR.Name AS EntryCurrency, SD.EntryAmount, SD.DefineCurrencyID, SD.SalaryID, SRM.CurrencyRuleSystemID,
                                            DECR.Name AS DefinitionCurrency, ISNULL(SD.DefineAmount,0) DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                              ELSE SD.SalaryHeadID END,
			                                CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay, ISNULL(SlrDis.IsBankPayment, 'TRUE') IsBankPayment, 
                                            ISNULL(SlrDis.IsCashPayment, 'TRUE') IsCashPayment, ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
											IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
																  ELSE SlrDis.IsNetPayEffect END, 
											SlrDis.FormulaDesID, ISNULL(SlrDis.BaseOnNetPay, Convert(bit, 'FALSE')) BaseOnNetPay, ISNULL(SlrDis.RefAbsentism, Convert(bit, 'FALSE')) RefAbsentism, 
											ISNULL(SlrDis.IsGNRBaseOthSlrHD, Convert(bit, 'FALSE')) IsGNRBaseOthSlrHD, SlrDis.GNRBaseOthSlrHDFormula, SlrDis.GNRApplicableMonthNo,
											SlrDis.IsRetain, SlrDis.IsMinWages, SD.SequenceNo, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                            ISNULL(SlrDis.IsWorkDaysInAMonthIncHold, 0) IsWorkDaysInAMonthIncHold, SD.SalaryCategory 
		                            FROM (
                                          SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
	                                             AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated, SequenceNo, SalaryCategory
                                          FROM SalaryInfoDefine
                                            UNION
                                          (
                                           SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
		                                          AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated, SequenceNo, SalaryCategory                   
                                           FROM SalaryInfoBack
                                          )
                                         ) SD
										INNER JOIN 
												(
												 SELECT SLM.* FROM 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoDefineMaster
                                                             UNION 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoBackMaster
                                                            )
                                                            ) SLM 
	                                                            INNER JOIN
			                                                            (
			                                                             SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
			                                                             FROM 
				                                                             (
				                                                               SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
						                                                              IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
				                                                               FROM SalaryInfoDefineMaster
						                                                           UNION 
				                                                              (
					                                                            SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
							                                                           IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
					                                                            FROM SalaryInfoBackMaster
				                                                              )
				                                                             ) A
				                                                            WHERE IsApproved = 1 AND EffectiveDate <= '" + sToDate + @"'
				                                                            GROUP BY EmpInfoSystemID
			                                                            ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
												) SEFD ON SD.SalaryID = SEFD.SystemID
			                            INNER JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
			                            INNER JOIN SalaryHead SH ON SD.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax' AND ISNULL(SH.HeadCategory, '') != 'PF Voluntary'
			                            INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
			                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SD.SalaryHeadID = CRC.SalaryHeadID
			                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
			                            LEFT JOIN 
					                            (
                                                 SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Gen' RuleType, IsGNRNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, ISNULL(IsBankPayment, Convert(bit, 'True')) IsBankPayment, 
						                                ISNULL(IsCashPayment, Convert(bit, 'True')) IsCashPayment, IsMonthWorkDay, IsFixedDisbus, BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo,
                                                        
                                                        FormulaDesID, IsRetain, IsMinWages ,IsWorkDaysInAMonthIncHold
												   FROM SalaryRuleGeneral
						                            UNION
					                             (
                                                  SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Abs' RuleType, IsAbsNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, Convert(bit, 'True') IsBankPayment, Convert(bit, 'True') IsCashPayment, 
						                                 IsMonthWorkDay, IsFixedDisbus, Convert(bit, 'FALSE') BaseOnNetPay, Convert(bit, 'FALSE') RefAbsentism, Convert(bit, 'FALSE') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, '' GNRApplicableMonthNo,
                                                         FormulaDesID, Convert(bit, 'FALSE') IsRetain, Convert(bit, 'FALSE') IsMinWages  ,Convert(bit, 'FALSE') IsWorkDaysInAMonthIncHold
												   FROM SalaryRuleAbsenteeism
                                                 )
                                                ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SD.SalaryHeadID = SlrDis.SalaryHeadID
			                            LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
											                            AND SD.SalaryHeadID = SRDSM.SalaryHeadID
                                        WHERE E.DOJ <= '" + sToDate + @"' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                                                                               OR E.DOS = '' OR E.DOS = '" + sFromDate + @"')
                                              AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + sToDate + @"'
                                    ) A 
                                  WHERE (" + sEmpInfo + @") ";
                if (sPlantID != "ALL" & sPlantID != "")
                {
                    strSql += @"
                                AND PlantID = '" + sPlantID + @"' ";
                }

                strSql += @"
                            ORDER BY EmpInfoSystemID, SequenceNo, HeadType DESC";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetBonusPaymentActualMaster(string sPlantID, string sCutOffDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                            FROM BonusPaymentActualMaster 
                          WHERE PlantID = '" + sPlantID + @"' AND EffectiveDate = '" + sCutOffDate + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetBonusPaymentActual(string sEmpInfo, string sCutOffDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                            FROM BonusPaymentActual 
                          WHERE (" + sEmpInfo + @")";

                if (sCutOffDate != "")
                {
                    strSQL = strSQL + @"
                                AND BnsMstSystemID IN (SELECT SystemID FROM BonusPaymentActualMaster WHERE EffectiveDate = '" + sCutOffDate + "')";
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetLocalCurrency(string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT C.Id AS LocalCurrency, C.[Name] AS Currency 		
		                        FROM scs.Currency C		
			                        INNER JOIN [ORG].[Company] CA ON C.id = CA.BaseCurrencyId		
		                        WHERE  CA.ID IN (SELECT DISTINCT CompanyID 		
													                FROM org.Plant		
													                WHERE ID = '" + sPlantID + @"')		
		                        ORDER BY C.[Description]";

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

        public void LoadGrdBonusPolicy(string sBonusPolicyMstSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT BPX.SystemID, BPX.BPMSystemID, m.EntitleFrm,   BPX.MinServLen, BPX.MaxServLen, 
	                              BPX.IsFixed, BPX.FixedAmount, BPX.IsPercentage, BPX.PerctSalaryHeadID, PrctSH.SalaryHead PerctSalaryHead, BPX.BonusPercentage, 
	                              BPX.IsProportionate, BPX.DivisionFactor, BPX.MinBonusAmt,m.ServiceLengthType
                           FROM BonusPolicyDetail BPX
						   left join BonusPolicyMaster m on m.SystemID = BPX.BPMSystemID
				                            LEFT JOIN SalaryHead PrctSH ON BPX.PerctSalaryHeadID = PrctSH.SalaryHeadID
                           WHERE BPX.BPMSystemID = '" + sBonusPolicyMstSystemID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function


        private void LoadSlrRuleInfo(string _plantId, string CutOffDate, out string strCurrencyID)
        {
            DataSet dsLocal = null;
            clsSalaryProc objSlrProc = null;

            StringCollection strCurrency = new StringCollection();

            strCurrencyID = "";
            string _currencyId = string.Empty;
            string _LocalCurrencyID = string.Empty;
            string lblLocalCurRate = "1";

            string lblForeignCurrencyID = string.Empty;
            string lblUseFrgCurID = string.Empty;
            string txtForeignCurRate = string.Empty;
            string lblForeignCurrency = string.Empty;


            try
            {
                objSlrProc = new clsSalaryProc();

                if (bplib.clsWebLib.IsDateOK(CutOffDate.Trim()) == false)
                {
                    Exception ex = new Exception("Please Define From Date.... (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')");
                    throw (ex);
                }

                if (string.IsNullOrEmpty(_plantId.Trim()) == true)
                {
                    Exception ex = new Exception("Please select Factory...");
                    throw (ex);
                }
                LoadLocalCurrency(_plantId, out _currencyId, out _LocalCurrencyID);
                objSlrProc.LoadSalaryRuleInfo(_plantId, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < dsLocal.Tables[0].Rows.Count; j++)
                    {
                        if (strCurrency.Contains(dsLocal.Tables[0].Rows[j]["AmtEntryCurrency"].ToString().Trim()) == false)
                        {
                            strCurrency.Add(dsLocal.Tables[0].Rows[j]["AmtEntryCurrency"].ToString().Trim());
                        }
                        if (strCurrency.Contains(dsLocal.Tables[0].Rows[j]["AmtDefinitionCurrency"].ToString().Trim()) == false)
                        {
                            strCurrency.Add(dsLocal.Tables[0].Rows[j]["AmtDefinitionCurrency"].ToString().Trim());
                        }
                        if (strCurrency.Contains(dsLocal.Tables[0].Rows[j]["AmtDisbusmentCurrency"].ToString().Trim()) == false)
                        {
                            strCurrency.Add(dsLocal.Tables[0].Rows[j]["AmtDisbusmentCurrency"].ToString().Trim());
                        }
                    }

                    for (int c = 0; c < strCurrency.Count; c++)
                    {
                        if (_LocalCurrencyID.Trim() != strCurrency[c].ToString())
                        {
                            strCurrencyID = strCurrency[c].ToString();
                            lblForeignCurrencyID = strCurrency[c].ToString();
                            lblUseFrgCurID = strCurrency[c].ToString();
                        }
                    }

                    dsLocal = null;
                    objSlrProc.GetEntityCurrencyRateInfo(strCurrencyID, _plantId, CutOffDate.Trim(), out dsLocal);
                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        if (dsLocal.Tables[0].Rows[0]["ToCurrencyCode"].ToString().Trim() == _LocalCurrencyID.Trim())
                        {
                            lblForeignCurrencyID = dsLocal.Tables[0].Rows[0]["FromCurrencyCode"].ToString().Trim();
                            lblForeignCurrency = dsLocal.Tables[0].Rows[0]["FromCurrencyDesc"].ToString().Trim();
                            txtForeignCurRate = dsLocal.Tables[0].Rows[0]["ToCurrencyBuying"].ToString().Trim();
                        }
                    }
                    else
                    {
                        lblForeignCurrencyID = _LocalCurrencyID.Trim();
                        lblForeignCurrency = _currencyId.Trim();
                        txtForeignCurRate = lblLocalCurRate.Trim();
                    }
                }

                LoadSlrHD();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objSlrProc = null;
                dsLocal = null;
            }
        }//End Function

        private void LoadSlrHD()
        {
            DataSet dsLocal = null;
            clsSalaryInfo objSal = null;

            try
            {
                objSal = new clsSalaryInfo();
                objSal.GetSalaryHeadLoadCboHeadCatWise("", "Festival Bonus", out dsLocal);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dsLocal = null;
            }
        }//End Function

        private void LoadLocalCurrency(string PlantId, out string CurrencyID, out string LocalCurrencyID)
        {
            DataSet dsLocal = null;
            CurrencyID = string.Empty;
            LocalCurrencyID = string.Empty;


            try
            {
                GetLocalCurrency(PlantId, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    CurrencyID = dsLocal.Tables[0].Rows[0]["Currency"].ToString().Trim();
                    LocalCurrencyID = dsLocal.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function

        //private void LoadSlrHD()
        //{
        //    DataSet dsLocal = null;
        //    clsSalaryInfo objSal = null;

        //    try
        //    {
        //        objSal = new clsSalaryInfo();

        //        objSal.GetSalaryHeadLoadCboHeadCatWise(lblCRSystemID.Text, "Festival Bonus", out dsLocal);
        //        ddBonusSlrHd.DataSource = dsLocal;
        //        ddBonusSlrHd.DataTextField = "SalaryHead";
        //        ddBonusSlrHd.DataValueField = "SalaryHeadID";
        //        ddBonusSlrHd.DataBind();
        //        ddBonusSlrHd.Items.Insert(0, "");

        //        if (ddBonusSlrHd.Items.Count == 2)
        //        {
        //            ddBonusSlrHd.SelectedIndex = 1;
        //        }
        //        else
        //        {
        //            ddBonusSlrHd.SelectedIndex = -1;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        dsLocal = null;
        //    }
        //}//End Function

        public void LoadEmpForNotConfiguredDefault(string sPlantID, string sBonusPolicyMstSystemID, string sCutOffDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT IsSelectBonusProc = Convert(bit, 'True'), * FROM
                                (
                                 SELECT '" + sBonusPolicyMstSystemID + @"' BnsPolMstSystemID, E.SystemID EmpSystemID, E.EmployeeCode, E.EmployeeName
                                        , REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, 
                                                       DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-'), '" + sCutOffDate + @"') + 1 ServiceLength,
                                                       REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC, 
                                                       ConfirmServiceLength = CASE WHEN (DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '" + sCutOffDate + @"') + 1) > 0 THEN 
                                                                                                        DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '" + sCutOffDate + @"') + 1 ELSE 0 END, 
                                                       DG.UserName DesignationGroup, EC.UserName EmpCategoryName, E.EmployeeCategorySystemID, '' SalaryHeadID,
                                                       '' SalaryHead, '' SalaryAmount, '' BonusAmount, '' EntryCurrencyID, '' DefineCurrencyID, '' DisbustCurrencyID, 
							                           '' AmtDefinationCurrencyID, 1 AmtDefinationRate, E.UnitID, U.UserName UnitName, E.DivisionID, Dv.UserName DivisionName,  
							                           E.DepartmentID, De.UserName DepartmentName, E.SectionID, Se.UserName SectionName, E.SubSectionID, SuS.UserName SubSectionName, 
                                                       E.LineID, Ln.UserName LineName, E.DesignationSystemID, Dsg.UserName DesignationName
                                 FROM EmployeeInformation E
                                          LEFT OUTER JOIN
							                        HKP.DesignationGroup DG ON E.DesignationGroupID = DG.id
                                          LEFT OUTER JOIN 
							                        HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.id 
                                          LEFT OUTER JOIN 
							                        ORG.Unit AS U ON U.id = E.UnitID 
                                          LEFT OUTER JOIN 
							                        ORG.Division AS Dv ON Dv.id = E.DivisionID 
                                          LEFT OUTER JOIN 
							                        ORG.Department AS De ON De.id = E.DepartmentID 
                                          LEFT OUTER JOIN 
							                        HKP.Designation AS Dsg ON Dsg.id = E.DesignationSystemID 
                                          LEFT OUTER JOIN 
							                        ORG.Section AS Se ON Se.id = E.SectionID 
                                          LEFT OUTER JOIN 
							                        ORG.SubSection AS SuS ON SuS.id = E.SubSectionID
				                          LEFT OUTER JOIN 
							                        ORG.Line AS Ln ON Ln.id = E.LineID
				                         
                                  WHERE E.PlantID = '" + sPlantID + @"' AND E.SalaryRuleMasterSystemID IS NOT NULL
                                        AND E.DOJ <= CONVERT(DATETIME, '" + sCutOffDate + @"')
                    AND (DOS >= CONVERT(DATETIME, '" + sCutOffDate + @"') OR DOS IS NULL
	                    and (
                                e.GivenDesignationId not in (--e.GivenDesignationId not in
														select m.DesignationId 
														from MST.DesignationMaster m
														left join (select * from scs.DesignationMasterConfiguration where PlantId='" + sPlantID + @"')
														c on m.id=c.DesignationMasterId
														where BonusPolicyMasterId is not null
								)--e.GivenDesignationId not in
								or
									e.GivenDesignationId  in (--e.GivenDesignationId  in
														select m.DesignationId 
														from MST.DesignationMaster m
														left join (select * from scs.DesignationMasterConfiguration where PlantId='" + sPlantID + @"')
														c on m.id=c.DesignationMasterId
														where BonusPolicyMasterId='" + sBonusPolicyMstSystemID + @"'
									)--e.GivenDesignationId  in
                            )
                            )
                                 ) A                             
                                                  ORDER BY EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void LoadEmployeeInGrdForDefaultBonusProcess(string sPlantID, string sBonusPolicyMstSystemID, string sCutOffDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT IsSelectBonusProc = Convert(bit, 'True'), * FROM
                                (
                                 SELECT ISNULL(dms.BonusPolicyMasterId, 'NULL') BnsPolMstSystemID, E.SystemID EmpSystemID, E.EmployeeCode, E.EmployeeName
                                        , REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, 

                                                       DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-'), '31-Jul-2020') + 1 ServiceLength_Day,
													   DATEDIFF(MM, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-'), '31-Jul-2020') ServiceLength_Month,

                                                       REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC, 

                                                    ConfirmServiceLength_Day = CASE WHEN (DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '31-Jul-2020') + 1) > 0 THEN 
                                                    DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '31-Jul-2020') + 1 ELSE 0 END, 

													ConfirmServiceLength_Month = CASE WHEN (DATEDIFF(MM, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '31-Jul-2020')) > 0 THEN 
                                                    DATEDIFF(MM, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '31-Jul-2020') ELSE 0 END,



                                                       DG.UserName DesignationGroup, EC.UserName EmpCategoryName, E.EmployeeCategorySystemID, '' SalaryHeadID,
                                                       '' SalaryHead, '' SalaryAmount, '' BonusAmount, '' EntryCurrencyID, '' DefineCurrencyID, '' DisbustCurrencyID, 
							                           '' AmtDefinationCurrencyID, 1 AmtDefinationRate, E.UnitID, U.UserName UnitName, E.DivisionID, Dv.UserName DivisionName,  
							                           E.DepartmentID, De.UserName DepartmentName, E.SectionID, Se.UserName SectionName, E.SubSectionID, SuS.UserName SubSectionName, 
                                                       E.LineID, Ln.UserName LineName, E.DesignationSystemID, Dsg.UserName DesignationName
                                 FROM EmployeeInformation E
                                          LEFT OUTER JOIN
							                        HKP.DesignationGroup DG ON E.DesignationGroupID = DG.id
                                          LEFT OUTER JOIN 
							                        HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.id 
                                          LEFT OUTER JOIN 
							                        ORG.Unit AS U ON U.id = E.UnitID 
                                          LEFT OUTER JOIN 
							                        ORG.Division AS Dv ON Dv.id = E.DivisionID 
                                          LEFT OUTER JOIN 
							                        ORG.Department AS De ON De.id = E.DepartmentID 
                                          LEFT OUTER JOIN 
							                        HKP.Designation AS Dsg ON Dsg.id = E.DesignationSystemID 
                                          LEFT OUTER JOIN 
							                        ORG.Section AS Se ON Se.id = E.SectionID 
                                          LEFT OUTER JOIN 
							                        ORG.SubSection AS SuS ON SuS.id = E.SubSectionID
				                          LEFT OUTER JOIN 
							                        ORG.Line AS Ln ON Ln.id = E.LineID
				                          INNER JOIN
							                        (select m.DesignationId,c.BonusPolicyMasterId from MST.DesignationMaster m
													left join (select * from scs.DesignationMasterConfiguration where PlantId= '" + sPlantID + @"')
													c on m.id=c.DesignationMasterId) DMS ON DMS.DesignationId = E.GivenDesignationId
                                  WHERE E.PlantID = '" + sPlantID + @"' AND E.SalaryRuleMasterSystemID IS NOT NULL
                                        AND E.DOJ <= CONVERT(DATETIME, '" + sCutOffDate + @"') AND (DOS >= CONVERT(DATETIME, '" + sCutOffDate + @"') OR DOS IS NULL)
                                 ) A
                                --WHERE (BnsPolMstSystemID = '" + sBonusPolicyMstSystemID + @"' OR BnsPolMstSystemID = 'NULL')
                                WHERE (BnsPolMstSystemID = '" + sBonusPolicyMstSystemID + @"')
                                                  ORDER BY EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetEmp(String MasterId, string plantId, string CutOffDate, out DataSet dsEmpLocal)
        {
            DataSet dsLocal = null;

            DataSet dsMaster = null;
            clsBonusProcess objBonusPoli = null;
            bool IsDefault = false;

            try
            {
                dsEmpLocal = null;
                objBonusPoli = new clsBonusProcess();
                string strCurrencyID = null;
                LoadSlrRuleInfo(plantId, CutOffDate, out strCurrencyID);

                #region -- Bonus Policy
                objBonusPoli.GetBonusPolicyMaster(MasterId.ToString().Trim(), plantId.ToString().Trim(), out dsMaster);

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    IsDefault = Convert.ToBoolean(dsMaster.Tables[0].Rows[0]["DefaultPolicy"]);
                }
                #endregion


                ///by monir for the time being 190516
                //if (IsDefault == true)
                //{
                //    objBonusPoli.LoadEmpForNotConfiguredDefault(plantId.ToString().Trim(), MasterId.ToString().Trim(), CutOffDate.Trim(), out dsEmpLocal);
                //}
                //else
                //{
                objBonusPoli.LoadEmployeeInGrdForDefaultBonusProcess(plantId.ToString().Trim(), MasterId.ToString().Trim(), CutOffDate.Trim(), out dsEmpLocal);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objBonusPoli = null;
                dsLocal = null;
            }
        }

        public string Report(string PlantId, string workDate)
        {
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsReport objRpt = null;

            DataSet dsSlrProc = null;
            DataView dvSlrProc = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            string NumberFormatString = "#,##0;(#,##0)";
            string USDNumberFormatString = "#,##0.00;(#,##0.00)";
            string FactoryName = "";
            string CmpName = "";

            #endregion Variable

            try
            {
                objRpt = new clsReport();

                if (string.IsNullOrEmpty(workDate.Trim()) == true)
                {
                    Exception ex = new Exception("Please enter cutt of date...");
                    throw (ex);
                }

                #region Variable

                //string sUnit = this.ddlUnit.SelectedValue.ToString().Trim();
                //string sDevi = this.ddlDivision.SelectedValue.ToString().Trim();
                //string sDept = this.ddlDepartment.SelectedValue.ToString().Trim();
                //string sSect = this.ddlSection.SelectedValue.ToString().Trim();
                //string sSbSe = this.ddlSubSection.SelectedValue.ToString().Trim();
                //string sLine = this.ddlLine.SelectedValue.ToString().Trim();
                //string sSbSeStr = this.ddlSubSecStruc.SelectedValue.ToString().Trim();
                //string sEmpC = this.ddlEmpCategor.SelectedValue.ToString().Trim();
                //string sDeGr = this.ddlDesignationGroup.SelectedValue.ToString().Trim();
                //string sDesi = this.ddlDesignation.SelectedValue.ToString().Trim();

                #endregion Variable

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region DataSet

                objRpt.GetBonusInfoEffectiveDateWiseAfterProc(workDate.Trim(), PlantId.Trim(), out dsSlrProc);
                dvSlrProc = new DataView();
                dvSlrProc.Table = dsSlrProc.Tables[0];

                objRpt.SelectedPlantWiseCompany(PlantId.ToString().Trim(), out dsCmp);

                objRpt.SelectedPlant(PlantId.ToString().Trim(), out dsFactory);

                #endregion DataSet

                if (dvSlrProc.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    #region------------------Column Header------------------
                    xlsRow = 5;
                    xlsCol = 1;
                    //1
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Sr. No.";
                    sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
                    int ColSr = xlsCol;
                    xlsCol += 1;
                    //2
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "ID No.";
                    sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 6;
                    int ColIDNo = xlsCol;
                    xlsCol += 1;
                    //3
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Name";
                    sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 17;
                    int ColName = xlsCol;
                    xlsCol += 1;
                    //4
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Date Of Joining [dd-MM-yyyy]";
                    sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 10.5;
                    int ColDOJ = xlsCol;
                    xlsCol += 1;
                    ////5
                    //sheet1.Range[xlsRow + 1, xlsCol].Text = "Date Of Confirmation [dd-MM-yyyy]";
                    //sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 10.5;
                    //int ColDOC = xlsCol;
                    //xlsCol += 1;
                    //6
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Service Lenght";
                    sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 8;
                    int ColServ = xlsCol;
                    xlsCol += 1;
                    //7
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Designation";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    int ColDG = xlsCol;
                    xlsCol += 1;
                    //8
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Staff Category";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    int ColStCt = xlsCol;
                    xlsCol += 1;
                    //9
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Unit";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    int ColUnit = xlsCol;
                    xlsCol += 1;
                    //10
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Division";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    int ColDvN = xlsCol;
                    xlsCol += 1;
                    //11
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Department";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    int ColDpN = xlsCol;
                    xlsCol += 1;
                    //13
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Section";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    int ColSec = xlsCol;
                    xlsCol += 1;
                    //14
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Sub Section";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    int ColSecS = xlsCol;
                    //Header Col
                    sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                    sheet1.Range[xlsRow, ColSr, xlsRow, xlsCol].Merge();
                    //xlsCol += 1;
                    ////15
                    //sheet1.Range[xlsRow + 1, xlsCol].Text = "Salary Head";
                    //sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 10;
                    //int ColSlrHD = xlsCol;
                    xlsCol += 1;
                    //16
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Salary Amount (Basic)";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    int ColSlrAmt = xlsCol;
                    xlsCol += 1;
                    //15
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Bonus Amount";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    int ColBNSAMT = xlsCol;
                    xlsCol += 1;
                    //16
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Bank Name";
                    sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 6;
                    int ColBankName = xlsCol;
                    xlsCol += 1;
                    //17
                    sheet1.Range[xlsRow + 1, xlsCol].Text = "Bank Account No.";
                    sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 12;
                    int ColBankAccNo = xlsCol;
                    //xlsCol += 1;
                    ////18
                    //sheet1.Range[xlsRow + 1, xlsCol].Text = "Signature";
                    //sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 26;
                    //int ColSigna = xlsCol;

                    //sheet1.Range[xlsRow, ColSlrHD, xlsRow, ColSigna].Merge();
                    sheet1.Range[xlsRow, ColSlrAmt, xlsRow, ColBankAccNo].Merge();

                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    endXlsCol = xlsCol;
                    #endregion------------------Column Header------------------

                    int RowIndex = xlsRow;

                    #region ******************Report Header******************
                    xlsRow = 1;
                    xlsCol = 1;
                    string FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Bonus Payment Sheet";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    string strRptDateRange = "";
                    strRptDateRange = "Bonus Effective Date: " + workDate.Trim();
                    sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    #endregion ******************Report Header******************

                    #region ----------------------Data-----------------------

                    #region Variable For Data
                    int SrNo = 0;
                    string x = "";
                    decimal dcSlrAmt = 0;
                    decimal dcBnsAmt = 0;

                    #endregion Variable For Data

                    xlsRow = RowIndex + 2;

                    for (int i = 0; i <= dvSlrProc.Count - 1; i++)
                    {
                        if ((string.Compare(x.ToUpper(), dvSlrProc[i]["EmpSystemID"].ToString().Trim().ToUpper())) != 0)
                        {
                            #region Variable Initialize For Data
                            dcSlrAmt = 0;
                            dcBnsAmt = 0;

                            #endregion Variable Initialize For Data
                            //1
                            sheet1.Range[xlsRow, ColSr].Number = (1 + SrNo);
                            sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //2
                            if (string.IsNullOrEmpty(dvSlrProc[i]["EmployeeCode"].ToString()) == false)
                                sheet1.Range[xlsRow, ColIDNo].Text = dvSlrProc[i]["EmployeeCode"].ToString();
                            sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //3
                            if (string.IsNullOrEmpty(dvSlrProc[i]["EmployeeName"].ToString()) == false)
                                sheet1.Range[xlsRow, ColName].Text = dvSlrProc[i]["EmployeeName"].ToString();
                            sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //4
                            if (string.IsNullOrEmpty(dvSlrProc[i]["DOJ"].ToString()) == false)
                                sheet1.Range[xlsRow, ColDOJ].Text = dvSlrProc[i]["DOJ"].ToString();
                            sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            ////5
                            //if (string.IsNullOrEmpty(dvSlrProc[i]["DOC"].ToString()) == false)
                            //    sheet1.Range[xlsRow, ColDOC].Text = dvSlrProc[i]["DOC"].ToString();
                            //sheet1.Range[xlsRow, ColDOC].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            //sheet1.Range[xlsRow, ColDOC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //6
                            if (string.IsNullOrEmpty(dvSlrProc[i]["ServiceLenght"].ToString()) == false)
                                sheet1.Range[xlsRow, ColServ].Text = dvSlrProc[i]["ServiceLenght"].ToString();
                            sheet1.Range[xlsRow, ColServ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColServ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //7
                            if (string.IsNullOrEmpty(dvSlrProc[i]["DesignationName"].ToString()) == false)
                                sheet1.Range[xlsRow, ColDG].Text = dvSlrProc[i]["DesignationName"].ToString();
                            sheet1.Range[xlsRow, ColDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColDG].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //8
                            if (string.IsNullOrEmpty(dvSlrProc[i]["EmpCategoryName"].ToString()) == false)
                                sheet1.Range[xlsRow, ColStCt].Text = dvSlrProc[i]["EmpCategoryName"].ToString();
                            sheet1.Range[xlsRow, ColStCt].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColStCt].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //9
                            if (string.IsNullOrEmpty(dvSlrProc[i]["UnitName"].ToString()) == false)
                                sheet1.Range[xlsRow, ColUnit].Text = dvSlrProc[i]["UnitName"].ToString();
                            sheet1.Range[xlsRow, ColUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //10
                            if (string.IsNullOrEmpty(dvSlrProc[i]["DivisionName"].ToString()) == false)
                                sheet1.Range[xlsRow, ColDvN].Text = dvSlrProc[i]["DivisionName"].ToString();
                            sheet1.Range[xlsRow, ColDvN].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColDvN].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //11
                            if (string.IsNullOrEmpty(dvSlrProc[i]["DepartmentName"].ToString()) == false)
                                sheet1.Range[xlsRow, ColDpN].Text = dvSlrProc[i]["DepartmentName"].ToString();
                            sheet1.Range[xlsRow, ColDpN].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColDpN].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //12
                            if (string.IsNullOrEmpty(dvSlrProc[i]["SectionName"].ToString()) == false)
                                sheet1.Range[xlsRow, ColSec].Text = dvSlrProc[i]["SectionName"].ToString();
                            sheet1.Range[xlsRow, ColSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //13
                            if (string.IsNullOrEmpty(dvSlrProc[i]["SubSectionName"].ToString()) == false)
                                sheet1.Range[xlsRow, ColSecS].Text = dvSlrProc[i]["SubSectionName"].ToString();
                            sheet1.Range[xlsRow, ColSecS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColSecS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsRow += 1;
                            SrNo += 1;
                        }
                        x = dvSlrProc[i]["EmpSystemID"].ToString().Trim().ToUpper();

                        ////14 Salary Head
                        //sheet1.Range[xlsRow - 1, ColSlrHD].Text = dvSlrProc[i]["SalaryHead"].ToString().Trim();
                        //sheet1.Range[xlsRow - 1, ColSlrHD].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        //sheet1.Range[xlsRow - 1, ColSlrHD].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //15 Salary Amount
                        dcSlrAmt += Convert.ToDecimal(dvSlrProc[i]["SalaryAmount"].ToString());

                        sheet1.Range[xlsRow - 1, ColSlrAmt].Number = Convert.ToInt32(dcSlrAmt);
                        sheet1.Range[xlsRow - 1, ColSlrAmt].NumberFormat = NumberFormatString;
                        sheet1.Range[xlsRow - 1, ColSlrAmt].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow - 1, ColSlrAmt].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //16 Bonus Amount
                        dcBnsAmt += Convert.ToDecimal(dvSlrProc[i]["BonusAmount"].ToString());

                        sheet1.Range[xlsRow - 1, ColBNSAMT].Number = Convert.ToInt32(dcBnsAmt);
                        sheet1.Range[xlsRow - 1, ColBNSAMT].NumberFormat = NumberFormatString;
                        sheet1.Range[xlsRow - 1, ColBNSAMT].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow - 1, ColBNSAMT].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        ////17
                        //if (string.IsNullOrEmpty(dvSlrProc[i]["BankName"].ToString()) == false)
                        //    sheet1.Range[xlsRow - 1, ColBankName].Text = dvSlrProc[i]["BankName"].ToString();
                        //sheet1.Range[xlsRow - 1, ColBankName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        //sheet1.Range[xlsRow - 1, ColBankName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        ////18
                        //if (string.IsNullOrEmpty(dvSlrProc[i]["BankAccNo"].ToString()) == false)
                        //    sheet1.Range[xlsRow - 1, ColBankAccNo].Text = dvSlrProc[i]["BankAccNo"].ToString();
                        //sheet1.Range[xlsRow - 1, ColBankAccNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        //sheet1.Range[xlsRow - 1, ColBankAccNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //////19 ColSigna
                        ////sheet1.Range[xlsRow - 1, ColSigna].Text = "";
                        ////sheet1.Range[xlsRow - 1, ColSigna].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        ////sheet1.Range[xlsRow - 1, ColSigna].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow - 1, ColSr, xlsRow - 1, ColBankAccNo].RowHeight = 60;
                        xlsCol = ColBankAccNo;
                    }
                    //}
                    #endregion ----------------------Data-----------------------

                    #region Line Setup
                    sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                    #endregion

                    #region Freeze Panes
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 7;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.Range["A3"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$7";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet1.Name = "BonusSheet-";
                    #endregion
                    //}
                    workbook.Version = ExcelVersion.Excel97to2003;
                    var filePath = "";
                    //var SheetName = "";
                    //return workbook;
                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sheet1.Name + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsSlrProc = null;
                dvSlrProc = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }
        }
    }
}

public class CurrencyModel
{
    public string Currency { get; set; }
    public string LocalCurrency { get; set; }

}

public class BonusProcessModel
{
    public string SystemId { get; set; }
    public string Work { get; set; }
    public string DSalaryHead { get; set; }
    public string BPolicy { get; set; }
    public string Remark { get; set; }
    public string BType { get; set; }
    public string Rate { get; set; }

}


public class BonusProcessLocal
{
    public bool IsSelectBonusProc { get; set; }
    public string BnsPolMstSystemID { get; set; }
    public string EmpSystemID { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }
    public string DOJ { get; set; }
    public int ServiceLength_Day { get; set; }
    public int ServiceLength_Month { get; set; }
    public string DOC { get; set; }
    public int ConfirmServiceLength_Day { get; set; }
    public int ConfirmServiceLength_Month { get; set; }
    //public string DesignationGroup { get; set; }
    //public string EmpCategoryName { get; set; }
    //public string EmployeeCategorySystemID { get; set; }
    //public string SalaryHeadID { get; set; }
    //public string SalaryHead { get; set; }
    //public string SalaryAmount { get; set; }
    //public string BonusAmount { get; set; }
    //public string EntryCurrencyID { get; set; }
    //public string DefineCurrencyID { get; set; }
    //public string DisbustCurrencyID { get; set; }
    //public string AmtDefinationCurrencyID { get; set; }
    //public int AmtDefinationRate { get; set; }
    //public string UnitID { get; set; }
    public string DesignationName { get; set; }
    //public string UnitName { get; set; }
    //public string DivisionID { get; set; }
    //public string DivisionName { get; set; }
    //public string DepartmentID { get; set; }
    //public string DepartmentName { get; set; }
    //public string SectionID { get; set; }
    //public string SectionName { get; set; }
    //public string SubSectionID { get; set; }
    //public string SubSectionName { get; set; }
    //public string LineID { get; set; }
    //public string LineName { get; set; }
    //public string DesignationSystemID { get; set; }

}