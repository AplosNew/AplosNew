using bplib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace OTSBD
{
    public class clsBonusMonthlyRetainNew
    {
        public string sFormulaValue = "";

        public clsBonusMonthlyRetainNew()
        {
            // TODO: Add constructor logic here
        }//End Function
        public void GetBonusPolicyMonthlyRetainMaster(string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM BonusPolicyMonthlyRetainMaster WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

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
        public void GetBonusPolicyMonthlyRetainDetails(string sBnsPlcMthRetainID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[BonusPolicyMonthlyRetainDetails] WHERE BnsPlcMthRetainID = '" + sBnsPlcMthRetainID + @"'";

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
        public void GetBonusPolicyMonthlyRetainMonthNo(string sBnsPlcMthRetainID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM BonusPolicyMonthlyRetainMonthNo
                              WHERE BnsPlcMthRetainMstID = '" + sBnsPlcMthRetainID + @"'";

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
        public void GetBonusPolicyMonthlyRetainDistribution(string sBnsPlcMthRetainID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * 
                                FROM [dbo].[BonusPolicyMonthlyRetainDistribution] 
                            WHERE BonusPolicyDetailsID IN (
                                                           SELECT ID
                                                                FROM [dbo].[BonusPolicyMonthlyRetainDetails] 
                                                            WHERE BnsPlcMthRetainID = '" + sBnsPlcMthRetainID + @"'
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
        public void GetSalaryRuleGovtGrd(BnsParaListNew para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.* FROM [dbo].[SalaryRuleGovtGrd] A
			                        INNER JOIN [dbo].[SalaryRuleMaster] B ON A.SalaryRuleMasterSystemID = B.SystemID
                          WHERE B.PlantID = '" + para.PlantID + "'";

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

        public void GetBonusPolicyMonthlyRetainEmpWiseCalculation(BnsParaListNew para, string sEmpSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT *
                                        FROM BonusPolicyMonthlyRetainEmpWiseCalculation 
                                      WHERE " + sEmpSystemID + @" AND MonthNo = " + para.iMonth + @" AND YearNo = " + para.iYear + @"";
                }
                else
                {
                    strSQL = @"SELECT *
                                        FROM BonusPolicyMonthlyRetainEmpWiseCalculation
                                      WHERE MonthNo = " + para.iMonth + @" AND YearNo = " + para.iYear + @"";
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
        public void GetBonusPolicyMonthlyRetainDistributionPmt(BnsParaListNew para, string sEmpSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT * FROM BonusPolicyMonthlyRetainDistributionPmt
                                WHERE BnsPlyMntRetainID IN (
                                                            SELECT ID FROM BonusPolicyMonthlyRetainEmpWiseCalculation 
                                                             WHERE " + sEmpSystemID + @" AND MonthNo = " + para.iMonth + @" AND YearNo = " + para.iYear + @"
                                                           )";
                }
                else
                {
                    strSQL = @"SELECT * FROM BonusPolicyMonthlyRetainDistributionPmt
                                WHERE BnsPlyMntRetainID IN (
                                                            SELECT ID FROM BonusPolicyMonthlyRetainEmpWiseCalculation 
                                                             WHERE MonthNo = " + para.iMonth + @" AND YearNo = " + para.iYear + @"
                                                           )";
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
        public void GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(BnsParaListNew para, string sEmpSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT *
                                        FROM BonusPolicyMonthlyRetainStrcEmpWiseCalculation 
                                      WHERE " + sEmpSystemID + @" AND MonthNo = " + para.iMonth + @" AND YearNo = " + para.iYear + @"";
                }
                else
                {
                    strSQL = @"SELECT *
                                        FROM BonusPolicyMonthlyRetainStrcEmpWiseCalculation
                                      WHERE MonthNo = " + para.iMonth + @" AND YearNo = " + para.iYear + @"";
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
        public void GetBonusPolicyMonthlyRetainDistributionStrcPmt(BnsParaListNew para, string sEmpSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT * FROM BonusPolicyMonthlyRetainDistributionStrcPmt
                                WHERE BnsPlyMntRetainID IN (
                                                            SELECT ID FROM BonusPolicyMonthlyRetainStrcEmpWiseCalculation 
                                                             WHERE " + sEmpSystemID + @" AND MonthNo = " + para.iMonth + @" AND YearNo = " + para.iYear + @"
                                                           )";
                }
                else
                {
                    strSQL = @"SELECT * FROM BonusPolicyMonthlyRetainDistributionStrcPmt
                                WHERE BnsPlyMntRetainID IN (
                                                            SELECT ID FROM BonusPolicyMonthlyRetainStrcEmpWiseCalculation 
                                                             WHERE MonthNo = " + para.iMonth + @" AND YearNo = " + para.iYear + @"
                                                           )";
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
        public void GetBonusPolicyMonthlyRetainEligibleEmployee(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT *
                                        FROM BonusPolicyMonthlyRetainEligibleEmployee 
                                      WHERE " + sEmpSystemID + @"";
                }
                else
                {
                    strSQL = @"SELECT *
                                        FROM BonusPolicyMonthlyRetainEligibleEmployee";
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
        public void GetDesignationMasterWiseMinSalary(BnsParaListNew para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT DMLD.LegalDesignationId DesignationId, LSSV.LegalSalaryStructureId, SH.SalaryHead, LSSV.SalaryHeadId, LSSV.SalaryHeadValue, LSS.EmployeeLocationId 
                            FROM [MST].[DesignationMaster] DM
		                            INNER JOIN [MST].[DesignationMasterLegalDesignation] DMLD ON DM.Id = DMLD.DesignationMasterId
		                            INNER JOIN [MST].[LegalSalaryGradeDesignation] LSGD ON LSGD.LegalDesignationId = DMLD.LegalDesignationId
		                            INNER JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id
		                            INNER JOIN (
					                            SELECT A.* FROM [MST].[LegalSalaryStructure] A
						                            INNER JOIN
								                              (
								                               SELECT LegalSalaryGradeId, EmployeeLocationId, MAX(EffectiveDate) EffectiveDate FROM [MST].[LegalSalaryStructure] 
                                                                WHERE EffectiveDate <= '" + para.EDate + @"'
									                           GROUP BY LegalSalaryGradeId, EmployeeLocationId
								                              ) B ON A.LegalSalaryGradeId = B.LegalSalaryGradeId AND A.EffectiveDate = B.EffectiveDate
																 AND A.EmployeeLocationId = B.EmployeeLocationId
					                            ) LSS ON LSG.Id = LSS.LegalSalaryGradeId
		                            INNER JOIN [MST].[LegalSalaryStructureValue] LSSV ON LSS.Id = LSSV.LegalSalaryStructureId
		                            LEFT JOIN [dbo].[SalaryHead] SH ON LSSV.SalaryHeadId = SH.SalaryHeadID  
                            WHERE LSGD.PlantId = '" + para.PlantID + @"'
							GROUP BY DMLD.LegalDesignationId, LSSV.LegalSalaryStructureId, SH.SalaryHead, LSSV.SalaryHeadId, LSSV.SalaryHeadValue, LSS.EmployeeLocationId 
							ORDER BY DMLD.LegalDesignationId";

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
        public void xGetDesignationMasterWiseMinSalary(BnsParaListNew para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT DM.DesignationId, LSSV.LegalSalaryStructureId, SH.SalaryHead, LSSV.SalaryHeadId, LSSV.SalaryHeadValue, LSS.EmployeeLocationId 
                            FROM [MST].[DesignationMaster] DM
		                            INNER JOIN [MST].[DesignationMasterLegalDesignation] DMLD ON DM.Id = DMLD.DesignationMasterId
		                            INNER JOIN [MST].[LegalSalaryGradeDesignation] LSGD ON LSGD.LegalDesignationId = DMLD.LegalDesignationId
		                            INNER JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id
		                            INNER JOIN (
					                            SELECT A.* FROM [MST].[LegalSalaryStructure] A
						                            INNER JOIN
								                              (
								                               SELECT LegalSalaryGradeId, EmployeeLocationId, MAX(EffectiveDate) EffectiveDate FROM [MST].[LegalSalaryStructure] 
                                                                WHERE EffectiveDate <= '" + para.ToDate + @"'
									                           GROUP BY LegalSalaryGradeId, EmployeeLocationId
								                              ) B ON A.LegalSalaryGradeId = B.LegalSalaryGradeId AND A.EffectiveDate = B.EffectiveDate
																 AND A.EmployeeLocationId = B.EmployeeLocationId
					                            ) LSS ON LSG.Id = LSS.LegalSalaryGradeId
		                            INNER JOIN [MST].[LegalSalaryStructureValue] LSSV ON LSS.Id = LSSV.LegalSalaryStructureId
		                            LEFT JOIN [dbo].[SalaryHead] SH ON LSSV.SalaryHeadId = SH.SalaryHeadID  
                            WHERE LSGD.PlantId = '" + para.PlantID + @"'
							GROUP BY DM.DesignationId, LSSV.LegalSalaryStructureId, SH.SalaryHead, LSSV.SalaryHeadId, LSSV.SalaryHeadValue, LSS.EmployeeLocationId 
							ORDER BY DM.DesignationId";

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

        public void GetUnTagEmployeeListWithBonusPolicyMonthlyRetain(BnsParaListNew para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (para.sEmpSystemID == "")
                {
                    strSQL = @"SELECT EEE.ID BnsEligibleEmpID, DM.BnsPlcMthRetainID, BC.EmployeeLocationId, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN  (SELECT DC.LeavePolicyMasterId,DC.BnsPlcMthRetainID,D.DesignationId FROM MST.DesignationMaster D
											LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
											WHERE DC.PlantId='" + para.PlantID + @"') DM ON E.GivenDesignationId = DM.DesignationId
                                            INNER JOIN [MST].[ManpowerBudget] BC ON E.BudgetCode = BC.Id
			                                LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                LEFT JOIN (SELECT * FROM [dbo].[BonusPolicyMonthlyRetainEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                INNER JOIN [dbo].[BonusPolicyMonthlyRetainMaster] BPMRMst ON DM.BnsPlcMthRetainID = BPMRMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL)
	                                  AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[BonusPolicyMonthlyRetainEligibleEmployee])";
                }
                else
                {
                    strSQL = @"SELECT EEE.ID BnsEligibleEmpID, DM.BnsPlcMthRetainID, BC.EmployeeLocationId, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN  (SELECT DC.LeavePolicyMasterId,DC.BnsPlcMthRetainID,D.DesignationId FROM MST.DesignationMaster D
											LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
											WHERE DC.PlantId='" + para.PlantID + @"') DM ON E.GivenDesignationId = DM.DesignationId
                                            INNER JOIN [MST].[ManpowerBudget] BC ON E.BudgetCode = BC.Id
			                                LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                LEFT JOIN (SELECT * FROM [dbo].[BonusPolicyMonthlyRetainEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                INNER JOIN [dbo].[BonusPolicyMonthlyRetainMaster] BPMRMst ON DM.BnsPlcMthRetainID = BPMRMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL)
	                                  AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[BonusPolicyMonthlyRetainEligibleEmployee]) 
                                      AND E.SystemId IN (" + para.sEmpSystemID + @")";
                }
                strSQL += @"
                                ORDER BY E.GivenDesignationId, E.SystemId";

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
        public void GetTagEmployeeListWithBonusPolicyMonthlyRetain(BnsParaListNew para, string sBnsPlcMthRetainID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (para.sEmpSystemID == "")
                {
                    strSQL = @"SELECT distinct ---EEE.ID 
                                  E.SystemId  BnsEligibleEmpID, DM.BnsPlcMthRetainID, BC.EmployeeLocationId, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN (SELECT DC.LeavePolicyMasterId,DC.BnsPlcMthRetainID,D.DesignationId FROM MST.DesignationMaster D
											                LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
											            WHERE DC.PlantId='" + para.PlantID + @"') DM ON E.GivenDesignationId = DM.DesignationId
                                            INNER JOIN [MST].[ManpowerBudget] BC ON E.BudgetCode = BC.Id
			                                LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                ---INNER JOIN (SELECT * FROM [dbo].[BonusPolicyMonthlyRetainEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                LEFT JOIN [dbo].[BonusPolicyMonthlyRetainMaster] BPMRMst ON DM.BnsPlcMthRetainID = BPMRMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' 
                                      AND DM.BnsPlcMthRetainID = '" + sBnsPlcMthRetainID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL)";
                }
                else
                {
                    strSQL = @"SELECT distinct ---EEE.ID 
                                E.SystemId  BnsEligibleEmpID, DM.BnsPlcMthRetainID, BC.EmployeeLocationId, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN (SELECT DC.LeavePolicyMasterId,DC.BnsPlcMthRetainID,D.DesignationId FROM MST.DesignationMaster D
											LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
											WHERE DC.PlantId='" + para.PlantID + @"') DM ON E.GivenDesignationId = DM.DesignationId
                                            INNER JOIN [MST].[ManpowerBudget] BC ON E.BudgetCode = BC.Id
			                                LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                ---INNER JOIN (SELECT * FROM [dbo].[BonusPolicyMonthlyRetainEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                LEFT JOIN [dbo].[BonusPolicyMonthlyRetainMaster] BPMRMst ON DM.BnsPlcMthRetainID = BPMRMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL)
                                      AND DM.BnsPlcMthRetainID = '" + sBnsPlcMthRetainID + @"' AND E.SystemId IN (" + para.sEmpSystemID + @")";
                }
                strSQL += @"
                              ORDER BY E.GivenDesignationId, E.SystemId";

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

        public void GenRefSrNoID(string strEntryDate, string strFieldName, int SrNo, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            //  int						lngRecCount=0;
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;

            System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                // strEntryDate = AppDateConvert(strEntryDate, getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();

                strSql = "SELECT * FROM Signature WHERE Field ='" + strFieldName.Trim() + "' AND Dates = '" + strEntryDate + "'";

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
                    //LastNumber = 1 + SrNo;
                    LastNumber = 1;

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
                    //LastNumber = LastNumber + SrNo;
                    //LastNumber = LastNumber;

                    drLocal.BeginEdit();
                    drLocal["LastNumber"] = LastNumber + 1;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                //strID = strID + "-" + ((int)LastNumber - SrNo);
                strID = strID + "-" + (int)LastNumber;
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
        }//End Function
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
        }// End of function
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
        }//End Function
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
        }// end function
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
        }//End Function
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
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon = null;
            }

        }//End Function  

        private void ReLoadFormulaWithValueForBonusCal(string sEmpSystemID, BnsParaListNew para, string sFormulaID, bool bEarning, bool bIsMinWages, string sCompMinWagesAndOrginal, string sSalaryRuleMasterSystemID, string sEmpLocationID, string sEmpGivenDesignationId, ref DataTable dtValue, ref DataTable dtSlrHd, ref DataTable dtSlrGrd, ref DataTable dtMinWagSalary, ref DataTable dtDw)
        {
            #region Declaretion

            DataSet dsOrgSlr = null;
            DataView dvOrgSlr = null;

            DataView dvSlrHd = null;
            DataView dvSlrGrd = null;
            DataSet dsGovSlrSD = null;

            DataSet dsGovSlr = null;
            DataView dvGovSlr = null;

            //DataTable dtDw = null;
            DataView dvDw = new DataView();

            string sGovSlrSDFormulaID = "";
            string sGovtSalaryHeadID = "";
            string sTemp = "";
            string sGovtTemp = "";
            string sGovtFormulaValue = "";

            string strFormulaIDTemp = sFormulaID.Trim();
            string sLocalCurrencyID = para.LocalCurrencyID;
            string sForeignCurRate = para.ForeignCurRate;

            decimal MinWagSalaryFormulaResult = 0;
            //decimal OrgnalFormulaResult = 0;

            //int DaysInMonth = 0;
            //int TotWorkingDay = 0;
            decimal DaysInMonth = 0;
            decimal TotWorkingDay = 0;

            #endregion

            try
            {
                #region Salary Structure Wise Salary

                dsOrgSlr = new DataSet();

                sFormulaValue = "";
                strFormulaIDTemp = strFormulaIDTemp.Replace("(", " ( ");
                strFormulaIDTemp = strFormulaIDTemp.Replace(")", " ) ");
                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dtOrgSlr = new DataTable();
                dtOrgSlr.TableName = "IDLIST";
                dtOrgSlr.Columns.Add("ID");
                DataRow drOrgSlr = null;
                foreach (string id in strIdCol)
                {
                    drOrgSlr = dtOrgSlr.NewRow();
                    drOrgSlr["ID"] = id.Trim();
                    dtOrgSlr.Rows.Add(drOrgSlr);
                }
                dsOrgSlr.Tables.Add(dtOrgSlr);

                for (int i = 0; i < dsOrgSlr.Tables[0].Rows.Count; i++)
                {
                    sTemp = "";

                    sTemp = dsOrgSlr.Tables[0].Rows[i]["ID"].ToString();
                    if (sTemp.Trim() == "+" || sTemp.Trim() == "-" || sTemp.Trim() == "*" || sTemp.Trim() == "/" || sTemp.Trim() == "(" || sTemp.Trim() == ")")
                    {
                        sTemp = dsOrgSlr.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvOrgSlr = new DataView();
                        dvOrgSlr.Table = dtValue;

                        dvOrgSlr.RowFilter = "SalaryHeadID = '" + sTemp.Trim() + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                        if (dvOrgSlr.Count == 1)
                        {
                            if (bEarning == false)
                            {
                                if (dvOrgSlr[0]["EntryCurrencyID"].ToString().Trim() == sLocalCurrencyID.Trim())
                                {
                                    sTemp = dvOrgSlr[0]["EntryAmount"].ToString().Trim();
                                }
                                else
                                {
                                    sTemp = (Convert.ToDecimal(dvOrgSlr[0]["EntryAmount"].ToString().Trim()) * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                }
                            }
                            else
                            {
                                decimal decAmount = Convert.ToDecimal(dvOrgSlr[0]["EarningAmount"].ToString().Trim());

                                if (decAmount == 0)
                                { decAmount = Convert.ToDecimal(dvOrgSlr[0]["EntryAmount"].ToString().Trim()); }

                                if (dvOrgSlr[0]["EarningCurrencyID"].ToString().Trim() == sLocalCurrencyID.Trim())
                                {
                                    sTemp = dvOrgSlr[0]["EarningAmount"].ToString().Trim();
                                }
                                else
                                {
                                    sTemp = (decAmount * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                }
                            }
                        }
                        else
                        {
                            dvSlrHd = new DataView();
                            dvSlrHd.Table = dtSlrHd;
                            dvSlrHd.RowFilter = "SalaryHeadID = '" + sTemp.Trim() + "'";
                            if (dvSlrHd.Count == 1)
                            {
                                sTemp = "0.00";
                            }
                        }
                        if (bIsMinWages == true)
                        {
                            #region Govt.Grad Wise Salary

                            sGovtFormulaValue = "";

                            dvSlrGrd = new DataView();
                            dvSlrGrd.Table = dtSlrGrd;
                            dvSlrGrd.RowFilter = "SalaryRuleMasterSystemID = '" + sSalaryRuleMasterSystemID + "'";
                            if (dvSlrGrd.Count > 0)
                            {
                                sGovtSalaryHeadID = dvSlrGrd[0]["GovtSalaryHeadID"].ToString();
                                sGovSlrSDFormulaID = "";
                                dtIdList(sGovtSalaryHeadID, out dsGovSlrSD);
                                for (int iGvg = 0; iGvg < dsGovSlrSD.Tables[0].Rows.Count; iGvg++)
                                {
                                    if (sGovSlrSDFormulaID == "")
                                    {
                                        sGovSlrSDFormulaID = dsGovSlrSD.Tables[0].Rows[iGvg]["ID"].ToString();
                                    }
                                    else
                                    {
                                        sGovSlrSDFormulaID += " + " + dsGovSlrSD.Tables[0].Rows[iGvg]["ID"].ToString();
                                    }
                                }
                            }

                            string[] strIdColGovSlr = sGovSlrSDFormulaID.Split(' ');

                            DataTable dtGovSlr = new DataTable();
                            dtGovSlr.TableName = "IDLIST";
                            dtGovSlr.Columns.Add("ID");
                            DataRow drGovSlr = null;
                            foreach (string id in strIdColGovSlr)
                            {
                                drGovSlr = dtGovSlr.NewRow();
                                drGovSlr["ID"] = id.Trim();
                                dtGovSlr.Rows.Add(drGovSlr);
                            }
                            dsGovSlr = new DataSet();
                            dsGovSlr.Tables.Add(dtGovSlr);

                            for (int j = 0; j < dsGovSlr.Tables[0].Rows.Count; j++)
                            {
                                sGovtTemp = "";

                                sGovtTemp = dsGovSlr.Tables[0].Rows[j]["ID"].ToString();
                                if (sGovtTemp.Trim() == "+" || sGovtTemp.Trim() == "-" || sGovtTemp.Trim() == "*" || sGovtTemp.Trim() == "/" || sGovtTemp.Trim() == "(" || sGovtTemp.Trim() == ")")
                                {
                                    sGovtTemp = dsGovSlr.Tables[0].Rows[j]["ID"].ToString();
                                }
                                else
                                {
                                    dvGovSlr = new DataView();
                                    dvGovSlr.Table = dtMinWagSalary;

                                    dvGovSlr.RowFilter = "SalaryHeadID = '" + sGovtTemp.Trim() + "' AND DesignationId = '" + sEmpGivenDesignationId + "' AND EmployeeLocationID = '" + sEmpLocationID + "'";
                                    if (dvGovSlr.Count == 1)
                                    {
                                        if (bEarning == false)
                                        {
                                            sGovtTemp = dvGovSlr[0]["SalaryHeadValue"].ToString().Trim();
                                        }
                                        else
                                        {
                                            decimal decAmount = 0;
                                            dvDw.Table = dtDw;
                                            dvDw.RowFilter = "EmpSystemID = '" + sEmpSystemID + "'";
                                            if (dvDw.Count > 0)
                                            {
                                                //DaysInMonth = Convert.ToInt32(dvDw[0].Row["DaysInMonth"].ToString());
                                                //TotWorkingDay = Convert.ToInt32(dvDw[0].Row["TotWorkingDay"].ToString());
                                                DaysInMonth = Convert.ToDecimal(dvDw[0].Row["DaysInMonth"].ToString());
                                                TotWorkingDay = Convert.ToDecimal(dvDw[0].Row["TotWorkingDay"].ToString());
                                            }
                                            if (DaysInMonth > 0)
                                            {
                                                decAmount = (Convert.ToDecimal(dvGovSlr[0]["SalaryHeadValue"].ToString().Trim()) / DaysInMonth) * TotWorkingDay;
                                            }
                                            //sGovtTemp = dvGovSlr[0]["SalaryHeadValue"].ToString().Trim();
                                            sGovtTemp = decAmount.ToString();
                                        }
                                    }
                                    else
                                    {
                                        dvSlrHd = new DataView();
                                        dvSlrHd.Table = dtSlrHd;
                                        dvSlrHd.RowFilter = "SalaryHeadID = '" + sGovtTemp.Trim() + "'";
                                        if (dvSlrHd.Count == 1)
                                        {
                                            sGovtTemp = "0.00";
                                            //*****************Modify 04-OCt-2018************************
                                            //Exception ex = new Exception("Salary Head '"+ dvSlrGrd.Table.Rows[0]["SalaryHead"] + "' not found in employee location wise legal salary structure...");
                                            //throw (ex);
                                            //*****************Modify 04-OCt-2018************************
                                        }
                                    }
                                }

                                sGovtFormulaValue += sGovtTemp.Trim();
                            }
                            if (sGovtFormulaValue != "")
                            {
                                MinWagSalaryFormulaResult = Convert.ToDecimal(Evaluate(sGovtFormulaValue.Trim()));
                            }

                            #endregion
                        }

                        if (sCompMinWagesAndOrginal == "Which Ever is Less")
                        {
                            if (MinWagSalaryFormulaResult < Convert.ToDecimal(sTemp))
                            {
                                sTemp = MinWagSalaryFormulaResult.ToString();
                            }
                        }
                        else if (sCompMinWagesAndOrginal == "Which Ever is More")
                        {
                            if (MinWagSalaryFormulaResult > Convert.ToDecimal(sTemp))
                            {
                                sTemp = MinWagSalaryFormulaResult.ToString();
                            }
                        }
                    }

                    sFormulaValue += sTemp.Trim();
                }

                //OrgnalFormulaResult = Convert.ToDecimal(Evaluate(sFormulaValue.Trim()));

                #endregion
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 
        private void ReLoadFormulaWithValue(string sEmpSystemID, BnsParaListNew para, string sFormulaID, bool bEarning, ref DataTable dtValue, ref DataTable dtSlrHd)
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

                        dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                        if (dvLocal.Count == 1)
                        {
                            if (bEarning == false)
                            {
                                if (dvLocal[0]["EntryCurrencyID"].ToString().Trim() == sLocalCurrencyID.Trim())
                                {
                                    strTemp = dvLocal[0]["EntryAmount"].ToString().Trim();
                                }
                                else
                                {
                                    strTemp = (Convert.ToDecimal(dvLocal[0]["EntryAmount"].ToString().Trim()) * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                }
                            }
                            else
                            {
                                decimal decAmount = Convert.ToDecimal(dvLocal[0]["EarningAmount"].ToString().Trim());

                                if (decAmount == 0)
                                { decAmount = Convert.ToDecimal(dvLocal[0]["EntryAmount"].ToString().Trim()); }

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

        private void xReLoadFormulaWithValue(string sEmpSystemID, BnsParaListNew para, string sFormulaID, bool bEarning, ref DataTable dtValue, ref DataTable dtSlrHd)
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

                        dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                        if (dvLocal.Count == 1)
                        {
                            if (bEarning == false)
                            {
                                if (dvLocal[0]["EntryCurrencyID"].ToString().Trim() == sLocalCurrencyID.Trim())
                                {
                                    strTemp = dvLocal[0]["EntryAmount"].ToString().Trim();
                                }
                                else
                                {
                                    strTemp = (Convert.ToDecimal(dvLocal[0]["EntryAmount"].ToString().Trim()) * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                }
                            }
                            else
                            {
                                decimal decAmount = Convert.ToDecimal(dvLocal[0]["EarningAmount"].ToString().Trim());

                                if (decAmount == 0)
                                { decAmount = Convert.ToDecimal(dvLocal[0]["EntryAmount"].ToString().Trim()); }

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
        private void dtIdList(string strIdCollection, out DataSet dsLocal)
        {
            dsLocal = new DataSet();

            strIdCollection = strIdCollection.Replace("'", "");
            string[] strIdCol = strIdCollection.Split(',');

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
        }//End Function
        public void LoadEmpSlrDefForSlrProcess(BnsParaListNew para, string sEmpInfo, out DataSet dsRef)
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
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay, --SlrDis.IsBankPayment, SlrDis.IsCashPayment,
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
			                            INNER JOIN SalaryHead SH ON SD.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax'
			                            INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
			                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SD.SalaryHeadID = CRC.SalaryHeadID
			                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
			                            LEFT JOIN 
					                            (
                                                 SELECT SalaryRuleMasterSystemID, srg.SalaryHeadID, 'Gen' RuleType, sh.PartOfNetPay IsNetPayEffect, FixedMonthDayValue, IsMonthDay, ---ISNULL(IsBankPayment, Convert(bit, 'True')) IsBankPayment, ISNULL(IsCashPayment, Convert(bit, 'True')) IsCashPayment,
						                                IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleGeneral srg
						                                LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = srg.SalaryHeadID
						                            UNION
					                             (
                                                  SELECT SalaryRuleMasterSystemID, srg.SalaryHeadID, 'Abs' RuleType, sh.PartOfNetPay IsNetPayEffect, FixedMonthDayValue, IsMonthDay, ---Convert(bit, 'True') IsBankPayment, Convert(bit, 'True') IsCashPayment, 
						                                 IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleAbsenteeism srg
						                                LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = srg.SalaryHeadID
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
                                        WHERE E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
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
        public void xLoadEmpSlrDefForSlrProcess(BnsParaListNew para, string sEmpInfo, out DataSet dsRef)
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
			                            INNER JOIN SalaryHead SH ON SD.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax'
			                            INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
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
                                        WHERE E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
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
        public void LoadCurrencyRule(BnsParaListNew para, out System.Data.DataSet dsRef)
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
                            WHERE CRM.GroupID = '" + para.GroupID + @"' AND CRM.PlantId = '" + para.PlantID + @"'";

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
        public void GeneratorBonusEligibleEmployeeForSalaryStracture(BnsParaListNew para, out DataTable dtBnsMntEmpWiseCalSt, out DataTable dtBnsMntDistSt)
        {
            #region Variable Dataset
            DataTable dtValue = null;
            //DataSet dsBnsEligibleEmp = null;
            //DataTable dtBnsEligibleEmp = null;
            //DataRow drBnsEligibleEmp = null;
            //DataView dvBnsEligibleEmp = null;

            DataSet dsBnsMntEmpWiseCal = null;
            DataTable dtBnsMntEmpWiseCal = null;
            DataRow drBnsMntEmpWiseCal = null;
            DataView dvBnsMntEmpWiseCal = null;
            ///==================

            dtBnsMntEmpWiseCal = new DataTable();
            dtBnsMntEmpWiseCal.TableName = "TempTableBonusPolicyMonthlyRetainEmpWiseCalculation";
            dtBnsMntEmpWiseCal.Columns.Add("ID");
            dtBnsMntEmpWiseCal.Columns.Add("EmpSystemID");
            dtBnsMntEmpWiseCal.Columns.Add("BnsPlcMthRetainID");
            dtBnsMntEmpWiseCal.Columns.Add("SlrProcMstSystemID");
            dtBnsMntEmpWiseCal.Columns.Add("MonthNo");
            dtBnsMntEmpWiseCal.Columns.Add("YearNo");
            dtBnsMntEmpWiseCal.Columns.Add("EarningAmount");
            dtBnsMntEmpWiseCal.Columns.Add("AddedBy");
            dtBnsMntEmpWiseCal.Columns.Add("AddedDate");
            dtBnsMntEmpWiseCal.Columns.Add("AddedFromIP");
            dtBnsMntEmpWiseCal.Columns.Add("UpdatedBy");
            dtBnsMntEmpWiseCal.Columns.Add("UpdatedDate");
            dtBnsMntEmpWiseCal.Columns.Add("UpdatedFromIP");






            //DataSet dsBnsMntDist = null;
            DataTable dtBnsMntDist = null;
            DataRow drBnsMntDist = null;
            DataView dvBnsMntDist = null;

            dtBnsMntDist = new DataTable();
            dtBnsMntDist.TableName = "TempTableBonusPolicyMonthlyRetainDistributionPmt";
            dtBnsMntDist.Columns.Add("ID");
            dtBnsMntDist.Columns.Add("BnsPlyMntRetainID");
            dtBnsMntDist.Columns.Add("SalaryHeadID");
            dtBnsMntDist.Columns.Add("Value");


            //DataSet 
            //dsBnsMntEmpWiseCalSt = null;
            //DataTable dtBnsMntEmpWiseCalSt = null;
            DataRow drBnsMntEmpWiseCalSt = null;
            DataView dvBnsMntEmpWiseCalSt = null;

            dtBnsMntEmpWiseCalSt = new DataTable();
            dtBnsMntEmpWiseCalSt.TableName = "TempTableBonusPolicyMonthlyRetainStrcEmpWiseCalculation";
            dtBnsMntEmpWiseCalSt.Columns.Add("ID");
            dtBnsMntEmpWiseCalSt.Columns.Add("EmpSystemID");
            dtBnsMntEmpWiseCalSt.Columns.Add("SlrProcMstSystemID");
            dtBnsMntEmpWiseCalSt.Columns.Add("BnsPlcMthRetainID");
            dtBnsMntEmpWiseCalSt.Columns.Add("MonthNo");
            dtBnsMntEmpWiseCalSt.Columns.Add("YearNo");
            dtBnsMntEmpWiseCalSt.Columns.Add("EarningAmount");
            dtBnsMntEmpWiseCalSt.Columns.Add("AddedBy");
            dtBnsMntEmpWiseCalSt.Columns.Add("AddedDate");
            dtBnsMntEmpWiseCalSt.Columns.Add("AddedFromIP");
            dtBnsMntEmpWiseCalSt.Columns.Add("UpdatedBy");
            dtBnsMntEmpWiseCalSt.Columns.Add("UpdatedDate");
            dtBnsMntEmpWiseCalSt.Columns.Add("UpdatedFromIP");


            //DataSet 
            //dsBnsMntDistSt = null;
            //DataTable dtBnsMntDistSt = null;
            DataRow drBnsMntDistSt = null;
            DataView dvBnsMntDistSt = null;

            dtBnsMntDistSt = new DataTable();
            dtBnsMntDistSt.TableName = "TempTableBonusPolicyMonthlyRetainDistributionStrcPmt";
            dtBnsMntDistSt.Columns.Add("ID");
            dtBnsMntDistSt.Columns.Add("BnsPlyMntRetainID");
            dtBnsMntDistSt.Columns.Add("SalaryHeadID");
            dtBnsMntDistSt.Columns.Add("Value");




            DataSet dsSalHd = null;
            DataTable dtSalHd = null;

            DataSet dsSalInfo = null;
            DataSet dsMinWagSalary = null;
            DataTable dtMinWagSalary = null;
            DataSet dsBnsPolicyMst = null;
            DataSet dsBnsPolicyDtl = null;
            DataSet dsBnsPolicyDist = null;
            DataTable dtBnsPolicyDist = null;
            DataView dvBnsPolicyDist = null;
            DataSet dsBnsMonthNo = null;
            DataSet dsUnTagEmp = null;

            DataSet dsSlrGrd = null;
            DataTable dtSlrGrd = null;

            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;

            DataTable dtDw = null;
            clsSalaryUtility obSS = new global::clsSalaryUtility();

            #endregion Variable Dataset
            #region Declare Variable

            string sBnsEligibleEmpID = "";
            string sBnsMntEmpCalID = "";
            string sBnsMntEmpCalStID = "";
            string sBnsMstID = "";
            string sBnsDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sBnsElgGentID = "";
            string sBnsDedGentID = "";
            string sSalaryRuleMasterSystemID = "";

            string sEarningFormulaID = "";
            string sEarningFormulaResult = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEmpLocationID = "";
            string sEmpGivenDesignationId = "";
            string sFormulaDesID = "";
            string sBonusPolicyDetailsID = "";
            string sCompMinWagesAndOrginal = "";
            string sCurrencyRuleSystemID = "";
            string sRoundOption = "";

            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iDecimalNo = 0;

            DateTime dtStartDate;
            DateTime dtEndDate;

            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValue = 0;
            decimal decEmpCtbtnAmount = 0;

            bool bCalculatedBns = false;
            bool bMandatory = false;
            bool bApplicable = false;
            bool bEligible = false;
            bool bIsActive = true;
            bool bIsFixed = false;
            bool bIsFormula = false;
            bool bIsDependOnEarning = false;
            bool bIsMinWages = false;
            bool bEarning = false;
            bool bIsEligibleApp = true;
            bool bIsAllEmpApplocable = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;

            #endregion Declare Variable

            try
            {
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetDesignationMasterWiseMinSalary(para, out dsMinWagSalary);
                dtMinWagSalary = dsMinWagSalary.Tables[0];

                dtDw = para.dsDw.Tables[0];

                GetBonusPolicyMonthlyRetainMaster(sGroupID.Trim(), sPlantID.Trim(), out dsBnsPolicyMst);

                if (dsBnsPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int BnsPlCnt = 0; BnsPlCnt < dsBnsPolicyMst.Tables[0].Rows.Count; BnsPlCnt++)
                    {
                        sBnsMstID = dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["ID"].ToString().Trim();
                        bIsAllEmpApplocable = Convert.ToBoolean(dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["IsAllEmpApplocable"].ToString().Trim());

                        #region DataSet

                        GetBonusPolicyMonthlyRetainDetails(sBnsMstID, out dsBnsPolicyDtl);
                        GetBonusPolicyMonthlyRetainMonthNo(sBnsMstID, out dsBnsMonthNo);
                        GetBonusPolicyMonthlyRetainDistribution(sBnsMstID, out dsBnsPolicyDist);
                        dtBnsPolicyDist = dsBnsPolicyDist.Tables[0];
                        dvBnsPolicyDist = new DataView();

                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];

                        GetSalaryRuleGovtGrd(para, out dsSlrGrd);
                        dtSlrGrd = dsSlrGrd.Tables[0];

                        #endregion DataSet

                        #region Tag Employee List

                        GetTagEmployeeListWithBonusPolicyMonthlyRetain(para, sBnsMstID.Trim(), out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            para.IsBonusRtnPolicyDefined = true;
                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {

                                    dvBnsMntEmpWiseCal = new DataView();

                                    //GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                                    //dtBnsMntDist = dsBnsMntDist.Tables[0];
                                    dvBnsMntDist = new DataView();

                                    if (para.bStructure == true)
                                    {
                                        //GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                                        //dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                                        dvBnsMntEmpWiseCalSt = new DataView();

                                        //GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                                        //dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                                        dvBnsMntDistSt = new DataView();
                                    }

                                    //GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                                    //dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                                    //dvBnsEligibleEmp = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                    sBnsElgGentID = "";
                                    sBnsDedGentID = "";
                                    sBnsElgGentID = "BNSE" + sBnsElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                                    sBnsDedGentID = "BNSC" + sBnsDedGentID;
                                    for (int iUnTgEmCnt = 0; iUnTgEmCnt < dsUnTagEmp.Tables[0].Rows.Count; iUnTgEmCnt++)
                                    {
                                        sBnsEligibleEmpID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["BnsEligibleEmpID"].ToString().Trim();//BnsEligibleEmpID
                                        //sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();//BnsEligibleEmpID
                                        //sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                        sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                                        //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                                        sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();

                                        #region Master Table Data Capture [Start Date]

                                        dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                        #endregion Master Table Data Capture 

                                        string sMatDt = "";
                                        //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                        if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                                        {
                                            for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                                            {
                                                sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                                                {
                                                    bIsEligibleApp = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bIsEligibleApp = true;

                                        }
                                        para.IsBonusMandatory = bIsEligibleApp;
                                        if (bIsEligibleApp == true)
                                        {
                                            var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                            if (dicSalInfo_Sub.Count > 0)
                                            {
                                                //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                //{
                                                sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                //}
                                            }
                                            else
                                            {
                                                sSalaryRuleMasterSystemID = para.sSalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = para.sCurrencyRuleSystemID;
                                            }

                                            #region Salary Amount Insert Into Virtual Table

                                            dtValue = new DataTable();
                                            dtValue.TableName = "TempTable";
                                            dtValue.Columns.Add("EmpSystemID");
                                            dtValue.Columns.Add("SalaryHeadID");
                                            dtValue.Columns.Add("EntryCurrencyID");
                                            dtValue.Columns.Add("EntryAmount");
                                            dtValue.Columns.Add("EarningCurrencyID");
                                            dtValue.Columns.Add("EarningAmount");

                                            dtValue = para.dsSalInfo.Tables[0];

                                            #endregion Salary Amount Insert Into Virtual Table

                                            if (dtValue.Rows.Count > 0)
                                            {
                                                for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                                                {
                                                    #region Clear

                                                    sBonusPolicyDetailsID = "";
                                                    sFormulaDesID = "";

                                                    decFixedValue = 0;
                                                    decEmpCtbtnAmount = 0;
                                                    decEarningValueRangeFrom = 0;
                                                    decEarningValueRangeTo = 0;

                                                    dtEndDate = System.DateTime.Now;
                                                    bMandatory = false;
                                                    bEarning = false;
                                                    bIsActive = true;
                                                    bIsFixed = false;
                                                    bIsFormula = false;
                                                    bIsDependOnEarning = false;

                                                    #endregion Clear

                                                    #region Select BonusPolicyDetails ID if have multiple column

                                                    sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                                                    sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                    if (dtMinWagSalary.Rows.Count > 0)
                                                    {
                                                        sEarningFormulaResult = dtMinWagSalary.Rows[0]["SalaryHeadValue"].ToString();
                                                    }
                                                    else
                                                    {
                                                        sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                    }
                                                    decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                    decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                                                    if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) <= decEarningValueRangeTo)
                                                    {
                                                        bApplicable = true;
                                                    }
                                                    else
                                                    {
                                                        bApplicable = false;
                                                    }
                                                    bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                                                    sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                                                    sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                                                    decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                                                    bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                                                    bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                                                    bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                                                    bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                                                    sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                                                    #endregion Select BonusPolicyDetails ID if have multiple column

                                                    //if (para.IsbuttonPFClicked == "YES")
                                                    //{


                                                    //    if (para.IsBonusEntitle == true)
                                                    //    {
                                                    //        bApplicable = para.IsBonusEntitle;
                                                    //    }

                                                    //    para.IsBonusMandatory = bMandatory;
                                                    //}
                                                    //else
                                                    //{
                                                    //    // old
                                                    //    if (bIsAllEmpApplocable)
                                                    //    {
                                                    //        bApplicable = true;
                                                    //        para.IsBonusEntitle = true;
                                                    //    }

                                                    //    if (bMandatory)
                                                    //    {
                                                    //        bApplicable = true;
                                                    //        para.IsBonusEntitle = true;
                                                    //    }
                                                    //    para.IsBonusMandatory = bMandatory;
                                                    //    if (bMandatory == false)
                                                    //    {
                                                    //        //para.IsPFOptionalNew = true;
                                                    //    }
                                                    //}







                                                    if (bApplicable == true)
                                                    {
                                                        #region Bonus Earning Amount
                                                        if (para.IsbuttonPFClicked == "YES")
                                                        {


                                                            if (para.IsBonusEntitle == true)
                                                            {
                                                                //bApplicable = para.IsBonusEntitle;
                                                            }
                                                            if (bMandatory)
                                                            {
                                                                //bApplicable = true;
                                                                para.IsBonusEntitle = true;
                                                            }
                                                            para.IsBonusMandatory = bMandatory;
                                                        }
                                                        else
                                                        {
                                                            // old
                                                            if (bIsAllEmpApplocable)
                                                            {
                                                                //bApplicable = true;
                                                                para.IsBonusEntitle = true;
                                                            }

                                                            if (bMandatory)
                                                            {
                                                                //bApplicable = true;
                                                                para.IsBonusEntitle = true;
                                                            }
                                                            para.IsBonusMandatory = bMandatory;
                                                            if (bMandatory == false)
                                                            {
                                                                //para.IsPFOptionalNew = true;
                                                            }
                                                        }





                                                        if (bIsFixed == true)
                                                        {
                                                            decEmpCtbtnAmount = decFixedValue;
                                                        }
                                                        else if (bIsFormula == true)
                                                        {
                                                            bEarning = bIsDependOnEarning;
                                                            ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                                                            sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                            decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                                                        }

                                                        #endregion Bonus Earning Amount

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                                                        //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                        if (dvBnsMntEmpWiseCal.Count == 0)
                                                        {//Add new block
                                                            sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                            drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                                                        }
                                                        else
                                                        {//edit block
                                                            sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                                                            drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                                                            drBnsMntEmpWiseCal.BeginEdit();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            drBnsMntEmpWiseCal.EndEdit();
                                                        }

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                                                            dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                            if (dvBnsMntEmpWiseCalSt.Count == 0)
                                                            {//Add new block
                                                                sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                                                            }
                                                            else
                                                            {//edit block
                                                                sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                                                                drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                                                                drBnsMntEmpWiseCalSt.BeginEdit();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                drBnsMntEmpWiseCalSt.EndEdit();
                                                            }
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        //by monir
                                                        //dvBnsMntDist.Table = dtBnsMntDist;
                                                        //dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        //if (dvBnsMntDist.Count > 0)
                                                        //{
                                                        //    while (dvBnsMntDist.Count > 0)
                                                        //    {
                                                        //        drBnsMntDist = dvBnsMntDist[0].Row;
                                                        //        drBnsMntDist.Delete();
                                                        //    }
                                                        //}

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                                                            if (dvBnsMntDistSt.Count > 0)
                                                            {
                                                                while (dvBnsMntDistSt.Count > 0)
                                                                {
                                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                    drBnsMntDistSt.Delete();
                                                                }
                                                            }
                                                        }

                                                        #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        dvBnsPolicyDist.Table = dtBnsPolicyDist;
                                                        dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                                                        if (dvBnsPolicyDist.Count > 0)
                                                        {
                                                            for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                                                            {
                                                                decimal decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }
                                                                    }
                                                                }

                                                                decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue1 = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue1);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue1);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }

                                                                    }
                                                                }
                                                            }
                                                        }
                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                                                        //dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                                                        //dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                                                        //if (dvBnsEligibleEmp.Count == 1)
                                                        //{//Add new block
                                                        //    drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                                                        //    UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                                                        //    drBnsEligibleEmp.EndEdit();
                                                        //}

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                }

                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                if (para.bStructure == true)
                                {
                                    ///SaveDataSets(dsBnsMntEmpWiseCal,  dsBnsMntEmpWiseCalSt, dsBnsMntDistSt); 
                                }
                                else
                                {
                                    //SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist);*******
                                }
                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                //dsBnsEligibleEmp = null;
                                dsBnsMntEmpWiseCal = null;
                            }

                        }
                        else
                        {
                            para.IsBonusRtnPolicyDefined = true;
                        }

                        #endregion Tag Employee List


                        #region offfffff
                        //if (para.ShouldNotProcessUntaggedEmp == false)
                        //{
                        //    #region Untag Employee List

                        //    GetUnTagEmployeeListWithBonusPolicyMonthlyRetain(para, out dsUnTagEmp);
                        //    if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        //    {
                        //        sEmpInfoSysIDColl = "";
                        //        sEmpSystemID = "";
                        //        TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                        //        TotProcComp = 0;
                        //        grdRowMaxCnt = 0;
                        //        SelectedEmpCnt = 0;
                        //        EmpCntForLoop = 0;

                        //        while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                        //        {
                        //            sEmpInfoSysIDColl = "";
                        //            sEmpSystemID = "";
                        //            EmpCntForLoop = 0;

                        //            if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                        //            {
                        //                grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                        //            }
                        //            else
                        //            {
                        //                grdRowMaxCnt = 30;
                        //            }

                        //            #region Employee System ID Collection

                        //            for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                        //            {
                        //                if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                        //                {
                        //                    sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                        //                    sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                        //                }
                        //                else
                        //                {
                        //                    sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                        //                    sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                        //                }
                        //                EmpCntForLoop++;
                        //            }

                        //            #endregion Employee System ID Collection

                        //            if (EmpCntForLoop == grdRowMaxCnt)
                        //            {
                        //                GetBonusPolicyMonthlyRetainEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCal);
                        //                dtBnsMntEmpWiseCal = dsBnsMntEmpWiseCal.Tables[0];
                        //                dvBnsMntEmpWiseCal = new DataView();

                        //                GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                        //                dtBnsMntDist = dsBnsMntDist.Tables[0];
                        //                dvBnsMntDist = new DataView();

                        //                if (para.bStructure == true)
                        //                {
                        //                    GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                        //                    dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                        //                    dvBnsMntEmpWiseCalSt = new DataView();

                        //                    GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                        //                    dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                        //                    dvBnsMntDistSt = new DataView();
                        //                }

                        //                GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                        //                dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                        //                dvBnsEligibleEmp = new DataView();

                        //                //Get General Salary Amount Head Wise
                        //                List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                        //                LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                        //                if (dsSalInfo.Tables[0].Rows.Count > 0)
                        //                    dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                        //                sBnsElgGentID = "";
                        //                sBnsDedGentID = "";
                        //                GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_ELIGIBLE", dsUnTagEmp.Tables[0].Rows.Count, out sBnsElgGentID);
                        //                sBnsElgGentID = "BNSE" + sBnsElgGentID;

                        //                GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                        //                sBnsDedGentID = "BNSC" + sBnsDedGentID;
                        //                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                        //                {
                        //                    sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                        //                    sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                        //                    sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                        //                    //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                        //                    sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();
                        //                    bCalculatedBns = false;

                        //                    #region Master Table Data Capture [Start Date]

                        //                    dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                        //                    #endregion Master Table Data Capture 

                        //                    string sMatDt = "";
                        //                    //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                        //                    if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                        //                    {
                        //                        for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                        //                        {
                        //                            sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                        //                            if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                        //                            {
                        //                                bIsEligibleApp = false;
                        //                            }
                        //                        }
                        //                    }
                        //                    else
                        //                    {
                        //                        bIsEligibleApp = true;
                        //                    }

                        //                    if (bIsEligibleApp == true)
                        //                    {
                        //                        var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                        //                        if (dicSalInfo_Sub.Count > 0)
                        //                        {
                        //                            //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                        //                            //{
                        //                            sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                        //                            sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                        //                            //}
                        //                        }

                        //                        #region Salary Amount Insert Into Virtual Table

                        //                        dtValue = new DataTable();
                        //                        dtValue.TableName = "TempTable";
                        //                        dtValue.Columns.Add("EmpSystemID");
                        //                        dtValue.Columns.Add("SalaryHeadID");
                        //                        dtValue.Columns.Add("EntryCurrencyID");
                        //                        dtValue.Columns.Add("EntryAmount");
                        //                        dtValue.Columns.Add("EarningCurrencyID");
                        //                        dtValue.Columns.Add("EarningAmount");

                        //                        dtValue = para.dsSalInfo.Tables[0];

                        //                        #endregion Salary Amount Insert Into Virtual Table

                        //                        if (dtValue.Rows.Count > 0)
                        //                        {
                        //                            for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                        //                            {
                        //                                #region Clear

                        //                                sBonusPolicyDetailsID = "";
                        //                                sFormulaDesID = "";

                        //                                decFixedValue = 0;
                        //                                decEmpCtbtnAmount = 0;
                        //                                decEarningValueRangeFrom = 0;
                        //                                decEarningValueRangeTo = 0;

                        //                                dtEndDate = System.DateTime.Now;
                        //                                bMandatory = false;
                        //                                bEarning = false;
                        //                                bIsActive = true;
                        //                                bIsFixed = false;
                        //                                bIsFormula = false;
                        //                                bIsDependOnEarning = false;

                        //                                #endregion Clear

                        //                                #region Select BonusPolicyDetails ID if have multiple column

                        //                                sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                        //                                sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                        //                                ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                        //                                sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                        //                                decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                        //                                decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                        //                                if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) < decEarningValueRangeTo)
                        //                                {
                        //                                    bEligible = true;
                        //                                    bApplicable = true;
                        //                                    bCalculatedBns = true;
                        //                                }
                        //                                else
                        //                                {
                        //                                    bApplicable = false;
                        //                                }

                        //                                #region If Bonus applicable for all employee

                        //                                if (bIsAllEmpApplocable == true && iBnsPlDtl == dsBnsPolicyDtl.Tables[0].Rows.Count && bApplicable == false && bCalculatedBns == false)
                        //                                {
                        //                                    bApplicable = true;
                        //                                }

                        //                                #endregion If Bonus applicable for all employee

                        //                                bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                        //                                sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                        //                                sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                        //                                decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                        //                                bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                        //                                bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                        //                                bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                        //                                bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                        //                                sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                        //                                #endregion Select BonusPolicyDetails ID if have multiple column

                        //                                if (bEligible == true)
                        //                                {
                        //                                    if (bApplicable == true)
                        //                                    {
                        //                                        #region Bonus Earning Amount

                        //                                        if (bIsFixed == true)
                        //                                        {
                        //                                            decEmpCtbtnAmount = decFixedValue;
                        //                                        }
                        //                                        else if (bIsFormula == true)
                        //                                        {
                        //                                            bEarning = bIsDependOnEarning;
                        //                                            ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                        //                                            sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                        //                                            decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                        //                                        }

                        //                                        #endregion Bonus Earning Amount

                        //                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                        //                                        dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                        //                                        //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                        //                                        dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                        //                                        if (dvBnsMntEmpWiseCal.Count == 0)
                        //                                        {//Add new block
                        //                                            sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                        //                                            drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                        //                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                        //                                            dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                        //                                        }
                        //                                        else
                        //                                        {//edit block
                        //                                            sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                        //                                            drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                        //                                            drBnsMntEmpWiseCal.BeginEdit();
                        //                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                        //                                            drBnsMntEmpWiseCal.EndEdit();
                        //                                        }

                        //                                        if (para.bStructure == true)
                        //                                        {
                        //                                            dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                        //                                            dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                        //                                            if (dvBnsMntEmpWiseCalSt.Count == 0)
                        //                                            {//Add new block
                        //                                                sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                        //                                                drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                        //                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                        //                                                dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                        //                                            }
                        //                                            else
                        //                                            {//edit block
                        //                                                sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                        //                                                drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                        //                                                drBnsMntEmpWiseCalSt.BeginEdit();
                        //                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                        //                                                drBnsMntEmpWiseCalSt.EndEdit();
                        //                                            }
                        //                                        }

                        //                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                        //                                        #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                        //                                        dvBnsMntDist.Table = dtBnsMntDist;
                        //                                        dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                        //                                        if (dvBnsMntDist.Count > 0)
                        //                                        {
                        //                                            while (dvBnsMntDist.Count > 0)
                        //                                            {
                        //                                                drBnsMntDist = dvBnsMntDist[0].Row;
                        //                                                drBnsMntDist.Delete();
                        //                                            }
                        //                                        }

                        //                                        if (para.bStructure == true)
                        //                                        {
                        //                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                        //                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                        //                                            if (dvBnsMntDistSt.Count > 0)
                        //                                            {
                        //                                                while (dvBnsMntDistSt.Count > 0)
                        //                                                {
                        //                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                        //                                                    drBnsMntDistSt.Delete();
                        //                                                }
                        //                                            }
                        //                                        }

                        //                                        #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                        //                                        #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                        //                                        dvBnsPolicyDist.Table = dtBnsPolicyDist;
                        //                                        dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                        //                                        if (dvBnsPolicyDist.Count > 0)
                        //                                        {
                        //                                            for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                        //                                            {
                        //                                                decimal decBonusValue = 0;
                        //                                                if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                        //                                                {
                        //                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                        //                                                    dvCurRl.Table = dtCurRl;
                        //                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                        //                                                    if (dvCurRl.Count > 0)
                        //                                                    {
                        //                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                        //                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                        //                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                        //                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                        //                                                    }
                        //                                                    string sOutValue = "0";
                        //                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                        //                                                    decBonusValue = Convert.ToDecimal(sOutValue);

                        //                                                    dvBnsMntDist.Table = dtBnsMntDist;
                        //                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                        //                                                    if (dvBnsMntDist.Count == 0)
                        //                                                    {
                        //                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                        //                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                        //                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                        //                                                        drBnsMntDist["Value"] = decBonusValue;
                        //                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                        //                                                    }
                        //                                                    else
                        //                                                    {
                        //                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                        //                                                        drBnsMntDist.BeginEdit();
                        //                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                        //                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                        //                                                        drBnsMntDist["Value"] = decBonusValue;
                        //                                                        drBnsMntDist.EndEdit();
                        //                                                    }

                        //                                                    if (para.bStructure == true)
                        //                                                    {
                        //                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                        //                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                        //                                                        if (dvBnsMntDistSt.Count == 0)
                        //                                                        {
                        //                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                        //                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                        //                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                        //                                                            drBnsMntDistSt["Value"] = decBonusValue;
                        //                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                        //                                                        }
                        //                                                        else
                        //                                                        {
                        //                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                        //                                                            drBnsMntDistSt.BeginEdit();
                        //                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                        //                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                        //                                                            drBnsMntDistSt["Value"] = decBonusValue;
                        //                                                            drBnsMntDistSt.EndEdit();
                        //                                                        }
                        //                                                    }
                        //                                                }

                        //                                                decBonusValue = 0;
                        //                                                if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                        //                                                {
                        //                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                        //                                                    dvCurRl.Table = dtCurRl;
                        //                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                        //                                                    if (dvCurRl.Count > 0)
                        //                                                    {
                        //                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                        //                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                        //                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                        //                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                        //                                                    }
                        //                                                    string sOutValue = "0";
                        //                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                        //                                                    decBonusValue = Convert.ToDecimal(sOutValue);

                        //                                                    dvBnsMntDist.Table = dtBnsMntDist;
                        //                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                        //                                                    if (dvBnsMntDist.Count == 0)
                        //                                                    {
                        //                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                        //                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                        //                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                        //                                                        drBnsMntDist["Value"] = decBonusValue;
                        //                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                        //                                                    }
                        //                                                    else
                        //                                                    {
                        //                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                        //                                                        drBnsMntDist.BeginEdit();
                        //                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                        //                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                        //                                                        drBnsMntDist["Value"] = decBonusValue;
                        //                                                        drBnsMntDist.EndEdit();
                        //                                                    }

                        //                                                    if (para.bStructure == true)
                        //                                                    {
                        //                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                        //                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                        //                                                        if (dvBnsMntDistSt.Count == 0)
                        //                                                        {
                        //                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                        //                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                        //                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                        //                                                            drBnsMntDistSt["Value"] = decBonusValue;
                        //                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                        //                                                        }
                        //                                                        else
                        //                                                        {
                        //                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                        //                                                            drBnsMntDistSt.BeginEdit();
                        //                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                        //                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                        //                                                            drBnsMntDistSt["Value"] = decBonusValue;
                        //                                                            drBnsMntDistSt.EndEdit();
                        //                                                        }
                        //                                                    }
                        //                                                }
                        //                                            }
                        //                                        }
                        //                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]
                        //                                    }
                        //                                    #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                        //                                    dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                        //                                    dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                        //                                    if (dvBnsEligibleEmp.Count == 0)
                        //                                    {//Add new block
                        //                                        drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                        //                                        UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                        //                                        dtBnsEligibleEmp.Rows.Add(drBnsEligibleEmp);
                        //                                    }

                        //                                    #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                        //                                }
                        //                            }
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //            //if (SelectedEmpCnt == grdRowMaxCnt)
                        //            //{
                        //            TotProcComp += grdRowMaxCnt;
                        //            TotSelectEmpForProc -= grdRowMaxCnt;
                        //            SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist, dsBnsMntEmpWiseCalSt, dsBnsMntDistSt);
                        //            //}
                        //            if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                        //            {
                        //                SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                        //                if (SelectedEmpCnt <= 0)
                        //                { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                        //            }
                        //            else
                        //            {
                        //                SelectedEmpCnt += 30;
                        //            }
                        //            dsBnsEligibleEmp = null;
                        //            dsBnsMntEmpWiseCal = null;
                        //        }
                        //        //if (bMaturity == true)
                        //        //{
                        //        //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                        //        //}
                        //    }

                        //    #endregion Untag Employee
                        //} 
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Clear DataSet 

                //dsBnsEligibleEmp = null;
                //dtBnsEligibleEmp = null;
                //drBnsEligibleEmp = null;
                //dvBnsEligibleEmp = null;

                dsBnsMntEmpWiseCal = null;
                dtBnsMntEmpWiseCal = null;
                drBnsMntEmpWiseCal = null;
                dvBnsMntEmpWiseCal = null;

                dsSalInfo = null;
                dsBnsPolicyMst = null;
                dsBnsPolicyDtl = null;
                dsBnsMonthNo = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function
        public void GeneratorBonusEligibleEmployee(BnsParaListNew para)
        {
            #region Variable Dataset
            DataTable dtValue = null;
            DataSet dsBnsEligibleEmp = null;
            DataTable dtBnsEligibleEmp = null;
            DataRow drBnsEligibleEmp = null;
            DataView dvBnsEligibleEmp = null;

            DataSet dsBnsMntEmpWiseCal = null;
            DataTable dtBnsMntEmpWiseCal = null;
            DataRow drBnsMntEmpWiseCal = null;
            DataView dvBnsMntEmpWiseCal = null;

            DataSet dsBnsMntDist = null;
            DataTable dtBnsMntDist = null;
            DataRow drBnsMntDist = null;
            DataView dvBnsMntDist = null;

            DataSet dsBnsMntEmpWiseCalSt = null;
            DataTable dtBnsMntEmpWiseCalSt = null;
            DataRow drBnsMntEmpWiseCalSt = null;
            DataView dvBnsMntEmpWiseCalSt = null;

            DataSet dsBnsMntDistSt = null;
            DataTable dtBnsMntDistSt = null;
            DataRow drBnsMntDistSt = null;
            DataView dvBnsMntDistSt = null;

            DataSet dsSalHd = null;
            DataTable dtSalHd = null;

            DataSet dsSalInfo = null;
            DataSet dsMinWagSalary = null;
            DataTable dtMinWagSalary = null;
            DataSet dsBnsPolicyMst = null;
            DataSet dsBnsPolicyDtl = null;
            DataSet dsBnsPolicyDist = null;
            DataTable dtBnsPolicyDist = null;
            DataView dvBnsPolicyDist = null;
            DataSet dsBnsMonthNo = null;
            DataSet dsUnTagEmp = null;

            DataSet dsSlrGrd = null;
            DataTable dtSlrGrd = null;

            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;

            DataTable dtDw = null;
            clsSalaryUtility obSS = new global::clsSalaryUtility();

            #endregion Variable Dataset
            #region Declare Variable

            string sBnsEligibleEmpID = "";
            string sBnsMntEmpCalID = "";
            string sBnsMntEmpCalStID = "";
            string sBnsMstID = "";
            string sBnsDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sBnsElgGentID = "";
            string sBnsDedGentID = "";
            string sSalaryRuleMasterSystemID = "";

            string sEarningFormulaID = "";
            string sEarningFormulaResult = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEmpLocationID = "";
            string sEmpGivenDesignationId = "";
            string sFormulaDesID = "";
            string sBonusPolicyDetailsID = "";
            string sCompMinWagesAndOrginal = "";
            string sCurrencyRuleSystemID = "";
            string sRoundOption = "";

            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iDecimalNo = 0;

            DateTime dtStartDate;
            DateTime dtEndDate;

            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValue = 0;
            decimal decEmpCtbtnAmount = 0;

            bool bCalculatedBns = false;
            bool bMandatory = false;
            bool bApplicable = false;
            bool bEligible = false;
            bool bIsActive = true;
            bool bIsFixed = false;
            bool bIsFormula = false;
            bool bIsDependOnEarning = false;
            bool bIsMinWages = false;
            bool bEarning = false;
            bool bIsEligibleApp = true;
            bool bIsAllEmpApplocable = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;

            #endregion Declare Variable

            try
            {
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetDesignationMasterWiseMinSalary(para, out dsMinWagSalary);
                dtMinWagSalary = dsMinWagSalary.Tables[0];

                dtDw = para.dsDw.Tables[0];

                GetBonusPolicyMonthlyRetainMaster(sGroupID.Trim(), sPlantID.Trim(), out dsBnsPolicyMst);

                if (dsBnsPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int BnsPlCnt = 0; BnsPlCnt < dsBnsPolicyMst.Tables[0].Rows.Count; BnsPlCnt++)
                    {
                        sBnsMstID = dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["ID"].ToString().Trim();
                        bIsAllEmpApplocable = Convert.ToBoolean(dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["IsAllEmpApplocable"].ToString().Trim());

                        #region DataSet

                        GetBonusPolicyMonthlyRetainDetails(sBnsMstID, out dsBnsPolicyDtl);
                        GetBonusPolicyMonthlyRetainMonthNo(sBnsMstID, out dsBnsMonthNo);
                        GetBonusPolicyMonthlyRetainDistribution(sBnsMstID, out dsBnsPolicyDist);
                        dtBnsPolicyDist = dsBnsPolicyDist.Tables[0];
                        dvBnsPolicyDist = new DataView();

                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];

                        GetSalaryRuleGovtGrd(para, out dsSlrGrd);
                        dtSlrGrd = dsSlrGrd.Tables[0];

                        #endregion DataSet

                        #region Tag Employee List

                        GetTagEmployeeListWithBonusPolicyMonthlyRetain(para, sBnsMstID.Trim(), out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {
                                    GetBonusPolicyMonthlyRetainEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCal);
                                    dtBnsMntEmpWiseCal = dsBnsMntEmpWiseCal.Tables[0];
                                    dvBnsMntEmpWiseCal = new DataView();

                                    GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                                    dtBnsMntDist = dsBnsMntDist.Tables[0];
                                    dvBnsMntDist = new DataView();

                                    if (para.bStructure == true)
                                    {
                                        GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                                        dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                                        dvBnsMntEmpWiseCalSt = new DataView();

                                        GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                                        dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                                        dvBnsMntDistSt = new DataView();
                                    }

                                    GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                                    dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                                    dvBnsEligibleEmp = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                    sBnsElgGentID = "";
                                    sBnsDedGentID = "";
                                    sBnsElgGentID = "BNSE" + sBnsElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                                    sBnsDedGentID = "BNSC" + sBnsDedGentID;
                                    for (int iUnTgEmCnt = 0; iUnTgEmCnt < dsUnTagEmp.Tables[0].Rows.Count; iUnTgEmCnt++)
                                    {
                                        sBnsEligibleEmpID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["BnsEligibleEmpID"].ToString().Trim();//BnsEligibleEmpID
                                        //sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();//BnsEligibleEmpID
                                        //sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                        sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                                        //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                                        sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();

                                        #region Master Table Data Capture [Start Date]

                                        dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                        #endregion Master Table Data Capture 

                                        string sMatDt = "";
                                        //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                        if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                                        {
                                            for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                                            {
                                                sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                                                {
                                                    bIsEligibleApp = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bIsEligibleApp = true;
                                        }

                                        if (bIsEligibleApp == true)
                                        {
                                            var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                            if (dicSalInfo_Sub.Count > 0)
                                            {
                                                //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                //{
                                                sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                //}
                                            }
                                            else
                                            {
                                                sSalaryRuleMasterSystemID = para.sSalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = para.sCurrencyRuleSystemID;
                                            }

                                            #region Salary Amount Insert Into Virtual Table

                                            dtValue = new DataTable();
                                            dtValue.TableName = "TempTable";
                                            dtValue.Columns.Add("EmpSystemID");
                                            dtValue.Columns.Add("SalaryHeadID");
                                            dtValue.Columns.Add("EntryCurrencyID");
                                            dtValue.Columns.Add("EntryAmount");
                                            dtValue.Columns.Add("EarningCurrencyID");
                                            dtValue.Columns.Add("EarningAmount");

                                            dtValue = para.dsSalInfo.Tables[0];

                                            #endregion Salary Amount Insert Into Virtual Table

                                            if (dtValue.Rows.Count > 0)
                                            {
                                                for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                                                {
                                                    #region Clear

                                                    sBonusPolicyDetailsID = "";
                                                    sFormulaDesID = "";

                                                    decFixedValue = 0;
                                                    decEmpCtbtnAmount = 0;
                                                    decEarningValueRangeFrom = 0;
                                                    decEarningValueRangeTo = 0;

                                                    dtEndDate = System.DateTime.Now;
                                                    bMandatory = false;
                                                    bEarning = false;
                                                    bIsActive = true;
                                                    bIsFixed = false;
                                                    bIsFormula = false;
                                                    bIsDependOnEarning = false;

                                                    #endregion Clear

                                                    #region Select BonusPolicyDetails ID if have multiple column

                                                    sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                                                    sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                    sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                    decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                    decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                                                    if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) < decEarningValueRangeTo)
                                                    {
                                                        bApplicable = true;
                                                    }
                                                    else
                                                    {
                                                        bApplicable = false;
                                                    }
                                                    bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                                                    sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                                                    sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                                                    decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                                                    bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                                                    bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                                                    bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                                                    bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                                                    sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                                                    #endregion Select BonusPolicyDetails ID if have multiple column

                                                    if (bApplicable == true)
                                                    {
                                                        #region Bonus Earning Amount

                                                        if (bIsFixed == true)
                                                        {
                                                            decEmpCtbtnAmount = decFixedValue;
                                                        }
                                                        else if (bIsFormula == true)
                                                        {
                                                            bEarning = bIsDependOnEarning;
                                                            ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                                                            sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                            decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                                                        }

                                                        #endregion Bonus Earning Amount

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                                                        //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                        if (dvBnsMntEmpWiseCal.Count == 0)
                                                        {//Add new block
                                                            sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                            drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                                                        }
                                                        else
                                                        {//edit block
                                                            sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                                                            drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                                                            drBnsMntEmpWiseCal.BeginEdit();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            drBnsMntEmpWiseCal.EndEdit();
                                                        }

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                                                            dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                            if (dvBnsMntEmpWiseCalSt.Count == 0)
                                                            {//Add new block
                                                                sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                                                            }
                                                            else
                                                            {//edit block
                                                                sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                                                                drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                                                                drBnsMntEmpWiseCalSt.BeginEdit();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                drBnsMntEmpWiseCalSt.EndEdit();
                                                            }
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        //by monir
                                                        //dvBnsMntDist.Table = dtBnsMntDist;
                                                        //dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        //if (dvBnsMntDist.Count > 0)
                                                        //{
                                                        //    while (dvBnsMntDist.Count > 0)
                                                        //    {
                                                        //        drBnsMntDist = dvBnsMntDist[0].Row;
                                                        //        drBnsMntDist.Delete();
                                                        //    }
                                                        //}

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                                                            if (dvBnsMntDistSt.Count > 0)
                                                            {
                                                                while (dvBnsMntDistSt.Count > 0)
                                                                {
                                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                    drBnsMntDistSt.Delete();
                                                                }
                                                            }
                                                        }

                                                        #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        dvBnsPolicyDist.Table = dtBnsPolicyDist;
                                                        dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                                                        if (dvBnsPolicyDist.Count > 0)
                                                        {
                                                            for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                                                            {
                                                                decimal decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }
                                                                    }
                                                                }

                                                                decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue1 = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue1);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue1);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }

                                                                    }
                                                                }
                                                            }
                                                        }
                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                                                        dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                                                        dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                                                        if (dvBnsEligibleEmp.Count == 1)
                                                        {//Add new block
                                                            drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                                                            drBnsEligibleEmp.EndEdit();
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                }

                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                if (para.bStructure == true)
                                {
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist, dsBnsMntEmpWiseCalSt, dsBnsMntDistSt);
                                }
                                else
                                {
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist);
                                }
                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                dsBnsEligibleEmp = null;
                                dsBnsMntEmpWiseCal = null;
                            }
                            //if (bMaturity == true)
                            //{
                            //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                            //}
                        }

                        #endregion Tag Employee List


                        if (para.ShouldNotProcessUntaggedEmp == false)
                        {
                            #region Untag Employee List

                            GetUnTagEmployeeListWithBonusPolicyMonthlyRetain(para, out dsUnTagEmp);
                            if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                                TotProcComp = 0;
                                grdRowMaxCnt = 0;
                                SelectedEmpCnt = 0;
                                EmpCntForLoop = 0;

                                while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    sEmpInfoSysIDColl = "";
                                    sEmpSystemID = "";
                                    EmpCntForLoop = 0;

                                    if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                    {
                                        grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                    }
                                    else
                                    {
                                        grdRowMaxCnt = 30;
                                    }

                                    #region Employee System ID Collection

                                    for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                    {
                                        if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                        {
                                            sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                            sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        }
                                        else
                                        {
                                            sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                            sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        }
                                        EmpCntForLoop++;
                                    }

                                    #endregion Employee System ID Collection

                                    if (EmpCntForLoop == grdRowMaxCnt)
                                    {
                                        GetBonusPolicyMonthlyRetainEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCal);
                                        dtBnsMntEmpWiseCal = dsBnsMntEmpWiseCal.Tables[0];
                                        dvBnsMntEmpWiseCal = new DataView();

                                        GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                                        dtBnsMntDist = dsBnsMntDist.Tables[0];
                                        dvBnsMntDist = new DataView();

                                        if (para.bStructure == true)
                                        {
                                            GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                                            dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                                            dvBnsMntEmpWiseCalSt = new DataView();

                                            GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                                            dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                                            dvBnsMntDistSt = new DataView();
                                        }

                                        GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                                        dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                                        dvBnsEligibleEmp = new DataView();

                                        //Get General Salary Amount Head Wise
                                        List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                                        LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                        if (dsSalInfo.Tables[0].Rows.Count > 0)
                                            dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                        sBnsElgGentID = "";
                                        sBnsDedGentID = "";
                                        GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_ELIGIBLE", dsUnTagEmp.Tables[0].Rows.Count, out sBnsElgGentID);
                                        sBnsElgGentID = "BNSE" + sBnsElgGentID;

                                        GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                                        sBnsDedGentID = "BNSC" + sBnsDedGentID;
                                        for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                        {
                                            sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                            sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                            sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                                            //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                                            sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();
                                            bCalculatedBns = false;

                                            #region Master Table Data Capture [Start Date]

                                            dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                            #endregion Master Table Data Capture 

                                            string sMatDt = "";
                                            //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                            if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                                            {
                                                for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                                                {
                                                    sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                    if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                                                    {
                                                        bIsEligibleApp = false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bIsEligibleApp = true;
                                            }

                                            if (bIsEligibleApp == true)
                                            {
                                                var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                                if (dicSalInfo_Sub.Count > 0)
                                                {
                                                    //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                    //{
                                                    sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                                                    sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                    //}
                                                }

                                                #region Salary Amount Insert Into Virtual Table

                                                dtValue = new DataTable();
                                                dtValue.TableName = "TempTable";
                                                dtValue.Columns.Add("EmpSystemID");
                                                dtValue.Columns.Add("SalaryHeadID");
                                                dtValue.Columns.Add("EntryCurrencyID");
                                                dtValue.Columns.Add("EntryAmount");
                                                dtValue.Columns.Add("EarningCurrencyID");
                                                dtValue.Columns.Add("EarningAmount");

                                                dtValue = para.dsSalInfo.Tables[0];

                                                #endregion Salary Amount Insert Into Virtual Table

                                                if (dtValue.Rows.Count > 0)
                                                {
                                                    for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                                                    {
                                                        #region Clear

                                                        sBonusPolicyDetailsID = "";
                                                        sFormulaDesID = "";

                                                        decFixedValue = 0;
                                                        decEmpCtbtnAmount = 0;
                                                        decEarningValueRangeFrom = 0;
                                                        decEarningValueRangeTo = 0;

                                                        dtEndDate = System.DateTime.Now;
                                                        bMandatory = false;
                                                        bEarning = false;
                                                        bIsActive = true;
                                                        bIsFixed = false;
                                                        bIsFormula = false;
                                                        bIsDependOnEarning = false;

                                                        #endregion Clear

                                                        #region Select BonusPolicyDetails ID if have multiple column

                                                        sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                                                        sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                        ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                        sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                        decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                        decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                                                        if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) < decEarningValueRangeTo)
                                                        {
                                                            bEligible = true;
                                                            bApplicable = true;
                                                            bCalculatedBns = true;
                                                        }
                                                        else
                                                        {
                                                            bApplicable = false;
                                                        }

                                                        #region If Bonus applicable for all employee

                                                        if (bIsAllEmpApplocable == true && iBnsPlDtl == dsBnsPolicyDtl.Tables[0].Rows.Count && bApplicable == false && bCalculatedBns == false)
                                                        {
                                                            bApplicable = true;
                                                        }

                                                        #endregion If Bonus applicable for all employee

                                                        bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                                                        sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                                                        sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                                                        decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                                                        bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                                                        bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                                                        bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                                                        bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                                                        sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                                                        #endregion Select BonusPolicyDetails ID if have multiple column

                                                        if (bEligible == true)
                                                        {
                                                            if (bApplicable == true)
                                                            {
                                                                #region Bonus Earning Amount

                                                                if (bIsFixed == true)
                                                                {
                                                                    decEmpCtbtnAmount = decFixedValue;
                                                                }
                                                                else if (bIsFormula == true)
                                                                {
                                                                    bEarning = bIsDependOnEarning;
                                                                    ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                                                                    sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                                    decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                                                                }

                                                                #endregion Bonus Earning Amount

                                                                #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                                dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                                                                //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                                dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                                if (dvBnsMntEmpWiseCal.Count == 0)
                                                                {//Add new block
                                                                    sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                    drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                                                                    UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                                    dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                                                                }
                                                                else
                                                                {//edit block
                                                                    sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                                                                    drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                                                                    drBnsMntEmpWiseCal.BeginEdit();
                                                                    UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                                    drBnsMntEmpWiseCal.EndEdit();
                                                                }

                                                                if (para.bStructure == true)
                                                                {
                                                                    dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                                                                    dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                                    if (dvBnsMntEmpWiseCalSt.Count == 0)
                                                                    {//Add new block
                                                                        sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                        drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                                                                        UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                        dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                                                                    }
                                                                    else
                                                                    {//edit block
                                                                        sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                                                                        drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                                                                        drBnsMntEmpWiseCalSt.BeginEdit();
                                                                        UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                        drBnsMntEmpWiseCalSt.EndEdit();
                                                                    }
                                                                }

                                                                #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                                #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                                dvBnsMntDist.Table = dtBnsMntDist;
                                                                dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                                if (dvBnsMntDist.Count > 0)
                                                                {
                                                                    while (dvBnsMntDist.Count > 0)
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.Delete();
                                                                    }
                                                                }

                                                                if (para.bStructure == true)
                                                                {
                                                                    dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                    dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                                                                    if (dvBnsMntDistSt.Count > 0)
                                                                    {
                                                                        while (dvBnsMntDistSt.Count > 0)
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.Delete();
                                                                        }
                                                                    }
                                                                }

                                                                #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                                #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                                dvBnsPolicyDist.Table = dtBnsPolicyDist;
                                                                dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                                                                if (dvBnsPolicyDist.Count > 0)
                                                                {
                                                                    for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                                                                    {
                                                                        decimal decBonusValue = 0;
                                                                        if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                                                                        {
                                                                            decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                                                                            dvCurRl.Table = dtCurRl;
                                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                            if (dvCurRl.Count > 0)
                                                                            {
                                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                            }
                                                                            string sOutValue = "0";
                                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                            decBonusValue = Convert.ToDecimal(sOutValue);

                                                                            dvBnsMntDist.Table = dtBnsMntDist;
                                                                            dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                            if (dvBnsMntDist.Count == 0)
                                                                            {
                                                                                drBnsMntDist = dtBnsMntDist.NewRow();
                                                                                drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                                drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                drBnsMntDist["Value"] = decBonusValue;
                                                                                dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                            }
                                                                            else
                                                                            {
                                                                                drBnsMntDist = dvBnsMntDist[0].Row;
                                                                                drBnsMntDist.BeginEdit();
                                                                                drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                                drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                drBnsMntDist["Value"] = decBonusValue;
                                                                                drBnsMntDist.EndEdit();
                                                                            }

                                                                            if (para.bStructure == true)
                                                                            {
                                                                                dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                                dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                                if (dvBnsMntDistSt.Count == 0)
                                                                                {
                                                                                    drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                                    drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                    drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                    drBnsMntDistSt["Value"] = decBonusValue;
                                                                                    dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                                }
                                                                                else
                                                                                {
                                                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                                    drBnsMntDistSt.BeginEdit();
                                                                                    drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                    drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                    drBnsMntDistSt["Value"] = decBonusValue;
                                                                                    drBnsMntDistSt.EndEdit();
                                                                                }
                                                                            }
                                                                        }

                                                                        decBonusValue = 0;
                                                                        if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                                                                        {
                                                                            decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                                                                            dvCurRl.Table = dtCurRl;
                                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                            if (dvCurRl.Count > 0)
                                                                            {
                                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                            }
                                                                            string sOutValue = "0";
                                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                            decBonusValue = Convert.ToDecimal(sOutValue);

                                                                            dvBnsMntDist.Table = dtBnsMntDist;
                                                                            dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                            if (dvBnsMntDist.Count == 0)
                                                                            {
                                                                                drBnsMntDist = dtBnsMntDist.NewRow();
                                                                                drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                                drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                drBnsMntDist["Value"] = decBonusValue;
                                                                                dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                            }
                                                                            else
                                                                            {
                                                                                drBnsMntDist = dvBnsMntDist[0].Row;
                                                                                drBnsMntDist.BeginEdit();
                                                                                drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                                drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                drBnsMntDist["Value"] = decBonusValue;
                                                                                drBnsMntDist.EndEdit();
                                                                            }

                                                                            if (para.bStructure == true)
                                                                            {
                                                                                dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                                dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                                if (dvBnsMntDistSt.Count == 0)
                                                                                {
                                                                                    drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                                    drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                    drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                    drBnsMntDistSt["Value"] = decBonusValue;
                                                                                    dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                                }
                                                                                else
                                                                                {
                                                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                                    drBnsMntDistSt.BeginEdit();
                                                                                    drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                    drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                    drBnsMntDistSt["Value"] = decBonusValue;
                                                                                    drBnsMntDistSt.EndEdit();
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]
                                                            }
                                                            #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                                                            dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                                                            dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                                                            if (dvBnsEligibleEmp.Count == 0)
                                                            {//Add new block
                                                                drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                                                                dtBnsEligibleEmp.Rows.Add(drBnsEligibleEmp);
                                                            }

                                                            #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //if (SelectedEmpCnt == grdRowMaxCnt)
                                    //{
                                    TotProcComp += grdRowMaxCnt;
                                    TotSelectEmpForProc -= grdRowMaxCnt;
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist, dsBnsMntEmpWiseCalSt, dsBnsMntDistSt);
                                    //}
                                    if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                    {
                                        SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                        if (SelectedEmpCnt <= 0)
                                        { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                    }
                                    else
                                    {
                                        SelectedEmpCnt += 30;
                                    }
                                    dsBnsEligibleEmp = null;
                                    dsBnsMntEmpWiseCal = null;
                                }
                                //if (bMaturity == true)
                                //{
                                //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                //}
                            }

                            #endregion Untag Employee
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Clear DataSet 

                dsBnsEligibleEmp = null;
                dtBnsEligibleEmp = null;
                drBnsEligibleEmp = null;
                dvBnsEligibleEmp = null;

                dsBnsMntEmpWiseCal = null;
                dtBnsMntEmpWiseCal = null;
                drBnsMntEmpWiseCal = null;
                dvBnsMntEmpWiseCal = null;

                dsSalInfo = null;
                dsBnsPolicyMst = null;
                dsBnsPolicyDtl = null;
                dsBnsMonthNo = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function
        public void BonusCalculationMonthly(BnsParaListNew para)
        {
            #region Variable Dataset
            DataTable dtValue = null;
            DataSet dsBnsEligibleEmp = null;
            DataTable dtBnsEligibleEmp = null;
            DataRow drBnsEligibleEmp = null;
            DataView dvBnsEligibleEmp = null;

            DataSet dsBnsMntEmpWiseCal = null;
            DataTable dtBnsMntEmpWiseCal = null;
            DataRow drBnsMntEmpWiseCal = null;
            DataView dvBnsMntEmpWiseCal = null;

            DataSet dsBnsMntDist = null;
            DataTable dtBnsMntDist = null;
            DataRow drBnsMntDist = null;
            DataView dvBnsMntDist = null;

            DataSet dsBnsMntEmpWiseCalSt = null;
            DataTable dtBnsMntEmpWiseCalSt = null;
            DataRow drBnsMntEmpWiseCalSt = null;
            DataView dvBnsMntEmpWiseCalSt = null;

            DataSet dsBnsMntDistSt = null;
            DataTable dtBnsMntDistSt = null;
            DataRow drBnsMntDistSt = null;
            DataView dvBnsMntDistSt = null;

            DataSet dsSalHd = null;
            DataTable dtSalHd = null;

            DataSet dsSalInfo = null;
            DataSet dsMinWagSalary = null;
            DataTable dtMinWagSalary = null;
            DataSet dsBnsPolicyMst = null;
            DataSet dsBnsPolicyDtl = null;

            DataSet dsBnsPolicyDist = null;
            DataTable dtBnsPolicyDist = null;
            DataView dvBnsPolicyDist = null;
            DataSet dsBnsMonthNo = null;
            DataSet dsUnTagEmp = null;

            DataSet dsSlrGrd = null;
            DataTable dtSlrGrd = null;

            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;

            DataTable dtDw = null;
            clsSalaryUtility obSS = new global::clsSalaryUtility();

            #endregion Variable Dataset
            #region Declare Variable

            string sBnsEligibleEmpID = "";
            string sBnsMntEmpCalID = "";
            string sBnsMntEmpCalStID = "";
            string sBnsMstID = "";
            string sBnsDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sBnsElgGentID = "";
            string sBnsDedGentID = "";
            string sSalaryRuleMasterSystemID = "";

            string sEarningFormulaID = "";
            string sEarningFormulaResult = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEmpLocationID = "";
            string sEmpGivenDesignationId = "";
            string sFormulaDesID = "";
            string sBonusPolicyDetailsID = "";
            string sCompMinWagesAndOrginal = "";
            string sCurrencyRuleSystemID = "";
            string sRoundOption = "";

            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iDecimalNo = 0;

            DateTime dtStartDate;
            DateTime dtEndDate;

            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValue = 0;
            decimal decEmpCtbtnAmount = 0;

            bool bCalculatedBns = false;
            bool bMandatory = false;
            bool bApplicable = false;
            bool bEligible = false;
            bool bIsActive = true;
            bool bIsFixed = false;
            bool bIsFormula = false;
            bool bIsDependOnEarning = false;
            bool bIsMinWages = false;
            bool bEarning = false;
            bool bIsEligibleApp = true;
            bool bIsAllEmpApplocable = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;

            #endregion Declare Variable

            try
            {
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetDesignationMasterWiseMinSalary(para, out dsMinWagSalary);
                dtMinWagSalary = dsMinWagSalary.Tables[0];

                dtDw = para.dsDw.Tables[0];

                GetBonusPolicyMonthlyRetainMaster(sGroupID.Trim(), sPlantID.Trim(), out dsBnsPolicyMst);

                if (dsBnsPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int BnsPlCnt = 0; BnsPlCnt < dsBnsPolicyMst.Tables[0].Rows.Count; BnsPlCnt++)
                    {
                        sBnsMstID = dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["ID"].ToString().Trim();
                        bIsAllEmpApplocable = Convert.ToBoolean(dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["IsAllEmpApplocable"].ToString().Trim());

                        #region DataSet

                        GetBonusPolicyMonthlyRetainDetails(sBnsMstID, out dsBnsPolicyDtl);
                        GetBonusPolicyMonthlyRetainMonthNo(sBnsMstID, out dsBnsMonthNo);
                        GetBonusPolicyMonthlyRetainDistribution(sBnsMstID, out dsBnsPolicyDist);
                        dtBnsPolicyDist = dsBnsPolicyDist.Tables[0];
                        dvBnsPolicyDist = new DataView();

                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];

                        GetSalaryRuleGovtGrd(para, out dsSlrGrd);
                        dtSlrGrd = dsSlrGrd.Tables[0];

                        #endregion DataSet

                        #region Tag Employee List

                        GetTagEmployeeListWithBonusPolicyMonthlyRetain(para, sBnsMstID.Trim(), out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {
                                    GetBonusPolicyMonthlyRetainEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCal);
                                    dtBnsMntEmpWiseCal = dsBnsMntEmpWiseCal.Tables[0];
                                    dvBnsMntEmpWiseCal = new DataView();

                                    GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                                    dtBnsMntDist = dsBnsMntDist.Tables[0];
                                    dvBnsMntDist = new DataView();

                                    if (para.bStructure == true)
                                    {
                                        GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                                        dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                                        dvBnsMntEmpWiseCalSt = new DataView();

                                        GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                                        dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                                        dvBnsMntDistSt = new DataView();
                                    }

                                    GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                                    dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                                    dvBnsEligibleEmp = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                    sBnsElgGentID = "";
                                    sBnsDedGentID = "";
                                    sBnsElgGentID = "BNSE" + sBnsElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                                    sBnsDedGentID = "BNSC" + sBnsDedGentID;
                                    for (int iUnTgEmCnt = 0; iUnTgEmCnt < dsUnTagEmp.Tables[0].Rows.Count; iUnTgEmCnt++)
                                    {
                                        sBnsEligibleEmpID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["BnsEligibleEmpID"].ToString().Trim();//BnsEligibleEmpID
                                        //sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();//BnsEligibleEmpID
                                        //sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                        sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                                        //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                                        sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();

                                        #region Master Table Data Capture [Start Date]

                                        dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                        #endregion Master Table Data Capture 

                                        string sMatDt = "";
                                        //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                        if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                                        {
                                            for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                                            {
                                                sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                                                {
                                                    bIsEligibleApp = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bIsEligibleApp = true;
                                        }

                                        if (bIsEligibleApp == true)
                                        {
                                            var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                            if (dicSalInfo_Sub.Count > 0)
                                            {
                                                //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                //{
                                                sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                //}
                                            }
                                            else
                                            {
                                                sSalaryRuleMasterSystemID = para.sSalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = para.sCurrencyRuleSystemID;
                                            }

                                            #region Salary Amount Insert Into Virtual Table

                                            dtValue = new DataTable();
                                            dtValue.TableName = "TempTable";
                                            dtValue.Columns.Add("EmpSystemID");
                                            dtValue.Columns.Add("SalaryHeadID");
                                            dtValue.Columns.Add("EntryCurrencyID");
                                            dtValue.Columns.Add("EntryAmount");
                                            dtValue.Columns.Add("EarningCurrencyID");
                                            dtValue.Columns.Add("EarningAmount");

                                            dtValue = para.dsSalInfo.Tables[0];

                                            #endregion Salary Amount Insert Into Virtual Table

                                            if (dtValue.Rows.Count > 0)
                                            {
                                                for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                                                {
                                                    #region Clear

                                                    sBonusPolicyDetailsID = "";
                                                    sFormulaDesID = "";

                                                    decFixedValue = 0;
                                                    decEmpCtbtnAmount = 0;
                                                    decEarningValueRangeFrom = 0;
                                                    decEarningValueRangeTo = 0;

                                                    dtEndDate = System.DateTime.Now;
                                                    bMandatory = false;
                                                    bEarning = false;
                                                    bIsActive = true;
                                                    bIsFixed = false;
                                                    bIsFormula = false;
                                                    bIsDependOnEarning = false;

                                                    #endregion Clear

                                                    #region Select BonusPolicyDetails ID if have multiple column

                                                    sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                                                    sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                    sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                    decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                    decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                                                    if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) < decEarningValueRangeTo)
                                                    {
                                                        bApplicable = true;
                                                    }
                                                    else
                                                    {
                                                        bApplicable = false;
                                                    }
                                                    bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                                                    sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                                                    sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                                                    decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                                                    bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                                                    bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                                                    bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                                                    bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                                                    sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                                                    #endregion Select BonusPolicyDetails ID if have multiple column

                                                    if (bApplicable == true)
                                                    {
                                                        #region Bonus Earning Amount

                                                        if (bIsFixed == true)
                                                        {
                                                            decEmpCtbtnAmount = decFixedValue;
                                                        }
                                                        else if (bIsFormula == true)
                                                        {
                                                            bEarning = bIsDependOnEarning;
                                                            ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                                                            sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                            decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                                                        }

                                                        #endregion Bonus Earning Amount

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                                                        //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                        if (dvBnsMntEmpWiseCal.Count == 0)
                                                        {//Add new block
                                                            sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                            drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                                                        }
                                                        else
                                                        {//edit block
                                                            sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                                                            drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                                                            drBnsMntEmpWiseCal.BeginEdit();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            drBnsMntEmpWiseCal.EndEdit();
                                                        }

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                                                            dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                            if (dvBnsMntEmpWiseCalSt.Count == 0)
                                                            {//Add new block
                                                                sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                                                            }
                                                            else
                                                            {//edit block
                                                                sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                                                                drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                                                                drBnsMntEmpWiseCalSt.BeginEdit();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                drBnsMntEmpWiseCalSt.EndEdit();
                                                            }
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        dvBnsMntDist.Table = dtBnsMntDist;
                                                        dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        if (dvBnsMntDist.Count > 0)
                                                        {
                                                            while (dvBnsMntDist.Count > 0)
                                                            {
                                                                drBnsMntDist = dvBnsMntDist[0].Row;
                                                                drBnsMntDist.Delete();
                                                            }
                                                        }

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                                                            if (dvBnsMntDistSt.Count > 0)
                                                            {
                                                                while (dvBnsMntDistSt.Count > 0)
                                                                {
                                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                    drBnsMntDistSt.Delete();
                                                                }
                                                            }
                                                        }

                                                        #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        dvBnsPolicyDist.Table = dtBnsPolicyDist;
                                                        dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                                                        if (dvBnsPolicyDist.Count > 0)
                                                        {
                                                            for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                                                            {
                                                                decimal decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }
                                                                    }
                                                                }

                                                                decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue1 = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue1);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue1);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }

                                                                    }
                                                                }
                                                            }
                                                        }
                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                                                        dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                                                        dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                                                        if (dvBnsEligibleEmp.Count == 1)
                                                        {//Add new block
                                                            drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                                                            drBnsEligibleEmp.EndEdit();
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                                                    }//bApplicable
                                                }//for dsBnsPolicyDtl
                                            }//virtual table # dtValue.Rows.Count 
                                        }
                                    }

                                    //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                }

                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                if (para.bStructure == true)
                                {
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist, dsBnsMntEmpWiseCalSt, dsBnsMntDistSt);
                                }
                                else
                                {
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist);
                                }
                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                dsBnsEligibleEmp = null;
                                dsBnsMntEmpWiseCal = null;
                            }
                            //if (bMaturity == true)
                            //{
                            //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                            //}
                        }

                        #endregion Tag Employee List

                        #region Untag Employee List

                        GetUnTagEmployeeListWithBonusPolicyMonthlyRetain(para, out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {
                                    GetBonusPolicyMonthlyRetainEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCal);
                                    dtBnsMntEmpWiseCal = dsBnsMntEmpWiseCal.Tables[0];
                                    dvBnsMntEmpWiseCal = new DataView();

                                    GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                                    dtBnsMntDist = dsBnsMntDist.Tables[0];
                                    dvBnsMntDist = new DataView();

                                    if (para.bStructure == true)
                                    {
                                        GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                                        dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                                        dvBnsMntEmpWiseCalSt = new DataView();

                                        GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                                        dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                                        dvBnsMntDistSt = new DataView();
                                    }

                                    GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                                    dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                                    dvBnsEligibleEmp = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                    sBnsElgGentID = "";
                                    sBnsDedGentID = "";
                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_ELIGIBLE", dsUnTagEmp.Tables[0].Rows.Count, out sBnsElgGentID);
                                    sBnsElgGentID = "BNSE" + sBnsElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                                    sBnsDedGentID = "BNSC" + sBnsDedGentID;
                                    for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                    {
                                        sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                        sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                                        //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                                        sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();
                                        bCalculatedBns = false;

                                        #region Master Table Data Capture [Start Date]

                                        dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                        #endregion Master Table Data Capture 

                                        string sMatDt = "";
                                        //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                        if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                                        {
                                            for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                                            {
                                                sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                                                {
                                                    bIsEligibleApp = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bIsEligibleApp = true;
                                        }

                                        if (bIsEligibleApp == true)
                                        {
                                            var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                            if (dicSalInfo_Sub.Count > 0)
                                            {
                                                //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                //{
                                                sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                //}
                                            }

                                            #region Salary Amount Insert Into Virtual Table

                                            dtValue = new DataTable();
                                            dtValue.TableName = "TempTable";
                                            dtValue.Columns.Add("EmpSystemID");
                                            dtValue.Columns.Add("SalaryHeadID");
                                            dtValue.Columns.Add("EntryCurrencyID");
                                            dtValue.Columns.Add("EntryAmount");
                                            dtValue.Columns.Add("EarningCurrencyID");
                                            dtValue.Columns.Add("EarningAmount");

                                            dtValue = para.dsSalInfo.Tables[0];

                                            #endregion Salary Amount Insert Into Virtual Table

                                            if (dtValue.Rows.Count > 0)
                                            {
                                                for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                                                {
                                                    #region Clear

                                                    sBonusPolicyDetailsID = "";
                                                    sFormulaDesID = "";

                                                    decFixedValue = 0;
                                                    decEmpCtbtnAmount = 0;
                                                    decEarningValueRangeFrom = 0;
                                                    decEarningValueRangeTo = 0;

                                                    dtEndDate = System.DateTime.Now;
                                                    bMandatory = false;
                                                    bEarning = false;
                                                    bIsActive = true;
                                                    bIsFixed = false;
                                                    bIsFormula = false;
                                                    bIsDependOnEarning = false;

                                                    #endregion Clear

                                                    #region Select BonusPolicyDetails ID if have multiple column

                                                    sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                                                    sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                    sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                    decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                    decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                                                    if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) < decEarningValueRangeTo)
                                                    {
                                                        bEligible = true;
                                                        bApplicable = true;
                                                        bCalculatedBns = true;
                                                    }
                                                    else
                                                    {
                                                        bApplicable = false;
                                                    }

                                                    #region If Bonus applicable for all employee

                                                    if (bIsAllEmpApplocable == true && iBnsPlDtl == dsBnsPolicyDtl.Tables[0].Rows.Count && bApplicable == false && bCalculatedBns == false)
                                                    {
                                                        bApplicable = true;
                                                    }

                                                    #endregion If Bonus applicable for all employee

                                                    bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                                                    sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                                                    sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                                                    decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                                                    bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                                                    bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                                                    bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                                                    bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                                                    sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                                                    #endregion Select BonusPolicyDetails ID if have multiple column

                                                    if (bEligible == true)
                                                    {
                                                        if (bApplicable == true)
                                                        {
                                                            #region Bonus Earning Amount

                                                            if (bIsFixed == true)
                                                            {
                                                                decEmpCtbtnAmount = decFixedValue;
                                                            }
                                                            else if (bIsFormula == true)
                                                            {
                                                                bEarning = bIsDependOnEarning;
                                                                ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                                                                sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                                decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                                                            }

                                                            #endregion Bonus Earning Amount

                                                            #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                            dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                                                            //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                            dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                            if (dvBnsMntEmpWiseCal.Count == 0)
                                                            {//Add new block
                                                                sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                                dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                                                            }
                                                            else
                                                            {//edit block
                                                                sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                                                                drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                                                                drBnsMntEmpWiseCal.BeginEdit();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                                drBnsMntEmpWiseCal.EndEdit();
                                                            }

                                                            if (para.bStructure == true)
                                                            {
                                                                dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                                                                dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                                if (dvBnsMntEmpWiseCalSt.Count == 0)
                                                                {//Add new block
                                                                    sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                    drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                                                                    UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                    dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                                                                }
                                                                else
                                                                {//edit block
                                                                    sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                                                                    drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                                                                    drBnsMntEmpWiseCalSt.BeginEdit();
                                                                    UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                    drBnsMntEmpWiseCalSt.EndEdit();
                                                                }
                                                            }

                                                            #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                            #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                            dvBnsMntDist.Table = dtBnsMntDist;
                                                            dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                            if (dvBnsMntDist.Count > 0)
                                                            {
                                                                while (dvBnsMntDist.Count > 0)
                                                                {
                                                                    drBnsMntDist = dvBnsMntDist[0].Row;
                                                                    drBnsMntDist.Delete();
                                                                }
                                                            }

                                                            if (para.bStructure == true)
                                                            {
                                                                dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                                                                if (dvBnsMntDistSt.Count > 0)
                                                                {
                                                                    while (dvBnsMntDistSt.Count > 0)
                                                                    {
                                                                        drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                        drBnsMntDistSt.Delete();
                                                                    }
                                                                }
                                                            }

                                                            #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                            #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                            dvBnsPolicyDist.Table = dtBnsPolicyDist;
                                                            dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                                                            if (dvBnsPolicyDist.Count > 0)
                                                            {
                                                                for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                                                                {
                                                                    decimal decBonusValue = 0;
                                                                    if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                                                                    {
                                                                        decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                                                                        dvCurRl.Table = dtCurRl;
                                                                        dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                        if (dvCurRl.Count > 0)
                                                                        {
                                                                            sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                            bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                            bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                            iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                        }
                                                                        string sOutValue = "0";
                                                                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                        decBonusValue = Convert.ToDecimal(sOutValue);

                                                                        dvBnsMntDist.Table = dtBnsMntDist;
                                                                        dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDist.Count == 0)
                                                                        {
                                                                            drBnsMntDist = dtBnsMntDist.NewRow();
                                                                            drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                            drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDist["Value"] = decBonusValue;
                                                                            dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDist = dvBnsMntDist[0].Row;
                                                                            drBnsMntDist.BeginEdit();
                                                                            drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                            drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDist["Value"] = decBonusValue;
                                                                            drBnsMntDist.EndEdit();
                                                                        }

                                                                        if (para.bStructure == true)
                                                                        {
                                                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                            if (dvBnsMntDistSt.Count == 0)
                                                                            {
                                                                                drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                                drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                drBnsMntDistSt["Value"] = decBonusValue;
                                                                                dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                            }
                                                                            else
                                                                            {
                                                                                drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                                drBnsMntDistSt.BeginEdit();
                                                                                drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                drBnsMntDistSt["Value"] = decBonusValue;
                                                                                drBnsMntDistSt.EndEdit();
                                                                            }
                                                                        }
                                                                    }

                                                                    decBonusValue = 0;
                                                                    if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                                                                    {
                                                                        decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                                                                        dvCurRl.Table = dtCurRl;
                                                                        dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                        if (dvCurRl.Count > 0)
                                                                        {
                                                                            sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                            bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                            bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                            iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                        }
                                                                        string sOutValue = "0";
                                                                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                        decBonusValue = Convert.ToDecimal(sOutValue);

                                                                        dvBnsMntDist.Table = dtBnsMntDist;
                                                                        dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDist.Count == 0)
                                                                        {
                                                                            drBnsMntDist = dtBnsMntDist.NewRow();
                                                                            drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                            drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDist["Value"] = decBonusValue;
                                                                            dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDist = dvBnsMntDist[0].Row;
                                                                            drBnsMntDist.BeginEdit();
                                                                            drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                            drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDist["Value"] = decBonusValue;
                                                                            drBnsMntDist.EndEdit();
                                                                        }

                                                                        if (para.bStructure == true)
                                                                        {
                                                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                            if (dvBnsMntDistSt.Count == 0)
                                                                            {
                                                                                drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                                drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                drBnsMntDistSt["Value"] = decBonusValue;
                                                                                dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                            }
                                                                            else
                                                                            {
                                                                                drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                                drBnsMntDistSt.BeginEdit();
                                                                                drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                drBnsMntDistSt["Value"] = decBonusValue;
                                                                                drBnsMntDistSt.EndEdit();
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]
                                                        }
                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                                                        dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                                                        dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                                                        if (dvBnsEligibleEmp.Count == 0)
                                                        {//Add new block
                                                            drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                                                            dtBnsEligibleEmp.Rows.Add(drBnsEligibleEmp);
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                //if (SelectedEmpCnt == grdRowMaxCnt)
                                //{
                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist, dsBnsMntEmpWiseCalSt, dsBnsMntDistSt);
                                //}
                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                dsBnsEligibleEmp = null;
                                dsBnsMntEmpWiseCal = null;
                            }
                            //if (bMaturity == true)
                            //{
                            //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                            //}
                        }

                        #endregion Untag Employee
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Clear DataSet 

                dsBnsEligibleEmp = null;
                dtBnsEligibleEmp = null;
                drBnsEligibleEmp = null;
                dvBnsEligibleEmp = null;

                dsBnsMntEmpWiseCal = null;
                dtBnsMntEmpWiseCal = null;
                drBnsMntEmpWiseCal = null;
                dvBnsMntEmpWiseCal = null;

                dsSalInfo = null;
                dsBnsPolicyMst = null;
                dsBnsPolicyDtl = null;
                dsBnsMonthNo = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function




        public void xGeneratorBonusEligibleEmployee(BnsParaListNew para)
        {
            #region Variable Dataset
            DataTable dtValue = null;
            DataSet dsBnsEligibleEmp = null;
            DataTable dtBnsEligibleEmp = null;
            DataRow drBnsEligibleEmp = null;
            DataView dvBnsEligibleEmp = null;

            DataSet dsBnsMntEmpWiseCal = null;
            DataTable dtBnsMntEmpWiseCal = null;
            DataRow drBnsMntEmpWiseCal = null;
            DataView dvBnsMntEmpWiseCal = null;

            DataSet dsBnsMntDist = null;
            DataTable dtBnsMntDist = null;
            DataRow drBnsMntDist = null;
            DataView dvBnsMntDist = null;

            DataSet dsBnsMntEmpWiseCalSt = null;
            DataTable dtBnsMntEmpWiseCalSt = null;
            DataRow drBnsMntEmpWiseCalSt = null;
            DataView dvBnsMntEmpWiseCalSt = null;

            DataSet dsBnsMntDistSt = null;
            DataTable dtBnsMntDistSt = null;
            DataRow drBnsMntDistSt = null;
            DataView dvBnsMntDistSt = null;

            DataSet dsSalHd = null;
            DataTable dtSalHd = null;

            DataSet dsSalInfo = null;
            DataSet dsMinWagSalary = null;
            DataTable dtMinWagSalary = null;
            DataSet dsBnsPolicyMst = null;
            DataSet dsBnsPolicyDtl = null;
            DataSet dsBnsPolicyDist = null;
            DataTable dtBnsPolicyDist = null;
            DataView dvBnsPolicyDist = null;
            DataSet dsBnsMonthNo = null;
            DataSet dsUnTagEmp = null;

            DataSet dsSlrGrd = null;
            DataTable dtSlrGrd = null;

            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;

            DataTable dtDw = null;
            clsSalaryUtility obSS = new global::clsSalaryUtility();

            #endregion Variable Dataset
            #region Declare Variable

            string sBnsEligibleEmpID = "";
            string sBnsMntEmpCalID = "";
            string sBnsMntEmpCalStID = "";
            string sBnsMstID = "";
            string sBnsDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sBnsElgGentID = "";
            string sBnsDedGentID = "";
            string sSalaryRuleMasterSystemID = "";

            string sEarningFormulaID = "";
            string sEarningFormulaResult = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEmpLocationID = "";
            string sEmpGivenDesignationId = "";
            string sFormulaDesID = "";
            string sBonusPolicyDetailsID = "";
            string sCompMinWagesAndOrginal = "";
            string sCurrencyRuleSystemID = "";
            string sRoundOption = "";

            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iDecimalNo = 0;

            DateTime dtStartDate;
            DateTime dtEndDate;

            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValue = 0;
            decimal decEmpCtbtnAmount = 0;

            bool bCalculatedBns = false;
            bool bMandatory = false;
            bool bApplicable = false;
            bool bEligible = false;
            bool bIsActive = true;
            bool bIsFixed = false;
            bool bIsFormula = false;
            bool bIsDependOnEarning = false;
            bool bIsMinWages = false;
            bool bEarning = false;
            bool bIsEligibleApp = true;
            bool bIsAllEmpApplocable = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;

            #endregion Declare Variable

            try
            {
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetDesignationMasterWiseMinSalary(para, out dsMinWagSalary);
                dtMinWagSalary = dsMinWagSalary.Tables[0];

                dtDw = para.dsDw.Tables[0];

                GetBonusPolicyMonthlyRetainMaster(sGroupID.Trim(), sPlantID.Trim(), out dsBnsPolicyMst);

                if (dsBnsPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int BnsPlCnt = 0; BnsPlCnt < dsBnsPolicyMst.Tables[0].Rows.Count; BnsPlCnt++)
                    {
                        sBnsMstID = dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["ID"].ToString().Trim();
                        bIsAllEmpApplocable = Convert.ToBoolean(dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["IsAllEmpApplocable"].ToString().Trim());

                        #region DataSet

                        GetBonusPolicyMonthlyRetainDetails(sBnsMstID, out dsBnsPolicyDtl);
                        GetBonusPolicyMonthlyRetainMonthNo(sBnsMstID, out dsBnsMonthNo);
                        GetBonusPolicyMonthlyRetainDistribution(sBnsMstID, out dsBnsPolicyDist);
                        dtBnsPolicyDist = dsBnsPolicyDist.Tables[0];
                        dvBnsPolicyDist = new DataView();

                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];

                        GetSalaryRuleGovtGrd(para, out dsSlrGrd);
                        dtSlrGrd = dsSlrGrd.Tables[0];

                        #endregion DataSet

                        #region Tag Employee List

                        GetTagEmployeeListWithBonusPolicyMonthlyRetain(para, sBnsMstID.Trim(), out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {
                                    GetBonusPolicyMonthlyRetainEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCal);
                                    dtBnsMntEmpWiseCal = dsBnsMntEmpWiseCal.Tables[0];
                                    dvBnsMntEmpWiseCal = new DataView();

                                    GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                                    dtBnsMntDist = dsBnsMntDist.Tables[0];
                                    dvBnsMntDist = new DataView();

                                    if (para.bStructure == true)
                                    {
                                        GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                                        dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                                        dvBnsMntEmpWiseCalSt = new DataView();

                                        GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                                        dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                                        dvBnsMntDistSt = new DataView();
                                    }

                                    GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                                    dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                                    dvBnsEligibleEmp = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                    sBnsElgGentID = "";
                                    sBnsDedGentID = "";
                                    sBnsElgGentID = "BNSE" + sBnsElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                                    sBnsDedGentID = "BNSC" + sBnsDedGentID;
                                    for (int iUnTgEmCnt = 0; iUnTgEmCnt < dsUnTagEmp.Tables[0].Rows.Count; iUnTgEmCnt++)
                                    {
                                        sBnsEligibleEmpID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["BnsEligibleEmpID"].ToString().Trim();//BnsEligibleEmpID
                                        //sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();//BnsEligibleEmpID
                                        //sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                        sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                                        //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                                        sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();

                                        #region Master Table Data Capture [Start Date]

                                        dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                        #endregion Master Table Data Capture 

                                        string sMatDt = "";
                                        //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                        if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                                        {
                                            for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                                            {
                                                sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                                                {
                                                    bIsEligibleApp = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bIsEligibleApp = true;
                                        }

                                        if (bIsEligibleApp == true)
                                        {
                                            var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                            if (dicSalInfo_Sub.Count > 0)
                                            {
                                                //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                //{
                                                sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                //}
                                            }
                                            else
                                            {
                                                sSalaryRuleMasterSystemID = para.sSalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = para.sCurrencyRuleSystemID;
                                            }

                                            #region Salary Amount Insert Into Virtual Table

                                            dtValue = new DataTable();
                                            dtValue.TableName = "TempTable";
                                            dtValue.Columns.Add("EmpSystemID");
                                            dtValue.Columns.Add("SalaryHeadID");
                                            dtValue.Columns.Add("EntryCurrencyID");
                                            dtValue.Columns.Add("EntryAmount");
                                            dtValue.Columns.Add("EarningCurrencyID");
                                            dtValue.Columns.Add("EarningAmount");

                                            dtValue = para.dsSalInfo.Tables[0];

                                            #endregion Salary Amount Insert Into Virtual Table

                                            if (dtValue.Rows.Count > 0)
                                            {
                                                for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                                                {
                                                    #region Clear

                                                    sBonusPolicyDetailsID = "";
                                                    sFormulaDesID = "";

                                                    decFixedValue = 0;
                                                    decEmpCtbtnAmount = 0;
                                                    decEarningValueRangeFrom = 0;
                                                    decEarningValueRangeTo = 0;

                                                    dtEndDate = System.DateTime.Now;
                                                    bMandatory = false;
                                                    bEarning = false;
                                                    bIsActive = true;
                                                    bIsFixed = false;
                                                    bIsFormula = false;
                                                    bIsDependOnEarning = false;

                                                    #endregion Clear

                                                    #region Select BonusPolicyDetails ID if have multiple column

                                                    sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                                                    sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                    sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                    decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                    decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                                                    if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) < decEarningValueRangeTo)
                                                    {
                                                        bApplicable = true;
                                                    }
                                                    else
                                                    {
                                                        bApplicable = false;
                                                    }
                                                    bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                                                    sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                                                    sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                                                    decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                                                    bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                                                    bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                                                    bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                                                    bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                                                    sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                                                    #endregion Select BonusPolicyDetails ID if have multiple column

                                                    if (bApplicable == true)
                                                    {
                                                        #region Bonus Earning Amount

                                                        if (bIsFixed == true)
                                                        {
                                                            decEmpCtbtnAmount = decFixedValue;
                                                        }
                                                        else if (bIsFormula == true)
                                                        {
                                                            bEarning = bIsDependOnEarning;
                                                            ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                                                            sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                            decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                                                        }

                                                        #endregion Bonus Earning Amount

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                                                        //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                        if (dvBnsMntEmpWiseCal.Count == 0)
                                                        {//Add new block
                                                            sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                            drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                                                        }
                                                        else
                                                        {//edit block
                                                            sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                                                            drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                                                            drBnsMntEmpWiseCal.BeginEdit();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            drBnsMntEmpWiseCal.EndEdit();
                                                        }

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                                                            dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                            if (dvBnsMntEmpWiseCalSt.Count == 0)
                                                            {//Add new block
                                                                sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                                                            }
                                                            else
                                                            {//edit block
                                                                sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                                                                drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                                                                drBnsMntEmpWiseCalSt.BeginEdit();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                drBnsMntEmpWiseCalSt.EndEdit();
                                                            }
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        //by monir
                                                        //dvBnsMntDist.Table = dtBnsMntDist;
                                                        //dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        //if (dvBnsMntDist.Count > 0)
                                                        //{
                                                        //    while (dvBnsMntDist.Count > 0)
                                                        //    {
                                                        //        drBnsMntDist = dvBnsMntDist[0].Row;
                                                        //        drBnsMntDist.Delete();
                                                        //    }
                                                        //}

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                                                            if (dvBnsMntDistSt.Count > 0)
                                                            {
                                                                while (dvBnsMntDistSt.Count > 0)
                                                                {
                                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                    drBnsMntDistSt.Delete();
                                                                }
                                                            }
                                                        }

                                                        #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        dvBnsPolicyDist.Table = dtBnsPolicyDist;
                                                        dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                                                        if (dvBnsPolicyDist.Count > 0)
                                                        {
                                                            for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                                                            {
                                                                decimal decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }
                                                                    }
                                                                }

                                                                decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue1 = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue1);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue1);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }

                                                                    }
                                                                }
                                                            }
                                                        }
                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                                                        dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                                                        dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                                                        if (dvBnsEligibleEmp.Count == 1)
                                                        {//Add new block
                                                            drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                                                            drBnsEligibleEmp.EndEdit();
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                }

                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                if (para.bStructure == true)
                                {
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist, dsBnsMntEmpWiseCalSt, dsBnsMntDistSt);
                                }
                                else
                                {
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist);
                                }
                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                dsBnsEligibleEmp = null;
                                dsBnsMntEmpWiseCal = null;
                            }
                            //if (bMaturity == true)
                            //{
                            //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                            //}
                        }

                        #endregion Tag Employee List


                        if (para.ShouldNotProcessUntaggedEmp == false)
                        {
                            #region Untag Employee List

                            GetUnTagEmployeeListWithBonusPolicyMonthlyRetain(para, out dsUnTagEmp);
                            if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                                TotProcComp = 0;
                                grdRowMaxCnt = 0;
                                SelectedEmpCnt = 0;
                                EmpCntForLoop = 0;

                                while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    sEmpInfoSysIDColl = "";
                                    sEmpSystemID = "";
                                    EmpCntForLoop = 0;

                                    if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                    {
                                        grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                    }
                                    else
                                    {
                                        grdRowMaxCnt = 30;
                                    }

                                    #region Employee System ID Collection

                                    for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                    {
                                        if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                        {
                                            sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                            sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        }
                                        else
                                        {
                                            sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                            sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        }
                                        EmpCntForLoop++;
                                    }

                                    #endregion Employee System ID Collection

                                    if (EmpCntForLoop == grdRowMaxCnt)
                                    {
                                        GetBonusPolicyMonthlyRetainEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCal);
                                        dtBnsMntEmpWiseCal = dsBnsMntEmpWiseCal.Tables[0];
                                        dvBnsMntEmpWiseCal = new DataView();

                                        GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                                        dtBnsMntDist = dsBnsMntDist.Tables[0];
                                        dvBnsMntDist = new DataView();

                                        if (para.bStructure == true)
                                        {
                                            GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                                            dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                                            dvBnsMntEmpWiseCalSt = new DataView();

                                            GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                                            dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                                            dvBnsMntDistSt = new DataView();
                                        }

                                        GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                                        dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                                        dvBnsEligibleEmp = new DataView();

                                        //Get General Salary Amount Head Wise
                                        List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                                        LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                        if (dsSalInfo.Tables[0].Rows.Count > 0)
                                            dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                        sBnsElgGentID = "";
                                        sBnsDedGentID = "";
                                        GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_ELIGIBLE", dsUnTagEmp.Tables[0].Rows.Count, out sBnsElgGentID);
                                        sBnsElgGentID = "BNSE" + sBnsElgGentID;

                                        GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                                        sBnsDedGentID = "BNSC" + sBnsDedGentID;
                                        for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                        {
                                            sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                            sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                            sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                                            //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                                            sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();
                                            bCalculatedBns = false;

                                            #region Master Table Data Capture [Start Date]

                                            dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                            #endregion Master Table Data Capture 

                                            string sMatDt = "";
                                            //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                            if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                                            {
                                                for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                                                {
                                                    sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                    if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                                                    {
                                                        bIsEligibleApp = false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bIsEligibleApp = true;
                                            }

                                            if (bIsEligibleApp == true)
                                            {
                                                var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                                if (dicSalInfo_Sub.Count > 0)
                                                {
                                                    //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                    //{
                                                    sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                                                    sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                    //}
                                                }

                                                #region Salary Amount Insert Into Virtual Table

                                                dtValue = new DataTable();
                                                dtValue.TableName = "TempTable";
                                                dtValue.Columns.Add("EmpSystemID");
                                                dtValue.Columns.Add("SalaryHeadID");
                                                dtValue.Columns.Add("EntryCurrencyID");
                                                dtValue.Columns.Add("EntryAmount");
                                                dtValue.Columns.Add("EarningCurrencyID");
                                                dtValue.Columns.Add("EarningAmount");

                                                dtValue = para.dsSalInfo.Tables[0];

                                                #endregion Salary Amount Insert Into Virtual Table

                                                if (dtValue.Rows.Count > 0)
                                                {
                                                    for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                                                    {
                                                        #region Clear

                                                        sBonusPolicyDetailsID = "";
                                                        sFormulaDesID = "";

                                                        decFixedValue = 0;
                                                        decEmpCtbtnAmount = 0;
                                                        decEarningValueRangeFrom = 0;
                                                        decEarningValueRangeTo = 0;

                                                        dtEndDate = System.DateTime.Now;
                                                        bMandatory = false;
                                                        bEarning = false;
                                                        bIsActive = true;
                                                        bIsFixed = false;
                                                        bIsFormula = false;
                                                        bIsDependOnEarning = false;

                                                        #endregion Clear

                                                        #region Select BonusPolicyDetails ID if have multiple column

                                                        sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                                                        sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                        ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                        sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                        decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                        decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                                                        if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) < decEarningValueRangeTo)
                                                        {
                                                            bEligible = true;
                                                            bApplicable = true;
                                                            bCalculatedBns = true;
                                                        }
                                                        else
                                                        {
                                                            bApplicable = false;
                                                        }

                                                        #region If Bonus applicable for all employee

                                                        if (bIsAllEmpApplocable == true && iBnsPlDtl == dsBnsPolicyDtl.Tables[0].Rows.Count && bApplicable == false && bCalculatedBns == false)
                                                        {
                                                            bApplicable = true;
                                                        }

                                                        #endregion If Bonus applicable for all employee

                                                        bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                                                        sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                                                        sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                                                        decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                                                        bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                                                        bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                                                        bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                                                        bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                                                        sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                                                        #endregion Select BonusPolicyDetails ID if have multiple column

                                                        if (bEligible == true)
                                                        {
                                                            if (bApplicable == true)
                                                            {
                                                                #region Bonus Earning Amount

                                                                if (bIsFixed == true)
                                                                {
                                                                    decEmpCtbtnAmount = decFixedValue;
                                                                }
                                                                else if (bIsFormula == true)
                                                                {
                                                                    bEarning = bIsDependOnEarning;
                                                                    ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                                                                    sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                                    decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                                                                }

                                                                #endregion Bonus Earning Amount

                                                                #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                                dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                                                                //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                                dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                                if (dvBnsMntEmpWiseCal.Count == 0)
                                                                {//Add new block
                                                                    sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                    drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                                                                    UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                                    dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                                                                }
                                                                else
                                                                {//edit block
                                                                    sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                                                                    drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                                                                    drBnsMntEmpWiseCal.BeginEdit();
                                                                    UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                                    drBnsMntEmpWiseCal.EndEdit();
                                                                }

                                                                if (para.bStructure == true)
                                                                {
                                                                    dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                                                                    dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                                    if (dvBnsMntEmpWiseCalSt.Count == 0)
                                                                    {//Add new block
                                                                        sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                        drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                                                                        UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                        dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                                                                    }
                                                                    else
                                                                    {//edit block
                                                                        sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                                                                        drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                                                                        drBnsMntEmpWiseCalSt.BeginEdit();
                                                                        UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                        drBnsMntEmpWiseCalSt.EndEdit();
                                                                    }
                                                                }

                                                                #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                                #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                                dvBnsMntDist.Table = dtBnsMntDist;
                                                                dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                                if (dvBnsMntDist.Count > 0)
                                                                {
                                                                    while (dvBnsMntDist.Count > 0)
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.Delete();
                                                                    }
                                                                }

                                                                if (para.bStructure == true)
                                                                {
                                                                    dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                    dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                                                                    if (dvBnsMntDistSt.Count > 0)
                                                                    {
                                                                        while (dvBnsMntDistSt.Count > 0)
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.Delete();
                                                                        }
                                                                    }
                                                                }

                                                                #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                                #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                                dvBnsPolicyDist.Table = dtBnsPolicyDist;
                                                                dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                                                                if (dvBnsPolicyDist.Count > 0)
                                                                {
                                                                    for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                                                                    {
                                                                        decimal decBonusValue = 0;
                                                                        if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                                                                        {
                                                                            decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                                                                            dvCurRl.Table = dtCurRl;
                                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                            if (dvCurRl.Count > 0)
                                                                            {
                                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                            }
                                                                            string sOutValue = "0";
                                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                            decBonusValue = Convert.ToDecimal(sOutValue);

                                                                            dvBnsMntDist.Table = dtBnsMntDist;
                                                                            dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                            if (dvBnsMntDist.Count == 0)
                                                                            {
                                                                                drBnsMntDist = dtBnsMntDist.NewRow();
                                                                                drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                                drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                drBnsMntDist["Value"] = decBonusValue;
                                                                                dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                            }
                                                                            else
                                                                            {
                                                                                drBnsMntDist = dvBnsMntDist[0].Row;
                                                                                drBnsMntDist.BeginEdit();
                                                                                drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                                drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                drBnsMntDist["Value"] = decBonusValue;
                                                                                drBnsMntDist.EndEdit();
                                                                            }

                                                                            if (para.bStructure == true)
                                                                            {
                                                                                dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                                dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                                if (dvBnsMntDistSt.Count == 0)
                                                                                {
                                                                                    drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                                    drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                    drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                    drBnsMntDistSt["Value"] = decBonusValue;
                                                                                    dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                                }
                                                                                else
                                                                                {
                                                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                                    drBnsMntDistSt.BeginEdit();
                                                                                    drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                    drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                    drBnsMntDistSt["Value"] = decBonusValue;
                                                                                    drBnsMntDistSt.EndEdit();
                                                                                }
                                                                            }
                                                                        }

                                                                        decBonusValue = 0;
                                                                        if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                                                                        {
                                                                            decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                                                                            dvCurRl.Table = dtCurRl;
                                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                            if (dvCurRl.Count > 0)
                                                                            {
                                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                            }
                                                                            string sOutValue = "0";
                                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                            decBonusValue = Convert.ToDecimal(sOutValue);

                                                                            dvBnsMntDist.Table = dtBnsMntDist;
                                                                            dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                            if (dvBnsMntDist.Count == 0)
                                                                            {
                                                                                drBnsMntDist = dtBnsMntDist.NewRow();
                                                                                drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                                drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                drBnsMntDist["Value"] = decBonusValue;
                                                                                dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                            }
                                                                            else
                                                                            {
                                                                                drBnsMntDist = dvBnsMntDist[0].Row;
                                                                                drBnsMntDist.BeginEdit();
                                                                                drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                                drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                drBnsMntDist["Value"] = decBonusValue;
                                                                                drBnsMntDist.EndEdit();
                                                                            }

                                                                            if (para.bStructure == true)
                                                                            {
                                                                                dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                                dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                                if (dvBnsMntDistSt.Count == 0)
                                                                                {
                                                                                    drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                                    drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                    drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                    drBnsMntDistSt["Value"] = decBonusValue;
                                                                                    dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                                }
                                                                                else
                                                                                {
                                                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                                    drBnsMntDistSt.BeginEdit();
                                                                                    drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                    drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                    drBnsMntDistSt["Value"] = decBonusValue;
                                                                                    drBnsMntDistSt.EndEdit();
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]
                                                            }
                                                            #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                                                            dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                                                            dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                                                            if (dvBnsEligibleEmp.Count == 0)
                                                            {//Add new block
                                                                drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                                                                dtBnsEligibleEmp.Rows.Add(drBnsEligibleEmp);
                                                            }

                                                            #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //if (SelectedEmpCnt == grdRowMaxCnt)
                                    //{
                                    TotProcComp += grdRowMaxCnt;
                                    TotSelectEmpForProc -= grdRowMaxCnt;
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist, dsBnsMntEmpWiseCalSt, dsBnsMntDistSt);
                                    //}
                                    if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                    {
                                        SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                        if (SelectedEmpCnt <= 0)
                                        { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                    }
                                    else
                                    {
                                        SelectedEmpCnt += 30;
                                    }
                                    dsBnsEligibleEmp = null;
                                    dsBnsMntEmpWiseCal = null;
                                }
                                //if (bMaturity == true)
                                //{
                                //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                //}
                            }

                            #endregion Untag Employee
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Clear DataSet 

                dsBnsEligibleEmp = null;
                dtBnsEligibleEmp = null;
                drBnsEligibleEmp = null;
                dvBnsEligibleEmp = null;

                dsBnsMntEmpWiseCal = null;
                dtBnsMntEmpWiseCal = null;
                drBnsMntEmpWiseCal = null;
                dvBnsMntEmpWiseCal = null;

                dsSalInfo = null;
                dsBnsPolicyMst = null;
                dsBnsPolicyDtl = null;
                dsBnsMonthNo = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function
        public void xBonusCalculationMonthly(BnsParaListNew para)
        {
            #region Variable Dataset
            DataTable dtValue = null;
            DataSet dsBnsEligibleEmp = null;
            DataTable dtBnsEligibleEmp = null;
            DataRow drBnsEligibleEmp = null;
            DataView dvBnsEligibleEmp = null;

            DataSet dsBnsMntEmpWiseCal = null;
            DataTable dtBnsMntEmpWiseCal = null;
            DataRow drBnsMntEmpWiseCal = null;
            DataView dvBnsMntEmpWiseCal = null;

            DataSet dsBnsMntDist = null;
            DataTable dtBnsMntDist = null;
            DataRow drBnsMntDist = null;
            DataView dvBnsMntDist = null;

            DataSet dsBnsMntEmpWiseCalSt = null;
            DataTable dtBnsMntEmpWiseCalSt = null;
            DataRow drBnsMntEmpWiseCalSt = null;
            DataView dvBnsMntEmpWiseCalSt = null;

            DataSet dsBnsMntDistSt = null;
            DataTable dtBnsMntDistSt = null;
            DataRow drBnsMntDistSt = null;
            DataView dvBnsMntDistSt = null;

            DataSet dsSalHd = null;
            DataTable dtSalHd = null;

            DataSet dsSalInfo = null;
            DataSet dsMinWagSalary = null;
            DataTable dtMinWagSalary = null;
            DataSet dsBnsPolicyMst = null;
            DataSet dsBnsPolicyDtl = null;

            DataSet dsBnsPolicyDist = null;
            DataTable dtBnsPolicyDist = null;
            DataView dvBnsPolicyDist = null;
            DataSet dsBnsMonthNo = null;
            DataSet dsUnTagEmp = null;

            DataSet dsSlrGrd = null;
            DataTable dtSlrGrd = null;

            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;

            DataTable dtDw = null;
            clsSalaryUtility obSS = new global::clsSalaryUtility();

            #endregion Variable Dataset
            #region Declare Variable

            string sBnsEligibleEmpID = "";
            string sBnsMntEmpCalID = "";
            string sBnsMntEmpCalStID = "";
            string sBnsMstID = "";
            string sBnsDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sBnsElgGentID = "";
            string sBnsDedGentID = "";
            string sSalaryRuleMasterSystemID = "";

            string sEarningFormulaID = "";
            string sEarningFormulaResult = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEmpLocationID = "";
            string sEmpGivenDesignationId = "";
            string sFormulaDesID = "";
            string sBonusPolicyDetailsID = "";
            string sCompMinWagesAndOrginal = "";
            string sCurrencyRuleSystemID = "";
            string sRoundOption = "";

            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iDecimalNo = 0;

            DateTime dtStartDate;
            DateTime dtEndDate;

            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValue = 0;
            decimal decEmpCtbtnAmount = 0;

            bool bCalculatedBns = false;
            bool bMandatory = false;
            bool bApplicable = false;
            bool bEligible = false;
            bool bIsActive = true;
            bool bIsFixed = false;
            bool bIsFormula = false;
            bool bIsDependOnEarning = false;
            bool bIsMinWages = false;
            bool bEarning = false;
            bool bIsEligibleApp = true;
            bool bIsAllEmpApplocable = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;

            #endregion Declare Variable

            try
            {
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetDesignationMasterWiseMinSalary(para, out dsMinWagSalary);
                dtMinWagSalary = dsMinWagSalary.Tables[0];

                dtDw = para.dsDw.Tables[0];

                GetBonusPolicyMonthlyRetainMaster(sGroupID.Trim(), sPlantID.Trim(), out dsBnsPolicyMst);

                if (dsBnsPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int BnsPlCnt = 0; BnsPlCnt < dsBnsPolicyMst.Tables[0].Rows.Count; BnsPlCnt++)
                    {
                        sBnsMstID = dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["ID"].ToString().Trim();
                        bIsAllEmpApplocable = Convert.ToBoolean(dsBnsPolicyMst.Tables[0].Rows[BnsPlCnt]["IsAllEmpApplocable"].ToString().Trim());

                        #region DataSet

                        GetBonusPolicyMonthlyRetainDetails(sBnsMstID, out dsBnsPolicyDtl);
                        GetBonusPolicyMonthlyRetainMonthNo(sBnsMstID, out dsBnsMonthNo);
                        GetBonusPolicyMonthlyRetainDistribution(sBnsMstID, out dsBnsPolicyDist);
                        dtBnsPolicyDist = dsBnsPolicyDist.Tables[0];
                        dvBnsPolicyDist = new DataView();

                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];

                        GetSalaryRuleGovtGrd(para, out dsSlrGrd);
                        dtSlrGrd = dsSlrGrd.Tables[0];

                        #endregion DataSet

                        #region Tag Employee List

                        GetTagEmployeeListWithBonusPolicyMonthlyRetain(para, sBnsMstID.Trim(), out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {
                                    GetBonusPolicyMonthlyRetainEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCal);
                                    dtBnsMntEmpWiseCal = dsBnsMntEmpWiseCal.Tables[0];
                                    dvBnsMntEmpWiseCal = new DataView();

                                    GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                                    dtBnsMntDist = dsBnsMntDist.Tables[0];
                                    dvBnsMntDist = new DataView();

                                    if (para.bStructure == true)
                                    {
                                        GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                                        dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                                        dvBnsMntEmpWiseCalSt = new DataView();

                                        GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                                        dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                                        dvBnsMntDistSt = new DataView();
                                    }

                                    GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                                    dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                                    dvBnsEligibleEmp = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                    sBnsElgGentID = "";
                                    sBnsDedGentID = "";
                                    sBnsElgGentID = "BNSE" + sBnsElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                                    sBnsDedGentID = "BNSC" + sBnsDedGentID;
                                    for (int iUnTgEmCnt = 0; iUnTgEmCnt < dsUnTagEmp.Tables[0].Rows.Count; iUnTgEmCnt++)
                                    {
                                        sBnsEligibleEmpID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["BnsEligibleEmpID"].ToString().Trim();//BnsEligibleEmpID
                                        //sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();//BnsEligibleEmpID
                                        //sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                        sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                                        //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                                        sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();

                                        #region Master Table Data Capture [Start Date]

                                        dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                        #endregion Master Table Data Capture 

                                        string sMatDt = "";
                                        //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                        if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                                        {
                                            for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                                            {
                                                sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                                                {
                                                    bIsEligibleApp = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bIsEligibleApp = true;
                                        }

                                        if (bIsEligibleApp == true)
                                        {
                                            var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                            if (dicSalInfo_Sub.Count > 0)
                                            {
                                                //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                //{
                                                sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                //}
                                            }
                                            else
                                            {
                                                sSalaryRuleMasterSystemID = para.sSalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = para.sCurrencyRuleSystemID;
                                            }

                                            #region Salary Amount Insert Into Virtual Table

                                            dtValue = new DataTable();
                                            dtValue.TableName = "TempTable";
                                            dtValue.Columns.Add("EmpSystemID");
                                            dtValue.Columns.Add("SalaryHeadID");
                                            dtValue.Columns.Add("EntryCurrencyID");
                                            dtValue.Columns.Add("EntryAmount");
                                            dtValue.Columns.Add("EarningCurrencyID");
                                            dtValue.Columns.Add("EarningAmount");

                                            dtValue = para.dsSalInfo.Tables[0];

                                            #endregion Salary Amount Insert Into Virtual Table

                                            if (dtValue.Rows.Count > 0)
                                            {
                                                for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                                                {
                                                    #region Clear

                                                    sBonusPolicyDetailsID = "";
                                                    sFormulaDesID = "";

                                                    decFixedValue = 0;
                                                    decEmpCtbtnAmount = 0;
                                                    decEarningValueRangeFrom = 0;
                                                    decEarningValueRangeTo = 0;

                                                    dtEndDate = System.DateTime.Now;
                                                    bMandatory = false;
                                                    bEarning = false;
                                                    bIsActive = true;
                                                    bIsFixed = false;
                                                    bIsFormula = false;
                                                    bIsDependOnEarning = false;

                                                    #endregion Clear

                                                    #region Select BonusPolicyDetails ID if have multiple column

                                                    sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                                                    sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                    sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                    decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                    decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                                                    if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) < decEarningValueRangeTo)
                                                    {
                                                        bApplicable = true;
                                                    }
                                                    else
                                                    {
                                                        bApplicable = false;
                                                    }
                                                    bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                                                    sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                                                    sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                                                    decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                                                    bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                                                    bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                                                    bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                                                    bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                                                    sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                                                    #endregion Select BonusPolicyDetails ID if have multiple column

                                                    if (bApplicable == true)
                                                    {
                                                        #region Bonus Earning Amount

                                                        if (bIsFixed == true)
                                                        {
                                                            decEmpCtbtnAmount = decFixedValue;
                                                        }
                                                        else if (bIsFormula == true)
                                                        {
                                                            bEarning = bIsDependOnEarning;
                                                            ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                                                            sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                            decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                                                        }

                                                        #endregion Bonus Earning Amount

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                                                        //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                        if (dvBnsMntEmpWiseCal.Count == 0)
                                                        {//Add new block
                                                            sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                            drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                                                        }
                                                        else
                                                        {//edit block
                                                            sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                                                            drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                                                            drBnsMntEmpWiseCal.BeginEdit();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                            drBnsMntEmpWiseCal.EndEdit();
                                                        }

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                                                            dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                            if (dvBnsMntEmpWiseCalSt.Count == 0)
                                                            {//Add new block
                                                                sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                                                            }
                                                            else
                                                            {//edit block
                                                                sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                                                                drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                                                                drBnsMntEmpWiseCalSt.BeginEdit();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                drBnsMntEmpWiseCalSt.EndEdit();
                                                            }
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                        #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        dvBnsMntDist.Table = dtBnsMntDist;
                                                        dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                        if (dvBnsMntDist.Count > 0)
                                                        {
                                                            while (dvBnsMntDist.Count > 0)
                                                            {
                                                                drBnsMntDist = dvBnsMntDist[0].Row;
                                                                drBnsMntDist.Delete();
                                                            }
                                                        }

                                                        if (para.bStructure == true)
                                                        {
                                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                                                            if (dvBnsMntDistSt.Count > 0)
                                                            {
                                                                while (dvBnsMntDistSt.Count > 0)
                                                                {
                                                                    drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                    drBnsMntDistSt.Delete();
                                                                }
                                                            }
                                                        }

                                                        #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        dvBnsPolicyDist.Table = dtBnsPolicyDist;
                                                        dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                                                        if (dvBnsPolicyDist.Count > 0)
                                                        {
                                                            for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                                                            {
                                                                decimal decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }
                                                                    }
                                                                }

                                                                decBonusValue = 0;
                                                                if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                                                                {
                                                                    decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue1 = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue1);
                                                                    decBonusValue = Convert.ToDecimal(sOutValue1);

                                                                    dvBnsMntDist.Table = dtBnsMntDist;
                                                                    dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                    if (dvBnsMntDist.Count == 0)
                                                                    {
                                                                        drBnsMntDist = dtBnsMntDist.NewRow();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                    }
                                                                    else
                                                                    {
                                                                        drBnsMntDist = dvBnsMntDist[0].Row;
                                                                        drBnsMntDist.BeginEdit();
                                                                        drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                        drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                        drBnsMntDist["Value"] = decBonusValue;
                                                                        drBnsMntDist.EndEdit();
                                                                    }

                                                                    if (para.bStructure == true)
                                                                    {
                                                                        dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                        dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDistSt.Count == 0)
                                                                        {
                                                                            drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                            drBnsMntDistSt.BeginEdit();
                                                                            drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                            drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDistSt["Value"] = decBonusValue;
                                                                            drBnsMntDistSt.EndEdit();
                                                                        }

                                                                    }
                                                                }
                                                            }
                                                        }
                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                                                        dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                                                        dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                                                        if (dvBnsEligibleEmp.Count == 1)
                                                        {//Add new block
                                                            drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                                                            drBnsEligibleEmp.EndEdit();
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                                                    }//bApplicable
                                                }//for dsBnsPolicyDtl
                                            }//virtual table # dtValue.Rows.Count 
                                        }
                                    }

                                    //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                }

                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                if (para.bStructure == true)
                                {
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist, dsBnsMntEmpWiseCalSt, dsBnsMntDistSt);
                                }
                                else
                                {
                                    SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist);
                                }
                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                dsBnsEligibleEmp = null;
                                dsBnsMntEmpWiseCal = null;
                            }
                            //if (bMaturity == true)
                            //{
                            //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                            //}
                        }

                        #endregion Tag Employee List

                        #region Untag Employee List

                        GetUnTagEmployeeListWithBonusPolicyMonthlyRetain(para, out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {
                                    GetBonusPolicyMonthlyRetainEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCal);
                                    dtBnsMntEmpWiseCal = dsBnsMntEmpWiseCal.Tables[0];
                                    dvBnsMntEmpWiseCal = new DataView();

                                    GetBonusPolicyMonthlyRetainDistributionPmt(para, sEmpSystemID, out dsBnsMntDist);
                                    dtBnsMntDist = dsBnsMntDist.Tables[0];
                                    dvBnsMntDist = new DataView();

                                    if (para.bStructure == true)
                                    {
                                        GetBonusPolicyMonthlyRetainStrcEmpWiseCalculation(para, sEmpSystemID, out dsBnsMntEmpWiseCalSt);
                                        dtBnsMntEmpWiseCalSt = dsBnsMntEmpWiseCalSt.Tables[0];
                                        dvBnsMntEmpWiseCalSt = new DataView();

                                        GetBonusPolicyMonthlyRetainDistributionStrcPmt(para, sEmpSystemID, out dsBnsMntDistSt);
                                        dtBnsMntDistSt = dsBnsMntDistSt.Tables[0];
                                        dvBnsMntDistSt = new DataView();
                                    }

                                    GetBonusPolicyMonthlyRetainEligibleEmployee(sEmpSystemID, out dsBnsEligibleEmp);
                                    dtBnsEligibleEmp = dsBnsEligibleEmp.Tables[0];
                                    dvBnsEligibleEmp = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfoNew = new List<dicSalInfoNew>();
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfoNew = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                    sBnsElgGentID = "";
                                    sBnsDedGentID = "";
                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_ELIGIBLE", dsUnTagEmp.Tables[0].Rows.Count, out sBnsElgGentID);
                                    sBnsElgGentID = "BNSE" + sBnsElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "BONUS_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sBnsDedGentID);
                                    sBnsDedGentID = "BNSC" + sBnsDedGentID;
                                    for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                    {
                                        sBnsEligibleEmpID = sBnsElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                        sEmpLocationID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EmployeeLocationId"].ToString().Trim();
                                        //sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["GivenDesignationId"].ToString().Trim();
                                        sEmpGivenDesignationId = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["LegalDesignationId"].ToString().Trim();
                                        bCalculatedBns = false;

                                        #region Master Table Data Capture [Start Date]

                                        dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                        #endregion Master Table Data Capture 

                                        string sMatDt = "";
                                        //string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                        if (dsBnsMonthNo.Tables[0].Rows.Count > 0)
                                        {
                                            for (int iMnt = 0; iMnt < dsBnsMonthNo.Tables[0].Rows.Count; iMnt++)
                                            {
                                                sMatDt = "01-" + dsBnsMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                if (dtStartDate.Month == Convert.ToDateTime(sMatDt).Month && dtStartDate.Year == Convert.ToDateTime(sMatDt).Year)
                                                {
                                                    bIsEligibleApp = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bIsEligibleApp = true;
                                        }

                                        if (bIsEligibleApp == true)
                                        {
                                            var dicSalInfo_Sub = dicSalInfoNew.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                            if (dicSalInfo_Sub.Count > 0)
                                            {
                                                //for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                //{
                                                sSalaryRuleMasterSystemID = dicSalInfo_Sub[0].SalaryRuleMasterSystemID;
                                                sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                //}
                                            }

                                            #region Salary Amount Insert Into Virtual Table

                                            dtValue = new DataTable();
                                            dtValue.TableName = "TempTable";
                                            dtValue.Columns.Add("EmpSystemID");
                                            dtValue.Columns.Add("SalaryHeadID");
                                            dtValue.Columns.Add("EntryCurrencyID");
                                            dtValue.Columns.Add("EntryAmount");
                                            dtValue.Columns.Add("EarningCurrencyID");
                                            dtValue.Columns.Add("EarningAmount");

                                            dtValue = para.dsSalInfo.Tables[0];

                                            #endregion Salary Amount Insert Into Virtual Table

                                            if (dtValue.Rows.Count > 0)
                                            {
                                                for (int iBnsPlDtl = 0; iBnsPlDtl < dsBnsPolicyDtl.Tables[0].Rows.Count; iBnsPlDtl++)
                                                {
                                                    #region Clear

                                                    sBonusPolicyDetailsID = "";
                                                    sFormulaDesID = "";

                                                    decFixedValue = 0;
                                                    decEmpCtbtnAmount = 0;
                                                    decEarningValueRangeFrom = 0;
                                                    decEarningValueRangeTo = 0;

                                                    dtEndDate = System.DateTime.Now;
                                                    bMandatory = false;
                                                    bEarning = false;
                                                    bIsActive = true;
                                                    bIsFixed = false;
                                                    bIsFormula = false;
                                                    bIsDependOnEarning = false;

                                                    #endregion Clear

                                                    #region Select BonusPolicyDetails ID if have multiple column

                                                    sBonusPolicyDetailsID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();
                                                    sEarningFormulaID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sEarningFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                    sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                    decEarningValueRangeFrom = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                    decEarningValueRangeTo = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["EarningValueRangeTo"].ToString().Trim());

                                                    if (Convert.ToDecimal(sEarningFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sEarningFormulaResult) < decEarningValueRangeTo)
                                                    {
                                                        bEligible = true;
                                                        bApplicable = true;
                                                        bCalculatedBns = true;
                                                    }
                                                    else
                                                    {
                                                        bApplicable = false;
                                                    }

                                                    #region If Bonus applicable for all employee

                                                    if (bIsAllEmpApplocable == true && iBnsPlDtl == dsBnsPolicyDtl.Tables[0].Rows.Count && bApplicable == false && bCalculatedBns == false)
                                                    {
                                                        bApplicable = true;
                                                    }

                                                    #endregion If Bonus applicable for all employee

                                                    bMandatory = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMandatory"].ToString().Trim());
                                                    sBnsDtlID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["ID"].ToString().Trim();

                                                    sFormulaDesID = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FormulaDesID"].ToString().Trim();
                                                    decFixedValue = Convert.ToDecimal(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["FixedValue"].ToString().Trim());
                                                    bIsFixed = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFixed"].ToString().Trim());
                                                    bIsFormula = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsFormula"].ToString().Trim());
                                                    bIsDependOnEarning = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsDependOnEarning"].ToString().Trim());

                                                    bIsMinWages = Convert.ToBoolean(dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["IsMinWages"].ToString().Trim());
                                                    sCompMinWagesAndOrginal = dsBnsPolicyDtl.Tables[0].Rows[iBnsPlDtl]["CompMinWagesAndOrginal"].ToString().Trim();

                                                    #endregion Select BonusPolicyDetails ID if have multiple column

                                                    if (bEligible == true)
                                                    {
                                                        if (bApplicable == true)
                                                        {
                                                            #region Bonus Earning Amount

                                                            if (bIsFixed == true)
                                                            {
                                                                decEmpCtbtnAmount = decFixedValue;
                                                            }
                                                            else if (bIsFormula == true)
                                                            {
                                                                bEarning = bIsDependOnEarning;
                                                                ReLoadFormulaWithValueForBonusCal(sEmpSysID, para, sFormulaDesID, bEarning, bIsMinWages, sCompMinWagesAndOrginal, sSalaryRuleMasterSystemID, sEmpLocationID, sEmpGivenDesignationId, ref dtValue, ref dtSalHd, ref dtSlrGrd, ref dtMinWagSalary, ref dtDw);
                                                                sEarningFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                                decEmpCtbtnAmount = Convert.ToDecimal(sEarningFormulaResult);
                                                            }

                                                            #endregion Bonus Earning Amount

                                                            #region Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                            dvBnsMntEmpWiseCal.Table = dtBnsMntEmpWiseCal;
                                                            //dvBnsMntEmpWiseCal.RowFilter = "ID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                            dvBnsMntEmpWiseCal.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                            if (dvBnsMntEmpWiseCal.Count == 0)
                                                            {//Add new block
                                                                sBnsMntEmpCalID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                drBnsMntEmpWiseCal = dtBnsMntEmpWiseCal.NewRow();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                                dtBnsMntEmpWiseCal.Rows.Add(drBnsMntEmpWiseCal);
                                                            }
                                                            else
                                                            {//edit block
                                                                sBnsMntEmpCalID = dvBnsMntEmpWiseCal[0]["ID"].ToString();

                                                                drBnsMntEmpWiseCal = dvBnsMntEmpWiseCal[0].Row;
                                                                drBnsMntEmpWiseCal.BeginEdit();
                                                                UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCal);
                                                                drBnsMntEmpWiseCal.EndEdit();
                                                            }

                                                            if (para.bStructure == true)
                                                            {
                                                                dvBnsMntEmpWiseCalSt.Table = dtBnsMntEmpWiseCalSt;
                                                                dvBnsMntEmpWiseCalSt.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "' AND MonthNo = " + para.iMonth + " AND YearNo = " + para.iYear + "";
                                                                if (dvBnsMntEmpWiseCalSt.Count == 0)
                                                                {//Add new block
                                                                    sBnsMntEmpCalStID = sBnsDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();

                                                                    drBnsMntEmpWiseCalSt = dtBnsMntEmpWiseCalSt.NewRow();
                                                                    UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("ADDNEW", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                    dtBnsMntEmpWiseCalSt.Rows.Add(drBnsMntEmpWiseCalSt);
                                                                }
                                                                else
                                                                {//edit block
                                                                    sBnsMntEmpCalStID = dvBnsMntEmpWiseCalSt[0]["ID"].ToString();

                                                                    drBnsMntEmpWiseCalSt = dvBnsMntEmpWiseCalSt[0].Row;
                                                                    drBnsMntEmpWiseCalSt.BeginEdit();
                                                                    UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation("EDIT", sEmpSysID, sBnsMntEmpCalStID, sBnsMstID, decEmpCtbtnAmount, para, ref drBnsMntEmpWiseCalSt);
                                                                    drBnsMntEmpWiseCalSt.EndEdit();
                                                                }
                                                            }

                                                            #endregion Data Save IN Table [BonusPolicyMonthlyRetainEmpWiseCalculation]

                                                            #region Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                            dvBnsMntDist.Table = dtBnsMntDist;
                                                            dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "'";
                                                            if (dvBnsMntDist.Count > 0)
                                                            {
                                                                while (dvBnsMntDist.Count > 0)
                                                                {
                                                                    drBnsMntDist = dvBnsMntDist[0].Row;
                                                                    drBnsMntDist.Delete();
                                                                }
                                                            }

                                                            if (para.bStructure == true)
                                                            {
                                                                dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "'";
                                                                if (dvBnsMntDistSt.Count > 0)
                                                                {
                                                                    while (dvBnsMntDistSt.Count > 0)
                                                                    {
                                                                        drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                        drBnsMntDistSt.Delete();
                                                                    }
                                                                }
                                                            }

                                                            #endregion Data Delete IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                            #region Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]

                                                            dvBnsPolicyDist.Table = dtBnsPolicyDist;
                                                            dvBnsPolicyDist.RowFilter = "BonusPolicyDetailsID = '" + sBonusPolicyDetailsID + "'";
                                                            if (dvBnsPolicyDist.Count > 0)
                                                            {
                                                                for (int idist = 0; idist < dvBnsPolicyDist.Count; idist++)
                                                                {
                                                                    decimal decBonusValue = 0;
                                                                    if (dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() != "")
                                                                    {
                                                                        decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["FstValue"].ToString())) / 100;

                                                                        dvCurRl.Table = dtCurRl;
                                                                        dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                        if (dvCurRl.Count > 0)
                                                                        {
                                                                            sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                            bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                            bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                            iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                        }
                                                                        string sOutValue = "0";
                                                                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                        decBonusValue = Convert.ToDecimal(sOutValue);

                                                                        dvBnsMntDist.Table = dtBnsMntDist;
                                                                        dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDist.Count == 0)
                                                                        {
                                                                            drBnsMntDist = dtBnsMntDist.NewRow();
                                                                            drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                            drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDist["Value"] = decBonusValue;
                                                                            dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDist = dvBnsMntDist[0].Row;
                                                                            drBnsMntDist.BeginEdit();
                                                                            drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                            drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                            drBnsMntDist["Value"] = decBonusValue;
                                                                            drBnsMntDist.EndEdit();
                                                                        }

                                                                        if (para.bStructure == true)
                                                                        {
                                                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString() + "'";
                                                                            if (dvBnsMntDistSt.Count == 0)
                                                                            {
                                                                                drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                                drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                drBnsMntDistSt["Value"] = decBonusValue;
                                                                                dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                            }
                                                                            else
                                                                            {
                                                                                drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                                drBnsMntDistSt.BeginEdit();
                                                                                drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["FstSalaryHeadID"].ToString();
                                                                                drBnsMntDistSt["Value"] = decBonusValue;
                                                                                drBnsMntDistSt.EndEdit();
                                                                            }
                                                                        }
                                                                    }

                                                                    decBonusValue = 0;
                                                                    if (dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() != "")
                                                                    {
                                                                        decBonusValue = (decEmpCtbtnAmount * Convert.ToDecimal(dvBnsPolicyDist[idist].Row["SndValue"].ToString())) / 100;

                                                                        dvCurRl.Table = dtCurRl;
                                                                        dvCurRl.RowFilter = "SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                        if (dvCurRl.Count > 0)
                                                                        {
                                                                            sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                            bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                            bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                            iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                        }
                                                                        string sOutValue = "0";
                                                                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decBonusValue.ToString(), out sOutValue);
                                                                        decBonusValue = Convert.ToDecimal(sOutValue);

                                                                        dvBnsMntDist.Table = dtBnsMntDist;
                                                                        dvBnsMntDist.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                        if (dvBnsMntDist.Count == 0)
                                                                        {
                                                                            drBnsMntDist = dtBnsMntDist.NewRow();
                                                                            drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                            drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDist["Value"] = decBonusValue;
                                                                            dtBnsMntDist.Rows.Add(drBnsMntDist);
                                                                        }
                                                                        else
                                                                        {
                                                                            drBnsMntDist = dvBnsMntDist[0].Row;
                                                                            drBnsMntDist.BeginEdit();
                                                                            drBnsMntDist["BnsPlyMntRetainID"] = sBnsMntEmpCalID;
                                                                            drBnsMntDist["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                            drBnsMntDist["Value"] = decBonusValue;
                                                                            drBnsMntDist.EndEdit();
                                                                        }

                                                                        if (para.bStructure == true)
                                                                        {
                                                                            dvBnsMntDistSt.Table = dtBnsMntDistSt;
                                                                            dvBnsMntDistSt.RowFilter = "BnsPlyMntRetainID = '" + sBnsMntEmpCalStID.Trim() + "' AND SalaryHeadID = '" + dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString() + "'";
                                                                            if (dvBnsMntDistSt.Count == 0)
                                                                            {
                                                                                drBnsMntDistSt = dtBnsMntDistSt.NewRow();
                                                                                drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                drBnsMntDistSt["Value"] = decBonusValue;
                                                                                dtBnsMntDistSt.Rows.Add(drBnsMntDistSt);
                                                                            }
                                                                            else
                                                                            {
                                                                                drBnsMntDistSt = dvBnsMntDistSt[0].Row;
                                                                                drBnsMntDistSt.BeginEdit();
                                                                                drBnsMntDistSt["BnsPlyMntRetainID"] = sBnsMntEmpCalStID;
                                                                                drBnsMntDistSt["SalaryHeadID"] = dvBnsPolicyDist[idist].Row["SndSalaryHeadID"].ToString();
                                                                                drBnsMntDistSt["Value"] = decBonusValue;
                                                                                drBnsMntDistSt.EndEdit();
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            #endregion Data Save IN Table [BonusPolicyMonthlyRetainDistributionPmt]
                                                        }
                                                        #region Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]

                                                        dvBnsEligibleEmp.Table = dtBnsEligibleEmp;
                                                        dvBnsEligibleEmp.RowFilter = "ID = '" + sBnsEligibleEmpID.Trim() + "'";
                                                        if (dvBnsEligibleEmp.Count == 0)
                                                        {//Add new block
                                                            drBnsEligibleEmp = dtBnsEligibleEmp.NewRow();
                                                            UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee("ADDNEW", sBnsEligibleEmpID.Trim(), sEmpSysID, sBnsMstID, sBnsDtlID, dtStartDate, bMandatory, bIsAllEmpApplocable, para.sUser, ref drBnsEligibleEmp);
                                                            dtBnsEligibleEmp.Rows.Add(drBnsEligibleEmp);
                                                        }

                                                        #endregion Data Save IN Table [BonusPolicyMonthlyRetainEligibleEmployee]
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                //if (SelectedEmpCnt == grdRowMaxCnt)
                                //{
                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                SaveDataSets(dsBnsEligibleEmp, dsBnsMntEmpWiseCal, dsBnsMntDist, dsBnsMntEmpWiseCalSt, dsBnsMntDistSt);
                                //}
                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                dsBnsEligibleEmp = null;
                                dsBnsMntEmpWiseCal = null;
                            }
                            //if (bMaturity == true)
                            //{
                            //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                            //}
                        }

                        #endregion Untag Employee
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Clear DataSet 

                dsBnsEligibleEmp = null;
                dtBnsEligibleEmp = null;
                drBnsEligibleEmp = null;
                dvBnsEligibleEmp = null;

                dsBnsMntEmpWiseCal = null;
                dtBnsMntEmpWiseCal = null;
                drBnsMntEmpWiseCal = null;
                dvBnsMntEmpWiseCal = null;

                dsSalInfo = null;
                dsBnsPolicyMst = null;
                dsBnsPolicyDtl = null;
                dsBnsMonthNo = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function

        private void UpdateTheDataRowInTableBonusPolicyMonthlyRetainEligibleEmployee(string OPN_FLAG, string sBnsEligibleEmpID, string sEmpSysID, string sBnsMstID, string sBnsDtlID, DateTime dtStartDate, bool bMandatory, bool bIsAllEmpApplocable, string sUser, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["ID"] = RetValidLen(sBnsEligibleEmpID);
                    drLocal["EmpSystemID"] = RetValidLen(sEmpSysID);
                    drLocal["BnsPlcMthRetainID"] = RetValidLen(sBnsMstID);
                    drLocal["BonusPolicyDetailsID"] = RetValidLen(sBnsDtlID);
                    drLocal["StartDate"] = dtStartDate;

                    drLocal["IsMaturity"] = true;
                    drLocal["IsMandatory"] = bMandatory;
                    if (bMandatory == true)
                    {
                        drLocal["IsActive"] = bMandatory;
                        drLocal["IsApproved"] = bMandatory;
                    }
                    else if (bIsAllEmpApplocable == true)
                    {
                        drLocal["IsActive"] = bIsAllEmpApplocable;
                        drLocal["IsApproved"] = bIsAllEmpApplocable;
                    }
                    else
                    {
                        drLocal["IsActive"] = false;
                        drLocal["IsApproved"] = false;
                    }

                    drLocal["AddedBy"] = RetValidLen(sUser);
                    drLocal["AddedDate"] = DateTime.Now.ToString();
                    drLocal["AddedFromIP"] = "";
                }

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
        private void UpdateTheDataRowInTableBonusPolicyMonthlyRetainEmpWiseCalculation(string OPN_FLAG, string sEmpSysID, string sBnsMntEmpCalID, string sBnsMstID, decimal decEmpCtbtnAmount, BnsParaListNew para, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["ID"] = RetValidLen(sBnsMntEmpCalID);

                    drLocal["AddedBy"] = RetValidLen(para.sUser);
                    drLocal["AddedDate"] = DateTime.Now.ToString();
                    drLocal["AddedFromIP"] = "";
                }

                drLocal["EmpSystemID"] = RetValidLen(sEmpSysID);
                drLocal["BnsPlcMthRetainID"] = RetValidLen(sBnsMstID);
                if (para.sSlrProcMstSystemID != "")
                {
                    drLocal["SlrProcMstSystemID"] = RetValidLen(para.sSlrProcMstSystemID);
                }
                drLocal["MonthNo"] = Convert.ToDateTime(para.ToDate).Month;
                drLocal["YearNo"] = Convert.ToDateTime(para.ToDate).Year;
                drLocal["EarningAmount"] = decEmpCtbtnAmount;

                drLocal["UpdatedBy"] = RetValidLen(para.sUser);
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
    public class BnsParaListNew
    {
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public string sEmpSystemID { get; set; }
        public string sSlrProcMstSystemID { get; set; }
        public string sSalaryRuleMasterSystemID { get; set; }
        public string sCurrencyRuleSystemID { get; set; }
        public string LocalCurrencyID { get; set; }
        public string ForeignCurRate { get; set; }
        public string FromDate { get; set; }
        public string EDate { get; set; }
        public string ToDate { get; set; }
        public int iMonth { get; set; } = 0;
        public int iYear { get; set; } = 0;
        public string sUser { get; set; }
        public DataSet dsSalInfo { get; set; }
        public DataSet dsDw { get; set; }
        public bool bStructure { get; set; } = false;
        public bool IsBonusMandatory { get; set; } = false;
        public bool ShouldNotProcessUntaggedEmp { get; set; } = false;
        public bool IsBonusRtnPolicyDefined { get; set; } = false;
        public bool IsBonusEntitle { get; set; } = false;
        public string IsbuttonPFClicked { get; set; } = "NO";

    }
    public class dicBnsMinWagSlrNew
    {
        public string DesignationId { get; set; }
        public string LegalSalaryStructureId { get; set; }
        public string SalaryHead { get; set; }
        public string SalaryHeadId { get; set; }
        public decimal SalaryHeadValue { get; set; } = 0;
    }
}