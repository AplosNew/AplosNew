using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee.BankInformationUploadXL
{
    public class clsTemplateDownloadBankInformation
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
                    var un = ds.Tables[0].Rows[i]["BankBranchList"].ToString();
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
                strSQL = @"SELECT e.SystemId,e.EmployeeCode,e.EmployeeName,e.JobLocationID
                                 from EmployeeInformation e
                                 where e.PlantId='" + plantid + "' AND e.PaymentMode='Bank' ";
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
        public void GetBankInformation( out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT bb.Id BankBranchId,bb.BankId,bb.UserName BankBranchName,b.UserName BankName ,bb.UserName+' ('+b.UserName+')_#'+bb.Id BankBranchList from hkp.Bank b
                           LEFT JOIN  hkp.BankBranch bb on bb.BankId=b.Id
                           ---where bb.Active=1 and b.Active=1  
                           ORDER BY bb.UserName";
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
            DataSet dsBankInformation;            
            DataSet dsEmp;
            int maxRow = 5001;

            #endregion
            try
            {
                //sorting
                //lock      
                GetEmployeeInformation(PlantId, out dsEmp);
                GetBankInformation(out dsBankInformation);
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


                CreateSource(dsBankInformation, 1, "LeaveType", ref sheetSource); int LeaveType = 1;






                #region ------------------Column Header------------------
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpSystemID", ExcelKnownColors.Red);
                int EmpSystemIDCol = xlsCol;
                xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode", ExcelKnownColors.Red);
                int EmployeeCodeCol = xlsCol;
                sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BankName", ExcelKnownColors.Red);
                ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, LeaveType, dsBankInformation.Tables[0].Rows.Count); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BankAccNo", ExcelKnownColors.Red); xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SalaryPercentage", ExcelKnownColors.Red); xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IFSCCode"); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MICRCode"); xlsCol += 1;


               

             
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
