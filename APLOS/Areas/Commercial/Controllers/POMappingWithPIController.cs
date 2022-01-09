#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Web.Script.Serialization;
using System;
using Newtonsoft.Json;
using Library.Data;
using System.IO;
using Library.HumanResource.Attendance.Manual;
using Library.Service.Helpers;
using System.Data;
using Library.OrderManagement.FabricRollClass;
using System.Linq;
using Library.Security.Core;
using Library.OrderManagement.TermsAndConditions;

#endregion using

namespace Aplos.Areas.Commercial.Controllers
{
    public class POMappingWithPIController : BaseController
    {
        #region -- Constructor

        Dictionary<string, List<Factors>> MaterialGroupMasterUOMList = new Dictionary<string, List<Factors>>();
        private SqlRepository _sqlRepository = new SqlRepository();
        public POMappingWithPIController()
        {
            //_sqlRepository = fabricRollMasterService;
        }

        #endregion -- Constructor
        TermsAndConditionsService tg = new TermsAndConditionsService();

        #region Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

      

        [HttpPost]
        public JsonResult Save(Dictionary<string, object> PIMaterial,List<Dictionary<string, object>> POList)
        {
            try
            {


                #region data update
                DataSet dsConversion;

                ConnectionManager.DAL.ConManager conPIMaterial = new ConnectionManager.DAL.ConManager("1");
                conPIMaterial.OpenDataSetThroughAdapter("select * from POMappingWithPI where PIMaterialID ='" + PIMaterial["Id"] + @"' ", out DataSet dsPIMaterial, false, "1");
                
                if (POList == null || POList.Count == 0)
                {
                    while (dsPIMaterial.Tables[0].DefaultView.Count > 0)
                        dsPIMaterial.Tables[0].DefaultView[0].Delete();
                }
                
                if ( POList != null)
                {
                    GetAllUOMConversionData();
                    for (int i = 0; i < dsPIMaterial.Tables[0].Rows.Count; i++)
                    {
                        var item = POList.Where(x => x["PODetailId"].ToString() == dsPIMaterial.Tables[0].Rows[i]["PODetailId"].ToString()).FirstOrDefault();
                        if (item == null || item.Count == 0)
                        {
                            dsPIMaterial.Tables[0].Rows[i].Delete();
                        }
                    }
                    foreach (var item in POList)
                    {
                        GetUOMConversionAtMaterialGroupMasterData(item["MaterialGroupMasterId"].ToString(), out dsConversion);

                        double conversiongroupListData = ConvertUoM(item["MaterialGroupMasterId"].ToString(), item["POUoMId"].ToString(), item["PIUoMId"].ToString(), Convert.ToDouble(item["POQuantity"]));
                        decimal BaseQty = Convert.ToDecimal(conversiongroupListData);


                        dsPIMaterial.Tables[0].DefaultView.RowFilter = "PODetailId='" + clsStaticInfo.nullrecorder(item["PODetailId"]) + "'";

                        DataView dv = new DataView(dsPIMaterial.Tables[0]);
                        dv.RowFilter = "PODetailId='" + clsStaticInfo.nullrecorder(item["PODetailId"]) + "'";
                        if (dv.Count > 0)
                        {
                            //edit
                            // _Id = VersionData[0]["Id"].ToString();
                            DataRow drmo = dv[0].Row;

                            drmo["PIMaterialID"] = item["PIMaterialId"];
                            drmo["PODetailId"] = item["PODetailId"];
                            drmo["QuantityAtPIUoM"] = BaseQty;
                            drmo["PIUoMId"] = item["PIUoMId"];
                            drmo["POQuantity"] = item["POQuantity"];
                            drmo["POUoMId"] = item["POUoMId"];

                            //EditRow(drmo, item);

                        }
                        else
                        {
                            string _materialId = "";
                            //add new
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("POMappingWithPI", out _materialId);
                            _materialId = "PM" + _materialId;
                            item["Id"] = _materialId;
                            item["PIMaterialID"] = PIMaterial["Id"];
                            item["QuantityAtPIUoM"] = BaseQty;
                            AddNewRow(dsPIMaterial.Tables[0], item);

                        }
                    }
                }
                else
                {

                }
                   

                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsPIMaterial);
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Error = true, Message = ex.Message });
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
      

