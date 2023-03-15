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
    public class InputConfirmationController : BaseController
    {
        #region -- Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IIssueRequestService _issueRequestService;
        private readonly IPKGeneratorService _pkGeneratorService;
        clsMaterial clsM = new clsMaterial();
        public InputConfirmationController(ISqlRepository R, IIssueRequestService issueRequestService, IPKGeneratorService pkGeneratorService)
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
    
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult EntityList()
        {
            return Json(clsM.EntityList(), JsonRequestBehavior.AllowGet);
        }
     
        [HttpPost, Authorize]
        public ActionResult GetList(string entityid, string column, string value)
        {
            return Json(clsM.GetRunningPOList(entityid, column, value), JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet, Authorize]
        public ActionResult GetIssueSlipDataByPOIdList(string ProductionOrderId)
        {
            return Json(clsM.GetIssueSlipDataByPOIdList(ProductionOrderId), JsonRequestBehavior.AllowGet);
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
                inventoryIssue.Orderspecific = "Yes";
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

        #endregion
    }
}