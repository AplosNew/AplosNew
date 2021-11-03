using System;
using System.Data;

namespace OTSBD
{
    public class clsTax
    {
        public clsTax()
        {
            // TODO: Add constructor logic here
        }

        #region Tax Group Tag With Employee

        public void GetTaxGrpInfo(string sGroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                            FROM TaxGroup WHERE GroupID = '" + sGroupID + @"'";

                strSQL = strSQL + " ORDER BY TaxGroupName";

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
        public void GetTaxGrpTagWithEmp(string sGroupID, string sPlantID, string sEmpSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.TaxGroupTagWithEmployee
	                        WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
                                    AND EmpInfoSystemID = '" + sEmpSysID + @"'";

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
        public void GetDefaultTaxGroup(string sGroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                            FROM TaxGroup WHERE GroupID = '" + sGroupID + @"' AND DefaultGroup = 1";

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

        #endregion Tax Group Tag With Employee

        #region Tax Policy

        public void GetTaxPeriodFromToDate(string strTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (strTaxYear != "")
                {
                    strSQL = @"SELECT TaxYearName, CONVERT(VARCHAR(12), StartDate, 106) Startmonth, CONVERT(VARCHAR(12), EndDate, 106) EndMonth 
                                    FROM scs.TaxYear  
                                    WHERE Id = '" + strTaxYear + @"'";
                }
                else
                {
                    strSQL = @"SELECT TaxYearName, CONVERT(VARCHAR(12), StartDate, 106) Startmonth, CONVERT(VARCHAR(12), EndDate, 106) EndMonth 
                                    FROM scs.TaxYear   
									ORDER BY TaxYearName DESC";
                }

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
        public void xGetTaxGroupWisePolicyMaster(string sGroupID, string sPlantID, string Gender, string strTaxGroupID, string strTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                            FROM TaxPolicyMaster 
                          WHERE TaxGroupID = '" + strTaxGroupID + @"' AND TaxYearID = '" + strTaxYear + @"' 
                                AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' and GenderId='" + Gender + "'";

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
        public void GetTaxGroupWisePolicyMaster(string sGroupID, string sPlantID,string Gender, string strTaxGroupID, string strTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string _wc = "";
                objCon = new ConnectionManager.DAL.ConManager("1");
                string _sql = "select distinct IsGenderSpecific from TaxPolicyMaster where TaxYearID='" + strTaxYear + @"' and TaxGroupID='" + strTaxGroupID + @"' and IsGenderSpecific=1";
                objCon.OpenDataSetThroughAdapter(_sql, out dsRef, false, "1");
                if(dsRef.Tables[0].Rows.Count>0)
                {//gender
                    _wc = " and GenderId='" + Gender + "'";
                }
                else
                {
                    _wc = "";
                }

                strSQL = @"SELECT * FROM TaxPolicyMaster 
                          WHERE TaxGroupID = '" + strTaxGroupID + @"'
                                AND TaxYearID = '" + strTaxYear + @"' 
                                AND GroupID = '" + sGroupID + @"' 
                                AND PlantID = '" + sPlantID + @"' "+_wc+"";

              
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
        public void xGetTaxGroupWisePolicyMaster(string sGroupID, string sPlantID, string strTaxGroupID, string strTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                            FROM TaxPolicyMaster 
                          WHERE TaxGroupID = '" + strTaxGroupID + @"' AND TaxYearID = '" + strTaxYear + @"' 
                                AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

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
        public void LoadGrdTaxPolicyGeneral(string sTaxPolicyMstSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TPX.SystemID, TPX.SalaryHeadID, SH.SalaryHead, SH.HeadCategory, TPX.IsTaxable, TPX.IsFixedTaxGeneral,  
									TPX.TaxFixedGeneral, TPX.IsPercentageTaxGeneral, TPX.TaxPercentageGeneral, TPX.IsExemption, 
									TPX.IsMaxExmpAmt, TPX.TaxMaxExmpAmt, TPX.IsExmBaseOnActual, TPX.IsExmBaseOnOtherSlrHd, 
                                    TPX.ExmSalaryHeadID, ESH.SalaryHead ExmSlrHd, TPX.PercentageExmAmtOtherSlrHd ExmAmtOtherSlrHd, TPX.IsExmWhichEverLess
                            FROM TaxPolicyGeneral TPX
		                            LEFT JOIN SalaryHead SH ON TPX.SalaryHeadID = SH.SalaryHeadID
					                LEFT JOIN SalaryHead ESH ON TPX.ExmSalaryHeadID = ESH.SalaryHeadID
                            WHERE TPX.TaxPolicyMstID = '" + sTaxPolicyMstSystemID + @"'";

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
        public void LoadTaxableIncomeSlrWiseDataOnGrid(string sGroupID, string sPlantID, string sEmpSystemID, string sTAXGroup, string sTAXYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TAISH.SystemID, TAISH.EmpInfoSystemID, TAISH.TaxDefineMasterSystemID, TAISH.TaxPolicyMstID, 
                                  TPM.TaxPolicyName, TAISH.TaxGroupID, TAISH.TaxYearID, TG.TaxGroupName, TAISH.SalaryHeadID, SH.SalaryHead, 
                                  TAISH.EntryIncomeCurrencyID, EC.Code EntryIncomeCurrency, TAISH.EntryIncome, 
                                  TAISH.DefinitionCurrencyID, DC.Code DefinitionCurrency, TAISH.DefinitionAmount, 
                                  TAISH.DefinitionCurrencyRate, TAISH.TaxPayablePeriod, TAISH.LocalCurrencyID, LC.Code LocalCurrency,
                                  TAISH.ConvertionRate, TAISH.YearlyIncome
                           FROM TaxableIncomeSalaryHeadWise TAISH
                                    INNER JOIN TaxDefineMaster TDM ON TAISH.TaxDefineMasterSystemID = TDM.SystemID AND TDM.TaxYearID = '" + sTAXYear + @"'
                                    INNER JOIN TaxGroup TG ON TAISH.TaxGroupID = TG.SystemID AND TG.SystemID = '" + sTAXGroup + @"'
                                    INNER JOIN TaxPolicyMaster TPM ON TAISH.TaxPolicyMstID = TPM.SystemID AND TPM.TaxYearID = '" + sTAXYear + @"'
                                    INNER JOIN SalaryHead SH ON TAISH.SalaryHeadID = SH.SalaryHeadID 
                                    LEFT JOIN scs.Currency EC ON TAISH.EntryIncomeCurrencyID = EC.Id
                                    LEFT JOIN scs.Currency DC ON TAISH.DefinitionCurrencyID = DC.Id
                                    LEFT JOIN scs.Currency LC ON TAISH.LocalCurrencyID = LC.Id
                           WHERE TAISH.GroupID = '" + sGroupID + @"'
                                 AND TAISH.PlantID = '" + sPlantID + @"'";

                if (sEmpSystemID != "")
                {
                    strSQL = strSQL + @" AND TAISH.EmpInfoSystemID IN ('" + sEmpSystemID + @"')";
                }

                strSQL = strSQL + @" 
                          ORDER BY TDM.TaxPaidUptoYear DESC, TDM.TaxPaidUptoMonth DESC";

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
        public void LoadTaxableIncomeSlrWiseDataOnGrid_Change(string MasterId,string sGroupID, string sPlantID, string sEmpSystemID, string sTAXGroup, string sTAXYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TAISH.SystemID, TAISH.EmpInfoSystemID, TAISH.TaxDefineMasterSystemID, TAISH.TaxPolicyMstID, 
                                  TPM.TaxPolicyName, TAISH.TaxGroupID, TAISH.TaxYearID, TG.TaxGroupName, TAISH.SalaryHeadID, SH.SalaryHead, 
                                  TAISH.EntryIncomeCurrencyID, EC.Code EntryIncomeCurrency, TAISH.EntryIncome, 
                                  TAISH.DefinitionCurrencyID, DC.Code DefinitionCurrency, TAISH.DefinitionAmount, 
                                  TAISH.DefinitionCurrencyRate, TAISH.TaxPayablePeriod, TAISH.LocalCurrencyID, LC.Code LocalCurrency,
                                  TAISH.ConvertionRate, TAISH.YearlyIncome
                           FROM TaxableIncomeSalaryHeadWise TAISH
                                    INNER JOIN (select * from TaxDefineMaster where systemid='"+ MasterId + @"') TDM ON TAISH.TaxDefineMasterSystemID = TDM.SystemID AND TDM.TaxYearID = '" + sTAXYear + @"'
                                    INNER JOIN TaxGroup TG ON TAISH.TaxGroupID = TG.SystemID AND TG.SystemID = '" + sTAXGroup + @"'
                                    INNER JOIN TaxPolicyMaster TPM ON TAISH.TaxPolicyMstID = TPM.SystemID AND TPM.TaxYearID = '" + sTAXYear + @"'
                                    INNER JOIN SalaryHead SH ON TAISH.SalaryHeadID = SH.SalaryHeadID 
                                    LEFT JOIN scs.Currency EC ON TAISH.EntryIncomeCurrencyID = EC.Id
                                    LEFT JOIN scs.Currency DC ON TAISH.DefinitionCurrencyID = DC.Id
                                    LEFT JOIN scs.Currency LC ON TAISH.LocalCurrencyID = LC.Id
                           WHERE TAISH.GroupID = '" + sGroupID + @"'
                                 AND TAISH.PlantID = '" + sPlantID + @"'";

                if (sEmpSystemID != "")
                {
                    strSQL = strSQL + @" AND TAISH.EmpInfoSystemID IN ('" + sEmpSystemID + @"')";
                }

                strSQL = strSQL + @" 
                          ORDER BY TDM.TaxPaidUptoYear DESC, TDM.TaxPaidUptoMonth DESC";

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
        public void LoadYearlyTaxableIncomeDataOnGrid(string sEmpSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TAYAISHW.SystemID, TAYAISHW.EmpInfoSystemID, TAYAISHW.TaxPolicyMstID, TAYAISHW.TaxGroupID, 
                                    TAYAISHW.SalaryHeadID, SH.SalaryHead, TAYAISHW.YearlyIncome, TPG.IsExemption, TPG.IsMaxExmpAmt, 
									TPG.TaxMaxExmpAmt, TPG.IsExmBaseOnActual, TPG.IsExmBaseOnOtherSlrHd, ExmSH.SalaryHead ExmSlrHd, 
                                    TPG.ExmSalaryHeadID, TPG.PercentageExmAmtOtherSlrHd ExmAmtOtherSlrHd, TPG.IsExmWhichEverLess,
                                    (TAYAISHW.YearlyIncome - TAYAISHW.YearlyTaxableIncome) Exemption, TAYAISHW.YearlyTaxableIncome
                            FROM TaxableYearlyActualIncomeSalaryHeadWise TAYAISHW
                                 LEFT JOIN SalaryHead SH ON TAYAISHW.SalaryHeadID = SH.SalaryHeadID
                                 LEFT JOIN TaxPolicyGeneral TPG ON TAYAISHW.SalaryHeadID = TPG.SalaryHeadID
					                            AND TAYAISHW.TaxPolicyMstID = TPG.TaxPolicyMstID
                                 LEFT JOIN SalaryHead ExmSH ON TPG.ExmSalaryHeadID = ExmSH.SalaryHeadID
                            WHERE TAYAISHW.TaxYearID = '" + sTaxYear + @"'";

                if (sEmpSystemID != "")
                {
                    strSQL = strSQL + @" AND TAYAISHW.EmpInfoSystemID IN ('" + sEmpSystemID + @"')";
                }

                strSQL = strSQL + @" 
                          ORDER BY SH.SalaryHead";

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
        public void LoadPaidTaxUptoDate(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sStartMonth, string strEndMonth, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpInfoSystemID, (SUM(DisbusmentAmount) * -1) AS PaidTaxAmount
                                FROM SalaryProcChild WHERE 
                                SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'Tax')
                                AND SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
			                                WHERE FromDate BETWEEN ('" + sStartMonth + @"') AND ('" + strEndMonth + @"')) 
                                AND EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND GroupID = '" + sGroupID + @"'
                                    AND PlantID = '" + sPlantID + @"'
                                GROUP BY EmpInfoSystemID";

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
        public void GetPLrocessLastDate(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sStartMonth, string strEndMonth,string EndOfTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                                            select
                                            SystemID,SalaryProcessedToDate,PaidTaxAmount,DOJ,CutOffDate
                                            , LastDate = CASE WHEN LastDate>CutOffDate THEN LastDate
				                                            ELSE CutOffDate END 
				                                             , DATEDIFF(MM,
															--deduct one day
															 DATEADD(day, -1, (CASE WHEN LastDate>CutOffDate THEN LastDate	ELSE CutOffDate END ))				                                             
				                                            , '"+EndOfTaxYear+@"') RemainingMonth
                                            from
                                            (
                                            select
                                            e.SystemID,d.SalaryProcessedToDate,isnull(a.PaidTaxAmount,0) PaidTaxAmount
                                            ,e.DOJ
                                            ,(
                                            SELECT CutOffDate FROM [SCS].[OpeningBalanceCutOffDate] AS OCD 
                                                                        WHERE OCD.CompanyGroupId='" + sGroupID + @"' 
                                                                       -- AND OCD.CompanyId = ''
                                                                        AND OCD.PlantId = '"+sPlantID+@"'
                                                                         AND OCD.ModuleName = '"+bplib.clsWebLib.MODULE+@"'
                                            ) CutOffDate
                                            , LastDate = CASE WHEN d.SalaryProcessedToDate<>'' THEN d.SalaryProcessedToDate
				                                            ELSE e.DOJ END 
                                             from
                                             (select * from employeeInformation WHERE SystemID IN ('"+ sEmpInfoSystemID + @"')) e
                                             left outer join
                                            (

								                                            SELECT max(m.ToDate) SalaryProcessedToDate,c.EmpInfoSystemID 
								                                            from SalaryProcMaster m 
								                                            left outer join SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID
								                                            WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')
								                                            group by c.EmpInfoSystemID
                                            ) d on  e.SystemID=d.EmpInfoSystemID
                                            left outer join

                                            (
                                            SELECT EmpInfoSystemID, (SUM(DisbusmentAmount) * -1) AS PaidTaxAmount
                                                                            FROM SalaryProcChild WHERE 
                                                                            SalaryHeadID IN (SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'Tax')
                                                                            AND SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
			                                                                            WHERE FromDate BETWEEN ('" + sStartMonth + @"') AND ('" + strEndMonth + @"')) 
                                                                            AND EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND GroupID = '" + sGroupID + @"'
                                                                                AND PlantID = '" + sPlantID + @"'
                                                                            GROUP BY EmpInfoSystemID
                                            ) a on d.EmpInfoSystemID=a.EmpInfoSystemID
                                            ) x";

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
        public void LoadTaxDefineMastGrd(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT T.SystemID, T.EmpInfoSystemID, T.SalaryID, T.TaxPolicyMstID, T.TaxGroupID, T.EffectiveDate, T.TaxStartFromYear, 
                                    T.TaxStartFromMonth, T.TaxableIncome, T.InvestmentAmount, T.RebateAmount, T.TaxableAmount, T.PaidTaxAmount, T.TaxPaidUptoYear, 
                                    T.TaxPaidUptoMonth, T.TaxToBePay
                           FROM TaxDefineMaster T
                            INNER JOIN
                            (SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate, MAX(TaxStartFromYear) TaxStartFromYear, 
                                    MAX(TaxStartFromMonth) TaxStartFromMonth FROM TaxDefineMaster  GROUP BY EmpInfoSystemID) M 
	                                    ON T.EmpInfoSystemID = M.EmpInfoSystemID AND T.EffectiveDate = M.EffectiveDate
		                                    --AND T.TaxStartFromYear = M.TaxStartFromYear AND T.TaxStartFromMonth = M.TaxStartFromMonth
                            INNER JOIN EmployeeInformation E ON T.EmpInfoSystemID = E.SystemID AND E.GroupID = '" + sGroupID + @"'
                                     AND E.PlantID = '" + sPlantID + @"'
                            INNER JOIN TaxPolicyMaster TPM ON T.TaxGroupID = TPM.TaxGroupID AND TPM.GroupID = '" + sGroupID + @"'
                                     AND TPM.PlantID = '" + sPlantID + @"' AND TPM.TaxYearID = '" + sTaxYear + @"'";

                if (sEmpInfoSystemID != "")
                {
                    strSQL = strSQL + @"
                         WHERE T.EmpInfoSystemID = '" + sEmpInfoSystemID + @"' AND T.TaxYearID = '" + sTaxYear + @"'";
                }
                else
                {
                    strSQL = strSQL + @"
                         WHERE T.TaxYearID = '" + sTaxYear + @"'";
                }

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
        public void LoadTaxDefineMastGrd_Change(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT T.SystemID, T.EmpInfoSystemID, T.SalaryID, T.TaxPolicyMstID, T.TaxGroupID, T.EffectiveDate, T.TaxStartFromYear, 
                                    T.TaxStartFromMonth, T.TaxableIncome, T.InvestmentAmount, T.RebateAmount, T.TaxableAmount, T.PaidTaxAmount, T.TaxPaidUptoYear, 
                                    T.TaxPaidUptoMonth, T.TaxToBePay
                           FROM TaxDefineMaster T
                            INNER JOIN
                            (SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate, MAX(TaxStartFromYear) TaxStartFromYear, 
                                    MAX(TaxStartFromMonth) TaxStartFromMonth FROM TaxDefineMaster where systemid=''  GROUP BY EmpInfoSystemID) M 
	                                    ON T.EmpInfoSystemID = M.EmpInfoSystemID AND T.EffectiveDate = M.EffectiveDate
		                                    --AND T.TaxStartFromYear = M.TaxStartFromYear AND T.TaxStartFromMonth = M.TaxStartFromMonth
                            INNER JOIN EmployeeInformation E ON T.EmpInfoSystemID = E.SystemID AND E.GroupID = '" + sGroupID + @"'
                                     AND E.PlantID = '" + sPlantID + @"'
                            INNER JOIN TaxPolicyMaster TPM ON T.TaxGroupID = TPM.TaxGroupID AND TPM.GroupID = '" + sGroupID + @"'
                                     AND TPM.PlantID = '" + sPlantID + @"' AND TPM.TaxYearID = '" + sTaxYear + @"'";

                if (sEmpInfoSystemID != "")
                {
                    strSQL = strSQL + @"
                         WHERE T.EmpInfoSystemID = '" + sEmpInfoSystemID + @"' AND T.TaxYearID = '" + sTaxYear + @"'";
                }
                else
                {
                    strSQL = strSQL + @"
                         WHERE T.TaxYearID = '" + sTaxYear + @"'";
                }

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
        public void LoadTaxDefineMastEffectiveDateGrd(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpInfoSystemID, MIN(EffectiveDate) EffectiveDate 
							FROM TaxDefineMaster 
							WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"' AND TaxYearID = '" + sTaxYear + @"'
									AND TaxGroupID IN (
														SELECT TaxGroupID FROM TaxPolicyMaster 
															WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
																	AND TaxYearID = '" + sTaxYear + @"'
													  )
								GROUP BY EmpInfoSystemID";

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
        public void LoadINSalaryDefineTaxSlabDefine(string sTaxPolicyMstSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID, '' EmpSystemID, TaxPolicyMstID, SlabDefine, 
                                TaxAbleIncome, TaxRate, '' TaxAmount FROM TaxSlabDefine 
                          WHERE TaxPolicyMstID = '" + sTaxPolicyMstSystemID + @"' Order By TaxRate";

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
        public void GetMaxTaxPayablePeriod(string sGroupID, string sPlantID, string sTaxYear, string sEmpInfoSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT ISNULL(MAX(TaxPayablePeriod), 0) TaxPayablePeriod FROM TaxDeductionInfoMonthWise 
                //                WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"' AND IsPaid = 1 AND TaxYearID = '" + sTaxYear + @"'
                //                AND TaxDefineMasterSystemID IN (SELECT SystemID FROM TaxDefineMaster 
                //                      WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"' AND TaxYearID = '" + sTaxYear + @"'
                //                         AND TaxGroupID IN (
                //                             SELECT TaxGroupID FROM TaxPolicyMaster 
                //		                            WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
                //			                              --AND TaxYearID = '" + sTaxYear + @"'
                //                            ))";

                strSQL = @"SELECT ISNULL(MAX(TaxPayablePeriod), 0) TaxPayablePeriod FROM TaxDeductionInfoMonthWise 
                                WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"' AND IsPaid = 1 
                                AND TaxDefineMasterSystemID IN (SELECT SystemID FROM TaxDefineMaster 
										                            WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"' AND TaxYearID = '" + sTaxYear + @"'
											                              AND TaxGroupID IN (
																                             SELECT TaxGroupID FROM TaxPolicyMaster 
																		                            WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
																			                              --AND TaxYearID = '" + sTaxYear + @"'
																                            ))";

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
        public void GetTaxYearPeriodValidity(string taxyear,string taxEffDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT TP.* FROM scs.TaxYearPeriod TP WHERE TP.TaxYearID = '"+ taxyear + @"'

                                                AND  '" + taxEffDate + @"' between StartDate and EndDate";
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
        public void GetTaxPeriod(string sGroupID, string sPlantID, string sTaxYear, string sEffectDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //by monir 170808
                //strSQL = @"SELECT COUNT(*) TaxPeriod, MIN(StartDate) Startmonth, MAX(EndDate) Endmonth FROM
                //                (SELECT TP.*
		              //                  FROM dbo.TaxYearPeriod TP
				            //                    INNER JOIN 
						          //                      (SELECT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment A
									       //                         INNER JOIN
											     //                           (SELECT * FROM PlantAndCompanyAssignment 
												    //                            WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"') B ON A.CompanyID = B.CompanyID
							         //                       WHERE A.GroupID = '" + sGroupID + @"') TPC ON TP.SystemID = TPC.PeriodSystemID	
                //                WHERE TP.TaxYearID = '" + sTaxYear + @"' AND TP.EndDate >= '" + sEffectDate + @"' AND TP.GroupID = '" + sGroupID + @"') A";

                strSQL = @"SELECT COUNT(*) TaxPeriod
	                                        ,MIN(StartDate) Startmonth
	                                        ,MAX(EndDate) Endmonth
                                            ,(select count(Id) TotalPeriod from scs.TaxYearPeriod where TaxYearId='20182') TotalPeriod
                                        FROM (
	                                        SELECT TP.*
	                                        FROM scs.TaxYearPeriod TP
	                                        INNER JOIN (
				                                        SELECT TaxYearPeriodId PeriodSystemID
				                                        FROM scs.CompanyTaxYearPeriod A
				                                        left outer join scs.CompanyTaxYear xy on xy.Id=A.CompanyTaxYearId
				                                        left outer join [ORG].Company c on c.Id=xy.CompanyId
				                                        INNER JOIN (
								                                        SELECT *
								                                        FROM [ORG].Plant
								                                        WHERE id = '" + sPlantID + @"'
									                                        AND CompanyGroupId = '" + sGroupID + @"'
							                                        ) B ON xy.CompanyID = B.CompanyID

				                                        WHERE c.CompanyGroupId = '" + sGroupID + @"'
		                                        ) TPC ON TP.Id = TPC.PeriodSystemID
	                                        WHERE TP.TaxYearID = '" + sTaxYear + @"'
		                                        AND TP.EndDate >= '" + sEffectDate + @"'
		                                        --AND TP.GroupID = '" + sGroupID + @"'
	                                        ) A";

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
        public void GetTaxPeriodDOJBetwMonth(string sGroupID, string sPlantID, string sTaxYear, string sEffectDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT (CONVERT(DECIMAL, DATEDIFF(d, StartDate, '" + sEffectDate + @"')) / CONVERT(DECIMAL, DATEDIFF(d, StartDate, EndDate))) TaxPeriod FROM
                //            (
                //             SELECT TP.*
		              //              FROM dbo.TaxYearPeriod TP
				            //                INNER JOIN 
						          //                  (
                //                                      SELECT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment A
									       //                     INNER JOIN
											     //                       (
                //                                                         SELECT * FROM PlantAndCompanyAssignment 
												    //                        WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"'
                //                                                        ) B ON A.CompanyID = B.CompanyID
							         //                   WHERE A.GroupID = '" + sGroupID + @"'
                //                                     ) TPC ON TP.SystemID = TPC.PeriodSystemID	
                //            WHERE TP.TaxYearID = '" + sTaxYear + @"' AND TP.EndDate >= '" + sEffectDate + @"' AND TP.GroupID = '" + sGroupID + @"') A";

                strSQL = @"SELECT (CONVERT(DECIMAL, DATEDIFF(d, StartDate, '" + sEffectDate + @"')) / CONVERT(DECIMAL, DATEDIFF(d, StartDate, EndDate))) TaxPeriod FROM
                            (
                             SELECT TP.*
		                            FROM scs.TaxYearPeriod TP
				                            INNER JOIN 
						                            (
                                                      SELECT [TaxYearPeriodId] FROM scs.CompanyTaxYearPeriod A
													  left outer join scs.CompanyTaxYear y on y.Id=A.CompanyTaxYearId
									                            INNER JOIN
											                            (
                                                                         SELECT * FROM [ORG].Plant 
												                            WHERE Id = '" + sPlantID + @"' and CompanyGroupId='" + sGroupID + @"'
                                                                        ) B ON y.CompanyID = B.CompanyID
                                                     ) TPC ON TP.Id = TPC.TaxYearPeriodId	
                           WHERE TP.TaxYearID = '" + sTaxYear + @"' AND TP.EndDate >= '" + sEffectDate + @"'
							) A";

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
        public void GetCurrentMonthConvertionRateMonthAndYear(string sGroupID, string sPlantID, string sEffectDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM ExchangerateDateWiseForHR 
                                WHERE PlantID = '" + sPlantID + @"' AND GroupID = '" + sGroupID + @"' 
                                      AND FromDate <= '" + sEffectDate + @"'";

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
        public void GetCurrentMonthConvertionRate(string sGroupID, string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.SystemID, A.FromDate, A.ToCurrencyBuying 
                                        FROM ExchangerateDateWiseForHR A
		                                        INNER JOIN (SELECT Max(FromDate) FromDate 
						                                        FROM ExchangerateDateWiseForHR WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"') B 
							                                        ON A.FromDate = B.FromDate
                                        WHERE A.GroupID = '" + sGroupID + @"' AND A.PlantID = '" + sPlantID + @"'";

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
        public void GetProjectionBonus(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sStartmonth, string sEndmonth, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpInfoSystemID, ISNULL(SUM(BonusAmount), 0) BonusAmount FROM BonusPaymentProjection 
                                    WHERE EffectiveDate BETWEEN '" + sStartmonth + @"' AND '" + sEndmonth + @"' 
                                    AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

                if (sEmpInfoSystemID != "")
                {
                    strSQL = strSQL + @" AND EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')";
                }

                strSQL = strSQL + @"
                                   GROUP BY EmpInfoSystemID";

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
        public void GetActualBonus(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sStartmonth, string sEndmonth, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpSystemID, ISNULL(SUM(BonusAmount), 0) BonusAmount FROM BonusPaymentActual 
                                    WHERE BnsMstSystemID IN (SELECT SystemID FROM BonusPaymentActualMaster WHERE EffectiveDate BETWEEN '" + sStartmonth + @"' AND '" + sEndmonth + @"')";

                if (sEmpInfoSystemID.Trim() != "")
                {
                    strSQL = strSQL + @"
                                    AND EmpSystemID IN ('" + sEmpInfoSystemID + @"')";
                }

                strSQL = strSQL + @"
                                   GROUP BY EmpSystemID";

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
        public void GetAnnualLeave(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sStartmonth, string sEndmonth, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpInfoSystemID, ISNULL(SUM(AnnualLeaveAmount), 0) Amount FROM AnnualLeavePayment 
                                    WHERE EffectiveDate BETWEEN '" + sStartmonth + @"' AND '" + sEndmonth + @"' AND GroupID = '" + sGroupID + @"' 
                                    AND PlantID = '" + sPlantID + @"'";

                if (sEmpInfoSystemID != "")
                {
                    strSQL = strSQL + @" AND EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')";
                }

                strSQL = strSQL + @"
                                   GROUP BY EmpInfoSystemID";

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
        public void GetProjectionAnnualLeave(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sStartmonth, string sEndmonth, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpInfoSystemID, ISNULL(SUM(Amount), 0) Amount FROM AnnualLeaveProjection 
                                    WHERE EffectiveDate BETWEEN '" + sStartmonth + @"' AND '" + sEndmonth + @"' AND GroupID = '" + sGroupID + @"' 
                                    AND PlantID = '" + sPlantID + @"'";

                if (sEmpInfoSystemID != "")
                {
                    strSQL = strSQL + @" AND EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')";
                }

                strSQL = strSQL + @"
                                   GROUP BY EmpInfoSystemID";

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
        public void GetTaxExemptedAmtSalaryHeadWise(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxExemptedAmtSalaryHeadWise WHERE TaxYearID = '" + sTaxYear + @"'";

                if (sEmpInfoSystemID != "")
                {
                    strSQL = strSQL + " AND EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')";
                }
                if (sGroupID != "")
                {
                    strSQL = strSQL + " AND GroupID = '" + sGroupID + @"'";
                }

                if (sPlantID != "")
                {
                    strSQL = strSQL + " AND PlantID = '" + sPlantID + @"'";
                }

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
        public void GetTaxDefineMaster(string sEmpInfoSystemID, string sTDMSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sTDMSystemID.Trim() != "")
                {
                    strSQL = @"SELECT TOP (1) * FROM TaxDefineMaster 
                            WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"' AND SystemID = '" + sTDMSystemID + @"' 
                            ORDER BY TaxPaidUptoYear DESC, TaxPaidUptoMonth DESC, EffectiveDate DESC";
                }
                else
                {
                    strSQL = @"SELECT TOP (1) * FROM TaxDefineMaster 
                                    WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"'
                                ORDER BY TaxPaidUptoYear DESC, TaxPaidUptoMonth DESC, EffectiveDate DESC";
                }

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
        public void GetTaxDefineMasterEffectiveDate(string sEmpInfoSystemID, string sEffectiveDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TOP (1) * FROM TaxDefineMaster 
                            WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND EffectiveDate = '" + sEffectiveDate + @"' 
                            ORDER BY TaxPaidUptoYear DESC, TaxPaidUptoMonth DESC, EffectiveDate DESC";

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

        public void GetOpeningBalance(string sEmpInfoSystemID,string TaxYearId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TOP 1000 [SystemID]
                          ,[EmpInfoSystemID]
                          ,[TaxYearID]
                          ,[OpeningBalance] FROM [dbo].[TaxOpeningBalance] where EmpInfoSystemID='"+ sEmpInfoSystemID + "' and TaxYearID='"+TaxYearId+"'";

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
        public void GetTaxRebateSlabDefine(string sTaxPolicyMstSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *, '' RebateAmount
                            FROM TaxRebateSlabDefine 
                          WHERE TaxPolicyMstID = '" + sTaxPolicyMstSystemID + @"'
                            order by TaxAbleIncomeLowerForRebate,
							TaxAbleIncomeUpperForRebate,
							SlabDefine
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
        public void GetActualExemptionByEntry(string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TEM.SystemID, TEM.EmpInfoSystemID, TEC.SalaryHeadID, TEC.ExemptionAmt 
                            FROM TaxExemptionAmtMaster TEM 
		                                LEFT JOIN TaxExemptionAmtChild TEC ON TEM.SystemID = TEC.MstSystemID
                            WHERE TEM.TaxYearID = '" + sTaxYear + @"'";

                if (sEmpInfoSystemID != "")
                {
                    strSQL = strSQL + @" AND TEM.EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')";
                }

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
        public void GetTaxActualInvesment(string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[TaxActualInvesment] 
                            WHERE TaxYearID = '" + sTaxYear + @"'";

                if (sEmpInfoSystemID.Trim() != "")
                {
                    strSQL = strSQL + @"
                                    AND EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')";
                }

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
        public void GetOpeningIncome(string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[TaxOpeningBalance] 
                            WHERE TaxYearID = '" + sTaxYear + @"'";

                if (sEmpInfoSystemID.Trim() != "")
                {
                    strSQL = strSQL + @"
                                    AND EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')";
                }

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
        public void GetTaxOpeningBalance(string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[TaxOpeningBalance] 
                            WHERE TaxYearID = '" + sTaxYear + @"'";

                if (sEmpInfoSystemID != "")
                {
                    strSQL = strSQL + @"
                         AND EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')";
                }

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
        public void GetTaxDirect(string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select EmpInfoSystemID,TaxYearID,sum(isnull(Amount,0)) Amount 
                                    from [MST].[EmpWiseDirectTaxPaymentHead]
                                    where EmpInfoSystemID='"+ sEmpInfoSystemID + @"' and TaxYearID='"+ sTaxYear + @"'
                                    group by EmpInfoSystemID,TaxYearID";
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
        public void GetTaxDefineMasterSave(string sEmpInfoSystemID, string sTDMSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxDefineMaster WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')";

                if (sTDMSystemID != "")
                {
                    strSQL = strSQL + @"
                           AND SystemID IN ('" + sTDMSystemID + @"')";
                }

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
        public void GetTaxDefineMasterLastMax(string sEmpInfoSystemID, string TaxYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxDefineMaster WHERE EmpInfoSystemID='"+ sEmpInfoSystemID + @"' and  TaxYearID='"+ TaxYearID + @"' and EffectiveDate=
                                        (
                                        SELECT max(EffectiveDate) FROM TaxDefineMaster WHERE EmpInfoSystemID='" + sEmpInfoSystemID + @"' and  TaxYearID='" + TaxYearID + @"'
                                        )";
                

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
        public void GetTaxDefineMasterTaxYearWiseSave(string sEmpInfoSystemID, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxDefineMaster 
                           WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')
                                 AND TaxYearID IN ('" + sTaxYear + @"')";

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
        public void GetTaxDefineMasterSalaryIDWise(string sEmpInfoSystemID, string sSalaryID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxDefineMaster WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"' AND SalaryID = '" + sSalaryID + @"'";

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
        public void GetTaxableYearlyActualIncomeSalaryHeadWise(string sEmpInfoSystemID, string sTaxYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxableYearlyActualIncomeSalaryHeadWise WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND TaxYearID = '" + sTaxYearID + @"'";

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
        public void GetTaxableIncomeSalaryHeadWise(string sEmpInfoSystemID, string sTaxYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxableIncomeSalaryHeadWise 
                              WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"')
                                    AND TaxYearID = '" + sTaxYearID + @"'";

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
        public void GetTaxableIncomeSalaryHeadWise(string TaxMasterId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TaxableIncomeSalaryHeadWise 
                              WHERE  TaxDefineMasterSystemID='" + TaxMasterId + "'";

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
        public void GetTaxDeductionInfoMonthWise(string sEmpInfoSystemID, string sCompanyID, string sTaxYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND 
                //            TaxPeriodSystemID IN (SELECT DISTINCT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment WHERE CompanyID = '" + sCompanyID + @"' 
                //                AND TaxYearID = '" + sTaxYearID + @"')";

                strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND 
                            TaxPeriodSystemID IN (SELECT DISTINCT p.TaxYearPeriodid FROM scs.CompanyTaxYearPeriod p
							left outer join scs.CompanyTaxYear y on y.Id=p.CompanyTaxYearId
							 WHERE y.CompanyID = '" + sCompanyID + @"' 
                                AND y.TaxYearID = '" + sTaxYearID + @"')";

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
        public void GetTaxDeductionMonthWise(string sEmpInfoSystemID, string paidUptoMonth, string TaxDefineMasterSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND 
                //            TaxPeriodSystemID IN (SELECT DISTINCT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment WHERE CompanyID = '" + sCompanyID + @"' 
                //                AND TaxYearID = '" + sTaxYearID + @"')";

                strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND 
                            TaxPayablePeriod>"+ paidUptoMonth + " and IsPaid=0 and TaxDefineMasterSystemID='"+ TaxDefineMasterSystemID + "'";

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
        public void GetTaxDeductionInfoMonthWise(string TaxDefineMasterSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND 
                //            TaxPeriodSystemID IN (SELECT DISTINCT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment WHERE CompanyID = '" + sCompanyID + @"' 
                //                AND TaxYearID = '" + sTaxYearID + @"')";

                strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise WHERE TaxDefineMasterSystemID = '" + TaxDefineMasterSystemID + @"'";

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
        public void GetTaxDeductionInfoMonthWiseForDelete(int PaidUpto,string TaxMasterId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND 
                //            TaxPeriodSystemID IN (SELECT DISTINCT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment WHERE CompanyID = '" + sCompanyID + @"' 
                //                AND TaxYearID = '" + sTaxYearID + @"')";

                strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise 
                                        WHERE IsPaid=0 
                                        and  TaxPayablePeriod>" + PaidUpto + @" 
                                        and TaxDefineMasterSystemID='" + TaxMasterId + @"'
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
        public void GetTaxDeductionMaxDate(string sEmpInfoSystemID, string sTaxYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM TaxDeductionInfoMonthWise WHERE EmpInfoSystemID IN ('" + sEmpInfoSystemID + @"') AND 
                //            TaxPeriodSystemID IN (SELECT DISTINCT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment WHERE CompanyID = '" + sCompanyID + @"' 
                //                AND TaxYearID = '" + sTaxYearID + @"')";

                strSQL = @"
                                                        select max(d.TaxPayablePeriod) MaxPeriod
                                                         from TaxDeductionInfoMonthWise d
                                                        left outer join [SCS].[TaxYearPeriod] p on p.Id=d.TaxPeriodSystemID
                                                        where d.IsPaid =1 and  d.EmpInfoSystemID='" + sEmpInfoSystemID + @"'
                                                        and p.TaxYearId='" + sTaxYearID + @"'
                                                        group by p.TaxYearId,d.EmpInfoSystemID
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
        public void xGetFactoryWisePeriod(string sCompanyID, string sTaxYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM TaxYearPeriod WHERE 
                //            SystemID IN (SELECT DISTINCT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment WHERE CompanyID = '" + sCompanyID + @"' 
                //                            AND TaxYearID = '" + sTaxYearID + @"') 
                //            ORDER BY period";

                strSQL = @"SELECT Id SystemID,	TaxYearId,	PeriodNo [Period],	PeriodName,	StartDate,	EndDate,	[Description]
 
                                        FROM scs.TaxYearPeriod WHERE 
                                        Id IN (SELECT DISTINCT TaxYearPeriodID FROM scs.CompanyTaxYearPeriod p
                                        left outer join scs.CompanyTaxYear y on y.Id=p.CompanyTaxYearId
                                         WHERE y.CompanyID = '" + sCompanyID + @"' AND  y.TaxYearID = '" + sTaxYearID + @"') 
                                        ORDER BY PeriodNo";

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
        public void GetFactoryWisePeriod(string sCompanyID, string sTaxYearID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM TaxYearPeriod WHERE 
                //            SystemID IN (SELECT DISTINCT PeriodSystemID FROM TaxYearPeriodAndCompanyAssignment WHERE CompanyID = '" + sCompanyID + @"' 
                //                            AND TaxYearID = '" + sTaxYearID + @"') 
                //            ORDER BY period";

                strSQL = @"SELECT *
                                        FROM scs.TaxYearPeriod WHERE 
                                        Id IN (SELECT DISTINCT TaxYearPeriodID FROM SCS.CompanyTaxYearPeriod p
                                        LEFT OUTER JOIN SCS.CompanyTaxYear y on y.Id = P.CompanyTaxYearId
                                         WHERE y.CompanyID = '" + sCompanyID + @"' AND  Y.TaxYearID = '" + sTaxYearID + @"') 
                                        ORDER BY PeriodNo";

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
        public bool DuplicateEmployeeBankAccNo(string sEmpSystemID, string sBankSystemID, string sBankAccNo)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsRef = null;

            bool blnStatus = false;

            try
            {
                strSQL = @"SELECT * FROM EmployeeInformation
                           WHERE (SystemID <> '" + sEmpSystemID + @"') AND (BankSystemID = '" + sBankSystemID + @"')
                                 AND (BankAccNo = '" + sBankAccNo + @"') AND EmployeeStatus = 'Active'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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

        public void LoadYearlyTaxableIncomeBeforeIncrementDataOnGrid(string sEmpSystemID, string sStartDate, string sEndDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpInfoSystemID, SalaryHeadID, EntryCurrencyID, SUM(EntryAmount * PayCount) EntryAmount,
	                              DefineCurrencyID, SUM(DefineAmount * PayCount) DefineAmount, AmtDefinitionCurrencyID,SalaryID
                           FROM
                                (
                                 SELECT *, (DATEDIFF(MM, StartDate, EndDate) + 1) PayCount
	                                FROM
		                                (
		                                 SELECT T1.EmpInfoSystemID, T1.SalaryRuleMasterSystemID, T1.SalaryID, T1.SalaryHeadID, T1.EffectiveDate StartDate, 
 				                                ISNULL(DATEADD(D, -1, T2.EffectiveDate), '" + sEndDate + @"') EndDate, T1.EntryCurrencyID, T1.EntryAmount, 
				                                T1.DefineCurrencyID, T1.DefineAmount, T1.AmtDefinitionCurrencyID
		                                 FROM
			                                 (
			                                  SELECT SD.SystemID, EmpInfoSystemID, SalaryRuleMasterSystemID, SalaryID, SalaryHeadID, EffectiveDate, EntryCurrencyID, EntryAmount, 
					                                 DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, DENSE_RANK() OVER (PARTITION BY SalaryHeadID ORDER BY EffectiveDate) SRNO
			                                  FROM SalaryInfoDefineMaster SDM 
																		INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
			                                  WHERE IsApproved = 1 AND EffectiveDate BETWEEN '" + sStartDate + @"' AND '" + sEndDate + @"'";
                if (sEmpSystemID.Trim() != "")
                {
                    strSQL = strSQL + @"
                                        AND EmpInfoSystemID IN ('" + sEmpSystemID + @"')";
                }

                strSQL = strSQL + @"
			                                 ) T1
			                                 LEFT JOIN 
			                                        (
			                                         SELECT SD.SystemID, SalaryHeadID, EffectiveDate,
					                                        DENSE_RANK() OVER (PARTITION BY SalaryHeadID ORDER BY EffectiveDate) SRNO
			                                         FROM SalaryInfoDefineMaster SDM 
																		INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
			                                         WHERE IsApproved = 1 AND EffectiveDate BETWEEN '" + sStartDate + @"' AND '" + sEndDate + @"'";
                if (sEmpSystemID.Trim() != "")
                {
                    strSQL = strSQL + @"
                                        AND EmpInfoSystemID IN ('" + sEmpSystemID + @"')";
                }

                strSQL = strSQL + @"
			                                        ) T2 ON T1.EffectiveDate < T2.EffectiveDate AND T1.SalaryHeadID = T2.SalaryHeadID AND (T1.SRNO + 1) = T2.SRNO
		                                ) A
                                ) B
                            GROUP BY EmpInfoSystemID, SalaryRuleMasterSystemID, SalaryHeadID, EntryCurrencyID, DefineCurrencyID, AmtDefinitionCurrencyID,SalaryID";

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

        public void LoadProcessIncome(string sEmpSystemID, string sStartDate, string sEndDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select 
                                c.GroupID,
                                c.PlantID,
                                m.YearNo,
                                c.EmpInfoSystemID,
                                c.SalaryHeadID,
                                sum(c.DefineAmount) DefineAmount,count(m.SalaryProcID) YearCount

                                from 
                                SalaryProcMaster m 
                                left outer join SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID
                                where c.EmpInfoSystemID='" + sEmpSystemID + @"' 
                                and m.FromDate >= '"+ sStartDate + @"'
                                and m.ToDate<'" + sEndDate + @"'
                                group by c.EntryAmount,SalaryHeadID,EmpInfoSystemID,GroupID,PlantID,m.YearNo";

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
        public void GetMaxDateFromProcessedSalary(string sEmpSystemID, string sStartDate, string sEndDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select x.ToDate,x.EmpInfoSystemID,ss.SalaryID
								from(
								select distinct max(m.ToDate) ToDate,c.EmpInfoSystemID from SalaryProcMaster m
								left outer join SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID
								where   m.FromDate >= '"+ sStartDate + @"'
                                and m.ToDate<'" + sEndDate + @"'
								and c.EmpInfoSystemID='" + sEmpSystemID + @"' 
								group by c.EntryAmount,EmpInfoSystemID,GroupID,PlantID,m.YearNo
								) x
								left outer join 
								(
								select distinct m.ToDate,c.EmpInfoSystemID,c.SalaryID from SalaryProcMaster m
								left outer join SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID
								) ss on ss.ToDate=x.ToDate and x.EmpInfoSystemID=ss.EmpInfoSystemID";

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
        public void GetToBeProcessedSalary(string SalaryID, string sStartDate, string sEndDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT 
                                MonthPeriod = ISNULL((DATEDIFF(m, '" + sStartDate + "', '" + sEndDate + @"')), 0),
                                c.DefineAmount*ISNULL((DATEDIFF(m, '" + sStartDate + "', '"+ sEndDate + @"')), 0) DefineAmount,
                                c.SalaryHeadID

                                 FROM SalaryInfoDefineMaster m
                                left outer join SalaryInfoDefine c on c.SalaryID=m.SystemID
                                where m.SystemID='"+ SalaryID + "'";

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

        public void GetDeductedMaxPeriod(string EmpId,string TaxYearId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select max(d.TaxPayablePeriod) MaxPeriod,d.EmpInfoSystemID,d.TaxPeriodSystemID
                                ,Replace(CONVERT(VARCHAR(11), p.EndDate, 106), ' ', '-') EndDate
                                from TaxDeductionInfoMonthWise d
                                left outer join [SCS].[TaxYearPeriod] p on p.Id=d.TaxPeriodSystemID
                                where d.IsPaid =1 and d.EmpInfoSystemID='" + EmpId+ @"'  and p.TaxYearId='"+ TaxYearId + @"' 
                                group by d.EmpInfoSystemID,d.TaxPeriodSystemID,p.EndDate";

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

        public void IsLastApprovedSalaryStructureHasBeenProcessed(string EmpInfoSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                //strSQL = @"select SystemId from SalaryProcChild
                //                    where SalaryID in (
                //                    select SystemID from SalaryInfoDefineMaster m
                //                    where m.IsApproved=1 and m.EmpInfoSystemID='"+ EmpInfoSystemID + @"'
                //                    )";
                strSQL = @"select isnull(m.SalaryStructure,0) SalaryStructure
                                    ,isnull(a.AlreadyApproved,0) AlreadyApproved
                                    ,isnull(p.AlreadyProcessed,0) AlreadyProcessed
                                     from 
                                      (select count(systemid) SalaryStructure,EmpInfoSystemID 
                                      from SalaryInfoDefineMaster
                                      where EmpInfoSystemID='" + EmpInfoSystemID + @"'
                                      group by EmpInfoSystemID
                                      ) m 
                                      left outer join 
                                     ( select Count(m.SystemID) AlreadyApproved,m.EmpInfoSystemID from SalaryInfoDefineMaster m
                                                                        where m.IsApproved=1 and m.EmpInfoSystemID='" + EmpInfoSystemID + @"'
									                                    group by m.IsApproved,m.EmpInfoSystemID
									                                    )a on m.EmpInfoSystemID=a.EmpInfoSystemID
                                     left outer join
								                                    ( select count(systemid) AlreadyProcessed,EmpInfoSystemID 
											                                    from   SalaryProcChild
                                                                        where SalaryID in (
                                                                        select SystemID from SalaryInfoDefineMaster m
                                                                        where m.IsApproved=1 and m.EmpInfoSystemID='" + EmpInfoSystemID + @"'									
													                                    )
									                                    group by EmpInfoSystemID
									                                    ) p on p.EmpInfoSystemID=m.EmpInfoSystemID
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
        public void GetTaxDefineMasterBaseOnEffectiveDate(string sEmpInfoSystemID, string sEffectDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[TaxDefineMaster] 
                               WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"' 
                                     AND EffectiveDate IN (
                                                           SELECT Max(EffectiveDate) EffectiveDate 
	                                                             FROM [dbo].[TaxDefineMaster] 
                                                            WHERE EmpInfoSystemID = '" + sEmpInfoSystemID + @"' 
                                                            AND EffectiveDate <= '" + sEffectDate + @"')";

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

        public void LoadTaxableIncomeSalaryHeadWiseBeforeIncrementSameTaxYearDataOnGrid(string sEmpSystemID, string sEndDate, string sTaxYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                // strSQL = @"SELECT A.SystemID, A.EmpInfoSystemID, A.TaxDefineMasterSystemID, A.TaxPolicyMstID, A.TaxGroupID, A.TaxYearID, B.EffectiveDate StartDate, D.EndDate, 
                //                (DATEDIFF(MM, B.EffectiveDate, '" + sEndDate + @"') + 1) TaxPayablePeriod, SH.SalaryHead, A.SalaryHeadID, SH.HeadCategory, ECR.CurrencyDesc EntryCurrency, 
                //                   A.EntryIncomeCurrencyID, A.EntryIncome, DCR.CurrencyDesc DefinationCurrency, A.DefinationCurrencyID, A.DefinationAmount, A.DefinationCurrencyRate,
                //                A.LocalCurrencyID, LCR.CurrencyCode LocalCurrency, A.LocalCurrencyID, A.ConvertionRate, 
                //                   YearlyIncome = CASE WHEN (SH.HeadCategory = 'Bonus' OR SH.HeadCategory = 'Earned Leave') THEN A.EntryIncome
                //ELSE (A.EntryIncome * (DATEDIFF(MM, B.EffectiveDate, '" + sEndDate + @"') + 1)) END 
                //             FROM [dbo].[TaxableIncomeSalaryHeadWise] A
                //                INNER JOIN (
                //                   SELECT * FROM [dbo].[TaxDefineMaster]
                //                    WHERE EmpInfoSystemID = '" + sEmpSystemID + @"'
                //                       AND EffectiveDate IN (SELECT MAX(EffectiveDate) FROM [dbo].[TaxDefineMaster]
                //                    WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND EffectiveDate <= '" + sEndDate + @"') AND TaxYearID = '" + sTaxYear + @"'
                //                  ) B ON A.TaxDefineMasterSystemID = B.SystemID
                //               LEFT JOIN [dbo].[TaxDeductionInfoMonthWise] C ON B.MonthlyTaxSystemID = C.SystemID
                //               LEFT JOIN [dbo].[TaxYearPeriod] D ON C.TaxPeriodSystemID = D.SystemID
                //               LEFT JOIN [dbo].[SalaryHead] SH ON A.SalaryHeadID = SH.SalaryHeadID
                //               LEFT JOIN [dbo].[Currency] ECR ON A.EntryIncomeCurrencyID = ECR.CurrencyCode
                //               LEFT JOIN [dbo].[Currency] DCR ON A.DefinationCurrencyID = DCR.CurrencyCode
                //               LEFT JOIN [dbo].[Currency] LCR ON A.LocalCurrencyID = LCR.CurrencyCode";

      

                strSQL = @" SELECT A.systemid,
                                               A.empinfosystemid,
                                               A.taxdefinemastersystemid,
                                               A.taxpolicymstid,
                                               A.taxgroupid,
                                               A.taxyearid,
                                               B.effectivedate StartDate,
                                               D.enddate,
                                               ( Datediff(mm, B.effectivedate, '" + sEndDate + @"') + 1 )  TaxPayablePeriod,
                                               SH.salaryhead,
                                               A.salaryheadid,
                                               SH.headcategory,
                                               ECR.code        EntryCurrency,
                                               A.entryincomecurrencyid,
                                               A.entryincome,
                                               DCR.code        DefinationCurrency,
                                               A.definitioncurrencyid,
                                               A.definitionamount,
                                               A.definitioncurrencyrate,
                                               A.localcurrencyid,
                                               LCR.id          LocalCurrency,
                                               A.localcurrencyid,
                                               A.convertionrate,
                                               YearlyIncome = CASE
                                                                WHEN( SH.headcategory = 'Bonus' OR SH.headcategory = 'Earned Leave' ) THEN A.entryincome
                                                                ELSE( A.entryincome * ( Datediff(mm, B.effectivedate,'" + sEndDate + @"')+ 1 ) )
                                                              END
                                        FROM  [dbo].[taxableincomesalaryheadwise] A
                                              INNER JOIN(SELECT* FROM   [dbo].[taxdefinemaster]
                                                         WHERE  empinfosystemid = '" + sEmpSystemID + @"'
                                                                AND effectivedate IN (SELECT Max(effectivedate)
                                                                                      FROM  [dbo].[taxdefinemaster]
                                                                                      WHERE empinfosystemid = '" + sEmpSystemID + @"' AND effectivedate <='" + sEndDate + @"')
                                                                AND taxyearid = '" + sTaxYear + @"') B ON A.taxdefinemastersystemid = B.systemid
                                              LEFT JOIN[dbo].[taxdeductioninfomonthwise] C ON B.monthlytaxsystemid = C.systemid
                                              LEFT JOIN[scs].[taxyearperiod] D ON C.taxperiodsystemid = D.id
                                              LEFT JOIN[dbo].[salaryhead] SH ON A.salaryheadid = SH.salaryheadid
                                              LEFT JOIN[scs].[currency] ECR ON A.entryincomecurrencyid = ECR.id
                                              LEFT JOIN[scs].[currency] DCR ON A.definitioncurrencyid = DCR.id
                                              LEFT JOIN[scs].[currency] LCR ON A.localcurrencyid = LCR.id";

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
        public void LoadTaxableIncomeSalaryHeadWiseBeforeLastIncrementDataOnGrid(string sEmpSystemID, string sStartDate, string sEndDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT A.SystemID, A.EmpInfoSystemID, A.TaxDefineMasterSystemID, A.TaxPolicyMstID, A.TaxGroupID, A.TaxYearID, B.EffectiveDate, 
                //               A.TaxPayablePeriod, SH.SalaryHead, A.SalaryHeadID, SH.HeadCategory, ECR.CurrencyDesc EntryCurrency, A.EntryIncomeCurrencyID, 
                //               A.EntryIncome, A.DefinationCurrencyID, DCR.CurrencyCode DefinationCurrency, A.DefinationCurrencyID, A.DefinationAmount, 
                //               A.DefinationCurrencyRate, LCR.CurrencyCode LocalCurrency, A.LocalCurrencyID, A.ConvertionRate, A.YearlyIncome 
                //           FROM [dbo].[TaxableIncomeSalaryHeadWise] A
                //               INNER JOIN (
                //                  SELECT * FROM [dbo].[TaxDefineMaster]
                //                   WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND EffectiveDate >= '" + sStartDate + @"' AND EffectiveDate < '" + sEndDate + @"'
                //                 ) B ON A.TaxDefineMasterSystemID = B.SystemID
                //              LEFT JOIN [dbo].[TaxDeductionInfoMonthWise] C ON B.MonthlyTaxSystemID = C.SystemID
                //              LEFT JOIN [dbo].[TaxYearPeriod] D ON C.TaxPeriodSystemID = D.SystemID
                //              LEFT JOIN [dbo].[SalaryHead] SH ON A.SalaryHeadID = SH.SalaryHeadID
                //              LEFT JOIN [dbo].[Currency] ECR ON A.EntryIncomeCurrencyID = ECR.CurrencyCode
                //              LEFT JOIN [dbo].[Currency] DCR ON A.DefinationCurrencyID = DCR.CurrencyCode
                //              LEFT JOIN [dbo].[Currency] LCR ON A.LocalCurrencyID = LCR.CurrencyCode";

                strSQL = @"SELECT A.SystemID, A.EmpInfoSystemID, A.TaxDefineMasterSystemID, A.TaxPolicyMstID, A.TaxGroupID, A.TaxYearID, B.EffectiveDate, 
	                              A.TaxPayablePeriod, SH.SalaryHead, A.SalaryHeadID, SH.HeadCategory, ECR.Code EntryCurrency, A.EntryIncomeCurrencyID, 
	                              A.EntryIncome, A.DefinitionCurrencyID, DCR.Id DefinitionCurrency, A.DefinitionCurrencyID, A.DefinitionAmount, 
	                              A.DefinitionCurrencyRate, LCR.Id LocalCurrency, A.LocalCurrencyID, A.ConvertionRate, A.YearlyIncome 
                           FROM [dbo].[TaxableIncomeSalaryHeadWise] A
		                             INNER JOIN (
					                             SELECT * FROM [dbo].[TaxDefineMaster]
							                            WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND EffectiveDate >= '" + sStartDate + @"' AND EffectiveDate < '" + sEndDate + @"'
					                            ) B ON A.TaxDefineMasterSystemID = B.SystemID
		                            LEFT JOIN [dbo].[TaxDeductionInfoMonthWise] C ON B.MonthlyTaxSystemID = C.SystemID
		                            LEFT JOIN scs.[TaxYearPeriod] D ON C.TaxPeriodSystemID = D.Id
		                            LEFT JOIN [dbo].[SalaryHead] SH ON A.SalaryHeadID = SH.SalaryHeadID
		                            LEFT JOIN scs.[Currency] ECR ON A.EntryIncomeCurrencyID = ECR.Id
		                            LEFT JOIN scs.[Currency] DCR ON A.DefinitionCurrencyID = DCR.Id
		                            LEFT JOIN scs.[Currency] LCR ON A.LocalCurrencyID = LCR.Id";

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

        public void LoadTaxableIncomeSalaryHeadWiseBeforeIncrementDataOnGrid(string sEmpSystemID, string sStartDate, string sEndDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.SystemID, A.EmpInfoSystemID, A.TaxDefineMasterSystemID, A.TaxPolicyMstID, A.TaxGroupID, A.TaxYearID, B.EffectiveDate StartDate, D.EndDate, 
	                              (DATEDIFF(MM, B.EffectiveDate, '" + sEndDate + @"') + 1) TaxPayablePeriod, SH.SalaryHead, A.SalaryHeadID, SH.HeadCategory, ECR.CurrencyDesc EntryCurrency, 
                                  A.EntryIncomeCurrencyID, A.EntryIncome, DCR.CurrencyDesc DefinationCurrency, A.DefinationCurrencyID, A.DefinationAmount, A.DefinationCurrencyRate,
	                              A.LocalCurrencyID, LCR.CurrencyCode LocalCurrency, A.LocalCurrencyID, A.ConvertionRate, 
                                  YearlyIncome = CASE WHEN (SH.HeadCategory = 'Bonus' OR SH.HeadCategory = 'Earned Leave') THEN A.EntryIncome
													  ELSE (A.EntryIncome * (DATEDIFF(MM, B.EffectiveDate, '" + sEndDate + @"') + 1)) END 
                            FROM [dbo].[TaxableIncomeSalaryHeadWise] A
		                             INNER JOIN (
					                             SELECT * FROM [dbo].[TaxDefineMaster]
							                            WHERE EmpInfoSystemID = '" + sEmpSystemID + @"'
								                              AND EffectiveDate IN (SELECT MAX(EffectiveDate) FROM [dbo].[TaxDefineMaster]
							                            WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND EffectiveDate <= '" + sStartDate + @"')	
					                            ) B ON A.TaxDefineMasterSystemID = B.SystemID
		                            LEFT JOIN [dbo].[TaxDeductionInfoMonthWise] C ON B.MonthlyTaxSystemID = C.SystemID
		                            LEFT JOIN [dbo].[TaxYearPeriod] D ON C.TaxPeriodSystemID = D.SystemID
		                            LEFT JOIN [dbo].[SalaryHead] SH ON A.SalaryHeadID = SH.SalaryHeadID
		                            LEFT JOIN [dbo].[Currency] ECR ON A.EntryIncomeCurrencyID = ECR.CurrencyCode
		                            LEFT JOIN [dbo].[Currency] DCR ON A.DefinationCurrencyID = DCR.CurrencyCode
		                            LEFT JOIN [dbo].[Currency] LCR ON A.LocalCurrencyID = LCR.CurrencyCode";

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
        #endregion Tax Policy

        #region Tax Year

        public void GetCompanyTaxYear(string strCompanyID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
       //         strSQL = @"SELECT TC.TaxYearID, TY.TaxYearName
	      //                      FROM dbo.TaxYearPeriodAndCompanyAssignment TC
					  //                          LEFT JOIN dbo.TaxYear TY ON TC.TaxYearID = TY.TaxYearID
       //                     WHERE TC.CompanyID = '" + strCompanyID + @"'
							//GROUP BY TC.TaxYearID, TY.TaxYearName 
       //                     ORDER BY TY.TaxYearName";

                strSQL = @"SELECT CTY.TaxYearID, TY.TaxYearName
	                            FROM scs.CompanyTaxYearPeriod TC
												LEFT OUTER JOIN SCS.CompanyTaxYear CTY ON CTY.ID = TC.CompanyTaxYearId
					                            LEFT JOIN SCS.TaxYear TY ON CTY.TaxYearId = TY.Id
                            WHERE cty.CompanyID = '" + strCompanyID + @"'
                            GROUP BY cty.TaxYearID, TY.TaxYearName 
                            ORDER BY TY.TaxYearName";

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
        public void GetCompanyTaxYearDateWise(string sGroupID, string strCompanyID, string strDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                ////strSQL = @"SELECT TC.TaxYearID, TY.TaxYearName, TYP.period  
	               ////             FROM dbo.TaxYearPeriodAndCompanyAssignment TC
					           ////                 LEFT JOIN dbo.TaxYear TY ON TC.TaxYearID = TY.TaxYearID
					           ////                 INNER JOIN (SELECT * FROM dbo.TaxYearPeriod 
										      ////                      WHERE '" + strDate + @"' BETWEEN StartDate AND EndDate
											     ////                        AND GroupID = '" + sGroupID + @"') TYP ON TC.PeriodSystemID = TYP.SystemID
                ////            WHERE TC.CompanyID = '" + strCompanyID + @"'
                ////            ORDER BY TY.TaxYearName";
                strSQL = @" SELECT TY.Id TaxYearID,
                                   TY.TaxYearName,
                                   TYP.PeriodNo
                            FROM [SCS].[CompanyTaxYearPeriod] TC
                                      INNER JOIN (SELECT * FROM [SCS].[CompanyTaxYear] WHERE CompanyId='" + strCompanyID + @"') C ON TC.CompanyTaxYearId = C.Id
                                      INNER JOIN SCS.Taxyear TY ON TY.Id = C.TaxYearId
                                      INNER JOIN (SELECT * FROM SCS.TaxYearPeriod
                                                  WHERE '" + strDate + @"' BETWEEN StartDate AND EndDate
                                                 ) TYP ON TC.TaxYearPeriodId = TYP.Id
                           ORDER  BY TY.TaxYearName   ";

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
        public void xGetCompanyTaxYearPreviousDateWise(string sGroupID, string strCompanyID, string strDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {//scs.CompanyTaxYearPeriod
                strSQL = @"SELECT TC.TaxYearID, TY.TaxYearName, TYP.period  
	                            FROM dbo.TaxYearPeriodAndCompanyAssignment TC
					                            LEFT JOIN dbo.TaxYear TY ON TC.TaxYearID = TY.TaxYearID
					                            INNER JOIN (SELECT * FROM dbo.TaxYearPeriod 
										                        WHERE EndDate = (
                                                                                 SELECT MAX(EndDate) FROM dbo.TaxYearPeriod 
										                                            WHERE EndDate < '" + strDate + @"' 
											                                              AND GroupID = '" + sGroupID + @"'
                                                                                )
											                          AND GroupID = '" + sGroupID + @"') TYP ON TC.PeriodSystemID = TYP.SystemID
                            WHERE TC.CompanyID = '" + strCompanyID + @"'
                            ORDER BY TY.TaxYearName ";

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
        public void GetCompanyTaxYearPreviousDateWise(string sGroupID, string strCompanyID, string strDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {//scs.CompanyTaxYearPeriod
             //strSQL = @"SELECT TC.TaxYearID, TY.TaxYearName, TYP.period  
             //             FROM scs.CompanyTaxYearPeriod TC
             //                 LEFT JOIN dbo.TaxYear TY ON TC.TaxYearID = TY.TaxYearID
             //                 INNER JOIN (SELECT * FROM dbo.TaxYearPeriod 
             //                  WHERE EndDate = (
             //                                                                 SELECT MAX(EndDate) FROM dbo.TaxYearPeriod 
             //                                      WHERE EndDate < '" + strDate + @"' 
             //                                         AND GroupID = '" + sGroupID + @"'
             //                                                                )
             //                     AND GroupID = '" + sGroupID + @"') TYP ON TC.PeriodSystemID = TYP.SystemID
             //            WHERE TC.CompanyID = '" + strCompanyID + @"'
             //            ORDER BY TY.TaxYearName ";

                strSQL = @"select p.TaxYearID,y.TaxYearName,TYP.PeriodNo
							from scs.TaxYearPeriod p 
							left outer join scs.CompanyTaxYearPeriod cp on p.Id=cp.TaxYearPeriodId
							left outer join scs.TaxYear y on y.Id=p.TaxYearId
							left outer join scs.CompanyTaxYear cy on cy.TaxYearId = y.Id
							
							INNER JOIN (
										SELECT * FROM scs.TaxYearPeriod
                                        WHERE EndDate = (
                                                            SELECT MAX(EndDate) FROM scs.TaxYearPeriod
                                                            WHERE EndDate < '" + strDate + @"'                                                                                    
                                                        )
									) TYP ON TYP.Id = p.Id 

							where cy.CompanyId='" + strCompanyID + @"'";







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
       
        #endregion Tax Year

        #region TAX Year Process

        public void LoadEmployeeInGrdForTaxYearProcess(string sPlantID, string strTaxGroupID, string sStartDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectTaxProc = Convert(bit, 'True'), E.SystemID EmpSystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ,
                                  E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, e.GenderID GenderName, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC, 
                                  U.UserName AS Unit, Dv.UserName AS Division, De.UserName AS Department, Se.UserName AS Section, SuS.UserName SubSection, Dsg.UserName AS Designation, '' SalaryID,
                                  EC.UserName EmpCategoryName, '' TaxDefineMasterSystemID, '01-Jul-2018' EffectiveDate, 'No' Taxable, '0' TotalTaxPayablePeriod, '0' PartialTaxPayablePeriod, '0' TaxPayablePeriod, '0' TaxAbleIncome, 
                                  '0' InvestmentAmount, '0' TaxPayableAmount, '0' RebateAmount, '0' OpeningBalance, '0' NetTaxPayableAmount, '0' MonthlyTaxPayableAmount
                           FROM [dbo].[EmployeeInformation] E
                                    INNER JOIN 
                                            [DBO].[TaxGroupTagWithEmployee] TGE ON E.SystemID = TGE.EmpInfoSystemID AND TGE.TaxGroupID = 'TAXG2017-3'	
                                    LEFT OUTER JOIN 
                                            [HKP].EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                                    LEFT OUTER JOIN 
                                            [ORG].[Unit] AS U ON U.Id = E.UnitID 
                                    LEFT OUTER JOIN 
                                            [ORG].[Division] AS Dv ON Dv.Id = E.DivisionID 
                                    LEFT OUTER JOIN 
                                            [ORG].[Department] AS De ON De.Id = E.DepartmentID 
                                    LEFT OUTER JOIN 
                                            hkp.[Designation] AS Dsg ON Dsg.Id = E.DesignationSystemID 
                                    LEFT OUTER JOIN 
                                            [ORG].[Section] AS Se ON Se.Id = E.SectionID 
                                    LEFT OUTER JOIN 
                                            [ORG].[SubSection] AS SuS ON SuS.Id = E.SubSectionID
							WHERE E.PlantID = '" + sPlantID + @"' AND E.EmployeeStatus = 'Active' AND E.DOJ <= '" + sStartDate + @"'
                            ORDER BY E.EmployeeCode";

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
        public void LoadEmpSalaryInfoDefineDataOnGrid(string sPlantID, string sEmpSystemID, string sTaxYrStartDt, string sTaxYrEndDt, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, SLID.SalaryID,
                                  A.SalaryRuleMasterSystemID, A.SalaryRuleName, A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, A.TagAndUnTag,  
                                  A.SalaryHeadID, A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinationCurrencyID, A.DefinationCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, A.SequenceNo, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), EndDate = CASE WHEN REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') = '" + sTaxYrEndDt + @"' THEN ''
																															     ELSE REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') END, 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinationCurrencyID, SLID.AmtDefinationRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												     WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													 WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												ELSE Convert(bit, 'False') END,
										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, 
                                        ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinationCurrency AS DefinationCurrencyID, DECR.CurrencyDesc AS DefinationCurrency,
                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.CurrencyDesc AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,
                                        SequenceNo = (ISNULL(Fml.SequenceNo, 0) + ISNULL(Fxd.SequenceNo, 0) + ISNULL(SG.SequenceNo, 0)), SH.HeadCategory
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN Currency ECR ON CRC.AmtEntryCurrency = ECR.CurrencyCode
                                        LEFT JOIN Currency DECR ON CRC.AmtDefinationCurrency = DECR.CurrencyCode
                                        LEFT JOIN Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.CurrencyCode
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes, 
                                                            ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo 
                                                    FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)) Fml 
                                                        ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleGeneral WHERE IsFixed = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)) Fxd
                                                        ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND SG.IsOpen = 1
                                  ) A
                                        LEFT JOIN (
                                                   SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, EffectiveDate = CASE WHEN ISNULL(SDED.EffectiveDate,'') != '' AND SDED.EffectiveDate <= '" + sTaxYrStartDt + @"' THEN '" + sTaxYrStartDt + @"'
																															   WHEN ISNULL(SDED.EffectiveDate,'') = '' AND SDM.EffectiveDate < '" + sTaxYrStartDt + @"' THEN '" + sTaxYrStartDt + @"'
																															   WHEN ISNULL(SDED.EffectiveDate,'') = '' THEN REPLACE(CONVERT(VARCHAR(11), SDM.EffectiveDate, 106),' ','-')
																															  ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EffectiveDate, 106),' ','-') END,
																										 EndDate = CASE WHEN ISNULL(SDED.EndDate,'') = '' THEN '" + sTaxYrEndDt + @"'
																															  ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EndDate, 106),' ','-') END,
														  SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													      SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinationCurrencyID, SD.AmtDefinationRate
												    FROM SalaryInfoDefineMaster SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
                                                                        INNER JOIN (
                                                                                    SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                    WHERE EffectiveDate <= '" + sTaxYrStartDt + @"'
                                                                                    GROUP BY EmpInfoSystemID
                                                                                   ) SLED ON SLED.EmpInfoSystemID = SDM.EmpInfoSystemID AND SLED.EffectiveDate = SDM.EffectiveDate
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID  				
                    WHERE A.SequenceNo > 0 AND ISNULL(A.HeadCategory, '') != 'Tax' AND A.PlantID = '" + sPlantID + @"'";

                if (sEmpSystemID.Trim() != "")
                {
                    strSQL = strSQL + @"
                                        AND SLID.EmpInfoSystemID IN ('" + sEmpSystemID + @"')";
                }

                strSQL = strSQL + @"
                    ORDER BY A.SequenceNo, A.HeadType DESC, A.SalaryHead ASC";

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

        public void LoadTaxableIncomeSlrWiseDataOnGridForTaxYearProc(string sGroupID, string sPlantID, string sEmpSystemID, string sTAXGroup, string sTAXYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TAISH.SystemID, TAISH.EmpInfoSystemID, TAISH.TaxDefineMasterSystemID, TAISH.TaxPolicyMstID, 
                                  TPM.TaxPolicyName, TAISH.TaxGroupID, TAISH.TaxYearID, TG.TaxGroupName, TAISH.SalaryHeadID, SH.SalaryHead, 
                                  TAISH.EntryIncomeCurrencyID, EC.CurrencyDesc EntryIncomeCurrency, TAISH.EntryIncome, 
                                  TAISH.DefinationCurrencyID, DC.CurrencyDesc DefinationCurrency, TAISH.DefinationAmount, 
                                  TAISH.DefinationCurrencyRate, TAISH.TaxPayablePeriod, TAISH.LocalCurrencyID, LC.CurrencyDesc LocalCurrency,
                                  TAISH.ConvertionRate, (TAISH.DefinationAmount * TAISH.TaxPayablePeriod) YearlyIncome
                           FROM TaxableIncomeSalaryHeadWise TAISH
                                    INNER JOIN TaxDefineMaster TDM ON TAISH.TaxDefineMasterSystemID = TDM.SystemID AND TDM.TaxYearID = '" + sTAXYear + @"'
                                    INNER JOIN TaxGroup TG ON TAISH.TaxGroupID = TG.SystemID AND TG.SystemID = '" + sTAXGroup + @"'
                                    INNER JOIN TaxPolicyMaster TPM ON TAISH.TaxPolicyMstID = TPM.SystemID AND TPM.TaxYearID = '" + sTAXYear + @"'
                                    INNER JOIN SalaryHead SH ON TAISH.SalaryHeadID = SH.SalaryHeadID 
                                    LEFT JOIN Currency EC ON TAISH.EntryIncomeCurrencyID = EC.CurrencyCode
                                    LEFT JOIN Currency DC ON TAISH.DefinationCurrencyID = DC.CurrencyCode
                                    LEFT JOIN Currency LC ON TAISH.LocalCurrencyID = LC.CurrencyCode
                           WHERE TAISH.GroupID = '" + sGroupID + @"' AND TAISH.PlantID = '" + sPlantID + @"'  
                                 AND TAISH.TaxPayablePeriod != 0";

                if (sEmpSystemID != "")
                {
                    strSQL = strSQL + @" AND TAISH.EmpInfoSystemID IN ('" + sEmpSystemID + @"')";
                }

                strSQL = strSQL + @" 
                          ORDER BY TDM.TaxPaidUptoYear DESC, TDM.TaxPaidUptoMonth DESC";

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
        #endregion TAX Year Process
        
        #region Bonus
        public void GetTaxGroupWisePolicyMaster(string sTaxPolMatSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * 
                            FROM TaxPolicyMaster 
                          WHERE SystemID IN ('" + sTaxPolMatSysID + @"')
                          ORDER BY SystemID";

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
        public void GetActualBonusDisbused(string sGroupID, string sPlantID, string sEmpInfoSystemID, string sStartmonth, string sEndmonth, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpSystemID, ISNULL(SUM(BonusAmount), 0) BonusAmount FROM BonusPaymentActual 
                                    WHERE BnsMstSystemID IN (
                                                             SELECT SystemID FROM BonusPaymentActualMaster 
                                                                WHERE EffectiveDate BETWEEN '" + sStartmonth + @"' AND '" + sEndmonth + @"'
                                                            )
                                           AND IsApproved = 1";

                if (sEmpInfoSystemID.Trim() != "")
                {
                    strSQL = strSQL + @"
                                    AND EmpSystemID IN ('" + sEmpInfoSystemID + @"')";
                }

                strSQL = strSQL + @"
                                   GROUP BY EmpSystemID";

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

        public void ApprovalRollback(string _SalarySystemId,string _EmployeeSystemId,string _TaxDefinedMasterSystemId,string _TaxYearId)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                string _SSM = @"update SalaryInfoDefineMaster set 
                                                                        IsApproved=0
                                                                        ,ApprovedBy='" + DBNull.Value + @"'
                                                                        ,DateApproved='" + DBNull.Value + @"'
                                                                    WHERE SystemId = '" + _SalarySystemId + @"' ";
                string _EMP = @"update EmployeeInformation set SalaryRuleMasterSystemID=NULL  where SystemId='" + _EmployeeSystemId + @"'";
                string _SSD = @"delete FROM SalaryInfoDefine 
		                            WHERE SalaryHeadID IN (
								                    SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'Tax'
							                      ) 
			                      AND SalaryID='" + _SalarySystemId + @"' ";

                string _TM = @"delete from TaxDefineMaster WHERE EmpInfoSystemID IN ('" + _EmployeeSystemId + @"') AND SystemID IN ('" + _TaxDefinedMasterSystemId + @"')";
                string _TY = @"delete FROM TaxableYearlyActualIncomeSalaryHeadWise WHERE EmpInfoSystemID IN ('" + _EmployeeSystemId + @"') AND TaxYearID = '" + _TaxYearId + @"'";
                string _TSH = @"delete FROM TaxableIncomeSalaryHeadWise WHERE EmpInfoSystemID IN ('" + _EmployeeSystemId + @"') AND TaxDefineMasterSystemID = '" + _TaxDefinedMasterSystemId + @"'";
                string _TD = @"delete FROM TaxDeductionInfoMonthWise WHERE EmpInfoSystemID IN ('" + _EmployeeSystemId + @"') AND TaxDefineMasterSystemID='" + _TaxDefinedMasterSystemId + "' ";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(_TD, true, "1");
                objCon.ExecuteNonQueryWrapper(_TSH, true, "1");
                objCon.ExecuteNonQueryWrapper(_TY, true, "1");
                objCon.ExecuteNonQueryWrapper(_TM, true, "1");

                objCon.ExecuteNonQueryWrapper(_SSD, true, "1");
                objCon.ExecuteNonQueryWrapper(_SSM, true, "1");
                objCon.ExecuteNonQueryWrapper(_EMP, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception)
            {
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                    throw;
                }               
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function 

        #endregion
        
        #region SSA
        public void GetTaxableIncome(string sEmpInfoSystemID, string Taxyearid, int taxpaidtomonth, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 

                                                         h.SalaryHeadID,h.EntryIncome,h.DefinitionAmount,h.EntryIncomeCurrencyID,h.DefinitionCurrencyID
                                                        ,h.DefinitionCurrencyRate,h.ConvertionRate,m.TaxStartFromYear,m.TaxStartFromMonth
                                                        ,isnull(d.c,0) c,m.SystemID
                                                        --,h.EntryIncome*d.c CalculatedAmount

                                                        ,h.EntryIncome*d.c PaidIncome
                                                        ,h.EntryIncome*(m.TaxStartFromMonth-1+ isnull(np.c,0)) NonPaidIncome

                                                        from TaxableIncomeSalaryHeadWise h
                                                            left outer join 
					                                        (

					                                        select TaxStartFromMonth,TaxStartFromYear ,SystemID
					                                        from TaxDefineMaster 
					                                        where EmpInfoSystemID='" + sEmpInfoSystemID + @"'  and TaxYearID='" + Taxyearid + @"'
					                                        and EffectiveDate=(select min(EffectiveDate) from TaxDefineMaster 
                                                            where EmpInfoSystemID='" + sEmpInfoSystemID + @"' and TaxYearID='" + Taxyearid + @"')

					                                        )m on h.TaxDefineMasterSystemID=m.SystemID

                                    left outer join (select count(SystemID) c,TaxDefineMasterSystemID 
						                                    from TaxDeductionInfoMonthWise
						                                    where EmpInfoSystemID='" + sEmpInfoSystemID + @"' 
						                                    and IsPaid=1 
						                                    and TaxPayablePeriod<=" + taxpaidtomonth + @"	
						                                    and TaxPeriodSystemID in (select Id from scs.TaxYearPeriod where TaxYearId='"+ Taxyearid + @"')	
						                                    group by TaxDefineMasterSystemID 				
				                                    ) d on h.TaxDefineMasterSystemID=d.TaxDefineMasterSystemID 

                                    left outer join (select count(SystemID) c,TaxDefineMasterSystemID 
						                                    from TaxDeductionInfoMonthWise
						                                    where EmpInfoSystemID='" + sEmpInfoSystemID + @"' 
						                                    and IsPaid=0
						                                    and TaxPayablePeriod<=" + taxpaidtomonth + @"	
						                                    and TaxPeriodSystemID in (select Id from scs.TaxYearPeriod where TaxYearId='" + Taxyearid + @"')	
						                                    group by TaxDefineMasterSystemID 				
				                                    ) np on h.TaxDefineMasterSystemID=np.TaxDefineMasterSystemID 


                                    where h.EmpInfoSystemID='" + sEmpInfoSystemID + @"'";

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
        public void GetPaidUpto(string sEmpInfoSystemID, string Taxyearid, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
	                            select TaxPayablePeriod from TaxDeductionInfoMonthWise where SystemID =(
                            select m.MonthlyTaxSystemID
					                            from TaxDefineMaster m
					                            where EmpInfoSystemID='" + sEmpInfoSystemID + @"'  and TaxYearID='" + Taxyearid + @"'
					                            and EffectiveDate=(select max(EffectiveDate) from TaxDefineMaster 
                                                where EmpInfoSystemID='" + sEmpInfoSystemID + @"' and TaxYearID='"+ Taxyearid + @"')
					                            )";

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
        public void GetTaxableIncomeYearly(string sEmpInfoSystemID, string Taxyearid, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select  m.SystemID,m.TaxableIncome,isnull(pd.c,0) PaidMonth,isnull(npd.c,0) NonPaidMonth from TaxDefineMaster  m
                            left outer join (select TaxDefineMasterSystemID,count(systemid) c from TaxDeductionInfoMonthWise where EmpInfoSystemID='" + sEmpInfoSystemID + @"' and IsPaid=1
                            group by TaxDefineMasterSystemID
                            ) pd on m.SystemID=pd.TaxDefineMasterSystemID
                            left outer join (select TaxDefineMasterSystemID,count(systemid) c from TaxDeductionInfoMonthWise where EmpInfoSystemID= '" + sEmpInfoSystemID + @"' and IsPaid=0
                            group by TaxDefineMasterSystemID
                            ) npd on m.SystemID=npd.TaxDefineMasterSystemID
                            where m.EmpInfoSystemID='" + sEmpInfoSystemID + "' and TaxYearID='"+ Taxyearid + "'";

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
        public void GetFirstTaxMaster(string sEmpInfoSystemID, string Taxyearid, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select  m.SystemID,m.TaxableIncome,isnull(pd.c,0) PaidMonth,isnull(npd.c,0) NonPaidMonth
                                ,isnull(12-isnull(pd.c,0)-isnull(npd.c,0),0) RestMonth,
                                isnull((m.TaxableIncome/12)*(isnull(pd.c,0)+(12-isnull(pd.c,0)-isnull(npd.c,0))),0) RestTaxableIncome
                                    ,isnull(isnull(pd.c,0)+(12-isnull(pd.c,0)-isnull(npd.c,0)),0) ValidMonth
                                 from TaxDefineMaster  m
                                left outer join (select TaxDefineMasterSystemID,count(systemid) c from TaxDeductionInfoMonthWise 
                                                                where EmpInfoSystemID='" + sEmpInfoSystemID + @"' and IsPaid=1
                                                            group by TaxDefineMasterSystemID
                                                            ) pd on m.SystemID=pd.TaxDefineMasterSystemID
                                                            left outer join (select TaxDefineMasterSystemID,count(systemid) c from TaxDeductionInfoMonthWise 
                                                                    where EmpInfoSystemID= '" + sEmpInfoSystemID + @"' and IsPaid=0
                                                            group by TaxDefineMasterSystemID
                                                            ) npd on m.SystemID=npd.TaxDefineMasterSystemID
                                where m.EmpInfoSystemID='" + sEmpInfoSystemID + @"' and m.TaxYearID='" + Taxyearid + @"'
                                and EffectiveDate=
                                (select min(EffectiveDate) from TaxDefineMaster where EmpInfoSystemID='" + sEmpInfoSystemID + @"' and TaxYearID='" + Taxyearid + @"')";

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