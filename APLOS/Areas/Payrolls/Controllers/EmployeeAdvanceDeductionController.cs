using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Payrolls.Setting;
using static Library.Service.Payrolls.Setting.clsCurrencyRule;
using Library.HumanResource.Payroll.Setting;
using Library.HumanResource.Payroll.Tax;
using System.Reflection;
using Library.Service.Logs;
using Library.Service.Enums;
using Library.HumanResource.Payroll;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class EmployeeAdvanceDeductionController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeProfileService _employeeProfileService;

        public EmployeeAdvanceDeductionController(ISqlRepository R, IEmployeeProfileService employeeProfileService)
        {
            _sqlRepository = R;
            _employeeProfileService = employeeProfileService;
        }

        #endregion Constructor

        #region View

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region -- Get --

        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadListeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SalaryHeadID as Id,SalaryHead+' ['+HeadType+']' as UserName 
                            from [dbo].[SalaryHead]  WHERE ExtDataUpload=1 and HeadCategory='Advance'
                            ORDER BY HeadType DESC,SalaryHead";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalaryInterest()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SalaryHeadID as Id,SalaryHead+' ['+HeadType+']' as UserName 
                            from [dbo].[SalaryHead]  WHERE HeadCategory='Interest Deduction'
                            ORDER BY HeadType DESC,SalaryHead";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetSalaryAdvance(string Year, string Month)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetEmployeeList(identity.PlantId, identity.CompanyId, Year, Month), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetEmployeeList(string plantId, string companyId, string Year, string Month)
        {
            try
            {
                string CmdText = @"select IsSelected = case when ead.EmployeeId is null then Convert(bit, 'False') ELSE Convert(bit, 'True') END,ead.Id,ars.YearNo,ars.MonthNo,EMAD.EmpSystemId EmployeeId,e.EmployeeCode,e.EmployeeName,
                                    EMAD.AdvanceAmount SanctionedAmount,'' AdvanceId,ars.Id AdvanceReqScheduleId,ead.EmployeeSalaryAdvanceId
                                    ,isnull(Recovered.RecoveredAmount,0) RecoveredAmount, EMAD.AdvanceAmount -isnull(Recovered.RecoveredAmount,0) Balance,
                                    ars.InstallmentAmount CurrentInstallment ,ars.PrincipalAmount,ars.ProfitAmount InterestAmount,v.VoucherNo,ars.EmployeeAdvanceDetailId
                                    from trn.EmployeeAdvanceDetail EMAD
									left join  trn.EmployeeAdvance esa  ON EMAD.EmployeeAdvanceId=esa.Id
                                    left join trn.voucher v on v.Id=EMAD.VoucherId
                                    join dbo.AdvanceReqSchedule ars on ars.EmployeeAdvanceDetailId=EMAD.Id
                                    left join EmployeeInformation e on e.SystemId=EMAD.EmpSystemId
                                    left join [TRN].[EmployeeAdvanceDeduction] ead on ead.EmployeeId = EMAD.EmpSystemId AND EMAD.Id=ead.EmployeeAdvanceDetailId AND ead.YearNo='" + Year + "' and ead.MonthNo='" + Month + @"'
                                    left join (select ead.EmployeeSalaryAdvanceId,SUM(ars.InstallmentAmount) RecoveredAmount 
									from trn.EmployeeSalaryAdvance esa 
													left join dbo.AdvanceReqSchedule ars ON ars.EmployeeSalaryAdvanceId=esa.Id
													 join  [TRN].[EmployeeAdvanceDeduction] ead   on esa.Id=ead.EmployeeSalaryAdvanceId
													WHERE ars.YearNo='" + Year + "' and ars.MonthNo='" + Month + @"' group by ead.EmployeeSalaryAdvanceId ) Recovered on Recovered.EmployeeSalaryAdvanceId = esa.Id
													
                                    WHERE ars.YearNo='" + Year + "' and ars.MonthNo='" + Month + @"' and EMAD.EmpSystemId<>'' and esa.PlantId='" + plantId + @"'
									UNION ALL
                                    select IsSelected = case when ead.EmployeeId is null then Convert(bit, 'False') ELSE Convert(bit, 'True') END,ead.Id,ars.YearNo,ars.MonthNo,esa.EmployeeId,e.EmployeeCode,e.EmployeeName,
                                    esa.Amount SanctionedAmount,a.Id AdvanceId,ars.Id AdvanceReqScheduleId,ars.EmployeeSalaryAdvanceId
                                    ,isnull(Recovered.RecoveredAmount,0) RecoveredAmount, esa.Amount-isnull(Recovered.RecoveredAmount,0) Balance,
                                    ars.InstallmentAmount CurrentInstallment ,ars.PrincipalAmount,ars.ProfitAmount InterestAmount,v.VoucherNo,'' EmployeeAdvanceDetailId
                                    from trn.EmployeeSalaryAdvance esa
                                    left join trn.voucher v on v.Id=esa.VoucherId
                                    left join  trn.Advance a  on esa.VoucherId=a.VoucherId
                                    join dbo.AdvanceReqSchedule ars on ars.EmployeeSalaryAdvanceId=esa.Id
                                    left join EmployeeInformation e on e.SystemId=esa.EmployeeId
                                    left join [TRN].[EmployeeAdvanceDeduction] ead on ead.EmployeeId = esa.EmployeeId AND esa.Id=ead.EmployeeSalaryAdvanceId AND ead.YearNo='" + Year + "' and ead.MonthNo='" + Month + @"'
                                    left join (select ead.EmployeeSalaryAdvanceId,SUM(ars.InstallmentAmount) RecoveredAmount 
									from trn.EmployeeSalaryAdvance esa 
													left join dbo.AdvanceReqSchedule ars ON ars.EmployeeSalaryAdvanceId=esa.Id
													 join  [TRN].[EmployeeAdvanceDeduction] ead   on esa.Id=ead.EmployeeSalaryAdvanceId
													WHERE ars.YearNo='" + Year + "' and ars.MonthNo='"+ Month + @"' group by ead.EmployeeSalaryAdvanceId ) Recovered on Recovered.EmployeeSalaryAdvanceId = esa.Id
													
                                    WHERE ars.YearNo='" + Year + "' and ars.MonthNo='" + Month + @"' and esa.EmployeeId<>'' and esa.PlantId='"+ plantId + @"'
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
        [HttpPost, Authorize]
        public ActionResult GetGeneralAdvance(string Year, string Month)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetEmployeeListG(identity.PlantId, identity.CompanyId, Year, Month), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetEmployeeListG(string plantId, string companyId, string Year, string Month)
        {
            try
            {
                string CmdText = @"select IsSelected = case when ead.EmployeeId is null then Convert(bit, 'False') ELSE Convert(bit, 'True') END,
                                    ars.YearNo,ars.MonthNo,a.EmployeeId,e.EmployeeCode,e.EmployeeName,
                                    a.Amount SanctionedAmount,a.Id AdvanceId,ars.Id AdvanceReqScheduleId,
                                    a.WrittenOffAmount RecoveredAmount,a.Amount-a.WrittenOffAmount Balance,
                                    ars.InstallmentAmount CurrentInstallment ,ars.PrincipalAmount,ars.ProfitAmount InterestAmount
                                    from trn.Advance a 
                                    left join trn.EmployeeSalaryAdvance esa on esa.VoucherId=a.VoucherId
                                    left join dbo.AdvanceReqSchedule ars on ars.EmployeeSalaryAdvanceId=esa.Id
                                    left join EmployeeInformation e on e.SystemId=a.EmployeeId and e.SystemId=esa.EmployeeId
                                    left join [TRN].[EmployeeAdvanceDeduction] ead on ead.EmployeeId = a.EmployeeId and ead.AdvanceId = a.Id and ead.YearNo='" + Year + @"' and ead.MonthNo='" + Month + @"'
                                    where ars.YearNo='" + Year + "' and ars.MonthNo='" + Month + "' and a.EmployeeId<>'' and a.PlantId='" + plantId + @"'
                                    and not a.JournalType = 'Salary' order by a.EmployeeId";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        [HttpGet, Authorize]
        public ActionResult EmployeeAdvanceDeductionReportExcelFormat(ReportFormat reportFormat,string Year,string Month,string MonthName)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Employee Advance Deduction";
            var workbook = GetEmployeeAdvanceDeductionReportWorkSheet(identity.PlantId, identity.CompanyId, Year, Month, MonthName);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetEmployeeAdvanceDeductionReportWorkSheet(string plantId, string companyId, string Year, string Month,string MonthName)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "EmployeeAdvanceDeduction";
           

            int colYear = 1;

            report.SetMasterHeaderText(ref sheet, 4, colYear, "Year");
            //sheet[4, colYear].ColumnWidth = 18;
            sheet.Range[4, colYear].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[4, colYear].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            int colYearValue = 2;
            report.SetText(ref sheet, 4, colYearValue, Year);
            //sheet[4, colYearValue].ColumnWidth = 12;
            sheet.Range[4, colYearValue].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colMonth = 3;
            report.SetMasterHeaderText(ref sheet, 4, colMonth, "Month");
            //sheet[4, colMonth].ColumnWidth = 18;
            sheet.Range[4, colMonth].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[4, colMonth].HorizontalAlignment = ExcelHAlign.HAlignLeft;


            int colMonthValue = 4;
            report.SetText(ref sheet, 4, colMonthValue, MonthName);
            //sheet[4, colMonthValue].ColumnWidth = 12;
            sheet.Range[4, colMonthValue].VerticalAlignment = ExcelVAlign.VAlignTop;

            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            

            DataTable data = EmployeeAdvanceDeductionList(plantId, companyId, Year, Month);

            
            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 10, ExcelHAlign.HAlignLeft);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 25, ExcelHAlign.HAlignLeft);
            int ColEmployeeName = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Sanctioned", 12, ExcelHAlign.HAlignRight);
            int ColSanctionedAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Recovered", 12, ExcelHAlign.HAlignRight);
            int ColRecoveredAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Current Installment", 12, ExcelHAlign.HAlignRight);
            int ColCurrentInstallment = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Principal", 12, ExcelHAlign.HAlignRight);
            int ColPrincipalAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Interest", 12, ExcelHAlign.HAlignRight);
            int ColInterestAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Balance", 12, ExcelHAlign.HAlignRight);
            int ColBalance = COL;
            //COL++;


            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                
                sheet[ROW, ColSanctionedAmount].Number = clsStaticInfo.dbl(data.Rows[i]["SanctionedAmount"].ToString());
                sheet[ROW, ColSanctionedAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColSanctionedAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColSanctionedAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColRecoveredAmount].Number = clsStaticInfo.dbl(data.Rows[i]["RecoveredAmount"].ToString());
                sheet[ROW, ColSanctionedAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColSanctionedAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColSanctionedAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColCurrentInstallment].Number = clsStaticInfo.dbl(data.Rows[i]["CurrentInstallment"].ToString());
                sheet[ROW, ColCurrentInstallment].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColCurrentInstallment].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColCurrentInstallment].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColPrincipalAmount].Number = clsStaticInfo.dbl(data.Rows[i]["PrincipalAmount"].ToString());
                sheet[ROW, ColPrincipalAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColPrincipalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColPrincipalAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColInterestAmount].Number = clsStaticInfo.dbl(data.Rows[i]["InterestAmount"].ToString());
                sheet[ROW, ColInterestAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColInterestAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColInterestAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
                sheet[ROW, ColBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.00";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyHeader(ref sheet, endCol, "Employee Advance Deduction", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }


		public DataTable EmployeeAdvanceDeductionList(string plantId, string companyId, string Year, string Month)
		{
            try
            {

                string strSQL = string.Empty;
                strSQL = @"select IsSelected = case when ead.EmployeeId is null then Convert(bit, 'False') ELSE Convert(bit, 'True') END,ars.YearNo,ars.MonthNo,a.EmployeeId,e.EmployeeCode,e.EmployeeName,
                                    isnull(a.Amount,0) SanctionedAmount,a.Id AdvanceId,ars.Id AdvanceReqScheduleId,
                                    isnull(a.WrittenOffAmount,0) RecoveredAmount,isnull(a.Amount-a.WrittenOffAmount,0) Balance,
                                    isnull(ars.InstallmentAmount,0) CurrentInstallment ,isnull(ars.PrincipalAmount,0) PrincipalAmount
                                    ,isnull(ars.ProfitAmount,0) InterestAmount
                                    from trn.Advance a 
                                    left join trn.EmployeeSalaryAdvance esa on esa.VoucherId=a.VoucherId
                                    left join dbo.AdvanceReqSchedule ars on ars.EmployeeSalaryAdvanceId=esa.Id
                                    left join EmployeeInformation e on e.SystemId=a.EmployeeId and e.SystemId=esa.EmployeeId
                                    left join [TRN].[EmployeeAdvanceDeduction] ead on ead.EmployeeId = a.EmployeeId and ead.AdvanceId = a.Id and ead.YearNo='" + Year + @"' and ead.MonthNo='" + Month + @"'
                                    where ars.YearNo='" + Year + "' and ars.MonthNo='" + Month + "' and a.EmployeeId<>'' and a.PlantId='" + plantId + @"'
                                    and a.JournalType = 'Salary' order by a.EmployeeId";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
		#endregion

		#region -- Save --
		[HttpPost]
        public JsonResult SaveSalaryAdvance(List<SalaryAdvance> data, string Year, string Month, List<SalaryHeadAD> SalaryHead,string Advance, string Interest,List<SalaryAdvance> DataToBeDelete)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsEmpAdvanceDeduction ep = new clsEmpAdvanceDeduction();
                ep.SaveAdvance(data, Year, Month, SalaryHead,Advance,Interest, DataToBeDelete);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion
    }
}