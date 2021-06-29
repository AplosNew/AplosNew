using System.Data;

namespace OTSBD
{
    public class clsBonusProcess
    {
        public clsBonusProcess()
        {
            // TODO: Add constructor logic here
        }

        public void SearchBonusPolicyMasterInfo(string sPlantID, string strKey, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM 
		                            (SELECT SystemID, PolicyName, BonusDescription, DefaultPolicy
                                    FROM BonusPolicyMaster TPM 
		                            WHERE PlantID = '" + sPlantID + @"') A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";

                }

                strSql = strSql + " Order By PolicyName";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetBonusPolicyMaster(string strSystemID, string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                            FROM BonusPolicyMaster 
                          WHERE SystemID = '" + strSystemID + @"' ";

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
        public void LoadGrdBonusPolicy(string sBonusPolicyMstSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT BPX.SystemID, BPX.BPMSystemID, BPX.EntitleFrm, BPX.EmpCategorySysID,  BPX.MinServLen, BPX.MaxServLen, 
	                              BPX.IsFixed, BPX.FixedAmount, BPX.IsPercentage, BPX.PerctSalaryHeadID, PrctSH.SalaryHead PerctSalaryHead, BPX.BonusPercentage, 
	                              BPX.IsProportionate, BPX.DivisionFactor, BPX.MinBonusAmt,BPX.ServiceLengthType
                           FROM BonusPolicyDetail BPX
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
        public void LoadEmployeeInGrdForBonusProcess(string sPlantID, string sBonusPolicyMstSystemID, string sCutOffDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectBonusProc = Convert(bit, 'True'), E.SystemID EmpSystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, 
	                              DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-'), '" + sCutOffDate + @"') + 1 ServiceLength,
	                              REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC, 
	                              DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '" + sCutOffDate + @"') + 1 ConfirmServiceLength, 
	                              DG.UserName DesignationGroup, EC.UserName EmpCategoryName, E.EmployeeCategorySystemID, '' SalaryHeadID,
	                              '' SalaryHead, '' SalaryAmount, '' BonusAmount, '' EntryCurrencyID, '' DefineCurrencyID, '' DisbustCurrencyID, 
								  '' AmtDefinationCurrencyID, 1 AmtDefinationRate, E.UnitID, U.UserName UnitName, E.DivisionID, Dv.UserName DivisionName,  
					              E.DepartmentID, De.UserName DepartmentName, E.SectionID, Se.UserName SectionName, E.SubSectionID, SuS.UserName SubSectionName, 
                                  E.LineID, Ln.UserName LineName, E.DesignationSystemID, Dsg.UserName DesignationName
                           FROM EmployeeInformation E
				                    LEFT OUTER JOIN
						                    hkp.DesignationGroup DG ON E.DesignationGroupID = DG.Id
				                    LEFT OUTER JOIN 
							                hkp.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
						            LEFT OUTER JOIN 
									        org.Unit AS U ON U.Id = E.UnitID 
						            LEFT OUTER JOIN 
									        org.Division AS Dv ON Dv.Id = E.DivisionID 
						            LEFT OUTER JOIN 
									        org.Department AS De ON De.Id = E.DepartmentID 
						            LEFT OUTER JOIN 
									        hkp.Designation AS Dsg ON Dsg.Id = E.DesignationSystemID
						            LEFT OUTER JOIN 
									        org.Section AS Se ON Se.Id = E.SectionID 
						            LEFT OUTER JOIN 
									        org.SubSection AS SuS ON SuS.Id = E.SubSectionID
									LEFT OUTER JOIN 
									        org.Line AS Ln ON Ln.Id = E.LineID
				                   -- INNER JOIN 
						                  --  (
						                    -- SELECT * FROM [dbo].[BonusPolMstTagEmp] WHERE BnsPolMstSystemID = '" + sBonusPolicyMstSystemID + @"'
						                   -- ) BMT ON E.SystemID = BMT.EmpSystemID 
                           WHERE E.PlantID = '" + sPlantID + @"' AND E.SalaryRuleMasterSystemID IS NOT NULL AND 
	                             E.DOJ <= CONVERT(DATETIME, '" + sCutOffDate + @"') AND (DOS > CONVERT(DATETIME, '" + sCutOffDate + @"') OR DOS IS NULL)
                           ORDER BY E.EmployeeCode";

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
        public void xLoadEmployeeInGrdForBonusProcess(string sPlantID, string sBonusPolicyMstSystemID, string sCutOffDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectBonusProc = Convert(bit, 'True'), E.SystemID EmpSystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, 
	                              DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-'), '" + sCutOffDate + @"') + 1 ServiceLength,
	                              REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC, 
	                              DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '" + sCutOffDate + @"') + 1 ConfirmServiceLength, 
	                              DG.DesignationGroupName DesignationGroup, EC.EmpCategoryName, E.EmployeeCategorySystemID, '' SalaryHeadID,
	                              '' SalaryHead, '' SalaryAmount, '' BonusAmount, '' EntryCurrencyID, '' DefineCurrencyID, '' DisbustCurrencyID, 
								  '' AmtDefinationCurrencyID, 1 AmtDefinationRate, E.UnitID, U.UnitName, E.DivisionID, Dv.DivisionName,  
					              E.DepartmentID, De.DepartmentName, E.SectionID, Se.SectionName, E.SubSectionID, SuS.SubSectionName, 
                                  E.LineID, Ln.LineName, E.DesignationSystemID, Dsg.DesignationName
                           FROM EmployeeInformation E
				                    LEFT OUTER JOIN
						                    DesignationGroup DG ON E.DesignationGroupID = DG.SystemID
				                    LEFT OUTER JOIN 
							                EmpCategory AS EC ON E.EmployeeCategorySystemID = EC.SystemID
						            LEFT OUTER JOIN 
									        Unit AS U ON U.SystemID = E.UnitID 
						            LEFT OUTER JOIN 
									        Division AS Dv ON Dv.SystemID = E.DivisionID 
						            LEFT OUTER JOIN 
									        Department AS De ON De.SystemID = E.DepartmentID 
						            LEFT OUTER JOIN 
									        Designation AS Dsg ON Dsg.SystemID = E.DesignationSystemID 
						            LEFT OUTER JOIN 
									        Section AS Se ON Se.SystemID = E.SectionID 
						            LEFT OUTER JOIN 
									        SubSection AS SuS ON SuS.SystemID = E.SubSectionID
									LEFT OUTER JOIN 
									        Line AS Ln ON Ln.SystemID = E.LineID
				                    INNER JOIN 
						                    (
						                     SELECT * FROM [dbo].[BonusPolMstTagEmp] WHERE BnsPolMstSystemID = '" + sBonusPolicyMstSystemID + @"'
						                    ) BMT ON E.SystemID = BMT.EmpSystemID 
                           WHERE E.PlantID = '" + sPlantID + @"' AND E.SalaryRuleMasterSystemID IS NOT NULL AND 
	                             E.DOJ <= CONVERT(DATETIME, '" + sCutOffDate + @"') AND (DOS > CONVERT(DATETIME, '" + sCutOffDate + @"') OR DOS IS NULL)
                           ORDER BY E.EmployeeCode";

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

                                                       DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-'), '" + sCutOffDate + @"') + 1 ServiceLength_Day,
													   DATEDIFF(MM, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-'), '" + sCutOffDate + @"') ServiceLength_Month,
                                                        DATEDIFF(MM, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-'), '28-Jun-2021') ServiceLength,
                                                       REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC, 

                                                    ConfirmServiceLength_Day = CASE WHEN (DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '" + sCutOffDate + @"') + 1) > 0 THEN 
                                                    DATEDIFF(DD, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '" + sCutOffDate + @"') + 1 ELSE 0 END, 

													ConfirmServiceLength_Month = CASE WHEN (DATEDIFF(MM, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '" + sCutOffDate + @"')) > 0 THEN 
                                                    DATEDIFF(MM, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-'), '" + sCutOffDate + @"') ELSE 0 END,



                                                       DG.UserName DesignationGroup, EC.UserName EmpCategoryName,dm.EmployeeCategoryId EmployeeCategorySystemID, '' SalaryHeadID,
                                                       '' SalaryHead, '' SalaryAmount, '' BonusAmount, '' EntryCurrencyID, '' DefineCurrencyID, '' DisbustCurrencyID, 
							                           '' AmtDefinationCurrencyID, 1 AmtDefinationRate, E.UnitID, U.UserName UnitName, E.DivisionID, Dv.UserName DivisionName,  
							                           PR.DepartmentID, DP.UserName DepartmentName, PR.SectionID, Se.UserName SectionName, PR.SubSectionID, SuS.UserName SubSectionName, 
                                                       PMB.LineID, L.UserName LineName, dm.DesignationId DesignationSystemID, DeG.UserName DesignationName
                                                                FROM EmployeeInformation E

								                                LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
                                                              LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                                              LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
                                                              LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                                                              LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = E.LegalDesignationId
                                                              LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                                              left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                                                              left join HKP.Designation DeG on DeG.Id=dm.DesignationId
                                                              left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                                                              left join ORG.Section SE on SE.Id=PR.SectionId
                                                              LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                                              LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId

                                                                LEFT OUTER JOIN HKP.DesignationGroup DG ON E.DesignationGroupID = DG.id
                                          
                                                                LEFT OUTER JOIN    ORG.Unit AS U ON U.id = En.UnitID 
                                                                LEFT OUTER JOIN 
							                                    ORG.Division AS Dv ON Dv.id = En.DivisionID 

				                                                INNER JOIN
							                                        (select m.DesignationId,c.BonusPolicyMasterId from MST.DesignationMaster m
													                left join (select * from scs.DesignationMasterConfiguration where PlantId= '" + sPlantID + @"')
													                c on m.id=c.DesignationMasterId) DMS ON DMS.DesignationId = E.GivenDesignationId
                                                                WHERE E.PlantID = '" + sPlantID + @"' AND E.SalaryRuleMasterSystemID IS NOT NULL
                                        AND E.DOJ <= CONVERT(DATETIME, '" + sCutOffDate + @"') AND (DOS >= CONVERT(DATETIME, '" + sCutOffDate + @"') OR DOS IS NULL)
                                 ) A
                                LEFT JOIN (	SELECT DENSE_RANK() OVER (PARTITION BY bnX.BPMSystemID ORDER BY bnX.MinServLen) AS RNK,*  FROM BonusPolicyDetail BNX ) AS BN ON bn.BPMSystemID = A.BnsPolMstSystemID 
                                    AND  ((bn.RNK=1 AND A.ServiceLength >= bn.MinServLen) OR (BN.RNK>1 AND A.ServiceLength > bn.MinServLen )) and A.ServiceLength <= bn.MaxServLen
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
        public void xLoadEmployeeInGrdForDefaultBonusProcess(string sPlantID, string sBonusPolicyMstSystemID, string sCutOffDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT IsSelectBonusProc = Convert(bit, 'True'), * FROM
                                (
                                 SELECT ISNULL(dms.BonusPolicyMasterId, 'NULL') BnsPolMstSystemID, E.SystemID EmpSystemID, E.EmployeeCode, E.EmployeeName
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
        public void GetBonusPolMstTagEmp(string sEmpInfo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                            FROM BonusPolMstTagEmp 
                          WHERE (" + sEmpInfo + @")";

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
        public void LoadEmployeeInGrdBonusProcessApproval(string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectBonusProc = Convert(bit, 'True'), E.SystemID EmpSystemID, BPA.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, 
	                              BPA.ServiceLenght ServiceLength,
	                              REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC,
	                              DG.DesignationGroupName DesignationGroup, EC.EmpCategoryName, E.EmployeeCategorySystemID, BPAM.SalaryHeadID,
	                              SH.SalaryHead, BPA.SalaryAmount, BPA.BonusAmount
                           FROM EmployeeInformation E
				                    INNER JOIN
											[dbo].[BonusPaymentActual] BPA ON E.SystemID = BPA.EmpSystemID
				                    INNER JOIN
											[dbo].[BonusPaymentActualMaster] BPAM ON BPA.BnsMstSystemID = BPAM.SystemID
									LEFT OUTER JOIN
						                    [dbo].[DesignationGroup] DG ON E.DesignationGroupID = DG.SystemID
				                    LEFT OUTER JOIN 
							                [dbo].[EmpCategory] AS EC ON E.EmployeeCategorySystemID = EC.SystemID 
									LEFT OUTER JOIN
						                    [dbo].[SalaryHead] SH ON BPAM.SalaryHeadID = SH.SalaryHeadID
                           WHERE E.PlantID = '" + sPlantID + @"' AND BPA.IsApproved = 0
                           ORDER BY E.EmployeeCode";

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
        public void LoadEmployeeInGrdBonusProcessDisbusment(string sPlantID, string sTaxYearID, string sEffectiveDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectBonusProc = Convert(bit, 'True'), E.SystemID EmpSystemID, BPA.SystemID, E.EmployeeCode, E.EmployeeName, BPA.ServiceLenght ServiceLength, 
	                              REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC,
	                              DG.DesignationGroupName DesignationGroup, EC.EmpCategoryName, E.EmployeeCategorySystemID, BPAM.SalaryHeadID,
	                              SH.SalaryHead, BPA.SalaryAmount, BPA.BonusAmount, TPM.TaxGroupID, TPM.SystemID TaxPolMstSystemID, TPM.TaxYearID,
								  TDM.EffectiveDate, 0 TotalTaxPayablePeriod, 0 PartialTaxPayablePeriod, 0 TaxPayablePeriod, '' Taxable, TDM.SystemID TaxDefineMasterSystemID, 
								  TDM.SalaryID, ISNULL(TDM.TaxAbleIncome, 0) TaxAbleIncome, ISNULL(TDM.InvestmentAmount, 0) InvestmentAmount, 
								  ISNULL(TDM.RebateAmount, 0) RebateAmount, ISNULL(TDM.TaxableAmount, 0) TaxableAmount, ISNULL(TDM.PaidTaxAmount, 0) PaidTaxAmount, 
								  ISNULL(TOB.OpeningBalance, 0) OpeningBalance, ISNULL(TDM.TaxToBePay, 0) TaxToBePay, ISNULL(TDM.ActualTaxAmountPerMonth, 0) ActualTaxAmountPerMonth
                           FROM EmployeeInformation E
				                    INNER JOIN
											[dbo].[BonusPaymentActual] BPA ON E.SystemID = BPA.EmpSystemID
				                    INNER JOIN
											[dbo].[BonusPaymentActualMaster] BPAM ON BPA.BnsMstSystemID = BPAM.SystemID
									INNER JOIN
											[dbo].[TaxGroupTagWithEmployee] TXGrp ON E.SystemID = TXGrp.EmpInfoSystemID
									LEFT OUTER JOIN 
											[dbo].[TaxPolicyMaster] TPM ON TXGrp.TaxGroupID = TPM.TaxGroupID AND TPM.PlantID = '" + sPlantID + @"' AND TPM.TaxYearID = '" + sTaxYearID + @"'
									LEFT OUTER JOIN
						                    [dbo].[DesignationGroup] DG ON E.DesignationGroupID = DG.SystemID
				                    LEFT OUTER JOIN 
							                [dbo].[EmpCategory] AS EC ON E.EmployeeCategorySystemID = EC.SystemID 
									LEFT OUTER JOIN
						                    [dbo].[SalaryHead] SH ON BPAM.SalaryHeadID = SH.SalaryHeadID 
									LEFT OUTER JOIN
						                    [dbo].[TaxOpeningBalance] TOB ON E.SystemID = TOB.EmpInfoSystemID AND TOB.TaxYearID = '" + sTaxYearID + @"'
									LEFT OUTER JOIN
						                    (
											 SELECT TDM.* FROM [dbo].[TaxDefineMaster] TDM
												INNER JOIN  
														  (
														   SELECT EmpInfoSystemID, Max(EffectiveDate) EffectiveDate 
															FROM [dbo].[TaxDefineMaster] 
															WHERE TaxYearID = '" + sTaxYearID + @"' AND EffectiveDate <= '" + sEffectiveDate + @"'
															GROUP BY EmpInfoSystemID
														  ) TDMEFD ON TDM.EmpInfoSystemID = TDMEFD.EmpInfoSystemID AND TDM.EffectiveDate = TDMEFD.EffectiveDate
											) TDM ON E.SystemID = TDM.EmpInfoSystemID
                           WHERE E.PlantID = '" + sPlantID + @"' AND BPA.IsApproved = 1 AND BPA.IsDisbused = 0
                           ORDER BY E.EmployeeCode";

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
                          ORDER BY TAYAISHW.EmpInfoSystemID, SH.SalaryHead";

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
        public void LoadTaxableIncomeSlrWiseDataOnGrid(string sEmpSystemID, string sPlantID, string sTAXGroup, string sTAXYear, out DataSet dsRef)
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
                                  TAISH.ConvertionRate, TAISH.YearlyIncome
                           FROM TaxableIncomeSalaryHeadWise TAISH
                                    INNER JOIN TaxDefineMaster TDM ON TAISH.TaxDefineMasterSystemID = TDM.SystemID AND TDM.TaxYearID = '" + sTAXYear + @"'
                                    INNER JOIN TaxGroup TG ON TAISH.TaxGroupID = TG.SystemID AND TG.SystemID IN ('" + sTAXGroup + @"')
                                    INNER JOIN TaxPolicyMaster TPM ON TAISH.TaxPolicyMstID = TPM.SystemID AND TPM.PlantID = '" + sPlantID + @"' AND TPM.TaxYearID = '" + sTAXYear + @"'
                                    INNER JOIN SalaryHead SH ON TAISH.SalaryHeadID = SH.SalaryHeadID 
                                    LEFT JOIN Currency EC ON TAISH.EntryIncomeCurrencyID = EC.CurrencyCode
                                    LEFT JOIN Currency DC ON TAISH.DefinationCurrencyID = DC.CurrencyCode
                                    LEFT JOIN Currency LC ON TAISH.LocalCurrencyID = LC.CurrencyCode
                           WHERE TAISH.PlantID = '" + sPlantID + @"'";

                if (sEmpSystemID != "")
                {
                    strSQL = strSQL + @" AND TAISH.EmpInfoSystemID IN ('" + sEmpSystemID + @"')";
                }

                strSQL = strSQL + @" 
                          ORDER BY TAISH.EmpInfoSystemID, TDM.TaxPaidUptoYear DESC, TDM.TaxPaidUptoMonth DESC";

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

        public void LoadEmpSalaryInfoDefineDataOnGrid(string sPlantID, string sEmpSystemID, string sBonusDisDt, string sTaxYrStartDt, string sTaxYrEndDt, out DataSet dsRef)
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
                                                    ELSE SLID.IsApproved END, '0' FinalPaidTaxAmount
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
					                                                                    WHERE EffectiveDate <= '" + sBonusDisDt + @"'
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
                    ORDER BY SLID.EmpInfoSystemID, A.SequenceNo, A.HeadType DESC, A.SalaryHead ASC";

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
        public void LoadBonusPolicyMasterFor(string sPlantID, string sSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[BonusPolicyMaster] WHERE PlantID = '" + sPlantID + @"'";

                if (sSystemID.Trim() != "")
                {
                    strSQL += @"
                            AND SystemID = '" + sSystemID + "'";
                }
                strSQL += @"
                            ORDER BY PolicyName";

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
    }
}