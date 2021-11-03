using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance.Manual
{
    public class clsManualAttendanceFileUpload
    {
        ISqlRepository _sqlRepository;
        public clsManualAttendanceFileUpload()
        {
            _sqlRepository = new SqlRepository();
        }
        public IWorkbook GetSampleFile(string Name)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();
                DataSet dsShift = null;
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                var iEmployeeCode = 0;
                var iShiftName = 0;
                var iWorkDate = 0;
                var iShiftId = 0;
                var iDayStatus = 0;
                var isl = 0;
                var iInTime = 0;
                var iInDate = 0;
                var iOutTime = 0;
                var iOutDate = 0;
                var iShiftInTime = 0;
                var iShiftOutTime = 0;
                                

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                xlsRow = 1;
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[0];

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "EmpSystemId";
                sheet1.Range[xlsRow, isl].ColumnWidth = 18;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "EmployeeCode";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 18;

                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "WorkDate";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                xlsCol += 1;
                iDayStatus = xlsCol;
                sheet1.Range[xlsRow, iDayStatus].Text = "DayStatus";
                sheet1.Range[xlsRow, iDayStatus].ColumnWidth = 18;

                xlsCol += 1;
                iShiftId = xlsCol;
                sheet1.Range[xlsRow, iShiftId].Text = "ShiftSystemID";
                sheet1.Range[xlsRow, iShiftId].ColumnWidth = 18;

                xlsCol += 1;
                iShiftName = xlsCol;
                sheet1.Range[xlsRow, iShiftName].Text = "ShiftName";
                sheet1.Range[xlsRow, iShiftName].ColumnWidth = 36;

                xlsCol += 1;
                iShiftInTime = xlsCol;
                sheet1.Range[xlsRow, iShiftInTime].Text = "InTime";
                sheet1.Range[xlsRow, iShiftInTime].ColumnWidth = 20;

                xlsCol += 1;
                iShiftOutTime = xlsCol;
                sheet1.Range[xlsRow, iShiftOutTime].Text = "OutTime";
                sheet1.Range[xlsRow, iShiftOutTime].ColumnWidth = 20;

                xlsCol += 1;
                iInDate = xlsCol;
                sheet1.Range[xlsRow, iInDate].Text = "Reason";
                sheet1.Range[xlsRow, iInDate].ColumnWidth = 20;

                xlsCol += 1;
                iInTime = xlsCol;
                sheet1.Range[xlsRow, iInTime].Text = "ProposedIntime";
                sheet1.Range[xlsRow, iInTime].ColumnWidth = 20;

                xlsCol += 1;
                iOutDate = xlsCol;
                sheet1.Range[xlsRow, iOutDate].Text = "ProposedOutTime";
                sheet1.Range[xlsRow, iOutDate].ColumnWidth = 20;                

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                #region data in column

                int ROW = 1;
                int endCol = 1;
                int COL = 1;
                var startRow = 0;

                int RowIndex = ROW;
                startRow = ROW;
                ROW++;

                #endregion

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


                #endregion  Lunch Out

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Save(string filename,string extension, ManualAttdnFile file, out DataSet dsMaster)
        {
            try
            {
                
                GetData(file.Id, out dsMaster);
                _Save(ref dsMaster, filename, extension, file);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region S a v e 
        void _Save(ref DataSet dsSaveBonusMaster, string filename, string extension, ManualAttdnFile ui_master)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);
                _dvSave.RowFilter = "Id ='" + ui_master.Id + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveBonusMaster.Tables[0].NewRow();
                    _SaveCol("ADDNEW", filename, extension, ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _SaveCol("Edit", filename, extension, ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _SaveCol(string OPN_FLAG, string filename, string extension, ManualAttdnFile ui_master, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string idFromDB = "";
            string systemID = "";

            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "File", out idFromDB);
                    //systemID =  idFromDB;
                    //ui_master.Id = systemID.Trim();
                    drLocal["Id"] = bplib.clsWebLib.RetValidLen(idFromDB);
                    drLocal["FileId"] = idFromDB + extension;
                    drLocal["FileName"] = filename;
                    drLocal["FileStatus"] = "Uploaded";
                    drLocal["PlantId"] = ui_master.PlantId;                                      

                    drLocal["AddedBy"] = identity.Name;
                    drLocal["AddedFromIP"] = identity.IPAddress;
                    drLocal["AddedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);

                }
                else
                {
                    

                    drLocal["UpdatedBy"] = ui_master.AddedBy;
                    drLocal["UpdatedFromIP"] = identity.IPAddress;
                    drLocal["UpdatedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
        #endregion S a v e Tax Policy Master

        public void GetData(string MasterID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from ManualAttdnFile where Id='"+ MasterID + "' ";
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
        public IEnumerable<object> GetMaster(string PlantId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select Id, FileId,FileName,FileStatus,AddedBy,FORMAT(AddedDate,'dd-MMM-yyyy')AddedDate From ManualAttdnFile  where PlantId ='" + PlantId+"'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
    }
}
public class ManualAttdnFile
{
    public string Id { get; set; }
    public string FileId { get; set; }
    public string FileName { get; set; }
    public string FileStatus { get; set; }
    public string PlantId { get; set; }
    public string AddedBy { get; set; }
    public string AddedDate { get; set; }
    public string AddedFromIP { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedDate { get; set; }
    public string UpdatedFromIP { get; set; }

}