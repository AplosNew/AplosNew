using Library.Data;
using Library.Data.Sql;
using Library.Model.Organizations;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Organizations
{
    public class PositionReportService : IPositionReportService
    {
        private readonly ISqlRepository _sqlRepository;

        public PositionReportService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        public IWorkbook PositionReport(string companyGroupId)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                CreatePositionSheet(ref sheet1, reportUtility, "Position", "Position Report", companyGroupId);
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreatePositionSheet(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyGroupId)
        {
            var sql = @"SELECT Id, Sequence, StandardName, UserName, SchemaName FROM ORG.StructureRelationship
                        WHERE Archive=0 AND RType='Position' ORDER BY Sequence";
            var structureRelationship = _sqlRepository.GetModelCollection<StructureRelationship>(sql);
            var positionList = PositionRelationData(structureRelationship);
            if (positionList.Rows.Count == 0)
                throw new CustomException("No data found!");
            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;
            _rowL = 6;
            _rowL++;

            for (int i = 0; i < positionList.Columns.Count; i++)
            {
                if (positionList.Columns[i].ColumnName != "TotalRows" && positionList.Columns[i].ColumnName != "Id")
                {
                    _colIndex++;
                    reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, positionList.Columns[i].ColumnName);
                }
            }
            shet2EndxlsCol = _colIndex;
            for (int i = 0; i < positionList.Rows.Count; i++)
            {
                _rowL++;
                var count = 2;
                reportUtility.SetText(ref sheet, _rowL, 1, positionList.Rows[i]["Code"].ToString(), 10);
                foreach (var item in structureRelationship)
                {
                    reportUtility.SetText(ref sheet, _rowL, count, positionList.Rows[i][item.UserName].ToString(), 26);
                    count++;
                }
                reportUtility.SetText(ref sheet, _rowL, count, positionList.Rows[i]["Designation"].ToString(), 26); count++;
                reportUtility.SetText(ref sheet, _rowL, count, positionList.Rows[i]["Position Name"].ToString(), 26); count++;
                reportUtility.SetText(ref sheet, _rowL, count, positionList.Rows[i]["Payment Link"].ToString(), 15); count++;
                reportUtility.SetText(ref sheet, _rowL, count, positionList.Rows[i]["IsDirect"].ToString(), 15); count++;
            }

            sheet.Range[(7), 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = sheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            reportUtility.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Position Report", companyGroupId);
            reportUtility.FreezePage(ref sheet, 1, 8);
            reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
        }

        private DataTable PositionRelationData(List<StructureRelationship> structureRelationship)
        {
            try
            {
                if (structureRelationship != null && structureRelationship.Count > 0)
                {
                    var col = "";
                    var table = "";
                    foreach (var item in structureRelationship)
                    {
                        col += ", " + item.StandardName + ".UserName AS " + item.UserName;
                        table += " LEFT OUTER JOIN [" + item.SchemaName + "].[" + item.StandardName + "] AS " + item.StandardName + " ON " + item.StandardName + ".Id = P." + item.StandardName + "Id ";
                    }
                    var sql = @"SELECT CONVERT(INT, P.Id) AS Id, P.Code AS [Code] " + col + @"
                                ,D.UserName AS Designation
                                ,P.username as [Position Name]
                                ,P.PaymentLink AS [Payment Link]
                                ,[IsDirect]=CASE WHEN P.IsDirect=1 THEN 'Yes' ELSE 'No' END
                                FROM ORG.Position AS P
                                " + table + @"
                                LEFT JOIN HKP.Designation AS D ON D.Id=P.DesignationId
                                WHERE P.Archive=0 ORDER BY Id";
                    return _sqlRepository.GetDataTable(sql);
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}