        #endregion -- Operations

        [HttpPost, Authorize]
        public ActionResult PIList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
                            ,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress,PM.RevisionNo
                            ,C.Code Currency,B.UserName Buyer,P.UserName Customer,pv.Id PIVersionId,PV.VersionNo AS LastVersionNo
							,isnull(PIM.Amount,0) Amount,isnull(PIM.POTaggedAmount,0) POTaggedAmount

                             FROM PIMaster PM 
							 left outer join 
							 (select sum(Amount) as Amount,sum(POD.TransactionAmount) POTaggedAmount,PIMasterId from PIMaterial PM
							 LEFT JOIN POMappingWithPI MAP ON MAP.PIMaterialID=PM.Id
							 left outer join TRN.PurchaseOrderDetail POD on POD.Id=MAP.PODetailId
							 group by PIMasterId
							 ) as PIM on PIM.PIMasterId=PM.Id
                            LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
                            LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
                            LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
                            LEFT OUTER JOIN PIVersion AS pv ON PM.Id=pv.PIMasterId and PV.Id=(select top 1 Id from PIVersion where PIMasterId=PM.Id ORDER BY VersionNo DESC)";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllData(string PIMasterId, string VersionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
							,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress,PM.RevisionNo
							,C.Code Currency,B.UserName Buyer,P.UserName Customer
							 FROM PIMaster PM 
							LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
							LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
							LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
						WHERE PM.Id='" + PIMasterId + @"'";

            var PIMasterData = _sqlRepository.GetDataCollection(sql, null);

            sql = @"SELECT p.Id, p.PIMasterId, p.PIVersionId, p.Rate, p.Quantity, p.Amount, p.UoMId,UoM.UserName AS MaterialGroupUOM,
							   p.[Description],FORMAT(p.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, p.MaterialGroupMasterId
							   ,mgm.UserName AS MaterialGroup,SUM(isnull(PD.TransactionAmount,0)) POTaggedAmount,SUM(isnull(MAP.QuantityAtPIUoM,0)) POTaggedQuantity
							   --,SUM(isnull(MAP.POQuantity,0)) POTaggedQuantity
							  FROM PIMaterial AS p
							  LEFT JOIN POMappingWithPI MAP ON MAP.PIMaterialID=P.Id
							  LEFT JOIN TRN.PurchaseOrderDetail PD ON PD.Id=MAP.PODetailId
							 LEFT JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=p.MaterialGroupMasterId
							 left join SCS.UnitOfMeasurement AS UoM on UoM.Id=p.UoMId
							 WHERE p.PIMasterId='" + PIMasterId + @"' AND p.PIVersionId='" + VersionId + @"'
							 GROUP BY p.Id, p.PIMasterId, p.PIVersionId, p.Rate, p.Quantity, p.Amount, p.UoMId,UoM.UserName
							 ,p.[Description],p.DeliveryDate, p.MaterialGroupMasterId,mgm.UserName";
            var PIMaterial = _sqlRepository.GetDataCollection(sql, null);

            sql = @"SELECT U.MaterialGroupMasterId,U.MaterialGroup,UOM.Code,UOM.Id FROM (
					SELECT mgm.Id MaterialGroupMasterId,mgm.UserName MaterialGroup, mgm.BaseUoMId AS UOMId FROM mst.MaterialGroupMaster AS mgm
					UNION ALL
					SELECT m.MaterialGroupMasterId,''MaterialGroup, m.AlternativeUoMId
					  FROM mst.MaterialGroupAlternativeUoM AS M
					) U
					JOIN scs.UnitOfMeasurement AS uom ON uom.Id=U.UOMId
					WHERE U.MaterialGroupMasterId IN (
						SELECT P.MaterialGroupMasterId FROM PIMaterial P WHERE p.PIMasterId='" + PIMasterId + @"' AND p.PIVersionId='" + VersionId + @"'
					)";
            var UOMList = _sqlRepository.GetDataCollection(sql, null);
            for (int i = 0; i < PIMaterial.Count; i++)
            {
                var U = UOMList.Where(w => w["MaterialGroupMasterId"] == PIMaterial[i]["MaterialGroupMasterId"].ToString()).ToList();
                PIMaterial[i]["MaterialGroupUOMList"] = U;
            }
            sql = @"SELECT * FROM PIVersion AS pv WHERE pv.PIMasterId='" + PIMasterId + @"'";
            var VersisonList = _sqlRepository.GetDataCollection(sql, null);

