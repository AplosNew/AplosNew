using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.OrderManagement.Production;

namespace Aplos.Areas.QMS.Controllers
{
    public class LWQRUpdateController : Controller
    {
        #region Constructor


        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;

        public LWQRUpdateController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations



        [Authorize, HttpGet]
        public ActionResult GetUpdateCustomerList()
        {
            return Json(_productionSummaryData.GetUpdateCustomerList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetUpdateInvoiceList(string PartyId)
        {
            return Json(_productionSummaryData.GetUpdateInvoiceList(PartyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetUpdatePOList(string InvoiceId)
        {
            return Json(_productionSummaryData.GetUpdateInvoicePOList(InvoiceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetUpdateLotNumberLists(string POId)
        {
            return Json(_productionSummaryData.GetUpdateLotNumberLists(POId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadLWQRUpdate(string POId, string LotNumber, string CustomerId, string InvoiceId)
        {
            return Json(_productionSummaryData.LoadLWQRUpdate(POId, LotNumber, CustomerId, InvoiceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetByWhomList()
        {
            return Json(_productionSummaryData.GetByWhomList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateCQRData(Dictionary<string, object> data, List<Dictionary<string, object>> DataList)
        {
            SaveCQRData(data, DataList, out string masterId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }


        public void SaveCQRData(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[CustomerQualityReportHeader] WHERE UserName='" + data["UserName"] + "'", out DataSet dsCustomerQualityReportHeaderUserNameValidation, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[CustomerQualityReportHeader] WHERE ProductionOrderId='" + data["ProductionOrderId"] + "' and LotNo='" + data["LotNo"] + "'", out DataSet dsCustomerQualityReportHeaderPOLotValidation, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[CustomerQualityReportHeader] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                // conC.BeginTransaction();
                //// conC.executeQuery("Update [TRN].[CustomerQualityReportDetails] set FinalReport=0 where CQRHeaderId = '" + data["Id"] + "'");
                // conC.CommitTransaction();

                string _Id = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    if (dsCustomerQualityReportHeaderPOLotValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("PO and Lot Already Exist.");
                    }
                    else if (dsCustomerQualityReportHeaderUserNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "CustomerQualityReportHeader", out _Id);
                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[CustomerQualityReportDetails] WHERE CQRHeaderId ='" + masterId + "'", out dsDetail, false, "1");
                con.OpenDataSetThroughAdapter("SELECT COUNT(Id)Id FROM [TRN].[CustomerQualityReportDetails] WHERE CQRHeaderId ='" + masterId + "'", out dsId, false, "1");

                for (int i = 0; i < dsDetail.Tables[0].Rows.Count; i++)
                {
                    if (Convert.ToBoolean(dsDetail.Tables[0].Rows[i]["FinalReport"])==false)
                    {
                        dsDetail.Tables[0].Rows[i].Delete();
                    }
                }

                int count = Convert.ToInt32(dsId.Tables[0].Rows[0]["Id"].ToString());


                foreach (var item in DataList)
                {

                    DataView dv = new DataView(dsDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        count++;

                        item["Id"] = masterId + "-" + count;
                        item["CQRHeaderId"] = masterId;
                        AddNewRow(dsDetail.Tables[0], item);
                    }

                    else
                    {
                        DataRow drmo = dv[0].Row;
                        if (drmo["Id"].ToString() != null && Convert.ToBoolean(drmo["FinalReport"].ToString()) == false)
                        {
                            drmo.Delete();
                        }
                        else
                        {
                            EditRow(drmo, item);
                        }
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public ActionResult UpdateParameterData(string ParameterChildId, string SpecialRemarks)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("Update [TRN].[CustomerQualityReportDetails] set SpecialRemarks='" + SpecialRemarks + "' where Id='" + ParameterChildId + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetCustomerQualityLotWiseUpdateJobCardReport(string CustomerId, string InvoiceId, string ProductionOrderId, string LotNumber)
        {
            try
            {
                NewJobCardReportService app = new NewJobCardReportService();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = app.GetCustomerQualityLotWiseUpdateJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, CustomerId, InvoiceId, ProductionOrderId, LotNumber);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Quality Test Report";
                return RenderReportAsExcel(workbook, reportFileName);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        private ActionResult RenderReportAsExcel(IWorkbook workbook, string fileName)
        {
            workbook.SaveAs(fileName + ".xls", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
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
        #endregion -- Operations
    }
}