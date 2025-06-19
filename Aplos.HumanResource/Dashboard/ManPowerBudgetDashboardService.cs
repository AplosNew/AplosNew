using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.Accounts;
using Library.ViewModel.Organizations;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Dashboard
{
    public class ManPowerBudgetDashboardService
    {
        #region Constructor

        private readonly SqlRepository _sqlRepository;

        public ManPowerBudgetDashboardService()
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

        #region GroupWiseSummaryOfManpowerBudget

        public IEnumerable<object> GroupWiseCompanyList(string companyGroupId,string date, string status, string EmplyeeTypeOrCategoryId)
        {
            var EmployeeCategory = string.Empty;
            var shortExcess = string.Empty;
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeCategory = "";
                shortExcess = "sum(short) Short,sum(Excess) Excess";
            }
            else
            {
                EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
                shortExcess = "SUM(short) Short,SUM(Excess) Excess";
            }
           
            try
            {
                var dStatus = string.Empty;
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
        //        var sql = @"SELECT  CompanyGroupId,GroupName,CompanyId,UserName,Case when  ISNULL(IsDirect,0) = 0 then 'Indirect' else 'Direct' end AS  IsDirect, ISNULL(SUM(TotalNumber),0) ProposedManpowerBudget, ISNULL(SUM(TotalManpower),0) TotalManpower , sum(short) Short,sum(Excess) Excess
								//,ISNULL(SUM(TotalSalary),0) OnRoleSalaryC
								//--,ISNULL((SUM(MaxSal)+SUM(MinSal))/2,0) ProposedSalaryC
								//,sum(BudgetedSalary) ProposedSalaryC
        //                         FROM
        //                         (
        //                             SELECT m.CgId CompanyGroupId, m.IsDirect,m.Id,b.TotalNumber,m.GroupName,m.CompanyId,m.CName as UserName,EmpInfo.TotalSalary
        //                             ,EmpInfo.TotalManpower,(ISNULL(Sal.MaximumSalary,0)) MaxSal,(ISNULL(Sal.MinimumSalary,0)) MinSal
								//	 ,((ISNULL(Sal.MaximumSalary,0)) + (ISNULL(Sal.MinimumSalary,0)) / 2 ) * b.TotalNumber BudgetedSalary
        //                             , Short = CASE WHEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) > 0
        //                                        THEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) ELSE 0 END
        //                             , Excess = CASE WHEN isNull(TotalManpower,0) - ISNULL(TotalNumber,0) > 0
        //                                        THEN ISNULL(TotalManpower,0) - ISNULL(TotalNumber,0) ELSE 0 END
        //                              FROM
        //                                  --------------------1 budgetCode from [MST].[ManpowerBudget]--------------------------------------
        //                                    (SELECT po.IsDirect, MB.Code,MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName FROM [MST].[ManpowerBudget]  MB
        //                                      LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
        //                                       LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
        //                                     LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId

        //                                     LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId

								//              LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
								//              LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
								//              LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

        //                                       WHERE Cg.Id = '" + companyGroupId + @"' " + dStatus + @" " + EmployeeCategory + @" AND MB.Active = 1
        //                                    )  M
        //                                   -----------------------2. EmployeeInformation from [dbo].[EmployeeInformation]--------------------------------
        //                                    LEFT OUTER JOIN
        //                                     (SELECT PO.IsDirect,COUNT(SystemID) TotalManpower,BudgetCode,GroupID,c.Id cid,SUM(TotalSalary) TotalSalary
        //                                       FROM [dbo].[EmployeeInformation]  em
								//                LEFT outer join [MST].[ManpowerBudget] AS MB  ON  MB.Id = em.BudgetCode
        //                                         LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
        //                                      LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
        //                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
        //                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
        //                                         LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
								//	            LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
								//	            LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
								//	            LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
								//	            LEFT JOIN [ORG].[Section] ON Section.Id = Po.SectionId
								//	            LEFT JOIN [ORG].[Subsection] ON Subsection.Id = Po.SubsectionId

								//	            LEFT JOIN [HKP].Designation GDes ON GDes.Id = EM.GivenDesignationId
								//                LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.GivenDesignationId
								//                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

        //                                       WHERE EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM')
        //                                        AND GroupID = '" + companyGroupId + @"'  " + dStatus + @" " + EmployeeCategory + @"
        //                                        group by BudgetCode,GroupID,c.Id, PO.IsDirect
        //                                    ) EmpInfo on m.Id=EmpInfo.BudgetCode and EmpInfo.GroupID = m.CgId and EmpInfo.cid = m.CompanyId and EmpInfo.IsDirect = m.IsDirect 
        //                                    --------------------------ManpowerBudgetWiseSalary-----------------------------------
								//            LEFT OUTER JOIN
								//            (

								//                SELECT MBA.ManpowerBudgetId,po.IsDirect,
								//	            MinimumSalary = case when MBA.EffectiveDate <= '" + date + @"'  then  isnull(MinimumSalary,0) else 0 end,
								//	            MaximumSalary = case when MBA.EffectiveDate <= '" + date + @"'  then  isnull(MaximumSalary,0) else 0 end
								//	            ,ED.EffectiveDate,m.CompanyId
								//	            FROM [MST].[ManpowerBudgetAllowance] MBA
								//                LEFT OUTER JOIN [MST].[ManpowerBudget] AS m ON m.Id = MBA.ManpowerBudgetId
								//	            LEFT OUTER JOIN [MST].[ManpowerBudgetDetail] AS MBD ON MBD.ManpowerBudgetId = MBA.ManpowerBudgetId
        //                                             LEFT OUTER JOIN [ORG].[Position] AS PO ON m.PositionId = PO.Id
								//	            LEFT OUTER JOIN (
								//	            SELECT MAX(EffectiveDate) EffectiveDate,ManpowerBudgetId,CompanyId from [MST].[ManpowerBudgetAllowance]
								//	             LEFT OUTER JOIN [MST].[ManpowerBudget] AS m on m.Id = ManpowerBudgetId
								//	             WHERE EffectiveDate=(SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetAllowance] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
								//	             GROUP BY ManpowerBudgetId ,CompanyId
								//	            )  ED ON ED.ManpowerBudgetId = MBA.ManpowerBudgetId and ED.EffectiveDate = MBA.EffectiveDate
								//	             WHERE
								//	             ED.EffectiveDate IS NOT NULL AND m.CompanyGroupId =  '" + companyGroupId + @"'
								//	             " + dStatus + @" AND MBD.EffectiveDate = (SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetDetail] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
								//	            ) Sal ON m.Id = Sal.ManpowerBudgetId AND m.CompanyId = Sal.CompanyId and m.IsDirect = Sal.IsDirect
        //                                     -------------------------3. Manpower Budget Detail from [MST].[ManpowerBudgetDetail]--------------------------------------------------------
        //                                      LEFT OUTER JOIN
        //                                    (
        //                                    SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id AS cid,PO.IsDirect FROM
        //                                    (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
								//	            FROM [MST].[ManpowerBudgetDetail]
								//	            WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
								//	            ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
        //                                     ) MBD

        //                                      LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId

        //                                      LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId

        //                                      LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
        //                                      LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
        //                                      LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
        //                                       LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
								//               LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
								//               LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
								//               LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
        //                                     WHERE CG.Id = '" + companyGroupId + @"' " + dStatus + @" AND TotalNumber > 0 
        //                                     ) B

        //                                     ON M.id = b.ManpowerBudgetId AND B.Id = M.CgId AND B.cid = M.CompanyId AND B.IsDirect = M.IsDirect
        //                         ) EDE GROUP BY GroupName,CompanyId,UserName,IsDirect,CompanyGroupId ORDER BY UserName";

               var sql = @"SELECT  CompanyGroupId,GroupName,CompanyId,UserName,Case when  ISNULL(IsDirect,0) = 0 then 'Indirect' else 'Direct' end AS  IsDirect, ISNULL(SUM(TotalNumber),0) ProposedManpowerBudget, ISNULL(SUM(TotalManpower),0) TotalManpower , sum(short) Short,sum(Excess) Excess
                            ,ISNULL(SUM(TotalSalary),0) OnRoleSalaryC
                            --,ISNULL((SUM(MaxSal)+SUM(MinSal))/2,0) ProposedSalaryC
                            ,sum(BudgetedSalary) ProposedSalaryC
                            FROM
                            (
                            	SELECT m.CgId CompanyGroupId, m.IsDirect,m.Id,IsNull(b.TotalNumber,0) TotalNumber,m.GroupName,m.CompanyId,m.CName as UserName, IsNull(EmpInfo.TotalSalary,0) TotalSalary
                            	,IsNull(EmpInfo.TotalManpower,0) TotalManpower,(ISNULL(Sal.MaximumSalary,0)) MaxSal,(ISNULL(Sal.MinimumSalary,0)) MinSal
                            	,(ISNULL(Sal.MaximumSalary,0) + ISNULL(Sal.MinimumSalary,0)) / 2  AvgSal
                            	,((ISNULL(Sal.MaximumSalary,0) + ISNULL(Sal.MinimumSalary,0)) / 2 ) * IsNull(b.TotalNumber,0) BudgetedSalary
                            	, Short = CASE WHEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) > 0
                            	THEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) ELSE 0 END
                            	, Excess = CASE WHEN isNull(TotalManpower,0) - ISNULL(TotalNumber,0) > 0
                            	THEN ISNULL(TotalManpower,0) - ISNULL(TotalNumber,0) ELSE 0 END
                            	From
                            	(
                            	SELECT PO.IsDirect, MB.Code,MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName 
                            	FROM [MST].[ManpowerBudget]  MB
                            	LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            	LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id = E.CompanyId
                            	LEFT OUTER JOIN [ORG].[CompanyGroup] AS CG ON CG.Id = C.CompanyGroupId
                            	LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            	WHERE CG.Id = '" + companyGroupId + @"' " + dStatus + @" " + EmployeeCategory + @"   AND MB.Active = 1
                            	) M
                            	Left Outer Join
                            	(
                            		SELECT BudgetCode,COUNT(SystemId) TotalManpower,SUM(TotalSalary) TotalSalary
                            		FROM [dbo].[EmployeeInformation]  
                            		WHERE EmployeeStatus = 'Active' and ISNULL(BudgetCode,'')<>'' AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') AND GroupID = '" + companyGroupId +@"'

                                    group by BudgetCode
                            	) EmpInfo on M.Id=EmpInfo.BudgetCode
                            	Left Outer Join
                            	(
                            		Select MBA.ManpowerBudgetId,MBA.MinimumSalary,MBA.MaximumSalary,MBA.EffectiveDate from 
                            		(
                            			Select MAX(EffectiveDate) EffectiveDate,ManpowerBudgetId from
                            			[MST].[ManpowerBudgetAllowance]
                            			WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'"+date+ @"')
                            			GROUP BY ManpowerBudgetId
                            		) x
                            		Left Join MST.ManpowerBudgetAllowance MBA on MBA.Id=(Select Top 1 id from MST.ManpowerBudgetAllowance M where M.ManpowerBudgetId=x.ManpowerBudgetId and M.EffectiveDate=x.EffectiveDate order by MBA.EffectiveDate Desc)
                            	) Sal on M.Id=Sal.ManpowerBudgetId
                            	Left Outer Join
                            	(
                            		SELECT x.ManpowerBudgetId,x.TotalNumber,x.EffectiveDate from
                            		(
                            			Select rank() over (partition by ManpowerBudgetId order by  EffectiveDate DESC,Id) RNK, TotalNumber, ManpowerBudgetId, EffectiveDate
                            			from [MST].[ManpowerBudgetDetail]
                            			WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"')
                            		) x
                            		Where x.RNK=1
                            	) B on M.Id=B.ManpowerBudgetId
                            ) EDE GROUP BY GroupName,CompanyId,UserName,IsDirect,CompanyGroupId ORDER BY UserName";

                DataTable dt = _sqlRepository.GetDataTable(sql);
                //GroupName,CompanyId,UserName,Case when  ISNULL(IsDirect, 0) = 0 then 'Indirect' else 'Direct' end AS  IsDirect,
                DataTable dtTemp = dt.Clone();
                if (dt.Rows.Count > 0)
                {
                    dtTemp = dt.AsEnumerable().GroupBy(x => new
                    {
                        CompanyId = x["CompanyId"],
                        GroupName = x["GroupName"],
                        UserName = x["UserName"],
                        CompanyGroupId = x["CompanyGroupId"],
                        IsDirect = "General"

                    })
                .Select(x =>
                {
                    DataRow row = dt.NewRow();
                    row["IsDirect"] = "General";
                    row["GroupName"] = x.Key.GroupName; row["CompanyId"] = x.Key.CompanyId; row["UserName"] = x.Key.UserName; row["CompanyGroupId"] = x.Key.CompanyGroupId;
                    row["ProposedManpowerBudget"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ProposedManpowerBudget"]));
                    row["TotalManpower"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["TotalManpower"])); row["Short"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["Short"]));
                    row["Excess"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["Excess"])); row["OnRoleSalaryC"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OnRoleSalaryC"]));
                    row["ProposedSalaryC"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ProposedSalaryC"]));
                    return row;
                }
                                      ).CopyToDataTable();
                }


                dt.Merge(dtTemp);
                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dt);

                // return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion GroupWiseSummaryOfManpowerBudget

        #region DetailDrillDownOfManpowerBudget

        public IEnumerable<object> DetailDrillDownTable(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId,string companyGroupId)
        {
            var EmployeeCategory = string.Empty;
            var shortExcess = string.Empty;
            if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            {
                EmployeeCategory = "";
                shortExcess = ",SUM(short) Short,SUM(Excess) Excess";
            }
            else
            {
                EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
                shortExcess = ",SUM(short) Short,SUM(Excess) Excess";
            }

            var cList = string.Empty;
            var cListSequence = string.Empty;

            var wc = string.Empty;
            var join = string.Empty;
            var cListId = string.Empty;
            var wcExt = string.Empty;
            var cListext = string.Empty;
            var cListextId = string.Empty;
            var cListEmp = string.Empty;
            var cListFinish = string.Empty;
            var cListOId = string.Empty;
            var cListextM = string.Empty;
            var cListextMSequence = string.Empty;

            var cListextIdM = string.Empty;
            var cListextF = string.Empty;
            var cListextIdF = string.Empty;
            var cListextSeq = string.Empty;

            var cListEmpG = string.Empty;
            var wcm = string.Empty;
            var dStatus = string.Empty;
            var cListextIdR = string.Empty;
            var JoinEm = string.Empty;
            var wcem = string.Empty;
            try
            {
                seq += 1;
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
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cListOId += "," + item.StandardName + "Id";
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                cListSequence = "," + item.StandardName + ".Sequence";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                    JoinEm += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = em." + item.StandardName + "Id\n";
                                }
                                else if (item.StandardName == "Plant")
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".CompanyId =  C.Id AND " + item.StandardName + ".Id = E." + item.StandardName + "Id  \n");
                                    JoinEm += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = e." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n");
                                    JoinEm += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListSequence = "," + item.StandardName + ".Sequence";
                                cListId = "," + item.StandardName + ".Id";
                                cListOId += "," + item.StandardName + "Id";

                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                                JoinEm += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList = "," + item.StandardName + "Defination.UserName";
                                cListSequence = "," + item.StandardName + "Defination.UserName";
                                cListId = "," + item.StandardName + "Defination.SystemId";
                                cListOId += "," + item.StandardName + "DefinationId";

                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n");
                                JoinEm += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListSequence = "," + item.StandardName + ".Sequence";
                                cListId = "," + item.StandardName + ".Id";
                                cListOId += "," + item.StandardName + "Id";

                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n");
                                JoinEm += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n";
                            }
                        }
                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND C.Id ='" + item.Id + "'";
                            wcem = "AND em.CompanyId ='" + item.Id + "'";
                            cListext = "";
                            cListextId = "";
                            cListEmp = "";
                            wcm = "AND C.Id='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    wc += " and ISNULL(" + item.StandardName + "Defination.SystemID,'')='" + item.Text + "'";
                                    wcem += " and ISNULL(" + item.StandardName + "Defination.SystemID,'')='" + item.Text + "'";
                                    wcm = " and ISNULL(" + item.StandardName + "DefinationId,'')='" + item.Text + "'";
                                    wcExt += " and ISNULL(" + item.StandardName + "DefinationId,'')='" + item.Text + "'";
                                    cListext += "," + item.StandardName + "Defination.UserName  " + item.StandardName + "DefinationName";
                                    cListextId += "," + item.StandardName + "Defination.SystemID  " + item.StandardName + "DefinationId";
                                    cListextIdR += "," + item.StandardName + "Defination.SystemID  " + item.StandardName + "DefinationId";
                                    cListEmpG += "," + item.StandardName + "Defination.SystemID  ";
                                    cListEmp += " and e." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId";
                                    cListextM += ",m." + item.StandardName + "DefinationName";
                                    cListextMSequence += ",m.Sequence";
                                    cListextIdM += ",m." + item.StandardName + "DefinationId";
                                    cListextF = "," + item.StandardName + "DefinationName";
                                    cListextIdF = "," + item.StandardName + "DefinationId";
                                    cListFinish += " and B." + item.StandardName + "DefinationId = m." + item.StandardName + "DefinationId";

                                }
                                else
                                {
                                    wc += " and " + item.StandardName + ".Id='" + item.Text + "'";
                                    wcem += " and " + item.StandardName + ".Id='" + item.Text + "'";
                                    wcm = " and " + item.StandardName + "Id='" + item.Text + "'";
                                    wcExt += " and " + item.StandardName + "Id='" + item.Text + "'";
                                    cListext += "," + item.StandardName + ".UserName  " + item.StandardName + "Name";
                                    cListextId += "," + item.StandardName + ".Id  " + item.StandardName + "Id";
                                    cListextIdR += "," + item.StandardName + ".Id  " + item.StandardName + "Id";
                                    cListEmpG += "," + item.StandardName + ".Id  ";
                                    cListEmp += " and e." + item.StandardName + "Id = m." + item.StandardName + "Id";
                                    cListextM += ",m." + item.StandardName + "Name";
                                    cListextMSequence += ",m.Sequence";
                                    cListextIdM += ",m." + item.StandardName + "Id";
                                    cListextF = "," + item.StandardName + "Name";
                                    cListextIdF = "," + item.StandardName + "Id";
                                    cListFinish += " and B." + item.StandardName + "Id = m." + item.StandardName + "Id";

                                }
                            }
                        }
                    }
                }

                join = joinBuilder.ToString();
                //var sql = @"SELECT CASE WHEN  ISNULL(IsDirect,0) = 0 THEN 'Indirect' ELSE 'Direct' END AS  IsDirect,
                //                        CompanyId,ISNULL(UserName,'N/A') UserName,UId,Sequence" + cListextF + @"" + cListextIdF + @"
                //                       ,ISNULL(sum(TotalNumber),0) ProposedManpowerBudget ,ISNULL(SUM(TotalManpower),0) TotalManpower
                //                        ,ISNULL(sum(TotalSalary),0) OnRoleSalaryC
                //                        -- ,ISNULL((sum(MaxSal)+sum(MinSal))/2,0) ProposedSalaryC
                //                        ,sum(BudgetedSalary) ProposedSalaryC
                //                        " + shortExcess + @" --Sum of Short / excess
                //                        FROM
                //                         (
                //                         SELECT m.Id
                //                         ,b.TotalNumber
                //                         ,m.CompanyId
                //                          " + cListextM + @" -- plant name and division name
                //                          " + cListextIdM + @" -- Plant Id & division Id

                //                           ,m.IsDirect
                //                          ,m.UId
                //                          ,m.UserName
                //                          ,m.Sequence
                //                          ,e.TotalSalary
                //                          ,e.TotalManpower
                //                            ,(ISNULL(Sal.MaximumSalary,0)) MaxSal
                //    ,(ISNULL(Sal.MinimumSalary,0)) MinSal
                //                          ,Short = CASE
                //                          WHEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) > 0
                //                          THEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0)
                //                          ELSE 0
                //                          END
                //                          ,Excess = CASE
                //                          WHEN ISNULL(TotalManpower,0) - ISNULL(TotalNumber,0) > 0
                //                          THEN ISNULL(TotalManpower,0) - ISNULL(TotalNumber,0)
                //                          ELSE 0 END
                //                        ,((ISNULL(Sal.MaximumSalary,0)) + (ISNULL(Sal.MinimumSalary,0)) / 2 ) * b.TotalNumber BudgetedSalary
                //                          FROM
                //                          ----------------------1 bc-------------------------------c-------
                //                          (SELECT
                //                            MB.Code
                //                            ,MB.Id
                //                            ,MB.CompanyGroupId
                //                            ,c.Id AS CompanyId
                //                            ,c.UserName AS CName
                //                            ,PO.IsDirect

                //                            " + cListext + @"
                //                            " + cListextIdR + @"
                //                            " + cList + @" UserName
                //                            " + cListId + @" UId
                //                            " + cListSequence + @" Sequence

                //                            FROM [MST].[ManpowerBudget]  MB
                //                            LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                //                           LEFT OUTER JOIN [ORG].[Company] AS c on c.Id = MB.CompanyId AND c.CompanyGroupId = cg.Id
                //                            LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                //                            LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId

                //	LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
                //    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
                //    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                //                            " + join + @"
                //                            WHERE Cg.Id = '" + companyGroupId + @"' " + dStatus + @"  " + wc + @" " + EmployeeCategory + @" AND MB.Active = 1
                //                            )  m
                //                        -----------------------2e--------------------------------
                //                           LEFT OUTER JOIN
                //                           (SELECT COUNT(em.SystemID) TotalManpower,PO.IsDirect,BudgetCode,em.GroupID,C.Id AS cid   " + cListextId + @",sum(TotalSalary) TotalSalary
                //                           FROM [dbo].[EmployeeInformation]  em
                //                            LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                //                              LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                //                          LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId  AND c.CompanyGroupId = cg.Id
                //                          LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                //                          LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId

                //	LEFT JOIN [HKP].Designation GDes ON GDes.Id = EM.GivenDesignationId
                //    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.GivenDesignationId
                //    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                //                             " + JoinEm + @"
                //                           WHERE EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM')

                //                            AND  em.GroupID  = '" + companyGroupId + @"' " + wcem + @" " + dStatus + @" " + EmployeeCategory + @"
                //                           GROUP BY BudgetCode,em.GroupID,C.Id,PO.IsDirect  " + cListEmpG + @"
                //                        ) e on m.Id=e.BudgetCode and e.GroupID = m.CompanyGroupId and e.cid = m.CompanyId and m.IsDirect = e.IsDirect " + cListEmp + @"
                //                       --------------------------------ManpowerBudgetWisealary-----------------------------------
                //LEFT OUTER JOIN
                //(

                //    SELECT MBA.ManpowerBudgetId,PO.IsDirect,
                //	MinimumSalary = case when MBA.EffectiveDate <= '" + date + @"'  then  isnull(MinimumSalary,0) else 0 end,
                //	MaximumSalary = case when MBA.EffectiveDate <= '" + date + @"'  then  isnull(MaximumSalary,0) else 0 end
                //	,ED.EffectiveDate,c.Id cId
                //	FROM [MST].[ManpowerBudgetAllowance] MBA
                //    LEFT outer join [MST].[ManpowerBudget] AS mb on mb.Id = MBA.ManpowerBudgetId
                //	LEFT OUTER JOIN [MST].[ManpowerBudgetDetail] AS MBD ON MBD.ManpowerBudgetId = MBA.ManpowerBudgetId
                //                            LEFT outer join [ORG].[CompanyGroup] AS cg on cg.Id = mb.CompanyGroupId
                //    LEFT outer join [ORG].[Company] AS c on c.Id = mb.CompanyId AND c.CompanyGroupId = cg.Id
                //    LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = mb.EntityId
                //                            LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = mb.PositionId
                //                             " + join + @"
                //	LEFT OUTER JOIN (
                //	SELECT MAX(MBA.EffectiveDate) EffectiveDate,mB.Id  ManpowerBudgetId,c.Id from [MST].[ManpowerBudgetAllowance] MBA
                //	  LEFT outer join [MST].[ManpowerBudget] AS mB on mB.Id = MBA.ManpowerBudgetId
                //	LEFT OUTER JOIN [MST].[ManpowerBudgetDetail] AS MBD ON MBD.ManpowerBudgetId = MBA.ManpowerBudgetId
                //                            LEFT outer join [ORG].[CompanyGroup] AS cg on cg.Id = mB.CompanyGroupId
                //    LEFT outer join [ORG].[Company] AS c on c.Id = mB.CompanyId AND c.CompanyGroupId = cg.Id
                //    LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = mB.EntityId
                //                            LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = mB.PositionId
                //                             " + join + @"
                //	 WHERE  MBA.EffectiveDate=(SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetAllowance] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
                //	 GROUP BY mB.Id  ,c.Id
                //	)  ED ON ED.ManpowerBudgetId = MBA.ManpowerBudgetId AND ED.EffectiveDate = MBA.EffectiveDate
                //	 where
                //	 ED.EffectiveDate IS NOT NULL AND cg.Id ='" + companyGroupId + @"' " + wc + @" " + dStatus + @"
                //	 AND MBD.EffectiveDate = (SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetDetail] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"') ORDER BY EffectiveDate DESC)
                //	) Sal on m.Id = Sal.ManpowerBudgetId and m.CompanyId = Sal.cId and m.IsDirect = Sal.IsDirect

                //                         -------------------------3b--------------------------------------------------------
                //                          LEFT OUTER JOIN
                //                        (
                //                        SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id as CgId, C.Id as cid,PO.IsDirect   " + cListextIdR + @"

                //                        FROM

                //                        (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
                //	FROM [MST].[ManpowerBudgetDetail]
                //	WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + date + @"')
                //	ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                //                         ) MBD

                //                          LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId

                //                          LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId

                //                          LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                //                          LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId

                //                          LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId

                //		LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
                //    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
                //    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                //                           " + join + @"

                //                         WHERE CG.Id = '" + companyGroupId + @"' " + wc + @" " + dStatus + @" " + EmployeeCategory + @" AND TotalNumber > 0
                //                         ) B
                //                         ON m.id = b.ManpowerBudgetId and b.CgId = m.CompanyGroupId and B.cid = m.CompanyId and B.IsDirect = m.IsDirect   " + cListFinish + @"
                //                         ) ede  GROUP BY CompanyId,UserName,UId,Sequence,IsDirect " + cListextF + @"" + cListextIdF + @" ORDER BY Sequence";

                var sql = @"SELECT  CompanyGroupId,GroupName,CompanyId,UserName,UId,Sequence,Case when  ISNULL(IsDirect,0) = 0 then 'Indirect' else 'Direct' end AS  IsDirect, ISNULL(SUM(TotalNumber),0) ProposedManpowerBudget, ISNULL(SUM(TotalManpower),0) TotalManpower , sum(short) Short,sum(Excess) Excess
                                ,ISNULL(SUM(TotalSalary),0) OnRoleSalaryC
                                --,ISNULL((SUM(MaxSal)+SUM(MinSal))/2,0) ProposedSalaryC
                                ,sum(BudgetedSalary) ProposedSalaryC
                                FROM
                                (
                                	SELECT m.CgId CompanyGroupId, m.IsDirect,m.Id,IsNull(b.TotalNumber,0) TotalNumber,m.GroupName,m.CompanyId, IsNull(EmpInfo.TotalSalary,0) TotalSalary
                                	,IsNull(EmpInfo.TotalManpower,0) TotalManpower,(ISNULL(Sal.MaximumSalary,0)) MaxSal,(ISNULL(Sal.MinimumSalary,0)) MinSal
                                	,(ISNULL(Sal.MaximumSalary,0) + ISNULL(Sal.MinimumSalary,0)) / 2  AvgSal
                                	,((ISNULL(Sal.MaximumSalary,0) + ISNULL(Sal.MinimumSalary,0)) / 2 ) * IsNull(b.TotalNumber,0) BudgetedSalary
                                	, Short = CASE WHEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) > 0
                                	THEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) ELSE 0 END
                                	, Excess = CASE WHEN isNull(TotalManpower,0) - ISNULL(TotalNumber,0) > 0
                                	THEN ISNULL(TotalManpower,0) - ISNULL(TotalNumber,0) ELSE 0 END
                                	" + cListextM + @" -- plant name and division name
                                                                  " + cListextIdM + @" -- Plant Id & division Id
                                            ,m.UId
                                          ,m.UserName
                                          ,m.Sequence
                                								  
                                	From
                                	(
                                	SELECT PO.IsDirect, MB.Code,MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName 
                " + cListext + @"
                                                                    " + cListextIdR + @"
                                                                    " + cList + @" UserName
                                                                    " + cListId + @" UId
                                                                    " + cListSequence + @" Sequence
                                	FROM [MST].[ManpowerBudget]  MB
                                	LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                	LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id = E.CompanyId
                                	LEFT OUTER JOIN [ORG].[CompanyGroup] AS CG ON CG.Id = C.CompanyGroupId
                                	LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                	LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
                                								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
                                								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                	" + join + @"
                                	WHERE CG.Id = '" + companyGroupId + @"' " + dStatus + @"  " + wc + @" " + EmployeeCategory + @"  AND MB.Active = 1
                                	) M
                                	Left Outer Join
                                	(
                                		SELECT BudgetCode,COUNT(SystemId) TotalManpower,SUM(TotalSalary) TotalSalary
                                		FROM [dbo].[EmployeeInformation]  
                                		WHERE EmployeeStatus = 'Active' and ISNULL(BudgetCode,'')<>'' AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') AND GroupID = '"+ companyGroupId + @"' 
                                		group by BudgetCode
                                	) EmpInfo on M.Id=EmpInfo.BudgetCode
                                	Left Outer Join
                                	(
                                		Select MBA.ManpowerBudgetId,MBA.MinimumSalary,MBA.MaximumSalary,MBA.EffectiveDate from 
                                		(
                                			Select MAX(EffectiveDate) EffectiveDate,ManpowerBudgetId from
                                			[MST].[ManpowerBudgetAllowance]
                                			WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"')
                                			GROUP BY ManpowerBudgetId
                                		) x
                                		Left Join MST.ManpowerBudgetAllowance MBA on MBA.Id=(Select Top 1 id from MST.ManpowerBudgetAllowance M where M.ManpowerBudgetId=x.ManpowerBudgetId and M.EffectiveDate=x.EffectiveDate order by MBA.EffectiveDate Desc)
                                	) Sal on M.Id=Sal.ManpowerBudgetId
                                	Left Outer Join
                                	(
                                		SELECT x.ManpowerBudgetId,x.TotalNumber,x.EffectiveDate from
                                		(
                                			Select rank() over (partition by ManpowerBudgetId order by  EffectiveDate DESC,Id) RNK, TotalNumber, ManpowerBudgetId, EffectiveDate
                                			from [MST].[ManpowerBudgetDetail]
                                			WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + date + @"')
                                		) x
                                		Where x.RNK=1
                                	) B on M.Id=B.ManpowerBudgetId
                                ) EDE GROUP BY GroupName,CompanyId,UserName,UId,Sequence,IsDirect,CompanyGroupId ORDER BY UserName";

                DataTable dt = _sqlRepository.GetDataTable(sql);
                DataTable dtTemp = dt.Clone();
                if (dt.Rows.Count > 0)
                {
                    dtTemp = dt.AsEnumerable().GroupBy(x => new
                    {
                        CompanyId = x["CompanyId"],
                        UId = x["UId"],
                        Sequence = x["Sequence"],

                        UserName = x["UserName"],
                        //CompanyGroupId = x["CompanyGroupId"],
                        IsDirect = "General"

                    })
                .Select(x =>
                {
                    DataRow row = dt.NewRow();
                    row["IsDirect"] = "General";
                    row["UId"] = x.Key.UId;row["Sequence"] = x.Key.Sequence;
                    row["CompanyId"] = x.Key.CompanyId; row["UserName"] = x.Key.UserName; //row["CompanyGroupId"] = x.Key.CompanyGroupId;
                    row["ProposedManpowerBudget"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ProposedManpowerBudget"]));
                    row["TotalManpower"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["TotalManpower"])); row["Short"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["Short"]));
                    row["Excess"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["Excess"])); row["OnRoleSalaryC"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OnRoleSalaryC"]));
                    row["ProposedSalaryC"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ProposedSalaryC"]));
                    return row;
                }
                                      ).CopyToDataTable();
                }


                dt.Merge(dtTemp);
                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
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

                                  WHERE EmployeeStatus = 'Active' " + dStatus + @"  AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') AND
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
                                  where EmployeeStatus = 'Active' " + dStatus + @"  AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') AND
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
                                  where EmployeeStatus = 'Active' AND MB.Active = 1  AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') " + dStatus + @"
                                  --AND em.GroupID  = '" + companyGroupId + @"' 
                                    and  em.CompanyId= '" + companyId + @"' " + wc + @" " + EmployeeCategory + @"";
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

                                       WHERE EmployeeStatus = 'Active'   AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') 
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

                                       WHERE EmployeeStatus = 'Active'  AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') " + DStatus + @"
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

                sqlText = @"Select MB.Id MbId ,ISNULL(emp.TotalManpower,0) as onRole,ISNULL(MBD.TotalNumber,0) as Proposed
										,Excess = CASE
										  WHEN ISNULL(emp.TotalManpower,0) - isNull(MBD.TotalNumber,0) > 0
										  THEN ISNULL(emp.TotalManpower,0) - isNull(MBD.TotalNumber,0)
										  ELSE 0 end
										,Short = CASE
										  WHEN ISNULL(MBD.TotalNumber,0) - isNull(emp.TotalManpower,0) > 0
										  THEN ISNULL(MBD.TotalNumber,0) - isNull(emp.TotalManpower,0)
										  ELSE 0 end
										  ,MB.Code BudgetCode,Cg.UserName as GroupName,MB.CompanyId
										, c.UserName as CompanyName
										,Plant.UserName  Plant,Division.UserName  Division,Department.UserName  Department
										,SubDivision.UserName  SubDivision,Section.UserName  Section,Unit.UserName  Unit
										,SubSection.UserName  SubSection,ShiftDefination.UserName  ShiftDefination
										,Des.UserName Designation,EmpC.UserName EmployeeCategory
										,EmpC.UserName EmployeeCategory
											
                            			from [MST].[ManpowerBudgetDetail] MBD
										left join [MST].[ManpowerBudget]  MB on MB.Id=MBD.ManpowerBudgetId 
										LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
										LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
										LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
										LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
										LEFT JOIN [HKP].Designation Des ON Des.Id = Po.DesignationId
										LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Des.Id
										LEFT JOIN [HKP].DesignationGroup DesG ON DesG.Id = DesM.DesignationGroupId
										LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
										LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
										LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
										LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
										LEFT JOIN [ORG].[SubDivision] ON SubDivision.Id = E.SubDivisionId
										LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
										LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
										LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
										LEFT JOIN [ShiftDefination] ON ShiftDefination.SystemId = MB.ShiftDefinationId

										left outer join (SELECT count(em.SystemID) TotalManpower,BudgetCode,em.GroupID,EmployeeCurrentStatus,EmployeeStatus,em.CompanyId
										FROM [dbo].[EmployeeInformation]  em
										LEFT outer join [MST].[ManpowerBudget] AS M  on M.Id = em.BudgetCode      
										WHERE EmployeeStatus = 'Active'   AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') 
                                        group by BudgetCode,em.GroupID,EmployeeCurrentStatus,EmployeeStatus,em.CompanyId
										) emp ON MB.Id=emp.BudgetCode


									where MB.Active = 1  AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') " + DStatus + @"                                  
                                     " + wc + @" " + EmployeeCategory + @"";
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
                                       WHERE EmployeeStatus = 'Active'   AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') " + DStatus + EmployeeCategory + @"
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

                                       WHERE EmployeeStatus = 'Active'   AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') " + DStatus + @"
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
                                      WHERE EmployeeStatus = 'Active'  AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') 
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
                                       WHERE EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') 
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

                                       WHERE EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') 
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
                                      WHERE EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') 
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
                                    AND EMP.EmployeeStatus = 'Active'  AND ISNULL(EMP.EmployeeCurrentStatus,'')  NOT IN ('TBS','LONG ABSENTEEISM') ";
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

        public string CreateOnRoleEmployeeReportSheet(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            ReportUtility reportUtility = new ReportUtility();

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Man Power Budget On Role Report";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;
                #region columns

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "Budget Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColBudgetCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Given Designation";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColGivenDesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                int Coldesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColEmployeeCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Salary";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColTotalSalary = COL;
                COL++;

                sheet[ROW, COL].Text = "Company";
                sheet[ROW, COL].ColumnWidth = 28;
                int ColCompany = COL;
                COL++;

                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColPlant = COL;
                COL++;

                sheet[ROW, COL].Text = "Division";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDivision = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColSubSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Unit";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColUnit = COL;
                COL++;

                sheet[ROW, COL].Text = "Shift Defination";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColShiftDefination = COL;
                COL++;

                sheet[ROW, COL].Text = "Line";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColLine = COL;
                
                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                //int startRow = ROW;
                int StartDataRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColDOJ].Text = data.Rows[i]["DOJ"].ToString();
                    sheet[ROW, ColBudgetCode].Text = data.Rows[i]["BudgetCode"].ToString();
                    sheet[ROW, ColGivenDesignation].Text = data.Rows[i]["GivenDesignation"].ToString();
                    sheet[ROW, Coldesignation].Text = data.Rows[i]["designation"].ToString();
                    sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColTotalSalary].Number = clsStaticInfo.dbl(data.Rows[i]["TotalSalary"].ToString());
                    sheet[ROW, ColCompany].Text = data.Rows[i]["Company"].ToString();
                    sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                    sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColUnit].Text = data.Rows[i]["Unit"].ToString();
                    sheet[ROW, ColShiftDefination].Text = data.Rows[i]["ShiftDefination"].ToString();
                    sheet[ROW, ColLine].Text = data.Rows[i]["Line"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }


                #region Total
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);

                sheet[ROW, ColTotalSalary - 1].Text = "Total";
                sheet[ROW, ColTotalSalary - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColTotalSalary].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalSalary) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColTotalSalary) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColTotalSalary].NumberFormat = "#,##0.00;(#,##0.00)";

               
                sheet.Range[ROW, ColTotalSalary - 1, ROW, COL].CellStyle.Font.Bold = true;

                #endregion Total

                sheet.AutoFilters.FilterRange = sheet.Range[StartDataRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartDataRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + StartDataRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Man Power Budget On Role Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CreateBudgetEmployeeReportReportSheet(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            ReportUtility reportUtility = new ReportUtility();

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Man Power Budget Budgeted Report";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;
                #region columns

                sheet[ROW, COL].Text = "Budget Code";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColBudgetCode = COL;
                COL++;

                sheet[ROW, COL].Text = "on Role";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColonRole = COL;
                COL++;

                sheet[ROW, COL].Text = "Proposed";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColProposed = COL;
                COL++;

                sheet[ROW, COL].Text = "Excess";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColExcess = COL;
                COL++;

                sheet[ROW, COL].Text = "Short";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColShort = COL;
                COL++;

                sheet[ROW, COL].Text = "Group Name";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGroupName = COL;
                COL++;

                sheet[ROW, COL].Text = "Company";
                sheet[ROW, COL].ColumnWidth = 28;
                int ColCompany = COL;
                COL++;

                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColPlant = COL;
                COL++;

                sheet[ROW, COL].Text = "Division";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDivision = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Division";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColSubDivision = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColSubSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Unit";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColUnit = COL;
                COL++;

                sheet[ROW, COL].Text = "Shift Defination";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColShiftDefination = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColEmployeeCategory = COL;
                 
                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                //int startRow = ROW;
                int StartDataRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColBudgetCode].Number = clsStaticInfo.dbl(data.Rows[i]["BudgetCode"].ToString());
                    sheet[ROW, ColonRole].Number = clsStaticInfo.dbl(data.Rows[i]["onRole"].ToString());
                    sheet[ROW, ColProposed].Number = clsStaticInfo.dbl(data.Rows[i]["Proposed"].ToString());
                    sheet[ROW, ColExcess].Number = clsStaticInfo.dbl(data.Rows[i]["Excess"].ToString());
                    sheet[ROW, ColShort].Number = clsStaticInfo.dbl(data.Rows[i]["Short"].ToString());
                    sheet[ROW, ColGroupName].Text = data.Rows[i]["GroupName"].ToString();
                    sheet[ROW, ColCompany].Text = data.Rows[i]["CompanyName"].ToString();
                    sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                    sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColSubDivision].Text = data.Rows[i]["SubDivision"].ToString();
                    sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColUnit].Text = data.Rows[i]["Unit"].ToString();
                    sheet[ROW, ColShiftDefination].Text = data.Rows[i]["ShiftDefination"].ToString();
                    sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }


                #region Total
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);
                
                //sheet[ROW, ColBudgetCode - 1].Text = "Total";
                //sheet[ROW, ColBudgetCode - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //sheet[ROW, ColBudgetCode].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBudgetCode) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColBudgetCode) + (ROW - 1).ToString() + ")";
                //sheet[ROW, ColBudgetCode].NumberFormat = "#,##0.00;(#,##0.00)";


                //sheet.Range[ROW, ColBudgetCode - 1, ROW, COL].CellStyle.Font.Bold = true;

                #endregion Total

                sheet.AutoFilters.FilterRange = sheet.Range[StartDataRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartDataRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + StartDataRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Man Power Budget Budgeted Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CreateShortEmployeeReportReportSheet(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            ReportUtility reportUtility = new ReportUtility();

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Man Power Budget Budgeted Report";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;
                #region columns

                sheet[ROW, COL].Text = "Budget Code";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColBudgetCode = COL;
                COL++;

                sheet[ROW, COL].Text = "on Role";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColonRole = COL;
                COL++;

                sheet[ROW, COL].Text = "Proposed";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColProposed = COL;
                COL++;

                //sheet[ROW, COL].Text = "Excess";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColExcess = COL;
                //COL++;

                sheet[ROW, COL].Text = "Short";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColShort = COL;
                COL++;

                sheet[ROW, COL].Text = "Group Name";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGroupName = COL;
                COL++;

                sheet[ROW, COL].Text = "Company";
                sheet[ROW, COL].ColumnWidth = 28;
                int ColCompany = COL;
                COL++;

                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColPlant = COL;
                COL++;

                sheet[ROW, COL].Text = "Division";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDivision = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDepartment = COL;
                COL++;

                //sheet[ROW, COL].Text = "Sub Division";
                //sheet[ROW, COL].ColumnWidth = 12;
                //int ColSubDivision = COL;
                //COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColSubSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Unit";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColUnit = COL;
                COL++;

                sheet[ROW, COL].Text = "Shift Defination";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColShiftDefination = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColEmployeeCategory = COL;

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                //int startRow = ROW;
                int StartDataRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColBudgetCode].Number = clsStaticInfo.dbl(data.Rows[i]["BudgetCode"].ToString());
                    sheet[ROW, ColonRole].Number = clsStaticInfo.dbl(data.Rows[i]["onRole"].ToString());
                    sheet[ROW, ColProposed].Number = clsStaticInfo.dbl(data.Rows[i]["Proposed"].ToString());
                    //sheet[ROW, ColExcess].Number = clsStaticInfo.dbl(data.Rows[i]["Excess"].ToString());
                    sheet[ROW, ColShort].Number = clsStaticInfo.dbl(data.Rows[i]["Short"].ToString());
                    sheet[ROW, ColGroupName].Text = data.Rows[i]["GroupName"].ToString();
                    sheet[ROW, ColCompany].Text = data.Rows[i]["CompanyName"].ToString();
                    sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                    sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    //sheet[ROW, ColSubDivision].Text = data.Rows[i]["SubDivision"].ToString();
                    sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColUnit].Text = data.Rows[i]["Unit"].ToString();
                    sheet[ROW, ColShiftDefination].Text = data.Rows[i]["ShiftDefination"].ToString();
                    sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }


                #region Total
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);

                //sheet[ROW, ColBudgetCode - 1].Text = "Total";
                //sheet[ROW, ColBudgetCode - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //sheet[ROW, ColBudgetCode].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBudgetCode) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColBudgetCode) + (ROW - 1).ToString() + ")";
                //sheet[ROW, ColBudgetCode].NumberFormat = "#,##0.00;(#,##0.00)";


                //sheet.Range[ROW, ColBudgetCode - 1, ROW, COL].CellStyle.Font.Bold = true;

                #endregion Total

                sheet.AutoFilters.FilterRange = sheet.Range[StartDataRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartDataRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + StartDataRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Man Power Budget Budgeted Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CreateExcessEmployeeReportReportSheet(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            ReportUtility reportUtility = new ReportUtility();

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Man Power Budget Budgeted Report";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;
                #region columns

                sheet[ROW, COL].Text = "Budget Code";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColBudgetCode = COL;
                COL++;

                sheet[ROW, COL].Text = "on Role";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColonRole = COL;
                COL++;

                sheet[ROW, COL].Text = "Proposed";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColProposed = COL;
                COL++;

                sheet[ROW, COL].Text = "Excess";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColExcess = COL;
                COL++;

                //sheet[ROW, COL].Text = "Short";
                //sheet[ROW, COL].ColumnWidth = 15;
                //int ColShort = COL;
                //COL++;

                sheet[ROW, COL].Text = "Group Name";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGroupName = COL;
                COL++;

                sheet[ROW, COL].Text = "Company";
                sheet[ROW, COL].ColumnWidth = 28;
                int ColCompany = COL;
                COL++;

                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColPlant = COL;
                COL++;

                sheet[ROW, COL].Text = "Division";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDivision = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDepartment = COL;
                COL++;

                //sheet[ROW, COL].Text = "Sub Division";
                //sheet[ROW, COL].ColumnWidth = 12;
                //int ColSubDivision = COL;
                //COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColSubSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Unit";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColUnit = COL;
                COL++;

                sheet[ROW, COL].Text = "Shift Defination";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColShiftDefination = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColEmployeeCategory = COL;

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                //int startRow = ROW;
                int StartDataRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColBudgetCode].Number = clsStaticInfo.dbl(data.Rows[i]["BudgetCode"].ToString());
                    sheet[ROW, ColonRole].Number = clsStaticInfo.dbl(data.Rows[i]["onRole"].ToString());
                    sheet[ROW, ColProposed].Number = clsStaticInfo.dbl(data.Rows[i]["Proposed"].ToString());
                    sheet[ROW, ColExcess].Number = clsStaticInfo.dbl(data.Rows[i]["Excess"].ToString());
                    //sheet[ROW, ColShort].Number = clsStaticInfo.dbl(data.Rows[i]["Short"].ToString());
                    sheet[ROW, ColGroupName].Text = data.Rows[i]["GroupName"].ToString();
                    sheet[ROW, ColCompany].Text = data.Rows[i]["CompanyName"].ToString();
                    sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                    sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    //sheet[ROW, ColSubDivision].Text = data.Rows[i]["SubDivision"].ToString();
                    sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColUnit].Text = data.Rows[i]["Unit"].ToString();
                    sheet[ROW, ColShiftDefination].Text = data.Rows[i]["ShiftDefination"].ToString();
                    sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }


                #region Total
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);

                //sheet[ROW, ColBudgetCode - 1].Text = "Total";
                //sheet[ROW, ColBudgetCode - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //sheet[ROW, ColBudgetCode].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBudgetCode) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColBudgetCode) + (ROW - 1).ToString() + ")";
                //sheet[ROW, ColBudgetCode].NumberFormat = "#,##0.00;(#,##0.00)";


                //sheet.Range[ROW, ColBudgetCode - 1, ROW, COL].CellStyle.Font.Bold = true;

                #endregion Total

                sheet.AutoFilters.FilterRange = sheet.Range[StartDataRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartDataRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + StartDataRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Man Power Budget Budgeted Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> MBWisefiltersData()
        {
            try
            {
                var sql = @"Select MB.Id BudgetId,MB.Code BudgetCode,D.Id DivisionId,D.UserName Division,E.Id EntityId,E.UserName Entity,DP.Id DepartmentId,DP.UserName Department,S.Id SectionId,S.UserName Section,SS.Id SubSectionId,SS.UserName SubSection,DG.Id DesignationId,DG.UserName Designation,SD.SystemID ShiftId,SD.ShiftDefinationName ShiftName,L.Id LineId,L.UserName Line 
from MST.ManpowerBudget MB
LEFT JOIN ORG.Position P ON P.id=MB.PositionId
LEFT JOIN ORG.Division D ON D.Id=P.DivisionId
LEFT JOIN ORG.Entity E ON E.Id=MB.EntityId
LEFT JOIN ORG.Department DP ON DP.Id=P.DepartmentId
LEFT JOIN ORG.Section S ON S.Id=P.SectionId
LEFT JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
LEFT JOIN HKP.Designation DG ON DG.Id=P.DesignationId
LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID=MB.ShiftDefinationId
LEFT JOIN ORG.Line L ON L.Id=MB.LineId
Order by MB.Code  ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable GetMBWiseSql(Dictionary<string, string> parameters)
        {
            try
            {
                var str = @"Select distinct PMB.Id BudgetId,PMB.Code,ISNULL(mbd.TotalNumber,0) Budgeted,ONR.OnRoll,PMB.Deployment,Short=CASE WHEN mbd.TotalNumber>ONR.OnRoll THEN mbd.TotalNumber-ONR.OnRoll ELSE 0 END
,Excess=CASE WHEN mbd.TotalNumber<ONR.OnRoll THEN ONR.OnRoll-mbd.TotalNumber ELSE 0 END
,D.UserName Division,E.UserName Entity,DP.UserName Department,S.UserName Section,SS.UserName SubSection,DG.UserName Designation,SD.ShiftDefinationName ShiftName
,L.UserName Line,PS.UserName Process 
FROM dbo.Employeeinformation EI
LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
LEFT JOIN(Select SUM(TotalNumber)TotalNumber,ManpowerBudgetId,Id from MST.ManpowerBudgetDetail Group BY ManpowerBudgetId,Id) AS mbd ON mbd.ManpowerBudgetId=PMB.Id
							  AND mbd.Id =(Select top(1) Id from MST.ManpowerBudgetDetail Where ManpowerBudgetId=PMB.Id order by EffectiveDate desc)
LEFT JOIN (SELECT COUNT(SystemId) OnRoll,BudgetCode FROM EmployeeInformation WHERE EmployeeStatus = 'Active' and ISNULL(BudgetCode,'')<>'' GROUP BY BudgetCode) ONR ON ONR.BudgetCode=EI.BudgetCode
LEFT JOIN ORG.Position P ON P.id=PMB.PositionId
LEFT JOIN ORG.Division D ON D.Id=P.DivisionId
LEFT JOIN ORG.Entity E ON E.Id=PMB.EntityId
LEFT JOIN ORG.Department DP ON DP.Id=P.DepartmentId
LEFT JOIN ORG.Section S ON S.Id=P.SectionId
LEFT JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
LEFT JOIN HKP.Designation DG ON DG.Id=P.DesignationId
LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID=PMB.ShiftDefinationId
LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
LEFT JOIN HKP.Process PS ON PS.Id=P.ProcessId
WHERE EI.EmployeeStatus = 'Active' AND PMB.Id<>'' 
AND PMB.Id in(" + parameters["BudgetId"] + @")
AND E.Id in(" + parameters["EntityId"] + @")
AND DP.Id in(" + parameters["DepartmentId"] + @")
AND S.Id in(" + parameters["SectionId"] + @")
AND SS.Id in(" + parameters["SubSectionId"] + @")
AND DG.Id in(" + parameters["DesignationId"] + @")
AND SD.SystemID in(" + parameters["ShiftId"] + @")
AND L.Id in(" + parameters["LineId"] + @")";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
