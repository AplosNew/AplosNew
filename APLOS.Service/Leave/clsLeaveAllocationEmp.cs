using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace OTSBD
{
    public class clsLeaveAllocationEmp
    {
        public clsLeaveAllocationEmp()
        {
            // TODO: Add constructor logic here
        }

        public void xGetEmployeeIDName(string sGroupID, string sPlantID, string strYrCal, string strLPMSysterID, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM 
		                            (
		                             SELECT [CheckBoxSelect] = Case WHEN LA.EmpSystemID IS NULL THEN Convert(bit, 'False')
                                            ELSE Convert(bit, 'True') END, E.SystemID, E.EmployeeCode, E.EmployeeName, 
                                            REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], 
                                            Dsg.UserName AS Designation
		                              FROM EmployeeInformation AS E 
                                            LEFT OUTER JOIN 
				                             HKP.EmployeeType AS EC ON E.EmployeeCategorySystemID = EC.Id
				                            LEFT OUTER JOIN 
							                            ORG.Unit AS U ON U.Id = E.UnitID 
				                            LEFT OUTER JOIN 
							                            ORG.Division AS Dv ON Dv.Id = E.DivisionID 
				                            LEFT OUTER JOIN 
							                            ORG.Department AS De ON De.Id = E.DepartmentID 
							                LEFT OUTER JOIN 
							                            HKP.DesignationGroup AS DsgGr ON DsgGr.Id = E.DesignationGroupID 
				                            LEFT OUTER JOIN 
							                            HKP.Designation AS Dsg ON Dsg.Id = E.DesignationSystemID 
				                            LEFT OUTER JOIN 
							                            ORG.Section AS Se ON Se.Id = E.SectionID 
				                            LEFT OUTER JOIN 
							                            ORG.SubSection AS SuS ON SuS.Id = E.SubSectionID
							                LEFT OUTER JOIN 
														(
														 SELECT DISTINCT EmpSystemID FROM dbo.LeaveAllocation WHERE YrCalSystemID = '" + strYrCal + @"' 
                                                            AND LvPolDetailsSystemID IN (
																						  SELECT SystemID FROM LeavePolicyDetail 
																						   WHERE LPMSystemID = '" + strLPMSysterID + @"' AND IsActive = 1 
																						  GROUP BY SystemID
																						 ) 
														 GROUP BY EmpSystemID
														) LA ON E.SystemID = LA.EmpSystemID  
									  WHERE E.EmployeeStatus = 'Active' AND E.SystemID NOT IN (
																							   SELECT DISTINCT EmpSystemID FROM dbo.LeaveAllocation 
																								WHERE YrCalSystemID = '" + strYrCal + @"' 
																									  AND LvPolDetailsSystemID IN (
																																	SELECT SystemID FROM LeavePolicyDetail 
																																	  WHERE LPMSystemID <> '" + strLPMSysterID + @"' AND IsActive = 1 
																																	 GROUP BY SystemID
																																	)
																							   GROUP BY EmpSystemID
																							  )
                                            AND E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + "'";
                if (sUnit.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.UnitID = '" + sUnit + "'";
                }
                if (sDevi.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.DivisionID = '" + sDevi + "'";
                }
                if (sDept.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.DepartmentID = '" + sDept + "'";
                }
                if (sSect.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.SectionID = '" + sSect + "'";
                }
                if (sSbSe.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.SubSectionID = '" + sSbSe + "'";
                }
                if (sEmpC.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.EmployeeCategorySystemID = '" + sEmpC + "'";
                }
                if (sDeGr.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.DesignationGroupID = '" + sDeGr + "'";
                }
                if (sDesi.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.DesignationSystemID = '" + sDesi + "'";
                }
                if (sEmpSysID.Trim() != "")
                {
                    strSql = strSql + " AND E.SystemID IN (" + sEmpSysID + ")";
                }

                strSql = strSql + ") A  Order By EmployeeCode";

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

        public void GetEmployeeIDName(string sGroupID, string sPlantID, string strYrCal, string strLPMSysterID, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, out DataSet dsRef)
        {
            clsStaticInfo obs = null;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                obs = new clsStaticInfo();
                strSql = @"SELECT * FROM 
		                            (
		                             SELECT [CheckBoxSelect] = Case WHEN LA.EmpSystemID IS NULL THEN Convert(bit, 'False')
                                            ELSE Convert(bit, 'True') END, E.SystemID, E.EmployeeCode, E.EmployeeName, 
                                            REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], 
                                            D.UserName AS Designation
		                              FROM EmployeeInformation AS E 
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id 
                                            " + obs.EntityTables()+@"
							                LEFT OUTER JOIN 
														(
														 SELECT DISTINCT EmpSystemID FROM dbo.LeaveAllocation WHERE YrCalSystemID = '" + strYrCal + @"' 
                                                            AND LvPolDetailsSystemID IN (
																						  SELECT SystemID FROM LeavePolicyDetail 
																						   WHERE LPMSystemID = '" + strLPMSysterID + @"' AND IsActive = 1 
																						  GROUP BY SystemID
																						 ) 
														 GROUP BY EmpSystemID
														) LA ON E.SystemID = LA.EmpSystemID  
									  WHERE E.EmployeeStatus = 'Active' AND E.SystemID NOT IN (
																							   SELECT DISTINCT EmpSystemID FROM dbo.LeaveAllocation 
																								WHERE YrCalSystemID = '" + strYrCal + @"' 
																									  AND LvPolDetailsSystemID IN (
																																	SELECT SystemID FROM LeavePolicyDetail 
																																	  WHERE LPMSystemID <> '" + strLPMSysterID + @"' AND IsActive = 1 
																																	 GROUP BY SystemID
																																	)
																							   GROUP BY EmpSystemID
																							  )
                                            AND E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + "'";
                if (sUnit.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.UnitID = '" + sUnit + "'";
                }
                if (sDevi.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.DivisionID = '" + sDevi + "'";
                }
                if (sDept.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.DepartmentID = '" + sDept + "'";
                }
                if (sSect.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.SectionID = '" + sSect + "'";
                }
                if (sSbSe.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.SubSectionID = '" + sSbSe + "'";
                }
                if (sEmpC.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.EmployeeCategorySystemID = '" + sEmpC + "'";
                }
                if (sDeGr.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.DesignationGroupID = '" + sDeGr + "'";
                }
                if (sDesi.Trim() != "ALL")
                {
                    strSql = strSql + " AND E.DesignationSystemID = '" + sDesi + "'";
                }
                if (sEmpSysID.Trim() != "")
                {
                    strSql = strSql + " AND E.SystemID IN (" + sEmpSysID + ")";
                }

                strSql = strSql + ") A  Order By EmployeeCode";

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

        public void GetLeaveAllocatLvYerWise(string sGroupID, string sPlantID, string sLvYeaID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.LeaveAllocation 
                            WHERE YrCalSystemID = '" + sLvYeaID + @"' AND GroupID = '" + sGroupID + @"'
									 AND PlantID = '" + sPlantID + @"'";

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

        public void XGetLeaveType(string sGroupID, string sPlantID, string strSystemID, string strYrCal, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LPD.SystemID LvPolDetailsSystemID, LPD.LTSystemID, LeaveType, LT.LeaveName,  
                                  LeaveDescription, LPD.LeaveDays 
							FROM dbo.LeavePolicyMaster LPM
                                LEFT JOIN dbo.LeavePolicyDetail LPD ON LPM.SystemID = LPD.LPMSystemID AND LPD.IsActive = 1
                                LEFT JOIN dbo.LeaveType LT ON LPD.LTSystemID = LT.SystemID
                            WHERE LPM.SystemID = '" + strSystemID + @"'	
                                    AND LPM.SystemID IN (SELECT LvPolMstSystemID FROM dbo.LvPolMstYearCalendar 
				                                        WHERE YrCalSystemID = '" + strYrCal + @"' 
                                                                        GROUP BY LvPolMstSystemID)
                                    AND LPM.GroupID = '" + sGroupID + @"' AND LPM.PlantID = '" + sPlantID + @"'
                            ORDER BY LT.LeaveName";

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

        public void GetLeaveType(string sGroupID, string sPlantID, string strSystemID, string strYrCal, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LPD.SystemID LvPolDetailsSystemID, LPD.LTSystemID, LeaveType, LT.UserName LeaveName,  
                                 Description LeaveDescription, LPD.LeaveDays 
							FROM dbo.LeavePolicyMaster LPM
                                LEFT JOIN dbo.LeavePolicyDetail LPD ON LPM.SystemID = LPD.LPMSystemID AND LPD.IsActive = 1
                                LEFT JOIN dbo.LeaveType LT ON LPD.LTSystemID = LT.Id
                            WHERE LPM.SystemID = '" + strSystemID + @"'	
                                    AND LPM.SystemID IN (SELECT LvPolMstSystemID FROM dbo.LvPolMstYearCalendar 
				                                        WHERE YrCalSystemID = '" + strYrCal + @"' 
                                                                        GROUP BY LvPolMstSystemID)
                                    AND LPM.GroupID = '" + sGroupID + @"' AND LPM.PlantID = '" + sPlantID + @"'
                            ORDER BY LT.UserName";

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
        public void GetLeaveAllocat(string sGroupID, string sPlantID, string strSystemID, string strLPMSysterID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.LeaveAllocation 
                            WHERE YrCalSystemID = '" + strSystemID + @"' AND 
                                    LvPolDetailsSystemID IN (SELECT SystemID FROM LeavePolicyDetail 
						                                    WHERE LPMSystemID = '" + strLPMSysterID + @"' AND IsActive = 1
                                                            GROUP BY SystemID)
                                    AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + "'";

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

        /// <summary>
        /// IsProrataPreviousyear
        /// IsProratacurrentyear
        /// IsAvailExceptionAllowedOnSpecialAppeal
        /// LeaveType
        /// CurrentAllocation
        /// Applied
        /// PreviousYearCarryForward
        /// Applied
        /// DaysCanBeSanctioned
        /// </summary>
        /// <param name="IsProrataPreviousyear"></param>
        /// <param name="IsProratacurrentyear" ></param>
        /// <param name="IsAvailExceptionAllowedOnSpecialAppeal"></param>
        public void DaysCanbeSanctioned(DataRow SourceRow, decimal LeaveDays, decimal Balance)
        {
            try
            {
                bool proDataPrevYear = Convert.ToBoolean(SourceRow["IsProrataPreviousyear"].ToString());
                bool proDataCurrentYear = Convert.ToBoolean(SourceRow["IsProratacurrentyear"].ToString());
                bool isAvailExceptionAllowed = Convert.ToBoolean(SourceRow["IsAvailExceptionAllowedOnSpecialAppeal"].ToString());

               
                //drLocal["Applied"] = SourceRow["Applied"].ToString().Trim();
                //drLocal["Availed"] = SourceRow["Availed"].ToString().Trim();

                if (SourceRow["LeaveType"].ToString().Trim().ToUpper() != "EARN")
                {
                    if (proDataCurrentYear == false)
                    {
                        #region 01
                        if (proDataPrevYear == false)
                        {
                            LeaveDays = Convert.ToDecimal(SourceRow["CurrentAllocation"].ToString().Trim());
                            Balance = Convert.ToDecimal(SourceRow["CurrentAllocation"].ToString().Trim()) - Convert.ToDecimal(SourceRow["Applied"].ToString().Trim());
                        }
                        else
                        {
                            LeaveDays = Convert.ToDecimal(SourceRow["CurrentAllocation"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim());
                            Balance = Convert.ToDecimal(SourceRow["CurrentAllocation"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim()) - Convert.ToDecimal(SourceRow["Applied"].ToString().Trim());
                        }
                        #endregion
                    }
                    else
                    {
                        #region 02
                        if (isAvailExceptionAllowed == false)
                        {
                            if (proDataPrevYear == false)
                            {
                                LeaveDays = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim());
                                Balance = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim()) - Convert.ToDecimal(SourceRow["Applied"].ToString().Trim());
                            }
                            else
                            {
                                LeaveDays = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim());
                                Balance = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim()) - Convert.ToDecimal(SourceRow["Applied"].ToString().Trim());
                            }
                        }
                        else
                        {
                            if (proDataPrevYear == false)
                            {
                                LeaveDays = Convert.ToDecimal(SourceRow["CurrentAllocation"].ToString().Trim());
                                Balance = Convert.ToDecimal(SourceRow["CurrentAllocation"].ToString().Trim()) - Convert.ToDecimal(SourceRow["Applied"].ToString().Trim());
                            }
                            else
                            {
                                LeaveDays = Convert.ToDecimal(SourceRow["CurrentAllocation"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim());
                                Balance = Convert.ToDecimal(SourceRow["CurrentAllocation"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim()) - Convert.ToDecimal(SourceRow["Applied"].ToString().Trim());
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    LeaveDays = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim());
                    Balance = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim()) - Convert.ToDecimal(SourceRow["Applied"].ToString().Trim());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}