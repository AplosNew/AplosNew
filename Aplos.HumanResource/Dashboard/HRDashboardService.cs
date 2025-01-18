using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.Accounts;
using Library.ViewModel.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Dashboard
{
    public class HRDashboardService
    {
        #region Constructor

        private readonly SqlRepository _sqlRepository;

        public HRDashboardService()
        {
            _sqlRepository = new SqlRepository();
        }

        #endregion Constructor

        public IEnumerable<ComboModel> GetReportingPersonCbo(string compnayGroupId, string companyId, string plantId)
        {
            var plant = string.Empty;
            if (plantId == null)
            {
                plant = "";
            }
            else
            {
                plant = @"AND Ei.PlantId = '" + plantId + @"'";
            }
            var _sql = @"SELECT Distinct EI.EmployeeName,RLP.RptEmpSystemID FROM EmpReportingPerson RLP
						INNER JOIN
						EmployeeInformation EI ON EI.SystemId = RLP.RptEmpSystemID
						WHERE EI.GroupID = '" + compnayGroupId + @"' AND EI.CompanyId = '" + companyId + @"' " + plant + @"
					   ORDER BY EI.EmployeeName";

            return _sqlRepository.GetCombo(_sql, "RptEmpSystemID", "EmployeeName");
        }

        public IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId)
        {
            try
            {
                var strSQL = @"  SELECT StandardName, UserName ColumnName, RType
									   FROM ORG.StructureRelationship
									   WHERE RType = 'Entity'  AND CompanyGroupId = '" + CompanyGroupId + @"' --AND CompanyId = 'C20181'
							   UNION
							   SELECT StandardName, UserName ColumnName, RType FROM ORG.StructureRelationship  AS k
								      WHERE rtype = 'Position' AND NOT EXISTS (
																	SELECT 1
																	FROM ORG.StructureRelationship  AS t
																	WHERE t.standardname = k.standardname
									       and t.rtype = 'Entity'  AND t.CompanyGroupId = '" + CompanyGroupId + @"') ORDER BY RType";
                return _sqlRepository.GetModelCollection<OrgStructureListViewModel>(strSQL).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> OrgStructureListColList(string CompanyGroupId, string CompanyId)
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
										   ORDER BY StandardName,Sequence";
                DataTable dt = _sqlRepository.GetDataTable(strSQL);
                string id = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    if (id == dt.Rows[i]["StandardName"].ToString())
                    {
                        id = dt.Rows[i]["StandardName"].ToString();
                        dt.Rows[i].Delete();
                    }
                    else
                    {
                        id = dt.Rows[i]["StandardName"].ToString();
                    }



                }
                dt.DefaultView.Sort = "RType,Sequence";
                dt.DefaultView.Sort = "Sequence";
                dt = dt.DefaultView.ToTable();


                //return _EmployeeInformationRepository.SqlQuery<OrgStructureListViewModel>(strSQL);
                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> HROverAllStatusDefault(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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

            try
            {
                var cList = string.Empty;
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;

                var sql = @"SELECT
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

							    LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								WHERE EMP.IsConfirmed = 0 AND (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @"
								AND CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) < convert(date,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'
								) probationOverDue,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								WHERE EMP.IsConfirmed = 0 AND (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @"
								AND CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) = CONVERT(DATE,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'
								) probationToday,

								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								WHERE EMP.IsConfirmed = 0 AND (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @"
								AND
								(CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) > CONVERT(DATE,'" + hrDate + @"')
									AND
									CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) <=  CONVERT(DATE,(DATEADD(DAY, 7,'" + hrDate + @"')))  AND EMP.GroupID  = '" + companyGroupId + @"'
								)
								) probationNext7Days,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								WHERE EMP.EmployeeStatus = 'Separated'and CONVERT(DATE,EMP.DOSDate) = CONVERT(DATE,'" + hrDate + @"') " + EmployeeCategory + @"
								AND RSG.ApprovalStatus = 'APPROVED' AND EMP.GroupID  = '" + companyGroupId + @"'
								) separatedToday,
								(
								SELECT COUNT(EMP.SystemId) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id  and Emp.PlantId = hs.PlantID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								WHERE (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"'))  " + EmployeeCategory + @" AND
								(CONVERT(DATE, RSG.ApprovedEffectiveDate) > CONVERT(DATE, '" + hrDate + @"')
									AND
									CONVERT(DATE, RSG.ApprovedEffectiveDate) <= CONVERT(DATE, (DATEADD(DAY, 7, '" + hrDate + @"')))  AND EMP.GroupID = '" + companyGroupId + @"'
								)

								AND RSG.ApprovalStatus = 'APPROVED'
								) separatedNext7Days,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId = EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId = PO.Id

								LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId = EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								WHERE (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @" AND CONVERT(DATE, RSG.AddedDate) = CONVERT(DATE, '" + hrDate + @"') AND EMP.GroupID = '" + companyGroupId + @"'
								AND RSG.ApprovalStatus = 'Pending'
								) todayResignationApply,

								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId = EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId = PO.Id

								LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId = EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								WHERE   (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) AND EMP.GroupID = '" + companyGroupId + @"' " + EmployeeCategory + @"
								AND RSG.ApprovalStatus = 'Pending'
								) resignationApprovalPending,
								(
								SELECT COUNT(*) from 
								 EmployeeInformation EMP 
								
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId = PO.Id

								LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId = EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								 LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId

								WHERE   (DOJ<='" + hrDate + @"' AND (DOS IS NULL OR DOS >= '" + hrDate + @"'))  " + EmployeeCategory + @" AND CONVERT(DATE, Tem.NextDueDate) < CONVERT(DATE, '" + hrDate + @"')  AND EMP.GroupID = '" + companyGroupId + @"'
								) incrementOverDue,
								(
								SELECT COUNT(*) from 
								 EmployeeInformation EMP 
							    LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId = PO.Id

								LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId = EN.Id
							    LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								 LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId

								WHERE   (DOJ<='" + hrDate + @"' AND (DOS IS NULL OR DOS >= '" + hrDate + @"')) " + EmployeeCategory + @" AND CONVERT(DATE, Tem.NextDueDate) = CONVERT(DATE, '" + hrDate + @"') AND EMP.GroupID = '" + companyGroupId + @"'
								) incrementToday,
								(
								SELECT COUNT(EMP.SystemId) from 
								 EmployeeInformation EMP 
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and Emp.PlantId = hs.PlantID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId = PO.Id

								LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId = EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								 LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId

								WHERE  (DOJ<='" + hrDate + @"' AND (DOS IS NULL OR DOS >= '" + hrDate + @"')) AND EMP.GroupID = '" + companyGroupId + @"'   " + EmployeeCategory + @" AND

						   (CONVERT(DATE, Tem.NextDueDate) Between CONVERT(DATE, '" + hrDate + @"')
                                AND
							    CONVERT(DATE, (DATEADD(DAY, 7, '" + hrDate + @"')))
						   )
								) incrementNext7Days,

								(
								SELECT Count(Emp.SystemId) from 
								 EmployeeInformation EMP 
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and Emp.PlantId = hs.PlantID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId = PO.Id

								LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId = EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								 LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId

								WHERE  (DOJ<='" + hrDate + @"' AND (DOS IS NULL OR DOS >= '" + hrDate + @"')) AND EMP.GroupID = '" + companyGroupId + @"'   " + EmployeeCategory + @" AND

						   (CONVERT(DATE, Tem.NextDueDate) Between CONVERT(DATE, '" + hrDate + @"')

							   AND

								 CONVERT(DATE, (DATEADD(DAY, 30, '" + hrDate + @"')))
						   )
								) incrementNext30Days";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> HROverAllStatusDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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

            try
            {
                var cList = string.Empty;
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;

                seq += 1;
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                var WhereClausebuilder = new System.Text.StringBuilder();
                WhereClausebuilder.Append(wc);
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")//For Line
                            {
                                cListId = "," + item.StandardName + "Defination].Id";
                                cList = "," + item.StandardName + "Defination].UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n");
                            }
                            if (item.RType == "ZA")//For Line
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n");
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            WhereClausebuilder.Append("  AND C.Id ='" + item.Id + "'");
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    WhereClausebuilder.Append(" AND ISNULL(" + item.StandardName + "Defination.SystemID,'')='" + item.Text + "'");
                                }
                                else
                                {
                                    WhereClausebuilder.Append(" AND " + item.StandardName + ".Id='" + item.Text + "'");

                                }
                            }
                        }
                    }
                }
                wc = WhereClausebuilder.ToString();
                join = joinBuilder.ToString();
                var sql = @"SELECT
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE EMP.IsConfirmed = 0 AND (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + wc + @" " + EmployeeCategory + @"
								AND convert(date,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) < convert(date,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'
								) probationOverDue,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE EMP.IsConfirmed = 0 AND (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + wc + @" " + EmployeeCategory + @"
								AND CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) = CONVERT(DATE,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'
								) probationToday,

								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE EMP.IsConfirmed = 0 AND (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + wc + @" " + EmployeeCategory + @"
								AND
								(CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) > CONVERT(DATE,'" + hrDate + @"')
									AND
									CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) <=  CONVERT(DATE,(DATEADD(DAY, 7,'" + hrDate + @"')))  AND EMP.GroupID  = '" + companyGroupId + @"'
								)
								) probationNext7Days,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"'))
								AND RSG.ApprovalStatus = 'APPROVED' AND EMP.GroupID  = '" + companyGroupId + @"'  " + wc + @" " + EmployeeCategory + @"
								) separatedToday,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and Emp.PlantId = hs.PlantID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
										" + join + @"
								WHERE (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @" AND
								(CONVERT(DATE,RSG.ApprovedEffectiveDate) > CONVERT(DATE,'" + hrDate + @"')
									AND
									CONVERT(DATE,RSG.ApprovedEffectiveDate) <=  CONVERT(DATE,(DATEADD(DAY, 7,'" + hrDate + @"')))  AND EMP.GroupID  = '" + companyGroupId + @"'
								)

								AND RSG.ApprovalStatus = 'APPROVED' " + wc + @"
								) separatedNext7Days,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
										" + join + @"
								WHERE (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @"AND CONVERT(DATE,RSG.AddedDate) = CONVERT(DATE,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'
								AND RSG.ApprovalStatus = 'Pending' " + wc + @"
								) todayResignationApply,

								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @" AND EMP.GroupID  = '" + companyGroupId + @"'
								AND RSG.ApprovalStatus = 'Pending' " + wc + @"
								) resignationApprovalPending,
								(
								SELECT COUNT(*) from 
								 EmployeeInformation EMP 
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId

										" + join + @"
								WHERE (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + " AND CONVERT(DATE,Tem.NextDueDate) < CONVERT(DATE,'" + hrDate + @"')  AND EMP.GroupID  = '" + companyGroupId + @"'  " + wc + @"
								) incrementOverDue,
								(
								SELECT COUNT(*) from 
								 EmployeeInformation EMP 
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId
										" + join + @"
								WHERE  (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @" AND CONVERT(DATE,Tem.NextDueDate) = CONVERT(DATE,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'  " + wc + @"
								) incrementToday,
								(
								SELECT COUNT(*) from 
								 EmployeeInformation EMP 
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId

										" + join + @"
								WHERE  (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @" AND	EMP.GroupID  = '" + companyGroupId + @"'  AND

								( CONVERT(DATE,Tem.NextDueDate) BETWEEN CONVERT(DATE,'" + hrDate + @"')
									AND
									   CONVERT(DATE,(DATEADD(DAY, 7,'" + hrDate + @"')))
								) " + wc + @"
								) incrementNext7Days
,
								(
								SELECT COUNT(*) from 
								 EmployeeInformation EMP 
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId

										" + join + @"
								WHERE  (EMP.DOJ<='" + hrDate + @"' AND (EMP.DOS IS NULL OR EMP.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @" AND	EMP.GroupID  = '" + companyGroupId + @"'  AND

								( CONVERT(DATE,Tem.NextDueDate) BETWEEN CONVERT(DATE,'" + hrDate + @"')
									AND
									  CONVERT(DATE,(DATEADD(DAY, 30,'" + hrDate + @"')))
								) " + wc + @"
								) incrementNext30Days";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> HRLongAbsentismDefault(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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

            try
            {
                var cList = string.Empty;
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;

                var sql = @"WITH CTE
									AS (
										SELECT *
											 -- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE
														 WHEN DayStatus = 'P' THEN 1
														 WHEN DayStatus = 'L' THEN 1
														 WHEN DayStatus = 'WL' THEN 1
														 WHEN DayStatus = 'HP' THEN 1
														 WHEN DayStatus = 'LVP' THEN 1
														 WHEN DayStatus = 'WP' THEN 1
														 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											 -- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)
									SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,COUNT(*) AS absentDays,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
									,Plant.UserName Plant ,Division.UserName Division ,SubDivision.UserName SubDivision ,Unit.UserName Unit ,Department.UserName Department ,Section.UserName Section ,SubSection.UserName SubSection 
									FROM cte

									LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
                                       LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
                                    LEFT JOIN [ORG].[SubDivision] ON SubDivision.Id = E.SubDivisionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                                    LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = Po.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = Po.SubSectionId

									WHERE fv = 'A' -- current status = 'A'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV') AND (EI.EmployeeStatus = 'Active' OR DOS >= '" + hrDate + @"') AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'  
									GROUP BY cte.EmpSystemID,EI.DOJ,EI.EmployeeName,EI.EmployeeCode,Plant.UserName,Division.UserName,SubDivision.UserName,Unit.UserName,Department.UserName,Section.UserName,SubSection.UserName
									HAVING
										-- at least three days absent
										COUNT(*) >= 10";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> HRLongAbsentismDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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

            try
            {
                var cList = string.Empty;
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;

                seq += 1;
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                var WhereClausebuilder = new System.Text.StringBuilder();
                WhereClausebuilder.Append(wc);
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")//For Line
                            {
                                cListId = "," + item.StandardName + "Defination.Id";
                                cList = "," + item.StandardName + "Defination.UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination ON " + item.StandardName + "Defination].Id = MPB." + item.StandardName + "Defination]Id\n");
                            }
                            if (item.RType == "ZA")//For Line
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n");
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            WhereClausebuilder.Append("  AND C.Id ='" + item.Id + "'");
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                WhereClausebuilder.Append(" AND " + item.StandardName + ".Id='" + item.Text + "'");
                            }
                        }
                    }
                }
                wc = WhereClausebuilder.ToString();
                join = joinBuilder.ToString();
                var sql = @"SELECT
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE EMP.IsConfirmed = 0 AND EmployeeStatus = 'Active' " + wc + @" " + EmployeeCategory + @"
								AND convert(date,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) < convert(date,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'
								) probationOverDue,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE EMP.IsConfirmed = 0 AND EmployeeStatus = 'Active' " + wc + @" " + EmployeeCategory + @"
								AND CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) = CONVERT(DATE,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'
								) probationToday,

								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE EMP.IsConfirmed = 0 AND EmployeeStatus = 'Active' " + wc + @" " + EmployeeCategory + @"
								AND
								(CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) > CONVERT(DATE,'" + hrDate + @"')
									AND
									CONVERT(DATE,(EMP.DOJ+(CASE WHEN EMP.DOCIsDay=1 THEN EMP.DOCDay	ELSE EMP.DOCMonth*30 END))) <=  CONVERT(DATE,(DATEADD(DAY, 7,'" + hrDate + @"')))  AND EMP.GroupID  = '" + companyGroupId + @"'
								)
								) probationNext7Days,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE EMP.EmployeeStatus = 'Separated'and CONVERT(DATE,EMP.DOSDate) = CONVERT(DATE,'" + hrDate + @"')
								AND RSG.ApprovalStatus = 'APPROVED' AND EMP.GroupID  = '" + companyGroupId + @"'  " + wc + @" " + EmployeeCategory + @"
								) separatedToday,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
										" + join + @"
								WHERE EMP.EmployeeStatus = 'Active' " + EmployeeCategory + @" AND
								(CONVERT(DATE,RSG.ApprovedEffectiveDate) > CONVERT(DATE,'" + hrDate + @"')
									AND
									CONVERT(DATE,RSG.ApprovedEffectiveDate) <=  CONVERT(DATE,(DATEADD(DAY, 7,'" + hrDate + @"')))  AND EMP.GroupID  = '" + companyGroupId + @"'
								)

								AND RSG.ApprovalStatus = 'APPROVED' " + wc + @"
								) separatedNext7Days,
								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
										" + join + @"
								WHERE EMP.EmployeeStatus = 'Active'" + EmployeeCategory + @"AND CONVERT(DATE,RSG.AddedDate) = CONVERT(DATE,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'
								AND RSG.ApprovalStatus = 'Pending' " + wc + @"
								) todayResignationApply,

								(
								SELECT COUNT(*) from EmployeeInformation EMP
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN TRN.Resignation RSG ON RSG.EmployeeId=EMP.SystemId

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										" + join + @"
								WHERE EMP.EmployeeStatus = 'Active' " + EmployeeCategory + @" AND EMP.GroupID  = '" + companyGroupId + @"'
								AND RSG.ApprovalStatus = 'Pending' " + wc + @"
								) resignationApprovalPending,
								(
								SELECT COUNT(*) from SalaryIncrementNextDueDate SIND
								LEFT OUTER JOIN EmployeeInformation EMP ON EMP.SystemId = SIND.EmpSystemId
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId

										" + join + @"
								WHERE  EmployeeStatus = 'Active' " + EmployeeCategory + " AND CONVERT(DATE,Tem.NextDueDate) < CONVERT(DATE,'" + hrDate + @"')  AND EMP.GroupID  = '" + companyGroupId + @"'  " + wc + @"
								) incrementOverDue,
								(
								SELECT COUNT(*) from SalaryIncrementNextDueDate SIND
								LEFT JOIN EmployeeInformation EMP ON EMP.SystemId = SIND.EmpSystemId

								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId
										" + join + @"
								WHERE  EmployeeStatus = 'Active' " + EmployeeCategory + @" AND CONVERT(DATE,Tem.NextDueDate) = CONVERT(DATE,'" + hrDate + @"') AND EMP.GroupID  = '" + companyGroupId + @"'  " + wc + @"
								) incrementToday,
								(
								SELECT COUNT(*) from SalaryIncrementNextDueDate SIND
								LEFT OUTER JOIN EmployeeInformation EMP ON EMP.SystemId = SIND.EmpSystemId
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID

								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id and hs.PlantID = emp.PlantId

                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = EMP.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = EMP.SystemId

										" + join + @"
								WHERE  EmployeeStatus = 'Active' " + EmployeeCategory + @" AND	EMP.GroupID  = '" + companyGroupId + @"'  AND

								( CONVERT(DATE,Tem.NextDueDate) > CONVERT(DATE,'" + hrDate + @"')
									AND
									 CONVERT(DATE,Tem.NextDueDate) <=  CONVERT(DATE,(DATEADD(DAY, 7,'" + hrDate + @"')))
								) " + wc + @"
								) incrementNext7Days";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #region Consecutive Absent And Late Status

        public IEnumerable<object> ConsecutiveAbsentStats(string hrDate, string EmplyeeTypeOrCategoryId)
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
            try
            {
                var sql = @"WITH CTE
									AS (
										SELECT *
											-- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE
													 WHEN DayStatus = 'P' THEN 1
													 WHEN DayStatus = 'L' THEN 1
													 WHEN DayStatus = 'WL' THEN 1
													 WHEN DayStatus = 'HP' THEN 1
													 WHEN DayStatus = 'LVP' THEN 1
													 WHEN DayStatus = 'WP' THEN 1
													 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											-- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)
									SELECT EmpSystemID
										,COUNT(*) AS absentDays
									FROM cte
										LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = CTE.EmpSystemID

								 LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									WHERE fv = 'A' -- current status = 'A'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV') " + EmployeeCategory + @" AND (EI.DOJ<='" + hrDate + @"' AND (EI.DOS is null or EI.DOS >= '" + hrDate + @"')) AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'
									GROUP BY EmpSystemID
									HAVING
										-- at least three days absent
										COUNT(*) >= 3";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> ConsecutiveLateStats(string hrDate, string EmplyeeTypeOrCategoryId)
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
            try
            {
                var sql = @"WITH CTE
									AS (
										SELECT *
											-- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE

													WHEN DayStatus = 'P' THEN 1
													 WHEN DayStatus = 'HP' THEN 1
													 WHEN DayStatus = 'LVP' THEN 1
													 WHEN DayStatus = 'WP' THEN 1
													 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											-- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)
									SELECT EmpSystemID
										,COUNT(*) AS absentDays
									FROM cte
LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
	 LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									WHERE fv = 'L' -- current status = 'L'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV','A') " + EmployeeCategory + @" AND EI.EmployeeStatus = 'Active' AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'
									GROUP BY EmpSystemID
									HAVING
										-- at least three days absent
										COUNT(*) >= 3";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Consecutive Absent And Late Status

        #region Dynamic Consecutive Absent and Late Status

        public IEnumerable<object> ConsecutiveAbsentStatsDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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
            try
            {
                var cList = string.Empty;
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;

                seq += 1;
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                var WhereClauseBuilder = new System.Text.StringBuilder();
                WhereClauseBuilder.Append(wc);
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }

                            if (item.RType == "ZA")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")
                            {
                                cListId = "," + item.StandardName + "Defination.SystemId";
                                cList = "," + item.StandardName + "Defination.UserName"; cListId = "," + item.StandardName + "Defination.SystemId";
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n");
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND C.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    WhereClauseBuilder.Append(" AND " + item.StandardName + "Defination.SystemId='" + item.Text + "'");

                                }
                                else
                                {
                                    WhereClauseBuilder.Append(" AND " + item.StandardName + ".Id='" + item.Text + "'");

                                }
                            }
                        }
                    }
                }
                wc = WhereClauseBuilder.ToString();
                join = joinBuilder.ToString();
                var sql = @"WITH CTE
									AS (
										SELECT *
											-- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE
													 WHEN DayStatus = 'P' THEN 1
													 WHEN DayStatus = 'L' THEN 1
													 WHEN DayStatus = 'WL' THEN 1
													 WHEN DayStatus = 'HP' THEN 1
													 WHEN DayStatus = 'LVP' THEN 1
													 WHEN DayStatus = 'WP' THEN 1
													 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											-- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)
									SELECT EmpSystemID
										,COUNT(*) AS absentDays
									FROM cte LEFT OUTER JOIN EmployeeInformation EMP ON EMP.SystemId = cte.EmpSystemId
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id
								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

								 LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

											" + join + @"
									WHERE fv = 'A' -- current status = 'A'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV') " + EmployeeCategory + @" AND  EMP.EmployeeStatus = 'Active' AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'  AND EMP.GroupID  = '" + companyGroupId + @"'  " + wc + @"
									GROUP BY EmpSystemID
									HAVING
										-- at least three days absent
										COUNT(*) >= 3";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> ConsecutiveLateStatsDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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
            try
            {
                var cList = string.Empty;
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;

                seq += 1;
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                var WhereClauseBuilder = new System.Text.StringBuilder();
                WhereClauseBuilder.Append(wc);
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")//For Line
                            {
                                cListId = "," + item.StandardName + "Defination.SystemId";
                                cList = "," + item.StandardName + "Defination.UserName"; cListId = "," + item.StandardName + "Defination.SystemId";
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n");
                            }
                            if (item.RType == "ZA")//For Line
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n");
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND C.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")//For Line
                                {
                                    WhereClauseBuilder.Append(" AND " + item.StandardName + "Defination.SystemId = '" + item.Text + "'");

                                }
                                else
                                {
                                    WhereClauseBuilder.Append(" AND " + item.StandardName + ".Id='" + item.Text + "'");

                                }
                            }
                        }
                    }
                }
                wc = WhereClauseBuilder.ToString();
                join = joinBuilder.ToString();
                var sql = @"WITH CTE
									AS (
										SELECT *
											-- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE
													 WHEN DayStatus = 'P' THEN 1
													 WHEN DayStatus = 'HP' THEN 1
													 WHEN DayStatus = 'LVP' THEN 1
													 WHEN DayStatus = 'WP' THEN 1
													 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											-- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)
									SELECT EmpSystemID
										,COUNT(*) AS absentDays
									FROM cte LEFT OUTER JOIN EmployeeInformation EMP ON EMP.SystemId = cte.EmpSystemId
								LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = EMP.GroupID
								LEFT OUTER JOIN PlantWiseHRMSSetting HS ON HS.GroupID = CG.Id
								LEFT OUTER JOIN ORG.Company C ON C.Id = EMP.CompanyId
								LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = EMP.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

								 LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

											" + join + @"
									WHERE fv = 'L' -- current status = 'L'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV','A') " + EmployeeCategory + @" AND  EMP.EmployeeStatus = 'Active' AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'   AND EMP.GroupID  = '" + companyGroupId + @"'  " + wc + @"
									GROUP BY EmpSystemID
									HAVING
										-- at least three days Late
										COUNT(*) >= 3";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Dynamic Consecutive Absent and Late Status

        public IEnumerable<object> JoiningStatusDaily(string companyGroupId, string companyId, string hrDate, string EmplyeeTypeOrCategoryId)
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
            try
            {
                var sql = @"DECLARE @startDate   DATE;
					         DECLARE @endDate     DATE;
					         SELECT  @startDate = CONVERT(DATETIME, '" + hrDate + @"') - 15;
					         SELECT  @endDate =   '" + hrDate + @"';

					         WITH dateRange AS
					         (
					           SELECT DOS = DATEADD(dd, 1, @startDate)
					           WHERE DATEADD(dd, 1, @startDate) <= @endDate
					           UNION ALL
					           SELECT DATEADD(dd, 1, DOS)
					           FROM dateRange
					           WHERE DATEADD(dd, 1, DOS) <= @endDate
					         )
					         SELECT DISTINCT dr.DOS,REPLACE(CONVERT(varchar(11),dr.DOS,6),' ','-') DO,ISNULL(s.TEDOS,0) TEDOS,ISNULL(j.TEDOJ,0) TEDOJ
					         FROM dateRange dr
                             LEFT OUTER JOIN
							  (
							      SELECT '' D,REPLACE(CONVERT(VARCHAR(11),DOS,6),' ','-') DO, COUNT(SystemId) TEDOS,''TEDOJ FROM EmployeeInformation EMP
									 LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
							      WHERE CONVERT(DATE,DOS) BETWEEN CONVERT(DATE,@startDate ) AND CONVERT(date,'" + hrDate + @"') " + EmployeeCategory + @"  GROUP BY DOS --ORDER BY DOS ASC
							  ) S ON S.DO = DR.DOS
							    LEFT OUTER JOIN
							  (
							      SELECT '' D,REPLACE(CONVERT(VARCHAR(11),DOJ,6),' ','-') DO,''TEDOS, COUNT(SystemId) TEDOJ FROM EmployeeInformation EMP
                                   LEFT JOIN [HKP].Designation GDes ON GDes.Id = EMP.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EMP.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
							      WHERE CONVERT(DATE,DOJ) BETWEEN CONVERT(DATE,@startDate ) AND CONVERT(date,'" + hrDate + @"') " + EmployeeCategory + @"  GROUP BY DOJ
							  ) J ON J.DO = DR.DOS

							order by DOS";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> DynamicJoiningOrSeparationStatusDaily(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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

            try
            {
                var cList = string.Empty;
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;
                seq += 1;
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                var WhereClausebuilder = new System.Text.StringBuilder();
                WhereClausebuilder.Append(wc);
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")//For Line
                            {
                                cListId = "," + item.StandardName + "Defination.SystemID";
                                cList = "," + item.StandardName + "Defination.UserName"; cListId = "," + item.StandardName + "Defination.SystemID";
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemID = MPB." + item.StandardName + "DefinationId\n");
                            }
                            if (item.RType == "ZA")//For Line
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n");
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            WhereClausebuilder.Append("  AND E.CompanyId ='" + item.Id + "'");
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {

                                    WhereClausebuilder.Append(" AND " + item.StandardName + "Defination.SystemID='" + item.Text + "'");
                                }
                                else
                                {
                                    WhereClausebuilder.Append(" AND " + item.StandardName + ".Id='" + item.Text + "'");

                                }
                            }
                        }
                    }
                }
                wc = WhereClausebuilder.ToString();
                join = joinBuilder.ToString();
                var sql = @"DECLARE @startDate   DATE;
					         DECLARE @endDate     DATE;
					         SELECT  @startDate = CONVERT(DATETIME, '" + hrDate + @"') - 15;
					         SELECT  @endDate =   '" + hrDate + @"';
					         WITH dateRange AS
					         (
					           SELECT DOS = DATEADD(dd, 1, @startDate)
					           WHERE DATEADD(dd, 1, @startDate) <= @endDate
					           UNION ALL
					           SELECT DATEADD(dd, 1, DOS)
					           FROM dateRange
					           WHERE DATEADD(dd, 1, DOS) <= @endDate
					         )
					         SELECT DISTINCT dr.DOS,REPLACE(CONVERT(varchar(11),dr.DOS,6),' ','-') DO,ISNULL(s.TEDOS,0) TEDOS,ISNULL(j.TEDOJ,0) TEDOJ
					         FROM dateRange dr
                             LEFT OUTER JOIN
							  (
							      SELECT '' D,REPLACE(CONVERT(VARCHAR(11),DOS,6),' ','-') DO, COUNT(e.SystemId) TEDOS,''TEDOJ FROM EmployeeInformation E
									LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON MPB.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
								 LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
										" + join + @"
							      WHERE CONVERT(DATE,DOS) BETWEEN CONVERT(DATE,@startDate ) AND CONVERT(date,'" + hrDate + @"') " + EmployeeCategory + @"
									AND e.GroupID = '" + companyGroupId + @"' " + wc + @"
									GROUP BY DOS --ORDER BY DOS ASC
							  ) S ON S.DO = DR.DOS
							    LEFT OUTER JOIN
							  (
							      SELECT '' D,REPLACE(CONVERT(VARCHAR(11),DOJ,6),' ','-') DO,''TEDOS, COUNT(e.SystemId) TEDOJ FROM EmployeeInformation E
										LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON MPB.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
										" + join + @"
							      WHERE CONVERT(DATE,DOJ) BETWEEN CONVERT(DATE,@startDate ) AND CONVERT(date,'" + hrDate + @"')
									AND e.GroupID = '" + companyGroupId + @"' " + wc + @" " + EmployeeCategory + @"
										GROUP BY DOJ
							  ) J ON J.DO = DR.DOS

							order by DOS";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> AbsentismStatusDaily(string companyGroupId, string companyId, string hrDate, string EmplyeeTypeOrCategoryId)
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
            try
            {
                var sql = @"  DECLARE @startDate   DATE;
					         DECLARE @endDate     DATE;
					         SELECT  @startDate = CONVERT(DATETIME, '" + hrDate + @"') - 30;
					         SELECT  @endDate =   '" + hrDate + @"';

					         WITH dateRange AS
					         (
					           SELECT DOS = DATEADD(dd, 1, @startDate)
					           WHERE DATEADD(dd, 1, @startDate) <= @endDate
					           UNION ALL
					           SELECT DATEADD(dd, 1, DOS)
					           FROM dateRange
					           WHERE DATEADD(dd, 1, DOS) <= @endDate
					         )
					         SELECT DISTINCT dr.DOS,REPLACE(CONVERT(varchar(11),dr.DOS,6),' ','-') WorkDate,ISNULL(s.totalAbsent,0) totalAbsent
					         FROM dateRange dr
							 LEFT JOIN
							 (
                                	SELECT '' D, COUNT(EmpSystemID) totalAbsent, Format(WorkDate,'dd-MMM-yyyy') WorkDate FROM AttdnProcessData APD

                                LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = APD.EmpSystemID
								LEFT JOIN [HKP].LegalDesignation GDes ON GDes.Id = EEI.LegalDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EEI.LegalDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                
                                        LEFT JOIN DayType DT ON dt.DayType = APD.DayStatus
                                         WHERE CONVERT(DATE, APD.WorkDate)
                                         BETWEEN CONVERT(datetime, '" + hrDate + @"') - 30 AND CONVERT(DATE, '" + hrDate + @"')   AND dt.Category = 'Absent' " + EmployeeCategory + @"
                                         									 Group By WorkDate	 ) S on DOS = WorkDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> DynamicAbsentismStatusDaily(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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

            try
            {
                var cList = string.Empty;
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;
                seq += 1;
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                var WhereClausebuilder = new System.Text.StringBuilder();
                WhereClausebuilder.Append(wc);
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "ZA")//For Line
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")//For Line
                            {
                                cListId = "," + item.StandardName + "Defination.SystemId";
                                cList = "," + item.StandardName + "Defination.UserName"; cListId = "," + item.StandardName + "Defination.SystemId";
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n");
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            WhereClausebuilder.Append("  AND E.CompanyId ='" + item.Id + "'");
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    WhereClausebuilder.Append(" AND " + item.StandardName + "Defination.SystemId='" + item.Text + "'");

                                }
                                else
                                {
                                    WhereClausebuilder.Append(" AND " + item.StandardName + ".Id='" + item.Text + "'");

                                }
                            }
                        }
                    }
                }
                wc = WhereClausebuilder.ToString();
                join = joinBuilder.ToString();
                var sql = @"DECLARE @startDate   DATE;
					         DECLARE @endDate     DATE;
					         SELECT  @startDate = CONVERT(DATETIME, '" + hrDate + @"') - 30;
					         SELECT  @endDate =   '" + hrDate + @"';

					         WITH dateRange AS
					         (
					           SELECT DOS = DATEADD(dd, 1, @startDate)
					           WHERE DATEADD(dd, 1, @startDate) <= @endDate
					           UNION ALL
					           SELECT DATEADD(dd, 1, DOS)
					           FROM dateRange
					           WHERE DATEADD(dd, 1, DOS) <= @endDate
					         )
					         SELECT DISTINCT dr.DOS,REPLACE(CONVERT(varchar(11),dr.DOS,6),' ','-') WorkDate,ISNULL(s.totalAbsent,0) totalAbsent
					         FROM dateRange dr
							 LEFT JOIN
							 (
                                	SELECT '' D, COUNT(EmpSystemID) totalAbsent, Format(WorkDate,'dd-MMM-yyyy') WorkDate FROM AttdnProcessData APD
                                LEFT JOIN EmployeeInformation E ON E.SystemId = APD.EmpSystemID
								LEFT JOIN [HKP].LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.LegalDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Position PO ON MPB.PositionId=PO.Id
                                LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id                            
                                        " + join + @"
                                        LEFT JOIN DayType DT ON dt.DayType = APD.DayStatus
                                         WHERE CONVERT(DATE, APD.WorkDate)
                                         BETWEEN CONVERT(datetime, '" + hrDate + @"') - 30 AND CONVERT(DATE, '" + hrDate + @"')   AND dt.Category = 'Absent' " + wc + @" " + EmployeeCategory + @"
                                         GROUP BY WorkDate ) S ON DOS = WorkDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Attendance Status
        //Group Level Attendance
        public IEnumerable<object> X_DefaultAttnStatus(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId, string PODirectIndirectStatus)
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
            string DirInDirStatus = "";

            if (PODirectIndirectStatus == "Default")
            {
                DirInDirStatus = "";
            }
            else if (PODirectIndirectStatus == "Direct")
            {
                DirInDirStatus = "and PO.IsDirect = 1";
            }
            else if (PODirectIndirectStatus == "Indirect")
            {
                DirInDirStatus = "and PO.IsDirect = 0";
            }
            try
            {
                var sql = @"SELECT DISTINCT OnRoleEmployee.CompanyId CompanyId,OnRoleEmployee.CompanyName ColumnName,OnRoleEmployee.GroupName GroupName,OnRoleEmployee.CompanyGroupId CompanyGroupId
								
                                , Case when  ISNULL(OnRoleEmployee.IsDirect,0) = 0 then 'Indirect' else 'Direct' end AS  IsDirect
                                ,ISNULL(OnRoleEmployee.totalEmployee,0) OnRoleEmployee
								,ISNULL(PresentEmployee.totalPresentEmployee,0) totalPresentEmployee
								,ISNULL(AbsentEmployee.totalAbsentEmployee,0) totalAbsentEmployee
								,ISNULL(LateEmployee.totalLateEmployee,0) totalLateEmployee
								,ISNULL(LeaveEmployee.totalLeaveEmployee,0) totalLeaveEmployee
								,ISNULL(WeekOffEmployee.totalWeekoffEmployee,'')totalWeekoffEmployee
								,ISNULL(ShiftNotAssignedEmployee.totalShiftNotAssignedEmployee,0) ShiftNotAssignedEmployee
								,ISNULL(AttdnNotProcessedToday.totalAttdnNotProcessedToday,0) totalAttdnNotProcessedToday
								,ISNULL(ShiftNotAssignAsofToday.totalShiftNotAssignAsofToday,0) totalShiftNotAssignAsofToday
                                ,ISNULL(LONGABSENTISM.totalEmployee,0) totalLongAbsentismEmployee
                            FROM
						   (SELECT ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalEmployee,C.UserName,cg.Id CompanyGroupId,c.Id CompanyId,c.UserName CompanyName,cg.UserName GroupName    FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											LEFT OUTER JOIN EmployeeInformation
											E ON e.GroupID = CG.Id and c.Id=E.CompanyId
									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                         LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
											where
												 GroupID = '" + companyGroupId + @"'   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @" " + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')

                                                GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,PO.IsDirect) OnRoleEmployee
                                                LEFT OUTER JOIN
                                  (SELECT ISNULL(PO.IsDirect,0) IsDirect,  COUNT(E.SystemId) totalPresentEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName


                                    FROM ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                               SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
                                   LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category = 'Present' AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id and c.Id = E.CompanyId

                                         LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE
                                        GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) " + EmployeeCategory + @" " + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')
                                            GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName,PO.IsDirect)
									PresentEmployee
                                    ON OnRoleEmployee.CompanyGroupId = PresentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = PresentEmployee.CompanyId AND OnRoleEmployee.IsDirect = PresentEmployee.IsDirect


                                    LEFT OUTER JOIN
                                    ----------------------------
                                     (SELECT ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalAbsentEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName


                                    FROM  ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN(--*
                                SELECT * FROM EmployeeInformation

                                WHERE SystemId IN(--**
                                SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD

                                    LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category = 'Absent' AND  CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
                                )-- * *

                                )-- *
                                E ON e.GroupID = CG.Id and c.Id = E.CompanyId

                                         LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                                 LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                        WHERE

                                                 GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) " + EmployeeCategory + @" " + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')

                                    GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect )

                                    AbsentEmployee ON OnRoleEmployee.CompanyGroupId = AbsentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = AbsentEmployee.CompanyId AND OnRoleEmployee.IsDirect = AbsentEmployee.IsDirect

                                    LEFT OUTER JOIN
                                    (SELECT ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalLateEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName

                                    FROM  ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                       LEFT OUTER JOIN(--*
                                SELECT * FROM EmployeeInformation

                                WHERE SystemId IN(--**
                                SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD

                                    LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category = 'Late' AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
                                )-- * *

                                )-- *
                                E ON e.GroupID = CG.Id and c.Id = E.CompanyId

                                        LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                        LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                                                        WHERE

                                                 GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) " + EmployeeCategory + @" " + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')

                                    GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect)

                                    LateEmployee on OnRoleEmployee.CompanyGroupId = LateEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LateEmployee.CompanyId AND OnRoleEmployee.IsDirect = LateEmployee.IsDirect

                                    LEFT OUTER JOIN
                                    (SELECT ISNULL(PO.IsDirect,0) IsDirect,COUNT(E.SystemId) totalWeekoffEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName


                                    FROM  ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN(--*
                                    SELECT * FROM EmployeeInformation

                                    WHERE SystemId IN(--**
                                    SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD

                                    LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category IN('Holiday', 'Weekend') AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
                                    )-- * *

                                    )-- *
                                    E ON e.GroupID = CG.Id and c.Id = E.CompanyId

                                        LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                        WHERE GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) " + EmployeeCategory + @" " + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName, PO.IsDirect)

                                    WeekOffEmployee ON OnRoleEmployee.CompanyGroupId = WeekOffEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = WeekOffEmployee.CompanyId AND OnRoleEmployee.IsDirect = WeekOffEmployee.IsDirect


                                    LEFT OUTER JOIN
                                    (SELECT ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalLeaveEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName


                                    FROM  ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN(--*
                                SELECT * FROM EmployeeInformation

                                WHERE SystemId IN(--**
                                SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD

                                    LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category = 'Leave' AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
                                )-- * *

                                )-- *
                                E ON e.GroupID = CG.Id and c.Id = E.CompanyId

                                        LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    WHERE

                                       GroupID = '" + companyGroupId + @"' AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @" " + DirInDirStatus + @"

                                    GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect)

                                LeaveEmployee on OnRoleEmployee.CompanyGroupId = LeaveEmployee.CompanyGroupId

                            AND OnRoleEmployee.CompanyId = LeaveEmployee.CompanyId AND OnRoleEmployee.IsDirect = LeaveEmployee.IsDirect

                                        LEFT OUTER JOIN
                                    (SELECT ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalOthersEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName


                                    FROM  ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                    LEFT OUTER JOIN(--*
                                    SELECT * FROM EmployeeInformation

                                    WHERE SystemId NOT IN(--**
                                    SELECT DISTINCT EmpSystemID FROM AttdnProcessData

                                    WHERE  CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
                                    )-- * *

                                    )-- *
                                    E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                        LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    WHERE

                                                 GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) " + EmployeeCategory + @"" + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')


                                    GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect)

                                    OthersEmployee ON OnRoleEmployee.CompanyGroupId = OthersEmployee.CompanyGroupId

                                    AND OnRoleEmployee.CompanyId = OthersEmployee.CompanyId AND OnRoleEmployee.IsDirect = OthersEmployee.IsDirect


                                    LEFT OUTER JOIN
                                    (SELECT ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalOthersShiftNotAssignedEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName

                                    FROM  ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                    LEFT OUTER JOIN(--*
                                    SELECT * FROM EmployeeInformation

                                    WHERE SystemId NOT IN(--**
                                    select EmpSystemId from EmployeeShiftAssign

                                    )-- * *

                                    )-- *
                                    E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                        LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                            WHERE

                                                 GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) " + EmployeeCategory + @" " + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')

                                    GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect)

                                    OthersShiftNotAssignedEmployee ON OnRoleEmployee.CompanyGroupId = OthersShiftNotAssignedEmployee.CompanyGroupId

                                        AND OnRoleEmployee.CompanyId = OthersShiftNotAssignedEmployee.CompanyId AND OnRoleEmployee.IsDirect = OthersShiftNotAssignedEmployee.IsDirect

                                    LEFT OUTER JOIN
                                (SELECT  ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalShiftNotAssignedEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                C.Id AS CompanyId, C.UserName CompanyName


                                FROM  ORG.CompanyGroup CG

                                LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                LEFT OUTER JOIN(--*
                                SELECT * FROM EmployeeInformation

                                WHERE SystemId NOT IN(--**
                                SELECT DISTINCT EmpSystemID FROM EmployeeShiftAssign
                                )-- * *

                                )-- *
                                E ON e.GroupID = CG.Id and c.Id = E.CompanyId

                                    LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) " + EmployeeCategory + @" " + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')


                                GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect)

                                    ShiftNotAssignedEmployee ON OnRoleEmployee.CompanyGroupId = ShiftNotAssignedEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = ShiftNotAssignedEmployee.CompanyId AND OnRoleEmployee.IsDirect = ShiftNotAssignedEmployee.IsDirect


                                LEFT OUTER JOIN
                                    (
                                     SELECT  ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemID) totalAttdnNotProcessedToday, cg.Id CompanyGroupId, cg.UserName GroupName,
                                            C.Id AS CompanyId, C.UserName  UId

                                        FROM  ORG.CompanyGroup CG

                                            LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                            INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id = E.CompanyId

                                            Inner JOIN(--*
                                                               SELECT TOP 1 WITH TIES *
                                                                FROM EmployeeShiftAssign

                                                                WHERE EffectiveDate <= GETDATE() and

                                                                EmpSystemID NOT IN(--**
                                                                                            SELECT DISTINCT EmpSystemID FROM AttdnProcessData

                                                                                            WHERE  CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
                                                                                    )

                                                                ORDER BY ROW_NUMBER() OVER(PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC)
                                                              )-- *
                                                        ESA

                                            ON E.SystemId = ESA.EmpSystemID


                                        LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                        WHERE

                                                 GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) " + EmployeeCategory + @" " + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')


                                    GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName, PO.IsDirect) AttdnNotProcessedToday ON OnRoleEmployee.CompanyGroupId = AttdnNotProcessedToday.CompanyGroupId AND OnRoleEmployee.CompanyId = AttdnNotProcessedToday.CompanyId AND OnRoleEmployee.IsDirect = AttdnNotProcessedToday.IsDirect

                                    LEFT OUTER JOIN
                                    (
                                    SELECT ISNULL(PO.IsDirect,0) IsDirect, COUNT(ESA.SystemID) totalShiftNotAssignAsofToday, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName  UId

                                    FROM  ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                    LEFT OUTER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id = E.CompanyId

                                    LEFT OUTER JOIN(--*
                                    SELECT SystemID FROM EmployeeInformation EI

                                    WHERE EI.SystemID NOT IN(--**
                                    SELECT DISTINCT EmpSystemID FROM EmpDateWiseShiftAssign

                                    WHERE  CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
                                    )-- * *

                                    )-- *
                                    ESA

                                    ON E.SystemId = ESA.SystemId


                                    LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    WHERE

                                                 GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) " + EmployeeCategory + @"" + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')

                                    GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect) ShiftNotAssignAsofToday ON OnRoleEmployee.CompanyGroupId = ShiftNotAssignAsofToday.CompanyGroupId AND OnRoleEmployee.CompanyId = ShiftNotAssignAsofToday.CompanyId AND OnRoleEmployee.IsDirect = ShiftNotAssignAsofToday.IsDirect
                                    LEFT JOIN
                                    (SELECT  ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalEmployee, C.UserName, cg.Id CompanyGroupId, c.Id CompanyId, c.UserName CompanyName, cg.UserName GroupName    FROM ORG.CompanyGroup CG

                                            LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                            LEFT OUTER JOIN EmployeeInformation

                                            E ON e.GroupID = CG.Id and c.Id= E.CompanyId

                                    LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                            where
                                                 GroupID = '" + companyGroupId + "'   AND E.EmployeeCurrentStatus = 'LONG ABSENTEEISM' AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + "')) " + EmployeeCategory + @" " + DirInDirStatus + @"
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')

                                                group by C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,po.IsDirect

												) LONGABSENTISM
                                            ON OnRoleEmployee.CompanyGroupId = LONGAbsentism.CompanyGroupId AND OnRoleEmployee.CompanyId = LONGAbsentism.CompanyId AND OnRoleEmployee.IsDirect = LONGAbsentism.IsDirect";
                DataTable dt = _sqlRepository.GetDataTable(sql);

                DataTable dtTemp = dt.Clone();
                if (dt.Rows.Count > 0)
                {
                    dtTemp = dt.AsEnumerable().GroupBy(x => new
                    {
                        CompanyId = x["CompanyId"],
                        ColumnName = x["ColumnName"],
                        GroupName = x["GroupName"],
                        CompanyGroupId = x["CompanyGroupId"],
                        IsDirect = "General"

                    })
                .Select(x =>
                {
                    DataRow row = dt.NewRow();
                    row["IsDirect"] = "General";
                    row["ColumnName"] = x.Key.ColumnName; row["CompanyId"] = x.Key.CompanyId; row["GroupName"] = x.Key.GroupName; row["CompanyGroupId"] = x.Key.CompanyGroupId;
                    row["OnRoleEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OnRoleEmployee"])); row["totalPresentEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalPresentEmployee"]));
                    row["totalAbsentEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalAbsentEmployee"])); row["totalLateEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLateEmployee"]));
                    row["totalLeaveEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLeaveEmployee"])); row["totalWeekoffEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalWeekoffEmployee"]));
                    row["ShiftNotAssignedEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ShiftNotAssignedEmployee"])); row["totalAttdnNotProcessedToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalAttdnNotProcessedToday"]));
                    row["totalShiftNotAssignAsofToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalShiftNotAssignAsofToday"])); row["totalLongAbsentismEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLongAbsentismEmployee"]));
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
        }//End - Group Level Attendance
        public IEnumerable<object> DefaultAttnStatus(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId, string PODirectIndirectStatus)
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

                var sql = @"SELECT IsDirect,CgId CompanyGroupId, GroupName,CompanyId,UserName ColumnName, ISNULL(SUM(TotalNumber),0) ProposedManpowerBudget, ISNULL(SUM(TotalManpower),0) OnRoleEmployee , sum(short) Short,sum(Excess) Excess
								--,ISNULL(SUM(TotalSalary),0) OnRoleSalaryC,ISNULL((SUM(MaxSal)+SUM(MinSal))/2,0) ProposedSalaryC
								,ISNULL(SUM(totalPresentEmployee),0) totalPresentEmployee,ISNULL(SUM(totalAbsentEmployee),0) totalAbsentEmployee
								,ISNULL(SUM(totalLateEmployee),0) totalLateEmployee,ISNULL(SUM(totalLeaveEmployee),0) totalLeaveEmployee
								,ISNULL(SUM(totalWeekoffEmployee),0) totalWeekoffEmployee,ISNULL(SUM(ShiftNotAssignedEmployee),0) ShiftNotAssignedEmployee
                                ,SUM(totalEarlyOutEmployee) totalEarlyOutEmployee, SUM(totalLounchOutEmployee) totalLounchOutEmployee,SUM(totalLateInEmployee) totalLateInEmployee
								,ISNULL(SUM(totalAttdnNotProcessedToday),0) totalAttdnNotProcessedToday,ISNULL(SUM(totalShiftNotAssignAsofToday),0) totalShiftNotAssignAsofToday
								,ISNULL(SUM(totalLongAbsentismEmployee),0) totalLongAbsentismEmployee

                                 FROM (
                                      SELECT 
									 Case when  ISNULL(m.IsDirect,0) = 0 then 'Indirect' else 'Direct' end AS  IsDirect
									 ,m.Id,b.TotalNumber,m.CgId,m.GroupName,m.CompanyId,m.CName as UserName,EmpInfo.TotalSalary
                                     ,EmpInfo.TotalManpower,(ISNULL(Sal.MaximumSalary,0)) MaxSal,(ISNULL(Sal.MinimumSalary,0)) MinSal
                                     , Short = CASE WHEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) > 0
                                                THEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) ELSE 0 END
                                     , Excess = CASE WHEN isNull(TotalManpower,0) - ISNULL(TotalNumber,0) > 0
                                                THEN ISNULL(TotalManpower,0) - ISNULL(TotalNumber,0) ELSE 0 END

                                ,ISNULL(totalEarlyOutEmployee,0) totalEarlyOutEmployee
                                ,ISNULL(totalLounchOutEmployee,0) totalLounchOutEmployee
                                ,ISNULL(totalLateInEmployee,0) totalLateInEmployee


									,ISNULL(PresentEmployee.totalPresentEmployee,0) totalPresentEmployee
								,ISNULL(AbsentEmployee.totalAbsentEmployee,0) totalAbsentEmployee
								,ISNULL(LateEmployee.totalLateEmployee,0) totalLateEmployee
								,ISNULL(LeaveEmployee.totalLeaveEmployee,0) totalLeaveEmployee
								,ISNULL(WeekOffEmployee.totalWeekoffEmployee,'')totalWeekoffEmployee
								,ISNULL(ShiftNotAssignedEmployee.totalShiftNotAssignedEmployee,0) ShiftNotAssignedEmployee
								,ISNULL(AttdnNotProcessedToday.totalAttdnNotProcessedToday,0) totalAttdnNotProcessedToday
								,ISNULL(ShiftNotAssignAsofToday.totalShiftNotAssignAsofToday,0) totalShiftNotAssignAsofToday
                                ,ISNULL(LONGABSENTISM.totalEmployee,0) totalLongAbsentismEmployee
                                      FROM
                                          --------------------1 budgetCode from [MST].[ManpowerBudget]--------------------------------------
                                            (SELECT MB.Code,MB.Id,Cg.Id as CgId,Cg.UserName as GroupName, c.Id as CompanyId, c.UserName as CName , ISNULL(po.IsDirect,0) IsDirect FROM [MST].[ManpowerBudget]  MB
                                              LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                               LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                             LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId

                                             LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId

								              LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
								              LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
								              LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                               WHERE Cg.Id = '" + companyGroupId + @"' " + dStatus + @" " + EmployeeCategory + @" AND MB.Active = 1
                                            )  M
                                           -----------------------2. EmployeeInformation from [dbo].[EmployeeInformation]--------------------------------
                                            LEFT OUTER JOIN
                                             (SELECT COUNT(SystemID) TotalManpower,BudgetCode,GroupID,c.Id cid,SUM(TotalSalary) TotalSalary
                                               FROM [dbo].[EmployeeInformation]  em
								                LEFT outer join [MST].[ManpowerBudget] AS MB  ON  MB.Id = em.BudgetCode
                                                 LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                                              LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                                              LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                              LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                                 LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
									            LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
									            LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
									            LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
									            LEFT JOIN [ORG].[Section] ON Section.Id = Po.SectionId
									            LEFT JOIN [ORG].[Subsection] ON Subsection.Id = Po.SubsectionId

									            LEFT JOIN [HKP].Designation GDes ON GDes.Id = EM.GivenDesignationId
								                LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.GivenDesignationId
								                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                               WHERE   (EM.EmployeeStatus != 'Separated' OR CONVERT(DATE,em.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @" 
                                                AND CONVERT(DATE, EM.DOJ) <= CONVERT(DATE, '" + hrDate + @"')
                                                AND EM.GroupID = '" + companyGroupId + @"'  " + dStatus + @" " + EmployeeCategory + @"
                                                group by BudgetCode,GroupID,c.Id
                                            ) EmpInfo on m.Id=EmpInfo.BudgetCode and EmpInfo.GroupID = m.CgId and EmpInfo.cid = m.CompanyId
                                        LEFT OUTER JOIN
                                  (
								  SELECT ISNULL(PO.IsDirect,0) IsDirect, 
								  COUNT(E.SystemId) totalPresentEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName, MPB.Id BudgetCode


                                    FROM ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                               SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
                                   LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category = 'Present' AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id and c.Id = E.CompanyId

                                         LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE
                                        e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"
                                            GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName, MPB.Id,PO.IsDirect
											)
									PresentEmployee
                                    ON  m.Id=PresentEmployee.BudgetCode and  EmpInfo.GroupID = PresentEmployee.CompanyGroupId AND EmpInfo.cid = PresentEmployee.CompanyId AND m.IsDirect = PresentEmployee.IsDirect

                                    LEFT OUTER JOIN
                                  (
								  SELECT ISNULL(PO.IsDirect,0) IsDirect, 
								  COUNT(E.SystemId) totalAbsentEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName, MPB.Id BudgetCode


                                    FROM ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                               SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
                                   LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category = 'Absent' AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                         LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE
                                        e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"
                                            GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName, MPB.Id,PO.IsDirect
											)
									AbsentEmployee
                                    ON  m.Id=AbsentEmployee.BudgetCode AND  EmpInfo.GroupID = AbsentEmployee.CompanyGroupId AND EmpInfo.cid = AbsentEmployee.CompanyId AND m.IsDirect = AbsentEmployee.IsDirect
                            ---- Late ---------------
                        LEFT OUTER JOIN
                                  (
								  SELECT ISNULL(PO.IsDirect,0) IsDirect, 
								  COUNT(E.SystemId) totalLateEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName, MPB.Id BudgetCode


                                    FROM ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                               SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
                                   LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category = 'Late' AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                        LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE
                                        e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"
                                            GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName, MPB.Id,PO.IsDirect
											)
									LateEmployee
                                    ON  m.Id=LateEmployee.BudgetCode AND  EmpInfo.GroupID = LateEmployee.CompanyGroupId AND EmpInfo.cid = LateEmployee.CompanyId AND m.IsDirect = LateEmployee.IsDirect

--------late End---------

                                                                        -----Leave-----
LEFT OUTER JOIN
                                  (
								  SELECT ISNULL(PO.IsDirect,0) IsDirect, 
								  COUNT(E.SystemId) totalLeaveEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName, MPB.Id BudgetCode


                                    FROM ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                               SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
                                   LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category = 'Leave' AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                          LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE
                                        e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"
                                            GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName, MPB.Id,PO.IsDirect
											)
									LeaveEmployee
                                    ON  m.Id=LeaveEmployee.BudgetCode AND  EmpInfo.GroupID = LeaveEmployee.CompanyGroupId AND EmpInfo.cid = LeaveEmployee.CompanyId AND m.IsDirect = LeaveEmployee.IsDirect

                                                                        ---Leave End----
                                                                  ----------WeekOff ----------
                                                                  LEFT OUTER JOIN
                                  (
								  SELECT ISNULL(PO.IsDirect,0) IsDirect, 
								  COUNT(E.SystemId) totalWeekOffEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName, MPB.Id BudgetCode


                                    FROM ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                               SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
                                   LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType

                                    WHERE DT.Category  IN('Holiday', 'Weekend') AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                         LEFT JOIN[HKP].Designation GDes ON GDes.Id = E.GivenDesignationId

                                    LEFT JOIN[MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId

                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE
                                        e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"
                                            GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName, MPB.Id,PO.IsDirect
											)
									WeekOffEmployee
                                    ON  m.Id=WeekOffEmployee.BudgetCode AND  EmpInfo.GroupID = WeekOffEmployee.CompanyGroupId AND EmpInfo.cid = WeekOffEmployee.CompanyId AND m.IsDirect = WeekOffEmployee.IsDirect

                                                                  --------WeekOff End---------

LEFT OUTER JOIN
                                    (SELECT ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalOthersShiftNotAssignedEmployee,mpb.Id BudgetCode, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName

                                    FROM  ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                    LEFT OUTER JOIN(--*
                                    SELECT * FROM EmployeeInformation

                                    WHERE SystemId NOT IN(--**
                                    select EmpSystemId from EmployeeShiftAssign

                                    )-- * *

                                    )-- *
                                    E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                        LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                            WHERE

                                                 e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"

                                     GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect, mpb.Id)

                                    OthersShiftNotAssignedEmployee   
									   ON  m.Id=OthersShiftNotAssignedEmployee.BudgetCode AND  EmpInfo.GroupID = OthersShiftNotAssignedEmployee.CompanyGroupId AND EmpInfo.cid = OthersShiftNotAssignedEmployee.CompanyId AND m.IsDirect = OthersShiftNotAssignedEmployee.IsDirect

                                  
---- LUNCHOUT ---------------
                        LEFT OUTER JOIN
                                  (
								  SELECT ISNULL(PO.IsDirect,0) IsDirect, 
								  COUNT(E.SystemId) totalLounchOutEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName, MPB.Id BudgetCode


                                    FROM ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                                 SELECT DISTINCT EmpSystemID FROM AttendanceInfoExtra   AIE
                                    WHERE AIE.InfoType = 'LUNCHOUT'  AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"') and ISNULL(OutTime,'')<>'' and  ISNULL(InTime,'')= ''
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                       LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE
                                        e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"
                                            GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName, MPB.Id,PO.IsDirect
											)
									LounchOutEmployee
                                    ON  m.Id=LounchOutEmployee.BudgetCode AND  EmpInfo.GroupID = LounchOutEmployee.CompanyGroupId AND EmpInfo.cid = LounchOutEmployee.CompanyId AND m.IsDirect = LounchOutEmployee.IsDirect

--------LUNCHOUT End---------
---- EARLYOUT ---------------
                        LEFT OUTER JOIN
                                  (
								  SELECT ISNULL(PO.IsDirect,0) IsDirect, 
								  COUNT(E.SystemId) totalEarlyOutEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName, MPB.Id BudgetCode


                                    FROM ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                                 SELECT DISTINCT EmpSystemID FROM AttendanceInfoExtra   AIE
                                    WHERE AIE.InfoType = 'EARLYOUT'  AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                         LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE
                                        e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"
                                            GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName, MPB.Id,PO.IsDirect
											)
									EARLYOUTEmployee
                                    ON  m.Id=EARLYOUTEmployee.BudgetCode AND  EmpInfo.GroupID = EARLYOUTEmployee.CompanyGroupId AND EmpInfo.cid = EARLYOUTEmployee.CompanyId AND m.IsDirect = EARLYOUTEmployee.IsDirect

--------EARLYOUT End---------
---- LATEINE ---------------
                        LEFT OUTER JOIN
                                  (
								  SELECT ISNULL(PO.IsDirect,0) IsDirect, 
								  COUNT(E.SystemId) totalLateInEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName CompanyName, MPB.Id BudgetCode


                                    FROM ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId


                                    LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                                 SELECT DISTINCT EmpSystemID FROM AttendanceInfoExtra   AIE
                                    WHERE AIE.InfoType = 'LATEIN'  AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

                                        LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								          LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                         LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE
                                        e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"
                                            GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName, MPB.Id,PO.IsDirect
											)
									LATEINEmployee
                                    ON  m.Id=LATEINEmployee.BudgetCode AND  EmpInfo.GroupID = LATEINEmployee.CompanyGroupId AND EmpInfo.cid = LATEINEmployee.CompanyId AND m.IsDirect = LATEINEmployee.IsDirect

--------LATEIN End---------
                        LEFT OUTER JOIN

                                (SELECT  ISNULL(PO.IsDirect,0) IsDirect, COUNT(E.SystemId) totalShiftNotAssignedEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,mpb.Id BudgetCode,
                                C.Id AS CompanyId, C.UserName CompanyName


                                FROM  ORG.CompanyGroup CG

                                LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                LEFT OUTER JOIN(--*
                                SELECT * FROM EmployeeInformation

                                WHERE SystemId NOT IN(--**
                                SELECT DISTINCT EmpSystemID FROM EmployeeShiftAssign
                                )-- * *

                                )-- *
                                E ON e.GroupID = CG.Id and c.Id = E.CompanyId

                                 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    WHERE e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"


                                GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect,mpb.Id)

                                  
                                    ShiftNotAssignedEmployee
								
									   ON  m.Id=ShiftNotAssignedEmployee.BudgetCode AND  EmpInfo.GroupID = ShiftNotAssignedEmployee.CompanyGroupId AND EmpInfo.cid = ShiftNotAssignedEmployee.CompanyId AND m.IsDirect = ShiftNotAssignedEmployee.IsDirect

                                LEFT OUTER JOIN
                                    (
                                     SELECT  ISNULL(PO.IsDirect,0) IsDirect,mpb.Id BudgetCode, COUNT(E.SystemID) totalAttdnNotProcessedToday, cg.Id CompanyGroupId, cg.UserName GroupName,
                                            C.Id AS CompanyId, C.UserName  UId

                                        FROM  ORG.CompanyGroup CG

                                            LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                            INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id = E.CompanyId

                                            Inner JOIN(--*
                                                               SELECT TOP 1 WITH TIES *
                                                                FROM EmployeeShiftAssign

                                                                WHERE EffectiveDate <= GETDATE() and

                                                                EmpSystemID NOT IN(--**
                                                                                            SELECT DISTINCT EmpSystemID FROM AttdnProcessData

                                                                                            WHERE  CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
                                                                                    )

                                                                ORDER BY ROW_NUMBER() OVER(PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC)
                                                              )-- *
                                                        ESA

                                            ON E.SystemId = ESA.EmpSystemID


                                      LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                        WHERE

                                                 e.GroupID = '" + companyGroupId + @"'  AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"


                                    GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName, PO.IsDirect,mpb.Id) AttdnNotProcessedToday 
				   ON  m.Id=AttdnNotProcessedToday.BudgetCode AND  EmpInfo.GroupID = AttdnNotProcessedToday.CompanyGroupId AND EmpInfo.cid = AttdnNotProcessedToday.CompanyId AND m.IsDirect = AttdnNotProcessedToday.IsDirect

                                    LEFT OUTER JOIN
                                    (
                                    SELECT ISNULL(PO.IsDirect,0) IsDirect,mpb.Id BudgetCode, COUNT(ESA.SystemID) totalShiftNotAssignAsofToday, cg.Id CompanyGroupId, cg.UserName GroupName,
                                    C.Id AS CompanyId, C.UserName  UId

                                    FROM  ORG.CompanyGroup CG

                                    LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                    LEFT OUTER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id = E.CompanyId

                                    LEFT OUTER JOIN(--*
                                    SELECT SystemID FROM EmployeeInformation EI

                                    WHERE EI.SystemID NOT IN(--**
                                    SELECT DISTINCT EmpSystemID FROM EmpDateWiseShiftAssign

                                    WHERE  CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
                                    )-- * *

                                    )-- *
                                    ESA

                                    ON E.SystemId = ESA.SystemId


                                  LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId

LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    WHERE

                                                 e.GroupID = '" + companyGroupId + @"'   AND(E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"')) 
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"') " + EmployeeCategory + @"

                                    GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,PO.IsDirect,mpb.Id)
                        ShiftNotAssignAsofToday 
				   ON  m.Id=ShiftNotAssignAsofToday.BudgetCode AND  EmpInfo.GroupID = ShiftNotAssignAsofToday.CompanyGroupId AND EmpInfo.cid = ShiftNotAssignAsofToday.CompanyId AND m.IsDirect = ShiftNotAssignAsofToday.IsDirect

                                    LEFT JOIN
                                    (SELECT  ISNULL(PO.IsDirect,0) IsDirect,mpb.Id BudgetCode, COUNT(E.SystemId) totalEmployee, C.UserName, cg.Id CompanyGroupId, c.Id CompanyId, c.UserName CompanyName, cg.UserName GroupName    FROM ORG.CompanyGroup CG

                                            LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

                                            LEFT OUTER JOIN EmployeeInformation

                                            E ON e.GroupID = CG.Id and c.Id= E.CompanyId

                                     LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget MPB ON MPB.Id = E.BudgetCode
								    LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT  JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId

                                            where
                                                 e.GroupID = '" + companyGroupId + @"'   AND E.EmployeeCurrentStatus = 'LONG ABSENTEEISM' AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + hrDate + @"'))  
                                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + hrDate + @"')  AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"') " + EmployeeCategory + @"

                                                group by C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,po.IsDirect,mpb.Id 
												) LONGABSENTISM
                                             
				   ON  m.Id=LONGABSENTISM.BudgetCode AND  EmpInfo.GroupID = LONGABSENTISM.CompanyGroupId AND EmpInfo.cid = LONGABSENTISM.CompanyId AND m.IsDirect = LONGABSENTISM.IsDirect

                                            --------------------------ManpowerBudgetWiseSalary-----------------------------------
								            LEFT OUTER JOIN
								            (

								                SELECT MBA.ManpowerBudgetId,
									            MinimumSalary = case when MBA.EffectiveDate <= '" + hrDate + @"'  then  isnull(MinimumSalary,0) else 0 end,
									            MaximumSalary = case when MBA.EffectiveDate <= '" + hrDate + @"'  then  isnull(MaximumSalary,0) else 0 end
									            ,ED.EffectiveDate,m.CompanyId
									            FROM [MST].[ManpowerBudgetAllowance] MBA
								                LEFT OUTER JOIN [MST].[ManpowerBudget] AS m ON m.Id = MBA.ManpowerBudgetId
									            LEFT OUTER JOIN [MST].[ManpowerBudgetDetail] AS MBD ON MBD.ManpowerBudgetId = MBA.ManpowerBudgetId
                                                     LEFT OUTER JOIN [ORG].[Position] AS PO ON m.PositionId = PO.Id
LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                             
									            LEFT OUTER JOIN (
									            SELECT MAX(EffectiveDate) EffectiveDate,ManpowerBudgetId,CompanyId from [MST].[ManpowerBudgetAllowance]
									             LEFT OUTER JOIN [MST].[ManpowerBudget] AS m on m.Id = ManpowerBudgetId
									             WHERE EffectiveDate=(SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetAllowance] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + hrDate + @"') ORDER BY EffectiveDate DESC)
									             GROUP BY ManpowerBudgetId ,CompanyId
									            )  ED ON ED.ManpowerBudgetId = MBA.ManpowerBudgetId and ED.EffectiveDate = MBA.EffectiveDate
									             WHERE
									             ED.EffectiveDate IS NOT NULL AND m.CompanyGroupId =  '" + companyGroupId + @"'
									             " + dStatus + @" " + EmployeeCategory + @" AND MBD.EffectiveDate = (SELECT DISTINCT TOP (1) EffectiveDate FROM [MST].[ManpowerBudgetDetail] WHERE CONVERT(DATE,(EffectiveDate) )<= CONVERT(DATE,'" + hrDate + @"') ORDER BY EffectiveDate DESC)
									            ) Sal ON m.Id = Sal.ManpowerBudgetId AND m.CompanyId = Sal.CompanyId
                                             -------------------------3. Manpower Budget Detail from [MST].[ManpowerBudgetDetail]--------------------------------------------------------
                                              LEFT OUTER JOIN
                                            (
                                            SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id, C.Id AS cid FROM
                                            (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									            FROM [MST].[ManpowerBudgetDetail]
									            WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + hrDate + @"')
									            ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                             ) MBD

                                              LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId

                                              LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId

                                              LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                              LEFT outer JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                              LEFT outer JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                               LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
								               LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
								               LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
								               LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
	LEFT JOIN [HKP].Designation GDes ON GDes.Id = PO.DesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                             WHERE CG.Id = '" + companyGroupId + @"' " + dStatus + @" " + EmployeeCategory + @"  AND TotalNumber > 0 
                                             ) B

                                             ON M.id = b.ManpowerBudgetId AND B.Id = M.CgId AND B.cid = M.CompanyId
                                 ) EDE GROUP BY GroupName,CompanyId,UserName,IsDirect,CgId ORDER BY UserName";
                //return _sqlRepository.GetDataCollection(sql);


                DataTable dt = _sqlRepository.GetDataTable(sql);

                DataTable dtTemp = dt.Clone();
                if (dt.Rows.Count > 0)
                {
                    dtTemp = dt.AsEnumerable().GroupBy(x => new
                    {
                        CompanyId = x["CompanyId"],
                        ColumnName = x["ColumnName"],
                        GroupName = x["GroupName"],
                        CompanyGroupId = x["CompanyGroupId"],
                        IsDirect = "General"

                    })
                .Select(x =>
                {
                    DataRow row = dt.NewRow();
                    row["IsDirect"] = "General";
                    row["ColumnName"] = x.Key.ColumnName; row["CompanyId"] = x.Key.CompanyId; row["GroupName"] = x.Key.GroupName; row["CompanyGroupId"] = x.Key.CompanyGroupId;
                    row["totalEarlyOutEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalEarlyOutEmployee"]));
                    row["totalLateInEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLateInEmployee"]));
                    row["totalLounchOutEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLounchOutEmployee"]));
                    row["ProposedManpowerBudget"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ProposedManpowerBudget"]));

                    row["OnRoleEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OnRoleEmployee"])); row["totalPresentEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalPresentEmployee"]));
                    row["totalAbsentEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalAbsentEmployee"])); row["totalLateEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLateEmployee"]));
                    row["totalLeaveEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLeaveEmployee"])); row["totalWeekoffEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalWeekoffEmployee"]));
                    row["ShiftNotAssignedEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ShiftNotAssignedEmployee"])); row["totalAttdnNotProcessedToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalAttdnNotProcessedToday"]));
                    row["totalShiftNotAssignAsofToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalShiftNotAssignAsofToday"])); row["totalLongAbsentismEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLongAbsentismEmployee"]));
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


        public IEnumerable<object> X_DrillDownAttnStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            try
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
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;

                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cListId = "," + item.StandardName + "Defination.SystemId";
                                cList = "," + item.StandardName + "Defination.UserName"; cListId = "," + item.StandardName + "Defination.SystemId";
                                join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND C.Id ='" + item.Id + "'";
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
                var sql = @"SELECT DISTINCT 
