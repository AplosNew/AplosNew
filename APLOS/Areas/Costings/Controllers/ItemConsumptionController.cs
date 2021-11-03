#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Costings.Controllers
{
    public class ItemConsumptionController : BaseController
    {
        #region Constructor

        string TableName = "dbo.ItemConsumtionMaster";
        string CTableName = "dbo.ItemConsumtionComponent";
        string DTableName = "dbo.ItemConsumtionChild";

        private readonly ISqlRepository _sqlRepository;
        public ItemConsumptionController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- O P E R A T I O N --
        [HttpPost, Authorize]
        public ActionResult GetCostingItemForSelection()
        {
            string sql = @"SELECT ci.Id, ci.ShortName,cat.UserName AS CostingCategory,  ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName,                         
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                            ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            LEFT OUTER JOIN hkp.CostingCategory AS cat ON cat.Id=ci.CostingCategoryId
                            WHERE cc.CostingSegment='directmaterial'
                            order by ci.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetMaster(string Production, string CostingItem)
        {
            string sql = @"select m.*,c.* ,ci.UserName CostingItemName
                            from ItemConsumtionMaster m
                            left join ItemConsumtionComponent c on c.ItemConsumtionMasterId=m.Id
							left join HKP.CostingItem ci on ci.Id = m.CostingItemId
                            where m.ProductMasterId='" + Production + "' and m.CostingItemId='" + CostingItem + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetData()
        {
            string sql = @"select m.*,p.UserName ProductName,ci.UserName CostingItemName
                            from ItemConsumtionMaster m
                            left join MST.ProductMaster p on p.Id = m.ProductMasterId
                            left join HKP.CostingItem ci on ci.Id = m.CostingItemId";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetComponent(string MId)
        {
            string sql = @"select c.* 
							from ItemConsumtionComponent c
							left join ItemConsumtionMaster m on m.Id=c.ItemConsumtionMasterId
							left join HKP.CostingItem ci on ci.Id = m.CostingItemId
							where c.ItemConsumtionMasterId='" + MId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost, Authorize]
        public ActionResult GetChildData(string MId, string ComId)
        {
            string sql = @"select ch.* 
                            from ItemConsumtionMaster m
                            left join ItemConsumtionComponent c on c.ItemConsumtionMasterId = m.Id
                            left join ItemConsumtionChild ch on ch.ItemConsumtionMasterId = m.Id and ch.ItemConsumtionComponentId = c.Id
                            where m.Id = '" + MId + "' and c.Id = '"+ ComId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(Dictionary<string, object> MasterData, Dictionary<string, object> ComponentData, List<Dictionary<string, object>> ChildData)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                #region - validation -
                if (ChildData == null || ChildData.Count == 0)
                    throw new Exception("No parameter data found!!");

                for (int i = 0; i < ChildData.Count; i++)
                {
                    if(OTSBD.clsStaticInfo.dbl( ChildData[i]["Actual"])<=0)
                        throw new Exception("Actual data cannot be less or equal zero");


                    if (OTSBD.clsStaticInfo.dbl(ChildData[i]["Allowance"]) < 0)
                        throw new Exception("Allowance data cannot be negative");
                    
                    if (OTSBD.clsStaticInfo.dbl(ChildData[i]["Number"]) <= 0)
                        throw new Exception("Number of Components cannot be less or equal zero");


                }

                DataSet dsComponents;
                con.OpenDataSetThroughAdapter("select * from " + CTableName + " where ComponentName='" + ComponentData["ComponentName"] + "' And ItemConsumtionMasterId= '" + MasterData["Id"] + "' AND  Id<>'" + ComponentData["Id"] + "'", out dsComponents, false, "1");
                if (dsComponents.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Component Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Description='" + MasterData["Description"] + "' And ProductMasterId = '"+MasterData["ProductMasterId"] + "' And CostingItemId = '" + MasterData["CostingItemId"] + "'  AND  Id<>'" + MasterData["Id"] + "'", out dsComponents, false, "1");
                if (dsComponents.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Description Name already exists!!!");

                #endregion
                #region Master
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + MasterData["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                string _DId = "";
                string _MId = "";
                string _CId = "";
                string _ComId = "";

                #region Master data Save
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    MasterData["Id"] = "ICM-" + _Id;
                    _MId = "ICM-" + _Id;
                    //MasterData["PlantId"] = identity.PlantId;
                    AddNewRow(dsMaster.Tables[0], MasterData);
                }
                else
                {
                    _MId = MasterData["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], MasterData);
                }
                #endregion

                #endregion

                #region Component Data

                DataSet dsComponent;
                con.OpenDataSetThroughAdapter("select * from " + CTableName + " where ItemConsumtionMasterId='" + _MId + "' and Id='"+ ComponentData["Id"] + "'", out dsComponent, false, "1");

                #region data update
                if (dsComponent.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(CTableName, out _CId);

                    ComponentData["Id"] = "ICC-" + _CId;
                    _ComId = "ICC-" + _CId;
                    ComponentData["ItemConsumtionMasterId"] = _MId;
                    //MasterData["PlantId"] = identity.PlantId;
                    AddNewRow(dsComponent.Tables[0], ComponentData);
                }
                else
                {
                    _ComId = ComponentData["Id"].ToString();
                    EditRow(dsComponent.Tables[0].Rows[0], ComponentData);
                }

                #endregion

                #endregion

                #region Child 

                DataSet dsChild;

                con.OpenDataSetThroughAdapter("select * from " + DTableName + " where  ItemConsumtionMasterId='" + _MId + "' and ItemConsumtionComponentId ='" + _ComId + "'", out dsChild, false, "1");

                for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                {
                    List<Dictionary<string, object>> temp = ChildData.Where(ee => ee["Id"] == dsChild.Tables[0].Rows[i]["Id"].ToString()).ToList();
                    if (temp == null || temp.Count == 0)
                        dsChild.Tables[0].Rows[i].Delete();
                }
                #region data Save

                foreach (var item in ChildData)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(DTableName, out _DId);

                        item["Id"] = "ICD-" + _DId;
                        item["ItemConsumtionMasterId"] = _MId;
                        item["ItemConsumtionComponentId"] = _ComId;
                        AddNewRow(dsChild.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }


                #endregion

                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsComponent, dsChild);

                return Json(new { Error = false, Data = MasterData, Message = AplosMessage.Updated });
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
        public ActionResult Delete(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + Id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult DeleteChild(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + DTableName + " where id='" + Id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult DeleteComponents(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + DTableName + " where ItemConsumtionComponentId='" + Id + "'");
                con.executeQuery("delete from " + CTableName + " where id='" + Id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}