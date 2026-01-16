using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Attendances;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.Core;
//using TBS;

namespace Aplos.Areas.HumanResource.Controllers
{

    public class RosterPatternController : BaseController
    {
        // add a header verification - 1. Basic Authentication .... 2. Payload

        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;

        RosterPatternService rs = new RosterPatternService();
        public RosterPatternController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion



        #region Aplos       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        //#region -- Operations


        [HttpGet, Authorize]
        public ActionResult SearchShift(string PlantId)
        {
            try
            {

                var data = rs.ShiftDefinationSearch(PlantId);
                return Json(new { Data = data }, JsonRequestBehavior.AllowGet);
                //return Json(new { LeaveInfo = "Saved" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        [HttpGet, Authorize]
        public ActionResult getCompany()
        {
            return Json(rs.getCompany(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getPlants(string cmp)
        {
            return Json(rs.getPlants(cmp), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getMaster()
        {
            return Json(rs.getMaster(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getChilds(string Id)
        {
            var data = rs.getDateChild(Id);
            var data1 = rs.getShiftChild(Id);
            return Json(new { Dates = data, Shifts = data1 }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult saveMasters(Dictionary<string, object> Master, List<Dictionary<string, object>> Effective)
        {
            try
            {
                string id = rs.saveMasters(Master, Effective);
                return Json(new { Error = false, Data = Master, ids = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        //[HttpPost]
        //public ActionResult deleteMaster(string id)
        //{
        //    string jj = rs.deleteMaster(id);
        //    if (jj == "Success")
        //    {
        //        return Json(new { Error = false, Data = id, Message = AplosMessage.Updated });
        //    }
        //    else
        //    {
        //        return Json(new { Error = true, Data = id, Message = jj });
        //    }
        //}

        [HttpPost]
        public JsonResult saveShifts(List<Dictionary<string, object>> Shifts , string HeaderId)
        {
            try
            {
                rs.saveShifts(Shifts , HeaderId);
                return Json(new { Error = false, Data = Shifts, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        //The Roster Process Checking Part

        [HttpGet, Authorize]
        public ActionResult run()
        {
            rs.run("202017");
            return Json(new { Date = "Hellow" }, JsonRequestBehavior.AllowGet);
        }

        //The Second Page 
        /// 
        //The Getting of Sample Report
        [HttpGet, Authorize]
        public ActionResult getCurrentList(string plantId)
        {
            return Json(rs.getCurrentList(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveFileList(List<Dictionary<string,object>>data , string plantId)
        {
            try
            {
                 rs.SaveFileList(data , plantId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetSampleReport(string plantId,string name,ReportFormat reportFormat )
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");//.Substring(0, DateTime.Now.Date.ToString().Length - 12);
            var reportFileName = "RosterBudgetUpload-"+ name+"-"+date;
            var workbook = GetRosterBudgetWorkSheet(plantId);
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

        private IWorkbook GetRosterBudgetWorkSheet(string plantId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            DataTable data = rs.getRosterBudgetFile(plantId);

            sheet.Name = "RosterBudgetUpload";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "RosterId", 12, ExcelHAlign.HAlignLeft);
            int ColRosterId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "BudgetId", 8, ExcelHAlign.HAlignLeft);
            int ColBudgetId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "BudgetCode", 8, ExcelHAlign.HAlignLeft);
            int ColBudgetCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 8, ExcelHAlign.HAlignLeft);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 8, ExcelHAlign.HAlignLeft);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Position", 8, ExcelHAlign.HAlignLeft);
            int ColPosition = COL;
            COL++;
            endCol = COL;
            #endregion Headers
            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColRosterId].Text = data.Rows[i]["RosterId"].ToString();
                sheet[ROW, ColBudgetId].Text = data.Rows[i]["BudgetId"].ToString();
                sheet[ROW, ColBudgetCode].Text = data.Rows[i]["BudgetCode"].ToString();
                sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColPosition].Text = data.Rows[i]["Position"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }


        [HttpPost, Authorize]
        public ActionResult ImportData(string plantId)
        {
            string path;

            try
            {
                var file = Request.Files["file"];
                //string plantId = Request.Files["plantId"].ToString();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = ReadData(path , plantId);

                var json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public List<rosbud> ReadData( string path , string plantId)
        {

            DataSet dsExcel = null;
            try
            {
                List<rosbud> data = new List<rosbud>();
                List<rosbud> ret = new List<rosbud>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<rosbud>();
                List<string> RostersList = rs.getRostersList(plantId);

                if(data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if(data[i].RosterId != null)
                        {
                            if(RostersList.Contains(data[i].RosterId))
                            {
                                ret.Add(data[i]);
                            }
                            else
                            {
                                throw new Exception("The Roster in Budget Id - " +data[i].BudgetCode +" is either not present or doesn't belong to this plant!!");
                            }
                            
                        }
                    }
                }

                return ret;
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

        public class rosbud
        {
            public string BudgetId { get; set; }
            public string RosterId { get; set; }
            public string BudgetCode { get; set; }
            
        }

        #region WeeklyStatus

        string TableName = "[HKP].WeeklyStatus";
        [Authorize]
        public ActionResult WeeklyStatus()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetWeeklyStatusCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetWeeklyStatusList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWeeklyStatusAutoSequence()
        {
            return Json(GetWeeklyStatusSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateWeeklyStatus(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("UserName already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where   Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Code already exists!!!");



                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

                    data["Id"] = "LC" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Error = false, Sequence = GetWeeklyStatusSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeleteWeeklyStatus(string id)
        {
            string sql = @"select * from [HKP].[LCOpenChargesTypeGL] where LCOpenChargesTypeId = '" + id + "'";
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetWeeklyStatusSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }


        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }




            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        private double GetWeeklyStatusSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

       

        #endregion
    }
}