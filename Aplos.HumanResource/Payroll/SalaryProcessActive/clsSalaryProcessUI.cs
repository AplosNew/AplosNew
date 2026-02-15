using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.SalaryProcessActive
{
    public class clsSalaryProcessUI
    {
        public void LoadEmpSalaryProcGrid(string Description, string FromDate, string ToDate, string PlantId, out AllDataset ads)
        {
            //TBD//
            //cbx enable/disable
            //nullify emp-count
            //1.grid color
            DataSet dsEmpBacInfo = null;
            DataSet dsEmpSeparated = null;
            //clsEmployeeLoad objEmpBasic = null;
            //clsStaticInfo objStatic = null;
            //clsSalaryInfo ob = null;
            string lblLocalCurrencyID = string.Empty;
            string lblForeignCurrencyID = string.Empty;
            string txtForeignCurRate = string.Empty;
            string lblUseFrgCurID = string.Empty;

            try
            {
                #region Validation
                ads = null;
                //lblEmpCount.Text = "";
                if (bplib.clsWebLib.IsDateOK(FromDate) == false)
                {
                    //txtFromDate.Focus();
                    Exception ex = new Exception(bplib.clsWebLib.DateValidationMsg("From Date"));
                    throw (ex);
                }
                if (bplib.clsWebLib.IsDateOK(ToDate) == false)
                {
                    //txtToDate.Focus();
                    Exception ex = new Exception(bplib.clsWebLib.DateValidationMsg("To Date"));
                    throw (ex);
                }

                if (Convert.ToDateTime(FromDate) > Convert.ToDateTime(ToDate))
                {
                    throw new Exception("'From Date' can not be greater than 'To Date' ...");
                }

                DateTime maxToDate = Convert.ToDateTime(FromDate).AddMonths(1).AddDays(-1);
                if (Convert.ToDateTime(ToDate) > maxToDate)
                {
                    throw new Exception("Process duration can not be more than one month ...");
                }

                if (string.IsNullOrEmpty(PlantId) == true)
                {
                    //ddlPlant.Focus();
                    Exception ex = new Exception("Please select Factory...");
                    throw (ex);
                }

                if (string.IsNullOrEmpty(Description))
                {
                    //txtDescription.Focus();
                    Exception ex = new Exception("Description can not be blank...");
                    throw (ex);
                }

                int FromMonthNo = (int)(Convert.ToDateTime(FromDate).Month);
                int ToMonthNo = (int)(Convert.ToDateTime(ToDate).Month);
                //by monir 180119
                //ValidationSalary(FromDate, ToDate,PlantId);
                #endregion Validation

                //objEmpBasic = new clsEmployeeLoad();
                //objStatic = new clsStaticInfo();

                //cbxActive.Checked = false;
                //cbxNewlyJoined.Checked = false;
                //cbxSeparated.Checked = false;
                //cbxPresentDaysZero.Checked = false;

                //EnableAll(false, dgSalaryProc);
                //EnableAll(false, DGNewlyJoined);
                //EnableAll(false, DGSeparated);
                //EnableAll(false, DGPaydaysZero);
                ads = new AllDataset();
                ads.FromDate = FromDate;
                ads.ToDate = ToDate;
                ads.PlantId = PlantId;

                LoadSeparatedEmp(PlantId, "ALL", FromDate, ToDate, out dsEmpSeparated);
                LoadEmpSalaryProcGrid(PlantId, "ALL", FromDate, ToDate, out dsEmpBacInfo);

                //LoadEmpSalaryProcGrid(PlantId, "ALL", FromDate, ToDate, out dsEmpBacInfo);
                if (dsEmpBacInfo.Tables[0].Rows.Count > 0)
                {
                    //panSalaryProc.Visible = true;
                    DataView dvActive = new DataView(dsEmpBacInfo.Tables[0]);
                    dvActive.RowFilter = " DOJs <  '" + FromDate + @"' AND (DOSs IS NULL OR DOSs > '" + ToDate + "')";
                    //dvActive.RowFilter = "EmployeeStatus='Active' and DOJs <  '" + FromDate + @"'";
                    DataTable dtActive = dvActive.ToTable();
                    GetList(ref ads, dtActive, ListEnum.Active);
                }
                //SetGridRowColor(dgSalaryProc);
                LoadOtherTabDG(dsEmpBacInfo, dsEmpSeparated, ref ads);
                LoadSlrRuleInfo(FromDate, ToDate, PlantId, lblLocalCurrencyID, out lblForeignCurrencyID, out txtForeignCurRate, out lblUseFrgCurID);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dsEmpBacInfo = null;
            }
        }//End Function
        public void LoadEmpSalaryProcGridNew(string Description, string FromDate, string ToDate, string PlantId, out AllDataset ads)
        {
            //TBD//
            //cbx enable/disable
            //nullify emp-count
            //1.grid color
            DataSet dsEmpBacInfo = null;
            DataSet dsEmpSeparated = null;
            //clsEmployeeLoad objEmpBasic = null;
            //clsStaticInfo objStatic = null;
            //clsSalaryInfo ob = null;
            string lblLocalCurrencyID = string.Empty;
            string lblForeignCurrencyID = string.Empty;
            string txtForeignCurRate = string.Empty;
            string lblUseFrgCurID = string.Empty;

            try
            {
                #region Validation
                ads = null;
                //lblEmpCount.Text = "";
                if (bplib.clsWebLib.IsDateOK(FromDate) == false)
                {
                    //txtFromDate.Focus();
                    Exception ex = new Exception(bplib.clsWebLib.DateValidationMsg("From Date"));
                    throw (ex);
                }
                if (bplib.clsWebLib.IsDateOK(ToDate) == false)
                {
                    //txtToDate.Focus();
                    Exception ex = new Exception(bplib.clsWebLib.DateValidationMsg("To Date"));
                    throw (ex);
                }

                if (Convert.ToDateTime(FromDate) > Convert.ToDateTime(ToDate))
                {
                    throw new Exception("'From Date' can not be greater than 'To Date' ...");
                }

                DateTime maxToDate = Convert.ToDateTime(FromDate).AddMonths(1).AddDays(-1);
                if (Convert.ToDateTime(ToDate) > maxToDate)
                {
                    throw new Exception("Process duration can not be more than one month ...");
                }

                if (string.IsNullOrEmpty(PlantId) == true)
                {
                    //ddlPlant.Focus();
                    Exception ex = new Exception("Please select Factory...");
                    throw (ex);
                }

                if (string.IsNullOrEmpty(Description))
                {
                    //txtDescription.Focus();
                    Exception ex = new Exception("Description can not be blank...");
                    throw (ex);
                }

                int FromMonthNo = (int)(Convert.ToDateTime(FromDate).Month);
                int ToMonthNo = (int)(Convert.ToDateTime(ToDate).Month);
                //by monir 180119
                //ValidationSalary(FromDate, ToDate,PlantId);
                #endregion Validation

                //objEmpBasic = new clsEmployeeLoad();
                //objStatic = new clsStaticInfo();

                //cbxActive.Checked = false;
                //cbxNewlyJoined.Checked = false;
                //cbxSeparated.Checked = false;
                //cbxPresentDaysZero.Checked = false;

                //EnableAll(false, dgSalaryProc);
                //EnableAll(false, DGNewlyJoined);
                //EnableAll(false, DGSeparated);
                //EnableAll(false, DGPaydaysZero);
                ads = new AllDataset();
                ads.FromDate = FromDate;
                ads.ToDate = ToDate;
                ads.PlantId = PlantId;

                LoadSeparatedEmp(PlantId, "ALL", FromDate, ToDate, out dsEmpSeparated);
                LoadEmpSalaryProcGridNew(PlantId, "ALL", FromDate, ToDate, out dsEmpBacInfo);

                //LoadEmpSalaryProcGrid(PlantId, "ALL", FromDate, ToDate, out dsEmpBacInfo);
                if (dsEmpBacInfo.Tables[0].Rows.Count > 0)
                {
                    //panSalaryProc.Visible = true;
                    DataView dvActive = new DataView(dsEmpBacInfo.Tables[0]);
                    dvActive.RowFilter = " DOJs <  '" + FromDate + @"' AND (DOSs IS NULL OR DOSs > '" + ToDate + "')";
                    //dvActive.RowFilter = "EmployeeStatus='Active' and DOJs <  '" + FromDate + @"'";
                    DataTable dtActive = dvActive.ToTable();
                    GetList(ref ads, dtActive, ListEnum.Active);
                }
                //SetGridRowColor(dgSalaryProc);
                LoadOtherTabDG(dsEmpBacInfo, dsEmpSeparated, ref ads);
                LoadSlrRuleInfo(FromDate, ToDate, PlantId, lblLocalCurrencyID, out lblForeignCurrencyID, out txtForeignCurRate, out lblUseFrgCurID);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dsEmpBacInfo = null;
            }
        }//End Function
        private void ValidationSalary(string FromDate, string ToDate, string PlantId)
        {
            //clsSalaryProc objSlrProc = null;
            DataSet dsSalarySetting = null;
            ParamSalary param = null;
            try
            {
                param = new ParamSalary();
                //objSlrProc = new clsSalaryProc();
                GetSalarySetting(PlantId, out dsSalarySetting);
                if (dsSalarySetting.Tables[0].Rows.Count > 0)
                {
                    param.IsLastSalaryProcessWithFixedHead = bplib.clsWebLib.GetBoolData(dsSalarySetting.Tables[0].Rows[0]["IsLastSalaryProcessWithFixedHead"].ToString());
                    param.IsLastDayFixed = bplib.clsWebLib.GetBoolData(dsSalarySetting.Tables[0].Rows[0]["IsLastDayFixed"].ToString());
                    param.LastDay = Convert.ToInt32(dsSalarySetting.Tables[0].Rows[0]["LastDay"].ToString());
                }
                else
                {
                    bplib.clsWebLib.Throw("Salary Setting is not found for the selected Plant...");
                }
                //----------------------------------
                if (param.IsLastDayFixed)//dec-jan
                {
                    if (Convert.ToDateTime(ToDate).Day > param.LastDay)//29-jan to 22-jan  //so first date in the curr month
                    {
                        DateTime dtLasDate = Convert.ToDateTime(ToDate).AddMonths(1).AddDays(-1);
                        param.intMonthNo = (int)Convert.ToDateTime(dtLasDate).Month;
                        param.intYearNo = (int)Convert.ToDateTime(dtLasDate).Year;

                        string LastDate = param.LastDay + "-" + bplib.clsWebLib.GetMonthName(param.intMonthNo.ToString()) + "-" + param.intYearNo;
                        DateTime dtFirstDate = Convert.ToDateTime(LastDate).AddMonths(-1).AddDays(1);
                        if (Convert.ToDateTime(FromDate) < dtFirstDate)//20-jan
                        {
                            throw new Exception("'From Date' can not be less than [" + dtFirstDate.ToString("dd-MMM-yyyy") + "]");
                        }
                    }
                    else if (Convert.ToDateTime(ToDate).Day < param.LastDay)//20-jan to 22-jan
                    {
                        param.intMonthNo = (int)Convert.ToDateTime(ToDate).Month;
                        param.intYearNo = (int)Convert.ToDateTime(ToDate).Year;

                        string LastDate = param.LastDay + "-" + bplib.clsWebLib.GetMonthName(param.intMonthNo.ToString()) + "-" + param.intYearNo;
                        DateTime dtFirstDate = Convert.ToDateTime(LastDate).AddMonths(-1).AddDays(1);

                        if (Convert.ToDateTime(FromDate) < dtFirstDate)//20-jan
                        {
                            throw new Exception("'From Date' can not be less than [" + dtFirstDate.ToString("dd-MMM-yyyy") + "]");
                        }
                    }
                    else//=22-jan
                    {
                        DateTime dtFirstDate = Convert.ToDateTime(ToDate).AddMonths(-1).AddDays(1);
                        if (Convert.ToDateTime(FromDate) < dtFirstDate)//20-jan
                        {
                            throw new Exception("'From Date' can not be less than [" + dtFirstDate.ToString("dd-MMM-yyyy") + "]");
                        }
                    }
                }
                else//1-31
                {
                    //DateTime dtLasDate = Convert.ToDateTime(FromDate);
                    param.intMonthNo = (int)Convert.ToDateTime(FromDate).Month;
                    param.intYearNo = (int)Convert.ToDateTime(FromDate).Year;

                    string FirstDate = "01" + "-" + bplib.clsWebLib.GetMonthName(param.intMonthNo.ToString()) + "-" + param.intYearNo;
                    DateTime LastDate = Convert.ToDateTime(FirstDate).AddMonths(1).AddDays(-1);

                    if (Convert.ToDateTime(ToDate) > LastDate)
                    {
                        throw new Exception("'To Date' can not be greater than [" + LastDate.ToString("dd-MMM-yyyy") + "]");
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }//End Function
        private void GetSalarySetting(string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT  *,IsLastSalaryProcessWithFixedHead=0
                            FROM PlantWiseHRMSSetting
                                WHERE plantid='" + plantid + "'";

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
        private void LoadSlrRuleInfo(string FromDate, string ToDate, string plantid, string lblLocalCurrencyID, out string lblForeignCurrencyID, out string txtForeignCurRate, out string lblUseFrgCurID)
        {
            DataSet dsLocal = null;
            //clsSalaryProc objSlrProc = null;
            StringCollection strCurrency = new StringCollection();

            string strCurrencyID = string.Empty;
            lblForeignCurrencyID = string.Empty;
            txtForeignCurRate = string.Empty;
            lblUseFrgCurID = string.Empty;
            try
            {
                //objSlrProc = new clsSalaryProc();

                if (bplib.clsWebLib.IsDateOK(FromDate) == false)
                {
                    Exception ex = new Exception("Please Define From Date.... (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')");
                    throw (ex);
                }
                if (bplib.clsWebLib.IsDateOK(ToDate) == false)
                {
                    Exception ex = new Exception("Please Define From Date.... (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')");
                    throw (ex);
                }
                if (string.IsNullOrEmpty(plantid) == true)
                {
                    Exception ex = new Exception("Please select Factory...");
                    throw (ex);
                }

                LoadSalaryRuleInfo(plantid, out dsLocal);
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
                        if (lblLocalCurrencyID != strCurrency[c].ToString())
                        {
                            strCurrencyID = strCurrency[c].ToString();
                            lblForeignCurrencyID = strCurrency[c].ToString();
                            lblUseFrgCurID = strCurrency[c].ToString();
                        }
                    }

                    dsLocal = null;
                    GetEntityCurrencyRateInfo(strCurrencyID, plantid, FromDate, out dsLocal);
                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        if (dsLocal.Tables[0].Rows[0]["ToCurrencyCode"].ToString().Trim() == lblLocalCurrencyID)
                        {
                            lblForeignCurrencyID = dsLocal.Tables[0].Rows[0]["FromCurrencyCode"].ToString().Trim();
                            //lblForeignCurrency.Text = "" + dsLocal.Tables[0].Rows[0]["FromCurrencyDesc"].ToString().Trim();
                            txtForeignCurRate = "1";// dsLocal.Tables[0].Rows[0]["ToCurrencyBuying"].ToString().Trim();
                        }
                    }
                    else
                    {
                        lblForeignCurrencyID = lblLocalCurrencyID;
                        //lblForeignCurrency.Text = lblLocalCurrency.Text.Trim();
                        txtForeignCurRate = "1";
                    }
                }
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
        private void LoadSalaryRuleInfo(string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SM.SystemID SalaryRuleMasterSystemID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.AmtEntryCurrency,
		                            CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency FROM SalaryRuleMaster SM
	                            INNER JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                            WHERE SM.PlantID = '" + sPlantID + @"'";

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
        private void GetEntityCurrencyRateInfo(string sCurrencyID, string sPlantID, string sFromDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.SystemID
	                                        ,A.FromCurrencyUnit
	                                        ,A.FromCurrencyCode
	                                        ,FR.Code FromCurrencyDesc
	                                        ,A.ToCurrencyBuying
	                                        ,A.ToCurrencySelling
	                                        ,A.ToCurrencyCode
	                                        ,LR.Code ToCurrencyDesc
	                                        ,A.FromDate
                                        FROM [dbo].[ExchangerateDateWiseForHR] A
                                        LEFT JOIN scs.Currency FR ON A.FromCurrencyCode = FR.Id
                                        LEFT JOIN scs.Currency LR ON A.ToCurrencyCode = LR.Id
                                        WHERE PlantID = '20171'
                                        GROUP BY A.SystemID
	                                        ,A.FromCurrencyUnit
	                                        ,A.FromCurrencyCode
	                                        ,FR.Code
	                                        ,A.ToCurrencyBuying
	                                        ,A.ToCurrencySelling
	                                        ,A.ToCurrencyCode
	                                        ,LR.Code
	                                        ,A.FromDate
                                        HAVING Max(A.FromDate) <= '" + sFromDate + @"'";
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

        public void LoadSeparatedEmp(string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");

                strSQL = @"SELECT --IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')  ELSE Convert(bit, 'True') END,
                                  Convert(bit, 'False') IsSelectSlrProc,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END,e.GivenDesignationId
                                        ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										,dm.EmployeeCategoryId,gr.LegalSalaryGradeId,'NO' IsLocked,EBI.IFSCCode,EBI.MICRCode,AG.UserName AccountsGroup

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
                                        left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID

										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
                                        LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DM.Id AND DMC.PlantId=E.PlantId
										LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + sPlantID + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

left join 
(								
								select EmpInfoSystemID,max(effectivedate) ed from SalaryInfoBackMaster where PlantId='" + sPlantID + @"' and effectivedate<= '" + sToDate + @"'
								group by EmpInfoSystemID

) zz on zz.EmpInfoSystemID=e.SystemId
left join

(
	select EmpInfoSystemID from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'
) zzz on zzz.EmpInfoSystemID=e.SystemId


                               WHERE E.DOJ <= '" + sToDate + @"' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                        OR E.DOS = '' OR E.DOS = '01/01/1901' )
and (zz.EmpInfoSystemID is not null or zzz.EmpInfoSystemID is not null)
                                                    and E.SystemId not in
										                (
														                select systemid from EmployeeInformation
													                left outer join
														                (
														                select max(ToDate) ToDate,EmpInfoSystemID from
														                (
														                select distinct m.SystemID,m.FromDate,m.ToDate,c.EmpInfoSystemID from SalaryProcMaster m
														                left outer join SalaryProcChild c on m.SystemID=c.SlrProcMstSystemID
														                where c.PlantID='" + sPlantID + @"'  and c.IsApproved=1
														                ) x
														                group by EmpInfoSystemID
														                ) y on y.EmpInfoSystemID=SystemId
													                 where
													                  EmployeeStatus = '" + bplib.clsWebLib.EmployeeStatus_Separated + @"' and
													                  (
													                  (dos>='" + sFromDate + @"' and  dos<='" + sToDate + @"')
														                and (y.ToDate is not null and dos<=y.ToDate)
													                  )
										                )--not in
                                                   
                                                    and E.SystemId not in
										                (														                
							                                ( 
																	select ss.EmpInfoSystemID from 
																				 (--date and emp
																				 select max(EffectiveDate) EffectiveDate,EmpInfoSystemID from
																				 (
																				 select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
																				 where EffectiveDate<='" + sToDate + @"'  and PlantId='" + sPlantID + @"'
																				 union
																				 select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
																				 where EffectiveDate<='" + sToDate + @"' and PlantId='" + sPlantID + @"'
																				 ) x

																				 group by EmpInfoSystemID
																				 ) DE -------------date and emp
																				 left join 
																				 (
																				 select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'
																				 union 
																				 select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoBackMaster where PlantId='" + sPlantID + @"'
																				 )
																				  ss on ss.EmpInfoSystemID=de.EmpInfoSystemID and ss.EffectiveDate=de.EffectiveDate
																				  where ss.IsApproved=0
																	)

										                )--not in
                                                and e.EmployeeStatus in ('Separated')
                                                and  e.SystemId not in (select systemid from EmployeeInformation where EmployeeStatus='Active' and EmployeeCurrentStatus in (" + bplib.clsWebLib.EMP_OTHER_STATUS + @") and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"')
                                               ------------processed salary approved---------------------
                                                and E.SystemId not in
										                (														                
							                          SELECT sc.EmpInfoSystemID FROM SalaryProcChild SC	WHERE (IsApproved = 1 or IsDisbursed = 1)
															and SlrProcMstSystemID in (SELECT systemid FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"') and plantid='" + sPlantID + @"')

										                )--not in
                                                --ssnot defined
												 and  E.SystemId  in
										                (
														                
															select EmpInfoSystemID from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'
															union
															select EmpInfoSystemID from SalaryInfoBackMaster where PlantId='" + sPlantID + @"'

										                )-- in
                                              --Approved SP
                                    and e.systemid not in
                                    (
                                     (select EmpSystemId from SalaryLock where  MonthNo=Month('" + sFromDate + @"') and YearNo = Year('" + sFromDate + @"') and IsLocked=1)  
                                    )--Approved SP

                                                --Exception emp                                                         
                                                        and e.systemid not in
                                                        (
                                                        " + ExceptionEmpsForSP(sPlantID) + @"
                                                        )--Exception emp
                                                --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVReturn_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                --MLV emp during
                                                 and e.systemid not in 
                                                    (
                                                    " + MLV_During_Emp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }

                strSQL += @"
                            ORDER BY E.EmployeeCode desc --F.UserName,dgs.UserName,";

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
        public void LoadEmpSalaryProcGrid(string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                string dtFDPrevM = Convert.ToDateTime(sFromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT  distinct Convert(bit, 'False') IsSelectSlrProc,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeCurrentStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END,e.GivenDesignationId
                                        ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										,dm.EmployeeCategoryId,gr.LegalSalaryGradeId
                                       ,IsLocked = case when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')='' then 'YES'
										when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')<>'' then 'NO'
										 else 'YES' end,EBI.IFSCCode,EBI.MICRCode,AG.UserName AccountsGroup

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                         left join SalaryLock   sl on  sl.MonthNo=Month('" + dtFDPrevM + @"') and sl.YearNo=Year('" + dtFDPrevM + @"') and sl.EmpSystemId=e.SystemId and sl.IsLocked=1
                                        left join (select distinct EmpInfoSystemID from SalaryProcChild where PlantID='" + sPlantID + @"' and  SlrProcMstSystemID in (select systemid from SalaryProcMaster where MonthNo=Month('" + dtFDPrevM + @"') and YearNo=Year('" + dtFDPrevM + @"'))) k on k.EmpInfoSystemID=e.SystemId

                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
                                        left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
                                        LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DM.Id  AND DMC.PlantId=e.PlantId
										LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + sPlantID + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

left join 
(								
							select EmpInfoSystemID,max(effectivedate) ed from SalaryInfoBackMaster where PlantId='" + sPlantID + @"' and effectivedate<='" + sToDate + @"'
								group by EmpInfoSystemID

) zz on zz.EmpInfoSystemID=e.SystemId
left join

(
	select EmpInfoSystemID from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'
) zzz on zzz.EmpInfoSystemID=e.SystemId

left join (select EmpSystemId from SalaryLock where  MonthNo=Month('" + sFromDate + @"') and YearNo = Year('" + sFromDate + @"') and IsLocked=1)  locka on locka.EmpSystemId=e.SystemId

                               WHERE E.DOJ <= '" + sToDate + @"' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                        OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
and (zz.EmpInfoSystemID is not null or zzz.EmpInfoSystemID is not null)
and isnull(locka.EmpSystemId,'')=''
                                                    and E.SystemId not in
										                (
														                select systemid from EmployeeInformation
													                left outer join
														                (
														                select max(ToDate) ToDate,EmpInfoSystemID from
														                (
														                select distinct m.SystemID,m.FromDate,m.ToDate,c.EmpInfoSystemID from SalaryProcMaster m
														                left outer join SalaryProcChild c on m.SystemID=c.SlrProcMstSystemID
														                where c.PlantID='" + sPlantID + @"'  and c.IsApproved=1
														                ) x
														                group by EmpInfoSystemID
														                ) y on y.EmpInfoSystemID=SystemId
													                 where
													                  EmployeeStatus = '" + bplib.clsWebLib.EmployeeStatus_Separated + @"' and
													                  (
													                  (dos>='" + sFromDate + @"' and  dos<='" + sToDate + @"')
														                and (y.ToDate is not null and dos<=y.ToDate)
													                  )
										                )--not in
                                                    and e.systemid not in 
                                                    (
                                                          SELECT apd.EmpSystemID FROM AttdnProcessData AS apd WHERE apd.WorkDate BETWEEN '" + sFromDate + @"' AND 	'" + sToDate + @"'	GROUP BY apd.EmpSystemID HAVING SUM(ISNULL(apd.PresentValue,0)+ISNULL(apd.LateValue,0)+ISNULL(apd.LvValue,0))=0											                
							                          )
                                                    and E.SystemId not in
										                (														                
							                                ( 
																	select ss.EmpInfoSystemID from 
																				 (--date and emp
																				 select max(EffectiveDate) EffectiveDate,EmpInfoSystemID from
																				 (
																				 select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
																				 where EffectiveDate<='" + sToDate + @"'  and PlantId='" + sPlantID + @"'
																				 union
																				 select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
																				 where EffectiveDate<='" + sToDate + @"' and PlantId='" + sPlantID + @"'
																				 ) x

																				 group by EmpInfoSystemID
																				 ) DE -------------date and emp
																				 left join 
																				 (
																				 select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'
																				 union 
																				 select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoBackMaster where PlantId='" + sPlantID + @"'
																				 )
																				  ss on ss.EmpInfoSystemID=de.EmpInfoSystemID and ss.EffectiveDate=de.EffectiveDate
																				  where ss.IsApproved=0
																	)

										                )--not in
                                                and e.EmployeeStatus in ('Separated','Active')
                                                and  e.SystemId not in (select systemid from EmployeeInformation where EmployeeStatus='Active' and EmployeeCurrentStatus in (" + bplib.clsWebLib.EMP_OTHER_STATUS + @") and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"')
                                               ------------processed salary approved---------------------
                                                

                                                --Exception emp                                                         
                                                        and e.systemid not in
                                                        (
                                                        " + ExceptionEmpsForSP(sPlantID) + @"
                                                        )--Exception emp
                                                --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVReturn_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                --MLV emp during
                                                 and e.systemid not in 
                                                    (
                                                    " + MLV_During_Emp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                            ORDER BY E.EmployeeCode desc";

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
        }
              

        public void LoadEmpSalaryProcGridNew(string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;

            try
            {
                DateTime dtFrom = Convert.ToDateTime(sFromDate);
                DateTime dtTo = Convert.ToDateTime(sToDate);

                strSQL = @"
DECLARE @FromDate DATE = '" + dtFrom.ToString("yyyy-MM-dd") + @"';
DECLARE @ToDate   DATE = '" + dtTo.ToString("yyyy-MM-dd") + @"';
DECLARE @PlantID  VARCHAR(20) = '" + sPlantID + @"';
DECLARE @UserGroupID VARCHAR(20) = '" + sUserGroupID + @"';
DECLARE @MonthNo INT = MONTH(@FromDate);
DECLARE @YearNo  INT = YEAR(@FromDate);

---------------------------------------------------------
-- Preload Current Month Unapproved Salary
---------------------------------------------------------
SELECT SC.SlrProcMstSystemID,
       SC.IsApproved,
       SC.IsDisbursed,
       SC.EmpInfoSystemID
INTO #CurrentProcess
FROM SalaryProcChild SC
JOIN SalaryProcMaster SM ON SC.SlrProcMstSystemID = SM.SystemID
WHERE SM.MonthNo = @MonthNo
AND SM.YearNo = @YearNo
AND SC.IsApproved = 0
AND SC.IsDisbursed = 0;
CREATE INDEX IX_CP ON #CurrentProcess(EmpInfoSystemID);

---------------------------------------------------------
-- Preload Last Approved Salary
---------------------------------------------------------
SELECT MAX(m.ToDate) AS ToDate, c.EmpInfoSystemID
INTO #LastApproved
FROM SalaryProcMaster m
JOIN SalaryProcChild c ON m.SystemID = c.SlrProcMstSystemID
WHERE c.PlantID = @PlantID
AND c.IsApproved = 1
GROUP BY c.EmpInfoSystemID;
CREATE INDEX IX_LAS ON #LastApproved(EmpInfoSystemID);

---------------------------------------------------------
-- Preload Locked Employees
---------------------------------------------------------
SELECT EmpSystemId
INTO #LockedEmp
FROM SalaryLock
WHERE MonthNo = @MonthNo
AND YearNo = @YearNo
AND IsLocked = 1;
CREATE INDEX IX_Locked ON #LockedEmp(EmpSystemId);

---------------------------------------------------------
-- Preload Zero Attendance Employees
---------------------------------------------------------
SELECT EmpSystemID
INTO #ZeroAttendance
FROM AttdnProcessData
WHERE WorkDate BETWEEN @FromDate AND @ToDate
GROUP BY EmpSystemID
HAVING SUM(ISNULL(PresentValue,0)+ISNULL(LateValue,0)+ISNULL(LvValue,0)) = 0;
CREATE INDEX IX_ZeroAttendance ON #ZeroAttendance(EmpSystemID);

---------------------------------------------------------
-- Preload Exception / MLV / Return / During Employees
---------------------------------------------------------
SELECT SystemID AS EmpSystemID INTO #ExceptionEmp FROM (" + ExceptionEmpsForSP(sPlantID) + @") X;
CREATE INDEX IX_Exception ON #ExceptionEmp(EmpSystemID);

SELECT EmpSystemID INTO #MLVEmp FROM (" + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @") X;
CREATE INDEX IX_MLV ON #MLVEmp(EmpSystemID);

SELECT EmpSystemID INTO #MLVReturn FROM (" + MLVReturn_WC(sPlantID, sFromDate, sToDate) + @") X;
CREATE INDEX IX_MLVReturn ON #MLVReturn(EmpSystemID);

SELECT EmpSystemID INTO #MLVDuring FROM (" + MLV_During_Emp_WC(sPlantID, sFromDate, sToDate) + @") X;
CREATE INDEX IX_MLVDuring ON #MLVDuring(EmpSystemID);

---------------------------------------------------------
-- Main Query
---------------------------------------------------------
SELECT DISTINCT
    CAST(0 AS BIT) AS IsSelectSlrProc,
    CP.SlrProcMstSystemID AS SystemID,
    ISNULL(CP.IsApproved,0) AS IsApproved,
    ISNULL(CP.IsDisbursed,0) AS IsDisbursed,
    E.SystemID AS EmpSystemID,
    E.EmployeeCode,
    E.EmployeeName,
    F.UserName AS PlantName,
    E.PlantID,
    REPLACE(CONVERT(VARCHAR(11),E.DOJ,106),' ','-') AS DOJ,
    E.DOJ AS DOJs,
    REPLACE(CONVERT(VARCHAR(11),E.DOS,106),' ','-') AS DOS,
    E.DOS AS DOSs,
    E.EmployeeStatus,
    E.EmployeeCurrentStatus,
    E.EmployeeGroupSystemID AS UserGroupSystemID,
    DM.DesignationGroupID,
    DG.UserName AS DesignationGroup,
    E.SalaryRuleMasterSystemID,
    SRM.SalaryRuleName,
    DGS.UserName AS GivenDesignation,
    REPLACE(CONVERT(VARCHAR(11),LAS.ToDate,106),' ','-') AS ToDate,
    CASE 
        WHEN LAS.ToDate > @FromDate THEN 'Overlap'
        WHEN LAS.ToDate < DATEADD(DAY,-1,@FromDate) THEN 'Gap'
        ELSE 'OK'
    END AS ProcessStatus,
    CASE 
        WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
        ELSE 'Cash Payment'
    END AS BankAccountStatus,
    E.GivenDesignationId,
    E.LegalDesignationId,
    E.PaymentMode,
    E.BudgetCode,
    EBI.BankSystemID,
    EBI.BankBranchId,
    EBI.BankAccNo,
    EBI.SalaryPercentage,
    EBI.IFSCCode,
    EBI.MICRCode,
    DM.EmployeeCategoryId,
    GR.LegalSalaryGradeId,
    CASE WHEN EXISTS (SELECT 1 FROM #LockedEmp L WHERE L.EmpSystemId = E.SystemID) THEN 'YES' ELSE 'NO' END AS IsLocked,
    AG.UserName AS AccountsGroup
FROM EmployeeInformation E
LEFT JOIN #CurrentProcess CP ON CP.EmpInfoSystemID = E.SystemID
LEFT JOIN #LastApproved LAS ON LAS.EmpInfoSystemID = E.SystemID
LEFT JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
LEFT JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved=1
LEFT JOIN org.Plant F ON E.PlantID = F.Id
LEFT JOIN hkp.Designation DGS ON DGS.Id = E.GivenDesignationId
LEFT JOIN mst.DesignationMaster DM ON DM.DesignationId = E.GivenDesignationId
LEFT JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
LEFT JOIN mst.LegalSalaryGradeDesignation GR ON GR.LegalDesignationId = E.LegalDesignationId AND GR.PlantId = E.PlantId
LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId = DM.Id AND DMC.PlantId = E.PlantId
LEFT JOIN dbo.AccountsGroup AG ON AG.Id = DMC.AccountsGroupId
WHERE E.DOJ <= @ToDate
  AND (E.DOS IS NULL OR E.DOS >= @FromDate)
  AND E.EmployeeStatus IN ('Active','Separated')
  AND (@PlantID = 'ALL' OR E.PlantID = @PlantID)
  AND (@UserGroupID = 'ALL' OR E.EmployeeGroupSystemID = @UserGroupID)
  AND NOT EXISTS (SELECT 1 FROM #ZeroAttendance Z WHERE Z.EmpSystemID = E.SystemID)
  AND NOT EXISTS (SELECT 1 FROM #ExceptionEmp X WHERE X.EmpSystemID = E.SystemID)
  AND NOT EXISTS (SELECT 1 FROM #MLVEmp X WHERE X.EmpSystemID = E.SystemID)
  AND NOT EXISTS (SELECT 1 FROM #MLVReturn X WHERE X.EmpSystemID = E.SystemID)
  AND NOT EXISTS (SELECT 1 FROM #MLVDuring X WHERE X.EmpSystemID = E.SystemID)
ORDER BY E.EmployeeCode DESC;

---------------------------------------------------------
-- Cleanup
---------------------------------------------------------
DROP TABLE #CurrentProcess;
DROP TABLE #LastApproved;
DROP TABLE #LockedEmp;
DROP TABLE #ZeroAttendance;
DROP TABLE #ExceptionEmp;
DROP TABLE #MLVEmp;
DROP TABLE #MLVReturn;
DROP TABLE #MLVDuring;
";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                objCon = null;
            }
        }



        public void _LoadEmpSalaryProcGridNew(string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                string dtFDPrevM = Convert.ToDateTime(sFromDate).AddMonths(-1).ToString("dd-MMM-yyyy");

                var _sql = @"DECLARE @FromDate DATE = '" + sFromDate + @"'
DECLARE @ToDate   DATE = '" + sToDate + @"'
DECLARE @PlantId  VARCHAR(10) = '" + sPlantID + @"'
DECLARE @MonthNo  INT = MONTH(@FromDate)
DECLARE @YearNo   INT = YEAR(@FromDate)

/* ===============================
   PRE-FILTER EMPLOYEES
=================================*/
SELECT *
INTO #EMP
FROM EmployeeInformation
WHERE PlantID = @PlantId
AND DOJ <= @ToDate
AND EmployeeStatus IN ('Active','Separated')

/* ===============================
   LAST APPROVED SALARY
=================================*/
SELECT 
    c.EmpInfoSystemID,
    MAX(m.ToDate) AS ToDate
INTO #LastApprovedSalary
FROM SalaryProcMaster m
JOIN SalaryProcChild c 
    ON m.SystemID = c.SlrProcMstSystemID
WHERE c.PlantID = @PlantId
AND c.IsApproved = 1
GROUP BY c.EmpInfoSystemID


/* ===============================
   CURRENT MONTH PROCESS DATA
=================================*/
SELECT 
    SC.EmpInfoSystemID,
    SC.SlrProcMstSystemID,
    SC.IsApproved,
    SC.IsDisbursed
INTO #CurrentProcess
FROM SalaryProcMaster SM
JOIN SalaryProcChild SC 
    ON SM.SystemID = SC.SlrProcMstSystemID
WHERE SM.MonthNo = @MonthNo
AND SM.YearNo  = @YearNo
AND SC.IsApproved = 0
AND SC.IsDisbursed = 0

/* ===============================
   ATTENDANCE VALID EMPLOYEES
=================================*/
SELECT EmpSystemID
INTO #AttendanceValid
FROM AttdnProcessData
WHERE WorkDate BETWEEN @FromDate AND @ToDate
GROUP BY EmpSystemID
HAVING SUM(ISNULL(PresentValue,0) 
         + ISNULL(LateValue,0) 
         + ISNULL(LvValue,0)) > 0


/* ===============================
   MAIN QUERY (ALL YOUR COLUMNS)
=================================*/

SELECT 

    CONVERT(BIT,'False') AS IsSelectSlrProc,

    CP.SlrProcMstSystemID AS SystemID,
    ISNULL(CP.IsApproved,0) AS IsApproved,
    ISNULL(CP.IsDisbursed,0) AS IsDisbursed,

    E.SystemID AS EmpSystemID,
    E.EmployeeCode,
    E.EmployeeName,
    F.UserName AS PlantName,
    E.PlantID,

    REPLACE(CONVERT(VARCHAR(11),E.DOJ,106),' ','-') AS DOJ,
    E.DOJ AS DOJs,

    REPLACE(CONVERT(VARCHAR(11),E.DOS,106),' ','-') AS DOS,
    E.DOS AS DOSs,

    E.EmployeeStatus,
    E.EmployeeCurrentStatus,
    E.EmployeeGroupSystemID AS UserGroupSystemID,

    DM.DesignationGroupID,
    DG.UserName AS DesignationGroup,

    E.SalaryRuleMasterSystemID,
    SRM.SalaryRuleName,

    DGS.UserName AS GivenDesignation,

    REPLACE(CONVERT(VARCHAR(11),LAS.ToDate,106),' ','-') AS ToDate,

    ProcessStatus = CASE 
        WHEN LAS.ToDate > @FromDate THEN 'Overlap'
        WHEN LAS.ToDate < DATEADD(DAY,-1,@FromDate) THEN 'Gap'
        ELSE 'OK'
    END,

    BankAccountStatus = CASE 
        WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
        ELSE 'Cash Payment'
    END,

    E.GivenDesignationId,
    E.LegalDesignationId,
    E.PaymentMode,
    E.BudgetCode,

    EBI.BankSystemID,
    EBI.BankBranchId,
    EBI.BankAccNo,
    EBI.SalaryPercentage,
    EBI.IFSCCode,
    EBI.MICRCode,

    DM.EmployeeCategoryId,
    GR.LegalSalaryGradeId,

    IsLocked = CASE 
        WHEN NOT EXISTS (
            SELECT 1 FROM SalaryLock sl
            WHERE sl.EmpSystemId = E.SystemID
            AND sl.MonthNo = @MonthNo
            AND sl.YearNo = @YearNo
            AND sl.IsLocked = 1
        ) THEN 'NO'
        ELSE 'YES'
    END,

    AG.UserName AS AccountsGroup

FROM #EMP E

LEFT JOIN #CurrentProcess CP
    ON E.SystemID = CP.EmpInfoSystemID

LEFT JOIN #LastApprovedSalary LAS
    ON LAS.EmpInfoSystemID = E.SystemID

LEFT JOIN SalaryRuleMaster SRM
    ON E.SalaryRuleMasterSystemID = SRM.SystemID

LEFT JOIN org.Plant F
    ON E.PlantID = F.Id

LEFT JOIN EmployeeBankInfo EBI
    ON E.SystemID = EBI.EmpSystemID
    AND EBI.IsApproved = 1

LEFT JOIN hkp.Designation DGS
    ON DGS.Id = E.GivenDesignationId

LEFT JOIN mst.DesignationMaster DM
    ON DM.DesignationId = E.GivenDesignationId

LEFT JOIN HKP.DesignationGroup DG
    ON DG.Id = DM.DesignationGroupID

LEFT JOIN mst.LegalSalaryGradeDesignation GR
    ON GR.LegalDesignationId = E.LegalDesignationId
    AND GR.PlantId = E.PlantId

LEFT JOIN SCS.DesignationMasterConfiguration DMC
    ON DMC.DesignationMasterId = DM.Id
    AND DMC.PlantId = E.PlantId

LEFT JOIN dbo.AccountsGroup AG
    ON AG.Id = DMC.AccountsGroupId


/* ===============================
   FILTER CONDITIONS (Optimized)
=================================*/

WHERE 

    -- Attendance Valid
    EXISTS (
        SELECT 1 FROM #AttendanceValid AV
        WHERE AV.EmpSystemID = E.SystemID
    )

    -- No unapproved salary info
    AND NOT EXISTS (
        SELECT 1
        FROM SalaryInfoDefineMaster s
        WHERE s.EmpInfoSystemID = E.SystemID
        AND s.PlantId = @PlantId
        AND s.IsApproved = 0
    )

    -- No maternity leave conflict
    AND NOT EXISTS (
        SELECT 1
        FROM LeaveTransaction lt
        WHERE lt.EmpSystemID = E.SystemID
        AND lt.PlantId = @PlantId
        AND lt.FromDate <= @ToDate
        AND lt.ToDate >= @FromDate
        AND lt.LTSystemID IN (
            SELECT Id FROM LeaveType WHERE LeaveType='Maternity'
        )
    )

ORDER BY E.EmployeeCode DESC

drop table  #EMP
drop table  #LastApprovedSalary
drop table  #CurrentProcess
drop table  #AttendanceValid
";

                string sql = @"DECLARE @FromDate DATE = '" + sFromDate + @"'
DECLARE @ToDate   DATE = '" + sToDate + @"'

DECLARE @Month INT = MONTH(@FromDate)
DECLARE @Year  INT = YEAR(@FromDate)

DECLARE @PrevDate DATE = DATEADD(MONTH,-1,@FromDate)
DECLARE @PrevMonth INT = MONTH(@PrevDate)
DECLARE @PrevYear  INT = YEAR(@PrevDate)

----------------------------------------------------------
-- Preload Locked Employees
----------------------------------------------------------
SELECT EmpSystemId
INTO #LockedEmp
FROM SalaryLock
WHERE MonthNo = @Month
AND YearNo = @Year
AND IsLocked = 1

----------------------------------------------------------
-- Preload Zero Attendance Employees
----------------------------------------------------------
SELECT EmpSystemID
INTO #ZeroAttendance
FROM AttdnProcessData
WHERE WorkDate BETWEEN @FromDate AND @ToDate
GROUP BY EmpSystemID
HAVING SUM(ISNULL(PresentValue,0)
         + ISNULL(LateValue,0)
         + ISNULL(LvValue,0)) = 0

----------------------------------------------------------
-- MAIN QUERY
----------------------------------------------------------

SELECT DISTINCT
    CAST(0 AS BIT) AS IsSelectSlrProc,

    S.SlrProcMstSystemID AS SystemID,
    ISNULL(S.IsApproved,0) AS IsApproved,
    ISNULL(S.IsDisbursed,0) AS IsDisbursed,

    E.SystemID AS EmpSystemID,
    E.EmployeeCode,
    E.EmployeeName,
    F.UserName AS PlantName,
    E.PlantID,

    CONVERT(VARCHAR(11),E.DOJ,106) AS DOJ,
    E.DOJ AS DOJs,

    CONVERT(VARCHAR(11),E.DOS,106) AS DOS,
    E.DOS AS DOSs,

    E.EmployeeStatus,
    E.EmployeeCurrentStatus,
    E.EmployeeGroupSystemID AS UserGroupSystemID,

    DM.DesignationGroupID,
    DG.UserName AS DesignationGroup,
    E.SalaryRuleMasterSystemID,
    SRM.SalaryRuleName,
    DGS.UserName AS GivenDesignation,

    CONVERT(VARCHAR(11),Y.ToDate,106) AS ToDate,

    CASE
        WHEN Y.ToDate > @FromDate THEN 'Overlap'
        WHEN Y.ToDate < DATEADD(DAY,-1,@FromDate) THEN 'Gap'
        ELSE 'OK'
    END AS ProcessStatus,

    CASE
        WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
        ELSE 'Cash Payment'
    END AS BankAccountStatus,

    E.GivenDesignationId,
    E.LegalDesignationId,
    E.PaymentMode,
    E.BudgetCode,

    EBI.BankSystemID,
    EBI.BankBranchId,
    EBI.BankAccNo,
    EBI.SalaryPercentage,
    EBI.IFSCCode,
    EBI.MICRCode,

    DM.EmployeeCategoryId,
    GR.LegalSalaryGradeId,

    AG.UserName AS AccountsGroup

FROM EmployeeInformation E

LEFT JOIN SalaryRuleMaster SRM
    ON E.SalaryRuleMasterSystemID = SRM.SystemID

LEFT JOIN EmployeeBankInfo EBI
    ON E.SystemID = EBI.EmpSystemID
    AND EBI.IsApproved = 1

LEFT JOIN org.Plant F
    ON E.PlantID = F.Id

LEFT JOIN hkp.Designation DGS
    ON DGS.Id = E.GivenDesignationId

LEFT JOIN mst.DesignationMaster DM
    ON DM.DesignationId = E.GivenDesignationId

LEFT JOIN HKP.DesignationGroup DG
    ON DG.Id = DM.DesignationGroupID

LEFT JOIN mst.LegalSalaryGradeDesignation GR
    ON GR.LegalDesignationId = E.LegalDesignationId
    AND GR.PlantId = E.PlantId

LEFT JOIN SCS.DesignationMasterConfiguration DMC
    ON DMC.DesignationMasterId = DM.Id
    AND DMC.PlantId = E.PlantId

LEFT JOIN dbo.AccountsGroup AG
    ON AG.Id = DMC.AccountsGroupId

LEFT JOIN
(
    SELECT MAX(ToDate) ToDate, EmpInfoSystemID
    FROM SalaryProcMaster M
    INNER JOIN SalaryProcChild C
        ON M.SystemID = C.SlrProcMstSystemID
    WHERE C.IsApproved = 1
    GROUP BY EmpInfoSystemID
) Y ON Y.EmpInfoSystemID = E.SystemID

LEFT JOIN
(
    SELECT SC.SlrProcMstSystemID,
           SC.IsApproved,
           SC.IsDisbursed,
           SC.EmpInfoSystemID
    FROM SalaryProcChild SC
    INNER JOIN SalaryProcMaster SM
        ON SC.SlrProcMstSystemID = SM.SystemID
    WHERE SM.MonthNo = @Month
    AND SM.YearNo = @Year
) S ON S.EmpInfoSystemID = E.SystemID

----------------------------------------------------------
-- FILTER SECTION (Optimized)
----------------------------------------------------------

WHERE
    E.DOJ <= @ToDate
    AND (E.DOS IS NULL OR E.DOS >= @FromDate)
    AND E.EmployeeStatus IN ('Active','Separated')

    AND NOT EXISTS (SELECT 1 FROM #LockedEmp L WHERE L.EmpSystemId = E.SystemID)
    AND NOT EXISTS (SELECT 1 FROM #ZeroAttendance Z WHERE Z.EmpSystemID = E.SystemID)

ORDER BY E.EmployeeCode DESC

----------------------------------------------------------
DROP TABLE #LockedEmp
DROP TABLE #ZeroAttendance
";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(_sql, out dsRef, false, "1");
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
        public void LoadEmp_For_LOG(string sPlantID, string sFromDate, string sToDate, string empids, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (empids.Length == 0)
                {
                    empids = "''";
                }
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                              ELSE Convert(bit, 'True') END,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END
                                            ,dm.DesignationId GivenDesignationId
                                        ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										,dm.EmployeeCategoryId,gr.LegalSalaryGradeId,'YES' IsLocked
,EBI.IFSCCode
,EBI.MICRCode,AG.UserName AccountsGroup

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id

                                        left join  mst.DesignationMasterLegalDesignation dl on dl.LegalDesignationId=e.LegalDesignationId
										left join mst.DesignationMaster dm on dm.id=dl.DesignationMasterId
										LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = dm.DesignationId

                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
                                        
                              
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
                                        LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DM.Id AND DMC.PlantId=e.PlantId
										LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                        LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + sPlantID + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId
                               WHERE E.systemid in (" + empids + @")";

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
        }
        public string MLVReturn_WC(string sPlantID, string sFromDate, string sToDate)
        {
            string strSQL;
            try
            {
                strSQL = @"select EmpSystemID from LeaveTransaction where DATEADD(DAY,1,ToDate) between 
                                        '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"' 
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                            and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
							";
                return strSQL;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 
        private string MLV_During_Emp_WC(string sPlantID, string sFromDate, string sToDate)
        {
            string strSQL;
            try
            {
                strSQL = @"select EmpSystemID from LeaveTransaction where ('" + sFromDate + @"' between FromDate and ToDate )
												and ('" + sToDate + @"' between FromDate and ToDate)
												and PlantId='" + sPlantID + @"'  
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                                        and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
							";
                return strSQL;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
        private void LoadExceptionEmps(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select  e.systemid,e.EmployeeCode,  Convert(bit, 'False') IsDisbursed,  Convert(bit, 'False') IsApproved
                                    ,e.EmployeeName 
                                    ,format(e.DOJ,'dd-MMM-yyyy') DOJ
                                    ,format(e.DOS,'dd-MMM-yyyy') DOS
                                    ,d.UserName GivenDesignation
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus
                                    ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										,dm.EmployeeCategoryId,gr.LegalSalaryGradeId,'NO' IsLocked,EBI.IFSCCode,EBI.MICRCode,'' Flag

                                    from ExceptionEmployee a
                                    inner join EmployeeInformation e on e.SystemId=a.EmpSystemID
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
                                    left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
									left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
                                    LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1

                                    where e.PlantId='" + sPlantID + @"' 
                                    and DOJ <=  '" + sToDate + @"' AND (DOS IS NULL OR DOS >= '" + sFromDate + @"')
                                    and a.[ExceptionCategory]='Salary Process'
									and a.IsActive=1
									and a.IsForever=1
                                    and e.systemid not in 
                                    (
                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                    )
                                                ";



                strSQL += @"
                             ORDER BY EmployeeCodePrefix,E.EmployeeCodeNumeric";

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

        public void SSValida(string PlantId, string FromDate, string ToDate)
        {
            DataSet dsSSND = null;
            DataSet dsSSNA = null;
            try
            {
                LoadNotDefinedSS("", PlantId, "ALL", FromDate, ToDate, out dsSSND);
                if (dsSSND.Tables[0].Rows.Count > 0)
                {
                    string msg = string.Empty;
                    GetMsg(dsSSND, out msg);
                    if (msg.Length > 0)
                    {
                        throw new Exception("Salary Structure is not defined for the following employees " + msg + "");
                    }
                }
                LoadUnapprovedSStructure("", PlantId, "ALL", FromDate, ToDate, out dsSSNA);
                if (dsSSNA.Tables[0].Rows.Count > 0)
                {
                    string msg = string.Empty;
                    GetMsg(dsSSNA, out msg);
                    if (msg.Length > 0)
                    {
                        //throw new Exception("Salary Structure is not approved for the following employees " + msg + "");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetMsg(DataSet ds, out string msg)
        {
            msg = string.Empty;
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (msg.Length == 0)
                    {
                        msg = " [" + ds.Tables[0].Rows[i]["EmployeeCode"].ToString() + "]";
                    }
                    else
                    {
                        msg += ", [" + ds.Tables[0].Rows[i]["EmployeeCode"].ToString() + "]";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private string ExceptionEmpsForSP(string sPlantID)
        {
            string strSQL = string.Empty;
            try
            {
                strSQL = @"select  e.systemid
                                    from ExceptionEmployee a
                                    inner join EmployeeInformation e on e.SystemId=a.EmpSystemID
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
                                    where e.PlantId='" + sPlantID + @"' 									
									and a.[ExceptionCategory]='Salary Process'
									and a.IsActive=1
									and a.IsForever=1
                                                ";

                return strSQL;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
        private string MLVEmp_WC(string sPlantID, string sFromDate, string sToDate)
        {
            string strSQL;
            try
            {
                //strSQL = @"select EmpSystemID from AttdnProcessData where isnull(MaternityStatus,'')='MLV' and WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"'";
                strSQL = @"select EmpSystemID from LeaveTransaction where DATEADD(DAY,-1,FromDate) between 
                                        '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"' 
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                            and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
							";
                return strSQL;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 
        private void LoadOtherTabDG(DataSet dslocalAll, DataSet dsSeparatedEmp, ref AllDataset ads)
        {
            string Status = string.Empty;
            DataSet dsLocal = null;
            try
            {
                string FromDate = ads.FromDate;
                string ToDate = ads.ToDate;
                string PlantId = ads.PlantId;
                ads.dtSNA = null;
                ads.dtSND = null;
                ads.dtSeparated = null;
                ads.dtPresetZero = null;
                ads.dtNewlyJoined = null;
                ads.dtMaternityReturn = null;
                ads.dtEXemp = null;
                ads.dtDifferentStatus = null;
                ads.dtApprovedSalary = null;
                ads.dtAttNotProcessed = null;

                //newlyjoined
                DataView dvNew = new DataView(dslocalAll.Tables[0]);
                dvNew.RowFilter = "DOJs>='" + FromDate + "' and DOJs<='" + ToDate + "' and (DOSs is null or DOSs>'" + ToDate + "')";
                //dvNew.RowFilter = "DOJs>='" + FromDate + "' and DOJs<='" + ToDate + "' and EmployeeStatus='Active'";
                DataTable dtvNew = dvNew.ToTable();
                GetList(ref ads, dtvNew, ListEnum.NewlyJoined);

                //separated
                DataView dvSep = new DataView(dsSeparatedEmp.Tables[0]);
                dvSep.RowFilter = "DOSs >= '" + FromDate + "' and DOSs <='" + ToDate + "'  ";
                DataTable dtSep = dvSep.ToTable();
                GetList(ref ads, dtSep, ListEnum.Separated);

                ///emp sstruc not defined
                LoadNotDefinedSS(Status, PlantId, "ALL", FromDate, ToDate, out dsLocal);
                GetList(ref ads, dsLocal.Tables[0], ListEnum.SalaryStructureNotDefined);

                LoadUnapprovedSStructure(Status, PlantId, "ALL", FromDate, ToDate, out dsLocal);
                GetList(ref ads, dsLocal.Tables[0], ListEnum.SalaryStructureNotApproved);
                ///salary processed and approved IsApproved
                LoadSalaryApproved(Status, PlantId, "ALL", FromDate, ToDate, out dsLocal);
                GetList(ref ads, dsLocal.Tables[0], ListEnum.ApprovedSalary);
                ///Paydays zero
                LoadZeroPresent(Status, PlantId, "ALL", FromDate, ToDate, out dsLocal);
                GetList(ref ads, dsLocal.Tables[0], ListEnum.PresentDaysZero);
                ///Status Different
                LoadStatusDifferent(Status, PlantId, "ALL", FromDate, ToDate, out dsLocal);
                GetList(ref ads, dsLocal.Tables[0], ListEnum.DifferentStatus);
                //ads.dtDifferentStatus = dsStatusDifferent.Tables[0];
                ///Exception emp

                LoadExceptionEmps(PlantId, FromDate, ToDate, out dsLocal);
                GetList_ExceptionEmp(ref ads, dsLocal.Tables[0], ListEnum.ExceptionEmp);
                //ads.dtEXemp = dslocal.Tables[0];               
                ///MLV
                LoadMLVReturn(PlantId, FromDate, ToDate, out dsLocal);
                GetList_MaternityRetun(ref ads, dsLocal.Tables[0], ListEnum.MaternityReturn);
                //ads.dtMaternityReturn = dslocal.Tables[0];
                //clsSalaryProcessQuery objq = new clsSalaryProcessQuery();
                DataSet dsBeyond = null;
                LoadBeyondEmps(PlantId, FromDate, ToDate, out dsBeyond);
                //ads.dtb
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function
        void GetList(ref AllDataset ads, DataTable dt, ListEnum le)
        {
            try
            {
                List<ActiveEmp> list = new List<ActiveEmp>();
                if (dt.Rows.Count > 0)
                {
                    list = dt.ToList<ActiveEmp>();
                }

                if (le == ListEnum.Active)
                {
                    ads.dtActive = list;
                }
                else if (le == ListEnum.NewlyJoined)
                {
                    ads.dtNewlyJoined = list;
                }
                else if (le == ListEnum.PresentDaysZero)
                {
                    ads.dtPresetZero = list;
                }
                else if (le == ListEnum.Separated)
                {
                    ads.dtSeparated = list;
                }
                else if (le == ListEnum.SalaryStructureNotDefined)
                {
                    ads.dtSND = list;
                }
                else if (le == ListEnum.SalaryStructureNotApproved)
                {
                    ads.dtSNA = list;
                }
                else if (le == ListEnum.ApprovedSalary)
                {
                    ads.dtApprovedSalary = list;
                }
                else if (le == ListEnum.DifferentStatus)
                {
                    ads.dtDifferentStatus = list;
                }
                else
                {
                    //ads.dtEXemp = list;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetList_MaternityRetun(ref AllDataset ads, DataTable dt, ListEnum le)
        {
            try
            {
                List<MaternityRetun> list = new List<MaternityRetun>();
                if (dt.Rows.Count > 0)
                {
                    list = dt.ToList<MaternityRetun>();
                }


                if (le == ListEnum.MaternityReturn)
                {
                    ads.dtMaternityReturn = list;
                }
                else
                {
                    //ads.dtEXemp = list;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetList_ExceptionEmp(ref AllDataset ads, DataTable dt, ListEnum le)
        {
            try
            {
                List<ExceptionEmp> list = new List<ExceptionEmp>();
                if (dt.Rows.Count > 0)
                {
                    list = dt.ToList<ExceptionEmp>();
                }


                if (le == ListEnum.ExceptionEmp)
                {
                    ads.dtEXemp = list;
                }
                else
                {
                    //ads.dtEXemp = list;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void LoadBeyondEmps(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select  e.systemid,e.EmployeeCode
                                    ,e.EmployeeName 
                                    ,format(e.DOJ,'dd-MMM-yyyy') DOJ
                                    ,format(e.DOS,'dd-MMM-yyyy') DOS
                                    ,d.UserName GivenDesignation
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus
                                    ,format(sm.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
									,srm.SalaryRuleName
                                    from  EmployeeInformation e 
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
                                    left join (
									select m.EmpInfoSystemID,m.EffectiveDate,m.SalaryRuleMasterSystemID from 
                                                    (
                                                    select EmpInfoSystemID,EffectiveDate,SalaryRuleMasterSystemID from SalaryInfoDefineMaster 
									                union
									                select EmpInfoSystemID,EffectiveDate,SalaryRuleMasterSystemID  from SalaryInfoBackMaster 
                                                    )
                                                    m 
									inner join (--ssm
									select max(EffectiveDate) ed,EmpInfoSystemID from 
                                                    (
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster m where EffectiveDate>'" + sToDate + @"' and IsApproved=1
                                                    union
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster m where EffectiveDate>'" + sToDate + @"' and IsApproved=1
                                                    ) x
                                                    group by EmpInfoSystemID
													) ssm on ssm.ed=m.EffectiveDate and ssm.EmpInfoSystemID=m.EmpInfoSystemID
									) sm on sm.EmpInfoSystemID=e.SystemId
									left join SalaryRuleMaster srm on srm.SystemID=sm.SalaryRuleMasterSystemID
                                    where e.PlantId='" + sPlantID + @"'
                                    and e.systemid in
                                                    (--3
                                                    select EmpInfoSystemID from
                                                    (--2
                                                    select max(EffectiveDate) ed,EmpInfoSystemID from 
                                                    (
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster m where EffectiveDate>'" + sToDate + @"' and IsApproved=1
                                                    union
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster m where EffectiveDate>'" + sToDate + @"' and IsApproved=1
                                                    ) x
                                                    group by EmpInfoSystemID
                                                    ) --2
                                                    y
                                                    )--3
                                                    -----------------------not in prev------------------------------
                                                    and e.systemid not in

                                                    (--3
                                                    select EmpInfoSystemID from
                                                    (--2
                                                    select max(EffectiveDate) ed,EmpInfoSystemID from 
                                                    (
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster m where EffectiveDate<='" + sToDate + @"' and IsApproved=1
                                                    union
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster m where EffectiveDate<='" + sToDate + @"' and IsApproved=1
                                                    ) x
                                                    group by EmpInfoSystemID
                                                    ) --2
                                                    y
                                                    )--3
                                                    and doj<='" + sToDate + @"'
                                                ";



                strSQL += @"
                             ORDER BY EmployeeCodePrefix,E.EmployeeCodeNumeric";

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
        private void LoadNotDefinedSS(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                              ELSE Convert(bit, 'True') END,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation, e.GivenDesignationId,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END
,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										,dm.EmployeeCategoryId,gr.LegalSalaryGradeId,'NO' IsLocked,EBI.IFSCCode,EBI.MICRCode,AG.UserName AccountsGroup,'' Flag

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
 left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DM.Id  AND DMC.PlantId=e.PlantId
										LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + sPlantID + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                                WHERE   e.EmployeeStatus in ('Separated','Active')
							   and e.doj  <='" + sToDate + @"' and 
							   (e.EmployeeStatus='Active' or (e.EmployeeStatus='Separated' and e.dos>='" + sFromDate + @"' ))

                                                     and  E.SystemId not in
										                (
														                
															select EmpInfoSystemID from SalaryInfoDefineMaster   where PlantID='" + sPlantID + @"'
															union
															select EmpInfoSystemID from SalaryInfoBackMaster where PlantID='" + sPlantID + @"'

										                )--not in
	                            ---------------present zero--------------
                                and e.systemid not in 
                                (
                                 SELECT apd.EmpSystemID FROM AttdnProcessData AS apd WHERE apd.WorkDate BETWEEN '" + sFromDate + @"' AND 	'" + sToDate + @"'	GROUP BY apd.EmpSystemID HAVING SUM(ISNULL(apd.PresentValue,0)+ISNULL(apd.LateValue,0)+ISNULL(apd.LvValue,0))=0		                               
                                )
                                            --Approved SP
                                                        and e.systemid not in
                                                        (
                                                        SELECT distinct SC.EmpInfoSystemID
                                                        FROM SalaryProcChild SC
                                                        INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                                        WHERE IsApproved = 1
                                                        )--Approved SP

                                            --Exception emp
                                                        and e.systemid not in
                                                        (
                                                        " + ExceptionEmpsForSP(sPlantID) + @"
                                                        )--Exception emp
                                            --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                             ORDER BY EmployeeCodePrefix,E.EmployeeCodeNumeric";

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
        }
        private void xLoadMLVReturn(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from
									(
									select  
                                    Convert(bit, 'False') IsSelectSlrProc
                                    ,Convert(bit, 'False') IsApproved
                                    ,Convert(bit, 'False') IsDisbursed
                                    ,e.systemid
                                    ,e.systemid EmpSystemID
                                    ,e.EmployeeCode
                                    ,e.EmployeeName 
									,t.BabyNo
                                    --,e.EmployeeCodePrefix,e.EmployeeCodeNumeric
									,WithBenefit=case when p.IsNoBenefit=1 then 'No' else 'Yes' end
									,format(t.FromDate,'dd-MMM-yyyy') GoingON
									,format(t.ToDate,'dd-MMM-yyyy') ComingON
                                    ,format(DATEADD(DAY,-1,t.FromDate),'dd-MMM-yyyy') MLVFrom
									,format(DATEADD(DAY,1,t.ToDate),'dd-MMM-yyyy') MLVTo
									--,t.ToDate
									--,p.IsNoBenefit
									--,tt.CG
									--,t.CG
                                    ,format(e.DOJ,'dd-MMM-yyyy') DOJ
                                    ,format(e.DOS,'dd-MMM-yyyy') DOS
                                    ,d.UserName GivenDesignation
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus		
									,t.CG		LeaveStatus	,'OK' ProcessStatus
																		
                                    from EmployeeInformation e                
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
									
									left join (
									select *,'Coming' CG from LeaveTransaction where DATEADD(DAY,1,ToDate) between 
                                         '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"'  
										 and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')

									) t on t.EmpSystemID=e.SystemId

									left join mst.MaternityLeavePolicy p on p.id=t.MaternityLeavePolicyId
                                    where e.PlantId='" + sPlantID + @"' 						
									and 
                                    --Mlv return
									e.SystemId in
									(
									select EmpSystemID from LeaveTransaction 
                                            where DATEADD(DAY,1,ToDate) between  '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"' 
                                            and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                                            and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
									)--Mlv return

                                    --Approved SP
                                    and e.systemid not in
                                    (
                                    SELECT distinct SC.EmpInfoSystemID
                                    FROM SalaryProcChild SC
                                    INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                    WHERE IsApproved = 1
                                    )--Approved SP

									) x
																	
									ORDER BY EmployeeCode";

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
        private void LoadStatusDifferent(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT               Convert(bit, 'False') IsSelectSlrProc,  Convert(bit, 'False') IsDisbursed,  Convert(bit, 'False') IsApproved
                                                ,E.SystemId,E.EmployeeCode
	                                            ,E.EmployeeName
	                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
	                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOS, 106), ' ', '-') DOS
	                                            ,E.EmployeeCurrentStatus EmployeeStatus
	                                            ,dgs.UserName LegalDesignation
                                            	,s.UserName Section
	                                            ,s.UserName Subsection
                                                ,E.SystemId EmpSystemId ,e.GivenDesignationId
                                                ,F.UserName PlantName
                                                ,DM.DesignationGroupID, DG.UserName AS DesignationGroup
                                                , E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation
                                                ,BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment' ELSE 'Cash Payment' END
                                                ,'" + sToDate + @"' ToDate
									            ,'OK' ProcessStatus 
                                                ,E.PlantID

                                                ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										        ,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										        ,dm.EmployeeCategoryId,gr.LegalSalaryGradeId,'NO' IsLocked,EBI.IFSCCode,EBI.MICRCode
                                                ,E.EmployeeCurrentStatus,AG.UserName AccountsGroup,'' Flag
                                            FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                            LEFT OUTER JOIN hkp.LegalDesignation dgs ON dgs.Id = E.LegalDesignationId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                            left join org.Section s on s.id=PR.SectionId
                                            left join org.SubSection ss on ss.id=PR.SubSectionId
                                            LEFT JOIN org.Plant F ON E.PlantID = F.Id
                                    LEFT OUTER JOIN hkp.Designation dd ON dd.Id = E.GivenDesignationId

                                        left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
                                        LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DM.Id AND DMC.PlantId=E.PlantId
										LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
											  LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
                                            WHERE (
		                                            e.SystemId in (select systemid from EmployeeInformation where  EmployeeStatus='Active' and EmployeeCurrentStatus in (" + bplib.clsWebLib.EMP_OTHER_STATUS + @") and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"')
		                                            )
	                                           -- AND E.SystemId IN (SELECT EmpSystemID FROM AttdnDataMonthlySummary WHERE YearNo = Year('" + sFromDate + @"') AND MonthNo = Month('" + sFromDate + @"') AND PlantID = '" + sPlantID + @"'
		                                         --   ) --not in
	                                          --  AND E.PlantID = '" + sPlantID + @"'  

                                            --Approved SP
                                                        and e.systemid not in
                                                        (
                                                        SELECT distinct SC.EmpInfoSystemID
                                                        FROM SalaryProcChild SC
                                                        INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                                        WHERE IsApproved = 1
                                                        )--Approved SP

                                                --Exception emp
                                                        and e.systemid not in
                                                        (
                                                        " + ExceptionEmpsForSP(sPlantID) + @"
                                                        )--Exception emp
                                                --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )

                                                ";

                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }

                strSQL += @"
                             ORDER BY EmployeeCodePrefix,E.EmployeeCodeNumeric ";

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
        private void xLoadZeroPresent(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                              ELSE Convert(bit, 'True') END,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,e.GivenDesignationId,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID

                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + sPlantID + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                                                WHERE (E.EmployeeStatus in ('Active','Separated')) 
                                                and  e.SystemId not in (select systemid from EmployeeInformation where  EmployeeStatus='Active' and EmployeeCurrentStatus in (" + bplib.clsWebLib.EMP_OTHER_STATUS + @") and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"')
                                                and (DOS is null or DOS >= '" + sFromDate + @"') AND E.DOJ <= '" + sToDate + @"'
                                                and  E.SystemId in
										                (														                
							                                select EmpSystemID from AttdnDataMonthlySummary where  YearNo=Year('" + sFromDate + @"') and MonthNo=Month('" + sFromDate + @"') and TotalPresent=0 and TotalLv=0 and TotalLate=0  and PlantID='" + sPlantID + @"'
										                )--not in

                                                    --Approved SP
                                                        and e.systemid not in
                                                        (
                                                        SELECT distinct SC.EmpInfoSystemID
                                                        FROM SalaryProcChild SC
                                                        INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                                        WHERE IsApproved = 1
                                                        )--Approved SP
                                                    --Exception emp
                                                        and e.systemid not in
                                                        (
                                                        " + ExceptionEmpsForSP(sPlantID) + @"
                                                        )--Exception emp
                                                    --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                             ORDER BY e.EmployeeCodePrefix,E.EmployeeCodeNumeric";

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
        public void LoadZeroPresent(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                string dtFDPrevM = Convert.ToDateTime(sFromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT Convert(bit, 'False') IsSelectSlrProc,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,e.GivenDesignationId,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END
                                        ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										,dm.EmployeeCategoryId,gr.LegalSalaryGradeId
                                       ,IsLocked = case when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')='' then 'YES'
										when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')<>'' then 'NO'
										 else 'YES' end,EBI.IFSCCode,EBI.MICRCode,AG.UserName AccountsGroup,'' Flag

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        left join (select distinct EmpSystemId,MonthNo,YearNo from SalaryLock where IsLocked=1) sl on sl.EmpSystemId=e.SystemId and sl.MonthNo=Month('" + dtFDPrevM + @"') and sl.YearNo=Year('" + dtFDPrevM + @"')  
                                        left join (select distinct EmpInfoSystemID from SalaryProcChild where SlrProcMstSystemID in (select systemid from SalaryProcMaster where MonthNo=Month('" + dtFDPrevM + @"') and YearNo=Year('" + dtFDPrevM + @"'))) k on k.EmpInfoSystemID=e.SystemId

                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
                                        left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DM.Id AND DMC.PlantId=e.PlantId
										LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
		                                left join (select EmpSystemId from SalaryLock where  MonthNo=Month('" + sFromDate + @"') and YearNo = Year('" + sFromDate + @"') and IsLocked=1)  lock
										on lock.EmpSystemId=e.SystemId
                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + sPlantID + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                                                WHERE   e.SystemId not in (select systemid from EmployeeInformation where  EmployeeStatus='Active' and EmployeeCurrentStatus in (" + bplib.clsWebLib.EMP_OTHER_STATUS + @") and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"')
                                                and (DOS is null or DOS > '" + sToDate + @"') AND E.DOJ <= '" + sToDate + @"'
                                                and isnull(lock.EmpSystemId,'') =''
                                                and  E.SystemId in
										                (	
   	                                                        SELECT apd.EmpSystemID FROM AttdnProcessData AS apd WHERE apd.WorkDate BETWEEN '" + sFromDate + @"' AND 	'" + sToDate + @"'	GROUP BY apd.EmpSystemID HAVING SUM(ISNULL(apd.PresentValue,0)+ISNULL(apd.LateValue,0)+ISNULL(apd.LvValue,0))=0											                
							                         
							                            )--not in
                                   
                                                    --Exception emp
                                                        and e.systemid not in
                                                        (
                                                        " + ExceptionEmpsForSP(sPlantID) + @"
                                                        )--Exception emp
                                                    --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                    --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVReturn_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                            ORDER BY E.EmployeeCode --F.UserName,dgs.UserName,";

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
        private void LoadSalaryApproved(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                              ELSE Convert(bit, 'True') END,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  '' ToDate,
                                 ''  ProcessStatus,
								  '' BankAccountStatus 
                                    ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										,dm.EmployeeCategoryId,gr.LegalSalaryGradeId,'' GivenDesignationId,'NO' IsLocked,EBI.IFSCCode,EBI.MICRCode,AG.UserName AccountsGroup,'' Flag

                           FROM EmployeeInformation E
                                        inner join (select Id,EmpSystemId from SalaryLock where  MonthNo=Month('" + sFromDate + @"') and YearNo = Year('" + sFromDate + @"') and IsLocked=1)sl on sl .EmpSystemId=e.SystemId
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        inner JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE sc.plantid='" + sPlantID + @"'
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
                                        left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
                                        LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DM.Id  AND DMC.PlantId=e.PlantId
										LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                            ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                             ORDER BY EmployeeCodePrefix,E.EmployeeCodeNumeric";

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
        private void LoadUnapprovedSStructure(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                              ELSE Convert(bit, 'True') END,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END
                                        ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										,dm.EmployeeCategoryId,gr.LegalSalaryGradeId,'NO' IsLocked,'' GivenDesignationId,EBI.IFSCCode,EBI.MICRCode,AG.UserName AccountsGroup,'' Flag


                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
 left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DM.Id AND DMC.PlantId=e.PlantId
										LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + sPlantID + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                               WHERE E.DOJ <= '" + sToDate + @"' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                        OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')

                                                    and E.SystemId in
										                (														                
							                                ( 
																	select ss.EmpInfoSystemID from 
																				 (--date and emp
																				 select max(EffectiveDate) EffectiveDate,EmpInfoSystemID from
																				 (
																				 select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
																				 where EffectiveDate<='" + sToDate + @"'  and  PlantId='" + sPlantID + @"'
																				 union
																				 select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
																				 where EffectiveDate<='" + sToDate + @"' and  PlantId='" + sPlantID + @"'
																				 ) x

																				 group by EmpInfoSystemID
																				 ) DE -------------date and emp
																				 left join 
																				 (
																				 select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoDefineMaster  where PlantId='" + sPlantID + @"'
																				 union 
																				 select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoBackMaster  where PlantId='" + sPlantID + @"'
																				 )
																				  ss on ss.EmpInfoSystemID=de.EmpInfoSystemID and ss.EffectiveDate=de.EffectiveDate
																				  where ss.IsApproved=0
																	)

										                )--not in
                                    and e.EmployeeStatus in ('Separated','Active')
							   	---------------present zero--------------
                                and e.systemid not in 
                                (
                                    SELECT apd.EmpSystemID FROM AttdnProcessData AS apd WHERE apd.WorkDate BETWEEN '" + sFromDate + @"' AND 	'" + sToDate + @"'	GROUP BY apd.EmpSystemID HAVING SUM(ISNULL(apd.PresentValue,0)+ISNULL(apd.LateValue,0)+ISNULL(apd.LvValue,0))=0		                               
                                                          
                                    )
                                --Approved SP
                                                        and e.systemid not in
                                                        (
                                                        SELECT distinct SC.EmpInfoSystemID
                                                        FROM SalaryProcChild SC
                                                        INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                                        WHERE IsApproved = 1
                                                        )--Approved SP
                                            --Exception emp
                                                        and e.systemid not in
                                                        (
                                                        " + ExceptionEmpsForSP(sPlantID) + @"
                                                        )--Exception emp
                                            --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                             ORDER BY EmployeeCodePrefix,E.EmployeeCodeNumeric";

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
        }
        public void LoadMLVReturn(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;//
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                string dtFDPrevM = Convert.ToDateTime(sFromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                strSQL = @"select * from
									(
									select  
                                    Convert(bit, 'False') IsSelectSlrProc
                                    --,Convert(bit, 'False') IsEnabled
                                    ,Convert(bit, 'False') IsApproved
                                    ,Convert(bit, 'False') IsDisbursed
                                    ,e.systemid
                                    ,e.systemid EmpSystemID
                                    ,e.EmployeeCode
                                    ,e.EmployeeName 
									,t.BabyNo
									,WithBenefit=case when p.IsNoBenefit=1 then 'No' else 'Yes' end
									,format(t.FromDate,'dd-MMM-yyyy') GoingON
									,format(t.ToDate,'dd-MMM-yyyy') ComingON
                                    ,format(DATEADD(DAY,-1,t.FromDate),'dd-MMM-yyyy') MLVFrom
									,format(DATEADD(DAY,1,t.ToDate),'dd-MMM-yyyy') MLVTo
									--,t.ToDate
									--,p.IsNoBenefit
									--,tt.CG
									--,t.CG
                                    ,format(e.DOJ,'dd-MMM-yyyy') DOJ
                                    ,format(e.DOS,'dd-MMM-yyyy') DOS
                                    ,d.UserName GivenDesignation
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus		
									,t.CG		LeaveStatus	,'OK' ProcessStatus,GivenDesignationId
                                    ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
									,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
									,dm.EmployeeCategoryId,gr.LegalSalaryGradeId
                                    ,IsLocked = case when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')='' then 'YES'
										when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')<>'' then 'NO'
										 else 'YES' end,EBI.IFSCCode,EBI.MICRCode,'' Flag
																		
                                    from EmployeeInformation e             
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
            left join (select distinct EmpSystemId,MonthNo,YearNo from SalaryLock where IsLocked=1) sl on sl.EmpSystemId=e.SystemId and sl.MonthNo=Month('" + dtFDPrevM + "') and sl.YearNo=Year('" + dtFDPrevM + @"') 
left join (select distinct EmpInfoSystemID from SalaryProcChild where SlrProcMstSystemID in (select systemid from SalaryProcMaster where MonthNo=Month('" + dtFDPrevM + @"') and YearNo=Year('" + dtFDPrevM + @"'))) k on k.EmpInfoSystemID=e.SystemId

 LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
 left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
									
									left join (
									select *,'Coming' CG from LeaveTransaction where DATEADD(DAY,1,ToDate) between 
                                         '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"'  
										 and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')

									) t on t.EmpSystemID=e.SystemId

									left join mst.MaternityLeavePolicy p on p.id=t.MaternityLeavePolicyId
                                    where e.PlantId='" + sPlantID + @"' 						
									and 
                                    --Mlv return
									e.SystemId in
									(
									select EmpSystemID from LeaveTransaction 
                                            where DATEADD(DAY,1,ToDate) between  '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"' 
                                            and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                                            and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
									)--Mlv return

                                   --Approved SP
                                    and e.systemid not in
                                    (
                                     (select EmpSystemId from SalaryLock where  MonthNo=Month('" + sFromDate + @"') and YearNo = Year('" + sFromDate + @"') and IsLocked=1)  
                                    )--Approved SP

                                   --active
									and isnull(e.dos,'') not between '" + sFromDate + @"' and '" + sToDate + @"'

									) x
																	
									order by EmployeeCode";//eee

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

        public void GetSalaryProcessedLockedEmp(string empids, string yearno, string monthno, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (empids.Length == 0)
                {
                    empids = "''";
                }
                strSQL = @"select e.EmployeeCode from salarylock c
                                left join EmployeeInformation e on e.SystemId=c.EmpSystemId
                                where YearNo=" + yearno + " and MonthNo=" + monthno + " and IsLocked=1 and EmpSystemId in (" + empids + ")";//eee

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
        public void GetSalaryProcessedUnLockedEmp(string empids, string yearno, string monthno, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (empids.Length == 0)
                {
                    empids = "''";
                }
                strSQL = @"select e.EmployeeCode from salarylock c
                                left join EmployeeInformation e on e.SystemId=c.EmpSystemId
                                where YearNo=" + yearno + " and MonthNo=" + monthno + " and isnull(IsLocked,0)=0 and EmpSystemId in (" + empids + ")";//eee

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

        public void GetSalaryProcessedUnlockedLockedEmp(string empids, string yearno, string monthno, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (empids.Length == 0)
                {
                    empids = "''";
                }
                //strSQL = @"select e.EmployeeCode from salarylock c
                //                left join EmployeeInformation e on e.SystemId=c.EmpSystemId
                //                where YearNo=" + yearno + " and MonthNo=" + monthno + " and IsLocked=1 and EmpSystemId in (" + empids + ") and isnull(c.EmpSystemId,'')=''";//eee
                strSQL = @"select distinct e.EmployeeCode,c.EmpSystemId  IsLocked ,spc.EmpInfoSystemID IsSalaryProcessed
                                from (select * from EmployeeInformation) e
                                left  join (select * from salarylock where YearNo=" + yearno + " and MonthNo=" + monthno + @" and IsLocked=1 )c on e.SystemId=c.EmpSystemId
                                left join (select * from SalaryProcChild where SlrProcMstSystemID in
                                (select systemid from SalaryProcMaster where MonthNo=" + monthno + @" and YearNo=" + yearno + @")) spc
                                on spc.EmpInfoSystemID=e.SystemId
                                 where e.SystemId in (" + empids + @")
                                and  isnull(c.EmpSystemId,'')=''";
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
        public string GetSalaryProcessedNotLockedMSG(DataSet ds)
        {
            string r = string.Empty;
            try
            {
                DataTable dt = new DataView(ds.Tables[0]).ToTable(true, "EmployeeCode", "IsLocked", "IsSalaryProcessed");
                //int cc = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    bool IsNotSalaryProcessed = string.IsNullOrEmpty(dt.Rows[i]["IsSalaryProcessed"].ToString());
                    bool IsNotLocked = string.IsNullOrEmpty(dt.Rows[i]["IsLocked"].ToString());
                    if (IsNotSalaryProcessed == false && IsNotLocked)//process and not locked
                    {
                        if (r.Length == 0)
                        {
                            r = "Last month's Salary is not locked for the following Employees:-" + Environment.NewLine;
                            r += " Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                        }
                        else
                        {
                            r += ", Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                        }
                    }//if
                }

                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetSalaryProcessedNotLockedMSGArrear(DataSet ds)
        {
            string r = string.Empty;
            try
            {
                //DataTable dtProcess = new DataView(ds.Tables[0]).ToTable(true, "EmployeeCode", "IsLocked", "IsSalaryProcessed");
                DataTable dtProcess = new DataView(ds.Tables[0]).ToTable(true, "IsSalaryProcessed");
                DataTable dtLock = new DataView(ds.Tables[0]).ToTable(true, "IsLocked");

                for (int i = 0; i < dtProcess.Rows.Count; i++)
                {
                    bool IsNotSalaryProcessed = string.IsNullOrEmpty(dtProcess.Rows[i]["IsSalaryProcessed"].ToString());
                    if (IsNotSalaryProcessed == false)//process and not locked
                    {
                        r += " Salary not processed ";
                        break;
                    }

                }
                for (int i = 0; i < dtLock.Rows.Count; i++)
                {
                    bool IsNotLocked = string.IsNullOrEmpty(dtLock.Rows[i]["IsLocked"].ToString());

                    if (IsNotLocked == false)
                    {
                        r += " Salary not locked ";
                        break;
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetSalaryProcessedLockedMSG(DataSet ds)
        {
            string r = string.Empty;
            try
            {
                DataTable dt = new DataView(ds.Tables[0]).ToTable(true, "EmployeeCode");
                //DataTable dtTab = new DataView(ds.Tables[0]).ToTable(true, "SystemId", "EmployeeCode", "EmployeeName", "DOJ", "DOS", "GivenDesignation", "LegalDesignation", "EmployeeStatus", "Subsection", "Section");
                // DGAttendanceNotLocked.DataSource = dtTab;
                // DGAttendanceNotLocked.DataBind();
                // tabAttNotLocked.Text = "Att. Not processed (" + dtTab.Rows.Count + ")";

                //int cc = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //cc++;
                    //if (cc < 10)
                    //{
                    if (r.Length == 0)
                    {
                        r = "Salary is locked for the following Employees:-" + Environment.NewLine;
                        r += " Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                    }
                    else
                    {
                        r += ", Employee [" + dt.Rows[i]["EmployeeCode"].ToString() + "]" + Environment.NewLine;
                    }
                    //}
                    //else
                    //{

                    //}
                }
                //if (r.Length > 0)
                //{
                //    r += "...see employee list in [Attendance not processed] Tab...";
                //    return r;
                //}
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ValidationSalaryLock(string emplist, string yearno, string monthno)
        {
            //clsSalaryProcessUI objel = null;
            DataSet dsSalaryLocked;
            try
            {
                //objel = new clsSalaryProcessUI();
                GetSalaryProcessedLockedEmp(emplist, yearno, monthno, out dsSalaryLocked);
                string r = GetSalaryProcessedLockedMSG(dsSalaryLocked);
                if (r.Length > 0)
                {
                    throw new Exception(r);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ValidationSalaryLockPreviousMonth(string emplist, string yearno, string monthno)
        {
            //clsSalaryProcessUI objel = null;
            DataSet dsSalaryLocked;
            try
            {
                //objel = new clsSalaryProcessUI();
                GetSalaryProcessedUnlockedLockedEmp(emplist, yearno, monthno, out dsSalaryLocked);
                string r = GetSalaryProcessedNotLockedMSG(dsSalaryLocked);
                if (r.Length > 0)
                {
                    throw new Exception(r);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ValidationSalaryLockForArrear(string emplist, string yearno, string monthno)
        {

            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (emplist.Length == 0)
                {
                    emplist = "''";
                }

                string strSQL = @"SELECT TOP 1 ei.SystemId,k.EmpInfoSystemID
                                  FROM EmployeeInformation AS ei
                                        LEFT JOIN (
                                                    SELECT * FROM (SELECT DENSE_RANK() OVER (PARTITION BY c.EmpInfoSystemID ORDER BY  C.SystemID) AS RNK,C.EmpInfoSystemID
                                                    FROM SalaryProcMaster M
                                                    JOIN SalaryProcChild AS c ON c.SlrProcMstSystemID=m.SystemID
                                                    WHERE m.MonthNo=" + monthno + @" AND M.YearNo=" + yearno + @") AS K WHERE K.RNK=1
                                                  ) AS K ON k.EmpInfoSystemID=ei.SystemId
                                WHERE ei.SystemId IN (" + emplist + @") AND ISNULL(k.EmpInfoSystemID,'')=''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out DataSet dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                    throw new Exception("Salary not processed");


                strSQL = @"SELECT TOP 1 ei.SystemId,k.EmpSystemId
                                FROM EmployeeInformation AS ei
                                LEFT JOIN salarylock K ON k.EmpSystemId=ei.SystemId AND k.MonthNo=" + monthno + @" AND k.YearNo=" + yearno + @"
                                WHERE ei.SystemId IN (" + emplist + @") AND ISNULL(k.EmpSystemId,'')=''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                    throw new Exception("Salary not locked");


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    public class AllDataset
    {
        //DataSet dslocal,string FromDate,string ToDate,string PlantId,out DataSet dsNewlyJoined
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string PlantId { get; set; }
        public bool IsNegativeSalaryApplicable { get; set; } = false;
        public string NegativeSalaryHeadId { get; set; }
        public List<ActiveEmp> dtActive { get; set; }
        public List<ActiveEmp> dtNewlyJoined { get; set; }
        public List<ActiveEmp> dtSND { get; set; }
        public List<ActiveEmp> dtSNA { get; set; }
        public List<ExceptionEmp> dtEXemp { get; set; }
        public List<ActiveEmp> dtAttNotProcessed { get; set; }
        public List<ActiveEmp> dtPresetZero { get; set; }
        public List<ActiveEmp> dtApprovedSalary { get; set; }
        public List<MaternityRetun> dtMaternityReturn { get; set; }
        public List<ActiveEmp> dtSeparated { get; set; }
        public List<ActiveEmp> dtDifferentStatus { get; set; }
    }
    public class ActiveEmp
    {
        public bool IsSelectSlrProc { get; set; }
        public bool IsDisbursed { get; set; }
        public bool IsApproved { get; set; }
        public string IsLocked { get; set; }
        public string SystemID { get; set; }
        public string EmpSystemID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string PlantName { get; set; }
        public string DOJ { get; set; }
        public string DOS { get; set; }
        public string EmployeeStatus { get; set; }
        public string DesignationGroup { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string SalaryRuleName { get; set; }
        public string PlantID { get; set; }
        public string ToDate { get; set; }
        public string ProcessStatus { get; set; }
        public string BankAccountStatus { get; set; }
        public string GivenDesignationId { get; set; }

        public string LegalDesignationId { get; set; }
        public string PaymentMode { get; set; }
        public string BudgetCode { get; set; }
        public string BankSystemID { get; set; }
        public string BankBranchId { get; set; }
        public string BankAccNo { get; set; }
        public decimal SalaryPercentage { get; set; }
        public string EmployeeCategoryId { get; set; }
        public string LegalSalaryGradeId { get; set; }
        public string IFSCCode { get; set; }
        public string MICRCode { get; set; }
        public string AccountsGroup { get; set; }



    }
    public class MaternityRetun
    {
        public bool IsSelectSlrProc { get; set; }
        public bool IsDisbursed { get; set; }
        public bool IsApproved { get; set; }
        public string SystemID { get; set; }
        public string EmpSystemID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public int BabyNo { get; set; }
        public string DOJ { get; set; }
        public string DOS { get; set; }
        public string EmployeeStatus { get; set; }
        public string WithBenefit { get; set; }
        public string GoingON { get; set; }
        public string MLVTo { get; set; }
        public string LeaveStatus { get; set; }
        public string LegalDesignation { get; set; }
        public string ProcessStatus { get; set; }
        public string Section { get; set; }
        public string Subsection { get; set; }

        public string LegalDesignationId { get; set; }
        public string PaymentMode { get; set; }
        public string BudgetCode { get; set; }
        public string BankSystemID { get; set; }
        public string BankBranchId { get; set; }
        public string BankAccNo { get; set; }
        public decimal SalaryPercentage { get; set; }
        public string EmployeeCategoryId { get; set; }
        public string LegalSalaryGradeId { get; set; }
        public string GivenDesignationId { get; set; }
        public string IFSCCode { get; set; }
        public string MICRCode { get; set; }
    }
    public class ExceptionEmp
    {
        public string SystemID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string LegalDesignation { get; set; }
        public string Section { get; set; }
        public string Subsection { get; set; }
        public string DOJ { get; set; }
        public string DOS { get; set; }
        public string EmployeeStatus { get; set; }
    }
    enum ListEnum
    {
        NewlyJoined,
        Separated,
        Active,
        MaternityReturn,
        SalaryStructureNotDefined,
        SalaryStructureNotApproved,
        ExceptionEmp,
        AttendanceNotProcessed,
        ApprovedSalary,
        PresentDaysZero,
        DifferentStatus,
        All
    }
}
