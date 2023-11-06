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

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class LotControlController : BaseController
    {
        #region Constructor
        string TableName = "dbo.LotControl";
        private readonly ISqlRepository _sqlRepository;
        public LotControlController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
    
        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public JsonResult GetPOLotControlSettingData(string entityId, string PoId)
        {
            try
            {
                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                return Json(order.GetPOLotControlSettingData(entityId, PoId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost, Authorize]
        public JsonResult CopyData(string ProductionOrderId, string Id)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment _attachment = new Library.OrderManagement.BOM.TemplateAttchment();
                CopyDetail(ProductionOrderId, Id);

                return Json(new { Error = false, Message = "BOM copied successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
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

        public void CopyDetail(string ProductionOrderId, string Id)
        {
            DataSet dsDetail, dsId;
            string NewId = ""; 
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ProductionOrderLotControl where 1=2", out dsDetail, false, "1");
                con.OpenDataSetThroughAdapter("SELECT CId=Count(Id)+1 from dbo.ProductionOrderLotControl Where ProductionOrderId='"+ ProductionOrderId + "'", out dsId, false, "1");

                DataTable dtDetail = _sqlRepository.GetDataTable("select * from dbo.ProductionOrderLotControl WHERE Id='" + Id + "'");

                NewId = ProductionOrderId + "-" + dsId.Tables[0].Rows[0]["CId"].ToString();

                DataRow drDestination = dsDetail.Tables[0].NewRow();
                CopyRow(dtDetail.Rows[0], ref drDestination);
                drDestination["Id"] = NewId;
                drDestination["ProductionOrderId"] = ProductionOrderId;
                dsDetail.Tables[0].Rows.Add(drDestination);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDetail);
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }

        [HttpPost, Authorize]
        public JsonResult SaveTNCRowData(Dictionary<string, object> data)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductionOrderLotControl where  Id='" + data["Id"] + "'", out dsChild, false, "1");

                if (data != null)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + data["Id"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, data);
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveLotSettingData(List<Dictionary<string, object>> data, string poId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsChild, dsId;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                #region LotControlSetting 

                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductionOrderLotControl where  ProductionOrderId='" + poId + "'", out dsChild, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT CId=Count(Id)+1 from dbo.ProductionOrderLotControl Where ProductionOrderId='" + poId + "'", out dsId, false, "1");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = poId + "-" + dsId.Tables[0].Rows[0]["CId"].ToString();
                            item["ProductionOrderId"] = poId;

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
                obj.SaveDataSets(dsChild);
                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}