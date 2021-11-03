using Library.Core;
using Library.Data.Sql;
using Library.Model.Organizations;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Organizations
{
    public class OrganizationReportService : IOrganizationReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IManpowerBudgetService _manpowerBudgetService;

        public OrganizationReportService(
            ISqlRepository sqlRepository
            , IManpowerBudgetService manpowerBudgetService)
        {
            _sqlRepository = sqlRepository;
            _manpowerBudgetService = manpowerBudgetService;
        }

        public IWorkbook GetEntity(string companyGroupId, string companyId)
        {
            try
            {
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = Entity_Report(excelEngine, companyGroupId, companyId);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook Entity_Report(ExcelEngine excelEngine, string companyGroupId, string companyId)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_Entity2(ref sheet1, oRU, "Entity", "Entity Report", companyGroupId, companyId);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_Entity2(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string companyGroupId, string companyId)
        {
            DataTable dtEntity = null;

            #region List data

            List<StructureRelationship> structureRelationship = GetEntityRelationship(companyId);

            DataSet EntityList = EntityRelationData(structureRelationship, companyId);
            DataView dvEntity = new DataView(EntityList.Tables[0])
            {
                Sort = "Id"
            };

            dtEntity = dvEntity.ToTable();
            if (dtEntity.Rows.Count == 0)
            {
                throw (new Exception("No Data Found !!!"));
            }

            #endregion List data

            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;
            var _col3 = 3;

            oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
            sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
            oRU.SetText(ref sheet, _rowL, _col + 2, dtEntity.Rows[0]["Company"].ToString()); _rowL++;
            sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

            _rowL = 6;
            _rowL++;

            for (int i = 0; i < dtEntity.Columns.Count; i++)
            {
                if (dtEntity.Columns[i].ColumnName != "Id" && dtEntity.Columns[i].ColumnName != "TotalRows" && dtEntity.Columns[i].ColumnName != "Company")
                {
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtEntity.Columns[i].ColumnName);
                }
            }
            shet2EndxlsCol = _colIndex;

            for (int i = 0; i < dtEntity.Rows.Count; i++)
            {
                _rowL++;
                var count = 2;
                //oRU.SetText(ref sheet, _rowL, 1, dtEntity.Rows[i]["Id"].ToString(), 10);
                oRU.SetText(ref sheet, _rowL, 1, dtEntity.Rows[i]["Code"].ToString(), 10);
                foreach (var item in structureRelationship)
                {
                    oRU.SetText(ref sheet, _rowL, count, dtEntity.Rows[i][item.UserName].ToString(), 26);
                    count++;
                }
                oRU.SetText(ref sheet, _rowL, count, dtEntity.Rows[i]["Entity Name"].ToString(), 26); count++;
                oRU.SetText(ref sheet, _rowL, count, dtEntity.Rows[i]["Entity Type"].ToString(), 20); count++;
                oRU.SetText(ref sheet, _rowL, count, dtEntity.Rows[i]["Responsible Person"].ToString(), 26); count++;
            }

            sheet.Range[(7), 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Entity Report", companyGroupId);
            oRU.FreezePage(ref sheet, 1, 8);
            oRU.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
        }

        private List<StructureRelationship> GetEntityRelationship(string companyId)
        {
            try
            {
                string _sql = @"SELECT Id,
                                Sequence,
                                StandardName,
                                UserName,
                                SchemaName
                                FROM ORG.StructureRelationship
                                WHERE Archive=0 AND CompanyId='" + companyId + @"' ORDER BY Sequence";
                var x = _sqlRepository.GetModelCollection<StructureRelationship>(_sql, null);
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet EntityRelationData(List<StructureRelationship> structureRelationship, string companyId)
        {
            GridParameter parameters = null;

            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };

                parameters.CmdText = "";

                if (structureRelationship != null && structureRelationship.Count > 0)
                {
                    string col = "";
                    string table = "";
                    foreach (var item in structureRelationship)
                    {
                        col += ", " + item.StandardName + ".UserName AS " + item.UserName;
                        table += " LEFT OUTER JOIN [" + item.SchemaName + "].[" + item.StandardName + "] AS " + item.StandardName + " ON " + item.StandardName + ".Id = E." + item.StandardName + "Id ";
                    }
                    parameters.CmdText = @"SELECT CONVERT(INT, E.Id) as Id, E.Code AS [Code] " + col + @", E.UserName AS [Entity Name], ET.UserName AS [Entity Type]
                                , CO.UserName AS Company
                                , concat(EI.SystemId,'-',EI.EmployeeName) AS [Responsible Person]
                                FROM ORG.Entity AS E
                                " + table + @"
                                LEFT JOIN  ORG.EntityType AS ET ON ET.Id=E.EntityTypeId
                                LEFT JOIN  ORG.Company AS CO ON CO.Id=E.CompanyId
                                LEFT JOIN  dbo.EmployeeInformation AS EI ON EI.SystemId=E.EmployeeId
                                WHERE E.Archive=0 AND E.CompanyId='" + companyId + "'";
                }
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetManpowerBudget(string companyGroupId, string companyId)
        {
            try
            {
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = ManpowerBudget_Report(excelEngine, companyGroupId, companyId);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook ManpowerBudget_Report(ExcelEngine excelEngine, string companyGroupId, string companyId)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_ManpowerBudget2(ref sheet1, oRU, "Manpower Budget", "Manpower Budget Report", companyGroupId, companyId);
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_ManpowerBudget2(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string companyGroupId, string companyId)
        {
            DataTable dtBudget = null;
            DataTable dtCompany = null;

            #region List data

            var manpowerBudgetList = GetManpowerBudgetData(companyGroupId, companyId);
            DataView dvManpowerBudget = new DataView(manpowerBudgetList)
            {
                Sort = "EntityName, PositionName"
            };

            dtBudget = dvManpowerBudget.ToTable();
            if (dtBudget.Rows.Count == 0)
                throw new Exception("No data found!");

            var CompanyList = GetCompany(companyId);
            dtCompany = CompanyList;
            if (dtCompany.Rows.Count == 0)
                throw new Exception("No data found!");

            #endregion List data

            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;

            var _col3 = 3;

            oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
            sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
            oRU.SetText(ref sheet, _rowL, _col + 2, dtCompany.Rows[0]["Company"].ToString()); _rowL++;
            sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

            _rowL = 6;
            _rowL++;

            for (int i = 0; i < dtBudget.Columns.Count; i++)
            {
                if ("Id" != dtBudget.Columns[i].ColumnName.Substring(dtBudget.Columns[i].ColumnName.Length - 2) && dtBudget.Columns[i].ColumnName != "TotalRows" && dtBudget.Columns[i].ColumnName != "ROBudgetCode" && dtBudget.Columns[i].ColumnName != "PRBudgetCode")
                {
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtBudget.Columns[i].ColumnName);
                }
            }

            shet2EndxlsCol = _colIndex;
            for (int i = 0; i < dtBudget.Rows.Count; i++)
            {
                _col = 0;
                _rowL++;
                for (int t = 0; t < dtBudget.Columns.Count; t++)
                {
                    if ("Id" != dtBudget.Columns[t].ColumnName.Substring(dtBudget.Columns[t].ColumnName.Length - 2) && dtBudget.Columns[t].ColumnName != "TotalRows" && dtBudget.Columns[t].ColumnName != "ROBudgetCode" && dtBudget.Columns[t].ColumnName != "PRBudgetCode")
                    {
                        _col++;
                        oRU.SetText(ref sheet, _rowL, _col, dtBudget.Rows[i][dtBudget.Columns[t].ColumnName].ToString(), 20);
                    }
                }
            }

            sheet.Range[(7), 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Manpower Budget Report", companyGroupId);
            oRU.FreezePage(ref sheet, 1, 8);
            oRU.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
        }

        private DataTable GetCompany(string companyId)
        {
            var sql = @"SELECT C.UserName AS Company FROM MST.ManpowerBudget AS M
                        LEFT JOIN ORG.Company AS C ON C.Id=M.CompanyId
                        WHERE M.CompanyId='" + companyId + "' AND M.Archive=0";
            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetManpowerBudgetData(string companyGroupId, string companyId)
        {
            var sql = _manpowerBudgetService.GetManpowerBudgetListSql(companyGroupId, companyId);
            return _sqlRepository.GetDataTable(sql);
        }

        public IWorkbook GetDesignationMaster(string companyGroupId)
        {
            try
            {
                var oRU = new ReportUtility();
                var excelEngine = new ExcelEngine();
                var workbook = oRU.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets[1];
                CreateSheet_DesignationMaster1(ref sheet1, oRU, "Designation Master Report", "Designation Master Report", companyGroupId);
                CreateSheet_DesignationMaster2(ref sheet2, oRU, "Designation Master List", "Designation Master Data", companyGroupId);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_DesignationMaster1(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string companyGroupId)
        {
            try
            {
                DataTable dtDesignationMaster = null;

                #region List data

                var DesignationMasterList = GetDesignationMasterData(companyGroupId);
                dtDesignationMaster = DesignationMasterList;

                DataView dvDesignationGroup = new DataView(DesignationMasterList);
                DataTable dtDesignationGroup = dvDesignationGroup.ToTable(true, "Designation Group", "DesignationGroupId", "Designation Group Code");
                dvDesignationGroup.Sort = "DG Sequence";

                DataView dvDesignation = null;
                DataTable dtDesignation = null;

                DataView dvLegalDesignation = null;
                DataTable dtLegalDesignation = null;

                if (dtDesignationMaster.Rows.Count == 0)
                {
                    throw (new Exception("No Data Found !!!"));
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var designationGroupCodeColIndex = 1;
                var designationGroupColIndex = 2;
                var designationCodeColIndex = 3;
                var designationColIndex = 4;
                var employeeTypeColIndex = 5;
                var legalDesignationCodeColIndex = 6;
                var legalDesignationColIndex = 7;

                for (int i = 0; i < dtDesignationMaster.Columns.Count; i++)
                {
                    if (dtDesignationMaster.Columns[i].ColumnName != "Id" && dtDesignationMaster.Columns[i].ColumnName != "TotalRows" && dtDesignationMaster.Columns[i].ColumnName != "DesignationGroupId" && dtDesignationMaster.Columns[i].ColumnName != "DesignationId" && dtDesignationMaster.Columns[i].ColumnName != "LegalDesignationId" && dtDesignationMaster.Columns[i].ColumnName != "DG Sequence" && dtDesignationMaster.Columns[i].ColumnName != "DEG Sequence" && dtDesignationMaster.Columns[i].ColumnName != "LDEG Sequence")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtDesignationMaster.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int p = 0; p < dtDesignationGroup.Rows.Count; p++)
                {
                    _rowL++;
                    string designationGroupId = dtDesignationGroup.Rows[p]["DesignationGroupId"].ToString();
                    dvDesignation = new DataView(dtDesignationMaster)
                    {
                        Sort = "DEG Sequence",
                        RowFilter = "DesignationGroupId='" + designationGroupId + "'"
                    };
                    dtDesignation = dvDesignation.ToTable(true, "Designation", "DesignationId", "Employee Type", "Designation Code");
                    var rowStartDesignationGroup = _rowL;
                    oRU.SetText(ref sheet, _rowL, designationGroupCodeColIndex, dtDesignationGroup.Rows[p]["Designation Group Code"].ToString(), 20);
                    oRU.SetText(ref sheet, _rowL, designationGroupColIndex, dtDesignationGroup.Rows[p]["Designation Group"].ToString(), 26);

                    for (int i = 0; i < dtDesignation.Rows.Count; i++)
                    {
                        string designationId = dtDesignation.Rows[i]["DesignationId"].ToString();
                        dvLegalDesignation = new DataView(dtDesignationMaster)
                        {
                            Sort = "LDEG Sequence",
                            RowFilter = "DesignationId='" + designationId + "' and DesignationGroupId='" + designationGroupId + "'"
                        };
                        dtLegalDesignation = dvLegalDesignation.ToTable(true, "Legal Designation", "LegalDesignationId", "Legal Designation Code");
                        var rowStartDesignation = _rowL;

                        oRU.SetText(ref sheet, _rowL, designationCodeColIndex, dtDesignation.Rows[i]["Designation Code"].ToString(), 20);
                        oRU.SetText(ref sheet, _rowL, designationColIndex, dtDesignation.Rows[i]["Designation"].ToString(), 26);
                        oRU.SetText(ref sheet, _rowL, employeeTypeColIndex, dtDesignation.Rows[i]["Employee Type"].ToString(), 15);

                        for (int q = 0; q < dtLegalDesignation.Rows.Count; q++)
                        {
                            oRU.SetText(ref sheet, _rowL, legalDesignationCodeColIndex, dtLegalDesignation.Rows[q]["Legal Designation Code"].ToString(), 20);
                            oRU.SetText(ref sheet, _rowL, legalDesignationColIndex, dtLegalDesignation.Rows[q]["Legal Designation"].ToString(), 26);
                            _rowL++;
                            designationGroupCodeColIndex = 1;
                            designationGroupColIndex = 2;
                            designationCodeColIndex = 3;
                            designationColIndex = 4;
                            employeeTypeColIndex = 5;
                            legalDesignationCodeColIndex = 6;
                            legalDesignationColIndex = 7;
                        }
                        //If Legal Designation number more than 0 apply for marge
                        if (dtLegalDesignation.Rows.Count > 0)
                        {
                            sheet[rowStartDesignation, designationCodeColIndex, (_rowL - 1), designationCodeColIndex].Merge();
                            sheet[rowStartDesignation, employeeTypeColIndex, (_rowL - 1), employeeTypeColIndex].Merge();
                            sheet[rowStartDesignation, designationColIndex, (_rowL - 1), designationColIndex].Merge();
                        }
                    }//Designation
                    sheet[rowStartDesignationGroup, designationGroupCodeColIndex, (_rowL - 1), designationGroupCodeColIndex].Merge();
                    sheet[rowStartDesignationGroup, designationGroupColIndex, (_rowL - 1), designationGroupColIndex].Merge();
                    _rowL--;
                }//Designation Group

                sheet.Range[(5), 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Designation Master Report", companyGroupId);
                oRU.FreezePage(ref sheet, 1, 6);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_DesignationMaster2(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string companyGroupId)
        {
            DataTable dtDesignationMaster = null;

            #region List data

            var DesignationMasterList = GetDesignationMasterData(companyGroupId);
            DataView dvDesignationMaster = new DataView(DesignationMasterList)
            {
                Sort = "DG Sequence, DEG Sequence, LDEG Sequence"
            };
            dtDesignationMaster = dvDesignationMaster.ToTable();
            if (dtDesignationMaster.Rows.Count == 0)
            {
                throw (new Exception("No Data Found !!!"));
            }

            #endregion List data

            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;
            _rowL++;

            for (int i = 0; i < dtDesignationMaster.Columns.Count; i++)
            {
                if (dtDesignationMaster.Columns[i].ColumnName != "Id" && dtDesignationMaster.Columns[i].ColumnName != "TotalRows" && dtDesignationMaster.Columns[i].ColumnName != "DesignationGroupId" && dtDesignationMaster.Columns[i].ColumnName != "DesignationId" && dtDesignationMaster.Columns[i].ColumnName != "LegalDesignationId" && dtDesignationMaster.Columns[i].ColumnName != "DG Sequence" && dtDesignationMaster.Columns[i].ColumnName != "DEG Sequence" && dtDesignationMaster.Columns[i].ColumnName != "LDEG Sequence")
                {
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtDesignationMaster.Columns[i].ColumnName);
                }
            }
            shet2EndxlsCol = _colIndex;

            for (int i = 0; i < dtDesignationMaster.Rows.Count; i++)
            {
                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, dtDesignationMaster.Rows[i]["Designation Group Code"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 2, dtDesignationMaster.Rows[i]["Designation Group"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 3, dtDesignationMaster.Rows[i]["Designation Code"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 4, dtDesignationMaster.Rows[i]["Designation"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 5, dtDesignationMaster.Rows[i]["Employee Type"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 6, dtDesignationMaster.Rows[i]["Legal Designation Code"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 7, dtDesignationMaster.Rows[i]["Legal Designation"].ToString(), 26);
            }

            sheet.Range[(6), 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Designation Master List", companyGroupId);
            oRU.FreezePage(ref sheet, 1, 7);
            oRU.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
        }

        private DataTable GetDesignationMasterData(string companyGroupId)
        {
            var cmdText = @"SELECT DM.Id, DM.DesignationGroupId, DG.Code AS [Designation Group Code], DG.Sequence [DG Sequence], DG.UserName AS [Designation Group]
                            , DM.DesignationId, D.Code AS [Designation Code], D.Sequence [DEG Sequence], D.UserName AS Designation, E.UserName AS [Employee Type]
							, DL.LegalDesignationId, L.Code AS [Legal Designation Code], L.Sequence [LDEG Sequence], L.UserName AS [Legal Designation]
                            FROM MST.DesignationMaster AS DM
                            LEFT JOIN MST.DesignationMasterLegalDesignation AS DL ON DL.DesignationMasterId=DM.Id
                            LEFT JOIN HKP.DesignationGroup AS DG ON DG.Id=DM.DesignationGroupId
                            LEFT JOIN HKP.Designation AS D ON D.Id=DM.DesignationId
                            LEFT JOIN HKP.EmployeeCategory AS E ON E.Id=DM.EmployeeCategoryId
                            LEFT JOIN HKP.LegalDesignation AS L ON L.Id=DL.LegalDesignationId
                            WHERE DM.CompanyGroupId='" + companyGroupId + "' AND DM.Archive=0";
            return _sqlRepository.GetDataTable(cmdText);
        }
    }
}