            return Json(new { PIMaster = PIMasterData, VarsionData = VersisonList, ItemData = PIMaterial }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllVersionData(string PIMasterId)
        {

            string sql = @"SELECT * FROM PIVersion AS pv WHERE pv.PIMasterId='" + PIMasterId + @"'";
            var VersisonList = _sqlRepository.GetDataCollection(sql, null);

            return Json(new { VarsionData = VersisonList }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet, Authorize]
        public ActionResult GetPODetailsData(string MaterialGroupMasterId,string PIMaterialId)
        {

            string sql = @"select  convert(bit,case when isnull(POMPI.Id,'')<>'' then 1 else 0 END) AS [check], 
                                    convert(bit,case when isnull(PACK.PODetailId,'')<>'' then 1 else 0 END) AS HasPackingList,
                                    POMPI.Id,PIM.Id PIMaterialId
							        ,POD.Id PODetailId,v.UserName Vendor,FORMAT(pod.DeliveryDate,'dd-MMM-yyyy') DeliveryDate,MG.Id MaterialGroupMasterId,MG.UserName MaterialGroup
							        ,MM.UserName Material,mma.StandardName Article,cv1.UserName SKU1,cv2.UserName SKU2,cv3.UserName SKU3
							        ,ISNULL(POD.TransactionQty,0) POQuantity,POD.TransactionUoMId POUoMId,pouom.UserName POUoM,POD.TransactionRate PORate
							        ,pod.TransactionAmount POAmount,C.code POCurrency,PIM.UoMId PIUoMId,ISNULL(POMPI.QuantityAtPIUoM,0)QuantityAtPIUoM,piuom.UserName PIUoM
                                    ,pack.PackedQty,POMPI.QuantityAtPIUoM-isnull(pack.PackedQty,0) AS BalanceToPack

                                    from TRN.PurchaseOrderDetail POD
							        left join TRN.PurchaseOrder AS po ON po.Id=pod.InventoryReceiveId
							        left join HKP.Party AS V ON V.Id=po.PartyId
							        left join SCS.Currency AS c ON c.Id=po.CurrencyId
							        left join PIMaterial PIM on pim.Id='" + PIMaterialId + @"'
							        left join SCS.UnitOfMeasurement AS piuom ON piuom.Id=PIM.UoMId
							        left join mst.MaterialGroupMaster MG ON MG.Id=PIM.MaterialGroupMasterId
							        left join TRN.InventoryMaterial AS IM ON IM.Id=POD.InventoryMaterialId
							        left join HKP.CharacteristicsValue AS cv1 ON cv1.Id=IM.FirstCharacteristicsValueId
							        left join HKP.CharacteristicsValue AS cv2 ON cv2.Id=IM.SecondCharacteristicsValueId
							        left join HKP.CharacteristicsValue AS cv3 ON cv3.Id=IM.ThirdCharacteristicsValueId
							        left join SCS.UnitOfMeasurement AS pouom ON pouom.Id=pod.TransactionUoMId 
							        left join POMappingWithPI POMPI on POMPI.PIMaterialId=PIM.Id and pod.Id=POMPI.PODetailId
							        left join (
							        select PIMaterialId,PODetailId,sum(Quantity) AS PackedQty from PIPackingListDetail group by PIMaterialId,	PODetailId
							        ) AS PACK on pack.PIMaterialId=POMPI.PIMaterialID and pack.PODetailId=POMPI.PODetailId
                                    left join mst.MaterialMasterArticle MMA on mma.id=POd.ArticleId
                                    left join MST.MaterialMaster MM on MM.Id=MMA.MaterialMasterId

                                    where MM.MaterialGroupMasterId = '" + MaterialGroupMasterId + @"' and POD.Id not in (select PODetailId from POMappingWithPI where PIMaterialId<>'" + PIMaterialId + @"')";
            
            var POList = _sqlRepository.GetDataCollection(sql, null);

            return Json(new { Polist = POList }, JsonRequestBehavior.AllowGet);
        }

        #region UOMConversionAtMaterialGroupMaster

        public void GetUOMConversionAtMaterialGroupMasterData(string MaterialGroupMasterId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT UOM.MaterialGroupMasterId, UOM.AlternativeUOMId, UOM.BaseUOMId,
       CONVERT(decimal(18,8),UOM.AltToBaseUOMFactor) AS AltToBaseUOMFactor,  convert(decimal(18,8),UOM.BaseToAltUOMFactor) AS BaseToAltUOMFactor, 
       UOM.UOMType FROM (
                    SELECT mm.Id AS MaterialGroupMasterId, mm.BaseUOMId AS AlternativeUOMId,mm.BaseUOMId,
                    1 AS AltToBaseUOMFactor,1 AS BaseToAltUOMFactor,
                    'BASE' AS UOMType FROM mst.MaterialGroupMaster AS mm
					WHERE mm.Id='" + MaterialGroupMasterId + @"'
                    UNION ALL
                    SELECT mmau.MaterialGroupMasterId, mmau.AlternativeUOMId, mmau.BaseUOMId,
                    mmau.BaseUOMFactor/mmau.AlternativeUOMFactor AS AltToBaseUOMFactor,mmau.AlternativeUOMFactor/mmau.BaseUOMFactor AS AltToBaseUOMFactor,
                    'ALT' AS UOMType FROM  mst.MaterialGroupAlternativeUoM AS mmau
					WHERE mmau.MaterialGroupMasterId='" + MaterialGroupMasterId + @"'
                    ) AS UOM
                    ORDER BY UOM.MaterialGroupMasterId";

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

        public double ConvertUoM(string MaterialGroupMasterId, string FromUOM, string ToUOM, double Value)
        {

            //If source and target uom are same, no need conversion
            if (FromUOM == ToUOM)
                return Value;

            List<Factors> AltUOM = MaterialGroupMasterUOMList[MaterialGroupMasterId].Where(ee => ee.AlternativeUOMId == FromUOM).ToList();

            //means, need to convert the source UOM to target UOM
            if (AltUOM.Count > 0)
            {
                //converting to base
                Value = Value * AltUOM[0].AltToBaseUOMFactor;
                //and if target uom is also base;no need to further conversion
                if (AltUOM[0].BaseUOMId == ToUOM)
                    return Value;//because we have already converted the source value to base UOM. no need to further conversion

                //second step conversion from base value to alternative target value
                AltUOM = MaterialGroupMasterUOMList[MaterialGroupMasterId].Where(ee => ee.AlternativeUOMId == ToUOM).ToList();
                if (AltUOM.Count > 0)
                {
                    //convert base value to alternative uom using basetoaltuomfactor
                    return Value = Value * AltUOM[0].BaseToAltUOMFactor;
                }
                else
                {
                    return 0;
                }
            }

            return 0;
        }

        public void GetUOMConversionData(string MaterialGroupMasterId)
        {
           // _sqlRepository = new SqlRepository();

            MakeMaterialCluster(_sqlRepository.GetModelCollection<Factors>(@"SELECT UOM.MaterialGroupMasterId, UOM.AlternativeUOMId, UOM.BaseUOMId,
       CONVERT(decimal(18,8),UOM.AltToBaseUOMFactor) AS AltToBaseUOMFactor,  convert(decimal(18,8),UOM.BaseToAltUOMFactor) AS BaseToAltUOMFactor, 
       UOM.UOMType FROM (
                    SELECT mm.Id AS MaterialGroupMasterId, mm.BaseUOMId AS AlternativeUOMId,mm.BaseUOMId,
                    1 AS AltToBaseUOMFactor,1 AS BaseToAltUOMFactor,
                    'BASE' AS UOMType FROM mst.MaterialGroupMaster AS mm
					WHERE mm.Id='" + MaterialGroupMasterId + @"'
                    UNION ALL
                    SELECT mmau.MaterialGroupMasterId, mmau.AlternativeUOMId, mmau.BaseUOMId,
                    mmau.BaseUOMFactor/mmau.AlternativeUOMFactor AS AltToBaseUOMFactor,mmau.AlternativeUOMFactor/mmau.BaseUOMFactor AS AltToBaseUOMFactor,
                    'ALT' AS UOMType FROM  mst.MaterialGroupAlternativeUoM AS mmau
					WHERE mmau.MaterialGroupMasterId='" + MaterialGroupMasterId + @"'
                    ) AS UOM
                    ORDER BY UOM.MaterialGroupMasterId"));
        }

        public void GetAllUOMConversionData()
        {
            // _sqlRepository = new SqlRepository();

            MakeMaterialCluster(_sqlRepository.GetModelCollection<Factors>(@"SELECT UOM.MaterialGroupMasterId, UOM.AlternativeUOMId, UOM.BaseUOMId,
       CONVERT(decimal(18,8),UOM.AltToBaseUOMFactor) AS AltToBaseUOMFactor,  convert(decimal(18,8),UOM.BaseToAltUOMFactor) AS BaseToAltUOMFactor, 
       UOM.UOMType FROM (
                    SELECT mm.Id AS MaterialGroupMasterId, mm.BaseUOMId AS AlternativeUOMId,mm.BaseUOMId,
                    1 AS AltToBaseUOMFactor,1 AS BaseToAltUOMFactor,
                    'BASE' AS UOMType FROM mst.MaterialGroupMaster AS mm
                    UNION ALL
                    SELECT mmau.MaterialGroupMasterId, mmau.AlternativeUOMId, mmau.BaseUOMId,
                    mmau.BaseUOMFactor/mmau.AlternativeUOMFactor AS AltToBaseUOMFactor,mmau.AlternativeUOMFactor/mmau.BaseUOMFactor AS AltToBaseUOMFactor,
                    'ALT' AS UOMType FROM  mst.MaterialGroupAlternativeUoM AS mmau
                    ) AS UOM
                    ORDER BY UOM.MaterialGroupMasterId"));
        }

        private void MakeMaterialCluster(List<Factors> UOMData)
        {
            MaterialGroupMasterUOMList = new Dictionary<string, List<Factors>>();
            List<Factors> _list = new List<Factors>();
            string MaterialGroupMasterId = "";
            foreach (Factors item in UOMData)
            {
                if (MaterialGroupMasterId != item.MaterialGroupMasterId)
                {
                    _list = new List<Factors>();
                    MaterialGroupMasterUOMList.Add(item.MaterialGroupMasterId, _list);
                }

                _list.Add(item);

                MaterialGroupMasterId = item.MaterialGroupMasterId;
            }
        }

        #endregion
    }

    class Factors : BaseModel
    {

        public string MaterialGroupMasterId { get; set; }
        public string AlternativeUOMId { get; set; }
        public string BaseUOMId { get; set; }
        public double AltToBaseUOMFactor { get; set; }
        public double BaseToAltUOMFactor { get; set; }
        public string UOMType { get; set; }
    }
}