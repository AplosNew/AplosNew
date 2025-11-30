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
									 SELECT SH.* FROM (SELECT 'Shift' StandardName, 'Shift'  ColumnName,'Z' RType, 99 Sequence) AS SH
										INNER JOIN (
										SELECT CASE WHEN ISNULL(m.id,'')='' THEN 'NOShift' else 'Shift' END AS HasShift FROM (select 'HasShift' AS Shift) AS K
										LEFT OUTER JOIN (select * from MST.ManpowerBudget where ISNULL(ShiftDefinationId,'') <>'') AS M ON 1=1										
										) AS AC ON AC.HasShift=SH.StandardName
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

        public IEnumerable<object> GroupWiseCompanyList(string companyGroupId, string date, string stat, string EmpCat, string EmpStat)
        {
            try
            {

                string empCat = "";
                string statP = "";
                string empStat = "";
                if (EmpCat != null)
                {
                    if (EmpCat.Length > 0)
                    {
                        empCat = "and dm.EmployeeCategoryId = '" + EmpCat + @"'";
                    }

                }
                if (stat == "All")
                {
                    statP = "";
                }
                if (stat == "Direct")
                {
                    statP = " and pos.IsDirect = 1";
                }
                if (stat == "InDirect")
                {
                    statP = "and pos.IsDirect = 0";
                }

                if (EmpStat == "All")
                {
                    empStat = "";
                }
                if (EmpStat == "Active")
                {
                    empStat = " and  ei.EmployeeCurrentStatus is null";
                }
                if (EmpStat == "TBS")
                {
                    empStat = " and  ei.EmployeeCurrentStatus = 'TBS'";
                }
                if (EmpStat == "LA")
                {
                    empStat = " and  ei.EmployeeCurrentStatus ='LONG ABSENTEEISM'";
                }


                //         var str = @"Select c.Id , c.UserName, cg.Id  as ComapnyGroupId , cg.UserName as GroupName ,Sum(case when BudgetId is not null then 1 else 0 end) as BB , Count( distinct EmpSystemID) as OnRoll,
                //                      Sum(Case When InStatus = 'IN' or InStatus = 'EI' or InStatus='LI' then 1 else 0 end) as OTIN,
                //                     Sum(Case When InStatus = 'IN' then 1 else 0 end) as InStat,
                //                     Sum(Case When InStatus ='EI' then 1 else 0 end) as EarlyIn,
                //                     Sum(Case When InStatus ='LI'then 1 else 0 end) as LateIn,
                //                     Sum(Case When InStatus ='IM'  then 1 else 0 end) as InMissing,
                //                     Sum(Case When IsOD=1 then 1 else 0 end) as OD,
                //                     Sum(Case when DayStatus='W' or DayStatus='H' or DayStatus='AH' or DayStatus='CW' then 1 else 0 end) as DayStatus,
                //                     Sum(Case when LeaveStatus is not null then 1 else 0 end) as Leave,
                //                     Sum(Case When InStatus ='O' then 1 else 0 end) as Other
                //                     ,Sum(Case when ManualInTime is null and PunchInTime is not null then 1 else 0 end) as INVM
                //,Sum(Case when ManualOutTime is null and PunchOutTime is not null then 1 else 0 end) as OVM
                //                     from dbo.AttdnProcessData apd
                //                     left join org.Plant p on p.Id = apd.PlantID
                //                     left join org.Company c on c.Id = p.CompanyId
                //                     left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                //                     left join org.Position pos on pos.Id = mb.PositionId
                //                     left join org.Division div on div.Id = pos.DivisionId
                //                     left join org.SubDivision sdiv on sdiv.id = pos.SubDivisionId
                //                     left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                //                     left join org.Unit u on u.Id = ei.UnitId
                //                     left join org.CompanyGroup cg on cg.Id = c.CompanyGroupId
                //                     left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                //                     left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                //                     where c.CompanyGroupId = '" + companyGroupId+@"' and apd.WorkDate = '"+date+ @"'  " + empStat + @" " + empCat+@" "+statP+@"
                //                     group by c.Id , c.UserName , cg.id , cg.userName
                //                     order by c.UserName asc
                //                     ";

                var sql = @"Select 
                            c.Id , c.UserName, cg.Id  as ComapnyGroupId , cg.UserName as GroupName ,isnull(Sum(bud.TotalNumber),0) as BB ,isnull(Sum(Cast(bud.Deployment as decimal)),0) as Dep , isnull(Sum(orole.OnRole),0) as OnRoll,
                                                        isnull(Sum(orole.OTIN),0) as OTIN,
                                                        isnull(Sum(orole.InStat),0) as InStat,
                                                        isnull(Sum(orole.EarlyIn),0) as EarlyIn,
                                                        isnull(Sum(orole.LateIn),0) as LateIn,
                                                        isnull(Sum(orole.InMissing),0) as InMissing,
                                                        isnull(Sum(orole.OD),0) as OD,
                                                        isnull(Sum(orole.DayStatus),0) as DayStatus,
                                                        isnull(Sum(orole.Leave),0) as Leave,
                                                        isnull(Sum(orole.Other),0) as Other,
                                                        isnull(Sum(orole.INVM),0) as INVM,
							                            isnull(Sum(orole.OVM),0) as OVM 
                            from mst.ManpowerBudget mb
                            left join 
                            (
                            Select mb.Id as BudgetId,Count(distinct ei.SystemId) as OnRole,
                             Sum(Case When InStatus = 'IN' or InStatus = 'EI' or InStatus='LI' then 1 else 0 end) as OTIN,
                                                        Sum(Case When InStatus = 'IN' then 1 else 0 end) as InStat,
                                                        Sum(Case When InStatus ='EI' then 1 else 0 end) as EarlyIn,
                                                        Sum(Case When InStatus ='LI'then 1 else 0 end) as LateIn,
                                                        Sum(Case When InStatus ='IM'  then 1 else 0 end) as InMissing,
                                                        Sum(Case When IsOD=1 then 1 else 0 end) as OD,
                                                        Sum(Case when DayStatus='W' or DayStatus='H' or DayStatus='AH' or DayStatus='CW' then 1 else 0 end) as DayStatus,
                                                        Sum(Case when LeaveStatus is not null then 1 else 0 end) as Leave,
                                                        Sum(Case When InStatus ='O' then 1 else 0 end) as Other
                                                       ,Sum(Case when ap.InTime is not null and pv.InTime is null then 1 else 0 end) as INVM
							                           ,Sum(Case when ap.OutTime is not null and pv.OutTime is null then 1 else 0 end) as OVM
                            from AttdnProcessData ap
                            left join EmployeeInformation ei on ei.SystemId = ap.EmpSystemID
                            left join mst.ManpowerBudget mb on mb.Id=ei.BudgetCode
                            left join dbo.PhysicalVerification pv on pv.EmpSystemID = ap.EmpSystemID and pv.WorkDate = '" + date + @"'
                            where ap.WorkDate = '" + date + @"'  " + empStat + @"

                            group by mb.Id 
                            ) as orole on orole.BudgetId = mb.Id
                            left join 
                            (
                            Select * from (
							                            Select rank() over (partition by ManpowerBudgetId order by  mb.EffectiveDate DESC,mb.Id) RNK, mb.TotalNumber, mb.ManpowerBudgetId, mb.EffectiveDate , mmb.Deployment
                                                        from [MST].[ManpowerBudgetDetail] mb
														left join  mst.ManpowerBudget mmb on mmb.Id = mb.ManpowerBudgetId
                                                        WHERE CONVERT(DATE,(mb.EffectiveDate) )<= CONVERT(DATE,'15-Mar-2022')
			                            ) as Bud where RNK = 1
                            ) as bud on bud.ManpowerBudgetId = mb.Id
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.Plant p on p.Id = e.PlantId
                            left join org.Company c on c.Id = p.CompanyId
                            left join org.Division div on div.Id = pos.DivisionId
                            left join org.SubDivision sdiv on sdiv.id = pos.SubDivisionId
                            left join org.CompanyGroup cg on cg.Id = c.CompanyGroupId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId

                            where  c.CompanyGroupId = '" + companyGroupId + @"'  " + empCat + @" " + statP + @"
                            group by c.Id , c.UserName , cg.id , cg.userName
                            order by c.UserName asc";
                var jj = _sqlRepository.GetDataCollection(sql);
                return jj;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion GroupWiseSummaryOfDashboard

        #region DetailDrillDownOfDashboard

        public IEnumerable<object> DetailDrillDownTable(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string companyGroupId, string stat, string EmpCat, string EmpStat)
        {
            try
            {

                //seq += 1;
                string selSt = string.Empty;
                string whereSt = string.Empty;
                string groupSt = string.Empty;
                string empStat = "";
                foreach (var item in ChartColumnList)
                {

                    if (item.Sequence <= seq && item.Sequence != -2 && item.Sequence != 7)
                    {
                        whereSt = whereSt + " and " + item.ColumnName + @".Id= '" + item.Id + @"'";
                    }

                    if (item.Sequence <= seq && item.Sequence == 7)
                    {
                        whereSt = whereSt + " and " + item.ColumnName + @".SystemId='" + item.Id + @"'";
                    }

                    if (item.Sequence == seq + 1)
                    {
                        selSt = item.ColumnName + ".Id , " + item.ColumnName + @".UserName ,";
                        groupSt = "group by " + item.ColumnName + ".Id , " + item.ColumnName + @".UserName ";
                        if (item.Sequence == 7)
                        {
                            selSt = item.ColumnName + ".SystemId as Id , " + item.ColumnName + @".UserName ,";
                            groupSt = "group by " + item.ColumnName + ".SystemId , " + item.ColumnName + @".UserName ";
                        }
                    }
                }

                string empCat = "";
                string statP = "";
                if (EmpCat != null)
                {
                    if (EmpCat.Length > 0)
                    {
                        empCat = "and dm.EmployeeCategoryId = '" + EmpCat + @"'";
                    }

                }
                if (stat == "All")
                {
                    statP = "";
                }
                if (stat == "Direct")
                {
                    statP = " and pos.IsDirect = 1";
                }
                if (stat == "InDirect")
                {
                    statP = "and pos.IsDirect = 0";
                }

                if (EmpStat == "Active")
                {
                    empStat = " and  ei.EmployeeCurrentStatus is null";
                }
                if (EmpStat == "TBS")
                {
                    empStat = " and  ei.EmployeeCurrentStatus = 'TBS'";
                }
                if (EmpStat == "LA")
                {
                    empStat = " and  ei.EmployeeCurrentStatus ='LONG ABSENTEEISM'";
                }

                //         var str = @"Select "+selSt+ @"Sum(case when BudgetId is not null then 1 else 0 end) as BB , Count( distinct EmpSystemID) as OnRoll,
                //                    Sum(Case When InStatus = 'IN' or InStatus = 'EI' or InStatus='LI' then 1 else 0 end) as OTIN,
                //                     Sum(Case When InStatus = 'IN' then 1 else 0 end) as InStat,
                //                     Sum(Case When InStatus ='EI' then 1 else 0 end) as EarlyIn,
                //                     Sum(Case When InStatus='LI' then 1 else 0 end) as LateIn,
                //                     Sum(Case When InStatus ='IM'  then 1 else 0 end) as InMissing,
                //                     Sum(Case When IsOD=1 then 1 else 0 end) as OD,
                //                     Sum(Case when DayStatus='W' or DayStatus='H' or DayStatus='AH' or DayStatus='CW' then 1 else 0 end) as DayStatus,
                //                     Sum(Case when LeaveStatus is not null then 1 else 0 end) as Leave,
                //                     Sum(Case When InStatus ='O' then 1 else 0 end) as Other
                //                     ,Sum(Case when ManualInTime is null and PunchInTime is not null then 1 else 0 end) as INVM
                //,Sum(Case when ManualOutTime is null and PunchOutTime is not null then 1 else 0 end) as OVM
                //                     from dbo.AttdnProcessData apd
                //                     left join org.Plant plant on plant.Id = apd.PlantID
                //                     left join org.Company company on company.Id = plant.CompanyId
                //                     left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                //                     left join org.Position pos on pos.Id = mb.PositionId
                //                     left join org.Division division on division.Id = pos.DivisionId
                //                     left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                //                     left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                //                     left join org.Unit unit on unit.Id = ei.UnitId
                //                     left join org.CompanyGroup cg on cg.Id = company.CompanyGroupId
                //                     left join org.Department department on department.Id = pos.DepartmentId
                //                     left join org.Section section on section.Id = pos.SectionId
                //                     left join org.SubSection subsection on subsection.id = pos.SubSectionId
                //                     left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                //                     left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                //                     left join hkp.Designation desg on desg.Id = dm.DesignationId
                //                     where company.CompanyGroupId = '" + companyGroupId+@"' and apd.WorkDate = '"+date+ @"'  " + empStat + @" " + whereSt+ @"  " + empCat + @" " + statP + @"
                //                     " + groupSt+@"
                //                     ";

                var sql = @"Select " + selSt + @" isnull(Sum(bud.TotalNumber),0) as BB ,isnull(Sum(Cast(bud.Deployment as decimal)),0) as Dep , isnull(Sum(orole.OnRole),0) as OnRoll,
                            isnull(Sum(orole.OTIN),0) as OTIN,
                            isnull(Sum(orole.InStat),0) as InStat,
                            isnull(Sum(orole.EarlyIn),0) as EarlyIn,
                            isnull(Sum(orole.LateIn),0) as LateIn,
                            isnull(Sum(orole.InMissing),0) as InMissing,
                            isnull(Sum(orole.OD),0) as OD,
                            isnull(Sum(orole.DayStatus),0) as DayStatus,
                            isnull(Sum(orole.Leave),0) as Leave,
                            isnull(Sum(orole.Other),0) as Other,
                            isnull(Sum(orole.INVM),0) as INVM,
							isnull(Sum(orole.OVM),0) as OVM 
                            from mst.ManpowerBudget mb
                            left join 
                            (
                            Select mb.Id as BudgetId,Count(distinct ei.SystemId) as OnRole,
                             Sum(Case When InStatus = 'IN' or InStatus = 'EI' or InStatus='LI' then 1 else 0 end) as OTIN,
                                                        Sum(Case When InStatus = 'IN' then 1 else 0 end) as InStat,
                                                        Sum(Case When InStatus ='EI' then 1 else 0 end) as EarlyIn,
                                                        Sum(Case When InStatus ='LI'then 1 else 0 end) as LateIn,
                                                        Sum(Case When InStatus ='IM'  then 1 else 0 end) as InMissing,
                                                        Sum(Case When IsOD=1 then 1 else 0 end) as OD,
                                                        Sum(Case when DayStatus='W' or DayStatus='H' or DayStatus='AH' or DayStatus='CW' then 1 else 0 end) as DayStatus,
                                                        Sum(Case when LeaveStatus is not null then 1 else 0 end) as Leave,
                                                        Sum(Case When InStatus ='O' then 1 else 0 end) as Other
                                                        ,Sum(Case when ap.InTime is not null and pv.InTime is null then 1 else 0 end) as INVM
							                           ,Sum(Case when ap.OutTime is not null and pv.OutTime is null then 1 else 0 end) as OVM
                            from AttdnProcessData ap
                            left join EmployeeInformation ei on ei.SystemId = ap.EmpSystemID
                            left join mst.ManpowerBudget mb on mb.Id=ei.BudgetCode
                            left join dbo.PhysicalVerification pv on pv.EmpSystemID = ap.EmpSystemID and pv.WorkDate = '" + date + @"'
                            where ap.WorkDate = '" + date + @"'  " + empStat + @"

                            group by mb.Id 
                            ) as orole on orole.BudgetId = mb.Id
                            left join 
                            (
                             Select * from (
							                            Select rank() over (partition by ManpowerBudgetId order by  mb.EffectiveDate DESC,mb.Id) RNK, mb.TotalNumber, mb.ManpowerBudgetId, mb.EffectiveDate , mmb.Deployment
                                                        from [MST].[ManpowerBudgetDetail] mb
														left join  mst.ManpowerBudget mmb on mmb.Id = mb.ManpowerBudgetId
                                                        WHERE CONVERT(DATE,(mb.EffectiveDate) )<= CONVERT(DATE,'15-Mar-2022')
			                            ) as Bud where RNK = 1
                            ) as bud on bud.ManpowerBudgetId = mb.Id
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.Plant plant on plant.Id = e.PlantId
                            left join org.Company company on company.Id = plant.CompanyId
                            left join org.Division division on division.Id = pos.DivisionId
                            left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                            left join org.Unit unit on unit.Id = e.UnitId
                            left join org.Section section on section.Id = pos.SectionId
                            left join org.SubSection subsection on subsection.Id = pos.SubSectionId

                            left join org.CompanyGroup companygroup on companygroup.Id = company.CompanyGroupId
                            left join org.Department department on department.Id = pos.DepartmentId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                             where company.CompanyGroupId = '" + companyGroupId + @"'   " + whereSt + @"  " + empCat + @" " + statP + @"
                                                        " + groupSt + @"
                            
                            ";

                //string jj = str;
                string kk = sql;
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion DetailDrillDownOfDashboard

        #region DetailedListOfColumn
        public IEnumerable<object> DetailTableClick(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string companyGroupId, string Column, Dictionary<string, string> data, string stat, string EmpCat, string EmpStat)
        {
            try
            {

                //seq += 1;
                string selSt = string.Empty;
                string whereSt = string.Empty;
                string groupSt = string.Empty;
                string whereCol = string.Empty;
                string empStat = "";
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence < seq && item.Sequence != -2)
                    {
                        whereSt = whereSt + " and " + item.ColumnName + @".Id= '" + item.Id + @"'";
                    }
                    if (item.Sequence == seq)
                    {
                        if (item.Sequence == 7)
                        {
                            whereSt = whereSt + " and " + item.ColumnName + @".SystemId ='" + data["Id"] + "'";
                        }
                        else
                        {
                            whereSt = whereSt + " and " + item.ColumnName + @".Id ='" + data["Id"] + "'";
                        }

                    }
                }

                string empCat = "";
                string statP = "";
                if (EmpCat != null)
                {
                    if (EmpCat.Length > 0)
                    {
                        empCat = "and dm.EmployeeCategoryId = '" + EmpCat + @"'";
                    }

                }
                if (stat == "All")
                {
                    statP = "";
                }
                if (stat == "Direct")
                {
                    statP = " and pos.IsDirect = 1";
                }
                if (stat == "InDirect")
                {
                    statP = "and pos.IsDirect = 0";
                }

                #region settingTheColumnStat
                int secSql = 0;
                if (Column == "OnRoll")
                {
                    whereCol = "";
                }
                if (Column == "BB")
                {
                    secSql = 1;
                }
                if (Column == "InStat")
                {
                    whereCol = " and apd.InStatus = 'IN'";
                }
                if (Column == "EarlyIn")
                {
                    whereCol = " and apd.InStatus ='EI'";
                }
                if (Column == "LateIn")
                {
                    whereCol = " and  apd.InStatus ='LI'";
                }
                if (Column == "InMissing")
                {
                    whereCol = "  and apd.InStatus = 'IM'";
                }
                if (Column == "OD")
                {
                    whereCol = " and IsOD=1";
                }
                if (Column == "DayStatus")
                {
                    whereCol = " and (DayStatus='W' or DayStatus='H' or DayStatus='AH' or DayStatus='CW')";
                }
                if (Column == "Leave")
                {
                    whereCol = " and LeaveStatus is not null";
                }
                if (Column == "Other")
                {
                    whereCol = " and InStatus ='O'";
                }
                if (Column == "OTIN")
                {
                    whereCol = " and  (apd.InStatus='IN' or apd.InStatus='EI' or apd.InStatus='LI' )";
                }
                if (Column == "INVM")
                {
                    whereCol = " and  apd.InTime is not null and pv.InTime is null";
                }
                if (Column == "OVM")
                {
                    whereCol = " and  apd.OutTime is not null and pv.OutTime is null";
                }
                #endregion settingTheColumnStat


                if (EmpStat == "Active")
                {
                    empStat = " and  ei.EmployeeCurrentStatus is null";
                }
                if (EmpStat == "TBS")
                {
                    empStat = " and  ei.EmployeeCurrentStatus = 'TBS'";
                }
                if (EmpStat == "LA")
                {
                    empStat = " and  ei.EmployeeCurrentStatus ='LONG ABSENTEEISM'";
                }

                var str = "";
                if (secSql == 0)
                {
                    str = @"select x.*,y.ROEmployeeName,y.RODOJ RO1Date,z.PREmployeeName,z.PRDOJ PO1Date from (
Select ei.EmployeeCode,ei.DOJ , ei.EmployeeName,  
                            FORMAT(CAST(apd.InTime AS DATETIME),'hh:mm tt') as InTime , FORMAT(CAST(apd.OutTime AS DATETIME),'hh:mm tt') as OutTime
                            , apd.DayStatus,desg.UserName as Designation ,ei.EmployeeCurrentStatus, plant.username as Plant , mb.Code as BudgetCode, shift.Username as Shift,
                            subsection.Username as SubSection , section.UserName as Section , department.Username as Department, e.UserName as Entity,
                            FORMAT(CAST(pv.InTime AS DATETIME),'hh:mm tt') as PVIn ,FORMAT(CAST(pv.OutTime AS DATETIME),'hh:mm tt') as PVOut 
                            , DATEDIFF(MINUTE, apd.InTime, pv.InTime) as InDuration --, DATEDIFF(MINUTE, apd.OutTime, pv.OutTime) as OutDuration
                             ,TG.UserName Transport,RG.UserName Residence,ei.EntryLevel EntryType,ei.CellPhnNo MobileNo
                             ,mb.ROBudgetCode,mb.PRBudgetCode,EC.userName EmployeeCategory,L.UserName Line
                            from dbo.AttdnProcessData apd
                             left join org.Plant plant on plant.Id = apd.PlantID
                            left join org.Company company on company.Id = plant.CompanyId
                            left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                            left join org.Position pos on pos.Id = mb.PositionId
							LEFT JOIN ORG.Line L ON L.Id=MB.LineId
                            left join org.Division division on division.Id = pos.DivisionId
                            left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.Unit unit on unit.Id = e.UnitId
                            left join org.CompanyGroup cg on cg.Id = company.CompanyGroupId
                            left join org.Department department on department.Id = pos.DepartmentId
                            left join org.Section section on section.Id = pos.SectionId
                            left join org.SubSection subsection on subsection.id = pos.SubSectionId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join hkp.Designation desg on desg.Id = dm.DesignationId
                            left join org.Department dept on dept.id = pos.DepartmentId
                            left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                            left join dbo.PhysicalVerification pv on pv.EmpSystemID = apd.EmpSystemID and pv.WorkDate = '" + date + @"'
                            left join dbo.ResidenceGroup RG on RG.Id=ei.ResidenceGroupId
                            left join dbo.TransportGroup TG on TG.Id=ei.TransportGroupId
                            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
							left join MST.DesignationMaster DMM on DMM.DesignationId=ei.GivenDesignationId
							left join [HKP].[EmployeeCategory] EC on EC.Id=DMM.EmployeeCategoryId
                            
                            where company.CompanyGroupId = '" + companyGroupId + @"' and apd.WorkDate='" + date + @"' " + empStat + @" " + whereSt + @"  " + empCat + @" " + statP + @"
                            " + whereCol + @")x
                             left outer join 
(select top(1) x.DOJ RODOJ, x.EmployeeName ROEmployeeName,x.EmployeeCode ROEmployeeCode from (
Select ei.EmployeeCode,ei.DOJ , ei.EmployeeName ,mb.ROBudgetCode
                            from dbo.AttdnProcessData apd
                             left join org.Plant plant on plant.Id = apd.PlantID
                            left join org.Company company on company.Id = plant.CompanyId
                            left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Division division on division.Id = pos.DivisionId
                            left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.Unit unit on unit.Id = e.UnitId
                            left join org.CompanyGroup cg on cg.Id = company.CompanyGroupId
                            left join org.Department department on department.Id = pos.DepartmentId
                            left join org.Section section on section.Id = pos.SectionId
                            left join org.SubSection subsection on subsection.id = pos.SubSectionId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join hkp.Designation desg on desg.Id = dm.DesignationId
                            left join org.Department dept on dept.id = pos.DepartmentId
                            left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                            left join dbo.PhysicalVerification pv on pv.EmpSystemID = apd.EmpSystemID and pv.WorkDate = '13-Feb-2023'
                            left join dbo.ResidenceGroup RG on RG.Id=ei.ResidenceGroupId
                            left join dbo.TransportGroup TG on TG.Id=ei.TransportGroupId
                            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            where company.CompanyGroupId = '" + companyGroupId + @"' and apd.WorkDate='" + date + @"' " + empStat + @" " + whereSt + @"  " + empCat + @" " + statP + @"
                            " + whereCol + @") x
                             order by x.DOJ asc ) y on y.ROEmployeeCode=x.EmployeeCode
							left outer join 
(select top(1) x.DOJ PRDOJ, x.EmployeeName PREmployeeName,x.EmployeeCode PREmployeeCode from (
Select ei.EmployeeCode,ei.DOJ , ei.EmployeeName ,mb.PRBudgetCode
                            from dbo.AttdnProcessData apd
                             left join org.Plant plant on plant.Id = apd.PlantID
                            left join org.Company company on company.Id = plant.CompanyId
                            left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Division division on division.Id = pos.DivisionId
                            left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.Unit unit on unit.Id = e.UnitId
                            left join org.CompanyGroup cg on cg.Id = company.CompanyGroupId
                            left join org.Department department on department.Id = pos.DepartmentId
                            left join org.Section section on section.Id = pos.SectionId
                            left join org.SubSection subsection on subsection.id = pos.SubSectionId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join hkp.Designation desg on desg.Id = dm.DesignationId
                            left join org.Department dept on dept.id = pos.DepartmentId
                            left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                            left join dbo.PhysicalVerification pv on pv.EmpSystemID = apd.EmpSystemID and pv.WorkDate = '13-Feb-2023'
                            left join dbo.ResidenceGroup RG on RG.Id=ei.ResidenceGroupId
                            left join dbo.TransportGroup TG on TG.Id=ei.TransportGroupId
                            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            where company.CompanyGroupId = '" + companyGroupId + @"' and apd.WorkDate='" + date + @"' " + empStat + @" " + whereSt + @"  " + empCat + @" " + statP + @"
                            " + whereCol + @") x
                             order by x.DOJ asc ) z on z.PREmployeeCode=x.EmployeeCode
                            ";
                }
                else
                {
                    str = @"Select Budget.ManpowerBudgetId as BudgetId, mb.Code as BudgetCode , isnull(Budget.TotalNumber,0) as Proposed , isnull(OnRole.OnRole,0) as OnRole , 
                            (Case when  isnull(Budget.TotalNumber,0) >  isnull(OnRole.OnRole,0) then isnull(Budget.TotalNumber,0)-isnull(OnRole.OnRole,0) else 0 end ) as Short, 
                            (Case when  isnull(Budget.TotalNumber,0) <  isnull(OnRole.OnRole,0) then isnull(OnRole.OnRole,0)-isnull(Budget.TotalNumber,0) else 0 end ) as Excess,
                            company.UserName as Company,plant.UserName as PLant , Division.UserName as Division  , UNit.UserName as Unit, e.UserName as Entity , section.UserName as Section ,
                            subsection.UserName as subSection , department.UserName as Department,L.UserName Line
                            from
                            (
                            Select * from (
							                            Select rank() over (partition by ManpowerBudgetId order by  EffectiveDate DESC,Id) RNK, TotalNumber, ManpowerBudgetId, EffectiveDate
                                                        from [MST].[ManpowerBudgetDetail]
                                                        WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"' )
			                            ) as Bud where RNK = 1
                            ) as Budget
                            left join mst.ManpowerBudget mb on mb.Id = Budget.ManpowerBudgetId
                            left join (
                            Select mb.Id as BudgetId,Count(distinct ei.SystemId) as OnRole
                            from AttdnProcessData ap
                            left join EmployeeInformation ei on ei.SystemId = ap.EmpSystemID
                            left join mst.ManpowerBudget mb on mb.Id=ei.BudgetCode
                            where ap.WorkDate = '" + date + @"'   " + empStat + @"
                            group by mb.Id 

                            ) OnRole on OnRole.BudgetId = mb.Id
                             left join org.Position pos on pos.Id = mb.PositionId
							LEFT JOIN ORG.Line L ON L.Id=MB.LineId
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.Plant plant on plant.Id = e.PlantId
                            left join org.Company company on company.Id = plant.CompanyId
                            left join org.Division division on division.Id = pos.DivisionId
                            left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                            left join org.Unit unit on unit.Id = e.UnitId
                            left join org.Section section on section.Id = pos.SectionId
                            left join org.SubSection subsection on subsection.Id = pos.SubSectionId
                            left join org.CompanyGroup companygroup on companygroup.Id = company.CompanyGroupId
                            left join org.Department department on department.Id = pos.DepartmentId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                            where company.CompanyGroupId = '" + companyGroupId + @"'  " + whereSt + @"  " + empCat + @" " + statP + @"";
                }



                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion DetailedListOfColumn

        #region ReportDownload
        public DataTable ReportDownloadSvc(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string companyGroupId, string Column, Dictionary<string, string> data, string stat, string EmpCat, string EmpStat)
        {
            try
            {

                //seq += 1;
                string selSt = string.Empty;
                string whereSt = string.Empty;
                string groupSt = string.Empty;
                string whereCol = string.Empty;
                string empStat = "";
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence < seq && item.Sequence != -2)
                    {
                        whereSt = whereSt + " and " + item.ColumnName + @".Id= '" + item.Id + @"'";
                    }
                    if (item.Sequence == seq)
                    {
                        if (item.Sequence == 7)
                        {
                            whereSt = whereSt + " and " + item.ColumnName + @".SystemId ='" + data["Id"] + "'";
                        }
                        else
                        {
                            whereSt = whereSt + " and " + item.ColumnName + @".Id ='" + data["Id"] + "'";
                        }

                    }
                }

                string empCat = "";
                string statP = "";
                if (EmpCat != null)
                {
                    if (EmpCat.Length > 0)
                    {
                        empCat = "and dm.EmployeeCategoryId = '" + EmpCat + @"'";
                    }

                }
                if (stat == "All")
                {
                    statP = "";
                }
                if (stat == "Direct")
                {
                    statP = " and pos.IsDirect = 1";
                }
                if (stat == "InDirect")
                {
                    statP = "and pos.IsDirect = 0";
                }

                #region settingTheColumnStat
                if (Column == "OnRoll")
                {
                    whereCol = "";
                }
                if (Column == "BB")
                {
                    whereCol = " and apd.BudgetId is not null";
                }
                if (Column == "InStat")
                {
                    whereCol = " and apd.InStatus = 'IN'";
                }
                if (Column == "EarlyIn")
                {
                    whereCol = " and apd.InStatus ='EI'";
                }
                if (Column == "LateIn")
                {
                    whereCol = " and  apd.InStatus ='LI'";
                }
                if (Column == "InMissing")
                {
                    whereCol = "  and apd.InStatus = 'IM'";
                }
                if (Column == "OD")
                {
                    whereCol = " and IsOD=1";
                }
                if (Column == "DayStatus")
                {
                    whereCol = " and (DayStatus='W' or DayStatus='H' or DayStatus='AH' or DayStatus='CW')";
                }
                if (Column == "Leave")
                {
                    whereCol = " and LeaveStatus is not null";
                }
                if (Column == "Other")
                {
                    whereCol = " and InStatus ='O'";
                }
                if (Column == "OTIN")
                {
                    whereCol = " and  (apd.InStatus='IN' or apd.InStatus='EI' or apd.InStatus='LI' )";
                }
                if (Column == "INVM")
                {
                    whereCol = " and  apd.InTime is not null and pv.InTime is null ";
                }
                if (Column == "OVM")
                {
                    whereCol = " and  apd.OutTime is not null and pv.OutTime is null";
                }
                #endregion settingTheColumnStat


                if (EmpStat == "Active")
                {
                    empStat = " and  ei.EmployeeCurrentStatus is null";
                }
                if (EmpStat == "TBS")
                {
                    empStat = " and  ei.EmployeeCurrentStatus = 'TBS'";
                }
                if (EmpStat == "LA")
                {
                    empStat = " and  ei.EmployeeCurrentStatus ='LONG ABSENTEEISM'";
                }

                var str = @"select x.*,y.ROEmployeeName,y.RODOJ RO1Date,z.PREmployeeName,z.PRDOJ PO1Date from 
(Select ei.EmployeeCode , ei.EmployeeName , apd.DayStatus , apd.InStatus , 
                            FORMAT(CAST(apd.InTime AS DATETIME),'hh:mm tt') as InTime , FORMAT(CAST(apd.OutTime AS DATETIME),'hh:mm tt') as OutTime
                            ,desg.UserName as Designation ,ei.EmployeeCurrentStatus, plant.username as Plant , mb.Code as BudgetCode, shift.Username as Shift,
                            subsection.Username as SubSection , section.UserName as Section , department.Username as Department, e.UserName as Entity,
                            unit.UserName as Unit , dess.UserName as LDesignation,
                           FORMAT(CAST(pv.InTime AS DATETIME),'hh:mm tt') as PVIn ,FORMAT(CAST(pv.OutTime AS DATETIME),'hh:mm tt') as PVOut , Pv.AddedBy as ScannedBy , uu.FullName as ScanName  , departmentu.UserName as SDept
							, sectionu.UserName as SSec , subsectionu.UserName as SSubSec, DATEDIFF(MINUTE, apd.InTime, pv.InTime) as InDuration , DATEDIFF(MINUTE, apd.OutTime, pv.OutTime) as OutDuration, pv.OThour
                            ,TG.UserName Transport,RG.UserName Residence,ei.EntryLevel EntryType,ei.CellPhnNo MobileNo,EC.userName EmployeeCategory,L.UserName Line
                            from dbo.AttdnProcessData apd
                             left join org.Plant plant on plant.Id = apd.PlantID
                            left join org.Company company on company.Id = plant.CompanyId
                            left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                            left join org.Position pos on pos.Id = mb.PositionId
							LEFT JOIN ORG.Line L ON L.Id=MB.LineId
                            left join org.Division division on division.Id = pos.DivisionId
                            left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join org.Unit unit on unit.Id = ei.UnitId
							left join org.Entity e on e.UnitId = ei.UnitId and e.CompanyId = ei.CompanyId and e.PlantId = ei.PlantId and ei.DivisionId = e.DivisionId
                            left join org.CompanyGroup cg on cg.Id = company.CompanyGroupId
                            left join org.Department department on department.Id = pos.DepartmentId
                            left join org.Section section on section.Id = pos.SectionId
                            left join org.SubSection subsection on subsection.id = pos.SubSectionId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join hkp.Designation desg on desg.Id = dm.DesignationId
                            left join org.Department dept on dept.id = pos.DepartmentId
                            left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                            left join hkp.LegalDesignation dess on dess.Id = ei.LegalDesignationId
                            left join dbo.PhysicalVerification pv on pv.EmpSystemID = apd.EmpSystemID and pv.WorkDate = '" + date + @"'
                            left join SEC.[User] uu on uu.AuthToken = pv.AddedBy
							left join dbo.EmployeeInformation eui on eui.SystemId = uu.EmployeeId
							left join org.Department departmentu on departmentu.Id = eui.DepartmentId
                            left join org.Section sectionu on sectionu.Id = eui.SectionId
                            left join org.SubSection subsectionu on subsectionu.id = eui.SubSectionId
                           left join dbo.ResidenceGroup RG on RG.Id=ei.ResidenceGroupId
                            left join dbo.TransportGroup TG on TG.Id=ei.TransportGroupId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
							left join MST.DesignationMaster DMM on DMM.DesignationId=ei.GivenDesignationId
							left join [HKP].[EmployeeCategory] EC on EC.Id=DMM.EmployeeCategoryId

                           where company.CompanyGroupId = '" + companyGroupId + @"' and apd.WorkDate='" + date + @"' " + empStat + @" " + whereSt + @"  " + empCat + @" " + statP + @"
                            " + whereCol + @")x
                             left outer join 
(select top(1) x.DOJ RODOJ, x.EmployeeName ROEmployeeName,x.EmployeeCode ROEmployeeCode from (
Select ei.EmployeeCode,ei.DOJ , ei.EmployeeName ,mb.ROBudgetCode
                            from dbo.AttdnProcessData apd
                             left join org.Plant plant on plant.Id = apd.PlantID
                            left join org.Company company on company.Id = plant.CompanyId
                            left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Division division on division.Id = pos.DivisionId
                            left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join org.Unit unit on unit.Id = ei.UnitId
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.CompanyGroup cg on cg.Id = company.CompanyGroupId
                            left join org.Department department on department.Id = pos.DepartmentId
                            left join org.Section section on section.Id = pos.SectionId
                            left join org.SubSection subsection on subsection.id = pos.SubSectionId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join hkp.Designation desg on desg.Id = dm.DesignationId
                            left join org.Department dept on dept.id = pos.DepartmentId
                            left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                            left join dbo.PhysicalVerification pv on pv.EmpSystemID = apd.EmpSystemID and pv.WorkDate = '13-Feb-2023'
                            left join dbo.ResidenceGroup RG on RG.Id=ei.ResidenceGroupId
                            left join dbo.TransportGroup TG on TG.Id=ei.TransportGroupId
                            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            where company.CompanyGroupId = '" + companyGroupId + @"' and apd.WorkDate='" + date + @"' " + empStat + @" " + whereSt + @"  " + empCat + @" " + statP + @"
                            " + whereCol + @") x
                             order by x.DOJ asc ) y on y.ROEmployeeCode=x.EmployeeCode
							left outer join 
(select top(1) x.DOJ PRDOJ, x.EmployeeName PREmployeeName,x.EmployeeCode PREmployeeCode from (
Select ei.EmployeeCode,ei.DOJ , ei.EmployeeName ,mb.PRBudgetCode
                            from dbo.AttdnProcessData apd
                             left join org.Plant plant on plant.Id = apd.PlantID
                            left join org.Company company on company.Id = plant.CompanyId
                            left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Division division on division.Id = pos.DivisionId
                            left join org.SubDivision subdivision on subdivision.id = pos.SubDivisionId
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join org.Unit unit on unit.Id = ei.UnitId
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.CompanyGroup cg on cg.Id = company.CompanyGroupId
                            left join org.Department department on department.Id = pos.DepartmentId
                            left join org.Section section on section.Id = pos.SectionId
                            left join org.SubSection subsection on subsection.id = pos.SubSectionId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join hkp.Designation desg on desg.Id = dm.DesignationId
                            left join org.Department dept on dept.id = pos.DepartmentId
                            left join dbo.ShiftDefination shift on shift.SystemID = mb.ShiftDefinationId
                            left join dbo.PhysicalVerification pv on pv.EmpSystemID = apd.EmpSystemID and pv.WorkDate = '13-Feb-2023'
                            left join dbo.ResidenceGroup RG on RG.Id=ei.ResidenceGroupId
                            left join dbo.TransportGroup TG on TG.Id=ei.TransportGroupId
                            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            where company.CompanyGroupId = '" + companyGroupId + @"' and apd.WorkDate='" + date + @"' " + empStat + @" " + whereSt + @"  " + empCat + @" " + statP + @"
                            " + whereCol + @") x
                             order by x.DOJ asc ) z on z.PREmployeeCode=x.EmployeeCode";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion ReportDownload
    }
}
