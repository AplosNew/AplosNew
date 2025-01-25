using Aplos.Controllers;
using Aplos.Properties;
using ConnectionManager.DAL;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class EmployeeServiceVariableController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private clsReport objRpt;

        public EmployeeServiceVariableController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        [HttpGet, Authorize]
        public JsonResult GetEmpServiceTypeCbo()
        {
            return Json(_sqlRepository.GetDataCollection(@"SELECT Id as Value,[Service] Text FROM dbo.EmpServiceType"), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmployeeServiceFixedReport(ReportFormat reportFormat, string FromDate, string ToDate, string Service)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Employee Service Variable";
            var workbook = GetReportWorkSheet(FromDate, ToDate, Service);
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

        private IWorkbook GetReportWorkSheet(string FromDate, string ToDate, string Service)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 2);
            workbook.Version = ExcelVersion.Excel2016;
            string FactoryName = string.Empty;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            string CmpName = "";
            string FactoryAddress = string.Empty;
            objRpt = new clsReport();
            objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
            objRpt.SelectedPlant(identity.PlantId, out dsFactory);
            var sheet = workbook.Worksheets[0];

            sheet.Name = "EmployeeServiceVariable";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetData(FromDate, ToDate, Service);

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 13, ExcelHAlign.HAlignLeft);
            int ColEmpId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 25, ExcelHAlign.HAlignLeft);
            int ColEmpName = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 15, ExcelHAlign.HAlignLeft);
            int ColEmpEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Depertment", 15, ExcelHAlign.HAlignLeft);
            int ColEmpDepertment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 15, ExcelHAlign.HAlignLeft);
            int ColDesignation = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Service Name", 15, ExcelHAlign.HAlignLeft);
            int ColServiceName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Service Category", 15, ExcelHAlign.HAlignLeft);
            int ColServiceCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "UOM", 15, ExcelHAlign.HAlignLeft);
            int ColUOM = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "From Time", 15, ExcelHAlign.HAlignRight);
            int ColFrom = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "To Time", 15, ExcelHAlign.HAlignRight);
            int ColTo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Quantity", 15, ExcelHAlign.HAlignRight);
            int ColQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 15, ExcelHAlign.HAlignLeft);
            int ColCurrency = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate", 15, ExcelHAlign.HAlignRight);
            int ColRate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Amount", 15, ExcelHAlign.HAlignRight);
            int ColAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Final Amount", 15, ExcelHAlign.HAlignRight);
            int ColFinalAmount = COL;
            COL++;
            
            report.SetHeaderText(ref sheet, ROW, COL, "Chargable", 15, ExcelHAlign.HAlignLeft);
            int ColChargable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "NonChargable", 15, ExcelHAlign.HAlignLeft);
            int ColNonChargable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 15, ExcelHAlign.HAlignLeft);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Added By", 15, ExcelHAlign.HAlignLeft);
            int ColAddedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Time", 15, ExcelHAlign.HAlignLeft);
            int ColAddedDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 15, ExcelHAlign.HAlignLeft);
            int ColActualDate = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Size = 9f;
            sheet.Range[ROW, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);

            endCol = COL;
            sheet.AutoFilters.FilterRange = sheet.Range[ROW - 1, 1, ROW, endCol];
            #endregion Headers

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColEmpId].Text = data.Rows[i]["EmpId"].ToString();
                sheet[ROW, ColEmpName].Text = data.Rows[i]["EmpName"].ToString();
                sheet[ROW, ColEmpEntity].Text = data.Rows[i]["EmpEntity"].ToString();
                sheet[ROW, ColEmpDepertment].Text = data.Rows[i]["EmpDepertment"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColServiceName].Text = data.Rows[i]["ServiceName"].ToString();
                sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                sheet[ROW, ColFinalAmount].Number = Convert.ToDouble(data.Rows[i]["FinalAmount"].ToString());
                sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();
                sheet[ROW, ColServiceCategory].Text = data.Rows[i]["ServiceCategory"].ToString();
                sheet[ROW, ColFrom].Number = Convert.ToDouble(data.Rows[i]["From"].ToString());
                sheet[ROW, ColTo].Number = Convert.ToDouble(data.Rows[i]["To"].ToString());
                sheet[ROW, ColQty].Number = Convert.ToDouble(data.Rows[i]["Qty"].ToString());
                sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rate"].ToString());
                sheet[ROW, ColAmount].Number = Convert.ToDouble(data.Rows[i]["Amount"].ToString());
                sheet[ROW, ColChargable].Text = data.Rows[i]["Chargable"].ToString();
                sheet[ROW, ColNonChargable].Text = data.Rows[i]["NonChargable"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                sheet[ROW, ColAddedDate].Text = data.Rows[i]["Time"].ToString();
                sheet[ROW, ColActualDate].Text = data.Rows[i]["Date"].ToString();
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;
            }

            #region Sheet2
            
            // Linq Getting the Other DataTable
            var newDt = data.AsEnumerable()
              .GroupBy(r => new { 
                  V = r["EmpId"],
                  J = r["EmpName"],
                  K = r["ServiceName"],
              })
              .Select(g =>
              {
                  var row = data.NewRow();

                  row["EmpId"] = g.Key.V;
                  row["EmpName"] = g.Key.J;
                  row["ServiceName"] = g.Key.K;
                  row["Amount"] = g.Sum(r => Convert.ToDouble(r["FinalAmount"]));

                  return row;
              }).CopyToDataTable();

            DataTable ddt = newDt;

            
            var sheetA = workbook.Worksheets[1];

            sheetA.Name = "EmployeeServiceVariableGroup";

            int COLA = 1;
            int ROWA = 1;
            int endColA = 1;
            report.SetHeaderText(ref sheetA, ROWA, COLA, "Employee Id", 12, ExcelHAlign.HAlignLeft);
            int EmpId = COLA;
            COLA++;

            report.SetHeaderText(ref sheetA, ROWA, COLA, "Employee Name", 25, ExcelHAlign.HAlignLeft);
            int EmpName = COLA;
            COLA++;


            report.SetHeaderText(ref sheetA, ROWA, COLA, "Service Name", 15, ExcelHAlign.HAlignLeft);
            int SvcName = COLA;
            COLA++;

            report.SetHeaderText(ref sheetA, ROWA, COLA, "Amount", 15, ExcelHAlign.HAlignLeft);
            int FinalAmount = COLA;
            COLA++;

            endColA = COLA - 1;

            sheetA.Range[ROWA, 1, ROWA, COLA - 1].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            sheetA.Range[ROWA, 1, ROWA, COLA - 1].BorderAround(ExcelLineStyle.Hair);
            sheetA.Range[ROWA, 1, ROWA, COLA - 1].BorderInside(ExcelLineStyle.Hair);
            sheetA.Range[ROWA, 1, ROWA, COLA - 1].CellStyle.Font.Bold = true;

            ROWA++;
            for (int i = 0; i < newDt.Rows.Count; i++)
            {
                sheetA[ROWA, ColEmpId].Text = newDt.Rows[i]["EmpId"].ToString();
                sheetA[ROWA, ColEmpName].Text = newDt.Rows[i]["EmpName"].ToString();
                sheetA[ROWA, ColEmpEntity].Text = newDt.Rows[i]["ServiceName"].ToString();
                sheetA[ROWA, ColEmpDepertment].Number = clsStaticInfo.dbl(newDt.Rows[i]["Amount"]); 
                
                ROWA++;
            }


            #endregion Sheet2

            #region Line Setup

            sheet.Range[6, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[6, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[6, 1, ROW - 1, endCol].WrapText = true;

            sheetA.Range[1, 1, ROWA - 1, endColA].BorderInside(ExcelLineStyle.Hair);
            sheetA.Range[1, 1, ROWA - 1, endColA].BorderAround(ExcelLineStyle.Hair);
            sheetA.Range[1, 1, ROWA - 1, endColA].WrapText = true;
            #endregion Line Setup

            #region ******************Report Header******************
            try
            {
                string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), identity.CompanyId + ".jpg");  // IDCardEng.xlsx
                Image companyLogo = Image.FromFile(strPath);
                if (companyLogo != null)
                {
                    double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(2);
                    int totalWidthPixel = (int)(totalWidth * 7.25);
                    int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(3) + sheet.GetRowHeight(3)) * 1.50);

                    companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                    IPictureShape pic = null;

                    pic = sheet.Pictures.AddPicture(1, 1, companyLogo);

                }


            }
            catch (Exception)
            {


            }

            ROW = 1;
            COL = 1;

            //string FactoryName = string.Empty;
            //string CmpName = "";
            //string FactoryAddress = string.Empty;
            //int SheetIndex = 0;
            if (dsCmp.Tables[0].Rows.Count > 0)
            {
                CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
            }
            else
            {
                CmpName = "";
            }
            sheet.Range[ROW, 3].Text = CmpName;
            sheet.Range[ROW, 3, COL, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 3].CellStyle.Font.Size = 12;
            sheet.Range[ROW, 3, COL, endCol].RowHeight = 20;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, COL, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            ROW += 1;
            if (dsFactory.Tables[0].Rows.Count > 0)
            {

                FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
            }
            else
            {
                FactoryName = "";
            }
            sheet.Range[ROW, 3].Text = FactoryName;
            sheet.Range[ROW, 3, ROW, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Size = 10;
            sheet.Range[ROW, 3, ROW, endCol].RowHeight = 20;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            ROW += 1;
            if (dsFactory.Tables[0].Rows.Count > 0)
            {
                FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
            }
            else
            {
                FactoryAddress = "";
            }
            sheet.Range[ROW, 3].Text = FactoryAddress;
            sheet.Range[ROW, 3, ROW, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Size = 22;
            sheet.Range[ROW, 3, ROW, endCol].RowHeight = 17;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            ROW += 1;
            sheet.Range[ROW, 3].Text = "Employee Service Variable: " + FromDate + " To " + ToDate;
            sheet.Range[ROW, 3, ROW, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Size = 10;
            sheet.Range[ROW, 3, ROW, endCol].RowHeight = 20;
            sheet.Range[ROW, 3].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            #endregion ******************Report Header******************

            #region UsedRange Alignment

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.Range["A1"].CellStyle.Font.Size = 14;
            sheet.Range["A2"].CellStyle.Font.Size = 10;
            sheet.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

            #endregion UsedRange Alignment

            #region Page Setup
            sheet.PageSetup.TopMargin = 0.5;
            sheet.PageSetup.BottomMargin = 0.7;
            sheet.PageSetup.PrintTitleRows = "$1:$5";
            sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
            sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
            sheet.PageSetup.LeftMargin = 0.5;
            sheet.PageSetup.RightMargin = 0.2;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            sheet.PageSetup.FitToPagesTall = 0;
            sheet.PageSetup.FitToPagesWide = 1;
            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.IsDisplayZeros = false;

            sheetA.PageSetup.TopMargin = 0.5;
            sheetA.PageSetup.BottomMargin = 0.7;
            sheetA.PageSetup.PrintTitleRows = "$1:$5";
            sheetA.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
            sheetA.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
            sheetA.PageSetup.LeftMargin = 0.5;
            sheetA.PageSetup.RightMargin = 0.2;
            sheetA.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            sheetA.PageSetup.FitToPagesTall = 0;
            sheetA.PageSetup.FitToPagesWide = 1;
            sheetA.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheetA.IsDisplayZeros = false;
            #endregion Page Setup


            #region Freeze Panes

            sheet.IsDisplayZeros = false;
            sheet.UsedRange["A7"].FreezePanes();
            sheet.FirstVisibleColumn = 1;
            sheet.FirstVisibleRow = 6;

            #endregion Freeze Panes
          
            return workbook;
        }

        private DataTable GetData(string FromDate, string ToDate, string Service)
        {
            try
            {
                string svc = "";
                if(bplib.clsWebLib.RetValidLen(Service).ToString() != "null")
                {
                    svc = "AND est.Id = '"+ Service + "'";
                }

                string sql = @"select A.* from (select ei.EmployeeCode EmpId,ei.EmployeeName EmpName,e.UserName EmpEntity,d.UserName EmpDepertment,ld.UserName Designation,
                                est.Service ServiceName,esc.Category ServiceCategory,uom.UserName UOM, ISNULL(esd.[From],0)[From] , ISNULL(esd.[To],0)[To],
								ISNULL(esd.Quantity,0) Qty,cu.Code Currency,ISNULL(esr.Rate,0)Rate ,ISNULL(esd.Amount,0)Amount,  
								 FinalAmount=case when ISNULL( esd.Amount,0) =0 then (isnull(esd.Quantity,0)* isnull(esr.Rate,0)) 
								 else ISNULL( esd.Amount,0) end
                                , case when esd.Chargeable = '1' then 'Yes' else '' end Chargable
                                ,case when esd.Chargeable = '0' then 'Yes' else '' end NonChargable,esr.Remarks, esd.AddedBy ,
								
								CONVERT(varchar(5),esd.[Time],108) Time, FORMAT(esd.Date, 'dd-MMM-yyyy') as Date
                                from [dbo].[EmpServiceData] esd
                                left join EmployeeInformation ei on ei.SystemId = esd.EmployeeId
                                left join mst.ManpowerBudget mb on mb.Id = ei.BudgetCode
                                LEFT JOIN ORG.Position PO ON MB.PositionId=PO.Id
                                left join org.Entity e on e.Id = mb.EntityId
                                left join ORG.Department d on d.Id = PO.DepartmentId
                                left join HKP.LegalDesignation ld on ld.Id = ei.LegalDesignationId
                                left join [dbo].[EmpServiceCategory] esc on esc.Id = esd.EmployeeServiceCategoryId
                                left join [dbo].[EmpServiceType] est on est.Id= esc.EmpServiceTypeId
								left join CurrencyRuleChild c on c.SalaryHeadID=est.SalaryHeadId 
								inner join CurrencyRuleMaster cm on cm.SystemID=c.MstSystemID 
								inner join SalaryRuleMaster sm on sm.CurrencyRuleSystemID=cm.SystemID and sm.SystemID=ei.SalaryRuleMasterSystemID
								left join SCS.Currency cu on cu.Id = c.AmtDefinitionCurrency
                                left join scs.UnitOfMeasurement uom on uom.Id =  est.UOMId
                                left join [dbo].[EmployeeServicesRate] esr on esr.EmployeeServiceCategoryId = esd.EmployeeServiceCategoryId
								AND esr.Id=(Select top(1) Id From [dbo].[EmployeeServicesRate] Where  EmployeeServiceCategoryId= esr.EmployeeServiceCategoryId AND EffectiveDate<='" + FromDate + @"' Order By EffectiveDate desc)
                                where esd.Date between '" + FromDate + "' and '" + ToDate + "' " + svc + @"
UNION
select ei.EmployeeCode EmpId, ei.EmployeeName EmpName, e.UserName EmpEntity, d.UserName EmpDepertment, ld.UserName Designation,
                                    est.Service ServiceName, esc.Category ServiceCategory, uom.UserName UOM, ISNULL(esd.[From],0)[From] , ISNULL(esd.[To],0)[To],
								ISNULL(esd.Quantity,0) Qty, cu.Code Currency, ISNULL(esr.Rate,0)Rate ,ISNULL(esd.Amount,0)Amount, 
								 FinalAmount =case when ISNULL(esd.Amount,0) = 0 then(isnull(esd.Quantity, 0) * isnull(esr.Rate, 0))

                                 else ISNULL(esd.Amount, 0) end
                                , case when esd.Chargeable = '1' then 'Yes' else '' end Chargable
                                ,case when esd.Chargeable = '0' then 'Yes' else '' end NonChargable, esr.Remarks, esd.AddedBy ,
								
								CONVERT(varchar(5), esd.[Time], 108) Time, FORMAT(esd.Date, 'dd-MMM-yyyy') as Date
                                from[dbo].[EmpServiceData] esd
                               left join EmployeeInformation ei on ei.SystemId = esd.EmployeeId
                                left join mst.ManpowerBudget mb on mb.Id = ei.BudgetCode
                                LEFT JOIN ORG.Position PO ON MB.PositionId=PO.Id
                                left join org.Entity e on e.Id = mb.EntityId
                                left join ORG.Department d on d.Id = PO.DepartmentId
                                left join HKP.LegalDesignation ld on ld.Id = ei.LegalDesignationId
                                left join[dbo].[EmpServiceCategory] esc on esc.Id = esd.EmployeeServiceCategoryId
                                left join[dbo].[EmpServiceType] est on est.Id = esc.EmpServiceTypeId
                                left join CurrencyRuleChild c on c.SalaryHeadID = est.SalaryHeadId
                                inner join CurrencyRuleMaster cm on cm.SystemID = c.MstSystemID
                                inner join SalaryRuleMaster sm on sm.CurrencyRuleSystemID = cm.SystemID
                                left join SCS.Currency cu on cu.Id = c.AmtDefinitionCurrency
                                left join scs.UnitOfMeasurement uom on uom.Id = est.UOMId
                                left join [dbo].[EmployeeServicesRate] esr on esr.EmployeeServiceCategoryId = esd.EmployeeServiceCategoryId
								AND esr.Id=(Select top(1) Id From [dbo].[EmployeeServicesRate] Where  EmployeeServiceCategoryId= esr.EmployeeServiceCategoryId AND EffectiveDate<='" + FromDate + @"' Order By EffectiveDate desc)
                                where esd.Date between '" + FromDate + "' and '" + ToDate + "' "+svc+ ")A Order BY A.EmpId,A.Date";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}