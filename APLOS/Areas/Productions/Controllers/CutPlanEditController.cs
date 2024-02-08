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
        public ActionResult GetCutPlanSummary(string MasterPlanId, string ColorId)
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

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR5List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR5List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR6List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR6List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR7List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR7List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR8List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR8List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR9List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR9List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsR10List(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsR10List(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCutPlanDetailsBalanceList(string MasterPlanId, string ColorId)
        {
            return Json(_productionSummaryData.GetCutPlanDetailsBalanceList(MasterPlanId, ColorId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateCutPlanEditR1Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR1Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            UpdateBalanceData(DataList);
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
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void UpdateBalanceData(List<Dictionary<string, object>> DataList)
        {
            try
            {

                foreach (var item in DataList)
                {
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("Update MST.MasterPlanChild set AllotedQty = (select Sum(AllotedQty) from  [MST].[AllotedChild] where MasterPlanChildId='" + item["MasterPlanChildId"] + "') where Id = '" + item["MasterPlanChildId"] + @"'");
                    conC.CommitTransaction();
                }
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
            UpdateBalanceData(DataList);
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
            UpdateBalanceData(DataList);
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
            UpdateBalanceData(DataList);
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
                    data["UserName"] = data["UserNameR4"];
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
        public JsonResult CreateCutPlanEditR5Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR5Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            UpdateBalanceData(DataList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR5Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R5Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R5Id"];
                    data["UserName"] = data["UserNameR5"];
                    data["MarkerId"] = data["MarkerIdR5"];
                    data["PackingTypeId"] = data["PackingTypeIdR5"];
                    data["NoOfPly"] = data["NoOfPlyR5"];
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
                        item["AllotedQty"] = item["AllotedQtyR5"];
                        item["Ratio"] = item["Ratio5"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR5"];
                        item["Ratio"] = item["Ratio5"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
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
        public JsonResult CreateCutPlanEditR6Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR6Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            UpdateBalanceData(DataList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR6Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R6Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R6Id"];
                    data["UserName"] = data["UserNameR6"];
                    data["MarkerId"] = data["MarkerIdR6"];
                    data["PackingTypeId"] = data["PackingTypeIdR6"];
                    data["NoOfPly"] = data["NoOfPlyR6"];
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
                        item["AllotedQty"] = item["AllotedQtyR6"];
                        item["Ratio"] = item["Ratio6"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR6"];
                        item["Ratio"] = item["Ratio6"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
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
        public JsonResult CreateCutPlanEditR7Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR7Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            UpdateBalanceData(DataList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR7Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R7Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R7Id"];
                    data["UserName"] = data["UserNameR7"];
                    data["MarkerId"] = data["MarkerIdR7"];
                    data["PackingTypeId"] = data["PackingTypeIdR7"];
                    data["NoOfPly"] = data["NoOfPlyR7"];
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
                        item["AllotedQty"] = item["AllotedQtyR7"];
                        item["Ratio"] = item["Ratio7"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR7"];
                        item["Ratio"] = item["Ratio7"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
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
        public JsonResult CreateCutPlanEditR8Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR8Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            UpdateBalanceData(DataList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR8Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R8Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R8Id"];
                    data["UserName"] = data["UserNameR8"];
                    data["MarkerId"] = data["MarkerIdR8"];
                    data["PackingTypeId"] = data["PackingTypeIdR8"];
                    data["NoOfPly"] = data["NoOfPlyR8"];
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
                        item["AllotedQty"] = item["AllotedQtyR8"];
                        item["Ratio"] = item["Ratio8"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR8"];
                        item["Ratio"] = item["Ratio8"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
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
        public JsonResult CreateCutPlanEditR9Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR9Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            UpdateBalanceData(DataList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR9Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R9Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R9Id"];
                    data["UserName"] = data["UserNameR9"];
                    data["MarkerId"] = data["MarkerIdR9"];
                    data["PackingTypeId"] = data["PackingTypeIdR9"];
                    data["NoOfPly"] = data["NoOfPlyR9"];
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
                        item["AllotedQty"] = item["AllotedQtyR9"];
                        item["Ratio"] = item["Ratio9"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR9"];
                        item["Ratio"] = item["Ratio9"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
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
        public JsonResult CreateCutPlanEditR10Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, string MasterPlanId)
        {
            SaveCutPlanEditR10Data(data, DataList, out string masterId, MasterPlanId);
            data["Id"] = masterId;
            UpdateBalanceData(DataList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveCutPlanEditR10Data(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId, string MasterPlanId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[AllotedHeader] WHERE Id='" + data["R10Id"] + "'", out dsMaster, false, "1");

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
                    data["Id"] = data["R10Id"];
                    data["UserName"] = data["UserNameR10"];
                    data["MarkerId"] = data["MarkerIdR10"];
                    data["PackingTypeId"] = data["PackingTypeIdR10"];
                    data["NoOfPly"] = data["NoOfPlyR10"];
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
                        item["AllotedQty"] = item["AllotedQtyR10"];
                        item["Ratio"] = item["Ratio10"];
                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        item["AllotedQty"] = item["AllotedQtyR10"];
                        item["Ratio"] = item["Ratio10"];
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
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
