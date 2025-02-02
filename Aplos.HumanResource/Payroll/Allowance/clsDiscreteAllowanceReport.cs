using ConnectionManager;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Allowance
{
    public class clsDiscreteAllowanceReport
    {
        ISqlRepository _sqlRepository;
        public clsDiscreteAllowanceReport()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetShift(string plantId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select SystemID, concat( UserName,' ','(' ,ShiftType,')') ShiftType from ShiftDefination where PlantID ='" + plantId + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetEmployeeCategoryList()
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select ec.Id, Ec.UserName as EmployeeCategory from HKP.EmployeeCategory as EC ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public IEnumerable<object> GetEntityList(string plantId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select e.Id,e.UserName as Entity from org.entity as e where e.PlantId  ='" + plantId + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public IEnumerable<object> GetSection(string Dept)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT DISTINCT S.Id,S.UserName FROM ORG.Position  P
						  LEFT JOIN ORG.Section S ON S.Id=P.SectionId
						   WHERE P.DepartmentId in ("+ Dept + ")";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public IEnumerable<object> GetSubSection(string ssub)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT distinct SB.Id,SB.UserName FROM ORG.Position P
                        LEFT JOIN ORG.SubSection SB ON SB.Id=P.SubSectionId
                          WHERE p.SectionId in (" + ssub + ")";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public IEnumerable<object> GetDeptListList(string EntityId,string plantId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"Select distinct D.Id,D.UserName Depertment from ORG.Department D
                            LEFT JOIN org.Position P ON P.DepartmentId=D.Id
                            LEFT JOIN MST.ManpowerBudget MB ON MB.PositionId=P.Id
                            LEFT JOIN org.Entity E ON E.Id=MB.EntityId
                            Where E.Id='" + EntityId + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public IWorkbook XlsEmpDayStatusRpt(string PlantId, string FromDate, string Shift, string Todate, bool Absent, bool Late, bool LvWP, bool LvWOP)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "EmployeeDayStatus";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetData(PlantId, FromDate, Shift, Todate, Absent, Late, LvWP, LvWOP);

            #region Headers


            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignLeft);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 25, ExcelHAlign.HAlignLeft);
            int ColEmployeeName = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Legal Designation", 15, ExcelHAlign.HAlignLeft);
            int ColLegalDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 15, ExcelHAlign.HAlignLeft);
            int ColDOJ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeStatus", 15, ExcelHAlign.HAlignLeft);
            int ColEmployeeStatus = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Unit", 15, ExcelHAlign.HAlignLeft);
            int ColUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Division", 15, ExcelHAlign.HAlignLeft);
            int ColDivision = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 15, ExcelHAlign.HAlignLeft);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 15, ExcelHAlign.HAlignLeft);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SubSection", 15, ExcelHAlign.HAlignLeft);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCategory", 20, ExcelHAlign.HAlignLeft);
            int ColEmployeeCategory = COL;
            COL++;

            endCol = COL;
            #endregion Headers

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColLegalDesignation].Text = data.Rows[i]["LegalDesignation"].ToString();
                sheet[ROW, ColDOJ].Text = data.Rows[i]["DOJ"].ToString();
                sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                sheet[ROW, ColUnit].Text = data.Rows[i]["Unit"].ToString();
                sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyHeader(ref sheet, endCol, "Employee Day Status: " + FromDate + " - " + Todate + " ", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetData(string PlantId, string FromDate, string sft, string Todate, bool Absent, bool Late, bool LvWP, bool LvWOP)
        {
            string sql = string.Empty;
            try
            {
                if (Absent == true)
                {
                    sql = sql + @"and e.SystemID not in
                                    (
                                    select EmpSystemID from AttdnProcessData where WorkDate between '" + FromDate + @"' and '" + Todate + @"'
                                    and ShiftSystemID in (" + sft+@")
                                    and DayStatus in (select DayType from DayType where Category='absent')
                                    )";
                }
                if (Late == true)
                {
                    sql = sql + @" and e.SystemID not in
                                    (
                                    select EmpSystemID from AttdnProcessData where WorkDate between '" + FromDate + @"' and '" + Todate + @"'
                                    and ShiftSystemID in (" + sft + @")
                                    and DayStatus in (select DayType from DayType where Category='Late')
                                    )";

                }
                if (LvWP == true)
                {
                    sql = sql + @"and e.SystemId not in (
                                select distinct EmpSystemID from AttdnProcessData a where
                                WorkDate between '" + FromDate + @"' and '" + Todate + @"'
                                and ShiftSystemID in (" + sft + @")
                                and LeaveDuration >0 and a.IsLWP=0
                                )  ";
                }
                if (LvWOP == true)
                {
                    sql = sql + @"and e.SystemId not in (
                                    select distinct EmpSystemID from AttdnProcessData a where
                                    WorkDate between '" + FromDate + @"' and '" + Todate + @"'
                                    and ShiftSystemID in (" + sft + @")
                                    and LeaveDuration >0 and IsLWP=1
                                    )";
                }
                string _sql = @"select e.EmployeeCode,e.EmployeeName,l.UserName LegalDesignation,
                                FORMAT( e.DOJ,'dd-MMM-yyyy') DOJ,e.EmployeeStatus, U.UserName Unit
                                ,Dv.UserName Division
                                ,Dp.UserName Department
                                ,S.UserName Section
                                ,SB.UserName SubSection,
                                ec.UserName EmployeeCategory
                                from EmployeeInformation e
 LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                left join HKP.LegalDesignation l on l.Id = e.LegalDesignationId
                                LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON PR.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dp ON PR.DepartmentID = Dp.Id
                                LEFT JOIN ORG.Section S ON PR.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SB ON PR.SubSectionID = SB.Id
                                left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                where e.SystemId in ( select distinct EmpSystemID from AttdnProcessData a where
                                a.WorkDate between '" + FromDate+@"' and '"+Todate+@"'
                                and a.ShiftSystemID in  ("+ sft +@")
                                and e.PlantId = '"+PlantId+@"'
                                and e.SystemID in
                                (
                                select systemid from EmployeeInformation e
                                left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
								--where ec.UserName = 'Worker'
                                ) 
								)" + sql + "";


                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
