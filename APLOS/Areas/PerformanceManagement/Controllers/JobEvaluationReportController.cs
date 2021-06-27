#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

using Library.Model.OrderManagements;
using Library.Service.PerformanceManagement;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.OrderManagement.OrderControl;
using System.IO;
using Library.Data;
using Library.Service.Helpers;


#endregion Using

namespace Aplos.Areas.PerformanceManagement.Controllers
{
    public class JobEvaluationReportController : BaseController
    {
        JobEvaluationReportService JER = new JobEvaluationReportService();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public JobEvaluationReportController(ISqlRepository R)
        {
            _sqlRepository = R;
            JER = new JobEvaluationReportService();
    
        }


        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllPositionDetailsForSelection(string Id)
        {
            try
            {
                var jsondata = Json(JER.LoadAllPositionDetailsForSelection(Id), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllEvaluatorDetails(string Id)
        {
            try
            {
                var jsondata = Json(JER.LoadAllEvaluatorDetails(Id), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetSearchedDetails(string PositionCodeId, string EmpSystemId)
        {
            try
            {
                var jsondata = Json(JER.GetSearchedDetails(PositionCodeId, EmpSystemId), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetJobEvaluationReport(string PositionCodeId, string DivisionId, string SubDivisionId, string DepartmentId, string SectionId, string SubSectionId, string DesignationId)
        {
            try
            {
                var workbook = GetData(PositionCodeId, DivisionId, SubDivisionId, DepartmentId, SectionId, SubSectionId, DesignationId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "JobEvaluation.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private IWorkbook GetData(string PositionCodeId, string DivisionId, string SubDivisionId, string DepartmentId, string SectionId, string SubSectionId, string DesignationId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "JobEvaluation";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = JER.GetReportData(PositionCodeId, DivisionId, SubDivisionId, DepartmentId, SectionId, SubSectionId, DesignationId);

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Position Code", 8, ExcelHAlign.HAlignLeft);
            int ColPositionCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Position Name", 12, ExcelHAlign.HAlignLeft);
            int ColPositionName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Division", 12, ExcelHAlign.HAlignLeft);
            int ColDivision = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SubDivision", 12, ExcelHAlign.HAlignLeft);
            int ColSubDivision = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department Name", 15, ExcelHAlign.HAlignLeft);
            int ColDepartmentName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 10, ExcelHAlign.HAlignLeft);
            int ColSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 12, ExcelHAlign.HAlignLeft);
            int ColSubSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignLeft);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Performance Attribute", 15, ExcelHAlign.HAlignLeft);
            int ColPerformanceAttribute = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "JE Code", 15, ExcelHAlign.HAlignLeft);
            int ColJECode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Category", 15, ExcelHAlign.HAlignLeft);
            int ColJEMCCCategory = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Criteria", 10, ExcelHAlign.HAlignLeft);
            int ColJEMCCCriteria = COL;
            COL++;

          

            report.SetHeaderText(ref sheet, ROW, COL, "Dimension1ControlName", 10, ExcelHAlign.HAlignLeft);
            int ColDimension1ControlName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Dimension1ControlLevel", 10, ExcelHAlign.HAlignLeft);
            int ColDimension1ControlLevel = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Dimension2ControlName", 15, ExcelHAlign.HAlignLeft);
            int ColDimension2ControlName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Dimension2ControlLevel", 10, ExcelHAlign.HAlignLeft);
            int ColDimension2ControlLevel = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Points", 10, ExcelHAlign.HAlignLeft);
            int ColJEMCCPoints = COL;
            COL++;       

            report.SetHeaderText(ref sheet, ROW, COL, "Factoring", 10, ExcelHAlign.HAlignLeft);
            int ColFactoring = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "JE Points", 15, ExcelHAlign.HAlignLeft);
            int ColJEPoints = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 10, ExcelHAlign.HAlignLeft);
            int ColJECRemarks = COL;
            ROW++;


            endCol = COL;
            #endregion Headers

            string PosCode = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (PosCode != data.Rows[i]["PositionCode"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                        sheet.Range[RowIndex, ColPositionCode, ROW - 1, ColPositionCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColPositionCode, ROW - 1, ColPositionCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    }
                    RowIndex = ROW;
                }

                sheet[ROW, ColPositionCode].Text = data.Rows[i]["PositionCode"].ToString();
                sheet[ROW, ColPositionName].Text = data.Rows[i]["PositionName"].ToString();
                sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                sheet[ROW, ColDepartmentName].Text = data.Rows[i]["Department"].ToString();

                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();

                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColSubDivision].Text = data.Rows[i]["SubDivision"].ToString();
                sheet[ROW, ColPerformanceAttribute].Text = data.Rows[i]["PerformanceAttribute"].ToString();
                sheet[ROW, ColJECode].Text = data.Rows[i]["JECode"].ToString();
                sheet[ROW, ColJEMCCCategory].Text = data.Rows[i]["JEMCCCategory"].ToString();
                sheet[ROW, ColJEMCCCriteria].Text = data.Rows[i]["JEMCCCriteria"].ToString();

                sheet[ROW, ColJEMCCPoints].Number = clsStaticInfo.dbl( data.Rows[i]["JEMCCPoints"].ToString());

                sheet[ROW, ColDimension1ControlName].Text = data.Rows[i]["Dimension1ControlName"].ToString();
                sheet[ROW, ColDimension1ControlLevel].Text = data.Rows[i]["Dimension1ControlLevel"].ToString();
                sheet[ROW, ColDimension2ControlName].Text = data.Rows[i]["Dimension2ControlName"].ToString();
                sheet[ROW, ColDimension2ControlLevel].Text = data.Rows[i]["Dimension2ControlLevel"].ToString();
                sheet[ROW, ColJEPoints].Number = clsStaticInfo.dbl(data.Rows[i]["JEPoints"].ToString());
                sheet[ROW, ColFactoring].Number = clsStaticInfo.dbl(data.Rows[i]["Factoring"].ToString());
                sheet[ROW, ColJECRemarks].Text = data.Rows[i]["JECRemarks"].ToString();


                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                PosCode = data.Rows[i]["PositionCode"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                sheet.Range[RowIndex, ColPositionCode, ROW - 1, ColPositionCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColPositionCode, ROW - 1, ColPositionCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColPositionName, ROW - 1, ColPositionName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColPositionName, ROW - 1, ColPositionName].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDivision, ROW - 1, ColDivision].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDivision, ROW - 1, ColDivision].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDepartmentName, ROW - 1, ColDepartmentName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDepartmentName, ROW - 1, ColDepartmentName].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColSection, ROW - 1, ColSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColSection, ROW - 1, ColSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColSubSection, ROW - 1, ColSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColSubSection, ROW - 1, ColSubSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDesignation, ROW - 1, ColDesignation].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDesignation, ROW - 1, ColDesignation].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColSubDivision, ROW - 1, ColSubDivision].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColSubDivision, ROW - 1, ColSubDivision].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColPerformanceAttribute, ROW - 1, ColPerformanceAttribute].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColPerformanceAttribute, ROW - 1, ColPerformanceAttribute].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJECode, ROW - 1, ColJECode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJECode, ROW - 1, ColJECode].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJEMCCCategory, ROW - 1, ColJEMCCCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJEMCCCategory, ROW - 1, ColJEMCCCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJEMCCCriteria, ROW - 1, ColJEMCCCriteria].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJEMCCCriteria, ROW - 1, ColJEMCCCriteria].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJEMCCPoints, ROW - 1, ColJEMCCPoints].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJEMCCPoints, ROW - 1, ColJEMCCPoints].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDimension1ControlName, ROW - 1, ColDimension1ControlName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDimension1ControlName, ROW - 1, ColDimension1ControlName].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDimension1ControlLevel, ROW - 1, ColDimension1ControlLevel].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDimension1ControlLevel, ROW - 1, ColDimension1ControlLevel].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDimension2ControlName, ROW - 1, ColDimension2ControlName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDimension2ControlName, ROW - 1, ColDimension2ControlName].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDimension2ControlLevel, ROW - 1, ColDimension2ControlLevel].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDimension2ControlLevel, ROW - 1, ColDimension2ControlLevel].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJEPoints, ROW - 1, ColJEPoints].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJEPoints, ROW - 1, ColJEPoints].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColFactoring, ROW - 1, ColFactoring].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColFactoring, ROW - 1, ColFactoring].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColJECRemarks, ROW - 1, ColJECRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJECRemarks, ROW - 1, ColJECRemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;


            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, endCol, "Job Evaluation", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }



    }
}