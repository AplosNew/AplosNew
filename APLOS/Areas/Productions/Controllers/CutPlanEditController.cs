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
        public ActionResult GetTotalStatusList(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetTotalStatusList(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
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
        public JsonResult CreateCutPlanEditR1Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR1Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR1Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R1Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R1Id"];
                    data["UserName"] = data["UserNameR1"];
                    data["MarkerId"] = data["MarkerIdR1"];
                    data["PackingTypeId"] = data["PackingTypeIdR1"];
                    data["NoOfPly"] = data["NoOfPlyR1"];
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
                        item["AllotedQty"] = item["AllotedQtyR1"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR1"];
                        item["Ratio"] = item["Ratio1"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("Update MST.MasterPlanChild set AllotedQty = 0 + " + item["AllotedQtyR1"]  +" where Id = '" +item["MasterPlanChildId"] + @"'");
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

        [HttpPost]
        public JsonResult CreateCutPlanEditR2Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR2Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR2Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R2Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R2Id"];
                    data["UserName"] = data["UserNameR2"];
                    data["MarkerId"] = data["MarkerIdR2"];
                    data["PackingTypeId"] = data["PackingTypeIdR2"];
                    data["NoOfPly"] = data["NoOfPlyR2"];
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
                        item["AllotedQty"] = item["AllotedQtyR2"];
                        item["Ratio"] = item["Ratio2"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR2"];
                        item["Ratio"] = item["Ratio2"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("Update MST.MasterPlanChild set AllotedQty = AllotedQty + " + item["AllotedQtyR2"] + " where Id='" + item["MasterPlanChildId"] + @"'");
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


        [HttpPost]
        public JsonResult CreateCutPlanEditR3Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR3Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR3Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R3Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R3Id"];
                    data["UserName"] = data["UserNameR3"];
                    data["MarkerId"] = data["MarkerIdR3"];
                    data["PackingTypeId"] = data["PackingTypeIdR3"];
                    data["NoOfPly"] = data["NoOfPlyR3"];
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
                        item["AllotedQty"] = item["AllotedQtyR3"];
                        item["Ratio"] = item["Ratio3"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR3"];
                        item["Ratio"] = item["Ratio3"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("Update MST.MasterPlanChild set AllotedQty = AllotedQty + " + item["AllotedQtyR3"] + " where Id='" + item["MasterPlanChildId"] + @"'");
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

        [HttpPost]
        public JsonResult CreateCutPlanEditR4Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR4Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR4Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R4Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R4Id"];
                    data["UserName"] = data["R4Id"];
                    data["MarkerId"] = data["MarkerIdR4"];
                    data["PackingTypeId"] = data["PackingTypeIdR4"];
                    data["NoOfPly"] = data["NoOfPlyR4"];
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
                        item["AllotedQty"] = item["AllotedQtyR4"];
                        item["Ratio"] = item["Ratio4"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR4"];
                        item["Ratio"] = item["Ratio4"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("Update MST.MasterPlanChild set AllotedQty = AllotedQty + " + item["AllotedQtyR4"] + " where Id='" + item["MasterPlanChildId"] + @"'");
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
