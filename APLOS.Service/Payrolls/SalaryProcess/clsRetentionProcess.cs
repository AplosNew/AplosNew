using bplib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace OTSBD
{
    public class xclsRetentionProcess
    {
        public string sFormulaValue = "";

        public xclsRetentionProcess()
        {
            // TODO: Add constructor logic here
        }
       
        public void GetSalaryHead(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryHead";

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

        public void GetEmployeeListForRetentionDetailsWise(ParaListForRet para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            string sSQLExten = "";
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if(para.sEmpSystemID!="")
                { sSQLExten = " AND E.SystemId IN (" + para.sEmpSystemID + ")"; }

                strSQL = @"SELECT AB.EmpSystemID, AB.DOJ, AB.LegalSalaryGradeId OrgLegalSalaryGradeId, AB.ExperienceSpan OrgExperienceSpan, 
	                               RAD.ID RetetionAllowDtlID, RAM.IsAbsentismApplicable, RAD.ExperienceSpan RetExperienceSpan, RAD.Amount 
                             FROM [SCS].[RetentionAllowanceDetail] RAD
				                            INNER JOIN
				                                       (
							                            SELECT ID, IsAbsentismApplicable, MAX(EffectiveDate) EffectiveDate 
							                             FROM [MST].[RetentionAllowanceMaster]
							                            WHERE CONVERT(DATE, EffectiveDate) <= CONVERT(DATE, '" + para.ToDate + @"')
							                            GROUP BY ID, IsAbsentismApplicable
						                               ) RAM ON RAD.RetentionAllowanceMasterId = RAM.Id
				                            INNER JOIN
				                                      (
							                            SELECT EmpSystemID, DOJ, LegalSalaryGradeId, ExperienceSpan
							                            FROM 
							                            (
								                            SELECT SystemId EmpSystemID, CONVERT(DATE, E.DOJ) DOJ, LSGD.LegalSalaryGradeId, 
									                               FORMAT(DATEDIFF(D, CONVERT(DATE, E.DOJ), CONVERT(DATE, '" + para.ToDate + @"')) / 365.0, 'N4') ExperienceSpan 
								                            FROM [dbo].[EmployeeInformation] E
												                            INNER JOIN [MST].[DesignationMaster] DM ON E.GivenDesignationId = DM.DesignationId
												                            INNER JOIN [MST].[DesignationMasterLegalDesignation] DMLD ON DM.Id = DMLD.DesignationMasterId
												                            INNER JOIN [MST].[LegalSalaryGradeDesignation] LSGD ON LSGD.LegalDesignationId = DMLD.LegalDesignationId
												                            INNER JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id
								                            WHERE E.PlantId = '" + para.PlantID + @"' 
                                                                  " + sSQLExten + @"
							                            ) A
							                            GROUP BY EmpSystemID, DOJ, LegalSalaryGradeId, ExperienceSpan
						                              ) AB ON RAD.LegalSalaryGradeId = AB.LegalSalaryGradeId 
                             WHERE AB.ExperienceSpan >= RAD.ExperienceSpan
                             ORDER BY AB.EmpSystemID";

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
        public void GetRetentionAllowEmployee(ParaListForRet para, string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            string sSQLExten = "";
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                { sSQLExten = " WHERE (" + sEmpSystemID + ")"; }

                strSQL = @"SELECT A.* FROM [dbo].[RetentionAllowEmployee] A
                            INNER JOIN (
                                        SELECT EmpSystemID, MAX(StartDate) StartDate FROM [dbo].[RetentionAllowEmployee]
							              WHERE CONVERT(DATE, StartDate) <= CONVERT(DATE, '" + para.ToDate + @"')
                                        GROUP BY EmpSystemID
                                       ) B ON A.EmpSystemID = B.EmpSystemID AND A.StartDate = B.StartDate
                             " + sSQLExten + @"
                             ORDER BY EmpSystemID";

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
        public void GetRetentionAllowMonthWise(ParaListForRet para, string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            string sSQLExten = "";
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                { sSQLExten = " AND (" + sEmpSystemID + ")"; }

                strSQL = @"SELECT * FROM [dbo].[RetentionAllowMonthWise]
                            WHERE RetenAllowEmpSystemID IN (
                                                            SELECT A.ID FROM [dbo].[RetentionAllowEmployee] A
                                                                INNER JOIN (
                                                                            SELECT EmpSystemID, MAX(StartDate) StartDate FROM [dbo].[RetentionAllowEmployee]
							                                                  WHERE CONVERT(DATE, StartDate) <= CONVERT(DATE, '" + para.ToDate + @"')
                                                                            GROUP BY EmpSystemID
                                                                           ) B ON A.EmpSystemID = B.EmpSystemID AND A.StartDate = B.StartDate
                                                            WHERE A.IsApproved = 1
                                                                  " + sSQLExten + @"
                                                           )
                                  AND MonthNo = Month(CONVERT(DATE, '" + para.ToDate + @"')) 
                                  AND YearNo = Year(CONVERT(DATE, '" + para.ToDate + @"')) 
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
        }

        public void GenRefSrNoID(string strEntryDate, string strFieldName, int SrNo, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            //int lngRecCount=0;
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;

            System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                //strEntryDate = AppDateConvert(strEntryDate, getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();

                strSql = "SELECT * FROM Signature WHERE Field = '" + strFieldName.Trim() + "' AND Dates = '" + strEntryDate + "'";

                SB = new System.Text.StringBuilder(strEntryDate);
                strID = SB.Replace(getUserDateSeparator().ToString(), "").ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();

                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and Dates = '" + strEntryDate + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    LastNumber = 1 + SrNo;

                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + SrNo;

                    drLocal.BeginEdit();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = strID + "-" + ((int)LastNumber - SrNo);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
        public static DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
        {
            string strDate = null;
            dateValue = chk_NullDateData(dateValue);
            strDate = dateValue.ToString();
            if (strDate != "")
            {
                if (input_date_format.Trim() != "")
                {
                    if (output_date_format.Trim() != "")
                    {
                        System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                        InputFormat.ShortDatePattern = input_date_format;
                        DateTime myDt = Convert.ToDateTime(strDate, InputFormat);
                        strDate = myDt.ToString(output_date_format);
                    }
                }
            }
            return Convert.ToDateTime(strDate);
        }//End Function
        public static object chk_NullDateData(object dateValue)
        {
            if (DateOkCheck("" + dateValue.ToString()) == false)
            {
                dateValue = "";
            }

            if (("" + dateValue.ToString()) == "")
            {
                DateTime dt = new DateTime(1901, 1, 1);
                dateValue = (object)dt;
            }
            return (object)dateValue;
        }//End function
        private static bool DateOkCheck(string strdate)
        {
            try
            {
                DateTime myDt = Convert.ToDateTime(strdate);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                //
            }
        }//End Function
        public static string getUserDateFormat()
        {
            System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            return USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString();
        }//End Function
        public static string getUserDateSeparator()
        {
            System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            return USER_TERMINAL_DATE_FORMAT.DateSeparator.ToString();
        }//End Function
        public static object RetValidLen(string str, int How_Long_Should_It_Be)
        {

            string removechar = "";
            if (str.Trim() == "")
            {
                return (object)Convert.DBNull;
            }
            removechar = str.Trim();
            removechar = removechar.Replace("'", " ");
            if ((removechar.Trim()).Length > How_Long_Should_It_Be)
            {
                return (object)(removechar.Substring(1, How_Long_Should_It_Be));
            }
            else
            {
                return (object)removechar.Trim();
            }
        }//End Function
        public static object RetValidLen(string str)
        {

            string removechar = "";
            if (str.Trim() == "")
            {
                return (object)Convert.DBNull;
            }
            removechar = str.Trim();
            removechar = removechar.Replace("'", " ");
            ////if ((removechar.Trim()).Length > How_Long_Should_It_Be)
            ////{
            ////    return (object)(removechar.Substring(1, How_Long_Should_It_Be));
            ////}
            ////else
            ////{
            ////    return (object)removechar.Trim();
            ////}
            return (object)removechar.Trim();

        }//End Function
        public static string GetNumData(string strNumber)
        {
            double d;
            strNumber = strNumber.Replace(",", "");
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0"; }
            else if (Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return strNumber;
            }
            else
            {
                return "0";
            }
        }//End function
        public void SaveDataSets(params DataSet[] dsRef)
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
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }

        }//End Function  

        private void ReLoadFormulaWithValue(ParaListForRet para, string sFormulaID, bool bEarning, ref DataTable dtValue, ref DataTable dtSlrHd)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;
            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = sFormulaID.Trim();
                string sLocalCurrencyID = para.LocalCurrencyID;
                string sForeignCurRate = para.ForeignCurRate;

                if(sForeignCurRate == "")
                { sForeignCurRate = "1"; }

                sFormulaValue = "";

                strFormulaIDTemp = strFormulaIDTemp.Replace("(", " ( ");
                strFormulaIDTemp = strFormulaIDTemp.Replace(")", " ) ");

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView();
                        dvLocal.Table = dtValue;

                        dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                        if (dvLocal.Count == 1)
                        {
                            if (bEarning == false)
                            {
                                if (dvLocal[0]["EntryCurrencyID"].ToString().Trim() == sLocalCurrencyID.Trim())
                                {
                                    strTemp = dvLocal[0]["Amount"].ToString().Trim();
                                }
                                else
                                {
                                    strTemp = (Convert.ToDecimal(dvLocal[0]["Amount"].ToString().Trim()) * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                }
                            }
                            else
                            {
                                decimal decAmount = Convert.ToDecimal(dvLocal[0]["EarningAmount"].ToString().Trim());

                                if(decAmount == 0)
                                { decAmount = Convert.ToDecimal(dvLocal[0]["Amount"].ToString().Trim()); }

                                if (dvLocal[0]["EarningCurrencyID"].ToString().Trim() == sLocalCurrencyID.Trim())
                                {
                                    strTemp = dvLocal[0]["EarningAmount"].ToString().Trim();
                                }
                                else
                                {
                                    strTemp = (decAmount * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                }
                            }
                        }
                        else
                        {
                            dvSlrHd = new DataView();
                            dvSlrHd.Table = dtSlrHd;
                            dvSlrHd.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                            if (dvSlrHd.Count == 1)
                            {
                                strTemp = "0.00";
                            }
                        }
                    }

                    sFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 
        public static double Evaluate(string expression)
        {
            // That is some code instruction, is'nt it?
            return (double)new System.Xml.XPath.XPathDocument
            (new StringReader("<r/>")).CreateNavigator().Evaluate
            (string.Format("number({0})", new
            System.Text.RegularExpressions.Regex(@"([\+\-\*])")
            .Replace(expression, " ${1} ")
            .Replace("/", " div ")
            .Replace("%", " mod ")));
        }//End Function 
        public void LoadEmpSlrDefForSlrProcess(ParaListForRet para, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM 
		                            (
                                     SELECT SD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                                            SD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate,	
                                            SD.EntryCurrencyID, ECR.Name AS EntryCurrency, SD.EntryAmount, SD.DefineCurrencyID, SD.SalaryID,
                                            DECR.Name AS DefinitionCurrency, SD.DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                              ELSE SD.SalaryHeadID END,
			                                CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay, SlrDis.IsBankPayment, SlrDis.IsCashPayment,
                                            ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
											IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
																  ELSE SlrDis.IsNetPayEffect END,
											SlrProc.DisbusmentAmount EarningAmount, SlrProc.DisbusmentCurrencyID EarningCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo, SRM.CurrencyRuleSystemID  
		                            FROM (
                                          SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
	                                             AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
                                          FROM SalaryInfoDefine
                                            UNION
                                          (
                                           SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
		                                          AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated                   
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
				                                                            WHERE IsApproved = 1 AND EffectiveDate <= '" + para.ToDate + @"'
				                                                            GROUP BY EmpInfoSystemID
			                                                            ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
												) SEFD ON SD.SalaryID = SEFD.SystemID
			                            INNER JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
			                            INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
                                        INNER JOIN SalaryRuleRetentionPmtMaster SRRPM ON SRM.SystemID = SRRPM.SalaryRuleMasterSystemID
			                            INNER JOIN SalaryHead SH ON SRRPM.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') = 'Retention Allowance'
			                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SD.SalaryHeadID = CRC.SalaryHeadID
			                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
			                            LEFT JOIN 
					                            (
                                                 SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Gen' RuleType, IsGNRNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, ISNULL(IsBankPayment, Convert(bit, 'True')) IsBankPayment, ISNULL(IsCashPayment, Convert(bit, 'True')) IsCashPayment,
						                                IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleGeneral
						                            UNION
					                             (
                                                  SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Abs' RuleType, IsAbsNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, Convert(bit, 'True') IsBankPayment, Convert(bit, 'True') IsCashPayment, 
						                                 IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleAbsenteeism
                                                 )
                                                ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SD.SalaryHeadID = SlrDis.SalaryHeadID
			                            LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
											                            AND SD.SalaryHeadID = SRDSM.SalaryHeadID
										LEFT JOIN 
										       (
												SELECT * FROM [dbo].[SalaryProcChild]
													WHERE SlrProcMstSystemID IN (
																				 SELECT SystemID FROM [dbo].[SalaryProcMaster]
																				  WHERE MonthNo = MONTH('" + para.ToDate + @"') AND YearNo = YEAR('" + para.ToDate + @"')
																				)
											   ) SlrProc ON E.SystemID = SlrProc.EmpInfoSystemID 
											                            AND SD.SalaryHeadID = SlrProc.SalaryHeadID 
                                        WHERE E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.ToDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                                                                               OR E.DOS = '' OR E.DOS = '01/01/1901')
                                              AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + para.ToDate + @"'
                                    ) A 
                                  WHERE (" + sEmpInfo + @") ";
                if (para.PlantID != "ALL" & para.PlantID != "")
                {
                    strSql += @" AND PlantID = '" + para.PlantID + @"' ";
                }

                strSql += @" ORDER BY EmpInfoSystemID, HeadType DESC";

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
        public void LoadCurrencyRule(ParaListForRet para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT CRC.SystemID CurrencyRuleChildSystemID, CRC.MstSystemID CurrencyRuleSystemID, CRC.SalaryHeadID, SD.HeadType, 
                                  CRC.AmtEntryCurrency, ECR.Code AS EntryCrc, CRC.AmtDefinitionCurrency, DECR.Code AS DefinCr,
                                  CRC.AmtDisbusmentCurrency, DICR.Code AS DisbCr, CRC.AccumulateExchangeRate, 
                                  CRC.AccumulateExchangeSalaryHeadID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                  ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
                            FROM CurrencyRuleChild CRC
												INNER JOIN CurrencyRuleMaster CRM ON CRC.MstSystemID = CRM.SystemID
					                            LEFT JOIN SCS.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                                    LEFT JOIN SCS.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                                    LEFT JOIN SCS.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id 
                                                LEFT JOIN SalaryHead SD ON CRC.SalaryHeadID = SD.SalaryHeadID
                            WHERE CRM.PlantId = '" + para.PlantID + @"'";

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

        public void GeneratorRetentionAllowanceEmployee(ParaListForRet para)
        {
            #region Variable Dataset
            
            DataSet dsRetAllowEmp = null;
            DataTable dtRetAllowEmp = null;
            DataRow drRetAllowEmp = null;
            DataView dvRetAllowEmp = null;

            DataSet dsRetAllowMntEmpWiseCal = null;
            DataTable dtRetAllowMntEmpWiseCal = null;
            DataRow drRetAllowMntEmpWiseCal = null;
            DataView dvRetAllowMntEmpWiseCal = null;

            DataSet dsSalInfo = null;
            DataSet dsEmpLstRetDtl = null;
            //DataSet dsSalHd = null;
            //DataTable dtSalHd = null;
            //DataView dvSlrHd = null;

            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;

            //clsSalaryStructureAplos obSS = new global::clsSalaryStructureAplos();

            #endregion Variable Dataset
            #region Declare Variable

            string sRetetionAllowDtlID = "";
            string sPlantID = para.PlantID;
            string sRetnAlwEmpGentID = "";
            string sRetnAlwMntGentID = "";

            string sFinlRetnAlwEmpGentID = "";
            string sFinlRetnAlwMntGentID = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemIDColl = "";
            string sEmpSystemID = "";
            string sSlrHD = "";
            string sCurrencyRuleSystemID = "";
            decimal decValue = 0;

            bool bAbsentismApplicable = false;

            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            
            #endregion Declare Variable

            try
            {
                #region Retention Allowance Employee List

                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetEmployeeListForRetentionDetailsWise(para, out dsEmpLstRetDtl);
                if (dsEmpLstRetDtl.Tables[0].Rows.Count > 0)
                {
                    clsGenID objPK = new clsGenID();
                    sEmpInfoSysIDColl = "";
                    sEmpSystemIDColl = "";
                    TotSelectEmpForProc = dsEmpLstRetDtl.Tables[0].Rows.Count;
                    TotProcComp = 0;
                    grdRowMaxCnt = 0;
                    SelectedEmpCnt = 0;
                    EmpCntForLoop = 0;

                    sRetnAlwEmpGentID = "";
                    sRetnAlwMntGentID = "";

                    objPK.GenID(DateTime.Now.ToString("dd-MMM-yyyy"), "RETN_ALW_EMP", out sRetnAlwEmpGentID);
                    //GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "RETN_ALW_EMP", dsEmpLstRetDtl.Tables[0].Rows.Count, out sRetnAlwEmpGentID);
                    sRetnAlwEmpGentID = "RE" + sRetnAlwEmpGentID;

                    objPK.GenID(DateTime.Now.ToString("dd-MMM-yyyy"), "RETN_ALW_MNT", out sRetnAlwMntGentID);
                    //GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "RETN_ALW_MNT", dsEmpLstRetDtl.Tables[0].Rows.Count, out sRetnAlwMntGentID);
                    sRetnAlwMntGentID = "RM" + sRetnAlwMntGentID;

                    while (SelectedEmpCnt < dsEmpLstRetDtl.Tables[0].Rows.Count)
                    {
                        sEmpSystemIDColl = "";
                        EmpCntForLoop = 0;

                        if ((SelectedEmpCnt + 1) <= dsEmpLstRetDtl.Tables[0].Rows.Count)
                        {
                            grdRowMaxCnt = dsEmpLstRetDtl.Tables[0].Rows.Count - TotProcComp;
                        }
                        else
                        {
                            grdRowMaxCnt = 30;
                        }

                        #region Employee System ID Collection

                        for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                        {
                            if (string.IsNullOrEmpty(sEmpSystemIDColl) == true)
                            {
                                sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsEmpLstRetDtl.Tables[0].Rows[iUnTgEmCnt]["EmpSystemID"].ToString().Trim() + "'";
                                sEmpSystemIDColl = "A.EmpSystemID = '" + dsEmpLstRetDtl.Tables[0].Rows[iUnTgEmCnt]["EmpSystemID"].ToString().Trim() + "'";
                            }
                            else
                            {
                                sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsEmpLstRetDtl.Tables[0].Rows[iUnTgEmCnt]["EmpSystemID"].ToString().Trim() + "'";
                                sEmpSystemIDColl += " OR A.EmpSystemID = '" + dsEmpLstRetDtl.Tables[0].Rows[iUnTgEmCnt]["EmpSystemID"].ToString().Trim() + "'";
                            }
                            EmpCntForLoop++;
                        }

                        #endregion Employee System ID Collection

                        if (EmpCntForLoop == grdRowMaxCnt)
                        {
                            #region DataSet

                            GetRetentionAllowEmployee(para, sEmpSystemIDColl, out dsRetAllowEmp);
                            dtRetAllowEmp = dsRetAllowEmp.Tables[0];
                            dvRetAllowEmp = new DataView();

                            GetRetentionAllowMonthWise(para, sEmpSystemIDColl, out dsRetAllowMntEmpWiseCal);
                            dtRetAllowMntEmpWiseCal = dsRetAllowMntEmpWiseCal.Tables[0];
                            dvRetAllowMntEmpWiseCal = new DataView();

                            List<dicSalInfo> dicSalInfo = new List<dicSalInfo>();
                            LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                            if (dsSalInfo.Tables[0].Rows.Count > 0)
                                dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfo>();

                            #endregion DataSet
                            //clsGenID objPK = new clsGenID();
                            //objPK.GenID(DateTime.Now.ToString("dd-MMM-yyyy"), "RETN_ALW_EMP", out sRetnAlwEmpGentID);
                            //objPK.GenID(DateTime.Now.ToString("dd-MMM-yyyy"), "RETN_ALW_EMP", out sRetnAlwEmpGentID);
                            int _Count = 0;
                            for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                            {
                                _Count += 1;
                                sSlrHD = "";
                                bAbsentismApplicable = Convert.ToBoolean(dsEmpLstRetDtl.Tables[0].Rows[iUnTgEmCnt]["IsAbsentismApplicable"].ToString().Trim());
                                sRetetionAllowDtlID = dsEmpLstRetDtl.Tables[0].Rows[iUnTgEmCnt]["RetetionAllowDtlID"].ToString().Trim();
                                sEmpSystemID = dsEmpLstRetDtl.Tables[0].Rows[iUnTgEmCnt]["EmpSystemID"].ToString().Trim();

                                //sFinlRetnAlwEmpGentID = sRetnAlwEmpGentID + "-" + (iUnTgEmCnt + 1).ToString();
                                //sFinlRetnAlwMntGentID = sRetnAlwMntGentID + "-" + (iUnTgEmCnt + 1).ToString();

                                sFinlRetnAlwEmpGentID = sRetnAlwEmpGentID + "-" + _Count;
                                sFinlRetnAlwMntGentID = sRetnAlwMntGentID + "-" + _Count;

                                decValue = Convert.ToDecimal(dsEmpLstRetDtl.Tables[0].Rows[iUnTgEmCnt]["Amount"].ToString().Trim());
                                
                                var dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == sEmpSystemID.Trim());
                                if (dicSalInfo_Sub.Count > 0)
                                {
                                    sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                    for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                    {
                                        sSlrHD = dicSalInfo_Sub[i].SalaryHeadID;
                                    }
                                }

                                #region Data Save IN Table [RetentionAllowEmployee]

                                dvRetAllowEmp.Table = dtRetAllowEmp;
                                dvRetAllowEmp.RowFilter = "RetetionAllowDtlID = '" + sRetetionAllowDtlID + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                                if (dvRetAllowEmp.Count == 0)
                                {//Add new block
                                    drRetAllowEmp = dtRetAllowEmp.NewRow();
                                    UpdateTheDataRowInTableRetentionAllowEmployee("ADDNEW", sFinlRetnAlwEmpGentID,  sEmpSystemID, sRetetionAllowDtlID, para.ToDate, para.sUser, ref drRetAllowEmp);
                                    dtRetAllowEmp.Rows.Add(drRetAllowEmp);
                                }
                                else
                                {//Edit block
                                    sFinlRetnAlwEmpGentID = dvRetAllowEmp[0].Row["ID"].ToString();
                                    //drRetAllowEmp = dvRetAllowEmp[0].Row;
                                    //drRetAllowEmp.BeginEdit();
                                    //UpdateTheDataRowInTableRetentionAllowEmployee("EDIT", sFinlRetnAlwEmpGentID, sEmpSystemID, sRetetionAllowDtlID, para.ToDate, para.sUser, ref drRetAllowEmp);
                                    //drRetAllowEmp.EndEdit();

                                    ///by monir 180901
                                    //if (Convert.ToBoolean(dvRetAllowEmp[0].Row["IsApproved"].ToString()) == true)
                                    //{
                                        #region Data Save IN Table [RetentionAllowMonthWise]

                                        dvRetAllowMntEmpWiseCal.Table = dtRetAllowMntEmpWiseCal;
                                        dvRetAllowMntEmpWiseCal.RowFilter = "RetenAllowEmpSystemID = '" + sFinlRetnAlwEmpGentID + "' AND MonthNo = '" + Convert.ToDateTime(para.ToDate).Month + "' AND YearNo = '" + Convert.ToDateTime(para.ToDate).Year + "'";
                                        if (dvRetAllowMntEmpWiseCal.Count == 0)
                                        {//Add new block
                                            drRetAllowMntEmpWiseCal = dtRetAllowMntEmpWiseCal.NewRow();
                                            UpdateTheDataRowInTableRetentionAllowMonthWise("ADDNEW", sFinlRetnAlwMntGentID, sFinlRetnAlwEmpGentID, sSlrHD, decValue, para.ToDate, para.sUser, ref drRetAllowMntEmpWiseCal);
                                            dtRetAllowMntEmpWiseCal.Rows.Add(drRetAllowMntEmpWiseCal);
                                        }
                                        else
                                        {//Edit block
                                            sFinlRetnAlwMntGentID = dvRetAllowMntEmpWiseCal[0].Row["ID"].ToString();
                                            drRetAllowMntEmpWiseCal = dvRetAllowMntEmpWiseCal[0].Row;
                                            drRetAllowMntEmpWiseCal.BeginEdit();
                                            UpdateTheDataRowInTableRetentionAllowMonthWise("EDIT", sFinlRetnAlwMntGentID, sFinlRetnAlwEmpGentID, sSlrHD, decValue, para.ToDate, para.sUser, ref drRetAllowMntEmpWiseCal);
                                            drRetAllowMntEmpWiseCal.EndEdit();
                                        }
                                        #endregion Data Save IN Table [RetentionAllowMonthWise]
                                    //}
                                }
                                #endregion Data Save IN Table [RetentionAllowEmployee]
                            }
                        }

                        TotProcComp += grdRowMaxCnt;
                        TotSelectEmpForProc -= grdRowMaxCnt;
                        SaveDataSets(dsRetAllowEmp, dsRetAllowMntEmpWiseCal);
                        
                        if ((dsEmpLstRetDtl.Tables[0].Rows.Count - TotProcComp) < 30)
                        {
                            SelectedEmpCnt += (dsEmpLstRetDtl.Tables[0].Rows.Count - TotProcComp);

                            if (SelectedEmpCnt <= 0)
                            { SelectedEmpCnt = dsEmpLstRetDtl.Tables[0].Rows.Count + 1; }
                        }
                        else
                        {
                            SelectedEmpCnt += 30;
                        }
                        dsRetAllowEmp = null;
                        dsRetAllowMntEmpWiseCal = null;
                    }
                }

                #endregion Retention Allowance Employee List
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Clear DataSet 

                dsRetAllowEmp = null;
                dtRetAllowEmp = null;
                drRetAllowEmp = null;
                dvRetAllowEmp = null;

                dsRetAllowMntEmpWiseCal = null;
                dtRetAllowMntEmpWiseCal = null;
                drRetAllowMntEmpWiseCal = null;
                dvRetAllowMntEmpWiseCal = null;

                dsEmpLstRetDtl = null;
                //dsSalHd = null;
                //dtSalHd = null;
                //dvSlrHd = null;

                #endregion Clear DataSet 
            }
        }///End Function
       
        private void UpdateTheDataRowInTableRetentionAllowEmployee(string OPN_FLAG, string sFinlRetnAlwEmpGentID, string sEmpSystemID, string sRetetionAllowDtlID, string sToDate, string sUser, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["ID"] = RetValidLen(sFinlRetnAlwEmpGentID.Trim());

                    drLocal["AddedBy"] = RetValidLen(sUser);
                    drLocal["AddedDate"] = DateTime.Now.ToString();
                    drLocal["AddedFromIP"] = "";

                    drLocal["IsApproved"] = false;
                    drLocal["ApprovedBy"] = RetValidLen(sUser);
                    drLocal["ApprovedDate"] = DateTime.Now.ToString();
                    drLocal["ApprovedFromIP"] = "";
                }
                
                drLocal["EmpSystemID"] = RetValidLen(sEmpSystemID.Trim());
                drLocal["RetetionAllowDtlID"] = RetValidLen(sRetetionAllowDtlID.Trim());
                drLocal["StartDate"] = sToDate;
                
                drLocal["UpdatedBy"] = RetValidLen(sUser);
                drLocal["UpdatedDate"] = DateTime.Now.ToString();
                drLocal["UpdatedFromIP"] = "";
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
        private void UpdateTheDataRowInTableRetentionAllowMonthWise(string OPN_FLAG, string sFinlRetnAlwMntGentID, string sFinlRetnAlwEmpGentID, string sSalaryHeadID, decimal decValue, string sToDate, string sUser, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["ID"] = RetValidLen(sFinlRetnAlwMntGentID);

                    drLocal["AddedBy"] = RetValidLen(sUser);
                    drLocal["AddedDate"] = DateTime.Now.ToString();
                    drLocal["AddedFromIP"] = "";
                }

                drLocal["RetenAllowEmpSystemID"] = RetValidLen(sFinlRetnAlwEmpGentID);

                if (sSalaryHeadID != "")
                {
                    drLocal["SalaryHeadID"] = sSalaryHeadID;
                }
                else
                {
                    drLocal["SalaryHeadID"] = DBNull.Value;
                }
                drLocal["MonthNo"] = Convert.ToDateTime(sToDate).Month;
                drLocal["YearNo"] = Convert.ToDateTime(sToDate).Year;
                drLocal["Amount"] = decValue;

                drLocal["UpdatedBy"] = RetValidLen(sUser);
                drLocal["UpdatedDate"] = DateTime.Now.ToString();
                drLocal["UpdatedFromIP"] = "";
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
    }

    namespace dd
    {
        public class ParaListForRet
        {
            public string PlantID { get; set; }
            public string sEmpSystemID { get; set; }
            public string LocalCurrencyID { get; set; }
            public string ForeignCurRate { get; set; }
            public string ToDate { get; set; }
            public string sUser { get; set; }
        }
    }
}