using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.ShiftChange
{
    public class clsShiftChangeSectionWise
    {
        ISqlRepository _sqlRepository;
        public clsShiftChangeSectionWise()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> GetSection(string PlantId,string GroupId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT Id,UserName FROM [ORG].Section
                           WHERE Id IN (SELECT SectionId FROM [ORG].[CompanyGroupSection] WHERE CompanyGroupId = '" + GroupId + @"') order by UserName";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetEmployee(string selectedEmpList, string sShift, string sDay, string attDate, string Plantid, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID, string sLineID, string sDesigGrpID, string sDesigID, string sEmpCatID)
        {
            try
            {                
                string strSql = string.Empty;
                strSql = @" SELECT EI.systemid,
                                               EI.employeecode,
                                               EI.employeename,
                                               sdfx.username                             FixShift,
                                               sdrs.username                             RosterStartShift,
                                               Replace(CONVERT(VARCHAR(11), esa.effectivedate, 106), ' ', '-') effectivedate,
                                               ewd.alignwithcc,
                                               ewd.individualweekoff,
                                               ewd.fstoffday,
                                               ewd.fstdaylengthtype,
                                               ewd.sndoffday,
                                               ewd.snddaylengthtype,
                                               esa.isfix,
                                               esa.isroster,
                                               esa.rostersystemid,
                                               esa.startfromday,
                                               sr.shiftrostername,
                                               esa.fixsystemid,
                                               esa.rosterstartshiftid
                                              ,DEG.UserName GivenDesignation,DEPT.UserName Department
                                        FROM   employeeinformation EI
                                        LEFT JOIN ORG.Department DEPT ON EI.DepartmentId=DEPT.Id
									    LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id
                                        LEFT JOIN (Select EmpSystemID,MAX(EffectiveDate) EffectiveDate from EmployeeShiftAssign
										Group By EmpSystemID) ESAM ON ESAM.EmpSystemID = EI.SystemId
										INNER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = ESAM.EmpSystemID AND  ESA.EffectiveDate = ESAM.EffectiveDate
										LEFT JOIN dbo.ShiftRosterMaster SR ON ESA.RosterSystemID=SR.SystemID
                                        LEFT JOIN dbo.EmployeeWeekOffByDay EWD ON ESA.EmpSystemID = EWD.EmpSystemID AND ESA.EffectiveDate = EWD.EffectiveDate
										AND ESA.FixSystemID=EWD.FixSystemID
										LEFT JOIN ShiftDefination SDFx ON ESA.FixSystemID = SDFx.SystemID
										LEFT JOIN ShiftDefination SDRs ON ESA.RosterStartShiftID = SDRs.SystemID
										Where EI.PlantId='" + Plantid + @"' AND EI.EmployeeStatus='Active'";

                if (sUnitID != "ALL")
                {
                    strSql = strSql + @" AND UnitID = '" + sUnitID + "'";
                }
                if (sDivID != "ALL")
                {
                    strSql = strSql + @" AND DivisionID = '" + sDivID + "'";
                }
                if (sDepID != "ALL")
                {
                    strSql = strSql + @" AND DepartmentID = '" + sDepID + "'";
                }
                if (sSecID != "ALL")
                {
                    strSql = strSql + @" AND SectionID = '" + sSecID + "'";
                }
                if (sSubSecID != "ALL")
                {
                    strSql = strSql + @" AND SubSectionID = '" + sSubSecID + "'";
                }
                if (sLineID != "ALL")
                {
                    strSql = strSql + @" AND LineID = '" + sLineID + "'";
                }
                if (sDesigGrpID != "ALL")
                {
                    strSql = strSql + @" AND DesignationGroupID = '" + sDesigGrpID + "'";
                }
                if (sDesigID != "ALL")
                {
                    strSql = strSql + @" AND GivenDesignationId = '" + sDesigID + "'";
                }
                if (selectedEmpList != "")
                {
                    strSql = strSql + @" AND EI.systemid in  (" + selectedEmpList + ") ";
                }
                if (sEmpCatID != "ALL")
                {
                    strSql = strSql + @" AND EmpCategoryID = '" + sEmpCatID + "'";
                }
                if (sDay != "ALL")
                {
                    strSql = strSql + @" AND daystatus = '" + sDay + "'";
                }
                if (sShift != "ALL")
                {
                    strSql = strSql + @" AND (ESA.FixSystemID='" + sShift + "' OR EDS.ShiftSystemID='" + sShift + "') ";//and (ESA.FixSystemID='' OR EDS.ShiftSystemID='')
                }
                strSql = strSql + " order by EmployeeName";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> LoadEmployeeDailyShift(string selectedEmpList, string sShift, string sDay, string attDate, string Plantid, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID, string sLineID, string sDesigGrpID, string sDesigID, string sEmpCatID)
        {
            string month = Convert.ToDateTime(attDate).ToString("MMM");
            string year = DateTime.Now.ToString("yyyy");
            string datestart = "01-" + month + "-" + year;
            string strSql;
            try
            {
                strSql = @" SELECT EI.systemid,
                                               EI.employeecode,
                                               EI.employeename,
                                               apd.daystatus,
                                               sdfx.username                             FixShift,
                                               sdrs.username                             RosterStartShift,
                                               sdcr.username                             CurrentShift,
                                               --CONVERT(VARCHAR(8), eds.shiftintime, 108) ShiftInTime,
                                               --CONVERT(VARCHAR(8), LIT.ptime, 108) +' ('+ ARD.PType+')' AS LeastInTime,
                                                CONVERT(varchar(15),CAST(eds.shiftintime AS TIME),100) ShiftInTime,
                                                CONVERT(varchar(15),CAST(LIT.ptime AS TIME),100) +' ('+ ARD.PType+')' AS LeastInTime,
                                               --esa.effectivedate,
                                               Replace(CONVERT(VARCHAR(11), esa.effectivedate, 106), ' ', '-') effectivedate,
                                               ewd.alignwithcc,
                                               ewd.individualweekoff,
                                               ewd.fstoffday,
                                               ewd.fstdaylengthtype,
                                               ewd.sndoffday,
                                               ewd.snddaylengthtype,
                                               CONVERT(VARCHAR(8), apd.intime, 108)      InTime,
                                               esa.isfix,
                                               esa.isroster,
                                               esa.rostersystemid,
                                               esa.startfromday,
                                               sr.shiftrostername,
                                               esa.fixsystemid,
                                               esa.rosterstartshiftid
                                               ,DEG.UserName GivenDesignation,DEPT.UserName Department
                                        FROM   EmployeeInformation EI
                                            LEFT JOIN ORG.Department DEPT ON EI.DepartmentId=DEPT.Id
									        LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id
                                            INNER JOIN [dbo].[EmpDateWiseShiftAssign] EDS ON EDS.EmpSystemID = EI.SystemId AND EDS.WorkDate = '" + attDate + @"'
                                           LEFT JOIN EmployeeShiftAssign ESA ON ESA.SystemID = EDS.EmpSftAssiSystemID
										LEFT JOIN dbo.ShiftRosterMaster SR ON ESA.RosterSystemID=SR.SystemID
                                        LEFT JOIN dbo.EmployeeWeekOffByDay EWD ON ESA.EmpSystemID = EWD.EmpSystemID AND ESA.EffectiveDate = EWD.EffectiveDate
										AND ESA.FixSystemID=EWD.FixSystemID
										left JOIN (Select * from AttdnProcessData Where WorkDate = '" + attDate + @"') APD ON EI.SystemId = APD.EmpSystemID
										LEFT JOIN ShiftDefination SDFx ON ESA.FixSystemID = SDFx.SystemID
										LEFT JOIN ShiftDefination SDRs ON ESA.RosterStartShiftID = SDRs.SystemID
										LEFT JOIN ShiftDefination SDCr ON EDS.ShiftSystemID = SDCr.SystemID
                                        
	                                    LEFT JOIN
										(
										SELECT LogDownLoadNum
										,min(ptime) ptime
										FROM AttdnRawData
										WHERE pdate='" + attDate + @"'
										GROUP BY LogDownLoadNum
										) LIT on LIT.LogDownLoadNum=EI.SystemId
                                        LEFT JOIN (select distinct PType,LogDownLoadNum,PTime from AttdnRawData) ARD ON  ARD.LogDownLoadNum =LIT.LogDownLoadNum AND ARD.PTime=LIT.ptime
										WHERE EI.PlantId='" + Plantid + @"' AND (DOS IS NULL OR DOS>='" + datestart + @"' OR EmployeeStatus<>'Separated')";
                if (sUnitID != "ALL")
                {
                    strSql = strSql + @" AND UnitID = '" + sUnitID + "'";
                }
                if (sDivID != "ALL")
                {
                    strSql = strSql + @" AND DivisionID = '" + sDivID + "'";
                }
                if (sDepID != "ALL")
                {
                    strSql = strSql + @" AND DepartmentID = '" + sDepID + "'";
                }
                if (sSecID != "ALL")
                {
                    strSql = strSql + @" AND SectionID = '" + sSecID + "'";
                }
                if (sSubSecID != "ALL")
                {
                    strSql = strSql + @" AND SubSectionID = '" + sSubSecID + "'";
                }
                if (sLineID != "ALL")
                {
                    strSql = strSql + @" AND LineID = '" + sLineID + "'";
                }
                if (sDesigGrpID != "ALL")
                {
                    strSql = strSql + @" AND DesignationGroupID = '" + sDesigGrpID + "'";
                }
                if (sDesigID != "ALL")
                {
                    strSql = strSql + @" AND GivenDesignationId = '" + sDesigID + "'";
                }
                if (selectedEmpList != "")
                {
                    strSql = strSql + @" AND EI.systemid in  (" + selectedEmpList + ") ";
                }
                if (sEmpCatID != "ALL")
                {
                    strSql = strSql + @" AND EmpCategoryID = '" + sEmpCatID + "'";
                }
                if (sDay != "ALL")
                {
                    strSql = strSql + @" AND daystatus = '" + sDay + "'";
                }
                if (sShift != "ALL")
                {
                    strSql = strSql + @" AND (ESA.FixSystemID='" + sShift + "' OR EDS.ShiftSystemID='" + sShift + "') ";//and (ESA.FixSystemID='' OR EDS.ShiftSystemID='')
                }
                strSql = strSql + " order by EmployeeName";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

    }
}
