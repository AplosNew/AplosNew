#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.OrderManagements;
using Library.OrderManagement.Production;
using Library.Security.Core;
using Library.Service.OrderManagements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class DispatchMasterController : BaseController
    {
        #region Constructor
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;
        public DispatchMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult DispatchPlan()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetDispatchEntityProcessSettingData(string EntityId)
        {
            return Json(_productionSummaryData.GetDispatchEntityProcessSettingData(EntityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Insert(Dictionary<string, object> data,List<Dictionary<string, object>> selectedSalesOrderList)
        {
            SaveData(data, selectedSalesOrderList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        private void SaveData(Dictionary<string, object> data,List<Dictionary<string, object>> selectedSalesOrderList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster, dsDispatchDetail, dsDispatchDetailSO, dsDispatchSKUMaster, dsDispatchSKUDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[DispatchMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id, _DispatchDetailId = "";
                string masterId = "";
                string dispatchDetailId = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchMaster", out _Id);

                    data["Id"] = "DM" + _Id;
                    data["PlantId"] = identity.PlantId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["PlantId"] = identity.PlantId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                 masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchDetail WHERE DispatchMasterId ='" + data["Id"] + "'", out dsDispatchDetail, false, "1");

                if (dsDispatchDetail.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchDetail", out _DispatchDetailId);

                    DataRow dr = dsDispatchDetail.Tables[0].NewRow();

                    dr["Id"] = "DD" + _DispatchDetailId;
                    dr["DispatchMasterId"] = masterId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsDispatchDetail.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsDispatchDetail.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                dispatchDetailId = dsDispatchDetail.Tables[0].Rows[0]["Id"].ToString();

                // DispatchDetailSO
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchDetailSO WHERE DispatchDetailId ='" + dispatchDetailId + "'", out dsDispatchDetailSO, false, "1");

                foreach (var item in selectedSalesOrderList)
                {
                    DataView dv = new DataView(dsDispatchDetailSO.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = GetDispatchOrderItemPK();
                        item["DispatchDetailId"] = dispatchDetailId;

                        AddNewRow(dsDispatchDetailSO.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }

                // DispatchSKUMaster & DispatchSKUDetail
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchSKUMaster WHERE DispatchDetailId ='" + dispatchDetailId + "'", out dsDispatchSKUMaster, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchSKUDetail WHERE 1=2", out dsDispatchSKUDetail, false, "1");
               
                DataTable Detail = _sqlRepository.GetDataTable("SELECT * FROM [dbo].[PackingContentMaster] where Id IN(SELECT PackingContentMasterId FROM [dbo].[PackingChild] Where IsConfirmed=1 AND ISNULL(DispatchSKUMasterId,'')='')");

                for (int i = 0; i < Detail.Rows.Count; i++)
                {
                    DataRow drDetailDestination = dsDispatchSKUMaster.Tables[0].NewRow();
                    CopyRow(Detail.Rows[i], ref drDetailDestination);
                    drDetailDestination["Id"] = dispatchDetailId + "-" + (i + 1);
                    drDetailDestination["DispatchDetailId"] = dispatchDetailId;
                    dsDispatchSKUMaster.Tables[0].Rows.Add(drDetailDestination);

                    //Process.DefaultView.RowFilter = "ProductionBulletinTemplateMasterId='" + Detail.Rows[i]["Id"].ToString() + "'";
                    //for (int K = 0; K < Process.DefaultView.Count; K++)
                    //{
                    //    GetOperationMasterByOperationVariation(Process.DefaultView[K].Row["OperationVariationId"].ToString(), out DataSet dsOperationMaster);

                    //    DataRow drDetailSKUDestination = ProductionBulletinTemplateDetail.Tables[0].NewRow();
                    //    CopyRow(Process.DefaultView[K].Row, ref drDetailSKUDestination);
                    //    drDetailSKUDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                    //    drDetailSKUDestination["ProductionBulletinTemplateMasterId"] = NewId + "-" + (i + 1);

                    //    if (string.IsNullOrEmpty(dsOperationMaster.Tables[0].Rows[0]["OperationMasterId"].ToString()))
                    //    {
                    //        drDetailSKUDestination["OperationMasterId"] = DBNull.Value;
                    //    }
                    //    else
                    //    {
                    //        drDetailSKUDestination["OperationMasterId"] = dsOperationMaster.Tables[0].Rows[0]["OperationMasterId"].ToString();
                    //    }

                    //    ProductionBulletinTemplateDetail.Tables[0].Rows.Add(drDetailSKUDestination);
                    //}

                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDispatchDetail, dsDispatchDetailSO, dsDispatchSKUMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }

        private string GetDispatchMaterialPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchMaterial", out sID);
            return sID;
        }
        private string GetDispatchOrderItemPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchDetailSO", out sID);
            return sID;
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

        [HttpGet, Authorize]
        public ActionResult GetSOList(string customerId)
        {
            try
            {
                return Json(_productionSummaryData.GetSOList(customerId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDispatchDetailSOList(string masterId)
        {
            try
            {
                return Json(_productionSummaryData.GetDispatchDetailSOList(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                return Json(_productionSummaryData.GetDispatchMasterList(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetAllConfirmedPackingContentData()
        {
            return Json(_productionSummaryData.GetAllConfirmedPackingContentData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPackingChildDataList(string MasterId)
        {
            string sql = @"SELECT 0 AS [Check],P.*, [State]=CASE WHEN P.IsConfirmed=1 THEN 1 ELSE 0 END FROM [dbo].[PackingChild] P WHERE P.PackingContentMasterId='" + MasterId + "' AND P.IsConfirmed=1";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #region DispatchPlan
        [HttpPost]
        public JsonResult DispatchPlanInsert(Dictionary<string, object> data)
        {
            SaveDispatchPlanData(data);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }
        private void SaveDispatchPlanData(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[DispatchPlanMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id ;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchPlanMaster", out _Id);

                    data["Id"] = "DP" + _Id;
                    data["PlantId"] = identity.PlantId;
                    data["ByWhom"] = identity.UserId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["PlantId"] = identity.PlantId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion
        #endregion
    }

}