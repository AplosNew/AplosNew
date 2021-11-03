#region Using

using clsAttendance;
using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using static Library.Service.Helpers.ReportUtility;

#endregion Using

namespace Library.Service.HumanResources
{
    public class SpecialFollowUPReportService : ISpecialFollowUPReportService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SpecialFollowUPReportService(

             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }
        #endregion


        public IWorkbook GetSpecialFollowUPReportSummaryExcel(string PlantId, string fromDate, string toDate)
        {

            try
            {
                
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                clsReport objRpt = null;

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtAttSummary = GetSpecialFollowUPReportSummarySql(fromDate, toDate);
                dvDaily = new DataView(dtAttSummary);

                if (dtAttSummary.Rows.Count == 0)
                {
                    throw new CustomException("No Data Found....");
                }
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 6;

                #region ColumnHeaderVariables              
                int cSrNo = 0; int cEmployeeCode = 0; int cEmployeeName = 0; int cCellPhnNo = 0; int cNationalId = 0; int cFatherName = 0; int cMotherName = 0; int cGenderID = 0; int cDOB = 0; int cEmployeeStatus = 0; int cSpecialFollowUP = 0;
                int dojEmployeeCode = 0; int dojEmployeeName = 0; int dojCellPhnNo = 0; int dojNationalId = 0; int dojFatherName = 0; int dojMotherName = 0; int dojGenderID = 0; int dojDOB = 0; int dojEmployeeStatus = 0; int dojSpecialFollowUP = 0;

                var startDosCol = 1;
                var dosEndCol = 0;
                var startDoJCol = 0;
                var doJEndCol = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sr.No", ExcelHAlign.HAlignCenter); cSrNo = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode", ExcelHAlign.HAlignCenter); cEmployeeCode = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeName", ExcelHAlign.HAlignCenter); cEmployeeName = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CellPhnNo", ExcelHAlign.HAlignCenter); cCellPhnNo = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "NationalId", ExcelHAlign.HAlignCenter); cNationalId = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FatherName", ExcelHAlign.HAlignCenter); cFatherName = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MotherName", ExcelHAlign.HAlignCenter); cMotherName = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GenderID ", ExcelHAlign.HAlignCenter); cGenderID = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOB", ExcelHAlign.HAlignCenter); cDOB = xlsCol; xlsCol++;
                //oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeStatus", ExcelHAlign.HAlignCenter); cEmployeeStatus = xlsCol; xlsCol++;
                //oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SpecialFollowUP", ExcelHAlign.HAlignCenter); cSpecialFollowUP = xlsCol; xlsCol++;

                dosEndCol = cDOB;
                startDoJCol = cDOB + 1;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode", ExcelHAlign.HAlignCenter); dojEmployeeCode = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeName", ExcelHAlign.HAlignCenter); dojEmployeeName = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CellPhnNo", ExcelHAlign.HAlignCenter); dojCellPhnNo = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "NationalId", ExcelHAlign.HAlignCenter); dojNationalId = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FatherName", ExcelHAlign.HAlignCenter); dojFatherName = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MotherName", ExcelHAlign.HAlignCenter); dojMotherName = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GenderID ", ExcelHAlign.HAlignCenter); dojGenderID = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOB", ExcelHAlign.HAlignCenter); dojDOB = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeStatus", ExcelHAlign.HAlignCenter); dojEmployeeStatus = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SpecialFollowUP", ExcelHAlign.HAlignCenter); dojSpecialFollowUP = xlsCol;
                doJEndCol = dojSpecialFollowUP;
                sheet1.Range[xlsRow - 1, startDosCol].Text = "Separation";
                sheet1.Range[xlsRow - 1, startDosCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow - 1, startDosCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Yellow;
                sheet1.Range[xlsRow - 1, startDosCol].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow - 1, startDosCol].ColumnWidth = 15;
                sheet1.Range[xlsRow - 1, startDosCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, startDosCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow - 1, startDosCol].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow - 1, startDosCol, xlsRow - 1, dosEndCol].Merge();
                sheet1.Range[xlsRow - 1, startDosCol, xlsRow - 1, dosEndCol].BorderAround(ExcelLineStyle.Thin);

                sheet1.Range[xlsRow - 1, startDoJCol].Text = "Joining";
                sheet1.Range[xlsRow - 1, startDoJCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_green;

                sheet1.Range[xlsRow - 1, startDoJCol].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow - 1, startDoJCol].ColumnWidth = 15;
                sheet1.Range[xlsRow - 1, startDoJCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, startDoJCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow - 1, startDoJCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow - 1, startDoJCol].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow - 1, startDoJCol, xlsRow - 1, doJEndCol].Merge();
                sheet1.Range[xlsRow - 1, startDoJCol, xlsRow - 1, doJEndCol].BorderAround(ExcelLineStyle.Thin);

                xlsRow++;
                endXlsCol = xlsCol;
                int DataStartRow = xlsRow;

                if (dtAttSummary.Rows.Count > 0)
                {

                    #endregion
                    var slCount = 0;
                    List<string> empList = new List<string>();
                    int str = 0;
                    int edr = 0;

                    for (int i = 0; i < dtAttSummary.Rows.Count; i++)
                    {
                        slCount++;


                        // DOS for marge
                        bool IsNewEmp = true;

                        if (empList.Contains(dtAttSummary.Rows[i]["EmployeeCodeDOS"].ToString()))
                        {
                            edr = xlsRow;
                            IsNewEmp = false;
                            sheet1.Range[str, cSrNo, edr, cSrNo].Merge();
                            sheet1.Range[str, cEmployeeCode, edr, cEmployeeCode].Merge();
                            sheet1.Range[str, cEmployeeName, edr, cEmployeeName].Merge();
                            sheet1.Range[str, cCellPhnNo, edr, cCellPhnNo].Merge();
                            sheet1.Range[str, cNationalId, edr, cNationalId].Merge();
                            sheet1.Range[str, cFatherName, edr, cFatherName].Merge();
                            sheet1.Range[str, cMotherName, edr, cMotherName].Merge();
                            sheet1.Range[str, cGenderID, edr, cGenderID].Merge();
                            sheet1.Range[str, cDOB, edr, cDOB].Merge();

                        }
                        else
                        {


                            empList.Add(dtAttSummary.Rows[i]["EmployeeCodeDOS"].ToString());


                        }

                        if (IsNewEmp)
                        {
                            str = xlsRow;
                            //dos Data plot
                            oRU.SetText(ref sheet1, xlsRow, cSrNo, slCount.ToString(), ExcelHAlign.HAlignJustify);
                            oRU.SetText(ref sheet1, xlsRow, cEmployeeCode, dtAttSummary.Rows[i]["EmployeeCodeDOS"].ToString(), ExcelHAlign.HAlignJustify);
                            oRU.SetText(ref sheet1, xlsRow, cEmployeeName, dtAttSummary.Rows[i]["EmployeeNameDOS"].ToString(), ExcelHAlign.HAlignJustify);
                            oRU.SetText(ref sheet1, xlsRow, cCellPhnNo, dtAttSummary.Rows[i]["CellPhnNoDOS"].ToString(), ExcelHAlign.HAlignJustify);
                            oRU.SetText(ref sheet1, xlsRow, cNationalId, dtAttSummary.Rows[i]["NationalIdDOS"].ToString(), ExcelHAlign.HAlignJustify);
                            oRU.SetText(ref sheet1, xlsRow, cFatherName, dtAttSummary.Rows[i]["FatherNameDOS"].ToString(), ExcelHAlign.HAlignJustify);
                            oRU.SetText(ref sheet1, xlsRow, cMotherName, dtAttSummary.Rows[i]["MotherNameDOS"].ToString(), ExcelHAlign.HAlignJustify);
                            oRU.SetText(ref sheet1, xlsRow, cGenderID, dtAttSummary.Rows[i]["GenderIDDOS"].ToString(), ExcelHAlign.HAlignJustify);
                            oRU.SetText(ref sheet1, xlsRow, cDOB, dtAttSummary.Rows[i]["DOBDOS"].ToString());
                            
                            //oRU.SetText(ref sheet1, xlsRow, cDOB, dtAttSummary.Rows[i]["EmployeeStatusDOS"].ToString());
                            //oRU.SetText(ref sheet1, xlsRow, dojDOB, dtAttSummary.Rows[i]["SpecialFollowUPDOS"].ToString());
                        }
                        
                        oRU.SetText(ref sheet1, xlsRow, dojEmployeeCode, dtAttSummary.Rows[i]["EmployeeCodeDOJ"].ToString());
                        //oRU.SetText(ref sheet1, xlsRow, dojEmployeeName, dtAttSummary.Rows[i]["cDOJ"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, dojEmployeeName, dtAttSummary.Rows[i]["EmployeeNameDOJ"].ToString());

                        if (dtAttSummary.Rows[i]["EmployeeNameDOS"].ToString() == dtAttSummary.Rows[i]["EmployeeNameDOJ"].ToString())
                        {
                            sheet1.Range[xlsRow, dojEmployeeName].CellStyle.ColorIndex = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, dojEmployeeName].CellStyle.Font.Color = ExcelKnownColors.White;
                        }
                        oRU.SetText(ref sheet1, xlsRow, dojCellPhnNo, dtAttSummary.Rows[i]["CellPhnNoDOJ"].ToString());


                        oRU.SetText(ref sheet1, xlsRow, dojNationalId, dtAttSummary.Rows[i]["NationalIdDOJ"].ToString());

                        if (dtAttSummary.Rows[i]["NationalIdDOS"].ToString() == dtAttSummary.Rows[i]["NationalIdDOJ"].ToString())
                        {
                            sheet1.Range[xlsRow, dojNationalId].CellStyle.ColorIndex = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, dojNationalId].CellStyle.Font.Color = ExcelKnownColors.White;
                        }

                        oRU.SetText(ref sheet1, xlsRow, dojFatherName, dtAttSummary.Rows[i]["FatherNameDOJ"].ToString());

                        if (dtAttSummary.Rows[i]["FatherNameDOS"].ToString() == dtAttSummary.Rows[i]["FatherNameDOJ"].ToString())
                        {
                            sheet1.Range[xlsRow, dojFatherName].CellStyle.ColorIndex = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, dojFatherName].CellStyle.Font.Color = ExcelKnownColors.White;
                        }
                        oRU.SetText(ref sheet1, xlsRow, dojMotherName, dtAttSummary.Rows[i]["MotherNameDOJ"].ToString());

                        if (dtAttSummary.Rows[i]["MotherNameDOS"].ToString() == dtAttSummary.Rows[i]["MotherNameDOJ"].ToString())
                        {
                            sheet1.Range[xlsRow, dojMotherName].CellStyle.ColorIndex = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, dojMotherName].CellStyle.Font.Color = ExcelKnownColors.White;
                        }
                        oRU.SetText(ref sheet1, xlsRow, dojGenderID, dtAttSummary.Rows[i]["GenderIDDOJ"].ToString());

                        if (dtAttSummary.Rows[i]["GenderIDDOS"].ToString() == dtAttSummary.Rows[i]["GenderIDDOJ"].ToString())
                        {
                            sheet1.Range[xlsRow, dojGenderID].CellStyle.ColorIndex = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, dojGenderID].CellStyle.Font.Color = ExcelKnownColors.White;
                        }
                        
                        oRU.SetText(ref sheet1, xlsRow, dojDOB, dtAttSummary.Rows[i]["DOBDOJ"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, dojEmployeeStatus, dtAttSummary.Rows[i]["EmployeeStatusDOJ"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, dojSpecialFollowUP, dtAttSummary.Rows[i]["SpecialFollowUPDOJ"].ToString());

                        xlsRow++;
                    }
                    //  edr = xlsRow;
                    //sheet1.Range[str, cEmployeeStatus, edr, cEmployeeStatus].Merge();
                    //sheet1.Range[str, cSpecialFollowUP, edr, cSpecialFollowUP].Merge();
                    
                }

                //Border start
                sheet1.Range[DataStartRow, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[DataStartRow, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Thin);
                sheet1.Range[DataStartRow, 1, xlsRow - 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignTop;
                //Border end
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

                //sheet1.Range[11, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                //sheet1.Range[11, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                //sheet1.Range[11, 4, xlsRow, 4].WrapText = true;
                objRpt = new clsReport();
                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
                xlsRow = 1;
                xlsCol = 1;

                FactoryName = string.Empty;

                var FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                //sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                //sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                //xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                xlsRow++;
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = " Matching Employee List  ";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;

                //#endregion *****************Report Header*****************
                #region Freeze Panes
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                //sheet1.UsedRange.CellStyle.Font.Size = 8;
                //  oRU.CompanyPlantHeader(ref sheet1, endXlsCol, "Joining Information from ",companyId, plantName);
                //sheet1.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(endXlsCol) + 5].Merge();
                oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);


                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        //



        private DataTable GetSpecialFollowUPReportSummarySql(string fromDate, string toDate)
        {
            string strSql1;

            try
            {

strSql1= @"select *
                     FROM
                    (Select EEI.SystemId EmpDOSSystId,EmployeeCode,EmployeeCode EmployeeCodeDOS, DOS, GroupID CompanyGroupIdDOS,
				                     EmployeeName EmployeeNameDOS, CellPhnNo CellPhnNoDOS, NationalId NationalIdDOS, 
				                     SpecialFollowUP SpecialFollowUPDOS, FatherName FatherNameDOS,
				                      MotherName MotherNameDOS, GenderID GenderIDDOS ,FORMAT(DOB,'dd-MMM-yyyy') DOBDOS 
				                    from EmployeeInformation EEI
                    INNER JOIN TRN.Resignation RSG ON EEI.SystemId = RSG.EmployeeId  where EEI.EmployeeStatus = 'Separated'
                    AND SpecialFollowUP=1

                    ) DOSEmp
                      INNER JOIN

                      (Select EEI.SystemId EmpDOJSystId,EmployeeCode EmployeeCodeDOJ, EmployeeName EmployeeNameDOJ,DOJ,EmployeeStatus EmployeeStatusDOJ,
                      GroupID CompanyGroupIdDOJ, CellPhnNo CellPhnNoDOJ, NationalId NationalIdDOJ, SpecialFollowUP SpecialFollowUPDOJ,
                       FatherName FatherNameDOJ, MotherName MotherNameDOJ, GenderID GenderIDDOJ ,FORMAT(DOB,'dd-MMM-yyyy') DOBDOJ
                       from EmployeeInformation EEI 
                       INNER JOIN TRN.Resignation RSG ON EEI.SystemId = RSG.EmployeeId  where
                        SpecialFollowUP=1
                        AND
                        DOJ <='" + toDate + @"'
                    )
                    DOJEmp ON DOSEmp.CompanyGroupIdDOS = DOJEmp.CompanyGroupIdDOJ

                    AND --DOSEmp.EmployeeCode<>DOJEmp.EmployeeCode
                    (
                     DOJEmp.CellPhnNoDOJ = DOSEmp.CellPhnNoDOS 
                    OR DOJEmp.NationalIdDOJ = DOSEmp.NationalIdDOS 
                    )

                    OR
                    (	
	                     DOJEmp.EmployeeNameDOJ = DOSEmp.EmployeeNameDOS
	                    AND DOJEmp.FatherNameDOJ LIKE '%' + DOSEmp.FatherNameDOS + '%' 
	                    AND DOJEmp.GenderIDDOJ LIKE '%' + DOSEmp.GenderIDDOS + '%' 
                    )

                    OR (
                         DOJEmp.FatherNameDOJ LIKE '%' + DOSEmp.FatherNameDOS + '%' 
	                    AND DOJEmp.MotherNameDOJ LIKE '%' + DOSEmp.MotherNameDOS + '%' 
	                    AND DOJEmp.GenderIDDOJ = DOSEmp.GenderIDDOS
	                    ) ORDER BY DOSEmp.EmployeeCode";

                return _sqlRepository.GetDataTable(strSql1);
            }
            catch (Exception)
            {

                throw;
            }





        }
    }
}

