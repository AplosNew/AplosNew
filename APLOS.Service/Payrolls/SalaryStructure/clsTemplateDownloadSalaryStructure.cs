using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;

namespace Library.Service.Payrolls.SalaryStructure
{
   public class clsTemplateDownloadSalaryStructure
    {
        public void GetList(DataSet ds, string ColumnName, out string[] list)
        {
            list = new string[ds.Tables[0].Rows.Count];
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    list[i] = ds.Tables[0].Rows[i][ColumnName].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetList(ref IWorksheet sheet, int frow, int lrow, int col, string[] List)
        {
            try
            {
                IDataValidation validation = sheet.Range[frow, col, lrow, col].DataValidation;
                validation.ListOfValues = List;
                //validation.ListOfValues = new string[] { "ListItem1", "ListItem2", "ListItem3" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetList(ref IWorksheet sheet, IWorksheet sheet_source, int Col_source, int frow, int lrow, int col, DataSet ds)
        {
            try
            {
                IRange irCountry = sheet_source.Range[1, Col_source, ds.Tables[0].Rows.Count, Col_source];
                IDataValidation validationCountry = sheet.Range[frow, col, lrow, col].DataValidation;
                validationCountry.DataRange = irCountry;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetList(ref IWorksheet sheet, int frow, int lrow, int col, DataSet ds, string ColumnName = "UserName")
        {
            try
            {
                string[] _list;
                GetList(ds, ColumnName, out _list);
                IDataValidation validation = sheet.Range[frow, col, lrow, col].DataValidation;
                //IRange ir = sheet.Range[frow, col, lrow, col];
                validation.ListOfValues = _list;
                //validation.DataRange = ir;
                //validation.ListOfValues = new string[] { "ListItem1", "ListItem2", "ListItem3" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, ExcelKnownColors Fontcolor)
        {
            sheet.Range[row, col].Text = txt;
            //sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].CellStyle.Font.Color = Fontcolor;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }
        public void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelKnownColors color)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].CellStyle.ColorIndex = color;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }
        public void GetSalaryHead(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SalaryHead+'_#'+SalaryHeadID UserName  FROM SalaryHead  order by UserName";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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
        public void GetCurrency(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Name+'_#'+Id UserName  FROM [SCS].[Currency] WHERE [Active]=1   order by UserName";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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

        public void GetSalaryRule(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SalaryRuleName+'_#'+SystemID UserName  FROM SalaryRuleMaster WHERE IsActive=1 AND PlantID='"+PlantId+@"'   order by UserName";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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
      



        public void CreateSource(DataSet ds, int Col, ref IWorksheet sheetSource)
        {
            try
            {
                ReportUtility ru = new ReportUtility();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    var un = ds.Tables[0].Rows[i]["UserName"].ToString();
                    int k = i + 1;
                    ru.SetText(ref sheetSource, k, Col, un);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IWorkbook GetSampleFile(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
        {
            #region declare
            clsReport objRpt = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            DataSet dsSalaryRule;
            DataSet dsCurrency;
            DataSet dsSalaryHead;
    

            #endregion
            try
            {
                //sorting
                //lock               
               
                GetCurrency(out dsCurrency);
                GetSalaryHead( out dsSalaryHead);
          
                GetSalaryRule(PlantId, out dsSalaryRule);

                if (dsSalaryHead.Tables[0].Rows.Count==0)
                {
                    throw new Exception("Salary Head not found.");
                }
                if (dsSalaryRule.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("Salary Rule not found.");
                }
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];
                xlsRow = 1;

                CreateSource(dsSalaryRule, 1, ref sheetSource);
                //CreateSource(dsCurrency, 2, ref sheetSource);
                CreateSource(dsSalaryHead, 3, ref sheetSource);
              


                #region ------------------Column Header------------------

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpSystemID", ExcelKnownColors.Red); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpSystemID"); xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SalaryRuleMasterSystemID");
                SetList(ref sheet1, sheetSource, 1, xlsRow + 1, xlsRow + 5000, xlsCol, dsSalaryRule); xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EffectiveDate"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "NextDueDate"); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FatherName"); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MotherName"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SalaryHeadID");
                SetList(ref sheet1, sheetSource,3, xlsRow + 1, xlsRow + 5000, xlsCol, dsSalaryHead); xlsCol += 1;


                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EntryCurrencyID");
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, dsCurrency); xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EntryAmount"); xlsCol += 1;


                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SequenceNo"); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SalaryCategory", ExcelKnownColors.Red);
                //string[] _Gender = { "Male", "Female" };



                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, _Gender); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PresentAddress2"); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PermanentAddress1"); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PermanentAddress2"); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpType", ExcelKnownColors.Red);
                //string[] _EmpType = { "Local", "Expatriate" };
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, _EmpType); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmploymentType", ExcelKnownColors.Red);
                //string[] _EmploymentType = { "Permanent", "Temporary" };
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, _EmploymentType); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Gender", ExcelKnownColors.Red);
                //string[] _Gender = { "Male", "Female" };
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, _Gender); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Religion", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, dsReligion); xlsCol += 1;


                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BloodGroup");
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, dsBloodGroup); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PhoneNo"); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CardNumber"); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "NID", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOB", ExcelKnownColors.Red); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CelebrationDOB"); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOJ", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PPeriod"); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShiftEffectiveDate", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "RosterShiftName");
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, dsRoster); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "AssignShiftName", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, dsShift); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "WeekOffEffectiveDate", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "AlignWithCompany", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IndividualWeekOff", ExcelKnownColors.Red);
                //string[] _IndividualWeekOff = { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, _IndividualWeekOff); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "JobLocation", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, dsJobLocation); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LegalDesignation", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, sheetSource, 1, xlsRow, xlsRow + 5000, xlsCol, dsLegalDesignation); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BudgetCode", ExcelKnownColors.Red); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PaymentMode", ExcelKnownColors.Red);
                //string[] _PaymentMode = { "Bank", "Cash" };
                //ru.SetList(ref sheet1, xlsRow + 1, xlsRow + 5000, xlsCol, _PaymentMode); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Country", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, sheetSource, 2, xlsRow, xlsRow + 5000, xlsCol, dsCountry); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "State_Division", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, sheetSource, 3, xlsRow, xlsRow + 5000, xlsCol, dsState); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "District", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, sheetSource, 4, xlsRow, xlsRow + 5000, xlsCol, dsDistrict); xlsCol += 1;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup

                //sheetSource.Protect("2020", ExcelSheetProtection.Content);


                #endregion  Lunch Out

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
