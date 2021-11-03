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

namespace Library.HumanResource.Payroll.IncrementSummaryReport
{
    public class IncrementSummaryReport
    {
        SqlRepository _sqlRepository = null;

        public IncrementSummaryReport()
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
                                    left join [MST].[LegalSalaryGradeDesignation] gr on gr.LegalDesignationId=e.LegalDesignationId
                                    left join scs.LegalSalaryGrade grd on grd.id=gr.LegalSalaryGradeId";

                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];
             
                sheet.Name = "Increment Summary Report";

                DataTable dtEmployeeData = _sqlRepository.GetDataTable(sql);

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsOTCalculation otc = new clsOTCalculation();

                otc.LoadSalaryStructureNew(identity.PlantId, FromDate, ToDate, "Gross", out DataSet dsNewGrossSStructure);
                Dictionary<string, DataRow> dicNewGross = Cluster(dsNewGrossSStructure, "Gross");

                otc.LoadSalaryStructureNew(identity.PlantId, FromDate, ToDate, "Basic", out DataSet dsNewBasicSStructure);
                Dictionary<string, DataRow> dicNewBasic= Cluster(dsNewBasicSStructure, "Basic");


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
                int colNewLegalDesignation = COL;
                COL++;
                sheet[ROW, COL].Text = "New  Designation";
                sheet[ROW, COL].ColumnWidth = 15;
                int colOldLegalDesignation = COL;
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

                    sheet[ROW, colTotalIncrementAmount].Formula = clsStaticInfo.GetxlsCol(colNewGross)+ ROW.ToString()+"-"+clsStaticInfo.GetxlsCol(colOldGross)+ROW.ToString();

                    if (dicNewGross.ContainsKey(dtEmployeeData.Rows[i]["SystemId"].ToString()))
                    {
                        
                        DataRow dr = dicNewGross[dtEmployeeData.Rows[i]["SystemId"].ToString()];
                        sheet[ROW, colNewGross].Number = clsStaticInfo.dbl(dr["Amount"].ToString());
                        sheet[ROW, colOldLegalDesignation].Text = dr["OldLegalDesignation"].ToString();
                        sheet[ROW, colOldGrade].Text = dr["OldGradeCode"].ToString();
                        sheet[ROW, colNewEffectiveDate].Text =Convert.ToDateTime( dr["EffectiveDate"].ToString()).ToString("dd-MMM-yyyy");

                    }
                    
                    if (dicNewBasic.ContainsKey(dtEmployeeData.Rows[i]["SystemId"].ToString()))
                    {
                        
                        DataRow dr = dicNewBasic[dtEmployeeData.Rows[i]["SystemId"].ToString()];
                        sheet[ROW, colNewBasic].Number = clsStaticInfo.dbl(dr["Amount"].ToString());
                     
                    }

                    if (dicOldGross.ContainsKey(dtEmployeeData.Rows[i]["SystemId"].ToString()))

                    {
                        DataRow dr = dicOldGross[dtEmployeeData.Rows[i]["SystemId"].ToString()];
                        sheet[ROW, colOldGross].Number = clsStaticInfo.dbl(dr["Amount"].ToString());
                        sheet[ROW, colOldEffectiveDate].Text = Convert.ToDateTime(dr["EffectiveDate"].ToString()).ToString("dd-MMM-yyyy");

                    }



                    if(dicOldBasic.ContainsKey(dtEmployeeData.Rows[i]["SystemId"].ToString()))
                    {
                        DataRow dr = dicOldBasic[dtEmployeeData.Rows[i]["SystemId"].ToString()];
                        sheet[ROW, colOldBasic].Number = clsStaticInfo.dbl(dr["Amount"].ToString());

                    }

                    //dsSStructure.Tables[0].DefaultView.RowFilter = "HeadCategory='Gross' AND EmpInfoSystemID='" + dtEmployeeData.Rows[i]["SystemId"].ToString() + "'";
                    //if (dsSStructure.Tables[0].DefaultView.Count > 0)
                    //{
                    //    sheet[ROW, colGross].Number = clsStaticInfo.dbl(dsSStructure.Tables[0].DefaultView[0]["Amount"].ToString());
                    //}

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A"+StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Increment(From " + FromDate + " To " + ToDate + ")", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "IncrementSummaryReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }




