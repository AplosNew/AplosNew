using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace OTSBD
{
    public class clsEmpExtraSalaryAmt
    {
        public clsEmpExtraSalaryAmt()
        {
            // TODO: Add constructor logic here
        }

        #region Arear Amount

        public void LoadEmpArearSalaryAmtOnGrid(string EmpSystemId, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM 
		                            (SELECT ESC.SystemID, ESC.CurrencyRuleSystemID, ESM.EmpInfoSystemID, ESM.MonthNo, ESM.YearNo, 
				                            ESC.SalaryHeadID, SH.SalaryHead, SH.HeadType, ESC.EntryCurrencyID, 
				                            CRE.CurrencyDesc AS EntryCurrency, ESC.EntryAmount, ESC.DefineCurrencyID AS DefinitionCurrencyID, 
				                            CRD.CurrencyDesc AS DefinitionCurrency, ESC.DefineAmount, ESC.AmtDefinationCurrencyID, 
				                            CRAD.CurrencyDesc AS AmtDefinationCurrency, ESC.AmtDefinationRate
		                            FROM MonthWiseExtraSalaryAmtChild ESC
				                            INNER JOIN MonthWiseExtraSalaryAmtMaster ESM ON ESC.MWESAMasterSystemID = ESM.SystemID
				                            LEFT JOIN SalaryHead SH ON ESC.SalaryHeadID = SH.SalaryHeadID
				                            LEFT JOIN Currency CRE ON ESC.EntryCurrencyID = CRE.CurrencyCode
				                            LEFT JOIN Currency CRD ON ESC.DefineCurrencyID = CRD.CurrencyCode
                                            LEFT JOIN Currency CRAD ON ESC.AmtDefinationCurrencyID = CRAD.CurrencyCode) A 
                          WHERE EmpInfoSystemID = '" + EmpSystemId + @"'";
                if (YearNo > 0)
                {
                    strSQL = strSQL + @" AND YearNo = " + YearNo + @"";
                }
                if (MonthNo > 0)
                {
                    strSQL = strSQL + @" AND MonthNo = " + MonthNo + @"";
                }

                strSQL = strSQL + @" ORDER BY YearNo, MonthNo";

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

        public void GetMonthWiseExtraSalaryAmtMaster(string EmpSystemId, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                                    FROM MonthWiseExtraSalaryAmtMaster 
                            WHERE EmpInfoSystemID = '" + EmpSystemId + @"' AND YearNo = " + YearNo + @"
                                 AND MonthNo = " + MonthNo + @"";

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

        public void DeleteMWESAChildB4Save(string MstSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM MonthWiseExtraSalaryAmtChild WHERE SystemID = '" + MstSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }// End Function

        public void GetMonthWiseExtraSalaryAmtChild(string MstSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE MWESAMasterSystemID = '" + MstSystemID + @"'";

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

        public void SaveDataSets(params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                //objCon.ExecuteNonQueryWrapper("DELETE FROM MonthWiseExtraSalaryAmtChild WHERE MWESAMasterSystemID = '" + MstSystemID + "'", true, "1");

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
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        } // End Function

        #endregion Arear Amount

        #region External Amount

        public void LoadEmpExternalUploadOnGrid(string EmpSystemId, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM 
		                            (SELECT ESC.SystemID, ESC.CurrencyRuleSystemID, ESM.EmpInfoSystemID, ESM.MonthNo, ESM.YearNo, 
				                            ESC.SalaryHeadID, SH.SalaryHead, SH.HeadType, ESC.EntryCurrencyID, 
				                            CRE.CurrencyDesc AS EntryCurrency, ESC.EntryAmount, ESC.DefineCurrencyID AS DefinitionCurrencyID, 
				                            CRD.CurrencyDesc AS DefinitionCurrency, ESC.DefineAmount, ESC.AmtDefinationCurrencyID, 
				                            CRAD.CurrencyDesc AS AmtDefinationCurrency, ESC.AmtDefinationRate
		                            FROM MonthWiseExtraSalaryAmtChild ESC
				                            INNER JOIN MonthWiseExtraSalaryAmtMaster ESM ON ESC.MWESAMasterSystemID = ESM.SystemID
				                            INNER JOIN SalaryHead SH ON ESC.SalaryHeadID = SH.SalaryHeadID
				                            LEFT JOIN Currency CRE ON ESC.EntryCurrencyID = CRE.CurrencyCode
				                            LEFT JOIN Currency CRD ON ESC.DefineCurrencyID = CRD.CurrencyCode
                                            LEFT JOIN Currency CRAD ON ESC.AmtDefinationCurrencyID = CRAD.CurrencyCode
                                    WHERE SH.ExtDataUpload = 1 AND ESC.ExtDataUploadApp = 'Yes') A 
                          WHERE EmpInfoSystemID = '" + EmpSystemId + @"'";
                if (YearNo > 0)
                {
                    strSQL = strSQL + @" AND YearNo = " + YearNo + @"";
                }
                if (MonthNo > 0)
                {
                    strSQL = strSQL + @" AND MonthNo = " + MonthNo + @"";
                }

                strSQL = strSQL + @" ORDER BY YearNo, MonthNo";

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

        #endregion External Amount

        #region External Amount From Excel

        public void LoadExternalUploadFromExcelOnGrid(string sEntityID, string strSalaryHdID, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            string fromdate = new DateTime(YearNo, MonthNo, 1).ToString("dd-MMM-yyyy");
            string todate = new DateTime(YearNo, MonthNo, DateTime.DaysInMonth(YearNo, MonthNo)).ToString("dd-MMM-yyyy");
            try
            {


                strSQL = @"SELECT'' MWESAMasterSystemID
                                    , '' MWESAChildSystemID, E.PlantId,PLN.UserName AS PlantName,
                                    E.SystemID EmpInfoSystemID, E.EmployeeCode, 
		                            E.EmployeeName, CR.MstSystemID CurrencyRuleSystemID, CR.SalaryHeadID, SD.SalaryHead, SD.HeadType
									, FORMAT(E.DOJ,'dd-MMM-yyyy') AS DOJ,FORMAT(E.DOS,'dd-MMM-yyyy') AS DOS,E.EmployeeStatus,
		                            '' ExistCurrencyID
									, '' ExistCurrency
									, '' ExistAmount,
		                            CR.AmtEntryCurrency EntryCurrencyID, CrEn.Code EntryCurrency, '0' EntryAmount, CR.AmtDefinitionCurrency 
		                            DefinitionCurrencyID, CrDe.Code DefinitionCurrency, '0' DefineAmount, CR.AmtDisbusmentCurrency 
		                            AmtDefinationCurrencyID, '0' AmtDefinationRate, '' Remarks,CASE WHEN ISNULL(sl.IsLocked,0)=0 THEN 'False' ELSE 'True' END AS isSalaryLocked
                            FROM dbo.EmployeeInformation E
                            left join org.Plant PLN ON PLN.Id=E.PlantId
                            LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=e.SystemId AND  sl.YearNo=YEAR('" + fromdate + @"') AND sl.MonthNo=MONTH('" + fromdate + @"')
		                            left join dbo.SalaryInfoDefineMaster SM ON SM.EmpInfoSystemID=E.SystemId and SM.IsApproved=1
        INNER JOIN dbo.SalaryRuleMaster SR ON SM.SalaryRuleMasterSystemID = SR.SystemID
		                            INNER JOIN dbo.CurrencyRuleChild CR ON SR.CurrencyRuleSystemID = CR.MstSystemID AND CR.SalaryHeadID = '" + strSalaryHdID + @"'
		                            LEFT JOIN dbo.SalaryHead SD ON CR.SalaryHeadID = SD.SalaryHeadID
		                            --LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster MWESMat ON E.SystemID = MWESMat.EmpInfoSystemID 
															                            ---AND MWESMat.MonthNo = '" + MonthNo + @"' 
															                            ---AND MWESMat.YearNo = '" + YearNo + @"'
		                            --LEFT JOIN dbo.MonthWiseExtraSalaryAmtChild MWESChd ON MWESMat.SystemID = MWESChd.MWESAMasterSystemID 
															                           --- AND SD.SalaryHeadID = MWESChd.SalaryHeadID
															                           --- AND CR.MstSystemID = MWESChd.CurrencyRuleSystemID
		                            ---LEFT JOIN SCS.Currency MWESChdCr On MWESChd.EntryCurrencyID = MWESChdCr.ID
		                            LEFT JOIN SCS.Currency CrEn ON CR.AmtEntryCurrency = CrEn.ID
		                            LEFT JOIN SCS.Currency CrDe ON CR.AmtDefinitionCurrency = CrDe.ID
                            WHERE E.PlantID = '" + sEntityID + @"'
                                   AND E.DOJ <= '" + todate + @"'
                                            --and E.EmployeeStatus='Active'3dszvya/,nib n
			                              AND (E.DOS >= '" + fromdate + @"' OR ISNULL(E.DOS,'') = '' OR E.DOS = '01/01/1901')
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

        public void LoadCompanyWiseExternalUploadFromExcelOnGrid(string sEntityID, string strSalaryHdID, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            string fromdate = new DateTime(YearNo, MonthNo, 1).ToString("dd-MMM-yyyy");
            string todate = new DateTime(YearNo, MonthNo, DateTime.DaysInMonth(YearNo, MonthNo)).ToString("dd-MMM-yyyy");
            try
            {


                strSQL = @"SELECT'' MWESAMasterSystemID
                                    , '' MWESAChildSystemID, E.PlantId,PLN.UserName AS PlantName,
                                    E.SystemID EmpInfoSystemID, E.EmployeeCode, 
		                            E.EmployeeName, CR.MstSystemID CurrencyRuleSystemID, CR.SalaryHeadID, SD.SalaryHead, SD.HeadType
									, FORMAT(E.DOJ,'dd-MMM-yyyy') AS DOJ,FORMAT(E.DOS,'dd-MMM-yyyy') AS DOS,E.EmployeeStatus,
		                            '' ExistCurrencyID
									, '' ExistCurrency
									, '' ExistAmount,
		                            CR.AmtEntryCurrency EntryCurrencyID, CrEn.Code EntryCurrency, '0' EntryAmount, CR.AmtDefinitionCurrency 
		                            DefinitionCurrencyID, CrDe.Code DefinitionCurrency, '0' DefineAmount, CR.AmtDisbusmentCurrency 
		                            AmtDefinationCurrencyID, '0' AmtDefinationRate, '' Remarks,CASE WHEN ISNULL(sl.IsLocked,0)=0 THEN 'False' ELSE 'True' END AS isSalaryLocked
                            FROM dbo.EmployeeInformation E
                            left join org.Plant PLN ON PLN.Id=E.PlantId
                            LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=e.SystemId AND  sl.YearNo=YEAR('" + fromdate + @"') AND sl.MonthNo=MONTH('" + fromdate + @"')
		                            INNER JOIN dbo.SalaryRuleMaster SR ON E.SalaryRuleMasterSystemID = SR.SystemID
		                            INNER JOIN dbo.CurrencyRuleChild CR ON SR.CurrencyRuleSystemID = CR.MstSystemID AND CR.SalaryHeadID = '" + strSalaryHdID + @"'
		                            LEFT JOIN dbo.SalaryHead SD ON CR.SalaryHeadID = SD.SalaryHeadID
		                            --LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster MWESMat ON E.SystemID = MWESMat.EmpInfoSystemID 
															                            ---AND MWESMat.MonthNo = '" + MonthNo + @"' 
															                            ---AND MWESMat.YearNo = '" + YearNo + @"'
		                            --LEFT JOIN dbo.MonthWiseExtraSalaryAmtChild MWESChd ON MWESMat.SystemID = MWESChd.MWESAMasterSystemID 
															                           --- AND SD.SalaryHeadID = MWESChd.SalaryHeadID
															                           --- AND CR.MstSystemID = MWESChd.CurrencyRuleSystemID
		                            ---LEFT JOIN SCS.Currency MWESChdCr On MWESChd.EntryCurrencyID = MWESChdCr.ID
		                            LEFT JOIN SCS.Currency CrEn ON CR.AmtEntryCurrency = CrEn.ID
		                            LEFT JOIN SCS.Currency CrDe ON CR.AmtDefinitionCurrency = CrDe.ID
                           WHERE E.CompanyId = '" + sEntityID + @"' AND
                                    E.DOJ <= '" + todate + @"'
                                            --and E.EmployeeStatus='Active'
			                              AND (E.DOS >= '" + fromdate + @"' OR ISNULL(E.DOS,'') = '' OR E.DOS = '01/01/1901')
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

        public void xGetMthWiseExtSalAmtMaster(string plantid, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL =
                             @"SELECT *
                                    FROM MonthWiseExtraSalaryAmtMaster 
                            WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and plantid='" + plantid + "'";

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

        public void xGetMthWiseExtSalAmtChild(string plantid, int YearNo, int MonthNo, string SalaryHeadId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                                                                    WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and plantid='" + plantid + @"')";

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
        public void xxGetMthWiseExtSalAmtChild(string empids, int YearNo, int MonthNo, string SalaryHeadId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                                                                    WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and EmpInfoSystemID in (" + empids + @"))";

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

        public void xDeleteOldExtraData(string empids, int YearNo, int MonthNo, string SalaryHeadId)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = @"delete FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                          WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" )";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");

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
        public void GetMthWiseExtSalAmtMaster(string empids, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                                    FROM MonthWiseExtraSalaryAmtMaster 
                            WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and EmpInfoSystemID in (" + empids + @")";

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
        public void CompanyWiseGetMonththWiseExtSalAmtMaster(string plantid, string empids, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                                    FROM MonthWiseExtraSalaryAmtMaster 
                            WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and EmpInfoSystemID in (" + empids + @")";

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

        public void GetMthWiseExtSalAmtChild(string empids, int YearNo, int MonthNo, string SalaryHeadId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                                                                    WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and EmpInfoSystemID in (" + empids + @"))";

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

        public void CompanyWiseGetMonthWiseExtSalAmtChild(string plantid, string empids, int YearNo, int MonthNo, string SalaryHeadId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                                                                    WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and EmpInfoSystemID in (" + empids + @"))";

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

        public void DeleteOldExtraData(int YearNo, int MonthNo, string plantId, string SalaryHeadId)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            string strSQL = string.Empty;
            string strSQL2 = string.Empty;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strSQL = @"DELETE FROM MonthWiseExtraSalaryAmtChild
                           FROM MonthWiseExtraSalaryAmtChild C
                           JOIN MonthWiseExtraSalaryAmtMaster M ON m.SystemID=c.MWESAMasterSystemID
                           
                           WHERE m.PlantID='" + plantId + @"' AND M.MonthNo= " + MonthNo + @" AND M.YearNo=" + YearNo + @" AND C.SalaryHeadID='" + SalaryHeadId + @"'
                           AND M.EmpInfoSystemID NOT IN (SELECT sl.EmpSystemId
                        FROM  SalaryLock AS sl where sl.YearNo=" + YearNo + @" AND sl.MonthNo=" + MonthNo + @")";

                //strSQL2 = @"DELETE FROM [MonthWiseExtraSalaryAmtMaster] 
                //          WHERE SystemID not in (SELECT MWESAMasterSystemID FROM MonthWiseExtraSalaryAmtChild  ) AND PlantID ='" + plantId + @"'";

                strSQL2 = @"  DELETE FROM [MonthWiseExtraSalaryAmtMaster]
                          WHERE ISNULL(SystemID,'') IN (SELECT ISNULL(M.SystemID,'') FROM [MonthWiseExtraSalaryAmtMaster] M 
                          LEFT JOIN MonthWiseExtraSalaryAmtChild C ON C.MWESAMasterSystemID=M.SystemID
                          WHERE ISNULL(c.SystemID,'')='') AND PlantID='" + plantId + @"'";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(300);
                con.BeginTransaction();

                con.executeQuery(strSQL);
                con.executeQuery(strSQL2);
                con.CommitTransaction();

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenConnection("1");
                //objCon.BeginTransaction();
                //IsTransactionStarted = true;

                //objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                //objCon.ExecuteNonQueryWrapper(strSQL2, true, "1");

                //objCon.CommitTransaction();
                //IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
        public void DeleteOldExtraDataCompanyWise(int YearNo, int MonthNo, string CompanyId, string SalaryHeadId)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            string strSQL = string.Empty;
            string strSQL2 = string.Empty;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strSQL = @"DELETE FROM MonthWiseExtraSalaryAmtChild
                           FROM MonthWiseExtraSalaryAmtChild C
                           JOIN MonthWiseExtraSalaryAmtMaster M ON m.SystemID=c.MWESAMasterSystemID
                           
                           WHERE m.PlantID IN (SELECT p.Id FROM org.Plant AS p WHERE p.CompanyId='" + CompanyId + @"') AND M.MonthNo= " + MonthNo + @" AND M.YearNo=" + YearNo + @" 
                          AND C.SalaryHeadID='" + SalaryHeadId + @"'  AND ISNULL(M.EmpInfoSystemID,'') NOT IN (SELECT isnull(sl.EmpSystemId,'')
                        FROM  SalaryLock AS sl where sl.YearNo=" + YearNo + @" AND sl.MonthNo=" + MonthNo + @")";

                //strSQL = @"DELETE FROM MonthWiseExtraSalaryAmtChild
                //     FROM MonthWiseExtraSalaryAmtChild C
                //           JOIN MonthWiseExtraSalaryAmtMaster M ON m.SystemID=c.MWESAMasterSystemID
                //           LEFT JOIN SalaryLock SL ON SL.EmpSystemId=M.EmpInfoSystemID AND sl.YearNo=M.YearNo AND SL.MonthNo=M.MonthNo
                //           WHERE m.PlantID IN (SELECT p.Id FROM org.Plant AS p WHERE p.CompanyId='C20201') AND M.MonthNo= 5 AND M.YearNo=2021 
                //          AND C.SalaryHeadID='SHD202055'  AND ISNULL(sl.IsLocked,0)=0";


                strSQL2 = @"  DELETE FROM [MonthWiseExtraSalaryAmtMaster]
                          WHERE ISNULL(SystemID,'') IN (SELECT ISNULL(M.SystemID,'') FROM [MonthWiseExtraSalaryAmtMaster] M 
                          LEFT JOIN MonthWiseExtraSalaryAmtChild C ON C.MWESAMasterSystemID=M.SystemID
                          WHERE ISNULL(c.SystemID,'')='') ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(300);
                con.BeginTransaction();

                con.executeQuery(strSQL);
                con.executeQuery(strSQL2);
                con.CommitTransaction();

              
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
               
            }
        }//End Function

        public void DeleteCompanyWiseOldExtraData(int YearNo, int MonthNo, string plantId, string SalaryHeadId)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            string strSQL = string.Empty;
            string strSQL2 = string.Empty;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = @"DELETE FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                          WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" )";

                strSQL2 = @"DELETE FROM [MonthWiseExtraSalaryAmtMaster] 
                          WHERE SystemID not in (SELECT MWESAMasterSystemID FROM MonthWiseExtraSalaryAmtChild  ) ";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL2, true, "1");

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

        #endregion External Amount From Excel
        public void GetMonthWiseExtraSalary(string sMonth, string sYear, string plantId, string sSalaryHeadId, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT mem.EmpInfoSystemid, e.Employeecode, e.Employeename, mec.SalaryHeadID, sh.SalaryHead, mec.entryamount Amount FROM DBO.EMPLOYEEINFORMATION E
                            LEFT JOIN  MONTHWISEEXTRASALARYAMTMASTER MEM ON MEM.EMPINFOSYSTEMID = E.SYSTEMID
                            LEFT JOIN  MONTHWISEEXTRASALARYAMTCHILD MEC ON MEC.MWESAMASTERSYSTEMID = MEM.SYSTEMID
                            LEFT JOIN  SALARYHEAD SH ON SH.SALARYHEADID = MEC.SALARYHEADID

                            WHERE E.SYSTEMID IN(SELECT empinfosystemid FROM MONTHWISEEXTRASALARYAMTMASTER where PlantID ='" + plantId + @"') 

                                And MEC.SalaryHeadID = '" + sSalaryHeadId + @"'  AND MEM.MonthNo = '" + sMonth + @"' AND MEM.YearNo = '" + sYear + @"'
                            ORDER BY E.SYSTEMID ";
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
}
