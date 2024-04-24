using System;
using System.Web.Mvc;
using Aplos.Controllers;
using System.Collections.Generic;
using Library.HumanResource.NewOTProcess;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using System.IO;
using Library.HumanResource.NewAttendanceProcess;
using Library.Security.Core;
using Aplos.Properties;

namespace Aplos.Areas.HumanResource.Controllers
{

    public class OTConfirmationProcessController : BaseController
    {
        
        #region Constructor
        
        OTConfirmationProcessService ot = new OTConfirmationProcessService();
        public OTConfirmationProcessController()
        {
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult OTApprove()
        {
            return View();
        }

        #endregion -- Pages


        #region Operations

        [Authorize , HttpGet]
        public ActionResult getFilters()
        {
            return Json(ot.getFilters(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getDayTypes()
        {
            return Json(ot.getDayTypes(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetWorkDateRange(string Year, string Month, string Week)
        {
            return Json(ot.GetWorkDateRange(Year,Month,Week), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult getGridData(string Week, string FromDate, string ToDate, Dictionary<string , string> Parameters)
        {
            var json = Json(ot.getGridData(Week, FromDate, ToDate,  Parameters), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost , Authorize]
        public ActionResult ProcessData(string Data,string OTWeek , string SelectedOT)
        {
            try
            {
                ot.ProcessData(Data, OTWeek , SelectedOT);             
            }
            catch (Exception ex)
            {
                ot.CommonLogFunction(ex);
                return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "OT Confirmation Process Ran Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        // Report Operations
        [HttpPost, Authorize]
        public ActionResult getReportData(string Week, string FromDate, string ToDate, string OTConfirmationValue, string OTLimit, string Process, string ProcessValue, string DayStatus
        , string DSApp, Dictionary<string, string> Parameters)
        {
            var json = Json(ot.getReportData(Week, FromDate, ToDate, OTConfirmationValue, OTLimit, Process, ProcessValue, DayStatus
                            , DSApp, Parameters) , JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }



        [HttpPost]
        public ActionResult getOTReportDownload(string Week, string FromDate, string ToDate, string OTConfirmationValue, string OTLimit, string Process, string ProcessValue, string DayStatus
        , string DSApp, Dictionary<string, string> Parameters)
        {

            try
            {
                var workbook = GetFilterData(Week, FromDate, ToDate, OTConfirmationValue, OTLimit, Process, ProcessValue, DayStatus
                            , DSApp, Parameters);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + "-" + "OTReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IWorkbook GetFilterData(string Week, string FromDate, string ToDate, string OTConfirmationValue, string OTLimit, string Process, string ProcessValue, string DayStatus
        , string DSApp, Dictionary<string, string> Parameters)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "OT Confirmation Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            DataTable dtData = ot.getReportDownload(Week, FromDate, ToDate, OTConfirmationValue, OTLimit, Process, ProcessValue, DayStatus
                            , DSApp, Parameters);


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 13, ExcelHAlign.HAlignCenter);
            int ColCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 13, ExcelHAlign.HAlignCenter);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 13, ExcelHAlign.HAlignCenter);
            int ColDept = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 13, ExcelHAlign.HAlignCenter);
            int ColSec = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SubSection", 13, ExcelHAlign.HAlignCenter);
            int ColSSec = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 13, ExcelHAlign.HAlignCenter);
            int ColDesg = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "WorkDate", 13, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DayStatus", 15, ExcelHAlign.HAlignCenter);
            int ColDStat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "InTime", 13, ExcelHAlign.HAlignCenter);
            int ColIn = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "OutTime", 13, ExcelHAlign.HAlignCenter);
            int ColOut = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ProcessedOT", 13, ExcelHAlign.HAlignCenter);
            int ColPOT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "TargetOT", 13, ExcelHAlign.HAlignCenter);
            int ColTOT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PlanOT", 13, ExcelHAlign.HAlignCenter);
            int ColPlOT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "AdditionalOT", 13, ExcelHAlign.HAlignCenter);
            int ColAOT = COL;
            COL++; 

            report.SetHeaderText(ref sheet, ROW, COL, "StandardOT", 13, ExcelHAlign.HAlignCenter);
            int ColSOT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OTWeek", 13, ExcelHAlign.HAlignCenter);
            int ColWeek = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OTMonth", 13, ExcelHAlign.HAlignCenter);
            int ColMonth = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OTYear", 13, ExcelHAlign.HAlignCenter);
            int ColYear = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                sheet[ROW, ColCode].Text = dtData.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColPlant].Text = dtData.Rows[i]["Plant"].ToString();
                sheet[ROW, ColDept].Text = dtData.Rows[i]["Department"].ToString();
                sheet[ROW, ColSec].Text = dtData.Rows[i]["Section"].ToString();
                sheet[ROW, ColSSec].Text = dtData.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDesg].Text = dtData.Rows[i]["Designation"].ToString();
                sheet[ROW, ColDate].Text = dtData.Rows[i]["WorkDate"].ToString();
                sheet[ROW, ColDStat].Text = dtData.Rows[i]["DayStatus"].ToString();
                sheet[ROW, ColIn].Text = dtData.Rows[i]["InTime"].ToString();
                sheet[ROW, ColOut].Text = dtData.Rows[i]["OutTime"].ToString();
                sheet[ROW, ColPOT].Text = dtData.Rows[i]["ProcessedOT"].ToString();
                sheet[ROW, ColTOT].Text = dtData.Rows[i]["TargetOT"].ToString();
                sheet[ROW, ColPlOT].Text = dtData.Rows[i]["PlanOT"].ToString();
                sheet[ROW, ColAOT].Text = dtData.Rows[i]["AdditionalOT"].ToString();
                sheet[ROW, ColSOT].Text = dtData.Rows[i]["StandardOT"].ToString();
                sheet[ROW, ColWeek].Text = dtData.Rows[i]["OTWeek"].ToString();
                sheet[ROW, ColMonth].Text = dtData.Rows[i]["OTMonth"].ToString();
                sheet[ROW, ColYear].Text = dtData.Rows[i]["OTYear"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "OT Confirmation Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }


        [HttpPost, Authorize]
        public ActionResult GetOTData(string Data)
        {
            try
            {
                //ot.ProcessData(Data, OTWeek, SelectedOT);
            }
            catch (Exception ex)
            {
                ot.CommonLogFunction(ex);
                return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "OT Confirmation Process Ran Successfully..." }, JsonRequestBehavior.AllowGet);

        }



        #endregion Operations

        #region OTApprove

        [HttpGet, Authorize]
        public ActionResult GetWorkOverStayData(string workDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var json = Json(ot.GetWorkOverStayData(workDate, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        private string GetOTPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(OTfromApp), out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult SaveOTData(Dictionary<string, object> data, IEnumerable<OTfromAppNew> SaveMultipleEmpOTExcel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet EmpExistOrNot;
                DataSet EmpDayStatus;
                DataSet IsEmpSalaryLocked;
                DataSet EmpExistInAttProData;
                string RowsEdit = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var empdetails = "' '";
                var empworkingdates = "''";
                var empcode = "''";
                foreach (var empitem in SaveMultipleEmpOTExcel)
                {
                    empdetails += ",'" + empitem.EmployeeSystemId + "' ";
                    empworkingdates += ",'" + empitem.APDEmpWorkDate + "' ";
                    //     empcode += ",'" + empitem.EmployeeCode + "' ";
                }
                con.OpenDataSetThroughAdapter("select * from dbo.OTfromApp where EmpSystemId IN ( " + empdetails + " ) and WorkDate IN (" + empworkingdates + ")  ", out EmpExistOrNot, false, "1");
                con.OpenDataSetThroughAdapter("select * from AttdnProcessData where EmpSystemId IN ( " + empdetails + " ) and WorkDate IN (" + empworkingdates + ") ", out EmpExistInAttProData, false, "1");

                string EmpYear = Convert.ToDateTime(data["FromDate"]).ToString("yyyy");
                string EmpMonth = Convert.ToDateTime(data["FromDate"]).ToString("MM");
                con.OpenDataSetThroughAdapter("select Id, EmpSystemId, YearNo, MonthNo, IsLocked from SalaryLock where YearNo = '" + EmpYear + "' and MonthNo = '" + EmpMonth + "' and EmpSystemId IN ( " + empdetails + " ) ", out IsEmpSalaryLocked, false, "1");

                foreach (var item in SaveMultipleEmpOTExcel)
                {
                    IsEmpSalaryLocked.Tables[0].DefaultView.RowFilter = "EmpSystemId='" + item.EmployeeSystemId + "'";
                    bool islocked = false;
                    if (IsEmpSalaryLocked.Tables[0].DefaultView.Count > 0)
                    {
                        islocked = bplib.clsWebLib.GetBoolData(IsEmpSalaryLocked.Tables[0].DefaultView[0]["IsLocked"].ToString());

                    }
                    if (islocked == false)
                    {
                        //  EmpDayStatus.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "' and WorkDate='" + item.WorkDate + "' ";

                        //if (EmpDayStatus.Tables[0].DefaultView.Count > 0)
                        //{
                        //    if (EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Present" || EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Late" || EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Weekend" || EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Holiday")

                        //    {

                        string newformat = Convert.ToDateTime(item.APDEmpWorkDate).ToString("yyyyMMdd");

                        //        EmpExistInAttProData.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "' and WorkDate='" + item.APDEmpWorkDate + "' ";
                        EmpExistInAttProData.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "' and WorkDate='" + item.APDEmpWorkDate + "' and RowId='" + newformat + item.EmployeeSystemId + "' ";

                        if (EmpExistInAttProData.Tables[0].DefaultView.Count != 0)
                        {
                            bool ManFlag = true;
                            //edit
                            DataRow dr = EmpExistInAttProData.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["ManualOt"] = item.OTHr;

                            dr["ManualByWhom"] = identity.Name;
                            dr["ManualEntryTime"] = System.DateTime.Now.ToString();
                            dr["ManualFlag"] = ManFlag;

                            dr["OTComfirmBy"] = DBNull.Value;
                            dr["DateOTComfirm"] = DBNull.Value;
                            dr["IsOTComfirm"] = false;

                            dr["PlanOT"] = DBNull.Value;
                            dr["AllowedOTLimit"] = DBNull.Value;
                            dr["AppliedOTLimit"] = DBNull.Value;
                            dr["StandardOT"] = DBNull.Value;
                            dr["AdditionalOT"] = DBNull.Value;
                            dr["TargetOT"] = DBNull.Value;

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();


                            dr.EndEdit();
                            RowsEdit = RowsEdit + ",'" + dr["RowId"].ToString() + "'";

                        }
                        else
                        {
                            EmpExistOrNot.Tables[0].DefaultView.RowFilter = "EmpSystemId ='" + item.EmployeeSystemId + "' and WorkDate='" + item.APDEmpWorkDate + "' ";

                            //     EmpExistInAttProData.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "' and WorkDate='" + item.APDEmpWorkDate + "' and RowId='" + newformat + item.EmployeeSystemId + "' ";

                            if (EmpExistOrNot.Tables[0].DefaultView.Count > 0)
                            {

                                //edit
                                DataRow drr = EmpExistOrNot.Tables[0].DefaultView[0].Row;

                                drr.BeginEdit();

                                drr["WorkDate"] = item.APDEmpWorkDate;

                                drr["OThour"] = item.OTHr;
                                drr["EmpSystemId"] = item.EmployeeSystemId;

                                drr["Remarks"] = data["Remarks"];
                                drr["IsConfirmed"] = data["IsConfirmed"];

                                drr["AddedBy"] = identity.Name;
                                drr["AddedDate"] = System.DateTime.Now.ToString();

                                drr["UpdatedBy"] = identity.Name;
                                drr["UpdatedDate"] = System.DateTime.Now.ToString();


                                drr.EndEdit();
                                //        RowsEdit = RowsEdit + ",'" + drr["RowId"].ToString() + "'";


                            }
                            if (EmpExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = EmpExistOrNot.Tables[0].NewRow();
                                dr["Id"] = "OT" + GetOTPK();

                                dr["WorkDate"] = item.APDEmpWorkDate;

                                dr["OThour"] = item.OTHr;
                                dr["EmpSystemId"] = item.EmployeeSystemId;

                                dr["Remarks"] = data["Remarks"];
                                dr["IsConfirmed"] = data["IsConfirmed"];

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();


                                EmpExistOrNot.Tables[0].Rows.Add(dr);
                                //        RowsEdit = RowsEdit + ",'" + dr["RowId"].ToString() + "'";
                            }

                        }
                        //         }
                        //       }

                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(EmpExistInAttProData, EmpExistOrNot);

                NewAttendanceProcessService ap = new NewAttendanceProcessService();
                ap.ManualScheduler(identity.PlantId, RowsEdit);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        #endregion


    }
} 