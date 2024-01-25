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
    public class CutPlanEditController : BaseController
    {
        #region Constructor
       
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;
        public CutPlanEditController(ISqlRepository R)
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
        public ActionResult GetMasterPlanList()
        {
            return Json(_productionSummaryData.GetMasterPlanList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetColorLists(string MasterPlanId)
        {
            return Json(_productionSummaryData.GetColorLists(MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanSummary(string MasterPlanId,string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanSummary(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAllotedHeaderCountList(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetAllotedHeaderCountList(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR1List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR1List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR2List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR2List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR3List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR3List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR4List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR4List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateCutPlanEditData(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditData(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditData(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from [MST].[AllotedHeader] where Usn n   nm nm merName='" + data["UserName"] + "'", out DataSet dsCutPlanEditUserNameValidation, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AllotedHeader", out _Id);
                        data["Id"] = _Id;
                        data["MasterPlanId"] = MasterPlanId;
                        AddNewRow(dsMaster.Tables[0], data);
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