CASE WHEN  ISNULL(OnRoleEmployee.IsDirect,0) = 0 THEN 'Indirect' ELSE 'Direct' END AS  IsDirect,
ISNULL(OnRoleEmployee.ColumnName,'N/A') ColumnName, ISNULL(OnRoleEmployee.UId,'') UId,OnRoleEmployee.CompanyId CompanyId,OnRoleEmployee.CompanyGroupId CompanyGroupId,ISNULL(OnRoleEmployee.totalEmployee,0) OnRoleEmployee,ISNULL(PresentEmployee.totalPresentEmployee,0) totalPresentEmployee, ISNULL(AbsentEmployee.totalAbsentEmployee,0) totalAbsentEmployee,ISNULL(LateEmployee.totalLateEmployee,0) totalLateEmployee
					  ,ISNULL(LeaveEmployee.totalLeaveEmployee,0) totalLeaveEmployee,ISNULL(WeekOffEmployee.totalWeekoffEmployee,'')totalWeekoffEmployee,ISNULL(ShiftNotAssignedEmployee.totalShiftNotAssignedEmployee,0) ShiftNotAssignedEmployee
						,ISNULL(AttdnNotProcessedToday.totalAttdnNotProcessedToday, 0) totalAttdnNotProcessedToday
						,ISNULL(ShiftNotAssignAsofToday.totalShiftNotAssignAsofToday,0) totalShiftNotAssignAsofToday
                        ,ISNULL(LONGABSENTISM.totalLongAbsentEmployee,0) totalLongAbsentismEmployee

					  FROM
							 (SELECT POS.IsDirect, COUNT(E.SystemId) totalEmployee,C.UserName,cg.Id CompanyGroupId,c.Id CompanyId,c.UserName CompanyName,cg.UserName GroupName
								" + cList + @" AS ColumnName
										" + cListId + @" AS UId
                               FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											LEFT OUTER JOIN EmployeeInformation
											E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								--LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
	                            LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + join + @"
											where
												    e.GroupID = '" + companyGroupId + @"' " + wc + @"  AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @"
                                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')
												GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName,POS.IsDirect" + cList + @" " + cListId + @") OnRoleEmployee
												LEFT OUTER JOIN
							  ( SELECT POS.IsDirect,COUNT(E.SystemId) totalPresentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

								LEFT OUTER JOIN  (--*
													SELECT * FROM EmployeeInformation
													WHERE SystemId IN (--**
																SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
																	LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
																	WHERE DT.Category = 'Present' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
																)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                            LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								--  LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
							--	LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + join + @"
								WHERE
							       e.GroupID = '" + companyGroupId + @"' " + wc + @"  AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,cg.UserName" + cList + @" " + cListId + @")
								PresentEmployee ON PresentEmployee.CompanyGroupId = OnRoleEmployee.CompanyGroupId AND PresentEmployee.CompanyId = OnRoleEmployee.CompanyId AND PresentEmployee.IsDirect = OnRoleEmployee.IsDirect AND  ISNULL(PresentEmployee.UId,'') = ISNULL(OnRoleEmployee.UId,'')

								LEFT OUTER JOIN

								 (SELECT POS.IsDirect,COUNT(E.SystemId) totalAbsentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName CompanyName
								" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

								LEFT OUTER JOIN  (--*
													SELECT * FROM EmployeeInformation
													WHERE SystemId IN (--**
																		SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
																				LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
																			WHERE DT.Category = 'Absent' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
																		)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                                LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								 -- LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								" + join + @"

								WHERE
							       e.GroupID = '" + companyGroupId + @"'  " + wc + @"  AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,cg.UserName" + cList + @" " + cListId + @")
								AbsentEmployee
								ON
								OnRoleEmployee.CompanyGroupId = AbsentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = AbsentEmployee.CompanyId AND OnRoleEmployee.IsDirect = AbsentEmployee.IsDirect AND  ISNULL(OnRoleEmployee.UId,'') = ISNULL(AbsentEmployee.UId,'')

								LEFT OUTER JOIN

								(SELECT POS.IsDirect,COUNT(E.SystemId) totalLateEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                   				LEFT OUTER JOIN  (--*
														SELECT * FROM EmployeeInformation
														WHERE SystemId IN (--**
														SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
															LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
															WHERE DT.Category = 'Late' AND CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
														)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								 -- LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
                            	LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
											" + join + @"
								WHERE
							    e.GroupID = '" + companyGroupId + @"' " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"'))  " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,cg.UserName " + cList + @" " + cListId + @") LateEmployee
                                ON OnRoleEmployee.CompanyGroupId = LateEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LateEmployee.CompanyId AND OnRoleEmployee.IsDirect = LateEmployee.IsDirect AND  ISNULL(OnRoleEmployee.UId,'') = ISNULL(LateEmployee.UId,'')
								LEFT OUTER JOIN
								(SELECT POS.IsDirect,COUNT(E.SystemId) totalLeaveEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN  (--*
													SELECT * FROM EmployeeInformation
													WHERE SystemId IN (--**
																SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
																		LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
																	WHERE DT.Category = 'Leave' AND CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
																)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
                                --LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + join + @"
								WHERE

							       e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"'))" + EmployeeCategory + @"

							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,cg.UserName " + cList + @" " + cListId + @") LeaveEmployee 
                                ON OnRoleEmployee.CompanyGroupId = LeaveEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LeaveEmployee.CompanyId AND OnRoleEmployee.IsDirect = LeaveEmployee.IsDirect AND  ISNULL(OnRoleEmployee.UId,'') = ISNULL(LeaveEmployee.UId,'')
								LEFT OUTER JOIN
								    (SELECT POS.IsDirect, COUNT(E.SystemId) totalWeekoffEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
														SELECT * FROM EmployeeInformation
														WHERE SystemId IN (--**
																			SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
																				LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
																			WHERE DT.Category IN ('Holiday','Weekend') AND CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
																		)--**
														)--*
									E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

	                            LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
									--  LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
									--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
									--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
									
                                    " + join + @"
									WHERE
									    e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
									GROUP BY C.UserName,cg.UserName,POS.IsDirect,C.Id,cg.Id,cg.UserName" + cList + @" " + cListId + @")
									WeekOffEmployee
									ON
									OnRoleEmployee.CompanyGroupId = WeekOffEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = WeekOffEmployee.CompanyId AND OnRoleEmployee.IsDirect = WeekOffEmployee.IsDirect AND  ISNULL(OnRoleEmployee.UId,'') = ISNULL(WeekOffEmployee.UId,'')

								LEFT OUTER JOIN
								(SELECT POS.IsDirect, COUNT(E.SystemId) totalShiftNotAssignedEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN  (--*
													SELECT * FROM EmployeeInformation
													WHERE SystemId NOT IN (--**
																		SELECT DISTINCT EmpSystemID FROM EmployeeShiftAssign
																		)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                              LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								--LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + join + @"
								WHERE
							       e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,POS.IsDirect,C.Id,cg.Id,cg.UserName " + cList + @" " + cListId + @")
								ShiftNotAssignedEmployee ON
								OnRoleEmployee.CompanyGroupId = ShiftNotAssignedEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = ShiftNotAssignedEmployee.CompanyId AND OnRoleEmployee.IsDirect = ShiftNotAssignedEmployee.IsDirect AND  ISNULL(OnRoleEmployee.UId,'') = ISNULL(ShiftNotAssignedEmployee.UId,'')
                                    LEFT OUTER JOIN
									(
									 SELECT POS.IsDirect, count(E.SystemID) totalAttdnNotProcessedToday,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
										FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id=E.CompanyId

											Inner JOIN(--*
															   SELECT TOP 1 WITH TIES *
																FROM EmployeeShiftAssign
																WHERE EffectiveDate <= GETDATE() and
																EmpSystemID NOT IN(--**
																							SELECT DISTINCT EmpSystemID FROM AttdnProcessData
																							WHERE  CONVERT(DATE, WorkDate) =  CONVERT(DATE, '" + hrDate + @"')
																					)
																ORDER BY ROW_NUMBER() OVER(PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC)
															  )-- *
														ESA
											ON E.SystemId = ESA.EmpSystemID

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                            LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								    --LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
									--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
									--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
									" + join + @"

									WHERE
									  e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"'))" + EmployeeCategory + @"
                                            AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')
							    GROUP BY C.UserName,cg.UserName,POS.IsDirect,C.Id,cg.Id,cg.UserName " + cList + @" " + cListId + @") 
                                AttdnNotProcessedToday ON OnRoleEmployee.CompanyGroupId = AttdnNotProcessedToday.CompanyGroupId AND OnRoleEmployee.CompanyId = AttdnNotProcessedToday.CompanyId AND OnRoleEmployee.IsDirect = AttdnNotProcessedToday.IsDirect  AND  ISNULL(OnRoleEmployee.UId,'') = ISNULL(AttdnNotProcessedToday.UId,'')
									LEFT OUTER JOIN
									(
									SELECT POS.IsDirect,COUNT(ESA.SystemID) totalShiftNotAssignAsofToday,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id=E.CompanyId
									LEFT OUTER JOIN  (--*
														SELECT SystemID FROM EmployeeInformation EI
															WHERE EI.SystemID NOT IN (--**
																  SELECT DISTINCT EmpSystemID FROM EmpDateWiseShiftAssign
																	WHERE  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
													)--**
									)--*
									ESA
									ON E.SystemId = ESA.SystemId

								    LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								   -- LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--	LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--	LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
	                            LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + join + @"

									WHERE

									 e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                      AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName,POS.IsDirect " + cList + @" " + cListId + @") ShiftNotAssignAsofToday
                    ON OnRoleEmployee.CompanyGroupId = ShiftNotAssignAsofToday.CompanyGroupId AND OnRoleEmployee.CompanyId = ShiftNotAssignAsofToday.CompanyId AND OnRoleEmployee.IsDirect = ShiftNotAssignAsofToday.IsDirect  AND  ISNULL(OnRoleEmployee.UId,'') = ISNULL(ShiftNotAssignAsofToday.UId,'')
	                            LEFT JOIN
								(
								SELECT POS.IsDirect, COUNT(E.SystemId) totalLongAbsentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

								LEFT OUTER JOIN  EmployeeInformation
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                            
                                LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								 -- LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
                                                " + join + @"
								WHERE
							       e.GroupID = '" + companyGroupId + @"'   " + wc + @" AND E.EmployeeCurrentStatus = 'LONG ABSENTEEISM' AND (E.EmployeeStatus != 'Separated'  OR E.DOS >= '" + hrDate + @"') 
							    GROUP BY C.UserName,POS.IsDirect,cg.UserName,C.Id,cg.Id,cg.UserName " + cList + @" " + cListId + @")
								LONGABSENTISM ON LONGABSENTISM.CompanyGroupId = OnRoleEmployee.CompanyGroupId AND LONGABSENTISM.CompanyId = OnRoleEmployee.CompanyId AND LONGABSENTISM.IsDirect = OnRoleEmployee.IsDirect AND  ISNULL(LONGABSENTISM.UId,'') = ISNULL(OnRoleEmployee.UId,'')
                                
                                ";
                DataTable dt = _sqlRepository.GetDataTable(sql);

                DataTable dtTemp = dt.Clone();
                if (dt.Rows.Count > 0)
                {
                    dtTemp = dt.AsEnumerable().GroupBy(x => new
                    {
                        CompanyId = x["CompanyId"],
                        UId = x["UId"],
                        ColumnName = x["ColumnName"],
                        //GroupName = x["GroupName"],
                        CompanyGroupId = x["CompanyGroupId"],
                        IsDirect = "General"

                    })
                .Select(x =>
                {
                    DataRow row = dt.NewRow();
                    row["IsDirect"] = "General";
                    row["UId"] = x.Key.UId; row["ColumnName"] = x.Key.ColumnName; row["CompanyId"] = x.Key.CompanyId; row["CompanyGroupId"] = x.Key.CompanyGroupId;
                    row["OnRoleEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OnRoleEmployee"])); row["totalPresentEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalPresentEmployee"]));
                    row["totalAbsentEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalAbsentEmployee"])); row["totalLateEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLateEmployee"]));
                    row["totalLeaveEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLeaveEmployee"])); row["totalWeekoffEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalWeekoffEmployee"]));
                    row["ShiftNotAssignedEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ShiftNotAssignedEmployee"])); row["totalAttdnNotProcessedToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalAttdnNotProcessedToday"]));
                    row["totalShiftNotAssignAsofToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalShiftNotAssignAsofToday"])); row["totalLongAbsentismEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLongAbsentismEmployee"]));
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
        public IEnumerable<object> DrillDownAttnStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            try
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
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;

                string wcem = "";
                string wcm = "";
                string wcExt = "";
                string cListext = "";
                string cListextId = "";
                string cListextIdR = "";
                string cListEmpG = "";
                string cListEmp = "";
                string cListextM = "";
                string cListextMSequence = "";
                string cListextIdM = "";
                string cListextF = "";
                string cListextIdF = "";
                string cListFinish = "";
                string cListSequence = "";


                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";
                                cListSequence = "," + item.StandardName + ".Sequence";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = ENT." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cListSequence = "," + item.StandardName + ".Sequence";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cListSequence = "," + item.StandardName + ".Sequence";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MB." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cListId = "," + item.StandardName + "Defination.SystemId";
                                cListSequence = "," + item.StandardName + "Defination.SequenceNo";
                                cList = "," + item.StandardName + "Defination.UserName"; cListId = "," + item.StandardName + "Defination.SystemId";
                                join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = "  AND C.Id ='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    wc += " AND ISNULL(" + item.StandardName + "Defination.SystemId,'')='" + item.Text + "'";
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
                                    wc += " AND " + item.StandardName + ".Id='" + item.Text + "'";
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
                var sql = @"SELECT
                                IsDirect,CompanyGroupId,CompanyId,ISNULL(ColumnName,'N/A') ColumnName,UId,Sequence" + cListextF + @"" + cListextIdF + @"
                               ,ISNULL(sum(TotalNumber),0) ProposedManpowerBudget ,ISNULL(SUM(TotalManpower),0) OnRoleEmployee
                                ,ISNULL(sum(TotalSalary),0) OnRoleSalaryC
                                 ,SUM(short) Short,SUM(Excess) Excess
                               ,ISNULL(SUM(totalPresentEmployee),0) totalPresentEmployee,ISNULL(SUM(totalAbsentEmployee),0) totalAbsentEmployee
								,ISNULL(SUM(totalLateEmployee),0) totalLateEmployee,ISNULL(SUM(totalLeaveEmployee),0) totalLeaveEmployee
								,ISNULL(SUM(totalWeekoffEmployee),0) totalWeekoffEmployee,ISNULL(SUM(ShiftNotAssignedEmployee),0) ShiftNotAssignedEmployee
                                ,SUM(totalEarlyOutEmployee) totalEarlyOutEmployee, SUM(totalLounchOutEmployee) totalLounchOutEmployee,SUM(totalLateInEmployee) totalLateInEmployee
								,ISNULL(SUM(totalAttdnNotProcessedToday),0) totalAttdnNotProcessedToday,ISNULL(SUM(totalShiftNotAssignAsofToday),0) totalShiftNotAssignAsofToday
								,ISNULL(SUM(totalLongAbsentismEmployee),0) totalLongAbsentismEmployee
                                FROM
                                 (
                                 SELECT m.Id
                                 ,b.TotalNumber
                                 ,m.CompanyId
                                 ,m.CompanyGroupId
                                    ,Case when  ISNULL(m.IsDirect,0) = 0 then 'Indirect' else 'Direct' end AS  IsDirect
                                  " + cListextM + @" 
                                  " + cListextIdM + @"
                                    

                                  ,m.UId
                                  ,m.ColumnName
                                  ,m.Sequence
                                  ,e.TotalSalary
                                  ,e.TotalManpower
                                   
                                  ,Short = CASE
                                  WHEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0) > 0
                                  THEN ISNULL(TotalNumber,0) - ISNULL(TotalManpower,0)
                                  ELSE 0
                                  END
                                  ,Excess = CASE
                                  WHEN ISNULL(TotalManpower,0) - ISNULL(TotalNumber,0) > 0
                                  THEN ISNULL(TotalManpower,0) - ISNULL(TotalNumber,0)
                                  ELSE 0 END

                                ,ISNULL(totalEarlyOutEmployee,0) totalEarlyOutEmployee
                                ,ISNULL(totalLounchOutEmployee,0) totalLounchOutEmployee
                                ,ISNULL(totalLateInEmployee,0) totalLateInEmployee

                                ,ISNULL(PresentEmployee.totalPresentEmployee,0) totalPresentEmployee
								,ISNULL(AbsentEmployee.totalAbsentEmployee,0) totalAbsentEmployee
								,ISNULL(LateEmployee.totalLateEmployee,0) totalLateEmployee
								,ISNULL(LeaveEmployee.totalLeaveEmployee,0) totalLeaveEmployee
								,ISNULL(WeekOffEmployee.totalWeekoffEmployee,'')totalWeekoffEmployee
								,ISNULL(ShiftNotAssignedEmployee.totalShiftNotAssignedEmployee,0) ShiftNotAssignedEmployee
								,ISNULL(AttdnNotProcessedToday.totalAttdnNotProcessedToday,0) totalAttdnNotProcessedToday
								,ISNULL(ShiftNotAssignAsofToday.totalShiftNotAssignAsofToday,0) totalShiftNotAssignAsofToday
                                 ,ISNULL(LONGABSENTISM.totalLongAbsentEmployee,0) totalLongAbsentismEmployee
                                  FROM
                                  ----------------------1 bc-------------------------------c-------
                                  (SELECT
                                    MB.Code
                                    ,MB.Id
                                    ,MB.CompanyGroupId
                                    ,c.Id AS CompanyId
                                    ,c.UserName AS CName
                                    ,POS.IsDirect

                                    " + cListext + @"
                                    " + cListextIdR + @"
                                   
                                   	" + cList + @" AS ColumnName
										" + cListId + @" AS UId
                                    " + cListSequence + @" Sequence

                                    FROM [MST].[ManpowerBudget]  MB
                                    LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                   LEFT OUTER JOIN [ORG].[Company] AS c on c.Id = MB.CompanyId AND c.CompanyGroupId = cg.Id
                                    LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
                                    LEFT OUTER JOIN [ORG].[Position] AS POS ON POS.Id = MB.PositionId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = POS.DesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                    " + join + @"
                                    WHERE Cg.Id = '" + companyGroupId + @"'  " + wc + @" " + EmployeeCategory + @" AND MB.Active = 1
                                    )  m
                                -----------------------2e--------------------------------
                                   LEFT OUTER JOIN
                                   (SELECT COUNT(em.SystemID) TotalManpower,BudgetCode,POS.IsDirect,em.CompanyId,em.GroupID CompanyGroupId	" + cList + @" AS ColumnName
										" + cListId + @" AS UId,sum(TotalSalary) TotalSalary
                                   FROM [dbo].[EmployeeInformation]  em
                                    LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = em.BudgetCode
                                      LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                                  LEFT outer JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId  AND c.CompanyGroupId = cg.Id
                                  LEFT outer JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
                                  LEFT outer JOIN [ORG].[Position] AS POS ON POS.Id = MB.PositionId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = EM.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.LegalDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                     " + join + @"
                                   WHERE  (em.EmployeeStatus != 'Separated' OR CONVERT(DATE,em.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @" 
                                                AND CONVERT(DATE, em.DOJ) <= CONVERT(DATE, '" + hrDate + @"')

                                    AND  em.GroupID  = '" + companyGroupId + @"' " + wcem + @" " + EmployeeCategory + @"
                                   GROUP BY BudgetCode,em.GroupID,em.CompanyId,POS.IsDirect  " + cListId + @" " + cList + @"
                                ) e on m.Id=e.BudgetCode and e.CompanyGroupId = m.CompanyGroupId and e.CompanyId = m.CompanyId  AND  ISNULL(m.UId,'') = ISNULL(e.UId,'')
                                LEFT OUTER JOIN
							  ( SELECT POS.IsDirect,COUNT(E.SystemId) totalPresentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName CompanyName,MB.Id BudgetCode
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

								LEFT OUTER JOIN  (--*
													SELECT * FROM EmployeeInformation
													WHERE SystemId IN (--**
																SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
																	LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
																	WHERE DT.Category = 'Present' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
																)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

								 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
	                            LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId

										" + join + @"
								WHERE
							       e.GroupID = '" + companyGroupId + @"' " + wc + @" and  (e.EmployeeStatus != 'Separated' OR CONVERT(DATE,e.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @" 
                                                AND CONVERT(DATE, e.DOJ) <= CONVERT(DATE, '" + hrDate + @"')" + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,MB.Id,cg.UserName" + cList + @" " + cListId + @")
								PresentEmployee ON PresentEmployee.BudgetCode = m.Id  and PresentEmployee.CompanyGroupId = e.CompanyGroupId AND PresentEmployee.CompanyId = e.CompanyId AND PresentEmployee.IsDirect = e.IsDirect AND  ISNULL(PresentEmployee.UId,'') = ISNULL(e.UId,'')

								LEFT OUTER JOIN

								 (SELECT POS.IsDirect,COUNT(E.SystemId) totalAbsentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
								C.Id AS CompanyId,C.UserName CompanyName
								" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

								LEFT OUTER JOIN  (--*
													SELECT * FROM EmployeeInformation
													WHERE SystemId IN (--**
																		SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
																				LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
																			WHERE DT.Category = 'Absent' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
																		)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
	                                LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								 -- LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								" + join + @"

								WHERE
							       e.GroupID = '" + companyGroupId + @"'  " + wc + @"  AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,MB.Id,cg.UserName" + cList + @" " + cListId + @")
								AbsentEmployee
								ON
								AbsentEmployee.BudgetCode = m.Id  and e.CompanyGroupId = AbsentEmployee.CompanyGroupId AND e.CompanyId = AbsentEmployee.CompanyId AND e.IsDirect = AbsentEmployee.IsDirect AND  ISNULL(e.UId,'') = ISNULL(AbsentEmployee.UId,'')

								LEFT OUTER JOIN

								(SELECT POS.IsDirect,COUNT(E.SystemId) totalLateEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                   				LEFT OUTER JOIN  (--*
														SELECT * FROM EmployeeInformation
														WHERE SystemId IN (--**
														SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
															LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
															WHERE DT.Category = 'Late' AND CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
														)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								 -- LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
                            	LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
											" + join + @"
								WHERE
							    e.GroupID = '" + companyGroupId + @"' " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"'))  " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,MB.Id,cg.UserName " + cList + @" " + cListId + @") LateEmployee
                                ON LateEmployee.BudgetCode = m.Id  and e.CompanyGroupId = LateEmployee.CompanyGroupId AND e.CompanyId = LateEmployee.CompanyId AND e.IsDirect = LateEmployee.IsDirect AND  ISNULL(e.UId,'') = ISNULL(LateEmployee.UId,'')
								LEFT OUTER JOIN
								(SELECT POS.IsDirect,COUNT(E.SystemId) totalLeaveEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN  (--*
													SELECT * FROM EmployeeInformation
													WHERE SystemId IN (--**
																SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
																		LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
																	WHERE DT.Category = 'Leave' AND CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
																)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId

									LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
                                --LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + join + @"
								WHERE

							       e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"'))" + EmployeeCategory + @"

							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,MB.Id,cg.UserName " + cList + @" " + cListId + @") LeaveEmployee 
                                ON LeaveEmployee.BudgetCode = m.Id  and e.CompanyGroupId = LeaveEmployee.CompanyGroupId AND e.CompanyId = LeaveEmployee.CompanyId AND e.IsDirect = LeaveEmployee.IsDirect AND  ISNULL(e.UId,'') = ISNULL(LeaveEmployee.UId,'')
								LEFT OUTER JOIN
								    (SELECT POS.IsDirect, COUNT(E.SystemId) totalWeekoffEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
									C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
														SELECT * FROM EmployeeInformation
														WHERE SystemId IN (--**
																			SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
																				LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
																			WHERE DT.Category IN ('Holiday','Weekend') AND CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
																		)--**
														)--*
									E ON e.GroupID = CG.Id and c.Id=E.CompanyId

								 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId

	                            LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
									--  LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
									--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
									--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
									
                                    " + join + @"
									WHERE
									    e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
									GROUP BY C.UserName,cg.UserName,POS.IsDirect,C.Id,cg.Id,MB.Id,cg.UserName" + cList + @" " + cListId + @")
									WeekOffEmployee
									ON
									WeekOffEmployee.BudgetCode = m.Id  and e.CompanyGroupId = WeekOffEmployee.CompanyGroupId AND e.CompanyId = WeekOffEmployee.CompanyId AND e.IsDirect = WeekOffEmployee.IsDirect AND  ISNULL(e.UId,'') = ISNULL(WeekOffEmployee.UId,'')
---LateIn----
	LEFT OUTER JOIN

								(SELECT POS.IsDirect,COUNT(E.SystemId) totalLateInEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                   				 LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                                 SELECT DISTINCT EmpSystemID FROM AttendanceInfoExtra   AIE
                                    WHERE AIE.InfoType = 'LATEIN'  AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

								 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId

								 -- LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
                            	LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
											" + join + @"
								WHERE
							    e.GroupID = '" + companyGroupId + @"' " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"'))  " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,MB.Id,cg.UserName " + cList + @" " + cListId + @") LATEINEmployee
                                ON LATEINEmployee.BudgetCode = m.Id  and e.CompanyGroupId = LATEINEmployee.CompanyGroupId AND e.CompanyId = LATEINEmployee.CompanyId AND e.IsDirect = LATEINEmployee.IsDirect AND  ISNULL(e.UId,'') = ISNULL(LATEINEmployee.UId,'')
								
---LateIn End----
---EARLYOUT---
LEFT OUTER JOIN

								(SELECT POS.IsDirect,COUNT(E.SystemId) totalEarlyOutEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                   				 LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                                 SELECT DISTINCT EmpSystemID FROM AttendanceInfoExtra   AIE
                                    WHERE AIE.InfoType = 'EARLYOUT'  AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

								 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId

								 -- LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
                            	LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
											" + join + @"
								WHERE
							    e.GroupID = '" + companyGroupId + @"' " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"'))  " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,MB.Id,cg.UserName " + cList + @" " + cListId + @") EARLYOUTEmployee
                                ON EARLYOUTEmployee.BudgetCode = m.Id  and e.CompanyGroupId = EARLYOUTEmployee.CompanyGroupId AND e.CompanyId = EARLYOUTEmployee.CompanyId AND e.IsDirect = EARLYOUTEmployee.IsDirect AND  ISNULL(e.UId,'') = ISNULL(EARLYOUTEmployee.UId,'')
			
---EARLYOUT End---

---LUNCHOUT---
LEFT OUTER JOIN

								(SELECT POS.IsDirect,COUNT(E.SystemId) totalLounchOutEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                   				 LEFT OUTER JOIN  (--*
                                SELECT * FROM EmployeeInformation
                                WHERE SystemId IN(--**
                                 SELECT DISTINCT EmpSystemID FROM AttendanceInfoExtra   AIE
                                    WHERE AIE.InfoType = 'LUNCHOUT'  AND CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + hrDate + @"')  and ISNULL(OutTime,'')<>'' and  ISNULL(InTime,'')= ''
								)--**
								)--*
                                E ON e.GroupID = CG.Id AND c.Id = E.CompanyId

								 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId

								 -- LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
                            	LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
											" + join + @"
								WHERE
							    e.GroupID = '" + companyGroupId + @"' " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"'))  " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,POS.IsDirect,MB.Id,cg.UserName " + cList + @" " + cListId + @") LounchOutEmployee
                                ON LounchOutEmployee.BudgetCode = m.Id  and e.CompanyGroupId = LounchOutEmployee.CompanyGroupId AND e.CompanyId = LounchOutEmployee.CompanyId AND e.IsDirect = LounchOutEmployee.IsDirect AND  ISNULL(e.UId,'') = ISNULL(LounchOutEmployee.UId,'')
			
---LUNCHOUT End---


								LEFT OUTER JOIN
								(SELECT POS.IsDirect, COUNT(E.SystemId) totalShiftNotAssignedEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
									C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN  (--*
													SELECT * FROM EmployeeInformation
													WHERE SystemId NOT IN (--**
																		SELECT DISTINCT EmpSystemID FROM EmployeeShiftAssign
																		)--**
													)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
	                              LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								--LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + join + @"
								WHERE
							       e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
							    GROUP BY C.UserName,cg.UserName,POS.IsDirect,C.Id,cg.Id,MB.Id,cg.UserName " + cList + @" " + cListId + @")
								ShiftNotAssignedEmployee ON
								ShiftNotAssignedEmployee.BudgetCode = m.Id  and e.CompanyGroupId = ShiftNotAssignedEmployee.CompanyGroupId AND e.CompanyId = ShiftNotAssignedEmployee.CompanyId AND e.IsDirect = ShiftNotAssignedEmployee.IsDirect AND  ISNULL(e.UId,'') = ISNULL(ShiftNotAssignedEmployee.UId,'')
                                    LEFT OUTER JOIN
									(
									 SELECT POS.IsDirect, count(E.SystemID) totalAttdnNotProcessedToday,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
									C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
										FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id=E.CompanyId

											Inner JOIN(--*
															   SELECT TOP 1 WITH TIES *
																FROM EmployeeShiftAssign
																WHERE EffectiveDate <= GETDATE() and
																EmpSystemID NOT IN(--**
																							SELECT DISTINCT EmpSystemID FROM AttdnProcessData
																							WHERE  CONVERT(DATE, WorkDate) =  CONVERT(DATE, '" + hrDate + @"')
																					)
																ORDER BY ROW_NUMBER() OVER(PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC)
															  )-- *
														ESA
											ON E.SystemId = ESA.EmpSystemID

									 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
	                            LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								    --LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
									--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
									--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
									" + join + @"

									WHERE
									  e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"'))" + EmployeeCategory + @"
                                            AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')
							    GROUP BY C.UserName,cg.UserName,POS.IsDirect,C.Id,cg.Id,MB.Id ,cg.UserName " + cList + @" " + cListId + @") 
                                AttdnNotProcessedToday ON AttdnNotProcessedToday.BudgetCode = m.Id  and e.CompanyGroupId = AttdnNotProcessedToday.CompanyGroupId AND e.CompanyId = AttdnNotProcessedToday.CompanyId AND e.IsDirect = AttdnNotProcessedToday.IsDirect  AND  ISNULL(e.UId,'') = ISNULL(AttdnNotProcessedToday.UId,'')
									LEFT OUTER JOIN
									(
									SELECT POS.IsDirect,COUNT(ESA.SystemID) totalShiftNotAssignAsofToday,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
									C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id=E.CompanyId
									LEFT OUTER JOIN  (--*
														SELECT SystemID FROM EmployeeInformation EI
															WHERE EI.SystemID NOT IN (--**
																  SELECT DISTINCT EmpSystemID FROM EmpDateWiseShiftAssign
																	WHERE  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
													)--**
									)--*
									ESA
									ON E.SystemId = ESA.SystemId

								    LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId

								   -- LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--	LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--	LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
	                            LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + join + @"

									WHERE

									 e.GroupID = '" + companyGroupId + @"'   " + wc + @"   AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                      AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')
							    GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName,MB.Id,POS.IsDirect " + cList + @" " + cListId + @") ShiftNotAssignAsofToday
                    ON ShiftNotAssignAsofToday.BudgetCode = m.Id  and e.CompanyGroupId = ShiftNotAssignAsofToday.CompanyGroupId AND e.CompanyId = ShiftNotAssignAsofToday.CompanyId AND e.IsDirect = ShiftNotAssignAsofToday.IsDirect  AND  ISNULL(e.UId,'') = ISNULL(ShiftNotAssignAsofToday.UId,'')
	                            LEFT JOIN
								(
								SELECT POS.IsDirect, COUNT(E.SystemId) totalLongAbsentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,MB.Id BudgetCode,
								C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @" AS ColumnName
										" + cListId + @" AS UId
								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

								LEFT OUTER JOIN  EmployeeInformation
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

								 LEFT JOIN HKP.LegalDesignation GDes ON GDes.Id = E.LegalDesignationId
                                    LEFT JOIN  Mst.DesignationMasterLegalDesignation DesM ON DesM.LegalDesignationId =  GDes.Id 
								    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = DesM.LegalDesignationId
                                    LEFT JOIN[HKP].EmployeeCategory EmpC ON EmpC.Id = DM.EmployeeCategoryId
	                            
                                LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.CompanyId = c.Id
								INNER JOIN [MST].[ManpowerBudget] AS MB  on  ENT.Id = MB.EntityId  and MB.Id = E.BudgetCode  
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
								 -- LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								--LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								--LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
                                                " + join + @"
								WHERE
							       e.GroupID = '" + companyGroupId + @"'   " + wc + @" AND E.EmployeeCurrentStatus = 'LONG ABSENTEEISM' AND (E.EmployeeStatus != 'Separated'  OR E.DOS >= '" + hrDate + @"') 
							    GROUP BY C.UserName,POS.IsDirect,cg.UserName,C.Id,MB.Id,cg.Id,cg.UserName " + cList + @" " + cListId + @")
								LONGABSENTISM ON   LONGABSENTISM.BudgetCode = m.Id AND e.CompanyGroupId = LONGABSENTISM.CompanyGroupId AND e.CompanyId = LONGABSENTISM.CompanyId AND e.IsDirect = LONGABSENTISM.IsDirect AND  ISNULL(e.UId,'') = ISNULL(LONGABSENTISM.UId,'')
                                

                                 -------------------------3b--------------------------------------------------------
                                  LEFT OUTER JOIN
                                (
                                SELECT MBD.TotalNumber, ManpowerBudgetId,Cg.Id as CgId, C.Id as cid   " + cListextIdR + @"

                                FROM

                                (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
									FROM [MST].[ManpowerBudgetDetail]
									WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + hrDate + @"')
									ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                 ) MBD

                                  LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId

                                  LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId

                                  LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                  LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

                                  LEFT OUTER JOIN [ORG].[Position] AS POS ON POS.Id = MB.PositionId

										LEFT JOIN [HKP].Designation GDes ON GDes.Id = POS.DesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = GDes.Id
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                   " + join + @"

                                 WHERE CG.Id = '" + companyGroupId + @"' " + wc + @"  " + EmployeeCategory + @" AND TotalNumber > 0
                                 ) B
                                 ON m.id = b.ManpowerBudgetId and b.CgId = m.CompanyGroupId and B.cid = m.CompanyId   " + cListFinish + @"
                                 ) ede  GROUP BY IsDirect,CompanyGroupId, CompanyId,ColumnName,UId,Sequence " + cListextF + @"" + cListextIdF + @" ORDER BY Sequence";

                DataTable dt = _sqlRepository.GetDataTable(sql);

                DataTable dtTemp = dt.Clone();
                if (dt.Rows.Count > 0)
                {
                    dtTemp = dt.AsEnumerable().GroupBy(x => new
                    {
                        CompanyId = x["CompanyId"],
                        UId = x["UId"],
                        ColumnName = x["ColumnName"],
                        //GroupName = x["GroupName"],
                        CompanyGroupId = x["CompanyGroupId"],
                        IsDirect = "General"

                    })
                .Select(x =>
                {
                    DataRow row = dt.NewRow();
                    row["IsDirect"] = "General";
                    row["UId"] = x.Key.UId; row["ColumnName"] = x.Key.ColumnName; row["CompanyId"] = x.Key.CompanyId; row["CompanyGroupId"] = x.Key.CompanyGroupId;
                    row["totalEarlyOutEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalEarlyOutEmployee"]));
                    row["totalLateInEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLateInEmployee"]));
                    row["totalLounchOutEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLounchOutEmployee"]));
                    row["ProposedManpowerBudget"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ProposedManpowerBudget"]));
                    row["OnRoleEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["OnRoleEmployee"])); row["totalPresentEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalPresentEmployee"]));
                    row["totalAbsentEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalAbsentEmployee"])); row["totalLateEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLateEmployee"]));
                    row["totalLeaveEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLeaveEmployee"])); row["totalWeekoffEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalWeekoffEmployee"]));
                    row["ShiftNotAssignedEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["ShiftNotAssignedEmployee"])); row["totalAttdnNotProcessedToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalAttdnNotProcessedToday"]));
                    row["totalShiftNotAssignAsofToday"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalShiftNotAssignAsofToday"])); row["totalLongAbsentismEmployee"] = x.Sum(r => (decimal)OTSBD.clsStaticInfo.dbl(r["totalLongAbsentismEmployee"]));
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

        #endregion Attendance Status

        //------------------------------------------------------Modal----------------------------------------------------------//

        private string dayStatus = string.Empty;

        private string inTime = string.Empty;

        #region syncfusion OnRole DataGrid
        public IEnumerable<object> ModalOnRoleEmployeeList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            var sqltext = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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
                sqltext = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode

                            ,ShiftInTime =  CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SDE.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100) end
                                            ,ISNULL(SDE.UserName,'-') Shift
                                            ,ISNULL(LDes.UserName,'-') Designation
                                            ,ISNULL(LDes.Id,'-') DesignationId
                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
                                         ,EmpC.UserName EmpCategory											
                                            ,ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName
											,ISNULL(OM.id,'-') OperationMasterId
                                            ,ISNULL(OM.Code,'-') OperationCode 
                                            ,cg.UserName GroupName
											,cg.Id CompanyGroupId
                                         ,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(edsgg.Id,'-') DesignationGroupId,ISNULL(GDes.UserName,'-') GivenDesignation								
									,C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @"
                                    ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId AND CONVERT(DATE,APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
     							LEFT JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID AND CONVERT(DATE,APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate

	                            LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m

                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID where CONVERT(DATE,c.ShiftDate) =  CONVERT(DATE,'" + hrDate + @"')
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = APD.WorkDate
								LEFT  JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID
                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								  LEFT OUTER JOIN HKP.Designation edsg on edsg.id=POS.DesignationID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=DesM.DesignationGroupId
								 " + Join + @"
		                        WHERE E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')";

                return _sqlRepository.GetDataCollection(sqltext);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ModalHRDailyPresentStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    //wc += " AND " + item.StandardName + "Defination.SystemId='" + item.Text + "'";
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

                string CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode
                     
                      
                            ,ShiftInTime =  CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100) end
      ,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime
   ,SD.ShiftDefinationName ShiftDefinationName
,ISNULL(LDes.UserName,'-') Designation,ISNULL(LDes.Id,'-') DesignationId
              ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ    
                            ,EmpC.UserName EmpCategory
                                            ,ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName										
                                            ,ISNULL(OM.id,'-') OperationMasterId  
											,ISNULL(OM.Code,'-') OperationCode 
              
                                ,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName CompanyName
                                ,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(edsgg.Id,'-') DesignationGroupId,ISNULL(GDes.UserName,'-') GivenDesignation	--,ISNULL(Line.UserName,'-') Line
									" + cList + @"
                                ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate
								LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m

                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = APD.WorkDate


								LEFT  JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID
                                LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
                                --LEFT OUTER JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
								LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                --LEFT JOIN ORG.Line Line ON Line.Id = E.LineId
								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								 " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"') AND  DT.Category = 'Present' AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"') 
								  ";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ModalHRDailyAbsentStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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

                string CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode
                                  
                                   ,ShiftInTime =  CASE
							                       WHEN cs.InTime IS NULL
							                       THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							                       ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100) End
                                 ,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime
                                ,SD.ShiftDefinationName ShiftDefinationName
                           ,ISNULL(LDes.UserName,'-') Designation,ISNULL(LDes.Id,'-') DesignationId,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,ISNULL(EmpC.UserName,'') EmployeeCategory
                            ,ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName
											,ISNULL(OM.id,'-') OperationMasterId 
                                            ,ISNULL(OM.id,'-') OperationMasterId  
											,ISNULL(OM.Code,'-') OperationCode 
                                
                        ,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName CompanyName
                                 ,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(edsgg.Id,'-') DesignationGroupId,ISNULL(GDes.UserName,'-') GivenDesignation	
									" + cList + @"
                                ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
                                LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate
								LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m

                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = APD.WorkDate


								LEFT  JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
								LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                --LEFT JOIN ORG.Line Line ON Line.Id = E.LineId
								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
							    " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"') AND  DT.Category = 'Absent' AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"') ";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ModalLongAbsenteismStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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

                string CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,SD.ShiftDefinationName ShiftDefinationName,LTRIM(RIGHT(CONVERT(VARCHAR(25),SD.InTime, 100), 7)) ShiftInTime,EmpC.UserName EmpCategory,ISNULL(LDes.UserName,'-') Designation,ISNULL(LDes.Id,'-') DesignationId
                                            , ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName
											,ISNULL(OM.id,'-') OperationMasterId 
                                            ,ISNULL(OM.id,'-') OperationMasterId  
											,ISNULL(OM.Code,'-') OperationCode 								
                                 ,C.Id AS CompanyId,C.UserName CompanyName
                                 ,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(GDes.UserName,'-') GivenDesignation	
									" + cList + @"
                                 ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate
                                LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
								LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                --LEFT JOIN ORG.Line Line ON Line.Id = E.LineId
								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
							    " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND E.EmployeeCurrentStatus = 'LONG ABSENTEEISM'  AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')  AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"') ";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ModalHRDailyLeaveStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            var strSql = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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

                strSql = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,ISNULL(LT.Code,'') LeaveType,REPLACE(CONVERT(VARCHAR(11), LTR.AppliedDate, 106), ' ', '-') AppliedDate,isnull(SDE.UserName,'') ShiftDefinationName,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,ISNULL(LDes.UserName,'-') Designation,ISNULL(LDes.Id,'-') DesignationId,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ ,EmpC.UserName EmpCategory
                                    ,ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName
											,ISNULL(OM.id,'-') OperationMasterId 
                                            ,ISNULL(OM.id,'-') OperationMasterId  
											,ISNULL(OM.Code,'-') OperationCode 
                               ,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.Id,'-') DesignationGroupId,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(GDes.UserName,'-') GivenDesignation	
                                 ,C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @"
                                ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
                                LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId

								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate
						LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m

                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = APD.WorkDate


								LEFT  JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
								LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                --LEFT JOIN ORG.Line Line ON Line.Id = E.LineId
								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
								LEFT OUTER JOIN LeaveTransaction LTR ON APD.LTSystemID = LTR.LTSystemID AND CONVERT(DATE,LTR.AppliedDate) = CONVERT(DATE,'" + hrDate + @"')
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								 " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"') AND  DT.Category = 'Leave' AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"') ";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ModalHRDailyLateStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            var strSql = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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

                strSql = @"  SELECT * FROM (SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode
 , CONVERT(CHAR(5), CONVERT(TIME,APD.InTime - (CASE WHEN CS.InTime IS NULL THEN 
 Format(APD.InTime, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END
 ELSE CS.InTime END)
 ) , 108) LateBy
                                , ShiftInTime =  CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100) end
                            ,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime
                            ,SDE.ShiftDefinationName ShiftDefinationName
                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
                                            ,ISNULL(LDes.UserName,'-') Designation
                                            ,ISNULL(LDes.Id,'-') DesignationId
                                            ,LDes.Sequence  LegalDesignationSequenceId
								            ,ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName
											
                                            ,ISNULL(OM.id,'-') OperationMasterId  
											,ISNULL(OM.Code,'-') OperationCode 		
                                
										,cg.Id CompanyGroupId,cg.UserName GroupName
                                 ,EmpC.UserName EmpCategory,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(GDes.UserName,'-') GivenDesignation	
                                ,C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @"
                                ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
                                LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
                                
                                LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate
								LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m

                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = APD.WorkDate


								LEFT  JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
								LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                --LEFT JOIN ORG.Line Line ON Line.Id = E.LineId 
								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								 " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"') AND  DT.Category = 'Late' AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
                                ) Late ORDER BY LateBy DESC,LegalDesignationSequenceId";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> ModalHRDailyLateInStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            var strSql = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListName += "," + item.StandardName;
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
                            cListName += "," + item.StandardName;
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                        }
                        if (item.RType == "Z")
                        {
                            cListName += "," + item.StandardName + "Defination";
                            cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                            Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MB." + item.StandardName + "DefinationId\n";
                        }
                        if (item.RType == "ZA")
                        {
                            cListName += "," + item.StandardName;
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
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

                strSql = @" SELECT SystemId,EmployeeName,EmployeeCode, CONVERT(CHAR(5), CONVERT(TIME,APDIntimeId - ShiftInTime) , 108) LateBy,ShiftInTime,inTime
 ,ShiftDefinationName,DOJ,Designation,OperationActivityName,OperationCode,OperationMasterName,GroupName,EmpCategory,BudgetedDesignation
 ,DesignationGroup,GivenDesignation,CompanyName" + cListName + @",CellPhnNo FROM (SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode, CONVERT(CHAR(5), CONVERT(TIME,APD.InTime - EDWSA.ShiftInTime) , 108) LateBy
                                       
                                , ShiftInTime =  CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100) end
                            ,APD.InTime APDIntimeId 
                            ,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime
                            ,SDE.ShiftDefinationName ShiftDefinationName
                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
                                            ,ISNULL(LDes.UserName,'-') Designation
                                            ,ISNULL(LDes.Id,'-') DesignationId
                                            ,LDes.Sequence  LegalDesignationSequenceId
								            ,ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName
											
                                            ,ISNULL(OM.id,'-') OperationMasterId  
											,ISNULL(OM.Code,'-') OperationCode 		
                                
										,cg.Id CompanyGroupId,cg.UserName GroupName
                                 ,EmpC.UserName EmpCategory,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(GDes.UserName,'-') GivenDesignation	
                                ,C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @"
                                ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
										LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId and CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
								LEFT OUTER JOIN AttendanceInfoExtra  AIE ON AIE.EmpSystemID = E.SystemId and  CONVERT(DATE, AIE.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
								
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
                                LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
                                
                                LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = AIE.EmpSystemID AND EDWSA.WorkDate = AIE.WorkDate
								LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m

                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = AIE.WorkDate


								LEFT  JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
								LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                          
							
                               
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								 " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"') AND  AIE.InfoType = 'LATEIN' AND CONVERT(DATE, AIE.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
                                ) Late ORDER BY LateBy DESC,LegalDesignationSequenceId";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> ModalHRDailyEarlyOutStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            var strSql = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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

                strSql = @" SELECT * FROM (SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode, CONVERT(CHAR(5), CONVERT(TIME,APD.InTime - EDWSA.ShiftInTime) , 108) LateBy
                                       
                                , ShiftInTime =  CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100) end
                            ,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime
                            ,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.OutTime, 100), 7)) OutTime
                            ,SDE.ShiftDefinationName ShiftDefinationName
                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
                                            ,ISNULL(LDes.UserName,'-') Designation
                                            ,ISNULL(LDes.Id,'-') DesignationId
                                            ,LDes.Sequence  LegalDesignationSequenceId
								            ,ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName
											
                                            ,ISNULL(OM.id,'-') OperationMasterId  
											,ISNULL(OM.Code,'-') OperationCode 		
                                
										,cg.Id CompanyGroupId,cg.UserName GroupName
                                 ,EmpC.UserName EmpCategory,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(GDes.UserName,'-') GivenDesignation	
                                ,C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @"
                                ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
										LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId and CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
								LEFT OUTER JOIN AttendanceInfoExtra  AIE ON AIE.EmpSystemID = E.SystemId and  CONVERT(DATE, AIE.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
								
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
                                LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
                                
                                LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = AIE.EmpSystemID AND EDWSA.WorkDate = AIE.WorkDate
								LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m

                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = AIE.WorkDate


								LEFT  JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
								LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                          
							
                               
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								 " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"') AND  AIE.InfoType = 'EARLYOUT' AND CONVERT(DATE, AIE.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
                                ) Late ORDER BY LateBy DESC,LegalDesignationSequenceId";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> ModalHRDailyLunchOutStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            var strSql = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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

                strSql = @" SELECT * FROM (SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode, CONVERT(CHAR(5), CONVERT(TIME,APD.InTime - EDWSA.ShiftInTime) , 108) LateBy
                                       
                                , ShiftInTime =  CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100) end
                            ,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime
                            ,LTRIM(RIGHT(CONVERT(VARCHAR(25), AIE.OutTime, 100), 7)) OutTime

                            ,SDE.ShiftDefinationName ShiftDefinationName
                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
                                            ,ISNULL(LDes.UserName,'-') Designation
                                            ,ISNULL(LDes.Id,'-') DesignationId
                                            ,LDes.Sequence  LegalDesignationSequenceId
								            ,ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName
											
                                            ,ISNULL(OM.id,'-') OperationMasterId  
											,ISNULL(OM.Code,'-') OperationCode 		
                                
										,cg.Id CompanyGroupId,cg.UserName GroupName
                                 ,EmpC.UserName EmpCategory,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(GDes.UserName,'-') GivenDesignation	
                                ,C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @"
                                ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
										LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId and CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
								LEFT OUTER JOIN AttendanceInfoExtra  AIE ON AIE.EmpSystemID = E.SystemId and  CONVERT(DATE, AIE.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
								
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
                                LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
                                
                                LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = AIE.EmpSystemID AND EDWSA.WorkDate = AIE.WorkDate
								LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m

                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = AIE.WorkDate


								LEFT  JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
								LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                          
							
                               
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								 " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"') AND  AIE.InfoType = 'LUNCHOUT' AND CONVERT(DATE, AIE.WorkDate) = CONVERT(DATE,'" + hrDate + @"')  and ISNULL(AIE.OutTime,'') <>'' and  ISNULL(AIE.InTime,'')= ''
                                ) Late ORDER BY LateBy DESC,LegalDesignationSequenceId";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> ModalHRDailyShiftNotAssignedStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            var strSql = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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

                strSql = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
                                            --, ShiftInTime =  CASE
							-- WHEN cs.InTime IS NULL
							 --THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 --ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
                                       ,cg.Id CompanyGroupId,cg.UserName GroupName--,ISNULL(Line.UserName,'-') Line
								,C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @"
                            ,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
									INNER  JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

								INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id=E.CompanyId
									INNER JOIN  (--*
														SELECT SystemID FROM EmployeeInformation EI
															WHERE EI.SystemID NOT IN (--**
																  SELECT DISTINCT EmpSystemID FROM EmployeeShiftAssign
																	--WHERE  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
													)--**
									)--*
									ESA
									ON E.SystemId = ESA.SystemId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
			
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                                --LEFT JOIN ORG.Line Line ON Line.Id = E.LineId 
								 " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion

        public IEnumerable<object> ModalHRDailyAttdnNotProcessedStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            try
            {
                string strSql = "";
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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

                strSql = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,ISNULL(SDE.ShiftDefinationName,'') ShiftDefinationName
                            ,ShiftInTime =  CASE
							    WHEN cs.InTime IS NULL
							    THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							    ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100) End
                            ,ISNULL(LDes.UserName,'') Designation
                            ,ISNULL(LDes.Id,'') DesignationId
                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ

                            ,ISNULL(OA.UserName,'-') OperationActivityName
							,ISNULL(OA.Id,'-') OperationActivityId
							,ISNULL(OM.UserName,'-') OperationMasterName
							,ISNULL(OM.id,'-') OperationMasterId 
                            ,ISNULL(OM.id,'-') OperationMasterId  
							,ISNULL(OM.Code,'-') OperationCode 		
