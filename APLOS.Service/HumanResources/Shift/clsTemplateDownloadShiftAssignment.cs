using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;

namespace Library.Service.HumanResources.Shift
{
   public class clsTemplateDownloadShiftAssignment
    {    
        public void GetShift(string PlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT UserName+'_#'+id UserName FROM scs.District where active=1 order by UserName";
                strSQL = @"select ShiftDefinationDescription +'_#'+ SystemId UserName from ShiftDefination where IsActive=1  and PlantID='"+ PlantID + "' order by ShiftDefinationDescription";
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
        public void GetRosterMaster(string PlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT UserName+'_#'+id UserName FROM scs.District where active=1 order by UserName";
                strSQL = @" select ShiftRosterName +'_#'+ SystemId UserName from ShiftRosterMaster where PlantID='" + PlantID+"' order by ShiftRosterName";
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
        public void GetRosterChild(string PlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT UserName+'_#'+id UserName FROM scs.District where active=1 order by UserName";
                strSQL = @" select s.ShiftDefinationDescription +'_#'+ s.SystemId UserName from ShiftRosterChild c
                                left join ShiftDefination s on s.SystemID=c.ShiftDefinationID
                                where c.PlantID='" + PlantID + @"' and SRMasterSystemID in (
                                select SystemId from ShiftRosterMaster where PlantID='" + PlantID+@"'
                                )
                                order by SRMasterSystemID,c.ShiftSequence";
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
        public void GetEmployeeInformation(string plantid,out System.Data.DataSet dsRef)
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
        public void CreateSource(string[] Arr, int Col,string Header, ref IWorksheet sheetSource)
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
        public IWorkbook GetSampleFile(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
        {
            #region declare
            clsReport objRpt = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            DataSet dsShift;
            DataSet dsRosterMaster;
            DataSet dsRosterChild;

            #endregion
            DataSet dsEmp;
            try
            {
                //sorting
                //lock               

                GetShift(PlantId,out dsShift);
                GetRosterMaster(PlantId,out dsRosterMaster);
                GetRosterChild(PlantId,out dsRosterChild);
                GetEmployeeInformation(PlantId, out dsEmp);

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


                //CreateSource(dsLegalDesignation, 1, "LegalDesignation", ref sheetSource); int LegalDesignationCol = 1;
                //CreateSource(dsCountry, 2, "Country", ref sheetSource); int CountryCol = 2;
                //CreateSource(dsState, 3, "State", ref sheetSource); int StateCol = 3;
                CreateSource(dsShift, 1, "Shift", ref sheetSource); int ShiftCol = 1;
                CreateSource(dsRosterMaster, 2, "RosterMaster", ref sheetSource); int RosterMasterCol = 2;
                CreateSource(dsRosterChild, 3, "RosterChild", ref sheetSource); int RosterChildCol = 3;
                //CreateSource(dsCity, 5, "City", ref sheetSource); int CityCol = 5;
                //CreateSource(dsBudgetCode, 6, "BudgetCode", ref sheetSource); int BudgetCodeCol = 6;

                //CreateSource(dsSalutation, 7, "Salutation", ref sheetSource); int SalutationCol = 7;
                //CreateSource(dsBloodGroup, 8, "BloodGroup", ref sheetSource); int BloodGroupCol = 8;
                //CreateSource(dsCivilStatus, 9, "CivilStatus", ref sheetSource); int CivilStatusCol = 9;
                //CreateSource(dsReligion, 10, "Religion", ref sheetSource); int ReligionCol = 10;

                //string[] _EmpType = { "Local", "Expatriate" };
                //CreateSource(_EmpType, 11, "EmpType", ref sheetSource);int EmpTypeCol = 11;
                //string[] _EmploymentType = { "Permanent", "Temporary" };
                //CreateSource(_EmploymentType, 12, "EmploymentType", ref sheetSource); int EmploymentTypeCol = 12;
                //string[] _Gender = { "Male", "Female" };
                //CreateSource(_Gender, 13, "Gender", ref sheetSource); int GenderCol = 13;
                //string[] _PaymentMode = { "Bank", "Cash" };
                //CreateSource(_PaymentMode, 14, "PaymentMode", ref sheetSource); int PaymentModeCol = 14;

                //CreateSource(dsJobLocation, 15, "JobLocation", ref sheetSource); int JobLocationCol = 15;






                #region ------------------Column Header------------------
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SystemId");
                int SystemidCol = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode");
                int EmployeeCodeCol = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeName");
                int EmployeeNameCol = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "JobLocation");
                int JobLocationCol = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShiftSystemId");
                ru.SetList(ref sheet1, xlsRow, dsEmp.Tables[0].Rows.Count, xlsCol, sheetSource, ShiftCol, dsShift.Tables[0].Rows.Count);
                int ShiftSystemIdCol = xlsCol; xlsCol += 1;
               
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EffectiveDate"); int EffectiveDateCol = xlsCol; xlsCol += 1;

               

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IsRoster"); int IsRosterCol = xlsCol;
                string[] IsRoster = { "YES", "NO" };
                ru.SetList(ref sheet1, xlsRow + 1, dsEmp.Tables[0].Rows.Count, xlsCol, IsRoster); xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "RosterSystemID");
                ru.SetList(ref sheet1, xlsRow, dsEmp.Tables[0].Rows.Count, xlsCol, sheetSource, RosterMasterCol, dsRosterMaster.Tables[0].Rows.Count);
                int RosterSystemIDCol = xlsCol; xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "RosterStartShiftID");
                ru.SetList(ref sheet1, xlsRow, dsEmp.Tables[0].Rows.Count, xlsCol, sheetSource, RosterChildCol, dsRosterChild.Tables[0].Rows.Count);
                int RosterStartShiftIDCol = xlsCol;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShiftEffectiveDate", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "RosterShiftName");
                //ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, dsRoster); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "AssignShiftName", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, dsShift); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "WeekOffEffectiveDate", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "AlignWithCompany", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IndividualWeekOff", ExcelKnownColors.Red);
                //string[] _IndividualWeekOff = { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
                //ru.SetList(ref sheet1, xlsRow + 1, maxRow, xlsCol, _IndividualWeekOff); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "JobLocation", ExcelKnownColors.Red);
                //ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, JobLocationCol, dsJobLocation.Tables[0].Rows.Count); xlsCol += 1;
                endXlsCol = xlsCol;

                for (int i = 0; i < dsEmp.Tables[0].Rows.Count; i++)//
                {
                    xlsRow++;
                    ru.SetText(ref sheet1, xlsRow, SystemidCol, dsEmp.Tables[0].Rows[i]["SystemId"].ToString()); /*xlsCol += 1;*/
                    ru.SetText(ref sheet1, xlsRow, EmployeeCodeCol, dsEmp.Tables[0].Rows[i]["EmployeeCode"].ToString()); /*xlsCol += 1;*/
                    ru.SetText(ref sheet1, xlsRow,EmployeeNameCol, dsEmp.Tables[0].Rows[i]["EmployeeName"].ToString()); /*xlsCol += 1;*/
                    ru.SetText(ref sheet1, xlsRow, JobLocationCol, dsEmp.Tables[0].Rows[i]["JobLocationId"].ToString()); /*xlsCol += 1;*/
                    //sheet1.Range[xlsRow, EffectiveDateCol].NumberFormat = "dd-MMM-yyyy";
                    //ru.SetText(ref sheet1, xlsRow, IsRosterCol, dsEmp.Tables[0].Rows[i]["IsRoster"].ToString()); xlsCol += 1;
                    //ru.SetText(ref sheet1, xlsRow, RosterSystemIDCol, dsEmp.Tables[0].Rows[i]["RosterSystemID"].ToString()); xlsCol += 1;
                    //ru.SetText(ref sheet1, xlsRow, RosterStartShiftIDCol, dsEmp.Tables[0].Rows[i]["RosterStartShiftID"].ToString()); 
                }



                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

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

                sheetSource.Protect("2020", ExcelSheetProtection.Content);


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
