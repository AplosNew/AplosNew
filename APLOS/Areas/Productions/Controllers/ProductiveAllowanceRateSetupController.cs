using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.HumanResource;
using Aplos.Controllers;
using Aplos.Properties;
using Library.HumanResource.NewAttendanceProcess;
using Library.OrderManagement.Production;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using System.Data;
using Library.Data;
using Library.Crosscutting.Security;
using System.Threading;

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductiveAllowanceRateSetupController : BaseController
    {
        ProductiveAllowanceRateSetupService pa = new ProductiveAllowanceRateSetupService();
        EmployeeOperationBudget eob = new EmployeeOperationBudget();
        SpecialOperationService so = new SpecialOperationService();
        EmployeeTimeOutApplicableService et = new EmployeeTimeOutApplicableService();
        public ProductiveAllowanceRateSetupController()
        { }

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        #region ProductiveAllowance

        #region GetOperations

        [Authorize, HttpPost]
        public ActionResult getProcess()
        {
            return Json(pa.getProcess(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            return Json(pa.getEntity(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getMasterData()
        {
            return Json(pa.getMasterData(), JsonRequestBehavior.AllowGet);
        }

        // Get All Rate Set up Data start
        [HttpPost, Authorize]
        public ActionResult getRsMasterData()
        {
            return Json(pa.getRsMasterData(), JsonRequestBehavior.AllowGet);
        }
        // Get All Rate Set up Data end

        [HttpPost, Authorize]
        public ActionResult getPaChildList(string Id)
        {
            return Json(pa.getPaChildList(Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getRsChildList(string Id)
        {
            return Json(pa.getRsChildList(Id), JsonRequestBehavior.AllowGet);
        }

        #endregion GetOperations


        #region Savings
        [HttpPost]
        public ActionResult saveHeaderPa(Dictionary<string, string> headerData, List<string> process, List<string> entity)
        {
            try
            {
                return Json(new { Error = "No", Data = pa.saveHeaderPa(headerData, process, entity), Msg = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Yes", Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult saveChildPa(List<Dictionary<string, string>> childData, string headerId)
        {
            try
            {
                return Json(new { Error = "No", Data = pa.saveChildPa(childData, headerId), Msg = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Savings

        #endregion ProductiveAllowance

        #region RateSetup

        #region Savings
        [HttpPost]
        public ActionResult saveHeaderRs(Dictionary<string, string> headerData, List<string> process, List<string> entity)
        {
            try
            {
                return Json(new { Error = "No", Data = pa.saveHeaderRs(headerData, process, entity), Msg = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Yes", Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // --------------------------------------Save Child RS
        [HttpPost]
        public ActionResult saveChildRs(List<Dictionary<string, string>> childData, string headerId)
        {
            try
            {
                return Json(new { Error = "No", Data = pa.saveChildRs(childData, headerId), Msg = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Savings

        #endregion

        #region BUDGET APPLICABLE

        #region GetOperations
        [HttpGet, Authorize]
        public ActionResult getPlants(string cmp)
        {
            return Json(eob.getPlants(cmp), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getCompany()
        {
            return Json(eob.getCompany(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getCurrentList(string plantId)
        {
            return Json(eob.getCurrentList(plantId), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region SampleDownload
        [HttpGet, Authorize]
        public ActionResult GetSampleReport(string plantId, string name, ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");//.Substring(0, DateTime.Now.Date.ToString().Length - 12);
            var reportFileName = "BudgetUpload-" + name + "-" + date;
            var workbook = GetBudgetWorkSheet(plantId);
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

        private IWorkbook GetBudgetWorkSheet(string plantId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            //DataTable data = eob.getEmployeeOperationBudgetFile(plantId);

            sheet.Name = "BudgetUpload";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers


            report.SetHeaderText(ref sheet, ROW, COL, "BudgetCode", 8, ExcelHAlign.HAlignLeft);
            int ColBudgetCode = COL;
            COL++;

            endCol = COL;
            #endregion Headers
            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            //for (int i = 0; i < data.Rows.Count; i++)
            //{
            //    sheet[ROW, ColBudgetCode].Text = data.Rows[i]["BudgetCode"].ToString();
            //    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            //    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

            //    ROW++;

            //}
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion

        [HttpPost]
        public ActionResult SaveFileList(List<Dictionary<string, string>> data, string plantId)
        {
            try
            {
                eob.SaveFileList(data, plantId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
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
                var data = ReadData(path, plantId);

                var json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public List<Dictionary<string, string>> ReadData(string path, string plantId)
        {

            DataSet dsExcel = null;
            try
            {
                List<eobud> data = new List<eobud>();
                List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
                ReadFile(path, out dsExcel);
                DataTable dtId = eob.getEmployeeOperationBudgetFile(plantId);
                data = dsExcel.Tables[0].ToList<eobud>();


                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        dtId.DefaultView.RowFilter = @"BudgetCode='" + data[i].BudgetCode + "'";
                        if (dtId.DefaultView.Count > 0)
                        {
                            Dictionary<string, string> jj = new Dictionary<string, string>();
                            jj.Add("BudgetId", dtId.DefaultView[0]["BudgetId"].ToString());
                            jj.Add("BudgetCode", dtId.DefaultView[0]["BudgetCode"].ToString());
                            ret.Add(jj);
                        }
                        //else
                        //{
                        //    throw new Exception("The BudgetCode at Line no - " + i + 1 + " doesn't exist!!");
                        //}

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



        public class eobud
        {
            public string BudgetCode { get; set; }

        }
        #endregion BUDGET APPLICABLE

        #region Special Operations

        #region GetOperations
        [HttpGet, Authorize]
        public ActionResult getEntitySP()
        {
            return Json(so.getEntitySP(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getProcessSP(string EntityId)
        {
            return Json(so.getProcessSP(EntityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getSpOpMaster()
        {
            return Json(so.getSpOpMaster(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getSpOpDates(string HeaderId)
        {
            return Json(so.getSpOpDates(HeaderId), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Save Operation
        [HttpPost]
        public ActionResult saveOperations(Dictionary<string, string> data, List<string> dates)
        {
            try
            {
                return Json(new { Error = "No", Data = so.saveOperations(data , dates), Msg = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #endregion

        #region EmployeeTimeOutApplicable

        #region GetOperations

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = et.Get(Id);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList()
        {
            return Json(et.GetList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCompanys()
        {
            return Json(et.getCompany(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPlant(string cmp)
        {
            return Json(et.getPlants(cmp), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEntity(string plant)
        {
            return Json(et.getEntity(plant), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProcess(string entity)
        {
            return Json(et.getProcesses(entity), JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Saving

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                string ret = et.Create(data);
                if (ret == "Success")
                {
                    return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
                }
                else
                {
                    return Json(new { Error = true, Message = ret });
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {

                string ret = et.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }



        #endregion

        #endregion

        #region --Order Size Allowance
        [HttpPost, Authorize]
        public ActionResult getOrderSizeAllowanceData()
        {
            return Json(pa.getOrderSizeAllowanceData(), JsonRequestBehavior.AllowGet);
        }
        #region Savings
        [HttpPost]
        public ActionResult saveOrderSizeAllowance(Dictionary<string, string> data)
        {
            try
            {
                return Json(new { Error = "No", Data = pa.saveOrderSizeAlw(data), Msg = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Yes", Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion Savings

        #endregion

    }
}