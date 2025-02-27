using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OTSBD
{
    public class clsSalaryInfoNew
    {
        public clsSalaryInfoNew()
        {
            // TODO: Add constructor logic here
        }

        #region SalaryHead

        public void GetSalaryHeadLoadCboHeadCatWise(string strCRSystemID, string strHdCat, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (strCRSystemID != "")
                {
                    strSQL = @"SELECT SH.SalaryHead, SH.SalaryHeadID FROM SalaryHead SH
                            INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                            AND CRC.MstSystemID = '" + strCRSystemID + "' AND  HeadCategory = '" + strHdCat + "'";
                }
                else
                {
                    strSQL = @"SELECT SH.SalaryHead, SH.SalaryHeadID FROM SalaryHead SH
                            INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                            AND HeadCategory = '" + strHdCat + "'";
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

        #endregion SalaryHead

        #region Currency
        public void GetLocalCurrency(string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT C.Id AS LocalCurrency, C.[Name] AS Currency 		
		              //          FROM scs.Currency C		
			             //           INNER JOIN [SCS].[CurrencyTransaction] CA ON C.id = CA.CurrencyId		
		              //          WHERE  CA.CompanyID IN (SELECT DISTINCT CompanyID 		
													   //             FROM org.Plant		
													   //             WHERE ID = '" + sPlantID + @"')		
		              //          ORDER BY C.[Description]";
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
        public void LoadEmpAmtDefinationCurrency(string sGroupID, string sPlantID, string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //      strSQL = @"SELECT TOP(1)SDCR.CurrencyDesc AS AmtDefinationCurrency, SD.AmtDefinationCurrencyID, SD.AmtDefinationRate
                //                   FROM SalaryInfoDefineMaster SDM 
                //LEFT JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
                //LEFT JOIN Currency SDCR ON SD.AmtDefinationCurrencyID = SDCR.CurrencyCode
                //                   WHERE SDM.EmpInfoSystemID = '" + strEmpSystemID + @"' 
                //                         AND SDM.GroupID = '" + sGroupID + @"' AND SDM.PlantID = '" + sPlantID + @"'";
                strSQL = @"SELECT TOP(1) SDCR.[Description] AS AmtDefinitionCurrency, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
                             FROM SalaryInfoDefineMaster SDM 
										LEFT JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
										LEFT JOIN scs.Currency SDCR ON SD.AmtDefinitionCurrencyID = SDCR.Id
                             WHERE SDM.EmpInfoSystemID = '" + strEmpSystemID + @"' 
                                   AND SDM.GroupID = '" + sGroupID + @"' AND SDM.PlantID = '" + sPlantID + @"'";


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
        #endregion Currency

        #region Salary Information
        public void LoadSalaryStructure(string sGroupID, string sPlantID, string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select distinct d.SystemID,
                                Replace(CONVERT(VARCHAR(11), d.EffectiveDate, 106), ' ', '-') EffectiveDate,
                                d.IsApproved,
                                r.SalaryRuleName
                                from SalaryInfoDefineMaster d
                                left outer join SalaryRuleMaster r on r.SystemID=d.SalaryRuleMasterSystemID
                                where d.EmpInfoSystemID='" + strEmpSystemID + "' and d.GroupId='" + sGroupID + "' and d.PlantId='" + sPlantID + @"'
                               
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
        public void LoadSalaryStructureUnApproved(string sGroupID, string sPlantID, string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select distinct d.SystemID,
                                Replace(CONVERT(VARCHAR(11), d.EffectiveDate, 106), ' ', '-') EffectiveDate,
                                d.IsApproved,
                                r.SalaryRuleName
                                from SalaryInfoDefineMaster d
                                left outer join SalaryRuleMaster r on r.SystemID=d.SalaryRuleMasterSystemID
                                where d.EmpInfoSystemID='" + strEmpSystemID + @"' 
                                and d.GroupId='" + sGroupID + @"' 
                                and d.IsApproved=0 
                                and d.PlantId='" + sPlantID + @"'
                               
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
        public void LoadLatestSalaryStructure(string sGroupID, string sPlantID, string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select distinct d.SystemID,
                                    Replace(CONVERT(VARCHAR(11), d.EffectiveDate, 106), ' ', '-') EffectiveDate,
                                    d.IsApproved,
                                    r.SalaryRuleName
                                    from SalaryInfoDefineMaster d
                                    left outer join SalaryRuleMaster r on r.SystemID=d.SalaryRuleMasterSystemID
                                    where d.EmpInfoSystemID='" + strEmpSystemID + "' and d.GroupId='" + sGroupID + "' and d.PlantId='"+sPlantID+@"'

                                    and EffectiveDate=
	                                (
	                                select max(EffectiveDate) from SalaryInfoDefineMaster where EmpInfoSystemID='"+ strEmpSystemID + "' and GroupId='"+ sGroupID + "' and PlantId='"+ sPlantID + @"'
	                                )
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
        public void GetCutOffDate(string sGroupID,string CompanyId, string sPlantID, string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM [SCS].[OpeningBalanceCutOffDate] AS OCD 
                            WHERE OCD.CompanyGroupId='" + sGroupID + @"' 
                            AND OCD.CompanyId = '" + CompanyId + @"'
                            AND OCD.PlantId = '" + sPlantID + @"'
                             AND OCD.ModuleName = '" + bplib.clsWebLib.MODULE + @"'
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
        public void GetEmployeeDOJ(string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT SystemId,DOJ FROM EmployeeInformation
                            where SystemId = '" + strEmpSystemID + @"'
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
        public void GetSalaryRule(string sGroupID, string sPlantID, string sSalaryRuleID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryRuleMaster 
                                WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

                if (sSalaryRuleID != "")
                {
                    strSQL = @" AND SystemID = '" + sSalaryRuleID + @"'";
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
        public void LoadSalaryRuleInfo(string sPlantID, string sSalaryRuleID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT SM.SystemID SalaryRuleMasterSystemID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.AmtEntryCurrency,
		                          CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, SM.CurrencyRuleSystemID
                            FROM SalaryRuleMaster SM
	                            INNER JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                            WHERE SM.PlantID = '" + sPlantID + @"' AND  SM.IsActive=1 ";

                if (sSalaryRuleID != "")
                {
                    strSQL += @" AND SM.SystemID = '" + sSalaryRuleID + @"'";
                }

                strSQL += @" 
                            GROUP BY SM.SystemID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.AmtEntryCurrency,
		                            CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, SM.CurrencyRuleSystemID order by SM.SalaryRuleDescription";

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
        public void LoadAllSalaryRuleInfo(string sPlantID, string sSalaryRuleID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT SM.SystemID SalaryRuleMasterSystemID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.AmtEntryCurrency,
		                          CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, SM.CurrencyRuleSystemID
                            FROM SalaryRuleMaster SM
	                            INNER JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                            WHERE SM.PlantID = '" + sPlantID + @"'";

                if (sSalaryRuleID != "")
                {
                    strSQL += @" AND SM.SystemID = '" + sSalaryRuleID + @"'";
                }

                strSQL += @" 
                            GROUP BY SM.SystemID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.AmtEntryCurrency,
		                            CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, SM.CurrencyRuleSystemID";

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
        //before loading NA SalaryHead
        public void LoadEmpSlrOpenHdDataOnGrid(string sPlantID, string sSlrRuleMstSystemID, string sEmpSystemID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription,  
                                HeadType = CASE WHEN HeadType = 'D' THEN 'Deduction' 
                                                WHEN HeadType = 'E' THEN 'Earning'  ELSE '' END, 
								CR.[Name] AS EntryCurrency, 
								--Amount = CASE WHEN SLID.EntryAmount IS NULL AND SG.IsFixed = 1 THEN SG.FixedValue
								--				ELSE SLID.EntryAmount END 
                                Amount = SLID.EntryAmount, SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID, SH.[Sequence] SalaryHdSequence
                            FROM SalaryRuleGeneral SG 
		                            INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
		                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
		                            LEFT JOIN scs.Currency CR ON CRC.AmtEntryCurrency = CR.Id
                                    LEFT JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
                                    LEFT JOIN (
                                                SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													  SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													  SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												FROM SalaryInfoDefineMaster SDM
																	INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                          AND SDM.EffectiveDate IN (
                                                                                    SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' 
                                                                                            AND EffectiveDate <= '" + sEffectiveDate + @"'
                                                                                    )
                                             ) SLID ON SG.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID 
														AND SG.SalaryHeadID = SLID.SalaryHeadID	
                            WHERE --(SG.IsOpen = 1 OR SG.IsFixed = 1)  
                                  SG.IsOpen = 1 AND SG.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"'     
                                  AND SM.PlantID = '" + sPlantID + @"'
                            
							GROUP BY SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription, SH.[Sequence],  
                                     HeadType, CR.[Name], SLID.EntryAmount, SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID
                            --ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                            ORDER BY SH.[Sequence]";

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
        public void SalaryOpenHeadOnGrid(string sPlantID, string sSlrRuleMstSystemID, string sEmpSystemID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription,  
                                HeadType = CASE WHEN HeadType = 'D' THEN 'Deduction' 
                                                WHEN HeadType = 'E' THEN 'Earning'  ELSE '' END, 
								CR.[Name] AS EntryCurrency, 
								----Amount = CASE WHEN SLID.EntryAmount IS NULL AND SG.IsFixed = 1 THEN SG.FixedValue
								----				ELSE SLID.EntryAmount END 
                                --Amount = CASE WHEN ISNULL(SLID.EmpInfoSystemID, '') != '' THEN CAST(SLID.EntryAmount AS DECIMAL(7, 2))
								--			  ELSE CAST(SG.FixedValue AS DECIMAL(7, 2)) END, 
                                Amount = CASE WHEN ISNULL(SLID.EmpInfoSystemID, '') != '' THEN CONVERT(DECIMAL(10, 2), SLID.EntryAmount)
											  ELSE CONVERT(DECIMAL(10, 2), SG.FixedValue) END, 
								SH.HeadCategory, CONVERT(DATE, SLID.EffectiveDate) EffectiveDate, SLID.SalaryID, SH.[Sequence] SalaryHdSequence 
                            FROM SalaryRuleGeneral SG 
                                    INNER JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
		                            INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
		                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
		                            LEFT JOIN scs.Currency CR ON CRC.AmtEntryCurrency = CR.Id
                                    LEFT JOIN (
                                                SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													  SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													  SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												FROM SalaryInfoDefineMaster SDM
																	INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                          AND SDM.EffectiveDate IN (
                                                                                    SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' 
                                                                                            ---Change By Prodipta Date: 25-Jan-2019
                                                                                            ---Salary Not Show After Save
                                                                                            AND EffectiveDate = '" + sEffectiveDate + @"'
                                                                                            ---AND EffectiveDate <= '" + sEffectiveDate + @"'
                                                                                    )
                                             ) SLID ON SG.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID 
														AND SG.SalaryHeadID = SLID.SalaryHeadID	
                            WHERE --(SG.IsOpen = 1 OR SG.IsFixed = 1)  
                                  SG.IsOpen = 1 AND SG.IsFormula != 1 AND SG.IsOpen = 1 AND SG.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"'     
                                  AND SM.PlantID = '" + sPlantID + @"' AND ISNULL(SH.HeadCategory, '') != 'Tax'
                            
							GROUP BY SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription, SG.FixedValue, 
                                     HeadType, CR.[Name], SLID.EntryAmount, SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID, 
									 SLID.EffectiveDate, SLID.EmpInfoSystemID, SH.[Sequence]
                            --ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                            ORDER BY SH.[Sequence]";

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

        public void xSalaryApprovedOpenHeadOnGrid(string sPlantID, string sSlrRuleMstSystemID, string sEmpSystemID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription,  
                                HeadType = CASE WHEN HeadType = 'D' THEN 'Deduction' 
                                                WHEN HeadType = 'E' THEN 'Earning'  ELSE '' END, 
								CR.[Name] AS EntryCurrency, 
								----Amount = CASE WHEN SLID.EntryAmount IS NULL AND SG.IsFixed = 1 THEN SG.FixedValue
								----				ELSE SLID.EntryAmount END 
                                --Amount = CASE WHEN ISNULL(SLID.EmpInfoSystemID, '') != '' THEN CAST(SLID.EntryAmount AS DECIMAL(7, 2))
								--			  ELSE CAST(SG.FixedValue AS DECIMAL(7, 2)) END, 
                                Amount = CASE WHEN ISNULL(SLID.EmpInfoSystemID, '') != '' THEN CONVERT(DECIMAL(10, 2), SLID.EntryAmount)
											  ELSE CONVERT(DECIMAL(10, 2), SG.FixedValue) END, 
								SH.HeadCategory, CONVERT(DATE, SLID.EffectiveDate) EffectiveDate, SLID.SalaryID, SH.[Sequence] SalaryHdSequence 
                            FROM SalaryRuleGeneral SG 
                                    INNER JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
		                            INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
		                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
		                            LEFT JOIN scs.Currency CR ON CRC.AmtEntryCurrency = CR.Id
                                    LEFT JOIN (
                                                SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													  SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													  SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												FROM SalaryInfoDefineMaster SDM
																	INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                          AND SDM.EffectiveDate IN (
                                                                                    SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' 
                                                                                            ---Change By Prodipta Date: 25-Jan-2019
                                                                                            ---Salary Not Show After Save
                                                                                            ---AND EffectiveDate = '" + sEffectiveDate + @"'
                                                                                            AND IsApproved=1 
                                                                                    )
                                             ) SLID ON SG.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID 
														AND SG.SalaryHeadID = SLID.SalaryHeadID	
                            WHERE --(SG.IsOpen = 1 OR SG.IsFixed = 1)  
                                  SG.IsOpen = 1 AND SG.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"'     
                                  AND SM.PlantID = '" + sPlantID + @"' AND ISNULL(SH.HeadCategory, '') != 'Tax'
                            
							GROUP BY SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription, SG.FixedValue, 
                                     HeadType, CR.[Name], SLID.EntryAmount, SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID, 
									 SLID.EffectiveDate, SLID.EmpInfoSystemID, SH.[Sequence]
                            --ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                            ORDER BY SH.[Sequence]";

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
        public void SalaryApprovedOpenHeadOnGrid(string sPlantID, string sSlrRuleMstSystemID, string sEmpSystemID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription,  
                                HeadType = CASE WHEN HeadType = 'D' THEN 'Deduction' 
                                                WHEN HeadType = 'E' THEN 'Earning'  ELSE '' END, 
								CR.[Name] AS EntryCurrency, 
								----Amount = CASE WHEN SLID.EntryAmount IS NULL AND SG.IsFixed = 1 THEN SG.FixedValue
								----				ELSE SLID.EntryAmount END 
                                --Amount = CASE WHEN ISNULL(SLID.EmpInfoSystemID, '') != '' THEN CAST(SLID.EntryAmount AS DECIMAL(7, 2))
								--			  ELSE CAST(SG.FixedValue AS DECIMAL(7, 2)) END, 
                                Amount = CASE WHEN ISNULL(SLID.EmpInfoSystemID, '') != '' THEN CONVERT(DECIMAL(10, 2), SLID.EntryAmount)
											  ELSE CONVERT(DECIMAL(10, 2), SG.FixedValue) END, 
								SH.HeadCategory, CONVERT(DATE, SLID.EffectiveDate) EffectiveDate, SLID.SalaryID, SH.[Sequence] SalaryHdSequence 
                            FROM SalaryRuleGeneral SG 
                                    INNER JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
		                            INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
		                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
		                            LEFT JOIN scs.Currency CR ON CRC.AmtEntryCurrency = CR.Id
                                    LEFT JOIN (



SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													  SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													  SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												FROM SalaryInfoDefineMaster SDM
																	INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'  AND IsApproved=1 AND SDM.EffectiveDate='" + sEffectiveDate + @"'


union

SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													  SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													  SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												FROM SalaryInfoBackMaster SDM
																	INNER JOIN SalaryInfoback SD ON SDM.SystemID = SD.SalaryID
												WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'  AND IsApproved=1    AND SDM.EffectiveDate='" + sEffectiveDate + @"'










                                             ) SLID ON SG.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID 
														AND SG.SalaryHeadID = SLID.SalaryHeadID	
                            WHERE --(SG.IsOpen = 1 OR SG.IsFixed = 1)  
                                  SG.IsOpen = 1 AND SG.IsFormula != 1 AND SG.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"'     
                                  AND SM.PlantID = '" + sPlantID + @"' AND ISNULL(SH.HeadCategory, '') != 'Tax'
                            
							GROUP BY SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription, SG.FixedValue, 
                                     HeadType, CR.[Name], SLID.EntryAmount, SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID, 
									 SLID.EffectiveDate, SLID.EmpInfoSystemID, SH.[Sequence]
                            --ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                            ORDER BY SH.[Sequence]";

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
        public void SalaryOpenHeadOnGridWithPrev(string sPlantID, string sEmpSystemID, string SalaryId,string SalaryRuleMasterSystemID, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription,  
                                HeadType = CASE WHEN HeadType = 'D' THEN 'Deduction' 
                                                WHEN HeadType = 'E' THEN 'Earning'  ELSE '' END, 
								CR.[Name] AS EntryCurrency, 
								--Amount = CASE WHEN SLID.EntryAmount IS NULL AND SG.IsFixed = 1 THEN SG.FixedValue
								--				ELSE SLID.EntryAmount END 
                                Amount = SLID.EntryAmount,x.EntryAmount LastAmount, SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID,
                                SH.[Sequence] SalaryHdSequence 
                            FROM SalaryRuleGeneral SG 
		                            INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
		                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
		                            LEFT JOIN scs.Currency CR ON CRC.AmtEntryCurrency = CR.Id
                                    LEFT JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
                                    LEFT JOIN (
                                                SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													  SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													  SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												FROM SalaryInfoDefineMaster SDM
																	INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'  and sdm.SystemID='" + SalaryId + @"'
                                                         
                                             ) SLID ON SG.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID 
														AND SG.SalaryHeadID = SLID.SalaryHeadID	

                                    LEFT outer JOIN (
                                                SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													  SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													  SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												FROM SalaryInfoDefineMaster SDM
																	INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'  and sdm.SystemID<>'"+SalaryId+@"'
                                                          AND SDM.EffectiveDate IN (
                                                                                    SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' 
                                                                                        and SystemID<>'" + SalaryId + @"'   
                                                                                    )
                                             ) x ON  SG.SalaryHeadID = x.SalaryHeadID	


                            WHERE   
                                  SG.IsOpen = 1       
                                  AND SM.PlantID = '" + sPlantID + @"'
                                   -- and isnull(SLID.SystemID,'')<>''
                                    AND SG.SalaryRuleMasterSystemID = '"+ SalaryRuleMasterSystemID + @"'  
                            
							
                            --ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                            ORDER BY SH.[Sequence]";

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
        public void GetSalaryID(string sPlantID, string SalaryRuleMasterSystemID, string EmpInfoSystemID, string EffectiveDate, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT * FROM (
                            SELECT [SystemID],[EmpInfoSystemID]      
                                  ,[SalaryRuleMasterSystemID]
                                  ,[EffectiveDate]
                                  ,[IsApproved] 
                            FROM SalaryInfoDefineMaster                          
                            where EmpInfoSystemID='" + EmpInfoSystemID + @"'
                                    and SalaryRuleMasterSystemID='" + SalaryRuleMasterSystemID + @"'
                                    and EffectiveDate='" + EffectiveDate + @"'
                                    and PlantID='" + sPlantID + @"'
                            union
                            SELECT [SystemID],[EmpInfoSystemID]      
                                  ,[SalaryRuleMasterSystemID]
                                  ,[EffectiveDate]
                                  ,[IsApproved] 
                            FROM SalaryInfoDefineMaster                          
                            where EmpInfoSystemID='" + EmpInfoSystemID + @"'
                                    and SalaryRuleMasterSystemID='" + SalaryRuleMasterSystemID + @"'
                                    and EffectiveDate='" + EffectiveDate + @"'
                                    and PlantID='" + sPlantID + @"') a ";

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
        public void LoadEmpSlrOpenHdDataOnGrid_DesignationWise(string sPlantID, string sSlrRuleMstSystemID, string DesignationSystemID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"

                                        SELECT SG.SalaryHeadID
	                                        , SH.SalaryHead
	                                        , SH.Description
	                                        , SM.SalaryRuleDescription
	                                        , HeadType = CASE 
		                                        WHEN HeadType = 'D'
			                                        THEN 'Deduction'
		                                        WHEN HeadType = 'E'
			                                        THEN 'Earning'
		                                        ELSE ''
		                                        END
	                                        , CR.[Description] AS EntryCurrency
	                                        ,
	                                        --Amount = CASE WHEN SLID.EntryAmount IS NULL AND SG.IsFixed = 1 THEN SG.FixedValue
	                                        --				ELSE SLID.EntryAmount END 
	                                        Amount = SLID.EntryAmount
	                                        , SH.HeadCategory
	                                        , SLID.EffectiveDate
	                                        , SLID.SalaryID, SH.[Sequence] SalaryHdSequence

                                        FROM SalaryRuleGeneral SG

                                        INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID

                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
	                                        AND SG.SalaryHeadID = CRC.SalaryHeadID

                                        LEFT JOIN scs.Currency CR ON CRC.AmtEntryCurrency = CR.Id

                                        LEFT JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID

                                        LEFT JOIN (
	                                        SELECT SD.SystemID
		                                        , SD.SalaryID
		                                        , SDM.DesignationSystemID
		                                        , SDM.EffectiveDate
		                                        , SDM.SalaryIncrementSystemID
		                                        , SDM.SalaryRuleMasterSystemID
		                                        , SDM.GroupID
		                                        , SDM.PlantID
		                                        , SDM.IsApproved
		                                        , SDM.ApprovedBy
		                                        , SDM.DateApproved
		                                        , SD.SalaryHeadID
		                                        , SD.EntryCurrencyID
		                                        , SD.EntryAmount
		                                        , SD.DefineCurrencyID
		                                        , SD.DefineAmount
		                                        , SD.AmtDefinitionCurrencyID
		                                        , SD.AmtDefinitionRate
	
	                                        FROM SalaryInfoDefineMasterDesignationWise SDM
	
	                                        INNER JOIN SalaryInfoDefineDesignationWise SD ON SDM.SystemID = SD.SalaryID
	
	                                        WHERE SDM.DesignationSystemID = '" + DesignationSystemID + @"'
		                                        AND SDM.EffectiveDate IN (
			                                        SELECT MAX(EffectiveDate) EffectiveDate
			
			                                        FROM SalaryInfoDefineMasterDesignationWise
			
			                                        WHERE DesignationSystemID = '" + DesignationSystemID + @"'
				                                        AND EffectiveDate <= '" + sEffectiveDate + @"'
			                                        )
	                                        ) SLID ON SG.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID
	                                        AND SG.SalaryHeadID = SLID.SalaryHeadID

                                        WHERE --(SG.IsOpen = 1 OR SG.IsFixed = 1)  
	                                        SG.IsOpen = 1
	                                        AND SG.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"'
	                                        AND SM.PlantID = '" + sPlantID + @"'

                                        GROUP BY SG.SalaryHeadID
	                                        , SH.SalaryHead
	                                        , SH.Description
	                                        , SM.SalaryRuleDescription
	                                        , HeadType
	                                        , CR.[Description]
	                                        , SLID.EntryAmount
	                                        , SH.HeadCategory
	                                        , SLID.EffectiveDate
	                                        , SLID.SalaryID, SH.[Sequence]

                                        --ORDER BY SH.HeadType DESC
	                                    --    , SH.SalaryHead ASC
                                        ORDER BY SH.[Sequence] ";

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

        public void LoadUnApprovedEmpSlrOpenHdDataOnGrid(string sPlantID, string sEmpSystemID, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription,  
                                HeadType = CASE WHEN HeadType = 'D' THEN 'Deduction' 
                                                WHEN HeadType = 'E' THEN 'Earning'  ELSE '' END, 
								CR.Description AS EntryCurrency, SLID.SalaryRuleMasterSystemID,
                                Amount = ISNULL(SLID.EntryAmount, '0'), SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID,
                                SH.[Sequence] SalaryHdSequence
                            FROM SalaryRuleGeneral SG 
		                            INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
		                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
		                            LEFT JOIN SCS.Currency CR ON CRC.AmtEntryCurrency = CR.Code
                                    LEFT JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
                                    INNER JOIN (
                                                SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													    SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													    SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												FROM SalaryInfoDefineMaster SDM
																	INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                            AND SDM.EffectiveDate IN (
                                                                                    SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                    WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND IsApproved = 0
                                                                                    )
                                                
                                              ) SLID ON SG.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID 
														AND SG.SalaryHeadID = SLID.SalaryHeadID	
                            WHERE SG.IsOpen = 1 AND SM.PlantID = '" + sPlantID + @"'
							GROUP BY SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription, SLID.SalaryRuleMasterSystemID, 
                                     HeadType, CR.Description, SLID.EntryAmount, SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID, SH.[Sequence]
                            --ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                            ORDER BY SH.[Sequence]";

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
        public void LoadApprovedEmpSlrOpenHdDataOnGrid(string sPlantID, string sEmpSystemID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription,  
                                HeadType = CASE WHEN HeadType = 'D' THEN 'Deduction' 
                                                WHEN HeadType = 'E' THEN 'Earning'  ELSE '' END, 
								CR.Description AS EntryCurrency, SLID.SalaryRuleMasterSystemID,
                                Amount = ISNULL(SLID.EntryAmount, '0'), SH.HeadCategory, SLID.EffectiveDate, 
                                SLID.SalaryID, SH.[Sequence] SalaryHdSequence
                            FROM SalaryRuleGeneral SG 
		                            INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
		                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
		                            LEFT JOIN SCS.Currency CR ON CRC.AmtEntryCurrency = CR.Code
                                    LEFT JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
                                    INNER JOIN (
                                                SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													    SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													    SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												FROM SalaryInfoDefineMaster SDM
																	INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                            AND SDM.EffectiveDate IN (
                                                                                      SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                        WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND IsApproved = 1 
                                                                                                AND EffectiveDate <= '" + sEffectiveDate + @"'
                                                                                     )
                                                ) SLID ON SG.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID 
														AND SG.SalaryHeadID = SLID.SalaryHeadID	
                            WHERE SG.IsOpen = 1 AND SM.PlantID = '" + sPlantID + @"'
							GROUP BY SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription, SLID.SalaryRuleMasterSystemID, 
                                     HeadType, CR.Description, SLID.EntryAmount, SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID, SH.[Sequence]
                            --ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                            ORDER BY SH.[Sequence]";

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
        public void LoadEmpSlrOpenHdDataForIncrementOnGrid(string sPlantID, string sSlrRuleMstSystemID, string sEmpSystemID, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription,  
                                HeadType = CASE WHEN HeadType = 'D' THEN 'Deduction' 
                                                WHEN HeadType = 'E' THEN 'Earning'  ELSE '' END, 
								CR.Code AS EntryCurrency, Amount = ISNULL(SLID.EntryAmount, '0'), '0' IncrementValue, 
                                '0' AfterIncrement, SH.HeadCategory, SLID.SalaryID, SH.[Sequence] SalaryHdSequence
                            FROM SalaryRuleGeneral SG 
		                            INNER JOIN SalaryRuleMaster SM ON SG.SalaryRuleMasterSystemID = SM.SystemID
		                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SG.SalaryHeadID = CRC.SalaryHeadID
		                            LEFT JOIN scs.Currency CR ON CRC.AmtEntryCurrency = CR.Id
                                    LEFT JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID
                                    LEFT JOIN (
                                               SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													  SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													  SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
											   FROM SalaryInfoDefineMaster SDM
																	INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
											   WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                            AND SDM.EffectiveDate IN (
                                                                                      SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                        WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND IsApproved = 1
                                                                                     )
                                              ) SLID ON SG.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID 
														AND SG.SalaryHeadID = SLID.SalaryHeadID	
                            WHERE --(SG.IsOpen = 1 OR SG.IsFixed = 1)  
                                  SG.IsOpen = 1 AND SG.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"'     
                                  AND SM.PlantID = '" + sPlantID + @"'
							GROUP BY SG.SalaryHeadID, SH.SalaryHead, SH.Description, SM.SalaryRuleDescription, SH.[Sequence],  
                                     HeadType, CR.Code, SLID.EntryAmount, SH.HeadCategory, SLID.EffectiveDate, SLID.SalaryID
                            --ORDER BY SH.HeadType DESC, SH.SalaryHead ASC
                            ORDER BY SH.[Sequence]";

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
        public void LoadEmpApprovedSalaryInfoDefineDataOnGrid(string sPlantID, string sEmpSystemID, string sSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, 
                                    A.SalaryRuleMasterSystemID, A.SalaryRuleName, A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, 
                                    A.SalaryHeadID, A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                                  WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                    A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, 
                                    A.FormulaDes, A.FormulaDesID, A.FixedValue, A.IsOpen, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, 
                                    A.SequenceNo, REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-') EffectiveDate, SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                    IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                      ELSE SLID.IsApproved END, SLID.SalaryID, A.SM
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, 
                                        ECR.code AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.code AS DefinitionCurrency,
                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.code AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,
                                        SequenceNo = (ISNULL(Fml.SequenceNo, 0) + ISNULL(Fxd.SequenceNo, 0) + ISNULL(SG.SequenceNo, 0)), SH.HeadCategory,
                                        SH.[Sequence] SalaryHdSequence
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.id
                                        LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.id
                                        LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.id
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, '' FormulaDes, 
                                                    ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo 
                                                    FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)) Fml 
                                                        ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FixedValue, SequenceNo FROM SalaryRuleGeneral WHERE IsFixed = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FixedValue, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FixedValue, SequenceNo FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)) Fxd
                                                        ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND SG.IsOpen = 1
                                    ) A
                                        LEFT JOIN (
                                                   SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													      SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												    FROM SalaryInfoDefineMaster SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                              AND SDM.EffectiveDate IN (
                                                                                        SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                      WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND IsApproved = 1
                                                                                        )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE A.SequenceNo > 0 AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                            AND A.PlantID = '" + sPlantID + @"'
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
        //before IsNA is calculated
        public void LoadEmpSalaryInfoDefineDataOnGrid(string sPlantID, string sEmpSystemID, string sTaxYrStartDt, string sTaxYrEndDt, string sSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, 
                                  A.SalaryRuleMasterSystemID, A.SalaryRuleName, A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, A.TagAndUnTag,  
                                  A.SalaryHeadID, A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen,A.IsNA, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, A.SequenceNo, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), EndDate = CASE WHEN REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') = '" + sTaxYrEndDt + @"' THEN ''
																															     ELSE REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') END, 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END, HeadCategory, A.SalaryHdSequence, SLID.SalaryID
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												     WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													 WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												ELSE Convert(bit, 'False') END,
										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, 
                                        --ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.CurrencyDesc AS DefinitionCurrency,
                                        ECR.[Name] AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency,

                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,isnull(SG.IsNA,0) IsNA,
                                        SequenceNo = (ISNULL(Fml.SequenceNo, 0) + ISNULL(Fxd.SequenceNo, 0) + ISNULL(SG.SequenceNo, 0)), SH.HeadCategory,
                                        SH.[Sequence] SalaryHdSequence
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
                                        LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
                                        LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes, 
                                                            ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo 
                                                    FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)) Fml 
                                                        ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)) Fxd
                                                        ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND (SG.IsOpen = 1 OR SG.IsNA=1)
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
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												    FROM SalaryInfoDefineMaster SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                              AND SDM.EffectiveDate IN (
                                                                                        SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                      WHERE EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                                                        )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE A.SequenceNo > 0 AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                            AND A.PlantID = '" + sPlantID + @"'
                    ORDER BY A.SequenceNo ASC";

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
        public void xSalaryStructureAPHeadOnGrid(string sPlantID, string sEmpSystemID, string sTaxYrStartDt, string sTaxYrEndDt, string EffectiveDate, string sSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, 
                                  SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, A.SalaryRuleMasterSystemID, A.SalaryRuleName,  
                                  A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, A.TagAndUnTag, A.SalaryHeadID,  
                                  A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen,A.IsNA, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, 
                                  --A.SequenceNo, 
                                  SequenceNo = CASE WHEN ISNULL(A.SequenceNo, 0) > 0 THEN ISNULL(A.SequenceNo, 0)
													ELSE SLID.SequenceNo END, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), 
                                  EndDate = CASE WHEN REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') = '" + sTaxYrEndDt + @"' THEN ''
												 ELSE REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') END, 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END,HeadCategory,
								  BaseOnNetPay, RefAbsentism
---, IsCTCComponent, IsGrossComponent
, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo, '' EarningCurrencyID,
								  '0' EarningAmount, IsGNRWhichEverLess, A.RoundOption, ISNULL(A.IntegerInDisb, 0) IntegerInDisb, ISNULL(A.IsDecimalInDisb, 0) IsDecimalInDisb, 
                                  ISNULL(A.DecimalNo, 0) DecimalNo, SLID.SalaryCategory, A.SalaryHdSequence, SLID.SalaryID 
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												     WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													 WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												ELSE Convert(bit, 'False') END,
										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                        ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                        --ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.CurrencyDesc AS DefinitionCurrency,
                                        ECR.[Name] AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency,

                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,isnull(SG.IsNA,0) IsNA,
                                        SequenceNo = CASE WHEN ISNULL(Fml.SequenceNo, 0) > 0 THEN ISNULL(Fml.SequenceNo, 0)
														  WHEN ISNULL(Fxd.SequenceNo, 0) > 0 THEN ISNULL(Fxd.SequenceNo, 0)
														  WHEN ISNULL(SG.SequenceNo, 0) > 0 THEN ISNULL(SG.SequenceNo, 0)
														  ELSE 0 END,
										SH.HeadCategory,
										BaseOnNetPay = CASE WHEN ISNULL(Fml.BaseOnNetPay, '') != '' THEN Fml.BaseOnNetPay
															WHEN ISNULL(Fxd.BaseOnNetPay, '') != '' THEN Fxd.BaseOnNetPay
															WHEN ISNULL(SG.BaseOnNetPay, '') != '' THEN SG.BaseOnNetPay
															ELSE Convert(bit, 'False') END, 
										RefAbsentism = CASE WHEN ISNULL(Fml.RefAbsentism, '') != '' THEN Fml.RefAbsentism
															WHEN ISNULL(Fxd.RefAbsentism, '') != '' THEN Fxd.RefAbsentism
															WHEN ISNULL(SG.RefAbsentism, '') != '' THEN SG.RefAbsentism
															ELSE Convert(bit, 'False') END, 
										---SH.IsCTCComponent, SH.IsGrossComponent, 
										IsGNRBaseOthSlrHD = CASE WHEN ISNULL(Fml.IsGNRBaseOthSlrHD, '') != '' THEN Fml.IsGNRBaseOthSlrHD
															WHEN ISNULL(Fxd.IsGNRBaseOthSlrHD, '') != '' THEN Fxd.IsGNRBaseOthSlrHD
															WHEN ISNULL(SG.IsGNRBaseOthSlrHD, '') != '' THEN SG.IsGNRBaseOthSlrHD
															ELSE Convert(bit, 'False') END, 
										GNRBaseOthSlrHDFormula = ISNULL(Fml.GNRBaseOthSlrHDFormula, '') + ISNULL(Fxd.GNRBaseOthSlrHDFormula, '') + ISNULL(SG.GNRBaseOthSlrHDFormula, ''), 
										GNRApplicableMonthNo = ISNULL(Fml.GNRApplicableMonthNo, '') + ISNULL(Fxd.GNRApplicableMonthNo, '') + ISNULL(SG.GNRApplicableMonthNo, ''),
                                        IsGNRWhichEverLess = CASE WHEN ISNULL(Fml.IsGNRWhichEverLess, '') != '' THEN Fml.IsGNRWhichEverLess
															WHEN ISNULL(Fxd.IsGNRWhichEverLess, '') != '' THEN Fxd.IsGNRWhichEverLess
															WHEN ISNULL(SG.IsGNRWhichEverLess, '') != '' THEN SG.IsGNRWhichEverLess
															ELSE Convert(bit, 'False') END, SH.[Sequence] SalaryHdSequence 
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
										LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
										LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, IsGNRWhichEverLess,
														   GNRApplicableMonthNo 
														FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo  
														FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes, 
                                                            ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)
												  ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, 
														   IsGNRWhichEverLess, GNRApplicableMonthNo  
														FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)
												  ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND (SG.IsOpen = 1 OR SG.IsNA=1)
                                  ) A
                                        LEFT JOIN (
                                                   SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, 
--EffectiveDate = CASE WHEN ISNULL(SDED.EffectiveDate,'') != '' AND SDED.EffectiveDate <= '" + sTaxYrStartDt + @"' THEN '" + sTaxYrStartDt + @"'
																															  -- WHEN ISNULL(SDED.EffectiveDate,'') = '' AND SDM.EffectiveDate < '" + sTaxYrStartDt + @"' THEN '" + sTaxYrStartDt + @"'
																															  --- WHEN ISNULL(SDED.EffectiveDate,'') = '' THEN REPLACE(CONVERT(VARCHAR(11), SDM.EffectiveDate, 106),' ','-')
																															 -- ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EffectiveDate, 106),' ','-') END,
																										-- EndDate = CASE WHEN ISNULL(SDED.EndDate,'') = '' THEN '" + sTaxYrEndDt + @"'
																															--  ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EndDate, 106),' ','-') END,







EffectiveDate = CASE 
			
			WHEN ISNULL(SDED.EffectiveDate, '') = ''
				THEN REPLACE(CONVERT(VARCHAR(11), SDM.EffectiveDate, 106), ' ', '-')
			ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EffectiveDate, 106), ' ', '-')
			END
		,EndDate = REPLACE(CONVERT(VARCHAR(11), SDED.EndDate, 106), ' ', '-'),
														  SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													      SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate, SD.SequenceNo, SD.SalaryCategory
												    FROM (SELECT * FROM SalaryInfoDefineMaster WHERE EffectiveDate='" + EffectiveDate + @"' AND EmpInfoSystemID = '" + sEmpSystemID + @"') SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                              --AND SDM.EffectiveDate IN (
                                                                                        --SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                    --  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                                                       -- )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE (A.SequenceNo > 0 OR SLID.SequenceNo > 0) --A.SequenceNo > 0 
                          AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                          AND A.PlantID = '" + sPlantID + @"'
                    ORDER BY A.SequenceNo ASC --, A.HeadType DESC, A.SalaryHead";

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
        public void SalaryStructureAPHeadOnGrid(string sPlantID, string sEmpSystemID, string sTaxYrStartDt, string sTaxYrEndDt, string EffectiveDate, string sSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, 
                                  SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, A.SalaryRuleMasterSystemID, A.SalaryRuleName,  
                                  A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, --A.TagAndUnTag, 
                                                                                    A.SalaryHeadID,  
                                  A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen,A.IsFormula,A.IsNA, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, 
                                  --A.SequenceNo, 
                                  SequenceNo = CASE WHEN ISNULL(A.SequenceNo, 0) > 0 THEN ISNULL(A.SequenceNo, 0)
													ELSE SLID.SequenceNo END, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), 
                                  EndDate = CASE WHEN REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') = '" + sTaxYrEndDt + @"' THEN ''
												 ELSE REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') END, 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END,HeadCategory,
								  BaseOnNetPay, RefAbsentism
--, IsCTCComponent
--, IsGrossComponent
, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo, '' EarningCurrencyID,
								  '0' EarningAmount, IsGNRWhichEverLess, A.RoundOption, ISNULL(A.IntegerInDisb, 0) IntegerInDisb, ISNULL(A.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(A.IsSlabBased, 0) IsSlabBased, 
                                  ISNULL(A.DecimalNo, 0) DecimalNo, SLID.SalaryCategory, A.SalaryHdSequence, SLID.SalaryID 
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID,  sg.IsSlabBased,
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, --TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												    -- WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													-- WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												--ELSE Convert(bit, 'False') END,


                                                                                                                    IsFormula = CASE WHEN isnull(fml.SalaryHeadID,'') <> '' THEN Convert(bit, 'true')
                                                                                                                    WHEN isnull(fxd.SalaryHeadID,'') <> '' THEN Convert(bit, 'False')
                                                                                                                    WHEN isnull(sg.IsFormula,'') = '1' THEN Convert(bit, 'true')	
                                                                                                                    ELSE Convert(bit, 'False') END,



										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                        ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                        --ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.CurrencyDesc AS DefinitionCurrency,
                                        ECR.[Name] AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency,

                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,isnull(SG.IsNA,0) IsNA,---ISNULL(SG.IsFormula, 0) IsFormula,
                                        SequenceNo = CASE WHEN ISNULL(Fml.SequenceNo, 0) > 0 THEN ISNULL(Fml.SequenceNo, 0)
														  WHEN ISNULL(Fxd.SequenceNo, 0) > 0 THEN ISNULL(Fxd.SequenceNo, 0)
														  WHEN ISNULL(SG.SequenceNo, 0) > 0 THEN ISNULL(SG.SequenceNo, 0)
														  ELSE SH.[Sequence] END,
										SH.HeadCategory,
										BaseOnNetPay = CASE WHEN ISNULL(Fml.BaseOnNetPay, '') != '' THEN Fml.BaseOnNetPay
															WHEN ISNULL(Fxd.BaseOnNetPay, '') != '' THEN Fxd.BaseOnNetPay
															WHEN ISNULL(SG.BaseOnNetPay, '') != '' THEN SG.BaseOnNetPay
															ELSE Convert(bit, 'False') END, 
										RefAbsentism = CASE WHEN ISNULL(Fml.RefAbsentism, '') != '' THEN Fml.RefAbsentism
															WHEN ISNULL(Fxd.RefAbsentism, '') != '' THEN Fxd.RefAbsentism
															WHEN ISNULL(SG.RefAbsentism, '') != '' THEN SG.RefAbsentism
															ELSE Convert(bit, 'False') END, 
										---SH.IsCTCComponent, SH.IsGrossComponent, 
										IsGNRBaseOthSlrHD = CASE WHEN ISNULL(Fml.IsGNRBaseOthSlrHD, '') != '' THEN Fml.IsGNRBaseOthSlrHD
															WHEN ISNULL(Fxd.IsGNRBaseOthSlrHD, '') != '' THEN Fxd.IsGNRBaseOthSlrHD
															WHEN ISNULL(SG.IsGNRBaseOthSlrHD, '') != '' THEN SG.IsGNRBaseOthSlrHD
															ELSE Convert(bit, 'False') END, 
										GNRBaseOthSlrHDFormula = ISNULL(Fml.GNRBaseOthSlrHDFormula, '') + ISNULL(Fxd.GNRBaseOthSlrHDFormula, '') + ISNULL(SG.GNRBaseOthSlrHDFormula, ''), 
										GNRApplicableMonthNo = ISNULL(Fml.GNRApplicableMonthNo, '') + ISNULL(Fxd.GNRApplicableMonthNo, '') + ISNULL(SG.GNRApplicableMonthNo, ''),
                                        IsGNRWhichEverLess = CASE WHEN ISNULL(Fml.IsGNRWhichEverLess, '') != '' THEN Fml.IsGNRWhichEverLess
															WHEN ISNULL(Fxd.IsGNRWhichEverLess, '') != '' THEN Fxd.IsGNRWhichEverLess
															WHEN ISNULL(SG.IsGNRWhichEverLess, '') != '' THEN SG.IsGNRWhichEverLess
															ELSE Convert(bit, 'False') END, SH.[Sequence] SalaryHdSequence 
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
										LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
										LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, ---IsGNRTagAndUnTag TagAndUnTag, 
FormulaDes, FormulaDesID, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, IsGNRWhichEverLess,
														   GNRApplicableMonthNo 
														FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, ---IsAbsTagAndUnTag TagAndUnTag, 
                                                            FormulaDes, FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo  
														FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID,--- IsDSPTagAndUnTag TagAndUnTag,
'' FormulaDes, 
                                                            ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)
												  ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, ---IsGNRTagAndUnTag TagAndUnTag, 
FixedValue, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, 
														   IsGNRWhichEverLess, GNRApplicableMonthNo  
														FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID,--- IsAbsTagAndUnTag TagAndUnTag,
FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, ---IsDSPTagAndUnTag TagAndUnTag, 
FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)
												  ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID --AND (SG.IsOpen = 1 OR SG.IsNA=1)
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
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate, SD.SequenceNo, SD.SalaryCategory
												    FROM (SELECT * FROM SalaryInfoDefineMaster WHERE EffectiveDate='" + EffectiveDate + @"' AND EmpInfoSystemID = '" + sEmpSystemID + @"') SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                              --AND SDM.EffectiveDate IN (
                                                                                        --SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                    --  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                                                       -- )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE (A.SequenceNo > 0 OR SLID.SequenceNo > 0) --A.SequenceNo > 0 
                          AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                          AND A.PlantID = '" + sPlantID + @"'
                    ORDER BY A.SequenceNo ASC --, A.HeadType DESC, A.SalaryHead";

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
        public void xxxxxSalaryStructureAPHeadOnGrid(string sPlantID, string sEmpSystemID, string sTaxYrStartDt, string sTaxYrEndDt, string EffectiveDate, string sSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, 
                                  SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, A.SalaryRuleMasterSystemID, A.SalaryRuleName,  
                                  A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, --A.TagAndUnTag, 
                                                                                    A.SalaryHeadID,  
                                  A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen,A.IsFormula,A.IsNA, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, 
                                  --A.SequenceNo, 
                                  SequenceNo = CASE WHEN ISNULL(A.SequenceNo, 0) > 0 THEN ISNULL(A.SequenceNo, 0)
													ELSE SLID.SequenceNo END, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), 
                                  EndDate = CASE WHEN REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') = '" + sTaxYrEndDt + @"' THEN ''
												 ELSE REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') END, 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END,HeadCategory,
								  BaseOnNetPay, RefAbsentism
--, IsCTCComponent
--, IsGrossComponent
, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo, '' EarningCurrencyID,
								  '0' EarningAmount, IsGNRWhichEverLess, A.RoundOption, ISNULL(A.IntegerInDisb, 0) IntegerInDisb, ISNULL(A.IsDecimalInDisb, 0) IsDecimalInDisb, 
                                  ISNULL(A.DecimalNo, 0) DecimalNo, SLID.SalaryCategory, A.SalaryHdSequence, SLID.SalaryID 
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, --TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												    -- WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													-- WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												--ELSE Convert(bit, 'False') END,
										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                        ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                        --ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.CurrencyDesc AS DefinitionCurrency,
                                        ECR.[Name] AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency,

                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,isnull(SG.IsNA,0) IsNA,ISNULL(SG.IsFormula, 0) IsFormula,
                                        SequenceNo = CASE WHEN ISNULL(Fml.SequenceNo, 0) > 0 THEN ISNULL(Fml.SequenceNo, 0)
														  WHEN ISNULL(Fxd.SequenceNo, 0) > 0 THEN ISNULL(Fxd.SequenceNo, 0)
														  WHEN ISNULL(SG.SequenceNo, 0) > 0 THEN ISNULL(SG.SequenceNo, 0)
														  ELSE 0 END,
										SH.HeadCategory,
										BaseOnNetPay = CASE WHEN ISNULL(Fml.BaseOnNetPay, '') != '' THEN Fml.BaseOnNetPay
															WHEN ISNULL(Fxd.BaseOnNetPay, '') != '' THEN Fxd.BaseOnNetPay
															WHEN ISNULL(SG.BaseOnNetPay, '') != '' THEN SG.BaseOnNetPay
															ELSE Convert(bit, 'False') END, 
										RefAbsentism = CASE WHEN ISNULL(Fml.RefAbsentism, '') != '' THEN Fml.RefAbsentism
															WHEN ISNULL(Fxd.RefAbsentism, '') != '' THEN Fxd.RefAbsentism
															WHEN ISNULL(SG.RefAbsentism, '') != '' THEN SG.RefAbsentism
															ELSE Convert(bit, 'False') END, 
										---SH.IsCTCComponent, SH.IsGrossComponent, 
										IsGNRBaseOthSlrHD = CASE WHEN ISNULL(Fml.IsGNRBaseOthSlrHD, '') != '' THEN Fml.IsGNRBaseOthSlrHD
															WHEN ISNULL(Fxd.IsGNRBaseOthSlrHD, '') != '' THEN Fxd.IsGNRBaseOthSlrHD
															WHEN ISNULL(SG.IsGNRBaseOthSlrHD, '') != '' THEN SG.IsGNRBaseOthSlrHD
															ELSE Convert(bit, 'False') END, 
										GNRBaseOthSlrHDFormula = ISNULL(Fml.GNRBaseOthSlrHDFormula, '') + ISNULL(Fxd.GNRBaseOthSlrHDFormula, '') + ISNULL(SG.GNRBaseOthSlrHDFormula, ''), 
										GNRApplicableMonthNo = ISNULL(Fml.GNRApplicableMonthNo, '') + ISNULL(Fxd.GNRApplicableMonthNo, '') + ISNULL(SG.GNRApplicableMonthNo, ''),
                                        IsGNRWhichEverLess = CASE WHEN ISNULL(Fml.IsGNRWhichEverLess, '') != '' THEN Fml.IsGNRWhichEverLess
															WHEN ISNULL(Fxd.IsGNRWhichEverLess, '') != '' THEN Fxd.IsGNRWhichEverLess
															WHEN ISNULL(SG.IsGNRWhichEverLess, '') != '' THEN SG.IsGNRWhichEverLess
															ELSE Convert(bit, 'False') END, SH.[Sequence] SalaryHdSequence 
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
										LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
										LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, ---IsGNRTagAndUnTag TagAndUnTag, 
FormulaDes, FormulaDesID, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, IsGNRWhichEverLess,
														   GNRApplicableMonthNo 
														FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, ---IsAbsTagAndUnTag TagAndUnTag, 
                                                            FormulaDes, FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo  
														FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID,--- IsDSPTagAndUnTag TagAndUnTag,
'' FormulaDes, 
                                                            ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)
												  ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, ---IsGNRTagAndUnTag TagAndUnTag, 
FixedValue, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, 
														   IsGNRWhichEverLess, GNRApplicableMonthNo  
														FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID,--- IsAbsTagAndUnTag TagAndUnTag,
FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, ---IsDSPTagAndUnTag TagAndUnTag, 
FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)
												  ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID --AND (SG.IsOpen = 1 OR SG.IsNA=1)
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
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate, SD.SequenceNo, SD.SalaryCategory
												    FROM (SELECT * FROM SalaryInfoDefineMaster WHERE EffectiveDate='" + EffectiveDate + @"' AND EmpInfoSystemID = '" + sEmpSystemID + @"') SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                              --AND SDM.EffectiveDate IN (
                                                                                        --SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                    --  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                                                       -- )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE (A.SequenceNo > 0 OR SLID.SequenceNo > 0) --A.SequenceNo > 0 
                          AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                          AND A.PlantID = '" + sPlantID + @"'
                    ORDER BY A.SequenceNo ASC --, A.HeadType DESC, A.SalaryHead";

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
        public void xxSalaryStructureAPHeadOnGrid(string sPlantID, string sEmpSystemID, string sTaxYrStartDt, string sTaxYrEndDt, string EffectiveDate, string sSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, 
                                  SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, A.SalaryRuleMasterSystemID, A.SalaryRuleName,  
                                  A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, A.TagAndUnTag, A.SalaryHeadID,  
                                  A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen,A.IsFormula,A.IsNA, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, 
                                  --A.SequenceNo, 
                                  SequenceNo = CASE WHEN ISNULL(A.SequenceNo, 0) > 0 THEN ISNULL(A.SequenceNo, 0)
													ELSE SLID.SequenceNo END, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), 
                                  EndDate = CASE WHEN REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') = '" + sTaxYrEndDt + @"' THEN ''
												 ELSE REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') END, 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END,HeadCategory,
								  BaseOnNetPay, RefAbsentism
--, IsCTCComponent
--, IsGrossComponent
, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo, '' EarningCurrencyID,
								  '0' EarningAmount, IsGNRWhichEverLess, A.RoundOption, ISNULL(A.IntegerInDisb, 0) IntegerInDisb, ISNULL(A.IsDecimalInDisb, 0) IsDecimalInDisb, 
                                  ISNULL(A.DecimalNo, 0) DecimalNo, SLID.SalaryCategory, A.SalaryHdSequence, SLID.SalaryID 
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												     WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													 WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												ELSE Convert(bit, 'False') END,
										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                        ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                        --ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.CurrencyDesc AS DefinitionCurrency,
                                        ECR.[Name] AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency,

                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,isnull(SG.IsNA,0) IsNA,ISNULL(SG.IsFormula, 0) IsFormula,
                                        SequenceNo = CASE WHEN ISNULL(Fml.SequenceNo, 0) > 0 THEN ISNULL(Fml.SequenceNo, 0)
														  WHEN ISNULL(Fxd.SequenceNo, 0) > 0 THEN ISNULL(Fxd.SequenceNo, 0)
														  WHEN ISNULL(SG.SequenceNo, 0) > 0 THEN ISNULL(SG.SequenceNo, 0)
														  ELSE 0 END,
										SH.HeadCategory,
										BaseOnNetPay = CASE WHEN ISNULL(Fml.BaseOnNetPay, '') != '' THEN Fml.BaseOnNetPay
															WHEN ISNULL(Fxd.BaseOnNetPay, '') != '' THEN Fxd.BaseOnNetPay
															WHEN ISNULL(SG.BaseOnNetPay, '') != '' THEN SG.BaseOnNetPay
															ELSE Convert(bit, 'False') END, 
										RefAbsentism = CASE WHEN ISNULL(Fml.RefAbsentism, '') != '' THEN Fml.RefAbsentism
															WHEN ISNULL(Fxd.RefAbsentism, '') != '' THEN Fxd.RefAbsentism
															WHEN ISNULL(SG.RefAbsentism, '') != '' THEN SG.RefAbsentism
															ELSE Convert(bit, 'False') END, 
										---SH.IsCTCComponent, SH.IsGrossComponent, 
										IsGNRBaseOthSlrHD = CASE WHEN ISNULL(Fml.IsGNRBaseOthSlrHD, '') != '' THEN Fml.IsGNRBaseOthSlrHD
															WHEN ISNULL(Fxd.IsGNRBaseOthSlrHD, '') != '' THEN Fxd.IsGNRBaseOthSlrHD
															WHEN ISNULL(SG.IsGNRBaseOthSlrHD, '') != '' THEN SG.IsGNRBaseOthSlrHD
															ELSE Convert(bit, 'False') END, 
										GNRBaseOthSlrHDFormula = ISNULL(Fml.GNRBaseOthSlrHDFormula, '') + ISNULL(Fxd.GNRBaseOthSlrHDFormula, '') + ISNULL(SG.GNRBaseOthSlrHDFormula, ''), 
										GNRApplicableMonthNo = ISNULL(Fml.GNRApplicableMonthNo, '') + ISNULL(Fxd.GNRApplicableMonthNo, '') + ISNULL(SG.GNRApplicableMonthNo, ''),
                                        IsGNRWhichEverLess = CASE WHEN ISNULL(Fml.IsGNRWhichEverLess, '') != '' THEN Fml.IsGNRWhichEverLess
															WHEN ISNULL(Fxd.IsGNRWhichEverLess, '') != '' THEN Fxd.IsGNRWhichEverLess
															WHEN ISNULL(SG.IsGNRWhichEverLess, '') != '' THEN SG.IsGNRWhichEverLess
															ELSE Convert(bit, 'False') END, SH.[Sequence] SalaryHdSequence 
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
										LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
										LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, IsGNRWhichEverLess,
														   GNRApplicableMonthNo 
														FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo  
														FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes, 
                                                            ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)
												  ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, 
														   IsGNRWhichEverLess, GNRApplicableMonthNo  
														FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)
												  ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID --AND (SG.IsOpen = 1 OR SG.IsNA=1)
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
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate, SD.SequenceNo, SD.SalaryCategory
												    FROM (SELECT * FROM SalaryInfoDefineMaster WHERE EffectiveDate='" + EffectiveDate + @"' AND EmpInfoSystemID = '" + sEmpSystemID + @"') SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                              --AND SDM.EffectiveDate IN (
                                                                                        --SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                    --  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                                                       -- )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE (A.SequenceNo > 0 OR SLID.SequenceNo > 0) --A.SequenceNo > 0 
                          AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                          AND A.PlantID = '" + sPlantID + @"'
                    ORDER BY A.SequenceNo ASC --, A.HeadType DESC, A.SalaryHead";

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
        public void SalaryStructureApprovedHeadOnGrid(string sPlantID, string sEmpSystemID, string sTaxYrStartDt, string sTaxYrEndDt, string EffectiveDate, string sSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, 
                                  SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, A.SalaryRuleMasterSystemID, A.SalaryRuleName,  
                                  A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, A.TagAndUnTag, A.SalaryHeadID,  
                                  A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen,A.IsNA, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, 
                                  --A.SequenceNo, 
                                  SequenceNo = CASE WHEN ISNULL(A.SequenceNo, 0) > 0 THEN ISNULL(A.SequenceNo, 0)
													ELSE SLID.SequenceNo END, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), 
                                  EndDate = CASE WHEN REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') = '" + sTaxYrEndDt + @"' THEN ''
												 ELSE REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') END, 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END,HeadCategory,
								  BaseOnNetPay, RefAbsentism --, IsCTCComponent, IsGrossComponent
                                        , IsGNRBaseOthSlrHD
                                    , GNRBaseOthSlrHDFormula, GNRApplicableMonthNo, '' EarningCurrencyID,
								  '0' EarningAmount, IsGNRWhichEverLess, A.RoundOption, ISNULL(A.IntegerInDisb, 0) IntegerInDisb, ISNULL(A.IsDecimalInDisb, 0) IsDecimalInDisb, 
                                  ISNULL(A.DecimalNo, 0) DecimalNo, SLID.SalaryCategory, A.SalaryHdSequence, SLID.SalaryID 
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												     WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													 WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												ELSE Convert(bit, 'False') END,
										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                        ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                        --ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.CurrencyDesc AS DefinitionCurrency,
                                        ECR.[Name] AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency,

                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,isnull(SG.IsNA,0) IsNA,
                                        SequenceNo = CASE WHEN ISNULL(Fml.SequenceNo, 0) > 0 THEN ISNULL(Fml.SequenceNo, 0)
														  WHEN ISNULL(Fxd.SequenceNo, 0) > 0 THEN ISNULL(Fxd.SequenceNo, 0)
														  WHEN ISNULL(SG.SequenceNo, 0) > 0 THEN ISNULL(SG.SequenceNo, 0)
														  ELSE 0 END,
										SH.HeadCategory,
										BaseOnNetPay = CASE WHEN ISNULL(Fml.BaseOnNetPay, '') != '' THEN Fml.BaseOnNetPay
															WHEN ISNULL(Fxd.BaseOnNetPay, '') != '' THEN Fxd.BaseOnNetPay
															WHEN ISNULL(SG.BaseOnNetPay, '') != '' THEN SG.BaseOnNetPay
															ELSE Convert(bit, 'False') END, 
										RefAbsentism = CASE WHEN ISNULL(Fml.RefAbsentism, '') != '' THEN Fml.RefAbsentism
															WHEN ISNULL(Fxd.RefAbsentism, '') != '' THEN Fxd.RefAbsentism
															WHEN ISNULL(SG.RefAbsentism, '') != '' THEN SG.RefAbsentism
															ELSE Convert(bit, 'False') END, 
										--SH.IsCTCComponent, SH.IsGrossComponent, 
										IsGNRBaseOthSlrHD = CASE WHEN ISNULL(Fml.IsGNRBaseOthSlrHD, '') != '' THEN Fml.IsGNRBaseOthSlrHD
															WHEN ISNULL(Fxd.IsGNRBaseOthSlrHD, '') != '' THEN Fxd.IsGNRBaseOthSlrHD
															WHEN ISNULL(SG.IsGNRBaseOthSlrHD, '') != '' THEN SG.IsGNRBaseOthSlrHD
															ELSE Convert(bit, 'False') END, 
										GNRBaseOthSlrHDFormula = ISNULL(Fml.GNRBaseOthSlrHDFormula, '') + ISNULL(Fxd.GNRBaseOthSlrHDFormula, '') + ISNULL(SG.GNRBaseOthSlrHDFormula, ''), 
										GNRApplicableMonthNo = ISNULL(Fml.GNRApplicableMonthNo, '') + ISNULL(Fxd.GNRApplicableMonthNo, '') + ISNULL(SG.GNRApplicableMonthNo, ''),
                                        IsGNRWhichEverLess = CASE WHEN ISNULL(Fml.IsGNRWhichEverLess, '') != '' THEN Fml.IsGNRWhichEverLess
															WHEN ISNULL(Fxd.IsGNRWhichEverLess, '') != '' THEN Fxd.IsGNRWhichEverLess
															WHEN ISNULL(SG.IsGNRWhichEverLess, '') != '' THEN SG.IsGNRWhichEverLess
															ELSE Convert(bit, 'False') END, SH.[Sequence] SalaryHdSequence 
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
										LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
										LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, IsGNRWhichEverLess,
														   GNRApplicableMonthNo 
														FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo  
														FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes, 
                                                            ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)
												  ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, 
														   IsGNRWhichEverLess, GNRApplicableMonthNo  
														FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)
												  ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND (SG.IsOpen = 1 OR SG.IsNA=1)
                                  ) A
                                        LEFT JOIN (
                                                   SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, 
--EffectiveDate = CASE WHEN ISNULL(SDED.EffectiveDate,'') != '' AND SDED.EffectiveDate <= '" + sTaxYrStartDt + @"' THEN '" + sTaxYrStartDt + @"'
																															  -- WHEN ISNULL(SDED.EffectiveDate,'') = '' AND SDM.EffectiveDate < '" + sTaxYrStartDt + @"' THEN '" + sTaxYrStartDt + @"'
																															  --- WHEN ISNULL(SDED.EffectiveDate,'') = '' THEN REPLACE(CONVERT(VARCHAR(11), SDM.EffectiveDate, 106),' ','-')
																															 -- ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EffectiveDate, 106),' ','-') END,
																										-- EndDate = CASE WHEN ISNULL(SDED.EndDate,'') = '' THEN '" + sTaxYrEndDt + @"'
																															--  ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EndDate, 106),' ','-') END,







