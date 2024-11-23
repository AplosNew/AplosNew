#region Using

using Aplos.Areas.OrderManagements.Controllers;
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
    public class QuickCostingMasterController : BaseController
    {
        string TableName = "dbo.CostingMasterTemplate";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        Library.Service.Materials.MaterialMasterService _Materialservice;
        public QuickCostingMasterController(ISqlRepository R, Library.Service.Materials.MaterialMasterService M)
        {
            _sqlRepository = R;
            _Materialservice = M;
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
        public ActionResult GetHeightData(string PreCostMaterialId, string ParameterName)
        {
            string sql = @"select Id,AreaType,ParameterName,Parameter,Actual,Allowance,(Actual+Allowance)WithAllowance,NoOfParameter,Total from [dbo].[PreCostingDirectMaterialConsumption] where PreCostingDirectMaterialId='" + PreCostMaterialId + "' and ParameterName='" + ParameterName + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetDataFromItemCon(string ProductId, string MaterialId)
        {
            string sql = @"select m.Id,m.Description
                            from ItemConsumtionMaster m
                            where m.ProductMasterId='" + ProductId + "' and m.CostingItemId='" + MaterialId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory,CUR.Code AS Currency,ct.UserName AS CostingTypeName
							,psc.UserName as ProductSubCategory
                             ,pm.CostingType
							from CostingMasterTemplate qcm 
							
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join scs.Currency CUR on CUR.Id=qcm.CurrencyId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
                            ) AS TEMP WHERE 1=1 AND " + strkey;


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

            string sqlPurchasegroup = "SELECT Id,pg.UserName  FROM org.PurchaseGroup AS pg";

            return Json(new { DATA = data, PurchaseGroup = _sqlRepository.GetDataCollection(sqlPurchasegroup) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetListItem(string Id)

        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory,ct.UserName AS CostingTypeName
							,psc.UserName as ProductSubCategory
                             ,pm.CostingType,eff.StandardWorkingHours AS StandardWorkingHoursForProduct,MMA.StandardName Article
							from CostingMasterTemplate qcm 
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=qcm.ArticleId
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT JOIN [TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId=qcm.ProductMasterId AND EfficencyName='Costing'  
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
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
        public ActionResult GetCostingItemForSelection(string CostingMasterTemplateId, string costingComponentId, string Segment)
        {


            string TableName = "";
            string aND = "";
            if (Segment == CostingSegment.DirectMaterial.ToString())
            {
                TableName = "PreCostingDirectMaterial";
                aND = "AND ci.IsSubMaterial = 0";
            }
            else if (Segment == CostingSegment.DirectProcess.ToString())
                TableName = "PreCostingDirectProcess";
            else if (Segment == CostingSegment.Operation.ToString())
                TableName = "PreCostingOperation";
            else if (Segment == CostingSegment.Profit.ToString())
                TableName = "PreCostingProfit";
            else if (Segment == CostingSegment.SalesExpense.ToString())
                TableName = "PreCostingSalesExpense";
            else if (Segment == CostingSegment.ValueLoss.ToString())
                TableName = "PreCostingValueLoss";

            string sql = @"SELECT ci.ShortName,cat.UserName AS CostingCategory, CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END) AS Selected, ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                        o.CostingMasterTemplateId,
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                            ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            LEFT OUTER JOIN hkp.CostingCategory AS cat ON cat.Id=ci.CostingCategoryId
                            LEFT join " + TableName + @" o on o.CostingItemId = ci.Id AND o.CostingMasterTemplateId='" + CostingMasterTemplateId + @"'
                            WHERE ci.CostingComponentId='" + costingComponentId + @"' "+ aND + @"
                            ORDER BY CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END), ci.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult SubMaterial()
        {
            var sql = @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult SaveCostingItemsForCostingComponent(List<Dictionary<string, object>> itemList, string CostingMasterTemplateId, string costingComponentId, string Segment)
        {
            try
            {



                if (itemList == null)
                    throw new Exception("No Data Found");


                string TableName = "";
                if (Segment == CostingSegment.DirectMaterial.ToString())
                    TableName = "PreCostingDirectMaterial";
                else if (Segment == CostingSegment.DirectProcess.ToString())
                    TableName = "PreCostingDirectProcess";
                else if (Segment == CostingSegment.Operation.ToString())
                    TableName = "PreCostingOperation";
                else if (Segment == CostingSegment.Profit.ToString())
                    TableName = "PreCostingProfit";
                else if (Segment == CostingSegment.SalesExpense.ToString())
                    TableName = "PreCostingSalesExpense";
                else if (Segment == CostingSegment.ValueLoss.ToString())
                    TableName = "PreCostingValueLoss";


                DataSet dsMaster;

                ConnectionManager.DAL.ConManager objCon;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _id = string.Empty;


                string CostingItemIds = "''";
                foreach (var item in itemList)
                    CostingItemIds += ",'" + item["CostingItemId"].ToString() + "'";

                string sql = "Select * from " + TableName + " where CostingItemId in (" + CostingItemIds + ") AND CostingMasterTemplateId='" + CostingMasterTemplateId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                double MaxSequence = 0;
                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    if (MaxSequence < clsStaticInfo.dbl(dsMaster.Tables[0].Rows[i]["Sequence"].ToString()))
                        MaxSequence = clsStaticInfo.dbl(dsMaster.Tables[0].Rows[i]["Sequence"].ToString());
                }


                DataTable dtMainItems = _sqlRepository.GetDataTable(@"SELECT * FROM hkp.CostingItem AS ci WHERE ci.Id IN  (" + CostingItemIds + ") ");


                string Consumption = "Select * from PreCostingDirectMaterialConsumption where CostingItemId in (" + CostingItemIds + ") AND CostingMasterTemplateId='" + CostingMasterTemplateId + "'";
                string ConsumptionReference = @"SELECT m.ProductMasterId, m.CostingItemId, m.GSMValue,co.ComponentName,CO.AreaType,CO.NoOfParts,icc.ParameterName,
                                               icc.Parameter, icc.Actual, icc.Allowance, icc.Number AS NoOfParameter, icc.Total
                                                 from ItemConsumtionMaster M
                                               join ItemConsumtionComponent CO ON  m.Id=co.ItemConsumtionMasterId
                                               JOIN ItemConsumtionChild AS icc ON icc.ItemConsumtionComponentId=co.Id AND m.Id=icc.ItemConsumtionMasterId
                                               WHERE m.ProductMasterId=(SELECT top 1 cmt.ProductMasterId
                                               FROM CostingMasterTemplate AS cmt WHERE cmt.Id='" + CostingMasterTemplateId + @"')
                                               AND m.CostingItemId IN (" + CostingItemIds + ") ORDER BY m.CostingItemId, co.ComponentName,CO.AreaType";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(Consumption, out DataSet dsConsumption, false, "1");

                DataTable dtConsumptionReference = _sqlRepository.GetDataTable(ConsumptionReference);


                int Index = 0;
                foreach (var item in itemList)
                {

                    Index++;
                    dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item["CostingItemId"].ToString() + "'";
                    if (bplib.clsWebLib.GetBoolData(item["Selected"].ToString()))
                    {
                        if (dsMaster.Tables[0].DefaultView.Count > 0)
                            continue;
                    }
                    else
                    {
                        while (dsMaster.Tables[0].DefaultView.Count > 0)
                        {
                            dsConsumption.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dsMaster.Tables[0].DefaultView[0]["CostingItemId"].ToString() + "'";
                            while (dsConsumption.Tables[0].DefaultView.Count > 0)
                            {
                                dsConsumption.Tables[0].DefaultView[0].Delete();
                            }
                            dsMaster.Tables[0].DefaultView[0].Delete();
                        }


                        continue;
                    }
                    if (_id == "")
                    {
                        _id = "" + GetPK("PreCosting");
                    }


                    MaxSequence++;
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = _id + Index;
                    dr["CostingItemId"] = item["CostingItemId"];
                    dr["CostingMasterTemplateId"] = CostingMasterTemplateId;
                    dr["Sequence"] = MaxSequence;



                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;


                    //setting direct process value loss percentage as default value
                    if (Segment == CostingSegment.DirectProcess.ToString())
                    {
                        dtMainItems.DefaultView.RowFilter = "Id='" + item["CostingItemId"].ToString() + "'";
                        if (dtMainItems.DefaultView.Count > 0)
                        {
                            dr["Value"] = dtMainItems.DefaultView[0]["ValueLossPercentage"];
                        }
                    }


                    if (Segment == CostingSegment.ValueLoss.ToString())
                    {
                        dr["Type"] = "Percentage";
                        DataTable dtProductConfig = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[ProductMasterEfficency] WHERE ProductMasterId=(SELECT ProductMasterId FROM CostingMasterTemplate WHERE Id='" + CostingMasterTemplateId + "') AND EfficencyName='Costing'");
                        dtMainItems.DefaultView.RowFilter = "Code='VLS'";
                        if (dtMainItems.DefaultView.Count > 0)
                        {
                            if (dtProductConfig.Rows.Count > 0)
                                dr["Value"] = dtProductConfig.Rows[0]["ValueLossPercentage"];
                        }
                    }

                    if (Segment == CostingSegment.DirectMaterial.ToString())
                    {
                        dtConsumptionReference.DefaultView.RowFilter = "CostingItemId='" + item["CostingItemId"] + "'";
                        for (int CONS = 0; CONS < dtConsumptionReference.DefaultView.Count; CONS++)
                        {
                            DataRow drConsumption = dsConsumption.Tables[0].NewRow();
                            CopyRow(dtConsumptionReference.DefaultView[CONS].Row, drConsumption);
                            drConsumption["PreCostingDirectMaterialId"] = dr["Id"];
                            drConsumption["CostingMasterTemplateId"] = CostingMasterTemplateId;
                            dsConsumption.Tables[0].Rows.Add(drConsumption);
                        }

                        //calculate Consumption
                        dr["Consumption"] = CalculateConsumption(dtConsumptionReference.DefaultView.ToTable());
                    }


                    dsMaster.Tables[0].Rows.Add(dr);
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsConsumption);

                CalculateFormula(CostingMasterTemplateId);
                RecalculateValues(CostingMasterTemplateId);
                return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        private double CalculateConsumption(DataTable dt)
        {
            if (dt.Rows.Count == 0)
                return 0;

            int CentimeterFactor = 100;//to Meter
            double GSM = clsStaticInfo.dbl(dt.Rows[0]["GSMValue"].ToString());


            DataTable dtSummary = dt.AsEnumerable().GroupBy(x => new
            {
                AreaType = x["AreaType"],
                ComponentName = x["ComponentName"],
                ParameterName = x["ParameterName"],
                NoOfParts = x["NoOfParts"],
            })
                                        .Select(x =>
                                        {
                                            DataRow row = dt.NewRow();
                                            row["ComponentName"] = x.Key.ComponentName;
                                            row["ParameterName"] = x.Key.ParameterName;
                                            row["AreaType"] = x.Key.AreaType;
                                            row["NoOfParts"] = x.Key.NoOfParts;
                                            row["Total"] = x.Sum(r => (decimal)r["Total"]);
                                            return row;
                                        }
                                        ).CopyToDataTable();

            string ComponentName = "";
            double TotalArea = 0;
            for (int i = 0; i < dtSummary.Rows.Count; i++)
            {
                if (ComponentName == dtSummary.Rows[i]["ComponentName"].ToString())
                {
                    ComponentName = dtSummary.Rows[i]["ComponentName"].ToString();
                    continue;
                }

                double Parameter1 = clsStaticInfo.dbl(dtSummary.Compute("SUM(Total)", "ComponentName='" + dtSummary.Rows[i]["ComponentName"].ToString() + "' AND ParameterName='Height'").ToString());
                double Parameter2 = clsStaticInfo.dbl(dtSummary.Compute("SUM(Total)", "ComponentName='" + dtSummary.Rows[i]["ComponentName"].ToString() + "' AND ParameterName='Width'").ToString());
                double NoOfParts = clsStaticInfo.dbl(dtSummary.Rows[i]["NoOfParts"].ToString());

                Parameter1 = Parameter1 / CentimeterFactor;
                Parameter2 = Parameter2 / CentimeterFactor;

                if (dtSummary.Rows[i]["AreaType"].ToString().ToUpper() == "RECTANGULAR")
                    TotalArea += Parameter1 * Parameter2 * NoOfParts;
                else if (dtSummary.Rows[i]["AreaType"].ToString().ToUpper() == "TRIANGLE")
                    TotalArea += 0.5 * Parameter1 * Parameter2 * NoOfParts;
                else if (dtSummary.Rows[i]["AreaType"].ToString().ToUpper() == "CIRCLE")
                    TotalArea += Math.PI * Parameter1 * Parameter1 * NoOfParts;


                ComponentName = dtSummary.Rows[i]["ComponentName"].ToString();
            }

            return TotalArea * GSM;

        }
        private void CopyRow(DataRow drSource, DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {

                    drDestination[drSource.Table.Columns[COL].ColumnName] = bplib.clsWebLib.RetValidLen(drSource[drSource.Table.Columns[COL].ColumnName].ToString());

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

        [HttpPost, Authorize]
        public ActionResult GetDirectProcessRateValue(string CostingItemId)
        {
            try
            {
                var dtMainItems = _sqlRepository.GetDataCollection(@"SELECT * FROM hkp.CostingItem AS ci WHERE ci.Id ='" + CostingItemId + "' ");

                return Json(dtMainItems, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetVersionByVersionId(string versionId)
        {
            string sql = @"select * from CostingVersionMasterTemplate where Id = '" + versionId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetVersion(string CostingMasterTemplateId)
        {
            string sql = @"select  qcm.*  from CostingMasterTemplate qcm 
                                where qcm.Id = '" + CostingMasterTemplateId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQuickCostingByMasterId(string VersionId)
        {
            try
            {

                string sql = @"select  A.* from (
                        select um.UserName UOM, qcd.*,ISNULL(ctc.CostingType,'0') as CostingType, csc.UserName,  csc.Code, csc.StandardName,csc.ShortName,  0 as Status from dbo.CostingDetailTemplate qcd 
                        left join [HKP].[CostingComponent] csc ON csc.Id = qcd.CostingComponentId
                        left join CostingTypeComponent as ctc on ctc.CostingComponentId = csc.Id

						left join hkp.CostingItem ci on ci.CostingComponentId = csc.Id
						left join scs.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                        where ISNULL(CostingMasterTemplateId,'null')='" + VersionId + @"'
                        union 
                        select um.UserName UOM, qcvd.*,ISNULL(ctc.CostingType,'0') as CostingType, csc.UserName,  csc.Code, csc.StandardName,csc.ShortName, 1 as Status  from dbo.CostingVersionDetailTemplate qcvd 
						left join [HKP].[CostingComponent] csc ON csc.Id = qcvd.CostingComponentId
						left join CostingTypeComponent as ctc on ctc.CostingComponentId = csc.Id

						left join hkp.CostingItem ci on ci.CostingComponentId = csc.Id
						left join scs.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                         where ISNULL(CostingMasterTemplateId,'null')='" + VersionId + "') as A order by A.Sequence";

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
                string sql = @"select A.* from (select qcd.*, csc.UserName as CostingSubCategory, 0 as Status from dbo.CostingDetailTemplate qcd left join HKP.CostingSubCategory csc ON csc.Id = qcd.CostingSubCategoryId
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
            string sql = @"select * from CostingDetailTemplate where CostingVersionMasterTemplateId = '" + VersionId + "'";
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

        private DataSet GetCostingDetail(string CostingMasterTemplateId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select qcvm.* from CostingVersionMasterTemplate qcvm
                            left join CostingMasterTemplate qcm ON qcm.Id = qcvm.CostingMasterTemplateId 
                            where CostingMasterTemplateId = '" + CostingMasterTemplateId + "'"
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

                    dr["CostingMasterTemplateId"] = dsversion.Tables[0].Rows[0]["CostingMasterTemplateId"].ToString();

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
                strSQL = @"SELECT * FROM CostingDetailTemplate where CostingVersionMasterTemplateId = '" + versionId + "'";

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
        public ActionResult CreateCostingVersionDetail(string versionId, List<CostingDetailTemplate> data, string versionDescription)
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

        #region CostingDetailTemplate


        public void GetQuickCosting(string CostingVersionMasterTemplateId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM [dbo].[CostingDetailTemplate] WHERE CostingMasterTemplateId= '" + CostingVersionMasterTemplateId + "'";
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
        private void SaveCostingDetail(string masterid, List<CostingDetailTemplate> data, out DataSet dsdetail)
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
                    List<CostingDetailTemplate> FilterData = data.Where(a => a.Id == ownid).ToList();
                    if (FilterData.Count == 0)
                        dsdetail.Tables[0].Rows[i].Delete();
                }


                if (data != null)
                {

                    DataView dv = null;


                    dv = new DataView(dsdetail.Tables[0]);

                    string _Id = string.Empty;

                    _Id = GetPK("CostingDetailTemplate");

                    int count = 0;
                    foreach (var item in data)
                    {
                        dv.RowFilter = "Id='" + item.Id + "'";
                        if (dv.Count == 0)
                        {
                            count++;


                            DataRow dr = dsdetail.Tables[0].NewRow();
                            dr["Id"] = "QD" + _Id + "_" + count;

                            //dr["CostingTypeComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingTypeComponentId));
                            dr["CostingComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingComponentId));
                            dr["CostingMasterTemplateId"] = masterid;
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
                            dr["CostingMasterTemplateId"] = masterid;
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
        public ActionResult CreateCostingBuyer(CostingBuyer data)
        {
            DataSet dsCostingBuyer;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            //con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[CostingBuyer] where Id= '" + data.Id.ToString()+ "'", out dsCostingBuyer, false, "1");
            con.OpenDataSetThroughAdapter("select * from [dbo].[CostingBuyer] where Id='" + data.Id + "'", out dsCostingBuyer, false, "1");

            string _Id = "";

            #region data update
            if (dsCostingBuyer.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("dbo.preCostingDetail", out _Id);

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
        #endregion CostingDetailTemplate


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



        public void SaveCostingItemsForCostingComponent(DataSet dsTemplateMaster, out DataSet dsMaster)
        {
            try
            {

                DataSet dsItem;

                ConnectionManager.DAL.ConManager objCon;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _id = string.Empty;



                string sql = @"Select * from PreCostingOperation where CostingItemId in (SELECT I.Id FROM hkp.CostingItem I 
                                        INNER JOIN  hkp.CostingComponent AS cc  ON cc.Id=i.CostingComponentId
                                        WHERE cc.Code='OPN') AND CostingMasterTemplateId='" + dsTemplateMaster.Tables[0].Rows[0]["Id"].ToString() + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                sql = @"SELECT I.* FROM hkp.CostingItem I 
                        INNER JOIN  hkp.CostingComponent AS cc  ON cc.Id=i.CostingComponentId
                        WHERE cc.Code='OPN' ORDER BY cc.Code,cc.Sequence";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsItem, false, "1");


                string ProductConfigSql = @"SELECT * FROM [TRN].[ProductMasterEfficency] EFF WHERE 
                                    eff.ProductMasterId='" + dsTemplateMaster.Tables[0].Rows[0]["ProductMasterId"].ToString() + @"' AND EfficencyName='Costing'  ";

                DataTable dtTempProductConfig = _sqlRepository.GetDataTable(ProductConfigSql);

                int Index = 0;
                double CMValue = 0;
                foreach (DataRow item in dsItem.Tables[0].Rows)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item["Id"].ToString() + "'";


                    Index++;
                    DataRow dr;
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        if (_id == "")
                        {
                            _id = "" + GetPK("PreCosting");
                        }

                        dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = _id + Index;
                        dr["CostingItemId"] = item["Id"];
                        dr["CostingMasterTemplateId"] = dsTemplateMaster.Tables[0].Rows[0]["Id"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr["Value"] = "0";
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["CostingItemId"] = item["Id"];
                        dr["CostingMasterTemplateId"] = dsTemplateMaster.Tables[0].Rows[0]["Id"].ToString();

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr["Value"] = "0";

                        dr.EndEdit();
                    }



                    if (item["Code"].ToString().ToUpper() == "CM")
                    {
                        double additionalCost = 0;
                        double StandardWorkingHours = clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHours"].ToString());
                        double StandardWorkingHoursForProduct = clsStaticInfo.dbl(dtTempProductConfig.Rows[0]["StandardWorkingHours"].ToString());


                        if (StandardWorkingHours > StandardWorkingHoursForProduct)
                        {
                            additionalCost = (StandardWorkingHours - StandardWorkingHoursForProduct) * clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["AdditionalWorkingHourCostPerHour"].ToString());
                        }

                        CMValue = ((clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHourCost"].ToString()) + additionalCost) /
                           clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHours"].ToString())) /
                           clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["MKTTargetPerHour"].ToString());
                        dr["Value"] = CMValue;
                    }
                    else if (item["Code"].ToString().ToUpper() == "UPC")
                    {
                        double _workdays = clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["OrderSize"].ToString()) /
                            (clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHours"].ToString()) *
                             clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["MKTTargetPerHour"].ToString()));

                        int WorkDysRequired = Convert.ToInt32(Math.Ceiling(_workdays));

                        DataTable UpChargeMatrix = _sqlRepository.GetDataTable("SELECT TOP 1 * FROM hkp.CostingUpchargeMatrix AS cum WHERE cum.WorkCenterDays=" + WorkDysRequired.ToString());
                        if (UpChargeMatrix.Rows.Count == 0)
                            UpChargeMatrix = _sqlRepository.GetDataTable("SELECT TOP 1 * FROM hkp.CostingUpchargeMatrix AS cum WHERE cum.WorkCenterDays<=" + WorkDysRequired.ToString() + " ORDER BY WorkCenterDays desc");

                        if (UpChargeMatrix.Rows.Count > 0)
                        {
                            dr["Value"] = CMValue * clsStaticInfo.dbl(UpChargeMatrix.Rows[0][dsTemplateMaster.Tables[0].Rows[0]["CriticalLevel"].ToString()].ToString()) / 100;
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        [HttpPost]
        public JsonResult Create(FormCollection form)
        {
            try
            {


                var pre = form["modelNew"];
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var cost = JsonConvert.DeserializeObject<CostingMasterTemplate>(pre, settings);
                if (string.IsNullOrEmpty(cost.FileName) == false)
                    if (cost.FileName.Length > 50)
                        throw new Exception("File name should be less than 50 characters");

                DataSet dsMaster;
                DataSet dsItems = new DataSet();
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

                    cost.Id = _Id;
                    _Id = cost.Id;
                    AddNewRow(dsMaster.Tables[0], cost);

                    dsMaster.Tables[0].Rows[0]["CustomerId"] = DBNull.Value;
                    if (cost.SpecifyTo.ToUpper() == "CUSTOMER")
                        dsMaster.Tables[0].Rows[0]["CustomerId"] = cost.CustomerId;
                }
                else
                {
                    _Id = cost.Id.ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], cost);


                    dsMaster.Tables[0].Rows[0]["CustomerId"] = DBNull.Value;
                    if (cost.SpecifyTo.ToUpper() == "CUSTOMER")
                        dsMaster.Tables[0].Rows[0]["CustomerId"] = cost.CustomerId;
                }

                DataTable dtProductParams = _sqlRepository.GetDataTable(ProductParameters(dsMaster.Tables[0].Rows[0]["ProductMasterId"].ToString()));
                if (dtProductParams.Rows.Count > 0)
                {
                    dsMaster.Tables[0].Rows[0]["StandardWorkingHourCost"] = dtProductParams.Rows[0]["StandardWorkingHourCost"].ToString();
                    dsMaster.Tables[0].Rows[0]["AdditionalWorkingHourCostPerHour"] = dtProductParams.Rows[0]["AdditionalWorkingHourCostPerHour"].ToString();
                }
                SaveCostingItemsForCostingComponent(dsMaster, out dsItems);


                #endregion data update

                DataSet dsCostingDetail = null;

                var _quickCostingData = form["QuickCostingData"];
                var QuickCostingData = JsonConvert.DeserializeObject<List<CostingDetailTemplate>>(_quickCostingData, settings);
                SaveCostingDetail(_Id, QuickCostingData, out dsCostingDetail);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsCostingDetail, dsItems);



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
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public Dictionary<string, object> GetDocumentFile(string CostingMasterTemplateId)
        {
            try
            {
                var sql = @"Select Id, FileName From " + TableName + "  Where Id='" + CostingMasterTemplateId + "'";
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
                con.executeQuery("delete from CostingDetailTemplate where id='" + costingDetailId + "'");
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
                con.executeQuery("delete from [CostingBuyer] where CostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [CostingDetailTemplate] where CostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [PreCostingDirectProcess] where CostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [PreCostingOperation] where CostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [PreCostingValueLoss] where CostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [PreCostingSalesExpense] where CostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [PreCostingDirectMaterial] where CostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [PreCostingProfit] where CostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [PreCostingDirectMaterialConsumption] where CostingMasterTemplateId='" + id + "'");
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
        private void AddNewRow(DataTable dt, CostingMasterTemplate sourceData)
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
            //     dr["Sequence"] = sourceData.Sequence;
            dr["ShortName"] = sourceData.ShortName;
            dr["SpecifyTo"] = sourceData.SpecifyTo;
            dr["StandardName"] = sourceData.StandardName;
            dr["TargetSellingPrice"] = sourceData.TargetSellingPrice;
            dr["UserName"] = sourceData.UserName;
            dr["EstNoOfPackingList"] = sourceData.EstNoOfPackingList;
            dr["ExcessShipmentPer"] = sourceData.ExcessShipmentPer;
            dr["CurrencyId"] = sourceData.CurrencyId;


            dr["UOM"] = sourceData.UOM;
            dr["TargetOrSPT"] = sourceData.TargetOrSPT;
            dr["CriticalLevel"] = sourceData.CriticalLevel;


            dr["SPT"] = sourceData.SPT;
            dr["NoOfWorkstation"] = sourceData.NoOfWorkstation;
            dr["EfficiencyPercentage"] = sourceData.EfficiencyPercentage;
            dr["StandardWorkingHours"] = sourceData.StandardWorkingHours;
            dr["WorkCenterTargetPerDay"] = sourceData.WorkCenterTargetPerDay;
            dr["StandardWorkingHourCost"] = sourceData.StandardWorkingHourCost;
            dr["AdditionalWorkingHourCostPerHour"] = sourceData.AdditionalWorkingHourCostPerHour;


            dr["TargetCM"] = sourceData.TargetCM;
            dr["TargetProfit"] = sourceData.TargetProfit;
            dr["IsPercentage"] = sourceData.IsPercentage;

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }


        private void EditRow(DataRow dr, CostingMasterTemplate sourceData)
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
            dr["ArticleId"] = sourceData.ArticleId;

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        [Authorize]
        public ActionResult GetProductByProductMasterId(string ProductMasterId)
        {
            string sql = @"select pm.*, pc.UserName as ProductCategory,psc.UserName as ProductSubCategory ,
                                NoOfWorkstation,	EfficencyPercentage AS EfficiencyPercentage,
                                StandardWorkingHours,SPT
							
							from [MST].[ProductMaster] pm 
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT OUTER JOIN [TRN].[ProductMasterEfficency] EF ON ef.ProductMasterId=pm.Id AND ef.EfficencyName='Costing'
							where pm.Id = '" + ProductMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ProductMasterDetail(string ProductMasterId)
        {
            string sql = ProductParameters(ProductMasterId);

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

            return Json(new { Product = data }, JsonRequestBehavior.AllowGet);


            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private string ProductParameters(string ProductMasterId)
        {
            return @"select  pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory
                             ,pm.CostingType ,ct.UserName AS CostingTypeName,
                                NoOfWorkstation,	EfficencyPercentage AS EfficiencyPercentage,StandardWorkingHourCost,AdditionalWorkingHourCostPerHour,
                                StandardWorkingHours,SPT
							from  [MST].[ProductMaster] pm 
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
                            LEFT OUTER JOIN [TRN].[ProductMasterEfficency] EF ON ef.ProductMasterId=pm.Id AND ef.EfficencyName='Costing'
                            LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
							where pm.Id = '" + ProductMasterId + "'";
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
                         d.Id,cc.CalculationMethod
                        ,0 as Status
	                    ,d.CostingValue
	                    ,d.BuyerTarget
	                    ,d.CostingVersionMasterTemplateId
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
                        left outer join CostingDetailTemplate D on cc.id=d.CostingComponentId and d.CostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'
                         LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM PreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=    '" + CostingVersionMasterTemplateId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM PreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM PreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM PreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM PreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM PreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
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

					    select CostingComponentId from CostingDetailTemplate where  ISNULL(CostingMasterTemplateId,'')='" + CostingVersionMasterTemplateId + @"'

					--union

					--select CostingComponentId from CostingVersionDetailTemplate where  ISNULL(CostingMasterTemplateId,'')= '" + CostingVersionMasterTemplateId + @"'
                    )  order by isnull(ctc.Sequence,999999),cc.Description";

            string sqlAllItem = @"  SELECT  ci.Id,CI.Code,cc.CalculationMethod, ctc.Sequence AS ComponentSequence,ci.Sequence AS ItemSequnce, ci.CostingCategoryId, ci.CostingComponentId,cc.CostingSegment,upper(isnull(itemval.ValueType,'FIXED')) AS ValueType,
                        isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount,isnull(itemval.Value,0) AS Value,isnull(itemval.Rate,0) AS Rate
						 from hkp.CostingComponent CC
						 INNER JOIN hkp.CostingItem AS ci ON ci.CostingComponentId=cc.Id
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')
                        left outer join CostingDetailTemplate D on cc.id=d.CostingComponentId and d.CostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'
                         LEFT OUTER JOIN (        SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.GrossAmount AS TotalGrossAmount FROM PreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=    '" + CostingVersionMasterTemplateId + @"' 
                                            UNION ALL SELECT 'PERCENTAGE' AS ValueType, PC.Value,PC.Rate, i.Id,pc.Amount AS TotalGrossAmount FROM PreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.[Value]  AS TotalGrossAmount FROM PreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,          pc.Amount AS TotalGrossAmount FROM PreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM PreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM PreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
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

					    select CostingComponentId from CostingDetailTemplate where  ISNULL(CostingMasterTemplateId,'')='" + CostingVersionMasterTemplateId + @"'

                    ) order by ctc.Sequence,ci.Sequence --order by isnull(ctc.Sequence,999999),cc.Description";

            string sqlUpChargeMatrix = "SELECT * FROM hkp.CostingUpchargeMatrix AS cum WHERE cum.CostingType=(SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"') ORDER BY WorkCenterDays desc";
            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            return Json(new { UpChargeMatrix = _sqlRepository.GetDataCollection(sqlUpChargeMatrix), ComponentList = _sqlRepository.GetDataCollection(sql, null), ItemList = _sqlRepository.GetDataCollection(sqlAllItem, null) }, JsonRequestBehavior.AllowGet);
        }

        private string GetChargesPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(CostingItem), out sID);
            return sID;
        }

        private void AddNewPreCostingDetailRow(DataTable dt, Dictionary<string, object> sourceData)
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
        private void EditNewPreCostingDetailRow(DataRow dr, Dictionary<string, object> sourceData)
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


        private void AddNewCostingBuyerRow(DataTable dt, CostingBuyer sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();


            dr["Id"] = sourceData.Id;
            dr["CostingMasterTemplateId"] = sourceData.CostingMasterTemplateId;
            dr["BuyerId"] = sourceData.BuyerId;
            dr["BuyerStyleRefNo"] = sourceData.BuyerStyleRefNo;
            dr["OwnStyleRefNo"] = sourceData.OwnStyleRefNo;

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditNewCostingBuyerRow(DataRow dr, CostingBuyer sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            dr["Id"] = sourceData.Id;
            dr["CostingMasterTemplateId"] = sourceData.CostingMasterTemplateId;
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
        private void SavePreCostingDetail(Dictionary<string, object> preCostingDetail, out DataSet dsCostingDetail)
        {

            dsCostingDetail = null;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[PreCostingDetail] where Id= '" + preCostingDetail["Id"] + "'", out dsCostingDetail, false, "1");

            string _Id = "";

            #region data update
            if (dsCostingDetail.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("dbo.preCostingDetail", out _Id);

                preCostingDetail["Id"] = "PCD" + _Id;
                AddNewPreCostingDetailRow(dsCostingDetail.Tables[0], preCostingDetail);
            }
            else
            {
                _Id = preCostingDetail["Id"].ToString();
                EditNewPreCostingDetailRow(dsCostingDetail.Tables[0].Rows[0], preCostingDetail);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsCostingDetail);
        }
        [HttpPost, Authorize]
        public ActionResult SaveCostingItemsIncludingComponent(IEnumerable<CostingItem> costingItems, Dictionary<string, object> preCostingDetail)
        {

            SaveCostingItems(costingItems, out DataSet dsMaster);
            SavePreCostingDetail(preCostingDetail, out DataSet dsCostingDetail);

            return Json(new { costingItems = dsMaster, preCostingDetail = dsCostingDetail, Message = AplosMessage.Updated });
        }

        [Authorize]
        public ActionResult GetBuyerDataByCostingMasterId(string costingMasterId)
        {
            string sql = @"select cb.*, b.UserName as Buyer from [dbo].[CostingBuyer] cb
                            left join hkp.Buyer b on b.Id = cb.BuyerId where CostingMasterTemplateId = '" + costingMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult DeleteCostingBuyer(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[CostingBuyer] where id='" + id + "'");
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
                            left join PreCostingDirectMaterial pcdm on pcdm.CostingItemId = ci.Id 
                            left join SCS.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                           where CostingComponentId = '" + costingComponentId + "' ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SavePreCostingDirectMaterial(IEnumerable<PreCostingDirectMaterial> data, string costingMasterTemplateId)
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

                    string sql = "SELECT * FROM [dbo].[PreCostingDirectMaterial] WHERE CostingItemId IN (" + CostingItemIds + ") AND CostingMasterTemplateId='" + costingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Consumption > 0 && item.Rate > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DM" + GetPK("PreCostingDirectMaterial");
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Consumption"] = item.Consumption;
                            dr["UOM"] = item.UOM;
                            dr["Rate"] = item.Rate;
                            dr["ValueLoss"] = item.ValueLoss;
                            dr["GrossConsumption"] = item.GrossConsumption;
                            dr["GrossAmount"] = item.GrossAmount;
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["SourcingType"] = item.SourcingType;
                            dr["Sequence"] = item.Sequence;
                            dr["Usage"] = item.Usage;
                            dr["POCriteria"] = item.POCriteria;
                            dr["IsUDApplicable"] = item.IsUDApplicable;
                            dr["IsGeneric"] = item.IsGeneric;
                            dr["IsMandatory"] = item.IsMandatory;
                            dr["MaterialMasterId"] = item.MaterialMasterId;
                            dr["ArticleId"] = item.ArticleId;

                            dr["ProcurementLevel"] = item.ProcurementLevel;
                            dr["BOQDays"] = item.BOQDays;
                            dr["BOQCriteria"] = item.BOQCriteria;
                            dr["DependentDate"] = item.DependentDate;

                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["Particulars"] = item.Particulars;
                            dr["Remarks"] = item.Remarks;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;




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
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["SourcingType"] = item.SourcingType;
                            dr["Sequence"] = item.Sequence;
                            dr["Usage"] = item.Usage;
                            dr["POCriteria"] = item.POCriteria;
                            dr["IsUDApplicable"] = item.IsUDApplicable;
                            dr["IsGeneric"] = item.IsGeneric;
                            dr["IsMandatory"] = item.IsMandatory;
                            dr["MaterialMasterId"] = item.MaterialMasterId;
                            dr["ArticleId"] = item.ArticleId;

                            dr["ProcurementLevel"] = item.ProcurementLevel;
                            dr["BOQDays"] = item.BOQDays;
                            dr["BOQCriteria"] = item.BOQCriteria;
                            dr["DependentDate"] = item.DependentDate;

                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["Particulars"] = item.Particulars;
                            dr["Remarks"] = item.Remarks;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateValues(costingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public ActionResult GetDirectCostingMaterialWithItemByComponentId(string costingComponentId, string costingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,m.Sequence,m.Consumption,CI.Id AS CostingItemId,m.CostingMasterTemplateId,m.GrossAmount,m.GrossConsumption,m.Id,m.Rate,m.ResponsiblePersonId,m.UOM
                        ,isnull(m.ValueLoss,ci.Wastage) AS ValueLoss,M.Remarks,M.ProcurementLevel,M.BOQDays,M.DependentDate,M.BOQCriteria,MGM.UserName AS MaterialGroup
                        ,m.SourcingType, ISNULL(m.IsUDApplicable,0) AS IsUDApplicable, m.Usage, ISNULL(m.IsGeneric,0) AS IsGeneric,ISNULL(m.IsMandatory,0) AS  IsMandatory
						,m.MaterialMasterId, m.ArticleId, mm.UserName as MaterialMasterName, mma.StandardName as ArticleName
                        ,e.EmployeeName as ResponsiblePerson, e.SystemId as ResponsiblePersonId,um.UserName as UnitOfMeasurement, um.Id as UoMId, ci.UserName,
                        ISNULL(m.MinimumOfQuantity,ci.MinimumOfQuantity) AS MinimumOfQuantity,ISNULL(m.POIssueDeadLine,ci.POIssueDeadLine)POIssueDeadLine,
                        ISNULL(m.PurchaseGroupId,ci.PurchaseGroupId) AS PurchaseGroupId,ISNULL(m.Particulars,ci.UserName) AS Particulars,
                        m.POCriteria
						 from hkp.CostingItem ci
                         JOIN [dbo].[PreCostingDirectMaterial] m on m.CostingItemId = ci.Id  and m.CostingMasterTemplateId = '" + costingMasterTemplateId + @"'
                        left join mst.MaterialGroupMaster MGM on MGM.Id=CI.MaterialGroupMasterId
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                        left join [SCS].[UnitOfMeasurement] um on um.Id = ci.UnitOfMeasurementId
						left join dbo.EmployeeInformation e on e.SystemId = m.ResponsiblePersonId
						left join mst.MaterialMaster mm on mm.Id = m.MaterialMasterId 
						left join [MST].[MaterialMasterArticle] mma on mma.Id = m.ArticleId 
						WHERE ci.CostingComponentId='" + costingComponentId + @"' Order By m.Sequence";


            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilter(string costingMasterTemplateId, string costingComponentId)
        {
            string sql = @" select m.*,e.EmployeeName as ResponsiblePerson,um.UserName as UnitOfMeasurement, um.Id as UoMId, ci.UserName, m.Description
                        ,m.MaterialMasterId, m.ArticleId, mm.UserName as MaterialMasterName, mma.StandardName as ArticleName from hkp.CostingItem ci
                        inner join [dbo].[PreCostingDirectMaterial] m on m.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                        left join [SCS].[UnitOfMeasurement] um on um.Id = ci.UnitOfMeasurementId
						left join EmployeeInformation e on e.SystemId = m.ResponsiblePersonId
						left join mst.MaterialMaster mm on mm.Id = m.MaterialMasterId 
						left join [MST].[MaterialMasterArticle] mma on mma.Id = m.ArticleId 
                        where m.CostingMasterTemplateId = '" + costingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductUOMCbo(string ProductMasterId)
        {
            string sql = @"Select P.Id ProductMasterId,BUoM.Id AS Value,BUoM.UserName AS Text from [MST].[ProductMaster] P
                            LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=P.BaseUOMId
                            Where ISNULL(BUoM.Id,'')<>'' and p.Id='" + ProductMasterId + @"'
                            UNION ALL
                            Select AUom.ProductMasterId,BUoM.Id,BUoM.UserName from MST.ProductMasterAlternativeUoM AUoM 
                            LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=AUom.AlternativeUOMId
                            Where ISNULL(BUoM.Id,'')<>'' and AUom.ProductMasterId='" + ProductMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DeleteDirectMaterial(string DirectMaterialId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from PreCostingDirectMaterialConsumption where PreCostingDirectMaterialId='" + DirectMaterialId + "'");
                con.executeQuery("delete from PreCostingDirectMaterial where id='" + DirectMaterialId + "'");
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
        public ActionResult GetOperationWithItemByComponentId(string costingComponentId, string costingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,o.Sequence,o.Id,CI.Id AS CostingItemId, o.[Value], o.[Description],e.EmployeeName as ResponsiblePerson, e.SystemId as ResponsiblePersonId,  ci.UserName, o.Description
                            from hkp.CostingItem ci
						 join [dbo].[PreCostingOperation] o on o.CostingItemId = ci.Id  and o.CostingMasterTemplateId = '" + costingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = o.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By o.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveOperation(IEnumerable<PreCostingOperation> data, string costingMasterTemplateId)
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

                    string sql = "SELECT * FROM [dbo].[PreCostingOperation] WHERE CostingItemId IN (" + CostingItemIds + ") AND CostingMasterTemplateId='" + costingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "CO" + GetPK("PreCostingOperation");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
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
                            dr["Sequence"] = item.Sequence;
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                            dr["Value"] = item.Value;
                            dr["Description"] = item.Description;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }

                RecalculateValues(costingMasterTemplateId);
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
            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, o.CostingMasterTemplateId,
                                 ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                                 o.Description as dmDescription,o.Value ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                                 from hkp.CostingItem ci 
                                 left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
            left join [dbo].[PreCostingOperation] o on o.CostingItemId = ci.Id 
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
                con.executeQuery("delete from PreCostingOperation where id='" + operationId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForOperation(string costingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select o.*,e.EmployeeName as ResponsiblePerson, ci.UserName, o.Description from hkp.CostingItem ci
                        inner join  [dbo].[PreCostingOperation] o on o.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = o.ResponsiblePersonId 
                            where o.CostingMasterTemplateId = '" + costingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion Operation

        #region PreCosting Direct Process
        [HttpGet, Authorize]
        public ActionResult GetDirectProcessWithItemByComponentId(string costingComponentId, string costingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,p.Sequence,ci.UserName,p.Id,CI.Id AS CostingItemId, p.CostingMasterTemplateId, p.ExecutionType,
       p.[Value], p.Rate, p.Amount, p.[Description],e.SystemId as ResponsiblePersonId, e.EmployeeName as ResponsiblePerson
                        from hkp.CostingItem ci
                        join [dbo].[PreCostingDirectProcess] p on CostingItemId = ci.Id and p.CostingMasterTemplateId = '" + costingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By p.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveDirectProcess(IEnumerable<PreCostingDirectProcess> data, string costingMasterTemplateId)
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

                    string sql = "SELECT * FROM [dbo].[PreCostingDirectProcess] WHERE CostingItemId IN (" + CostingItemIds + ") AND CostingMasterTemplateId='" + costingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Amount > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DP" + GetPK("PreCostingDirectProcess");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
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
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
                            dr["Sequence"] = item.Sequence;
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

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateValues(costingMasterTemplateId);
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
                            left join [dbo].[PreCostingDirectProcess] p on p.CostingItemId = ci.Id 
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
                con.executeQuery("delete from PreCostingDirectProcess where id='" + directProcessId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForDirectProcess(string costingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select p.*,e.EmployeeName as ResponsiblePerson, ci.UserName
						from hkp.CostingItem ci
                        inner join [dbo].[PreCostingDirectProcess]  p on p.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId 
                        where p.CostingMasterTemplateId = '" + costingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion Pre Costing Direct Process

        #region Precosting SalesExpense
        [HttpGet, Authorize]
        public ActionResult GetSalesExpenseWithItemByComponentId(string costingComponentId, string costingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,s.Sequence,s.Id,CI.Id AS CostingItemId, s.CostingMasterTemplateId, s.[Type], s.[Value],
       s.Amount, s.[Description],e.SystemId as ResponsiblePersonId, e.EmployeeName as ResponsiblePerson,ci.UserName
                        from hkp.CostingItem ci
                        join [dbo].[PreCostingSalesExpense] s on CostingItemId = ci.Id  and s.CostingMasterTemplateId = '" + costingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = s.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By s.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveSalesExpense(IEnumerable<PreCostingSalesExpense> data, string costingMasterTemplateId)
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

                    string sql = "SELECT * FROM [dbo].[PreCostingSalesExpense] WHERE CostingItemId IN (" + CostingItemIds + ") AND CostingMasterTemplateId='" + costingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DP" + GetPK("PreCostingSalesExpense");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
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
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
                            dr["Sequence"] = item.Sequence;
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

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);


                }
                RecalculateValues(costingMasterTemplateId);
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
                            left join [dbo].[PreCostingSalesExpense] s on s.CostingItemId = ci.Id 
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
                con.executeQuery("delete from PreCostingSalesExpense where id='" + salesExpenseId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForSalesExpense(string costingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select s.*,e.EmployeeName as ResponsiblePerson, ci.UserName
						from hkp.CostingItem ci
                        inner join [dbo].[PreCostingSalesExpense]  s on s.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = s.ResponsiblePersonId
                        where s.CostingMasterTemplateId = '" + costingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion Precosting SalesExpense

        #region Precosting ValueLoss
        [HttpGet, Authorize]
        public ActionResult GetValueLossWithItemByComponentId(string costingComponentId, string costingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,p.Sequence,p.Id,CI.Id AS CostingItemId, p.CostingMasterTemplateId, p.[Type], p.[Value],
                    p.Amount, p.[Description],e.EmployeeName as ResponsiblePerson,p.ResponsiblePersonId,ci.UserName
                        from hkp.CostingItem ci
                        join [dbo].[PreCostingValueLoss] p on CostingItemId = ci.Id  and p.CostingMasterTemplateId = '" + costingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By p.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProfitWithItemByComponentId(string costingComponentId, string costingMasterTemplateId)
        {
            string sql = @"select ci.CostingComponentId,p.Sequence,p.Id,CI.Id AS CostingItemId, p.CostingMasterTemplateId, p.[Type], p.[Value],
                    p.Amount, p.[Description],e.EmployeeName as ResponsiblePerson,p.ResponsiblePersonId,ci.UserName
                        from hkp.CostingItem ci
                        join [dbo].[PreCostingProfit] p on CostingItemId = ci.Id  and p.CostingMasterTemplateId = '" + costingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By p.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveValueLoss(IEnumerable<PreCostingValueLoss> data, string costingMasterTemplateId)
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

                    string sql = "SELECT * FROM [dbo].[PreCostingValueLoss] WHERE CostingItemId IN (" + CostingItemIds + ") AND CostingMasterTemplateId='" + costingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DP" + GetPK("PreCostingSalesExpense");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
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
                            dr["Sequence"] = item.Sequence;
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
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

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateValues(costingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost, Authorize]
        public ActionResult SaveProfit(IEnumerable<PreCostingProfit> data, string costingMasterTemplateId)
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

                    string sql = "SELECT * FROM [dbo].[PreCostingProfit] WHERE CostingItemId IN (" + CostingItemIds + ") AND CostingMasterTemplateId='" + costingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "P" + GetPK("PreCostingProfit");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
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
                            dr["CostingMasterTemplateId"] = costingMasterTemplateId;
                            dr["Sequence"] = item.Sequence;
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

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateValues(costingMasterTemplateId);
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
                            left join [dbo].[PreCostingSalesExpense] s on s.CostingItemId = ci.Id 
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
                con.executeQuery("delete from PreCostingValueLoss where id='" + ValueLossId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForValueLoss(string costingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select p.*, ci.UserName, e.EmployeeName ResponsiblePerson
						from hkp.CostingItem ci
                        inner join [dbo].[PreCostingValueLoss]  p on p.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where p.CostingMasterTemplateId = '" + costingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Costing formula 

        [HttpGet, Authorize]
        public ActionResult CalculateFormula(string costingMasterTemplateId)
        {
            string sql = @"select sum(D.DirectMaterialCost) AS TotalDirectMaterial, sum(D.OperationCost) as TotalOperation,sum(D.ProcessCost) TotalProcess from 
                    (
                    select  sum(GrossAmount) AS DirectMaterialCost,0 AS OperationCost,0 AS ProcessCost from PreCostingDirectMaterial  where CostingMasterTemplateId = '" + costingMasterTemplateId + @"'
                    union all 
                    select 0, sum(Value) as OperationCost, 0 as ProcessCost from [dbo].[PreCostingOperation] where CostingMasterTemplateId = '" + costingMasterTemplateId + @"'
                    union all
                    select 0 , 0, sum(Value) as TotalOperation from [dbo].[PreCostingDirectProcess] where CostingMasterTemplateId = '" + costingMasterTemplateId + @"'
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
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE  EMP.EmployeeStatus='Active' 
                                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

                var json =  Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
               
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void RecalculateValues_backup(string TemplateMasterId)
        {
            try
            {
                //recalculate Direct Process
                string sql = @"UPDATE [PreCostingDirectProcess] SET amount=isnull(Rate,0)+((MM.GrossAmount)*VALUE/100)
                            FROM [PreCostingDirectProcess] AS K
                            LEFT JOIN (
                            SELECT CostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.CostingMasterTemplateId, M.GrossAmount
							                              FROM [PreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.CostingMasterTemplateId,M.[Value] 
								                        FROM [PreCostingOperation] M) AS K GROUP BY K.CostingMasterTemplateId
                            ) AS MM ON mm.CostingMasterTemplateId=k.CostingMasterTemplateId

                            WHERE k.CostingMasterTemplateId='" + TemplateMasterId + "'";

                _sqlRepository.ExecuteSqlCommand(sql);

                sql = @"UPDATE [PreCostingSalesExpense] SET amount=case when k.[Type]='FIXED' THEN Value ELSE ((MM.GrossAmount)*VALUE/100) END
                            FROM [PreCostingSalesExpense] AS K
                            LEFT JOIN (
                            SELECT CostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.CostingMasterTemplateId, M.GrossAmount
								                            FROM [PreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.CostingMasterTemplateId,M.[Value] 
								                        FROM [PreCostingOperation] M
							                            UNION ALL
								                            SELECT m.CostingMasterTemplateId,M.Amount 
								                            FROM [PreCostingDirectProcess] M

								                            ) AS K GROUP BY K.CostingMasterTemplateId
                            ) AS MM ON mm.CostingMasterTemplateId=k.CostingMasterTemplateId

                            WHERE k.CostingMasterTemplateId='" + TemplateMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(sql);

                sql = @"UPDATE [PreCostingValueLoss] SET amount=case when k.[Type]='FIXED' THEN Value ELSE ((MM.GrossAmount)*VALUE/100) END
                            FROM [PreCostingValueLoss] AS K
                            LEFT JOIN (
                            SELECT CostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.CostingMasterTemplateId, M.GrossAmount
								                            FROM [PreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.CostingMasterTemplateId,M.[Value] 
								                        FROM [PreCostingOperation] M
							                            UNION ALL
								                            SELECT m.CostingMasterTemplateId,M.Amount 
								                            FROM [PreCostingDirectProcess] M

								                            ) AS K GROUP BY K.CostingMasterTemplateId
                            ) AS MM ON mm.CostingMasterTemplateId=k.CostingMasterTemplateId

                            WHERE k.CostingMasterTemplateId='" + TemplateMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(sql);

                sql = @"UPDATE [PreCostingProfit] SET amount=case when k.[Type]='FIXED' THEN Value ELSE ((MM.GrossAmount)*VALUE/100) END
                            FROM [PreCostingProfit] AS K
                            LEFT JOIN (
                            SELECT CostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.CostingMasterTemplateId, M.GrossAmount
								                            FROM [PreCostingDirectMaterial] M
							                                UNION ALL
							                                SELECT m.CostingMasterTemplateId,M.[Value] 
								                            FROM [PreCostingOperation] M
							                                UNION ALL
								                            SELECT m.CostingMasterTemplateId,M.Amount 
								                            FROM [PreCostingDirectProcess] M
								                             UNION ALL
								                            SELECT m.CostingMasterTemplateId,M.Amount 
								                            FROM PreCostingValueLoss M
								                            
								                               UNION ALL
								                            SELECT m.CostingMasterTemplateId,M.Amount 
								                            FROM PreCostingSalesExpense AS M
								                            ) AS K GROUP BY K.CostingMasterTemplateId
                            ) AS MM ON mm.CostingMasterTemplateId=k.CostingMasterTemplateId

                            WHERE k.CostingMasterTemplateId='" + TemplateMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(sql);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        public void RecalculateValues(string TemplateMasterId)
        {
            try
            {
                //recalculate Direct Process
                string sql = @" SELECT  ci.Id,cc.CalculationMethod, ctc.Sequence AS ComponentSequence,ci.Sequence AS ItemSequnce, ci.CostingCategoryId, ci.CostingComponentId,cc.CostingSegment,upper(isnull(itemval.ValueType,'FIXED')) AS ValueType,
                        isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount,isnull(itemval.Value,0) AS Value,isnull(itemval.Rate,0) AS Rate
						  from  CostingDetailTemplate D 
						 INNER JOIN CostingMasterTemplate AS cmt ON cmt.Id=d.CostingMasterTemplateId
						 inner join hkp.CostingComponent CC on cc.id=d.CostingComponentId
						 INNER JOIN hkp.CostingItem AS ci ON ci.CostingComponentId=cc.Id
                         left outer join [dbo].[CostingTypeComponent] AS ctc  
                         ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster 
                                                                                  WHERE Id = cmt.ProductMasterId)

                         LEFT OUTER JOIN (SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.GrossAmount AS TotalGrossAmount FROM PreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=    '" + TemplateMasterId + @"' 
                                            UNION ALL SELECT 'PERCENTAGE' AS ValueType, PC.Value,PC.Rate, i.Id,pc.Amount AS TotalGrossAmount FROM PreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                            UNION ALL SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.[Value]  AS TotalGrossAmount FROM PreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId='" + TemplateMasterId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,          pc.Amount AS TotalGrossAmount FROM PreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM PreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM PreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.CostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                  )AS ITEMVAL ON  itemval.Id=ci.Id
                         WHERE d.CostingMasterTemplateId='" + TemplateMasterId + @"'
                          order by ctc.Sequence,ci.Sequence";

                DataTable dtReference = _sqlRepository.GetDataTable(sql);

                for (int i = 0; i < dtReference.Rows.Count; i++)
                {
                    if (dtReference.Rows[i]["CostingSegment"].ToString().ToUpper() != "DIRECTPROCESS" && (clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString()) > 0 || dtReference.Rows[i]["ValueType"].ToString().ToUpper() == "PERCENTAGE"))
                    {
                        double TotalFixedValue = getFixedAmount(dtReference, clsStaticInfo.dbl(dtReference.Rows[i]["ComponentSequence"].ToString()));
                        double TotalCurrentFixed = getCurrentFixedAmount(dtReference, clsStaticInfo.dbl(dtReference.Rows[i]["ComponentSequence"].ToString()));
                        double CurrentPercent = getCurrentPercent(dtReference, clsStaticInfo.dbl(dtReference.Rows[i]["ComponentSequence"].ToString()));

                        double CurrentGrossValue = 0;
                        double Percentage = 0;
                        if (clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString()) > 0)
                        {
                            //because we have both fixed value and a rate (e.g. DirectProcess) which will sum up to TotalGrossValue
                            CurrentGrossValue = clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString());
                        }
                        Percentage = clsStaticInfo.dbl(dtReference.Rows[i]["Value"].ToString());


                        //now add percentage portion with the CurrentGrossValue
                        if (dtReference.Rows[i]["CalculationMethod"].ToString().ToUpper() == "CUMULATIVE")
                            CurrentGrossValue += TotalFixedValue * (Percentage / 100);
                        else
                            CurrentGrossValue += ((TotalFixedValue + TotalCurrentFixed) / ((100 - CurrentPercent) / 100)) * (Percentage / 100);
                        //CurrentGrossValue += TotalFixedValue * (Percentage / 100);
                        dtReference.Rows[i]["TotalGrossAmount"] = CurrentGrossValue;
                    }
                    else if (dtReference.Rows[i]["CostingSegment"].ToString().ToUpper() == "DIRECTPROCESS")
                    {
                        double TotalFixedValue = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "CostingSegment='DirectMaterial'").ToString());
                        double CurrentGrossValue = 0;
                        double Percentage = 0;
                        if (clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString()) > 0)
                        {
                            //because we have both fixed value and a rate (e.g. DirectProcess) which will sum up to TotalGrossValue
                            CurrentGrossValue = clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString());
                        }
                        Percentage = clsStaticInfo.dbl(dtReference.Rows[i]["Value"].ToString());


                        //now add percentage portion with the CurrentGrossValue
                        CurrentGrossValue += (TotalFixedValue / ((100 - Percentage) / 100)) - TotalFixedValue; //TotalFixedValue * (Percentage / 100);

                        dtReference.Rows[i]["TotalGrossAmount"] = CurrentGrossValue;
                    }
                }


                //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                dtReference.DefaultView.RowFilter = null;
                DataTable dvDistinctSegment = dtReference.DefaultView.ToTable(true, "CostingSegment");
                for (int i = 0; i < dvDistinctSegment.Rows.Count; i++)
                {
                    if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.DirectMaterial.ToString())
                        UpdateCostingItems(TemplateMasterId, "PreCostingDirectMaterial", CostingSegment.DirectMaterial.ToString(), "GrossAmount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.Operation.ToString())
                        UpdateCostingItems(TemplateMasterId, "PreCostingOperation", CostingSegment.Operation.ToString(), "Value", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.DirectProcess.ToString())
                        UpdateCostingItems(TemplateMasterId, "PreCostingDirectProcess", CostingSegment.DirectProcess.ToString(), "Amount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.SalesExpense.ToString())
                        UpdateCostingItems(TemplateMasterId, "PreCostingSalesExpense", CostingSegment.SalesExpense.ToString(), "Amount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.ValueLoss.ToString())
                        UpdateCostingItems(TemplateMasterId, "PreCostingValueLoss", CostingSegment.ValueLoss.ToString(), "Amount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.Profit.ToString())
                        UpdateCostingItems(TemplateMasterId, "PreCostingProfit", CostingSegment.Profit.ToString(), "Amount", dtReference);


                }

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        private void UpdateCostingItems(string CostingMasterTemplateId, string TableName, string SegmentName, string UpdateColumnName, DataTable dtReference)
        {

            string strSql = "Select* from " + TableName + " Where CostingMasterTemplateId='" + CostingMasterTemplateId + "'";
            DataSet dsData;
            ConnectionManager.DAL.ConManager objCon;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(strSql, out dsData, false, "1");

            if (dsData.Tables[0].Rows.Count == 0)
                return;


            dtReference.DefaultView.RowFilter = "CostingSegment='" + SegmentName + "'";
            for (int i = 0; i < dtReference.DefaultView.Count; i++)
            {
                dsData.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dtReference.DefaultView[i]["Id"].ToString() + "'";
                if (dsData.Tables[0].DefaultView.Count > 0)
                {
                    DataRow dr = dsData.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr[UpdateColumnName] = clsStaticInfo.dbl(dtReference.DefaultView[i]["TotalGrossAmount"].ToString());
                    dr.EndEdit();
                }
            }


            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsData);
        }

        private double getFixedAmount(DataTable dtReference, double CurrentSequence)
        {
            double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "ComponentSequence<" + CurrentSequence).ToString());
            //totalPrevious += clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "(Rate=0 OR ValueType<>'PERCENTAGE') AND ComponentSequence=" + CurrentSequence).ToString());
            return totalPrevious;
        }
        private double getCurrentFixedAmount(DataTable dtReference, double CurrentSequence)
        {
            //double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "ComponentSequence<" + CurrentSequence).ToString());
            double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "(ValueType<>'PERCENTAGE') AND ComponentSequence=" + CurrentSequence).ToString());
            totalPrevious += clsStaticInfo.dbl(dtReference.Compute("SUM(Rate)", "(Rate>0 AND ValueType='PERCENTAGE') AND ComponentSequence=" + CurrentSequence).ToString());
            return totalPrevious;
        }
        private double getCurrentPercent(DataTable dtReference, double CurrentSequence)
        {
            //double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "ComponentSequence<" + CurrentSequence).ToString());
            double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(Value)", "ValueType='PERCENTAGE' AND ComponentSequence=" + CurrentSequence).ToString());
            return totalPrevious;
        }

        #region Remove document 
        //public ActionResult DeleteDocumentPosition(string id)
        //{
        //    //_complianceDocumentPositonCodeService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}
        #endregion Remove document 

        #region Update By Saad
        [HttpPost, Authorize]
        public JsonResult SaveNewItemConsumptionData(string PreCostingDirectMaterialId, string ItemConsumtionId, string CostingMasterTemplateId)
        {
            try
            {
                string _id = string.Empty;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string Consumption = "Select * from PreCostingDirectMaterialConsumption where PreCostingDirectMaterialId ='" + PreCostingDirectMaterialId + "'";

                string PreCostingDirectMaterial = "Select * from PreCostingDirectMaterial where Id ='" + PreCostingDirectMaterialId + "'";

                DataSet dsChild;
                con.OpenDataSetThroughAdapter(" ", out dsChild, false, "1");
                string ConsumptionReference = @"SELECT m.ProductMasterId, m.CostingItemId, m.GSMValue,co.ComponentName,CO.AreaType,CO.NoOfParts,icc.ParameterName,icc.Parameter, icc.Actual, icc.Allowance, icc.Number AS NoOfParameter, icc.Total from ItemConsumtionMaster M
                                               join ItemConsumtionComponent CO ON  m.Id = co.ItemConsumtionMasterId
                                               JOIN ItemConsumtionChild AS icc ON icc.ItemConsumtionComponentId = co.Id AND m.Id = icc.ItemConsumtionMasterId
                                               WHERE m.Id = '" + ItemConsumtionId + "'";

                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(Consumption, out DataSet dsConsumption, false, "1");
                con.OpenDataSetThroughAdapter(PreCostingDirectMaterial, out DataSet dsPreCostingDirectMaterial, false, "1");
                DataTable dtConsumptionReference = _sqlRepository.GetDataTable(ConsumptionReference);



                while (dsConsumption.Tables[0].DefaultView.Count > 0)
                {
                    dsConsumption.Tables[0].DefaultView[0].Delete();
                }

                //_id = "" + GetPK("PreCosting");
                //dr["Id"] = _id;

                for (int CONS = 0; CONS < dtConsumptionReference.DefaultView.Count; CONS++)
                {
                    DataRow drConsumption = dsConsumption.Tables[0].NewRow();
                    CopyRow(dtConsumptionReference.DefaultView[CONS].Row, drConsumption);
                    drConsumption["PreCostingDirectMaterialId"] = PreCostingDirectMaterialId;
                    drConsumption["CostingMasterTemplateId"] = CostingMasterTemplateId;
                    dsConsumption.Tables[0].Rows.Add(drConsumption);
                }

                //calculate Consumption
                dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"] = CalculateConsumption(dtConsumptionReference.DefaultView.ToTable());


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsConsumption, dsPreCostingDirectMaterial);
                return Json(new { Error = false, Consumption = clsStaticInfo.dbl(dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"].ToString()), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult SaveUpdate(string PreCostingDirectMaterialId, List<UpdatedModel> ChildData)
        {
            try
            {
                for (int i = 0; i < ChildData.Count; i++)
                {
                    if (OTSBD.clsStaticInfo.dbl(ChildData[i].Allowance) < 0)
                        throw new Exception("Allowance data cannot be negative");
                    if (OTSBD.clsStaticInfo.dbl(ChildData[i].Actual) <= 0)
                        throw new Exception("Actual data cannot be less or equal zero");

                    var xy = ChildData.Where(parameter => parameter.Parameter == ChildData[i].Parameter).ToList();
                    if (xy.Count > 1)
                    {
                        throw new Exception("Duplicate Parameter");
                    }
                }
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsChild;
                con.OpenDataSetThroughAdapter("select * from [dbo].[PreCostingDirectMaterialConsumption] where  PreCostingDirectMaterialId='" + PreCostingDirectMaterialId + "'", out dsChild, false, "1");
                string PreCostingDirectMaterial = "Select * from PreCostingDirectMaterial where Id ='" + PreCostingDirectMaterialId + "'";
                con.OpenDataSetThroughAdapter(PreCostingDirectMaterial, out DataSet dsPreCostingDirectMaterial, false, "1");
                foreach (var item in ChildData)
                {
                    dsChild.Tables[0].DefaultView.RowFilter = "Id='" + item.Id + "'";
                    if (dsChild.Tables[0].DefaultView.Count == 1)
                    {
                        DataRow dr = dsChild.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["Parameter"] = item.Parameter;
                        dr["Actual"] = item.Actual;
                        dr["Allowance"] = item.Allowance;
                        dr["Parameter"] = item.Parameter;
                        dr["NoOfParameter"] = item.NoOfParameter;
                        dr["Total"] = item.Total;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                }

                dsChild.Tables[0].DefaultView.RowFilter = null;
                dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"] = CalculateConsumption(dsChild.Tables[0]);
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild, dsPreCostingDirectMaterial);

                return Json(new { Error = false, Consumption = clsStaticInfo.dbl(dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"].ToString()), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public ActionResult GetPreCostingReport(string CostingTempleteId)
        {
            try
            {
                Library.OrderManagement.Costing.CostingReport Report = new Library.OrderManagement.Costing.CostingReport();
                Report.CostingTempleteReport(CostingTempleteId);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public JsonResult SaveSubMaterial(List<Dictionary<string, object>> itemList, Dictionary<string, object> PreCDMaterial)
        {
            try
            {
                if (itemList == null)
                {
                    throw new Exception("Nothing to update");
                }
                DataSet dsMaster; DataRow drMSave; var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity; int count = 0;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "PreCostingDirectMaterialChild", out string seed_detail);
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from PreCostingDirectMaterialChild where PreCostingDirectMaterialId='" + PreCDMaterial["Id"] + "' ", out dsMaster, false, "1");

                foreach (var item in itemList)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId = '" + item["CostingItemId"] + "' ";

                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                        continue;

                    count++;
                    string pk = "MC" + seed_detail + "_" + count;
                    drMSave = dsMaster.Tables[0].NewRow();
                    drMSave["Id"] = pk;
                    drMSave["PreCostingDirectMaterialId"] = PreCDMaterial["Id"];
                    drMSave["CostingItemId"] = item["CostingItemId"];
                    drMSave["CostingMasterTemplateId"] = item["CostingMasterTemplateId"];
                    drMSave["ParentCostingItemId"] = PreCDMaterial["CostingItemId"];

                    drMSave["Consumption"] = 0;
                    drMSave["Rate"] = 0;
                    drMSave["ValueLoss"] = 0;
                    drMSave["GrossConsumption"] = 0;
                    drMSave["GrossAmount"] = 0;

                    drMSave["AddedBy"] = identity.Name;
                    drMSave["AddedDate"] = DateTime.Now;
                    drMSave["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(drMSave);

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetSubMaterialData(string MasterId)
        {
            string sql = @"SELECT  pcdmc.*,ci.UserName CostingItemName,cmt.StandardName  CostingMasterTemplate,pcdm.Id PCDMCID
                              FROM PreCostingDirectMaterialChild AS pcdmc 
                            LEFT JOIN HKP.CostingItem AS ci ON ci.Id = pcdmc.CostingItemId
                            LEFT JOIN CostingMasterTemplate AS cmt ON cmt.Id = pcdmc.CostingMasterTemplateId
                            LEFT JOIN PreCostingDirectMaterial AS pcdm ON pcdm.Id = pcdmc.PreCostingDirectMaterialId
                            where PreCostingDirectMaterialId ='" + MasterId + "'";
            return Json(new { data = _sqlRepository.GetDataCollection(sql) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdatePreCostingChild(List<Dictionary<string, object>> subMaterilaList, string MasterId)
        {
            try
            {
                DataSet dsMaster; DataRow drMSave; var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity; 
                
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from PreCostingDirectMaterialChild where PreCostingDirectMaterialId='" + MasterId + "' ", out dsMaster, false, "1");

                foreach (var item in subMaterilaList)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "Id = '" + item["Id"] + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {
                        drMSave = dsMaster.Tables[0].DefaultView[0].Row;
                        drMSave.BeginEdit();
                        drMSave["Consumption"] = clsStaticInfo.dbl(item["Consumption"]);
                        drMSave["Rate"] = clsStaticInfo.dbl(item["Rate"]);
                        drMSave["ValueLoss"] = clsStaticInfo.dbl(item["ValueLoss"]);
                        drMSave["GrossConsumption"] = clsStaticInfo.dbl(item["GrossConsumption"]);
                        drMSave["GrossAmount"] = clsStaticInfo.dbl(item["GrossAmount"]);

                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = DateTime.Now;
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        drMSave.EndEdit();
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetSubMaterialSelection(string CostingMasterTemplateId, string costingComponentId, string Segment)
        {

            string sql = @"SELECT ci.ShortName,cat.UserName AS CostingCategory, CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END) AS Selected, ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                        o.CostingMasterTemplateId,
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                            ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            LEFT OUTER JOIN hkp.CostingCategory AS cat ON cat.Id=ci.CostingCategoryId
                            LEFT join PreCostingDirectMaterial o on o.CostingItemId = ci.Id AND o.CostingMasterTemplateId='" + CostingMasterTemplateId + @"'
                            WHERE ci.CostingComponentId='" + costingComponentId + @"' AND ci.IsSubMaterial = 1
                            ORDER BY CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END), ci.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DeleteSubMaterial(string SubMaterialId)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from PreCostingDirectMaterialChild where id='" + SubMaterialId + "'");
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

    #region Models

    public class UpdatedModel
    {
        public string Id { get; set; }
        public string Parameter { get; set; }
        public string Actual { get; set; }
        public string Allowance { get; set; }
        public string NoOfParameter { get; set; }
        public decimal Total { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class PreCostingSalesExpense
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }

        public string CostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class PreCostingValueLoss
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }

        public string CostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class PreCostingProfit
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }

        public string CostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class PreCostingDirectProcess
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public string CostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string ExecutionType { get; set; }
        public decimal Value { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Sequence { get; set; }
        public string Description { get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class PreCostingOperation
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }
        public string CostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public decimal Value { get; set; }
        public string Description { get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class PreCostingDetail
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public string PreCostingVersionMasterId { get; set; }
        public double Sequence { get; set; }
        public double CostingValue { get; set; }
        public double BuyerTarget { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

    }

    //public class CostingItem
    //{
    //    public string Id { get; set; }
    //    public double Sequence { get; set; }
    //    public string Code { get; set; }
    //    public string ShortName { get; set; }
    //    public string StandardName { get; set; }
    //    public string UserName { get; set; }
    //    public string Description { get; set; }
    //    public string Remarks { get; set; }
    //    public int POIssueDeadLine { get; set; }
    //    public bool Active { get; set; }
    //    public string CostingCategoryId { get; set; }
    //    public string CostingComponentId { get; set; }
    //    public string UnitOfMeasurementId { get; set; }
    //    public decimal MinimumOfQuantity { get; set; }
    //    public decimal Wastage { get; set; }
    //    public string ProcessId { get; set; }
    //    public string BudgetMasterId { get; set; }
    //    public string ActivityId { get; set; }
    //    public string PurchaseGroupId { get; set; }
    //    public string CostingGroupId { get; set; }
    //    public string CostingItemType { get; set; }

    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //}



    public class CostingVersionDetailTemplate
    {
        public string Id { get; set; }
        public string CostingSubCategoryId { get; set; }
        public string CostingVersionMasterTemplateId { get; set; }
        public decimal Sequence { get; set; }
        public decimal CostingValue { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }


    }
    public class CostingDetailTemplate
    {
        public string Id { get; set; }
        public string CostingComponentId { get; set; }
        public string CostingVersionMasterTemplateId { get; set; }
        public decimal Sequence { get; set; }
        public decimal CostingValue { get; set; }
        public decimal BuyerTarget { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }


    }



    public class CostingBuyer
    {
        public string Id { get; set; }
        public string CostingMasterTemplateId { get; set; }
        public string BuyerId { get; set; }
        public string BuyerStyleRefNo { get; set; }
        public string OwnStyleRefNo { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

    }

    public class PreCostingDirectMaterial
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Consumption { get; set; }
        public decimal UOM { get; set; }
        public decimal Rate { get; set; }
        public decimal ValueLoss { get; set; }
        public decimal GrossConsumption { get; set; }
        public decimal GrossAmount { get; set; }
        public string CostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string SourcingType { get; set; }
        public string Usage { get; set; }
        public string POCriteria { get; set; }
        public bool IsUDApplicable { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsMandatory { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }

        public string ProcurementLevel { get; set; }
        public decimal BOQDays { get; set; }
        public string BOQCriteria { get; set; }
        public string DependentDate { get; set; }


        public decimal MinimumOfQuantity { get; set; }
        public decimal Sequence { get; set; }
        public int POIssueDeadLine { get; set; }
        public string PurchaseGroupId { get; set; }
        public string Particulars { get; set; }
        public string Remarks { get; set; }



        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    #endregion

}