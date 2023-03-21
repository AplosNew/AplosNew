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
            var jsondata = Json(clsM.GetRunningPOList(entityid, column, value), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
       
        [HttpGet, Authorize]
        public ActionResult GetIssueSlipDataByPOIdList(string ProductionOrderId)
        {
            var jsondata = Json(clsM.GetIssueSlipDataByPOIdList(ProductionOrderId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public ActionResult GetInventoryMaterialData(string confirmdate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(clsM.GetInventoryMaterialData(identity.PlantId, confirmdate), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetSavedData(string column, string value)
        {
            var jsondata = Json(clsM.GetInputSavedData(column, value), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedChildData(string masterId)
        {
            var jsondata = Json(clsM.GetSavedinputDetailData(masterId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpGet, Authorize]
        public ActionResult GetSOItemList(string entityid, string ProductionOrderId)
        {
            var jsondata = Json(clsM.GetSOItemList(entityid, ProductionOrderId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> model, List<Dictionary<string, object>> dataList)
        {
            try
            {
                SaveData(model, dataList);

                return Json(new { Data = model, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }
        private void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> dataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild, dsIdChild;
            string _Id = string.Empty;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.InputConfirmationMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("InputConfirmationMaster", out _Id);

                    data["Id"] = "M" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                               

                #region InputConfirmationDetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.InputConfirmationDetail where  InputConfirmationMasterId='" + _Id + "'", out dsChild, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT Count(Id)Idc FROM dbo.InputConfirmationDetail where  InputConfirmationMasterId='" + _Id + "'", out dsIdChild, false, "1");
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
                            item["InputConfirmationMasterId"] = _Id;
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
                obj.SaveDataSets(dsMaster, dsChild);

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
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            DataSet dsIssue=null;
            string strSQL, strMDSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                ConnectionManager.DAL.ConManager Con = new ConnectionManager.DAL.ConManager("1");

                strMDSQL = @"delete from InputConfirmationDetail Where InputConfirmationMasterId IN(select Id from InputConfirmationMaster Where Id ='" + Id + "')";
                strSQL = @"delete from InputConfirmationMaster Where Id ='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                

                objCon.ExecuteNonQueryWrapper(strMDSQL, true, "1");
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

        #endregion
    }
}