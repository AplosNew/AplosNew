using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.Accounts;
using Library.ViewModel.Organizations;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class NewAttdnDashboardService
    {
        #region Constructor

        private readonly SqlRepository _sqlRepository;

        public NewAttdnDashboardService()
        {
            _sqlRepository = new SqlRepository();
        }

        #endregion Constructor

        #region ColumnList

        public IEnumerable<object> DrillDownList(string CompanyGroupId, string CompanyId)
        {
            try

            {
                string wcCompanyEntity = "";
                string wcCompanyPosition = "";

                if (!string.IsNullOrEmpty(CompanyId) && CompanyId != "undefined")
                {
                    wcCompanyEntity = " AND CompanyId = '" + CompanyId + @"'";
                    wcCompanyPosition = " AND t.CompanyId = '" + CompanyId + @"'";
                }
                var strSQL = @"SELECT StandardName, UserName ColumnName, RType,Sequence
									   FROM ORG.StructureRelationship
									   WHERE RType = 'Entity'  AND CompanyGroupId = '" + CompanyGroupId + @"' " + wcCompanyEntity + @"
							   UNION
							   SELECT StandardName, UserName ColumnName, RType,Sequence FROM ORG.StructureRelationship  AS k
								      WHERE RType = 'Position' AND NOT EXISTS (
																	SELECT 1
																	FROM ORG.StructureRelationship  AS t
																	WHERE t.standardname = k.standardname
									       AND t.rtype = 'Entity'  AND t.CompanyGroupId = '" + CompanyGroupId + @"' " + wcCompanyPosition + @")
										UNION
 										     SELECT LN.* FROM (SELECT 'Line' StandardName, 'Line'  ColumnName,'ZA' RType, 100 Sequence) AS LN
										INNER JOIN (
										SELECT CASE WHEN ISNULL(m.id,'')='' THEN 'NOLine' else 'Line' END AS HasLine FROM (select 'HasLine' AS Line) AS K
										LEFT OUTER JOIN ( select * from MST.ManpowerBudget where ISNULL(LineId,'') <>'' ) AS M ON 1=1										
										) AS AC ON AC.HasLine=LN.StandardName
										   ORDER BY RType,Sequence";
                DataTable dt = _sqlRepository.GetDataTable(strSQL);
                string id = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    if (id.ToUpper().Trim() == dt.Rows[i]["StandardName"].ToString().ToUpper().Trim())
                    {
                        id = dt.Rows[i]["StandardName"].ToString();
                        dt.Rows[i].Delete();
                    }
                    else
                    {
                        id = dt.Rows[i]["StandardName"].ToString();
                    }



                }
                dt.DefaultView.Sort = "RType, Sequence";
                dt = dt.DefaultView.ToTable(true);




                //return _EmployeeInformationRepository.SqlQuery<OrgStructureListViewModel>(strSQL);
                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> CompanyWiseDrillDownList(string companyGroupId, string companyId)
        {
            
            try
            {
                var sql = @"SELECT StandardName, UserName ColumnName, RType,Sequence
									   FROM ORG.StructureRelationship
									   WHERE RType = 'Entity'  AND CompanyGroupId = '" + companyGroupId + @"' AND CompanyId = '" + companyId + @"'
							   UNION
							   SELECT StandardName, UserName ColumnName, RType,Sequence FROM ORG.StructureRelationship  AS k
								      WHERE rtype = 'Position' AND NOT EXISTS (
																	SELECT 1
																	FROM ORG.StructureRelationship  AS t
																	WHERE t.standardname = k.standardname
									       and t.rtype = 'Entity'  AND t.CompanyGroupId = '" + companyGroupId + @"' AND t.CompanyId = '" + companyId + @"' ) ORDER BY RType,Sequence ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion ColumnList

        #region GroupWiseSummaryOfDashboard

        public IEnumerable<object> GroupWiseCompanyList(string companyGroupId,string date)
        {
            try
            {
                var str = @"Select c.Id , c.UserName, cg.Id  as ComapnyGroupId , cg.UserName as GroupName ,Sum(case when BudgetId is not null then 1 else 0 end) as BB , Count( distinct EmpSystemID) as OnRoll,
                            Sum(Case When InStatus = 'IN' then 1 else 0 end) as InStat,
                            Sum(Case When EarlyLateIn ='EI' then 1 else 0 end) as EarlyIn,
                            Sum(Case When EarlyLateIn='LI'then 1 else 0 end) as LateIn,
                            Sum(Case When InStatus ='IM'  then 1 else 0 end) as InMissing,
                            Sum(Case When IsOD=1 then 1 else 0 end) as OD,
                            Sum(Case when DayStatus='W' or DayStatus='H' or DayStatus='AH' or DayStatus='CW' then 1 else 0 end) as DayStatus,
                            Sum(Case when LeaveStatus is not null then 1 else 0 end) as Leave,
                            Sum(Case When InStatus ='O' then 1 else 0 end) as Other
                            from dbo.AttdnProcessData apd
                            left join org.Plant p on p.Id = apd.PlantID
                            left join org.Company c on c.Id = p.CompanyId
                            left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Division div on div.Id = pos.DivisionId
                            left join org.SubDivision sdiv on sdiv.id = pos.SubDivisionId
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join org.Unit u on u.Id = ei.UnitId
                            left join org.CompanyGroup cg on cg.Id = c.CompanyGroupId
                            where c.CompanyGroupId = '" + companyGroupId+@"' and apd.WorkDate = '"+date+@"'
                            group by c.Id , c.UserName , cg.id , cg.userName
                            order by c.UserName asc
                            ";
                var jj = _sqlRepository.GetDataCollection(str);
                return jj;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        #endregion GroupWiseSummaryOfDashboard

        #region DetailDrillDownOfDashboard

        public IEnumerable<object> DetailDrillDownTable(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string companyGroupId , Dictionary<string,string> data)
        {
            try
            {
                string ColumnId = data["Id"];
                //seq += 1;
                string selSt = string.Empty;
                string whereSt = string.Empty;
                string groupSt = string.Empty;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence <= seq &&item.Sequence!=-2)
                    {
                        whereSt = whereSt+" and " + item.ColumnName + @".Id= '" + item.Id + @"'";
                    }
                    if(item.Sequence == seq+1)
                    {
                        selSt = item.ColumnName + ".Id , " + item.ColumnName + @".UserName ,";
                        groupSt = "group by " + item.ColumnName + ".Id , " + item.ColumnName + @".UserName ";
                    }
                }
                var str = @"Select "+selSt+@"Sum(case when BudgetId is not null then 1 else 0 end) as BB , Count( distinct EmpSystemID) as OnRoll,
                            Sum(Case When InStatus = 'IN' then 1 else 0 end) as InStat,
                            Sum(Case When EarlyLateIn ='EI' then 1 else 0 end) as EarlyIn,
                            Sum(Case When EarlyLateIn='LI'then 1 else 0 end) as LateIn,
                            Sum(Case When InStatus ='IM'  then 1 else 0 end) as InMissing,
                            Sum(Case When IsOD=1 then 1 else 0 end) as OD,
                            Sum(Case when DayStatus='W' or DayStatus='H' or DayStatus='AH' or DayStatus='CW' then 1 else 0 end) as DayStatus,
                            Sum(Case when LeaveStatus is not null then 1 else 0 end) as Leave,
                            Sum(Case When InStatus ='O' then 1 else 0 end) as Other
                            from dbo.AttdnProcessData apd
                            left join org.Plant plant on plant.Id = apd.PlantID
                            left join org.Company company on company.Id = plant.CompanyId
                            left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Division division on division.Id = pos.DivisionId
                            left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join org.Unit unit on unit.Id = ei.UnitId
                            left join org.CompanyGroup cg on cg.Id = company.CompanyGroupId
                            left join org.Department department on department.Id = pos.DepartmentId
                            left join org.Section section on section.Id = pos.SectionId
                            left join org.SubSection subsection on subsection.id = pos.SubSectionId
                            where company.CompanyGroupId = '"+companyGroupId+@"' and apd.WorkDate = '"+date+@"' "+whereSt+@"
                            "+groupSt+@"
                            ";
                
                
                return _sqlRepository.GetDataCollection(str);
            }            
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion DetailDrillDownOfDashboard

    }
}
