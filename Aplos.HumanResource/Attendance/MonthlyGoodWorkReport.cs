using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Attendance.Compliance;
using Library.Service.Extension;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance
{
    public class MonthlyGoodWorkReport
    {
        ISqlRepository _sqlRepository;
        public MonthlyGoodWorkReport()
        {
            _sqlRepository = new SqlRepository();
        }


        public void GetMonthlyGoodWorkReport(Dictionary<string, string> parameters, string frmDate, string toDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sLine, string sEmpC, string sDeGr, string sDesi, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string filters = "";

            if (parameters["PlantId"] != "'',''")
            {
                filters = filters + "and isnull(e.PlantId,'') in (" + parameters["PlantId"] + ") ";
            }
            if (parameters["EntityId"] != "'',''")
            {
                filters = filters + "and isnull(en.Id,'') in (" + parameters["EntityId"] + ") ";
            }
            if (parameters["DepartmentId"] != "'',''")
            {
                filters = filters + "and isnull(P.DepartmentId,'') in (" + parameters["DepartmentId"] + ") ";
            }
            if (parameters["SectionId"] != "'',''")
            {
                filters = filters + "and isnull(P.SectionId,'') in (" + parameters["SectionId"] + ") ";
            }
            if (parameters["SubSectionId"] != "'',''")
            {
                filters = filters + "and isnull(P.SubSectionId,'') in (" + parameters["SubSectionId"] + ") ";
            }
            if (parameters["PayrollGroupId"] != "'',''")
            {
                filters = filters + "and isnull(pg.Id,'') in (" + parameters["PayrollGroupId"] + ") ";
            }
            if (parameters["AttndGroupId"] != "'',''")
            {
                filters = filters + "and isnull(ag.Id,'') in (" + parameters["AttndGroupId"] + ") ";
            }

            try
            {
                strSql = @"SELECT A.* FROM
                                    (SELECT E.EmployeeCode, E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ,
                                            D.UserName Designation,ISNULL( LG.UserName, '') LegalDG, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
                                            S.UserName Section, SB.UserName SubSection, L.UserName Line
                                            ,ot.WorkDate , ot.OThour as TotalOTHr ,DD.UserName GivenDesignation,hr.OTConsiderOn,E.EmployeeCodeNumeric,E.PlantId , PL.UserName as Plant
                                    FROM dbo.EmployeeInformation E
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
LEFT OUTER JOIN ORG.Entity EN ON mb.EntityId=EN.Id
                                                LEFT JOIN dbo.OTfromApp ot on ot.EmpSystemId = E.SystemId
                                                LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                                                LEFT JOIN ORG.Division Dv ON P.DivisionID = Dv.Id
                                                LEFT JOIN ORG.Department Dp ON P.DepartmentID = Dp.Id
                                                LEFT JOIN ORG.Section S ON P.SectionID = S.Id
                                                LEFT JOIN ORG.SubSection SB ON P.SubSectionID = SB.Id
                                                LEFT JOIN ORG.Line L ON MB.LineID = L.Id
                                                LEFT JOIN HKP.LegalDesignation D ON E.LegalDesignationId = D.Id
                                                LEFT JOIN HKP.Designation DD ON E.GivenDesignationId = DD.Id
                                                LEFT join PlantWiseHRMSSetting hr on hr.PlantID=e.PlantId
                                                LEFT JOIN HKP.LegalDesignation LG ON LG.Id = E.LegalDesignationId
                                                left join mst.PayrollGroupMaster pgm on pgm.EmployeeId = e.SystemId
												left join hkp.PayrollGroup pg on pg.id = pgm.PayrollGroupId
                                                left join org.Plant PL on PL.Id = E.PlantId
                                                left join dbo.EmployeeAttendanceGroup eag on eag.EmployeeId = E.SystemId
                                                left join dbo.AttendanceGroup ag on ag.Id = eag.AttendanceGroupId
                                    WHERE ot.WorkDate BETWEEN '" + frmDate + @"' AND '" + toDate + @"' AND ot.OThour > 0
AND (E.EmployeeStatus<>'Separated' OR DOS >= '" + frmDate + @"' ) " + filters + @"
";

               

                strSql = strSql + @") A
                        GROUP BY A.EmployeeCode, A.EmployeeName, A.DOJ, A.Designation,A.LegalDG, A.Unit, A.Division, A.Department,
		                            A.Section, A.SubSection, A.Line, A.WorkDate, A.TotalOTHr,A.GivenDesignation,OTConsiderOn,A.EmployeeCodeNumeric, A.PlantId , A.Plant
                        ORDER BY A.Unit, A.EmployeeCodeNumeric, A.Section, A.SubSection";

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
        }

        public void GetMonthlyGoodWorkReportNew(Dictionary<string, string> parameters, string typeId,string frmDate, string toDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sLine, string sEmpC, string sDeGr, string sDesi, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string filters = "";

            if (parameters["PlantId"] != "'',''")
            {
                filters = filters + "and isnull(e.PlantId,'') in (" + parameters["PlantId"] + ") ";
            }
            if (parameters["EntityId"] != "'',''")
            {
                filters = filters + "and isnull(en.Id,'') in (" + parameters["EntityId"] + ") ";
            }
            if (parameters["DepartmentId"] != "'',''")
            {
                filters = filters + "and isnull(P.DepartmentId,'') in (" + parameters["DepartmentId"] + ") ";
            }
            if (parameters["SectionId"] != "'',''")
            {
                filters = filters + "and isnull(P.SectionId,'') in (" + parameters["SectionId"] + ") ";
            }
            if (parameters["SubSectionId"] != "'',''")
            {
                filters = filters + "and isnull(P.SubSectionId,'') in (" + parameters["SubSectionId"] + ") ";
            }
            if (parameters["PayrollGroupId"] != "'',''")
            {
                filters = filters + "and isnull(pg.Id,'') in (" + parameters["PayrollGroupId"] + ") ";
            }
            if (parameters["AttndGroupId"] != "'',''")
            {
                filters = filters + "and isnull(ag.Id,'') in (" + parameters["AttndGroupId"] + ") ";
            }

            try
            {
                strSql = @"SELECT A.* FROM
                                    (SELECT E.EmployeeCode, E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ,
                                            D.UserName Designation,ISNULL( LG.UserName, '') LegalDG, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
                                            S.UserName Section, SB.UserName SubSection, L.UserName Line
                                            ,ot.WorkDate , ot.ProcessedOT as TotalOTHr ,DD.UserName GivenDesignation,hr.OTConsiderOn,E.EmployeeCodeNumeric,E.PlantId , PL.UserName as Plant, ECT.UserName as EmployeeCodeType,BS.Basic
                                    FROM dbo.EmployeeInformation E
LEFT JOIN (
									SELECT SID.DefineAmount Basic,M.EmpInfoSystemID
FROM SalaryInfoDefine SID 
LEFT JOIN dbo.SalaryInfoDefineMaster M ON M.SystemID=SID.SalaryID
LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
WHERE SH.HeadCategory='Basic')BS ON BS.EmpInfoSystemID=E.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
LEFT OUTER JOIN ORG.Entity EN ON mb.EntityId=EN.Id
                                                left join AttdnProcessData ot on ot.EmpSystemID=e.SystemId                                
                                                LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                                                LEFT JOIN ORG.Division Dv ON P.DivisionID = Dv.Id
                                                LEFT JOIN ORG.Department Dp ON P.DepartmentID = Dp.Id
                                                LEFT JOIN ORG.Section S ON P.SectionID = S.Id
                                                LEFT JOIN ORG.SubSection SB ON P.SubSectionID = SB.Id
                                                LEFT JOIN ORG.Line L ON MB.LineID = L.Id
                                                LEFT JOIN HKP.LegalDesignation D ON E.LegalDesignationId = D.Id
                                                LEFT JOIN HKP.Designation DD ON E.GivenDesignationId = DD.Id
                                                LEFT join PlantWiseHRMSSetting hr on hr.PlantID=e.PlantId
                                                LEFT JOIN HKP.LegalDesignation LG ON LG.Id = E.LegalDesignationId
                                                left join mst.PayrollGroupMaster pgm on pgm.EmployeeId = e.SystemId
												left join hkp.PayrollGroup pg on pg.id = pgm.PayrollGroupId
                                                left join org.Plant PL on PL.Id = E.PlantId
                                                left join dbo.EmployeeAttendanceGroup eag on eag.EmployeeId = E.SystemId
                                                left join dbo.AttendanceGroup ag on ag.Id = eag.AttendanceGroupId
                                                left join dbo.EmployeeCodeType ECT on ECT.Id = e.EmployeeCodeTypeId
                                    WHERE ot.WorkDate BETWEEN '" + frmDate + @"' AND '" + toDate + @"' AND ot.ProcessedOT > 0 AND E.EmployeeCodeTypeId in (" + typeId + @")
AND (E.EmployeeStatus<>'Separated' OR DOS >= '" + frmDate + @"' ) " + filters + @"
";

               
              
                strSql = strSql + @") A
                        GROUP BY A.EmployeeCodeType,A.EmployeeCode, A.EmployeeName, A.DOJ, A.Designation,A.LegalDG, A.Unit, A.Division, A.Department,
		                            A.Section, A.SubSection, A.Line, A.WorkDate, A.TotalOTHr,A.GivenDesignation,OTConsiderOn,A.EmployeeCodeNumeric, A.PlantId , A.Plant,A.Basic
                        ORDER BY A.Unit, A.EmployeeCodeNumeric, A.Section, A.SubSection";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");


                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.BeginTransaction();
                con.getDataSet(strSql, out dsRef);
                con.CommitTransaction();
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

        public IEnumerable<object> getFilters(string CompanyId)
        {
            try
            {
                var str = @"Select isnull(e.Id,'') as EntityId , isnull(e.UserName,'') as Entity,
                        isnull(dept.Id,'') as DepartmentId , isnull(dept.UserName,'') as Department,
                        isnull(sec.Id,'') as SectionId , isnull(sec.UserName ,'') as Section,
                        isnull(ssec.Id,'') as SubSectionId , isnull(ssec.UserName,'') as SubSection,
                        isnull(pg.Id,'') as PayrollGroupId , isnull(pg.UserName,'') as PayRollGroup,
                        isnull(ag.Id , '') as AttndGroupId , isnull(ag.Username ,'') as AttndGroup,
						isnull(p.Id,'') as PlantId , isnull(ei.PlantId,'') as ppId , isnull (p.UserName,'') as PlantName
                        from
                        dbo.EmployeeInformation ei
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                        left join org.Plant p on p.Id = ei.PlantId
                        left join org.Entity e on e.PlantId = p.Id
                        left join org.Department dept on dept.Id = pr.DepartmentId
                        left join org.Section sec on sec.Id = pr.SectionId
                        left join org.SubSection ssec on ssec.Id = pr.SubSectionId
                        left join mst.PayrollGroupMaster pgm on pgm.EmployeeId = ei.SystemId
                        left join hkp.PayrollGroup pg on pg.id = pgm.PayrollGroupId
                        left join dbo.EmployeeAttendanceGroup eag on eag.EmployeeId = ei.SystemId
                        left join dbo.AttendanceGroup ag on ag.Id = eag.AttendanceGroupId
						where p.Id is not null and p.CompanyId = '" + CompanyId+@"'
                        group by e.Id , e.UserName ,
                        dept.Id , dept.UserName ,
                        sec.Id  , sec.UserName ,
                        ssec.Id  , ssec.UserName , pg.Id , pg.Username , ag.Id , ag.Username , p.Id , ei.PlantId , p.UserName
                        
                        ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }

        }
    }
}


