#region using
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Data;
using Library.Service.Materials;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Collections.Generic;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.Security.Core;
using Library.MaterialManagement.Inventory;
using Library.Model.Inventory;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Newtonsoft.Json;
using Library.MaterialManagement.Products;
using Library.Model.Products;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System.Collections.Specialized;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using Library.Service.Helpers;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using Aplos.Areas.Commercial.Controllers;
using Library.Service.Systems;
using Library.MaterialManagement.Material;
#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialIssueControlController : BaseController
    {
        #region -- Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IIssueRequestService _issueRequestService;
        private readonly IPKGeneratorService _pkGeneratorService;
        clsMaterial clsM = new clsMaterial();
        public MaterialIssueControlController(ISqlRepository R, IIssueRequestService issueRequestService, IPKGeneratorService pkGeneratorService)
        {
            _sqlRepository = R;
            _issueRequestService = issueRequestService;
            _pkGeneratorService = pkGeneratorService;
        }
        #endregion

        #region Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult Approval()
        {
            return View();
        }
        public ActionResult Issue()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult EntityList()
        {
            return Json(clsM.EntityList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedUnApprovedData()
        {
            try
            {

                return Json(clsM.GetSavedUnApprovedData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetApprovedData(string column, string value)
        {
            try
            {

                return Json(clsM.GetApprovedData(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedSODetailData(string masterId)
        {
            try
            {
                return Json(clsM.GetSavedSODetailData(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetIssueRequestList(string masterId)
        {
            try
            {
                return Json(clsM.GetIssueRequestList(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetIssueRequestBOQMapList(string masterId)
        {
            try
            {
                return Json(clsM.GetIssueRequestBOQMapList(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedDetailDataToApprove(string masterId)
        {
            try
            {
                return Json(clsM.GetSavedDetailDataToApprove(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedDetailData(string masterId)
        {
            try
            {
                return Json(clsM.GetSavedDetailData(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string entityid, string column, string value)
        {

            return Json(clsM.GetList(entityid, column, value), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSOItemList(string entityid, string ProductionOrderId)
        {
            return Json(clsM.GetSOItemList(entityid, ProductionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMOIItemList(string entityid, string ProductionOrderId)
        {
            return Json(clsM.GetMOIItemList(entityid, ProductionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(Dictionary<string, object> model, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList, List<Dictionary<string, object>> IssueRequestList, List<Dictionary<string, object>> BOQMapList)
        {
            try
            {
                UpdateData(model, soList, dataList, IssueRequestList, BOQMapList);
                return Json(new { Data = model, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        private void UpdateData(Dictionary<string, object> data, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList, List<Dictionary<string, object>> IssueRequestList, List<Dictionary<string, object>> BOQMapList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild, dsSOChild, dsIssueRequest, dsIssueRequestBOQMap, dsIssue;
            string _Id = string.Empty;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM TRN.IssueRequestMaster Where Id='" + data["IssueId"] + "' AND CheckedByStatus='Checked'", out dsIssue, false, "1");
                if (dsIssue.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Checked issue slip could not update.");
                }

                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region MaterialIssueControlSODetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlSODetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsSOChild, false, "1");
                int socount = 0;
                if (soList != null)
                {
                    foreach (var item in soList)
                    {
                        socount++;
                        DataView dv = new DataView(dsSOChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }

                    }
                }

                #endregion

                #region MaterialIssueControlDetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlDetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsChild, false, "1");

                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }

                    }
                }

                #endregion

                #region IssueRequest 
                objCon.OpenDataSetThroughAdapter("select * from TRN.IssueRequest where MaterialIssueControlDetailId IN(select Id from MaterialIssueControlDetail Where MaterialIssueControlMasterId IN(select Id from MaterialIssueControlMaster Where Id IN('" + _Id + "')))", out dsIssueRequest, false, "1");

                if (IssueRequestList != null)
                {
                    foreach (var item in IssueRequestList)
                    {
                        socount++;
                        DataView dv = new DataView(dsIssueRequest.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion

                #region IssueRequestBOQMap 
                objCon.OpenDataSetThroughAdapter("select * from TRN.IssueRequestBOQMap Where IssueRequestDetailId IN(select Id from TRN.IssueRequest where MaterialIssueControlDetailId IN(select Id from MaterialIssueControlDetail Where MaterialIssueControlMasterId IN(select Id from MaterialIssueControlMaster Where Id IN('" + _Id + "'))))", out dsIssueRequestBOQMap, false, "1");

                if (BOQMapList != null)
                {
                    foreach (var item in BOQMapList)
                    {
                        socount++;
                        DataView dv = new DataView(dsIssueRequestBOQMap.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsSOChild, dsChild, dsIssueRequest, dsIssueRequestBOQMap);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild, dsSOChild, dsIdChild;
            string _Id = string.Empty;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaterialIssueControlMaster", out _Id);

                    data["Id"] = "M" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region MaterialIssueControlSODetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlSODetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsSOChild, false, "1");
                int socount = 0;
                if (soList != null)
                {
                    foreach (var item in soList)
                    {
                        socount++;
                        DataView dv = new DataView(dsSOChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + socount;
                            item["MaterialIssueControlMasterId"] = _Id;
                            item["SOQty"] = item["PlannedQty"];

                            AddNewRow(dsSOChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                #region MaterialIssueControlDetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlDetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsChild, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT Count(Id)Idc FROM dbo.MaterialIssueControlDetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsIdChild, false, "1");
                int ccount = Convert.ToInt32(dsIdChild.Tables[0].Rows[0]["Idc"].ToString());
                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            ccount++;

                            string id = _pkGeneratorService.MakePK(_Id, ccount, 2);
                            item["Id"] = id;
                            item["MaterialIssueControlMasterId"] = _Id;
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
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsSOChild, dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult CreateApprove(Dictionary<string, object> model, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList)
        {
            try
            {
                SaveApproveData(model, soList, dataList);
                return Json(new { Data = model, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        private void SaveApproveData(Dictionary<string, object> data, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild, dsSOChild;
            string _Id = string.Empty;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaterialIssueControlMaster", out _Id);

                    data["Id"] = "M" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["IsApproved"] = true;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region MaterialIssueControlSODetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlSODetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsSOChild, false, "1");
                int socount = 0;
                if (soList != null)
                {
                    foreach (var item in soList)
                    {
                        socount++;
                        DataView dv = new DataView(dsSOChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + socount;
                            item["MaterialIssueControlMasterId"] = _Id;
                            item["SOQty"] = item["PlannedQty"];

                            AddNewRow(dsSOChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                #region MaterialIssueControlDetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlDetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsChild, false, "1");
                int ccount = 0;
                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        ccount++;
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + ccount;
                            item["MaterialIssueControlMasterId"] = _Id;
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
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsSOChild, dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult CreateIssue(Dictionary<string, object> model, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList, List<IssueRequestViewModel> dataLists)
        {
            try
            {
                IssueRequestMaster inventoryIssue = new IssueRequestMaster();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
                inventoryIssue.CompanyId = identity.CompanyId;
                inventoryIssue.PlantId = identity.PlantId;
                inventoryIssue.CheckedBy = model["CheckedBy"].ToString();
                //if (string.IsNullOrEmpty(model["IssueId"].ToString()))
                //{
                //    inventoryIssue.Id = model["IssueId"].ToString(); 
                //}
                inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
                inventoryIssue.CompanyId = identity.CompanyId;
                inventoryIssue.PlantId = identity.PlantId;
                inventoryIssue.Orderspecific = "No";
                inventoryIssue.IssueSlipType = "InventorySlip";
                inventoryIssue.CheckedByStatus = "ForChecked";
                inventoryIssue.Preparedby = model["ByWhomId"].ToString();
                inventoryIssue.ProductionOrderId = model["POId"].ToString();

                SaveData(model, soList, dataList);
                List<IssueRequestViewModel> entityDetailVM = dataLists;
                List<IssueRequestViewModel> entityGroupDataVM = dataLists;

                foreach (var item in entityGroupDataVM)
                {
                    foreach (var ditem in dataList)
                    {
                        if (item.SrNo == Convert.ToInt32(ditem["SrNo"]))
                        {
                            item.MaterialIssueControlDetailId = ditem["Id"].ToString();
                        }
                    }
                }

                List<IssueRequestViewModel> SOListSelectedNewDetailVM = null;
                List<IssueRequestViewModel> MaterialColorListNewDetailVM = null;

                _issueRequestService.InsertOrUpdateGraphIssueSlipCreate(inventoryIssue, entityDetailVM, entityGroupDataVM, inventoryIssue.IssueSlipType, null, null, SOListSelectedNewDetailVM, MaterialColorListNewDetailVM, null, null);


                return Json(new { Data = model, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }



        private void SaveIssueData(Dictionary<string, object> data, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild, dsSOChild;
            string _Id = string.Empty;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaterialIssueControlMaster", out _Id);

                    data["Id"] = "M" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["IsApproved"] = true;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region MaterialIssueControlSODetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlSODetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsSOChild, false, "1");
                int socount = 0;
                if (soList != null)
                {
                    foreach (var item in soList)
                    {
                        socount++;
                        DataView dv = new DataView(dsSOChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + socount;
                            item["MaterialIssueControlMasterId"] = _Id;
                            item["SOQty"] = item["PlannedQty"];

                            AddNewRow(dsSOChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                #region MaterialIssueControlDetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlDetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsChild, false, "1");
                int ccount = 0;
                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        ccount++;
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + ccount;
                            item["MaterialIssueControlMasterId"] = _Id;
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
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

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

        [HttpPost]
        public JsonResult Delete(string id, string issueId)
        {
            DeleteData(id, issueId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id, string issueId)
        {
            DataSet dsIssue=null;
            string strSQL, strBSQL, strIRSQL, strMDSQL, strSOSQL, strISMSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                ConnectionManager.DAL.ConManager Con = new ConnectionManager.DAL.ConManager("1");
                Con.OpenDataSetThroughAdapter("SELECT * FROM TRN.IssueRequestMaster Where Id='" + issueId + "' AND CheckedByStatus='Checked'", out dsIssue, false, "1");

                if (dsIssue.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Checked issue slip could not Delete.");
                }

                strBSQL = @"delete from TRN.IssueRequestBOQMap Where IssueRequestDetailId IN(select Id from TRN.IssueRequest where MaterialIssueControlDetailId IN(select Id from MaterialIssueControlDetail Where MaterialIssueControlMasterId IN(select Id from MaterialIssueControlMaster Where Id ='" + Id + "')))";
                strIRSQL = @"delete from TRN.IssueRequest where MaterialIssueControlDetailId IN(select Id from MaterialIssueControlDetail Where MaterialIssueControlMasterId IN(select Id from MaterialIssueControlMaster Where Id ='" + Id + "'))";
                strMDSQL = @"delete from MaterialIssueControlDetail Where MaterialIssueControlMasterId IN(select Id from MaterialIssueControlMaster Where Id ='" + Id + "')";
                strSOSQL = @"delete from MaterialIssueControlSODetail Where MaterialIssueControlMasterId IN(select Id from MaterialIssueControlMaster Where Id ='" + Id + "')";
                strSQL = @"delete from MaterialIssueControlMaster Where Id ='" + Id + "'";
                strISMSQL = @"delete from TRN.IssueRequestMaster Where Id='" + issueId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                

                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strIRSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strMDSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strISMSQL, true, "1");
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

        [HttpGet, Authorize]
        public ActionResult GetCostingDataList(string LineItemId)
        {

            return Json(clsM.GetCostingDataList(LineItemId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetQBOQDataList(string LineItemId)
        {
            return Json(clsM.GetQBOQDataList(LineItemId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult IssueRequestReport(string mId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IssueRequestReport(identity.PlantId, mId);
            return null;
        }

        public void IssueRequestReport(string plantId, string mId)
        {
            _ = new ReportUtility();
            string issueId = "";
            string fileName = "IssueRequestReport" + plantId + ".docx";
            string strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
            string File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            //makeDictionary();
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                var sqlissue = @"Select distinct IssueRequestMasterId from [TRN].[IssueRequest] Where MaterialIssueControlDetailId IN(SELECT Id FROM dbo.MaterialIssueControlDetail Where MaterialIssueControlMasterId='" + mId + "')";
                DataTable dtIssue = _sqlRepository.GetDataTable(sqlissue);
                if (dtIssue.Rows.Count > 0)
                {
                    issueId = dtIssue.Rows[0]["IssueRequestMasterId"].ToString();
                }
                else
                {
                    throw new CustomException("No issue slip found.");
                }

                WSection section = document.Sections[0];

                DataTable dtOrderMaster;
                dtOrderMaster = clsM.loadIssueRequestMaster(issueId);


                Dictionary<string, string> columns = new Dictionary<string, string>();


                //document.Replace("{Remarks}", dtOrderMaster.Rows[0]["Remarks"].ToString(), false, false);
                //document.Replace("{PreparedBy}", dtOrderMaster.Rows[0]["PreparedBy"].ToString(), false, false);
                document.Replace("{CheckedByName}", dtOrderMaster.Rows[0]["CheckedByName"].ToString(), false, false);
                document.Replace("{AuthorizedByName}", dtOrderMaster.Rows[0]["AuthorizedByName"].ToString(), false, false);
                document.Replace("{EmployeeName}", dtOrderMaster.Rows[0]["ReceivedBy"].ToString(), false, false);



                foreach (DataColumn item in dtOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var dsServiceItems = clsM.loadIssueRequestDetail(issueId);
                var materialTotal = makeIssueDetailsTable(document, dsServiceItems, issueId);//Material Details 
                var serviceTotal = 0.00;


                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();

                StringCollection strColDistinct = new StringCollection();

                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());             //For Same Name Use
                    string text = strReplace[i].ToUpper();

                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }
                document.Replace("{IssueId}", dtOrderMaster.Rows[0]["IssueId"].ToString(), false, false);
                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                ////Creates an instance of the DocToPDFConverter
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);

                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects
                document.Close();
                var filename = "IssueRequestReport-" + plantId + "-" + issueId;
                //Saves the PDF file 
                pdfDocument.Save(filename + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);

                document.Close();


            }
            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }

        public double makeIssueDetailsTable(WordDocument document, DataTable dsOrderMaster, string issueId)
        {
            string replaceString = "{IssueSlipDetails}";

            DataTable dsOrderItems, dsTax;

            //dsOrderItems = loadOrderMasterItems(grnId);
            //dsTax = loadOrderMasterTax(grnId);

            int LasColumnIndex = 10;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            //DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            //if (dv.Count > 0)
            //{
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        LasColumnIndex++;
            //        dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
            //        LasColumnIndex++;
            //    }
            //}


            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            range.ApplyCharacterFormat(FontBold);
            int colSLNo = COL; COL++;
            wTable.Rows[ROW].Cells[colSLNo].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("IssueId");
            range.ApplyCharacterFormat(FontBold);
            int colIssueId = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Cost Center Name");
            range.ApplyCharacterFormat(FontBold);
            int CostCenterNameId = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Expe.Activity Code ");
            range.ApplyCharacterFormat(FontBold);
            int colActivityCode = COL; COL++;




            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
            range.ApplyCharacterFormat(FontBold);
            int colItemName = COL; COL++;
            wTable.Rows[ROW].Cells[colItemName].Width = 120;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticleCode = COL; COL++;
            wTable.Rows[ROW].Cells[colArticleCode].Width = 120;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colSku1 = COL; COL++;
            wTable.Rows[ROW].Cells[colSku1].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colSku2 = COL; COL++;
            wTable.Rows[ROW].Cells[colSku2].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colSku3 = COL; COL++;
            wTable.Rows[ROW].Cells[colSku3].Width = 70;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUOM = COL; COL++;
            wTable.Rows[ROW].Cells[colUOM].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Qty");
            range.ApplyCharacterFormat(FontBold);
            int colValidQty = COL; //COL++;

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    //TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }


                TROW.Cells[colSLNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SiNo"].ToString());
                TROW.Cells[colIssueId].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Id"].ToString());
                TROW.Cells[CostCenterNameId].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CostCenterName"].ToString());
                TROW.Cells[colActivityCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["GLBudgetActivity"].ToString());
                TROW.Cells[colItemName].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMasterName"].ToString());
                TROW.Cells[colArticleCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["StandardName"].ToString());
                TROW.Cells[colSku1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());

                TROW.Cells[colSku2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());

                TROW.Cells[colSku3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());


                TROW.Cells[colUOM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["UOM"].ToString());
                TROW.Cells[colValidQty].AddParagraph().AppendText(dsOrderMaster.Rows[i]["RequestedQty"].ToString());


                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["Total"].ToString());

                //if (dv.Count > 0)
                //{
                //    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                //    //double totalTax = 0;

                //    for (int T = 0; T < dv.Count; T++)
                //    {
                //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                //        if (dvtax.Count > 0)
                //        {
                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));

                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));

                //        }
                //    }

                //}
                //ROW++;
            }


            #region Total
            int TotalRow = ROW + 1;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colSLNo || C == CostCenterNameId || C == colIssueId || C == colActivityCode || C == colItemName || C == colArticleCode || C == colSku1 || C == colSku2 || C == colSku3 || C == colUOM)
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            //double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString());
            //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
            //+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable

            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 30;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }

            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            double total = 0.00;
            return total;
        }


        #endregion
    }
}