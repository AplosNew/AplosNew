using bplib;
using ConnectionManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Library.HumanResource.Payroll.SalaryProcessActive
{
    public class clsPFProcess
    {
        public string sFormulaValue = "";

        public clsPFProcess()
        {
            // TODO: Add constructor logic here
        }
        public void GetPFPolicyMaster(string sPFMstSystemID, string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sPFMstSystemID != "")
                {
                    strSQL = @"SELECT *
                                FROM PFPolicyMaster 
                              WHERE ID = '" + sPFMstSystemID + @"'
                                    AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";
                }
                else
                {
                    strSQL = @"SELECT *
                                FROM PFPolicyMaster
                                WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";
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
        public void GetPFPolicyDetails(string sPFMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                                FROM [dbo].[PFPolicyDetails] WHERE PFPolicyMasterID = '" + sPFMstSystemID + @"'";

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
        public void GetEnumEligibility(string EmpSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"select * from (
                //                        SELECT Id,SalaryHeadEnum,EmpSystemId EmpInfoSystemID,SalaryStructureId,IsEligible FROM EmployeeEligibleForSalaryHeadEnum
                //                        ) x
                //              where (" + EmpSystemId + ") ";
                strSQL = @"select * from (
                                        SELECT Id,SalaryHeadEnum,EmpSystemId ,SalaryStructureId,IsEligible FROM EmployeeEligibleForSalaryHeadEnum
                                        ) x
                              where EmpSystemId IN (" + EmpSystemId + ") ";

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
        public void GetPFPFEmployeeDistribution(string sPFMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM PFEmployeeDistribution
                              WHERE PFPolicyDetailsID IN (SELECT ID FROM [dbo].[PFPolicyDetails] WHERE PFPolicyMasterID = '" + sPFMstSystemID + @"')";

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
        public void GetPFPFEmployerDistribution(string sPFMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM PFEmployerDistribution
                              WHERE PFPolicyDetailsID IN (SELECT ID FROM [dbo].[PFPolicyDetails] WHERE PFPolicyMasterID = '" + sPFMstSystemID + @"')";

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

        public void GetPFEmployeeApplied(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM PFEmployeeApplied WHERE IsEligible = 0";

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
        public void GetPFEligibleEmployee(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT *
                                        FROM PFEligibleEmployee 
                                      WHERE " + sEmpSystemID + @"";
                }
                else
                {
                    strSQL = @"SELECT *
                                        FROM PFEligibleEmployee";
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
        public void GetPFMonthlyEmpWiseCalculation(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT *
                                        FROM PFMonthlyEmpWiseCalculation 
                                        WHERE PFEligibleEmpID IN (SELECT ID FROM PFEligibleEmployee 
                                                                        WHERE " + sEmpSystemID + @")";
                }
                else
                {
                    strSQL = @"SELECT *
                                        FROM PFMonthlyEmpWiseCalculation";
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
        public void GetPFMonthlyDistributionEmployee(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT *
                                        FROM PFMonthlyDistributionEmployee 
                                      WHERE PFMntEmpWiseCalID IN (
                                                                  SELECT ID FROM PFMonthlyEmpWiseCalculation 
                                                                    WHERE PFEligibleEmpID IN (SELECT ID FROM PFEligibleEmployee 
                                                                                                WHERE " + sEmpSystemID + @")
                                                                 )";
                }
                else
                {
                    strSQL = @"SELECT *
                                        FROM PFMonthlyDistributionEmployee";
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
        public void GetPFMonthlyDistributionEmployer(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT *
                                        FROM PFMonthlyDistributionEmployer 
                                      WHERE PFMntEmpWiseCalID IN (
                                                                  SELECT ID FROM PFMonthlyEmpWiseCalculation 
                                                                    WHERE PFEligibleEmpID IN (SELECT ID FROM PFEligibleEmployee 
                                                                                                WHERE " + sEmpSystemID + @")
                                                                 )";
                }
                else
                {
                    strSQL = @"SELECT *
                                        FROM PFMonthlyDistributionEmployer";
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

        public void GetUnTagEmployeeListWithPFPolicyMaster(ParaList para, string sPFMstSystemID, string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (para.sEmpSystemID == "")
                {
                    strSQL = @"SELECT DM.PFPolicyMasterID, PFPLMst.Eligibility, PFPLMst.EligibilityBaseOn, PFPLMst.EligibilityTimeLenght, PFPLMst.MaturityBaseOn, 
	                                  PFPLMst.MaturityTimeLenght, ISNULL(VPF.VoluntaryPFValue, 0) VoluntaryPFValue, E.*,
                                      (CONVERT(int, CONVERT(char(8), CONVERT(date, '" + para.FromDate + @"'), 112)) - CONVERT(char(8), E.DOB, 112)) / 10000 AS AgeIntYears 
                                FROM [dbo].[EmployeeInformation] E
										    --INNER JOIN (SELECT * FROM [dbo].[PFEmployeeApplied] WHERE IsEligible = 0) EA ON E.SystemId = EA.EmpSystemId
			                                INNER JOIN (
                                                          SELECT DC.LeavePolicyMasterId, DC.PFPolicyMasterID, D.DesignationId 
                                                            FROM MST.DesignationMaster D
												                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
												            WHERE DC.PlantId='" + sPlantID + @"'
                                                         ) DM ON E.GivenDesignationId = DM.DesignationId
										    --LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                LEFT JOIN [dbo].[PFPolicyMaster] PFPLMst ON DM.PFPolicyMasterID = PFPLMst.ID
                                            LEFT JOIN (
                                                       SELECT * FROM [dbo].[PFEmployeeVoluntaryValue] 
                                                        WHERE  MONTH(EffectiveDate) = MONTH('" + para.FromDate + @"') AND YEAR(EffectiveDate) = YEAR('" + para.FromDate + @"')
                                                      ) VPF ON E.SystemId = VPF.EmpSystemID
                                WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantId = '" + sPlantID + @"' --AND E.EmployeeStatus = 'Active'
                                     AND E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[PFEligibleEmployee])
	                                  AND DM.PFPolicyMasterID = '" + sPFMstSystemID + @"'";
                }
                else
                {
                    strSQL = @"SELECT DM.PFPolicyMasterID, PFPLMst.Eligibility, PFPLMst.EligibilityBaseOn, PFPLMst.EligibilityTimeLenght, PFPLMst.MaturityBaseOn, 
	                                   PFPLMst.MaturityTimeLenght, ISNULL(VPF.VoluntaryPFValue, 0) VoluntaryPFValue, E.*,
                                      (CONVERT(int, CONVERT(char(8), CONVERT(date, '" + para.FromDate + @"'), 112)) - CONVERT(char(8), E.DOB, 112)) / 10000 AS AgeIntYears 
                                FROM [dbo].[EmployeeInformation] E
										    --INNER JOIN (SELECT * FROM [dbo].[PFEmployeeApplied] WHERE IsEligible = 0) EA ON E.SystemId = EA.EmpSystemId
			                                INNER JOIN (
                                                        SELECT DC.LeavePolicyMasterId, DC.PFPolicyMasterID, D.DesignationId 
                                                         FROM MST.DesignationMaster D
												                LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
												        WHERE DC.PlantId='" + sPlantID + @"'
                                                       ) DM ON E.GivenDesignationId = DM.DesignationId
										    --LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                LEFT JOIN [dbo].[PFPolicyMaster] PFPLMst ON DM.PFPolicyMasterID = PFPLMst.ID
                                            LEFT JOIN (
                                                       SELECT * FROM [dbo].[PFEmployeeVoluntaryValue] 
                                                        WHERE  MONTH(EffectiveDate) = MONTH('" + para.FromDate + @"') AND YEAR(EffectiveDate) = YEAR('" + para.FromDate + @"')
                                                      ) VPF ON E.SystemId = VPF.EmpSystemID
                                WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantId = '" + sPlantID + @"' --AND E.EmployeeStatus = 'Active'
                                    and  E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[PFEligibleEmployee])
	                                  AND DM.PFPolicyMasterID = '" + sPFMstSystemID + @"' AND E.SystemId IN (" + para.sEmpSystemID + @")";
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
        public void GetEmpwithPFPolicyMaster(ParaList para, string sPFMstSystemID, string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            //ConnectionManager.DAL.ConManager objCon;
            ConnectionManager.clsConnectionManager con;
            try
            {
                //string _WC = " WHERE IsApproved = 1 ";
                //if (para.ShouldNotProcessUntaggedEmp)
                //{
                //    _WC = " WHERE IsApproved = 1 and IsActive=1 ";
                //}

                strSQL = @"SELECT  DM.PFPolicyMasterID,  ISNULL(VPF.VoluntaryPFValue, 0) VoluntaryPFValue, E.*,
                                      (CONVERT(int, CONVERT(char(8), CONVERT(date, '" + para.FromDate + @"'), 112)) - CONVERT(char(8), E.DOB, 112)) / 10000 AS AgeIntYears 
                                FROM [dbo].[EmployeeInformation] E			                                
                                              INNER JOIN (
                                                          SELECT DC.LeavePolicyMasterId, DC.PFPolicyMasterID, D.DesignationId 
                                                            FROM MST.DesignationMaster D
												                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
												            WHERE DC.PlantId='" + sPlantID + @"'
                                                         ) DM ON E.GivenDesignationId = DM.DesignationId										      
			                                  LEFT JOIN [dbo].[PFPolicyMaster] PFPLMst ON DM.PFPolicyMasterID = PFPLMst.ID
                                              LEFT JOIN (
														  SELECT A.* FROM [dbo].[PFEmployeeVoluntaryValue] A
														   INNER JOIN 
																   (
																	SELECT EmpSystemId, MAX(EffectiveDate) EffectiveDate FROM [dbo].[PFEmployeeVoluntaryValue] 
																	 WHERE  CONVERT(date, EffectiveDate) <= CONVERT(date, '" + para.ToDate + @"') 
																	GROUP BY EmpSystemId
																   ) B ON A.EmpSystemId = B.EmpSystemId AND A.EffectiveDate = B.EffectiveDate

                                                                    where a.SalaryStructureId in (select systemid from SalaryInfoDefineMaster where PlantID= '" + sPlantID + @"')
																   or a.SalaryStructureId in (select systemid from SalaryInfoBackMaster where PlantID= '" + sPlantID + @"')

                                                        ) VPF ON E.SystemId = VPF.EmpSystemID
                                WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantId = '" + sPlantID + @"' 
                                    and  E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND DM.PFPolicyMasterID = '" + sPFMstSystemID + @"' AND E.SystemId IN (" + para.sEmpSystemID + @")";

                strSQL += @"
                                ORDER BY E.GivenDesignationId, E.SystemId";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                con = new clsConnectionManager(3600);
                con.getDataSet(strSQL, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetTagEmployeeListWithPFPolicyMaster(ParaList para, string sPFMstSystemID, string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string _WC = " WHERE IsApproved = 1 ";
                if (para.ShouldNotProcessUntaggedEmp)
                {
                    _WC = " WHERE IsApproved = 1 and IsActive=1 ";
                }
                if (para.sEmpSystemID == "")
                {
                    strSQL = @"SELECT PFEL.ID PFEligibleEmpID, DM.PFPolicyMasterID, PFPLMst.Eligibility, PFPLMst.EligibilityBaseOn, PFPLMst.EligibilityTimeLenght, PFPLMst.MaturityBaseOn, 
	                                   PFPLMst.MaturityTimeLenght, ISNULL(VPF.VoluntaryPFValue, 0) VoluntaryPFValue, PFEL. IsActive, E.*,
                                      (CONVERT(int, CONVERT(char(8), CONVERT(date, '" + para.FromDate + @"'), 112)) - CONVERT(char(8), E.DOB, 112)) / 10000 AS AgeIntYears 
                                FROM [dbo].[EmployeeInformation] E
			                                --INNER JOIN [MST].[DesignationMaster] DM ON E.GivenDesignationId = DM.DesignationId
                                            INNER JOIN (
                                                          SELECT DC.LeavePolicyMasterId, DC.PFPolicyMasterID, D.DesignationId 
                                                            FROM MST.DesignationMaster D
												                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
												            WHERE DC.PlantId='" + sPlantID + @"'
                                                         )  DM ON E.GivenDesignationId = DM.DesignationId
										    INNER JOIN (SELECT * FROM [dbo].[PFEligibleEmployee]  " + _WC + @") PFEL ON E.SystemId = PFEL.EmpSystemID
			                                LEFT JOIN [dbo].[PFPolicyMaster] PFPLMst ON DM.PFPolicyMasterID = PFPLMst.ID
                                            LEFT JOIN (
														SELECT A.* FROM [dbo].[PFEmployeeVoluntaryValue] A
														INNER JOIN 
																   (
																	SELECT EmpSystemId, MAX(EffectiveDate) EffectiveDate FROM [dbo].[PFEmployeeVoluntaryValue] 
																	 WHERE  CONVERT(date, EffectiveDate) <= CONVERT(date, '" + para.FromDate + @"') 
																	GROUP BY EmpSystemId
																   ) B ON A.EmpSystemId = B.EmpSystemId AND A.EffectiveDate = B.EffectiveDate
                                                      ) VPF ON E.SystemId = VPF.EmpSystemID
                                WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantId = '" + sPlantID + @"' --AND E.EmployeeStatus = 'Active'
                                      AND E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND DM.PFPolicyMasterID = '" + sPFMstSystemID + @"'";
                }
                else
                {
                    strSQL = @"SELECT PFEL.ID PFEligibleEmpID, DM.PFPolicyMasterID, PFPLMst.Eligibility, PFPLMst.EligibilityBaseOn, PFPLMst.EligibilityTimeLenght, PFPLMst.MaturityBaseOn, 
	                                   PFPLMst.MaturityTimeLenght, ISNULL(VPF.VoluntaryPFValue, 0) VoluntaryPFValue, PFEL.IsActive, E.*,
                                      (CONVERT(int, CONVERT(char(8), CONVERT(date, '" + para.FromDate + @"'), 112)) - CONVERT(char(8), E.DOB, 112)) / 10000 AS AgeIntYears 
                                FROM [dbo].[EmployeeInformation] E
			                                --INNER JOIN [MST].[DesignationMaster] DM ON E.GivenDesignationId = DM.DesignationId
                                              INNER JOIN (
                                                          SELECT DC.LeavePolicyMasterId, DC.PFPolicyMasterID, D.DesignationId 
                                                            FROM MST.DesignationMaster D
												                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
												            WHERE DC.PlantId='" + sPlantID + @"'
                                                         ) DM ON E.GivenDesignationId = DM.DesignationId
										      INNER JOIN (SELECT * FROM [dbo].[PFEligibleEmployee] " + _WC + @") PFEL ON E.SystemId = PFEL.EmpSystemID
			                                  LEFT JOIN [dbo].[PFPolicyMaster] PFPLMst ON DM.PFPolicyMasterID = PFPLMst.ID
                                              LEFT JOIN (
														  SELECT A.* FROM [dbo].[PFEmployeeVoluntaryValue] A
														   INNER JOIN 
																   (
																	SELECT EmpSystemId, MAX(EffectiveDate) EffectiveDate FROM [dbo].[PFEmployeeVoluntaryValue] 
																	 WHERE  CONVERT(date, EffectiveDate) <= CONVERT(date, '" + para.FromDate + @"') 
																	GROUP BY EmpSystemId
																   ) B ON A.EmpSystemId = B.EmpSystemId AND A.EffectiveDate = B.EffectiveDate
                                                        ) VPF ON E.SystemId = VPF.EmpSystemID
                                WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantId = '" + sPlantID + @"' --AND E.EmployeeStatus = 'Active'
                                    and  E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND DM.PFPolicyMasterID = '" + sPFMstSystemID + @"' AND E.SystemId IN (" + para.sEmpSystemID + @")";
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
        public void ReLoadFormulaWithValueNew(string sEmpSystemID, ParaList para, string strFormulaID, out string sFormulaValue, bool bEarning, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSlrHd)
        {
            DataSet dsLocal = null;
            //DataView dvLocal = null;
            //DataView dvSlrHd = null;
            string strTemp = "";

            try
            {
                //dtValue
                //List<SPvalueHeadWise> list = new List<SPvalueHeadWise>();
                //list= dtValue.ToList<SPvalueHeadWise>();

                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();
                string sLocalCurrencyID = para.LocalCurrencyID;
                string sForeignCurRate = para.ForeignCurRate;

                if (sForeignCurRate == "")
                { sForeignCurRate = "1"; }

                sFormulaValue = "";

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
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == ">" || strTemp.Trim() == "<" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        //dvLocal = new DataView();
                        //dvLocal.Table = dtValue;

                        var dtv = dtValue.FindAll(x => x.SalaryHeadID == strTemp.Trim() && x.EmpSystemID == sEmpSystemID);
                        // dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                        if (dtv.Count() > 0)
                        {
                            if (bEarning == false)
                            {
                                if (dtv[0].EntryCurrencyID == para.LocalCurrencyID.Trim())
                                {
                                    strTemp = dtv[0].EntryAmount;
                                }
                                else
                                {
                                    strTemp = (Convert.ToDecimal(dtv[0].EntryAmount) * Convert.ToDecimal(para.ForeignCurRate.Trim())).ToString();
                                }

                                //if (dvLocal[0]["EntryCurrencyID"].ToString().Trim() == para.lblLocalCurrencyID.Trim())
                                //{
                                //    strTemp = dvLocal[0]["EntryAmount"].ToString().Trim();
                                //}
                                //else
                                //{
                                //    strTemp = (Convert.ToDecimal(dvLocal[0]["EntryAmount"].ToString().Trim()) * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                                //}
                            }
                            else
                            {
                                //decimal decAmount = Convert.ToDecimal(Convert.ToDecimal(dvLocal[0]["EarningAmount"].ToString().Trim()).ToString("0.00"));
                                decimal decAmount = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EarningAmount).ToString("0.00"));

                                if (decAmount == 0)
                                { decAmount = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EntryAmount).ToString("0.00")); }

                                if (dtv[0].EarningCurrencyID == para.LocalCurrencyID.Trim())
                                {
                                    strTemp = Convert.ToDecimal(dtv[0].EarningAmount).ToString("0.00");
                                }
                                else
                                {
                                    strTemp = (decAmount * Convert.ToDecimal(para.ForeignCurRate.Trim())).ToString();
                                }
                            }
                        }
                        else
                        {
                            var dicsh = dicSlrHd.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                            if (dicsh.Count() > 0)
                            {
                                strTemp = "0.00";
                            }
                            // var dvSPChd_dic = dicProcChild.FindAll(x => x.EmpInfoSystemID == dicLocal_Sub[i].EmpInfoSystemID && x.SalaryHeadID == dicLocal_Sub[i].SalaryHeadID && x.SlrProcMstSystemID == para.lblSalaryProcSystemId.Trim());

                            //dicsal
                            //dvSlrHd = new DataView();
                            //dvSlrHd.Table = dtSlrHd;
                            //dvSlrHd.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                            //if (dvSlrHd.Count == 1)
                            //{
                            //    strTemp = "0.00";
                            //}
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
        private void ReLoadFormulaWithValue(string sEmpSystemID, ParaList para, string sFormulaID, bool bEarning, ref DataTable dtValue, ref DataTable dtSlrHd)
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

                if (sForeignCurRate == "")
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
        private void xReLoadFormulaWithValue(string sEmpSystemID, ParaList para, string sFormulaID, bool bEarning, ref DataTable dtValue, ref DataTable dtSlrHd)
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

                if (sForeignCurRate == "")
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
        public void xLoadEmpSlrDefForSlrProcess(ParaList para, string sEmpInfo, out DataSet dsRef)
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
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay

                                            ,ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
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
                                                 SELECT SalaryRuleMasterSystemID, g.SalaryHeadID, 'Gen' RuleType,  h.PartOfNetPay  IsNetPayEffect, FixedMonthDayValue, IsMonthDay, 
						                                IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleGeneral g
														left join SalaryHead h on h.SalaryHeadID=g.SalaryHeadID
						                            UNION
					                             (
                                                  SELECT SalaryRuleMasterSystemID, g.SalaryHeadID, 'Abs' RuleType,  h.PartOfNetPay  IsNetPayEffect, FixedMonthDayValue, IsMonthDay, 
						                                 IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleAbsenteeism  g
														left join SalaryHead h on h.SalaryHeadID=g.SalaryHeadID
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
        public void LoadEmpSlrDefForSlrProcess(ParaList para, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                //      strSql = @"SELECT * FROM 
                //                    (
                //                           SELECT SD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                //                                  SD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate,	
                //                                  SD.EntryCurrencyID, ECR.Name AS EntryCurrency, SD.EntryAmount, SD.DefineCurrencyID, SD.SalaryID,
                //                                  DECR.Name AS DefinitionCurrency, SD.DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                //                                  AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
                //                                          ELSE SD.SalaryHeadID END,
                //                         CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
                //                         SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay, SlrDis.IsBankPayment, SlrDis.IsCashPayment,
                //                                  ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
                //	IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
                //						  ELSE SlrDis.IsNetPayEffect END,
                //	SlrProc.DisbusmentAmount EarningAmount, SlrProc.DisbusmentCurrencyID EarningCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                //                                  ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo, SRM.CurrencyRuleSystemID  
                //                    FROM (
                //                                SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
                //                                    AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
                //                                FROM SalaryInfoDefine
                //                                  UNION
                //                                (
                //                                 SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
                //                                  AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated                   
                //                                 FROM SalaryInfoBack
                //                                )
                //                               ) SD
                //INNER JOIN 
                //		(
                //		 SELECT SLM.* FROM 
                //                                                  (
                //                                                   SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
                //                                                    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                //                                                   FROM SalaryInfoDefineMaster
                //                                                   UNION 
                //                                                  (
                //                                                   SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
                //                                                    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                //                                                   FROM SalaryInfoBackMaster
                //                                                  )
                //                                                  ) SLM 
                //                                                   INNER JOIN
                //                                                     (
                //                                                      SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
                //                                                      FROM 
                //                                                       (
                //                                                         SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
                //                                                          IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                //                                                         FROM SalaryInfoDefineMaster
                //                                                       UNION 
                //                                                        (
                //                                                       SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
                //                                                        IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                //                                                       FROM SalaryInfoBackMaster
                //                                                        )
                //                                                       ) A
                //                                                      WHERE IsApproved = 1 AND EffectiveDate <= '" + para.ToDate + @"'
                //                                                      GROUP BY EmpInfoSystemID
                //                                                     ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
                //		) SEFD ON SD.SalaryID = SEFD.SystemID
                //                     INNER JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
                //                     INNER JOIN SalaryHead SH ON SD.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax'
                //                     INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
                //                     LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SD.SalaryHeadID = CRC.SalaryHeadID
                //                     LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
                //                     LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
                //                     LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                //                     LEFT JOIN 
                //                       (
                //                                       SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Gen' RuleType, IsGNRNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, ISNULL(IsBankPayment, Convert(bit, 'True')) IsBankPayment, ISNULL(IsCashPayment, Convert(bit, 'True')) IsCashPayment,
                //                            IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleGeneral
                //                        UNION
                //                        (
                //                                        SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Abs' RuleType, IsAbsNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, Convert(bit, 'True') IsBankPayment, Convert(bit, 'True') IsCashPayment, 
                //                             IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleAbsenteeism
                //                                       )
                //                                      ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SD.SalaryHeadID = SlrDis.SalaryHeadID
                //                     LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
                //	                            AND SD.SalaryHeadID = SRDSM.SalaryHeadID
                //LEFT JOIN 
                //       (
                //		SELECT * FROM [dbo].[SalaryProcChild]
                //			WHERE SlrProcMstSystemID IN (
                //										 SELECT SystemID FROM [dbo].[SalaryProcMaster]
                //										  WHERE MonthNo = MONTH('" + para.ToDate + @"') AND YearNo = YEAR('" + para.ToDate + @"')
                //										)
                //	   ) SlrProc ON E.SystemID = SlrProc.EmpInfoSystemID 
                //	                            AND SD.SalaryHeadID = SlrProc.SalaryHeadID 
                //                              WHERE E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                //                                                                     OR E.DOS = '' OR E.DOS = '01/01/1901')
                //                                    AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + para.ToDate + @"'
                //                          ) A 
                //                        WHERE (" + sEmpInfo + @") ";



                strSql = @"SELECT * FROM 
		                            (
                                     SELECT SEFD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                                            SEFD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SEFD.AmtDefinitionCurrencyID, SEFD.AmtDefinitionRate,	
                                            SEFD.EntryCurrencyID, ECR.Name AS EntryCurrency, SEFD.EntryAmount, SEFD.DefineCurrencyID, SEFD.SalaryID,
                                            DECR.Name AS DefinitionCurrency, SEFD.DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                              ELSE SEFD.SalaryHeadID END,
			                                CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay, SlrDis.IsBankPayment, SlrDis.IsCashPayment,
                                            ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
											IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
																  ELSE SlrDis.IsNetPayEffect END,
											SlrProc.DisbusmentAmount EarningAmount, SlrProc.DisbusmentCurrencyID EarningCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                            ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo, SRM.CurrencyRuleSystemID  
		                                          FROM (
		                            	
		                            	SELECT * FROM (  SELECT  *,
				DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                    from (
							                    SELECT SD.SystemID,SDM.PlantID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,
								                    SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                    from SalaryInfoDefineMaster SDM
								                    JOIN SalaryInfoDefine AS SD ON sdm.SystemID=SD.SalaryID 
                                                    WHERE (" + sEmpInfo + @") AND SDM.IsApproved=1
								                    union ALL
								                    select SD.SystemID,SDM.PlantID,EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,
								                    SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                     from SalaryInfoBackMaster SDM
								                    JOIN SalaryInfoBack AS SD ON sdm.SystemID=SD.SalaryID 
                                                    WHERE (" + sEmpInfo + @") AND SDM.IsApproved=1
							
			                    ) AS SDM
			
			            ) AS SDM 
                        WHERE ISNULL(sdm.IsApproved,'')=1 AND EffectiveDate <= '" + para.ToDate + @"' AND rnk=1 
		                            ) SEFD 
			                            LEFT JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
			                            LEFT JOIN SalaryHead SH ON SEFD.SalaryHeadID = SH.SalaryHeadID 
			                            LEFT JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
			                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SEFD.SalaryHeadID = CRC.SalaryHeadID
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
                                                ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SEFD.SalaryHeadID = SlrDis.SalaryHeadID
			                            LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
											                            AND SEFD.SalaryHeadID = SRDSM.SalaryHeadID
										LEFT JOIN 
										       (
												SELECT * FROM [dbo].[SalaryProcChild]
													WHERE SlrProcMstSystemID IN (
																				 SELECT SystemID FROM [dbo].[SalaryProcMaster]
																				  WHERE MonthNo = MONTH('" + para.ToDate + @"') AND YearNo = YEAR('" + para.ToDate + @"')
																				)
											   ) SlrProc ON E.SystemID = SlrProc.EmpInfoSystemID 
											                            AND SEFD.SalaryHeadID = SlrProc.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax'
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
        public void Load_Salary_Struc(ParaList para, string sEmpInfo, bool IsBack, out DataSet dsRef)
        {
            //ConnectionManager.DAL.ConManager objCon;
            ConnectionManager.clsConnectionManager con;
            string strSql = "";

            try
            {
                string kk = @"      SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
	                                             AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
                                          FROM SalaryInfoDefine";
                if (IsBack)
                {
                    kk = @" SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
                                                  AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
                                           FROM SalaryInfoBack";
                }
                //      strSql = @"SELECT * FROM 
                //                    (
                //                           SELECT SD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                //                                  SD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate,	
                //                                  SD.EntryCurrencyID, ECR.Name AS EntryCurrency, SD.EntryAmount, SD.DefineCurrencyID, SD.SalaryID,
                //                                  DECR.Name AS DefinitionCurrency, SD.DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                //                                  AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
                //                                          ELSE SD.SalaryHeadID END,
                //                         CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
                //                         SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay, SlrDis.IsBankPayment, SlrDis.IsCashPayment,
                //                                  ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
                //	IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
                //						  ELSE SlrDis.IsNetPayEffect END,
                //	SlrProc.DisbusmentAmount EarningAmount, SlrProc.DisbusmentCurrencyID EarningCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                //                                  ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo, SRM.CurrencyRuleSystemID  
                //                    FROM (

                //                                "+kk+@"

                //                               ) SD
                //INNER JOIN 
                //		(
                //		 SELECT SLM.* FROM 
                //                                                  (
                //                                                   SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
                //                                                    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                //                                                   FROM SalaryInfoDefineMaster
                //                                                   UNION 
                //                                                  (
                //                                                   SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
                //                                                    IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                //                                                   FROM SalaryInfoBackMaster
                //                                                  )
                //                                                  ) SLM 
                //                                                   INNER JOIN
                //                                                     (
                //                                                      SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
                //                                                      FROM 
                //                                                       (
                //                                                         SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
                //                                                          IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                //                                                         FROM SalaryInfoDefineMaster
                //                                                       UNION 
                //                                                        (
                //                                                       SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
                //                                                        IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                //                                                       FROM SalaryInfoBackMaster
                //                                                        )
                //                                                       ) A
                //                                                      WHERE IsApproved = 1 AND EffectiveDate <= '" + para.ToDate + @"'
                //                                                      GROUP BY EmpInfoSystemID
                //                                                     ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
                //		) SEFD ON SD.SalaryID = SEFD.SystemID
                //                     INNER JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
                //                     INNER JOIN SalaryHead SH ON SD.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax'
                //                     INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
                //                     LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SD.SalaryHeadID = CRC.SalaryHeadID
                //                     LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
                //                     LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
                //                     LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                //                     LEFT JOIN 
                //                       (
                //                                       SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Gen' RuleType, IsGNRNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, ISNULL(IsBankPayment, Convert(bit, 'True')) IsBankPayment, ISNULL(IsCashPayment, Convert(bit, 'True')) IsCashPayment,
                //                            IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleGeneral
                //                        UNION
                //                        (
                //                                        SELECT SalaryRuleMasterSystemID, SalaryHeadID, 'Abs' RuleType, IsAbsNetPayEffect IsNetPayEffect, FixedMonthDayValue, IsMonthDay, Convert(bit, 'True') IsBankPayment, Convert(bit, 'True') IsCashPayment, 
                //                             IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleAbsenteeism
                //                                       )
                //                                      ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SD.SalaryHeadID = SlrDis.SalaryHeadID
                //                     LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
                //	                            AND SD.SalaryHeadID = SRDSM.SalaryHeadID
                //LEFT JOIN 
                //       (
                //		SELECT * FROM [dbo].[SalaryProcChild]
                //			WHERE SlrProcMstSystemID IN (
                //										 SELECT SystemID FROM [dbo].[SalaryProcMaster]
                //										  WHERE MonthNo = MONTH('" + para.ToDate + @"') AND YearNo = YEAR('" + para.ToDate + @"')
                //										)
                //	   ) SlrProc ON E.SystemID = SlrProc.EmpInfoSystemID 
                //	                            AND SD.SalaryHeadID = SlrProc.SalaryHeadID 
                //                              WHERE E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                //                                                                     OR E.DOS = '' OR E.DOS = '01/01/1901')
                //                                    AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + para.ToDate + @"'
                //                          ) A 
                //                        WHERE (" + sEmpInfo + @") ";


                strSql = @"SELECT * FROM 
                                    (
                                           SELECT SEFD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                                                  SEFD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SEFD.AmtDefinitionCurrencyID, SEFD.AmtDefinitionRate,	
                                                  SEFD.EntryCurrencyID, ECR.Name AS EntryCurrency, SEFD.EntryAmount, SEFD.DefineCurrencyID, SEFD.SalaryID,
                                                  DECR.Name AS DefinitionCurrency, SEFD.DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                                  AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
                                                          ELSE SEFD.SalaryHeadID END,
                                         CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
                                         SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay, SlrDis.IsBankPayment, SlrDis.IsCashPayment,
                                                  ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
                	IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
                						  ELSE SlrDis.IsNetPayEffect END,
                	SlrProc.DisbusmentAmount EarningAmount, SlrProc.DisbusmentCurrencyID EarningCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                                  ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo, SRM.CurrencyRuleSystemID  
                                     FROM (
		                            	
		                            	SELECT * FROM (  SELECT  *,
				DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                    from (
							                    SELECT SD.SystemID,SDM.PlantID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,
								                    SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                    from SalaryInfoDefineMaster SDM
								                    JOIN SalaryInfoDefine AS SD ON sdm.SystemID=SD.SalaryID 
                                                    WHERE EmpInfoSystemID IN (" + sEmpInfo + @") AND SDM.IsApproved=1
								                    union ALL
								                    select SD.SystemID,SDM.PlantID,EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, EffectiveDate,IsApproved, DateApproved,
								                    SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, AmtDefinitionCurrencyID, AmtDefinitionRate  
								                     from SalaryInfoBackMaster SDM
								                    JOIN SalaryInfoBack AS SD ON sdm.SystemID=SD.SalaryID 
                                                    WHERE EmpInfoSystemID IN (" + sEmpInfo + @") AND SDM.IsApproved=1
							
			                    ) AS SDM
			
			            ) AS SDM 
                        WHERE ISNULL(sdm.IsApproved,'')=1 AND EffectiveDate <= '" + para.ToDate + @"' AND rnk=1 
		                            ) SEFD 
                                     left JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
                                     LEFT JOIN SalaryHead SH ON SEFD.SalaryHeadID = SH.SalaryHeadID 
                                     left JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
                                     LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SEFD.SalaryHeadID = CRC.SalaryHeadID
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
                                                      ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SEFD.SalaryHeadID = SlrDis.SalaryHeadID
                                     LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
                	                            AND SEFD.SalaryHeadID = SRDSM.SalaryHeadID
                LEFT JOIN 
                       (
                		SELECT * FROM [dbo].[SalaryProcChild]
                			WHERE SlrProcMstSystemID IN (
                										 SELECT SystemID FROM [dbo].[SalaryProcMaster]
                										  WHERE MonthNo = MONTH('" + para.ToDate + @"') AND YearNo = YEAR('" + para.ToDate + @"')
                										)
                	   ) SlrProc ON E.SystemID = SlrProc.EmpInfoSystemID 
                	                            AND SEFD.SalaryHeadID = SlrProc.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax'
                                              WHERE E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                                                                                     OR E.DOS = '' OR E.DOS = '01/01/1901')
                                                    AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + para.ToDate + @"'
                                          ) A 
                                        --WHERE EmpInfoSystemID IN (" + sEmpInfo + @") ";

                if (para.PlantID != "ALL" & para.PlantID != "")
                {
                    strSql += @" AND PlantID = '" + para.PlantID + @"' ";
                }

                strSql += @"
                            ORDER BY EmpInfoSystemID, HeadType DESC";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                con = new clsConnectionManager(3600);
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void LoadEmpSlrDefForSlrProcessFORVPF(ParaList para, string sEmpInfo, out DataSet dsRef)
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
				                                                            WHERE IsApproved = 0 AND EffectiveDate <= '" + para.ToDate + @"'
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
                                              AND SEFD.IsApproved = 0 AND SEFD.EffectiveDate <= '" + para.ToDate + @"'
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
        public void LoadCurrencyRule(ParaList para, out System.Data.DataSet dsRef)
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
        public void GetPFEligibleEmpByPK(string emppk, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select* from PFEligibleEmployee  where EmpSystemID='" + emppk + "'";

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


        public void CalculateEarnPF(ParaList para, out List<EmpSalaryHeadAmount> _List_PFHeadValue)
        {
            #region Variable Dataset

            DataSet dsSalInfo = null;
            DataSet dsSalHd = null;
            DataTable dtSalHd = null;
            DataView dvSlrHd = null;
            DataSet dsPFPolicyMst = null;
            DataSet dsPFPolicyDtl = null;
            DataSet dsPFEmpDisb = null;
            DataSet dsPFEmprDisb = null;
            DataSet dsUnTagEmp = null;
            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;
            clsSalaryUtility obSS = new clsSalaryUtility();

            #endregion Variable Dataset
            #region Declare Variable

            string sPFEligibleEmpID = "";
            string sPFMntEmpCalID = "";
            string sPFMstID = "";
            string sPFDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sFormulaID = "";
            string sFormulaResult = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEntCurID = "";
            string sEarnCurID = "";
            string sSlrHD = "";
            string sFormulaDesIDEmpDis = "";
            string sFormulaDesIDEmprDis = "";
            string sSalaryHeadIDEmp = "";
            string sResidualValueSlrHdIDEmp = "";
            string sSalaryHeadIDEmpr = "";
            string sResidualValueSlrHdIDEmpr = "";
            string sAlwnSlrHd = "";
            string sRoundOption = "";
            string sCurrencyRuleSystemID = "";

            string sPFContSalaryHeadIDEmp = "";
            string sPFContSalaryHeadIDEmpr = "";
            string sPFVoluntarySalaryHeadID = "";

            DateTime dtEligibilityDate;
            DateTime dtStartDate;
            DateTime dtMaturityDate;

            decimal decValueEmp = 0;
            decimal decUpperLimitEmp = 0;
            decimal decValueEmpr = 0;
            decimal decUpperLimitEmpr = 0;
            decimal decValueTempEmpr = 0;
            decimal decEntCur = 0;
            decimal decEarnCur = 0;
            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValueEmp = 0;
            decimal decFixedValueEmpr = 0;
            decimal decEmpCtbtnAmount = 0;
            decimal decEmprCtbtnAmount = 0;
            decimal decEmpCtbtnAmountTemp = 0;
            decimal decEmprCtbtnAmountTemp = 0;
            decimal decPFVoluntary = 0;
            decimal decPFVoluntaryPer = 0;
            decimal decEmpCntValPer = 0;
            decimal decEmployerCntValPer = 0;

            int decEligibilityTimeLenght = 0;
            int decMaturityTimeLenght = 0;
            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iAgeLimit = 0;
            int iAgeIntYears = 0;
            int iDecimalNo = 0;

            bool bEmpNotEntGetEmplrAlwn = false;
            bool bPFELIsActive = false;
            bool bMaturity = false;
            bool bIsFixedEmp = false;
            bool bIsFormulaEmp = false;
            bool bIsContributionSlrHDdependOnEarningEmp = false;
            bool bIsDistributionEmp = false;

            bool bIsFixedEmpr = false;
            bool bIsFormulaEmpr = false;
            bool bIsContributionSlrHDdependOnEarningEmpr = false;
            bool bIsDistributionEmpr = false;
            bool bEarning = false;
            bool bVoluntaryPF = false;
            bool bNotEntGetEmplrAlwn = false;
            bool bIndividualAlwn = false;
            bool bIsAllEmpApplocable = false;
            bool bIsAgeLimit = false;
            bool bIsAgeLimitDistributionEmpr = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;

            #endregion Declare Variable

            try
            {
                _List_PFHeadValue = new List<EmpSalaryHeadAmount>();
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();
                List<SPvalueHeadWise> dtValue = new List<SPvalueHeadWise>();


                GetPFPolicyMaster("", sGroupID.Trim(), sPlantID.Trim(), out dsPFPolicyMst);
                if (dsPFPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int PFPlCnt = 0; PFPlCnt < dsPFPolicyMst.Tables[0].Rows.Count; PFPlCnt++)
                    {
                        sPFMstID = dsPFPolicyMst.Tables[0].Rows[PFPlCnt]["ID"].ToString().Trim();
                        bIsAllEmpApplocable = Convert.ToBoolean(dsPFPolicyMst.Tables[0].Rows[PFPlCnt]["IsAllEmpApplocable"].ToString().Trim());

                        #region DataSet

                        GetPFPolicyDetails(sPFMstID, out dsPFPolicyDtl);

                        List<dicPFEmpDisb> dicPFEmpDisb = new List<dicPFEmpDisb>();
                        GetPFPFEmployeeDistribution(sPFMstID, out dsPFEmpDisb);
                        if (dsPFEmpDisb.Tables[0].Rows.Count > 0)
                            dicPFEmpDisb = dsPFEmpDisb.Tables[0].ToList<dicPFEmpDisb>();

                        List<dicPFEmprDisb> dicPFEmprDisb = new List<dicPFEmprDisb>();
                        GetPFPFEmployerDistribution(sPFMstID, out dsPFEmprDisb);
                        if (dsPFEmprDisb.Tables[0].Rows.Count > 0)
                            dicPFEmprDisb = dsPFEmprDisb.Tables[0].ToList<dicPFEmprDisb>();

                        string strTemp = "PF Voluntary";
                        string sVPFSLRHD = "";
                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];
                        dvSlrHd = new DataView();
                        dvSlrHd.Table = dtSalHd;
                        dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                        if (dvSlrHd.Count > 0)
                        { sVPFSLRHD = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }
                        ///=============================
                        List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                        DataView dvsh = new DataView(dsSalHd.Tables[0]);
                        DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                        if (dtSalHdx.Rows.Count > 0)
                            dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();
                        //====================================================================
                        #endregion DataSet

                        #region Tag Employee List

                        GetEmpwithPFPolicyMaster(para, sPFMstID.Trim(), sGroupID.Trim(), sPlantID.Trim(), out dsUnTagEmp);
                        //GetTagEmployeeListWithPFPolicyMaster(para, sPFMstID.Trim(), sGroupID.Trim(), sPlantID.Trim(), out dsUnTagEmp); 
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

                                #region loop
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
                                        sEmpInfoSysIDColl = "'" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "'" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += ",'" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += ",'" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {
                                    #region DataSet

                                    //GetPFEligibleEmployee(sEmpSystemID, out dsPFEligibleEmp);
                                    //dtPFEligibleEmp = dsPFEligibleEmp.Tables[0];
                                    //dvPFEligibleEmp = new DataView();

                                    //GetPFMonthlyEmpWiseCalculation(sEmpSystemID, out dsPFMntEmpWiseCal);
                                    //dtPFMntEmpWiseCal = dsPFMntEmpWiseCal.Tables[0];
                                    //dvPFMntEmpWiseCal = new DataView();

                                    //GetPFMonthlyDistributionEmployee(sEmpSystemID, out dsPFMntDisEmp);
                                    //dtPFMntDisEmp = dsPFMntDisEmp.Tables[0];
                                    //dvPFMntDisEmp = new DataView();

                                    //GetPFMonthlyDistributionEmployer(sEmpSystemID, out dsPFMntDisEmpr);
                                    //dtPFMntDisEmpr = dsPFMntDisEmpr.Tables[0];
                                    //dvPFMntDisEmpr = new DataView();

                                    //Get General Salary Amount Head Wise
                                    DataSet dsSalInfoBack = null;
                                    List<dicSalInfo> dicSalInfo = new List<dicSalInfo>();
                                    List<dicSalInfo> dicSalInfoBack = new List<dicSalInfo>();
                                    Load_Salary_Struc(para, sEmpInfoSysIDColl, false, out dsSalInfo);
                                    //Load_Salary_Struc(para, sEmpInfoSysIDColl,true, out dsSalInfoBack);

                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfo>();

                                    //if (dsSalInfoBack.Tables[0].Rows.Count > 0)
                                    //    dicSalInfoBack = dsSalInfoBack.Tables[0].ToList<dicSalInfo>();

                                    DataSet dsEnumPF = null;
                                    bool IsPFHolder = false;
                                    //string _ssid = string.Empty;
                                    List<EmployeeEligibleForSalaryHeadEnumSAL> dicEnum = new List<EmployeeEligibleForSalaryHeadEnumSAL>();
                                    //GetEnumEligibility(sEmpInfoSysIDColl,out dsEnumPF);//and SalaryHeadEnum='PF' and IsEligible=1
                                    GetEnumEligibility(sEmpSystemID, out dsEnumPF);//and SalaryHeadEnum='PF' and IsEligible=1
                                    if (dsEnumPF.Tables[0].Rows.Count > 0)
                                        dicEnum = dsEnumPF.Tables[0].ToList<EmployeeEligibleForSalaryHeadEnumSAL>();


                                    #endregion DataSet

                                    
                                    for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                    {
                                         sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();

                                        iAgeIntYears = Convert.ToInt32(bplib.clsWebLib.GetNumData(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["AgeIntYears"].ToString().Trim()));
                                        decPFVoluntaryPer = Convert.ToDecimal(bplib.clsWebLib.GetNumData(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["VoluntaryPFValue"].ToString()));
                                        bIsAgeLimitDistributionEmpr = true;
                                        bEmpNotEntGetEmplrAlwn = false;
                                                                               
                                        #region Salary Amount Insert Into Virtual Table

                                        dtValue = para.dtValue;
                                        bVoluntaryPF = false;
                                        IsPFHolder = false;

                                        List<dicSalInfo> dicSalInfo_Sub = null;
                                        GetSubList(dicSalInfo, dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim(), out dicSalInfo_Sub);

                                        if (dicSalInfo_Sub.Count > 0)
                                        {
                                            sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                            string _sstructureid = dicSalInfo_Sub[0].SalaryID;

                                            var dicSubEnumPF = dicEnum.FindAll(x => x.EmpSystemId == dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() && x.SalaryStructureId == _sstructureid && x.SalaryHeadEnum == "PF" && x.IsEligible == true);
                                            if (dicSubEnumPF.Count > 0)
                                            {
                                                IsPFHolder = true;
                                            }

                                            var dicSubEnumVPF = dicEnum.FindAll(x => x.EmpSystemId == dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() && x.SalaryStructureId == _sstructureid && x.SalaryHeadEnum == "VPF" && x.IsEligible == true);
                                            if (dicSubEnumVPF.Count > 0)
                                            {
                                                bVoluntaryPF = true;
                                            }

                                            if (para.dicProcChild.Count == 0)
                                            {
                                                for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                {
                                                    sSlrHD = dicSalInfo_Sub[i].SalaryHeadID;
                                                    sEntCurID = dicSalInfo_Sub[i].EntryCurrencyID;
                                                    decEntCur = dicSalInfo_Sub[i].EntryAmount;
                                                    sEarnCurID = dicSalInfo_Sub[i].EarningCurrencyID;
                                                    decEarnCur = dicSalInfo_Sub[i].EarningAmount;

                                                    iDecimalNo = dicSalInfo_Sub[i].DecimalNo;
                                                    bIntegerInDisb = dicSalInfo_Sub[i].IntegerInDisb;
                                                    bIsDecimalInDisb = dicSalInfo_Sub[i].IsDecimalInDisb;
                                                    sRoundOption = dicSalInfo_Sub[i].RoundOption;
                                                    if (para.dsSalInfo == null)
                                                    {
                                                        #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                        dtValue = para.dtValue;
                                                       
                                                        #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                                                        if (dicSalInfo_Sub[i].HeadCategory == "PF Employee Contribution")
                                                        {
                                                            sPFContSalaryHeadIDEmp = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                        if (dicSalInfo_Sub[i].HeadCategory == "PF Employer Contribution")
                                                        {
                                                            sPFContSalaryHeadIDEmpr = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                        if (dicSalInfo_Sub[i].HeadCategory == "PF Voluntary")
                                                        {
                                                            sPFVoluntarySalaryHeadID = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        //if (para.dsSalInfo != null)
                                        if (para.dicProcChild.Count > 0)
                                        {
                                            dtValue = para.dtValue;
                                            //dtValue = para.dsSalInfo.Tables[0];
                                            strTemp = "PF Employee Contribution";

                                            dvSlrHd = new DataView();
                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            {
                                                sPFContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim();
                                                //IsPFHolder = true;
                                            }

                                            strTemp = "PF Employer Contribution";

                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            {
                                                //sPFContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim();
                                                sPFContSalaryHeadIDEmpr = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim();
                                            }

                                            //sPFContSalaryHeadIDEmp = sVPFSLRHD;
                                            sPFVoluntarySalaryHeadID = sVPFSLRHD;
                                        }

                                        #endregion Salary Amount Insert Into Virtual Table

                                        for (int iPFDtl = 0; iPFDtl < dsPFPolicyDtl.Tables[0].Rows.Count; iPFDtl++)
                                        {
                                            #region Clear
                                            decimal _employeer_amount = 0;
                                            sFormulaDesIDEmpDis = "";
                                            sSalaryHeadIDEmp = "";
                                            sResidualValueSlrHdIDEmp = "";
                                            sFormulaDesIDEmprDis = "";
                                            sSalaryHeadIDEmpr = "";
                                            sResidualValueSlrHdIDEmpr = "";
                                            sAlwnSlrHd = "";
                                            decEmpCntValPer = 0;
                                            decEmployerCntValPer = 0;

                                            bIsFixedEmpr = false;
                                            bIsFormulaEmpr = false;
                                            bIsContributionSlrHDdependOnEarningEmpr = false;
                                            bIsDistributionEmpr = false;
                                            bIsFixedEmp = false;
                                            bIsFormulaEmp = false;
                                            bIsContributionSlrHDdependOnEarningEmp = false;
                                            bIsDistributionEmp = false;

                                            bEarning = false;
                                            //bVoluntaryPF = false;
                                            bNotEntGetEmplrAlwn = false;
                                            bIndividualAlwn = false;

                                            decEmpCtbtnAmount = 0;
                                            decEmprCtbtnAmount = 0;
                                            decUpperLimitEmpr = 0;
                                            decValueTempEmpr = 0;
                                            decValueEmpr = 0;
                                            decFixedValueEmpr = 0;
                                            decValueEmp = 0;
                                            decUpperLimitEmp = 0;
                                            decFixedValueEmp = 0;
                                            decPFVoluntary = 0;

                                            #endregion Clear

                                            #region Select PFPolicyDetails ID if have multiple column

                                            sFormulaID = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FormulaDesIDEarning"].ToString().Trim();
                                            ReLoadFormulaWithValueNew(sEmpSysID, para, sFormulaID, out sFormulaValue, bEarning, dtValue, dicSalaryHead);

                                            // spu.ReLoadFormulaWithValueSalaryProc(sEmpSysID, para, sFormulaID, out sFormulaValue, bEarning, dtValue, dicSalaryHead);

                                            sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                            decEarningValueRangeFrom = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EarningValueRangeFrom"].ToString().Trim());
                                            decEarningValueRangeTo = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EarningValueRangeTo"].ToString().Trim());

                                            if (Convert.ToDecimal(sFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sFormulaResult) < decEarningValueRangeTo)
                                            {
                                                bMaturity = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsMandatory"].ToString().Trim());
                                            }
                                            else
                                            {
                                                bMaturity = false;
                                            }
                                            sPFDtlID = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["ID"].ToString().Trim();

                                            sFormulaDesIDEmpDis = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FormulaDesIDEmpDis"].ToString().Trim();
                                            decFixedValueEmp = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FixedValueEmp"].ToString().Trim());
                                            bIsFixedEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFixedEmp"].ToString().Trim());
                                            bIsFormulaEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFormulaEmp"].ToString().Trim());
                                            bIsContributionSlrHDdependOnEarningEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsContributionSlrHDdependOnEarningEmp"].ToString().Trim());
                                            bIsDistributionEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsDistributionEmp"].ToString().Trim());

                                            decEmpCntValPer = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EmpCntValPer"].ToString().Trim());
                                            decEmployerCntValPer = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EmployerCntValPer"].ToString().Trim());
                                            sFormulaDesIDEmprDis = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FormulaDesIDEmployerDis"].ToString().Trim();
                                            decFixedValueEmpr = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FixedValueEmployer"].ToString().Trim());
                                            bIsFixedEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFixedEmployer"].ToString().Trim());
                                            bIsFormulaEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFormulaEmployer"].ToString().Trim());
                                            bIsContributionSlrHDdependOnEarningEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsContributionSlrHDdependOnEarningEmployer"].ToString().Trim());
                                            bIsDistributionEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsDistributionEmployer"].ToString().Trim());

                                            sAlwnSlrHd = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["AlwnSlrHd"].ToString().Trim();
                                            // bVoluntaryPF = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsVoluntaryPF"].ToString().Trim());
                                            bNotEntGetEmplrAlwn = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsNotEntGetEmplrAlwn"].ToString().Trim());
                                            bIndividualAlwn = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsIndividualAlwn"].ToString().Trim());
                                                                                  

                                            bIsAgeLimit = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsAgeLimit"].ToString().Trim());
                                            if (dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["AgeLimit"].ToString().Trim() != "")
                                            { iAgeLimit = Convert.ToInt32(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["AgeLimit"].ToString().Trim()); }
                                            else { iAgeLimit = 0; }

                                            if (bIsAgeLimit == true)
                                            {
                                                bIsAgeLimitDistributionEmpr = true;
                                                if (iAgeIntYears >= iAgeLimit)
                                                {
                                                    bIsAgeLimitDistributionEmpr = false;
                                                }
                                            }
                                            else
                                            {
                                                bIsAgeLimitDistributionEmpr = false;
                                            }

                                            #endregion Select PFPolicyDetails ID if have multiple column
                                            //if (bPFELIsActive == true)//
                                            if (IsPFHolder)//IsPFHolder
                                            {
                                                #region Employee Contribution Amount

                                                if (bIsFixedEmp == true)
                                                {
                                                    decEmpCtbtnAmount = decFixedValueEmp;
                                                }
                                                else if (bIsFormulaEmp == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmp;
                                                    //ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmpDis, bEarning, ref dtValue, ref dtSalHd);
                                                    ReLoadFormulaWithValueNew(sEmpSysID, para, sFormulaDesIDEmpDis, out sFormulaValue, bEarning, dtValue, dicSalaryHead);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmpCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                GetHeadWiseAmount(decEmpCtbtnAmount, sEmpSysID, sPFContSalaryHeadIDEmp, ref _List_PFHeadValue);
                                              

                                                #endregion Employee Contribution Amount

                                                #region Employer Contribution Amount

                                                if (bIsFixedEmpr == true)
                                                {
                                                    decEmprCtbtnAmount = decFixedValueEmpr;
                                                }
                                                else if (bIsFormulaEmpr == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                                                    //ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmprDis, bEarning, ref dtValue, ref dtSalHd);
                                                    ReLoadFormulaWithValueNew(sEmpSysID, para, sFormulaDesIDEmprDis, out sFormulaValue, bEarning, dtValue, dicSalaryHead);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                if (bIsAgeLimitDistributionEmpr == false)
                                                {
                                                    GetHeadWiseAmount(decEmprCtbtnAmount, sEmpSysID, sPFContSalaryHeadIDEmpr, ref _List_PFHeadValue);
                                                }
                                                //_ERAmount = decEmprCtbtnAmount;
                                                #endregion Employer Contribution Amount

                                                
                                                decEmpCtbtnAmountTemp = decEmpCtbtnAmount;
                                                decEmprCtbtnAmountTemp = decEmprCtbtnAmount;
                                                _employeer_amount = decEmprCtbtnAmount;

                                                decEmpCtbtnAmount = (decEmpCtbtnAmount * 100) / decEmpCntValPer;
                                                decEmprCtbtnAmount = (decEmprCtbtnAmount * 100) / decEmployerCntValPer;

                                                #region Select PFEmployeeDistribution ID if have multiple column
                                                                                              

                                                if (bIsDistributionEmp == true)
                                                {
                                                    var dicPFEmpDisb_Sub = dicPFEmpDisb.FindAll(x => x.PFPolicyDetailsID == dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["ID"].ToString().Trim());
                                                    if (dicPFEmpDisb_Sub.Count > 0)
                                                    {
                                                        for (int iEmpDis = 0; iEmpDis < dicPFEmpDisb_Sub.Count; iEmpDis++)
                                                        {
                                                            decValueEmp = dicPFEmpDisb_Sub[iEmpDis].Value;
                                                            sSalaryHeadIDEmp = dicPFEmpDisb_Sub[iEmpDis].SalaryHeadID;
                                                            decUpperLimitEmp = Convert.ToDecimal(GetNumData(dicPFEmpDisb_Sub[iEmpDis].UpperLimit.ToString()));
                                                            sResidualValueSlrHdIDEmp = dicPFEmpDisb_Sub[iEmpDis].ResidualValueSlrHdID;

                                                            decValueEmp = (decEmpCtbtnAmount * decValueEmp) / 100;

                                                            if (decValueEmp > decUpperLimitEmp)
                                                            {
                                                                decUpperLimitEmp = decValueEmp - decUpperLimitEmp;
                                                            }
                                                            else
                                                            {
                                                                decUpperLimitEmp = 0;
                                                            }

                                                            dvCurRl.Table = dtCurRl;
                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + sSalaryHeadIDEmp + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                            if (dvCurRl.Count > 0)
                                                            {
                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                            }
                                                            string sOutValue = "0";
                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmp.ToString(), out sOutValue);
                                                            decValueEmp = Convert.ToDecimal(sOutValue);

                                                            GetHeadWiseAmount(decValueEmp, sEmpSysID, sSalaryHeadIDEmp, ref _List_PFHeadValue);

                                                            dvCurRl.Table = dtCurRl;
                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + sResidualValueSlrHdIDEmp + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                            if (dvCurRl.Count > 0)
                                                            {
                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                            }
                                                            string sOutValue1 = "0";
                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decUpperLimitEmp.ToString(), out sOutValue1);
                                                            decUpperLimitEmp = Convert.ToDecimal(sOutValue1);

                                                        }
                                                    }
                                                }

                                                #endregion Select PFEmployeeDistribution ID if have multiple column

                                                #region Voluntary PF Data Save IN Table [PFMonthlyEmpWiseCalculation]
                                                if (bVoluntaryPF == true)
                                                {
                                                    if (decPFVoluntaryPer > 0)
                                                    {
                                                        //decEmpCtbtnAmount = (decEmpCtbtnAmount * 100) / decEmpCntValPer;
                                                        decPFVoluntary = (decEmpCtbtnAmount * decPFVoluntaryPer) / 100;
                                                        sPFVoluntarySalaryHeadID = sVPFSLRHD;

                                                        dvCurRl.Table = dtCurRl;
                                                        dvCurRl.RowFilter = "SalaryHeadID = '" + sPFVoluntarySalaryHeadID + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                        if (dvCurRl.Count > 0)
                                                        {
                                                            sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                            bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                            bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                            iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                        }
                                                        string sOutValue = "0";
                                                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decPFVoluntary.ToString(), out sOutValue);
                                                        decPFVoluntary = Convert.ToDecimal(sOutValue);
                                                        // _VPFAmount = decPFVoluntary;
                                                        GetHeadWiseAmount(decPFVoluntary, sEmpSysID, sPFVoluntarySalaryHeadID, ref _List_PFHeadValue);
                                                        
                                                    }
                                                }
                                                #endregion Voluntary PF Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                #region Select PFEmployerDistribution ID if have multiple column

                                              
                                                if (bIsDistributionEmpr == true)
                                                {
                                                    if (bIsAgeLimitDistributionEmpr == true)
                                                    {
                                                        if (dsPFEmprDisb.Tables[0].Rows.Count > 0)
                                                        {
                                                            var dicPFEmprDisb_Sub = dicPFEmprDisb.FindAll(x => x.PFPolicyDetailsID == dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["ID"].ToString().Trim());
                                                            if (dicPFEmprDisb_Sub.Count > 0)
                                                            {
                                                                decimal _cumulative_Total_of_all_head_but_last = 0;
                                                                for (int iEmpDis = 0; iEmpDis < dicPFEmprDisb_Sub.Count; iEmpDis++)
                                                                {
                                                                    bool IsLastHead = false;
                                                                    decValueEmpr = dicPFEmprDisb_Sub[iEmpDis].Value;
                                                                    sSalaryHeadIDEmpr = dicPFEmprDisb_Sub[iEmpDis].SalaryHeadID;
                                                                    decUpperLimitEmpr = Convert.ToDecimal(GetNumData(dicPFEmprDisb_Sub[iEmpDis].UpperLimit.ToString()));
                                                                    sResidualValueSlrHdIDEmpr = dicPFEmprDisb_Sub[iEmpDis].ResidualValueSlrHdID;

                                                                    decValueEmpr = (decEmprCtbtnAmount * decValueEmpr) / 100;

                                                                    if (decUpperLimitEmpr != 0)
                                                                    {
                                                                        if (decValueEmpr > decUpperLimitEmpr)
                                                                        {
                                                                            decValueTempEmpr = decUpperLimitEmpr;
                                                                            decUpperLimitEmpr = decValueEmpr - decUpperLimitEmpr;
                                                                            decValueEmpr = decValueTempEmpr;
                                                                        }
                                                                        else
                                                                        {
                                                                            decUpperLimitEmpr = 0;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        IsLastHead = true;
                                                                    }

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + sSalaryHeadIDEmpr + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmpr.ToString(), out sOutValue);
                                                                    decValueEmpr = Convert.ToDecimal(sOutValue);



                                                                    if (IsLastHead)
                                                                    {
                                                                        //lasthead=total ee - _cumulative_Total_of_all_head_but_last
                                                                        var TotalEmployercont =
                                                                        decValueEmpr = _employeer_amount - _cumulative_Total_of_all_head_but_last;
                                                                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmpr.ToString(), out sOutValue);
                                                                        decValueEmpr = Convert.ToDecimal(sOutValue);
                                                                    }
                                                                    _cumulative_Total_of_all_head_but_last += decValueEmpr;

                                                                    GetHeadWiseAmount(decValueEmpr, sEmpSysID, sSalaryHeadIDEmpr, ref _List_PFHeadValue);
                                                                   

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + sResidualValueSlrHdIDEmpr + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue1 = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decUpperLimitEmpr.ToString(), out sOutValue1);
                                                                    decUpperLimitEmpr = Convert.ToDecimal(sOutValue1);
                                                                    GetHeadWiseAmount(decUpperLimitEmpr, sEmpSysID, sResidualValueSlrHdIDEmpr, ref _List_PFHeadValue);

                                                                    _cumulative_Total_of_all_head_but_last += decUpperLimitEmpr;

                                                                }//for
                                                            }//dicPFEmprDisb_Sub.Count
                                                        }
                                                    }
                                                    else
                                                    {
                                                        
                                                        
                                                    }
                                                }
                                                #endregion Select PFEmployerDistribution ID if have multiple column
                                            }
                                            else if (bPFELIsActive == false && bNotEntGetEmplrAlwn == true)
                                            {
                                                #region Employer Contribution Amount
                                                if (bEmpNotEntGetEmplrAlwn == true)
                                                {
                                                    if (bIsFixedEmpr == true)
                                                    {
                                                        decEmprCtbtnAmount = decFixedValueEmpr;
                                                    }
                                                    else if (bIsFormulaEmpr == true)
                                                    {
                                                        bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                                                        ReLoadFormulaWithValueNew(sEmpSysID, para, sFormulaDesIDEmprDis, out sFormulaValue, bEarning, dtValue, dicSalaryHead);
                                                        //ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmprDis, bEarning, ref dtValue, ref dtSalHd);
                                                        sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                        decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                    }
                                                }

                                                #endregion Employer Contribution Amount
                                                                                

                                                #region If Not Entitle PF Get Allowance Save IN Table PFEmployerDistribution

                                                decValueEmpr = decEmprCtbtnAmount;
                                                sSalaryHeadIDEmpr = sAlwnSlrHd;
                                                decUpperLimitEmpr = 0;
                                                sResidualValueSlrHdIDEmpr = null;

                                                dvCurRl.Table = dtCurRl;
                                                dvCurRl.RowFilter = "SalaryHeadID = '" + sSalaryHeadIDEmpr + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                if (dvCurRl.Count > 0)
                                                {
                                                    sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                    bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                    bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                    iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                }
                                                string sOutValue = "0";
                                                obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmpr.ToString(), out sOutValue);
                                                decValueEmpr = Convert.ToDecimal(sOutValue);

                                            
                                                #endregion If Not Entitle PF Get Allowance Save IN Table PFEmployerDistribution
                                            }
                                        }
                                    }
                                }

                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                
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
                                
                                #endregion
                            }//while
                        }//dsUnTagEmp.Tables[0].Rows.Count 
                        #endregion Tag Employee List
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

                //dsPFEligibleEmp = null;
                //dtPFEligibleEmp = null;
                //drPFEligibleEmp = null;
                //dvPFEligibleEmp = null;

                //dsPFMntEmpWiseCal = null;
                //dtPFMntEmpWiseCal = null;
                //drPFMntEmpWiseCal = null;
                //dvPFMntEmpWiseCal = null;

                //dsPFMntDisEmp = null;
                //dtPFMntDisEmp = null;
                //drPFMntDisEmp = null;
                //dvPFMntDisEmp = null;

                //dsPFMntDisEmpr = null;
                //dtPFMntDisEmpr = null;
                //drPFMntDisEmpr = null;
                //dvPFMntDisEmpr = null;

                dsSalInfo = null;
                dsPFPolicyMst = null;
                dsPFPolicyDtl = null;
                dsPFEmpDisb = null;
                dsPFEmprDisb = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function
        void GetSubList(List<dicSalInfo> dicSalInfo, string empid, out List<dicSalInfo> dicSalInfo_Sub)
        {
            try
            {
                dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == empid);
                var ssmainTable = dicSalInfo.FindAll(x => x.EmpInfoSystemID == empid);
                if (ssmainTable.Count > 0)
                {
                    dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == empid);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetSubList_backup(List<dicSalInfo> dicSalInfo, List<dicSalInfo> dicSalInfoBack, string empid, out List<dicSalInfo> dicSalInfo_Sub)
        {
            try
            {
                var ssmainTable = dicSalInfo.FindAll(x => x.EmpInfoSystemID == empid);
                var ssbackTable = dicSalInfoBack.FindAll(x => x.EmpInfoSystemID == empid);
                if (ssbackTable.Count > 0 && ssmainTable.Count > 0)
                {
                    var edb = ssbackTable[0].EffectiveDate;
                    var edm = ssmainTable[0].EffectiveDate;
                    if (Convert.ToDateTime(edb) > Convert.ToDateTime(edm))
                    {
                        dicSalInfo_Sub = dicSalInfoBack.FindAll(x => x.EmpInfoSystemID == empid);
                    }
                    else
                    {
                        dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == empid);
                    }
                }
                else
                {
                    if (ssbackTable.Count > 0)
                    {
                        dicSalInfo_Sub = dicSalInfoBack.FindAll(x => x.EmpInfoSystemID == empid);
                    }
                    else
                    {
                        dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == empid);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetHeadWiseAmount(decimal decEmpCtbtnAmount, string sEmpSysID, string sPFContSalaryHeadIDEmp, ref List<EmpSalaryHeadAmount> _List_PFHeadValue)
        {
            try
            {
                var _list_sub = _List_PFHeadValue.FindAll(x => x.EmpSystemid == sEmpSysID && x.SalaryHeadId == sPFContSalaryHeadIDEmp);
                if (_list_sub.Count > 0)
                {
                    var ob = _list_sub[0];
                    ob.Amount = ob.Amount + decEmpCtbtnAmount;
                }
                else
                {
                    if (string.IsNullOrEmpty(sPFContSalaryHeadIDEmp) == false)
                    {
                        EmpSalaryHeadAmount oba = new EmpSalaryHeadAmount();
                        oba.EmpSystemid = sEmpSysID;
                        oba.SalaryHeadId = sPFContSalaryHeadIDEmp;
                        oba.Amount = decEmpCtbtnAmount;
                        _List_PFHeadValue.Add(oba);
                    }//shead
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void xGeneratorPFEligibleEmployee(ParaList para)
        {
            #region Variable Dataset

            DataSet dsPFEligibleEmp = null;
            DataTable dtPFEligibleEmp = null;
            DataRow drPFEligibleEmp = null;
            DataView dvPFEligibleEmp = null;

            DataSet dsPFMntEmpWiseCal = null;
            DataTable dtPFMntEmpWiseCal = null;
            DataRow drPFMntEmpWiseCal = null;
            DataView dvPFMntEmpWiseCal = null;

            DataSet dsPFMntDisEmp = null;
            DataTable dtPFMntDisEmp = null;
            DataRow drPFMntDisEmp = null;
            DataView dvPFMntDisEmp = null;

            DataSet dsPFMntDisEmpr = null;
            DataTable dtPFMntDisEmpr = null;
            DataRow drPFMntDisEmpr = null;
            DataView dvPFMntDisEmpr = null;

            DataSet dsSalInfo = null;

            DataSet dsSalHd = null;
            DataTable dtSalHd = null;
            DataView dvSlrHd = null;
            DataSet dsPFPolicyMst = null;
            DataSet dsPFPolicyDtl = null;
            DataSet dsPFEmpDisb = null;
            DataSet dsPFEmprDisb = null;
            DataSet dsUnTagEmp = null;
            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;
            //clsSalaryStructureAplos obSS = new global::clsSalaryStructureAplos();
            clsSalaryUtility obSS = new clsSalaryUtility();

            #endregion Variable Dataset
            #region Declare Variable

            string sPFEligibleEmpID = "";
            string sPFMntEmpCalID = "";
            string sPFMstID = "";
            string sPFDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sPFElgGentID = "";
            string sPFDedGentID = "";
            string sFormulaID = "";
            string sFormulaResult = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEntCurID = "";
            string sEarnCurID = "";
            string sSlrHD = "";
            string sFormulaDesIDEmpDis = "";
            string sFormulaDesIDEmprDis = "";
            string sSalaryHeadIDEmp = "";
            string sResidualValueSlrHdIDEmp = "";
            string sSalaryHeadIDEmpr = "";
            string sResidualValueSlrHdIDEmpr = "";
            string sAlwnSlrHd = "";
            string sRoundOption = "";
            string sCurrencyRuleSystemID = "";

            string sPFContSalaryHeadIDEmp = "";
            string sPFContSalaryHeadIDEmpr = "";
            string sPFVoluntarySalaryHeadID = "";

            DateTime dtEligibilityDate;
            DateTime dtStartDate;
            DateTime dtMaturityDate;

            decimal decValueEmp = 0;
            decimal decUpperLimitEmp = 0;
            decimal decValueEmpr = 0;
            decimal decUpperLimitEmpr = 0;
            decimal decValueTempEmpr = 0;
            decimal decEntCur = 0;
            decimal decEarnCur = 0;
            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValueEmp = 0;
            decimal decFixedValueEmpr = 0;
            decimal decEmpCtbtnAmount = 0;
            decimal decEmprCtbtnAmount = 0;
            decimal decEmpCtbtnAmountTemp = 0;
            decimal decEmprCtbtnAmountTemp = 0;
            decimal decPFVoluntary = 0;
            decimal decPFVoluntaryPer = 0;
            decimal decEmpCntValPer = 0;
            decimal decEmployerCntValPer = 0;

            int decEligibilityTimeLenght = 0;
            int decMaturityTimeLenght = 0;
            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iAgeLimit = 0;
            int iAgeIntYears = 0;
            int iDecimalNo = 0;

            bool bEmpNotEntGetEmplrAlwn = false;
            bool bPFELIsActive = false;
            bool bMaturity = false;
            bool bIsFixedEmp = false;
            bool bIsFormulaEmp = false;
            bool bIsContributionSlrHDdependOnEarningEmp = false;
            bool bIsDistributionEmp = false;

            bool bIsFixedEmpr = false;
            bool bIsFormulaEmpr = false;
            bool bIsContributionSlrHDdependOnEarningEmpr = false;
            bool bIsDistributionEmpr = false;
            bool bEarning = false;
            bool bVoluntaryPF = false;
            bool bNotEntGetEmplrAlwn = false;
            bool bIndividualAlwn = false;
            bool bIsAllEmpApplocable = false;
            bool bIsAgeLimit = false;
            bool bIsAgeLimitDistributionEmpr = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;

            #endregion Declare Variable

            try
            {
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetPFPolicyMaster("", sGroupID.Trim(), sPlantID.Trim(), out dsPFPolicyMst);
                if (dsPFPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int PFPlCnt = 0; PFPlCnt < dsPFPolicyMst.Tables[0].Rows.Count; PFPlCnt++)
                    {
                        sPFMstID = dsPFPolicyMst.Tables[0].Rows[PFPlCnt]["ID"].ToString().Trim();
                        bIsAllEmpApplocable = Convert.ToBoolean(dsPFPolicyMst.Tables[0].Rows[PFPlCnt]["IsAllEmpApplocable"].ToString().Trim());

                        #region DataSet

                        GetPFPolicyDetails(sPFMstID, out dsPFPolicyDtl);

                        List<dicPFEmpDisb> dicPFEmpDisb = new List<dicPFEmpDisb>();
                        GetPFPFEmployeeDistribution(sPFMstID, out dsPFEmpDisb);
                        if (dsPFEmpDisb.Tables[0].Rows.Count > 0)
                            dicPFEmpDisb = dsPFEmpDisb.Tables[0].ToList<dicPFEmpDisb>();

                        List<dicPFEmprDisb> dicPFEmprDisb = new List<dicPFEmprDisb>();
                        GetPFPFEmployerDistribution(sPFMstID, out dsPFEmprDisb);
                        if (dsPFEmprDisb.Tables[0].Rows.Count > 0)
                            dicPFEmprDisb = dsPFEmprDisb.Tables[0].ToList<dicPFEmprDisb>();

                        string strTemp = "PF Voluntary";
                        string sVPFSLRHD = "";
                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];
                        dvSlrHd = new DataView();
                        dvSlrHd.Table = dtSalHd;
                        dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                        if (dvSlrHd.Count > 0)
                        { sVPFSLRHD = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                        #endregion DataSet

                        #region Tag Employee List

                        GetTagEmployeeListWithPFPolicyMaster(para, sPFMstID.Trim(), sGroupID.Trim(), sPlantID.Trim(), out dsUnTagEmp);
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
                                    if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() == "1800164")
                                    {

                                    }
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
                                    #region DataSet

                                    GetPFEligibleEmployee(sEmpSystemID, out dsPFEligibleEmp);
                                    dtPFEligibleEmp = dsPFEligibleEmp.Tables[0];
                                    dvPFEligibleEmp = new DataView();

                                    GetPFMonthlyEmpWiseCalculation(sEmpSystemID, out dsPFMntEmpWiseCal);
                                    dtPFMntEmpWiseCal = dsPFMntEmpWiseCal.Tables[0];
                                    dvPFMntEmpWiseCal = new DataView();

                                    GetPFMonthlyDistributionEmployee(sEmpSystemID, out dsPFMntDisEmp);
                                    dtPFMntDisEmp = dsPFMntDisEmp.Tables[0];
                                    dvPFMntDisEmp = new DataView();

                                    GetPFMonthlyDistributionEmployer(sEmpSystemID, out dsPFMntDisEmpr);
                                    dtPFMntDisEmpr = dsPFMntDisEmpr.Tables[0];
                                    dvPFMntDisEmpr = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfo> dicSalInfo = new List<dicSalInfo>();
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);//LoadEmpSlrDefForSlrProcessFORVPF
                                    //LoadEmpSlrDefForSlrProcessFORVPF(para, sEmpInfoSysIDColl, out dsSalInfo);//LoadEmpSlrDefForSlrProcessFORVPF
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfo>();

                                    #endregion DataSet

                                    sPFElgGentID = "";
                                    sPFDedGentID = "";
                                    bplib.clsGenID objGenID = new bplib.clsGenID();
                                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "PF_ELIGIBLE", out sPFElgGentID);
                                    //GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "PF_ELIGIBLE", dsUnTagEmp.Tables[0].Rows.Count, out sPFElgGentID);
                                    sPFElgGentID = "PE" + sPFElgGentID;

                                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "PF_CALCULATION", out sPFDedGentID);
                                    //GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "PF_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sPFDedGentID);
                                    sPFDedGentID = "PC" + sPFDedGentID;

                                    //for child  calc
                                    string pfCalChild = "";
                                    int pfCalChildCount = 0;
                                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "PF_CALCULATION_CHILD", out pfCalChild);

                                    for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                    {
                                        pfCalChildCount++;
                                        sPFEligibleEmpID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["PFEligibleEmpID"].ToString().Trim();
                                        //sPFMntEmpCalID = sPFDedGentID.ToString() + (iUnTgEmCnt + 1).ToString();
                                        sPFMntEmpCalID = "P" + DateTime.Now.ToString("yy") + pfCalChild + "-" + pfCalChildCount;
                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                        iAgeIntYears = Convert.ToInt32(bplib.clsWebLib.GetNumData(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["AgeIntYears"].ToString().Trim()));
                                        decPFVoluntaryPer = Convert.ToDecimal(bplib.clsWebLib.GetNumData(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["VoluntaryPFValue"].ToString()));
                                        bPFELIsActive = Convert.ToBoolean(bplib.clsWebLib.GetBoolData(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["IsActive"].ToString().Trim()));
                                        bEmpNotEntGetEmplrAlwn = false;
                                        bIsAgeLimitDistributionEmpr = true;
                                        if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["Eligibility"].ToString().ToUpper().Trim() == "DOJ")
                                        {
                                            dtEligibilityDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());
                                        }
                                        else if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["Eligibility"].ToString().ToUpper().Trim() == "DOC")
                                        {
                                            dtEligibilityDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOC"].ToString().Trim());
                                        }
                                        else
                                        {
                                            dtEligibilityDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());
                                        }

                                        if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EligibilityBaseOn"].ToString().ToUpper().Trim() == "DAY")
                                        {
                                            dtStartDate = dtEligibilityDate.AddDays(decEligibilityTimeLenght);
                                        }
                                        else if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EligibilityBaseOn"].ToString().ToUpper().Trim() == "MONTH")
                                        {
                                            dtStartDate = dtEligibilityDate.AddYears(decEligibilityTimeLenght);
                                        }
                                        else
                                        {
                                            dtStartDate = dtEligibilityDate;
                                        }

                                        if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["MaturityBaseOn"].ToString().ToUpper().Trim() == "MONTH")
                                        {
                                            dtMaturityDate = dtStartDate.AddMonths(decMaturityTimeLenght);
                                        }
                                        else if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["MaturityBaseOn"].ToString().ToUpper().Trim() == "YEAR")
                                        {
                                            dtMaturityDate = dtStartDate.AddYears(decMaturityTimeLenght);
                                        }
                                        else
                                        {
                                            dtMaturityDate = dtStartDate;
                                        }

                                        #region Salary Amount Insert Into Virtual Table

                                        DataTable dtValue = new DataTable();
                                        dtValue.TableName = "TempTable";
                                        dtValue.Columns.Add("EmpSystemID");
                                        dtValue.Columns.Add("SalaryHeadID");
                                        dtValue.Columns.Add("EntryCurrencyID");
                                        dtValue.Columns.Add("EntryAmount");
                                        dtValue.Columns.Add("EarningCurrencyID");
                                        dtValue.Columns.Add("EarningAmount");
                                        dtValue.Columns.Add("DecimalNo");
                                        dtValue.Columns.Add("IntegerInDisb");
                                        dtValue.Columns.Add("IsDecimalInDisb");
                                        dtValue.Columns.Add("RoundOption");

                                        var dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim());
                                        if (dicSalInfo_Sub.Count > 0)
                                        {
                                            sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                            if (para.dsSalInfo == null)
                                            {
                                                for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                {
                                                    sSlrHD = dicSalInfo_Sub[i].SalaryHeadID;
                                                    sEntCurID = dicSalInfo_Sub[i].EntryCurrencyID;
                                                    decEntCur = dicSalInfo_Sub[i].EntryAmount;
                                                    sEarnCurID = dicSalInfo_Sub[i].EarningCurrencyID;
                                                    decEarnCur = dicSalInfo_Sub[i].EarningAmount;

                                                    iDecimalNo = dicSalInfo_Sub[i].DecimalNo;
                                                    bIntegerInDisb = dicSalInfo_Sub[i].IntegerInDisb;
                                                    bIsDecimalInDisb = dicSalInfo_Sub[i].IsDecimalInDisb;
                                                    sRoundOption = dicSalInfo_Sub[i].RoundOption;
                                                    if (para.dsSalInfo == null)
                                                    {
                                                        #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                        DataRow dtValueRow = dtValue.NewRow();

                                                        dtValueRow["EmpSystemID"] = dicSalInfo_Sub[i].EmpInfoSystemID;
                                                        dtValueRow["SalaryHeadID"] = sSlrHD;
                                                        dtValueRow["EntryCurrencyID"] = sEntCurID;
                                                        dtValueRow["EntryAmount"] = decEntCur;
                                                        dtValueRow["EarningCurrencyID"] = sEarnCurID;
                                                        dtValueRow["EarningAmount"] = decEarnCur;
                                                        dtValueRow["DecimalNo"] = iDecimalNo;
                                                        dtValueRow["IntegerInDisb"] = bIntegerInDisb;
                                                        dtValueRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                                                        dtValueRow["RoundOption"] = sRoundOption;

                                                        dtValue.Rows.Add(dtValueRow);

                                                        #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                                                        if (dicSalInfo_Sub[i].HeadCategory == "PF Employee Contribution")
                                                        {
                                                            sPFContSalaryHeadIDEmp = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                        if (dicSalInfo_Sub[i].HeadCategory == "PF Employer Contribution")
                                                        {
                                                            sPFContSalaryHeadIDEmpr = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                        if (dicSalInfo_Sub[i].HeadCategory == "PF Voluntary")
                                                        {
                                                            sPFVoluntarySalaryHeadID = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (para.dsSalInfo != null)
                                        {
                                            dtValue = para.dsSalInfo.Tables[0];
                                            strTemp = "PF Employee Contribution";

                                            dvSlrHd = new DataView();
                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            { sPFContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                                            strTemp = "PF Employer Contribution";

                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            { sPFContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                                            sPFContSalaryHeadIDEmp = sVPFSLRHD;
                                        }

                                        #endregion Salary Amount Insert Into Virtual Table

                                        if (bIndividualAlwn == true)
                                        {
                                            dvPFEligibleEmp.Table = dtPFEligibleEmp;
                                            dvPFEligibleEmp.RowFilter = "ID = '" + sPFEligibleEmpID.Trim() + "'";
                                            if (dvPFEligibleEmp.Count > 0)
                                            {
                                                bEmpNotEntGetEmplrAlwn = Convert.ToBoolean(dvPFEligibleEmp[0].Row["IsNotEntGetEmplrAlwn"].ToString());
                                            }
                                        }
                                        else
                                        {
                                            bEmpNotEntGetEmplrAlwn = true;
                                        }

                                        for (int iPFDtl = 0; iPFDtl < dsPFPolicyDtl.Tables[0].Rows.Count; iPFDtl++)
                                        {
                                            #region Clear
                                            decimal _employeer_amount = 0;
                                            sFormulaDesIDEmpDis = "";
                                            sSalaryHeadIDEmp = "";
                                            sResidualValueSlrHdIDEmp = "";
                                            sFormulaDesIDEmprDis = "";
                                            sSalaryHeadIDEmpr = "";
                                            sResidualValueSlrHdIDEmpr = "";
                                            sAlwnSlrHd = "";
                                            decEmpCntValPer = 0;
                                            decEmployerCntValPer = 0;

                                            bIsFixedEmpr = false;
                                            bIsFormulaEmpr = false;
                                            bIsContributionSlrHDdependOnEarningEmpr = false;
                                            bIsDistributionEmpr = false;
                                            bIsFixedEmp = false;
                                            bIsFormulaEmp = false;
                                            bIsContributionSlrHDdependOnEarningEmp = false;
                                            bIsDistributionEmp = false;

                                            bEarning = false;
                                            bVoluntaryPF = false;
                                            bNotEntGetEmplrAlwn = false;
                                            bIndividualAlwn = false;

                                            decEmpCtbtnAmount = 0;
                                            decEmprCtbtnAmount = 0;
                                            decUpperLimitEmpr = 0;
                                            decValueTempEmpr = 0;
                                            decValueEmpr = 0;
                                            decFixedValueEmpr = 0;
                                            decValueEmp = 0;
                                            decUpperLimitEmp = 0;
                                            decFixedValueEmp = 0;
                                            decPFVoluntary = 0;

                                            #endregion Clear

                                            #region Select PFPolicyDetails ID if have multiple column

                                            sFormulaID = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FormulaDesIDEarning"].ToString().Trim();
                                            ReLoadFormulaWithValue(sEmpSysID, para, sFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                            sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                            decEarningValueRangeFrom = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EarningValueRangeFrom"].ToString().Trim());
                                            decEarningValueRangeTo = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EarningValueRangeTo"].ToString().Trim());

                                            if (Convert.ToDecimal(sFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sFormulaResult) < decEarningValueRangeTo)
                                            {
                                                bMaturity = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsMandatory"].ToString().Trim());
                                            }
                                            else
                                            {
                                                bMaturity = false;
                                            }
                                            sPFDtlID = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["ID"].ToString().Trim();

                                            sFormulaDesIDEmpDis = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FormulaDesIDEmpDis"].ToString().Trim();
                                            decFixedValueEmp = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FixedValueEmp"].ToString().Trim());
                                            bIsFixedEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFixedEmp"].ToString().Trim());
                                            bIsFormulaEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFormulaEmp"].ToString().Trim());
                                            bIsContributionSlrHDdependOnEarningEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsContributionSlrHDdependOnEarningEmp"].ToString().Trim());
                                            bIsDistributionEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsDistributionEmp"].ToString().Trim());

                                            decEmpCntValPer = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EmpCntValPer"].ToString().Trim());
                                            decEmployerCntValPer = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EmployerCntValPer"].ToString().Trim());
                                            sFormulaDesIDEmprDis = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FormulaDesIDEmployerDis"].ToString().Trim();
                                            decFixedValueEmpr = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FixedValueEmployer"].ToString().Trim());
                                            bIsFixedEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFixedEmployer"].ToString().Trim());
                                            bIsFormulaEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFormulaEmployer"].ToString().Trim());
                                            bIsContributionSlrHDdependOnEarningEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsContributionSlrHDdependOnEarningEmployer"].ToString().Trim());
                                            bIsDistributionEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsDistributionEmployer"].ToString().Trim());

                                            sAlwnSlrHd = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["AlwnSlrHd"].ToString().Trim();
                                            bVoluntaryPF = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsVoluntaryPF"].ToString().Trim());
                                            bNotEntGetEmplrAlwn = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsNotEntGetEmplrAlwn"].ToString().Trim());
                                            bIndividualAlwn = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsIndividualAlwn"].ToString().Trim());

                                            bIsAgeLimit = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsAgeLimit"].ToString().Trim());
                                            if (dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["AgeLimit"].ToString().Trim() != "")
                                            { iAgeLimit = Convert.ToInt32(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["AgeLimit"].ToString().Trim()); }
                                            else { iAgeLimit = 0; }

                                            if (bIsAgeLimit == true)
                                            {
                                                if (iAgeIntYears >= iAgeLimit)
                                                {
                                                    bIsAgeLimitDistributionEmpr = false;
                                                }
                                            }

                                            #endregion Select PFPolicyDetails ID if have multiple column
                                            if (bPFELIsActive == true)
                                            {
                                                #region Employee Contribution Amount

                                                if (bIsFixedEmp == true)
                                                {
                                                    decEmpCtbtnAmount = decFixedValueEmp;
                                                }
                                                else if (bIsFormulaEmp == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmp;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmpDis, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmpCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                #endregion Employee Contribution Amount

                                                #region Employer Contribution Amount

                                                if (bIsFixedEmpr == true)
                                                {
                                                    decEmprCtbtnAmount = decFixedValueEmpr;
                                                }
                                                else if (bIsFormulaEmpr == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmprDis, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                #endregion Employer Contribution Amount

                                                #region Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                dvPFMntEmpWiseCal.Table = dtPFMntEmpWiseCal;
                                                dvPFMntEmpWiseCal.RowFilter = "PFEligibleEmpID = '" + sPFEligibleEmpID + "' AND MonthNo = '" + Convert.ToDateTime(para.ToDate).Month + "' AND YearNo = '" + Convert.ToDateTime(para.ToDate).Year + "'";
                                                if (dvPFMntEmpWiseCal.Count == 0)
                                                {//Add new block
                                                    drPFMntEmpWiseCal = dtPFMntEmpWiseCal.NewRow();
                                                    UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("ADDNEW", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                    dtPFMntEmpWiseCal.Rows.Add(drPFMntEmpWiseCal);
                                                }
                                                else
                                                {//Edit block
                                                    sPFMntEmpCalID = dvPFMntEmpWiseCal[0].Row["ID"].ToString();
                                                    drPFMntEmpWiseCal = dvPFMntEmpWiseCal[0].Row;
                                                    drPFMntEmpWiseCal.BeginEdit();
                                                    UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("EDIT", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                    drPFMntEmpWiseCal.EndEdit();
                                                }
                                                #endregion Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                decEmpCtbtnAmountTemp = decEmpCtbtnAmount;
                                                decEmprCtbtnAmountTemp = decEmprCtbtnAmount;
                                                _employeer_amount = decEmprCtbtnAmount;

                                                decEmpCtbtnAmount = (decEmpCtbtnAmount * 100) / decEmpCntValPer;
                                                decEmprCtbtnAmount = (decEmprCtbtnAmount * 100) / decEmployerCntValPer;

                                                #region Select PFEmployeeDistribution ID if have multiple column

                                                dvPFMntDisEmp.Table = dtPFMntDisEmp;
                                                dvPFMntDisEmp.RowFilter = "PFMntEmpWiseCalID = '" + sPFMntEmpCalID.Trim() + "'";
                                                if (dvPFMntDisEmp.Count > 0)
                                                {
                                                    while (dvPFMntDisEmp.Count > 0)
                                                    {
                                                        drPFMntDisEmp = dvPFMntDisEmp[0].Row;
                                                        drPFMntDisEmp.Delete();
                                                    }
                                                }

                                                if (bIsDistributionEmp == true)
                                                {
                                                    var dicPFEmpDisb_Sub = dicPFEmpDisb.FindAll(x => x.PFPolicyDetailsID == dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["ID"].ToString().Trim());
                                                    if (dicPFEmpDisb_Sub.Count > 0)
                                                    {
                                                        for (int iEmpDis = 0; iEmpDis < dicPFEmpDisb_Sub.Count; iEmpDis++)
                                                        {
                                                            decValueEmp = dicPFEmpDisb_Sub[iEmpDis].Value;
                                                            sSalaryHeadIDEmp = dicPFEmpDisb_Sub[iEmpDis].SalaryHeadID;
                                                            decUpperLimitEmp = Convert.ToDecimal(GetNumData(dicPFEmpDisb_Sub[iEmpDis].UpperLimit.ToString()));
                                                            sResidualValueSlrHdIDEmp = dicPFEmpDisb_Sub[iEmpDis].ResidualValueSlrHdID;

                                                            decValueEmp = (decEmpCtbtnAmount * decValueEmp) / 100;

                                                            if (decValueEmp > decUpperLimitEmp)
                                                            {
                                                                decUpperLimitEmp = decValueEmp - decUpperLimitEmp;
                                                            }
                                                            else
                                                            {
                                                                decUpperLimitEmp = 0;
                                                            }

                                                            dvCurRl.Table = dtCurRl;
                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + sSalaryHeadIDEmp + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                            if (dvCurRl.Count > 0)
                                                            {
                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                            }
                                                            string sOutValue = "0";
                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmp.ToString(), out sOutValue);
                                                            decValueEmp = Convert.ToDecimal(sOutValue);

                                                            dvCurRl.Table = dtCurRl;
                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + sResidualValueSlrHdIDEmp + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                            if (dvCurRl.Count > 0)
                                                            {
                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                            }
                                                            string sOutValue1 = "0";
                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decUpperLimitEmp.ToString(), out sOutValue1);
                                                            decUpperLimitEmp = Convert.ToDecimal(sOutValue1);

                                                            drPFMntDisEmp = dtPFMntDisEmp.NewRow();
                                                            UpdateTheDataRowInTablePFMonthlyDistributionEmployeeAndEmployer(sPFMntEmpCalID, decValueEmp, sSalaryHeadIDEmp, decUpperLimitEmp, sResidualValueSlrHdIDEmp, para.sUser, ref drPFMntDisEmp);
                                                            dtPFMntDisEmp.Rows.Add(drPFMntDisEmp);
                                                        }
                                                    }
                                                }

                                                #endregion Select PFEmployeeDistribution ID if have multiple column

                                                #region Voluntary PF Data Save IN Table [PFMonthlyEmpWiseCalculation]
                                                if (bVoluntaryPF == true)
                                                {
                                                    if (decPFVoluntaryPer > 0)
                                                    {
                                                        //decEmpCtbtnAmount = (decEmpCtbtnAmount * 100) / decEmpCntValPer;
                                                        decPFVoluntary = (decEmpCtbtnAmount * decPFVoluntaryPer) / 100;
                                                        sPFVoluntarySalaryHeadID = sVPFSLRHD;

                                                        dvCurRl.Table = dtCurRl;
                                                        dvCurRl.RowFilter = "SalaryHeadID = '" + sPFVoluntarySalaryHeadID + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                        if (dvCurRl.Count > 0)
                                                        {
                                                            sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                            bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                            bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                            iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                        }
                                                        string sOutValue = "0";
                                                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decPFVoluntary.ToString(), out sOutValue);
                                                        decPFVoluntary = Convert.ToDecimal(sOutValue);

                                                        drPFMntDisEmp = dtPFMntDisEmp.NewRow();
                                                        UpdateTheDataRowInTablePFMonthlyDistributionEmployeeAndEmployer(sPFMntEmpCalID, decPFVoluntary, sPFVoluntarySalaryHeadID, 0, "", para.sUser, ref drPFMntDisEmp);
                                                        dtPFMntDisEmp.Rows.Add(drPFMntDisEmp);
                                                    }
                                                }
                                                #endregion Voluntary PF Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                #region Select PFEmployerDistribution ID if have multiple column

                                                dvPFMntDisEmpr.Table = dtPFMntDisEmpr;
                                                dvPFMntDisEmpr.RowFilter = "PFMntEmpWiseCalID = '" + sPFMntEmpCalID.Trim() + "'";
                                                if (dvPFMntDisEmpr.Count > 0)
                                                {
                                                    while (dvPFMntDisEmpr.Count > 0)
                                                    {
                                                        drPFMntDisEmpr = dvPFMntDisEmpr[0].Row;
                                                        drPFMntDisEmpr.Delete();
                                                    }
                                                }

                                                if (bIsDistributionEmpr == true)
                                                {
                                                    if (bIsAgeLimitDistributionEmpr == true)
                                                    {
                                                        if (dsPFEmprDisb.Tables[0].Rows.Count > 0)
                                                        {
                                                            var dicPFEmprDisb_Sub = dicPFEmprDisb.FindAll(x => x.PFPolicyDetailsID == dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["ID"].ToString().Trim());
                                                            if (dicPFEmprDisb_Sub.Count > 0)
                                                            {
                                                                decimal _cumulative_Total_of_all_head_but_last = 0;
                                                                for (int iEmpDis = 0; iEmpDis < dicPFEmprDisb_Sub.Count; iEmpDis++)
                                                                {
                                                                    bool IsLastHead = false;
                                                                    decValueEmpr = dicPFEmprDisb_Sub[iEmpDis].Value;
                                                                    sSalaryHeadIDEmpr = dicPFEmprDisb_Sub[iEmpDis].SalaryHeadID;
                                                                    decUpperLimitEmpr = Convert.ToDecimal(GetNumData(dicPFEmprDisb_Sub[iEmpDis].UpperLimit.ToString()));
                                                                    sResidualValueSlrHdIDEmpr = dicPFEmprDisb_Sub[iEmpDis].ResidualValueSlrHdID;

                                                                    decValueEmpr = (decEmprCtbtnAmount * decValueEmpr) / 100;

                                                                    if (decUpperLimitEmpr != 0)
                                                                    {
                                                                        if (decValueEmpr > decUpperLimitEmpr)
                                                                        {
                                                                            decValueTempEmpr = decUpperLimitEmpr;
                                                                            decUpperLimitEmpr = decValueEmpr - decUpperLimitEmpr;
                                                                            decValueEmpr = decValueTempEmpr;
                                                                        }
                                                                        else
                                                                        {
                                                                            decUpperLimitEmpr = 0;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        IsLastHead = true;
                                                                    }

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + sSalaryHeadIDEmpr + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmpr.ToString(), out sOutValue);
                                                                    decValueEmpr = Convert.ToDecimal(sOutValue);



                                                                    if (IsLastHead)
                                                                    {
                                                                        //lasthead=total ee - _cumulative_Total_of_all_head_but_last
                                                                        var TotalEmployercont =
                                                                        decValueEmpr = _employeer_amount - _cumulative_Total_of_all_head_but_last;
                                                                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmpr.ToString(), out sOutValue);
                                                                        decValueEmpr = Convert.ToDecimal(sOutValue);
                                                                    }
                                                                    _cumulative_Total_of_all_head_but_last += decValueEmpr;

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + sResidualValueSlrHdIDEmpr + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue1 = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decUpperLimitEmpr.ToString(), out sOutValue1);
                                                                    decUpperLimitEmpr = Convert.ToDecimal(sOutValue1);

                                                                    _cumulative_Total_of_all_head_but_last += decUpperLimitEmpr;

                                                                    drPFMntDisEmpr = dtPFMntDisEmpr.NewRow();
                                                                    UpdateTheDataRowInTablePFMonthlyDistributionEmployeeAndEmployer(sPFMntEmpCalID, decValueEmpr, sSalaryHeadIDEmpr, decUpperLimitEmpr, sResidualValueSlrHdIDEmpr, para.sUser, ref drPFMntDisEmpr);
                                                                    dtPFMntDisEmpr.Rows.Add(drPFMntDisEmpr);
                                                                }//for
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        #region Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                        bIsDistributionEmpr = false;

                                                        decEmpCtbtnAmount = decEmpCtbtnAmountTemp;
                                                        decEmprCtbtnAmount = decEmprCtbtnAmountTemp;

                                                        dvPFMntEmpWiseCal.Table = dtPFMntEmpWiseCal;
                                                        dvPFMntEmpWiseCal.RowFilter = "PFEligibleEmpID = '" + sPFEligibleEmpID + "' AND MonthNo = '" + Convert.ToDateTime(para.ToDate).Month + "' AND YearNo = '" + Convert.ToDateTime(para.ToDate).Year + "'";
                                                        if (dvPFMntEmpWiseCal.Count == 0)
                                                        {//Add new block
                                                            drPFMntEmpWiseCal = dtPFMntEmpWiseCal.NewRow();
                                                            UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("ADDNEW", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                            dtPFMntEmpWiseCal.Rows.Add(drPFMntEmpWiseCal);
                                                        }
                                                        else
                                                        {//Edit block
                                                            sPFMntEmpCalID = dvPFMntEmpWiseCal[0].Row["ID"].ToString();
                                                            drPFMntEmpWiseCal = dvPFMntEmpWiseCal[0].Row;
                                                            drPFMntEmpWiseCal.BeginEdit();
                                                            UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("EDIT", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                            drPFMntEmpWiseCal.EndEdit();
                                                        }
                                                        #endregion Data Save IN Table [PFMonthlyEmpWiseCalculation]
                                                    }
                                                }
                                                #endregion Select PFEmployerDistribution ID if have multiple column
                                            }
                                            else if (bPFELIsActive == false && bNotEntGetEmplrAlwn == true)
                                            {
                                                #region Employer Contribution Amount
                                                if (bEmpNotEntGetEmplrAlwn == true)
                                                {
                                                    if (bIsFixedEmpr == true)
                                                    {
                                                        decEmprCtbtnAmount = decFixedValueEmpr;
                                                    }
                                                    else if (bIsFormulaEmpr == true)
                                                    {
                                                        bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                                                        ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmprDis, bEarning, ref dtValue, ref dtSalHd);
                                                        sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                        decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                    }
                                                }

                                                #endregion Employer Contribution Amount

                                                //decEmpCtbtnAmount = decEmpCtbtnAmountTemp;
                                                //decEmprCtbtnAmount = decEmprCtbtnAmountTemp;

                                                #region Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                dvPFMntEmpWiseCal.Table = dtPFMntEmpWiseCal;
                                                dvPFMntEmpWiseCal.RowFilter = "PFEligibleEmpID = '" + sPFEligibleEmpID + "' AND MonthNo = '" + Convert.ToDateTime(para.ToDate).Month + "' AND YearNo = '" + Convert.ToDateTime(para.ToDate).Year + "'";
                                                if (dvPFMntEmpWiseCal.Count == 0)
                                                {//Add new block
                                                    drPFMntEmpWiseCal = dtPFMntEmpWiseCal.NewRow();
                                                    UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("ADDNEW", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                    dtPFMntEmpWiseCal.Rows.Add(drPFMntEmpWiseCal);
                                                }
                                                else
                                                {//Edit block
                                                    sPFMntEmpCalID = dvPFMntEmpWiseCal[0].Row["ID"].ToString();
                                                    drPFMntEmpWiseCal = dvPFMntEmpWiseCal[0].Row;
                                                    drPFMntEmpWiseCal.BeginEdit();
                                                    UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("EDIT", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                    drPFMntEmpWiseCal.EndEdit();
                                                }
                                                #endregion Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                #region If Not Entitle PF Get Allowance Save IN Table PFEmployerDistribution

                                                decValueEmpr = decEmprCtbtnAmount;
                                                sSalaryHeadIDEmpr = sAlwnSlrHd;
                                                decUpperLimitEmpr = 0;
                                                sResidualValueSlrHdIDEmpr = null;

                                                dvCurRl.Table = dtCurRl;
                                                dvCurRl.RowFilter = "SalaryHeadID = '" + sSalaryHeadIDEmpr + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                if (dvCurRl.Count > 0)
                                                {
                                                    sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                    bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                    bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                    iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                }
                                                string sOutValue = "0";
                                                obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmpr.ToString(), out sOutValue);
                                                decValueEmpr = Convert.ToDecimal(sOutValue);

                                                dvPFMntDisEmpr.Table = dtPFMntDisEmpr;
                                                dvPFMntDisEmpr.RowFilter = "PFMntEmpWiseCalID = '" + sPFMntEmpCalID.Trim() + "' and SalaryHeadID = '" + sAlwnSlrHd.Trim() + "'";
                                                if (dvPFMntDisEmpr.Count == 0)
                                                {
                                                    drPFMntDisEmpr = dtPFMntDisEmpr.NewRow();
                                                    UpdateTheDataRowInTablePFMonthlyDistributionEmployeeAndEmployer(sPFMntEmpCalID, decValueEmpr, sSalaryHeadIDEmpr, decUpperLimitEmpr, sResidualValueSlrHdIDEmpr, para.sUser, ref drPFMntDisEmpr);
                                                    dtPFMntDisEmpr.Rows.Add(drPFMntDisEmpr);
                                                }
                                                else
                                                {
                                                    while (dvPFMntDisEmpr.Count > 0)
                                                    {
                                                        drPFMntDisEmpr = dvPFMntDisEmpr[0].Row;
                                                        drPFMntDisEmpr.Delete();
                                                    }
                                                    if (bEmpNotEntGetEmplrAlwn == true)
                                                    {
                                                        drPFMntDisEmpr = dtPFMntDisEmpr.NewRow();
                                                        UpdateTheDataRowInTablePFMonthlyDistributionEmployeeAndEmployer(sPFMntEmpCalID, decValueEmpr, sSalaryHeadIDEmpr, decUpperLimitEmpr, sResidualValueSlrHdIDEmpr, para.sUser, ref drPFMntDisEmpr);
                                                        dtPFMntDisEmpr.Rows.Add(drPFMntDisEmpr);
                                                    }
                                                }

                                                #endregion If Not Entitle PF Get Allowance Save IN Table PFEmployerDistribution
                                            }
                                        }

                                        #region Data Save IN Table [PFEligibleEmployee]

                                        dvPFEligibleEmp.Table = dtPFEligibleEmp;
                                        dvPFEligibleEmp.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "'";
                                        if (dvPFEligibleEmp.Count == 1)
                                        {//Edit block
                                            drPFEligibleEmp = dvPFEligibleEmp[0].Row;
                                            drPFEligibleEmp.BeginEdit();
                                            UpdateTheDataRowInTablePFEligibleEmp("EDIT", sPFEligibleEmpID, sEmpSysID, sPFMstID, dtStartDate, dtMaturityDate, sPFDtlID, bMaturity, bIsAllEmpApplocable, para.sUser, ref drPFEligibleEmp);
                                            drPFEligibleEmp.EndEdit();
                                        }

                                        #endregion Data Save IN Table [PFEligibleEmployee]
                                    }
                                    //SaveDataSets(dsPFMntEmpWiseCal, dsPFMntDisEmp, dsPFMntDisEmpr);
                                }

                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                SaveDataSets(dsPFEligibleEmp, dsPFMntEmpWiseCal, dsPFMntDisEmp, dsPFMntDisEmpr);
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
                                dsPFMntEmpWiseCal = null;
                                dsPFMntDisEmp = null;
                                dsPFMntDisEmpr = null;
                            }
                        }

                        #endregion Tag Employee List

                        if (para.ShouldNotProcessUntaggedEmp == false)
                        {
                            #region Untag Employee List

                            GetUnTagEmployeeListWithPFPolicyMaster(para, sPFMstID.Trim(), sGroupID.Trim(), sPlantID.Trim(), out dsUnTagEmp);
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
                                        #region DataSet

                                        GetPFEligibleEmployee(sEmpSystemID, out dsPFEligibleEmp);
                                        dtPFEligibleEmp = dsPFEligibleEmp.Tables[0];
                                        dvPFEligibleEmp = new DataView();

                                        GetPFMonthlyEmpWiseCalculation(sEmpSystemID, out dsPFMntEmpWiseCal);
                                        dtPFMntEmpWiseCal = dsPFMntEmpWiseCal.Tables[0];
                                        dvPFMntEmpWiseCal = new DataView();

                                        GetPFMonthlyDistributionEmployee(sEmpSystemID, out dsPFMntDisEmp);
                                        dtPFMntDisEmp = dsPFMntDisEmp.Tables[0];
                                        dvPFMntDisEmp = new DataView();

                                        GetPFMonthlyDistributionEmployer(sEmpSystemID, out dsPFMntDisEmpr);
                                        dtPFMntDisEmpr = dsPFMntDisEmpr.Tables[0];
                                        dvPFMntDisEmpr = new DataView();

                                        //Get General Salary Amount Head Wise
                                        List<dicSalInfo> dicSalInfo = new List<dicSalInfo>();
                                        LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                        if (dsSalInfo.Tables[0].Rows.Count > 0)
                                            dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfo>();
                                        #endregion DataSet
                                        clsGenID objGenID = new clsGenID();
                                        sPFElgGentID = "";
                                        sPFDedGentID = "";
                                        // GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "PF_ELIGIBLE", dsUnTagEmp.Tables[0].Rows.Count, out sPFElgGentID);
                                        objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "PF_ELIGIBLE", out sPFElgGentID);
                                        sPFElgGentID = "PE" + sPFElgGentID;

                                        //GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "PF_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sPFDedGentID);
                                        //sPFDedGentID = "PFCA" + sPFDedGentID;
                                        objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "PF_CALCULATION", out sPFDedGentID);
                                        sPFDedGentID = "PC" + sPFDedGentID;

                                        //for child elig

                                        string pfEliChild = "";
                                        int pfEliChildCount = 0;
                                        objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "PF_ELIGIBLE_CHILD", out pfEliChild);

                                        //for child  calc
                                        string pfCalChild = "";
                                        int pfCalChildCount = 0;
                                        objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "PF_CALCULATION_CHILD", out pfCalChild);


                                        for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                        {
                                            #region Clear

                                            sFormulaDesIDEmpDis = "";
                                            sSalaryHeadIDEmp = "";
                                            sResidualValueSlrHdIDEmp = "";
                                            sFormulaDesIDEmprDis = "";
                                            sSalaryHeadIDEmpr = "";
                                            sResidualValueSlrHdIDEmpr = "";
                                            sAlwnSlrHd = "";

                                            decEmpCntValPer = 0;
                                            decEmployerCntValPer = 0;
                                            bIsFixedEmpr = false;
                                            bIsFormulaEmpr = false;
                                            bIsContributionSlrHDdependOnEarningEmpr = false;
                                            bIsDistributionEmpr = false;
                                            bIsFixedEmp = false;
                                            bIsFormulaEmp = false;
                                            bIsContributionSlrHDdependOnEarningEmp = false;
                                            bIsDistributionEmp = false;

                                            bEarning = false;
                                            bVoluntaryPF = false;
                                            bNotEntGetEmplrAlwn = false;
                                            bIndividualAlwn = false;

                                            decEmpCtbtnAmount = 0;
                                            decEmprCtbtnAmount = 0;
                                            decUpperLimitEmpr = 0;
                                            decValueTempEmpr = 0;
                                            decValueEmpr = 0;
                                            decFixedValueEmpr = 0;
                                            decValueEmp = 0;
                                            decUpperLimitEmp = 0;
                                            decFixedValueEmp = 0;
                                            decPFVoluntary = 0;

                                            #endregion Clear
                                            pfEliChildCount++;
                                            pfCalChildCount++;
                                            decPFVoluntaryPer = Convert.ToDecimal(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["VoluntaryPFValue"].ToString());
                                            //sPFEligibleEmpID = sPFElgGentID.ToString() + (iUnTgEmCnt + 1).ToString();
                                            sPFEligibleEmpID = pfEliChild + DateTime.Now.ToString("yy") + "-" + pfEliChildCount;

                                            //sPFMntEmpCalID = sPFDedGentID.ToString() + (iUnTgEmCnt + 1).ToString();
                                            sPFMntEmpCalID = "P" + DateTime.Now.ToString("yy") + pfCalChild + "-" + pfCalChildCount;

                                            sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();
                                            iAgeIntYears = Convert.ToInt32(bplib.clsWebLib.GetNumData(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["AgeIntYears"].ToString().Trim()));
                                            decEarningValueRangeFrom = 0;
                                            decEarningValueRangeTo = 0;
                                            bMaturity = false;
                                            bEarning = false;
                                            bIsAgeLimitDistributionEmpr = true;

                                            #region Master Table Data Capture [Eligibility, EligibilityBaseOn, MaturityBaseOn, Start Date & Maturity Date]
                                            if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["Eligibility"].ToString().ToUpper().Trim() == "DOJ")
                                            {
                                                dtEligibilityDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());
                                            }
                                            else if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["Eligibility"].ToString().ToUpper().Trim() == "DOC")
                                            {
                                                dtEligibilityDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOC"].ToString().Trim());
                                            }
                                            else
                                            {
                                                dtEligibilityDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());
                                            }

                                            decEligibilityTimeLenght = Convert.ToInt32(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EligibilityTimeLenght"].ToString().Trim());
                                            decMaturityTimeLenght = Convert.ToInt32(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["MaturityTimeLenght"].ToString().Trim());

                                            if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EligibilityBaseOn"].ToString().ToUpper().Trim() == "DAY")
                                            {
                                                dtStartDate = dtEligibilityDate.AddDays(decEligibilityTimeLenght);
                                            }
                                            else if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["EligibilityBaseOn"].ToString().ToUpper().Trim() == "MONTH")
                                            {
                                                dtStartDate = dtEligibilityDate.AddYears(decEligibilityTimeLenght);
                                            }
                                            else
                                            {
                                                dtStartDate = dtEligibilityDate;
                                            }

                                            if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["MaturityBaseOn"].ToString().ToUpper().Trim() == "MONTH")
                                            {
                                                dtMaturityDate = dtStartDate.AddMonths(decMaturityTimeLenght);
                                            }
                                            else if (dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["MaturityBaseOn"].ToString().ToUpper().Trim() == "YEAR")
                                            {
                                                dtMaturityDate = dtStartDate.AddYears(decMaturityTimeLenght);
                                            }
                                            else
                                            {
                                                dtMaturityDate = dtStartDate;
                                            }

                                            #endregion Master Table Data Capture 

                                            #region Salary Amount Insert Into Virtual Table

                                            DataTable dtValue = new DataTable();
                                            dtValue.TableName = "TempTable";
                                            dtValue.Columns.Add("EmpSystemID");
                                            dtValue.Columns.Add("SalaryHeadID");
                                            dtValue.Columns.Add("EntryCurrencyID");
                                            dtValue.Columns.Add("EntryAmount");
                                            dtValue.Columns.Add("EarningCurrencyID");
                                            dtValue.Columns.Add("EarningAmount");
                                            dtValue.Columns.Add("DecimalNo");
                                            dtValue.Columns.Add("IntegerInDisb");
                                            dtValue.Columns.Add("IsDecimalInDisb");
                                            dtValue.Columns.Add("RoundOption");

                                            var dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim());
                                            if (dicSalInfo_Sub.Count > 0)
                                            {
                                                sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                if (para.dsSalInfo == null)
                                                {
                                                    for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                    {
                                                        sSlrHD = dicSalInfo_Sub[i].SalaryHeadID;
                                                        sEntCurID = dicSalInfo_Sub[i].EntryCurrencyID;
                                                        decEntCur = dicSalInfo_Sub[i].EntryAmount;
                                                        sEarnCurID = dicSalInfo_Sub[i].EarningCurrencyID;
                                                        decEarnCur = dicSalInfo_Sub[i].EarningAmount;

                                                        iDecimalNo = dicSalInfo_Sub[i].DecimalNo;
                                                        bIntegerInDisb = dicSalInfo_Sub[i].IntegerInDisb;
                                                        bIsDecimalInDisb = dicSalInfo_Sub[i].IsDecimalInDisb;
                                                        sRoundOption = dicSalInfo_Sub[i].RoundOption;

                                                        #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                        DataRow dtValueRow = dtValue.NewRow();
                                                        dtValueRow["EmpSystemID"] = dicSalInfo_Sub[i].EmpInfoSystemID;
                                                        dtValueRow["SalaryHeadID"] = sSlrHD;
                                                        dtValueRow["EntryCurrencyID"] = sEntCurID;
                                                        dtValueRow["EntryAmount"] = decEntCur;
                                                        dtValueRow["EarningCurrencyID"] = sEarnCurID;
                                                        dtValueRow["EarningAmount"] = decEarnCur;
                                                        dtValueRow["DecimalNo"] = iDecimalNo;
                                                        dtValueRow["IntegerInDisb"] = bIntegerInDisb;
                                                        dtValueRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                                                        dtValueRow["RoundOption"] = sRoundOption;

                                                        dtValue.Rows.Add(dtValueRow);

                                                        #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                                                        if (dicSalInfo_Sub[i].HeadCategory == "PF Employee Contribution")
                                                        {
                                                            sPFContSalaryHeadIDEmp = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                        if (dicSalInfo_Sub[i].HeadCategory == "PF Employer Contribution")
                                                        {
                                                            sPFContSalaryHeadIDEmpr = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                        if (dicSalInfo_Sub[i].HeadCategory == "PF Voluntary")
                                                        {
                                                            sPFVoluntarySalaryHeadID = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                    }
                                                }
                                            }
                                            if (para.dsSalInfo != null)
                                            {
                                                dtValue = para.dsSalInfo.Tables[0];
                                                strTemp = "PF Employee Contribution";

                                                dvSlrHd = new DataView();
                                                dvSlrHd.Table = dtSalHd;
                                                dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                                if (dvSlrHd.Count > 0)
                                                { sPFContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                                                strTemp = "PF Employer Contribution";

                                                dvSlrHd.Table = dtSalHd;
                                                dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                                if (dvSlrHd.Count > 0)
                                                { sPFContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                                                sPFContSalaryHeadIDEmp = sVPFSLRHD;
                                            }

                                            #endregion Salary Amount Insert Into Virtual Table

                                            for (int iPFDtl = 0; iPFDtl < dsPFPolicyDtl.Tables[0].Rows.Count; iPFDtl++)
                                            {
                                                #region Select PFPolicyDetails ID if have multiple column

                                                sFormulaID = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                ReLoadFormulaWithValue(sEmpSysID, para, sFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                decEarningValueRangeFrom = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                decEarningValueRangeTo = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EarningValueRangeTo"].ToString().Trim());

                                                sAlwnSlrHd = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["AlwnSlrHd"].ToString().Trim();
                                                bVoluntaryPF = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsVoluntaryPF"].ToString().Trim());
                                                bNotEntGetEmplrAlwn = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsNotEntGetEmplrAlwn"].ToString().Trim());
                                                bIndividualAlwn = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsIndividualAlwn"].ToString().Trim());

                                                if (Convert.ToDecimal(sFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sFormulaResult) < decEarningValueRangeTo)
                                                {
                                                    bMaturity = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsMandatory"].ToString().Trim());
                                                }
                                                else
                                                {
                                                    bMaturity = false;
                                                }
                                                sPFDtlID = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["ID"].ToString().Trim();

                                                sFormulaDesIDEmpDis = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FormulaDesIDEmpDis"].ToString().Trim();
                                                decFixedValueEmp = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FixedValueEmp"].ToString().Trim());
                                                bIsFixedEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFixedEmp"].ToString().Trim());
                                                bIsFormulaEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFormulaEmp"].ToString().Trim());
                                                bIsContributionSlrHDdependOnEarningEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsContributionSlrHDdependOnEarningEmp"].ToString().Trim());
                                                bIsDistributionEmp = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsDistributionEmp"].ToString().Trim());

                                                decEmpCntValPer = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EmpCntValPer"].ToString().Trim());
                                                decEmployerCntValPer = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["EmployerCntValPer"].ToString().Trim());
                                                sFormulaDesIDEmprDis = dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FormulaDesIDEmployerDis"].ToString().Trim();
                                                decFixedValueEmpr = Convert.ToDecimal(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["FixedValueEmployer"].ToString().Trim());
                                                bIsFixedEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFixedEmployer"].ToString().Trim());
                                                bIsFormulaEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsFormulaEmployer"].ToString().Trim());
                                                bIsContributionSlrHDdependOnEarningEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsContributionSlrHDdependOnEarningEmployer"].ToString().Trim());
                                                bIsDistributionEmpr = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsDistributionEmployer"].ToString().Trim());

                                                bIsAgeLimit = Convert.ToBoolean(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["IsAgeLimit"].ToString().Trim());
                                                if (dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["AgeLimit"].ToString().Trim() != "")
                                                { iAgeLimit = Convert.ToInt32(dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["AgeLimit"].ToString().Trim()); }
                                                else { iAgeLimit = 0; }

                                                if (bIsAgeLimit == true)
                                                {
                                                    if (iAgeIntYears >= iAgeLimit)
                                                    {
                                                        bIsAgeLimitDistributionEmpr = false;
                                                    }
                                                }

                                                #endregion Select PFPolicyDetails ID if have multiple column

                                                #region Employee Contribution Amount

                                                if (bIsFixedEmp == true)
                                                {
                                                    decEmpCtbtnAmount = decFixedValueEmp;
                                                }
                                                else if (bIsFormulaEmp == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmp;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmpDis, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmpCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                #endregion Employee Contribution Amount

                                                #region Employer Contribution Amount

                                                if (bIsFixedEmpr == true)
                                                {
                                                    decEmprCtbtnAmount = decFixedValueEmpr;
                                                }
                                                else if (bIsFormulaEmpr == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmprDis, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                #endregion Employer Contribution Amount

                                                #region Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                dvPFMntEmpWiseCal.Table = dtPFMntEmpWiseCal;
                                                //dvPFMntEmpWiseCal.RowFilter = "ID = '" + sPFMntEmpCalID.Trim() + "'";
                                                dvPFMntEmpWiseCal.RowFilter = "PFEligibleEmpID = '" + sPFEligibleEmpID + "' AND MonthNo = '" + Convert.ToDateTime(para.ToDate).Month + "' AND YearNo = '" + Convert.ToDateTime(para.ToDate).Year + "'";
                                                if (dvPFMntEmpWiseCal.Count == 0)
                                                {//Add new block
                                                    drPFMntEmpWiseCal = dtPFMntEmpWiseCal.NewRow();
                                                    UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("ADDNEW", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                    dtPFMntEmpWiseCal.Rows.Add(drPFMntEmpWiseCal);
                                                }
                                                else
                                                {//edit block
                                                    sPFMntEmpCalID = dvPFMntEmpWiseCal[0].Row["ID"].ToString();
                                                    drPFMntEmpWiseCal = dvPFMntEmpWiseCal[0].Row;
                                                    drPFMntEmpWiseCal.BeginEdit();
                                                    UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("EDIT", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                    drPFMntEmpWiseCal.EndEdit();
                                                }
                                                #endregion Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                decEmpCtbtnAmountTemp = decEmpCtbtnAmount;
                                                decEmprCtbtnAmountTemp = decEmprCtbtnAmount;

                                                decEmpCtbtnAmount = (decEmpCtbtnAmount * 100) / decEmpCntValPer;
                                                decEmprCtbtnAmount = (decEmprCtbtnAmount * 100) / decEmployerCntValPer;

                                                #region Select PFEmployeeDistribution ID if have multiple column

                                                dvPFMntDisEmp.Table = dtPFMntDisEmp;
                                                dvPFMntDisEmp.RowFilter = "PFMntEmpWiseCalID = '" + sPFMntEmpCalID.Trim() + "'";
                                                if (dvPFMntDisEmp.Count > 0)
                                                {
                                                    while (dvPFMntDisEmp.Count > 0)
                                                    {
                                                        drPFMntDisEmp = dvPFMntDisEmp[0].Row;
                                                        drPFMntDisEmp.Delete();
                                                    }
                                                }

                                                if (bIsDistributionEmp == true)
                                                {
                                                    var dicPFEmpDisb_Sub = dicPFEmpDisb.FindAll(x => x.PFPolicyDetailsID == dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["ID"].ToString().Trim());
                                                    if (dicPFEmpDisb_Sub.Count > 0)
                                                    {
                                                        for (int iEmpDis = 0; iEmpDis < dicPFEmpDisb_Sub.Count; iEmpDis++)
                                                        {
                                                            decValueEmp = dicPFEmpDisb_Sub[iEmpDis].Value;
                                                            sSalaryHeadIDEmp = dicPFEmpDisb_Sub[iEmpDis].SalaryHeadID;
                                                            decUpperLimitEmp = Convert.ToDecimal(GetNumData(dicPFEmpDisb_Sub[iEmpDis].UpperLimit.ToString()));

                                                            sResidualValueSlrHdIDEmp = dicPFEmpDisb_Sub[iEmpDis].ResidualValueSlrHdID;

                                                            decValueEmp = (decEmpCtbtnAmount * decValueEmp) / 100;

                                                            if (decValueEmp > decUpperLimitEmp)
                                                            {
                                                                decUpperLimitEmp = decValueEmp - decUpperLimitEmp;
                                                            }
                                                            else
                                                            {
                                                                decUpperLimitEmp = 0;
                                                            }

                                                            dvCurRl.Table = dtCurRl;
                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + sSalaryHeadIDEmp + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                            if (dvCurRl.Count > 0)
                                                            {
                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                            }
                                                            string sOutValue = "0";
                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmp.ToString(), out sOutValue);
                                                            decValueEmp = Convert.ToDecimal(sOutValue);

                                                            dvCurRl.Table = dtCurRl;
                                                            dvCurRl.RowFilter = "SalaryHeadID = '" + sResidualValueSlrHdIDEmp + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                            if (dvCurRl.Count > 0)
                                                            {
                                                                sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                            }
                                                            string sOutValue1 = "0";
                                                            obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decUpperLimitEmp.ToString(), out sOutValue1);
                                                            decUpperLimitEmp = Convert.ToDecimal(sOutValue1);

                                                            drPFMntDisEmp = dtPFMntDisEmp.NewRow();
                                                            UpdateTheDataRowInTablePFMonthlyDistributionEmployeeAndEmployer(sPFMntEmpCalID, decValueEmp, sSalaryHeadIDEmp, decUpperLimitEmp, sResidualValueSlrHdIDEmp, para.sUser, ref drPFMntDisEmp);
                                                            dtPFMntDisEmp.Rows.Add(drPFMntDisEmp);
                                                        }
                                                    }
                                                }

                                                #endregion Select PFEmployeeDistribution ID if have multiple column

                                                #region Voluntary PF Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                if (bVoluntaryPF == true)
                                                {
                                                    if (decPFVoluntaryPer > 0)
                                                    {
                                                        decPFVoluntary = (decEmpCtbtnAmount * decPFVoluntaryPer) / 100;
                                                        sPFVoluntarySalaryHeadID = sVPFSLRHD;

                                                        dvCurRl.Table = dtCurRl;
                                                        dvCurRl.RowFilter = "SalaryHeadID = '" + sPFVoluntarySalaryHeadID + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                        if (dvCurRl.Count > 0)
                                                        {
                                                            sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                            bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                            bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                            iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                        }
                                                        string sOutValue = "0";
                                                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decPFVoluntary.ToString(), out sOutValue);
                                                        decPFVoluntary = Convert.ToDecimal(sOutValue);

                                                        drPFMntDisEmp = dtPFMntDisEmp.NewRow();
                                                        UpdateTheDataRowInTablePFMonthlyDistributionEmployeeAndEmployer(sPFMntEmpCalID, decPFVoluntary, sPFVoluntarySalaryHeadID, 0, "", para.sUser, ref drPFMntDisEmp);
                                                        dtPFMntDisEmp.Rows.Add(drPFMntDisEmp);
                                                    }
                                                }

                                                #endregion Voluntary PF Data Save IN Table [PFMonthlyEmpWiseCalculation]

                                                #region Select PFEmployerDistribution ID if have multiple column

                                                dvPFMntDisEmpr.Table = dtPFMntDisEmpr;
                                                dvPFMntDisEmpr.RowFilter = "PFMntEmpWiseCalID = '" + sPFMntEmpCalID.Trim() + "'";
                                                if (dvPFMntDisEmpr.Count > 0)
                                                {
                                                    while (dvPFMntDisEmpr.Count > 0)
                                                    {
                                                        drPFMntDisEmpr = dvPFMntDisEmpr[0].Row;
                                                        drPFMntDisEmpr.Delete();
                                                    }
                                                }
                                                if (bIsDistributionEmpr == true)
                                                {
                                                    if (bIsAgeLimitDistributionEmpr == true)
                                                    {
                                                        if (dsPFEmprDisb.Tables[0].Rows.Count > 0)
                                                        {
                                                            var dicPFEmprDisb_Sub = dicPFEmprDisb.FindAll(x => x.PFPolicyDetailsID == dsPFPolicyDtl.Tables[0].Rows[iPFDtl]["ID"].ToString().Trim());
                                                            if (dicPFEmprDisb_Sub.Count > 0)
                                                            {
                                                                for (int iEmpDis = 0; iEmpDis < dicPFEmprDisb_Sub.Count; iEmpDis++)
                                                                {
                                                                    decValueEmpr = dicPFEmprDisb_Sub[iEmpDis].Value;
                                                                    sSalaryHeadIDEmpr = dicPFEmprDisb_Sub[iEmpDis].SalaryHeadID;
                                                                    decUpperLimitEmpr = Convert.ToDecimal(GetNumData(dicPFEmprDisb_Sub[iEmpDis].UpperLimit.ToString()));
                                                                    sResidualValueSlrHdIDEmpr = dicPFEmprDisb_Sub[iEmpDis].ResidualValueSlrHdID;

                                                                    decValueEmpr = (decEmprCtbtnAmount * decValueEmpr) / 100;

                                                                    if (decUpperLimitEmpr != 0)
                                                                    {
                                                                        if (decValueEmpr > decUpperLimitEmpr)
                                                                        {
                                                                            decValueTempEmpr = decUpperLimitEmpr;
                                                                            decUpperLimitEmpr = decValueEmpr - decUpperLimitEmpr;
                                                                            decValueEmpr = decValueTempEmpr;
                                                                        }
                                                                        else
                                                                        {
                                                                            decUpperLimitEmpr = 0;
                                                                        }
                                                                    }

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + sSalaryHeadIDEmpr + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decValueEmpr.ToString(), out sOutValue);
                                                                    decValueEmpr = Convert.ToDecimal(sOutValue);

                                                                    dvCurRl.Table = dtCurRl;
                                                                    dvCurRl.RowFilter = "SalaryHeadID = '" + sResidualValueSlrHdIDEmpr + "' AND CurrencyRuleSystemID = '" + sCurrencyRuleSystemID + "'";
                                                                    if (dvCurRl.Count > 0)
                                                                    {
                                                                        sRoundOption = dvCurRl[0].Row["RoundOption"].ToString().Trim();
                                                                        bIntegerInDisb = Convert.ToBoolean(dvCurRl[0].Row["IntegerInDisb"].ToString().Trim());
                                                                        bIsDecimalInDisb = Convert.ToBoolean(dvCurRl[0].Row["IsDecimalInDisb"].ToString().Trim());
                                                                        iDecimalNo = Convert.ToInt32(dvCurRl[0].Row["DecimalNo"].ToString().Trim());
                                                                    }
                                                                    string sOutValue1 = "0";
                                                                    obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, decUpperLimitEmpr.ToString(), out sOutValue1);
                                                                    decUpperLimitEmpr = Convert.ToDecimal(sOutValue1);

                                                                    drPFMntDisEmpr = dtPFMntDisEmpr.NewRow();
                                                                    UpdateTheDataRowInTablePFMonthlyDistributionEmployeeAndEmployer(sPFMntEmpCalID, decValueEmpr, sSalaryHeadIDEmpr, decUpperLimitEmpr, sResidualValueSlrHdIDEmpr, para.sUser, ref drPFMntDisEmpr);
                                                                    dtPFMntDisEmpr.Rows.Add(drPFMntDisEmpr);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        #region Data Save IN Table [PFMonthlyEmpWiseCalculation]
                                                        bIsDistributionEmpr = false;

                                                        decEmpCtbtnAmount = decEmpCtbtnAmountTemp;
                                                        decEmprCtbtnAmount = decEmprCtbtnAmountTemp;

                                                        dvPFMntEmpWiseCal.Table = dtPFMntEmpWiseCal;
                                                        dvPFMntEmpWiseCal.RowFilter = "PFEligibleEmpID = '" + sPFEligibleEmpID + "' AND MonthNo = '" + Convert.ToDateTime(para.ToDate).Month + "' AND YearNo = '" + Convert.ToDateTime(para.ToDate).Year + "'";
                                                        if (dvPFMntEmpWiseCal.Count == 0)
                                                        {//Add new block
                                                            drPFMntEmpWiseCal = dtPFMntEmpWiseCal.NewRow();
                                                            UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("ADDNEW", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                            dtPFMntEmpWiseCal.Rows.Add(drPFMntEmpWiseCal);
                                                        }
                                                        else
                                                        {//Edit block
                                                            sPFMntEmpCalID = dvPFMntEmpWiseCal[0].Row["ID"].ToString();
                                                            drPFMntEmpWiseCal = dvPFMntEmpWiseCal[0].Row;
                                                            drPFMntEmpWiseCal.BeginEdit();
                                                            UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation("EDIT", sPFMntEmpCalID, sPFEligibleEmpID, para.ToDate, decEmpCtbtnAmount, bIsDistributionEmp, decEmprCtbtnAmount, bIsDistributionEmpr, para.sUser, ref drPFMntEmpWiseCal);
                                                            drPFMntEmpWiseCal.EndEdit();
                                                        }
                                                        #endregion Data Save IN Table [PFMonthlyEmpWiseCalculation]
                                                    }
                                                }
                                                #endregion Select PFEmployerDistribution ID if have multiple column
                                            }

                                            #region Data Save IN Table [PFEligibleEmployee]

                                            dvPFEligibleEmp.Table = dtPFEligibleEmp;
                                            dvPFEligibleEmp.RowFilter = "ID = '" + sPFMntEmpCalID.Trim() + "'";
                                            if (dvPFEligibleEmp.Count == 0)
                                            {//Add new block
                                                drPFEligibleEmp = dtPFEligibleEmp.NewRow();
                                                UpdateTheDataRowInTablePFEligibleEmp("ADDNEW", sPFEligibleEmpID, sEmpSysID, sPFMstID, dtStartDate, dtMaturityDate, sPFDtlID, bMaturity, bIsAllEmpApplocable, para.sUser, ref drPFEligibleEmp);
                                                dtPFEligibleEmp.Rows.Add(drPFEligibleEmp);
                                            }
                                            else
                                            {//Edit block
                                                drPFEligibleEmp = dvPFEligibleEmp[0].Row;
                                                drPFEligibleEmp.BeginEdit();
                                                UpdateTheDataRowInTablePFEligibleEmp("EDIT", sPFEligibleEmpID, sEmpSysID, sPFMstID, dtStartDate, dtMaturityDate, sPFDtlID, bMaturity, bIsAllEmpApplocable, para.sUser, ref drPFEligibleEmp);
                                                drPFEligibleEmp.EndEdit();
                                            }

                                            #endregion Data Save IN Table [PFEligibleEmployee]
                                        }

                                        //SaveDataSets(dsPFEligibleEmp, dsPFMntEmpWiseCal, dsPFMntDisEmp, dsPFMntDisEmpr);
                                    }
                                    //if (SelectedEmpCnt == grdRowMaxCnt)
                                    //{
                                    TotProcComp += grdRowMaxCnt;
                                    TotSelectEmpForProc -= grdRowMaxCnt;
                                    SaveDataSets(dsPFEligibleEmp, dsPFMntEmpWiseCal, dsPFMntDisEmp, dsPFMntDisEmpr);
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
                                    dsPFEligibleEmp = null;
                                    dsPFMntEmpWiseCal = null;
                                    dsPFMntDisEmp = null;
                                    dsPFMntDisEmpr = null;
                                }
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

                dsPFEligibleEmp = null;
                dtPFEligibleEmp = null;
                drPFEligibleEmp = null;
                dvPFEligibleEmp = null;

                dsPFMntEmpWiseCal = null;
                dtPFMntEmpWiseCal = null;
                drPFMntEmpWiseCal = null;
                dvPFMntEmpWiseCal = null;

                dsPFMntDisEmp = null;
                dtPFMntDisEmp = null;
                drPFMntDisEmp = null;
                dvPFMntDisEmp = null;

                dsPFMntDisEmpr = null;
                dtPFMntDisEmpr = null;
                drPFMntDisEmpr = null;
                dvPFMntDisEmpr = null;

                dsSalInfo = null;
                dsPFPolicyMst = null;
                dsPFPolicyDtl = null;
                dsPFEmpDisb = null;
                dsPFEmprDisb = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function

        private void UpdateTheDataRowInTablePFEligibleEmp(string OPN_FLAG, string sPFEligibleEmpID, string sEmpSysID, string sPFMstID, DateTime dtStartDate, DateTime dtMaturityDate, string sPFDtlID, bool bMaturity, bool bIsAllEmpApplocable, string sUser, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["ID"] = RetValidLen(sPFEligibleEmpID.Trim());

                    drLocal["AddedBy"] = RetValidLen(sUser);
                    drLocal["AddedDate"] = DateTime.Now.ToString();
                    drLocal["AddedFromIP"] = "";
                }

                drLocal["EmpSystemID"] = RetValidLen(sEmpSysID.Trim());
                drLocal["PFMstID"] = RetValidLen(sPFMstID.Trim());
                drLocal["StartDate"] = dtStartDate;
                drLocal["MaturityDate"] = dtMaturityDate;
                drLocal["PFDtlID"] = RetValidLen(sPFDtlID.Trim());

                drLocal["IsMandatory"] = bMaturity;
                if (bMaturity == true)
                {
                    drLocal["IsActive"] = bMaturity;
                    drLocal["IsApproved"] = bMaturity;
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

                drLocal["AlwnSlrHd"] = DBNull.Value;
                drLocal["IsVoluntaryPF"] = false;
                drLocal["IsNotEntGetEmplrAlwn"] = false;
                //drLocal["IsIndividualAlwn"] = false;

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
        }//End Functionon

        private void UpdateTheDataRowInTablePFMonthlyEmpWiseCalculation(string OPN_FLAG, string sPFMntDedID, string sPFEligibleEmpID, string sToDate, decimal decEmpCtbtnAmount, bool bIsDistributionEmp, decimal decEmprCtbtnAmount, bool bIsDistributionEmpr, string sUser, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["ID"] = RetValidLen(sPFMntDedID.Trim());

                    drLocal["AddedBy"] = RetValidLen(sUser);
                    drLocal["AddedDate"] = DateTime.Now.ToString();
                    drLocal["AddedFromIP"] = "";
                }

                drLocal["PFEligibleEmpID"] = RetValidLen(sPFEligibleEmpID.Trim());
                drLocal["MonthNo"] = Convert.ToDateTime(sToDate).Month;
                drLocal["YearNo"] = Convert.ToDateTime(sToDate).Year;
                drLocal["EmployeeContributionAmount"] = decEmpCtbtnAmount;
                drLocal["IsDistributionEmp"] = bIsDistributionEmp;

                drLocal["EmployerContributionAmount"] = decEmprCtbtnAmount;
                drLocal["IsDistributionEmpr"] = bIsDistributionEmpr;

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
        private void UpdateTheDataRowInTablePFMonthlyDistributionEmployeeAndEmployer(string sPFMntEmpCalID, decimal sValue, string sSalaryHeadID, decimal sUpperLimit, string sResidualValueSlrHdID, string sUser, ref DataRow drLocal)
        {
            try
            {

                drLocal["AddedBy"] = RetValidLen(sUser);
                drLocal["AddedDate"] = DateTime.Now.ToString();
                drLocal["AddedFromIP"] = "";

                drLocal["PFMntEmpWiseCalID"] = RetValidLen(sPFMntEmpCalID);

                drLocal["Value"] = sValue;
                if (sSalaryHeadID != "")
                {
                    drLocal["SalaryHeadID"] = sSalaryHeadID;
                }
                else
                {
                    drLocal["SalaryHeadID"] = DBNull.Value;
                }
                drLocal["UpperLimit"] = sUpperLimit;
                if (sResidualValueSlrHdID != "")
                {
                    drLocal["ResidualValueSlrHdID"] = sResidualValueSlrHdID;
                }
                else
                {
                    drLocal["ResidualValueSlrHdID"] = DBNull.Value;
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
    }
    public class ParaList
    {
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public string sEmpSystemID { get; set; }
        public string LocalCurrencyID { get; set; }
        public string ForeignCurRate { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string sUser { get; set; }
        public DataSet dsSalInfo { get; set; }
        public bool ShouldNotProcessUntaggedEmp { get; set; }
        //List<ProcChild> dicProcChild
        public List<ProcChild> dicProcChild { get; set; }
        public List<SPvalueHeadWise> dtValue { get; set; }
    }
    public class dicPFEmpDisb
    {
        public string ID { get; set; } = "";
        public string PFPolicyDetailsID { get; set; } = "";
        public decimal Value { get; set; } = 0;
        public string SalaryHeadID { get; set; } = "";
        public decimal UpperLimit { get; set; } = 0;
        public string ResidualValueSlrHdID { get; set; } = "";
    }
    public class dicPFEmprDisb
    {
        public string ID { get; set; } = "";
        public string PFPolicyDetailsID { get; set; } = "";
        public decimal Value { get; set; } = 0;
        public string SalaryHeadID { get; set; } = "";
        public decimal UpperLimit { get; set; } = 0;
        public string ResidualValueSlrHdID { get; set; } = "";
    }
}