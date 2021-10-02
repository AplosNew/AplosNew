using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using Library.Service.Payrolls.OT;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.IncrementReport
{
    public class IncrementReport
    {
        SqlRepository _sqlRepository = null;

        public IncrementReport()
        {
            _sqlRepository = new SqlRepository();
        }
        public Dictionary<string, DataRow> Cluster(DataSet dsNewSStructure, string HeadCategory)
        {
            Dictionary<string, DataRow> dcCluster = new Dictionary<string, DataRow>();
            dsNewSStructure.Tables[0].DefaultView.RowFilter = "HeadCategory='" + HeadCategory + "'";
            DataTable dtNewGross = dsNewSStructure.Tables[0].DefaultView.ToTable();
            for (int i = 0; i < dtNewGross.Rows.Count; i++)
            {
                dcCluster.Add(dtNewGross.Rows[i]["EmpInfoSystemID"].ToString(), dtNewGross.Rows[i]);
            }
            dsNewSStructure.Tables[0].DefaultView.RowFilter = null;
            return dcCluster;
        }

        public void EmployeeInformation(string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (string.IsNullOrEmpty(FromDate))
                    throw new Exception("Plase select from date");

                if (string.IsNullOrEmpty(ToDate))
                    throw new Exception("Plase select to date");

                FromDate = Convert.ToDateTime(FromDate).ToString("dd-MMM-yyyy");
                ToDate = Convert.ToDateTime(ToDate).ToString("dd-MMM-yyyy");


                string sql = @"select e.SystemId,

                                    e.EmployeeCode,e.EmployeeName,m.Code BudgetCode,d.UserName NewLegalDesignation
                                    ,c.UserName EmployeeCategory
                                    ,dp.UserName Department,s.UserName Section,ss.UserName Subsection
                                    ,grd.UserName Grade

                                    from EmployeeInformation e
                                    left join mst.ManpowerBudget m on e.BudgetCode=m.Id
                                    left join hkp.LegalDesignation d on e.LegalDesignationId=d.Id
                                    left join org.Position p on p.id=m.PositionId
                                    left join mst.DesignationMasterLegalDesignation dm on dm.LegalDesignationId=e.LegalDesignationId
                                    left join mst.DesignationMaster dd on dd.id=dm.DesignationMasterId
                                    left join hkp.EmployeeCategory c on c.id=dd.EmployeeCategoryId
                                    left join org.Department dp on dp.id=p.DepartmentId
                                    left join org.Section s on s.id=p.SectionId
                                    left join org.SubSection ss on ss.id=p.SubSectionId
                                    left join [MST].[LegalSalaryGradeDesignation] gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
                                    left join scs.LegalSalaryGrade grd on grd.id=gr.LegalSalaryGradeId
                                    where e.PlantId='" + identity.PlantId + @"' 
									AND e.SystemId in 									
									                (
								                    SELECT  sidm.EmpInfoSystemID FROM SalaryInfoDefineMaster AS sidm WHERE sidm.IsApproved=1 AND sidm.PlantID='" + identity.PlantId + @"' AND e.DOJ<>sidm.EffectiveDate AND sidm.EffectiveDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'
								                    UNION 
								                    SELECT  sidm.EmpInfoSystemID FROM SalaryInfoBackMaster AS sidm WHERE sidm.IsApproved=1 AND sidm.PlantID='" + identity.PlantId + @"' AND e.DOJ<>sidm.EffectiveDate  AND sidm.EffectiveDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'
								                    )
									--AND  e.SystemId in 	(
									--				SELECT  sidm.EmpInfoSystemID FROM SalaryInfoDefineMaster AS sidm WHERE sidm.IsApproved=1 AND sidm.PlantID='20181' AND sidm.EffectiveDate <'01-Sep-2021'

								 --                   UNION 
								 --                   SELECT  sidm.EmpInfoSystemID FROM SalaryInfoBackMaster AS sidm WHERE sidm.IsApproved=1 AND sidm.PlantID='20181'  AND sidm.EffectiveDate < '01-Sep-2021'
									--				)
                                                   -- AND DATEDIFF(day, FORMAT(E.DOJ,'dd-MMM-yyyy'),FORMAT(GetDate(),'dd-MMM-yyyy'))>365";

                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Increment Report";

                DataTable dtEmployeeData = _sqlRepository.GetDataTable(sql);

                clsOTCalculation otc = new clsOTCalculation();

                otc.LoadSalaryStructureNew(identity.PlantId, FromDate, ToDate, "Gross", out DataSet dsNewGrossSStructure);
                Dictionary<string, DataRow> dicNewGross = Cluster(dsNewGrossSStructure, "Gross");

                otc.LoadSalaryStructureNew(identity.PlantId, FromDate, ToDate, "Basic", out DataSet dsNewBasicSStructure);
                Dictionary<string, DataRow> dicNewBasic = Cluster(dsNewBasicSStructure, "Basic");


                otc.LoadSalaryStructureOld(identity.PlantId, FromDate, ToDate, "Gross", out DataSet dsOldGrossSStructure);
                Dictionary<string, DataRow> dicOldGross = Cluster(dsOldGrossSStructure, "Gross");

                otc.LoadSalaryStructureOld(identity.PlantId, FromDate, ToDate, "Basic", out DataSet dsOldBasicSStructure);
                Dictionary<string, DataRow> dicOldBasic = Cluster(dsOldBasicSStructure, "Basic");

                int ROW = 6;
                int COL = 1;

                sheet[ROW, COL].Text = "Sl.No";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 6;
                int colSlNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colEmployeeCode = COL;
                COL++;
                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 30;
                int colUserEmployeeName = COL;
                COL++;
                sheet[ROW, COL].Text = "Budget Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBudgetCode = COL;
                COL++;
                sheet[ROW, COL].Text = "Old  Designation";
                sheet[ROW, COL].ColumnWidth = 15;
                int colOldLegalDesignation = COL;
                COL++;
                sheet[ROW, COL].Text = "New  Designation";
                sheet[ROW, COL].ColumnWidth = 15;
                int colNewLegalDesignation = COL;
                COL++;
                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 15;
                int colEmployeeCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 20;
                int colDepartment = COL;
                COL++;
                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 15;
                int colSection = COL;
                COL++;
                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 15;
                int colSubsection = COL;
                COL++;
                sheet[ROW, COL].Text = "Old Grade";
                sheet[ROW, COL].ColumnWidth = 15;
                int colOldGrade = COL;
                COL++;
                sheet[ROW, COL].Text = "New Grade";
                sheet[ROW, COL].ColumnWidth = 15;
                int colNewGrade = COL;

                COL++;
                sheet[ROW, COL].Text = "Old Effective Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOldEffectiveDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Old Gross";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colOldGross = COL;
                COL++;
                sheet[ROW, COL].Text = "Old Basic";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colOldBasic = COL;
                COL++;
                sheet[ROW, COL].Text = "New Effective Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colNewEffectiveDate = COL;
                COL++;
                sheet[ROW, COL].Text = "New Gross";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colNewGross = COL;

                COL++;
                sheet[ROW, COL].Text = "New Basic";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colNewBasic = COL;
                COL++;
                sheet[ROW, COL].Text = "Total Increment Amount";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colTotalIncrementAmount = COL;


                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtEmployeeData.Rows.Count; i++)
                {
                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colEmployeeCode].Text = dtEmployeeData.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, colUserEmployeeName].Text = dtEmployeeData.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, colBudgetCode].Text = dtEmployeeData.Rows[i]["BudgetCode"].ToString();
                    sheet[ROW, colNewLegalDesignation].Text = dtEmployeeData.Rows[i]["NewLegalDesignation"].ToString();
                    sheet[ROW, colEmployeeCategory].Text = dtEmployeeData.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, colDepartment].Text = dtEmployeeData.Rows[i]["Department"].ToString();
                    sheet[ROW, colSection].Text = dtEmployeeData.Rows[i]["Section"].ToString();
                    sheet[ROW, colSubsection].Text = dtEmployeeData.Rows[i]["Subsection"].ToString();
                    sheet[ROW, colNewGrade].Text = dtEmployeeData.Rows[i]["Grade"].ToString();

                    sheet[ROW, colTotalIncrementAmount].Formula = "If(" + clsStaticInfo.GetxlsCol(colOldGross) + ROW.ToString() + "<>0," + clsStaticInfo.GetxlsCol(colNewGross) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colOldGross) + ROW.ToString() + ",0)";
                    sheet[ROW, colTotalIncrementAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                    if (dicNewGross.ContainsKey(dtEmployeeData.Rows[i]["SystemId"].ToString()))
                    {
                        DataRow dr = dicNewGross[dtEmployeeData.Rows[i]["SystemId"].ToString()];
                        sheet[ROW, colNewGross].Number = clsStaticInfo.dbl(dr["Amount"].ToString());
                        sheet[ROW, colNewGross].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet[ROW, colOldLegalDesignation].Text = dr["OldLegalDesignation"].ToString();
                        sheet[ROW, colOldGrade].Text = dr["OldGradeCode"].ToString();
                        sheet[ROW, colNewEffectiveDate].Text = Convert.ToDateTime(dr["EffectiveDate"].ToString()).ToString("dd-MMM-yyyy");

                    }

                    if (dicNewBasic.ContainsKey(dtEmployeeData.Rows[i]["SystemId"].ToString()))
                    {
                        DataRow dr = dicNewBasic[dtEmployeeData.Rows[i]["SystemId"].ToString()];
                        sheet[ROW, colNewBasic].Number = clsStaticInfo.dbl(dr["Amount"].ToString());
                        sheet[ROW, colNewBasic].NumberFormat = "#,##0.00;(#,##0.00)";
                    }

                    if (dicOldGross.ContainsKey(dtEmployeeData.Rows[i]["SystemId"].ToString()))
                    {
                        DataRow dr = dicOldGross[dtEmployeeData.Rows[i]["SystemId"].ToString()];
                        sheet[ROW, colOldGross].Number = clsStaticInfo.dbl(dr["Amount"].ToString());
                        sheet[ROW, colOldGross].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colOldEffectiveDate].Text = Convert.ToDateTime(dr["EffectiveDate"].ToString()).ToString("dd-MMM-yyyy");

                    }


                    if (dicOldBasic.ContainsKey(dtEmployeeData.Rows[i]["SystemId"].ToString()))
                    {
                        DataRow dr = dicOldBasic[dtEmployeeData.Rows[i]["SystemId"].ToString()];
                        sheet[ROW, colOldBasic].Number = clsStaticInfo.dbl(dr["Amount"].ToString());
                        sheet[ROW, colOldBasic].NumberFormat = "#,##0.00;(#,##0.00)";
                    }
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Increment(From " + FromDate + " To " + ToDate + ")", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "IncrementReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }






    }
}
