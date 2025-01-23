using Library.Core;
using Library.Data.Sql;
using Library.HumanResource.Report.OT;
using Library.Model.Enums;
using Library.Service.Helpers;
using Library.Service.Payrolls.OT;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using static Library.Service.Helpers.ReportUtility;

namespace Library.HumanResource.Report.Payroll
{
    public class PayrollReports
    {
        ISqlRepository _sqlRepository;
        public PayrollReports()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> GetEmpInfoYearlySalaryPorcessed(string companyGroupId, string plantId, string taxYearId, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                string fromDate = "";
                string toDate = "";
                var wcPayrollGroup = "";

                var salaryProcessColumn = "";
                string salaryProcessFlag = "";
                string wcEmpStatus = " Where (1=0 ";
                //string salaryProcessID = "";

                frtoDateTaxYear(taxYearId, out fromDate, out toDate);

                var strDOJ = @"AND DOJ<='" + toDate + @"' AND (DOS is null OR DOS>= '" + fromDate + "')";
                if (sa == true || ca == true)
                {
                    wcPayrollGroup = @"";
                }
                else
                {
                    string inPayrollGroup = "";
                    DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid FROM MST.PayrollGroupMaster WHERE PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");
                    DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable(@"SELECT SystemId FROM EmployeeInformation E 
                    WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

                    if (dtPayRollGrpEmpId.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
                        {
                            inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
                        }
                        if (dtNotPayRollGrpEmpId.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
                            {
                                inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
                            }
                        }
                        wcPayrollGroup = @"AND E.SystemId  IN (" + inPayrollGroup + @")";
                    }
                    else
                    {
                        wcPayrollGroup = @"";
                    }

                    //wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
                }


                string strSqlSal = @"SELECT  m.SystemID FROM SalaryProcMaster m
                                    INNER JOIN SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID and c.PlantID='" + plantId + @"'
                                        WHERE 1=1 
                                        " + getMonthYear(fromDate, toDate, "MonthNo", "YearNo") + @"";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
                string salaryProcessId = "''";
                dtSalPrcId = dtSalPrcId.DefaultView.ToTable(true, "SystemID");
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }

                salaryProcessFlag = ", Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag";
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
                //}





                wcEmpStatus += ")";

                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when isnull(DOS,'') <> '' then   'Separated' else 'Regular' end SalaryProcFlag
                                    
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(E.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                     FROM EmployeeInformation e
                                
								    LEFT JOIN mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
								LEFT JOIN mst.DesignationMaster dm on dm.id=m.DesignationMasterId
								LEFT JOIN hkp.EmployeeCategory EmpC on EmpC.Id = dm.EmployeeCategoryId
								LEFT JOIN hkp.LegalDesignation ld on ld.Id = e.LegalDesignationId
                                    --LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=SPLD.LegalDesignationId
                                   
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=E.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId                                   
			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    
								    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = E.BankSystemID
									left join [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
   
                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"                                
                                     ) DD " + wcEmpStatus + @" ORDER BY ISNULL(EmployeeCodePreFix,''),ISNULL(EmployeeCodeNumeric,0)";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetEmpInfoYearlySalaryPorcessedFromYear(string companyGroupId, string plantId, string ToYear, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity, string ToMonth, string FromYear, string FromMonth)
        {
            try
            {

                string fromDate = "";
                string toDate = "";
                var wcPayrollGroup = "";

                string salaryProcessFlag = "";
                string wcEmpStatus = " Where (1=0 ";
               
                fromtoDateTaxYear(FromYear, FromMonth, ToYear, ToMonth, out fromDate, out toDate);

                if (Convert.ToDateTime(fromDate) > Convert.ToDateTime(toDate))
                {
                    throw new Exception("From month cannot be greater than To month");
                }

                if (((Convert.ToDateTime(toDate).Year - Convert.ToDateTime(fromDate).Year) * 12) + Convert.ToDateTime(toDate).Month - Convert.ToDateTime(fromDate).Month > 12)
                    throw new Exception("Cannot be greater than 1 year");

                var strDOJ = @"AND DOJ<='" + toDate + @"' AND (DOS is null OR DOS>= '" + fromDate + "')";
                if (sa == true || ca == true)
                {
                    wcPayrollGroup = @"";
                }
                else
                {
                    string inPayrollGroup = "''";
                    DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid FROM MST.PayrollGroupMaster WHERE PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");
                    DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable(@"SELECT SystemId FROM EmployeeInformation E 
                    WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

                    if (dtPayRollGrpEmpId.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
                        {
                            inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
                        }
                        if (dtNotPayRollGrpEmpId.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
                            {
                                inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
                            }
                        }
                        wcPayrollGroup = @"AND E.SystemId  IN (" + inPayrollGroup + @")";
                    }
                    else
                    {
                        wcPayrollGroup = @"";
                    }

                   
                }             

                salaryProcessFlag = ", Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag";
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
               

                wcEmpStatus += ")";

                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when isnull(DOS,'') <> '' then   'Separated' else 'Regular' end SalaryProcFlag
                                    
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(E.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                     FROM EmployeeInformation e
                                
								    LEFT JOIN mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
								LEFT JOIN mst.DesignationMaster dm on dm.id=m.DesignationMasterId
								LEFT JOIN hkp.EmployeeCategory EmpC on EmpC.Id = dm.EmployeeCategoryId
								LEFT JOIN hkp.LegalDesignation ld on ld.Id = e.LegalDesignationId
                                   
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=E.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId                                   
			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    
								    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = E.BankSystemID
									left join [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
   
                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"                                
                                     ) DD " + wcEmpStatus + @" ORDER BY ISNULL(EmployeeCodePreFix,''),ISNULL(EmployeeCodeNumeric,0)";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetEmpInfoDaily(string companyGroupId, string plantId, string effectiveDate, string attdnStatusCatg, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var wcPayrollGroup = "";
                var wcSalaryProcess = "";
                var salaryProcessJoin = "";
                var salaryProcessColumn = "";
                var strDOJ = "";
                string salaryProcessFlag = "";
                string salaryProcessId = "STRUCTURE";
                string wcEmpStatus = " Where (1=0 ";

                if (sa == true || ca == true)
                {
                    wcPayrollGroup = @"";
                }
                else
                {
                    string inPayrollGroup = "' '";
                    DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid FROM MST.PayrollGroupMaster WHERE PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");
                    DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable(@"SELECT SystemId FROM EmployeeInformation E 
                    WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

                    if (dtPayRollGrpEmpId.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
                        {
                            inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
                        }
                        if (dtNotPayRollGrpEmpId.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
                            {
                                inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
                            }
                        }
                        wcPayrollGroup = @"AND E.SystemId  IN (" + inPayrollGroup + @")";
                    }
                    else
                    {
                        wcPayrollGroup = @"";
                    }
                    //wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
                }
                if (salaryProcessId == "STRUCTURE")
                {
                    salaryProcessColumn = "";
                    salaryProcessJoin = "";
                    wcSalaryProcess = "";
                    strDOJ = "AND DOJ<='" + effectiveDate + @"' AND (DOS is null OR DOS>= '" + effectiveDate + @"')";


                }
                else if (!string.IsNullOrEmpty(salaryProcessId))
                {
                    salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";
                    salaryProcessJoin = @"  LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                                    LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')";
                    wcSalaryProcess = @"AND SPC.SlrProcMstSystemID IN('" + salaryProcessId + @"')";

                }
                else if (string.IsNullOrEmpty(salaryProcessId) == true && salaryProcessId != "STRUCTURE")
                {
                    salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";
                    salaryProcessJoin = @"  LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                                    LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')";

                    wcSalaryProcess = @" AND SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + effectiveDate + @"') AND YearNo =  YEAR('" + effectiveDate + @"')  )";
                }
                if (salaryProcessId == "STRUCTURE")
                {
                    wcEmpStatus = " Where (1=1 ";
                    salaryProcessFlag = "";
                }
                else
                {
                    salaryProcessFlag = ", Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag";
                    wcEmpStatus = " Where (1=0 ";

                    if (isActive == true && isSeperated == true && isMaternity == true)
                    {
                        wcEmpStatus = " Where (1=1 ";
                    }
                    else
                    {
                        if (isActive == true)
                        {
                            wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                        }
                        if (isSeperated == true)
                        {
                            wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                        }
                        if (isMaternity == true)
                        {
                            wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                        }
                    }
                }

                wcEmpStatus += ")";

                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    ,ISNULL(dt.Category,'')  attdnStatus
                                    " + salaryProcessFlag + @"
                                    " + salaryProcessColumn + @"
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                    FROM EmployeeInformation e                               
                                   
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                   
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId

                                      left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
                                    left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = dm.EmployeeCategoryId
			                             
			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    " + salaryProcessJoin + @"
                                    LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
                                    INNER JOIN AttdnProcessData adp ON adp.EmpSystemID = e.SystemId and adp.WorkDate = '" + effectiveDate + @"'
                                    INNER JOIN DayType dt ON dt.DayType = adp.DayStatus  and Dt.Category IN ('Present','Late','Leave')
   
                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"  " + wcSalaryProcess + @"                                       
                                     ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IWorkbook GetEmployeeSalaryStructureDaily(string companyGroupId, string companyId, string plantId, string userId, string effectiveDate, string payRollGroup, Dictionary<string, string> parameters, string payDays)
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsSlrProc = null;
            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

            #endregion Variable

            try
            {

                if (clsStaticInfo.dbl(payDays) <= 0)
                {
                    throw new Exception("Days can not be zero.");
                }

                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                ParamList para = new ParamList();

                #endregion Variable


                #region DataSet

                Dictionary<string, double> dicNW = null;
                Dictionary<string, double> dicW = null;
                Dictionary<string, double> dicH = null;
                List<SalaryStructureReport> listdsSlrProc = new List<SalaryStructureReport>();
                GetEmpSalaryInformationRpt(plantId, effectiveDate, payRollGroup, parameters, out dsSlrProc);
                dvEmp = new DataView();
                dvEmp.Table = dsSlrProc.Tables[0];

                if (dsSlrProc.Tables[0].Rows.Count > 0)
                {
                    listdsSlrProc = dsSlrProc.Tables[0].ToList<SalaryStructureReport>();
                }
                else
                {
                    throw new Exception("No Data Found");
                }
                DataTable dtEmployees = dvEmp.ToTable(true, "SystemID", "Department", "LegalDesignation", "DOJ", "DOS", "DOB", "Grade", "GradeCode", "EmployeeName", "EmployeeCode", "SalaryHeadValue", "Line", "Gender", "PayRollGroup", "JobLocation", "PaymentMode", "BankName", "Section", "Unit", "OTHr", "OTConsiderOn", "OTHrMinute");


                objRpt.SelectedPlantWiseCompany(para.PlantId, out dsCmp);

                objRpt.SelectedPlant(para.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 5;
                xlsCol = 1;

                int ColSr = 0;
                int ColIDNo = 0;
                int ColName = 0;
                int ColDOJ = 0;
                int ColDOB = 0;
                int ColDOs = 0;
                int ColGrade = 0;
                int ColGVDG = 0;
                int ColGrs = 0;
                int ColDepartment = 0;
                int ColSection = 0;
                int ColUnit = 0;
                int ColLine = 0;
                int ColpayrollGroup = 0;
                int ColpaymentMode = 0;
                int ColJobLocation = 0;
                int ColGender = 0;


                //1
                ru.SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                ru.SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 8);
                ru.SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 30);
                ru.SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                ru.SetCellValue("DOB", sheet1, xlsRow, ref xlsCol, out ColDOB, 12);
                ru.SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOs, 12);
                ru.SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 20);
                ru.SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out ColDepartment, 20);
                ru.SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out ColSection, 20);
                ru.SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out ColUnit, 20);
                ru.SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out ColLine, 20);
                ru.SetCellValue("Payroll Group", sheet1, xlsRow, ref xlsCol, out ColpayrollGroup, 20);
                ru.SetCellValue("Payment Mode", sheet1, xlsRow, ref xlsCol, out ColpaymentMode, 20);
                ru.SetCellValue("Job Location", sheet1, xlsRow, ref xlsCol, out ColJobLocation, 20);
                ru.SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out ColGender, 20);
                ru.SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out ColGrade, 20);
                ru.SetCellValue("OT Hours", sheet1, xlsRow, ref xlsCol, out int ColOTHours, 20);
                //ru.SetCellValue("OT Rate", sheet1, xlsRow, ref xlsCol, out int ColOTRate, 20);
                ru.SetCellValue("OT Amount", sheet1, xlsRow, ref xlsCol, out int ColOTAmount, 20);


                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, xlsCol - 1].Merge();
                ColGrs = xlsCol - 1;

                DataView dvSalaryHead = new DataView(dsSlrProc.Tables[0]);
                dvSalaryHead.Sort = "HeadType desc,Sequence";
                DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo");

                int _count_earning_head = 0;
                int _count_earning_ctchead = 0;
                int _count_deducting_head = 0;
                int _total_head_count = 0;

                List<SalaryHeadSequence> list = null;

                CreateDynamicSHeadDstr(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);

                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_ctchead].Merge();
                }

                int ds = ColGrs + _count_earning_ctchead + 1;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                int np = 0;
                if (list.Count > 0)
                {
                    //xlsCol++;
                    np = ColGrs + list.Count;

                }
                xlsCol = ColGrs + _count_earning_ctchead + 1;
                ru.SetCellValue("Total Gross", sheet1, xlsRow, ref xlsCol, out int ColTotalGross, 20);
                ru.SetCellValue("Total CTC", sheet1, xlsRow, ref xlsCol, out int ColTotalCTC, 20);

                DataSet dsCurrency = null;
                DataSet dsOTPolicy = null;
                DataSet dsSStructure = null;
                string _currencyId = "";
                clsOTCalculation otc = new clsOTCalculation();
                otc.LoadOverTimePolicy(plantId, effectiveDate, effectiveDate, out dsOTPolicy);
                LoadSalaryStructureAttdn(plantId, effectiveDate, effectiveDate, out dsSStructure);

                clsSalaryInfo objSal = new clsSalaryInfo();
                objSal.GetLocalCurrency(companyGroupId, plantId, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }

                GenerateDic(dsOTPolicy, dsSStructure, _currencyId, out dicNW, out dicW, out dicH);


                endXlsCol = np;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;


                ru.Header(ref sheet1, param, endXlsCol, "Employee Salary Information");

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------

                int SrNo = 0;
                string x = "";

                ReportUtility oRU = new ReportUtility();

                xlsRow = RowIndex;

                int startRow = xlsRow;
                xlsRow--;
                DataRow drOT = null;
                //Test();
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    SrNo += 1;
                    x = dtEmployees.Rows[i]["SystemID"].ToString().Trim();
                    string yot = string.Empty;
                    double nwRate = 0;
                    //1
                    sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                        sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOB"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOB].Text = dtEmployees.Rows[i]["DOB"].ToString();
                    sheet1.Range[xlsRow, ColDOB].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOB].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOs].Text = dtEmployees.Rows[i]["DOS"].ToString();
                    sheet1.Range[xlsRow, ColDOs].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOs].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                    sheet1.Range[xlsRow, ColGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                    sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                    sheet1.Range[xlsRow, ColGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                        sheet1.Range[xlsRow, ColpayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, ColpayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColpayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColpaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                    sheet1.Range[xlsRow, ColpaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColpaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDepartment].Text = dtEmployees.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, ColDepartment].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDepartment].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Section"].ToString()) == false)
                        sheet1.Range[xlsRow, ColSection].Text = dtEmployees.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, ColSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSection].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Line"].ToString()) == false)
                        sheet1.Range[xlsRow, ColLine].Text = dtEmployees.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, ColLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColLine].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                        sheet1.Range[xlsRow, ColpayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, ColpayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColpayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                    sheet1.Range[xlsRow, ColJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Line"].ToString()) == false)
                        sheet1.Range[xlsRow, ColLine].Text = dtEmployees.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, ColLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColLine].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Unit"].ToString()) == false)
                        sheet1.Range[xlsRow, ColUnit].Text = dtEmployees.Rows[i]["Unit"].ToString();
                    sheet1.Range[xlsRow, ColUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    #endregion
                    nwRate = dicNW[x];
                    ru.GetOT(dtEmployees.Rows[i]["OTConsiderOn"].ToString(), dtEmployees.Rows[i]["OTHrMinute"].ToString(), out yot);


                    sheet1.Range[xlsRow, ColOTHours].Text = yot;
                    sheet1.Range[xlsRow, ColOTHours].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, ColOTHours].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColOTHours].NumberFormat = ru.NumberFormatDecimalTwo();

                    double amt = clsStaticInfo.dbl(dtEmployees.Rows[i]["OTHr"].ToString()) * nwRate;
                    sheet1.Range[xlsRow, ColOTAmount].Number = amt;
                    sheet1.Range[xlsRow, ColOTAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColOTAmount].NumberFormat = ru.NumberFormatDecimalTwo();

                    int _total_head_count_body = 0;

                    for (int ci = 0; ci < list.Count; ci++)
                    {
                        #region Head wise loop
                        var ob = list[ci];
                        if (ob.SalaryHead.Length > 0)
                        {
                            var hId = ob.SalaryHeadId;
                            _total_head_count_body++;

                            var _data = listdsSlrProc.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == x).FirstOrDefault();

                            if (_data != null)
                            {

                                sheet1.Range[xlsRow, ob.XLColIndex].Number = clsStaticInfo.dbl(bplib.clsWebLib.GetNumData(_data.EntryAmount > 0 ? _data.EntryAmount.ToString() : (_data.EntryAmount * (-1)).ToString())) / clsStaticInfo.dbl(payDays);

                                sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);// oRU.NumberFormatInt();
                                sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }//row found
                        }// 
                        #endregion
                        //});
                    }//for dtSalaryHead

                    sheet1.Range[xlsRow, ColTotalCTC].Formula = "= SUM(" + ru.GetColumnNameForXls(ColOTAmount) + (xlsRow) + "+" + ru.GetColumnNameForXls(ColOTAmount + 2) + (xlsRow) + ")";
                    sheet1.Range[xlsRow, ColTotalCTC].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, ColTotalCTC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColTotalCTC].NumberFormat = ru.NumberFormatDecimalTwo();
                    sheet1.Range[xlsRow, ColTotalGross].Formula = "= SUM(" + ru.GetColumnNameForXls(ColOTAmount) + (xlsRow) + "+" + ru.GetColumnNameForXls(ColOTAmount + 1) + (xlsRow) + ")";
                    sheet1.Range[xlsRow, ColTotalGross].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, ColTotalGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColTotalGross].NumberFormat = ru.NumberFormatDecimalTwo();

                    xlsRow++;
                }//for emp count
                sheet1.Range[xlsRow, ColGrade].Text = "Total";
                sheet1.Range[xlsRow, ColGrade].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, ColGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, ColGrade].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, ColOTHours].Formula = "= SUM(" + ru.GetColumnNameForXls(ColOTHours) + (startRow - 1) + ":" + ru.GetColumnNameForXls(ColOTHours) + (xlsRow - 1) + ")";//clsStaticInfo.dbl(bplib.clsWebLib.GetNumData(_data.EntryAmount > 0 ? _data.EntryAmount.ToString() : (_data.EntryAmount * (-1)).ToString())) / clsStaticInfo.dbl(payDays);

                //sheet1.Range[xlsRow, ColOTHours].NumberFormat = ru.NumberFormatDecimalTwo();// oRU.NumberFormatInt();
                //sheet1.Range[xlsRow, ColOTHours].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[xlsRow, ColOTHours].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, ColOTHours].CellStyle.Font.Bold = true;

                sheet1.Range[xlsRow, ColOTAmount].Formula = "= SUM(" + ru.GetColumnNameForXls(ColOTAmount) + (startRow - 1) + ":" + ru.GetColumnNameForXls(ColOTAmount) + (xlsRow - 1) + ")";//clsStaticInfo.dbl(bplib.clsWebLib.GetNumData(_data.EntryAmount > 0 ? _data.EntryAmount.ToString() : (_data.EntryAmount * (-1)).ToString())) / clsStaticInfo.dbl(payDays);

                sheet1.Range[xlsRow, ColOTAmount].NumberFormat = ru.NumberFormatDecimalTwo();// oRU.NumberFormatInt();
                sheet1.Range[xlsRow, ColOTAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, ColOTAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, ColOTAmount].CellStyle.Font.Bold = true;
                for (int ci = 0; ci < list.Count; ci++)
                {
                    #region Head wise loop
                    var ob = list[ci];
                    if (ob.SalaryHead.Length > 0)
                    {
                        sheet1.Range[xlsRow, ob.XLColIndex].Formula = "= SUM(" + ru.GetColumnNameForXls(ob.XLColIndex) + (startRow - 1) + ":" + ru.GetColumnNameForXls(ob.XLColIndex) + (xlsRow - 1) + ")";//clsStaticInfo.dbl(bplib.clsWebLib.GetNumData(_data.EntryAmount > 0 ? _data.EntryAmount.ToString() : (_data.EntryAmount * (-1)).ToString())) / clsStaticInfo.dbl(payDays);

                        sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);// oRU.NumberFormatInt();
                        sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, ob.XLColIndex].CellStyle.Font.Bold = true;

                    }
                    #endregion

                }

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
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 7;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "SalaryInformation";
                #endregion

                workbook.Version = ExcelVersion.Excel2013;
                string strFileName = "SalaryInfo" + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
            }
        }
        void GenerateDic(DataSet dsPolicy, DataSet dsSalaryStruc, string _currencyId, out Dictionary<string, double> dicNW, out Dictionary<string, double> dicW, out Dictionary<string, double> dicH)
        {
            double nwRate = 0;
            double wRate = 0;
            double hRate = 0;
            dicNW = null;
            dicW = null;
            dicH = null;
            try
            {
                DataTable dtemp = new DataView(dsSalaryStruc.Tables[0]).ToTable(true, "EmpInfoSystemID");
                dicNW = new Dictionary<string, double>();
                dicW = new Dictionary<string, double>();
                dicH = new Dictionary<string, double>();
                for (int i = 0; i < dtemp.Rows.Count; i++)
                {
                    string _empid = dtemp.Rows[i]["EmpInfoSystemID"].ToString();
                    GetFormula(dsPolicy, dsSalaryStruc, _currencyId, _empid, out nwRate, out wRate, out hRate);
                    dicNW.Add(_empid, nwRate);
                    dicW.Add(_empid, wRate);
                    dicH.Add(_empid, hRate);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetFormula(DataSet dsPolicy, DataSet dsSalaryStruc, string _currencyId, string empid, out double nwRate, out double wRate, out double hRate)
        {
            nwRate = 0;
            wRate = 0;
            hRate = 0;
            //out string FormulaDesIDN, out string FormulaDesIDW, out string FormulaDesIDH
            string FormulaDesIDN = string.Empty;
            string FormulaDesIDW = string.Empty;
            string FormulaDesIDH = string.Empty;
            try
            {
                DataView dv = new DataView(dsPolicy.Tables[0]);
                dv.RowFilter = "systemid='" + empid + "'";
                if (dv.Count > 0)
                {
                    FormulaDesIDN = dv[0]["FormulaDesIDN"].ToString();
                    FormulaDesIDW = dv[0]["FormulaDesIDW"].ToString();
                    FormulaDesIDH = dv[0]["FormulaDesIDH"].ToString();
                    string EmployeeCode = dv[0]["EmployeeCode"].ToString();

                    if (string.IsNullOrEmpty(FormulaDesIDN))
                    {
                        throw new Exception("Employee " + EmployeeCode + " has no OT policy with her/his designation ...");
                    }


                    DataView dvss = new DataView(dsSalaryStruc.Tables[0]);
                    dvss.RowFilter = "EmpInfoSystemID='" + empid + "'";
                    if (dvss.Count > 0)
                    {
                        string FormulaValue = string.Empty;
                        DataTable dtValue = dvss.ToTable();
                        DataTable dtSalaryHead = dvss.ToTable(true, "SalaryHeadID", "SalaryHead");


                        // GetFormulValue(FormulaDesIDH, ref dtValue, _currencyId, out hRate, ref dtSalaryHead);

                        //GetFormulValue(FormulaDesIDW, ref dtValue, _currencyId, out wRate, ref dtSalaryHead);

                        GetFormulValue(FormulaDesIDN, ref dtValue, _currencyId, out nwRate, ref dtSalaryHead);

                    }//if
                }//if dv

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetFormulValue(string FormulaDesIDN, ref DataTable dtValue, string _currencyId, out double nwRate, ref DataTable dtSalaryHead)
        {
            string FormulaValue = string.Empty;
            nwRate = 0;
            try
            {
                clsSalaryUtility su = new clsSalaryUtility();
                su.ReLoadFormulaWithValue(FormulaDesIDN, ref dtValue, _currencyId, "1", out FormulaValue, ref dtSalaryHead);
                string sFormulaResult = clsSalaryStructureAplos.Evaluate(FormulaValue).ToString();
                if (sFormulaResult == "NaN")
                {
                    throw new Exception("Salary Head is not orderly tagged in Salary Rule");
                }

                //get formula wise value
                var vv = Convert.ToDouble(sFormulaResult).ToString("00.00");
                nwRate = Convert.ToDouble(vv);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void LoadSalaryStructureAttdn(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                            select m.*,d.EntryAmount Amount,d.DefineAmount,d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType 
                            ,LD.UserName OldLegalDesignation, LG.Code OldGradeCode from
                                (---m
                            select max(ed) ed,EmpInfoSystemID from
                            (
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid='" + sPlantID + @"' group by EmpInfoSystemID
												                            union 
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid='" + sPlantID + @"'  group by EmpInfoSystemID
                            ) x 
                            group by EmpInfoSystemID
                            ) ---m
                            mx
                            left join (
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid='" + sPlantID + @"'
                            union
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid='" + sPlantID + @"'
                            )
                             m on m.EmpInfoSystemID=mx.EmpInfoSystemID and m.EffectiveDate=mx.ed
                            left join (
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoDefine
                            union
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoBack
                            )
                            d on d.SalaryID=m.SystemID
                            left join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
                            LEFT JOIN IncrementHistory IH on IH.ToSalaryId=d.SalaryID
                            
                            LEFT JOIN Hkp.LegalDesignation LD ON LD.Id = ih. FromLegalDesignationId
                            LEFT JOIN MST.LegalSalaryGradeDesignation LGD ON LGD.LegalDesignationId = ih.FromLegalDesignationId AND LGD.PlantId='" + sPlantID + @"'
                            
                            LEFT JOIN scs.LegalSalaryGrade LG ON LG.Id = LGD.LegalSalaryGradeId
                            INNER JOIN AttdnProcessData adp ON adp.EmpSystemID = m.EmpInfoSystemID and adp.WorkDate = '" + sFromDate + @"'
                            INNER JOIN DayType dt ON dt.DayType = adp.DayStatus  and Dt.Category IN ('Present','Late','Leave')

                            ORDER BY m.EmpInfoSystemID";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);
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

        private void CreateDynamicSHeadDstr(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list)
        {
            try
            {
                list = new List<SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                int countGrossPostion = 0;
                string deductionFormula = "";

                xlsCol += 1;
                countGrossPostion++;

                //salaryHSGross.SalaryHeadId = "Gross";

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {


                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper())
                        {
                            _total_head_count++;


                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;


                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;



                            list.Add(salaryHeadSequence);
                            countCTCPosition++;
                        }


                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;


                _count_earning_ctchead = countCTCPosition - 1;

                int countDeductionPosition = countCTCPosition - 1;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        //{
                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                        {
                            _total_head_count++;
                            countDeductionPosition++;

                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 10;
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
                            //sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
                            if (deductionFormula.Length == 0)
                            {
                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                            }
                            else
                            {
                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                            }

                            //countDeductionPosition++;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;

                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();


                            list.Add(salaryHeadSequence);

                            _count_deducting_head++;
                        }
                        //}//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetEmpSalaryInformationRpt(string plantId, string effectiveDate, string payRollGroup, Dictionary<string, string> parameters, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {

                obs = new clsStaticInfo();
                strSql = @"SELECT * FROM
                          (
                           SELECT E.SystemID,  E.EmployeeCode EmployeeCode, E.EmployeeName, REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB,
	                              E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,
	                              E.GenderID GenderName, REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                  REPLACE(Convert(VARCHAR(11), E.DOS, 106), ' ', '-') AS DOS,
	                              REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC, ISNULL(LG.UserName,'') LegalDesignation
								  , L.UserName Line, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
								  S.UserName Section, SB.UserName SubSection, EC.UserName AS EmpCategory, Cm.UserName CompanyName
								  ,  E.EmployeeCategorySystemID, E.UnitID, E.DivisionID, E.DepartmentID, E.DesignationSystemID,
	                              E.SectionID, E.SubSectionID, E.LineID, E.DesignationGroupID, E.SubSecStrucSystemID, E.EmployeeStatus,
	                              P.UserName PlantName, 
	                              GC.UserName GroupName,
	                              E.PlantID, BK.UserName BankNameShort, E.BankAccNo, 
								  EmpSlr.SalaryHeadID, SH.SalaryHead, ISNULL(PSH.Sequence, 99) Sequence, SH.HeadType, ISNULL(SH.HeadCategory,'') HeadCategory, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount,
	                              EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinationRate
	                              , EmpSlr.EmpInfoSystemID, MW.SalaryHeadValue                                
	                            ,CRC.IntegerInDisb, CRC.DecimalNo, MW.Grade,CRC.IsDecimalInDisb IsDecimal
                                ,ISNULL(E.GenderID,'') Gender,ISNULL(LSalGr.Code,'') GradeCode
                                    ,ISNULL(adp.OTHr,0)/60 OTHr,ISNULL(adp.OTHr,0) OTHrMinute,pwhr.OTConsiderOn

											,ISNULL(PG.UserName,'') PayRollGroup

                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
				            FROM (SELECT * FROM EmployeeInformation  WHERE (EmployeeStatus != 'Separated' or DOS is null or DOS >='" + effectiveDate + @"')) AS E

                                          
                                           LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode                                      								      

											LEFT OUTER JOIN ORG.Position PO ON MB.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] Dp ON Dp.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] Dv ON Dv.Id = EN.DivisionId
                                 
                                    LEFT JOIN [ORG].[Section] S ON S.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] SB ON SB.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] U ON U.Id = EN.UnitId
                                            LEFT JOIN ORG.Line L ON MB.LineID = L.Id
											LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                  
                                            LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                                            LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id and E.PlantId = LSGD.PlantId
                                            LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId  and E.PlantId = LSalGr.PlantId
											inner JOIN AttdnProcessData adp ON adp.EmpSystemID = e.SystemId and adp.WorkDate = '" + effectiveDate + @"'
                                            inner JOIN DayType dt ON dt.DayType = adp.DayStatus  and Dt.Category IN ('Present','Late','Leave')
											LEFT JOIN ORG.Plant AS p ON E.PlantId = p.Id
											LEFT JOIN ORG.Company AS Cm ON E.CompanyID = Cm.Id
											LEFT JOIN ORG.CompanyGroup AS GC ON E.GroupID = GC.Id
											LEFT JOIN HKP.Bank AS BK ON E.BankSystemID = BK.Id
								 LEFT JOIN [dbo].[PlantWiseHRMSSetting] pwhr ON pwhr.PlantID=e.PlantID
                                            left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
                                            left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                            left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
											LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + effectiveDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = E.SystemId

										
												INNER JOIN (
													SELECT * FROM
																(
																 --SELECT MST.EmpInfoSystemID, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
																	--	EmpSlr.AmtDefinitionCurrencyID AmtDefinationCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID
																-- FROM SalaryInfoDefine EmpSlr
																	--	INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID 
                                                                   Select SalaryDetails.* from  ( SELECT MAX(EffectiveDate) EffectiveDate,EmpInfoSystemID--,SalaryHead,SalaryHeadID,EntryCurrencyID
	 FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1 
					left outer join SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID where HeadCategory IN ('GROSS','CTC')
					--where EmpInfoSystemID = '1800118'
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID where HeadCategory IN ('GROSS','CTC')
					--where EmpInfoSystemID = '1800118'
                        )dd where EffectiveDate <= '" + effectiveDate + @"' 					

					GROUP BY EmpInfoSystemID) effDateSalary


					Inner JOIN
					
            ( SELECT EmpInfoSystemID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount 
			,AmtDefinitionCurrencyID , AmtDefinationRate, SalaryRuleMasterSystemID,EffectiveDate
	            FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1
					LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID where HeadCategory IN ('GROSS','CTC')
				--	WHERE EmpInfoSystemID = '1800118'
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID where HeadCategory IN ('GROSS','CTC')
				--	where EmpInfoSystemID = '1800118'
                )dd where EffectiveDate <= '" + effectiveDate + @"'  ) SalaryDetails ON effDateSalary.EffectiveDate= SalaryDetails.EffectiveDate and effDateSalary.EmpInfoSystemID = SalaryDetails.EmpInfoSystemID



                                                                  -----------------------AND MST.IsApproved = 1---------------------
																) A
																
													) EmpSlr ON E.SystemID = EmpSlr.EmpInfoSystemID
										LEFT JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
										LEFT JOIN (SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId='" + plantId + @"') PSH ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
										
										LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpSlr.SalaryRuleMasterSystemID 
										--LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID	AND SRG.SalaryHeadID = SH.SalaryHeadID									
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID

                                        
                         ) A  where  ISNULL(EmpInfoSystemID,'')<>'' AND PlantID = '" + plantId + @"' AND
                            Convert(date ,DOJ) <='" + effectiveDate + @"' AND (DOS IS NULL OR DOS >='" + effectiveDate + @"') ";

                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSql += @"and EmpInfoSystemID IN(" + parameters["EmpSystemId"] + ")";

                    }
                }


                strSql = strSql + @" ORDER BY EmployeeCode";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void frtoDateTaxYear(string taxYearId, out string FromDate, out string ToDate)
        {
            FromDate = "";
            ToDate = "";
            DataTable dtTaxYear = null;
            dtTaxYear = _sqlRepository.GetDataTable("SELECT * FROM SCS.TaxYear WHERE TaxYearName = '" + taxYearId + @"'");

            int fromYear = Convert.ToDateTime(dtTaxYear.Rows[0]["StartDate"]).Year;//EndDate
            int toYear = Convert.ToDateTime(dtTaxYear.Rows[0]["EndDate"]).Year;
            FromDate = Convert.ToDateTime(dtTaxYear.Rows[0]["StartDate"]).ToString("dd-MMM-yyyy");
            ToDate = Convert.ToDateTime(dtTaxYear.Rows[0]["EndDate"]).ToString("dd-MMM-yyyy");
        }
        public void fromtoDateTaxYear(string FromYear, string FromMonth, string ToYear, string ToMonth, out string FromDate, out string ToDate)
        {
            FromDate = "";
            ToDate = "";
            DataTable dtTaxFromYear = null;
            DataTable dtTaxToYear = null;
            dtTaxFromYear = _sqlRepository.GetDataTable("select StartDate from scs.TaxYearPeriod where datename(month,enddate) = '" + FromMonth + "' and datename(year,enddate)='" + FromYear + "'");
            dtTaxToYear = _sqlRepository.GetDataTable("select EndDate from scs.TaxYearPeriod where datename(month,enddate) = '" + ToMonth + "' and datename(year,enddate)='" + ToYear + "'");

            FromDate = Convert.ToDateTime(dtTaxFromYear.Rows[0]["StartDate"]).ToString("dd-MMM-yyyy");
            ToDate = Convert.ToDateTime(dtTaxToYear.Rows[0]["EndDate"]).ToString("dd-MMM-yyyy");
        }
        public string getMonthYear(string fromDate, string toDate, string monthNo, string yearNo)
        {
            var r = "";
            var _fDate = Convert.ToDateTime(fromDate);
            var _tDate = Convert.ToDateTime(toDate);
            while (_fDate < _tDate)
            {
                if (r.Length == 0)
                {
                    r = " (" + monthNo + " =" + _fDate.Month + " AND " + yearNo + " =" + _fDate.Year + ")";
                }
                else
                {
                    r += " OR (" + monthNo + " =" + _fDate.Month + " AND " + yearNo + " =" + _fDate.Year + ")";

                }
                _fDate = _fDate.AddMonths(1);

            }
            if (r.Length > 0)
            {
                r = " AND (" + r + ")";
            }

            return r;
        }

        public DataTable GetMonthYearDataTable(string fromDate, string toDate)
        {

            var _fDate = Convert.ToDateTime(fromDate);
            var _tDate = Convert.ToDateTime(toDate);
            //List<DataRow> _data = new List<DataRow>();
            string year = "";
            string month = "";
            DataTable dtMonth = new DataTable();
            dtMonth.Columns.Add("Year", typeof(String));
            dtMonth.Columns.Add("Month", typeof(String));
            int i = 0;
            while (_fDate < _tDate)
            {
                dtMonth.Rows.Add(i);
                dtMonth.Rows[i]["Year"] = _fDate.Year;
                dtMonth.Rows[i]["Month"] = _fDate.Month;


                _fDate = _fDate.AddMonths(1);
                i++;

            }


            return dtMonth;
        }

        public IWorkbook GetEmployeeSalaryProcessedReportSalaryYearly(string companyGroupId, string companyId, string plantId, string userId, string taxYearId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool withGoodWork)
        {
            #region Variable
            clsReport objRpt = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

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
                Dictionary<string, double> subTotalDictSalaryProcess = new Dictionary<string, double>();
                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet
                string fromDate = "";
                string toDate = "";
                DataTable dtMonthYear = null;

                frtoDateTaxYear(taxYearId, out fromDate, out toDate);
                dtMonthYear = GetMonthYearDataTable(fromDate, toDate);
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();
                Dictionary<string, DataRow> dicAttdn = new Dictionary<string, DataRow>();
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();

                GetYearlyEmployeeInfoDetailSalaryLogWise(companyGroupId, companyId, plantId, fromDate, toDate, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo, out dicAttdn);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetYearlyEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fromDate, toDate, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------

                xlsCol = 1;

                #region Column Variables
                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0
                   , ColLWP = 0, colBank = 0, cDMP = 0, colBankAccountNo = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;

                #endregion

                //1
                //SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                //SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                //SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                //SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                //SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                //SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                //SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                //SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                //SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                //SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                //SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                //SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                //SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                //SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                //SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                //SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                //SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                ////SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                //SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                //SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                //SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                //SetCellValue("IFSCCode", sheet1, xlsRow, ref xlsCol, out int colBankIFSCCode, 25);

                //SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                ////SetCellValue("Direct Manpower", sheet1, xlsRow, ref xlsCol, out cDMP, 25);
                //SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                //SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                //SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                //SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                //SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                //SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                //SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                //SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                //SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                //SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                //SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                //SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                //SetCellValue("Month, Year", sheet1, xlsRow, ref xlsCol, out int colMonthYear, 11);


                #region GWR Extra OT (Weekend WeekOFF & Holiday)
                //Dictionary<string, double> dicNW = null;
                //Dictionary<string, double> dicW = null;
                //Dictionary<string, double> dicH = null;
                //DataSet dsCurrency = null;
                //WeekOFFandHolidayOT clsWeekOFFOTReport = new WeekOFFandHolidayOT();

                //Dictionary<string, DataRow> dicHourlyOTNW = new Dictionary<string, DataRow>();
                //Dictionary<string, DataRow> dicHourlyOTW = new Dictionary<string, DataRow>();
                //Dictionary<string, DataRow> dicHourlyOTH = new Dictionary<string, DataRow>();

                //dicHourlyOTNW = GetDictionaryHourotmonthReportwithoutWeekendHoliday(year, month, plantId, companyId, companyGroupId, parameters, isActive, isSeperated);
                //dicHourlyOTW = GetDictionaryHourOTMonthReportWithWeekendORHoliday(year, month, plantId, companyId, companyGroupId, parameters, isActive, isSeperated, "Weekend");
                //dicHourlyOTH = GetDictionaryHourOTMonthReportWithWeekendORHoliday(year, month, plantId, companyId, companyGroupId, parameters, isActive, isSeperated, "Holiday");


                ////otc.LoadSalaryStructure(plantId, fdateOfMonth, ldateOfMonth, out dsSStructureOT);
                ////otc.LoadOverTimePolicy(plantId, fdateOfMonth, ldateOfMonth, out dsOTPolicy);



                //Dictionary<string, List<DataRow>> dicSalStructure = LoadSalaryStructure(plantId, fdateOfMonth, ldateOfMonth);
                //Dictionary<string, DataRow> dicOTpolicy = LoadOverTimePolicy(plantId, fdateOfMonth, ldateOfMonth);

                //clsSalaryInfo objSal = new clsSalaryInfo();
                //objSal.GetLocalCurrency(companyGroupId, plantId, out dsCurrency);
                //string _currencyId = "";
                //if (dsCurrency.Tables[0].Rows.Count > 0)
                //{
                //    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                //}
                //else
                //{
                //    throw new Exception("No currency found...");
                //}

                //clsWeekOFFOTReport.GenerateDic(dicOTpolicy, dicSalStructure, _currencyId, out dicNW, out dicW, out dicH);

                #endregion
                int colMonthYear = 0;
                int colPayDays = 0;


                xlsRow += 2;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();
                int RowIndex = 5;
                xlsRow = RowIndex;
                xlsRow = 6;
                //xlsRow--;
                dtEmployees = dtEmployees.DefaultView.ToTable(true, "EmpSystemID", "EmployeeCode", "WorkingDaysInAMonth",
                    "EmployeeName", "DOJ", "DOS", "Department");
                DataRow drAttdn = null;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {

                    try
                    {
                        subTotalDictSalaryProcess = new Dictionary<string, double>();
                        xlsCol = 1;
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)
                        {
                            sheet1.Range[xlsRow, 1].Text = "Department Name";
                            sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 1, xlsRow, 2].Merge();
                            sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 14;


                            sheet1.Range[xlsRow, 3].Text = dtEmployees.Rows[i]["Department"].ToString();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, 5].Merge();
                        }
                        xlsRow++;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                        {
                            sheet1.Range[xlsRow, 1].Text = "Employee Code";
                            sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 14;
                            sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 1, xlsRow, 2].Merge();


                            sheet1.Range[xlsRow, 3].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, 5].Merge();
                        }

                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                        {
                            sheet1.Range[xlsRow, 6].Text = "Employee Name";
                            sheet1.Range[xlsRow, 6].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();
                            sheet1.Range[xlsRow, 6].CellStyle.Font.Size = 14;


                            sheet1.Range[xlsRow, 8].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                            sheet1.Range[xlsRow, 8].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 8].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 8, xlsRow, 10].Merge();
                            sheet1.Range[xlsRow, 8].CellStyle.Font.Size = 14;
                        }
                        xlsRow++;
                        xlsRow++;

                        #region Salary Head
                        SetCellValue("Month, Year", sheet1, xlsRow, ref xlsCol, out colMonthYear, 11);
                        SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 11);


                        endGenericColumn = xlsCol - 1;

                        //SR to
                        //sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                        //sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                        //xlsCol += 1;
                        ColGrs = endGenericColumn;
                        // 9

                        var _count_earning_head = 0;
                        var _count_earning_ctchead = 0;
                        var _count_deducting_head = 0;
                        var _total_head_count = 0;

                        Dictionary<string, SalaryHeadSequence> shtList = null;

                        CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);

                        List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                        salList.AddRange(shtList.Values);

                        xlsCol--;

                        //Header Col
                        if (_count_earning_ctchead > 0)
                        {
                            sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                            sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                        }

                        var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                        if (_count_deducting_head > 0)
                        {
                            sheet1.Range[xlsRow, ds].Text = "Deduction head";
                            sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                        }
                        npstruct = 0;
                        if (shtList.Count > 0)
                        {
                            xlsCol++;
                            npstruct = ColGrs + shtList.Count + 1;
                            sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                            //sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                            //sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();
                        }
                        endXlsCol = npstruct;

                        sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                        sheet1.Range[xlsRow, 1, xlsRow + 1, npstruct].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        #endregion

                        #region EmpInfo







                        //4
                        xlsRow++;
                        xlsRow++;
                        //xlsRow++;


                        #endregion

                        double payDays = 0.00;

                        //var _total_head_count_body = 0;

                        #region ------------------------------------Salary Sheet----------------------------------
                        for (int mi = 0; mi < dtMonthYear.Rows.Count; mi++)
                        {

                            sheet1.Range[xlsRow, colMonthYear].Text = bplib.clsWebLib.GetMonthName(dtMonthYear.Rows[mi]["Month"].ToString()) + ", " + dtMonthYear.Rows[mi]["Year"].ToString();
                            sheet1.Range[xlsRow, colMonthYear].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, colMonthYear].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            string key = dtEmployees.Rows[i]["EmpSystemID"].ToString() + "-" + dtMonthYear.Rows[mi]["Year"].ToString() + "-" + dtMonthYear.Rows[mi]["Month"].ToString();
                            if (dicAttdn.ContainsKey(key))
                            {
                                drAttdn = dicAttdn[key];
                                if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                                {
                                    if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                                    {
                                        payDays = clsStaticInfo.dbl(drAttdn["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalAbsent"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalWeekOff"].ToString());

                                    }
                                    if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                                    {
                                        payDays = clsStaticInfo.dbl(drAttdn["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalAbsent"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalWeekOff"].ToString());
                                    }
                                }
                                else
                                {
                                    payDays = clsStaticInfo.dbl(drAttdn["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalAbsent"].ToString());
                                }



                                sheet1.Range[xlsRow, colPayDays].Number = payDays; //bplib.clsWebLib.GetMonthName(dtMonthYear.Rows[mi]["Month"].ToString()) + ", " + dtMonthYear.Rows[mi]["Year"].ToString();
                                sheet1.Range[xlsRow, colPayDays].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, colPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }

                            if (dicEmpSalry.ContainsKey(key))
                            {
                                List<DataRow> drSalaryHeadCollection = dicEmpSalry[key];
                                if (drSalaryHeadCollection.Count > 0)
                                {
                                    for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                    {
                                        if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                        {
                                            sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            getTotalAmount(npstruct.ToString(), clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()), ref subTotalDictSalaryProcess);

                                            continue;
                                        }
                                        try
                                        {
                                            SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                            if (xx != null)
                                            {
                                                if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                                {
                                                    sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                                    getTotalAmount(xx.XLColIndex.ToString(), clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1), ref subTotalDictSalaryProcess);
                                                }

                                                else
                                                {

                                                    sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                                    getTotalAmount(xx.XLColIndex.ToString(), clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()), ref subTotalDictSalaryProcess);

                                                }

                                                sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                                sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                                sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                            throw ex;
                                        }

                                    }
                                }
                            }

                            xlsRow++;
                        }



                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                    xlsRow++;
                    sheet1.Range[xlsRow, colMonthYear].Text = "Total";
                    foreach (var item in subTotalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                    {
                        try
                        {
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].NumberFormat = oRU.NumberFormatIntLocal("");
                            //sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
                        }
                        catch (Exception exe)
                        {
                            throw exe;
                        }
                    }


                    #endregion

                    xlsRow++;
                }//for emp count


                //xlsRow++;
                //sheet1.Range[xlsRow, colMonthYear].Text = "Total";
                //foreach (var item in subTotalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                //{
                //    try
                //    {
                //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].NumberFormat = oRU.NumberFormatIntLocal("");
                //        //sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
                //    }
                //    catch (Exception exe)
                //    {
                //        throw exe;
                //    }
                //}
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                #endregion------------------Column Header------------------

                // int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

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


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month From " + Convert.ToDateTime(fromDate).ToString("MMMM") + ", " + Convert.ToDateTime(fromDate).ToString("yyyy") + " TO " + Convert.ToDateTime(toDate).ToString("MMMM") + ", " + Convert.ToDateTime(toDate).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow, 1].Text = "Report Ref No.";




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
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;


                return workbook;

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

        public IWorkbook GetEmployeeSalaryProcessedReportSalaryYearlyWise(string companyGroupId, string companyId, string plantId, string userId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool withGoodWork, string FromYear, string FromMonth, string ToYear, string ToMonth)
        {
            #region Variable
            clsReport objRpt = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

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
                Dictionary<string, double> subTotalDictSalaryProcess = new Dictionary<string, double>();
                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet
                string fromDate = "";
                string toDate = "";
                DataTable dtMonthYear = null;

                fromtoDateTaxYear(FromYear, FromMonth, ToYear, ToMonth, out fromDate, out toDate);

                if (Convert.ToDateTime(fromDate) > Convert.ToDateTime(toDate))
                {
                    throw new Exception("From month cannot be greater than To month");
                }

                if (((Convert.ToDateTime(toDate).Year - Convert.ToDateTime(fromDate).Year) * 12) + Convert.ToDateTime(toDate).Month - Convert.ToDateTime(fromDate).Month > 12)
                    throw new Exception("Cannot be greater than 1 year");

                dtMonthYear = GetMonthYearDataTable(fromDate, toDate);
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();
                Dictionary<string, DataRow> dicAttdn = new Dictionary<string, DataRow>();
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();

                GetYearlyEmployeeInfoDetailSalaryLogWise(companyGroupId, companyId, plantId, fromDate, toDate, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo, out dicAttdn);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetYearlyEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fromDate, toDate, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------

                xlsCol = 1;

                #region Column Variables
                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0
                   , ColLWP = 0, colBank = 0, cDMP = 0, colBankAccountNo = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;

                #endregion

                //1
                //SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                //SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                //SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                //SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                //SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                //SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                //SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                //SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                //SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                //SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                //SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                //SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                //SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                //SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                //SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                //SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                //SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                ////SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                //SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                //SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                //SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                //SetCellValue("IFSCCode", sheet1, xlsRow, ref xlsCol, out int colBankIFSCCode, 25);

                //SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                ////SetCellValue("Direct Manpower", sheet1, xlsRow, ref xlsCol, out cDMP, 25);
                //SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                //SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                //SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                //SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                //SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                //SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                //SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                //SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                //SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                //SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                //SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                //SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                //SetCellValue("Month, Year", sheet1, xlsRow, ref xlsCol, out int colMonthYear, 11);


                #region GWR Extra OT (Weekend WeekOFF & Holiday)
                //Dictionary<string, double> dicNW = null;
                //Dictionary<string, double> dicW = null;
                //Dictionary<string, double> dicH = null;
                //DataSet dsCurrency = null;
                //WeekOFFandHolidayOT clsWeekOFFOTReport = new WeekOFFandHolidayOT();

                //Dictionary<string, DataRow> dicHourlyOTNW = new Dictionary<string, DataRow>();
                //Dictionary<string, DataRow> dicHourlyOTW = new Dictionary<string, DataRow>();
                //Dictionary<string, DataRow> dicHourlyOTH = new Dictionary<string, DataRow>();

                //dicHourlyOTNW = GetDictionaryHourotmonthReportwithoutWeekendHoliday(year, month, plantId, companyId, companyGroupId, parameters, isActive, isSeperated);
                //dicHourlyOTW = GetDictionaryHourOTMonthReportWithWeekendORHoliday(year, month, plantId, companyId, companyGroupId, parameters, isActive, isSeperated, "Weekend");
                //dicHourlyOTH = GetDictionaryHourOTMonthReportWithWeekendORHoliday(year, month, plantId, companyId, companyGroupId, parameters, isActive, isSeperated, "Holiday");


                ////otc.LoadSalaryStructure(plantId, fdateOfMonth, ldateOfMonth, out dsSStructureOT);
                ////otc.LoadOverTimePolicy(plantId, fdateOfMonth, ldateOfMonth, out dsOTPolicy);



                //Dictionary<string, List<DataRow>> dicSalStructure = LoadSalaryStructure(plantId, fdateOfMonth, ldateOfMonth);
                //Dictionary<string, DataRow> dicOTpolicy = LoadOverTimePolicy(plantId, fdateOfMonth, ldateOfMonth);

                //clsSalaryInfo objSal = new clsSalaryInfo();
                //objSal.GetLocalCurrency(companyGroupId, plantId, out dsCurrency);
                //string _currencyId = "";
                //if (dsCurrency.Tables[0].Rows.Count > 0)
                //{
                //    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                //}
                //else
                //{
                //    throw new Exception("No currency found...");
                //}

                //clsWeekOFFOTReport.GenerateDic(dicOTpolicy, dicSalStructure, _currencyId, out dicNW, out dicW, out dicH);

                #endregion
                int colMonthYear = 0;
                int colPayDays = 0;


                xlsRow += 2;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();
                int RowIndex = 5;
                xlsRow = RowIndex;
                xlsRow = 6;
                //xlsRow--;
                dtEmployees = dtEmployees.DefaultView.ToTable(true, "EmpSystemID", "EmployeeCode", "WorkingDaysInAMonth",
                    "EmployeeName", "DOJ", "DOS", "Department");
                DataRow drAttdn = null;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {

                    try
                    {
                        subTotalDictSalaryProcess = new Dictionary<string, double>();
                        xlsCol = 1;
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)
                        {
                            sheet1.Range[xlsRow, 1].Text = "Department Name";
                            sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 1, xlsRow, 2].Merge();
                            sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 14;


                            sheet1.Range[xlsRow, 3].Text = dtEmployees.Rows[i]["Department"].ToString();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, 5].Merge();
                        }
                        xlsRow++;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                        {
                            sheet1.Range[xlsRow, 1].Text = "Employee Code";
                            sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 14;
                            sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 1, xlsRow, 2].Merge();


                            sheet1.Range[xlsRow, 3].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, 5].Merge();
                        }

                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                        {
                            sheet1.Range[xlsRow, 6].Text = "Employee Name";
                            sheet1.Range[xlsRow, 6].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();
                            sheet1.Range[xlsRow, 6].CellStyle.Font.Size = 14;


                            sheet1.Range[xlsRow, 8].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                            sheet1.Range[xlsRow, 8].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 8].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 8, xlsRow, 10].Merge();
                            sheet1.Range[xlsRow, 8].CellStyle.Font.Size = 14;
                        }
                        xlsRow++;
                        xlsRow++;

                        #region Salary Head
                        SetCellValue("Month, Year", sheet1, xlsRow, ref xlsCol, out colMonthYear, 11);
                        SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 11);


                        endGenericColumn = xlsCol - 1;

                        //SR to
                        //sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                        //sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                        //xlsCol += 1;
                        ColGrs = endGenericColumn;
                        // 9

                        var _count_earning_head = 0;
                        var _count_earning_ctchead = 0;
                        var _count_deducting_head = 0;
                        var _total_head_count = 0;

                        Dictionary<string, SalaryHeadSequence> shtList = null;

                        CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);

                        List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                        salList.AddRange(shtList.Values);

                        xlsCol--;

                        //Header Col
                        if (_count_earning_ctchead > 0)
                        {
                            sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                            sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                        }

                        var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                        if (_count_deducting_head > 0)
                        {
                            sheet1.Range[xlsRow, ds].Text = "Deduction head";
                            sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                        }
                        npstruct = 0;
                        if (shtList.Count > 0)
                        {
                            xlsCol++;
                            npstruct = ColGrs + shtList.Count + 1;
                            sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                            //sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                            //sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();
                        }
                        endXlsCol = npstruct;

                        sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                        sheet1.Range[xlsRow, 1, xlsRow + 1, npstruct].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        #endregion

                        #region EmpInfo







                        //4
                        xlsRow++;
                        xlsRow++;
                        //xlsRow++;


                        #endregion

                        double payDays = 0.00;

                        //var _total_head_count_body = 0;

                        #region ------------------------------------Salary Sheet----------------------------------
                        for (int mi = 0; mi < dtMonthYear.Rows.Count; mi++)
                        {

                            sheet1.Range[xlsRow, colMonthYear].Text = bplib.clsWebLib.GetMonthName(dtMonthYear.Rows[mi]["Month"].ToString()) + ", " + dtMonthYear.Rows[mi]["Year"].ToString();
                            sheet1.Range[xlsRow, colMonthYear].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, colMonthYear].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            string key = dtEmployees.Rows[i]["EmpSystemID"].ToString() + "-" + dtMonthYear.Rows[mi]["Year"].ToString() + "-" + dtMonthYear.Rows[mi]["Month"].ToString();
                            if (dicAttdn.ContainsKey(key))
                            {
                                drAttdn = dicAttdn[key];
                                if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                                {
                                    if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                                    {
                                        payDays = clsStaticInfo.dbl(drAttdn["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalAbsent"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalWeekOff"].ToString());

                                    }
                                    if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                                    {
                                        payDays = clsStaticInfo.dbl(drAttdn["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalAbsent"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalWeekOff"].ToString());
                                    }
                                }
                                else
                                {
                                    payDays = clsStaticInfo.dbl(drAttdn["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdn["TotalAbsent"].ToString());
                                }



                                sheet1.Range[xlsRow, colPayDays].Number = payDays; //bplib.clsWebLib.GetMonthName(dtMonthYear.Rows[mi]["Month"].ToString()) + ", " + dtMonthYear.Rows[mi]["Year"].ToString();
                                sheet1.Range[xlsRow, colPayDays].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, colPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }

                            if (dicEmpSalry.ContainsKey(key))
                            {
                                List<DataRow> drSalaryHeadCollection = dicEmpSalry[key];
                                if (drSalaryHeadCollection.Count > 0)
                                {
                                    for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                    {
                                        if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                        {
                                            sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            getTotalAmount(npstruct.ToString(), clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()), ref subTotalDictSalaryProcess);

                                            continue;
                                        }
                                        try
                                        {
                                            SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                            if (xx != null)
                                            {
                                                if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                                {
                                                    sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                                    getTotalAmount(xx.XLColIndex.ToString(), clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1), ref subTotalDictSalaryProcess);
                                                }

                                                else
                                                {

                                                    sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                                    getTotalAmount(xx.XLColIndex.ToString(), clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()), ref subTotalDictSalaryProcess);

                                                }

                                                sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                                sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                                sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                            throw ex;
                                        }

                                    }
                                }
                            }

                            xlsRow++;
                        }



                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                    xlsRow++;
                    sheet1.Range[xlsRow, colMonthYear].Text = "Total";
                    foreach (var item in subTotalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                    {
                        try
                        {
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].NumberFormat = oRU.NumberFormatIntLocal("");
                            //sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
                        }
                        catch (Exception exe)
                        {
                            throw exe;
                        }
                    }


                    #endregion

                    xlsRow++;
                }//for emp count


                //xlsRow++;
                //sheet1.Range[xlsRow, colMonthYear].Text = "Total";
                //foreach (var item in subTotalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                //{
                //    try
                //    {
                //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].NumberFormat = oRU.NumberFormatIntLocal("");
                //        //sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
                //    }
                //    catch (Exception exe)
                //    {
                //        throw exe;
                //    }
                //}
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                #endregion------------------Column Header------------------

                // int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

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


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month From " + Convert.ToDateTime(fromDate).ToString("MMMM") + ", " + Convert.ToDateTime(fromDate).ToString("yyyy") + " TO " + Convert.ToDateTime(toDate).ToString("MMMM") + ", " + Convert.ToDateTime(toDate).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow, 1].Text = "Report Ref No.";




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
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;


                return workbook;

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

        public void GetYearlyEmployeeInfoDetailSalaryLogWise(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef, out Dictionary<string, DataRow> dicAttdn)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            string salaryProcessId = "";
            var _wc = string.Empty;


            dicAttdn = new Dictionary<string, DataRow>();





            var wcPayrollGroup = "";

            var salaryProcessColumn = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = " Where (1=0 ";
            //string salaryProcessID = "";
            try
            {




                var strDOJ = @"AND DOJ<='" + toDate + @"' AND (DOS is null OR DOS>= '" + fromDate + "')";
                //if (sa == true || ca == true)
                //{
                //    wcPayrollGroup = @"";
                //}
                //else
                //{
                //    string inPayrollGroup = "";
                //    DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid FROM MST.PayrollGroupMaster WHERE PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");
                //    DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable(@"SELECT SystemId FROM EmployeeInformation E 
                //        WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

                //    if (dtPayRollGrpEmpId.Rows.Count > 0)
                //    {
                //        for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
                //        {
                //            inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
                //        }
                //        if (dtNotPayRollGrpEmpId.Rows.Count > 0)
                //        {
                //            for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
                //            {
                //                inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
                //            }
                //        }
                //        wcPayrollGroup = @"AND E.SystemId  IN (" + inPayrollGroup + @")";
                //    }
                //    else
                //    {
                //        wcPayrollGroup = @"";
                //    }

                //    //wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
                //}


                string strSqlSal = @"SELECT  m.SystemID FROM SalaryProcMaster m
                                    INNER JOIN SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID and c.PlantID='" + plantId + @"'
                                        WHERE 1=1 
                                        " + getMonthYear(fromDate, toDate, "MonthNo", "YearNo") + @"";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
                //string salaryProcessId = "''";
                dtSalPrcId = dtSalPrcId.DefaultView.ToTable(true, "SystemID");
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }

                salaryProcessFlag = ", Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag";
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
                //}





                wcEmpStatus += ")";

                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
                                    ,ISNULL(EmpC.WorkingDaysInAMonth,'') WorkingDaysInAMonth
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    , Case when isnull(DOS,'') <> '' then   'Separated' else 'Regular' end SalaryProcFlag
                                    
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(E.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                     FROM EmployeeInformation e
                                
								    LEFT JOIN mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
								LEFT JOIN mst.DesignationMaster dm on dm.id=m.DesignationMasterId
								LEFT JOIN hkp.EmployeeCategory EmpC on EmpC.Id = dm.EmployeeCategoryId
								LEFT JOIN hkp.LegalDesignation ld on ld.Id = e.LegalDesignationId
                                    --LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=SPLD.LegalDesignationId
                                   
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=E.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId                                   
			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    
								    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = E.BankSystemID
									left join [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
   
                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"                                
                                     ) DD " + wcEmpStatus + @" ";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            cmdText += @"and EmpSystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }


                catch (Exception)
                {

                }

                cmdText += @" ORDER BY ISNULL(EmployeeCodePreFix,''),ISNULL(EmployeeCodeNumeric,0)";




                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(cmdText, out dsRef);
                string strAttdn = @"SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
                                        , ISNULL(TotalLv, 0) TotalLv
										,ISNULL(TotalMLv, 0) TotalMLv,ISNULL(TotalCompAssignLv, 0) TotalCompAssignLv,ISNULL(TotalWeekOff, 0) + ISNULL(TotalWeekOffHoliDay, 0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay, 0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr, 0) TotalOTHr,ISNULL(TotalNormalOTHr, 0) TotalNormalOTHr,ISNULL(TotalExtraOTHr, 0) TotalExtraOTHr,ISNULL(WeekOffOTHr, 0) WeekOffOTHr
										,ISNULL(HoliDayOTHr, 0) HoliDayOTHr,ISNULL(TotalLWP, 0) TotalLWP,ISNULL(IsOTEntitled, 0) IsOTEntitled,ISNULL(OTRate, 0) OTRate,ISNULL(TotalHoliDay, 0) TotalHoliDay
                                               FROM SalaryProceAttdnData MMDSA 
                                            Left join EmployeeInformation E ON MMDSA.EmpSystemID = E.SystemId
                                            
                                        WHERE E.PlantID = '" + plantId + @"' " + getMonthYear(fromDate, toDate, "MonthNo", "YearNo") + @"";


                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strAttdn += @"and E.SystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }


                catch (Exception)
                {
                }
                ConnectionManager.clsConnectionManager con2 = new ConnectionManager.clsConnectionManager(600);
                con2.getDataSet(strAttdn, out DataSet dsRef2);

                DataTable dt = dsRef2.Tables[0];
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    empId = dt.Rows[i]["EmpSystemID"].ToString() + "-" + dt.Rows[i]["YearNo"].ToString() + "-" + dt.Rows[i]["MonthNo"].ToString();

                    dicAttdn.Add(empId, dt.Rows[i]);
                }
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

        public Dictionary<string, List<DataRow>> GetYearlyEmployeeSalaryInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, Dictionary<string, string> parameters, out DataTable distinctSalaryHead)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            distinctSalaryHead = new DataTable("Tmp");
            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild where PlantID='" + plantId + @"'
                                                         )
                                        " + getMonthYear(fromDate, toDate, "MonthNo", "YearNo") + @"";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }


            try
            {
                strSQL = @"SELECT EmpSlr.*,PSH.Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
                                                    SPC.EmpInfoSystemID EmpSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
                                                    , sh.SalaryHead, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay

                                     FROM SalaryProcChild SPC

                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID



                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID= spc.SalaryHeadID


                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id

                                                        LEFT JOIN (
                                                                   SELECT* FROM ExchangerateDateWiseForHR

                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster


                                                                                                            WHERE SystemID IN(" + salaryProcessID + @")
																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode

                                                                                            AND SPC.PlantID = Exr.PlantID

                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

                                                        where isnull(SPC.SlrProcMstSystemID,'')  IN(" + salaryProcessID + @")) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpSystemID

                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EEI.PlantId = '" + plantId + @"'";

                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"AND EmpSlr.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";

                        }
                    }
                }
                catch (Exception)
                {
                }
                strSQL += "ORDER BY EEI.EmployeeCode ,YearNo,MonthNo ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);

                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "PartOfNetPay", "IsCTCComponent", "IsGrossComponent");
                distinctSalaryHead.DefaultView.Sort = "Sequence";
                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        if (empId != dt.Rows[i]["EmpSystemID"].ToString() + "-" + dt.Rows[i]["YearNo"].ToString() + "-" + dt.Rows[i]["MonthNo"].ToString())
                        {
                            string x = dt.Rows[i]["EmpSystemID"].ToString() + "-" + dt.Rows[i]["YearNo"].ToString() + "-" + dt.Rows[i]["MonthNo"].ToString();
                            _data = new List<DataRow>();
                            dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString() + "-" + dt.Rows[i]["YearNo"].ToString() + "-" + dt.Rows[i]["MonthNo"].ToString(), _data);
                        }
                        _data.Add(dt.Rows[i]);
                        empId = dt.Rows[i]["EmpSystemID"].ToString() + "-" + dt.Rows[i]["YearNo"].ToString() + "-" + dt.Rows[i]["MonthNo"].ToString();
                    }
                    catch (Exception)
                    {

                    }
                }



                return dicBonus;


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function

        private void getTotalAmount(string colIndex, double Amount, ref Dictionary<string, double> dict)
        {
            try
            {
                if (dict.ContainsKey(colIndex))//If has Same head
                {
                    var value = dict[colIndex];
                    double totalAmount = Convert.ToDouble(Amount) + Convert.ToDouble(value);
                    dict[colIndex] = totalAmount;

                }
                else // If New Head
                {
                    dict.Add(colIndex, Amount);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        #region Bonus Register Report       
        public IWorkbook GetBonusReportC(string companyGroupId, string companyId, string plantId, string userName, string yearId, bool withBonusValue, string FromDate, string ToDate)
        {

            #region Variable

            clsReport objRpt = null;
            int slCount = 0;
            DataTable dtEmpInfo = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsMonth = null;
            DataSet dsEmpAttdn = null;
            DataTable dtEmpAttdn = null;
            List<DataRow> BonusList = null;
            //DataSet dsEmpBonus = null;
            //DataTable dtEmpBonus = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable

            try
            {
                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                int rowTobeAdded = 1;

                if (withBonusValue == true)
                {
                    rowTobeAdded = 2;
                }

                ru = new ReportUtility();

                objRpt = new clsReport(_sqlRepository);

                #region Variable
                ParamList para = new ParamList();
                ParamList leavePara = new ParamList();
                ParamList attdnProcessParam = new ParamList();

                var FactoryName = "";
                var CmpName = "";

                para.PlantId = plantId;
                //string FromDate = "";
                //string ToDate = "";

                #endregion Variable

                #region DataSet
                DataTable dtTaxYear = null;
                dtTaxYear = _sqlRepository.GetDataTable("SELECT * FROM SCS.TaxYear WHERE TaxYearName = '" + yearId + @"'");

                int fromYear = Convert.ToDateTime(dtTaxYear.Rows[0]["StartDate"]).Year;//EndDate
                int toYear = Convert.ToDateTime(dtTaxYear.Rows[0]["EndDate"]).Year;


                int DaysInMonth = DateTime.DaysInMonth(Convert.ToInt16(Convert.ToDateTime(ToDate).ToString("yyyy")), Convert.ToInt16(Convert.ToDateTime(ToDate).ToString("MM")));

                ToDate = DaysInMonth + "-" + Convert.ToDateTime(ToDate).ToString("MMM") + "-" + Convert.ToDateTime(ToDate).ToString("yyyy");

                objRpt.GetFiscalMonthListSql(FromDate, ToDate, out dsMonth);
                objRpt.GetMonthWiseEmpMonthlyAttdnInfo(FromDate, ToDate, dsMonth.Tables[0], out dsEmpAttdn);
                dtEmpAttdn = dsEmpAttdn.Tables[0];
                Dictionary<string, List<DataRow>> dicBonus = GetSummarisedEmpBonusInfo(FromDate, ToDate, companyGroupId, companyId, plantId, dsMonth.Tables[0]);
                Dictionary<string, List<DataRow>> dicBonusH = GetSummarisedEmpBonusInfoH(FromDate, ToDate, companyGroupId, companyId, plantId, dsMonth.Tables[0]);

                DataTable dtMonthInfo = dsMonth.Tables[0];

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                var colSr = 0;
                var colEmpCode = 0;
                var colEmpName = 0;
                var colTotalAmount = 0;
                var colBonusPercentage = 0;
                var colBonusAmount = 0;
                var colDOS = 0;


                #endregion------------------Column Header------------------


                var oRU = new ReportUtility();



                List<FiscalYearMonthSequence> list = null;


                SetHeaderValue("EmpCode", sheet1, xlsRow, ref xlsCol, out colEmpCode, 9);
                SetHeaderValue("Department", sheet1, xlsRow, ref xlsCol, out int colDepartment, 25);
                SetHeaderValue("Name", sheet1, xlsRow, ref xlsCol, out colEmpName, 25);
                SetHeaderValue("Whether Completed 15 Years", sheet1, xlsRow, ref xlsCol, out int col15Year, 10);
                SetHeaderValue("Designation", sheet1, xlsRow, ref xlsCol, out int colDesignation, 12);
                SetHeaderValue("Work in years No. of Days", sheet1, xlsRow, ref xlsCol, out int colWorkInYears, 10);
                SetHeaderValue("Total Salary A/C Year", sheet1, xlsRow, ref xlsCol, out int colTotalYearlyBasicSalary, 10);
                SetHeaderValue("BonusRate", sheet1, xlsRow, ref xlsCol, out colBonusPercentage, 10);
                SetHeaderValue("Amount of Bonus PMT Under Sec 10 & 11", sheet1, xlsRow, ref xlsCol, out colBonusAmount, 10);
                SetHeaderValue("Amount Actual Payment", sheet1, xlsRow, ref xlsCol, out colTotalAmount, 10);
                SetHeaderValue("Date of Payment", sheet1, xlsRow, ref xlsCol, out int colDateOfPayment, 10);
                SetHeaderValue("Signature", sheet1, xlsRow, ref xlsCol, out int colSignature, 23);

                endXlsCol = colDateOfPayment;
                var fPanRow = xlsRow + 1;

                #region ******************Report Header******************
                if (dicBonus.Count == 0 && dicBonusH.Count == 0)
                {
                    throw new Exception("No data found");
                }
                if (dicBonus.Count == 0 && dicBonusH.Count != 0)
                {
                    DataView view = new DataView(dicBonusH.Values.ElementAt(0)[0].Table);
                    dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "DepartmentName", "DesignationName", "BankName"
                                                           , "DOJ", "BankShortName", "BankAccNo", "DOS", "PaymentMode", "WorkingDaysInAMonth", "TotalProcDate", "TotalAbsent", "TotalWeekOff", "TotalHoliDay");

                }
                else
                {
                    DataView view = new DataView(dicBonus.Values.ElementAt(0)[0].Table);
                    dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "DepartmentName", "DesignationName", "BankName"
                                                           , "DOJ", "BankShortName", "BankAccNo", "DOS", "PaymentMode", "WorkingDaysInAMonth", "TotalProcDate", "TotalAbsent", "TotalWeekOff", "TotalHoliDay");

                }

                double totalSalaryAmount = 0.00;
                double earningBonusAmount = 0.00;
                double totalEarningBonusAmountYearly = 0.00;
                bool isDecimal = false;
                double decimalNo = 0;
                xlsRow++;
                int startxlsRow = xlsRow;
                for (int dti = 0; dti < dtEmpInfo.Rows.Count; dti++)
                {
                    totalSalaryAmount = 0.00;
                    earningBonusAmount = 0.00;
                    totalEarningBonusAmountYearly = 0.00;
                    if (dicBonus.Count == 0)
                    {

                    }
                    else
                    {
                        BonusList = dicBonus[dtEmpInfo.Rows[dti]["EmpSystemId"].ToString()];
                    }
                    //if (BonusList.Count == 1)
                    //{
                    //    continue;
                    //}
                    sheet1.Range[xlsRow, colEmpCode].RowHeight = 43;

                    var timeSpan = DateTime.Now - Convert.ToDateTime(dtEmpInfo.Rows[dti]["DOJ"].ToString()); ;
                    int yearSpan = new DateTime(timeSpan.Ticks).Year - 1;

                    //if (dtEmpInfo.Rows[dti]["EmployeeCode"].ToString() == "10005866")
                    //{

                    //}
                    sheet1.Range[xlsRow, colEmpCode].Text = dtEmpInfo.Rows[dti]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, colEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet1.Range[xlsRow, colEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                    sheet1.Range[xlsRow, colEmpName].Text = dtEmpInfo.Rows[dti]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, colDepartment].Text = dtEmpInfo.Rows[dti]["DepartmentName"].ToString();
                    sheet1.Range[xlsRow, colDesignation].Text = dtEmpInfo.Rows[dti]["DesignationName"].ToString();

                    sheet1.Range[xlsRow, colBonusPercentage].Text = "8.33";
                    var payDays = 0.00;
                    // clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalAbsent"]);
                    if (!String.IsNullOrEmpty(dtEmpInfo.Rows[dti]["WorkingDaysInAMonth"].ToString().ToUpper()))
                    {
                        if (dtEmpInfo.Rows[dti]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                        {
                            //payDays = ru.cnDgt((Convert.ToDouble(dtEmpInfo.Rows[dti]["TotalProcDate"]) - Convert.ToDouble(dtEmpInfo.Rows[dti]["TotalWeekOff"]) - Convert.ToDouble(dtEmpInfo.Rows[dti]["TotalHoliDay"]) - Convert.ToDouble(dtEmpInfo.Rows[dti]["TotalAbsent"]) + ExtraAbsentHoliday + ExtraAbsentWeekOFF).ToString(), localLanguage);

                            payDays = (Convert.ToDouble(dtEmpInfo.Rows[dti]["TotalProcDate"]) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalWeekOff"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalAbsent"].ToString()));
                        }
                        if (dtEmpInfo.Rows[dti]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                        {
                            payDays = (clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalWeekOff"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalAbsent"].ToString()));

                            //payDays = ru.cnDgt((Convert.ToDouble(dtEmpInfo.Rows[dti]["TotalProcDate"]) - Convert.ToDouble(dtEmpInfo.Rows[dti]["TotalWeekOff"]) - Convert.ToDouble(dtEmpInfo.Rows[dti]["TotalAbsent"]) + ExtraAbsentWeekOFF).ToString(), localLanguage);
                        }
                    }
                    else
                    {
                        payDays = (clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalAbsent"].ToString()));
                    }
                    sheet1.Range[xlsRow, colWorkInYears].Number = payDays;

                    sheet1.Range[xlsRow, col15Year].Text = yearSpan >= 15 ? "Yes" : "No";
                    //sheet1.Range[xlsRow + 1, colEmpName].RowHeight = 19;
                    endXlsCol = colSignature;
                    sheet1.Range[xlsRow, colEmpCode, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    try
                    {
                        earningBonusAmount = 0.00;
                        if (BonusList != null)
                        {
                            for (int BNS = 0; BNS < BonusList.Count; BNS++)
                            {
                                totalSalaryAmount += Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                                isDecimal = bplib.clsWebLib.GetBoolData(BonusList[BNS]["IntegerInDisb"].ToString());
                                decimalNo = clsStaticInfo.dbl(BonusList[BNS]["DecimalNo"].ToString());

                            }
                        }

                        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].Number = Service.Extension.clsStaticInfo.dbl(totalSalaryAmount);
                        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //if (dicBonusH.ContainsKey(dtEmpInfo.Rows[dti]["EmpSystemId"].ToString())==true)
                        //{

                        //}
                        if (dicBonusH.ContainsKey(dtEmpInfo.Rows[dti]["EmpSystemId"].ToString()) == true)
                        {
                            List<DataRow> BonusListH = dicBonusH[dtEmpInfo.Rows[dti]["EmpSystemId"].ToString()];
                            for (int i = 0; i < BonusListH.Count; i++)
                            {
                                if (BonusListH[i]["HeadCategory"].ToString().ToUpper() == "OTHER BONUS")
                                {
                                    earningBonusAmount = clsStaticInfo.dbl(BonusListH[i]["DisbusmentAmount"].ToString());
                                    totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                }

                                if (BonusListH[i]["HeadCategory"].ToString().ToUpper() == "RetainedBonus".ToUpper())
                                {
                                    earningBonusAmount += clsStaticInfo.dbl(BonusListH[i]["DisbusmentAmount"].ToString());
                                    totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                }
                                if (BonusListH[i]["HeadCategory"].ToString().ToUpper() == "Monthly Bonus Retain".ToUpper())
                                {
                                    earningBonusAmount += clsStaticInfo.dbl(BonusListH[i]["DisbusmentAmount"].ToString());
                                    totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                }
                            }
                        }

                        if (totalEarningBonusAmountYearly == 0)
                        {
                            sheet1.Range[xlsRow, colBonusAmount].Text = "-";// + Environment.NewLine + totalPayDay;                              
                            sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        }
                        else
                        {
                            sheet1.Range[xlsRow, colBonusAmount].Number = Convert.ToDouble(totalEarningBonusAmountYearly);// + Environment.NewLine + totalPayDay;
                            sheet1.Range[xlsRow, colBonusAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                            sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                            sheet1.Range[xlsRow, colTotalAmount].Number = Convert.ToDouble(totalEarningBonusAmountYearly);// + Environment.NewLine + totalPayDay;
                            sheet1.Range[xlsRow, colTotalAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                            sheet1.Range[xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    xlsRow++;
                }
                sheet1.Range[startxlsRow, 1, xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[startxlsRow, colWorkInYears, xlsRow, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].Text = "Total:";// + Environment.NewLine + totalPayDay;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet1.Range[xlsRow, colTotalAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colTotalAmount) + startxlsRow + ":" + clsStaticInfo.GetxlsCol(colTotalAmount) + (xlsRow - 1) + ")";// + Environment.NewLine + totalPayDay;
                sheet1.Range[xlsRow, colTotalAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                sheet1.Range[xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                sheet1.Range[xlsRow, colBonusAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBonusAmount) + startxlsRow + ":" + clsStaticInfo.GetxlsCol(colBonusAmount) + (xlsRow - 1) + ")";// + Environment.NewLine + totalPayDay;
                sheet1.Range[xlsRow, colBonusAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, colTotalYearlyBasicSalary].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colTotalYearlyBasicSalary) + startxlsRow + ":" + clsStaticInfo.GetxlsCol(colTotalYearlyBasicSalary) + (xlsRow - 1) + ")";// + Environment.NewLine + totalPayDay;
                sheet1.Range[xlsRow, colTotalYearlyBasicSalary].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                sheet1.Range[xlsRow, colTotalYearlyBasicSalary].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, colTotalYearlyBasicSalary].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                xlsRow += 9;

                ru.SetTextMiddle(ref sheet1, xlsRow, 1, "Prepared By", true, 25, 15);
                sheet1[ru.GetColumnNameForXls(1) + xlsRow + ":" + ru.GetColumnNameForXls(2) + xlsRow].Merge();

                ru.SetTextMiddle(ref sheet1, xlsRow, 4, "Manager(Payroll)", true, 25, 15);
                sheet1[ru.GetColumnNameForXls(4) + xlsRow + ":" + ru.GetColumnNameForXls(6) + xlsRow].Merge();

                ru.SetTextMiddle(ref sheet1, xlsRow, 7, "VP(HR)", true, 25, 15);
                sheet1[ru.GetColumnNameForXls(7) + xlsRow + ":" + ru.GetColumnNameForXls(9) + xlsRow].Merge();

                ru.SetTextMiddle(ref sheet1, xlsRow, 9, "DGM (Account)", true, 25, 15);
                sheet1[ru.GetColumnNameForXls(9) + xlsRow + ":" + ru.GetColumnNameForXls(endXlsCol) + xlsRow].Merge();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
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


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Bonus Paid For Employee As of " + Convert.ToDateTime(FromDate).ToString("MMMM") + " : " + Convert.ToDateTime(FromDate).Year.ToString() + " TO " + Convert.ToDateTime(ToDate).ToString("MMMM") + "," + Convert.ToDateTime(ToDate).Year.ToString();
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow, 1].Text = "Bonus Format Form-C";
                sheet1.Range[xlsRow, 1, xlsRow, 2].Merge();

                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet1.UsedRange["A" + fPanRow].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;

                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$A$5:$IV$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;

                sheet1.Name = "BonusFormC";
                #endregion            
                return workbook;
            }
            catch (Exception ex)
            {
                //return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw new Exception(ex.Message);
            }
        }

        public IWorkbook GetBonusReportProvisional(string companyGroupId, string companyId, string plantId, string userName, string yearId, bool withBonusValue, string FromDate, string ToDate)
        {

            #region Variable

            clsReport objRpt = null;
            int slCount = 0;

            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsMonth = null;
            DataSet dsEmpAttdn = null;
            DataTable dtEmpAttdn = null;

            //DataSet dsEmpBonus = null;
            //DataTable dtEmpBonus = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable

            try
            {
                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                int rowTobeAdded = 1;

                if (withBonusValue == true)
                {
                    rowTobeAdded = 2;
                }

                ru = new ReportUtility();

                objRpt = new clsReport(_sqlRepository);

                #region Variable
                ParamList para = new ParamList();
                ParamList leavePara = new ParamList();
                ParamList attdnProcessParam = new ParamList();
                DataTable dtEmpInfo = null;
                DataView view = null;
                var FactoryName = "";
                var CmpName = "";
                List<DataRow> BonusList = null;
                para.PlantId = plantId;
                //string FromDate = "";
                //string ToDate = "";

                #endregion Variable

                #region DataSet
                DataTable dtTaxYear = null;
                dtTaxYear = _sqlRepository.GetDataTable("SELECT * FROM SCS.TaxYear WHERE TaxYearName = '" + yearId + @"'");

                int fromYear = Convert.ToDateTime(dtTaxYear.Rows[0]["StartDate"]).Year;//EndDate
                int toYear = Convert.ToDateTime(dtTaxYear.Rows[0]["EndDate"]).Year;


                int DaysInMonth = DateTime.DaysInMonth(Convert.ToInt16(Convert.ToDateTime(ToDate).ToString("yyyy")), Convert.ToInt16(Convert.ToDateTime(ToDate).ToString("MM")));

                ToDate = DaysInMonth + "-" + Convert.ToDateTime(ToDate).ToString("MMM") + "-" + Convert.ToDateTime(ToDate).ToString("yyyy");

                objRpt.GetFiscalMonthListSql(FromDate, ToDate, out dsMonth);
                objRpt.GetMonthWiseEmpMonthlyAttdnInfo(FromDate, ToDate, dsMonth.Tables[0], out dsEmpAttdn);
                dtEmpAttdn = dsEmpAttdn.Tables[0];

                Dictionary<string, List<DataRow>> dicBonus = GetSummarisedEmpBonusInfo(FromDate, ToDate, companyGroupId, companyId, plantId, dsMonth.Tables[0]);
                Dictionary<string, List<DataRow>> dicBonusH = GetSummarisedEmpBonusInfoH(FromDate, ToDate, companyGroupId, companyId, plantId, dsMonth.Tables[0]);

                DataTable dtMonthInfo = dsMonth.Tables[0];

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                var colSr = 0;
                var colEmpCode = 0;
                var colEmpName = 0;
                var colTotalAmount = 0;
                var colBonusPercentage = 0;
                var colBonusAmount = 0;
                var colDOS = 0;


                #endregion------------------Column Header------------------


                var oRU = new ReportUtility();



                List<FiscalYearMonthSequence> list = null;


                SetHeaderValue("EmpCode", sheet1, xlsRow, ref xlsCol, out colEmpCode, 9);
                SetHeaderValue("Name", sheet1, xlsRow, ref xlsCol, out colEmpName, 25);
                SetHeaderValue("Work Days", sheet1, xlsRow, ref xlsCol, out int colWorkInYears, 10);
                SetHeaderValue("Actual Salary", sheet1, xlsRow, ref xlsCol, out int colTotalYearlyActualBasicSalary, 15);
                SetHeaderValue("Total Salary", sheet1, xlsRow, ref xlsCol, out int colTotalYearlyBasicSalary, 15);
                SetHeaderValue("Amount of Bonus", sheet1, xlsRow, ref xlsCol, out colBonusAmount, 15);


                endXlsCol = colBonusAmount;
                var fPanRow = xlsRow + 1;

                #region ******************Report Header******************
                //DataView view = new DataView(dicBonus.Values.ElementAt(0)[0].Table);
                //DataTable dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "DepartmentName", "DesignationName", "BankName", "DOJ", "BankShortName", "BankAccNo", "DOS", "PaymentMode");

                if (dicBonus.Count == 0 && dicBonusH.Count == 0)
                {
                    throw new Exception("No data found");
                }
                if (dicBonus.Count == 0 && dicBonusH.Count != 0)
                {
                    view = new DataView(dicBonusH.Values.ElementAt(0)[0].Table);
                    dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "DepartmentName", "DesignationName", "BankName"
                                                           , "DOJ", "BankShortName", "BankAccNo", "DOS", "PaymentMode", "WorkingDaysInAMonth", "TotalProcDate", "TotalAbsent", "TotalWeekOff", "TotalHoliDay");

                }
                else
                {
                    view = new DataView(dicBonus.Values.ElementAt(0)[0].Table);
                    dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "DepartmentName", "DesignationName", "BankName"
                                                           , "DOJ", "BankShortName", "BankAccNo", "DOS", "PaymentMode", "WorkingDaysInAMonth", "TotalProcDate", "TotalAbsent", "TotalWeekOff", "TotalHoliDay");

                }
                //DataTable dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "DepartmentName", "DesignationName", "BankName"
                //                                   , "DOJ", "BankShortName", "BankAccNo", "DOS", "PaymentMode", "WorkingDaysInAMonth", "TotalProcDate", "TotalAbsent", "TotalWeekOff", "TotalHoliDay");

                double totalSalaryAmount = 0.00;
                double totalSalaryAmounts = 0.00;
                double earningBonusAmount = 0.00;
                double totalEarningBonusAmountYearly = 0.00;
                bool isDecimal = false;
                double decimalNo = 0;
                xlsRow++;
                int startxlsRow = xlsRow;
                for (int dti = 0; dti < dtEmpInfo.Rows.Count; dti++)
                {
                    totalSalaryAmount = 0.00;
                    totalSalaryAmounts = 0.00;
                    totalEarningBonusAmountYearly = 0.00;
                    if (dicBonus.Count == 0)
                    {
                        //BonusList = dicBonusH[dtEmpInfo.Rows[dti]["EmpSystemId"].ToString()];
                    }
                    else
                    {
                        BonusList = dicBonus[dtEmpInfo.Rows[dti]["EmpSystemId"].ToString()];
                    }
                    //BonusList = dicBonus[dtEmpInfo.Rows[dti]["EmpSystemId"].ToString()];


                    var timeSpan = DateTime.Now - Convert.ToDateTime(dtEmpInfo.Rows[dti]["DOJ"].ToString()); ;
                    int yearSpan = new DateTime(timeSpan.Ticks).Year - 1;


                    sheet1.Range[xlsRow, colEmpCode].Text = dtEmpInfo.Rows[dti]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, colEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                    sheet1.Range[xlsRow, colEmpName].Text = dtEmpInfo.Rows[dti]["EmployeeName"].ToString();

                    var payDays = 0.00;
                    // clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"]);
                    if (!String.IsNullOrEmpty(dtEmpInfo.Rows[dti]["WorkingDaysInAMonth"].ToString().ToUpper()))
                    {
                        if (dtEmpInfo.Rows[dti]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                        {
                            payDays = clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalWeekOff"].ToString());

                        }
                        if (dtEmpInfo.Rows[dti]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                        {
                            payDays = clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalWeekOff"].ToString());
                        }
                    }
                    else
                    {
                        payDays = clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[dti]["TotalAbsent"].ToString());
                    }
                    sheet1.Range[xlsRow, colWorkInYears].Number = payDays;


                    sheet1.Range[xlsRow, colEmpCode, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    try
                    {
                        earningBonusAmount = 0.00;
                        //earningBonusAmount = 0.00;
                        if (BonusList != null)
                        {
                            for (int BNS = 0; BNS < BonusList.Count; BNS++)
                            {
                                totalSalaryAmount += Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                                totalSalaryAmounts += Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["EntryAmount"].ToString());
                                isDecimal = bplib.clsWebLib.GetBoolData(BonusList[BNS]["IntegerInDisb"].ToString());
                                decimalNo = clsStaticInfo.dbl(BonusList[BNS]["DecimalNo"].ToString());

                            }
                        }
                        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].Number = Service.Extension.clsStaticInfo.dbl(totalSalaryAmount);
                        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        sheet1.Range[xlsRow, colTotalYearlyActualBasicSalary].Number = Service.Extension.clsStaticInfo.dbl(totalSalaryAmounts);
                        sheet1.Range[xlsRow, colTotalYearlyActualBasicSalary].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalYearlyActualBasicSalary].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        if (dicBonusH.ContainsKey(dtEmpInfo.Rows[dti]["EmpSystemId"].ToString()) == true)
                        {
                            List<DataRow> BonusListH = dicBonusH[dtEmpInfo.Rows[dti]["EmpSystemId"].ToString()];
                            for (int i = 0; i < BonusListH.Count; i++)
                            {
                                if (BonusListH[i]["HeadCategory"].ToString().ToUpper() == "OTHER BONUS")
                                {
                                    earningBonusAmount = clsStaticInfo.dbl(BonusListH[i]["DisbusmentAmount"].ToString());
                                    totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                }

                                if (BonusListH[i]["HeadCategory"].ToString().ToUpper() == "RetainedBonus".ToUpper())
                                {
                                    earningBonusAmount += clsStaticInfo.dbl(BonusListH[i]["DisbusmentAmount"].ToString());
                                    totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                }
                                if (BonusListH[i]["HeadCategory"].ToString().ToUpper() == "Monthly Bonus Retain".ToUpper())
                                {
                                    earningBonusAmount += clsStaticInfo.dbl(BonusListH[i]["DisbusmentAmount"].ToString());
                                    totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                }
                            }
                        }

                        if (totalEarningBonusAmountYearly == 0)
                        {
                            sheet1.Range[xlsRow, colBonusAmount].Text = "-";// + Environment.NewLine + totalPayDay;                              
                            sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        }
                        else
                        {
                            sheet1.Range[xlsRow, colBonusAmount].Number = Convert.ToDouble(totalEarningBonusAmountYearly);// + Environment.NewLine + totalPayDay;
                            sheet1.Range[xlsRow, colBonusAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                            sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                            sheet1.Range[xlsRow, colTotalAmount].Number = Convert.ToDouble(totalEarningBonusAmountYearly);// + Environment.NewLine + totalPayDay;
                            sheet1.Range[xlsRow, colTotalAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                            sheet1.Range[xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        }


                        //for (int BNS = 0; BNS < BonusList.Count; BNS++)
                        //{
                        //    if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "BASIC")
                        //    {
                        //        totalSalaryAmount += Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                        //        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].Number = Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                        //        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //        sheet1.Range[xlsRow, colTotalYearlyBasicSalary].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //        sheet1.Range[xlsRow, colTotalYearlyActualBasicSalary].Number = Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["EntryAmount"].ToString());
                        //        sheet1.Range[xlsRow, colTotalYearlyActualBasicSalary].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //        sheet1.Range[xlsRow, colTotalYearlyActualBasicSalary].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //    }
                        //    else
                        //    {
                        //        totalSalaryAmount += Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["EntryAmount"].ToString());
                        //    }
                        //    if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "OTHER BONUS")
                        //    {
                        //        earningBonusAmount = clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                        //        totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                        //    }

                        //    if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "RetainedBonus".ToUpper())
                        //    {
                        //        earningBonusAmount += clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                        //        totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                        //    }

                        //    if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "Monthly Bonus Retain".ToUpper())
                        //    {
                        //        earningBonusAmount += clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                        //        totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                        //    }

                        //    isDecimal = bplib.clsWebLib.GetBoolData(BonusList[BNS]["IntegerInDisb"].ToString());
                        //    decimalNo = clsStaticInfo.dbl(BonusList[BNS]["DecimalNo"].ToString());
                        //    if (earningBonusAmount == 0)
                        //    {
                        //        sheet1.Range[xlsRow, colBonusAmount].Text = "-";// + Environment.NewLine + totalPayDay;                              
                        //        sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //        sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //    }
                        //    else
                        //    {
                        //        sheet1.Range[xlsRow, colBonusAmount].Number = Convert.ToDouble(earningBonusAmount);// + Environment.NewLine + totalPayDay;
                        //        sheet1.Range[xlsRow, colBonusAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                        //        sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //        sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        //        //sheet1.Range[xlsRow, colTotalAmount].Number = Convert.ToDouble(earningBonusAmount);// + Environment.NewLine + totalPayDay;
                        //        //sheet1.Range[xlsRow, colTotalAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                        //        //sheet1.Range[xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //        //sheet1.Range[xlsRow, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {

                    }
                    xlsRow++;
                }
                sheet1.Range[xlsRow, 1, xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, colWorkInYears, xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].Text = "Total:";// + Environment.NewLine + totalPayDay;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                //sheet1.Range[xlsRow, colTotalAmount].Formula = clsStaticInfo.GetxlsCol(colTotalAmount) + startxlsRow + ":" + clsStaticInfo.GetxlsCol(colTotalAmount) + (xlsRow - 1);// + Environment.NewLine + totalPayDay;
                //sheet1.Range[xlsRow, colTotalAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                //sheet1.Range[xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                sheet1.Range[xlsRow, colBonusAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBonusAmount) + startxlsRow + ":" + clsStaticInfo.GetxlsCol(colBonusAmount) + (xlsRow - 1) + ")";// + Environment.NewLine + totalPayDay;
                sheet1.Range[xlsRow, colBonusAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, colTotalYearlyBasicSalary].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colTotalYearlyBasicSalary) + startxlsRow + ":" + clsStaticInfo.GetxlsCol(colTotalYearlyBasicSalary) + (xlsRow - 1) + ")";// + Environment.NewLine + totalPayDay;
                sheet1.Range[xlsRow, colTotalYearlyBasicSalary].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                sheet1.Range[xlsRow, colTotalYearlyBasicSalary].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, colTotalYearlyBasicSalary].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                //xlsRow += 9;

                //ru.SetTextMiddle(ref sheet1, xlsRow, 1, "Prepared By", true, 25, 15);
                //sheet1[ru.GetColumnNameForXls(1) + xlsRow + ":" + ru.GetColumnNameForXls(2) + xlsRow].Merge();

                //ru.SetTextMiddle(ref sheet1, xlsRow, 4, "Manager(Payroll)", true, 25, 15);
                //sheet1[ru.GetColumnNameForXls(4) + xlsRow + ":" + ru.GetColumnNameForXls(6) + xlsRow].Merge();

                //ru.SetTextMiddle(ref sheet1, xlsRow, 7, "VP(HR)", true, 25, 15);
                //sheet1[ru.GetColumnNameForXls(7) + xlsRow + ":" + ru.GetColumnNameForXls(9) + xlsRow].Merge();

                //ru.SetTextMiddle(ref sheet1, xlsRow, 9, "DGM (Account)", true, 25, 15);
                //sheet1[ru.GetColumnNameForXls(9) + xlsRow + ":" + ru.GetColumnNameForXls(endXlsCol) + xlsRow].Merge();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
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


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Provisional Bonus As of " + Convert.ToDateTime(FromDate).ToString("MMMM") + " : " + Convert.ToDateTime(FromDate).Year.ToString() + " TO " + Convert.ToDateTime(ToDate).ToString("MMMM") + "," + Convert.ToDateTime(ToDate).Year.ToString();
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow, 1].Text = "Report Ref no:";
                sheet1.Range[xlsRow, 1, xlsRow, 2].Merge();

                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet1.UsedRange["A" + fPanRow].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;

                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$A$5:$IV$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;

                sheet1.Name = "BonusProvisional";
                #endregion            
                return workbook;
            }
            catch (Exception ex)
            {
                //return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Excel Cell settings
        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetCellTextDR(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(0);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }

        private void SetCellTextNumber(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }
        private void SetCellTextAttdn(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 7;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;

            ColIndex = xlsCol;
            xlsCol += 1;
        }
        #endregion

        private void CreateDynamicMonthHead(DataTable dtMonthList, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColStart, out List<FiscalYearMonthSequence> list)
        {
            try
            {
                list = new List<FiscalYearMonthSequence>();
                _total_head_count = 0;

                int countGross = 0;
                string grossFormula = "";
                string deductionFormula = "";
                for (int ci = 0; ci < dtMonthList.Rows.Count; ci++)
                {
                    _total_head_count++;
                    countGross++;
                    sheet1.Range[xlsRow, ColStart + countGross].Text = dtMonthList.Rows[ci]["MonthName"].ToString().Substring(0, 3) + "," + dtMonthList.Rows[ci]["MonthYear"].ToString().Substring(2, 2);
                    sheet1.Range[xlsRow, ColStart + countGross].ColumnWidth = 8;
                    sheet1.Range[xlsRow, ColStart + countGross].CellStyle.Font.Bold = true;
                    //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    sheet1.Range[xlsRow, ColStart + countGross].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].BorderAround(ExcelLineStyle.Thin);

                    FiscalYearMonthSequence fiscalYearMonthSequence = new FiscalYearMonthSequence();
                    fiscalYearMonthSequence.MonthName = dtMonthList.Rows[ci]["MonthName"].ToString();
                    fiscalYearMonthSequence.MonthNo = dtMonthList.Rows[ci]["MonthNumber"].ToString();
                    fiscalYearMonthSequence.LastDayOfMonth = dtMonthList.Rows[ci]["LastDayOfMonth"].ToString();
                    fiscalYearMonthSequence.MonthYear = dtMonthList.Rows[ci]["MonthYear"].ToString();
                    fiscalYearMonthSequence.XLColIndex = ColStart + countGross;

                    list.Add(fiscalYearMonthSequence);
                    xlsCol += 1;
                }//for         
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetDecimalFormat(SalaryHeadSequence shs)
        {
            try
            {
                var ob = new ReportUtility();
                if (shs.IsInt)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(shs.DecimalNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetDecimalFormat(bool isInt, int decimalNo)
        {
            try
            {
                var ob = new ReportUtility();
                if (isInt == true)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(decimalNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out Dictionary<string, SalaryHeadSequence> list)
        {
            try
            {
                list = new Dictionary<string, SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                int countGrossPostion = 0;
                string deductionFormula = "";

                xlsCol += 1;
                countGrossPostion++;

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {

                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper())
                        {
                            _total_head_count++;


                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;

                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                            salaryHeadSequence.IsNetPayEffect = Convert.ToBoolean(dtSalaryHead.Rows[ci]["PartOfNetPay"]);
                            salaryHeadSequence.IsGrossComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsGrossComponent"]);
                            salaryHeadSequence.IsCTCComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsCTCComponent"]);

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);
                            countCTCPosition++;
                        }


                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;


                _count_earning_ctchead = countCTCPosition - 1;

                int countDeductionPosition = countCTCPosition - 1;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                        {
                            _total_head_count++;
                            countDeductionPosition++;

                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 10;
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
                            if (deductionFormula.Length == 0)
                            {
                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                            }
                            else
                            {
                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                            }

                            //countDeductionPosition++;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;

                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();


                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

                            _count_deducting_head++;
                        }
                        //}//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, DataRow> GetDictionaryHourotmonthReportwithoutWeekendHoliday(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsRef = null;
            Dictionary<string, DataRow> dicHourlyOt = new Dictionary<string, DataRow>();
            string strSql = string.Empty;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                      ,sum(ho.Duration)as Duration,sum(CAST(ho.Duration AS decimal)/60)as DurationH
                                    ,ad.IsAllDesignation--1
                                    ,ISNULL(ad.IsFixed,0)as IsFixed---1--rate--0-farmula
                                    ,ISNULL(ad.Rate,0) as Rate
                                    ,ad.FormulaDesID
                                    ,ISNULL(dar.IsFixed,0)as IsFixedFromRate--1--rate--0--farmula
                                    ,ISNULL(dar.rate,0)as ratear
                                    ,dar.FormulaDesID FormulaDesIDFromRate
		                            ,ISNULL(bb.UserName,'') BankName
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory
                                      FROM HourlyOT  HO 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
                                      LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=HO.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ho.PlantId
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId							
                                      LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ad.PlantId AND dar.DesignationId=ei.GivenDesignationId
                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @"
                                   AND DT.Category NOT IN('Weekend','Holiday')  " + wcDos + @" AND ei.plantid='" + plantId + @"' " + wcEmpSystemId + @" 
                                    --AND ad.Catagory='HourlyOffDuty' 
                                    --AND ad.Active=1
                                    group by  EmployeeName
									,EmployeeCode
                                    ,ei.SystemId
									,DOJ
									,s.UserName
									,sb.UserName
									,lg.UserName
									,d.UserName
									,ei.GenderID
									,HO.EmpSystemId
									,l.UserName
									,hr.OTConsiderOn
                                    --,EntryAmount
                                    ,ad.IsAllDesignation
                                    ,ad.IsFixed
                                    ,ad.FormulaDesID
                                    ,dar.IsFixed
                                    ,dar.FormulaDesID
                                    ,ad.Rate
                                    ,dar.rate
	                                ,ei.DOS	,bb.UserName
									,PG.UserName
                                    ,ebi.IFSCCode
									,ebi.BankAccNo
                                    ,ec.UserName
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicHourlyOt.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicHourlyOt;
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

        public Dictionary<string, DataRow> GetDictionaryHourOTMonthReportWithWeekendORHoliday(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, string DayCategory)
        {
            ConnectionManager.DAL.ConManager objCon;
            Dictionary<string, DataRow> dicOTPolicy = new Dictionary<string, DataRow>();
            string strSql = string.Empty;
            DataSet dsRef = null;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                      ,sum(ho.Duration) AS Duration,SUM(CAST(ho.Duration AS decimal)/60) AS DurationH

                                    ,AD.IsAllDesignation--1
                                    ,ISNULL(ad.IsFixed,0) AS IsFixed---1--rate--0-farmula
                                    ,ISNULL(ad.Rate,0) AS Rate
                                    ,AD.FormulaDesID
                                    ,ISNULL(dar.IsFixed,0) AS IsFixedFromRate--1--rate--0--farmula
                                    ,ISNULL(dar.rate,0) AS ratear
                                    ,dar.FormulaDesID FormulaDesIDFromRate
		                            ,ISNULL(bb.UserName,'') BankName
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory
                                      FROM HourlyOT  HO 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
                                      LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=HO.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ho.PlantId
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
									
                                      LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ad.PlantId AND dar.DesignationId=ei.GivenDesignationId

                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @" AND DT.Category IN ('" + DayCategory + @"')  " + wcDos + @" AND ei.plantid='" + plantId + @"' " + wcEmpSystemId + @" 
                                        --AND ad.Catagory='HourlyOffDuty' AND ad.Active=1
                                    GROUP BY  EmployeeName,EmployeeCode,ei.SystemId,DOJ,s.UserName,sb.UserName,lg.UserName
									,d.UserName,ei.GenderID,HO.EmpSystemId,l.UserName,hr.OTConsiderOn --,EntryAmount
                                    ,ad.IsAllDesignation
                                    ,ad.IsFixed
                                    ,ad.FormulaDesID
                                    ,dar.IsFixed
                                    ,dar.FormulaDesID
                                    ,ad.Rate
                                    ,dar.rate
	                                ,ei.DOS	,bb.UserName
									,PG.UserName
                                    ,ebi.IFSCCode
									,ebi.BankAccNo
                                    ,ec.UserName
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicOTPolicy.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicOTPolicy;
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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


        class SalarySheetReportUD
        {
            public string EmpSystemID { get; set; }
            public string SalaryHeadID { get; set; }
            public string HeadCategory { get; set; }
            public decimal DisbusmentAmount { get; set; } = 0;
            public decimal EntryAmount { get; set; } = 0;
        }
        public Dictionary<string, List<DataRow>> GetMonthWiseEmpBonusInfo(string fromDate, string toDate, string companyGroupId, string companyId, string plantId, DataTable dtMonth)
        {
            string salaryProcessId = "";
            string strSqlSal = @"SELECT  m.SystemID FROM SalaryProcMaster m
                                    INNER JOIN SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID and c.PlantID='" + plantId + @"'
                                        WHERE 1=1 
                                        " + getMonthYearBonus(fromDate, toDate, "YearNo", "MonthNo") + @"";

            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
            salaryProcessId = "''";
            dtSalPrcId = dtSalPrcId.DefaultView.ToTable(true, "SystemID");
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }



            string strSql = @"SeLECT * FROM (SELECT  EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate,
								 EmpSlr.MonthNo, EmpSlr.YearNo
                                , EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID --TotalEmployee
								, EmpSlr.EntryAmount
                                , EmpSlr.DisbusmentAmount
								, EmpBasic.SystemId EmpSystemId
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
                                , EmpBasic.EmployeeCategory
                                , EmpBasic.WorkingDaysInAMonth
								, FORMAT(EmpBasic.DOS,'dd-MMM-yyyy') DOS
                                , ISNULL(TotalProcDate,0) TotalProcDate
                                  ,ISNULL(TotalPresent,0)  TotalPresent
                                  ,ISNULL(TotalLate,0) TotalLate
                                  ,ISNULL(TotalAbsent,0) TotalAbsent
                                  ,ISNULL(TotalLv,0) TotalLv
                                  ,ISNULL(TotalMLv,0) TotalMLv
                                  ,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv
                                  ,ISNULL(TotalWeekOff,0) TotalWeekOff
                                  ,ISNULL(TotalHoliDay,0) TotalHoliDay
                                  ,ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
                                  ,ISNULL(TotalOTHr,0) TotalOTHr
                                  ,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr
                                  ,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr
                                  ,ISNULL(WeekOffOTHr,0) WeekOffOTHr
                                  ,ISNULL(HoliDayOTHr,0) HoliDayOTHr
                                  ,ISNULL(TotalLWP,0) TotalLWP
                                , ISNULL(EmpSlr.IntegerInDisb,0) IntegerInDisb
								, ISNULL(EmpSlr.DecimalNo,0)  DecimalNo

							FROM
                                    (
										 SELECT  E.SystemID, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,DATEDIFF(YY,E.DOB,'" + fromDate + @"') As Age
											--DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
											,'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName, Bank.ShortName BankShortName, Bank.UserName BankName, EBI.BankAccNo
                                            ,E.PaymentMode,ISNULL(EC.UserName,'') EmployeeCategory, ISNULL(EC.WorkingDaysInAMonth,'') WorkingDaysInAMonth
                                     FROM EmployeeInformation E
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
left join org.Entity en on en.id=mb.EntityId   
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.LegalDesignation DE ON E.LegalDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON EN.UnitID = FU.Id
												LEFT JOIN org.Division DV ON p.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON p.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON p.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON p.SubSectionID = SS.Id
												LEFT JOIN EmployeeBankInfo EBI ON EBI.EmpSystemID = E.SystemId
												LEFT JOIN  HKP.Bank Bank ON EBI.BankSystemID = Bank.Id
												LEFT JOIN mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
                                                LEFT JOIN mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                                LEFT JOIN hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                    --LEFT JOIN [HKP].EmployeeCategory EC ON EC.Id = DesM.EmployeeCategoryId												    
                                     WHERE ISNULL(E.VendorId,'')=''
									) EmpBasic
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,
													SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType
                                                    ,sh.IsCTCComponent,sh.IsGrossComponent
                                                    ,CRC.IntegerInDisb, CRC.DecimalNo
													,sl.IsLocked
													,sh.IsRetained
											 FROM SalaryProcChild SPC
												INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN(" + salaryProcessId + @")
												INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID    
                                                LEFT JOIN SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                                            LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = SPC.SlrProcMstSystemID
																	LEFT JOIN CurrencyRuleMaster CRM ON CRM.SystemID = SRM.CurrencyRuleSystemID                                                                    
										LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID
														--WHERE	SPC.PlantId = 	'" + plantId + @"'
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID --AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, ISNULL(TotalProcDate,0)TotalProcDate
                                                         ,ISNULL(TotalPresent,0)  TotalPresent
                                                         ,ISNULL(TotalLate,0) TotalLate
                                                         ,ISNULL(TotalAbsent,0) TotalAbsent
                                                         ,ISNULL(TotalLv,0) TotalLv
                                                         ,ISNULL(TotalMLv,0) TotalMLv
                                                         ,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv
                                                         ,ISNULL(TotalWeekOff,0) TotalWeekOff
                                                         ,ISNULL(TotalHoliDay,0) TotalHoliDay
                                                         ,ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
                                                         ,ISNULL(TotalOTHr,0) TotalOTHr
                                                         ,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr
                                                         ,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr
                                                         ,ISNULL(WeekOffOTHr,0) WeekOffOTHr
                                                         ,ISNULL(HoliDayOTHr,0) HoliDayOTHr
                                                         ,ISNULL(TotalLWP,0) TotalLWP
				                              FROM SalaryProceAttdnData  WHERE " + getMonthYearWithoutAnd(fromDate, toDate, "YearNo", "MonthNo") + @" 
											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID AND EmpSlr.MonthNo =  MMDSA.MonthNo AND EMPslr.YearNo = MMDSA.YearNo												   
													   WHERE EmpSlr.IsLocked = 1  and 1=1
														and EmpBasic.PlantId = '" + plantId + @"' 
                                                " + getMonthYearBonus(fromDate, toDate, "EmpSlr.YearNo", "EmpSlr.MonthNo") + @"
													AND EmpSlr.HeadCategory In( 'Basic','Other Bonus','RetainedBonus','Monthly Bonus Retain','Annual Bonus Retain') --AND EmpSystemId = '2010025' 
                                                    -- ORDER BY EmpSystemId,YearNo,MonthNo
													)A

													Where A.HeadCategory In('Annual Bonus Retain')
													ORDER BY A.EmpSystemId,A.YearNo,A.MonthNo";
            
            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            List<DataRow> _data = new List<DataRow>();
            string empId = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                {
                    _data = new List<DataRow>();
                    dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                }
                _data.Add(dt.Rows[i]);

                empId = dt.Rows[i]["EmpSystemID"].ToString();
            }

            return dicBonus;
        }

        public Dictionary<string, List<DataRow>> GetSummarisedEmpBonusInfoO(string fromDate, string toDate, string companyGroupId, string companyId, string plantId, DataTable dtMonth)
        {
            string salaryProcessId = "";
            string strSqlSal = @"SELECT  m.SystemID FROM SalaryProcMaster m
                                    INNER JOIN SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID and c.PlantID='" + plantId + @"'
                                        WHERE 1=1 
                                        " + getMonthYearBonus(fromDate, toDate, "YearNo", "MonthNo") + @"";

            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
            salaryProcessId = "''";
            dtSalPrcId = dtSalPrcId.DefaultView.ToTable(true, "SystemID");
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }



            string strSql = @"SELECT    EmpSlr.PlantID
                                , EmpSlr.SalaryHead
								, EmpSlr.HeadCategory
								, EmpSlr.SalaryHeadID
								, EmpSlr.EntryCurrencyID
								, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID							
								, EmpBasic.SystemId EmpSystemId
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
                                , EmpBasic.DepartmentName
                                ,EmpBasic.WorkingDaysInAMonth
                                ,EmpBasic.EmpCategoryName

                                , EmpBasic.DesignationName
								, FORMAT(EmpBasic.DOS,'dd-MMM-yyyy') DOS
								, FORMAT(EmpBasic.DOJ,'dd-MMM-yyyy') DOJ
                                , TotalProcDate
	                            , TotalPresent
	                            , TotalLate
	                            , TotalAbsent 
	                            , TotalLv
	                            , TotalMLv
	                            , TotalCompAssignLv
	                            , TotalWeekOff
	                            , TotalHoliDay
	                            , TotalWeekOffHoliDay
	                            , TotalOTHr
	                            , TotalNormalOTHr
	                            , TotalExtraOTHr
                                --, ISNULL(MMDSA.TotalProcDate,0) - ISNULL(MMDSA.AbsentDays,0) PayDays
                                , ISNULL(EmpSlr.IntegerInDisb,0) IntegerInDisb
								, ISNULL(EmpSlr.DecimalNo,0)  DecimalNo
								, SUM(EmpSlr.EntryAmount) EntryAmount
                                , SUM(EmpSlr.DisbusmentAmount) DisbusmentAmount

							FROM
                                    (
										 SELECT  E.SystemID, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,DATEDIFF(YY,E.DOB,'" + fromDate + @"') As Age
											--DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
                                            , DE.UserName DesignationName
											,'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,Ec.WorkingDaysInAMonth, Bank.ShortName BankShortName, Bank.UserName BankName, EBI.BankAccNo
                                            ,E.PaymentMode
                                     FROM EmployeeInformation E
left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.LegalDesignation DE ON E.LegalDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON EN.UnitID = FU.Id
												LEFT JOIN org.Division DV ON p.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON P.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON p.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON P.SubSectionID = SS.Id
												left join EmployeeBankInfo EBI ON EBI.EmpSystemID = E.SystemId
												left join  HKP.Bank Bank ON EBI.BankSystemID = Bank.Id
													left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
													left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
													left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId											    
												LEFT JOIN hkp.DesignationGroup DG ON DM.DesignationGroupId = DG.ID
                                    --WHERE E.PlantId='" + plantId + @"'
                                    WHERE ISNULL(E.VendorId,'') = ''
									) EmpBasic
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,
													SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType
                                                    ,sh.IsCTCComponent,sh.IsGrossComponent
                                                    ,CRC.IntegerInDisb, CRC.DecimalNo,sl.IsLocked
													,sh.IsRetained
											 FROM SalaryProcChild SPC
												INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN(" + salaryProcessId + @")
												inner join EmployeeInformation einfo on einfo.SystemId=spc.EmpInfoSystemID
												--left join BonusPolicyMonthlyRetainMaster bn on bn.PlantID = einfo.PlantId
												--join BonusPolicyMonthlyRetainMasterSalaryHead bns on bns.BonusPolicyMonthlyRetainMasterId = bn.ID
												INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID 
                                                LEFT JOIN SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                                            LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = SPC.SlrProcMstSystemID
																	LEFT JOIN CurrencyRuleMaster CRM ON CRM.SystemID = SRM.CurrencyRuleSystemID                                                                    
										LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID
															WHERE sh.HeadCategory in ('Basic','OTHER BONUS','RetainedBonus','Monthly Bonus Retain')	
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID --AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID
                                                  , 	SUM(ISNULL(TotalProcDate,0)) TotalProcDate
	                                                ,SUM(ISNULL(TotalPresent,0)) TotalPresent
	                                                ,SUM(ISNULL(TotalLate,0)) TotalLate
	                                                ,SUM(ISNULL(TotalAbsent,0)) TotalAbsent
	                                                ,SUM(ISNULL(TotalLv,0)) TotalLv
	                                                ,SUM(ISNULL(TotalMLv,0)) TotalMLv
	                                                ,SUM(ISNULL(TotalCompAssignLv,0)) TotalCompAssignLv
	                                                ,SUM(ISNULL(TotalWeekOff,0)) TotalWeekOff
	                                                ,SUM(ISNULL(TotalHoliDay,0)) TotalHoliDay
	                                                ,SUM(ISNULL(TotalWeekOffHoliDay,0)) TotalWeekOffHoliDay
	                                                ,SUM(ISNULL(TotalOTHr,0)) TotalOTHr
	                                                ,SUM(ISNULL(TotalNormalOTHr,0)) TotalNormalOTHr
	                                                ,SUM(ISNULL(TotalExtraOTHr,0)) TotalExtraOTHr

				                              FROM SalaryProceAttdnData  WHERE " + getMonthYearWithoutAnd(fromDate, toDate, "YearNo", "MonthNo") + @" 
											 GROUP BY EmpSystemID
                            ) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID --AND EmpSlr.MonthNo =  MMDSA.MonthNo AND EMPslr.YearNo = MMDSA.YearNo												   
													   WHERE EmpSlr.IsLocked = 1  and 1=1
														and EmpBasic.PlantId = '" + plantId + @"' 
                                                " + getMonthYearBonus(fromDate, toDate, "EmpSlr.YearNo", "EmpSlr.MonthNo") + @"
													
                                Group by  EmpSlr.PlantID 
                                , EmpSlr.SalaryHead
								, EmpSlr.HeadCategory
								, EmpSlr.SalaryHeadID
								, EmpSlr.EntryCurrencyID
								, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID 
							    , EmpBasic.DepartmentName
								, EmpBasic.SystemId 
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
								, EmpBasic.DOS
								, EmpBasic.DOJ
                                , EmpSlr.IntegerInDisb
								, EmpSlr.DecimalNo
                                , EmpBasic.DesignationName
                                , TotalProcDate
	                            , TotalPresent
	                            , TotalLate
	                            , TotalAbsent 
	                            , TotalLv
	                            , TotalMLv
	                            , TotalCompAssignLv
	                            , TotalWeekOff
	                            , TotalHoliDay
	                            , TotalWeekOffHoliDay
	                            , TotalOTHr
	                            , TotalNormalOTHr
	                            , TotalExtraOTHr
                                ,EmpBasic.WorkingDaysInAMonth
                                ,EmpBasic.EmpCategoryName
                                HAVING SUM(EmpSlr.DisbusmentAmount) > 0
								ORDER BY EmpSystemId";
            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            List<DataRow> _data = new List<DataRow>();
            string empId = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                {
                    _data = new List<DataRow>();
                    dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                }
                _data.Add(dt.Rows[i]);

                empId = dt.Rows[i]["EmpSystemID"].ToString();
            }

            return dicBonus;
        }
        public Dictionary<string, List<DataRow>> GetSummarisedEmpBonusInfo(string fromDate, string toDate, string companyGroupId, string companyId, string plantId, DataTable dtMonth)
        {
            string salaryProcessId = "";
            string strSqlSal = @"SELECT  m.SystemID FROM SalaryProcMaster m
                                    INNER JOIN SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID and c.PlantID='" + plantId + @"'
                                        WHERE 1=1 
                                        " + getMonthYearBonus(fromDate, toDate, "YearNo", "MonthNo") + @"";

            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
            salaryProcessId = "''";
            dtSalPrcId = dtSalPrcId.DefaultView.ToTable(true, "SystemID");
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }



            string strSql = @"SELECT    EmpSlr.PlantID
                                , EmpSlr.SalaryHead
								, EmpSlr.HeadCategory
								, EmpSlr.SalaryHeadID
								, EmpSlr.EntryCurrencyID
								, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID							
								, EmpBasic.SystemId EmpSystemId
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
                                , EmpBasic.DepartmentName
                                ,EmpBasic.WorkingDaysInAMonth
                                ,EmpBasic.EmpCategoryName

                                , EmpBasic.DesignationName
								, FORMAT(EmpBasic.DOS,'dd-MMM-yyyy') DOS
								, FORMAT(EmpBasic.DOJ,'dd-MMM-yyyy') DOJ
                                , TotalProcDate
	                            , TotalPresent
	                            , TotalLate
	                            , TotalAbsent 
	                            , TotalLv
	                            , TotalMLv
	                            , TotalCompAssignLv
	                            , TotalWeekOff
	                            , TotalHoliDay
	                            , TotalWeekOffHoliDay
	                            , TotalOTHr
	                            , TotalNormalOTHr
	                            , TotalExtraOTHr
                                --, ISNULL(MMDSA.TotalProcDate,0) - ISNULL(MMDSA.AbsentDays,0) PayDays
                                , ISNULL(EmpSlr.IntegerInDisb,0) IntegerInDisb
								, ISNULL(EmpSlr.DecimalNo,0)  DecimalNo
								, SUM(EmpSlr.EntryAmount) EntryAmount
                                , SUM(EmpSlr.DisbusmentAmount) DisbusmentAmount

							FROM
                                    (
										 SELECT  E.SystemID, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,DATEDIFF(YY,E.DOB,'" + fromDate + @"') As Age
											--DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
                                            , DE.UserName DesignationName
											,'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,Ec.WorkingDaysInAMonth, Bank.ShortName BankShortName, Bank.UserName BankName, EBI.BankAccNo
                                            ,E.PaymentMode
                                     FROM EmployeeInformation E
left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.LegalDesignation DE ON E.LegalDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON EN.UnitID = FU.Id
												LEFT JOIN org.Division DV ON P.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON P.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON P.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON P.SubSectionID = SS.Id
												left join EmployeeBankInfo EBI ON EBI.EmpSystemID = E.SystemId
												left join  HKP.Bank Bank ON EBI.BankSystemID = Bank.Id
													left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
													left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
												LEFT JOIN hkp.DesignationGroup DG ON DM.DesignationGroupId = DG.ID
													left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId											    
                                    --WHERE E.PlantId='" + plantId + @"'
                                    WHERE ISNULL(E.VendorId,'') = ''
									) EmpBasic
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,
													SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType
                                                    ,sh.IsCTCComponent,sh.IsGrossComponent
                                                    ,CRC.IntegerInDisb, CRC.DecimalNo,sl.IsLocked
													,sh.IsRetained
											 FROM SalaryProcChild SPC
												INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN(" + salaryProcessId + @")
												inner join EmployeeInformation einfo on einfo.SystemId=spc.EmpInfoSystemID
												left join BonusPolicyMonthlyRetainMaster bn on bn.PlantID = einfo.PlantId
												join BonusPolicyMonthlyRetainMasterSalaryHead bns on bns.BonusPolicyMonthlyRetainMasterId = bn.ID
												INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID 
                                                LEFT JOIN SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                                            LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = SPC.SlrProcMstSystemID
																	LEFT JOIN CurrencyRuleMaster CRM ON CRM.SystemID = SRM.CurrencyRuleSystemID                                                                    
										LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID
															WHERE sh.SalaryHeadID IN ( bns.SalaryHeadID)	
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID --AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID
                                                  , 	SUM(ISNULL(TotalProcDate,0)) TotalProcDate
	                                                ,SUM(ISNULL(TotalPresent,0)) TotalPresent
	                                                ,SUM(ISNULL(TotalLate,0)) TotalLate
	                                                ,SUM(ISNULL(TotalAbsent,0)) TotalAbsent
	                                                ,SUM(ISNULL(TotalLv,0)) TotalLv
	                                                ,SUM(ISNULL(TotalMLv,0)) TotalMLv
	                                                ,SUM(ISNULL(TotalCompAssignLv,0)) TotalCompAssignLv
	                                                ,SUM(ISNULL(TotalWeekOff,0)) TotalWeekOff
	                                                ,SUM(ISNULL(TotalHoliDay,0)) TotalHoliDay
	                                                ,SUM(ISNULL(TotalWeekOffHoliDay,0)) TotalWeekOffHoliDay
	                                                ,SUM(ISNULL(TotalOTHr,0)) TotalOTHr
	                                                ,SUM(ISNULL(TotalNormalOTHr,0)) TotalNormalOTHr
	                                                ,SUM(ISNULL(TotalExtraOTHr,0)) TotalExtraOTHr

				                              FROM SalaryProceAttdnData  WHERE " + getMonthYearWithoutAnd(fromDate, toDate, "YearNo", "MonthNo") + @" 
											 GROUP BY EmpSystemID
                            ) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID --AND EmpSlr.MonthNo =  MMDSA.MonthNo AND EMPslr.YearNo = MMDSA.YearNo												   
													   WHERE EmpSlr.IsLocked = 1  and 1=1
														and EmpBasic.PlantId = '" + plantId + @"' 
                                                " + getMonthYearBonus(fromDate, toDate, "EmpSlr.YearNo", "EmpSlr.MonthNo") + @"
													
                                Group by  EmpSlr.PlantID 
                                , EmpSlr.SalaryHead
								, EmpSlr.HeadCategory
								, EmpSlr.SalaryHeadID
								, EmpSlr.EntryCurrencyID
								, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID 
							    , EmpBasic.DepartmentName
								, EmpBasic.SystemId 
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
								, EmpBasic.DOS
								, EmpBasic.DOJ
                                , EmpSlr.IntegerInDisb
								, EmpSlr.DecimalNo
                                , EmpBasic.DesignationName
                                , TotalProcDate
	                            , TotalPresent
	                            , TotalLate
	                            , TotalAbsent 
	                            , TotalLv
	                            , TotalMLv
	                            , TotalCompAssignLv
	                            , TotalWeekOff
	                            , TotalHoliDay
	                            , TotalWeekOffHoliDay
	                            , TotalOTHr
	                            , TotalNormalOTHr
	                            , TotalExtraOTHr
                                ,EmpBasic.WorkingDaysInAMonth
                                ,EmpBasic.EmpCategoryName
                                HAVING SUM(EmpSlr.DisbusmentAmount) > 0
								ORDER BY EmpSystemId";
            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            List<DataRow> _data = new List<DataRow>();
            string empId = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                {
                    _data = new List<DataRow>();
                    dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                }
                _data.Add(dt.Rows[i]);

                empId = dt.Rows[i]["EmpSystemID"].ToString();
            }

            return dicBonus;
        }
        public Dictionary<string, List<DataRow>> GetSummarisedEmpBonusInfoOld(string fromDate, string toDate, string companyGroupId, string companyId, string plantId, DataTable dtMonth)
        {
            string salaryProcessId = "";
            string strSqlSal = @"SELECT  m.SystemID FROM SalaryProcMaster m
                                    INNER JOIN SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID and c.PlantID='" + plantId + @"'
                                        WHERE 1=1 
                                        " + getMonthYearBonus(fromDate, toDate, "YearNo", "MonthNo") + @"";

            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
            salaryProcessId = "''";
            dtSalPrcId = dtSalPrcId.DefaultView.ToTable(true, "SystemID");
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }



            string strSql = @"SELECT    EmpSlr.PlantID
                                , EmpSlr.SalaryHead
								, EmpSlr.HeadCategory
								, EmpSlr.SalaryHeadID
								, EmpSlr.EntryCurrencyID
								, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID							
								, EmpBasic.SystemId EmpSystemId
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
                                , EmpBasic.DepartmentName
                                ,EmpBasic.WorkingDaysInAMonth
                                ,EmpBasic.EmpCategoryName

                                , EmpBasic.DesignationName
								, FORMAT(EmpBasic.DOS,'dd-MMM-yyyy') DOS
								, FORMAT(EmpBasic.DOJ,'dd-MMM-yyyy') DOJ
                                , TotalProcDate
	                            , TotalPresent
	                            , TotalLate
	                            , TotalAbsent 
	                            , TotalLv
	                            , TotalMLv
	                            , TotalCompAssignLv
	                            , TotalWeekOff
	                            , TotalHoliDay
	                            , TotalWeekOffHoliDay
	                            , TotalOTHr
	                            , TotalNormalOTHr
	                            , TotalExtraOTHr
                                --, ISNULL(MMDSA.TotalProcDate,0) - ISNULL(MMDSA.AbsentDays,0) PayDays
                                , ISNULL(EmpSlr.IntegerInDisb,0) IntegerInDisb
								, ISNULL(EmpSlr.DecimalNo,0)  DecimalNo
								, SUM(EmpSlr.EntryAmount) EntryAmount
                                , SUM(EmpSlr.DisbusmentAmount) DisbusmentAmount

							FROM
                                    (
										 SELECT  E.SystemID, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,DATEDIFF(YY,E.DOB,'" + fromDate + @"') As Age
											--DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
                                            , DE.UserName DesignationName
											,'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,Ec.WorkingDaysInAMonth, Bank.ShortName BankShortName, Bank.UserName BankName, EBI.BankAccNo
                                            ,E.PaymentMode
                                     FROM EmployeeInformation E
left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.LegalDesignation DE ON E.LegalDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON EN.UnitID = FU.Id
												LEFT JOIN org.Division DV ON P.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON P.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON p.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON P.SubSectionID = SS.Id
												left join EmployeeBankInfo EBI ON EBI.EmpSystemID = E.SystemId
												left join  HKP.Bank Bank ON EBI.BankSystemID = Bank.Id
													left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
													left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
													left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId											    
												LEFT JOIN hkp.DesignationGroup DG ON DM.DesignationGroupId = DG.ID
                                    --WHERE E.PlantId='" + plantId + @"'
                                    WHERE ISNULL(E.VendorId,'') = ''
									) EmpBasic
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,
													SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType
                                                    ,sh.IsCTCComponent,sh.IsGrossComponent
                                                    ,CRC.IntegerInDisb, CRC.DecimalNo
											 FROM SalaryProcChild SPC
												INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN(" + salaryProcessId + @")
												INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID                                                                      
                                            LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = SPC.SlrProcMstSystemID
																	LEFT JOIN CurrencyRuleMaster CRM ON CRM.SystemID = SRM.CurrencyRuleSystemID                                                                    
										LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID
															WHERE HeadCategory IN ( 'Basic','Other Bonus','RetainedBonus') 	
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID --AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID
                                                  , 	SUM(ISNULL(TotalProcDate,0)) TotalProcDate
	,SUM(ISNULL(TotalPresent,0)) TotalPresent
	,SUM(ISNULL(TotalLate,0)) TotalLate
	,SUM(ISNULL(TotalAbsent,0)) TotalAbsent
	,SUM(ISNULL(TotalLv,0)) TotalLv
	,SUM(ISNULL(TotalMLv,0)) TotalMLv
	,SUM(ISNULL(TotalCompAssignLv,0)) TotalCompAssignLv
	,SUM(ISNULL(TotalWeekOff,0)) TotalWeekOff
	,SUM(ISNULL(TotalHoliDay,0)) TotalHoliDay
	,SUM(ISNULL(TotalWeekOffHoliDay,0)) TotalWeekOffHoliDay
	,SUM(ISNULL(TotalOTHr,0)) TotalOTHr
	,SUM(ISNULL(TotalNormalOTHr,0)) TotalNormalOTHr
	,SUM(ISNULL(TotalExtraOTHr,0)) TotalExtraOTHr

				                              FROM SalaryProceAttdnData  WHERE " + getMonthYearWithoutAnd(fromDate, toDate, "YearNo", "MonthNo") + @" 
											 GROUP BY EmpSystemID
) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID --AND EmpSlr.MonthNo =  MMDSA.MonthNo AND EMPslr.YearNo = MMDSA.YearNo												   
													   WHERE 1=1
														and EmpBasic.PlantId = '" + plantId + @"' 
                                                " + getMonthYearBonus(fromDate, toDate, "EmpSlr.YearNo", "EmpSlr.MonthNo") + @"
													
                                Group by  EmpSlr.PlantID 
                                , EmpSlr.SalaryHead
								, EmpSlr.HeadCategory
								, EmpSlr.SalaryHeadID
								, EmpSlr.EntryCurrencyID
								, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID 
							    , EmpBasic.DepartmentName
								, EmpBasic.SystemId 
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
								, EmpBasic.DOS
								, EmpBasic.DOJ
                                , EmpSlr.IntegerInDisb
								, EmpSlr.DecimalNo
                                , EmpBasic.DesignationName
                                , TotalProcDate
	                            , TotalPresent
	                            , TotalLate
	                            , TotalAbsent 
	                            , TotalLv
	                            , TotalMLv
	                            , TotalCompAssignLv
	                            , TotalWeekOff
	                            , TotalHoliDay
	                            , TotalWeekOffHoliDay
	                            , TotalOTHr
	                            , TotalNormalOTHr
	                            , TotalExtraOTHr
                                ,EmpBasic.WorkingDaysInAMonth
                                ,EmpBasic.EmpCategoryName
                                HAVING SUM(EmpSlr.DisbusmentAmount) > 0
								ORDER BY EmpSystemId";
            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            List<DataRow> _data = new List<DataRow>();
            string empId = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                {
                    _data = new List<DataRow>();
                    dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                }
                _data.Add(dt.Rows[i]);

                empId = dt.Rows[i]["EmpSystemID"].ToString();
            }

            return dicBonus;
        }
        public Dictionary<string, List<DataRow>> GetSummarisedEmpBonusInfoH(string fromDate, string toDate, string companyGroupId, string companyId, string plantId, DataTable dtMonth)
        {
            string salaryProcessId = "";
            string strSqlSal = @"SELECT  m.SystemID FROM SalaryProcMaster m
                                    INNER JOIN SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID and c.PlantID='" + plantId + @"'
                                        WHERE 1=1 
                                        " + getMonthYearBonus(fromDate, toDate, "YearNo", "MonthNo") + @"";

            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
            salaryProcessId = "''";
            dtSalPrcId = dtSalPrcId.DefaultView.ToTable(true, "SystemID");
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }



            string strSql = @"SELECT    EmpSlr.PlantID
                                , EmpSlr.SalaryHead
								, EmpSlr.HeadCategory
								, EmpSlr.SalaryHeadID
								, EmpSlr.EntryCurrencyID
								, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID							
								, EmpBasic.SystemId EmpSystemId
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
                                , EmpBasic.DepartmentName
                                ,EmpBasic.WorkingDaysInAMonth
                                ,EmpBasic.EmpCategoryName

                                , EmpBasic.DesignationName
								, FORMAT(EmpBasic.DOS,'dd-MMM-yyyy') DOS
								, FORMAT(EmpBasic.DOJ,'dd-MMM-yyyy') DOJ
                                , TotalProcDate
	                            , TotalPresent
	                            , TotalLate
	                            , TotalAbsent 
	                            , TotalLv
	                            , TotalMLv
	                            , TotalCompAssignLv
	                            , TotalWeekOff
	                            , TotalHoliDay
	                            , TotalWeekOffHoliDay
	                            , TotalOTHr
	                            , TotalNormalOTHr
	                            , TotalExtraOTHr
                                --, ISNULL(MMDSA.TotalProcDate,0) - ISNULL(MMDSA.AbsentDays,0) PayDays
                                , ISNULL(EmpSlr.IntegerInDisb,0) IntegerInDisb
								, ISNULL(EmpSlr.DecimalNo,0)  DecimalNo
								, SUM(EmpSlr.EntryAmount) EntryAmount
                                , SUM(EmpSlr.DisbusmentAmount) DisbusmentAmount

							FROM
                                    (
										 SELECT  E.SystemID, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,DATEDIFF(YY,E.DOB,'" + fromDate + @"') As Age
											--DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
                                            , DE.UserName DesignationName
											,'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,Ec.WorkingDaysInAMonth, Bank.ShortName BankShortName, Bank.UserName BankName, EBI.BankAccNo
                                            ,E.PaymentMode
                                     FROM EmployeeInformation E
left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.LegalDesignation DE ON E.LegalDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON EN.UnitID = FU.Id
												LEFT JOIN org.Division DV ON P.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON P.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON P.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON P.SubSectionID = SS.Id
												left join EmployeeBankInfo EBI ON EBI.EmpSystemID = E.SystemId
												left join  HKP.Bank Bank ON EBI.BankSystemID = Bank.Id
													left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
													left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
													left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId											    
												LEFT JOIN hkp.DesignationGroup DG ON DM.DesignationGroupId = DG.ID
                                    --WHERE E.PlantId='" + plantId + @"'
                                    WHERE ISNULL(E.VendorId,'') = ''
									) EmpBasic
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,
													SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType
                                                    ,sh.IsCTCComponent,sh.IsGrossComponent
                                                    ,CRC.IntegerInDisb, CRC.DecimalNo,sl.IsLocked
													,sh.IsRetained
											 FROM SalaryProcChild SPC
												INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN(" + salaryProcessId + @")
												inner join EmployeeInformation einfo on einfo.SystemId=spc.EmpInfoSystemID
												--left join BonusPolicyMonthlyRetainMaster bn on bn.PlantID = einfo.PlantId
												--join BonusPolicyMonthlyRetainMasterSalaryHead bns on bns.BonusPolicyMonthlyRetainMasterId = bn.ID
												INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID 
                                                LEFT JOIN SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                                            LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = SPC.SlrProcMstSystemID
																	LEFT JOIN CurrencyRuleMaster CRM ON CRM.SystemID = SRM.CurrencyRuleSystemID                                                                    
										LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID
															WHERE sh.HeadCategory in ('OTHER BONUS','RetainedBonus','Monthly Bonus Retain')
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID --AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID
                                                  , 	SUM(ISNULL(TotalProcDate,0)) TotalProcDate
	                                                ,SUM(ISNULL(TotalPresent,0)) TotalPresent
	                                                ,SUM(ISNULL(TotalLate,0)) TotalLate
	                                                ,SUM(ISNULL(TotalAbsent,0)) TotalAbsent
	                                                ,SUM(ISNULL(TotalLv,0)) TotalLv
	                                                ,SUM(ISNULL(TotalMLv,0)) TotalMLv
	                                                ,SUM(ISNULL(TotalCompAssignLv,0)) TotalCompAssignLv
	                                                ,SUM(ISNULL(TotalWeekOff,0)) TotalWeekOff
	                                                ,SUM(ISNULL(TotalHoliDay,0)) TotalHoliDay
	                                                ,SUM(ISNULL(TotalWeekOffHoliDay,0)) TotalWeekOffHoliDay
	                                                ,SUM(ISNULL(TotalOTHr,0)) TotalOTHr
	                                                ,SUM(ISNULL(TotalNormalOTHr,0)) TotalNormalOTHr
	                                                ,SUM(ISNULL(TotalExtraOTHr,0)) TotalExtraOTHr

				                              FROM SalaryProceAttdnData  WHERE " + getMonthYearWithoutAnd(fromDate, toDate, "YearNo", "MonthNo") + @" 
											 GROUP BY EmpSystemID
                            ) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID --AND EmpSlr.MonthNo =  MMDSA.MonthNo AND EMPslr.YearNo = MMDSA.YearNo												   
													   WHERE EmpSlr.IsLocked = 1  and 1=1
														and EmpBasic.PlantId = '" + plantId + @"' 
                                                " + getMonthYearBonus(fromDate, toDate, "EmpSlr.YearNo", "EmpSlr.MonthNo") + @"
													
                                Group by  EmpSlr.PlantID 
                                , EmpSlr.SalaryHead
								, EmpSlr.HeadCategory
								, EmpSlr.SalaryHeadID
								, EmpSlr.EntryCurrencyID
								, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID 
							    , EmpBasic.DepartmentName
								, EmpBasic.SystemId 
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
								, EmpBasic.DOS
								, EmpBasic.DOJ
                                , EmpSlr.IntegerInDisb
								, EmpSlr.DecimalNo
                                , EmpBasic.DesignationName
                                , TotalProcDate
	                            , TotalPresent
	                            , TotalLate
	                            , TotalAbsent 
	                            , TotalLv
	                            , TotalMLv
	                            , TotalCompAssignLv
	                            , TotalWeekOff
	                            , TotalHoliDay
	                            , TotalWeekOffHoliDay
	                            , TotalOTHr
	                            , TotalNormalOTHr
	                            , TotalExtraOTHr
                                ,EmpBasic.WorkingDaysInAMonth
                                ,EmpBasic.EmpCategoryName
                                HAVING SUM(EmpSlr.DisbusmentAmount) > 0
								ORDER BY EmpSystemId";
            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            List<DataRow> _data = new List<DataRow>();
            string empId = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                {
                    _data = new List<DataRow>();
                    dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                }
                _data.Add(dt.Rows[i]);

                empId = dt.Rows[i]["EmpSystemID"].ToString();
            }

            return dicBonus;
        }
        public string getMonthYearWithoutAnd(string fromDate, string toDate, string monthNo, string yearNo)
        {
            var r = "";
            var _fDate = Convert.ToDateTime(fromDate);
            var _tDate = Convert.ToDateTime(toDate);
            while (_fDate < _tDate)
            {
                if (r.Length == 0)
                {
                    r = " (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";
                }
                else
                {
                    r += " OR (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";

                }
                _fDate = _fDate.AddMonths(1);

            }
            if (r.Length > 0)
            {
                r = " (" + r + ")";
            }

            return r;
        }


        public string getMonthYearBonus(string fromDate, string toDate, string monthNo, string yearNo)
        {
            var r = "";
            var _fDate = Convert.ToDateTime(fromDate);
            var _tDate = Convert.ToDateTime(toDate);
            while (_fDate < _tDate)
            {
                if (r.Length == 0)
                {
                    r = " (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";
                }
                else
                {
                    r += " OR (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";

                }
                _fDate = _fDate.AddMonths(1);

            }
            if (r.Length > 0)
            {
                r = " AND (" + r + ")";
            }

            return r;
        }
    }
}
