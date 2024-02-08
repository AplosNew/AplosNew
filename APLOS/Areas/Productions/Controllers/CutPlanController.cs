#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Crosscutting.Security;
using System.Data;
using Library.Security.Core;
using System.Threading;
using Library.MaterialManagement.Material;
using System.Web;
using Newtonsoft.Json;
using Library.Service.Helpers;
using System.IO;
using Library.Core;
using Library.Service.OrderManagements;
using System.Linq;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class CutPlanController : BaseController
    {
        #region Constructor
       
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;
        public CutPlanController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region Operation

        [Authorize, HttpGet]
        public ActionResult GetMPDProcessList()
        {
            return Json(_productionSummaryData.GetMPDProcessList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetUserNameList(string ProcessId)
        {
            return Json(_productionSummaryData.GetUserNameList(ProcessId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetSKU1ColorLists(string MasterPlanId)
        {
            return Json(_productionSummaryData.GetSKU1ColorLists(MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetPackingTypeLists()
        {
            return Json(_productionSummaryData.GetPackingTypeLists(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMasterPlanListForCutPlan(string ProcessId)
        {
            return Json(_productionSummaryData.GetMasterPlanListForCutPlan(ProcessId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanList(string ProcessId, string MasterPlanId)
        {
            return Json(_productionSummaryData.GetCutPlanList(ProcessId, MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMPDLineItemList(string MasterPlanId)
        {
            return Json(_productionSummaryData.GetMPDLineItemList(MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMPDSKU1List(string MasterPlanId)
        {
            return Json(_productionSummaryData.GetMPDSKU1List(MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMPDSKU2List(string MasterPlanId)
        {
            return Json(_productionSummaryData.GetMPDSKU2List(MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanQtyList(string MasterPlanId, bool LineItem, bool SKU1, bool SKU2, string MinQty, string SKU1ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanQtyList(MasterPlanId, LineItem, SKU1, SKU2, MinQty, SKU1ColorId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateCutPlanData(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanData(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanData(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from [MST].[AllotedHeader] where Usn n   nm nm merName='" + data["UserName"] + "'", out DataSet dsCutPlanUserNameValidation, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    if(data["MarkerGSM"] == null)
                    {
                        throw new Exception("GSM is missing..");
                    }
                    if (data["MarkerWidth"] == null)
                    {
                        throw new Exception("MarkerWidth is missing..");
                    }
                    if (data["MarkerLength"] == null)
                    {
                        throw new Exception("MarkerLength is missing..");
                    }
                    if (data["NoOfPly"] == null)
                    {
                        throw new Exception("NoOfPly is missing..");
                    }
                    else
                    { 
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AllotedHeader", out _Id);
                        data["Id"] = _Id;
                        data["MasterPlanId"] = MasterPlanId;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

               

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedChild] WHERE AllotedHeaderId ='" + masterId + "'", out dsDetail, false, "1");
                con.OpenDataSetThroughAdapter("SELECT COUNT(Id)Id FROM [MST].[AllotedChild] WHERE AllotedHeaderId ='" + masterId + "'", out dsId, false, "1");

                int count = Convert.ToInt32(dsId.Tables[0].Rows[0]["Id"].ToString());


                foreach (var item in DataList)
                {

                    DataView dv = new DataView(dsDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        count++;

                        item["Id"] = masterId + "-" + count;
                        item["AllotedHeaderId"] = masterId;
                        item["AllotedQty"] = item["CurrentAllotedQty"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("Update MST.MasterPlanChild set AllotedQty = AllotedQty + " + item["CurrentAllotedQty"]  +" where Id='" +item["MasterPlanChildId"] + @"'");
                    conC.CommitTransaction();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);

            }
            catch (Exception ex)
            {
                throw (ex);
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