EffectiveDate = CASE 
			
			WHEN ISNULL(SDED.EffectiveDate, '') = ''
				THEN REPLACE(CONVERT(VARCHAR(11), SDM.EffectiveDate, 106), ' ', '-')
			ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EffectiveDate, 106), ' ', '-')
			END
		,EndDate = REPLACE(CONVERT(VARCHAR(11), SDED.EndDate, 106), ' ', '-'),
														  SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													      SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate, SD.SequenceNo, SD.SalaryCategory
												    FROM (SELECT * FROM SalaryInfoDefineMaster WHERE IsApproved=1  AND EmpInfoSystemID = '" + sEmpSystemID + @"') SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                              --AND SDM.EffectiveDate IN (
                                                                                        --SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                    --  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                                                       -- )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE (A.SequenceNo > 0 OR SLID.SequenceNo > 0) --A.SequenceNo > 0 
                          AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                          AND A.PlantID = '" + sPlantID + @"'
                    ORDER BY A.SequenceNo ASC --, A.HeadType DESC, A.SalaryHead";

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
        public void SalaryStructureAPHeadOnGridAfterLoadGrid(string sSlrRuleMstSystemID, string SlrInfoDefSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Convert(bit, 'True'), SLRD.SystemID AS SlrInfoDefSystemID, SLRM.EmpInfoSystemID, SLRM.SalaryRuleMasterSystemID,
	                              SM.SalaryRuleName,  SM.SalaryRuleDescription, SM.CurrencyRuleSystemID, CRC.MstSystemID CurrencyRuleChildSystemID, 
	                              TagAndUnTag = Convert(bit, 'False'), SLRD.SalaryHeadID, SH.SalaryHead, HeadType = CASE WHEN SH.HeadType = 'D' THEN 'Deduction' 
																						                              WHEN SH.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo, 
                                  
	                              CRC.AmtEntryCurrency AS EntryCurrencyID,  ECR.[Name] AS EntryCurrency, SLRD.EntryAmount, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, 
	                              DECR.[Name] AS DefinitionCurrency, SLRD.DefineAmount, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, 
	                              '' FormulaDes, '' FormulaDesID, 0 FixedValue, Convert(bit, 'False') IsOpen, Convert(bit, 'False') IsNA,
                                  SequenceNo = 0, SH.HeadCategory, EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLRM.EffectiveDate, 106),' ','-'), 
                                  EndDate = CASE WHEN ISNULL(SDED.EndDate,'') = '' THEN ''
					                             ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EndDate, 106),' ','-') END, 
	                              MonthPeriod = ISNULL((DATEDIFF(m, SLRM.EffectiveDate, SDED.EndDate) + 1), 0), SLRD.AmtDefinitionCurrencyID, SLRD.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLRM.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                   ELSE SLRM.IsApproved END, SH.HeadCategory,
	                              --SLRD.BaseOnNetPay, SLRD.RefAbsentism, 
								  BaseOnNetPay = CASE WHEN ISNULL(Fml.BaseOnNetPay, '') != '' THEN Fml.BaseOnNetPay
													  WHEN ISNULL(Fxd.BaseOnNetPay, '') != '' THEN Fxd.BaseOnNetPay
													  WHEN ISNULL(SG.BaseOnNetPay, '') != '' THEN SG.BaseOnNetPay
													  ELSE Convert(bit, 'False') END, 
								  RefAbsentism = CASE WHEN ISNULL(Fml.RefAbsentism, '') != '' THEN Fml.RefAbsentism
													  WHEN ISNULL(Fxd.RefAbsentism, '') != '' THEN Fxd.RefAbsentism
													  WHEN ISNULL(SG.RefAbsentism, '') != '' THEN SG.RefAbsentism
													  ELSE Convert(bit, 'False') END
                            	 ---, SH.IsCTCComponent, SH.IsGrossComponent
                                , IsGNRBaseOthSlrHD = Convert(bit, 'False'), 
	                              --SLRD.GNRBaseOthSlrHDFormula, SLRD.GNRApplicableMonthNo, 
								  GNRBaseOthSlrHDFormula = ISNULL(Fml.GNRBaseOthSlrHDFormula, '') + ISNULL(Fxd.GNRBaseOthSlrHDFormula, '') + ISNULL(SG.GNRBaseOthSlrHDFormula, ''), 
								  GNRApplicableMonthNo = ISNULL(Fml.GNRApplicableMonthNo, '') + ISNULL(Fxd.GNRApplicableMonthNo, '') + ISNULL(SG.GNRApplicableMonthNo, ''),
								  '' EarningCurrencyID, '0' EarningAmount, SLRD.SequenceNo, SLRD.SalaryCategory,
                                  SH.[Sequence] SalaryHdSequence,ISNULL(SG.IsSlabBased, 0) IsSlabBased
	                        FROM SalaryInfoDefineMaster SLRM 
				                            INNER JOIN SalaryInfoDefine SLRD ON SLRM.SystemID = SLRD.SalaryID
				                            INNER JOIN SalaryRuleMaster SM ON SLRM.SalaryRuleMasterSystemID = SM.SystemID
				                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SLRD.SalaryHeadID = CRC.SalaryHeadID
                                            LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                            LEFT JOIN scs.Currency ECR ON SLRD.EntryCurrencyID = ECR.Id
				                            LEFT JOIN scs.Currency DECR ON SLRD.DefineCurrencyID = DECR.Id
				                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
											LEFT JOIN (
														SELECT SalaryRuleMasterSystemID, SalaryHeadID, --IsGNRTagAndUnTag TagAndUnTag, 
FormulaDes, FormulaDesID, SequenceNo,
															   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, IsGNRWhichEverLess,
															   GNRApplicableMonthNo 
															FROM SalaryRuleGeneral WHERE IsFormula = 1
														UNION
														(SELECT SalaryRuleMasterSystemID, SalaryHeadID, --IsAbsTagAndUnTag TagAndUnTag, 
FormulaDes, FormulaDesID, SequenceNo,
															   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
															   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo  
															FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
														UNION
														(SELECT SalaryRuleMasterSystemID, SalaryHeadID, --IsDSPTagAndUnTag TagAndUnTag,
'' FormulaDes, 
																('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo,
															   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
															   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
															FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)
													  ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
											LEFT JOIN (
														SELECT SalaryRuleMasterSystemID, SalaryHeadID, --IsGNRTagAndUnTag TagAndUnTag, 
FixedValue, SequenceNo,
															   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, 
															   IsGNRWhichEverLess, GNRApplicableMonthNo  
															FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
														UNION
														(SELECT SalaryRuleMasterSystemID, SalaryHeadID, --IsAbsTagAndUnTag TagAndUnTag, 
FixedValue, SequenceNo,
															   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
															   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
															FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
														UNION
														(SELECT SalaryRuleMasterSystemID, SalaryHeadID, --IsDSPTagAndUnTag TagAndUnTag, 
FixedValue, SequenceNo,
															   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
															   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
															FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)
													  ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
											LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND (SG.IsOpen = 1 OR SG.IsNA=1)
				                            LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SLRM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SLRD.SalaryHeadID
	                       WHERE SLRM.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND SLRD.SystemID NOT IN ('" + SlrInfoDefSystemID + @"')
	                       ORDER BY SH.HeadType DESC, SH.SalaryHead";

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
        public void xSalaryStructureAPHeadOnGridAfterLoadGrid(string sSlrRuleMstSystemID, string SlrInfoDefSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Convert(bit, 'True'), SLRD.SystemID AS SlrInfoDefSystemID, SLRM.EmpInfoSystemID, SLRM.SalaryRuleMasterSystemID,
	                              SM.SalaryRuleName,  SM.SalaryRuleDescription, SM.CurrencyRuleSystemID, CRC.MstSystemID CurrencyRuleChildSystemID, 
	                              TagAndUnTag = Convert(bit, 'False'), SLRD.SalaryHeadID, SH.SalaryHead, HeadType = CASE WHEN SH.HeadType = 'D' THEN 'Deduction' 
																						                              WHEN SH.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo, 
                                  
	                              CRC.AmtEntryCurrency AS EntryCurrencyID,  ECR.[Name] AS EntryCurrency, SLRD.EntryAmount, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, 
	                              DECR.[Name] AS DefinitionCurrency, SLRD.DefineAmount, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, 
	                              '' FormulaDes, '' FormulaDesID, 0 FixedValue, Convert(bit, 'False') IsOpen, Convert(bit, 'False') IsNA,
                                  SequenceNo = 0, SH.HeadCategory, EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLRM.EffectiveDate, 106),' ','-'), 
                                  EndDate = CASE WHEN ISNULL(SDED.EndDate,'') = '' THEN ''
					                             ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EndDate, 106),' ','-') END, 
	                              MonthPeriod = ISNULL((DATEDIFF(m, SLRM.EffectiveDate, SDED.EndDate) + 1), 0), SLRD.AmtDefinitionCurrencyID, SLRD.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLRM.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                   ELSE SLRM.IsApproved END, SH.HeadCategory,
	                              --SLRD.BaseOnNetPay, SLRD.RefAbsentism, 
								  BaseOnNetPay = CASE WHEN ISNULL(Fml.BaseOnNetPay, '') != '' THEN Fml.BaseOnNetPay
													  WHEN ISNULL(Fxd.BaseOnNetPay, '') != '' THEN Fxd.BaseOnNetPay
													  WHEN ISNULL(SG.BaseOnNetPay, '') != '' THEN SG.BaseOnNetPay
													  ELSE Convert(bit, 'False') END, 
								  RefAbsentism = CASE WHEN ISNULL(Fml.RefAbsentism, '') != '' THEN Fml.RefAbsentism
													  WHEN ISNULL(Fxd.RefAbsentism, '') != '' THEN Fxd.RefAbsentism
													  WHEN ISNULL(SG.RefAbsentism, '') != '' THEN SG.RefAbsentism
													  ELSE Convert(bit, 'False') END
                            	 ---, SH.IsCTCComponent, SH.IsGrossComponent
                                , IsGNRBaseOthSlrHD = Convert(bit, 'False'), 
	                              --SLRD.GNRBaseOthSlrHDFormula, SLRD.GNRApplicableMonthNo, 
								  GNRBaseOthSlrHDFormula = ISNULL(Fml.GNRBaseOthSlrHDFormula, '') + ISNULL(Fxd.GNRBaseOthSlrHDFormula, '') + ISNULL(SG.GNRBaseOthSlrHDFormula, ''), 
								  GNRApplicableMonthNo = ISNULL(Fml.GNRApplicableMonthNo, '') + ISNULL(Fxd.GNRApplicableMonthNo, '') + ISNULL(SG.GNRApplicableMonthNo, ''),
								  '' EarningCurrencyID, '0' EarningAmount, SLRD.SequenceNo, SLRD.SalaryCategory,
                                  SH.[Sequence] SalaryHdSequence
	                        FROM SalaryInfoDefineMaster SLRM 
				                            INNER JOIN SalaryInfoDefine SLRD ON SLRM.SystemID = SLRD.SalaryID
				                            INNER JOIN SalaryRuleMaster SM ON SLRM.SalaryRuleMasterSystemID = SM.SystemID
				                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID AND SLRD.SalaryHeadID = CRC.SalaryHeadID
                                            LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                            LEFT JOIN scs.Currency ECR ON SLRD.EntryCurrencyID = ECR.Id
				                            LEFT JOIN scs.Currency DECR ON SLRD.DefineCurrencyID = DECR.Id
				                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
											LEFT JOIN (
														SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
															   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, IsGNRWhichEverLess,
															   GNRApplicableMonthNo 
															FROM SalaryRuleGeneral WHERE IsFormula = 1
														UNION
														(SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
															   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
															   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo  
															FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
														UNION
														(SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes, 
																('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo,
															   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
															   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
															FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)
													  ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
											LEFT JOIN (
														SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
															   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, 
															   IsGNRWhichEverLess, GNRApplicableMonthNo  
															FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
														UNION
														(SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
															   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
															   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
															FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
														UNION
														(SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
															   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
															   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
															FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)
													  ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
											LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND (SG.IsOpen = 1 OR SG.IsNA=1)
				                            LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SLRM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SLRD.SalaryHeadID
	                       WHERE SLRM.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND SLRD.SystemID NOT IN ('" + SlrInfoDefSystemID + @"')
	                       ORDER BY SH.HeadType DESC, SH.SalaryHead";

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
        public void SalaryStructureOnGrid_Change(string sPlantID, string sEmpSystemID, string sTaxYrStartDt, string sTaxYrEndDt, string SalaryInfoDefineMastersystemid, string sSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, 
                                  A.SalaryRuleMasterSystemID, A.SalaryRuleName, A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, A.TagAndUnTag,  
                                  A.SalaryHeadID, A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen,A.IsNA, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, A.SequenceNo, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), EndDate = CASE WHEN REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') = '" + sTaxYrEndDt + @"' THEN ''
																															     ELSE REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') END, 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END,HeadCategory, A.SalaryHdSequence
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												     WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													 WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												ELSE Convert(bit, 'False') END,
										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, 
                                        --ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.CurrencyDesc AS DefinitionCurrency,
                                        ECR.[Name] AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency,

                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,isnull(SG.IsNA,0) IsNA,
                                        SequenceNo = (ISNULL(Fml.SequenceNo, 0) + ISNULL(Fxd.SequenceNo, 0) + ISNULL(SG.SequenceNo, 0)), SH.HeadCategory,
                                        SH.[Sequence] SalaryHdSequence
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
                                        LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
                                        LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes, 
                                                            ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo 
                                                    FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)) Fml 
                                                        ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)) Fxd
                                                        ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND (SG.IsOpen = 1 OR SG.IsNA=1)
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
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												    FROM (select * from SalaryInfoDefineMaster where systemid='" + SalaryInfoDefineMastersystemid + @"' ) SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                              --AND SDM.EffectiveDate IN (
                                                                                        --SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                    --  WHERE EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                                                       -- )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE A.SequenceNo > 0 AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                            AND A.PlantID = '" + sPlantID + @"'
                    ORDER BY A.SequenceNo ASC";

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
        public void GetEmployeeWisePFMonthlyEmpWiseCalculation(string sEmpSystemID, string sDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT PFMC.* FROM [dbo].[PFMonthlyEmpWiseCalculation] PFMC
                              INNER JOIN (
                                          SELECT * FROM[dbo].[PFEligibleEmployee]
                                              WHERE IsApproved = 1 AND EmpSystemID = '" + sEmpSystemID + @"'
                                          ) PFELG ON PFMC.PFEligibleEmpID = PFELG.ID
                        WHERE PFMC.MonthNo = MONTH('" + sDate + @"') AND PFMC.YearNo = YEAR('" + sDate + @"')";

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
        public void GetPFMonthlyDistributionEmployee(string sPFMntEmpWiseCalID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM(Amount) Amount FROM
					            (
					             SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM([Value]) Amount FROM [dbo].[PFMonthlyDistributionEmployee]
						            WHERE ISNULL(SalaryHeadID, '') != '' AND [Value] > 0 AND PFMntEmpWiseCalID = '" + sPFMntEmpWiseCalID + @"'
									GROUP BY PFMntEmpWiseCalID, SalaryHeadID
					             UNION
					             (SELECT PFMntEmpWiseCalID, ResidualValueSlrHdID SalaryHeadID, SUM([UpperLimit]) Amount FROM [dbo].[PFMonthlyDistributionEmployee]
						            WHERE ISNULL(ResidualValueSlrHdID, '') != '' AND [UpperLimit] > 0 AND PFMntEmpWiseCalID = '" + sPFMntEmpWiseCalID + @"'
									GROUP BY PFMntEmpWiseCalID, ResidualValueSlrHdID)
					            ) A 
	                            GROUP BY PFMntEmpWiseCalID, SalaryHeadID";

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
        public void GetPFMonthlyDistributionEmployer(string sPFMntEmpWiseCalID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM(Amount) Amount FROM
	                       (
		                    SELECT PFMntEmpWiseCalID, SalaryHeadID, SUM([Value]) Amount FROM [dbo].[PFMonthlyDistributionEmployer]
		                    WHERE ISNULL(SalaryHeadID, '') != '' AND [Value] > 0 AND PFMntEmpWiseCalID = '" + sPFMntEmpWiseCalID + @"'
		                    GROUP BY PFMntEmpWiseCalID, SalaryHeadID
		                    UNION
		                    (SELECT PFMntEmpWiseCalID, ResidualValueSlrHdID SalaryHeadID, SUM([UpperLimit]) Amount FROM [dbo].[PFMonthlyDistributionEmployer]
		                    WHERE ISNULL(ResidualValueSlrHdID, '') != '' AND [UpperLimit] > 0 AND PFMntEmpWiseCalID = '" + sPFMntEmpWiseCalID + @"'
		                    GROUP BY PFMntEmpWiseCalID, ResidualValueSlrHdID)
	                       ) A 
	                    GROUP BY PFMntEmpWiseCalID, SalaryHeadID";

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
        public void GetPFEligibleEmployeeWithPFDtls(string sPFElegID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.*, B.IsContributionSlrHDdependOnEarningEmp, B.IsContributionSlrHDdependOnEarningEmployer 
                            FROM PFEligibleEmployee A
                                            LEFT JOIN PFPolicyDetails B ON A.PFDtlID = B.ID
                            WHERE A.ID = '" + sPFElegID + @"'";

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
        public void GetESICEligibleEmployeeWithPFDtls(string sESICElegID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.*, B.IsContributionSlrHDdependOnEarningEmp, B.IsContributionSlrHDdependOnEarningEmployer 
                            FROM ESICEligibleEmployee A
                                        LEFT JOIN ESICPolicyDetails B ON A.ESICDtlID = B.ID
                            WHERE A.ID = '" + sESICElegID + @"'";

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
        public void GetEmployeeWisePFMonthlyEmpWiseCalculationSalaryHeadForSS(string strSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SLRPF.SalaryRuleMasterSystemID, CRC.SystemID CRCSystemID, SLRPF.SalaryHeadID, SDPF.SalaryHead, SDPF.HeadType, SDPF.HeadCategory, 
						           CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, 
						           CRC.AccumulateExchangeSalaryHeadID, CRC.IntegerInDisb, CRC.RoundOption, CRC.AmtEntryCurrency AS EntryCurrencyID, ECR.[Name] AS EntryCurrency, 
						           CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, 
                                   DICR.[Name] AS DisbusmentCurrency, CRC.RoundOption, CRC.IsDecimalInDisb, CRC.DecimalNo, SDPF.[Sequence] SalaryHdSequence,srg.SequenceNo
                                  -- ISNULL(SDPF.IsCTCComponent, 0) IsCTCComponent, ISNULL(SDPF.IsGrossComponent, 0) IsGrossComponent     
					        FROM [dbo].[SalaryRulePF] SLRPF
							        LEFT JOIN [dbo].[SalaryHead] SDPF ON SLRPF.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[SalaryRuleMaster] SLRMT ON SLRPF.SalaryRuleMasterSystemID = SLRMT.SystemID
                                    LEFT JOIN SalaryRuleGeneral AS srg ON srg.SalaryRuleMasterSystemID = SLRMT.SystemID AND srg.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[CurrencyRuleChild] CRC ON SLRMT.CurrencyRuleSystemID = CRC.MstSystemID AND SLRPF.SalaryHeadID = CRC.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
									LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
									LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        WHERE SLRPF.SalaryRuleMasterSystemID = '" + strSlrRuleMstSystemID + @"'";

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
        public void GetEmployeeWisePFMonthlyEmpWiseCalculationSalaryHead(string strSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SLRPF.SalaryRuleMasterSystemID, CRC.SystemID CRCSystemID, SLRPF.SalaryHeadID, SDPF.SalaryHead, SDPF.HeadType, SDPF.HeadCategory, 
						           CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, 
						           CRC.AccumulateExchangeSalaryHeadID, CRC.IntegerInDisb, CRC.RoundOption, CRC.AmtEntryCurrency AS EntryCurrencyID, ECR.[Name] AS EntryCurrency, 
						           CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, 
                                   DICR.[Name] AS DisbusmentCurrency, CRC.RoundOption, CRC.IsDecimalInDisb, CRC.DecimalNo, SDPF.[Sequence] SalaryHdSequence
                                  -- ISNULL(SDPF.IsCTCComponent, 0) IsCTCComponent, ISNULL(SDPF.IsGrossComponent, 0) IsGrossComponent     
					        FROM [dbo].[SalaryRulePF] SLRPF
							        LEFT JOIN [dbo].[SalaryHead] SDPF ON SLRPF.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[SalaryRuleMaster] SLRMT ON SLRPF.SalaryRuleMasterSystemID = SLRMT.SystemID
							        LEFT JOIN [dbo].[CurrencyRuleChild] CRC ON SLRMT.CurrencyRuleSystemID = CRC.MstSystemID AND SLRPF.SalaryHeadID = CRC.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
									LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
									LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        WHERE SLRPF.SalaryRuleMasterSystemID = '" + strSlrRuleMstSystemID + @"'";

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
        public void GetSalaryRuleWiseCurrencyChildForSS(string strSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SLRMT.SystemID SalaryRuleMasterSystemID, CRC.SystemID CRCSystemID, CRC.SalaryHeadID, SDPF.SalaryHead, SDPF.HeadType, SDPF.HeadCategory, 
						           CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, 
						           CRC.AccumulateExchangeSalaryHeadID, CRC.IntegerInDisb, CRC.RoundOption, CRC.AmtEntryCurrency AS EntryCurrencyID, ECR.[Name] AS EntryCurrency, 
						           CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, 
                                   DICR.[Name] AS DisbusmentCurrency, CRC.RoundOption, CRC.IsDecimalInDisb, CRC.DecimalNo, SDPF.[Sequence] SalaryHdSequence,srg.SequenceNo
                                   --ISNULL(SDPF.IsCTCComponent, 0) IsCTCComponent, ISNULL(SDPF.IsGrossComponent, 0) IsGrossComponent    
					        FROM [dbo].[CurrencyRuleChild] CRC
							        INNER JOIN [dbo].[SalaryRuleMaster] SLRMT ON SLRMT.CurrencyRuleSystemID = CRC.MstSystemID
							        LEFT JOIN [dbo].[SalaryHead] SDPF ON CRC.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN SalaryRuleGeneral AS srg ON srg.SalaryRuleMasterSystemID = SLRMT.SystemID AND srg.SalaryHeadID = SDPF.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
									LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
									LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        WHERE SLRMT.SystemID = '" + strSlrRuleMstSystemID + @"'";

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
        public void GetSalaryRuleWiseCurrencyChild(string strSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SLRMT.SystemID SalaryRuleMasterSystemID, CRC.SystemID CRCSystemID, CRC.SalaryHeadID, SDPF.SalaryHead, SDPF.HeadType, SDPF.HeadCategory, 
						           CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, 
						           CRC.AccumulateExchangeSalaryHeadID, CRC.IntegerInDisb, CRC.RoundOption, CRC.AmtEntryCurrency AS EntryCurrencyID, ECR.[Name] AS EntryCurrency, 
						           CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, 
                                   DICR.[Name] AS DisbusmentCurrency, CRC.RoundOption, CRC.IsDecimalInDisb, CRC.DecimalNo, SDPF.[Sequence] SalaryHdSequence
                                   --ISNULL(SDPF.IsCTCComponent, 0) IsCTCComponent, ISNULL(SDPF.IsGrossComponent, 0) IsGrossComponent    
					        FROM [dbo].[CurrencyRuleChild] CRC
							        INNER JOIN [dbo].[SalaryRuleMaster] SLRMT ON SLRMT.CurrencyRuleSystemID = CRC.MstSystemID
							        LEFT JOIN [dbo].[SalaryHead] SDPF ON CRC.SalaryHeadID = SDPF.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
									LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
									LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        WHERE SLRMT.SystemID = '" + strSlrRuleMstSystemID + @"'";

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
        public void GetEmployeeWiseESICMonthlyEmpWiseCalculation(string sEmpSystemID, string sDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT ESICMC.* FROM[dbo].[ESICMonthlyEmpWiseCalculation] ESICMC
                              INNER JOIN (
                                          SELECT * FROM[dbo].[ESICEligibleEmployee]
                                              WHERE IsActive = 1 AND EmpSystemID = '" + sEmpSystemID + @"'
                                          ) ESICELG ON ESICMC.ESICEligibleEmpID = ESICELG.ID
                        WHERE ESICMC.MonthNo = MONTH('" + sDate + @"') AND ESICMC.YearNo = YEAR('" + sDate + @"')";

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
        public void GetEmployeeWiseESICMonthlyEmpWiseCalculationSalaryHead(string strSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SLRPF.SalaryRuleMasterSystemID, CRC.SystemID CRCSystemID, SLRPF.SalaryHeadID, SDPF.SalaryHead, SDPF.HeadType, SDPF.HeadCategory, 
						           CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, 
						           CRC.AccumulateExchangeSalaryHeadID, CRC.IntegerInDisb, CRC.RoundOption, CRC.AmtEntryCurrency AS EntryCurrencyID, ECR.[Name] AS EntryCurrency, 
						           CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, 
                                   DICR.[Name] AS DisbusmentCurrency, CRC.RoundOption, CRC.IsDecimalInDisb, CRC.DecimalNo, SDPF.[Sequence] SalaryHdSequence
                                   --ISNULL(SDPF.IsCTCComponent, 0) IsCTCComponent, ISNULL(SDPF.IsGrossComponent, 0) IsGrossComponent     
					        FROM [dbo].[SalaryRuleESIC] SLRPF
							        LEFT JOIN [dbo].[SalaryHead] SDPF ON SLRPF.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[SalaryRuleMaster] SLRMT ON SLRPF.SalaryRuleMasterSystemID = SLRMT.SystemID
							        LEFT JOIN [dbo].[CurrencyRuleChild] CRC ON SLRMT.CurrencyRuleSystemID = CRC.MstSystemID AND SLRPF.SalaryHeadID = CRC.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
									LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
									LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        WHERE SLRPF.SalaryRuleMasterSystemID = '" + strSlrRuleMstSystemID + @"'";

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
        public void GetEmployeeWiseESICMonthlyEmpWiseCalculationSalaryHeadForSS(string strSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SLRPF.SalaryRuleMasterSystemID, CRC.SystemID CRCSystemID, SLRPF.SalaryHeadID, SDPF.SalaryHead, SDPF.HeadType, SDPF.HeadCategory, 
						           CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, 
						           CRC.AccumulateExchangeSalaryHeadID, CRC.IntegerInDisb, CRC.RoundOption, CRC.AmtEntryCurrency AS EntryCurrencyID, ECR.[Name] AS EntryCurrency, 
						           CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, 
                                   DICR.[Name] AS DisbusmentCurrency, CRC.RoundOption, CRC.IsDecimalInDisb, CRC.DecimalNo, SDPF.[Sequence] SalaryHdSequence,srg.SequenceNo
                                   --ISNULL(SDPF.IsCTCComponent, 0) IsCTCComponent, ISNULL(SDPF.IsGrossComponent, 0) IsGrossComponent     
					        FROM [dbo].[SalaryRuleESIC] SLRPF
							        LEFT JOIN [dbo].[SalaryHead] SDPF ON SLRPF.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[SalaryRuleMaster] SLRMT ON SLRPF.SalaryRuleMasterSystemID = SLRMT.SystemID
                                    LEFT JOIN SalaryRuleGeneral AS srg ON srg.SalaryRuleMasterSystemID = SLRMT.SystemID AND srg.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[CurrencyRuleChild] CRC ON SLRMT.CurrencyRuleSystemID = CRC.MstSystemID AND SLRPF.SalaryHeadID = CRC.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
									LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
									LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        WHERE SLRPF.SalaryRuleMasterSystemID = '" + strSlrRuleMstSystemID + @"'";

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
        public void GetEmployeeWiseRetentionMonthlyEmpWiseCalculationSalaryHeadForSS(string strSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SLRRTN.SalaryRuleMasterSystemID, CRC.SystemID CRCSystemID, SLRRTN.SalaryHeadID, SDPF.SalaryHead, SDPF.HeadType, SDPF.HeadCategory, 
						           CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, 
						           CRC.AccumulateExchangeSalaryHeadID, CRC.IntegerInDisb, CRC.RoundOption, CRC.AmtEntryCurrency AS EntryCurrencyID, ECR.[Name] AS EntryCurrency, 
						           CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, 
                                   DICR.[Name] AS DisbusmentCurrency, CRC.RoundOption, CRC.IsDecimalInDisb, CRC.DecimalNo, SDPF.[Sequence] SalaryHdSequence,srg.SequenceNo
                                   ---ISNULL(SDPF.IsCTCComponent, 0) IsCTCComponent, ISNULL(SDPF.IsGrossComponent, 0) IsGrossComponent
					        FROM [dbo].[SalaryRuleRetentionPmtMaster] SLRRTN
							        LEFT JOIN [dbo].[SalaryHead] SDPF ON SLRRTN.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[SalaryRuleMaster] SLRMT ON SLRRTN.SalaryRuleMasterSystemID = SLRMT.SystemID
                                    LEFT JOIN SalaryRuleGeneral AS srg ON srg.SalaryRuleMasterSystemID = SLRMT.SystemID AND srg.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[CurrencyRuleChild] CRC ON SLRMT.CurrencyRuleSystemID = CRC.MstSystemID AND SLRRTN.SalaryHeadID = CRC.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
									LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
									LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        WHERE SLRRTN.SalaryRuleMasterSystemID = '" + strSlrRuleMstSystemID + @"'";

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
        public void GetEmployeeWiseRetentionMonthlyEmpWiseCalculationSalaryHead(string strSlrRuleMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SLRRTN.SalaryRuleMasterSystemID, CRC.SystemID CRCSystemID, SLRRTN.SalaryHeadID, SDPF.SalaryHead, SDPF.HeadType, SDPF.HeadCategory, 
						           CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, 
						           CRC.AccumulateExchangeSalaryHeadID, CRC.IntegerInDisb, CRC.RoundOption, CRC.AmtEntryCurrency AS EntryCurrencyID, ECR.[Name] AS EntryCurrency, 
						           CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, 
                                   DICR.[Name] AS DisbusmentCurrency, CRC.RoundOption, CRC.IsDecimalInDisb, CRC.DecimalNo, SDPF.[Sequence] SalaryHdSequence
                                   ---ISNULL(SDPF.IsCTCComponent, 0) IsCTCComponent, ISNULL(SDPF.IsGrossComponent, 0) IsGrossComponent
					        FROM [dbo].[SalaryRuleRetentionPmtMaster] SLRRTN
							        LEFT JOIN [dbo].[SalaryHead] SDPF ON SLRRTN.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[SalaryRuleMaster] SLRMT ON SLRRTN.SalaryRuleMasterSystemID = SLRMT.SystemID
							        LEFT JOIN [dbo].[CurrencyRuleChild] CRC ON SLRMT.CurrencyRuleSystemID = CRC.MstSystemID AND SLRRTN.SalaryHeadID = CRC.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
									LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
									LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        WHERE SLRRTN.SalaryRuleMasterSystemID = '" + strSlrRuleMstSystemID + @"'";

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
        public void GetEmployeeWiseRetentionEmpWiseCalculation(string sEmpSystemID, string sDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT RAE.EmpSystemID, RAE.RetetionAllowID, RA.ExperienceSpan, RA.Amount 
	                            FROM [dbo].[RetentionAllowEmployee] RAE
				                            INNER JOIN (
						                               SELECT EmpSystemID, MAX(StartDate) StartDate FROM [dbo].[RetentionAllowEmployee]
							                            WHERE StartDate < '" + sDate + @"'
							                            GROUP BY EmpSystemID
						                               ) RAEFD ON RAE.EmpSystemID = RAEFD.EmpSystemID AND RAE.StartDate = RAEFD.StartDate
				                            INNER JOIN [SCS].[RetentionAllowance] RA ON RAE.RetetionAllowID = RA.Id
	                            WHERE RAE.EmpSystemID = '" + sEmpSystemID + @"'
                            ORDER BY RAE.EmpSystemID";

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
        public void GetEmployeeWiseBonusRetainMonthlyEmpWiseCalculationSalaryHead(string sEmpSystemID, string sDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT E.SalaryRuleMasterSystemID, CRC.SystemID CRCSystemID, SLRRTN.SalaryHeadID, SDPF.SalaryHead, SDPF.HeadType, SDPF.HeadCategory, 
						           CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, 
						           CRC.AccumulateExchangeSalaryHeadID, CRC.IntegerInDisb, CRC.RoundOption, CRC.AmtEntryCurrency AS EntryCurrencyID, ECR.[Name] AS EntryCurrency, 
						           CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency, CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, 
                                   DICR.[Name] AS DisbusmentCurrency, CRC.RoundOption, CRC.IsDecimalInDisb, CRC.DecimalNo, SDPF.[Sequence] SalaryHdSequence,
                                   ---ISNULL(SDPF.IsCTCComponent, 0) IsCTCComponent, ISNULL(SDPF.IsGrossComponent, 0) IsGrossComponent, 
                                    SLRRTN.[Value] Amount
					        FROM [dbo].[BonusPolicyMonthlyRetainDistributionStrcPmt] SLRRTN
									INNER JOIN [dbo].[BonusPolicyMonthlyRetainStrcEmpWiseCalculation] M ON M.ID = SLRRTN.BnsPlyMntRetainID 
									LEFT JOIN [dbo].[EmployeeInformation] E ON E.SystemId = M.EmpSystemID
							        LEFT JOIN [dbo].[SalaryHead] SDPF ON SLRRTN.SalaryHeadID = SDPF.SalaryHeadID
							        LEFT JOIN [dbo].[SalaryRuleMaster] SLRMT ON E.SalaryRuleMasterSystemID = SLRMT.SystemID
							        LEFT JOIN [dbo].[CurrencyRuleChild] CRC ON SLRMT.CurrencyRuleSystemID = CRC.MstSystemID AND SLRRTN.SalaryHeadID = CRC.SalaryHeadID
                                    LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
									LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
									LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
					        WHERE M.MonthNo = DATEPART(MONTH,'" + sDate + @"') AND M.YearNo = DATEPART(YEAR,'" + sDate + @"')	AND M.EmpSystemID = '" + sEmpSystemID + @"'";

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
        public void LoadEmpSalaryInfoDefineDataOnGridDesignationWise(string PlantID, string DesignationSystemID, string sTaxYrStartDt, string sTaxYrEndDt, string SalaryRuleMasterSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, SLID.SystemID AS SlrInfoDefSystemID, SLID.DesignationSystemID, 
                                  A.SalaryRuleMasterSystemID, A.SalaryRuleName, A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, A.TagAndUnTag,  
                                  A.SalaryHeadID, A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, A.SequenceNo, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), EndDate = CASE WHEN REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') = '" + sTaxYrEndDt + @"' THEN ''
																															     ELSE REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') END, 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												     WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													 WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												ELSE Convert(bit, 'False') END,
										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, 
                                        --ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.CurrencyDesc AS DefinitionCurrency,
                                        ECR.[description] AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[description] AS DefinitionCurrency,

                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[description] AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,
                                        SequenceNo = (ISNULL(Fml.SequenceNo, 0) + ISNULL(Fxd.SequenceNo, 0) + ISNULL(SG.SequenceNo, 0)), SH.HeadCategory
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                         LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
                                       LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
                                       LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
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
                                                   SELECT SD.SystemID, SD.SalaryID, SDM.DesignationSystemID, EffectiveDate = CASE WHEN ISNULL(SDED.EffectiveDate,'') != '' AND SDED.EffectiveDate <= '" + sTaxYrStartDt + @"' THEN '" + sTaxYrStartDt + @"'
																															   WHEN ISNULL(SDED.EffectiveDate,'') = '' AND SDM.EffectiveDate < '" + sTaxYrStartDt + @"' THEN '" + sTaxYrStartDt + @"'
																															   WHEN ISNULL(SDED.EffectiveDate,'') = '' THEN REPLACE(CONVERT(VARCHAR(11), SDM.EffectiveDate, 106),' ','-')
																															  ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EffectiveDate, 106),' ','-') END,
																										 EndDate = CASE WHEN ISNULL(SDED.EndDate,'') = '' THEN '" + sTaxYrEndDt + @"'
																															  ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EndDate, 106),' ','-') END,
														  SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													      SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												    FROM SalaryInfoDefineMasterDesignationWise SDM
																	    INNER JOIN SalaryInfoDefineDesignationWise SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
												    WHERE SDM.DesignationSystemID = '" + DesignationSystemID + @"'
                                                              AND SDM.EffectiveDate IN (
                                                                                        SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMasterDesignationWise
					                                                                      WHERE DesignationSystemID = '" + DesignationSystemID + @"'
                                                                                        )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.DesignationSystemID = '" + DesignationSystemID + @"' 				
                    WHERE A.SequenceNo > 0 AND A.SalaryRuleMasterSystemID = '" + SalaryRuleMasterSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                            AND A.PlantID = '" + PlantID + @"'
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
        public void LoadEmpSalaryRuleInfoDefineDataOnGrid(string sPlantID, string strSlrRuleMstSystemID, string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Convert(bit, 'False'), SLID.SystemID AS SlrInfoDefSystemID, A.SalaryRuleMasterSystemID, A.SalaryRuleName, A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, 
                                  A.SalaryHeadID, A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                                            WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, 
                                  A.FormulaDes, A.FormulaDesID, ISNULL(A.FixedValue, 0) FixedValue, ISNULL(A.IsOpen, 0) IsOpen
                            FROM (
                                  SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                         CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, 
                                         ECR.code AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.code AS DefinitionCurrency,
                                         CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.code AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                         ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen,0) IsOpen,
                                         SequenceNo = (ISNULL(Fml.SequenceNo, 0) + ISNULL(Fxd.SequenceNo, 0) + ISNULL(SG.SequenceNo, 0)), SH.HeadCategory
                                  FROM SalaryRuleMaster SM
                                            LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                            LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.id
                                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.id
                                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.id
                                            LEFT JOIN (
                                                       SELECT SalaryRuleMasterSystemID, SalaryHeadID, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                        UNION
                                                        (
                                                         SELECT SalaryRuleMasterSystemID, SalaryHeadID, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                          UNION
                                                        (
                                                         SELECT SalaryRuleMasterSystemID, SalaryHeadID, '' FormulaDes, 
                                                         ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo 
                                                         FROM SalaryRuleDayStatusMaster WHERE IsPercemtage = 1)) Fml 
                                                            ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                            LEFT JOIN (
                                                       SELECT SalaryRuleMasterSystemID, SalaryHeadID, FixedValue, SequenceNo FROM SalaryRuleGeneral WHERE IsFixed = 1
                                                        UNION
                                                        (
                                                         SELECT SalaryRuleMasterSystemID, SalaryHeadID, FixedValue, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFixed = 1
                                                        )
                                                        UNION
                                                        (
                                                         SELECT SalaryRuleMasterSystemID, SalaryHeadID, FixedValue, SequenceNo FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)
                                                        ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                            LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND SG.IsOpen = 1
                                   ) A		
                                        LEFT JOIN (
                                                   SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													      SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												    FROM SalaryInfoDefineMaster SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                              AND SDM.EffectiveDate IN (
                                                                                        SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                      WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND IsApproved = 1
                                                                                        )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE A.SequenceNo > 0 AND A.SalaryRuleMasterSystemID = '" + strSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                            AND A.PlantID = '" + sPlantID + @"'
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
        public void LoadEmpSalaryInfoDefineDataOnGridEffectiveDate(string sPlantID, string sEmpSystemID, string sSlrRuleMstSystemID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, 
                                    A.SalaryRuleMasterSystemID, A.SalaryRuleName, A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, 
                                    A.SalaryHeadID, A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                                  WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                    A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, 
                                    A.FormulaDes, A.FormulaDesID, A.FixedValue, A.IsOpen, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, 
                                    A.SequenceNo, REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-') EffectiveDate, SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                    IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                      ELSE SLID.IsApproved END
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, 
                                        ECR.Code AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.Code AS DefinitionCurrency,
                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Code AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,
                                        SequenceNo = (ISNULL(Fml.SequenceNo, 0) + ISNULL(Fxd.SequenceNo, 0) + ISNULL(SG.SequenceNo, 0)), SH.HeadCategory
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
                                        LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
                                        LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FormulaDes, FormulaDesID, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, '' FormulaDes, 
                                                    ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo 
                                                    FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)) Fml 
                                                        ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FixedValue, SequenceNo FROM SalaryRuleGeneral WHERE IsFixed = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FixedValue, SequenceNo FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, FixedValue, SequenceNo FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)) Fxd
                                                        ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND SG.IsOpen = 1
                                  ) A
                                        LEFT JOIN (
                                                    SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, SDM.EffectiveDate, SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													        SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													        SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate
												    FROM SalaryInfoDefineMaster SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
												    WHERE SDM.EmpInfoSystemID = '" + sEmpSystemID + @"'
                                                                AND SDM.EffectiveDate = '" + sEffectiveDate + @"'
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + sEmpSystemID + @"' 				
                    WHERE A.SequenceNo > 0 AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                            AND A.PlantID = '" + sPlantID + @"'
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

        public void GetUnApprovedSalaryProcYearNo(string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT DISTINCT SM.YearNo 
					        FROM SalaryProcMaster SM
									INNER JOIN (SELECT * FROM SalaryProcChild WHERE IsApproved = 0 AND IsDisbursed = 0) SC ON SM.SystemID = SC.SlrProcMstSystemID
                            WHERE SC.GroupID = '" + sGroupID + @"' AND SC.PlantID = '" + sPlantID + @"'
					        ORDER BY SM.YearNo";


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
        public void GetUnApprovedSalaryProcMonthNo(string sGroupID, string sPlantID, int iYearNo, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT DISTINCT SM.MonthNo 
					        FROM SalaryProcMaster SM
									INNER JOIN (SELECT * FROM SalaryProcChild WHERE IsApproved = 0 AND IsDisbursed = 0) SC ON SM.SystemID = SC.SlrProcMstSystemID
                            WHERE SC.GroupID = '" + sGroupID + @"' AND SC.PlantID = '" + sPlantID + @"' AND SM.YearNo = " + iYearNo + @"
					        ORDER BY SM.MonthNo";


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

        public void GetUnDisbursedSalaryProcYearNo(string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT DISTINCT SM.YearNo 
					        FROM SalaryProcMaster SM
									INNER JOIN (SELECT * FROM SalaryProcChild WHERE IsApproved = 1 AND IsDisbursed = 0) SC ON SM.SystemID = SC.SlrProcMstSystemID
                            WHERE SC.GroupID = '" + sGroupID + @"' AND SC.PlantID = '" + sPlantID + @"'
					        ORDER BY SM.YearNo";


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
        public void GetUnDisbursedSalaryProcMonthNo(string sGroupID, string sPlantID, int iYearNo, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT DISTINCT SM.MonthNo 
					        FROM SalaryProcMaster SM
									INNER JOIN (SELECT * FROM SalaryProcChild WHERE IsApproved = 1 AND IsDisbursed = 0) SC ON SM.SystemID = SC.SlrProcMstSystemID
                            WHERE SC.GroupID = '" + sGroupID + @"' AND SC.PlantID = '" + sPlantID + @"' AND SM.YearNo = " + iYearNo + @"
					        ORDER BY SM.MonthNo";


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

        public bool SalaryIDFoundInSalaryProc(string sSalaryID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            bool blnStatus = false;

            try
            {
                strSql = @"SELECT * FROM [dbo].[SalaryProcChild] 
                            WHERE SalaryID = '" + sSalaryID + @"'";

                //                                        IN (
                //					                            SELECT DISTINCT SalaryID FROM [dbo].[SalaryInfoDefine] 
                //					                             WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND EffectiveDate = '" + sEffectiveDate + @"'
                //				                              )

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

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
        public bool SalaryInfoFoundAfterSelectedEffectiveDate(string sEmpSystemID, string sEffectiveDate)
        {
            System.Data.DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            bool blnStatus = false;

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineMaster 
					        WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND EffectiveDate > '" + sEffectiveDate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

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

        public void GetCurrencyInfo(string sGroupID, string sPlantID, string strCurrencyID, string sFromDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TOP(1) EWER.SystemID, EWER.FromCurrencyCode CurrencyCode,CR.Code CurrencyDesc, EWER.ToCurrencyBuying ExchangeRate, 
		                          EWER.ToCurrencyBuying BuyingRate, EWER.ToCurrencySelling SellingRate 
	                        FROM ExchangeRateDateWiseForHR EWER
		                            INNER JOIN scs.Currency CR ON EWER.FromCurrencyCode = CR.Id
	                        WHERE EWER.GroupID = '" + sGroupID + @"' AND EWER.PlantID = '" + sPlantID + @"' AND FromDate <= '" + sFromDate + @"'
                                  AND EWER.FromCurrencyCode = '" + strCurrencyID + @"'";

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

        public void GetSalaryStructure(string empid, string groupid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Systemid from SalaryInfoDefineMaster where EmpInfoSystemID='" + empid + "' and Groupid='" + groupid + "'";

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

        public void GetSalaryInfoDefine(string strEmpSysID, string sSalaryID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefine 
                              WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster
                                                WHERE EmpInfoSystemID IN ('" + strEmpSysID + @"')
                                                      AND SystemID IN ('" + sSalaryID + @"'))";

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
        public void GetSalaryInfoDefineApproved(string strEmpSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefine WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID IN ('" + strEmpSysID + @"') AND IsApproved=1)";

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
        public void GetSalaryInfoDefine(string strEmpSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefine WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID = '" + strEmpSysID + @"')";

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
        public void GetUnApprovedSalaryInfoDefineDesignationWise(string DesignationSystemID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineDesignationWise 
                            WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMasterDesignationWise
                                                WHERE DesignationSystemID IN ('" + DesignationSystemID + @"')
                                                      AND IsApproved = 0 AND SystemID IN ('" + sSysID + @"'))";

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
        public void GetUnApprovedSalaryInfoDefine(string strEmpSysID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefine 
                            WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster
                                                WHERE EmpInfoSystemID IN ('" + strEmpSysID + @"')
                                                      AND IsApproved = 0 AND SystemID IN ('" + sSysID + @"'))";

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
        public void GetTotalSalary(string strEmpSysID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT ISNULL(c.DefineAmount,0) DefineAmount,HeadCategory  FROM 
                                    SalaryInfoDefineMaster m
                                    LEFT OUTER JOIN SalaryInfoDefine c on m.SystemID=c.SalaryID
                                    LEFT OUTER JOIN SalaryHead h on h.SalaryHeadID=c.SalaryHeadID
                                    WHERE m.EmpInfoSystemID='" + strEmpSysID + @"' AND m.SystemId='" + sSysID + @"' 
                                    --AND HeadCategory = 'TOTAL GROSS'
                                ";


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

        public void GetSalaryInfo(string strEmpSysID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"select SUM(ISNULL(A.CTCAmount,0)) CTCAmount,SUM(ISNULL(A.BasicAmount,0))BasicAmount,SUM(ISNULL(A.GrossAmount,0)) GrossAmount,A.Type from (
SELECT SUM(ISNULL(c.DefineAmount,0)) CTCAmount,0 BasicAmount,0 GrossAmount,'CTC' Type  FROM 
                                    SalaryInfoDefineMaster m
                                    LEFT OUTER JOIN SalaryInfoDefine c on m.SystemID=c.SalaryID
                                    LEFT OUTER JOIN SalaryHead h on h.SalaryHeadID=c.SalaryHeadID
                                    WHERE m.EmpInfoSystemID='" + strEmpSysID + @"' AND m.SystemId='" + sSysID + @"' AND h.IsCTCComponent=1
Union ALL
SELECT 0 CTCAmount, SUM(ISNULL(c.DefineAmount,0)) BasicAmount,0 GrossAmount,'Basic' Type  FROM 
                                    SalaryInfoDefineMaster m
                                    LEFT OUTER JOIN SalaryInfoDefine c on m.SystemID=c.SalaryID
                                    LEFT OUTER JOIN SalaryHead h on h.SalaryHeadID=c.SalaryHeadID
                                    WHERE m.EmpInfoSystemID='" + strEmpSysID + @"' AND m.SystemId='" + sSysID + @"' AND h.IsBasicComponent=1
Union ALL
									SELECT 0 CTCAmount, 0 BasicAmount,SUM(ISNULL(c.DefineAmount,0)) GrossAmount,'Gross' Type  FROM 
                                    SalaryInfoDefineMaster m
                                    LEFT OUTER JOIN SalaryInfoDefine c on m.SystemID=c.SalaryID
                                    LEFT OUTER JOIN SalaryHead h on h.SalaryHeadID=c.SalaryHeadID
                                    WHERE m.EmpInfoSystemID='" + strEmpSysID + @"' AND m.SystemId='" + sSysID + @"' AND h.IsGrossComponent=1)A
									Group bY A.Type ";


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

        public void GetSalaryInfoDefineMaster(string sEmpSysID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID = '" + sEmpSysID + @"' AND SystemID = '" + sSysID + @"'";

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
         //IncrementHistory
        public void GetSalaryInfoIncrementHistory(string sEmpSysID, string sSalaryID, string EffectiveDate, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM IncrementHistory Where IsApproved=0 and EmpSystemID='" + sEmpSysID + @"' and ToEffectiveDate='" + EffectiveDate + @"' and ToSalaryId='" + sSalaryID + @"'";
                           

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

        public void GetSalaryInfoDefineMasterForIncrement(string sEmpSysID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineMaster 
                            WHERE IsApproved=0 and EmpInfoSystemID = '" + sEmpSysID + @"' 
                                  AND SystemID = '" + sSysID + @"'";

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
        public void GetSalaryInfoDefineMasterApproved(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID = '" + sEmpSysID + @"' AND IsApproved = 1";

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
        public void GetSalaryInfoDefineMasterDesignationWise(string DesignationSystemID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineMasterDesignationWise 
                            WHERE DesignationSystemID = '" + DesignationSystemID + @"' 
                                  AND SystemID = '" + sSysID + @"'";

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
        public void GetSalaryInfoDefineMaster(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineMaster 
                            WHERE EmpInfoSystemID = '" + sEmpSysID + @"'";

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
        public void GetSalaryInfoDefineMasterBasedOnEffectiveDate(string strEmpSysID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineMaster
                                                    WHERE EmpInfoSystemID = '" + strEmpSysID + @"' 
                                                          AND EffectiveDate = '" + sEffectiveDate + @"'";

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
        public void GetSalaryInfoBackMaster(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoBackMaster WHERE EmpInfoSystemID = '" + sEmpSysID + @"' AND SystemID IS NULL";

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
        public void GetSalaryInfoBack(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoBack WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID = '" + sEmpSysID + @"' AND SystemID IS NULL)";

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
        public void GetTaxSlrSDINSalaryRule(string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT CH.* FROM CurrencyRuleChild CH
					                            INNER JOIN CurrencyRuleMaster CM ON CH.MstSystemID = CM.SystemID
					                            INNER JOIN SalaryHead SH ON CH.SalaryHeadID = SH.SalaryHeadID AND SH.HeadCategory = 'Tax'
                             WHERE CM.PlantID = '" + sPlantID + @"'";

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
        public void GetSalaryStructure(string EmpInfoSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from [SalaryInfoDefine]
                                    where SalaryHeadID in (select SalaryHeadID from SalaryHead where HeadCategory='Tax')
                                    and salaryId=(select systemid from SalaryInfoDefineMaster where EmpInfoSystemID='"+ EmpInfoSystemID + "' and IsApproved=1)";

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
        public void GetTaxSalaryInfoDefine(string strEmpSysID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefine 
		                    WHERE SalaryHeadID IN (
								                    SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'Tax'
							                      ) 
			                      AND SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster
                                                    WHERE EmpInfoSystemID IN ('" + strEmpSysID + @"')
                                                          AND SystemID IN ('" + sSysID + @"'))";

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
        public void GetTaxSalaryInfoDefineMax(string strEmpSysID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT MAX(ISNULL(SequenceNo, 0)) SequenceNo FROM SalaryInfoDefine 
		                    WHERE --SalaryHeadID IN (
								  --                  SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'Tax'
							      --                ) 
			                      --AND 
                                  SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster
                                                    WHERE EmpInfoSystemID IN ('" + strEmpSysID + @"')
                                                          AND SystemID IN ('" + sSysID + @"'))";

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
        public void GetTaxSalaryInfoDefineDesignationWise(string DesignationSystemID, string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineDesignationWise 
		                    WHERE SalaryHeadID IN (
								                    SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory = 'Tax'
							                      ) 
			                      AND SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMasterDesignationWise
                                                    WHERE DesignationSystemID IN ('" + DesignationSystemID + @"')
                                                          AND SystemID IN ('" + sSysID + @"'))";

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
        public void GetSalaryInfoDefineEffectiveDate(string sSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryInfoDefineEffectiveDate 
        		                    WHERE SalaryID = '" + sSysID + @"'";

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
        public void LoadSalaryApprovedEffectiveDate(string sPlantID, string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT REPLACE(CONVERT(VARCHAR(11), EffectiveDate, 113), ' ', '-') EffectiveDate
                            FROM 
                                (
                                 SELECT ROW_NUMBER() OVER (ORDER BY EffectiveDate) AS RowNum, EffectiveDate 
                                   FROM [dbo].[SalaryInfoDefineMaster] 
                                  WHERE IsApproved = 1 AND EmpInfoSystemID = '" + sEmpSystemID + @"' AND PlantID = '" + sPlantID + @"'
	                              GROUP BY EffectiveDate
                                ) A 
                            ORDER BY RowNum DESC";

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
        public void GetMBAllowance(string ManpowerBudgetId, string EffectiveDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                string ed = Convert.ToDateTime(EffectiveDate).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT top 1 *
                                FROM mst.manpowerbudgetallowance a
                                WHERE a.ManpowerBudgetId = '" + ManpowerBudgetId + @"'
	                                AND a.EffectiveDate<='" + ed + @"'
									order by a.EffectiveDate desc";

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
        public void xCheckFreshEntry(string PlantId, string EmpSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                //strSQL = @"SELECT *
                //                FROM mst.manpowerbudgetallowance a
                //                WHERE a.ManpowerBudgetId = '"+ ManpowerBudgetId + @"'
                //                 AND a.EffectiveDate IN (
                //                  SELECT MAX(EffectiveDate) EffectiveDate		
                //                  FROM mst.manpowerbudgetallowance		
                //                  WHERE ManpowerBudgetId = '" + ManpowerBudgetId + @"'
                //                  )";

                strSQL = @"SELECT *  FROM dbo.Employeeinformation EI
                              WHERE  EI.EmployeeStatus ='Active' AND EI.PlantId='" + PlantId + @"' AND EI.SystemId='" + EmpSystemId + @"'  AND EI.SystemId IN (
                              SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 1
                              union
                              SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 0)                            
                              union
                              SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster)
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
        public void CheckFreshEntry(string PlantId, string EmpSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                //strSQL = @"SELECT *
                //                FROM mst.manpowerbudgetallowance a
                //                WHERE a.ManpowerBudgetId = '"+ ManpowerBudgetId + @"'
                //                 AND a.EffectiveDate IN (
                //                  SELECT MAX(EffectiveDate) EffectiveDate		
                //                  FROM mst.manpowerbudgetallowance		
                //                  WHERE ManpowerBudgetId = '" + ManpowerBudgetId + @"'
                //                  )";

                strSQL = @"SELECT *  FROM dbo.Employeeinformation EI
                              WHERE  EI.EmployeeStatus ='Active' AND EI.PlantId='" + PlantId + @"' AND EI.SystemId='" + EmpSystemId + @"'  AND EI.SystemId IN (
                              SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 1
                              union
                              SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 0)                            
                              union
                              SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster)
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

        public void GetEntityAllowance(string EffectiveDate, string BudgetCodeId, string DesignationGroupId, string CompanyGroupId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {               
                strSQL = @" SELECT top 1*
                            FROM org.entityallowance a
                            WHERE a.DesignationGroupId = '" + DesignationGroupId + @"'
	                            AND entityid in (select EntityId from mst.ManpowerBudget where Id='" + BudgetCodeId + @"')
	                            AND CompanyGroupId = '" + CompanyGroupId + @"'
	                             AND a.EffectiveDate<='" + EffectiveDate + @"'
								 order by a.EffectiveDate desc";

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
        #endregion Salary Information

        #region Salary Increment

        public void GetBankNameInfo(string sGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM BankNameInfo 
                //            ORDER BY BankNameShort";
                strSQL = @"SELECT b.id SystemID, B.UserName+' '+bb.UserName BankNameShort FROM  HKP.Bank B		
	                                    left outer join hkp.BankBranch bb on b.Id=bb.BankId		
	                           --WHERE CompanyGroupID = 'CG20171'		
	                           ORDER BY BankNameShort";

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
        public void GetCurrencyInfoForExternalDataUpLoad(string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EWER.FromCurrencyCode CurrencyCode, CR.CurrencyDesc, EWER.ToCurrencyBuying ExchangeRate, 
                                  EWER.ToCurrencyBuying BuyingRate, EWER.ToCurrencySelling SellingRate 
	                        FROM ExchangeRateDateWiseForHR EWER
		                            INNER JOIN Currency CR ON EWER.FromCurrencyCode = CR.CurrencyCode
	                        WHERE EWER.PlantID = '" + sPlantID + @"'";

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
        public void GetExtDataUploadFromExcelSalaryHeadLoadCbo(string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SH.SalaryHead, SH.SalaryHeadID FROM SalaryHead SH
                                    INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
							        INNER JOIN CurrencyRuleMaster CRM ON CRC.MstSystemID = CRM.SystemID
                            WHERE CRM.PlantID = '" + sPlantID + "' AND SH.ExtDataUpload = 1";

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
        public void GetSalaryIncrementInfoDefineMaster(string sIncSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryIncrementInfoDefineMaster
                                WHERE SystemID = '" + sIncSysID + @"'";

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
        public void GetUnApprovedSalaryIncrementInfoDefineMaster(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryIncrementInfoDefineMaster
                                WHERE EmpInfoSystemID = '" + sEmpSysID + @"' AND IsApproved = 0";

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
        public void GetSalaryIncrementInfoDefine(string sIncSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryIncrementInfoDefine
                                WHERE IncMstSystemID = '" + sIncSysID + @"'";

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
        public void GetApprovedSalaryIncrementInfoDefineDefine(string sEmpSysID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM SalaryIncrementInfoDefineMaster 
                             WHERE EmpInfoSystemID = '" + sEmpSysID + @"' AND IsApproved = 1
                                   AND EffectiveDate = '" + sEffectiveDate + @"'";

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
        public void LoadSalaryIncrementApprovedEffectiveDate(string sPlantID, string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT REPLACE(CONVERT(VARCHAR(11), EffectiveDate, 113), ' ', '-') EffectiveDate
                            FROM 
                                (
                                 SELECT ROW_NUMBER() OVER (ORDER BY EffectiveDate) AS RowNum, EffectiveDate 
                                   FROM [dbo].[SalaryIncrementInfoDefineMaster] 
                                  WHERE IsApproved = 1 AND EmpInfoSystemID = '" + sEmpSystemID + @"' AND PlantID = '" + sPlantID + @"'
	                              GROUP BY EffectiveDate
                                ) A 
                            ORDER BY RowNum DESC";

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
        public void LoadSalaryIncrementApprovedBasedOnEffectiveDate(string sPlantID, string sEmpSystemID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT *
                            FROM [dbo].[SalaryIncrementInfoDefineMaster] 
                            WHERE IsApproved = 1 AND EmpInfoSystemID = '" + sEmpSystemID + @"' AND PlantID = '" + sPlantID + @"'
                                  AND EffectiveDate = '" + sEffectiveDate + @"'";

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

        #endregion Salary Increment
        public void GetAllSalaryStructureByempForIncrement(string EmpId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID,IsApproved,
		                          Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate
                            FROM SalaryInfoDefineMaster
                            WHERE IsApproved=0 and EmpInfoSystemID = '" + EmpId + @"'"; 

                         

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
        public void GetAllSalaryStructureByemp(string EmpId,  out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID,IsApproved,
		                          Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate
                            FROM SalaryInfoDefineMaster
                            WHERE EmpInfoSystemID = '" + EmpId + @"' 

                           UNION

                           SELECT SystemID,IsApproved,
		                          Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate
                            FROM SalaryInfoBackMaster
                            WHERE EmpInfoSystemID = '" + EmpId + @"' ";

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
        public void GetAllSalaryStructureByempForDelete(string EmpId,  out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT b.SystemID,a.IsApproved,a.EffectiveDate FROM (
                        SELECT IsApproved, Replace(CONVERT(VARCHAR(11),MAX(EffectiveDate), 106), ' ', '-') EffectiveDate,EmpInfoSystemID
                        FROM SalaryInfoDefineMaster
                        WHERE EmpInfoSystemID ='" + EmpId + @"'  and IsApproved=0
                        GROUP BY IsApproved,EmpInfoSystemID
                        ) a 
                        JOIN SalaryInfoDefineMaster b ON b.EffectiveDate=a.EffectiveDate AND a.EmpInfoSystemID=b.EmpInfoSystemID
                        UNION 
                        SELECT b.SystemID,a.IsApproved,a.EffectiveDate FROM (
                        SELECT IsApproved, Replace(CONVERT(VARCHAR(11),MAX(EffectiveDate), 106), ' ', '-') EffectiveDate,EmpInfoSystemID
                        FROM SalaryInfoBackMaster
                        WHERE EmpInfoSystemID ='" + EmpId + @"' and IsApproved=0
                        GROUP BY IsApproved,EmpInfoSystemID
                        ) a 
                         JOIN SalaryInfoBackMaster b ON b.EffectiveDate=a.EffectiveDate AND a.EmpInfoSystemID=b.EmpInfoSystemID";

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
        public void GetEmployeeSalaryRuleEditable(string plantid,string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from hkp.EmployeeSalaryRuleEditable where plantid='"+ plantid + "' and EmployeeId='"+ EmployeeId + "'";

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

        public void GetSalaryProcessSchduleHead(string SalaryProcSystemId,string GroupId,string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from SalaryProcessScheduleHead where SalaryProcSystemId='" + SalaryProcSystemId + "' and PlantId='" + PlantId + "' and GroupId='"+ GroupId + "'";

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
        }//End Function .
        public void GetSalaryProcessSchduleHead( out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from SalaryProcessScheduleHead";

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
        }//End Function .
        public void GetSalaryProcessScheduleDetail(string HeadSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from SalaryProcessScheduleDetail where HeadSystemId='" + HeadSystemId + "'";

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
        }//End Function .
        public void GetSalaryProcessSchduleDetail(string HeadSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from SalaryProcessScheduleDetail where HeadSystemId='" + HeadSystemId + "' ";

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

        public void SalaryStructureChangedByemp(string EmpId, string TextEffectiveDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID,IsApproved,
		                          Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate
                            FROM SalaryInfoDefineMaster
                            WHERE EmpInfoSystemID = '" + EmpId + @"' and EffectiveDate>='" + TextEffectiveDate + @"' 
                          UNION
                          SELECT SystemID,IsApproved,
		                       Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate
                           FROM SalaryInfoBackMaster
                           WHERE EmpInfoSystemID = '" + EmpId + @"' and EffectiveDate>='" + TextEffectiveDate + @"'";

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
        public void CheckEffDateOnSalProcMasterChild(string EffDate, string EmpId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.SalaryProcMaster SPM 
                            LEFT OUTER JOIN dbo.SalaryProcChild SPC ON SPM.SystemID=SPC.SlrProcMstSystemID
                            WHERE SPM.ToDate>='" + EffDate + "' AND SPC.EmpInfoSystemID='" + EmpId + "' AND isnull(spc.IsApproved,0) = 1";

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
        public void CheckEmpFromSalInfoDefineMaster(string empId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID='" + empId + "' AND IsApproved=0";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objCon = null;
            }
        }//End Function 
        public void GetSSEmpidAndDateWise(string EmpId, string EffectiveDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID,IsApproved,
		                          Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate
                            FROM SalaryInfoDefineMaster
                            WHERE EmpInfoSystemID = '" + EmpId + @"' AND EffectiveDate='" + EffectiveDate + @"'
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
        public void GetSalaryInfoDefineMasterForValidation(string EmpId, string EffDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID,IsApproved,
                                  Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate
                           FROM SalaryInfoDefineMaster
                           WHERE EmpInfoSystemID = '" + EmpId + @"' 
                                 AND EffectiveDate >= '" + EffDate + @"'";

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
        public void GetApprovedInfo(string EmpId, string EffDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID, Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate
                            FROM SalaryInfoDefineMaster
                            WHERE EmpInfoSystemID = '" + EmpId + @"' AND EffectiveDate <'" + EffDate + @"' and IsApproved = 1";

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
        public void GetPossibleMaxEffectiveDate(string EmpId,  out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT MAX(ED) ED FROM
                             (
                               SELECT MAX(EffectiveDate) ED FROM [dbo].[SalaryInfoDefineMaster] WHERE EmpInfoSystemID = '" + EmpId + @"'
                               UNION
                               SELECT MAX(EffectiveDate) FROM [dbo].[SalaryInfoBackMaster] WHERE EmpInfoSystemID = '" + EmpId + @"'
                               UNION
                               SELECT MAX(ToDate) ED FROM SalaryProcMaster 
                                   WHERE Systemid IN (
                                                      SELECT SlrProcMstSystemID from SalaryProcChild WHERE EmpInfoSystemID='" + EmpId + @"'
                                                     )
                              ) x ";

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
        public void GetSalaryStructureUnapproved(string ToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.* , E.EmployeeName, E.EmployeeCode FROM 
			                (
			                 SELECT SystemID, EmpInfoSystemID, GroupID, PlantID, EffectiveDate, 
					                IsApproved
			                 FROM SalaryInfoDefineMaster
				             UNION 
			                 (
			                  SELECT SystemID, EmpInfoSystemID,  GroupID, PlantID, EffectiveDate, 
					                 IsApproved
			                  FROM SalaryInfoBackMaster
			                 )
			                ) A
			                LEFT OUTER JOIN EmployeeInformation E ON E.SystemId = A.EmpInfoSystemID
		                    WHERE A.IsApproved = 0 AND A.EffectiveDate <= '" + ToDate + @"'	";

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
        public void GetMaxEffectiveDate(string EmpId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpInfoSystemID,max(EffectiveDate) EffectiveDate FROM ( SELECT EmpInfoSystemID,max(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
                            WHERE EmpInfoSystemID = '" + EmpId + @"' AND IsApproved=1
                            GROUP BY EmpInfoSystemID
                            UNION
                            SELECT EmpInfoSystemID,max(EffectiveDate) EffectiveDate FROM SalaryInfoBackMaster
                            WHERE EmpInfoSystemID = '" + EmpId + @"' AND IsApproved=1
                            GROUP BY EmpInfoSystemID )  x GROUP BY x.EmpInfoSystemID ";

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
        public void GetUnapprovedSalaryStructure(string PlantId, string GroupId, string FromDate, string ToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT PlantID, GroupID, Replace(CONVERT(VARCHAR(11), MAX(EffectiveDate), 106), ' ', '-') EffectiveDate
                                 ,SystemID 
                            FROM SalaryInfoDefineMaster
                            WHERE IsApproved=0 AND PlantId = '" + PlantId + @"' AND GroupId='" + GroupId + @"'												
                            GROUP BY PlantID, GroupID, SystemID
							HAVING MAX(EffectiveDate) BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'";

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
        public void GetMaxEffectiveDateProcessed(string EmpId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpInfoSystemID
                            ,Replace(CONVERT(VARCHAR(11), MAX(M.ToDate), 106), ' ', '-') EffectiveDate
                                    FROM SalaryProcMaster m
                                left outer join (select * from SalaryProcChild where IsApproved=1) c on c.SlrProcMstSystemID=m.SystemID
                                                    WHERE EmpInfoSystemID = '" + EmpId + @"' 
                            group by EmpInfoSystemID";

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
        public void Delete(string EmployeeId, string SalaryInfoDefineMasterId)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                //set HeadCategory =CAST(NULL As nvarchar(100))
                string updateEmp = @"update EmployeeInformation set SalaryRuleMasterSystemID=CAST(NULL As nvarchar(100)) Where Systemid='" + EmployeeId + "'";

                objCon.ExecuteNonQueryWrapper(updateEmp, true, "1");
                //objCon.ExecuteNonQueryWrapper("Delete FROM EmployeeBankInfo   WHERE EmpSystemID='" + EmployeeId + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper("Delete from TaxGroupTagWithEmployee Where EmpInfoSystemID='" + EmployeeId + "'", true, "1");

                string sqlSalaryInfoDefine = @"Delete FROM SalaryInfoDefine 
                                                WHERE SalaryID='" + SalaryInfoDefineMasterId + @"'";
                objCon.ExecuteNonQueryWrapper(sqlSalaryInfoDefine, true, "1");
                string sqlSalaryInfoDefineEffectiveDate = @"Delete FROM SalaryInfoDefineEffectiveDate WHERE  SalaryID='" + SalaryInfoDefineMasterId + @"'"; ;
                objCon.ExecuteNonQueryWrapper(sqlSalaryInfoDefineEffectiveDate, true, "1");

                string taxsh = @"Delete from TaxableIncomeSalaryHeadWise Where TaxDefineMasterSystemID in 
                                (SELECT SystemID from TaxDefineMaster Where salaryid = '" + SalaryInfoDefineMasterId + @"')";
                objCon.ExecuteNonQueryWrapper(taxsh, true, "1");
                string tdm = @"Delete from TaxDeductionInfoMonthWise Where TaxDefineMasterSystemID in 
                                (SELECT SystemID from TaxDefineMaster Where salaryid ='" + SalaryInfoDefineMasterId + @"')";
                objCon.ExecuteNonQueryWrapper(tdm, true, "1");
                objCon.ExecuteNonQueryWrapper(@"Delete from TaxDefineMaster Where salaryid='" + SalaryInfoDefineMasterId + @"'", true, "1");
                //objCon.ExecuteNonQueryWrapper("Delete from TaxableYearlyActualIncomeSalaryHeadWise Where EmpInfoSystemID='" + EmployeeId + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper("Delete from TaxGroupTagWithEmployee Where EmpInfoSystemID='" + EmployeeId + "'", true, "1");

                objCon.ExecuteNonQueryWrapper("Delete from SalaryInfoDefineMaster Where SystemID='" + SalaryInfoDefineMasterId + "'", true, "1");



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
        }//End Function 
        public void xDelete(string EmployeeId, string EffectiveDate)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                //set HeadCategory =CAST(NULL As nvarchar(100))
                string updateEmp = @"update EmployeeInformation set SalaryRuleMasterSystemID=CAST(NULL As nvarchar(100)) Where Systemid='" + EmployeeId + "'";

                objCon.ExecuteNonQueryWrapper(updateEmp, true, "1");
                //objCon.ExecuteNonQueryWrapper("Delete FROM EmployeeBankInfo   WHERE EmpSystemID='" + EmployeeId + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper("Delete from TaxGroupTagWithEmployee Where EmpInfoSystemID='" + EmployeeId + "'", true, "1");

                string sqlSalaryInfoDefine = @"Delete FROM SalaryInfoDefine 
                                                WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster
                                                WHERE EmpInfoSystemID IN ('" + EmployeeId + @"')
                                                AND IsApproved = 0  --and EffectiveDate='" + EffectiveDate + @"')";
                objCon.ExecuteNonQueryWrapper(sqlSalaryInfoDefine, true, "1");
                string sqlSalaryInfoDefineEffectiveDate = @"Delete FROM SalaryInfoDefineEffectiveDate WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster
                                                WHERE EmpInfoSystemID IN ('" + EmployeeId + @"') AND IsApproved = 0   and EffectiveDate='" + EffectiveDate + @"')";
                objCon.ExecuteNonQueryWrapper(sqlSalaryInfoDefineEffectiveDate, true, "1");

                string taxsh = @"Delete from TaxableIncomeSalaryHeadWise Where TaxDefineMasterSystemID in 
                                (SELECT SystemID from TaxDefineMaster Where salaryid in 
                                (SELECT SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID IN('" + EmployeeId + @"') and EffectiveDate='" + EffectiveDate + @"'))";
                objCon.ExecuteNonQueryWrapper(taxsh, true, "1");
                string tdm = @"Delete from TaxDeductionInfoMonthWise Where TaxDefineMasterSystemID in 
                                (SELECT SystemID from TaxDefineMaster Where salaryid in 
                                (SELECT SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID IN('" + EmployeeId + @"')  and EffectiveDate='" + EffectiveDate + @"'))";
                objCon.ExecuteNonQueryWrapper(tdm, true, "1");
                objCon.ExecuteNonQueryWrapper(@"Delete from TaxDefineMaster Where salaryid in (SELECT SystemID FROM SalaryInfoDefineMaster
                                                WHERE EmpInfoSystemID IN('" + EmployeeId + @"')  and EffectiveDate='" + EffectiveDate + @"')", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from TaxableYearlyActualIncomeSalaryHeadWise Where EmpInfoSystemID='" + EmployeeId + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from TaxGroupTagWithEmployee Where EmpInfoSystemID='" + EmployeeId + "'", true, "1");

                objCon.ExecuteNonQueryWrapper("Delete from SalaryInfoDefineMaster Where EmpInfoSystemID='" + EmployeeId + "'  and EffectiveDate='" + EffectiveDate + @"'", true, "1");



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
        }//End Function 
        public void DeletePFAftEnt(string sPFEligibleEmpID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                //set HeadCategory =CAST(NULL As nvarchar(100))
                string DelPFEmp = @"DELETE FROM PFMonthlyDistributionEmployee 
	                                    WHERE PFMntEmpWiseCalID IN (SELECT ID FROM PFMonthlyEmpWiseCalculation WHERE PFEligibleEmpID = '" + sPFEligibleEmpID + @"')";

                objCon.ExecuteNonQueryWrapper(DelPFEmp, true, "1");

                string DelPFEmpr = @"DELETE FROM PFMonthlyDistributionEmployer
	                                    WHERE PFMntEmpWiseCalID IN (SELECT ID FROM PFMonthlyEmpWiseCalculation WHERE PFEligibleEmpID = '" + sPFEligibleEmpID + @"')";
                objCon.ExecuteNonQueryWrapper(DelPFEmpr, true, "1");

                string DelVPF = @"DELETE FROM PFEmployeeVoluntaryValue WHERE PFEligibleEmpID = '" + sPFEligibleEmpID + @"'";
                objCon.ExecuteNonQueryWrapper(DelVPF, true, "1");

                string DelPFCal = @"DELETE FROM PFMonthlyEmpWiseCalculation WHERE PFEligibleEmpID = '" + sPFEligibleEmpID + @"'";
                objCon.ExecuteNonQueryWrapper(DelPFCal, true, "1");
                
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
        }//End Function 

    }
}