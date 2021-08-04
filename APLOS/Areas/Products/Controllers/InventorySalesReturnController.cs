using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Service.Invoices;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data;
using System.Linq;
using Library.Data.Sql;
using Library.Accounting.Accounts;
using Library.Core;
using System;
using System.Data;
using Library.Security.Core;

namespace Aplos.Areas.Products.Controllers
{
    public class InventorySalesReturnController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly AccountsSalesService _accountsSalesService;

        public InventorySalesReturnController(
             ISqlRepository sqlRepository
            , AccountsSalesService accountsSalesService
            )
        {
            _sqlRepository = sqlRepository;
            _accountsSalesService = accountsSalesService;
        }

        

        #region Inventory Sales Posting
        
        public ActionResult Aplos()
        {
            return View();
        }

      
        [Authorize, HttpGet]
        public JsonResult GetInventorySaleDetailGLList(string inventorySalesId, string customerId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventorySalesService.GetInventorySaleDetailGLListData(identity.CompanyId, identity.PlantId, inventorySalesId, customerId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(Dictionary<string, object> entity, List<Dictionary<string, object>> attributes)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (entity != null)
                {

                    DataRow dr;

                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductLibrary WHERE Id='" + entity["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from dbo.ProductLibrary where Code='" + entity["Code"] + "' AND  Id<>'" + entity["Id"] + "'", out DataSet dsCodeMaster, false, "1");
                    if (dsCodeMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Code already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from dbo.ProductLibrary where UserName='" + entity["UserName"] + "' AND  Id<>'" + entity["Id"] + "'", out DataSet dsUserMaster, false, "1");
                    if (dsUserMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same User Name already exists!!!");


                    string _Id = "";
                    string _DId = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductLibrary", out _Id);

                        entity["CompanyGroupId"] = identity.CompanyGroupId;

                        entity["AddedBy"] = identity.Name;
                        entity["AddedDate"] = System.DateTime.Now.ToString();
                        entity["AddedFromIP"] = identity.IPAddress;

                        entity["Id"] = "PL" + _Id;
                        _Id = entity["Id"].ToString();
                        AddNewRow(dsMaster.Tables[0], entity);
                    }
                    else
                    {
                        _Id = entity["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], entity);
                    }

                    #endregion data update

                    #region Child 

                    DataSet dsChild;


                    con.OpenDataSetThroughAdapter("select * from  where  ProductLibraryId='" + _Id + "'", out dsChild, false, "1");
                    #region data update


                    if (attributes != null)
                    {
                        foreach (var item in attributes)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("", out _DId);

                            DataView dv = new DataView(dsChild.Tables[0]);
                            dv.RowFilter = "Id='" + item["Id"] + "'";

                            if (dv.Count == 0)
                            {
                                item["Id"] = _DId;
                                item["ProductLibraryId"] = _Id;
                                AddNewRow(dsChild.Tables[0], item);
                            }
                            else
                            {
                                DataRow drmo = dv[0].Row;
                                EditRow(drmo, item);

                            }
                        }
                    }
                    #endregion

                    #endregion


                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsChild);



                }
                return Json(new { Error = false, Data = entity, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

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

        #endregion
    }
}