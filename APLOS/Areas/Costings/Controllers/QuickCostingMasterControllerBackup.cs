#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Costings;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


#endregion Using

namespace Aplos.Areas.Costings.Controllers
{
    public class OrderCostingController_backup : BaseController
    {
        string TableName = "dbo.[OrderCostingMasterTemplate]";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public OrderCostingController_backup(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        [Authorize, Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            string sql = @"select * from '" + TableName + "' wher Id = '" + Id + "' ";
            try
            {
                var _master = _sqlRepository.GetDataCollection(sql);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)

        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory,CUR.Code AS Currency
							--,CostingType=case  when pm.CostingType = 'CostingType1' then 'Garment' 
							--	else   'Fabric' end
                             ,pm.CostingType
							from OrderCostingMasterTemplate qcm 
							
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join scs.Currency CUR on CUR.Id=qcm.CurrencyId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
                          WHERE QCM.PlantId='" + identity.PlantId + @"'  ) AS TEMP WHERE 1=1 AND " + strkey;


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);
            Dictionary<string, string> CostingType = new Dictionary<string, string>();
            foreach (var item in Enum.GetValues(typeof(CostingType)))
            {
                CostingType.Add(item.ToString(), AccessInfo.GetEnumDescription((CostingType)(int)item));
            }

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    data[i]["CostingType"] = CostingType[data[i]["CostingType"].ToString()];
                }
                catch (Exception)
                {


                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetSOList(string column, string value, string TemplateId)

        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (
                                SELECT convert(bit,0) AS isChecked,mm.UserName AS Material,mma.StandardName AS Article,  moi.MasterOrderId,p.Id AS PartyId,pm.UserName AS Product, p.UserName AS Customer, SO.Id AS SalesOrder, SO.DeliveryDate,pm.Id
                                  FROM trn.MasterOrder AS mo
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                                left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId
                                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                                left outer join hkp.Party AS p ON p.Id=mo.PartyId

                                WHERE --p.Id IN (SELECT ocmt.CustomerId
                                                 --FROM OrderCostingMasterTemplate AS ocmt WHERE ocmt.Id='" + TemplateId + @"'
                                --) 
                                --AND pm.Id IN (SELECT ocmt.ProductMasterId
                                                 --FROM OrderCostingMasterTemplate AS ocmt WHERE ocmt.Id='" + TemplateId + @"') 
                                 --AND 
                                isnull(so.OrderCostingMasterTemplateId,'')=''
                               
                            ) AS TEMP WHERE 1=1 AND " + strkey + " ORDER BY TEMP.MasterOrderId, TEMP.Product, TEMP.SalesOrder";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);


            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetSOListForTemplate(string TemplateId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                                SELECT convert(bit,0) AS isChecked,mm.UserName AS Material,mma.StandardName AS Article, moi.MasterOrderId,p.Id AS PartyId,pm.UserName AS Product, p.UserName AS Customer, SO.Id AS SalesOrder, SO.DeliveryDate,pm.Id
                                  FROM trn.MasterOrder AS mo
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                                left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId
                                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                                left outer join hkp.Party AS p ON p.Id=mo.PartyId

                                WHERE isnull(so.OrderCostingMasterTemplateId,'')='" + TemplateId + @"'
                                ORDER BY mo.Id,pm.UserName,so.Id
                            ";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult UpdateSOData(string TemplateId, List<Dictionary<string, object>> SOList)
        {

            try
            {
                string _soList = "''";
                for (int i = 0; i < SOList.Count; i++)
                    _soList += ",'" + SOList[i]["SalesOrder"].ToString() + "'";


                string sql = "update trn.SalesOrder set OrderCostingMasterTemplateId='" + TemplateId + "' Where Id in (" + _soList + ")";

                _sqlRepository.ExecuteSqlCommand(sql);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Message = "SO Updated Successfully" });

        }
        [HttpPost, Authorize]
        public ActionResult DeleteSOData(string TemplateId, string SOId)
        {

            try
            {

                string sql = "update trn.SalesOrder set OrderCostingMasterTemplateId=NULL Where OrderCostingMasterTemplateId='" + TemplateId + "' AND Id ='" + SOId + "'";

                _sqlRepository.ExecuteSqlCommand(sql);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Message = "SO Updated Successfully" });

        }
        [HttpPost, Authorize]
        public ActionResult GetListItem(string Id)

        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory
							--,CostingType=case  when pm.CostingType = 'CostingType1' then 'Garment' 
							--	else   'Fabric' end
                             ,pm.CostingType
							from OrderCostingMasterTemplate qcm 
							
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
                            WHERE QCM.ID='" + Id + @"'";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);
            Dictionary<string, string> CostingType = new Dictionary<string, string>();
            foreach (var item in Enum.GetValues(typeof(CostingType)))
            {
                CostingType.Add(item.ToString(), AccessInfo.GetEnumDescription((CostingType)(int)item));
            }

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    data[i]["CostingType"] = CostingType[data[i]["CostingType"].ToString()];
                }
                catch (Exception)
                {


                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult CopyCostingTemplate(Dictionary<string, object> CopyData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string NewId = "";
            try
            {
                DataSet OrderCostingMasterTemplate
                , OrderCostingDetailTemplate
                , OrderPreCostingDirectMaterial
                , OrderPreCostingOperation
                , OrderPreCostingDirectProcess
                , OrderPreCostingValueLoss
                , OrderPreCostingSalesExpense
                , OrderPreCostingProfit;

                string SourceId = CopyData["CostingMasterTemplateId"].ToString();

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + CopyData["Code"].ToString() + "'", out OrderCostingMasterTemplate, false, "1");
                if (OrderCostingMasterTemplate.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + CopyData["UserName"].ToString() + "'", out OrderCostingMasterTemplate, false, "1");
                if (OrderCostingMasterTemplate.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists");

                con.OpenDataSetThroughAdapter("select * from OrderCostingDetailTemplate where 1=2", out OrderCostingDetailTemplate, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectMaterial where 1=2", out OrderPreCostingDirectMaterial, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingOperation where 1=2", out OrderPreCostingOperation, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectProcess where 1=2", out OrderPreCostingDirectProcess, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingValueLoss where 1=2", out OrderPreCostingValueLoss, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingSalesExpense where 1=2", out OrderPreCostingSalesExpense, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingProfit where 1=2", out OrderPreCostingProfit, false, "1");

                DataTable CostingMasterTemplate = _sqlRepository.GetDataTable("select * from [dbo].[CostingMasterTemplate] WHERE Id='" + SourceId + "'");
                DataTable CostingDetailTemplate = _sqlRepository.GetDataTable("select * from [dbo].[CostingDetailTemplate] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectMaterial = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingDirectMaterial] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingOperation = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingOperation] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectProcess = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingDirectProcess] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingValueLoss = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingValueLoss] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingSalesExpense = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingSalesExpense] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingProfit = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingProfit] WHERE CostingMasterTemplateId='" + SourceId + "'");

                NewId = "X" + GetPK(TableName);
                CopyDataTable(CostingMasterTemplate, OrderCostingMasterTemplate.Tables[0], "");
                CopyDataTable(CostingDetailTemplate, OrderCostingDetailTemplate.Tables[0], NewId);
                CopyDataTable(PreCostingDirectMaterial, OrderPreCostingDirectMaterial.Tables[0], NewId);
                CopyDataTable(PreCostingOperation, OrderPreCostingOperation.Tables[0], NewId);
                CopyDataTable(PreCostingDirectProcess, OrderPreCostingDirectProcess.Tables[0], NewId);
                CopyDataTable(PreCostingValueLoss, OrderPreCostingValueLoss.Tables[0], NewId);
                CopyDataTable(PreCostingSalesExpense, OrderPreCostingSalesExpense.Tables[0], NewId);
                CopyDataTable(PreCostingProfit, OrderPreCostingProfit.Tables[0], NewId);

                OrderCostingMasterTemplate.Tables[0].Rows[0]["CostingMasterTemplateId"] = SourceId;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["Id"] = NewId;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["Code"] = CopyData["Code"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["UserName"] = CopyData["UserName"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ShortName"] = CopyData["ShortName"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["StandardName"] = CopyData["StandardName"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PlantId"] = identity.PlantId;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["Version"] = "1";
                OrderCostingMasterTemplate.Tables[0].Rows[0]["isQuickCostingApproved"] = false;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["isPreCostingApproved"] = false;


                SetForeignKey(OrderCostingDetailTemplate, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectMaterial, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingOperation, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectProcess, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingValueLoss, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingSalesExpense, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingProfit, "OrderCostingMasterTemplateId", NewId);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(OrderCostingMasterTemplate, OrderCostingDetailTemplate, OrderPreCostingDirectMaterial, OrderPreCostingOperation, OrderPreCostingDirectProcess, OrderPreCostingValueLoss, OrderPreCostingSalesExpense, OrderPreCostingProfit);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Id = NewId, Message = "Template copied successfully" });
        }
        private void SetForeignKey(DataSet ds, string ColumnName, string KeyValue)
        {
            foreach (DataRow drSource in ds.Tables[0].Rows)
            {
                drSource[ColumnName] = KeyValue;

            }
        }
        private void CopyDataTable(DataTable dtSource, DataTable dtDestination, string PK)
        {
            int Index = 0;
            foreach (DataRow drSource in dtSource.Rows)
            {
                Index++;
                DataRow drDestination = dtDestination.NewRow();
                CopyRow(drSource, ref drDestination);
                if (PK != "")
                    drDestination["Id"] = PK + Index;
                dtDestination.Rows.Add(drDestination);
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
        [HttpGet, Authorize]
        public ActionResult GetVersionByVersionId(string versionId)
        {
            string sql = @"select * from CostingVersionMasterTemplate where Id = '" + versionId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetVersion(string OrderCostingMasterTemplateId)
        {
            string sql = @"select  qcm.*  from OrderCostingMasterTemplate qcm 
                                where qcm.Id = '" + OrderCostingMasterTemplateId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQuickCostingByMasterId(string VersionId)
        {
            try
            {

                string sql = @"select  A.* from (
                        select um.UserName UOM, qcd.*,ISNULL(ctc.CostingType,'0') as CostingType, csc.UserName,  csc.Code, csc.StandardName,csc.ShortName,  0 as Status from dbo.OrderCostingDetailTemplate qcd 
                        left join [HKP].[CostingComponent] csc ON csc.Id = qcd.CostingComponentId
                        left join CostingTypeComponent as ctc on ctc.CostingComponentId = csc.Id

						left join hkp.CostingItem ci on ci.CostingComponentId = csc.Id
						left join scs.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                        where ISNULL(OrderCostingMasterTemplateId,'null')='" + VersionId + @"'
                        union 
                        select um.UserName UOM, qcvd.*,ISNULL(ctc.CostingType,'0') as CostingType, csc.UserName,  csc.Code, csc.StandardName,csc.ShortName, 1 as Status  from dbo.CostingVersionDetailTemplate qcvd 
						left join [HKP].[CostingComponent] csc ON csc.Id = qcvd.CostingComponentId
						left join CostingTypeComponent as ctc on ctc.CostingComponentId = csc.Id

						left join hkp.CostingItem ci on ci.CostingComponentId = csc.Id
						left join scs.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                         where ISNULL(OrderCostingMasterTemplateId,'null')='" + VersionId + "') as A order by A.Sequence";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public DataTable GetQuickCostingByVersionMasterId(string VersionId)
        {
            try
            {
                string sql = @"select A.* from (select qcd.*, csc.UserName as CostingSubCategory, 0 as Status from dbo.OrderCostingDetailTemplate qcd left join HKP.CostingSubCategory csc ON csc.Id = qcd.CostingSubCategoryId
                        where CostingVersionMasterTemplateId='" + VersionId + @"'
                        union 
                        select qcvd.*, csc.UserName as CostingSubCategory, 1 as Status  from dbo.CostingVersionDetailTemplate qcvd left join hkp.CostingSubCategory csc ON csc.Id = qcvd.CostingSubCategoryId
                         where CostingVersionMasterTemplateId='" + VersionId + "') as A order by Sequence ";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQuickCostinDetailByuickCostingVersionMasterId(string VersionId)
        {
            string sql = @"select * from OrderCostingDetailTemplate where CostingVersionMasterTemplateId = '" + VersionId + "'";
            try
            {
                var _data = _sqlRepository.GetDataCollection(sql);


                return Json(new { data = _data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        private DataSet GetCostingDetail(string OrderCostingMasterTemplateId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select qcvm.* from CostingVersionMasterTemplate qcvm
                            left join OrderCostingMasterTemplate qcm ON qcm.Id = qcvm.OrderCostingMasterTemplateId 
                            where OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet GetCostingVersionData(string versionId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select * from CostingVersionMasterTemplate where Id = '" + versionId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private string GetPK(string TableName)
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), TableName, out sID);
            return sID;
        }

        private void SaveData(string versionId, out string NewId, string versionDescription)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                NewId = string.Empty;
                DataSet dsDetail = null;
                string sql = "SELECT * FROM [dbo].[CostingVersionMasterTemplate] WHERE Id=''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                DataSet dsversion = GetCostingVersionData(versionId);

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    //bplib.clsGenID genid = new bplib.clsGenID();
                    //genid.GenID("dbo.QuickCostingVersionMaster", out _Id);
                    _Id = GetPK("CostingVersionMasterTemplate");
                    dr["Id"] = "VQCM" + _Id;

                    dr["OrderCostingMasterTemplateId"] = dsversion.Tables[0].Rows[0]["OrderCostingMasterTemplateId"].ToString();

                    dr["Version"] = Convert.ToDouble(dsversion.Tables[0].Rows[0]["Version"]) + 1;
                    dr["Description"] = versionDescription;

                    dr["AddedBy"] = dsversion.Tables[0].Rows[0]["AddedBy"];
                    dr["AddedDate"] = dsversion.Tables[0].Rows[0]["AddedDate"];
                    dr["AddedFromIP"] = dsversion.Tables[0].Rows[0]["AddedFromIP"];

                    //dr["UpdatedBy"] = dsversion.Tables[0].Rows[0]["UpdatedBy"];
                    //dr["UpdatedDate"] = dsversion.Tables[0].Rows[0]["UpdatedDate"];
                    //dr["UpdatedFromIP"] = dsversion.Tables[0].Rows[0]["UpdatedFromIP"];

                    dsMaster.Tables[0].Rows.Add(dr);
                }


                NewId = dsMaster.Tables[0].Rows[0]["Id"].ToString();


                SaveDetailData(versionId, NewId, out dsDetail);

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveDetailData(string versionId, string NewId, out DataSet dsDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            GetCostingDeatilByVersionId(versionId, out DataSet detailData);
            dsDetail = detailData;
            if (dsDetail.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsDetail.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = dsDetail.Tables[0].Rows[i];

                    dr.BeginEdit();

                    dr["CostingVersionMasterTemplateId"] = NewId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
            }
        }


        #region CostingVersionDetailTemplate

        public void GetCostingDeatilByVersionId(string versionId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM OrderCostingDetailTemplate where CostingVersionMasterTemplateId = '" + versionId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function


        [HttpPost, Authorize]
        public ActionResult CreateCostingVersionDetail(string versionId, List<OrderCostingDetailTemplate> data, string versionDescription)
        {
            string NewId = string.Empty;
            GetCostingDeatilByVersionId(versionId, out DataSet dsCostingDetail);

            SaveCostingVersionCopyDetail(versionId, dsCostingDetail);
            SaveData(versionId, out NewId, versionDescription);
            string newVersionId = NewId;
            //SaveCostingDetail(newVersionId, data);
            return Json(new { Message = AplosMessage.Insert });
        }

        private void SaveCostingVersionCopyDetail(string versionId, DataSet dsCostingDetail)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "select * from CostingVersionDetailTemplate where CostingVersionMasterTemplateId = '" + versionId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsCostingDetail.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsCostingDetail.Tables[0].Rows.Count; i++)
                    {
                        string _Id = "";
                        //bplib.clsGenID genid = new bplib.clsGenID();
                        //genid.GenID("dbo.CostingVersionDetailTemplate", out _Id);
                        _Id = GetPK("CostingVersionDetailTemplate");

                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = "VD" + _Id;
                        //dr["CostingTypeComponentId"] = dsCostingDetail.Tables[0].Rows[i]["CostingTypeComponentId"];
                        dr["CostingComponentId"] = dsCostingDetail.Tables[0].Rows[i]["CostingComponentId"];

                        dr["CostingVersionMasterTemplateId"] = dsCostingDetail.Tables[0].Rows[i]["CostingVersionMasterTemplateId"];
                        dr["Sequence"] = dsCostingDetail.Tables[0].Rows[i]["Sequence"];
                        dr["CostingValue"] = dsCostingDetail.Tables[0].Rows[i]["CostingValue"];
                        dr["BuyerTarget"] = dsCostingDetail.Tables[0].Rows[i]["BuyerTarget"];

                        dr["AddedBy"] = dsCostingDetail.Tables[0].Rows[i]["AddedBy"];
                        dr["AddedDate"] = dsCostingDetail.Tables[0].Rows[i]["AddedDate"];
                        dr["AddedFromIP"] = dsCostingDetail.Tables[0].Rows[i]["AddedFromIP"];

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion CostingVersionDetailTemplate

        #region OrderCostingDetailTemplate
        public void GetQuickCosting(string CostingVersionMasterTemplateId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM [dbo].[OrderCostingDetailTemplate] WHERE OrderCostingMasterTemplateId= '" + CostingVersionMasterTemplateId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        //End of function
        private void SaveCostingDetail(string masterid, List<OrderCostingDetailTemplate> data, out DataSet dsdetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GetQuickCosting(masterid, out dsdetail);
            try
            {
                if (data == null)
                    return;

                for (int i = 0; i < dsdetail.Tables[0].Rows.Count; i++)
                {
                    string ownid = dsdetail.Tables[0].Rows[i]["Id"].ToString();
                    List<OrderCostingDetailTemplate> FilterData = data.Where(a => a.Id == ownid).ToList();
                    if (FilterData.Count == 0)
                        dsdetail.Tables[0].Rows[i].Delete();
                }


                if (data != null)
                {

                    DataView dv = null;


                    dv = new DataView(dsdetail.Tables[0]);

                    string _Id = string.Empty;

                    _Id = GetPK("OrderCostingDetailTemplate");

                    int count = 0;
                    foreach (var item in data)
                    {
                        dv.RowFilter = "Id='" + item.Id + "'";
                        if (dv.Count == 0)
                        {
                            count++;


                            DataRow dr = dsdetail.Tables[0].NewRow();
                            dr["Id"] = "OD" + _Id + "_" + count;

                            //dr["CostingTypeComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingTypeComponentId));
                            dr["CostingComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingComponentId));
                            dr["OrderCostingMasterTemplateId"] = masterid;
                            dr["Sequence"] = clsStaticInfo.dbl(clsStaticInfo.nullrecorder(item.Sequence));
                            dr["CostingValue"] = item.CostingValue;
                            dr["BuyerTarget"] = item.BuyerTarget;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsdetail.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dv[0].Row;

                            dr.BeginEdit();
                            //dr["CostingTypeComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingTypeComponentId));
                            dr["CostingComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingComponentId));
                            dr["OrderCostingMasterTemplateId"] = masterid;
                            dr["Sequence"] = clsStaticInfo.dbl(clsStaticInfo.nullrecorder(item.Sequence));
                            dr["CostingValue"] = Convert.ToDouble(item.CostingValue);
                            dr["BuyerTarget"] = item.BuyerTarget;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr["UpdatedDate"] = DateTime.Now;

                            dr.EndEdit();
                        }
                    }
                    //clsStaticInfo obj = new clsStaticInfo();
                    // obj.SaveDataSets(dsdetail);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public ActionResult CreateCostingBuyer(OrderCostingBuyer data)
        {
            DataSet dsCostingBuyer;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            //con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[OrderCostingBuyer] where Id= '" + data.Id.ToString()+ "'", out dsCostingBuyer, false, "1");
            con.OpenDataSetThroughAdapter("select * from [dbo].[OrderCostingBuyer] where Id='" + data.Id + "'", out dsCostingBuyer, false, "1");

            string _Id = "";

            #region data update
            if (dsCostingBuyer.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("dbo.OrderCostingDetail", out _Id);

                data.Id = "CB_" + _Id;
                AddNewCostingBuyerRow(dsCostingBuyer.Tables[0], data);
            }
            else
            {
                _Id = data.Id.ToString();
                EditNewCostingBuyerRow(dsCostingBuyer.Tables[0].Rows[0], data);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsCostingBuyer);


            return Json(new { data = Helpers.CustomJsonResult.DataTableToJson(dsCostingBuyer.Tables[0]), Message = AplosMessage.Insert });
        }
        #endregion OrderCostingDetailTemplate


        [HttpGet, Authorize]
        public ActionResult GetCostingSubCategory()
        {
            string sql = @"select 0 as flag,c.* from hkp.CostingComponent c where c.Active = 1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetCostingComponents()
        //{
        //    string sql = @"select ci.CostingComponentId,ci.Code,ci.StandardName from hkp.CostingItem ci
        //            left JOIN  hkp.CostingComponent cc ON ci.CostingComponentId=cc.Id";
        //    return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        //}





        [HttpPost, Authorize]
        public JsonResult Create(FormCollection form)
        {
            var pre = form["modelNew"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var cost = JsonConvert.DeserializeObject<OrderCostingMasterTemplate>(pre, settings);

            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + cost.Code + "' AND  Id<>'" + cost.Id + "'", out dsMaster, false, "1");
            if (dsMaster.Tables[0].Rows.Count > 0)
                throw new Exception("Code is already exists!!!");

            con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + cost.UserName + "'  AND  Id<>'" + cost.Id + "'", out dsMaster, false, "1");
            if (dsMaster.Tables[0].Rows.Count > 0)
                throw new Exception("UserName is already exists!!!");

            con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + cost.Id + "'", out dsMaster, false, "1");

            string _Id = "";

            #region data update
            if (dsMaster.Tables[0].Rows.Count == 0)
            {
                //bplib.clsGenID genid = new bplib.clsGenID();
                //genid.GenID(TableName, out _Id);
                _Id = GetPK(TableName);

                cost.Id = "O" + _Id;
                _Id = cost.Id;
                AddNewRow(dsMaster.Tables[0], cost);
            }
            else
            {
                _Id = cost.Id.ToString();
                EditRow(dsMaster.Tables[0].Rows[0], cost);
            }
            #endregion data update

            DataSet dsCostingDetail = null;

            var _quickCostingData = form["QuickCostingData"];
            var QuickCostingData = JsonConvert.DeserializeObject<List<OrderCostingDetailTemplate>>(_quickCostingData, settings);
            SaveCostingDetail(_Id, QuickCostingData, out dsCostingDetail);

            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster, dsCostingDetail);



            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                {
                    cost.FileName = extension;
                    if (!string.IsNullOrEmpty(cost.FileName))
                        cost.FileName = cost.Id.ToString() + cost.FileName;
                }
                else
                    throw new CustomException(Resources.ImageUploadError);
            }

            if (file != null)
            {
                var path = Path.Combine(ResourcesPathReader.GetCostingPicPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeImage, typeof(string)).ToString())*/, cost.FileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                {
                    file.SaveAs(path);
                }
            }


            return Json(new { Error = false, data = cost, Message = AplosMessage.Updated });
        }

        public JsonResult xCreate(FormCollection form, HttpPostedFileBase[] file)
        {

            OrderCostingMasterTemplate cost = new JavaScriptSerializer().Deserialize<OrderCostingMasterTemplate>(form["modelNew"]);

            // _SOPDocumentService.Insert(sopDocument);

            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + cost.Code + "' AND  Id<>'" + cost.Id + "'", out dsMaster, false, "1");
            if (dsMaster.Tables[0].Rows.Count > 0)
                throw new Exception("Code is already exists!!!");

            con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + cost.UserName + "'  AND  Id<>'" + cost.Id + "'", out dsMaster, false, "1");
            if (dsMaster.Tables[0].Rows.Count > 0)
                throw new Exception("UserName is already exists!!!");

            con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + cost.Id + "'", out dsMaster, false, "1");

            string _Id = "";

            #region data update
            if (dsMaster.Tables[0].Rows.Count == 0)
            {
                //bplib.clsGenID genid = new bplib.clsGenID();
                //genid.GenID(TableName, out _Id);
                _Id = GetPK(TableName);

                cost.Id = "QCM" + _Id;
                AddNewRow(dsMaster.Tables[0], cost);
            }
            else
            {
                _Id = cost.Id.ToString();
                EditRow(dsMaster.Tables[0].Rows[0], cost);
            }
            #endregion data update


            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster);

            return Json(new { Error = false, data = cost, Message = AplosMessage.Updated });
        }
        public Dictionary<string, object> GetDocumentFile(string OrderCostingMasterTemplateId)
        {
            try
            {
                var sql = @"Select Id, FileName From " + TableName + "  Where Id='" + OrderCostingMasterTemplateId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
                //throw new CustomException(ex.Message, ex,
                // Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public ActionResult DeleteCostingDetail(string costingDetailId)
        {
            try
            {
                if (string.IsNullOrEmpty(costingDetailId))
                    throw new Exception("costingDetailId Not Found");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderCostingDetailTemplate where id='" + costingDetailId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Delete(string id)
        {
            //string sql = @"select * from [HKP].[CostingGroupGL] where CostingGroupId = '"+ id + "'";
            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [OrderCostingDetailTemplate] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingDirectProcess] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingOperation] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingValueLoss] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingSalesExpense] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingDirectMaterial] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingProfit] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("REFERENCE"))
                    return Json(new { Error = true, Message = "This costing template has been used. Cannot delete" }, JsonRequestBehavior.AllowGet);

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }
        private void AddNewRow(DataTable dt, OrderCostingMasterTemplate sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            //foreach (var item in sourceData)
            //{
            //    try
            //    {
            //        dr[item] = sourceData[item];
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
            dr["Id"] = sourceData.Id;
            dr["Code"] = sourceData.Code;
            dr["Active"] = sourceData.Active;
            dr["CustomerId"] = sourceData.CustomerId;
            dr["Description"] = sourceData.Description;
            dr["FileName"] = sourceData.FileName;
            dr["MKTTargetPerHour"] = sourceData.MKTTargetPerHour;
            dr["OrderSize"] = sourceData.OrderSize;
            dr["PackingType"] = sourceData.PackingType;
            dr["PaymentDays"] = sourceData.PaymentDays;
            dr["ProductionAvailableDays"] = sourceData.ProductionAvailableDays;
            dr["ProductMasterId"] = sourceData.ProductMasterId;
            dr["Remarks"] = sourceData.Remarks;
            dr["Version"] = "1";
            dr["ShortName"] = sourceData.ShortName;
            dr["SpecifyTo"] = sourceData.SpecifyTo;
            dr["StandardName"] = sourceData.StandardName;
            dr["TargetSellingPrice"] = sourceData.TargetSellingPrice;
            dr["UserName"] = sourceData.UserName;
            dr["EstNoOfPackingList"] = sourceData.EstNoOfPackingList;
            dr["ExcessShipmentPer"] = sourceData.ExcessShipmentPer;
            dr["CurrencyId"] = sourceData.CurrencyId;

            dr["SPT"] = sourceData.SPT;
            dr["NoOfWorkstation"] = sourceData.NoOfWorkstation;
            dr["EfficiencyPercentage"] = sourceData.EfficiencyPercentage;
            dr["StandardWorkingHours"] = sourceData.StandardWorkingHours;
            dr["WorkCenterTargetPerDay"] = sourceData.WorkCenterTargetPerDay;
            dr["StandardWorkingHourCost"] = sourceData.StandardWorkingHourCost;
            dr["AdditionalWorkingHourCostPerHour"] = sourceData.AdditionalWorkingHourCostPerHour;

            dr["UOM"] = sourceData.UOM;
            dr["TargetOrSPT"] = sourceData.TargetOrSPT;
            dr["CriticalLevel"] = sourceData.CriticalLevel;

            dr["isQuickCostingApproved"] = false;
            dr["isPreCostingApproved"] = false;

            dr["TargetCM"] = sourceData.TargetCM;
            dr["TargetProfit"] = sourceData.TargetProfit;
            dr["IsPercentage"] = sourceData.IsPercentage;

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dr["PlantId"] = identity.PlantId;

            dt.Rows.Add(dr);
        }


        private void EditRow(DataRow dr, OrderCostingMasterTemplate sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            //foreach (var item in sourceData)
            //{
            //    try
            //    {
            //        dr[item] = sourceData[item];
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
            dr["Id"] = sourceData.Id;
            dr["Code"] = sourceData.Code;
            dr["Active"] = sourceData.Active;
            dr["CustomerId"] = sourceData.CustomerId;
            dr["Description"] = sourceData.Description;
            dr["FileName"] = sourceData.FileName;
            dr["MKTTargetPerHour"] = sourceData.MKTTargetPerHour;
            dr["OrderSize"] = sourceData.OrderSize;
            dr["PackingType"] = sourceData.PackingType;
            dr["PaymentDays"] = sourceData.PaymentDays;
            dr["ProductionAvailableDays"] = sourceData.ProductionAvailableDays;
            dr["ProductMasterId"] = sourceData.ProductMasterId;
            dr["Remarks"] = sourceData.Remarks;
            //  dr["Sequence"] = sourceData.Sequence;
            dr["ShortName"] = sourceData.ShortName;
            dr["SpecifyTo"] = sourceData.SpecifyTo;
            dr["StandardName"] = sourceData.StandardName;
            dr["TargetSellingPrice"] = sourceData.TargetSellingPrice;
            dr["UserName"] = sourceData.UserName;
            dr["EstNoOfPackingList"] = sourceData.EstNoOfPackingList;
            dr["ExcessShipmentPer"] = sourceData.ExcessShipmentPer;
            dr["CurrencyId"] = sourceData.CurrencyId;

            dr["SPT"] = sourceData.SPT;
            dr["NoOfWorkstation"] = sourceData.NoOfWorkstation;
            dr["EfficiencyPercentage"] = sourceData.EfficiencyPercentage;
            dr["StandardWorkingHours"] = sourceData.StandardWorkingHours;
            dr["WorkCenterTargetPerDay"] = sourceData.WorkCenterTargetPerDay;
            dr["StandardWorkingHourCost"] = sourceData.StandardWorkingHourCost;
            dr["AdditionalWorkingHourCostPerHour"] = sourceData.AdditionalWorkingHourCostPerHour;


            dr["UOM"] = sourceData.UOM;
            dr["TargetOrSPT"] = sourceData.TargetOrSPT;
            dr["CriticalLevel"] = sourceData.CriticalLevel;

            dr["TargetCM"] = sourceData.TargetCM;
            dr["TargetProfit"] = sourceData.TargetProfit;
            dr["IsPercentage"] = sourceData.IsPercentage;

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        public ActionResult GetProductByProductMasterId(string ProductMasterId)
        {
            string sql = @"
							select pm.* ,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory 
							from [MST].[ProductMaster] pm 
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							where pm.Id = '" + ProductMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ProductMasterDetail(string ProductMasterId)
        {
            string sql = @"select  pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory
							
                             ,pm.CostingType
							from  [MST].[ProductMaster] pm 
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							where pm.Id = '" + ProductMasterId + "'";

            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);
            Dictionary<string, string> CostingType = new Dictionary<string, string>();
            foreach (var item in Enum.GetValues(typeof(CostingType)))
            {
                CostingType.Add(item.ToString(), AccessInfo.GetEnumDescription((CostingType)(int)item));
            }

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    data[i]["CostingType"] = CostingType[data[i]["CostingType"].ToString()];
                }
                catch (Exception)
                {


                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);


            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingComponentByProductMasterId(string ProductMasterId)
        {
            string sql = @"SELECT 
                        cc.Id as CostingComponentId
                     ,cc.Code
                     ,cc.ShortName
                     ,cc.UserName
                        ,ctc.Sequence
                     ,cc.StandardName
                     ,ctc.CostingType
                     ,cc.CostingSegment

                    FROM [dbo].[CostingTypeComponent] AS ctc
                    inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                    WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + "') order by ctc.Sequence";

            //string itemSql = @"select * from hkp.CostingItemselect * from hkp.CostingItem";
            // return Json(new { data = _sqlRepository.GetDataCollection(sql, null), items = _sqlRepository.GetDataCollection(itemSql,null) }, JsonRequestBehavior.AllowGet);
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetQuickCostingDetailByProductMaster(string ProductMasterId, string CostingVersionMasterTemplateId)
        {
            string sql = "";

            sql = @" select isnull(d.id,'New') isNewId, case when isnull(d.Id,'')<>'' THEN isnull(TEMPLATE.CostingComponentId,'DELETE') ELSE '' END AS isToBeDeleted,
                         d.Id
                        ,0 as Status
	                    ,d.CostingValue
	                    ,d.BuyerTarget
	                    --,d.CostingVersionMasterTemplateId
                        ,cc.Id as CostingComponentId
	                    ,cc.Code
	                    ,cc.ShortName
	                    ,cc.UserName
                        ,ctc.Sequence
	                    ,cc.StandardName
	                    ,ctc.CostingType
                        ,cc.CostingSegment
                        ,isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount 
						 from hkp.CostingComponent CC
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')
                        left outer join OrderCostingDetailTemplate D on cc.id=d.CostingComponentId and d.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'
                         LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderPreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + CostingVersionMasterTemplateId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM OrderPreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                  )AS ITEMVAL ON  itemval.CostingComponentId=d.CostingComponentId
                        left outer join  (
                        select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')) AS TEMPLATE 
					    on template.CostingComponentId=d.CostingComponentId


                        where   cc.Id IN (
                            select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')

					    UNION

					    select CostingComponentId from OrderCostingDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')='" + CostingVersionMasterTemplateId + @"'

					--union

					--select CostingComponentId from CostingVersionDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')= '" + CostingVersionMasterTemplateId + @"'
                    )  order by isnull(ctc.Sequence,999999),cc.Description";

            string sqlAllItem = @" SELECT  ci.Id, ci.CostingCategoryId, ci.CostingComponentId,cc.CostingSegment,upper(isnull(itemval.ValueType,'FIXED')) AS ValueType,
                        isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount,isnull(itemval.Value,0) AS Value,isnull(itemval.Rate,0) AS Rate
						 from hkp.CostingComponent CC
						 INNER JOIN hkp.CostingItem AS ci ON ci.CostingComponentId=cc.Id
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')
                        left outer join OrderCostingDetailTemplate D on cc.id=d.CostingComponentId and d.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'
                         LEFT OUTER JOIN (        SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.GrossAmount AS TotalGrossAmount FROM OrderPreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + CostingVersionMasterTemplateId + @"' 
                                            UNION ALL SELECT 'PERCENTAGE' AS ValueType, PC.Value,PC.Rate, i.Id,pc.Amount AS TotalGrossAmount FROM OrderPreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.[Value]  AS TotalGrossAmount FROM OrderPreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,          pc.Amount AS TotalGrossAmount FROM OrderPreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderPreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderPreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                  )AS ITEMVAL ON  itemval.Id=ci.Id
                        left outer join  (
                        select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')) AS TEMPLATE 
					    on template.CostingComponentId=d.CostingComponentId
                        where   cc.Id IN (
                            select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')

					    UNION

					    select CostingComponentId from OrderCostingDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')='" + CostingVersionMasterTemplateId + @"'

                    )  order by isnull(ctc.Sequence,999999),cc.Description";

            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            return Json(new { ComponentList = _sqlRepository.GetDataCollection(sql, null), ItemList = _sqlRepository.GetDataCollection(sqlAllItem, null) }, JsonRequestBehavior.AllowGet);
        }

        private string GetChargesPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(CostingItem), out sID);
            return sID;
        }

        private void AddNewOrderCostingDetailRow(DataTable dt, Dictionary<string, object> sourceData)
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
        private void EditNewOrderCostingDetailRow(DataRow dr, Dictionary<string, object> sourceData)
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


        private void AddNewCostingBuyerRow(DataTable dt, OrderCostingBuyer sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();


            dr["Id"] = sourceData.Id;
            dr["OrderCostingMasterTemplateId"] = sourceData.OrderCostingMasterTemplateId;
            dr["BuyerId"] = sourceData.BuyerId;
            dr["BuyerStyleRefNo"] = sourceData.BuyerStyleRefNo;
            dr["OwnStyleRefNo"] = sourceData.OwnStyleRefNo;

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditNewCostingBuyerRow(DataRow dr, OrderCostingBuyer sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            dr["Id"] = sourceData.Id;
            dr["OrderCostingMasterTemplateId"] = sourceData.OrderCostingMasterTemplateId;
            dr["BuyerId"] = sourceData.BuyerId;
            dr["BuyerStyleRefNo"] = sourceData.BuyerStyleRefNo;
            dr["OwnStyleRefNo"] = sourceData.OwnStyleRefNo;

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        private void SaveCostingItems(IEnumerable<CostingItem> data, out DataSet dsMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [HKP].[CostingItem] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = GetChargesPK();

                            dr["Sequence"] = item.Sequence;
                            dr["Code"] = item.Code;
                            dr["ShortName"] = item.ShortName;
                            dr["StandardName"] = item.StandardName;
                            dr["UserName"] = item.UserName;
                            dr["Description"] = item.Description;
                            dr["Remarks"] = item.Remarks;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["CostingCategoryId"] = item.CostingCategoryId;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["UnitOfMeasurementId"] = item.UnitOfMeasurementId;
                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["Wastage"] = item.Wastage;
                            dr["ProcessId"] = item.ProcessId;
                            dr["BudgetMasterId"] = item.BudgetMasterId;
                            dr["ActivityId"] = item.ActivityId;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;
                            //dr["CostingGroupId"] = item.CostingGroupId;
                            //dr["CostingItemType"] = item.CostingItemType;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["Id"] = item.Id;
                            dr["Sequence"] = item.Sequence;
                            dr["Code"] = item.Code;
                            dr["ShortName"] = item.ShortName;
                            dr["StandardName"] = item.StandardName;
                            dr["UserName"] = item.UserName;
                            dr["Description"] = item.Description;
                            dr["Remarks"] = item.Remarks;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["CostingCategoryId"] = item.CostingCategoryId;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["UnitOfMeasurementId"] = item.UnitOfMeasurementId;
                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["Wastage"] = item.Wastage;
                            dr["ProcessId"] = item.ProcessId;
                            dr["BudgetMasterId"] = item.BudgetMasterId;
                            dr["ActivityId"] = item.ActivityId;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;
                            //dr["CostingGroupId"] = item.CostingGroupId;
                            //dr["CostingItemType"] = item.CostingItemType;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

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
        private void SaveOrderCostingDetail(Dictionary<string, object> OrderCostingDetail, out DataSet dsCostingDetail)
        {

            dsCostingDetail = null;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[OrderCostingDetail] where Id= '" + OrderCostingDetail["Id"] + "'", out dsCostingDetail, false, "1");

            string _Id = "";

            #region data update
            if (dsCostingDetail.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("dbo.OrderCostingDetail", out _Id);

                OrderCostingDetail["Id"] = "PCD" + _Id;
                AddNewOrderCostingDetailRow(dsCostingDetail.Tables[0], OrderCostingDetail);
            }
            else
            {
                _Id = OrderCostingDetail["Id"].ToString();
                EditNewOrderCostingDetailRow(dsCostingDetail.Tables[0].Rows[0], OrderCostingDetail);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsCostingDetail);
        }
        [HttpPost, Authorize]
        public ActionResult SaveCostingItemsIncludingComponent(IEnumerable<CostingItem> costingItems, Dictionary<string, object> OrderCostingDetail)
        {

            SaveCostingItems(costingItems, out DataSet dsMaster);
            SaveOrderCostingDetail(OrderCostingDetail, out DataSet dsCostingDetail);

            return Json(new { costingItems = dsMaster, OrderCostingDetail = dsCostingDetail, Message = AplosMessage.Updated });
        }

        public ActionResult GetBuyerDataByCostingMasterId(string costingMasterId)
        {
            string sql = @"select cb.*, b.UserName as Buyer from [dbo].[OrderCostingBuyer] cb
                            left join hkp.Buyer b on b.Id = cb.BuyerId where OrderCostingMasterTemplateId = '" + costingMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteCostingBuyer(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[OrderCostingBuyer] where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemByComponentId(string costingComponentId)
        {
            //string sql = @"select ci.CostingComponentId,ci.Code,ci.StandardName from hkp.CostingItem ci
            //        left JOIN  hkp.CostingComponent cc ON ci.CostingComponentId=cc.Id";

            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,um.UserName as UnitOfMeasurement,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, pcdm.Consumption,
                            pcdm.UOM, pcdm.Rate,pcdm.Description as dmDescription, pcdm.ValueLoss,pcdm.GrossConsumption,pcdm.GrossAmount, pcdm.Id  ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            left join OrderPreCostingDirectMaterial pcdm on pcdm.CostingItemId = ci.Id 
                            left join SCS.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                           where CostingComponentId = '" + costingComponentId + "' ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveOrderPreCostingDirectMaterial(IEnumerable<OrderPreCostingDirectMaterial> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingDirectMaterial] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        if (item.Consumption > 0 && item.Rate > 0)
                        {

                            if (dsMaster.Tables[0].DefaultView.Count == 0)
                            {
                                _id = "DM" + GetPK("OrderPreCostingDirectMaterial");
                                DataRow dr = dsMaster.Tables[0].NewRow();
                                dr["Id"] = _id;
                                dr["CostingItemId"] = item.CostingItemId;
                                dr["Consumption"] = item.Consumption;
                                dr["UOM"] = item.UOM;
                                dr["Rate"] = item.Rate;
                                dr["ValueLoss"] = item.ValueLoss;
                                dr["GrossConsumption"] = item.GrossConsumption;
                                dr["GrossAmount"] = item.GrossAmount;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                                dr["POIssueDeadLine"] = item.POIssueDeadLine;
                                dr["Particulars"] = item.Particulars;
                                dr["Remarks"] = item.Remarks;
                                dr["PurchaseGroupId"] = item.PurchaseGroupId;


                                dr["SourcingType"] = item.SourcingType;
                                dr["Usage"] = item.Usage;
                                dr["IsUDApplicable"] = item.IsUDApplicable;
                                dr["IsGeneric"] = item.IsGeneric;
                                dr["IsMandatory"] = item.IsMandatory;
                                dr["MaterialMasterId"] = item.MaterialMasterId;
                                dr["ArticleId"] = item.ArticleId;

                                dr["VendorId"] = item.VendorId;


                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                dsMaster.Tables[0].Rows.Add(dr);
                            }
                            else
                            {
                                //edit
                                DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();


                                dr["CostingItemId"] = item.CostingItemId;
                                dr["Consumption"] = item.Consumption;
                                dr["UOM"] = item.UOM;
                                dr["Rate"] = item.Rate;
                                dr["ValueLoss"] = item.ValueLoss;
                                dr["GrossConsumption"] = item.GrossConsumption;

                                dr["GrossAmount"] = item.GrossAmount;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                                dr["POIssueDeadLine"] = item.POIssueDeadLine;
                                dr["Particulars"] = item.Particulars;
                                dr["Remarks"] = item.Remarks;
                                dr["PurchaseGroupId"] = item.PurchaseGroupId;


                                dr["SourcingType"] = item.SourcingType;
                                dr["Usage"] = item.Usage;
                                dr["IsUDApplicable"] = item.IsUDApplicable;
                                dr["IsGeneric"] = item.IsGeneric;
                                dr["IsMandatory"] = item.IsMandatory;
                                dr["MaterialMasterId"] = item.MaterialMasterId;
                                dr["ArticleId"] = item.ArticleId;
                                dr["VendorId"] = item.VendorId;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;

                                dr.EndEdit();
                            }

                        }
                        else
                        {

                            while (dsMaster.Tables[0].DefaultView.Count > 0)
                                dsMaster.Tables[0].DefaultView[0].Delete();

                        }


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public ActionResult GetDirectCostingMeterialWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,m.Consumption,m.VendorId,p.UserName AS Vendor,CI.Id AS CostingItemId,m.OrderCostingMasterTemplateId,m.GrossAmount,m.GrossConsumption,m.Id,m.Rate,m.ResponsiblePersonId,m.UOM
                        ,isnull(m.ValueLoss,ci.Wastage) AS ValueLoss,M.Remarks
                        ,m.SourcingType, ISNULL(m.IsUDApplicable,0) AS IsUDApplicable, m.Usage, ISNULL(m.IsGeneric,0) AS IsGeneric,ISNULL(m.IsMandatory,0) AS  IsMandatory
						,m.MaterialMasterId, m.ArticleId, mm.UserName as MaterialMasterName, mma.StandardName as ArticleName
                        ,e.EmployeeName as ResponsiblePerson, e.SystemId as ResponsiblePersonId,um.UserName as UnitOfMeasurement, um.Id as UoMId, ci.UserName
                        ,ISNULL(m.MinimumOfQuantity,ci.MinimumOfQuantity) AS MinimumOfQuantity,ISNULL(m.POIssueDeadLine,ci.POIssueDeadLine)POIssueDeadLine
                        ,ISNULL(m.PurchaseGroupId,ci.PurchaseGroupId) AS PurchaseGroupId,ISNULL(m.Particulars,ci.UserName) AS Particulars
                     
						 from hkp.CostingItem ci
                        JOIN [dbo].[OrderPreCostingDirectMaterial] m on m.CostingItemId = ci.Id  and m.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"'
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                        left join [SCS].[UnitOfMeasurement] um on um.Id = ci.UnitOfMeasurementId
						left join dbo.EmployeeInformation e on e.SystemId = m.ResponsiblePersonId
						left join mst.MaterialMaster mm on mm.Id = m.MaterialMasterId 
						left join [MST].[MaterialMasterArticle] mma on mma.Id = m.ArticleId 
						LEFT JOIN hkp.Party AS p ON p.Id=m.VendorId
						WHERE ci.CostingComponentId='" + costingComponentId + @"'  Order By CI.Sequence";


            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilter(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @" select m.*,e.EmployeeName as ResponsiblePerson,um.UserName as UnitOfMeasurement, um.Id as UoMId, ci.UserName, m.Description
                        ,m.MaterialMasterId, m.ArticleId, mm.UserName as MaterialMasterName, mma.StandardName as ArticleName from hkp.CostingItem ci
                        inner join [dbo].[OrderPreCostingDirectMaterial] m on m.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                        left join [SCS].[UnitOfMeasurement] um on um.Id = ci.UnitOfMeasurementId
						left join EmployeeInformation e on e.SystemId = m.ResponsiblePersonId
						left join mst.MaterialMaster mm on mm.Id = m.MaterialMasterId 
						left join [MST].[MaterialMasterArticle] mma on mma.Id = m.ArticleId 
                        where m.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteDirectMaterial(string DirectMaterialId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingDirectMaterial where id='" + DirectMaterialId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("REFERENCE"))
                    return Json(new { Error = true, Message = "Selected Issue Group has been used in Issue therefor cannot delete." }, JsonRequestBehavior.AllowGet);

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        #region Pre Costing Operation
        [HttpGet, Authorize]
        public ActionResult GetOperationWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,o.Id,CI.Id AS CostingItemId, o.[Value], o.[Description],e.EmployeeName as ResponsiblePerson, e.SystemId as ResponsiblePersonId,  ci.UserName, o.Description
                            from hkp.CostingItem ci
						 join [dbo].[OrderPreCostingOperation] o on o.CostingItemId = ci.Id  and o.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = o.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By CI.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveOperation(IEnumerable<OrderPreCostingOperation> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingOperation] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        if (item.Value > 0)
                        {

                            if (dsMaster.Tables[0].DefaultView.Count == 0)
                            {
                                _id = "CO" + GetPK("OrderPreCostingOperation");
                                DataRow dr = dsMaster.Tables[0].NewRow();

                                dr["Id"] = _id;
                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["Value"] = item.Value;
                                dr["Description"] = item.Description;


                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                dsMaster.Tables[0].Rows.Add(dr);
                            }
                            else
                            {
                                //edit
                                DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                                dr["Value"] = item.Value;
                                dr["Description"] = item.Description;


                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;

                                dr.EndEdit();
                            }

                        }
                        else
                        {

                            while (dsMaster.Tables[0].DefaultView.Count > 0)
                                dsMaster.Tables[0].DefaultView[0].Delete();

                        }


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }

                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public ActionResult GetCostingItemWithOperationByComponentId(string costingComponentId)
        {
            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, o.OrderCostingMasterTemplateId,
                                 ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                                 o.Description as dmDescription,o.Value ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                                 from hkp.CostingItem ci 
                                 left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
            left join [dbo].[OrderPreCostingOperation] o on o.CostingItemId = ci.Id 
                                where CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DeleteOperation(string operationId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingOperation where id='" + operationId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForOperation(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select o.*,e.EmployeeName as ResponsiblePerson, ci.UserName, o.Description from hkp.CostingItem ci
                        inner join  [dbo].[OrderPreCostingOperation] o on o.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = o.ResponsiblePersonId 
                            where o.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion Operation

        #region OrderCosting Direct Process
        [HttpGet, Authorize]
        public ActionResult GetDirectProcessWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,ci.UserName,p.Id,CI.Id AS CostingItemId, p.OrderCostingMasterTemplateId, p.ExecutionType,
       p.[Value], p.Rate, p.Amount, p.[Description],e.SystemId as ResponsiblePersonId, e.EmployeeName as ResponsiblePerson
                        from hkp.CostingItem ci
                        join [dbo].[OrderPreCostingDirectProcess] p on CostingItemId = ci.Id and p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By CI.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveDirectProcess(IEnumerable<OrderPreCostingDirectProcess> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingDirectProcess] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        if (item.Amount > 0)
                        {

                            if (dsMaster.Tables[0].DefaultView.Count == 0)
                            {
                                _id = "DP" + GetPK("OrderPreCostingDirectProcess");
                                DataRow dr = dsMaster.Tables[0].NewRow();

                                dr["Id"] = _id;
                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["ExecutionType"] = item.ExecutionType;
                                dr["Value"] = item.Value;
                                dr["Rate"] = item.Rate;
                                dr["Amount"] = item.Amount;
                                dr["Description"] = item.Description;

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                dsMaster.Tables[0].Rows.Add(dr);
                            }
                            else
                            {
                                //edit
                                DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["ExecutionType"] = item.ExecutionType;
                                dr["Value"] = item.Value;
                                dr["Rate"] = item.Rate;
                                dr["Amount"] = item.Amount;
                                dr["Description"] = item.Description;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;


                                dr.EndEdit();
                            }

                        }
                        else
                        {

                            while (dsMaster.Tables[0].DefaultView.Count > 0)
                                dsMaster.Tables[0].DefaultView[0].Delete();

                        }


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemWithDirectProcessByComponentId(string costingComponentId)
        {
            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,um.UserName as UnitOfMeasurement,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                             p.Id , p.ExecutionType, p.Value, p.Rate, p.Amount,p.Description
							 ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            left join [dbo].[OrderPreCostingDirectProcess] p on p.CostingItemId = ci.Id 
                            left join SCS.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                            where CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteDirectProcess(string directProcessId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingDirectProcess where id='" + directProcessId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForDirectProcess(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select p.*,e.EmployeeName as ResponsiblePerson, ci.UserName
						from hkp.CostingItem ci
                        inner join [dbo].[OrderPreCostingDirectProcess]  p on p.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId 
                        where p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion Pre Costing Direct Process

        #region OrderCosting SalesExpense
        [HttpGet, Authorize]
        public ActionResult GetSalesExpenseWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,s.Id,CI.Id AS CostingItemId, s.OrderCostingMasterTemplateId, s.[Type], s.[Value],
       s.Amount, s.[Description],e.SystemId as ResponsiblePersonId, e.EmployeeName as ResponsiblePerson,ci.UserName
                        from hkp.CostingItem ci
                        join [dbo].[OrderPreCostingSalesExpense] s on CostingItemId = ci.Id  and s.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = s.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By CI.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveSalesExpense(IEnumerable<OrderPreCostingSalesExpense> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingSalesExpense] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        if (item.Value > 0)
                        {

                            if (dsMaster.Tables[0].DefaultView.Count == 0)
                            {
                                _id = "DP" + GetPK("OrderPreCostingSalesExpense");
                                DataRow dr = dsMaster.Tables[0].NewRow();

                                dr["Id"] = _id;
                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["Type"] = item.Type;
                                dr["Value"] = item.Value;
                                dr["Amount"] = item.Amount;
                                dr["Description"] = item.Description;

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                dsMaster.Tables[0].Rows.Add(dr);
                            }
                            else
                            {
                                //edit
                                DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();


                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["Type"] = item.Type;
                                dr["Value"] = item.Value;
                                dr["Amount"] = item.Amount;
                                dr["Description"] = item.Description;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;


                                dr.EndEdit();
                            }

                        }
                        else
                        {

                            while (dsMaster.Tables[0].DefaultView.Count > 0)
                                dsMaster.Tables[0].DefaultView[0].Delete();

                        }


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);


                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemWithSalesExpenseByComponentId(string costingComponentId)
        {
            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,um.UserName as UnitOfMeasurement,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId,
                            s.Type, s.Value,s.Amount,s.Description as dmDescription,s.Id  ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                            
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            left join [dbo].[OrderPreCostingSalesExpense] s on s.CostingItemId = ci.Id 
                            left join SCS.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                            where CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteSalesExpense(string salesExpenseId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingSalesExpense where id='" + salesExpenseId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForSalesExpense(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select s.*,e.EmployeeName as ResponsiblePerson, ci.UserName
						from hkp.CostingItem ci
                        inner join [dbo].[OrderPreCostingSalesExpense]  s on s.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = s.ResponsiblePersonId
                        where s.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion OrderCosting SalesExpense

        #region OrderCosting ValueLoss
        [HttpGet, Authorize]
        public ActionResult GetValueLossWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,p.Id,CI.Id AS CostingItemId, p.OrderCostingMasterTemplateId, p.[Type], p.[Value],
                    p.Amount, p.[Description],e.EmployeeName as ResponsiblePerson,p.ResponsiblePersonId,ci.UserName
                        from hkp.CostingItem ci
                        join [dbo].[OrderPreCostingValueLoss] p on CostingItemId = ci.Id  and p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By CI.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProfitWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,p.Id,CI.Id AS CostingItemId, p.OrderCostingMasterTemplateId, p.[Type], p.[Value],
                    p.Amount, p.[Description],e.EmployeeName as ResponsiblePerson,p.ResponsiblePersonId,ci.UserName
                        from hkp.CostingItem ci
                        join [dbo].[OrderPreCostingProfit] p on CostingItemId = ci.Id  and p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By CI.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult SaveValueLoss(IEnumerable<OrderPreCostingValueLoss> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingValueLoss] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        if (item.Value > 0)
                        {

                            if (dsMaster.Tables[0].DefaultView.Count == 0)
                            {
                                _id = "DP" + GetPK("OrderPreCostingSalesExpense");
                                DataRow dr = dsMaster.Tables[0].NewRow();

                                dr["Id"] = _id;
                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["Type"] = item.Type;
                                dr["Value"] = item.Value;
                                dr["Amount"] = item.Amount;
                                dr["Description"] = item.Description;

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                dsMaster.Tables[0].Rows.Add(dr);
                            }
                            else
                            {
                                //edit
                                DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();


                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["Type"] = item.Type;
                                dr["Value"] = item.Value;
                                dr["Amount"] = item.Amount;
                                dr["Description"] = item.Description;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;

                                dr.EndEdit();
                            }

                        }
                        else
                        {

                            while (dsMaster.Tables[0].DefaultView.Count > 0)
                                dsMaster.Tables[0].DefaultView[0].Delete();

                        }


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost, Authorize]
        public ActionResult SaveProfit(IEnumerable<OrderPreCostingProfit> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingProfit] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        if (item.Value > 0)
                        {

                            if (dsMaster.Tables[0].DefaultView.Count == 0)
                            {
                                _id = "DP" + GetPK("OrderPreCostingProfit");
                                DataRow dr = dsMaster.Tables[0].NewRow();

                                dr["Id"] = _id;
                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["Type"] = item.Type;
                                dr["Value"] = item.Value;
                                dr["Amount"] = item.Amount;
                                dr["Description"] = item.Description;

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                dsMaster.Tables[0].Rows.Add(dr);
                            }
                            else
                            {
                                //edit
                                DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();


                                dr["CostingItemId"] = item.CostingItemId;
                                dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                                dr["Type"] = item.Type;
                                dr["Value"] = item.Value;
                                dr["Amount"] = item.Amount;
                                dr["Description"] = item.Description;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;

                                dr.EndEdit();
                            }

                        }
                        else
                        {

                            while (dsMaster.Tables[0].DefaultView.Count > 0)
                                dsMaster.Tables[0].DefaultView[0].Delete();

                        }


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemWithValueLossByComponentId(string costingComponentId)
        {
            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,um.UserName as UnitOfMeasurement,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId,
                            s.Type, s.Value,s.Amount,s.Description as dmDescription,s.Id  ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                            
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            left join [dbo].[OrderPreCostingSalesExpense] s on s.CostingItemId = ci.Id 
                            left join SCS.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                            where CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteValueLoss(string ValueLossId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingValueLoss where id='" + ValueLossId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForValueLoss(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select p.*, ci.UserName, e.EmployeeName ResponsiblePerson
						from hkp.CostingItem ci
                        inner join [dbo].[OrderPreCostingValueLoss]  p on p.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Costing formula 

        [HttpGet, Authorize]
        public ActionResult CalculateFormula(string OrderCostingMasterTemplateId)
        {
            string sql = @"select sum(D.DirectMaterialCost) AS TotalDirectMaterial, sum(D.OperationCost) as TotalOperation,sum(D.ProcessCost) TotalProcess from 
                    (
                    select  sum(GrossAmount) AS DirectMaterialCost,0 AS OperationCost,0 AS ProcessCost from OrderPreCostingDirectMaterial  where OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"'
                    union all 
                    select 0, sum(Value) as OperationCost, 0 as ProcessCost from [dbo].[OrderPreCostingOperation] where OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"'
                    union all
                    select 0 , 0, sum(Value) as TotalOperation from [dbo].[OrderPreCostingDirectProcess] where OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"'
                    ) AS D";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion end Costing formula 
        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            try
            {
                string CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE  EMP.EmployeeStatus='Active' 
                                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void RecalculateValues(string TemplateMasterId)
        {
            try
            {
                //recalculate Direct Process
                string sql = @"UPDATE [OrderPreCostingDirectProcess] SET amount=isnull(Rate,0)+((MM.GrossAmount)*VALUE/100)
                            FROM [OrderPreCostingDirectProcess] AS K
                            LEFT JOIN (
                            SELECT OrderCostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.OrderCostingMasterTemplateId, M.GrossAmount
							                              FROM [OrderPreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.OrderCostingMasterTemplateId,M.[Value] 
								                        FROM [OrderPreCostingOperation] M) AS K GROUP BY K.OrderCostingMasterTemplateId
                            ) AS MM ON mm.OrderCostingMasterTemplateId=k.OrderCostingMasterTemplateId

                            WHERE k.OrderCostingMasterTemplateId='" + TemplateMasterId + "'";

                _sqlRepository.ExecuteSqlCommand(sql);

                sql = @"UPDATE [OrderPreCostingSalesExpense] SET amount=case when k.[Type]='FIXED' THEN Value ELSE ((MM.GrossAmount)*VALUE/100) END
                            FROM [OrderPreCostingSalesExpense] AS K
                            LEFT JOIN (
                            SELECT OrderCostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.OrderCostingMasterTemplateId, M.GrossAmount
								                            FROM [OrderPreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.OrderCostingMasterTemplateId,M.[Value] 
								                        FROM [OrderPreCostingOperation] M
							                            UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM [OrderPreCostingDirectProcess] M

								                            ) AS K GROUP BY K.OrderCostingMasterTemplateId
                            ) AS MM ON mm.OrderCostingMasterTemplateId=k.OrderCostingMasterTemplateId

                            WHERE k.OrderCostingMasterTemplateId='" + TemplateMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(sql);

                sql = @"UPDATE [OrderPreCostingValueLoss] SET amount=case when k.[Type]='FIXED' THEN Value ELSE ((MM.GrossAmount)*VALUE/100) END
                            FROM [OrderPreCostingValueLoss] AS K
                            LEFT JOIN (
                            SELECT OrderCostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.OrderCostingMasterTemplateId, M.GrossAmount
								                            FROM [OrderPreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.OrderCostingMasterTemplateId,M.[Value] 
								                        FROM [OrderPreCostingOperation] M
							                            UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM [OrderPreCostingDirectProcess] M

								                            ) AS K GROUP BY K.OrderCostingMasterTemplateId
                            ) AS MM ON mm.OrderCostingMasterTemplateId=k.OrderCostingMasterTemplateId

                            WHERE k.OrderCostingMasterTemplateId='" + TemplateMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(sql);


                sql = @"UPDATE [OrderPreCostingProfit] SET amount=case when k.[Type]='FIXED' THEN Value ELSE ((MM.GrossAmount)*VALUE/100) END
                            FROM [OrderPreCostingProfit] AS K
                            LEFT JOIN (
                            SELECT OrderCostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.OrderCostingMasterTemplateId, M.GrossAmount
								                            FROM [OrderPreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.OrderCostingMasterTemplateId,M.[Value] 
								                        FROM [OrderPreCostingOperation] M
							                            UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM [OrderPreCostingDirectProcess] M

							                            UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM [OrderPreCostingValueLoss] M
                                                            UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM [OrderPreCostingSalesExpense] M

								                            ) AS K GROUP BY K.OrderCostingMasterTemplateId
                            ) AS MM ON mm.OrderCostingMasterTemplateId=k.OrderCostingMasterTemplateId

                            WHERE k.OrderCostingMasterTemplateId='" + TemplateMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(sql);


            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

        #region Remove document 
        //public ActionResult DeleteDocumentPosition(string id)
        //{
        //    //_complianceDocumentPositonCodeService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}
        #endregion Remove document 
    }
    //public class OrderCostingBuyer
    //{
    //    public string Id { get; set; }
    //    public string OrderCostingMasterTemplateId { get; set; }
    //    public string BuyerId { get; set; }
    //    public string BuyerStyleRefNo { get; set; }
    //    public string OwnStyleRefNo { get; set; }

    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }

    //}
    //public class OrderCostingDetailTemplate
    //{
    //    public string Id { get; set; }
    //    public string CostingComponentId { get; set; }
    //    public string OrderCostingMasterTemplateId { get; set; }
    //    public string CostingVersionMasterTemplateId { get; set; }
    //    public decimal Sequence { get; set; }
    //    public decimal CostingValue { get; set; }
    //    public decimal BuyerTarget { get; set; }

    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }


    //}

    //public class OrderCostingMasterTemplate : BaseModel
    //{
    //    public string Id { get; set; }
    //    public string ProductMasterId { get; set; }
    //    public string CustomerId { get; set; }
    //    public string CostingMasterTemplateId { get; set; }
    //    public int Version { get; set; }
    //    public string Code { get; set; }
    //    public string SpecifyTo { get; set; }
    //    public string ShortName { get; set; }
    //    public string StandardName { get; set; }
    //    public string UserName { get; set; }
    //    public decimal OrderSize { get; set; }
    //    public int ProductionAvailableDays { get; set; }

    //    public decimal TargetSellingPrice { get; set; }
    //    public decimal PaymentDays { get; set; }
    //    public string PackingType { get; set; }
    //    public int EstNoOfPackingList { get; set; }
    //    public string FileName { get; set; }
    //    public string Description { get; set; }
    //    public string Remarks { get; set; }
    //    public bool Active { get; set; }

    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime? UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //    public decimal ExcessShipmentPer { get; set; }
    //    public string CurrencyId { get; set; }
    //    public string UOM { get; set; }
    //    public string TargetOrSPT { get; set; }
    //    public string CriticalLevel { get; set; }
    //    public decimal MKTTargetPerHour { get; set; }

    //    public decimal SPT { get; set; }
    //    public int NoOfWorkstation { get; set; }
    //    public decimal EfficiencyPercentage { get; set; }
    //    public decimal StandardWorkingHours { get; set; }
    //    public decimal WorkCenterTargetPerDay { get; set; }

    //    public decimal StandardWorkingHourCost { get; set; }
    //    public decimal AdditionalWorkingHourCostPerHour { get; set; }

    //    public decimal TargetCM { get; set; }
    //    public decimal TargetProfit { get; set; }
    //    public bool IsPercentage { get; set; }
    //}
    //public class OrderOrderCostingDetailTemplate
    //{
    //    public string Id { get; set; }
    //    public string CostingComponentId { get; set; }
    //    public string OrderCostingVersionMasterTemplateId { get; set; }
    //    public string OrderCostingMasterTemplateId { get; set; }
    //    public decimal Sequence { get; set; }
    //    public decimal CostingValue { get; set; }
    //    public decimal BuyerTarget { get; set; }

    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime? UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //    public decimal ExcessShipmentPer { get; set; }

    //}

    //public class OrderPreCostingDirectMaterial
    //{
    //    public string Id { get; set; }
    //    public string CostingItemId { get; set; }
    //    public decimal Consumption { get; set; }
    //    public decimal UOM { get; set; }
    //    public decimal Rate { get; set; }
    //    public decimal ValueLoss { get; set; }
    //    public decimal GrossConsumption { get; set; }
    //    public decimal GrossAmount { get; set; }
    //    public string OrderCostingMasterTemplateId { get; set; }
    //    public string ResponsiblePersonId { get; set; }

    //    public string SourcingType { get; set; }
    //    public string Usage { get; set; }
    //    public bool IsUDApplicable { get; set; }
    //    public bool IsGeneric { get; set; }
    //    public bool IsMandatory { get; set; }
    //    public string MaterialMasterId { get; set; }
    //    public string ArticleId { get; set; }
    //    public string VendorId { get; set; }



    //    public decimal MinimumOfQuantity { get; set; }
    //    public int POIssueDeadLine { get; set; }
    //    public string PurchaseGroupId { get; set; }
    //    public string Particulars { get; set; }
    //    public string Remarks { get; set; }


    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //}
    //public class OrderPreCostingSalesExpense
    //{
    //    public string Id { get; set; }
    //    public string CostingItemId { get; set; }
    //    public string OrderCostingMasterTemplateId { get; set; }
    //    public string ResponsiblePersonId { get; set; }

    //    public string Type { get; set; }
    //    public decimal Value { get; set; }
    //    public decimal Amount { get; set; }
    //    public string Description { get; set; }

    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //}
    //public class OrderPreCostingValueLoss
    //{
    //    public string Id { get; set; }
    //    public string CostingItemId { get; set; }
    //    public string OrderCostingMasterTemplateId { get; set; }
    //    public string ResponsiblePersonId { get; set; }

    //    public string Type { get; set; }
    //    public decimal Value { get; set; }
    //    public decimal Amount { get; set; }
    //    public string Description { get; set; }

    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //}
    //public class OrderPreCostingProfit
    //{
    //    public string Id { get; set; }
    //    public string CostingItemId { get; set; }
    //    public string OrderCostingMasterTemplateId { get; set; }
    //    public string ResponsiblePersonId { get; set; }

    //    public string Type { get; set; }
    //    public decimal Value { get; set; }
    //    public decimal Amount { get; set; }
    //    public string Description { get; set; }

    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //}
    //public class OrderPreCostingDirectProcess
    //{
    //    public string Id { get; set; }
    //    public string CostingItemId { get; set; }
    //    public string OrderCostingMasterTemplateId { get; set; }
    //    public string ResponsiblePersonId { get; set; }

    //    public string ExecutionType { get; set; }
    //    public decimal Value { get; set; }
    //    public decimal Rate { get; set; }
    //    public decimal Amount { get; set; }
    //    public string Description { get; set; }


    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //}
    //public class OrderPreCostingOperation
    //{
    //    public string Id { get; set; }
    //    public string CostingItemId { get; set; }
    //    public string OrderCostingMasterTemplateId { get; set; }
    //    public string ResponsiblePersonId { get; set; }

    //    public decimal Value { get; set; }
    //    public string Description { get; set; }


    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //}
}