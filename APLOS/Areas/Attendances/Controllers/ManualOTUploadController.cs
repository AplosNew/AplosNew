#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.OrderManagement.OrderControl;
using System.IO;
using Library.Data;
using Library.Service.Helpers;


#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class ManualOTUploadController : BaseController
    {
        string TableName = "dbo.OTfromApp";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public ManualOTUploadController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + "  "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getplant()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_sqlRepository.GetDataCollection("select Id as Value, UserName as Text from ORG.Plant where CompanyId='" + identity.CompanyId + @"' order by UserName "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.OTfromApp where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string sql = @"select distinct ot.*,FORMAT(ot.WorkDate,'dd-MMM-yyyy') as OTWorkDate,ei.SystemId,ei.EmployeeCode, ei.EmployeeName as EmpName,ei.EmployeeStatus
                                                                    from dbo.OTfromApp ot
                                                                    left join dbo.EmployeeInformation ei on ei.SystemId=ot.EmpSystemId
																    WHERE " + strkey + " order by ot.WorkDate desc ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        private string GetOTPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(OTfromApp), out sID);
            return sID;
        }
        

        [HttpPost, Authorize]
        public ActionResult LoadAllEmpDetailsForSelection(string ToDate, string FromDate, string Id, string PlantId, IEnumerable<OTfromAppExcelData> GetValuesOfExcel)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var empcodedetails = "' '";
                var empworkdates = "''";
                foreach (var get in GetValuesOfExcel)
                {
                    empcodedetails += ",'" + get.EmployeeCode + "' ";
                    empworkdates += ",'" + get.WorkingDate + "' ";
                }

                string sql = "";
                if (!string.IsNullOrEmpty(ToDate) && !string.IsNullOrEmpty(FromDate))
                {
                    sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                            EMP.EmployeeName,EMP.EmployeeCode AS Code,apd.InTime as EMPAPDInTime, CONVERT(varchar(5),apd.[InTime],108)[APDInTime],apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5),apd.[OutTime],108)[APDOutTime], apd.OTHr,apd.WorkDate, FORMAT(apd.WorkDate,'dd-MMM-yyyy') as APDEmpWorkDate,
							apd.DayStatus,dt.Category,
                            EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant,PL.Id as PlantId
                            ,mo.OThour as ManualOT
                            ,dmc.IsOTEntitled
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId =D.Id
                            left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId=dd.DesignationMasterId and dmc.PlantId=EMP.PlantId
                            left join AttdnProcessData apd on apd.EmpSystemID=EMP.SystemId
							left join DayType dt on dt.DayType=apd.DayStatus
                            left join OTfromApp mo on mo.EmpSystemID=emp.SystemId and mo.WorkDate=apd.WorkDate
                            WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.doj<='" + ToDate + @"' and (dos is null or dos>='" + FromDate + @"')                   
                            And EMP.PlantId='" + PlantId + @"'  and EMP.EmployeeCode IN ("+ empcodedetails + ") and apd.WorkDate IN (" + empworkdates + ") ";

                }
            

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpGet, Authorize]
        public JsonResult LoadEmpOfShiftWorkDate(string EmpWorkDate)
        {

            string sql = @"select ot.*, ei.SystemId,ei.EmployeeCode as Code,ei.EmployeeName, ei.EmployeeStatus
                                    , ot.OThour as OTHr, FORMAT(ot.WorkDate,'dd-MMM-yyyy') as APDEmpWorkDate
									,CONVERT(varchar(5),apd.[InTime],108)[APDInTime], CONVERT(varchar(5),apd.[OutTime],108)[APDOutTime]
									from dbo.OTfromApp ot left join dbo.EmployeeInformation ei on ei.SystemId=ot.EmpSystemId
									left join AttdnProcessData apd on apd.EmpSystemID=ot.EmpSystemId
									where apd.WorkDate='" + EmpWorkDate + "' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }



        [HttpPost, Authorize]
        public JsonResult ImportData()
        {
            string path;
        
            try
            {
      
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public List<OTfromAppExcelData> ReadData(string plantid, string path)
        {
            List<OTfromAppExcelData> data = null;
     
            DataSet dsExcel = null;
            try
            {
                data = new List<OTfromAppExcelData>();
            
                ReadFile(path, out dsExcel);
  
                data = dsExcel.Tables[0].ToList<OTfromAppExcelData>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ReadFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    //exception += "\r\nTrying to delete";
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }


        public void SaveFile(out string path)
        {
            path = "";
            try
            {
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetOTManualFile(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult SaveExcelData(Dictionary<string, object> data, IEnumerable<OTfromApp> SaveMultipleEmpOTExcel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet EmpExistOrNot;
                DataSet EmpDayStatus;
                DataSet IsEmpSalaryLocked;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var empdetails = "' '";
                var empworkingdates = "''";
                foreach (var empitem in SaveMultipleEmpOTExcel)
                {
                    empdetails += ",'" + empitem.EmployeeSystemId + "' ";
                    empworkingdates += ",'" + empitem.APDEmpWorkDate + "' ";
                }
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where EmpSystemId IN ( " + empdetails + " ) and WorkDate IN ("+ empworkingdates + ")  ", out EmpExistOrNot, false, "1");
                con.OpenDataSetThroughAdapter("select apd.EmpSystemID,apd.WorkDate,apd.DayStatus, dt.Category from AttdnProcessData apd left join DayType dt on apd.DayStatus=dt.DayType where apd.EmpSystemID IN ( " + empdetails + " ) and apd.WorkDate IN (" + empworkingdates + ") ", out EmpDayStatus, false, "1");

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
                        EmpDayStatus.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "' and WorkDate='" + item.WorkDate + "' ";

                        if (EmpDayStatus.Tables[0].DefaultView.Count > 0)
                        {
                            if (EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Present" || EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Late" || EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Weekend" || EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Holiday")

                            {


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

                                }
                                else
                                {
                                    EmpExistOrNot.Tables[0].DefaultView.RowFilter = "EmpSystemId ='" + item.EmployeeSystemId + "' and WorkDate='" + item.WorkDate + "' ";
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

                                       
                                    }
                                    if(EmpExistOrNot.Tables[0].DefaultView.Count == 0)
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
                                    }
                                  
                                }
                            }
                        }



                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(EmpExistOrNot);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        // Get Sample Report

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }

        [HttpGet, Authorize]
        public ActionResult GetSampleReport(ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " OTManualSampleUpload";
            var workbook = GetManualOTWorkSheet();
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetManualOTWorkSheet()
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "ManualOTUpload";


            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCode", 12, ExcelHAlign.HAlignLeft);
            int ColPlotNameNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OTHour", 8, ExcelHAlign.HAlignLeft);
            int ColFmpPlotStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "WorkingDate", 12, ExcelHAlign.HAlignRight);
            int ColPlotArea = COL;
            ROW++;

            endCol = COL;
            #endregion Headers

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }
    }

}

public class OTfromAppExcelData
{

    #region Scalar Properties

    public string EmployeeCode { get; set; }
    public string OTHour { get; set; }
    public string WorkingDate { get; set; }


    #endregion Scalar Properties

    #region Audit Properties


    //[NeverUpdate]
    //public string AddedBy { get; set; }


    //[NeverUpdate]
    //public DateTime AddedDate { get; set; }


    //[NeverUpdate]
    //public string AddedFromIP { get; set; }


    //public string UpdatedBy { get; set; }



    //public DateTime? UpdatedDate { get; set; }



    //public string UpdatedFromIP { get; set; }

    #endregion Audit Properties
}