,cg.Id CompanyGroupId,cg.UserName GroupName
                            ,C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @"
,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								INNER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

								INNER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								left outer join EmployeeShiftAssign ASS on ass.EmpSystemID=e.SystemId and ass.SystemID=(select TOP 1 SystemID FROM EmployeeShiftAssign where CONVERT(Date,EffectiveDate) <= Convert(Date,'" + hrDate + @"') AND EmpSystemID=e.SystemId ORDER BY EffectiveDate DESC)
								--INNER JOIN(--*
															   --SELECT TOP 1 WITH TIES *
																--FROM EmployeeShiftAssign
															--	WHERE EffectiveDate <= GETDATE() and
															--	EmpSystemID NOT IN(--**
															--								SELECT DISTINCT EmpSystemID FROM AttdnProcessData
															---								WHERE  CONVERT(DATE, WorkDate) =  CONVERT(DATE,'" + hrDate + @"')
															--						)
													--			ORDER BY ROW_NUMBER() OVER(PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC)
												---			  )-- *
											--	ESA
											--ON E.SystemId = ESA.EmpSystemID
								--LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = ESA.EmpSystemID
								LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = ASS.EmpSystemID AND CONVERT(DATE,APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
		LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								Left outer JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = E.SystemID  AND CONVERT(DATE,EDWSA.WorkDate) = CONVERT(DATE,'" + hrDate + @"')
								LEFT JOIN ShiftDefination SD ON SD.SystemID = EDWSA.ShiftSystemID
                                LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
	
                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = APD.WorkDate


								LEFT  JOIN ShiftDefination SDE ON SDE.SystemID = EDWSA.ShiftSystemID
														--ESA
											--ON E.SystemId = ESA.EmpSystemID
								--LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = ESA.EmpSystemID
								--LEFT JOIN ShiftDefination SD ON SD.SystemID = ESA.FixSystemID OR SD.SystemID = ESA.RosterStartShiftID
								--LEFT JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = E.SystemID AND SD.SystemID =EDWSA.ShiftSystemID AND CONVERT(DATE,EDWSA.WorkDate) = CONVERT(DATE," + hrDate + @")

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
                                    
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								 " + Join + @"
                                WHERE ISNULL(apd.WorkDate,'')='' AND
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ModalHRDailyOffDayStatusList(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            string strSql = "";
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
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

                strSql = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,LTRIM(RIGHT(CONVERT(VARCHAR(25), SD.InTime, 100), 7)) ShiftInTime,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,SD.ShiftDefinationName ShiftDefinationName
								            ,ISNULL(LDes.UserName,'-') Designation
                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
                                            ,ISNULL(EmpC.UserName,'') EmployeeCategory
                                            ,ISNULL(LDes.Id,'-') DesignationId 
                                            ,ISNULL(OA.UserName,'-') OperationActivityName
											,ISNULL(OA.Id,'-') OperationActivityId
											,ISNULL(OM.UserName,'-') OperationMasterName
											,ISNULL(OM.id,'-') OperationMasterId 
                                            ,ISNULL(OM.id,'-') OperationMasterId  
											,ISNULL(OM.Code,'-') OperationCode 	
                                ,cg.Id CompanyGroupId,cg.UserName GroupName
                                ,C.Id AS CompanyId,C.UserName CompanyName
									" + cList + @"
,ISNULL(E.CellPhnNo,'') CellPhnNo
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
                                LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								LEFT JOIN MSt.OperationMaster OM on OM.Id =  E.OperationMasterID
                                LEFT JOIN HKP.OperationActivity OA on OA.Id = OM.OperationActivityId
								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								 " + Join + @"
                                WHERE
								E.GroupID  = '" + companyGroupId + @"' " + wc + @"  AND (E.EmployeeStatus != 'Separated' OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrDate + @"')) " + EmployeeCategory + @"
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"') AND DT.Category IN ('Holiday','Weekend') AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"')";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void GetStatus(string status)
        {
            if (status == "A")
            {
                dayStatus = "AND  DT.Category = 'Absent' AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,GETDATE())";
                inTime = "";
            }
            else if (status == "P")
            {
                dayStatus = "AND  DT.Category = 'Present' AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,GETDATE())";
                inTime = " LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,";
            }
            else if (status == "L")
            {
                dayStatus = " AND  DT.Category = 'Late'  AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,GETDATE())";
                inTime = "LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,";
            }
            else if (status == "LV")
            {
                dayStatus = " AND  DT.Category = 'Leave' AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,GETDATE())";
                inTime = "";
            }
            else if (status == "SNA")
            {
                dayStatus = "AND E.SystemId NOT IN(SELECT DISTINCT EmpSystemID FROM EmployeeShiftAssign )";
                inTime = "";
            }
            else if (status == "ANP")
            {
                dayStatus = @"AND ESA.EmpSystemID NOT IN(SELECT DISTINCT EmpSystemID FROM AttdnProcessData
									WHERE CONVERT(DATE,WorkDate) = CONVERT(DATE, GETDATE())) ";
                inTime = "";
            }
            else if (status == "OFFDAY")
            {
                dayStatus = @"AND DT.Category IN ('Holiday','Weekend') AND CONVERT(DATE,APD.WorkDate) = CONVERT(DATE,GETDATE()) ";
                inTime = "";
            }
        }

        public IEnumerable<object> ModalOthersDetail(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var cListId = string.Empty;
            var wcc = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.RType == "Entity")
                        {
                            cList = "," + item.StandardName + ".UserName ";

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
                            cList = "," + item.StandardName + ".UserName " + item.StandardName + " ";
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
                            wc = " AND  E.CompanyId='" + item.Id + "'";
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {

                                if (item.RType == "Z")
                                {
                                    cListId = "," + item.StandardName + "Defination.SystemId";
                                    wc += " AND ISNULL(" + item.StandardName + "Defination.SystemId,'')='" + item.Text + "'";
                                }
                                else
                                {
                                    cListId = "," + item.StandardName + ".Id";
                                    wc += " AND " + item.StandardName + ".Id='" + item.Text + "'";
                                }
                            }
                        }
                    }
                }

                var gsql = @"SELECT OnRoleEmployee.CompanyId,OnRoleEmployee.UId,
									ISNULL(OnRoleEmployee.totalEmployee,0) totalEmployee,
									ISNULL(ShiftNotAssignedEmployee.totalShiftNotAssignedEmployee,0) totalShiftNotAssignedEmployee,
									ISNULL(WeekOffEmployee.totalWeekoffEmployee,'')totalWeekoffEmployee,
									ISNULL(ShiftNotAssignAsofToday.totalShiftNotAssignAsofToday,0) totalShiftNotAssignAsofToday,
									ISNULL(AttdnNotProcessedAsofToday.totalAttdnNotProcessedAsofToday,0) totalAttdnNotProcessedAsofToday
									FROM
								   (SELECT COUNT(E.SystemId) totalEmployee,cg.Id CompanyGroupId,C.Id CompanyId ,C.Id " + cListId + @" UId,cg.UserName GroupName
											FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											LEFT OUTER JOIN EmployeeInformation
											E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

										LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
												" + Join + @"

											WHERE
												 E.GroupID = '" + companyGroupId + @"'   " + wc + @"  AND  (E.DOJ<='" + hrDate + @"' AND (E.DOS is null or E.DOS >= '" + hrDate + @"'))" + EmployeeCategory + @"

												GROUP BY C.UserName,cg.Id,c.Id,cg.UserName " + cListId + @" ) OnRoleEmployee
								LEFT OUTER JOIN
                                (SELECT COUNT(E.SystemId) totalShiftNotAssignedEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName " + cListId + @" UId

								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId NOT IN (--**
								SELECT DISTINCT EmpSystemID FROM EmployeeShiftAssign
								)--**
								)--*
								E ON e.GroupID = CG.Id and c.Id=E.CompanyId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
								LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + Join + @"

								WHERE
							       E.GroupID = '" + companyGroupId + @"'    " + wc + @"  " + EmployeeCategory + @"
                                    AND  (E.DOJ<='" + hrDate + @"' AND (E.DOS is null or E.DOS >= '" + hrDate + @"'))
							    GROUP BY C.UserName,C.Id,cg.Id,cg.UserName " + cListId + @"  ) ShiftNotAssignedEmployee ON OnRoleEmployee.CompanyGroupId = ShiftNotAssignedEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = ShiftNotAssignedEmployee.CompanyId
                                    LEFT OUTER JOIN
									(
                                    SELECT count(ESA.SystemID) totalShiftNotAssignAsofToday,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName " + cListId + @" UId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id=E.CompanyId
									LEFT OUTER JOIN  (--*
									SELECT SystemID FROM EmployeeInformation EI
									WHERE EI.SystemID NOT IN (--**
									SELECT DISTINCT EmpSystemID FROM EmpDateWiseShiftAssign
									WHERE  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
									)--**
									)--*
									ESA
									ON E.SystemId = ESA.SystemId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								    LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
									LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
									LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
														" + Join + @"

									WHERE
									  E.GroupID = '" + companyGroupId + @"'   " + wc + @"    AND  (E.DOJ<='" + hrDate + @"' AND (E.DOS is null or E.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @"
									GROUP BY C.UserName,cg.UserName,C.Id,cg.Id,cg.UserName " + cListId + @"  ) ShiftNotAssignAsofToday ON OnRoleEmployee.CompanyGroupId = ShiftNotAssignAsofToday.CompanyGroupId AND OnRoleEmployee.CompanyId = ShiftNotAssignAsofToday.CompanyId
									LEFT OUTER JOIN
									(
									SELECT COUNT(E.SystemId) totalWeekoffEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName " + cListId + @" UId

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                                  LEFT OUTER JOIN  (--*
									SELECT * FROM EmployeeInformation
									WHERE SystemId IN (--**
									SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category IN ('Holiday','Weekend') AND CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + hrDate + @"')
									)--**
									)--*
									E ON e.GroupID = CG.Id and c.Id=E.CompanyId

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									  LEFT outer join [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
									LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
									LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId
										" + Join + @"

									WHERE
									   E.GroupID = '" + companyGroupId + @"'   " + wc + @" AND   (E.DOJ<='" + hrDate + @"' AND (E.DOS is null or E.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @"
									GROUP BY C.UserName,C.Id,cg.Id,cg.UserName " + cListId + @"
									) WeekOffEmployee ON OnRoleEmployee.CompanyGroupId = WeekOffEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = WeekOffEmployee.CompanyId
									LEFT OUTER JOIN
									(
									 SELECT count(E.SystemID) totalAttdnNotProcessedAsofToday, cg.Id CompanyGroupId, cg.UserName GroupName,
											C.Id AS CompanyId, C.UserName  UId
										FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id = E.CompanyId
											Inner JOIN(--*
															   SELECT TOP 1 WITH TIES *
																FROM EmployeeShiftAssign
																WHERE EffectiveDate <= GETDATE() and
																EmpSystemID NOT IN(--**
																							SELECT DISTINCT EmpSystemID FROM AttdnProcessData
																							WHERE  CONVERT(DATE, WorkDate) =  CONVERT(DATE, '" + hrDate + @"')
																					)
																ORDER BY ROW_NUMBER() OVER(PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC)
															  )-- *
														ESA
											ON E.SystemId = ESA.EmpSystemID

										LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on  MB.Id = E.BudgetCode
									LEFT OUTER JOIN  ORG.Entity AS ENT ON ENT.Id = MB.EntityId
									LEFT OUTER JOIN  ORG.Position AS POS ON POS.Id = MB.PositionId

									" + Join + @"

									WHERE
									   E.GroupID = '" + companyGroupId + @"' " + wc + @" AND (E.DOJ<='" + hrDate + @"' AND (E.DOS is null or E.DOS >= '" + hrDate + @"')) " + EmployeeCategory + @"
                                        AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrDate + @"')
									GROUP BY C.UserName,C.Id,cg.Id,cg.UserName " + cListId + @"
									) AttdnNotProcessedAsofToday ON OnRoleEmployee.CompanyGroupId = AttdnNotProcessedAsofToday.CompanyGroupId AND OnRoleEmployee.CompanyId = AttdnNotProcessedAsofToday.CompanyId";

                return _sqlRepository.GetDataCollection(gsql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region Modal of  Increament Due

        public IEnumerable<object> ListOfIncrementDue(IEnumerable<ChartColumnList> ChartColumnList, int seq, string condition, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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

            try
            {
                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                var incrementDueCondition = "";
                var overdue = "overdue";
                var today = "today";
                var n7d = "next7days";
                var n30d = "next30days";

                var wc = string.Empty;

                if (condition == overdue)
                {
                    incrementDueCondition = "CONVERT(DATE,Tem.NextDueDate) <= CONVERT(DATE,'" + hrDate + @"')";
                }
                else if (condition == today)
                {
                    incrementDueCondition = "CONVERT(DATE,Tem.NextDueDate) = CONVERT(DATE,'" + hrDate + @"')";
                }
                else if (condition == n7d)
                {
                    incrementDueCondition = @"CONVERT(DATE,Tem.NextDueDate) between CONVERT(DATE,'" + hrDate + @"') AND  DATEADD(day, 7, '" + hrDate + @"')";
                }
                else if (condition == n30d)
                {
                    incrementDueCondition = @"CONVERT(DATE,Tem.NextDueDate) between CONVERT(DATE,'" + hrDate + @"') AND DATEADD(day, 30, '" + hrDate + @"')";
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
                                    Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";

                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
                            }
                        }
                    }

                    var gsql = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB

                                    ,mpb.EntityId,mpb.PositionId
									--Increment Due list
									,DATEDIFF(day, '" + hrDate + @"', (Tem.NextDueDate)) IncDaysToGO
									,Tem.NextDueDate sIncrementNextDueDate
									,REPLACE(CONVERT(VARCHAR(11), Tem.NextDueDate, 106), ' ', '-') IncrementNextDueDate
									,REPLACE(CONVERT(VARCHAR(11), EfTem.EffectiveDate, 106), ' ', '-') IncrementEffectiveDate
                                    ,e.CellPhnNo
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    ,ld.UserName Designation
									,PO.Code PositionCode,EN.Code EntityCode
									       " + cList + @"
                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN org.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN org.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN org.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN org.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN org.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN org.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN org.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN org.Unit eu on eu.id=e.UnitId
                                    LEFT OUTER JOIN hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN hkp.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
									LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = mpb.CompanyGroupId
                                    LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mpb.CompanyId= C.Id
			                                       " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                     LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = e.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = e.SystemId
                                   	--LEFT OUTER JOIN  DBO.SalaryIncrementNextDueDate AS SINDD ON SINDD.EmpSystemId = e.SystemId

								    --LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
                                    WHERE   (E.DOJ<='" + hrDate + @"' AND (E.DOS is null or E.DOS >= '" + hrDate + @"'))   AND CG.Id = '" + companyGroupId + @"' " + EmployeeCategory + @"
									AND  " + incrementDueCondition + @"";

                    return _sqlRepository.GetDataCollection(gsql);
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
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";

                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
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
                                    }
                                    else
                                    {
                                        wc += " AND " + item.StandardName + ".Id='" + item.Text + "'";
                                    }
                                }
                            }
                        }
                    }

                    var gsql = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB

                                    ,mpb.EntityId,mpb.PositionId
									--Increment Due list
									,DATEDIFF(day, '" + hrDate + @"', (Tem.NextDueDate)) IncDaysToGO
									,Tem.NextDueDate sIncrementNextDueDate
									,REPLACE(CONVERT(VARCHAR(11), Tem.NextDueDate, 106), ' ', '-') IncrementNextDueDate
									,REPLACE(CONVERT(VARCHAR(11), EfTem.EffectiveDate, 106), ' ', '-') IncrementEffectiveDate
                                    ,e.CellPhnNo
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName Designation
									,PO.Code PositionCode,EN.Code EntityCode
									       " + cList + @"
                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN org.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN org.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN org.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN org.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN org.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN org.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN org.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN org.Unit eu on eu.id=e.UnitId
                                   -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN hkp.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                           -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
									LEFT outer JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = mpb.CompanyGroupId
                                    LEFT outer JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mpb.CompanyId= C.Id
			                                       " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId

						        LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = e.SystemId
									LEFT OUTER JOIN
									(select Max(EffectiveDate) EffectiveDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) EfTem on EfTem.EmpSystemId = e.SystemId
                                   	--LEFT OUTER JOIN DBO.SalaryIncrementNextDueDate AS SINDD ON SINDD.EmpSystemId = e.SystemId
								    --LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
                                    WHERE (E.DOJ<='" + hrDate + @"' AND (E.DOS is null or E.DOS >= '" + hrDate + @"'))   AND CG.Id = '" + companyGroupId + @"' " + wc + @" " + EmployeeCategory + @"
									AND " + incrementDueCondition + @" ";
                    return _sqlRepository.GetDataCollection(gsql, null);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal of  Increament Due

        #region Modal of Probation Period

        public IEnumerable<object> ListProbationOverDue(IEnumerable<ChartColumnList> ChartColumnList, int seq, string condition, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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
            try
            {
                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                var probCondition = "";
                var overdue = "overdue";
                var today = "today";
                var n7d = "next7days";
                var wc = string.Empty;

                if (condition == overdue)
                {
                    probCondition = "AND E.IsConfirmed = 0 AND CONVERT(DATE,(E.DOJ+(CASE WHEN E.DOCIsDay=1 THEN E.DOCDay ELSE E.DOCMonth*30 END))) < convert(date,'" + hrDate + @"')";
                }
                else if (condition == today)
                {
                    probCondition = "AND E.IsConfirmed = 0 AND CONVERT(DATE,(e.DOJ+(CASE WHEN e.DOCIsDay=1 THEN e.DOCDay ELSE e.DOCMonth*30 END))) = CONVERT(DATE,'" + hrDate + @"')";
                }
                else if (condition == n7d)
                {
                    probCondition = @"AND E.IsConfirmed = 0 AND
								(CONVERT(DATE, (e.DOJ + (CASE WHEN e.DOCIsDay = 1 THEN e.DOCDay    ELSE e.DOCMonth * 30 END))) > CONVERT(DATE, '" + hrDate + @"')
									AND
									CONVERT(DATE, (e.DOJ + (CASE WHEN e.DOCIsDay = 1 THEN e.DOCDay ELSE e.DOCMonth * 30 END))) <= DATEADD(day, 7, '" + hrDate + @"')
								)";
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
                                    Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";

                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
                            }
                        }

                        //if (item.Sequence != -2)
                        //{
                        //	if (item.Sequence == -1)
                        //	{
                        //		wc = " and c.id='" + item.Id + "'";
                        //	}
                        //	else
                        //	{
                        //		if (item.Sequence < seq)
                        //		{
                        //			wc += " and e." + item.StandardName + "='" + item.Text + "'";
                        //		}
                        //	}
                        //}
                    }

                    var gsql = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                    ,REPLACE(CONVERT(VARCHAR(11), e.ApprovedDateTime, 106), ' ', '-') ApprovedDateTime
                                    ,CONVERT(date, e.ApprovedDateTime) vApprovedDateTime
									--,ISNULL(e.IsApproved,0) IsApproved
                                    --Resignation
                                    ,ProbationPeriod= case when e.DOCIsDay=1 then e.DOCDay
									ELSE e.DOCMonth*30 end
									,e.DOCDay,e.DOCMonth
                                    ,e.EmployeeStatus EmployeeStatus
									,e.DOJ+(CASE WHEN e.DOCIsDay=1 THEN e.DOCDay
												ELSE e.DOCMonth*30 END) DOCs
									,Replace(CONVERT(VARCHAR(11),
									e.DOJ+(case when e.DOCIsDay=1 then e.DOCDay
												else e.DOCMonth*30 end)
									 , 106), ' ', '-') DOC
                                    ,Replace(CONVERT(VARCHAR(11),e.DOC,106),' ','-') empDOC
                                    ,Replace(CONVERT(VARCHAR(11),(e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)), 106), ' ', '-') AlermStartDate

                                    ,DATEDIFF(DAY, '" + hrDate + @"', (e.DOJ + (case when e.DOCIsDay=1 THEN e.DOCDay
									else e.DOCMonth*30 END))) DaysToGO
									
	                                ,e.DOJ + (CASE WHEN e.DOCIsDay=1 THEN e.DOCDay
									else e.DOCMonth*30 END - ProbationPeriodAlertBeforeDays)  AlermStartDateCmp
                                        
                                    
									--Probation Confirmation Date
									,CONVERT(DATE,e.ProbationConfirmEntryDate) ProbationConfirmEntryDate
                                    ,MPB.EntityId,mpb.PositionId
                                    ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info

									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup

                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
									" + cList + @"
                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = E.GroupID
									LEFT OUTER JOIN ORG.Company C ON C.Id = E.CompanyId

                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId

									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                                       " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                             
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                     WHERE   (E.DOJ<='" + hrDate + @"' AND (E.DOS is null or E.DOS >= '" + hrDate + @"'))  " + probCondition + "  " + wc + @" " + EmployeeCategory + @"";

                    return _sqlRepository.GetDataCollection(gsql);
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
                                    Join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";

                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
                            }

                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                wc = " and c.id = '" + item.Id + "'";
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

                    var gsql = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                    ,REPLACE(CONVERT(VARCHAR(11), e.ApprovedDateTime, 106), ' ', '-') ApprovedDateTime
                                    --Resignation
                                    ,ProbationPeriod= case when e.DOCIsDay=1 then e.DOCDay
									ELSE e.DOCMonth*30 end
									,e.DOCDay,e.DOCMonth
                                    ,e.EmployeeStatus EmployeeStatus
									,e.DOJ+(CASE WHEN e.DOCIsDay=1 THEN e.DOCDay
												ELSE e.DOCMonth*30 END) DOCs
									,Replace(CONVERT(VARCHAR(11),
									e.DOJ+(case when e.DOCIsDay=1 then e.DOCDay
												else e.DOCMonth*30 end)
									 , 106), ' ', '-') DOC
                                    ,Replace(CONVERT(VARCHAR(11),e.DOC,106),' ','-') empDOC
                                    ,Replace(CONVERT(VARCHAR(11),(e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)), 106), ' ', '-') AlermStartDate

                                    ,DATEDIFF(DAY,'" + hrDate + @"', (e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end))) DaysToGO
									
	                                ,e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)  AlermStartDateCmp

                                     
									--Probation Confirmation Date
									,CONVERT(DATE,e.ProbationConfirmEntryDate) ProbationConfirmEntryDate
                                    --,mpb.EntityId,mpb.PositionId

                                     --,e.DepartmentId,e.DivisionId,e.LineId
                                    --,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info

									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup

                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
									" + cList + @"
                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = E.GroupID
									LEFT OUTER JOIN ORG.Company C ON C.Id = E.CompanyId

                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId

									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                                       " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId                        
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                     WHERE   (E.DOJ<='" + hrDate + @"' AND (E.DOS is null or E.DOS >= '" + hrDate + @"'))  " + probCondition + " " + wc + @" " + EmployeeCategory + @" ";
                    return _sqlRepository.GetDataCollection(gsql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal of Probation Period

        #region Separated Employee List

        public IEnumerable<object> ListSeparationStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string condition, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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
            try
            {
                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                var separatCondition = "";
                var today = "today";
                var n7d = "next7days";
                var todayResApplied = "todyaResigApplied";
                var resigApprovedPanding = "resigApprovedPanding";
                var wc = "";

                if (condition == today)
                {
                    separatCondition = "e.EmployeeStatus = 'Separated'and CONVERT(DATE,e.DOSDate) = CONVERT(DATE,'" + hrDate + @"' ) AND RSG.ApprovalStatus = 'APPROVED'";
                }
                else if (condition == n7d)
                {
                    separatCondition = @"
								(CONVERT(DATE, RSG.ApprovedEffectiveDate) > CONVERT(DATE, '" + hrDate + @"' )
									AND
									CONVERT(DATE, RSG.ApprovedEffectiveDate) <= DATEADD(DAY, 7, '" + hrDate + @"')
								)

								AND RSG.ApprovalStatus = 'APPROVED'";
                }
                else if (condition == todayResApplied)
                {
                    separatCondition = @" CONVERT(DATE,RSG.AddedDate) = CONVERT(DATE,'" + hrDate + @"')
								AND RSG.ApprovalStatus = 'Pending'";
                }
                else if (condition == resigApprovedPanding)
                {
                    separatCondition = @" RSG.ApprovalStatus = 'Pending'";
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
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
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
                                    }
                                    else
                                    {
                                        wc += " AND " + item.StandardName + ".Id='" + item.Text + "'";
                                    }
                                }
                            }
                        }
                    }

                    var gsql = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                    ,REPLACE(CONVERT(VARCHAR(11), e.ApprovedDateTime, 106), ' ', '-') ApprovedDateTime
                                    --,CONVERT(date, e.ApprovedDateTime) vApprovedDateTime
									,ISNULL(e.IsApproved,0) IsApproved
                                    --Resignation
								    ,ISNULL(rsg.ApprovalStatus,'') rsgApprovalStatus
									,REPLACE(CONVERT(VARCHAR(11), rsg.ApprovedEffectiveDate, 106), ' ', '-') resignationApprovedEffectiveDate
                                    ,CONVERT(DATE, rsg.ApprovedEffectiveDate) resignationApprovedEffectiveDateS
									,REPLACE(CONVERT(VARCHAR(11), rsg.ResignationDate, 106), ' ', '-') ApplicantResignationDate
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOSDate, 106), ' ', '-') ApplicantSeparationDate
								    ,CONVERT(DATE, rsg.ApprovedDate) resignationApprovedEntryDateS
									,CONVERT(date,rsg.AddedDate) resignationAddedDate
								    ,CONVERT(DATE, rsg.EffectiveDate) EffectiveDateS
									,CONVERT(DATE, e.DOSDate) ApplicantSeparationDateEX
									,REPLACE(CONVERT(VARCHAR(11),rsg.EffectiveDate,106),' ','-') RsgSelfEffectiveDate
                                    ,ProbationPeriod= case when e.DOCIsDay=1 then e.DOCDay
									ELSE e.DOCMonth*30 end
									,e.DOCDay,e.DOCMonth
                                    ,e.EmployeeStatus EmployeeStatus
									,e.DOJ+(CASE WHEN e.DOCIsDay=1 THEN e.DOCDay
												ELSE e.DOCMonth*30 END) DOCs
									,Replace(CONVERT(VARCHAR(11),
									e.DOJ+(case when e.DOCIsDay=1 then e.DOCDay
												else e.DOCMonth*30 end)
									 , 106), ' ', '-') DOC
                                    ,Replace(CONVERT(VARCHAR(11),e.DOC,106),' ','-') empDOC
                                    ,Replace(CONVERT(VARCHAR(11),(e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)), 106), ' ', '-') AlermStartDate

                                    ,DATEDIFF(day, '" + hrDate + @"', (e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end))) DaysToGO
									,DATEDIFF(day, '" + hrDate + @"', rsg.ApprovedEffectiveDate) RsgDaysToGO
	                                ,e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)  AlermStartDateCmp

                                        ,Replace(CONVERT(VARCHAR(11),PRE.ConfirmationDate,106),' ','-') PREConfirmationDate
									 ,convert(date,PRE.ConfirmationDate) PREConfirmationDateExc
									  , pre.Completed preCompleted
                                    ,isnull(e.IsConfirmed,0) IsConfirmedProbation

                                    ,mpb.EntityId,mpb.PositionId
                                    --emp ids
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
											" + cList + @"
                                    from EmployeeInformation e
									LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = E.GroupID
									LEFT OUTER JOIN ORG.Company C ON C.Id = E.CompanyId

                                    LEFT OUTER JOIN org.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN org.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN org.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN org.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN org.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN org.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN org.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN org.Unit eu on eu.id=e.UnitId
                                    --left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									from mst.DesignationMaster dm
									left outer join hkp.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                            --left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									and egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                            " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    where  " + separatCondition + "  " + wc + @"" + EmployeeCategory + @" ";

                    return _sqlRepository.GetDataCollection(gsql);
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
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                Join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
                            }

                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                wc = " and c.id = '" + item.Id + "'";
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

                    var gsql = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                    ,REPLACE(CONVERT(VARCHAR(11), e.ApprovedDateTime, 106), ' ', '-') ApprovedDateTime
                                    ,CONVERT(date, e.ApprovedDateTime) vApprovedDateTime
									,ISNULL(e.IsApproved,0) IsApproved
                                    --Resignation
								    ,ISNULL(rsg.ApprovalStatus,'') rsgApprovalStatus
									,REPLACE(CONVERT(VARCHAR(11), rsg.ApprovedEffectiveDate, 106), ' ', '-') resignationApprovedEffectiveDate
                                    ,CONVERT(DATE, rsg.ApprovedEffectiveDate) resignationApprovedEffectiveDateS
									,REPLACE(CONVERT(VARCHAR(11), rsg.ResignationDate, 106), ' ', '-') ApplicantResignationDate
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOSDate, 106), ' ', '-') ApplicantSeparationDate
								    ,CONVERT(DATE, rsg.ApprovedDate) resignationApprovedEntryDateS
									,CONVERT(date,rsg.AddedDate) resignationAddedDate
								    ,CONVERT(DATE, rsg.EffectiveDate) EffectiveDateS
									,CONVERT(DATE, e.DOSDate) ApplicantSeparationDateEX
									,REPLACE(CONVERT(VARCHAR(11),rsg.EffectiveDate,106),' ','-') RsgSelfEffectiveDate
                                    ,ProbationPeriod= case when e.DOCIsDay=1 then e.DOCDay
									ELSE e.DOCMonth*30 end
									,e.DOCDay,e.DOCMonth
                                    ,e.EmployeeStatus EmployeeStatus
									,e.DOJ+(CASE WHEN e.DOCIsDay=1 THEN e.DOCDay
												ELSE e.DOCMonth*30 END) DOCs
									,Replace(CONVERT(VARCHAR(11),
									e.DOJ+(case when e.DOCIsDay=1 then e.DOCDay
												else e.DOCMonth*30 end)
									 , 106), ' ', '-') DOC
                                    ,Replace(CONVERT(VARCHAR(11),e.DOC,106),' ','-') empDOC
                                    ,Replace(CONVERT(VARCHAR(11),(e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)), 106), ' ', '-') AlermStartDate

                                    ,DATEDIFF(day, '" + hrDate + @"', (e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end))) DaysToGO
									,DATEDIFF(day,'" + hrDate + @"', rsg.ApprovedEffectiveDate) RsgDaysToGO
	                                ,e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)  AlermStartDateCmp

                                        ,Replace(CONVERT(VARCHAR(11),PRE.ConfirmationDate,106),' ','-') PREConfirmationDate
									 ,convert(date,PRE.ConfirmationDate) PREConfirmationDateExc
									  , pre.Completed preCompleted
                                    ,isnull(e.IsConfirmed,0) IsConfirmedProbation

                                    ,mpb.EntityId,mpb.PositionId
                                    --emp ids
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
											" + cList + @"
                                    from EmployeeInformation e
								    LEFT OUTER JOIN ORG.CompanyGroup CG ON CG.Id = E.GroupID
									LEFT OUTER JOIN ORG.Company C ON C.Id = E.CompanyId
                                    LEFT OUTER JOIN org.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN org.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN org.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN org.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN org.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN org.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN org.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN org.Unit eu on eu.id=e.UnitId
                                    --left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									from mst.DesignationMaster dm
									left outer join hkp.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                            --left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									and egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                            " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
	                              LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    where  " + separatCondition + "  " + wc + @" " + EmployeeCategory + @"";
                    return _sqlRepository.GetDataCollection(gsql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Separated Employee List

        public IEnumerable<object> ModalConsecutiveLateStats(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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

            try
            {
                var cListG = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var join = string.Empty;
                var wc = string.Empty;
                if (seq == -2)
                {
                    var cListGBuilder = new System.Text.StringBuilder();
                    cListGBuilder.Append(cListG);
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                cListGBuilder.Append("," + item.StandardName + "Defination.UserName");
                                join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";

                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    cListG = cListGBuilder.ToString();

                    var sql = @"WITH CTE
									AS (
										SELECT *
											 -- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE

													 WHEN DayStatus = 'P' THEN 1
													 WHEN DayStatus = 'HP' THEN 1
													 WHEN DayStatus = 'LVP' THEN 1
													 WHEN DayStatus = 'WP' THEN 1
													 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											 -- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)
									SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,COUNT(*) AS LateDays,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
									" + cList + @"
									FROM cte

									LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
                                       " + join + @"
									WHERE fv = 'L' -- current status = 'L'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV','A') AND (EI.DOJ<='" + hrDate + @"' AND (EI.DOS is null or EI.DOS >= '" + hrDate + @"'))  AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'  " + wc + @"
									GROUP BY cte.EmpSystemID,EI.DOJ,EI.EmployeeName,EI.EmployeeCode" + cListG + @"
									HAVING
										-- at least three days absent
										COUNT(*) >= 3";
                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                {
                    seq += 1;
                    var cListGBuilder = new System.Text.StringBuilder();
                    cListGBuilder.Append(cListG);
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName ");

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                cListGBuilder.Append("," + item.StandardName + "Defination.UserName");
                                join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";

                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
                            }
                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                wc = " and c.id = '" + item.Id + "'";
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
                    cListG = cListGBuilder.ToString();

                    var sql = @"WITH CTE
									AS (
										SELECT *
											 -- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE

													WHEN DayStatus = 'P' THEN 1
													 WHEN DayStatus = 'HP' THEN 1
													 WHEN DayStatus = 'LVP' THEN 1
													 WHEN DayStatus = 'WP' THEN 1
													 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											 -- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)

									SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ,
										COUNT(*) AS LateDays " + cList + @"
									FROM cte

									LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id

								" + join + @"

									WHERE fv = 'L' -- current status = 'L'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV','A') AND (EI.DOJ<='" + hrDate + @"' AND (EI.DOS is null or EI.DOS >= '" + hrDate + @"'))  AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'   " + wc + @"
									GROUP BY cte.EmpSystemID,EI.EmployeeName,EI.DOJ,EI.EmployeeCode " + cListG + @"
									HAVING
										-- at least three days absent
										COUNT(*) >= 3";
                    return _sqlRepository.GetDataCollection(sql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ModalConsecutiveAbsentStats(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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
            try
            {
                var cListG = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var join = string.Empty;
                var wc = string.Empty;
                if (seq == -2)
                {
                    var cListGBuilder = new System.Text.StringBuilder();
                    cListGBuilder.Append(cListG);
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }
                            //if (item.RType == "Z")
                            //{
                            //    cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            //    cListGBuilder.Append("," + item.StandardName + ".UserName");
                            //    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
                            //}
                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                cListGBuilder.Append("," + item.StandardName + "Defination.UserName");

                                join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
                            }
                        }
                    }
                    cListG = cListGBuilder.ToString();

                    var sql = @"WITH CTE
									AS (
										SELECT *
											 -- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE
														 WHEN DayStatus = 'P' THEN 1
														 WHEN DayStatus = 'L' THEN 1
														 WHEN DayStatus = 'WL' THEN 1
														 WHEN DayStatus = 'HP' THEN 1
														 WHEN DayStatus = 'LVP' THEN 1
														 WHEN DayStatus = 'WP' THEN 1
														 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											 -- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)
									SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,COUNT(*) AS absentDays,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
									" + cList + @"
									FROM cte

									LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
                                       " + join + @"
									WHERE fv = 'A' -- current status = 'L'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV') AND  (EI.DOJ<='" + hrDate + @"' AND (EI.DOS is null or EI.DOS >= '" + hrDate + @"'))  AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'   " + wc + @"
									GROUP BY cte.EmpSystemID,EI.DOJ,EI.EmployeeName,EI.EmployeeCode" + cListG + @"
									HAVING
										-- at least three days absent
										COUNT(*) >= 3";
                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                {
                    seq += 1;
                    var cListGBuilder = new System.Text.StringBuilder();
                    cListGBuilder.Append(cListG);
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            if (item.RType == "Entity")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName ");

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                                else
                                {
                                    join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n";
                            }

                            if (item.RType == "Z")
                            {
                                cList += "," + item.StandardName + "Defination.UserName " + item.StandardName + "Defination ";
                                cListGBuilder.Append("," + item.StandardName + "Defination.UserName");

                                join += "LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemId = MPB." + item.StandardName + "DefinationId\n";
                            }
                            if (item.RType == "ZA")
                            {
                                cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                                cListGBuilder.Append("," + item.StandardName + ".UserName");
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n";
                            }
                        }

                        if (item.Sequence != -2)
                        {
                            if (item.Sequence == -1)
                            {
                                wc = " and c.id = '" + item.Id + "'";
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
                    cListG = cListGBuilder.ToString();

                    var sql = @"WITH CTE
									AS (
										SELECT *
											 -- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE
													 WHEN DayStatus = 'P' THEN 1
													 WHEN DayStatus = 'L' THEN 1
													 WHEN DayStatus = 'WL' THEN 1
													 WHEN DayStatus = 'HP' THEN 1
													 WHEN DayStatus = 'LVP' THEN 1
													 WHEN DayStatus = 'WP' THEN 1
													 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											 -- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)

									SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,
										COUNT(*) AS absentDays,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ " + cList + @"
									FROM cte

									LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id

								" + join + @"

									WHERE fv = 'A' -- current status = 'L'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV','OD') AND (EI.DOJ<='" + hrDate + @"' AND (EI.DOS is null or EI.DOS >= '" + hrDate + @"'))  AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'  " + wc + @"
									GROUP BY cte.EmpSystemID,EI.DOJ,EI.EmployeeName,EI.EmployeeCode " + cListG + @"
									HAVING
										-- at least three days absent
										COUNT(*) >= 3";
                    return _sqlRepository.GetDataCollection(sql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region Future Leave Trends

        public IEnumerable<object> LeaveStatus(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            try
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
                var lSql = @"SELECT COUNT(LTD.SystemID) totalLeave, REPLACE(CONVERT(VARCHAR(11), LTD.WorkDate,6),' ','-') WorkDate FROM [dbo].[LeaveTransaction] LT
								LEFT JOIN  [dbo].[LeaveTransactionDetails] LTD ON LT.SystemID = LTD.LvTrnsSystemID
								LEFT JOIN [DBO].[EmployeeInformation] E on E.SystemId  =  LT.EmpSystemID
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								WHERE LTD.IsAvailed = 0 AND LT.IsApproved = 1 AND LTD.WorkDate > '" + hrDate + @"'  AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @"
							     GROUP BY LTD.WorkDate ORDER BY LTD.WorkDate ";
                return _sqlRepository.GetDataCollection(lSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> DynamicLeaveStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
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

            try
            {
                var cList = string.Empty;
                var cListId = string.Empty;
                var join = string.Empty;
                var wc = string.Empty;
                var cListextG = string.Empty;
                var cListextIdG = string.Empty;
                seq += 1;
                var joinBuilder = new System.Text.StringBuilder();
                joinBuilder.Append(join);
                var WhereClausebuilder = new System.Text.StringBuilder();
                WhereClausebuilder.Append(wc);
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            if (item.RType == "Entity")
                            {
                                cList = "," + item.StandardName + ".UserName";
                                cListId = "," + item.StandardName + ".Id";

                                if (item.StandardName == "EmployeeGroup")
                                {
                                    joinBuilder.Append("LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                                else
                                {
                                    joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = EN." + item.StandardName + "Id\n");
                                }
                            }
                            if (item.RType == "Position")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = PO." + item.StandardName + "Id\n");
                            }
                            if (item.RType == "Z")
                            {
                                cListId = "," + item.StandardName + "Defination.SystemID";
                                cList = "," + item.StandardName + "Defination.UserName"; cListId = "," + item.StandardName + "Defination.SystemID";
                                joinBuilder.Append("LEFT JOIN [" + item.StandardName + "Defination] ON " + item.StandardName + "Defination.SystemID = MPB." + item.StandardName + "DefinationId\n");
                            }
                            if (item.RType == "ZA")
                            {
                                cListId = "," + item.StandardName + ".Id";
                                cList = "," + item.StandardName + ".UserName"; cListId = "," + item.StandardName + ".Id";
                                joinBuilder.Append("LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = MPB." + item.StandardName + "Id\n");
                            }
                        }
                    }
                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            WhereClausebuilder.Append("  AND E.CompanyId ='" + item.Id + "'");
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                if (item.RType == "Z")
                                {
                                    WhereClausebuilder.Append(" AND " + item.StandardName + "Defination.SystemID = '" + item.Text + "'");

                                }
                                else
                                {
                                    WhereClausebuilder.Append(" AND " + item.StandardName + ".Id='" + item.Text + "'");

                                }
                            }
                        }
                    }
                }
                wc = WhereClausebuilder.ToString();
                join = joinBuilder.ToString();
                var lSql = @"SELECT COUNT(LTD.SystemID) totalLeave, REPLACE(CONVERT(VARCHAR(11), LTD.WorkDate,6),' ','-') WorkDate FROM [dbo].[LeaveTransaction] LT
								LEFT JOIN  [dbo].[LeaveTransactionDetails] LTD ON LT.SystemID = LTD.LvTrnsSystemID
								LEFT JOIN [DBO].[EmployeeInformation] E on E.SystemId  =  LT.EmpSystemID
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
									" + join + @"
								WHERE LTD.IsAvailed = 0 AND LT.IsApproved = 1 AND LTD.WorkDate > '" + hrDate + @"'  AND E.EmployeeStatus = 'Active' " + wc + @" " + EmployeeCategory + @"
							     GROUP BY LTD.WorkDate";
                return _sqlRepository.GetDataCollection(lSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Future Leave Trends

        #region Consecutive Late and Absent and Present Day Modal POP Up

        public IEnumerable<object> ModalConsecutiveAbsentDateList(string companyGroupId, string companyId, string plantId, string empSystemID, string hrDate)
        {
            try
            {
                var sql = @"WITH CTE AS (SELECT *,MAX(
                                            CASE
											WHEN DayStatus = 'P' THEN 1
										    WHEN DayStatus = 'L' THEN 1
											WHEN DayStatus = 'WL' THEN 1
											WHEN DayStatus = 'HP' THEN 1
											WHEN DayStatus = 'LVP' THEN 1
											WHEN DayStatus = 'WP' THEN 1
											WHEN DayStatus = 'MLVP' THEN 1
												ELSE 0
										END) OVER (
											PARTITION BY EmpSystemID ORDER BY WorkDate DESC
										) AS mx
											 -- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData where EmpSystemID = '" + empSystemID + @"'
										)
									SELECT REPLACE(CONVERT(VARCHAR(11), WorkDate, 106), ' ', '-') WorkDate,DayStatus,DT.Description
									FROM cte
									LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
									INNER JOIN 	DayType	DT ON DT.DayType = cte.DayStatus

									WHERE fv = 'A' -- current status = 'A'
										AND mx = 0 -- all rows before the 1st 'P'
										AND EmpSystemID =  '" + empSystemID + @"'
										AND DayStatus NOT IN('H','W','L','LV','HLV','WLV')
										AND EI.EmployeeStatus = 'Active' AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"' ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ModalConsecutiveLateDateList(string companyGroupId, string companyId, string plantId, string empSystemID, string hrDate)
        {
            try
            {
                var sql = @"WITH CTE AS (SELECT *,MAX(CASE
										 WHEN DayStatus = 'P' THEN 1
										 WHEN DayStatus = 'HP' THEN 1
										 WHEN DayStatus = 'LVP' THEN 1
										 WHEN DayStatus = 'WP' THEN 1
										 WHEN DayStatus = 'MLVP' THEN 1
												ELSE 0
										END) OVER (
											PARTITION BY EmpSystemID ORDER BY WorkDate DESC
										) AS mx
											 -- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData where EmpSystemID = '" + empSystemID + @"'
										)
									SELECT REPLACE(CONVERT(VARCHAR(11), CTE.WorkDate, 106), ' ', '-') WorkDate,DayStatus,DT.Description
									--,CONVERT(char(5), CONVERT(TIME,CTE.InTime - EDWSA.ShiftInTime) , 108) lateBy

									 , CONVERT(CHAR(5), CONVERT(TIME,CTE.InTime - (CASE WHEN CS.InTime IS NULL THEN 
                                        Format(CTE.InTime, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END
                                        ELSE CS.InTime END)
                                        ) , 108) LateBy
									FROM cte
										LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
									INNER JOIN 	DayType	DT ON DT.DayType = cte.DayStatus
									inner JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = CTE.EmpSystemID
									inner JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m

                                LEFT JOIN [ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = EDWSA.ShiftSystemID and cs.ShiftDate = CTE.WorkDate

														AND EDWSA.WorkDate = CTE.WorkDate 
																LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = CTE.ShiftSystemID
									WHERE fv = 'L' -- current status = 'A'
										AND mx = 0 -- all rows before the 1st 'P'
										AND CTE.EmpSystemID =  '" + empSystemID + @"'
										AND DayStatus NOT IN('H','W','LV','HLV','WLV','A')
										AND EI.EmployeeStatus = 'Active' AND CONVERT(DATE,CTE.WorkDate) <= '" + hrDate + @"'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public string ModalConsecutivePresentDateListSql(string companyGroupId, string companyId, string plantId, string hrfromDate, string hrtoDate, string dayCount, string presentComparator)
        {
            string presnetsql = @"select Count(max_Present_days) PresentDaysOccured,EmployeeCodePreFix,EmployeeCodeNumeric,DOSS, EmpSystemID, EmployeeCode,DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeName,CompanyName  from (
                                    SELECT EmpSystemID,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric, DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeName
                                  , DOSS,CompanyName , count(*) max_Present_days, min(WorkDate) workDate, max(WorkDate) mxworkDate
                                    FROM (
                                    	SELECT *, sum(xx) OVER (
                                    			PARTITION BY EmpSystemID ORDER BY WorkDate
                                    			) ss
                                    	FROM (
                                    		SELECT ad.WorkDate, ad.EmpSystemID, DT.Category,EI.EmployeeCode, ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric,EI.EmployeeName
                                    		,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJS
                                    		,REPLACE(CONVERT(VARCHAR(11), EI.DOS, 106), ' ', '-') DOSS,C.UserName CompanyName
                                    									,Division.UserName Division ,Plant.UserName Plant ,Unit.UserName Unit ,Department.UserName Department ,Section.UserName Section ,SubSection.UserName SubSection ,ShiftDefination.UserName ShiftDefination ,Line.UserName Line 
                                    									,Ldes.UserName LegalDesignation,ec.UserName EmployeeCategorys
                                    		,CASE 
                                    				WHEN Category = lag(Category) OVER (
                                    						PARTITION BY EmpSystemID ORDER BY WorkDate
                                    						)
                                    					THEN 0
                                    				ELSE 1
                                    				END AS xx
                                    		FROM AttdnProcessData ad
                                    		INNER JOIN DayType dt ON dt.DayType = ad.DayStatus
                                    
                                    
                                    		
                                    	LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = ad.EmpSystemID
                                    									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                                                      LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
                                    									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                                                        LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
                                                                           LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
                                                                            LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                                                                            LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                                                                            LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                                                            LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                                                            LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                                                            LEFT JOIN [ShiftDefination] ON ShiftDefination.SystemId = MPB.ShiftDefinationId
                                                                            LEFT JOIN [ORG].[Line] ON Line.Id = MPB.LineId
                                    	 LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = EI.LegalDesignationId
                                    								left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                    left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                    left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId		
                                    		WHERE EI.CompanyId = '" + companyId + @"' --and EmpSystemID ='2000037'
                                    			AND WorkDate BETWEEN '" + hrfromDate + @"' AND '" + hrtoDate + @"'
                                    		) x
                                    	) y
                                    WHERE Category IN ('Present','Late')
                                    GROUP BY EmpSystemID,CompanyName, DOSS,DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric,EmployeeName, ss
                                   HAVING count(*) " + presentComparator + " " + dayCount + @"
                                    
                                    ) dd Group by 
                                      EmployeeCodePreFix,EmployeeCodeNumeric,DOSS,CompanyName, EmpSystemID, EmployeeCode,DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys ,EmployeeName
                                    ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
                                    
                                    ";
            return presnetsql;
        }

        public IEnumerable<object> ModalConsecutivePresentDateList(string companyGroupId, string companyId, string plantId, string hrfromDate, string hrtoDate, string dayCount, string presentComparator)
        {
            try
            {

                string sql = ModalConsecutivePresentDateListSql(companyGroupId, companyId, plantId, hrfromDate, hrtoDate, dayCount, presentComparator);


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetEEmpJobCardInfoWithInDateTimes(string wrHrFromDate, string ToDate, string companyId, string comparator, string workingHour)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"   SELECT A.EmployeeCode
                            	,A.EmployeeName
                                ,A.EmployeeStatus
                            	,A.DOJS
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
                            	,A.Division
                            	,A.Department
                            	,A.Section
                            	,A.SubSection
                            	,REPLACE(CONVERT(VARCHAR(11), A.PDate, 113), ' ', '-') PDate
                                ,PDay
                            	,A.DayStatus
                                ,A.IsHalfDayLeave
                            	,A.InTime
                                ,ShiftInTimeShow
								 ,ShiftInTime
                            	,A.InDeviceID
                            	,A.OutTime
                            	,A.OutDeviceID
                            	,A.IsManual
                            	,A.OTHr OverStay
                                ,A.TotalOTHr FinalOT
                            	,A.LvShortName
                            	,A.Code
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,A.OTConsiderOn
                                ,ShiftTime = CASE WHEN ShiftChangeInTime IS NULL THEN ShiftInTime ELSE ShiftChangeInTime END
                                ,ShiftName
								,ShiftType
							    ,ShiftOutTime
                                ,A.IsManualDayStatus,A.IsManualInTime,A.IsManualOutTime, A.ShortLeave,A.IsOTEntitled,A.IsOTComfirm,A.WorkDate,
                                ReConfirm = CASE  WHEN A.IsOTComfirm=0 AND A.WorkDate IS NOT NULL  THEN 1   ELSE 0  END,A.DayCategory
                                ,A.InTimelate,A.OutTimelate
                                ,A.ShiftInTimeLate
                               
	                            ,A.LeaveDuration                               
								,A.DurationInMin
                                ,DATEDIFF(second, intime, OutTime) / 3600.0 WorkHour
	                                ,A.EO 
									,A.LIN
									,A.LO
                                    ,A.Line,A.ExtraOT
                            FROM(
                                SELECT E.EmployeeCode
                                    , E.EmployeeName
                                    ,E.EmployeeStatus
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJS
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOSS
                                    , D.UserName GivenDesignation
                                    , U.UserName Unit
                                    , Dv.UserName Division
                                    , Dp.UserName Department
                                    , S.UserName Section
                                    ,ar.IsHalfDayLeave
                                    , SB.UserName SubSection
                                    ,datename(dw,AR.WorkDate) as PDay
                                    , AR.WorkDate PDate
                                    , AR.DayStatus
                                  
                                    , HR.OTConsiderOn
                                    ---, AR.InTime InTime
                                    ,  InTime=case when dt.OriginalDayType='W' and ar.IsOTEntitled=0 and  ma.InTime IS NOT NUll then ma.InTime 
									               when dt.OriginalDayType='H' and ar.IsOTEntitled=0  and ma.InTime IS NOT NUll  then ma.InTime 
												   when dt.OriginalDayType='W' and ar.IsOTEntitled=1  and  EOT.FromDate IS NOT NUll  then EOT.FromDate 
												   when dt.OriginalDayType='H' and ar.IsOTEntitled=1  and  EOT.FromDate IS NOT NUll  then EOT.FromDate                                                    
									else   ISNULL(ar.InTime,ar.PunchInTime) end

                                    , AR.InTime InTimeShow
                                   	,l.UserName as Line
                            ,ShiftInTimeLate=CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),108)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 108)
						     END
                                    , CONVERT(VARCHAR(5), AR.InTime, 108) InTimelate
                             ,ShiftInTimeShow = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END
                                    , ARIN.DeviceID InDeviceID
                                    ---, AR.OutTime OutTime
                                    , OutTime=case when dt.OriginalDayType='W' and ar.IsOTEntitled=0 and ma.OutTime IS NOT NUll  then ma.OutTime 
									               when dt.OriginalDayType='H' and ar.IsOTEntitled=0 and ma.OutTime IS NOT NUll then ma.OutTime 
												   when dt.OriginalDayType='W' and ar.IsOTEntitled=1 and EOT.ToDate IS NOT NUll  then EOT.ToDate 
												   when dt.OriginalDayType='H' and ar.IsOTEntitled=1 and EOT.ToDate IS NOT NUll  then EOT.ToDate 
                                                   when dt.OriginalDayType='NW' and ar.IsOTEntitled=1 and EOT.ToDate IS NOT NUll then EOT.ToDate 
									else ar.OutTime end



                                    , AR.OutTime OutTimeShow
                                    , CONVERT(VARCHAR(5), AR.OutTime, 108) OutTimelate
                                    , AROUT.DeviceID OutDeviceID
                                    , AR.IsManualInTime IsManual
                                    ---, AR.OTHr 
                                    , ISNULL( AR.OTHr,0) +ISNULL( EOT.Duration,0) OTHr
                                    ,OT.TotalOTHr
                                    , LT.UserName LvShortName
                                    , LT.Description LvDescrip
                                    , LT.LeaveType
                                    , LT.Code
                                    , Isnull(LG.UserName, '') LegalDesignation
                                    , AR.InTime dti, AR.OutTime dto
                                    , CONVERT(VARCHAR(5), cs.InTime, 108) ShiftChangeInTime
                                    , SD.ShiftDefinationName ShiftName
									,sd.ShiftType
                                    ,LEAVE.LeaveDuration	                            
									,HODD.DurationInMin

		                            ,EO.OffDuration AS EO
									,EIN.OffDuration AS LIN
									,LO= Case when LO.InfoType='LUNCHOUT' THEN 'YES' ELSE 'NO' END,ISNULL( EOT.Duration,0)ExtraOT

						   ,ShiftOutTime = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                     ,ShiftInTime = Format(AR.InTime, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END
                                    , AR.IsManualDayStatus, AR.IsManualInTime, AR.IsManualOutTime, ar.CountedShortLeave ShortLeave,AR.IsOTEntitled,AR.IsOTComfirm,OT.WorkDate,dt.Category DayCategory
                                FROM dbo.EmployeeInformation E

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                INNER JOIN dbo.AttdnProcessData AR ON E.SystemID = AR.EmpSystemID
	                           LEFT JOIN (select LET.SystemID,LTD.LeaveDuration,LTD.WorkDate,LET.EmpSystemID from  LeaveTransaction LET 
										    left join LeaveTransactionDetails LTD ON LTD.LvTrnsSystemID=LET.SystemID	
                                        where ltd.WorkDate Between '" + wrHrFromDate + @"' and '" + ToDate + @"'
								         ) LEAVE ON LEAVE.EmpSystemID=E.SystemId and LEAVE.WorkDate= AR.WorkDate

                                left join (select EmpSystemID,WorkDate,SUM(DurationInMin)AS DurationInMin
		                    From  [dbo].[HourlyOffDuty] 
	                        WHERE  ApproveType='Deducation' AND WorkDate Between '" + wrHrFromDate + @"' and '" + ToDate + @"'
		                    Group BY  EmpSystemID,WorkDate)as HODD on HODD.EmpSystemID=E.SystemId and HODD.WorkDate=AR.WorkDate

                                LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + ToDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
                                ON AR.ShiftSystemID = SFCG.ShiftDefinationID
                                LEFT JOIN dbo.AttdnRawData ARIN ON AR.InTimeRowID = ARIN.RowID
                                LEFT JOIN dbo.AttdnRawData AROUT ON AR.OutTimeRowID = AROUT.RowID
                                LEFT JOIN dbo.LeaveType LT ON AR.LTSystemID = LT.Id
                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id

                                  LEFT JOIN ORG.Section S ON PO.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SB ON PO.SubSectionID = SB.Id
								left join org.Line l on l.Id=mpb.LineId

                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                            
                                
                                left join EmpDateWiseShiftAssign es on es.EmpSystemID = E.SystemId
                                AND AR.WorkDate = ES.WorkDate
                                left join(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
                                left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = es.ShiftSystemID and cs.ShiftDate = ar.WorkDate
                                left join[ShiftDefination] sd on sd.SystemID = es.ShiftSystemID
                                LEFT JOIN HKP.Designation D ON E.GivenDesignationId = D.Id
                                LEFT JOIN FinalOT OT ON E.SystemId = OT.EmpSystemID and ot.WorkDate=ar.WorkDate
                                LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=E.PlantId
                                LEFT JOIN DayType dt on dt.Daytype=AR.DayStatus

                                left join AttendanceInfoExtra LO on LO.EmpSystemId=e.SystemId and LO.WorkDate=ar.WorkDate and LO.InfoType='LUNCHOUT'
								left join AttendanceInfoExtra EO on EO.EmpSystemId=e.SystemId and EO.WorkDate=ar.WorkDate and EO.InfoType='EARLUOUT'
								left join AttendanceInfoExtra EIN on EIN.EmpSystemId=e.SystemId and EIN.WorkDate=ar.WorkDate and EIN.InfoType='EARLUIN'


								left join (
								SELECT EmpSystemId,FromDate,ToDate,Duration,WorkDate FROM HourlyOT where  OTType IN ('EXTRAOT','OTLIMIT') 
								
								) EOT on EOT.EmpSystemId=AR.EmpSystemID and EOT.WorkDate=ar.WorkDate

								left join AttdnManualData MA on MA.EmpSystemID=ar.EmpSystemID and MA.WorkDate=ar.WorkDate



                                WHERE E.CompanyId = '" + companyId + @"' AND
                                     AR.WorkDate BETWEEN '" + wrHrFromDate + @"'
                                        AND '" + ToDate + @"' AND (EmployeeStatus = 'Active' OR COnvert(date,DOS) >= Convert(Date,'" + wrHrFromDate + @"'))
                                ) A
                                    where DATEDIFF(second, intime, OutTime) / 3600.0 " + workingHour + @" " + comparator + @" 
                            GROUP BY A.EmployeeCode
                            	,A.EmployeeName
                            	,A.DOJS
                                ,A.DOSS
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
                            	,A.Division
                            	,A.Department
                            	,A.Section
                            	,A.SubSection
                            	,PDate
                                ,PDay	
                            	,A.DayStatus
                                ,a.IsHalfDayLeave
                            	,A.InTime
                                ,A.ShiftInTime
                                ,A.ShiftInTimeShow
                            	,A.InDeviceID
                            	,A.OutTime
                            	,A.OutDeviceID
                            	,A.IsManual
                            	,A.OTHr
                                ,A.TotalOTHr
                            	,A.LvShortName
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,A.Code
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,A.OTConsiderOn
                                ,ShiftChangeInTime
                                ,ShiftName
								,ShiftType
                                ,A.ShiftInTimeLate
                              
                                ,A.EmployeeStatus
                                ,A.ShiftOutTime
                                ,A.IsManualDayStatus
                                ,A.IsManualInTime
                                ,A.IsManualOutTime
                                , a.ShortLeave
                                ,A.IsOTEntitled
                                ,A.IsOTComfirm
                                ,A.WorkDate
                                ,A.DayCategory
                                ,A.InTimelate
                                ,A.OutTimelate
	                            ,A.LeaveDuration                       
								,A.DurationInMin
                                ,A.EO 
								,A.LIN
								,A.LO
,A.Line,A.ExtraOT
                            ORDER BY A.EmployeeCode
                            	,A.PDate ";

                return _sqlRepository.GetDataCollection(strSql);
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


        public IWorkbook GetEmployeePresentStatusReport(string companyGroupId, string companyId, string plantId, string hrfromDate, string hrtoDate, string dayCount, string presentComparator, string userId)
        {
            #region Variable
            clsReport objRpt = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            DataTable dtEmployees = null;



            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;
            #endregion Variable

            try
            {

                string strPath = "";
                Image companyLogo = null;
                //string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                //try
                //{
                //    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                //    companyLogo = Image.FromFile(strPath);
                //}
                //catch (Exception)
                //{
                //}
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                string sql = ModalConsecutivePresentDateListSql(companyGroupId, companyId, plantId, hrfromDate, hrtoDate, dayCount, presentComparator);


                dtEmployees = _sqlRepository.GetDataTable(sql);
                //Sql Salary Structure 


                //Sql Salary Process 



                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);

                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0;

                #endregion

                //1


                //SR to

                //xlsCol += 1;

                // 9
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("Company", sheet1, xlsRow, ref xlsCol, out int colCompany);
                SetCellValue("Plant", sheet1, xlsRow, ref xlsCol, out int colPlant);

                SetCellValue("Employee Code", sheet1, xlsRow, ref xlsCol, out int ColemployeeCode);


                SetCellValue("Employee Name", sheet1, xlsRow, ref xlsCol, out int ColName);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out int ColDOJ);

                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out int ColDOS);
                SetCellValue("Legal Designation", sheet1, xlsRow, ref xlsCol, out int ColLDG);
                SetCellValue("EmployeeCategory", sheet1, xlsRow, ref xlsCol, out int colEmpCategory);
                SetCellValue("PresentDaysOccured", sheet1, xlsRow, ref xlsCol, out int colPresentDaysOccured);



                endXlsCol = xlsCol;





                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //endXlsCol = endxlsCol;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;


                string FactoryAddress = string.Empty;
                try
                {

                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }


                //if (dsCmp.Tables[0].Rows.Count > 0)
                //{
                //    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                //}
                //else
                //{
                //    CmpName = "";
                //}
                //sheet1.Range[xlsRow, 3].Text = CmpName;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                //sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //xlsRow += 1;
                //if (dsCmp.Tables[0].Rows.Count > 0)
                //{
                //    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                //}
                //else
                //{
                //    FactoryName = "";
                //}
                //if (dsCmp.Tables[0].Rows.Count > 0)
                //{
                //    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                //}
                //else
                //{
                //    FactoryAddress = "";
                //}
                //sheet1.Range[xlsRow, 3].Text = FactoryName;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                //sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //xlsRow += 1;
                //sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                //sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                //xlsRow += 1;
                ////sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                //sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["CompanyName"].ToString()) == false)
                            sheet1.Range[xlsRow, colCompany].Text = dtEmployees.Rows[i]["CompanyName"].ToString();
                        sheet1.Range[xlsRow, colCompany].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colCompany].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Plant"].ToString()) == false)
                            sheet1.Range[xlsRow, colCompany].Text = dtEmployees.Rows[i]["Plant"].ToString();
                        sheet1.Range[xlsRow, colCompany].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colCompany].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColemployeeCode].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColemployeeCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColemployeeCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJS"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOSS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOSS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColLDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColLDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColLDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCategorys"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmployeeCategorys"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PresentDaysOccured"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colPresentDaysOccured].Text = dtEmployees.Rows[i]["PresentDaysOccured"].ToString();
                        sheet1.Range[xlsRow, colPresentDaysOccured].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colPresentDaysOccured].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        #endregion
                        #region Attendance Data


                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "AttdnInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;


                IWorksheet sheet2 = workbook.Worksheets[1];
                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }






        #endregion Consecutive Late and Absent Day Modal POP Up

        #region Absent Status

        public IEnumerable<object> DateWiseAbsentList(string companyGroupId, string companyId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var daYCountStatus = string.Empty;
            var dateBetween = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            //if (plantId == null || plantId == "null")
            //{
            //    plant = "";
            //}
            //else
            //{
            //    plant = @"AND  E.PlantId = '" + plantId + @"'";
            //}
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }

            if (hrFromDate == "undefined" || hrToDate == "undefined")
            {
                dateBetween = "";
            }
            else
            {
                dateBetween = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN	CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"') ";
            }

            try
            {
                //IEnumerable<object> OrgStrList = OrgStructureListColList(companyGroupId);
                var OrgStrList = OrgStructureList(companyGroupId);

                var cListG = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var join = string.Empty;
                var wc = string.Empty;

                var cListGBuilder = new System.Text.StringBuilder();
                cListGBuilder.Append(cListG);
                foreach (var item in OrgStrList)
                {
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            if (item.StandardName == "EmployeeGroup")
                            {
                                join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        else
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                        }
                    }
                }
                cListG = cListGBuilder.ToString();

                string strSql = @"SELECT DISTINCT ISNULL(C.UserName,'') CompanyName,ISNULL(Plant.UserName,'') Plant, E.SystemId
									,ISNULL(E.EmployeeCode,'') EmployeeCode,ISNULL(E.EmployeeName,'') EmployeeName,ISNULL(EmpC.UserName,'') EmpCategorys
									,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJs
									,REPLACE(CONVERT(VARCHAR(11), ISNULL(E.DOS,''), 106), ' ', '-') DOSs
									--,cg.Id CompanyGroupId,cg.UserName GroupName,E.BudgetCode
									,C.Id AS CompanyId, ISNULL(LDes.UserName,'') Designation
                                    ,DayNumber.DaysCount AbsentDays
                                    ,ISNULL(E.EmployeeCurrentStatus,E.EmployeeStatus)EmployeeCurrentStatus,MB.Code BudgetCode,POS.Activity,E.CellPhnNo ContactNo
									,ISNULL(rg.UserName,NULL) ResGroup,tg.UserName TransportGroup,A.DayStatus LatestWorkingDayStatus,FORMAT(A.WorkDate,'dd-MMM-yyyy')LatestPresentDate,''AbsentReasonifApplicable,''Remark

								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
								LEFT  JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate
								LEFT JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								LEFT JOIN [MST].DesignationMasterLegalDesignation LDM ON LDM.LegalDesignationId=E.LegalDesignationId
			                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.Id = LDM.DesignationMasterId
                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                                LEFT JOIN dbo.ResidenceGroup AS rg ON rg.Id=E.ResidenceGroupId
								LEFT JOIN dbo.TransportGroup AS tg ON tg.Id=E.TransportGroupId	
								
								LEFT JOIN(SELECT APD.WorkDate, APD.EmpSystemId,APD.DayStatus
								FROM AttdnProcessData  APD 
								WHERE APD.WorkDate=FORMAT(GETDATE(),'dd-MMM-yyyy')) A ON A.EmpSystemId=E.SystemId
								--LEFT JOIN [ORG].[Plant] Plant ON Plant.Id = E.PlantId
								" + join + @"

								INNER JOIN (
								SELECT COUNT(WorkDate) DaysCount, EmpSystemId FROM AttdnProcessData  APD
								INNER JOIN DayType DT ON DT.DayType = APD.DayStatus Where  DT.Category = 'Absent' " + dateBetween + @"
								GROUP BY EmpSystemId
								)  DayNumber ON DayNUmber.EmpSystemID = E.SystemId
								WHERE (E.DOS IS NULL OR E.DOS <= '" + hrToDate + @"')
								AND
								E.GroupID = '" + companyGroupId + @"'  " + daYCountStatus + @" " + dateBetween + @" AND  
                                (E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrFromDate + @"'))  
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrToDate + @"') " + company + @" ORDER BY DayNumber.DaysCount DESC  
                             ";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IWorkbook DateWiseAbsentListInExcel(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var daYCountStatus = string.Empty;
            var dateBetween = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "null")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId = '" + plantId + @"'";
            }
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }

            if (hrFromDate == "undefined" || hrToDate == "undefined")
            {
                dateBetween = "";
            }
            else
            {
                dateBetween = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN	CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"') ";
            }

            try
            {
                //IEnumerable<object> OrgStrList = OrgStructureListColList(companyGroupId);
                var OrgStrList = OrgStructureList(companyGroupId);

                var cListG = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var join = string.Empty;
                var wc = string.Empty;

                var cListGBuilder = new System.Text.StringBuilder();
                cListGBuilder.Append(cListG);
                foreach (var item in OrgStrList)
                {
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            if (item.StandardName == "EmployeeGroup")
                            {
                                join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        else
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                        }
                    }
                }
                cListG = cListGBuilder.ToString();

                string sqlText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,DayNumber.DaysCount AbsentDays
									,E.EmployeeCode,EmpC.UserName EmpCategory
									,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
									,cg.Id CompanyGroupId,cg.UserName GroupName,E.BudgetCode
									,C.Id AS CompanyId,C.UserName CompanyName,Plant.UserName Plant, GDes.UserName GivenDesignation

								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
								LEFT  JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								--LEFT JOIN [ORG].[Plant] Plant ON Plant.Id = E.PlantId
								" + join + @"

								INNER JOIN (
								SELECT COUNT(WorkDate) DaysCount, EmpSystemId FROM AttdnProcessData  APD
								INNER JOIN DayType DT ON DT.DayType = APD.DayStatus Where  DT.Category = 'Absent' " + dateBetween + @"
								GROUP BY EmpSystemId
								)  DayNumber ON DayNUmber.EmpSystemID = E.SystemId
								WHERE E.EmployeeStatus = 'Active'
								AND
								E.GroupID = '" + companyGroupId + @"'  " + daYCountStatus + @" " + dateBetween + @" AND E.EmployeeStatus = 'Active'
							     " + company + @" " + plant + @"";
                var absentData = _sqlRepository.GetDataTable(sqlText);

                #region Report
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //DataTable dtEmpAbsentInfo = HRDailyAbsentStatusListSql(chartColList, companyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId);
                //dvDaily = new DataView(dtEmpAbsentInfo);

                if (absentData.Rows.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 5;

                    #region ColumnHeaderVariables              
                    int cSrNo = 0; int cLine = 0; int cEmpCode = 0; int cEmpName = 0; int cCompany = 0; int cPlant = 0;
                    int cDOJ = 0; int cLegalDesig = 0; int cGDesig = 0; int cEmpCategory = 0; int cDesigGroup = 0; int cAbsentDays = 0;
                    int cBudgetCode = 0;
                    #endregion
                    #region ColumnHeaders
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Sr.No", ExcelHAlign.HAlignCenter); cSrNo = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Company", ExcelHAlign.HAlignCenter); cCompany = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Plant", ExcelHAlign.HAlignCenter); cPlant = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "BudgetCode", ExcelHAlign.HAlignCenter); cBudgetCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Emp.Code", ExcelHAlign.HAlignCenter); cEmpCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Employee name", ExcelHAlign.HAlignCenter); cEmpName = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Emp Category", ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cGDesig = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "DOJ", ExcelHAlign.HAlignCenter); cDOJ = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Absent Days", ExcelHAlign.HAlignCenter); cAbsentDays = xlsCol; xlsCol++;

                    var orgCollist = xlsCol;

                    endXlsCol = xlsCol;
                    #endregion
                    var slCount = 0;
                    for (int i = 0; i < absentData.Rows.Count; i++)
                    {
                        slCount++;
                        #region Loop
                        oRU.SetText(ref sheet1, xlsRow, cSrNo, slCount.ToString());
                        oRU.SetText(ref sheet1, xlsRow, cCompany, absentData.Rows[i]["CompanyName"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cPlant, absentData.Rows[i]["Plant"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cBudgetCode, absentData.Rows[i]["BudgetCode"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cEmpCode, absentData.Rows[i]["EmployeeCode"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cEmpName, absentData.Rows[i]["EmployeeName"].ToString());//LegalDesignation
                        oRU.SetText(ref sheet1, xlsRow, cEmpCategory, absentData.Rows[i]["EmpCategory"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cGDesig, absentData.Rows[i]["GivenDesignation"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cDOJ, absentData.Rows[i]["DOJ"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cAbsentDays, absentData.Rows[i]["AbsentDays"].ToString());// 
                        xlsRow++;
                    }



                    xlsRow += 1;



                    #region Line Setup
                    //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion

                    sheet1.Range[11, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[11, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[11, 4, xlsRow, 4].WrapText = true;

                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    oRU.CompanyGroupHeader(ref sheet1, endXlsCol, "Absent Information of ", companyGroupId);
                    sheet1.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(endXlsCol) + 5].Merge();
                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
                }
                return workbook;
                #endregion

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> ConsecutiveDateWiseAbsentList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount)
        {
            var daYCountStatus = string.Empty;
            if (dayCount == null || dayCount == string.Empty || dayCount == "null")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount <= " + dayCount + @"";
            }
            try
            {
                //IEnumerable<object> OrgStrList = OrgStructureListColList(companyGroupId);
                var OrgStrList = OrgStructureList(companyGroupId);

                var cListG = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var join = string.Empty;
                var wc = string.Empty;

                var cListGBuilder = new System.Text.StringBuilder();
                cListGBuilder.Append(cListG);
                foreach (var item in OrgStrList)
                {
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            if (item.StandardName == "EmployeeGroup")
                            {
                                join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        else
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                        }
                    }
                }
                cListG = cListGBuilder.ToString();

                var sql = @"WITH CTE
									AS (
										SELECT *
											 -- cumulative Max, returns 0 as long as there's no P status
											,MAX(CASE

													WHEN DayStatus = 'P' THEN 1
													 WHEN DayStatus = 'HP' THEN 1
													 WHEN DayStatus = 'LVP' THEN 1
													 WHEN DayStatus = 'WP' THEN 1
													 WHEN DayStatus = 'MLVP' THEN 1
													ELSE 0
													END) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS mx
											 -- status of the latest date
											,first_value(DayStatus) OVER (
												PARTITION BY EmpSystemID ORDER BY WorkDate DESC
												) AS fv
										FROM AttdnProcessData
										)
									SELECT cte.EmpSystemID,EI.EmployeeName,EI.EmployeeCode,COUNT(*) AS ConLateDays,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ,EmpC.UserName EmpCategory
									,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
									,CG.Id CompanyGroupId,CG.UserName GroupName,EI.BudgetCode
									,C.Id AS CompanyId,C.UserName CompanyName,GDes.UserName GivenDesignation
									FROM cte
									LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = cte.EmpSystemID
									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
									LEFT OUTER JOIN ORG.org.CompanyGroup CG ON CG.Id = EI.GroupID
                                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

									WHERE fv = 'L' -- current status = 'L'
										AND mx = 0 -- all rows before the 1st 'P'
										 AND DayStatus NOT IN('H','W','LV','HLV','WLV','A') AND EI.EmployeeStatus = 'Active' AND CONVERT(DATE,CTE.WorkDate) BETWEEN  '" + hrFromDate + @"' AND '" + hrToDate + @"'
								    AND  EI.CompanyId='" + companyId + @"' AND  EI.PlantId = '" + plantId + @"'
									GROUP BY cte.EmpSystemID,EI.DOJ,EI.EmployeeName,EI.EmployeeCode
									HAVING
										-- at least three days absent
										COUNT(*) >= " + dayCount + "";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion 
        #endregion Absent Status

        #region Late Status

        public IEnumerable<object> DateWiseLatetListStatus(string companyGroupId, string companyId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var company = string.Empty;
            var plant = string.Empty;

            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            //if (plantId == null || plantId == "null")
            //{
            //    plant = "";
            //}
            //else
            //{
            //    plant = @"AND  E.PlantId = '" + plantId + @"'";
            //}
            var daYCountStatus = string.Empty;
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }
            try
            {
                //IEnumerable<object> OrgStrList = OrgStructureListColList(companyGroupId);
                var OrgStrList = OrgStructureList(companyGroupId);

                var cListG = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var join = string.Empty;
                var wc = string.Empty;

                var cListGBuilder = new System.Text.StringBuilder();
                cListGBuilder.Append(cListG);
                foreach (var item in OrgStrList)
                {
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            if (item.StandardName == "EmployeeGroup")
                            {
                                join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        else
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                        }
                    }
                }
                cListG = cListGBuilder.ToString();

                string CmdText = @"SELECT
                                    DISTINCT ISNULL(C.UserName,'') CompanyName,ISNULL(Plant.UserName,'') Plant, E.SystemId
									,ISNULL(E.EmployeeCode,'') EmployeeCode,ISNULL(E.EmployeeName,'') EmployeeName,ISNULL(EmpC.UserName,'') EmpCategorys
									,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJs
									,REPLACE(CONVERT(VARCHAR(11), ISNULL(E.DOS,''), 106), ' ', '-') DOSs
									--,cg.Id CompanyGroupId,cg.UserName GroupName,E.BudgetCode
									,C.Id AS CompanyId, ISNULL(LDes.UserName,'') Designation
                                    ,DayNumber.DaysCount LateDays
                                    ,ISNULL(E.EmployeeCurrentStatus,E.EmployeeStatus)EmployeeCurrentStatus,MB.Code BudgetCode,POS.Activity,E.CellPhnNo ContactNo
									,ISNULL(rg.UserName,NULL) ResGroup,tg.UserName TransportGroup,A.DayStatus LatestWorkingDayStatus,FORMAT(A.WorkDate,'dd-MMM-yyyy')LatestPresentDate,''AbsentReasonifApplicable,''Remark

								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID
								LEFT  JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate
								LEFT JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								LEFT JOIN [MST].DesignationMasterLegalDesignation LDM ON LDM.LegalDesignationId=E.LegalDesignationId
			                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.Id = LDM.DesignationMasterId
                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								LEFT JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								LEFT JOIN dbo.ResidenceGroup AS rg ON rg.Id=E.ResidenceGroupId
								LEFT JOIN dbo.TransportGroup AS tg ON tg.Id=E.TransportGroupId	
								
								LEFT JOIN(SELECT APD.WorkDate, APD.EmpSystemId,APD.DayStatus
								FROM AttdnProcessData  APD 
								WHERE APD.WorkDate=FORMAT(GETDATE(),'dd-MMM-yyyy')) A ON A.EmpSystemId=E.SystemId
								" + join + @"

								INNER JOIN (
								SELECT COUNT(WorkDate) DaysCount, EmpSystemId FROM AttdnProcessData  APD
								INNER JOIN DayType DT ON DT.DayType = APD.DayStatus Where  DT.Category = 'Late' AND CONVERT(DATE, APD.WorkDate)
								BETWEEN
								CONVERT(DATE,'" + hrFromDate + @"') and  CONVERT(DATE,'" + hrToDate + @"')
								GROUP BY EmpSystemId
								)  DayNumber ON DayNUmber.EmpSystemID = E.SystemId
								WHERE 
								E.GroupID = '" + companyGroupId + @"'  " + daYCountStatus + @" AND
                                (E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + hrFromDate + @"'))  
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + hrToDate + @"') AND
									CONVERT(DATE, APD.WorkDate) BETWEEN	CONVERT(DATE,'" + hrFromDate + @"') and  CONVERT(DATE,'" + hrToDate + @"')
									   " + company + @" " + plant + @" order by DayNumber.DaysCount desc";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Late Status

        #region Joining Status

        public IEnumerable<object> DateJoiningStatus(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var company = string.Empty;
            var plant = string.Empty;

            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "null")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId = '" + plantId + @"'";
            }

            try
            {
                //IEnumerable<object> OrgStrList = OrgStructureListColList(companyGroupId);
                var OrgStrList = OrgStructureList(companyGroupId);

                var cListG = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var join = string.Empty;
                var wc = string.Empty;

                var cListGBuilder = new System.Text.StringBuilder();
                cListGBuilder.Append(cListG);
                foreach (var item in OrgStrList)
                {
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            if (item.StandardName == "EmployeeGroup")
                            {
                                join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        else
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                        }
                    }
                }
                cListG = cListGBuilder.ToString();

                string CmdText = @"SELECT  E.SystemId,E.EmployeeName,
									E.EmployeeCode,ISNULL(EmpC.UserName,'') EmpCategorys
									,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJs
									,cg.Id CompanyGroupId--,cg.UserName GroupName,E.BudgetCode
									,C.Id AS CompanyId,C.UserName CompanyName,Plant.UserName Plant,LDes.UserName Designation
                                    ,ISNULL(E.EmployeeCurrentStatus,E.EmployeeStatus)EmployeeCurrentStatus,MB.Code BudgetCode,POS.Activity,E.CellPhnNo ContactNo
									,ISNULL(rg.UserName,NULL) ResGroup,tg.UserName TransportGroup,A.DayStatus LatestWorkingDayStatus,FORMAT(A.WorkDate,'dd-MMM-yyyy')LatestPresentDate,''AbsentReasonifApplicable,''Remark
								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId

								LEFT JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId

								LEFT JOIN [MST].DesignationMasterLegalDesignation LDM ON LDM.LegalDesignationId=E.LegalDesignationId
			                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.Id = LDM.DesignationMasterId
                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId


								LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								LEFT JOIN [ORG].[Plant] Plant ON Plant.Id = E.PlantId
                                LEFT JOIN dbo.ResidenceGroup AS rg ON rg.Id=E.ResidenceGroupId
								LEFT JOIN dbo.TransportGroup AS tg ON tg.Id=E.TransportGroupId	
								
								LEFT JOIN(SELECT APD.WorkDate, APD.EmpSystemId,APD.DayStatus
								FROM AttdnProcessData  APD 
								WHERE APD.WorkDate=FORMAT(GETDATE(),'dd-MMM-yyyy')) A ON A.EmpSystemId=E.SystemId
								WHERE  (E.DOS IS NULL OR E.DOS <='" + hrToDate + @"')
								AND
								E.GroupID = '" + companyGroupId + @"' AND
									CONVERT(DATE,DOJ)  between  CONVERT(DATE,'" + hrFromDate + @"') and  CONVERT(DATE,'" + hrToDate + @"')
									   " + company + @" " + plant + @"";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> DateSepartaionStatus(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var company = string.Empty;
            var plant = string.Empty;

            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "null")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId = '" + plantId + @"'";
            }

            try
            {
                //IEnumerable<object> OrgStrList = OrgStructureListColList(companyGroupId);
                var OrgStrList = OrgStructureList(companyGroupId);

                var cListG = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var join = string.Empty;
                var wc = string.Empty;

                var cListGBuilder = new System.Text.StringBuilder();
                cListGBuilder.Append(cListG);
                foreach (var item in OrgStrList)
                {
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            if (item.StandardName == "EmployeeGroup")
                            {
                                join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        else
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                        }
                    }
                }
                cListG = cListGBuilder.ToString();

                string CmdText = @"SELECT  E.SystemId,E.EmployeeName,E.EmployeeStatus,E.UpdatedBy,
									E.EmployeeCode,EmpC.UserName EmpCategorys
									,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJs
									,REPLACE(CONVERT(VARCHAR(11), E.DOS, 106), ' ', '-') DOSs

									,cg.Id CompanyGroupId,cg.UserName GroupName,Resig.Reason,Resig.Remarks
									,C.Id AS CompanyId,C.UserName CompanyName,Plant.UserName Plant,ISNULL(LDes.UserName,'') Designation
                                    ,Dept.UserName Department,Sec.UserName Section,SubSec.UserName SubSection
                                    ,ISNULL(E.EmployeeCurrentStatus,E.EmployeeStatus)EmployeeCurrentStatus,MB.Code BudgetCode,POS.Activity,E.CellPhnNo ContactNo
									,ISNULL(rg.UserName,NULL) ResGroup,tg.UserName TransportGroup,A.DayStatus LatestWorkingDayStatus,FORMAT(A.WorkDate,'dd-MMM-yyyy')LatestPresentDate,''AbsentReasonifApplicable,''Remark
								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								INNER join TRN.Resignation  Resig ON  Resig.Id =   (select  Top 1 id from TRN.Resignation where EmployeeId = E.SystemId order by AddedDate desc) 
								LEFT JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId

	                            LEFT JOIN [MST].DesignationMasterLegalDesignation LDM ON LDM.LegalDesignationId=E.LegalDesignationId
			                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.Id = LDM.DesignationMasterId
                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId


								LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								LEFT JOIN [ORG].[Plant] Plant ON Plant.Id = E.PlantId
								LEFT JOIN [ORG].[Department] Dept ON Dept.Id = E.DepartmentId
								LEFT JOIN [ORG].[Section] Sec ON Sec.Id = E.SectionId
								LEFT JOIN [ORG].[SubSection] SubSec ON SubSec.Id = E.SubSectionId
                                LEFT JOIN dbo.ResidenceGroup AS rg ON rg.Id=E.ResidenceGroupId
								LEFT JOIN dbo.TransportGroup AS tg ON tg.Id=E.TransportGroupId	
								
								LEFT JOIN(SELECT APD.WorkDate, APD.EmpSystemId,APD.DayStatus
								FROM AttdnProcessData  APD 
								WHERE APD.WorkDate=FORMAT(GETDATE(),'dd-MMM-yyyy')) A ON A.EmpSystemId=E.SystemId
								WHERE 
								(--DOS
								E.EmployeeStatus = 'Separated' 
									 AND
							     CONVERT(DATE,DOS)  BETWEEN  CONVERT(DATE,'" + hrFromDate + @"') AND  CONVERT(DATE,'" + hrToDate + @"')
									)--DOS
									--OR
									--(--TBS
									--E.EmployeeStatus = 'TBS' 
									-- AND
									--CONVERT(DATE,E.DateUpdated)  BETWEEN  CONVERT(DATE,'" + hrFromDate + @"') AND  CONVERT(DATE,'" + hrToDate + @"')
									--)--TBS
									AND  (E.GroupID = '" + companyGroupId + @"' " + company + @" " + plant + @") AND Resig.ApprovalStatus = 'Approved'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IWorkbook DateWiseJoiningListInExcel(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            try
            {
                var daYCountStatus = string.Empty;
                var dateBetween = string.Empty;
                var company = string.Empty;
                var plant = string.Empty;
                if (companyId == null || companyId == "null")
                {
                    company = "";
                }
                else
                {
                    company = @"AND  E.CompanyId = '" + companyId + @"'";
                }
                if (plantId == null || plantId == "null")
                {
                    plant = "";
                }
                else
                {
                    plant = @"AND  E.PlantId = '" + plantId + @"'";
                }



                if (hrFromDate == "undefined" || hrToDate == "undefined")
                {
                    dateBetween = "";
                }
                else
                {
                    dateBetween = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN	CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"') ";
                }
                //IEnumerable<object> OrgStrList = OrgStructureListColList(companyGroupId);
                var OrgStrList = OrgStructureList(companyGroupId);

                var cListG = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var join = string.Empty;
                var wc = string.Empty;

                var cListGBuilder = new System.Text.StringBuilder();
                cListGBuilder.Append(cListG);
                foreach (var item in OrgStrList)
                {
                    {
                        if (item.RType == "Entity")
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            if (item.StandardName == "EmployeeGroup")
                            {
                                join += "LEFT JOIN [HKP].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                            else
                            {
                                join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = E." + item.StandardName + "Id\n";
                            }
                        }
                        else
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            cListGBuilder.Append("," + item.StandardName + ".UserName");
                            join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
                        }
                    }
                }
                cListG = cListGBuilder.ToString();

                string sqlText = @"SELECT  E.SystemId,E.EmployeeName,
									E.EmployeeCode,EmpC.UserName EmpCategory
									,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
									,cg.Id CompanyGroupId,cg.UserName GroupName,E.BudgetCode
									,C.Id AS CompanyId,C.UserName CompanyName,Plant.UserName Plant,GDes.UserName GivenDesignation

								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								LEFT JOIN [ORG].[Plant] Plant ON Plant.Id = E.PlantId
								WHERE E.EmployeeStatus = 'Active'
								AND
								E.GroupID = '" + companyGroupId + @"' AND
									CONVERT(DATE,DOJ)  between  CONVERT(DATE,'" + hrFromDate + @"') and  CONVERT(DATE,'" + hrToDate + @"')
									   " + company + @" " + plant + @"";

                var absentData = _sqlRepository.GetDataTable(sqlText);

                #region Report
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //DataTable dtEmpAbsentInfo = HRDailyAbsentStatusListSql(chartColList, companyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId);
                //dvDaily = new DataView(dtEmpAbsentInfo);

                if (absentData.Rows.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 5;

                    #region ColumnHeaderVariables              
                    int cSrNo = 0; int cLine = 0; int cEmpCode = 0; int cEmpName = 0; int cCompany = 0; int cPlant = 0;
                    int cDOJ = 0; int cLegalDesig = 0; int cGDesig = 0; int cEmpCategory = 0; int cDesigGroup = 0; int cAbsentDays = 0;
                    int cBudgetCode = 0;
                    #endregion
                    #region ColumnHeaders
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Sr.No", ExcelHAlign.HAlignCenter); cSrNo = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Company", ExcelHAlign.HAlignCenter); cCompany = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Plant", ExcelHAlign.HAlignCenter); cPlant = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "BudgetCode", ExcelHAlign.HAlignCenter); cBudgetCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Emp.Code", ExcelHAlign.HAlignCenter); cEmpCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Employee name", ExcelHAlign.HAlignCenter); cEmpName = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Emp Category", ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cGDesig = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "DOJ", ExcelHAlign.HAlignCenter); cDOJ = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Absent Days", ExcelHAlign.HAlignCenter); cAbsentDays = xlsCol; xlsCol++;

                    var orgCollist = xlsCol;

                    endXlsCol = xlsCol;
                    #endregion
                    var slCount = 0;
                    for (int i = 0; i < absentData.Rows.Count; i++)
                    {
                        slCount++;

                        oRU.SetText(ref sheet1, xlsRow, cSrNo, slCount.ToString());
                        oRU.SetText(ref sheet1, xlsRow, cCompany, absentData.Rows[i]["CompanyName"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cPlant, absentData.Rows[i]["Plant"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cBudgetCode, absentData.Rows[i]["BudgetCode"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cEmpCode, absentData.Rows[i]["EmployeeCode"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cEmpName, absentData.Rows[i]["EmployeeName"].ToString());//LegalDesignation
                        oRU.SetText(ref sheet1, xlsRow, cEmpCategory, absentData.Rows[i]["EmpCategory"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cGDesig, absentData.Rows[i]["GivenDesignation"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cDOJ, absentData.Rows[i]["DOJ"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cAbsentDays, absentData.Rows[i]["AbsentDays"].ToString());// 
                        xlsRow++;
                    }
                    xlsRow += 1;
                    #region Line Setup
                    //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion

                    sheet1.Range[11, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[11, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[11, 4, xlsRow, 4].WrapText = true;

                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    oRU.CompanyPlantHeader(ref sheet1, endXlsCol, "Joining Information from " + hrFromDate + " To " + hrToDate, companyId, "", hrFromDate);
                    sheet1.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(endXlsCol) + 5].Merge();
                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
                }
                return workbook;

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Joining Status

        #region Reporting Person Wise Attendance Status

        public IEnumerable<object> ROPersonWiseAttnStatus(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId)
        {
            var ROWiseDate = string.Empty;
            var ROWiseSHDate = string.Empty;
            var ROWiseAPDDate = string.Empty;
            var ROWiseSHDDate = string.Empty;

            if (hrFromDate != null && (hrToDate != null && hrToDate != "undefined"))
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
                ROWiseAPDDate = @" CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
                ROWiseSHDate = @"AND CONVERT(DATE, ESA.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
                ROWiseSHDDate = @"CONVERT(DATE, ESA.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
            }
            else if (hrToDate == null || hrToDate == "undefined")
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
                ROWiseAPDDate = @" CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
                ROWiseSHDate = @"AND CONVERT(DATE, ESA.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
                ROWiseSHDDate = @"CONVERT(DATE, ESA.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
            }

            var EmployeeCategory = string.Empty;
            //if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            //{
            EmployeeCategory = "";
            //}
            //else
            //{
            //	EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            //}
            try
            {
                var sql = @"SELECT distinct OnRoleEmployee.CompanyId CompanyId,OnRoleEmployee.CompanyName ColumnName,OnRoleEmployee.GroupName GroupName,OnRoleEmployee.CompanyGroupId CompanyGroupId,ISNULL(OnRoleEmployee.totalEmployee,0) OnRoleEmployee,ISNULL(PresentEmployee.totalPresentEmployee,0) totalPresentEmployee, ISNULL(AbsentEmployee.totalAbsentEmployee,0) totalAbsentEmployee,ISNULL(LateEmployee.totalLateEmployee,0) totalLateEmployee
					  ,ISNULL(LeaveEmployee.totalLeaveEmployee,0) totalLeaveEmployee,ISNULL(WeekOffEmployee.totalWeekoffEmployee,'')totalWeekoffEmployee,
					  ISNULL(ShiftNotAssignedEmployee.totalShiftNotAssignedEmployee,0) ShiftNotAssignedEmployee,
					  ISNULL(AttdnNotProcessedToday.totalAttdnNotProcessedToday,0) totalAttdnNotProcessedToday,
					  ISNULL(ShiftNotAssignAsofToday.totalShiftNotAssignAsofToday,0) totalShiftNotAssignAsofToday
					     from
						   (SELECT COUNT(E.SystemId) totalEmployee,C.UserName,cg.Id CompanyGroupId,c.Id CompanyId,c.UserName CompanyName,cg.UserName GroupName    FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											LEFT OUTER JOIN EmployeeInformation
											E ON e.GroupID = CG.Id and c.Id=E.CompanyId
									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
								    INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
											where
												 GroupID = '" + companyGroupId + @"'  AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "'  AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @" AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'

												group by C.UserName,cg.Id,c.Id,c.UserName,cg.UserName) OnRoleEmployee
								LEFT OUTER JOIN
								  (
									SELECT  Count(APD.EmpSystemID) totalPresentEmployee,
								cg.Id CompanyGroupId, cg.UserName GroupName
								,C.Id AS CompanyId, C.UserName CompanyName,E.PlantId
									 FROM    AttdnProcessData AS APD

								INNER join EmployeeInformation  AS E ON E.SystemId = APD.EmpSystemID
								INNER join ORG.CompanyGroup  AS cg ON E.GroupID = CG.Id

								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN ORG.Plant Pl ON pl.Id = E.PlantId

							    INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Present' " + ROWiseDate + @"
						                  AND cg.Id = '" + companyGroupId + @"'  AND C.Id= '" + companyId + @"'   AND pl.Id = '" + plantId + @"'
									   AND E.EmployeeStatus = 'Active'  AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'
									GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,E.PlantId
									)
									PresentEmployee ON OnRoleEmployee.CompanyGroupId = PresentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = PresentEmployee.CompanyId

												LEFT OUTER JOIN
								  (
									SELECT  Count(APD.EmpSystemID) totalAbsentEmployee,
								cg.Id CompanyGroupId, cg.UserName GroupName
								,C.Id AS CompanyId, C.UserName CompanyName,E.PlantId
									 FROM    AttdnProcessData AS APD

								INNER join EmployeeInformation  AS E ON E.SystemId = APD.EmpSystemID
								INNER join ORG.CompanyGroup  AS cg ON E.GroupID = CG.Id

								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN ORG.Plant Pl ON pl.Id = E.PlantId

							    INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Absent' " + ROWiseDate + @"
						                  AND cg.Id = '" + companyGroupId + @"'  AND C.Id= '" + companyId + @"'   AND pl.Id = '" + plantId + @"'
									   AND E.EmployeeStatus = 'Active'  AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'
									GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,E.PlantId
									)
									AbsentEmployee ON OnRoleEmployee.CompanyGroupId = AbsentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = AbsentEmployee.CompanyId
									LEFT OUTER JOIN
									(
							     SELECT  Count(APD.EmpSystemID) totalLateEmployee,
								cg.Id CompanyGroupId, cg.UserName GroupName
								,C.Id AS CompanyId, C.UserName CompanyName,E.PlantId
									 FROM    AttdnProcessData AS APD

								INNER join EmployeeInformation  AS E ON E.SystemId = APD.EmpSystemID
								INNER join ORG.CompanyGroup  AS cg ON E.GroupID = CG.Id

								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN ORG.Plant Pl ON pl.Id = E.PlantId

							    INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Late'
								" + ROWiseDate + @"
						                  AND cg.Id = '" + companyGroupId + @"'  AND C.Id= '" + companyId + @"'   AND pl.Id = '" + plantId + @"'
									   AND E.EmployeeStatus = 'Active'  AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'
									GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,E.PlantId
								)
									LateEmployee on OnRoleEmployee.CompanyGroupId = LateEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LateEmployee.CompanyId
									LEFT OUTER JOIN
									(
								     SELECT  Count(APD.EmpSystemID) totalWeekoffEmployee,
								cg.Id CompanyGroupId, cg.UserName GroupName
								,C.Id AS CompanyId, C.UserName CompanyName,E.PlantId
									 FROM    AttdnProcessData AS APD

								INNER join EmployeeInformation  AS E ON E.SystemId = APD.EmpSystemID
								INNER join ORG.CompanyGroup  AS cg ON E.GroupID = CG.Id

								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN ORG.Plant Pl ON pl.Id = E.PlantId

							    INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category IN('Holiday', 'Weekend') " + ROWiseDate + @"
						                  AND cg.Id = '" + companyGroupId + @"'  AND C.Id= '" + companyId + @"'   AND pl.Id = '" + plantId + @"'
									   AND E.EmployeeStatus = 'Active'  AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'
									GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,E.PlantId
                                   )
									WeekOffEmployee ON OnRoleEmployee.CompanyGroupId = WeekOffEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = WeekOffEmployee.CompanyId

									LEFT OUTER JOIN
									(
									  SELECT  Count(APD.EmpSystemID) totalLeaveEmployee,
								cg.Id CompanyGroupId, cg.UserName GroupName
								,C.Id AS CompanyId, C.UserName CompanyName,E.PlantId
									 FROM    AttdnProcessData AS APD

								INNER join EmployeeInformation  AS E ON E.SystemId = APD.EmpSystemID
								INNER join ORG.CompanyGroup  AS cg ON E.GroupID = CG.Id

								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN ORG.Plant Pl ON pl.Id = E.PlantId

							    INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Leave' " + ROWiseDate + @"
						                  AND cg.Id = '" + companyGroupId + @"'  AND C.Id= '" + companyId + @"'   AND pl.Id = '" + plantId + @"'
									   AND E.EmployeeStatus = 'Active'  AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'
									GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName,E.PlantId
									)
								LeaveEmployee on OnRoleEmployee.CompanyGroupId = LeaveEmployee.CompanyGroupId
							AND OnRoleEmployee.CompanyId = LeaveEmployee.CompanyId
										LEFT OUTER JOIN
									(
								SELECT COUNT(E.SystemId) totalOthersEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
									C.Id AS CompanyId, C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN(--*
									SELECT * FROM EmployeeInformation
									WHERE SystemId NOT IN(--**
									SELECT DISTINCT EmpSystemID FROM AttdnProcessData APD
									WHERE   " + ROWiseAPDDate + @"
									)-- * *
									)-- *
									E ON e.GroupID = CG.Id AND c.Id = E.CompanyId
										LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
									WHERE
									  GroupID = '" + companyGroupId + @"'  AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "' AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @" AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'

									GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName)
									OthersEmployee ON OnRoleEmployee.CompanyGroupId = OthersEmployee.CompanyGroupId
									AND OnRoleEmployee.CompanyId = OthersEmployee.CompanyId

									LEFT OUTER JOIN
									(SELECT COUNT(E.SystemId) totalOthersShiftNotAssignedEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
									C.Id AS CompanyId, C.UserName CompanyName
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN(--*
									SELECT * FROM EmployeeInformation
									WHERE SystemId NOT IN(--**
									select EmpSystemId from EmployeeShiftAssign
									)-- * *
									)-- *
									E ON e.GroupID = CG.Id AND c.Id = E.CompanyId
										LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
									WHERE
									   GroupID = '" + companyGroupId + @"'  AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "' AND EmployeeStatus = 'Active'  " + EmployeeCategory + @" AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'
									GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName)
									OthersShiftNotAssignedEmployee ON OnRoleEmployee.CompanyGroupId = OthersShiftNotAssignedEmployee.CompanyGroupId
										AND OnRoleEmployee.CompanyId = OthersShiftNotAssignedEmployee.CompanyId
									LEFT OUTER JOIN
								(SELECT COUNT(E.SystemId) totalShiftNotAssignedEmployee, cg.Id CompanyGroupId, cg.UserName GroupName,
								C.Id AS CompanyId, C.UserName CompanyName

								FROM  ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN(--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId NOT IN(--**
								SELECT DISTINCT EmpSystemID FROM EmployeeShiftAssign
								)-- * *
								)-- *
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
								WHERE

								   GroupID = '" + companyGroupId + @"'  AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "'  AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @" AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'

								GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName)
									ShiftNotAssignedEmployee ON OnRoleEmployee.CompanyGroupId = ShiftNotAssignedEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = ShiftNotAssignedEmployee.CompanyId

								LEFT OUTER JOIN
									(
									 SELECT count(ESA.EmpSystemID) totalAttdnNotProcessedToday, cg.Id CompanyGroupId, cg.UserName GroupName,
											C.Id AS CompanyId, C.UserName  UId
										FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											LEFT OUTER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id = E.CompanyId
											LEFT OUTER JOIN(--*
															   SELECT EmpSystemID FROM EmpDateWiseShiftAssign ESA
																	   WHERE ESA.EmpSystemID NOT IN(--**
																			SELECT DISTINCT EmpSystemID FROM AttdnProcessData APD
																		WHERE  " + ROWiseAPDDate + @"
																		)  " + ROWiseSHDate + @"-- * *
															  )-- *
														ESA
											ON E.SystemId = ESA.EmpSystemID

										LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
									WHERE
									  GroupID = '" + companyGroupId + @"'  AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "'   AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @" AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'
									GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName) AttdnNotProcessedToday ON OnRoleEmployee.CompanyGroupId = AttdnNotProcessedToday.CompanyGroupId AND OnRoleEmployee.CompanyId = AttdnNotProcessedToday.CompanyId
									LEFT OUTER JOIN
									(
									SELECT count(ESA.SystemID) totalShiftNotAssignAsofToday, cg.Id CompanyGroupId, cg.UserName GroupName,
									C.Id AS CompanyId, C.UserName  UId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id = E.CompanyId
									LEFT OUTER JOIN(--*
									SELECT SystemID FROM EmployeeInformation EI
									WHERE EI.SystemID NOT IN(--**
									SELECT DISTINCT EmpSystemID FROM EmpDateWiseShiftAssign ESA
									WHERE  " + ROWiseSHDDate + @"
									)-- * *
									)-- *
									ESA
									ON E.SystemId = ESA.SystemId

								    LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
									WHERE
									GroupID = '" + companyGroupId + @"' AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "'    AND E.EmployeeStatus = 'Active' " + EmployeeCategory + @" AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'

									GROUP BY C.UserName, cg.UserName, C.Id, cg.Id, cg.UserName) ShiftNotAssignAsofToday ON OnRoleEmployee.CompanyGroupId = ShiftNotAssignAsofToday.CompanyGroupId AND OnRoleEmployee.CompanyId = ShiftNotAssignAsofToday.CompanyId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel ROPersonWisePresentStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var EmployeeCategory = string.Empty;
            //if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            //{
            EmployeeCategory = "";
            //}
            //else
            //{
            //	EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            //}
            var ROWiseDate = string.Empty;

            if (hrFromDate != null && (hrToDate != null && hrToDate != "undefined"))
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
            }
            else if (hrToDate == null)
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
            }
            try
            {
                parameters.CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,SD.ShiftDefinationName ShiftDefinationName,LTRIM(RIGHT(CONVERT(VARCHAR(25), EDWSA.ShiftInTime, 100), 7)) ShiftInTime,cg.Id CompanyGroupId,cg.UserName GroupName,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,
								C.Id AS CompanyId,C.UserName CompanyName,REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,GDes.UserName GivenDesignation,EmpC.UserName EmployeeCategory

								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID

								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId

								    INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId

								WHERE E.EmployeeStatus = 'Active' AND    DT.Category = 'Present' AND
								E.GroupID = '" + companyGroupId + @"' AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "' AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'  " + EmployeeCategory + @"  " + ROWiseDate + @" ";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel ROPersonWiseAbsentStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var EmployeeCategory = string.Empty;
            //if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            //{
            EmployeeCategory = "";
            //}
            //else
            //{
            //	EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            //}
            var ROWiseDate = string.Empty;

            if (hrFromDate != null && (hrToDate != null && hrToDate != "undefined"))
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
            }
            else if (hrToDate == null)
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
            }
            try
            {
                parameters.CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,SD.ShiftDefinationName ShiftDefinationName,LTRIM(RIGHT(CONVERT(VARCHAR(25), EDWSA.ShiftInTime, 100), 7)) ShiftInTime,cg.Id CompanyGroupId,cg.UserName GroupName,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,
								C.Id AS CompanyId,C.UserName CompanyName,REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,GDes.UserName GivenDesignation,EmpC.UserName EmployeeCategory

								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID

								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
							    INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId
								WHERE E.EmployeeStatus = 'Active' AND   DT.Category = 'Absent' AND
								E.GroupID = '" + companyGroupId + @"' AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "' AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'  " + EmployeeCategory + @"  " + ROWiseDate + @" ";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel ROPersonWiseLatetStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var EmployeeCategory = string.Empty;
            //if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            //{
            EmployeeCategory = "";
            //}
            //else
            //{
            //	EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            //}
            var ROWiseDate = string.Empty;

            if (hrFromDate != null && (hrToDate != null && hrToDate != "undefined"))
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
            }
            else if (hrToDate == null)
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
            }
            try
            {
                parameters.CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,SD.ShiftDefinationName ShiftDefinationName,LTRIM(RIGHT(CONVERT(VARCHAR(25), EDWSA.ShiftInTime, 100), 7)) ShiftInTime,cg.Id CompanyGroupId,cg.UserName GroupName,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,
								C.Id AS CompanyId,C.UserName CompanyName,REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,GDes.UserName GivenDesignation,EmpC.UserName EmployeeCategory

								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID

								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId

								WHERE E.EmployeeStatus = 'Active' AND   DT.Category = 'Late' AND
								E.GroupID = '" + companyGroupId + @"' AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "' AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'  " + EmployeeCategory + @"  " + ROWiseDate + @"";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel ROPersonWiseLeavetStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var EmployeeCategory = string.Empty;
            //if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            //{
            EmployeeCategory = "";
            //}
            //else
            //{
            //	EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            //}
            var ROWiseDate = string.Empty;

            if (hrFromDate != null && (hrToDate != null && hrToDate != "undefined"))
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
            }
            else if (hrToDate == null)
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
            }
            try
            {
                parameters.CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,SD.ShiftDefinationName ShiftDefinationName,LTRIM(RIGHT(CONVERT(VARCHAR(25), EDWSA.ShiftInTime, 100), 7)) ShiftInTime,cg.Id CompanyGroupId,cg.UserName GroupName,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,
								C.Id AS CompanyId,C.UserName CompanyName,REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,GDes.UserName GivenDesignation,EmpC.UserName EmployeeCategory

								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID

								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId

								WHERE E.EmployeeStatus = 'Active' AND   DT.Category = 'Leave' AND
								E.GroupID = '" + companyGroupId + @"' AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "' AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'  " + EmployeeCategory + @"  " + ROWiseDate + @"";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel ROPersonWiseWeekOffHolidayStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var EmployeeCategory = string.Empty;
            //if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            //{
            EmployeeCategory = "";
            //}
            //else
            //{
            //	EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            //}
            var ROWiseDate = string.Empty;

            if (hrFromDate != null && (hrToDate != null && hrToDate != "undefined"))
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
            }
            else if (hrToDate == null)
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
            }
            try
            {
                parameters.CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,SD.ShiftDefinationName ShiftDefinationName,LTRIM(RIGHT(CONVERT(VARCHAR(25), EDWSA.ShiftInTime, 100), 7)) ShiftInTime,cg.Id CompanyGroupId,cg.UserName GroupName,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,
								C.Id AS CompanyId,C.UserName CompanyName,REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,GDes.UserName GivenDesignation,EmpC.UserName EmployeeCategory

								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID

								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId

								WHERE E.EmployeeStatus = 'Active' AND DT.Category IN ('Holiday', 'Weekend') AND
								E.GroupID = '" + companyGroupId + @"' AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "' AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'  " + EmployeeCategory + @"  " + ROWiseDate + @"";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel ModalROPHRDailyShiftNotAssignedStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var EmployeeCategory = string.Empty;
            //if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            //{
            EmployeeCategory = "";
            //}
            //else
            //{
            //	EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            //}
            var ROWiseDate = string.Empty;
            var ROWiseSHDate = string.Empty;
            var ROWiseAPDDate = string.Empty;
            var ROWiseSHDDate = string.Empty;

            if (hrFromDate != null && (hrToDate != null && hrToDate != "undefined"))
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
                ROWiseAPDDate = @" CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
                ROWiseSHDate = @"AND CONVERT(DATE, ESA.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
                ROWiseSHDDate = @"CONVERT(DATE, ESA.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
            }
            else if (hrToDate == null || hrToDate == "undefined")
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
                ROWiseAPDDate = @" CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
                ROWiseSHDate = @"AND CONVERT(DATE, ESA.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
                ROWiseSHDDate = @"CONVERT(DATE, ESA.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
            }
            try
            {
                parameters.CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,SD.ShiftDefinationName ShiftDefinationName,cg.Id CompanyGroupId,cg.UserName GroupName,
								C.Id AS CompanyId,C.UserName CompanyName,REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,GDes.UserName GivenDesignation,EmpC.UserName EmployeeCategory

								FROM ORG.CompanyGroup CG
									INNER  JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

								INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id   and c.Id=E.CompanyId
									INNER JOIN  (--*
														SELECT SystemID FROM EmployeeInformation EI
															WHERE EI.SystemID NOT IN (--**
																  SELECT DISTINCT EmpSystemID FROM EmpDateWiseShiftAssign ESA
																	WHERE  " + ROWiseSHDDate + @"
													)--**
									)--*
									ESA
									ON E.SystemId = ESA.SystemId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID

								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId

									INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId

								WHERE E.EmployeeStatus = 'Active' AND
								E.GroupID = '" + companyGroupId + @"'  AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "' AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'  " + EmployeeCategory + @"  ";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel ModalROHRDailyAttdnNotProcessedStatusList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var EmployeeCategory = string.Empty;
            //if (EmplyeeTypeOrCategoryId == null || EmplyeeTypeOrCategoryId == "")
            //{
            EmployeeCategory = "";
            //}
            //else
            //{
            //	EmployeeCategory = @"AND Empc.Id = '" + EmplyeeTypeOrCategoryId + @"'";
            //}
            var ROWiseDate = string.Empty;
            var ROWiseSHDate = string.Empty;
            var ROWiseAPDDate = string.Empty;
            var ROWiseSHDDate = string.Empty;

            if (hrFromDate != null && (hrToDate != null && hrToDate != "undefined"))
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
                ROWiseAPDDate = @" CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
                ROWiseSHDate = @"AND CONVERT(DATE, ESA.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
                ROWiseSHDDate = @"CONVERT(DATE, ESA.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') AND CONVERT(DATE,'" + hrToDate + @"') ";
            }
            else if (hrToDate == null || hrToDate == "undefined")
            {
                ROWiseDate = @"AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
                ROWiseAPDDate = @" CONVERT(DATE, APD.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
                ROWiseSHDate = @"AND CONVERT(DATE, ESA.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
                ROWiseSHDDate = @"CONVERT(DATE, ESA.WorkDate) = CONVERT(DATE, '" + hrFromDate + @"')";
            }
            try
            {
                parameters.CmdText = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,SD.ShiftDefinationName ShiftDefinationName--,LTRIM(RIGHT(CONVERT(VARCHAR(25), EDWSA.ShiftInTime, 100), 7)) ShiftInTime
											,cg.Id CompanyGroupId,cg.UserName GroupName,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime,
								C.Id AS CompanyId,C.UserName CompanyName,REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,GDes.UserName GivenDesignation,EmpC.UserName EmployeeCategory

								FROM ORG.CompanyGroup CG
								INNER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								INNER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								INNER JOIN
												(--*
															   SELECT EmpSystemID FROM EmpDateWiseShiftAssign ESA
																	   WHERE ESA.EmpSystemID NOT IN (--**
																			SELECT DISTINCT EmpSystemID FROM AttdnProcessData APD
																		WHERE " + ROWiseAPDDate + @"
																		)  AND " + ROWiseSHDDate + @"--**
															  )--*
														ESA
													 ON ESA.EmpSystemID = E.SystemID
								LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = ESA.EmpSystemID
								LEFT JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								--LEFT JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = E.SystemID AND SD.SystemID =EDWSA.ShiftSystemID AND CONVERT(DATE,EDWSA.WorkDate) = CONVERT(DATE,'" + /*hrDate +*/ @"')

									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId

								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId

									INNER JOIN EmpReportingPerson EmRP ON  EmRP.EmpSystemID = E.SystemId

								WHERE E.EmployeeStatus = 'Active' AND
								E.GroupID = '" + companyGroupId + @"' AND E.CompanyId = '" + companyId + @"' AND E.PlantId = '" + plantId + "' AND EmRP.RptEmpSystemID = '" + reportingPersonId + @"'  " + EmployeeCategory + @"  ";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Reporting Person Wise Attendance Status

        public IEnumerable<object> ModalEmployeeWiseAbsentDateList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string employeeCode, string dayCount, string comparator)
        {
            var daYCountStatus = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "null")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId = '" + plantId + @"'";
            }
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }

            try
            {
                var sql = @"SELECT 	REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,APD.WorkDate WD,DATEName(DW, APD.WorkDate) WeekDays,APD.DayStatus,DT.Description
								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								INNER JOIN 	DayType	DT ON DT.DayType = APD.DayStatus

								INNER JOIN (
								SELECT COUNT(WorkDate) DaysCount, EmpSystemId FROM AttdnProcessData  APD
								INNER JOIN DayType DT ON DT.DayType = APD.DayStatus Where  DT.Category = 'Absent' AND CONVERT(DATE, APD.WorkDate) BETWEEN	CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"')
								GROUP BY EmpSystemId
								)  DayNumber ON DayNUmber.EmpSystemID = E.SystemId
								WHERE E.EmployeeStatus = 'Active'  AND E.EmployeeCode = '" + employeeCode + @"' AND DT.Category = 'Absent'
								AND
								E.GroupID = '" + companyGroupId + @"' " + company + @" " + plant + @" " + daYCountStatus + @" AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"')   AND E.EmployeeStatus = 'Active'
								";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> ModalEmployeeWiseLateDateList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string employeeCode, string dayCount, string comparator)
        {
            var daYCountStatus = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "null")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId = '" + plantId + @"'";
            }
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }

            try
            {
                var sql = @"SELECT 	REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,APD.WorkDate WD,DATEName(DW, APD.WorkDate) WeekDays,APD.DayStatus,DT.Description
								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								INNER JOIN 	DayType	DT ON DT.DayType = APD.DayStatus

								INNER JOIN (
								SELECT COUNT(WorkDate) DaysCount, EmpSystemId FROM AttdnProcessData  APD
								INNER JOIN DayType DT ON DT.DayType = APD.DayStatus Where  DT.Category = 'Late' AND CONVERT(DATE, APD.WorkDate) BETWEEN	CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"')
								GROUP BY EmpSystemId
								)  DayNumber ON DayNUmber.EmpSystemID = E.SystemId
								WHERE E.EmployeeStatus = 'Active'  AND E.EmployeeCode = '" + employeeCode + @"' AND DT.Category = 'Late'
								AND
								E.GroupID = '" + companyGroupId + @"' " + company + @" " + plant + @" " + daYCountStatus + @" AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"')   AND E.EmployeeStatus = 'Active'
								";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> ModalEmployeeWisePresentStatusDateWiseList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string EmpSystemId, string dayCount, string comparator)
        {
            var daYCountStatus = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "null")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId = '" + plantId + @"'";
            }
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }

            try
            {
                var sql = @"SELECT 	REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,APD.WorkDate WD,DATEName(DW, APD.WorkDate) WeekDays,APD.DayStatus,DT.Description
                             ,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.OutTime, 100), 7)) outTime,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime
								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								INNER JOIN 	DayType	DT ON DT.DayType = APD.DayStatus

								INNER JOIN (
								SELECT COUNT(WorkDate) DaysCount, EmpSystemId FROM AttdnProcessData  APD
								INNER JOIN DayType DT ON DT.DayType = APD.DayStatus Where  DT.Category IN('Present','Late') AND CONVERT(DATE, APD.WorkDate) BETWEEN	CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"')
								GROUP BY EmpSystemId
								)  DayNumber ON DayNUmber.EmpSystemID = E.SystemId
								WHERE E.SystemId = '" + EmpSystemId + @"' AND DT.Category IN('Present','Late')
								AND
								E.GroupID = '" + companyGroupId + @"' " + company + @"  AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"')  
								";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }




        public IEnumerable<object> ModalEmployeeWisePresentDateList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string EmpSystemId, string dayCount, string comparator)
        {
            var daYCountStatus = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "null")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId = '" + plantId + @"'";
            }
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }

            try
            {
                var sql = @"SELECT EmpSystemID,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric,EmployeeName, DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys
                                , COUNT(*) max_Present_days, format(min(WorkDate), 'dd-MMM-yyyy') fromDate, format(max(WorkDate),'dd-MMM-yyyy') toDate
                                FROM (
                                	SELECT *, sum(xx) OVER (
                                			PARTITION BY EmpSystemID ORDER BY WorkDate
                                			) ss
                                	FROM (
                                		SELECT ad.WorkDate, ad.EmpSystemID, DT.Category,EI.EmployeeCode,EI.EmployeeName, ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric 
                                		,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJS
                                									,Division.UserName Division ,Plant.UserName Plant ,Unit.UserName Unit ,Department.UserName Department ,Section.UserName Section ,SubSection.UserName SubSection ,ShiftDefination.UserName ShiftDefination ,Line.UserName Line 
                                									,Ldes.UserName LegalDesignation,ec.UserName EmployeeCategorys
                                		,CASE 
                                				WHEN Category = lag(Category) OVER (
                                						PARTITION BY EmpSystemID ORDER BY WorkDate
                                						)
                                					THEN 0
                                				ELSE 1
                                				END AS xx
                                		FROM AttdnProcessData ad
                                		INNER JOIN DayType dt ON dt.DayType = ad.DayStatus                              
                                
                                		
                                	LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = ad.EmpSystemID
                                									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                                                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
                                									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                                                    LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
                                                                       LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
                                                                        LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                                                                        LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                                                                        LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                                                        LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                                                        LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                                                        LEFT JOIN [ShiftDefination] ON ShiftDefination.SystemId = MPB.ShiftDefinationId
                                                                        LEFT JOIN [ORG].[Line] ON Line.Id = MPB.LineId
                                	 LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = EI.LegalDesignationId
                                								left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId		
                                		WHERE EI.CompanyId = '" + companyId + @"' and EI.SystemId ='" + EmpSystemId + @"'
                                			AND WorkDate BETWEEN '" + hrFromDate + @"' AND '" + hrToDate + @"'
                                		) x
                                	) y
                                WHERE Category IN ('Present', 'Late')
                                GROUP BY EmpSystemID, DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric,EmployeeName, ss
                                HAVING count(*) " + comparator + @" " + dayCount + @"
		                        ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion 2nd Part

        #region Excel Reports
        IEnumerable<ChartColumnList> chartColList;
        public IEnumerable<ChartColumnList> GetChartColumnList(IEnumerable<ChartColumnList> chartColumnList)
        {
            try
            {
                chartColList = chartColumnList;

                return chartColList;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #region Excel Reports for Absent As of Dashboard List
        public IWorkbook HRDailyAbsentStatusListForExcel(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            try
            {
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                DataTable dtEmpAbsentInfo = HRDailyAbsentStatusListSql(chartColList, companyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId);
                //dvDaily = new DataView(dtEmpAbsentInfo);

                if (dtEmpAbsentInfo.Rows.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 5;

                    #region ColumnHeaderVariables              
                    int cSrNo = 0; int cLine = 0; int cEmpCode = 0; int cEmpName = 0;
                    int cDOJ = 0; int cLegalDesig = 0; int cGDesig = 0; int cEmpCategory = 0; int cDesigGroup = 0;
                    int cBudgetedDesig = 0; int cShift = 0; int cShiftInTime = 0;
                    #endregion
                    #region ColumnHeaders
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Sr.No", ExcelHAlign.HAlignCenter); cSrNo = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Line", ExcelHAlign.HAlignCenter); cLine = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Emp Category", ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Designation Group", ExcelHAlign.HAlignCenter); cDesigGroup = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Budgeted Designation", ExcelHAlign.HAlignCenter); cBudgetedDesig = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Given Designation", ExcelHAlign.HAlignCenter); cGDesig = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Legal Designation", ExcelHAlign.HAlignCenter); cLegalDesig = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Emp.Code", ExcelHAlign.HAlignCenter); cEmpCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Employee name", ExcelHAlign.HAlignCenter); cEmpName = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "DOJ", ExcelHAlign.HAlignCenter); cDOJ = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Shift", ExcelHAlign.HAlignCenter); cShift = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "ShiftInTime", ExcelHAlign.HAlignCenter); cShiftInTime = xlsCol; xlsCol++;
                    var orgCollist = xlsCol;
                    foreach (var item in chartColList)
                    {
                        oRU.SetHeaderText(ref sheet1, xlsRow - 1, orgCollist, item.ColumnName, ExcelHAlign.HAlignCenter); xlsCol++;
                        orgCollist++;
                    }
                    endXlsCol = xlsCol;
                    #endregion
                    var slCount = 0;
                    for (int i = 0; i < dtEmpAbsentInfo.Rows.Count; i++)
                    {
                        slCount++;
                        #region Loop
                        oRU.SetText(ref sheet1, xlsRow, cSrNo, slCount.ToString());
                        oRU.SetText(ref sheet1, xlsRow, cLine, dtEmpAbsentInfo.Rows[i]["Line"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cEmpCategory, dtEmpAbsentInfo.Rows[i]["EmpCategory"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cDesigGroup, dtEmpAbsentInfo.Rows[i]["DesignationGroup"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cBudgetedDesig, dtEmpAbsentInfo.Rows[i]["BudgetedDesignation"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cGDesig, dtEmpAbsentInfo.Rows[i]["GivenDesignation"].ToString());//LegalDesignation
                        oRU.SetText(ref sheet1, xlsRow, cLegalDesig, dtEmpAbsentInfo.Rows[i]["LegalDesignation"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cEmpCode, dtEmpAbsentInfo.Rows[i]["EmployeeCode"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cEmpName, dtEmpAbsentInfo.Rows[i]["EmployeeName"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cDOJ, dtEmpAbsentInfo.Rows[i]["DOJ"].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cShift, dtEmpAbsentInfo.Rows[i]["ShiftDefinationName "].ToString());//
                        oRU.SetText(ref sheet1, xlsRow, cShiftInTime, dtEmpAbsentInfo.Rows[i]["ShiftInTime "].ToString());//
                        orgCollist = 0;
                        orgCollist = cShiftInTime + 1;
                        foreach (var item in chartColList)
                        {
                            oRU.SetText(ref sheet1, xlsRow, orgCollist, dtEmpAbsentInfo.Rows[i][item.StandardName].ToString());//
                            orgCollist++;
                        }
                    }



                    xlsRow += 1;



                    #region Line Setup
                    //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion

                    sheet1.Range[11, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[11, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[11, 4, xlsRow, 4].WrapText = true;

                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    oRU.CompanyGroupHeader(ref sheet1, endXlsCol, "Absent Information of ", null);
                    sheet1.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(endXlsCol) + 5].Merge();
                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
                }
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Sql Queries for Absent List
        private DataTable HRDailyAbsentStatusListSql(IEnumerable<ChartColumnList> chartColumnList, string companyGroupId, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
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
            var cListName = string.Empty;
            var wcc = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in chartColumnList)
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
                        else
                        {
                            cList += "," + item.StandardName + ".UserName " + item.StandardName + " ";
                            Join += "LEFT JOIN [ORG].[" + item.StandardName + "] ON " + item.StandardName + ".Id = POS." + item.StandardName + "Id\n";
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
                                wc += " AND " + item.StandardName + ".Id='" + item.Text + "'";
                            }
                        }
                    }
                }

                string sqlTxt = @"SELECT DISTINCT E.SystemId,E.EmployeeName,E.EmployeeCode,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ,SD.ShiftDefinationName ShiftDefinationName,LTRIM(RIGHT(CONVERT(VARCHAR(25), EDWSA.ShiftInTime, 100), 7)) ShiftInTime,cg.Id CompanyGroupId,cg.UserName GroupName
								,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime, 	CONVERT(char(5), CONVERT(TIME,APD.InTime - EDWSA.ShiftInTime) , 108) lateBy
                                 ,EmpC.UserName EmpCategory,ISNULL(LDes.UserName,'-') LegalDesignation,ISNULL(edsg.UserName,'-') BudgetedDesignation,ISNULL(edsgg.UserName,'-') DesignationGroup,ISNULL(GDes.UserName,'-') GivenDesignation	
                                ,C.Id AS CompanyId,C.UserName CompanyName,ISNULL(Line.UserName,'-') Line
									" + cList + @"
								FROM ORG.CompanyGroup CG
								LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT OUTER JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								LEFT OUTER JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
								LEFT OUTER JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID = APD.EmpSystemID                               

								LEFT OUTER JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = APD.EmpSystemID AND EDWSA.WorkDate = APD.WorkDate
								
								LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                                LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
								LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                LEFT JOIN ORG.Line Line ON Line.Id = E.LineId                                
								LEFT OUTER JOIN LeaveType LT ON APD.LTSystemID = LT.Id
                                LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
								LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
								 " + Join + @"
								WHERE E.EmployeeStatus = 'Active' AND
								E.GroupID = '" + companyGroupId + @"' " + wc + @" " + EmployeeCategory + @" AND  DT.Category = 'Absent' AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + hrDate + @"')  ";

                return _sqlRepository.GetDataTable(sqlTxt);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion

        #endregion
        #endregion
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 12;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
    }
}
