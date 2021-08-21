#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;
using System;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.Data.Sql;
using System.Collections.Generic;
using Library.Data;
using Library.Service.Systems;
using Library.Model.Materials;
using Library.Service.Materials;
using Library.OrderManagement.Production;
using Library.OrderManagement.Packing;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class PackingContentController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ICharacteristicsValueService _characteristicsValueService;
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        clsPacking _clsPacking = new clsPacking();
        public PackingContentController(ISqlRepository R, IPKGeneratorService pkGeneratorService, ICharacteristicsValueService characteristicsValueService)
        {
            _sqlRepository = R;
            _pkGeneratorService = pkGeneratorService;
            _characteristicsValueService = characteristicsValueService;

        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetProductionOrderDataList()
        {
            return Json(_productionSummaryData.GetProductionOrderDataList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEntityProcessSettingData(string EntityId)
        {
            return Json(_productionSummaryData.GetEntityProcessSettingData(EntityId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetPackingContentDataByPRId(string PRId)
        {
            return Json(_productionSummaryData.GetPackingContentDataByPRId(PRId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPackingContentDataByPRIdWithTran(string PRId)
        {
            return Json(_productionSummaryData.GetPackingContentDataByPRIdWithTran(PRId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            string strSQL = @"SELECT A.*,
                                ProcessNature=(Select EPT.ProcessNature FROM TRN.ProductionOrder PO 
							                        LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=PPO.ProductionOrderId AND EPT.ProcessNature='Packing') 
							  ,IsPackingSKURequired=(Select EPT.IsPackingSKURequired FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=PPO.ProductionOrderId AND EPT.ProcessNature='Packing') 
                              ,PackingForm=(Select EPT.PackingForm FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=PPO.ProductionOrderId AND EPT.ProcessNature='Packing')
                            FROM [dbo].[PackingContentMaster] A
                            LEFT JOIN(Select top(1) * from  dbo.PackingProductionOrder) PPO ON PPO.PackingContentMasterId=A.Id";
            return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPackingProductionOrderData(string MasterId)
        {
            return Json(_productionSummaryData.GetPackingProductionOrderData(MasterId), JsonRequestBehavior.AllowGet);
        }

        private bool CheckCombination(Dictionary<string, object> data)
        {
            try
            {
                //var _sql = @"SELECT * FROM [dbo].[PackingContentMaster]  where id<>'" + data["Id"] + "' and ProductionOrderId='" + data["ProductionOrderId"] + "'";
                string _sql = @"";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, IEnumerable<PackingContentDetail> packingContentDetails, IEnumerable<PackingChild> packingChilds, List<Dictionary<string, object>> packingProductionOrderList)
        {
            try
            {
                if (data != null)
                {
                    //var IsDuplicateEntryAllowed = CheckCombination(data);

                    //if (IsDuplicateEntryAllowed)
                    //{
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    DataSet dsMaster, dsProductionOrder;
                    DataView dvProductionOrder = null;
                    DataRow drProductionOrder = null;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[PackingContentMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[PackingProductionOrder] WHERE PackingContentMasterId='" + data["Id"] + "'", out dsProductionOrder, false, "1");

                    string _Id = "";

                    #region data insert-update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PackingContentMaster", out _Id);

                        data["Id"] = "PB" + _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data insert-update

                    _Id = data["Id"].ToString();

                    #region PackingProductionOrder
                    int count = 0;
                    foreach (var item in packingProductionOrderList)
                    {

                        dvProductionOrder = new DataView(dsProductionOrder.Tables[0]);
                        
                       // dvProductionOrder.RowFilter = "Id='" + item["Id"].ToString() + "'";
                        dvProductionOrder.RowFilter = "PackingContentMasterId='" + _Id + "' AND ProductionOrderId='"+ item["ProductionOrderId"].ToString() + "'";

                        if (dvProductionOrder.Count == 0)
                        {
                            count++;
                            string pk = _Id + "-" + count;
                            drProductionOrder = dsProductionOrder.Tables[0].NewRow();
                            drProductionOrder["Id"] = pk;
                            drProductionOrder["PackingContentMasterId"] = _Id;
                            drProductionOrder["ProductionOrderId"] = item["ProductionOrderId"].ToString();
                           

                            drProductionOrder["AddedBy"] = identity.Name;
                            drProductionOrder["AddedDate"] = DateTime.Now;
                            drProductionOrder["AddedFromIp"] = identity.IPAddress;

                            dsProductionOrder.Tables[0].Rows.Add(drProductionOrder);
                        }
                        else
                        {
                            drProductionOrder = dvProductionOrder[0].Row;
                            drProductionOrder.BeginEdit();
                            drProductionOrder["PackingContentMasterId"] = _Id;
                            drProductionOrder["ProductionOrderId"] = item["ProductionOrderId"].ToString();


                            drProductionOrder["UpdatedBy"] = identity.Name;
                            drProductionOrder["UpdatedDate"] = DateTime.Now;
                            drProductionOrder["UpdatedFromIP"] = identity.IPAddress;

                            drProductionOrder.EndEdit();
                        }
                        dvProductionOrder.RowFilter = null;
                    }

                    #endregion



                    SavePackingContentDetail(packingContentDetails, _Id, out DataSet dsBp);
                    SavePackingChildData(packingChilds, _Id, out DataSet dsPC);

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsBp, dsPC, dsProductionOrder);
                    //}
                    //else
                    //{
                    //    throw new Exception("Selected combination already exists...");
                    //}
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        [HttpPost]
        public JsonResult Edit(Dictionary<string, object> data, IEnumerable<PackingContentDetail> packingContentDetails, IEnumerable<PackingChild> packingChilds, List<Dictionary<string, object>> packingProductionOrderList)
        {
            try
            {
                if (data != null)
                {
                    //var IsDuplicateEntryAllowed = CheckCombination(data);

                    //if (IsDuplicateEntryAllowed)
                    //{
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    DataSet dsMaster, dsProductionOrder;
                    DataView dvProductionOrder = null;
                    DataRow drProductionOrder = null;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[PackingContentMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[PackingProductionOrder] WHERE PackingContentMasterId='" + data["Id"] + "'", out dsProductionOrder, false, "1");

                    string _Id = "";

                    #region data insert-update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PackingContentMaster", out _Id);

                        data["Id"] = "PB" + _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data insert-update

                    _Id = data["Id"].ToString();

                    #region PackingProductionOrder
                    int count = 0;
                    foreach (var item in packingProductionOrderList)
                    {

                        dvProductionOrder = new DataView(dsProductionOrder.Tables[0]);

                        dvProductionOrder.RowFilter = "PackingContentMasterId='" + _Id + "' AND ProductionOrderId='" + item["ProductionOrderId"].ToString() + "'";

                        if (dvProductionOrder.Count == 0)
                        {
                            count++;
                            string pk = _Id + "_" + count;
                            drProductionOrder = dsProductionOrder.Tables[0].NewRow();
                            drProductionOrder["Id"] = pk;
                            drProductionOrder["PackingContentMasterId"] = _Id;
                            //drProductionOrder["ProductionOrderId"] = _Id;


                            drProductionOrder["AddedBy"] = identity.Name;
                            drProductionOrder["AddedDate"] = DateTime.Now;
                            drProductionOrder["AddedFromIp"] = identity.IPAddress;

                            dsProductionOrder.Tables[0].Rows.Add(drProductionOrder);
                        }
                        else
                        {
                            drProductionOrder = dvProductionOrder[0].Row;
                            drProductionOrder.BeginEdit();
                            drProductionOrder["PackingContentMasterId"] = _Id;
                            //drProductionOrder["ProductionOrderId"] = item.MaterialMasterId;


                            drProductionOrder["UpdatedBy"] = identity.Name;
                            drProductionOrder["UpdatedDate"] = DateTime.Now;
                            drProductionOrder["UpdatedFromIP"] = identity.IPAddress;

                            drProductionOrder.EndEdit();
                        }
                        dvProductionOrder.RowFilter = null;
                    }

                    #endregion



                    SavePackingContentDetail(packingContentDetails, _Id, out DataSet dsBp);
                    SavePackingChildData(packingChilds, _Id, out DataSet dsPC);

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsBp, dsPC, dsProductionOrder);
                    //}
                    //else
                    //{
                    //    throw new Exception("Selected combination already exists...");
                    //}
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
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

        private string GeteDetailPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PackingContentDetail), out sID);
            return sID;
        }

        private string GetPackingChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PackingChild), out sID);
            return sID;
        }
        [HttpPost, Authorize]
        public JsonResult CreateDetail(IEnumerable<PackingContentDetail> entities, string MasterId)
        {
            try
            {
                SaveDetailData(entities, MasterId);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void SaveDetailData(IEnumerable<PackingContentDetail> data, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;


                    foreach (var item in data)
                    {

                        string sql = "SELECT * FROM [dbo].[PackingContentDetail] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {

                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = GeteDetailPK();

                            dr["PackingContentMasterId"] = MasterId;
                            dr["MaterialMasterId"] = item.MaterialMasterId;
                            dr["ArticleId"] = item.ArticleId;
                            dr["Qty"] = item.Qty;
                            dr["FirstCharacteristicsValueId"] = item.FirstCharacteristicsValueId;
                            dr["SecondCharacteristicsValueId"] = item.SecondCharacteristicsValueId;
                            dr["ThirdCharacteristicsValueId"] = item.ThirdCharacteristicsValueId;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIp"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["MaterialMasterId"] = item.MaterialMasterId;
                            dr["ArticleId"] = item.ArticleId;
                            dr["Qty"] = item.Qty;
                            dr["FirstCharacteristicsValueId"] = item.FirstCharacteristicsValueId;
                            dr["SecondCharacteristicsValueId"] = item.SecondCharacteristicsValueId;
                            dr["ThirdCharacteristicsValueId"] = item.ThirdCharacteristicsValueId;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIp"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void SavePackingContentDetail(IEnumerable<PackingContentDetail> BP, string PackingContentMasterId, out DataSet dsBp)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                DataTable dtBp = null;
                DataView dvBp = null;
                DataRow drBp = null;
                string BPId = string.Empty;
                string sql = "SELECT * FROM [dbo].[PackingContentDetail] ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");

                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();

                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PackingContentDetail), out BPId);
                int count = 0;

                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");

                foreach (var item in BP)
                {

                    dvBp = new DataView(dsBp.Tables[0]);
                    //dvBp.Table = ;
                    dvBp.RowFilter = "Id='" + item.Id + "'";

                    if (dvBp.Count == 0)
                    {
                        count++;
                        string pk = BPId + "-" + count;
                        drBp = dsBp.Tables[0].NewRow();
                        drBp["Id"] = pk;
                        drBp["PackingContentMasterId"] = PackingContentMasterId;
                        drBp["MaterialMasterId"] = item.MaterialMasterId;
                        drBp["SalesOrderId"] = item.SalesOrderId;
                        drBp["ArticleId"] = item.ArticleId;
                        drBp["Qty"] = item.Qty;
                        drBp["FirstCharacteristicsValueId"] = item.FirstCharacteristicsValueId;
                        drBp["SecondCharacteristicsValueId"] = item.SecondCharacteristicsValueId;
                        drBp["ThirdCharacteristicsValueId"] = item.ThirdCharacteristicsValueId;

                        drBp["AddedBy"] = identity.Name;
                        drBp["AddedDate"] = DateTime.Now;
                        drBp["AddedFromIp"] = identity.IPAddress;

                        dsBp.Tables[0].Rows.Add(drBp);
                    }
                    else
                    {
                        drBp = dvBp[0].Row;
                        drBp.BeginEdit();
                        drBp["PackingContentMasterId"] = PackingContentMasterId;
                        drBp["MaterialMasterId"] = item.MaterialMasterId;
                        drBp["SalesOrderId"] = item.SalesOrderId;
                        drBp["ArticleId"] = item.ArticleId;
                        drBp["Qty"] = item.Qty;
                        drBp["FirstCharacteristicsValueId"] = item.FirstCharacteristicsValueId;
                        drBp["SecondCharacteristicsValueId"] = item.SecondCharacteristicsValueId;
                        drBp["ThirdCharacteristicsValueId"] = item.ThirdCharacteristicsValueId;

                        drBp["UpdatedBy"] = identity.Name;
                        drBp["UpdatedDate"] = DateTime.Now;
                        drBp["UpdatedFromIP"] = identity.IPAddress;

                        drBp.EndEdit();
                    }
                    dvBp.RowFilter = null;
                }

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void SavePackingChildData(IEnumerable<PackingChild> BP, string PackingContentMasterId, out DataSet dsPC)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                DataTable dtBp = null;
                //DataSet dsBp = null;
                DataView dvBp = null;
                DataRow drBp = null;
                string BPId = string.Empty;
                string sql = "SELECT * FROM [dbo].[PackingChild] ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsPC, false, "1");

                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();

                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PackingChild), out BPId);
                int count = 0;

                objCon.OpenDataSetThroughAdapter(sql, out dsPC, false, "1");

                foreach (var item in BP)
                {

                    dvBp = new DataView(dsPC.Tables[0]);
                    //dvBp.Table = ;
                    dvBp.RowFilter = "Id='" + item.Id + "'";

                    if (dvBp.Count == 0)
                    {
                        count++;
                        string pk = BPId + "-" + count;
                        drBp = dsPC.Tables[0].NewRow();
                        drBp["Id"] = pk;
                        drBp["PackingContentMasterId"] = PackingContentMasterId;
                        drBp["Sequence"] = item.Sequence;

                        drBp["AddedBy"] = identity.Name;
                        drBp["AddedDate"] = DateTime.Now;
                        drBp["AddedFromIp"] = identity.IPAddress;

                        dsPC.Tables[0].Rows.Add(drBp);
                    }
                }

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreatePackingChild(IEnumerable<PackingChild> Childs, string PackingContentMasterId)
        {
            try
            {
                DeletePackingChildData(PackingContentMasterId);
                SavePackingChildData(Childs);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }
        public void DeletePackingChildData(string PackingContentMasterId)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[PackingChild] WHERE PackingContentMasterId = '" + PackingContentMasterId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
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
        private void SavePackingChildData(IEnumerable<PackingChild> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;


                    foreach (var item in data)
                    {

                        string sql = "SELECT * FROM [dbo].[PackingChild] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {

                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = GetPackingChildPK();

                            dr["PackingContentMasterId"] = item.PackingContentMasterId;
                            dr["Sequence"] = item.Sequence;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIp"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["PackingContentMasterId"] = item.PackingContentMasterId;
                            dr["Sequence"] = item.Sequence;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIp"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPackingContentDetailDataList(string MasterId)
        {
            string sql = @"SELECT  PCD.*,MM.UserName AS MaterialMasterName,MMA.StandardName AS ArticleName,CV1.UserName as FirstCharacteristicsValue
                           ,CV2.UserName as SecondCharacteristicsValue, CV3.UserName as ThirdCharacteristicsValue,A.TotalQty,A.SalesOrderId

                           FROM [dbo].[PackingContentDetail] PCD
                           left join MSt.MaterialMaster MM on MM.id= PCD.MaterialMasterId
                           left join MST.MaterialMasterArticle MMA on MMA.id= PCD.ArticleId
                           left join HKP.CharacteristicsValue CV1 on cv1.id= PCD.FirstCharacteristicsValueId
                           left join HKP.CharacteristicsValue CV2 on cv2.id= PCD.SecondCharacteristicsValueId
                           left join HKP.CharacteristicsValue CV3 on CV3.id= PCD.ThirdCharacteristicsValueId
                           LEFT JOIN (
                           SELECT
                         SO.Id SalesOrderId,SUM(
                        CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE
                        CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                        CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END
                        ) AS TotalQty,pod.ProductionOrderId

                        FROM trn.SalesOrder AS so
                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                        INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                        LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId
                        LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId

                        LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id
                        LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
                        LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id

                        LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=fc.CharacteristicsValueId
                        LEFT JOIN hkp.Characteristics AS c1 ON c1.Id=cv1.CharacteristicsId

                        LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=sc.CharacteristicsValueId
                        LEFT JOIN hkp.Characteristics AS c2 ON c2.Id=cv2.CharacteristicsId

                        LEFT JOIN hkp.CharacteristicsValue AS cv3 ON cv3.Id=tc.CharacteristicsValueId
                        LEFT JOIN hkp.Characteristics AS c3 ON c3.Id=cv3.CharacteristicsId

                        GROUP BY pod.ProductionOrderId,SO.Id
                           ) A ON A.SalesOrderId=PCD.SalesOrderId
                           WHERE PCD.PackingContentMasterId='" + MasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPackingChildDataList(string MasterId)
        {
            string sql = @"SELECT P.*, [State]=CASE WHEN P.IsConfirmed=1 THEN 1 ELSE 0 END FROM [dbo].[PackingChild] P WHERE PackingContentMasterId='" + MasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetSalesOrderListSearch(string column, string value, string productionorderid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_clsPacking.GetSalesOrderListSearch(column, value, productionorderid, identity.PlantId), JsonRequestBehavior.AllowGet);
        }



        #endregion Operations

    }

}