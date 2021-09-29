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
        public JsonResult GetGRNDetailListForPostInvoice(string inventoryReceiveId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetGRNDetailListForPostInvoice(inventoryReceiveId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> master,List<Dictionary<string, object>> dataList)
        {
            try
            {
                SaveData(master, dataList);
                return Json(new { Error = false, Data = master, Message = AplosMessage.Updated });

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
                strCSQL = "DELETE FROM [dbo].[JWReceiveBillingDetail] WHERE JWReceiveBillingId='" + Id + "'";
                strSQL = "DELETE FROM [dbo].[JWReceiveBilling] WHERE Id = '" + Id + "'";
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
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"select top 100 * from (SELECT PGI.*,P.UserName PartyName,C.Code Currency FROM [dbo].[PostGRNInvoice] PGI
                            LEFT JOIN HKP.Party P ON P.Id=PGI.PartyId
                            LEFT JOIN SCS.Currency C ON C.Id=PGI.CurrencyId) AS TEMP WHERE " + strkey + "";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}