        public void IncrementSummary()
        {
            try
            {
               

                string sql = @"SELECT salaryInfoTo.EffectiveDate,salaryInfoFrom.EntryAmount FromEntryAmount,salaryInfoTo.EntryAmount EntryAmountTo,
salaryInfoTo.EntryAmount-salaryInfoFrom.EntryAmount IncrementAmount,sh.SalaryHead,LD.UserName Designation,'' FromDesignation,EI.* from  
IncrementHistory IH 
LEFT JOIN (
SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate,SD.EntryAmount,SD.SalaryHeadID FROM SalaryInfoDefineMaster SM
LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
WHERE SM.EmpInfoSystemID='1800001' AND SM.IsApproved=1
Union
SELECT SMB.SystemID,SMB.EmpInfoSystemID,SMB.EffectiveDate,SDB.EntryAmount,SDB.SalaryHeadID FROM SalaryInfoBackMaster SMB
LEFT JOIN SalaryInfoBack SDB ON SDB.SalaryID=SMB.SystemID
WHERE SMB.EmpInfoSystemID='1800001'
) salaryInfoTo on IH.EmpSystemID=salaryInfoTo.EmpInfoSystemID AND IH.ToEffectiveDate=salaryInfoTo.EffectiveDate and IH.ToSalaryId=salaryInfoTo.SystemID
LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=salaryInfoTo.SalaryHeadID

LEFT JOIN (
SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate,SD.EntryAmount,SD.SalaryHeadID FROM SalaryInfoDefineMaster SM
LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
WHERE SM.EmpInfoSystemID='1800001' AND SM.IsApproved=1
Union
SELECT SMB.SystemID,SMB.EmpInfoSystemID,SMB.EffectiveDate,SDB.EntryAmount,SDB.SalaryHeadID FROM SalaryInfoBackMaster SMB
LEFT JOIN SalaryInfoBack SDB ON SDB.SalaryID=SMB.SystemID
WHERE SMB.EmpInfoSystemID='1800001'
) salaryInfoFrom on IH.EmpSystemID=salaryInfoFrom.EmpInfoSystemID AND IH.FromEffectiveDate=salaryInfoFrom.EffectiveDate and IH.FromSalaryId=salaryInfoFrom.SystemID
LEFT JOIN SalaryHead SH1 ON SH1.SalaryHeadID=salaryInfoFrom.SalaryHeadID
LEFT JOIN EmployeeInformation ei ON EI.SystemId=salaryInfoTo.EmpInfoSystemID                          
LEFT JOIN hkp.LegalDesignation LD ON IH.ToLegalDesignationId = LD.Id


where IH.EmpSystemID='1800001' and sh.HeadCategory='gross' and sh1.HeadCategory='gross'
ORDER BY convert(date,salaryInfoFrom.EffectiveDate)";

                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Increment Summary Report";

                DataTable dtEmployeeData = _sqlRepository.GetDataTable(sql);

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsOTCalculation otc = new clsOTCalculation();

                //otc.LoadSalaryStructureNew(identity.PlantId, FromDate, ToDate, "Gross", out DataSet dsNewGrossSStructure);
                //Dictionary<string, DataRow> dicNewGross = Cluster(dsNewGrossSStructure, "Gross");

                //otc.LoadSalaryStructureNew(identity.PlantId, FromDate, ToDate, "Basic", out DataSet dsNewBasicSStructure);
                //Dictionary<string, DataRow> dicNewBasic = Cluster(dsNewBasicSStructure, "Basic");


                //otc.LoadSalaryStructureOld(identity.PlantId, FromDate, ToDate, "Gross", out DataSet dsOldGrossSStructure);
                //Dictionary<string, DataRow> dicOldGross = Cluster(dsOldGrossSStructure, "Gross");

                //otc.LoadSalaryStructureOld(identity.PlantId, FromDate, ToDate, "Basic", out DataSet dsOldBasicSStructure);
                //Dictionary<string, DataRow> dicOldBasic = Cluster(dsOldBasicSStructure, "Basic");


                int ROW = 6;
                int COL = 1;




                sheet[ROW, COL].Text = "Sl.No";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 6;
                int colSlNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Appraisal Date";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 20;
                int colAppraisalDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Previous Gross";
                sheet[ROW, COL].ColumnWidth = 20;
                int colPreviousGross = COL;
                COL++;
                sheet[ROW, COL].Text = "New Gross";
                sheet[ROW, COL].ColumnWidth = 10;
                int colNewGross = COL;
                COL++;
                sheet[ROW, COL].Text = "IncrementAmount";
                sheet[ROW, COL].ColumnWidth = 20;
                int colIncrementAmount = COL;
                COL++;
                sheet[ROW, COL].Text = "New Designation";
                sheet[ROW, COL].ColumnWidth = 15;
                int colNewDesignation = COL;
                COL++;
                sheet[ROW, COL].Text = "Previous Designation";
                sheet[ROW, COL].ColumnWidth = 20;
                int colPreviousDesignation = COL;
             


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

                    sheet[ROW, colAppraisalDate].Text =Convert.ToDateTime( dtEmployeeData.Rows[i]["EffectiveDate"].ToString()).ToString("dd-MMM-yyyy");
                    sheet[ROW, colPreviousGross].Text = dtEmployeeData.Rows[i]["FromEntryAmount"].ToString();
                    sheet[ROW, colNewGross].Text = dtEmployeeData.Rows[i]["EntryAmountTo"].ToString();
                    sheet[ROW, colIncrementAmount].Text = dtEmployeeData.Rows[i]["IncrementAmount"].ToString();
                    sheet[ROW, colNewDesignation].Text = dtEmployeeData.Rows[i]["Designation"].ToString();
                    sheet[ROW, colPreviousDesignation].Text = dtEmployeeData.Rows[i]["FromDesignation"].ToString();

                   

                    //dsSStructure.Tables[0].DefaultView.RowFilter = "HeadCategory='Gross' AND EmpInfoSystemID='" + dtEmployeeData.Rows[i]["SystemId"].ToString() + "'";
                    //if (dsSStructure.Tables[0].DefaultView.Count > 0)
                    //{
                    //    sheet[ROW, colGross].Number = clsStaticInfo.dbl(dsSStructure.Tables[0].DefaultView[0]["Amount"].ToString());
                    //}

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
               
                reportUtility.PlantHeader(ref sheet, endCol, "Increment Summary Report",identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "IncrementSummaryReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }




    }
}
