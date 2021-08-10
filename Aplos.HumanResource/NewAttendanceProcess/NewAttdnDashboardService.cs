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

        public IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId, string CompanyId)
        {
            try
            {
                var sql = @"SELECT StandardName, UserName ColumnName, RType,Sequence
									   FROM ORG.StructureRelationship
									   WHERE RType = 'Entity'  AND CompanyGroupId = '" + CompanyGroupId + @"' --AND CompanyId = 'C20181'
							   UNION
							   SELECT StandardName, UserName ColumnName, RType,Sequence FROM ORG.StructureRelationship  AS k
								      WHERE rtype = 'Position' AND NOT EXISTS (
																	SELECT 1
																	FROM ORG.StructureRelationship  AS t
																	WHERE t.standardname = k.standardname
									       and t.rtype = 'Entity'  AND t.CompanyGroupId = '" + CompanyGroupId + @"') ORDER BY RType,Sequence ";
                return _sqlRepository.GetModelCollection<OrgStructureListViewModel>(sql).AsEnumerable();

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

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

        #region GroupWiseSummaryOfManpowerBudget

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

        #endregion GroupWiseSummaryOfManpowerBudget

        #region DetailDrillDownOfManpowerBudget

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

        #endregion DetailDrillDownOfManpowerBudget

        #region Modal for EmployeSummary List

        public IEnumerable<object> ModalGroupWiseEmlpoyeeList(string CompanyGroupId, IEnumerable<ChartColumnList> ChartColumnList, int seq, string status, string EmplyeeTypeOrCategoryId)
        {
            string sqltext = "";
            var EmployeeCategory = string.Empty;
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeCategory = "";
            }
            else
            {
                EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            }
            var cList = string.Empty;
            var wc = string.Empty;
            var Join = string.Empty;
            var cListName = string.Empty;
            var dStatus = string.Empty;

            try
            {
                if (status == "Default")
                {
                    dStatus = "";
                }
                else if (status == "Direct")
                {
                    dStatus = "and PO.IsDirect = 1";
                }
                else if (status == "Indirect")
                {
                    dStatus = "and PO.IsDirect = 0";
                }
                if (seq == -2)
                {
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                if (item.StandardName == "EmployeeGroup")
                                {
                                    Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "DefinationId\n";
                            }
                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                wc = " and c.id=" + item.Id;
                            }
                            else
                            {
                                if (item.Sequence < seq)
                                {
                                    if (item.RType == "Z")
                                    {
                                        wc += " and " + item.StandardName + "Defination.SystemId='" + item.Text + "'";
                                    }
                                    else
                                    {
                                        wc += " and " + item.StandardName + ".Id='" + item.Text + "'";

                                    }
                                }
                            }
                        }
                    }

                    sqltext = @"SELECT EmployeeName EmployeeName , EM.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), EM.DOJ, 106), ' ', '-') DOJ, MB.Code BudgetCode ,GDes.UserName GivenDesignation,PDes.UserName designation,EmpC.UserName EmployeeCategory ,TotalSalary TotalSalary,c.UserName Company  " + cList + @"

                            from [dbo].[EmployeeInformation] em
                                  LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                  LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                  LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= C.Id
                                  LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                  LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
								  LEFT JOIN [HKP].Designation PDes ON PDes.Id = Po.DesignationId
								  LEFT JOIN [HKP].Designation GDes ON GDes.Id = em.GivenDesignationId
                                  LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = em.GivenDesignationId
                                  LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                 " + Join + @"

                                  WHERE EmployeeStatus = 'Active' " + dStatus + @" AND
                                   CG.Id  = '" + CompanyGroupId + @"' " + EmployeeCategory + @" ";
                    //return _sqlRepository.GetGridData(parameters);

                    return _sqlRepository.GetDataCollection(sqltext);
                }
                else
                {
                    seq += 1;
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n";
                            }
                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                wc = " and em.CompanyId='" + item.Id + "'";
                            }
                            else
                            {
                                if (item.Sequence < seq)
                                {
                                    if (item.RType == "Z")
                                    {
                                        wc += " and " + item.StandardName + "Defination.SystemId='" + item.Text + "'";
                                    }
                                    else
                                    {
                                        wc += " and " + item.StandardName + ".Id='" + item.Text + "'";

                                    }
                                }
                            }
                        }
                    }

                    sqltext = @"SELECT EmployeeName EmployeeName , EM.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), EM.DOJ, 106), ' ', '-') DOJ, MB.Code BudgetCode ,GDes.UserName GivenDesignation,PDes.UserName designation,EmpC.UserName EmployeeCategory ,TotalSalary TotalSalary,c.UserName Company   " + cList + @"
                                 from [dbo].[EmployeeInformation] em
                                  LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                  LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                  LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= C.Id
                                  LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                  LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
								  LEFT JOIN [HKP].Designation PDes ON PDes.Id = Po.DesignationId
								  LEFT JOIN [HKP].Designation GDes ON GDes.Id = em.GivenDesignationId
                                  LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId =  em.GivenDesignationId
                                  LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                 " + Join + @"
                                  where EmployeeStatus = 'Active' " + dStatus + @" AND
                                   Em.GroupID  = '" + CompanyGroupId + @"'" + wc + @" " + EmployeeCategory + @"";

                    return _sqlRepository.GetDataCollection(sqltext);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal for EmployeSummary List

        #region Modal for DynamicdetaliEmlpoyee List

        public IEnumerable<object> ModalEmlpoyeeListDetail(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, string companyId, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var strSql = "";
            var EmployeeCategory = string.Empty;
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeCategory = "";
            }
            else
            {
                EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            }

            var cList = string.Empty;
            var wc = string.Empty;
            var join = string.Empty;
            var cListName = string.Empty;
            var wcc = string.Empty;

            var dStatus = string.Empty;
            try
            {
                 if (status == "Direct")
                {
                    dStatus = "and POS.IsDirect = 1";
                }
                else if (status == "Indirect")
                {
                    dStatus = "and POS.IsDirect = 0";
                }
                seq += 1;
                //var cList = string.Empty;
                //var wc = string.Empty;
                var Join = string.Empty;
                //var cListName = string.Empty;
                //var wcc = string.Empty;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";

                            if (item.StandardName == "EmployeeGroup")
                            {
                                Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                            }
                        }
                        if (item.RType == "Position")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                        }
                        if (item.RType == "Z")
                        {
                            cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                            Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                        }
                        if (item.RType == "ZA")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n";
                        }
                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = " AND  ENT.CompanyId='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    wc += " AND ISNULL(" + item.StandardName + "Defination.SystemId,'')='" + item.Text + "'";

                                }
                                else
                                {
                                    wc += " AND " + item.StandardName + ".Id='" + item.Text + "'";

                                }
                            }
                        }
                    }
                }
                //wc = whereClausebuilder.ToString();
                //join = joinBuilder.ToString();
                //cList = colListbuilder.ToString();

                strSql = @"SELECT ROW_NUMBER() OVER (ORDER BY BudgetCode) AS SL,EmployeeName EmployeeName ,REPLACE(CONVERT(VARCHAR(11), EM.DOJ, 106), ' ', '-') DOJ, MB.Code BudgetCode ,GDes.UserName GivenDesignation,PDes.UserName designation,EmpC.UserName EmployeeCategory ,TotalSalary TotalSalary,c.UserName Company   " + cList + @"
                            FROM [dbo].[EmployeeInformation] em
                                  LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                             LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupID
                                  LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                                  LEFT outer JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
                                  LEFT outer JOIN [ORG].[Position] AS POS ON POS.Id = MB.PositionId
								  LEFT JOIN [HKP].Designation PDes ON PDes.Id = POS.DesignationId
								  LEFT JOIN [HKP].Designation GDes ON GDes.Id = em.GivenDesignationId
                                  LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId =  em.GivenDesignationId
                                  LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                 " + Join + @"
                                  where EmployeeStatus = 'Active' " + dStatus + @"
                                  AND em.GroupID  = '" + companyGroupId + @"' and  em.CompanyId= '" + companyId + @"' " + wc + @" " + EmployeeCategory + @"";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal for DynamicdetaliEmlpoyee List

        #region Modal for BudgetSummary List

        public IEnumerable<object> ModalBudgetSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters,string companyGroupId)
        {
            var EmployeeCategory = string.Empty;
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeCategory = "";
            }
            else
            {
                EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            }
            var cList = string.Empty;
            var wc = string.Empty;
            var Join = string.Empty;
            var cListId = string.Empty;
            var cListext = string.Empty;
            var cListextId = string.Empty;
            var cListEmp = string.Empty;
            var cListFinish = string.Empty;
            var cListOId = string.Empty;
            var cListextM = string.Empty;
            var cListextIdM = string.Empty;
            var cListextF = string.Empty;
            var cListextIdF = string.Empty;
            var cListEmpG = string.Empty;
            var DStatus = string.Empty;
            if (status == "Default")
            {
                DStatus = "";
            }
            else if (status == "Direct")
            {
                DStatus = "and PO.IsDirect = 1";
            }
            else if (status == "Indirect")
            {
                DStatus = "and PO.IsDirect = 0";
            }
            try
            {
                var sqlText = "";
                if (seq == -2)
                {

                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                //cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                                cListextM += ",m." + item.StandardName;

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                //cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                                cListextM += ",m." + item.StandardName;
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                //cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListext += "," + item.StandardName + "Defination.UserName  " + item.StandardName;
                                cListextM += ",m." + item.StandardName;
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                //cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                                cListextM += ",m." + item.StandardName;
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n";
                            }
                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                wc = " AND  E.CompanyId='" + item.Id + "'";
                            }
                            else
                            {
                                if (item.Sequence < seq)
                                {
                                    if (item.RType == "Z")
                                    {
                                        wc += " AND " + item.StandardName + "Defination.Id='" + item.Text + "'";
                                    }
                                    else
                                    {
                                        wc += " AND " + item.StandardName + ".Id='" + item.Text + "'";

                                    }
                                }
                            }
                        }
                    }


                    sqlText = @"SELECT m.Id MbId,Code BudgetCode
                                ,ISNULL(e.TotalManpower,0) AS onRole
                                ,ISNULL(b.TotalNumber,0) AS Proposed
                                ,Excess = CASE WHEN isNull(TotalManpower,0) - isNull(TotalNumber,0) > 0
                                           THEN isNull(TotalManpower,0) - isNull(TotalNumber,0) ELSE 0 end
                                ,Short = CASE
                                  WHEN isNull(TotalNumber,0) - isNull(TotalManpower,0) > 0
                                  THEN isNull(TotalNumber,0) - isNull(TotalManpower,0)
                                  ELSE 0 end
                                   ,m.Code budgetCodeE
                                ,m.GroupName
                                ,m.CompanyId
                                ,m.CName as CompanyName
                                " + cListextM + @"
                                ,m.Designation
                                ,m.DesGName
                                ,m.EmployeeCategory
                                 from
                                ----------------------------1 bc--------------------------------------
                                (select MB.Code, MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName
                                 " + cListext + @",Des.UserName Designation,EmpC.UserName EmployeeCategory,DesG.UserName DesGName
                                      from [MST].[ManpowerBudget]  MB
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
                                      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
                                      LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId
                                      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                     " + Join + @"
                                     where Cg.Id = '" + companyGroupId + @"' " + DStatus + @" " + wc + @" " + EmployeeCategory + @" AND MB.Active = 1
                                    )  m
                                    -----------------------2e--------------------------------
                                    left outer join
                                     (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID
                                       FROM [dbo].[EmployeeInformation]  em
                                      LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= C.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId

										LEFT JOIN [HKP].Designation GDes ON GDes.Id = EM.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                     " + Join + @"

                                       WHERE EmployeeStatus = 'Active'
                                        AND em.GroupID = '" + companyGroupId + @"'  " + DStatus + @"
                                        group by BudgetCode,em.GroupID
                                       ) e on m.Id=e.BudgetCode and e.GroupID = m.CgId
                                     -------------------------3b--------------------------------------------------------
                                      left outer join
                                      (
                                         select MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id as cid from
                                            (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                        ) MBD
                                      left outer join [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      " + Join + @"
                                      where CG.Id = '" + companyGroupId + @"'  " + DStatus + @" " + wc + @"
                                     ) B
                                     ON m.id = b.ManpowerBudgetId AND b.Id = m.CgId AND B.cid = m.CompanyId
                                		 WHERE TotalNumber > 0
                                     group by m.Code,GroupName,CompanyId,m.Id,b.ManpowerBudgetId,TotalManpower,TotalNumber,CName " + cListextM + @"
                                     ,m.Designation,m.EmployeeCategory,m.DesGName";

                    return _sqlRepository.GetDataCollection(sqlText);
                }
                else
                {
                    seq += 1;


                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                                cListextM += ",m." + item.StandardName;

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                                cListextM += ",m." + item.StandardName;
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                cListext += "," + item.StandardName + "Defination.UserName  " + item.StandardName + "Defination";
                                cListextM += ",m." + item.StandardName + "Defination";
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                                cListextM += ",m." + item.StandardName;
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n";
                            }
                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                wc += " and c.id='" + item.Id + "'";
                            }
                            else
                            {
                                if (item.Sequence < seq)
                                {
                                    if (item.RType == "Z")
                                    {
                                        wc += " and " + item.StandardName + "Defination.Id='" + item.Text + "'";

                                        cListextId += "," + item.StandardName + "Defination.Id  " + item.StandardName + "Id";
                                        cListEmpG += ",em." + item.StandardName + "Defination.Id  ";
                                        cListEmp += " and e." + item.StandardName + "DefinationId = m." + item.StandardName + "Id";

                                        cListextIdM += ",m." + item.StandardName + "DefinationId";
                                        cListextF = "," + item.StandardName + "DefinationName";
                                        cListextIdF = "," + item.StandardName + "DefinationId";
                                        cListFinish += " and B." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId";
                                    }
                                    else
                                    {
                                        wc += " and " + item.StandardName + ".Id='" + item.Text + "'";

                                        cListextId += "," + item.StandardName + ".Id  " + item.StandardName + "Id";
                                        cListEmpG += ",em." + item.StandardName + ".Id  ";
                                        cListEmp += " and e." + item.StandardName + "Id = m." + item.StandardName + "Id";

                                        cListextIdM += ",m." + item.StandardName + "Id";
                                        cListextF = "," + item.StandardName + "Name";
                                        cListextIdF = "," + item.StandardName + "Id";
                                        cListFinish += " and B." + item.StandardName + "Id = m." + item.StandardName + "Id";
                                    }

                                }
                            }
                        }
                    }






                    //cListextIdM = cListextIdMBuilder.ToString();
                    //cListFinish = cListFinishBuilder.ToString();
                    //cListEmp = cListEmpBuilder.ToString();
                    //cListEmpG = cListEmpGBuilder.ToString();
                    //cListextId = cListextIdBuilder.ToString();
                    //wc = whereClausebuilder.ToString();
                    ////join = joinBuilder.ToString();
                    //cListextM = cListextMBuilder.ToString();
                    //cListext = cListextBuilder.ToString();
                    //cList = cListBuilder.ToString();

                    sqlText = @"select m.Id MbId,Code BudgetCode
                                ,isnull(e.TotalManpower,0) as onRole
                                ,isnull(b.TotalNumber,0) as Proposed
                                ,Excess = CASE WHEN isNull(TotalManpower,0) - isNull(TotalNumber,0) > 0
                                           THEN isNull(TotalManpower,0) - isNull(TotalNumber,0) ELSE 0 end
                                ,Short = CASE
                                  WHEN isNull(TotalNumber,0) - isNull(TotalManpower,0) > 0
                                  THEN isNull(TotalNumber,0) - isNull(TotalManpower,0)
                                  ELSE 0 end
                                   ,m.Code budgetCodeE
                                ,m.GroupName
                                ,m.CompanyId
                                ,m.CName as CompanyName
                                " + cListextM + @"
                                ,m.Designation
                                ,m.DesGName DesGName
                                ,m.EmployeeCategory
                                 from
                                ----------------------------1 bc--------------------------------------
                                (select MB.Code, MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName
                                 " + cListext + @",Des.UserName Designation,EmpC.UserName EmployeeCategory, DesG.UserName DesGName
                                      from [MST].[ManpowerBudget]  MB
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
                                      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
                                      LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId
                                      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                     " + Join + @"
                                     where Cg.Id = '" + companyGroupId + @"' " + wc + @" " + EmployeeCategory + @" AND MB.Active = 1
                                    )  m
                                    -----------------------2e--------------------------------
                                    left outer join
                                     (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID
                                       FROM [dbo].[EmployeeInformation]  em
                                      LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= C.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = EM.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                     " + Join + @"

                                       WHERE EmployeeStatus = 'Active' " + DStatus + @"
                                        AND em.GroupID = '" + companyGroupId + @"' " + EmployeeCategory + @"
                                        group by BudgetCode,em.GroupID
                                       ) e on m.Id = e.BudgetCode and e.GroupID = m.CgId
                                     -------------------------3b--------------------------------------------------------
                                      left outer join
                                      (
                                         select MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id as cid from
                                            (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                        ) MBD
                                      left outer join [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      " + Join + @"
                                      where CG.Id = '" + companyGroupId + @"'  " + wc + @" " + DStatus + @" " + EmployeeCategory + @"
                                     ) B
                                     on m.id = b.ManpowerBudgetId and b.Id = m.CgId and B.cid = m.CompanyId
                                		 where TotalNumber>0
                                     group by m.Code,GroupName,CompanyId,m.Id,b.ManpowerBudgetId,TotalManpower,TotalNumber,CName " + cListextM + @"
                                     ,m.Designation,m.EmployeeCategory,m.DesGName";

                    return _sqlRepository.GetDataCollection(sqlText);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal for BudgetSummary List

        #region Modal for BudgetDetail List

        public IEnumerable<Object> ModalBudgetDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters,string companyGroupId)
        {
            var EmployeeCategory = string.Empty;
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeCategory = "";
            }
            else
            {
                EmployeeCategory = @"AND EmpC.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            }

            var cList = string.Empty;
            var wc = string.Empty;
            var join = string.Empty;
            var cListId = string.Empty;
            var cn = string.Empty;
            var wcExt = string.Empty;
            var cListext = string.Empty;
            var cListextId = string.Empty;
            var cListEmp = string.Empty;
            var cListFinish = string.Empty;
            var cListOId = string.Empty;
            var cListextM = string.Empty;
            var cListextIdM = string.Empty;
            var cListextF = string.Empty;
            var cListextIdF = string.Empty;
            var cListEmpG = string.Empty;
            var DStatus = string.Empty;

            if (status == "Default")
            {
                DStatus = "";
            }
            else if (status == "Direct")
            {
                DStatus = "and PO.IsDirect = 1";
            }
            else if (status == "Indirect")
            {
                DStatus = "and PO.IsDirect = 0";
            }
            try
            {
                var sqlText = "";
                seq += 1;


                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {

                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                            cListextM += ",m." + item.StandardName;

                            if (item.StandardName == "EmployeeGroup")
                            {
                                join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        if (item.RType.Trim().ToUpper() == "POSITION")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                            cListextM += ",m." + item.StandardName;
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                        }
                        if (item.RType == "Z")
                        {
                            cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                            cListext += "," + item.StandardName + "Defination.UserName  " + item.StandardName + "Defination";
                            cListextM += ",m." + item.StandardName + "Defination";
                            join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                        }
                        if (item.RType == "ZA")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                            cListextM += ",m." + item.StandardName;
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n";
                        }

                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc += " and c.id='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    wc += " AND ISNULL(" + item.StandardName + "Defination.SystemId,'')='" + item.Text + "'";

                                    wcExt += " AND ISNULL(" + item.StandardName + "DefinationId,'')='" + item.Text + "'";

                                    cListextId += "," + item.StandardName + "Defination.SystemId  " + item.StandardName + "DefinationId";
                                    cListEmpG += "," + item.StandardName + "Defination.Id  ";
                                    cListEmp += " and e." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId";

                                    cListextIdM += ",m." + item.StandardName + "DefinationId";
                                    cListextF = "," + item.StandardName + "DefinationName";
                                    cListextIdF = "," + item.StandardName + "DefinationId";
                                    cListFinish += " and B." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId";
                                }
                                else
                                {

                                    wc += " and " + item.StandardName + ".Id='" + item.Text + "'";

                                    wcExt += " and " + item.StandardName + "Id='" + item.Text + "'";

                                    cListextId += "," + item.StandardName + ".Id  " + item.StandardName + "Id";
                                    cListEmpG += "," + item.StandardName + ".Id  ";
                                    cListEmp += " and e." + item.StandardName + "Id = m." + item.StandardName + "Id";

                                    cListextIdM += ",m." + item.StandardName + "Id";
                                    cListextF = "," + item.StandardName + "Name";
                                    cListextIdF = "," + item.StandardName + "Id";
                                    cListFinish += " and B." + item.StandardName + "Id = m." + item.StandardName + "Id";
                                }

                            }
                        }
                    }
                }

                sqlText = @"SELECT m.Id MbId,Code BudgetCode
                                ,ISNULL(e.TotalManpower,0) as onRole
                                ,ISNULL(b.TotalNumber,0) as Proposed
                                ,Excess = CASE
                                  WHEN ISNULL(TotalManpower,0) - isNull(TotalNumber,0) > 0
                                  THEN ISNULL(TotalManpower,0) - isNull(TotalNumber,0)
                                  ELSE 0 end
                                ,Short = CASE
                                  WHEN ISNULL(TotalNumber,0) - isNull(TotalManpower,0) > 0
                                  THEN ISNULL(TotalNumber,0) - isNull(TotalManpower,0)
                                  ELSE 0 end
                                 ,m.Code budgetCodeE
                                ,m.GroupName
                                ,m.CompanyId
                                ,m.CName as CompanyName
                                " + cListextM + @"
                                ,Designation
                                ,m.DesGName
                                ,EmployeeCategory
                                from
                                ----------------------------1 bc--------------------------------------
                                (SELECT  MB.Code,MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName " + cListext + @"
                                    ,Des.UserName Designation,EmpC.UserName EmployeeCategory,DesG.UserName DesGName
                                      from [MST].[ManpowerBudget]  MB
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
                                      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
                                      LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId
                                      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                      " + join + @"
                                      WHERE Cg.Id = '" + companyGroupId + @"' " + wc + @" " + EmployeeCategory + @" AND MB.Active = 1
                                )  m
                                    -----------------------2e--------------------------------
                                LEFT OUTER JOIN
                                (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID
                                      FROM [dbo].[EmployeeInformation]  em
                                      LEFT outer join [MST].[ManpowerBudget] AS MB  on MB.Id = em.BudgetCode
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= C.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId

									  LEFT JOIN [HKP].Designation GDes ON GDes.Id = EM.GivenDesignationId
								      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.GivenDesignationId
								      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                     " + join + @"
                                      WHERE EmployeeStatus = 'Active'  " + DStatus + @"
                                        AND em.GroupID = '" + companyGroupId + @"' " + EmployeeCategory + @"
                                        group by BudgetCode,em.GroupID
                                ) e ON m.Id=e.BudgetCode and e.GroupID = m.CgId
                                     -------------------------3b--------------------------------------------------------
                                LEFT OUTER JOIN
                                (
                                 SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id as cid from
                                 (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                 ) MBD
                                 LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId
                                 LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                 LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                 LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                 LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = PO.DesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                 " + join + @"
                                 WHERE CG.Id = '" + companyGroupId + @"' " + DStatus + @" " + wc + @" " + EmployeeCategory + @"
                                 ) B
                                 ON m.id = b.ManpowerBudgetId AND b.Id = m.CgId AND B.cid = m.CompanyId
             	                		 WHERE TotalNumber>0
                                 GROUP BY m.Code,GroupName,CompanyId,m.Id,b.ManpowerBudgetId,TotalManpower,TotalNumber,CName " + cListextM + @"
                                 ,Designation,EmployeeCategory, m.DesGName";
                return _sqlRepository.GetDataCollection(sqlText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal for BudgetDetail List

        #region Modal for ExcessSummary List

        public IEnumerable<object> ModalExcessSummary(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var EmployeeCategory = string.Empty;
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeCategory = "";
            }
            else
            {
                EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            }
            var cList = string.Empty;
            var wc = string.Empty;
            var join = string.Empty;
            var cListId = string.Empty;
            var cn = string.Empty;
            var wcExt = string.Empty;
            var cListext = string.Empty;
            var cListextId = string.Empty;
            var cListEmp = string.Empty;
            var cListFinish = string.Empty;
            var cListOId = string.Empty;
            var cListextM = string.Empty;
            var cListextIdM = string.Empty;
            var cListextF = string.Empty;
            var cListextIdF = string.Empty;
            var cListEmpG = string.Empty;
            var DStatus = string.Empty;
            if (status == "Default")
            {
                DStatus = "";
            }
            else if (status == "Direct")
            {
                DStatus = "and PO.IsDirect = 1";
            }
            else if (status == "Indirect")
            {
                DStatus = "and PO.IsDirect = 0";
            }
            try
            {
                var sqlText = "";
                if (seq == -2)
                {
                    var cListBuilder = new System.Text.StringBuilder();
                    cListBuilder.Append(cList);
                    var cListextBuilder = new System.Text.StringBuilder();
                    cListextBuilder.Append(cListext);
                    var cListextMBuilder = new System.Text.StringBuilder();
                    cListextMBuilder.Append(cListextM);
                    var joinBuilder = new System.Text.StringBuilder();
                    joinBuilder.Append(join);
                    var whereClauseBuilder = new System.Text.StringBuilder();
                    whereClauseBuilder.Append(wc);
                    var wcExtBuilder = new System.Text.StringBuilder();
                    wcExtBuilder.Append(wcExt);
                    var cListextIdBuilder = new System.Text.StringBuilder();
                    cListextIdBuilder.Append(cListextId);
                    var cListEmpGBuilder = new System.Text.StringBuilder();
                    cListEmpGBuilder.Append(cListEmpG);
                    var cListEmpBuilder = new System.Text.StringBuilder();
                    cListEmpBuilder.Append(cListEmp);
                    var cListextIdMBuilder = new System.Text.StringBuilder();
                    cListextIdMBuilder.Append(cListextIdM);
                    var cListFinishBuilder = new System.Text.StringBuilder();
                    cListFinishBuilder.Append(cListFinish);

                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(", m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")
                            {
                                cListBuilder.Append("," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ");
                                cListextBuilder.Append("," + item.StandardName + "Defination.UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n");
                            }
                            if (item.RType == "ZA")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n");
                            }

                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                whereClauseBuilder.Append(" and c.id='" + item.Id + "'");
                            }
                            else
                            {
                                if (item.Sequence < seq)
                                {
                                    if (item.RType == "ZA")
                                    {
                                        whereClauseBuilder.Append(" AND " + item.StandardName + "Defination.Id='" + item.Text + "'");
                                        wcExtBuilder.Append(" AND " + item.StandardName + "DefinationId='" + item.Text + "'");
                                        cListextIdBuilder.Append("," + item.StandardName + "Defination.Id  " + item.StandardName + "DefinationId");
                                        cListEmpGBuilder.Append("," + item.StandardName + "Defination.Id  ");
                                        cListEmpBuilder.Append(" AND E." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");
                                        cListextIdMBuilder.Append(",M." + item.StandardName + "DefinationId");
                                        cListextF = "," + item.StandardName + "DefinationName";
                                        cListextIdF = "," + item.StandardName + "DefinationId";
                                        cListFinishBuilder.Append(" and B." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");
                                    }
                                    else
                                    {
                                        whereClauseBuilder.Append(" and " + item.StandardName + ".Id='" + item.Text + "'");
                                        wcExtBuilder.Append(" and " + item.StandardName + "Id='" + item.Text + "'");
                                        cListextIdBuilder.Append("," + item.StandardName + ".Id  " + item.StandardName + "Id");
                                        cListEmpGBuilder.Append("," + item.StandardName + ".Id  ");
                                        cListEmpBuilder.Append(" and e." + item.StandardName + "Id = m." + item.StandardName + "Id");
                                        cListextIdMBuilder.Append(",m." + item.StandardName + "Id");
                                        cListextF = "," + item.StandardName + "Name";
                                        cListextIdF = "," + item.StandardName + "Id";
                                        cListFinishBuilder.Append(" and B." + item.StandardName + "Id = m." + item.StandardName + "Id");
                                    }

                                }
                            }
                        }
                    }
                    cListFinish = cListFinishBuilder.ToString();
                    cListextIdM = cListextIdMBuilder.ToString();
                    cListEmp = cListEmpBuilder.ToString();
                    cListEmpG = cListEmpGBuilder.ToString();
                    cListextId = cListextIdBuilder.ToString();
                    wcExt = wcExtBuilder.ToString();
                    wc = whereClauseBuilder.ToString();
                    join = joinBuilder.ToString();
                    cListextM = cListextMBuilder.ToString();
                    cListext = cListextBuilder.ToString();
                    cList = cListBuilder.ToString();

                    sqlText = @"select m.Id MbId,Code BudgetCode
                                ,Excess = CASE WHEN isNull(TotalManpower,0) - isNull(TotalNumber,0) > 0
                                          THEN isNull(TotalManpower,0) - isNull(TotalNumber,0)  ELSE 0 end
                                ,isnull(e.TotalManpower,0) as onRole
                                ,isnull(b.TotalNumber,0) as Proposed
								,e.TotalSalary/e.TotalManpower as avgSalary
								,isnull(sal.MaximumSalary,0)+isnull(sal.MinimumSalary,0)/e.TotalManpower ProposedSalary
                                ,m.Code budgetCodeE
                                ,m.GroupName
                                ,m.CompanyId
                                ,m.CName as CompanyName
                                " + cListextM + @"
                                ,m.Designation
                                ,m.DesGName
                                ,m.EmployeeCategory
                                FROM
                                ---------------------------1 bc--------------------------------------
                                (SELECT MB.Code, MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName " + cListext + @"
                                 ,Des.UserName Designation,EmpC.UserName EmployeeCategory, DesG.UserName  DesGName
                                 FROM [MST].[ManpowerBudget]  MB
                                      LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                      LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                          LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
                                      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
                                      LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId
                                      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                     " + join + @"
                                     WHERE Cg.Id = '" + companyGroupId + @"' " + DStatus + @" " + EmployeeCategory + @" AND MB.Active = 1
                                    )  m
                                    -----------------------2e--------------------------------
                                    LEFT OUTER JOIN
                                     (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID,c.Id  cid,sum(TotalSalary) TotalSalary
                                       FROM [dbo].[EmployeeInformation]  em
                                      LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                            LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                                  LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                                      LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      " + join + @"
                                       WHERE EmployeeStatus = 'Active'  " + DStatus + EmployeeCategory + @"
                                        AND em.GroupID = '" + companyGroupId + @"'
                                        GROUP BY BudgetCode,em.GroupID,c.Id
                                     ) e ON m.Id=e.BudgetCode AND e.GroupID = m.CgId AND e.cid = m.CompanyId
                                    ----------------------Budgeted Salary------------------------------------------
								LEFT OUTER JOIN
								(

								    SELECT MBA.ManpowerBudgetId,
									MinimumSalary = sum(case when MBA.EffectiveDate <= '" + date + @"'  then  isnull(MinimumSalary,0) else 0 end),
									MaximumSalary = sum(case when MBA.EffectiveDate <= '" + date + @"'  then  isnull(MaximumSalary,0) else 0 end)
									,ED.EffectiveDate,m.CompanyId
									FROM [MST].[ManpowerBudgetAllowance] MBA

								    LEFT OUTER JOIN [MST].[ManpowerBudget] AS m on m.Id = MBA.ManpowerBudgetId
									LEFT OUTER JOIN [MST].[ManpowerBudgetDetail] AS MBD ON MBD.ManpowerBudgetId = MBA.ManpowerBudgetId
                                      LEFT OUTER JOIN [ORG].[Position] AS PO on PO.Id = m.PositionId
									LEFT OUTER JOIN (
									SELECT MAX(EffectiveDate) EffectiveDate,ManpowerBudgetId,CompanyId from [MST].[ManpowerBudgetAllowance]
									 LEFT OUTER JOIN [MST].[ManpowerBudget] AS m on m.Id = ManpowerBudgetId
									 WHERE EffectiveDate = (SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetAllowance] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
								     GROUP BY ManpowerBudgetId ,CompanyId
									)  ED ON ED.ManpowerBudgetId = MBA.ManpowerBudgetId AND ED.EffectiveDate = MBA.EffectiveDate
									 WHERE
									 ED.EffectiveDate IS NOT NULL AND m.CompanyGroupId =  '" + companyGroupId + @"'  " + DStatus + @"
									AND MBD.EffectiveDate = (SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetDetail] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
									GROUP BY MBA.ManpowerBudgetId,ED.EffectiveDate,m.CompanyId
									) Sal ON m.Id = Sal.ManpowerBudgetId AND m.CompanyId = Sal.CompanyId
                                     -------------------------3b--------------------------------------------------------
                                      LEFT OUTER JOIN
                                     (
                                       SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id as cid from
                                        (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                     ) MBD
                                      LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId
                                      LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                      LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                    LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                      " + join + @"
                                     where CG.Id = '" + companyGroupId + @"'  " + DStatus + EmployeeCategory + @" 
                                     ) B
                                     ON m.id = b.ManpowerBudgetId and b.Id = m.CgId and B.cid = m.CompanyId
                                    	WHERE ISNULL(TotalManpower,0) > ISNULL(TotalNumber,0)
                                     GROUP BY m.Code, GroupName,m.CompanyId,m.Id,b.ManpowerBudgetId,TotalManpower,TotalNumber,CName " + cListextM + @"
                                     ,e.TotalSalary,sal.MaximumSalary,sal.MinimumSalary,m.Designation,m.EmployeeCategory,m.DesGName";

                    return _sqlRepository.GetDataCollection(sqlText);
                }
                else
                {
                    seq += 1;

                    var cListBuilder = new System.Text.StringBuilder();
                    cListBuilder.Append(cList);
                    var cListextBuilder = new System.Text.StringBuilder();
                    cListextBuilder.Append(cListext);
                    var cListextMBuilder = new System.Text.StringBuilder();
                    cListextMBuilder.Append(cListextM);
                    var joinBuilder = new System.Text.StringBuilder();
                    joinBuilder.Append(join);
                    var whereClauseBuilder = new System.Text.StringBuilder();
                    whereClauseBuilder.Append(wc);
                    var wcExtBuilder = new System.Text.StringBuilder();
                    wcExtBuilder.Append(wcExt);
                    var cListextIdBuilder = new System.Text.StringBuilder();
                    cListextIdBuilder.Append(cListextId);
                    var cListEmpGBuilder = new System.Text.StringBuilder();
                    cListEmpGBuilder.Append(cListEmpG);
                    var cListEmpBuilder = new System.Text.StringBuilder();
                    cListEmpBuilder.Append(cListEmp);
                    var cListextIdMBuilder = new System.Text.StringBuilder();
                    cListextIdMBuilder.Append(cListextIdM);
                    var cListFinishBuilder = new System.Text.StringBuilder();
                    cListFinishBuilder.Append(cListFinish);
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")
                            {
                                cListBuilder.Append("," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ");
                                cListextBuilder.Append("," + item.StandardName + "Defination.UserName  " + item.StandardName + "Defination");
                                cListextMBuilder.Append(",m." + item.StandardName + "Defination");
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n");
                            }
                            if (item.RType == "ZA")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n");
                            }

                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                whereClauseBuilder.Append(" and c.id='" + item.Id + "'");
                            }
                            else
                            {
                                if (item.Sequence < seq)
                                {
                                    if (item.RType == "Z")
                                    {
                                        whereClauseBuilder.Append(" and " + item.StandardName + ".Id='" + item.Text + "'");
                                        wcExtBuilder.Append(" and " + item.StandardName + "Id='" + item.Text + "'");
                                        cListextIdBuilder.Append("," + item.StandardName + ".Id  " + item.StandardName + "Id");
                                        cListEmpGBuilder.Append("," + item.StandardName + ".Id  ");
                                        cListEmpBuilder.Append(" and e." + item.StandardName + "Id = m." + item.StandardName + "Id");
                                        cListextIdMBuilder.Append(",m." + item.StandardName + "Id");
                                        cListextF = "," + item.StandardName + "Name";
                                        cListextIdF = "," + item.StandardName + "Id";
                                        cListFinishBuilder.Append(" and B." + item.StandardName + "Id = m." + item.StandardName + "Id");
                                    }
                                    else
                                    {
                                        whereClauseBuilder.Append(" and " + item.StandardName + "Defination.Id='" + item.Text + "'");
                                        wcExtBuilder.Append(" and " + item.StandardName + "DefinationId='" + item.Text + "'");
                                        cListextIdBuilder.Append("," + item.StandardName + "Defination.Id  " + item.StandardName + "Id");
                                        cListEmpGBuilder.Append("," + item.StandardName + "Defination.Id  ");
                                        cListEmpBuilder.Append(" and e." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");
                                        cListextIdMBuilder.Append(",m." + item.StandardName + "DefinationId");
                                        cListextF = "," + item.StandardName + "DefinationName";
                                        cListextIdF = "," + item.StandardName + "DefinationId";
                                        cListFinishBuilder.Append(" and B." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");
                                    }
                                }
                            }
                        }
                    }
                    cListFinish = cListFinishBuilder.ToString();
                    cListextIdM = cListextIdMBuilder.ToString();
                    cListEmp = cListEmpBuilder.ToString();
                    cListEmpG = cListEmpGBuilder.ToString();
                    cListextId = cListextIdBuilder.ToString();
                    wcExt = wcExtBuilder.ToString();
                    wc = whereClauseBuilder.ToString();
                    join = joinBuilder.ToString();
                    cListextM = cListextMBuilder.ToString();
                    cListext = cListextBuilder.ToString();
                    cList = cListBuilder.ToString();

                    sqlText = @"select m.Id MbId,Code BudgetCode
                                ,Excess = CASE WHEN isNull(TotalManpower,0) - isNull(TotalNumber,0) > 0
                                           THEN isNull(TotalManpower,0) - isNull(TotalNumber,0) ELSE 0 end
                                ,isnull(e.TotalManpower,0) as onRole
                                ,isnull(b.TotalNumber,0) as Proposed
                                ,e.TotalSalary/e.TotalManpower as avgSalary
								,isnull(sal.MaximumSalary,0)+isnull(sal.MinimumSalary,0)/e.TotalManpower ProposedSalary
                                ,m.Code budgetCodeE
                                ,m.GroupName
                                ,m.CompanyId
                                ,m.CName as CompanyName
                                " + cListextM + @"
                                ,m.Designation
                                ,m.DesGName
                                ,m.EmployeeCategory
                                 from
                                ----------------------------1 bc--------------------------------------
                                (select MB.Code, MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName " + cListext + @"
                                  ,Des.UserName Designation,EmpC.UserName EmployeeCategory,DesG.UserName DesGName
                                      from [MST].[ManpowerBudget]  MB
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
                                      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
                                      LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId

                                      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                     " + join + @"
                                     where Cg.Id = '" + companyGroupId + @"'  " + wc + @"  " + DStatus + @" AND MB.Active = 1
                                    )  m
                                    -----------------------2e--------------------------------
                                    left outer join
                                     (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID,c.Id  cid, sum(TotalSalary) TotalSalary
                                       FROM [dbo].[EmployeeInformation]  em
                                      LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                             LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                                  LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                     " + join + @"

                                       WHERE EmployeeStatus = 'Active'  " + DStatus + @"
                                        AND em.GroupID = '" + companyGroupId + @"'
                                        group by BudgetCode,em.GroupID,c.Id
                                       ) e on m.Id=e.BudgetCode and e.GroupID = m.CgId and e.cid = m.CompanyId
                                         ----------------------Budgeted Salary------------------------------------------
								LEFT OUTER JOIN
								(
								    SELECT MBA.ManpowerBudgetId,
									MinimumSalary = sum(case when MBA.EffectiveDate <= '" + date + @"'  then  isnull(MinimumSalary,0) else 0 end),
									MaximumSalary = sum(case when MBA.EffectiveDate <= '" + date + @"'  then  isnull(MaximumSalary,0) else 0 end)
									,ED.EffectiveDate,m.CompanyId
									FROM [MST].[ManpowerBudgetAllowance] MBA
								    LEFT OUTER JOIN [MST].[ManpowerBudget] AS m on m.Id = MBA.ManpowerBudgetId
									LEFT OUTER JOIN [MST].[ManpowerBudgetDetail] AS MBD ON MBD.ManpowerBudgetId = MBA.ManpowerBudgetId
                                    LEFT OUTER JOIN [ORG].[Position] AS PO on PO.Id = m.PositionId
									LEFT OUTER JOIN (
									Select MAX(EffectiveDate) EffectiveDate,ManpowerBudgetId,CompanyId from [MST].[ManpowerBudgetAllowance]
									 LEFT OUTER JOIN [MST].[ManpowerBudget] AS m on m.Id = ManpowerBudgetId
									 WHERE EffectiveDate=(SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetAllowance] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
									 GROUP BY ManpowerBudgetId ,CompanyId
									)  ED ON ED.ManpowerBudgetId = MBA.ManpowerBudgetId AND ED.EffectiveDate = MBA.EffectiveDate
									 WHERE
									 ED.EffectiveDate IS NOT NULL AND m.CompanyGroupId =  '" + companyGroupId + @"'  " + DStatus + @"
									AND MBD.EffectiveDate = (SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetDetail] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
									group by MBA.ManpowerBudgetId,ED.EffectiveDate,m.CompanyId
									) Sal on m.Id = Sal.ManpowerBudgetId and m.CompanyId = Sal.CompanyId
                                     -------------------------3b--------------------------------------------------------
                                      left outer join
                                      (
                                         SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id AS cid from

                                            (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                        ) MBD
                                      left outer join [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      " + join + @"
                                      where CG.Id = '" + companyGroupId + @"'  " + wc + @"  " + DStatus + @"
                                     ) B
                                     on m.id = b.ManpowerBudgetId and b.Id = m.CgId and B.cid = m.CompanyId
                                  	 where isnull(TotalManpower,0) > isnull(TotalNumber,0)
                                     group by m.Code,GroupName,m.CompanyId,m.Id,b.ManpowerBudgetId,TotalManpower,e.TotalSalary,TotalNumber,CName " + cListextM + @"
                                     ,sal.MaximumSalary,sal.MinimumSalary,m.Designation,m.EmployeeCategory,m.DesGName";

                    return _sqlRepository.GetDataCollection(sqlText);

                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal for ExcessSummary List

        #region Modal for ExcessDetail List

        public IEnumerable<object> ModalExcessDetail(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, int seq, string date, string status, GridParameter parameters)
        {
            var cList = string.Empty;
            var wc = string.Empty;
            var Join = string.Empty;
            var cListId = string.Empty;
            var cn = string.Empty;
            var wcExt = string.Empty;
            var cListext = string.Empty;
            var cListextId = string.Empty;
            var cListEmp = string.Empty;
            var cListFinish = string.Empty;
            var cListOId = string.Empty;
            var cListextM = string.Empty;
            var cListextIdM = string.Empty;
            var cListextF = string.Empty;
            var cListextIdF = string.Empty;
            var cListEmpG = string.Empty;
            var DStatus = string.Empty;

            if (status == "Default")
            {
                DStatus = "";
            }
            else if (status == "Direct")
            {
                DStatus = "and PO.IsDirect = 1";
            }
            else if (status == "Indirect")
            {
                DStatus = "and PO.IsDirect = 0";
            }
            try
            {
                var sqlText = "";
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                            cListextM += ",m." + item.StandardName;

                            if (item.StandardName == "EmployeeGroup")
                            {
                                Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        if (item.RType == "Position")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                            cListextM += ",m." + item.StandardName;
                            Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                        }
                        if (item.RType == "Z")
                        {
                            cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                            cListext += "," + item.StandardName + "Defination.UserName  " + item.StandardName + "Defination";
                            cListextM += ",m." + item.StandardName + "Defination";
                            Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                        }
                        if (item.RType == "ZA")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                            cListextM += ",m." + item.StandardName;
                            Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n";
                        }
                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = " and c.id='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    wc += " AND ISNULL(" + item.StandardName + "Defination.SystemId,'')='" + item.Text + "'";

                                    wcExt += " AND ISNULL(" + item.StandardName + "DefinationId,'')='" + item.Text + "'";

                                    cListextId += ",ISNULL(" + item.StandardName + "Defination.SystemId,'')  " + item.StandardName + "DefinationId";
                                    cListEmpG += "," + item.StandardName + ".SystemId  ";
                                    cListEmp += " and e." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId";

                                    cListextIdM += ",m." + item.StandardName + "DefinationId";
                                    cListextF = "," + item.StandardName + "DefinationName";
                                    cListextIdF = "," + item.StandardName + "DefinationId";
                                    cListFinish += " and B." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId";
                                }
                                else
                                {
                                    wc += " and " + item.StandardName + ".Id='" + item.Text + "'";

                                    wcExt += " and " + item.StandardName + "Id='" + item.Text + "'";

                                    cListextId += "," + item.StandardName + ".Id  " + item.StandardName + "Id";
                                    cListEmpG += "," + item.StandardName + ".Id  ";
                                    cListEmp += " and e." + item.StandardName + "Id = m." + item.StandardName + "Id";

                                    cListextIdM += ",m." + item.StandardName + "Id";
                                    cListextF = "," + item.StandardName + "Name";
                                    cListextIdF = "," + item.StandardName + "Id";
                                    cListFinish += " and B." + item.StandardName + "Id = m." + item.StandardName + "Id";
                                }
                            }
                        }
                    }
                }
                sqlText = @"SELECT m.Id MbId,Code BudgetCode
                                ,Excess = CASE
                                  WHEN isNull(TotalManpower,0) - isNull(TotalNumber,0) > 0
                                  THEN isNull(TotalManpower,0) - isNull(TotalNumber,0)
                                  ELSE 0 end
                                ,ISNULL(e.TotalManpower,0) AS onRole
                                ,ISNULL(b.TotalNumber,0) AS Proposed
                                ,e.TotalSalary/e.TotalManpower AS avgSalary
                                ,isnull(sal.MaximumSalary,0)+isnull(sal.MinimumSalary,0)/e.TotalManpower ProposedSalary
                                ,m.Code budgetCodeE
                                ,m.GroupName
                                ,m.CompanyId
                                ,m.CName AS CompanyName
                                " + cListextM + @"
                                ,m.Designation
                                ,m.DesGName
                                ,m.EmployeeCategory
                                FROM
                          --------------------------------------1 bc------------------------------------------
                                (select MB.Code, MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName " + cListext + @"
                                 ,Des.UserName Designation,EmpC.UserName EmployeeCategory,DesG.UserName DesGName
                                  from [MST].[ManpowerBudget]  MB
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                          LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
                                      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
                                      LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId

                                      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                      " + Join + @" 
                                      where Cg.Id = '" + companyGroupId + @"' " + wc + @" " + DStatus + @" AND MB.Active = 1
                                )  m
                                    -----------------------2e--------------------------------
                                LEFT OUTER JOIN
                                (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID,c.Id  cid,sum(TotalSalary) TotalSalary
                                      FROM [dbo].[EmployeeInformation]  em
                                      LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                          LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                                  LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                     " + Join + @"
                                      WHERE EmployeeStatus = 'Active'
                                        AND em.GroupID = '" + companyGroupId + @"' " + DStatus + @"
                                        group by BudgetCode,em.GroupID,c.Id
                                ) e ON m.Id=e.BudgetCode and e.GroupID = m.CgId and e.cid = m.CompanyId
                        ----------------------Budgeted Salary------------------------------------------
								LEFT OUTER JOIN
								(
								    SELECT MBA.ManpowerBudgetId,
									MinimumSalary = SUM(CASE WHEN MBA.EffectiveDate <= '" + date + @"'  THEN  ISNULL(MinimumSalary,0) else 0 end),
									MaximumSalary = SUM(CASE WHEN MBA.EffectiveDate <= '" + date + @"'  THEN  ISNULL(MaximumSalary,0) else 0 end)
									,ED.EffectiveDate,m.CompanyId
									FROM [MST].[ManpowerBudgetAllowance] MBA

								    LEFT OUTER JOIN [MST].[ManpowerBudget] AS m on m.Id = MBA.ManpowerBudgetId
									LEFT OUTER JOIN [MST].[ManpowerBudgetDetail] AS MBD ON MBD.ManpowerBudgetId = MBA.ManpowerBudgetId
                                      LEFT OUTER JOIN [ORG].[Position] AS PO on PO.Id = m.PositionId
									LEFT OUTER JOIN (
									SELECT MAX(EffectiveDate) EffectiveDate,ManpowerBudgetId,CompanyId from [MST].[ManpowerBudgetAllowance]
									 LEFT OUTER JOIN [MST].[ManpowerBudget] AS m on m.Id = ManpowerBudgetId
									 WHERE  EffectiveDate=(SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetAllowance] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
									 GROUP BY ManpowerBudgetId ,CompanyId
									)  ED ON ED.ManpowerBudgetId = MBA.ManpowerBudgetId AND ED.EffectiveDate = MBA.EffectiveDate
									 WHERE
									 ED.EffectiveDate IS NOT NULL AND m.CompanyGroupId =  '" + companyGroupId + @"'  " + DStatus + @"
										AND MBD.EffectiveDate = (SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetDetail] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
									GROUP BY MBA.ManpowerBudgetId,ED.EffectiveDate,m.CompanyId
									) Sal ON m.Id = Sal.ManpowerBudgetId AND m.CompanyId = Sal.CompanyId
                                     -------------------------3b--------------------------------------------------------
                                LEFT OUTER JOIN
                                (
                                 SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id as cid from
                                 (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                 ) MBD
                                 LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId
                                 LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                 LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                 LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                 LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                 " + Join + @"
                                 where CG.Id = '" + companyGroupId + @"'  " + wc + @"  " + DStatus + @"
                                 ) B
                                 on m.id = b.ManpowerBudgetId and b.Id = m.CgId and B.cid = m.CompanyId
             	                 where isnull(TotalManpower,0) > isnull(TotalNumber,0)
                                 group by m.Code,GroupName,m.CompanyId,m.Id,b.ManpowerBudgetId,TotalSalary,TotalManpower,e.TotalSalary,TotalNumber,CName " + cListextM + @"
                                 ,m.Designation,sal.MaximumSalary,sal.MinimumSalary,m.EmployeeCategory,m.DesGName";

                return _sqlRepository.GetDataCollection(sqlText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal for ExcessDetail List

        #region Modal for ShortSummary List

        public IEnumerable<Object> ModalShortSummary(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, int seq, string date, string status, GridParameter parameters)
        {
            var cList = string.Empty;
            var wc = string.Empty;
            var join = string.Empty;
            var cListId = string.Empty;
            var cn = string.Empty;
            var wcExt = string.Empty;
            var cListext = string.Empty;
            var cListextId = string.Empty;
            var cListEmp = string.Empty;
            var cListFinish = string.Empty;
            var cListOId = string.Empty;
            var cListextM = string.Empty;
            var cListextIdM = string.Empty;
            var cListextF = string.Empty;
            var cListextIdF = string.Empty;
            var cListEmpG = string.Empty;
            var DStatus = string.Empty;

            if (status == "Default")
            {
                DStatus = "";
            }
            else if (status == "Direct")
            {
                DStatus = "and PO.IsDirect = 1";
            }
            else if (status == "Indirect")
            {
                DStatus = "and PO.IsDirect = 0";
            }
            try
            {
                var sqlText = "";
                if (seq == -2)
                {
                    var cListBuilder = new System.Text.StringBuilder();
                    cListBuilder.Append(cList);
                    var cListextBuilder = new System.Text.StringBuilder();
                    cListextBuilder.Append(cListext);
                    var cListextMBuilder = new System.Text.StringBuilder();
                    cListextMBuilder.Append(cListextM);
                    var joinBuilder = new System.Text.StringBuilder();
                    joinBuilder.Append(join);
                    var whereClauseBuilder = new System.Text.StringBuilder();
                    whereClauseBuilder.Append(wc);
                    var wcExtBuilder = new System.Text.StringBuilder();
                    wcExtBuilder.Append(wcExt);
                    var cListextIdBuilder = new System.Text.StringBuilder();
                    cListextIdBuilder.Append(cListextId);
                    var cListEmpGBuilder = new System.Text.StringBuilder();
                    cListEmpGBuilder.Append(cListEmpG);
                    var cListEmpBuilder = new System.Text.StringBuilder();
                    cListEmpBuilder.Append(cListEmp);
                    var cListextIdMBuilder = new System.Text.StringBuilder();
                    cListextIdMBuilder.Append(cListextIdM);
                    var cListFinishBuilder = new System.Text.StringBuilder();
                    cListFinishBuilder.Append(cListFinish);

                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(", m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")
                            {
                                cListBuilder.Append("," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ");
                                cListextBuilder.Append("," + item.StandardName + "Defination.UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n");
                            }
                            if (item.RType == "ZA")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n");
                            }

                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                whereClauseBuilder.Append(" and c.id='" + item.Id + "'");
                            }
                            else
                            {
                                if (item.Sequence < seq)
                                {
                                    if (item.RType == "ZA")
                                    {
                                        whereClauseBuilder.Append(" AND " + item.StandardName + "Defination.Id='" + item.Text + "'");
                                        wcExtBuilder.Append(" AND " + item.StandardName + "DefinationId='" + item.Text + "'");
                                        cListextIdBuilder.Append("," + item.StandardName + "Defination.Id  " + item.StandardName + "DefinationId");
                                        cListEmpGBuilder.Append("," + item.StandardName + "Defination.Id  ");
                                        cListEmpBuilder.Append(" AND E." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");
                                        cListextIdMBuilder.Append(",M." + item.StandardName + "DefinationId");
                                        cListextF = "," + item.StandardName + "DefinationName";
                                        cListextIdF = "," + item.StandardName + "DefinationId";
                                        cListFinishBuilder.Append(" and B." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");
                                    }
                                    else
                                    {
                                        whereClauseBuilder.Append(" and " + item.StandardName + ".Id='" + item.Text + "'");
                                        wcExtBuilder.Append(" and " + item.StandardName + "Id='" + item.Text + "'");
                                        cListextIdBuilder.Append("," + item.StandardName + ".Id  " + item.StandardName + "Id");
                                        cListEmpGBuilder.Append("," + item.StandardName + ".Id  ");
                                        cListEmpBuilder.Append(" and e." + item.StandardName + "Id = m." + item.StandardName + "Id");
                                        cListextIdMBuilder.Append(",m." + item.StandardName + "Id");
                                        cListextF = "," + item.StandardName + "Name";
                                        cListextIdF = "," + item.StandardName + "Id";
                                        cListFinishBuilder.Append(" and B." + item.StandardName + "Id = m." + item.StandardName + "Id");
                                    }

                                }
                            }
                        }
                    }
                    cListFinish = cListFinishBuilder.ToString();
                    cListextIdM = cListextIdMBuilder.ToString();
                    cListEmp = cListEmpBuilder.ToString();
                    cListEmpG = cListEmpGBuilder.ToString();
                    cListextId = cListextIdBuilder.ToString();
                    wcExt = wcExtBuilder.ToString();
                    wc = whereClauseBuilder.ToString();
                    join = joinBuilder.ToString();
                    cListextM = cListextMBuilder.ToString();
                    cListext = cListextBuilder.ToString();
                    cList = cListBuilder.ToString();

                    sqlText = @"select m.Id,b.ManpowerBudgetId MbId,Code BudgetCode
                                ,Short = CASE WHEN isNull(TotalNumber,0) - isNull(TotalManpower,0) > 0
                                      THEN isNull(TotalNumber,0) - isNull(TotalManpower,0) end
                                ,isnull(e.TotalManpower,0) as onRole
                                ,isnull(b.TotalNumber,0) as Proposed
                                ,m.Code budgetCodeE
                                ,e.TotalSalary/e.TotalManpower as avgSalary
                                ,m.GroupName
                                ,m.CompanyId
                                ,m.CName as CompanyName
                                " + cListextM + @"
                                ,m.Designation
                                ,m.DesGName
                                ,m.EmployeeCategory
                                from
                                ---------------------------1 bc--------------------------------------
                                (select  MB.Code,MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName " + cListext + @"
                                  ,Des.UserName Designation,EmpC.UserName EmployeeCategory,DesG.UserName DesGName
                                 from [MST].[ManpowerBudget]  MB
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                          LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
                                      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
                                      LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId

                                      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                     " + join + @"
                                     where Cg.Id = '" + companyGroupId + @"' " + DStatus + @" AND MB.Active = 1
                                    )  m
                                    -----------------------2e--------------------------------
                                    left outer join
                                     (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID,sum(TotalSalary) TotalSalary
                                       FROM [dbo].[EmployeeInformation]  em
                                      LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                          LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                                  LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      " + join + @"
                                       WHERE EmployeeStatus = 'Active'
                                        AND em.GroupID = '" + companyGroupId + @"'  " + DStatus + @"
                                        group by BudgetCode,em.GroupID
                                     ) e on m.Id = e.BudgetCode and e.GroupID = m.CgId
                                     -------------------------3b--------------------------------------------------------
                                      LEFT OUTER JOIN
                                     (
                                       SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id as cid from
                                        (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                     ) MBD
                                      LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  ON  Mb.Id = MBD.ManpowerBudgetId
                                      LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id AND mb.CompanyId= c.Id
                                      LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      " + join + @"
                                     WHERE CG.Id = '" + companyGroupId + @"'  " + DStatus + @"
                                     ) B
                                     ON m.id = b.ManpowerBudgetId AND b.Id = m.CgId AND B.cid = m.CompanyId
                                    		WHERE ISNULL(TotalNumber,0) > ISNULL(TotalManpower,0)
                                     GROUP BY m.Code,GroupName,CompanyId,m.Id,b.ManpowerBudgetId,TotalManpower,e.TotalSalary,TotalNumber,CName " + cListextM + @"
                                     ,m.Designation,m.EmployeeCategory,m.DesGName";

                    return _sqlRepository.GetDataCollection(sqlText);
                }
                else
                {
                    seq += 1;

                    var cListBuilder = new System.Text.StringBuilder();
                    cListBuilder.Append(cList);
                    var cListextBuilder = new System.Text.StringBuilder();
                    cListextBuilder.Append(cListext);
                    var cListextMBuilder = new System.Text.StringBuilder();
                    cListextMBuilder.Append(cListextM);
                    var joinBuilder = new System.Text.StringBuilder();
                    joinBuilder.Append(join);
                    var whereClauseBuilder = new System.Text.StringBuilder();
                    whereClauseBuilder.Append(wc);
                    var wcExtBuilder = new System.Text.StringBuilder();
                    wcExtBuilder.Append(wcExt);
                    var cListextIdBuilder = new System.Text.StringBuilder();
                    cListextIdBuilder.Append(cListextId);
                    var cListEmpGBuilder = new System.Text.StringBuilder();
                    cListEmpGBuilder.Append(cListEmpG);
                    var cListEmpBuilder = new System.Text.StringBuilder();
                    cListEmpBuilder.Append(cListEmp);
                    var cListextIdMBuilder = new System.Text.StringBuilder();
                    cListextIdMBuilder.Append(cListextIdM);
                    var cListFinishBuilder = new System.Text.StringBuilder();
                    cListFinishBuilder.Append(cListFinish);
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")
                            {
                                cListBuilder.Append("," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ");
                                cListextBuilder.Append("," + item.StandardName + "Defination.UserName  " + item.StandardName + "Defination");
                                cListextMBuilder.Append(",m." + item.StandardName + "Defination");
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n");
                            }
                            if (item.RType == "ZA")
                            {
                                cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                                cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                                cListextMBuilder.Append(",m." + item.StandardName);
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n");
                            }

                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                whereClauseBuilder.Append(" and c.id='" + item.Id + "'");
                            }
                            else
                            {
                                if (item.Sequence < seq)
                                {
                                    if (item.RType == "Z")
                                    {
                                        whereClauseBuilder.Append(" and " + item.StandardName + "Defination.Id='" + item.Text + "'");
                                        wcExtBuilder.Append(" and " + item.StandardName + "DefinationId='" + item.Text + "'");
                                        cListextIdBuilder.Append("," + item.StandardName + "Defination.Id  " + item.StandardName + "Id");
                                        cListEmpGBuilder.Append("," + item.StandardName + "Defination.Id  ");
                                        cListEmpBuilder.Append(" and e." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");
                                        cListextIdMBuilder.Append(",m." + item.StandardName + "DefinationId");
                                        cListextF = "," + item.StandardName + "DefinationName";
                                        cListextIdF = "," + item.StandardName + "DefinationId";
                                        cListFinishBuilder.Append(" and B." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");
                                    }
                                    else
                                    {
                                        whereClauseBuilder.Append(" and " + item.StandardName + ".Id='" + item.Text + "'");
                                        wcExtBuilder.Append(" and " + item.StandardName + "Id='" + item.Text + "'");
                                        cListextIdBuilder.Append("," + item.StandardName + ".Id  " + item.StandardName + "Id");
                                        cListEmpGBuilder.Append("," + item.StandardName + ".Id  ");
                                        cListEmpBuilder.Append(" and e." + item.StandardName + "Id = m." + item.StandardName + "Id");
                                        cListextIdMBuilder.Append(",m." + item.StandardName + "Id");
                                        cListextF = "," + item.StandardName + "Name";
                                        cListextIdF = "," + item.StandardName + "Id";
                                        cListFinishBuilder.Append(" and B." + item.StandardName + "Id = m." + item.StandardName + "Id");

                                    }
                                }
                            }
                        }
                    }
                    cListFinish = cListFinishBuilder.ToString();
                    cListextIdM = cListextIdMBuilder.ToString();
                    cListEmp = cListEmpBuilder.ToString();
                    cListEmpG = cListEmpGBuilder.ToString();
                    cListextId = cListextIdBuilder.ToString();
                    wcExt = wcExtBuilder.ToString();
                    wc = whereClauseBuilder.ToString();
                    join = joinBuilder.ToString();
                    cListextM = cListextMBuilder.ToString();
                    cListext = cListextBuilder.ToString();
                    cList = cListBuilder.ToString();

                    sqlText = @"select m.Id,b.ManpowerBudgetId  MbId,Code BudgetCode
                                ,Short = CASE WHEN isNull(TotalNumber,0) - isNull(TotalManpower,0) > 0
                                      THEN isNull(TotalNumber,0) - isNull(TotalManpower,0) ELSE 0 end
                                ,isnull(e.TotalManpower,0) as onRole
                                ,isnull(b.TotalNumber,0) as Proposed
                                ,e.TotalSalary/e.TotalManpower as avgSalary
                                ,m.Code budgetCodeE
                                ,m.GroupName
                                ,m.CompanyId
                                ,m.CName as CompanyName
                                " + cListextM + @"
                                ,m.Designation
                                ,m.DesGName
                                ,m.EmployeeCategory
                                 from
                                ----------------------------1 bc--------------------------------------
                                (select  MB.Code,MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName " + cListext + @"
                                 ,Des.UserName Designation,EmpC.UserName EmployeeCategory,DesG.UserName DesGName
                                      from [MST].[ManpowerBudget]  MB
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                        LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
                                      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
                                      LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId

                                      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                     " + join + @"
                                     where Cg.Id = '" + companyGroupId + @"' " + wc + @"  " + DStatus + @" AND MB.Active = 1
                                    )  m
                                    -----------------------2e--------------------------------
                                    left outer join
                                     (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID,sum(TotalSalary) TotalSalary
                                       FROM [dbo].[EmployeeInformation]  em
                                      LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                               LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                                  LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                     " + join + @"

                                       WHERE EmployeeStatus = 'Active'
                                        AND em.GroupID = '" + companyGroupId + @"'
                                        group by BudgetCode,em.GroupID
                                       ) e on m.Id=e.BudgetCode and e.GroupID = m.CgId
                                     -------------------------3b--------------------------------------------------------
                                      LEFT OUTER JOIN
                                      (
                                         SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id as cid from
                                            (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                        ) MBD
                                      LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId
                                      LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                      LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      " + join + @"
                                      WHERE CG.Id = '" + companyGroupId + @"'  " + wc + @"  " + DStatus + @"
                                     ) B
                                     ON m.id = b.ManpowerBudgetId AND b.Id = m.CgId AND B.cid = m.CompanyId
                                  		WHERE isnull(TotalNumber,0) > isnull(TotalManpower,0)
                                     GROUP BY m.Code,GroupName,CompanyId,m.Id,b.ManpowerBudgetId,TotalManpower,e.TotalSalary,TotalNumber,CName " + cListextM + @"
                                     ,m.Designation,m.EmployeeCategory,m.DesGName";
                    return _sqlRepository.GetDataCollection(sqlText);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal for ShortSummary List

        #region Modal for ShortDetail List

        public IEnumerable<Object> ModalShortDetail(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, int seq, string date, string status, GridParameter parameters)
        {
            var cList = string.Empty;
            var wc = string.Empty;
            var join = string.Empty;
            var cListId = string.Empty;
            var cn = string.Empty;
            var wcExt = string.Empty;
            var cListext = string.Empty;
            var cListextId = string.Empty;
            var cListEmp = string.Empty;
            var cListFinish = string.Empty;
            var cListOId = string.Empty;
            var cListextM = string.Empty;
            var cListextIdM = string.Empty;
            var cListextF = string.Empty;
            var cListextIdF = string.Empty;
            var cListEmpG = string.Empty;
            var DStatus = string.Empty;

            if (status == "Default")
            {
                DStatus = "";
            }
            else if (status == "Direct")
            {
                DStatus = "and PO.IsDirect = 1";
            }
            else if (status == "Indirect")
            {
                DStatus = "and PO.IsDirect = 0";
            }
            try
            {
                var sqlText = "";
                seq += 1;
                var cListBuilder = new System.Text.StringBuilder();
                cListBuilder.Append(cList);
                var cListextBuilder = new System.Text.StringBuilder();
                cListextBuilder.Append(cListext);
                var cListextMBuilder = new System.Text.StringBuilder();
                cListextMBuilder.Append(cListextM);
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                var whereClausebuilder = new System.Text.StringBuilder();
                whereClausebuilder.Append(wc);
                var wcExtBuilder = new System.Text.StringBuilder();
                wcExtBuilder.Append(wcExt);
                var cListextIdBuilder = new System.Text.StringBuilder();
                cListextIdBuilder.Append(cListextId);
                var cListEmpGBuilder = new System.Text.StringBuilder();
                cListEmpGBuilder.Append(cListEmpG);
                var cListEmpBuilder = new System.Text.StringBuilder();
                cListEmpBuilder.Append(cListEmp);
                var cListextIdMBuilder = new System.Text.StringBuilder();
                cListextIdMBuilder.Append(cListextIdM);
                var cListFinishBuilder = new System.Text.StringBuilder();
                cListFinishBuilder.Append(cListFinish);

                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.RType == "Entity")
                        {
                            cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                            cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                            cListextMBuilder.Append(",m." + item.StandardName);

                            if (item.StandardName == "EmployeeGroup")
                            {
                                joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                            }
                            else
                            {
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                            }
                        }
                        if (item.RType == "Position")
                        {
                            cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                            cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                            cListextMBuilder.Append(",m." + item.StandardName);
                            joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                        }
                        if (item.RType == "Z")
                        {
                            cListBuilder.Append("," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ");
                            cListextBuilder.Append("," + item.StandardName + "Defination.UserName  " + item.StandardName + "Defination");
                            cListextMBuilder.Append(",m." + item.StandardName + "Defination");
                            joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n");
                        }
                        if (item.RType == "ZA")
                        {
                            cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                            cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                            cListextMBuilder.Append(",m." + item.StandardName);
                            joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n");
                        }

                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            whereClausebuilder.Append(" and c.id='" + item.Id + "'");
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    whereClausebuilder.Append(" AND ISNULL(" + item.StandardName + "Defination.SystemId,'')='" + item.Text + "'");

                                    wcExtBuilder.Append(" AND ISNULL( " + item.StandardName + "DefinationId,'')='" + item.Text + "'");

                                    cListextIdBuilder.Append(",AND ISNULL(" + item.StandardName + "Defination.SystemId,'')  " + item.StandardName + "DefinationId");
                                    cListEmpGBuilder.Append(",AND ISNULL(" + item.StandardName + "Defination.SystemId,'')  ");
                                    cListEmpBuilder.Append(" and e." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");

                                    cListextIdMBuilder.Append(",m." + item.StandardName + "DefinationId");
                                    cListextF = "," + item.StandardName + "DefinationName";
                                    cListextIdF = "," + item.StandardName + "DefinationId";
                                    cListFinishBuilder.Append(" and B." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId");
                                }
                                else
                                {
                                    whereClausebuilder.Append(" and " + item.StandardName + ".Id='" + item.Text + "'");

                                    wcExtBuilder.Append(" and " + item.StandardName + "Id='" + item.Text + "'");

                                    cListextIdBuilder.Append("," + item.StandardName + ".Id  " + item.StandardName + "Id");
                                    cListEmpGBuilder.Append("," + item.StandardName + ".Id  ");
                                    cListEmpBuilder.Append(" and e." + item.StandardName + "Id = m." + item.StandardName + "Id");

                                    cListextIdMBuilder.Append(",m." + item.StandardName + "Id");
                                    cListextF = "," + item.StandardName + "Name";
                                    cListextIdF = "," + item.StandardName + "Id";
                                    cListFinishBuilder.Append(" and B." + item.StandardName + "Id = m." + item.StandardName + "Id");
                                }
                            }
                        }
                    }
                }
                cListFinish = cListFinishBuilder.ToString();
                cListextIdM = cListextIdMBuilder.ToString();
                cListEmp = cListEmpBuilder.ToString();
                cListEmpG = cListEmpGBuilder.ToString();
                cListextId = cListextIdBuilder.ToString();
                wcExt = wcExtBuilder.ToString();
                wc = whereClausebuilder.ToString();
                join = joinBuilder.ToString();
                cListextM = cListextMBuilder.ToString();
                cListext = cListextBuilder.ToString();
                cList = cListBuilder.ToString();
                sqlText = @"select m.Id,b.ManpowerBudgetId  MbId,Code BudgetCode
                                ,Short = CASE WHEN isNull(TotalNumber,0) - isNull(TotalManpower,0) > 0
                                      THEN isNull(TotalNumber,0) - isNull(TotalManpower,0) ELSE 0 end
                                ,isnull(e.TotalManpower,0) as onRole
                                ,isnull(b.TotalNumber,0) as Proposed
                                ,e.TotalSalary/e.TotalManpower as avgSalary
                                ,m.Code budgetCodeE
                                ,m.GroupName
                                ,m.CompanyId
                                ,m.CName as CompanyName
                                " + cListextM + @"
                                ,m.Designation
                                ,m.DesGName
                                ,m.EmployeeCategory
                                from
                                ----------------------------1 bc--------------------------------------
                                (select MB.Code, MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName " + cListext + @"
                                  ,Des.UserName Designation,EmpC.UserName EmployeeCategory,DesG.UserName DesGName
                                  from [MST].[ManpowerBudget]  MB
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                      LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
                                      LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
                                      LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId

                                      LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                      " + join + @"
                                      where Cg.Id = '" + companyGroupId + @"' " + wc + @" AND MB.Active = 1
                                )  m
                                    -----------------------2e--------------------------------
                                left outer join
                                (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID,sum(TotalSalary) TotalSalary
                                      FROM [dbo].[EmployeeInformation]  em
                                      LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                     LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                                  LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                     " + join + @"
                                      WHERE EmployeeStatus = 'Active'
                                        AND em.GroupID = '" + companyGroupId + @"'   " + DStatus + @"
                                        group by BudgetCode,em.GroupID
                                ) e on m.Id=e.BudgetCode and e.GroupID = m.CgId
                                     -------------------------3b--------------------------------------------------------
                                LEFT OUTER JOIN
                                (
                                 SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id as cid from
                                 (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                 ) MBD
                                 left outer join [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId
                                 LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                 LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                 LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                 LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                 " + join + @"
                                 where CG.Id = '" + companyGroupId + @"'  " + wc + @"  " + DStatus + @"
                                 ) B
                                 on m.id = b.ManpowerBudgetId and b.Id = m.CgId and B.cid = m.CompanyId
             	                 where isnull(TotalNumber,0) > isnull(TotalManpower,0)
                                 group by m.Code,GroupName,CompanyId,m.Id,b.ManpowerBudgetId,TotalManpower,e.TotalSalary,TotalNumber,CName " + cListextM + @"
                                 ,m.Designation,m.EmployeeCategory,m.DesGName";

                return _sqlRepository.GetDataCollection(sqlText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal for ShortDetail List

        #region BudgetCode Wise Employee List

        public IEnumerable<Object> BudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, string budgetCode, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var EmployeeCategory = string.Empty;
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeCategory = "";
            }
            else
            {
                EmployeeCategory = @"AND EmpC.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            }
            var cList = string.Empty;
            var cListext = string.Empty;
            var cListEmpG = string.Empty;
            var cListextM = string.Empty;
            var join = string.Empty;

            try
            {
                var sqlText = "";
                var cListBuilder = new System.Text.StringBuilder();
                cListBuilder.Append(cList);
                var cListextBuilder = new System.Text.StringBuilder();
                cListextBuilder.Append(cListext);
                var cListextMBuilder = new System.Text.StringBuilder();
                cListextMBuilder.Append(cListextM);
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.RType == "Entity")
                        {
                            cListBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                            cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                            cListextMBuilder.Append(",m." + item.StandardName);

                            if (item.StandardName == "EmployeeGroup")
                            {
                                joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                            }
                            else
                            {
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                            }
                        }
                        else
                        {
                            cListextMBuilder.Append("," + item.StandardName + ".UserName " + item.StandardName + " ");
                            cListextBuilder.Append("," + item.StandardName + ".UserName  " + item.StandardName);
                            cListextMBuilder.Append(",m." + item.StandardName);

                            joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n");
                        }
                    }
                }
                join = joinBuilder.ToString();
                cListextM = cListextMBuilder.ToString();
                cListext = cListextBuilder.ToString();
                cList = cListBuilder.ToString();

                sqlText = @"SELECT EmployeeCode,EmployeeName,Des.UserName GivDesignation,REPLACE(CONVERT(VARCHAR(11), EMP.DOJ, 106), ' ', '-') DOJ,pd.UserName pDesname " + cList + @"
                                FROM [dbo].[EmployeeInformation] EMP
                                  LEFT JOIN [HKP].Designation Des ON Des.Id = emp.GivenDesignationId
                                  LEFT outer join [MST].[ManpowerBudget] AS MB  on MB.Id = emp.BudgetCode
							         LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                  LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                    LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id

								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                  " + join + @"
                                  WHERE
                                  MB.Id='" + budgetCode + @"' 
                                    AND EMP.GroupID = '" + companyGroupId + @"' " + EmployeeCategory + @"
                                    AND EMP.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(sqlText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> WpBudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string budgetCode)
        {
            var cList = string.Empty;
            var cListext = string.Empty;
            var cListEmpG = string.Empty;
            var cListextM = string.Empty;
            var Join = string.Empty;

            try
            {
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                            cListextM += ",m." + item.StandardName;

                            if (item.StandardName == "EmployeeGroup")
                            {
                                Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        else
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListext += "," + item.StandardName + ".UserName  " + item.StandardName;
                            cListextM += ",m." + item.StandardName;

                            Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = Po." + item.StandardName + "Id\n";
                        }
                    }
                }

                var gsql = @"SELECT EmployeeCode,EmployeeName,Des.UserName GivDesignation,REPLACE(CONVERT(VARCHAR(11), EMP.DOJ, 106), ' ', '-') DOJ,pd.UserName pDesname " + cList + @"
                                FROM [dbo].[EmployeeInformation] emp
                                  LEFT JOIN [HKP].Designation Des ON Des.Id = emp.GivenDesignationId
                                  LEFT outer join [MST].[ManpowerBudget] AS MB  on MB.Id = emp.BudgetCode
							         LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                  LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                    LEFT OUTER JOIN HKP.Designation PD ON PO.DesignationId=PD.Id
                                  " + Join + @"
                                  where
                                  MB.Id='" + budgetCode + @"' order by emp.EmployeeName";
                return _sqlRepository.GetDataCollection(gsql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion BudgetCode Wise Employee List

    }
}
