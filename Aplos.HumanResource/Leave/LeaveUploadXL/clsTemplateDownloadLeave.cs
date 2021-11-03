using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Leave.LeaveUploadXL
{
    public class clsTemplateDownloadLeave
    {//
        public void GetLeaveType(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT UserName+'_#'+Id UserName  FROM LeaveType order by UserName";
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
              
        public void CreateSource(DataSet ds, int Col, string Header, ref IWorksheet sheetSource)
        {
            try
            {
                ReportUtility ru = new ReportUtility();
                ru.SetText(ref sheetSource, 1, Col, Header);
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    var un = ds.Tables[0].Rows[i]["UserName"].ToString();
                    int k = i + 2;
                    ru.SetText(ref sheetSource, k, Col, un);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void CreateSource(string[] Arr, int Col, string Header, ref IWorksheet sheetSource)
        {
            try
            {
                ReportUtility ru = new ReportUtility();
                ru.SetText(ref sheetSource, 1, Col, Header);
                for (int i = 0; i < Arr.Length; i++)
                {
                    var un = Arr[i].ToString();
                    int k = i + 2;
                    ru.SetText(ref sheetSource, k, Col, un);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetEmployeeInformation(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
                                 e.SystemId,e.EmployeeCode,e.EmployeeName,e.JobLocationID
                                 from EmployeeInformation e
                                 where e.PlantId='" + plantid + "'";
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
        public IWorkbook GetSampleFile(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
        {
            #region declare
            clsReport objRpt = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            DataSet dsLeaveType;            
            DataSet dsEmp;
            int maxRow = 5001;

            #endregion
            try
            {
                //sorting
                //lock      
                GetEmployeeInformation(PlantId, out dsEmp);
                GetLeaveType(out dsLeaveType);
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


                CreateSource(dsLeaveType, 1, "LeaveType", ref sheetSource); int LeaveType = 1;

                //string[] _EmpType = { "Local", "Expatriate" };
                //CreateSource(_EmpType, 11, "EmpType", ref sheetSource); int EmpTypeCol = 11;
                //string[] _EmploymentType = { "Permanent", "Temporary" };
                //CreateSource(_EmploymentType, 12, "EmploymentType", ref sheetSource); int EmploymentTypeCol = 12;
                //string[] _Gender = { "Male", "Female" };
                //CreateSource(_Gender, 13, "Gender", ref sheetSource); int GenderCol = 13;
                //string[] _PaymentMode = { "Bank", "Cash" };
                //CreateSource(_PaymentMode, 14, "PaymentMode", ref sheetSource); int PaymentModeCol = 14;

                //CreateSource(dsJobLocation, 15, "JobLocation", ref sheetSource); int JobLocationCol = 15;






                #region ------------------Column Header------------------
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpSystemID", ExcelKnownColors.Red);
                int EmpSystemIDCol = xlsCol;
                xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode", ExcelKnownColors.Red);
                int EmployeeCodeCol = xlsCol;
                sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LTSystemID", ExcelKnownColors.Red);
                ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, LeaveType, dsLeaveType.Tables[0].Rows.Count); xlsCol += 1;

                sheet1.Range[xlsRow, xlsCol, maxRow, xlsCol].NumberFormat = "dd-MMM-yyyy";
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LvDate", ExcelKnownColors.Red); xlsCol += 1;

                //sheet1.Range[xlsRow, xlsCol, maxRow, xlsCol].NumberFormat = "dd-MMM-yyyy";
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ToDate", ExcelKnownColors.Red); xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LvReason", ExcelKnownColors.Red);
                xlsCol += 1;

                sheet1.Range[xlsRow, xlsCol, maxRow, xlsCol].NumberFormat = "dd-MMM-yyyy";
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "AppliedDate", ExcelKnownColors.Red);

                #region commented
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Salutation", ExcelKnownColors.Red);
                ////ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, dsSalutation); xlsCol += 1;
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, SalutationCol, dsSalutation.Tables[0].Rows.Count); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FirstName", ExcelKnownColors.Red);
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";

                ////IRange range2 = sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol];
                ////range2.Text = "'";
                ////IRichTextString richText = range2.RichText;
                ////richText.RtfText = "012";
                //xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LastName");
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@"; xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FatherName");
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@"; xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MotherName");
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@"; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MaritalStatus");
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, CivilStatusCol, dsCivilStatus.Tables[0].Rows.Count); xlsCol += 1;
                ////ru.SetList(ref sheet1, sheetSource, 9, xlsRow, maxRow, xlsCol, dsCivilStatus); xlsCol += 1;

                ////ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, dsCivilStatus); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SpouseName"); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PresentAddress1"); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PresentAddress2"); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PermanentAddress1"); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PermanentAddress2"); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpType", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, EmpTypeCol, _EmpType.Length); xlsCol += 1;
                ////ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, _EmpType); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmploymentType", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, EmploymentTypeCol, _EmploymentType.Length); xlsCol += 1;

                ////ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, _EmploymentType); xlsCol += 1;//

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Gender", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, GenderCol, _Gender.Length); xlsCol += 1;
                ////ru.SetList(ref sheet1, sheetSource, 13, xlsRow, maxRow, xlsCol, dsReligion); xlsCol += 1;
                ////ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, _Gender); xlsCol += 1;


                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Religion", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, ReligionCol, dsCivilStatus.Tables[0].Rows.Count); xlsCol += 1;
                ////ru.SetList(ref sheet1, sheetSource, 10, xlsRow, maxRow, xlsCol, dsReligion); xlsCol += 1;

                ////ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, dsReligion); xlsCol += 1;


                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BloodGroup");
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, BloodGroupCol, dsBloodGroup.Tables[0].Rows.Count); xlsCol += 1;
                ////ru.SetList(ref sheet1, sheetSource, 8, xlsRow, maxRow, xlsCol, dsBloodGroup); xlsCol += 1;

                ////ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, dsBloodGroup); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PhoneNo");
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@"; xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CardNumber");
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@"; xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "NID", ExcelKnownColors.Red);
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                //xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "NID", ExcelKnownColors.Red);
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                //xlsCol += 1;

                //sheet1.Range[xlsRow, xlsCol, maxRow, xlsCol].NumberFormat = "dd-MMM-yyyy";
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOB", ExcelKnownColors.Red); xlsCol += 1;

                //sheet1.Range[xlsRow, xlsCol, maxRow, xlsCol].NumberFormat = "dd-MMM-yyyy";
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CelebrationDOB"); xlsCol += 1;

                //sheet1.Range[xlsRow, xlsCol, maxRow, xlsCol].NumberFormat = "dd-MMM-yyyy";
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOJ", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PPeriod_Date"); xlsCol += 1;



                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LegalDesignation", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, LegalDesignationCol, dsLegalDesignation.Tables[0].Rows.Count); xlsCol += 1;
                ////ru.SetList(ref sheet1, sheetSource, 1, xlsRow, maxRow, xlsCol, dsLegalDesignation); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BudgetCode", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, BudgetCodeCol, dsBudgetCode.Tables[0].Rows.Count); xlsCol += 1;

                ////ru.SetList(ref sheet1, sheetSource, 6, xlsRow, maxRow, xlsCol, dsBudgetCode); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PaymentMode", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, PaymentModeCol, _PaymentMode.Length); xlsCol += 1;

                ////ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, _PaymentMode); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Country_permanent");
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, CountryCol, dsCountry.Tables[0].Rows.Count); xlsCol += 1;
                ////ru.SetList(ref sheet1, sheetSource, 2, xlsRow, maxRow, xlsCol, dsCountry); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Citizen", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, CountryCol, dsCountry.Tables[0].Rows.Count); xlsCol += 1;


                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "State_Division", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, StateCol, dsState.Tables[0].Rows.Count); xlsCol += 1;
                ////ru.SetList(ref sheet1, sheetSource, 3, xlsRow, maxRow, xlsCol, dsState); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "District", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, DistrictCol, dsDistrict.Tables[0].Rows.Count); xlsCol += 1;
                ////ru.SetList(ref sheet1, sheetSource, 4, xlsRow, maxRow, xlsCol, dsDistrict); xlsCol += 1;


                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "City");
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, CityCol, dsCity.Tables[0].Rows.Count); xlsCol += 1;
                ////ru.SetList(ref sheet1, sheetSource, 5, xlsRow, maxRow, xlsCol, dsCity); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IsConfirmed"); xlsCol += 1; 
                #endregion

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                //xlsRow++;

                #endregion ------------------Column Header------------------

                for (int i = 0; i < dsEmp.Tables[0].Rows.Count; i++)//
                {
                    xlsRow++;
                    ru.SetText(ref sheet1, xlsRow, EmpSystemIDCol, dsEmp.Tables[0].Rows[i]["SystemId"].ToString()); /*xlsCol += 1;*/
                    ru.SetText(ref sheet1, xlsRow, EmployeeCodeCol, dsEmp.Tables[0].Rows[i]["EmployeeCode"].ToString()); /*xlsCol += 1;*/                  
                }

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
