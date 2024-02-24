using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.ChartOfAccounts;
using Library.Model.Enums;
using Library.Security.Core;
using Library.Service.ChartOfAccounts;
using Library.Service.Helpers;
using Library.ViewModel.Accounts;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class BalanceSheetSchedulingController : BaseController
    {
        string TableName = "dbo.BalanceSheetScheduling";
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public BalanceSheetSchedulingController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion


        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,OptionNo AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from " + TableName + " where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM " + TableName + "  order by Id desc";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetBalanceSheetSchedulingList(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM dbo.BalanceSheetScheduling Where Id<>'"+id+"' order by Id desc";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where OptionNo='" + data["OptionNo"].ToString() + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Option No already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
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

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Id),0) AS Id FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Id"].ToString()) + 1;
            return 1;
        }

        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            IWorkbook workbook = accountsInventoryPayableReportService.GetSampleFileBalanceSheetScheduling(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "BalanceSheetScheduling Data upload Sample File";

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
        [HttpPost, Authorize]
        public JsonResult ImportData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<BalanceSheetSchedulingUploadedDataViewModel> data = new List<BalanceSheetSchedulingUploadedDataViewModel>();

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
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
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
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
                    try
                    {
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                BalanceSheetSchedulingUploadedDataViewModel vm = new BalanceSheetSchedulingUploadedDataViewModel();

                                vm.BudgetMasterActivityId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.Level1 = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.Level2 = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                vm.Level3 = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                vm.Level4 = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                vm.GLGeneralInfoCode = dsExcel.Tables[0].Rows[i][5].ToString().Trim();
                                vm.GLName = dsExcel.Tables[0].Rows[i][6].ToString().Trim();
                                vm.BudgetGroup = dsExcel.Tables[0].Rows[i][7].ToString().Trim();
                                vm.BudgetCategory = dsExcel.Tables[0].Rows[i][8].ToString().Trim();
                                vm.BudgetSubCategory = dsExcel.Tables[0].Rows[i][9].ToString().Trim();
                                vm.Budget = dsExcel.Tables[0].Rows[i][10].ToString().Trim();
                                vm.RefNo = dsExcel.Tables[0].Rows[i][11].ToString().Trim();
                                vm.Activity = dsExcel.Tables[0].Rows[i][12].ToString().Trim();
                                vm.Register = dsExcel.Tables[0].Rows[i][13].ToString().Trim();
                                vm.BalanceSheetSchedulingId = dsExcel.Tables[0].Rows[i][14].ToString().Trim();
                                vm.TaxApplicable = dsExcel.Tables[0].Rows[i][15].ToString().Trim();
                                vm.TaxType = dsExcel.Tables[0].Rows[i][16].ToString().Trim();
                                vm.UserCategory = dsExcel.Tables[0].Rows[i][17].ToString().Trim();
                                vm.UserSubCategory = dsExcel.Tables[0].Rows[i][18].ToString().Trim();
                                vm.UserItem = dsExcel.Tables[0].Rows[i][19].ToString().Trim();
                                vm.UserReport = dsExcel.Tables[0].Rows[i][20].ToString().Trim();
                                vm.IsAllowed = dsExcel.Tables[0].Rows[i][21].ToString().Trim();
                                vm.AllowedDays = Convert.ToInt32(dsExcel.Tables[0].Rows[i][22]);
                                vm.MonthDay = Convert.ToInt32(dsExcel.Tables[0].Rows[i][23]);
                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
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
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveBalanceSheetSchedulingUploadedData(IEnumerable<BalanceSheetSchedulingUploadedData> balanceSheetSchedulingUploadedDataList)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            accountsCommonService.SaveBalanceSheetSchedulingUploadedData(balanceSheetSchedulingUploadedDataList);

            return Json(new { Message = AplosMessage.Updated });
        }
    }
}