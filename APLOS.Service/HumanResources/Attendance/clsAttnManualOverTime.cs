using System;
using System.Data;

namespace OTSBD
{
    public class clsAttnManualOverTime
    {
        public clsAttnManualOverTime()
        {
            // TODO: Add constructor logic here
        }

        public void GetAttdnDataForMonthlyProcForManualEntry(string sGroupID, string sPlantID, string strEmpSysID, string strAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, COUNT(WorkDate) TotalProcDate, 
		                            SUM(ISNULL(TotalPresent, 0)) TotalPresent, SUM(ISNULL(TotalLate, 0)) TotalLate, 
		                            SUM(ISNULL(TotalAbsent, 0)) TotalAbsent, SUM(ISNULL(TotalLv, 0)) TotalLv, 
		                            SUM(ISNULL(TotalMLv, 0)) TotalMLv, SUM(ISNULL(TotalWeekOff, 0)) TotalWeekOff, 
		                            SUM(ISNULL(TotalHoliDay, 0)) TotalHoliDay, SUM(ISNULL(TotalWeekOffHoliDay, 0)) TotalWeekOffHoliDay,
                                    SUM(ISNULL(OTHr, 0)) TotalOTHr   
                            FROM (SELECT EmpSystemID, WorkDate,
			                            TotalPresent = CASE WHEN DayStatus = 'P' THEN 1 
                                                       WHEN DayStatus = 'WP' THEN 1 
						                               WHEN DayStatus = 'HP' THEN 1  
                                                       WHEN DayStatus = 'WHP' THEN 1 
						                               WHEN DayStatus = 'HWP' THEN 1  
                                                       ELSE 0 END,
			                            TotalLate = CASE WHEN DayStatus = 'L' THEN 1
                                                       WHEN DayStatus = 'WL' THEN 1 
						                               WHEN DayStatus = 'HL' THEN 1  
                                                       WHEN DayStatus = 'WHL' THEN 1 
						                               WHEN DayStatus = 'HWL' THEN 1  
                                                       ELSE 0 END,
			                            TotalAbsent = CASE WHEN DayStatus = 'A' THEN 1 ELSE 0 END,
			                            TotalLv = CASE WHEN DayStatus = 'LV' THEN 1 
						                               WHEN DayStatus = 'LVP' THEN 1 
						                               WHEN DayStatus = 'LVL' THEN 1
						                               WHEN DayStatus = 'WLV' THEN 1 
						                               WHEN DayStatus = 'HLV' THEN 1  
						                               WHEN DayStatus = 'WLVP' THEN 1 
						                               WHEN DayStatus = 'HLVP' THEN 1  
						                               WHEN DayStatus = 'WLVL' THEN 1 
						                               WHEN DayStatus = 'HLVL' THEN 1  
                                                       WHEN DayStatus = 'WHLV' THEN 1 
                                                       WHEN DayStatus = 'WHLVP' THEN 1 
                                                       WHEN DayStatus = 'WHLVL' THEN 1 
						                               WHEN DayStatus = 'HWLV' THEN 1  
						                               WHEN DayStatus = 'HWLVP' THEN 1  
						                               WHEN DayStatus = 'HWLVL' THEN 1  
						                               ELSE 0 END,
			                            TotalMLv = CASE WHEN DayStatus = 'MLV' THEN 1 
						                                WHEN DayStatus = 'MLVP' THEN 1 
						                                WHEN DayStatus = 'MLVL' THEN 1
						                                WHEN DayStatus = 'WMLV' THEN 1 
						                                WHEN DayStatus = 'HMLV' THEN 1  
						                                WHEN DayStatus = 'WMLVP' THEN 1 
						                                WHEN DayStatus = 'HMLVP' THEN 1  
						                                WHEN DayStatus = 'WMLVL' THEN 1 
						                                WHEN DayStatus = 'HMLVL' THEN 1  
                                                        WHEN DayStatus = 'WHMLV' THEN 1 
                                                        WHEN DayStatus = 'WHMLVP' THEN 1 
                                                        WHEN DayStatus = 'WHMLVL' THEN 1 
						                                WHEN DayStatus = 'HWMLV' THEN 1  
						                                WHEN DayStatus = 'HWMLVP' THEN 1  
						                                WHEN DayStatus = 'HWMLVL' THEN 1  
						                                ELSE 0 END,
			                            TotalWeekOff = CASE WHEN DayStatus = 'W' THEN 1 
						                                ELSE 0 END,
			                            TotalHoliDay = CASE WHEN DayStatus = 'H' THEN 1 
						                               ELSE 0 END,
                                        TotalWeekOffHoliDay = CASE WHEN DayStatus = 'WH' THEN 1 
							                            WHEN DayStatus = 'HW' THEN 1 
						                               ELSE 0 END,
                                        OTHr
	                             FROM dbo.AttdnProcessData 
                                WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                    AND EmpSystemID IN (" + strEmpSysID + @") AND MONTH(WorkDate) = MONTH('" + strAttnDate + @"')
                                    AND YEAR(WorkDate) = YEAR('" + strAttnDate + @"')) A
                            GROUP BY EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetAttdnDataMonthlySummaryForManualEntry(string sGroupID, string sPlantID, string strEmpSysID, int MonthNo, int YearNo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.AttdnDataMonthlySummary
                           WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                    AND EmpSystemID IN (" + strEmpSysID + @") 
                                    AND MonthNo = " + MonthNo + @" AND YearNo = " + YearNo + @"";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetActiveEmpCodeManualOTDataForGrdLoad(string sGroupID, string sPlantID, string strAttnDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, int iFix, string sFixShift, int iRst, string sRosterShift, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 'False'), 
								E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName, ISNULL(Atd.InTime, '00:00') InTime, ISNULL(Atd.IsManualInTime, 0) IsManualInTime,
                                ISNULL(Atd.OutTime, '00:00') OutTime, ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, Atd.DayStatus, ISNULL(Atd.OTHr, 0) OTHr, ISNULL(Atd.IsManualOTHr, 0) IsManualOTHr, 
                                ISNULL(Atd.IsLock, 0) IsLock, ES.ShiftType, ES.OfficeStartTime, OfficeTime, ES.InTimeStartMargin, ES.OfficeEndTime, ES.OTStartTime, 
                                ES.DayType, ES.BreakStratTime, ES.BreakEndTime, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle, EmOT.OTStartDate, EmOT.OTEndDate
                            FROM EmployeeInformation AS E 
                                    LEFT OUTER JOIN 
							                    HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= E.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= E.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = E.DepartmentID 
				                    LEFT OUTER JOIN 
							                    HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON E.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= E.SectionID 
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID
                                    INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            ISNULL(IsManualOTHr, 0) IsManualOTHr, IsLock 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' 
                                                           AND PlantID = '" + sPlantID + @"'
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID
                                    INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                        DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                        OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + sGroupID + @"' 
                                                        AND EDSA.PlantID = '" + sPlantID + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID
                                    INNER JOIN
                                                (
											      SELECT * FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) 
												  									    AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 1                      
										        ) EmOT ON E.SystemID = EmOT.EmpSystemID
                            WHERE (E.DOS > '" + strAttnDate + @"' OR DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                    AND E.PlantID = '" + sPlantID + @"'";

                if (sUnit.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.UnitID = '" + sUnit + "'";
                }
                if (sDevi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DivisionID = '" + sDevi + "'";
                }
                if (sDept.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DepartmentID = '" + sDept + "'";
                }
                if (sSect.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SectionID = '" + sSect + "'";
                }
                if (sSbSe.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SubSectionID = '" + sSbSe + "'";
                }
                if (sEmpC.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCategorySystemID = '" + sEmpC + "'";
                }
                if (sDeGr.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationGroupID = '" + sDeGr + "'";
                }
                if (sDesi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationSystemID = '" + sDesi + "'";
                }
                if (sEmpSysID.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND E.SystemID IN (" + sEmpSysID + ")";
                }

                if (iFix == 1 && iRst == 0)
                {
                    strSql = strSql + @"
                                        AND ES.FixSystemID = '" + sFixShift + "'";
                }
                if (iFix == 0 && iRst == 1)
                {
                    strSql = strSql + @"
                                        AND ES.RosterSystemID = '" + sRosterShift + "'";
                }
                if (iFix == 1 && iRst == 1)
                {
                    strSql = strSql + @"
                                        AND (ES.FixSystemID = '" + sFixShift + "' OR ES.RosterSystemID = '" + sRosterShift + "')";
                }

                strSql = strSql + @"
                        ORDER BY EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmpCodeLoadForOTConfirmation(string sGroupID, string sPlantID, string strAttnDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, string sFixShift, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {

                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 'True'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                  ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, ISNULL(Atd.InTime, '00:00') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, ISNULL(Atd.OutTime, '00:00') OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";

                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 ,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								                                  else ISNULL(EmOT.IsOTEntitle, 0) end 
                                ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                            FROM EmployeeInformation AS E  
                                    LEFT OUTER JOIN 
							                    HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= E.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= E.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = E.DepartmentID 
				                    LEFT OUTER JOIN 
							                    HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON E.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= E.SectionID 
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                        DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                        OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + sGroupID + @"' 
                                                        AND EDSA.PlantID = '" + sPlantID + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID
                                        ----------OT
												 left JOIN
                                                (
											      SELECT IsOTEntitle,EmpSystemID FROM dbo.EmployeeOTEntitle																                   
										        ) IsOT ON E.SystemID = IsOT.EmpSystemID

                                    ---OT entitle as per individual tagging
                                    left JOIN
                                                (
											      SELECT IsOTEntitle,EmpSystemID FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 1 
																                   
										        ) EmOT ON E.SystemID = EmOT.EmpSystemID
									       ---OT entitle as per designation
									 left JOIN  (
														SELECT DC.IsOTEntitled IsOTEntitle,Dm.DesignationId,PlantId
																			FROM MST.DesignationMaster DM
                                                        LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
													) DCOT ON DCOT.DesignationId = E.GivenDesignationId AND DCOT.PlantId=E.PlantId

									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM FinalOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS FOT ON E.SystemID = FOT.EmpSystemID ";

                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }




                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                    AND E.PlantID = '" + sPlantID + @"'
                                     and 
									(
									(	isnull(IsOT.IsOTEntitle,0)=1 and isnull(EmOT.IsOTEntitle,0)=1)
                                    or (isnull(IsOT.IsOTEntitle,0)=0 and isnull(DCOT.IsOTEntitle,0)=1)                                   
									)
                                    and e.SystemId not in (
									 SELECT EmpSystemID FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 0
									                        )
                                                            ";

                if (sUnit.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.UnitID = '" + sUnit + "'";
                }
                if (sDevi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DivisionID = '" + sDevi + "'";
                }
                if (sDept.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DepartmentID = '" + sDept + "'";
                }
                if (sSect.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SectionID = '" + sSect + "'";
                }
                if (sSbSe.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SubSectionID = '" + sSbSe + "'";
                }
                if (sEmpC.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCategorySystemID = '" + sEmpC + "'";
                }
                if (sDeGr.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationGroupID = '" + sDeGr + "'";
                }
                if (sDesi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationSystemID = '" + sDesi + "'";
                }
                if (sEmpSysID.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCode IN (" + sEmpSysID + ")";
                }
                if (sFixShift.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND ES.ShiftSystemID = '" + sFixShift + "'";
                }



                strSql = strSql + @"
                        ORDER BY EmployeeCode";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void xxGetEmpCodeLoadForOTConfirmation(string sGroupID, string sPlantID, string strAttnDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, string sFixShift, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {

                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 'True'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                  ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, ISNULL(Atd.InTime, '00:00') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, ISNULL(Atd.OutTime, '00:00') OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";

                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                            FROM EmployeeInformation AS E  
                                    LEFT OUTER JOIN 
							                    HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= E.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= E.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = E.DepartmentID 
				                    LEFT OUTER JOIN 
							                    HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON E.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= E.SectionID 
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                        DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                        OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + sGroupID + @"' 
                                                        AND EDSA.PlantID = '" + sPlantID + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID
                                    INNER JOIN
                                                (
											      SELECT * FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 1                      
										        ) EmOT ON E.SystemID = EmOT.EmpSystemID
									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM FinalOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS FOT ON E.SystemID = FOT.EmpSystemID ";

                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }




                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                    AND E.PlantID = '" + sPlantID + @"'
                                    and e.SystemId not in (
									 SELECT EmpSystemID FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 0
									                        )
                                                            ";

                if (sUnit.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.UnitID = '" + sUnit + "'";
                }
                if (sDevi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DivisionID = '" + sDevi + "'";
                }
                if (sDept.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DepartmentID = '" + sDept + "'";
                }
                if (sSect.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SectionID = '" + sSect + "'";
                }
                if (sSbSe.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SubSectionID = '" + sSbSe + "'";
                }
                if (sEmpC.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCategorySystemID = '" + sEmpC + "'";
                }
                if (sDeGr.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationGroupID = '" + sDeGr + "'";
                }
                if (sDesi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationSystemID = '" + sDesi + "'";
                }
                if (sEmpSysID.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCode IN (" + sEmpSysID + ")";
                }
                if (sFixShift.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND ES.ShiftSystemID = '" + sFixShift + "'";
                }

               

                strSql = strSql + @"
                        ORDER BY EmployeeCode";

               

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void xGetEmpCodeLoadForOTConfirmation(string sGroupID, string sPlantID, string strAttnDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, string sFixShift, string sOTValCons,  out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {


















                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 'True'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                  ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, ISNULL(Atd.InTime, '00:00') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, ISNULL(Atd.OutTime, '00:00') OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";

                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                            FROM EmployeeInformation AS E  
                                    LEFT OUTER JOIN 
							                    HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= E.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= E.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = E.DepartmentID 
				                    LEFT OUTER JOIN 
							                    HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON E.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= E.SectionID 
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID
                                    INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        AND IsOTComfirm = 0 AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID
                                    INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                        DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                        OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + sGroupID + @"' 
                                                        AND EDSA.PlantID = '" + sPlantID + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID
                                    INNER JOIN
                                                (
											      SELECT * FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 1                      
										        ) EmOT ON E.SystemID = EmOT.EmpSystemID
									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM FinalOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS FOT ON E.SystemID = FOT.EmpSystemID
									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID
                            WHERE (E.DOS > '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                    AND E.PlantID = '" + sPlantID + @"'";

                if (sUnit.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.UnitID = '" + sUnit + "'";
                }
                if (sDevi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DivisionID = '" + sDevi + "'";
                }
                if (sDept.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DepartmentID = '" + sDept + "'";
                }
                if (sSect.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SectionID = '" + sSect + "'";
                }
                if (sSbSe.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SubSectionID = '" + sSbSe + "'";
                }
                if (sEmpC.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCategorySystemID = '" + sEmpC + "'";
                }
                if (sDeGr.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationGroupID = '" + sDeGr + "'";
                }
                if (sDesi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationSystemID = '" + sDesi + "'";
                }
                if (sEmpSysID.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCode IN (" + sEmpSysID + ")";
                }
                if (sFixShift.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND ES.ShiftSystemID = '" + sFixShift + "'";
                }

                //if (iFix == 1 && iRst == 0)
                //{
                //    strSql = strSql + @"
                //                        AND ES.FixSystemID = '" + sFixShift + @"'";
                //}
                //if (iFix == 0 && iRst == 1)
                //{
                //    strSql = strSql + @"
                //                        AND ES.RosterSystemID = '" + sRosterShift + @"' AND ES.ShiftSystemID = '" + sCurtRosterShift + @"'";
                //}
                //if (iFix == 1 && iRst == 1)
                //{
                //    strSql = strSql + @"
                //                        AND (ES.FixSystemID = '" + sFixShift + @"' OR (ES.RosterSystemID = '" + sRosterShift + @"' AND ES.ShiftSystemID = '" + sCurtRosterShift + @"'))";
                //}

                strSql = strSql + @"
                        ORDER BY EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmpCodeLoadForOTConfirmationAfterSave(string sGroupID, string sPlantID, string strAttnDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, int iFix, string sFixShift, int iRst, string sRosterShift, string sCurtRosterShift, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 0), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                  ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, ISNULL(Atd.InTime, '00:00') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, ISNULL(Atd.OutTime, '00:00') OutTime, Atd.DayStatus, ISNULL(Atd.OTHr, 0) OTHr, 
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), 
                                  NormalOTHrHour = CAST((CAST(ISNULL(FOT.NormalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  NormalOTHrMinute = CAST((CAST(ISNULL(FOT.NormalOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  NormalOTHr = CAST((CAST(ISNULL(FOT.NormalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(FOT.NormalOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  NormalOTHrInDecimal = CAST(ISNULL(FOT.NormalOTHr, 0) / 60 AS DECIMAL(10, 2)), ISNULL(Atd.IsLock, 0) IsLock,
								  ES.ShiftType, ES.DayType, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                            FROM EmployeeInformation AS E  
                                    LEFT OUTER JOIN 
							                    HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= E.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= E.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = E.DepartmentID 
				                    LEFT OUTER JOIN 
							                    HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON E.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= E.SectionID 
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID
                                    INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID
                                    INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														 CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                         DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														 S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                         OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + sGroupID + @"' 
                                                        AND EDSA.PlantID = '" + sPlantID + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID
                                    INNER JOIN
                                                (
											      SELECT * FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 1                      
										        ) EmOT ON E.SystemID = EmOT.EmpSystemID
									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM FinalOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS FOT ON E.SystemID = FOT.EmpSystemID
									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID
                            WHERE (E.DOS > '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                    AND E.PlantID = '" + sPlantID + @"'";

                if (sUnit.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.UnitID = '" + sUnit + "'";
                }
                if (sDevi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DivisionID = '" + sDevi + "'";
                }
                if (sDept.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DepartmentID = '" + sDept + "'";
                }
                if (sSect.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SectionID = '" + sSect + "'";
                }
                if (sSbSe.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SubSectionID = '" + sSbSe + "'";
                }
                if (sEmpC.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCategorySystemID = '" + sEmpC + "'";
                }
                if (sDeGr.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationGroupID = '" + sDeGr + "'";
                }
                if (sDesi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationSystemID = '" + sDesi + "'";
                }
                if (sEmpSysID.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND E.SystemID IN (" + sEmpSysID + ")";
                }

                if (iFix == 1 && iRst == 0)
                {
                    strSql = strSql + @"
                                        AND ES.FixSystemID = '" + sFixShift + @"'";
                }
                if (iFix == 0 && iRst == 1)
                {
                    strSql = strSql + @"
                                        AND ES.RosterSystemID = '" + sRosterShift + @"' AND ES.ShiftSystemID = '" + sCurtRosterShift + @"'";
                }
                if (iFix == 1 && iRst == 1)
                {
                    strSql = strSql + @"
                                        AND (ES.FixSystemID = '" + sFixShift + @"' OR (ES.RosterSystemID = '" + sRosterShift + @"' AND ES.ShiftSystemID = '" + sCurtRosterShift + @"'))";
                }

                strSql = strSql + @"
                        ORDER BY EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmpCodeLoadForManualOT(string sGroupID, string sPlantID, string strAttnDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, int iFix, string sFixShift, int iRst, string sRosterShift, string sCurtRosterShift, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 0), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                  ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, 
                                  ISNULL(Atd.InTime, '00:00') InTime, ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, ISNULL(Atd.OutTime, '00:00') OutTime, Atd.DayStatus,  
                                  ISNULL(Atd.OTHr, 0) OTHr, NormalOTHrHour = CAST((CAST(ISNULL(FOT.NormalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),   
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  NormalOTHrMinute = CAST((CAST(ISNULL(FOT.NormalOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  NormalOTHr = CAST((CAST(ISNULL(FOT.NormalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(FOT.NormalOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  NormalOTHrInDecimal = CAST(ISNULL(FOT.NormalOTHr, 0) / 60 AS DECIMAL(10, 2)), ISNULL(Atd.IsLock, 0) IsLock,
								  ES.ShiftType, ES.DayType, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                            FROM EmployeeInformation AS E  
                                    LEFT OUTER JOIN 
							                    HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= E.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= E.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = E.DepartmentID 
				                    LEFT OUTER JOIN 
							                    HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON E.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= E.SectionID 
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID
                                    LEFT OUTER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID
                                    LEFT OUTER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                        DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                        OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + sGroupID + @"' 
                                                        AND EDSA.PlantID = '" + sPlantID + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID
                                    INNER JOIN
                                                (
											      SELECT * FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 1                      
										        ) EmOT ON E.SystemID = EmOT.EmpSystemID
									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM ManualOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS FOT ON E.SystemID = FOT.EmpSystemID
									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID
                            WHERE (E.DOS > '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                    AND E.PlantID = '" + sPlantID + @"'";

                if (sUnit.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.UnitID = '" + sUnit + "'";
                }
                if (sDevi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DivisionID = '" + sDevi + "'";
                }
                if (sDept.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DepartmentID = '" + sDept + "'";
                }
                if (sSect.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SectionID = '" + sSect + "'";
                }
                if (sSbSe.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SubSectionID = '" + sSbSe + "'";
                }
                if (sEmpC.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCategorySystemID = '" + sEmpC + "'";
                }
                if (sDeGr.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationGroupID = '" + sDeGr + "'";
                }
                if (sDesi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationSystemID = '" + sDesi + "'";
                }
                if (sEmpSysID.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND E.SystemID IN (" + sEmpSysID + ")";
                }

                if (iFix == 1 && iRst == 0)
                {
                    strSql = strSql + @"
                                        AND ES.FixSystemID = '" + sFixShift + @"'";
                }
                if (iFix == 0 && iRst == 1)
                {
                    strSql = strSql + @"
                                        AND ES.RosterSystemID = '" + sRosterShift + @"' AND ES.ShiftSystemID = '" + sCurtRosterShift + @"'";
                }
                if (iFix == 1 && iRst == 1)
                {
                    strSql = strSql + @"
                                        AND (ES.FixSystemID = '" + sFixShift + @"' OR (ES.RosterSystemID = '" + sRosterShift + @"' AND ES.ShiftSystemID = '" + sCurtRosterShift + @"'))";
                }

                strSql = strSql + @"
                        ORDER BY EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetManualOT(string sGroupID, string sPlantID, string strEmpSysID, string strAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * 
                            FROM [dbo].[ManualOT]
                                WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                    AND EmpSystemID IN (" + strEmpSysID + @") AND WorkDate = '" + strAttnDate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetFinalOT(string sGroupID, string sPlantID, string strEmpSysID, string strAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * 
                            FROM FinalOT
                                WHERE GroupID = '" + sGroupID + @"' ---AND PlantID = '" + sPlantID + @"' 
                                    AND EmpSystemID IN (" + strEmpSysID + @") AND WorkDate = '" + strAttnDate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetAttdnProcessData(string sGroupID, string sPlantID, string strEmpSysID, string strAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * 
                            FROM AttdnProcessData
                                WHERE GroupID = '" + sGroupID + @"' --AND PlantID = '" + sPlantID + @"' 
                                    AND EmpSystemID IN (" + strEmpSysID + @") AND WorkDate = '" + strAttnDate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        #region Preallocation

        public void GetEmpCodeLoadForOTPreallocation(string sGroupID, string sPlantID, string strAttnDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 'True'), 
								E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName,  
                                OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)), 
								ES.ShiftType, ES.DayType, ISNULL(EmOT.IsOTEntitle, 0) IsOTEntitle
                            FROM EmployeeInformation AS E  
                                    LEFT OUTER JOIN 
							                    HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= E.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= E.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = E.DepartmentID 
				                    LEFT OUTER JOIN 
							                    HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON E.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= E.SectionID 
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID
                                    INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                        DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                        OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + sGroupID + @"' 
                                                        AND EDSA.PlantID = '" + sPlantID + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID
                                    INNER JOIN
                                                (
											      SELECT * FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 1                      
										        ) EmOT ON E.SystemID = EmOT.EmpSystemID
									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID
                            WHERE (E.DOS > '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                    AND E.PlantID = '" + sPlantID + @"'";

                if (sUnit.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.UnitID = '" + sUnit + "'";
                }
                if (sDevi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DivisionID = '" + sDevi + "'";
                }
                if (sDept.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DepartmentID = '" + sDept + "'";
                }
                if (sSect.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SectionID = '" + sSect + "'";
                }
                if (sSbSe.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.SubSectionID = '" + sSbSe + "'";
                }
                if (sEmpC.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.EmployeeCategorySystemID = '" + sEmpC + "'";
                }
                if (sDeGr.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationGroupID = '" + sDeGr + "'";
                }
                if (sDesi.Trim() != "ALL")
                {
                    strSql = strSql + @"
                                        AND E.DesignationSystemID = '" + sDesi + "'";
                }
                if (sEmpSysID.Trim() != "")
                {
                    strSql = strSql + @"
                                        AND E.SystemID IN (" + sEmpSysID + ")";
                }

                strSql = strSql + @"
                        ORDER BY E.EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetPreallocatedOT(string strEmpSysID, string strAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * 
                            FROM PreallocatedOT
                                WHERE EmpSystemID IN (" + strEmpSysID + @") AND WorkDate = '" + strAttnDate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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