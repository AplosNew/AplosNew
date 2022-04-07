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

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductiveAllowanceRateSetupController : BaseController
    {
        ProductiveAllowanceRateSetupService pa = new ProductiveAllowanceRateSetupService();
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
        public ActionResult saveHeaderPa(Dictionary<string, object> headerData, List<string> process, List<string> entity)
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
        public ActionResult saveChildPa(List<Dictionary<string, object>> childData, string headerId)
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
        public ActionResult saveHeaderRs(Dictionary<string, object> headerData, List<string> process, List<string> entity)
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
        public ActionResult saveChildRs(List<Dictionary<string, object>> childData, string headerId)
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
        [HttpPost, Authorize]
       /* public ActionResult ImportData(string plantId)
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
        }*/
        /*
        public List<> ReadData(string path, string plantId)
        {

            DataSet dsExcel = null;
            try
            {
                List<> data = new List<>();
                List<> ret = new List<>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<rosbud>();
                List<string> RostersList = rs.getRostersList(plantId);

                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i].RosterId != null)
                        {
                            if (RostersList.Contains(data[i].RosterId))
                            {
                                ret.Add(data[i]);
                            }
                            else
                            {
                                throw new Exception("The Roster in Budget Id - " + data[i].BudgetCode + " is either not present or doesn't belong to this plant!!");
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
        */
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
        #endregion BUDGET APPLICABLE

    }
}