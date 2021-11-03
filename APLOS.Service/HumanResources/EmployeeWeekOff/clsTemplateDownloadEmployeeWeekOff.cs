using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;

namespace Library.Service.HumanResources.Shift
{
   public class clsTemplateDownloadEmployeeWeekOff
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
                strSQL = @" select SystemId Id,ShiftRosterName UserName from ShiftRosterMaster where PlantID='"+PlantID+"' order by ShiftRosterName";
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
                strSQL = @" select s.SystemId Id,s.ShiftDefinationDescription UserName from ShiftRosterChild c
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

                //GetShift(PlantId,out dsShift);
                //GetRosterMaster(PlantId,out dsRosterMaster);
                //GetRosterChild(PlantId,out dsRosterChild);
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
                //IWorksheet sheetSource = null;
                //sheetSource = workbook.Worksheets[1];
                xlsRow = 1;




                #region ------------------Column Header------------------
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpSystemID"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpSystemID");
                int SystemidCol = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode");
                int EmployeeCodeCol = xlsCol; xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EffectiveDate"); xlsCol += 1;
               
               

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "AlignWithCC");
                string[] AlignWithCC = { "YES", "No" };
                ru.SetList(ref sheet1, xlsRow + 1, 5000, xlsCol, AlignWithCC); xlsCol += 1;


                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IndividualWeekOff");
                string[] IndividualWeekOff = { "YES", "No" };
                ru.SetList(ref sheet1, xlsRow + 1, 5000, xlsCol, IndividualWeekOff); xlsCol += 1;


                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FstOffDay");
                string[] _IndividualWeekOff = { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
                ru.SetList(ref sheet1, xlsRow + 1, 5000, xlsCol, _IndividualWeekOff); xlsCol += 1;


                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FstDayLengthType");
                //string[] _FstDayLengthType = { "Full Day" };
                //ru.SetList(ref sheet1, xlsRow + 1, 5000, xlsCol, _FstDayLengthType); xlsCol += 1;



                endXlsCol = xlsCol;

                for (int i = 0; i < dsEmp.Tables[0].Rows.Count; i++)//
                {
                    xlsRow++;
                    ru.SetText(ref sheet1, xlsRow, SystemidCol, dsEmp.Tables[0].Rows[i]["SystemId"].ToString()); /*xlsCol += 1;*/
                    ru.SetText(ref sheet1, xlsRow, EmployeeCodeCol, dsEmp.Tables[0].Rows[i]["EmployeeCode"].ToString()); /*xlsCol += 1;*/
                    
                    //ru.SetText(ref sheet1, xlsRow, IsRosterCol, dsEmp.Tables[0].Rows[i]["IsRoster"].ToString()); xlsCol += 1;
                    //ru.SetText(ref sheet1, xlsRow, RosterSystemIDCol, dsEmp.Tables[0].Rows[i]["RosterSystemID"].ToString()); xlsCol += 1;
                    //ru.SetText(ref sheet1, xlsRow, RosterStartShiftIDCol, dsEmp.Tables[0].Rows[i]["RosterStartShiftID"].ToString()); 
                }


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
