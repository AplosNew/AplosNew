using Aplos.Controllers;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Mvc;
using Library.Accounting.Accounts;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using Aplos.Properties;
using System.Data;
using Library.Security.Core;
using Library.ViewModel.Vouchers;
using Library.Data;
using System.Linq;
using Library.Model.Enums;

namespace Aplos.Areas.Accounts.Controllers
{
    public class PostInvoiceController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;

        public PostInvoiceController(
             ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }


        public ActionResult Aplos()
        {
            return View();
        }

        

        #region GRN Operation

        [Authorize, HttpGet]
        public JsonResult GetListForInvPayable()
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetGRNListForPostInvoice(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetGRNDetailListForPostInvoice(string inventoryReceiveId, string masterId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetGRNDetailListForPostInvoice(inventoryReceiveId, masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> master,List<Dictionary<string, object>> dataList)
        {
            try
            {
                SaveData(master, dataList);
                return Json(new { Error = false, Data = master, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PostGRNInvoiceDetail", out sID);
            return sID;
        }

        private void SaveData(Dictionary<string, object> master, List<Dictionary<string, object>> dataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            objCon = new ConnectionManager.DAL.ConManager("1");
            DataSet dsMaster, dsDetails;
            try
            {
                string _Id = "";
                string masterId = "";
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.PostGRNInvoice Where Id='" + master["Id"] + "'", out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.PostGRNInvoiceDetail Where PostGRNInvoiceId='" + master["Id"] + "'", out dsDetails, false, "1");

                if (master != null)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PostGRNInvoice", out _Id);

                        master["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], master);
                    }
                    else
                    {
                        _Id = master["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], master);
                    }

                    masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                    if (dataList != null)
                    {
                        foreach (var item in dataList)
                        {
                            DataView dv = new DataView(dsDetails.Tables[0]);
                            dv.RowFilter = "Id='" + item["Id"] + "'";

                            if (dv.Count == 0)
                            {
                                item["Id"] = GetPK();
                                item["PostGRNInvoiceId"] = masterId;
                                AddNewRow(dsDetails.Tables[0], item);
                            }
                            else
                            {
                                DataRow drmo = dv[0].Row;
                                EditRow(drmo, item);
                            }
                        }
                    }

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetails);


            }
            catch (Exception ex)
            {
                throw ex;
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL, strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strCSQL = "DELETE FROM [dbo].[PostGRNInvoiceDetail] WHERE PostGRNInvoiceId='" + Id + "'";
                strSQL = "DELETE FROM [dbo].[PostGRNInvoice] WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetPostInvoiceList(column, value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDetailList(string column, string value)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetPostInvoiceList(column, value), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetPostInvoiceDetailData(string masterId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetPostInvoiceDetailData(masterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSavedGRNListForPostInvoice(string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetSavedGRNListForPostInvoice(identity.PlantId,masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetPostableList(string id)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetPostableList(id), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetPostableJVList(string id,string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetPostableJVList(identity.CompanyId, identity.PlantId, id,partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Postdata(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            AccountsPostInvoiceService accountsPostInvoiceService = new AccountsPostInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {
                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Activity is Not Mapped!");
                }

                if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
                if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.BaseDrAmount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.BaseCrAmount))
                    throw new CustomException("Books Dr Cr Amount not equal");
            }
            else
                throw new CustomException("No Journal");

            return Json(new
            {
                Message = string.Format(AplosMessage.VoucherSave, accountsPostInvoiceService.InsertPostInvoice(voucherVM, voucherDetailVMList))
            });

        }
        
        [HttpGet, Authorize]
        public ActionResult PostInvoiceVoucherReport(ReportFormat reportFormat, string voucherId)
        {
            AccountsPostInvoiceService accountsPostInvoiceService = new AccountsPostInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = accountsPostInvoiceService.GetPostInvoiceVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }
        #endregion

    }
}