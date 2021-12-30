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
using Library.OrderManagement.ProformaInvoice;

#endregion using

namespace Aplos.Areas.Commercial.Controllers
{
    public class PIPackingListController : BaseController
    {
        #region Constructor
        ProformaInvoice PI = new ProformaInvoice();
        private readonly ISqlRepository _sqlRepository;
        public PIPackingListController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
        TermsAndConditionsService tg = new TermsAndConditionsService();

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations
        public JsonResult savePIPackingList(Dictionary<string, object> PIPackingListMasterData, Dictionary<string, object> MaterialData, List<Dictionary<string, object>> DataList)
        {
            try
            {
                #region Validations
                //if (DataList.Count == 0)
                //{
                //    throw new Exception("Select from Invoice list ");
                //}
                //for (int i = 0; i < DataList.Count; i++)
                //{
                //    if (DataList[i]["PartyId"].ToString() != LcData["VendorId"].ToString())
                //    {
                //        throw new Exception("Vendor should be matched with Purchase LC for [" + DataList[i]["PartyPlantName"].ToString() + "]");
                //    }
                //    if (DataList[i]["CurrencyId"].ToString() != LcData["CurrencyId"].ToString())
                //    {
                //        throw new Exception("Currency should be matched with Purchase LC for [" + DataList[i]["PartyPlantName"].ToString() + "]");
                //    }
                //}
                #endregion
                string PackingListMasterId = PI.Save(PIPackingListMasterData, MaterialData, DataList);
                return Json(new { Error = false, Message = AplosMessage.Updated, PIPackingListMasterId = PackingListMasterId });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult create(Dictionary<string, object> HeaderData, List<Dictionary<string, object>> MaterialData, string PIMasterId, string PIVersionId)
        {
            try
            {
                if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(HeaderData["PINo"])))
                    throw new Exception("Please enter PI No.");
                if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(HeaderData["PIDate"])))
                    throw new Exception("Please select PI Date.");
                if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(HeaderData["CurrencyId"])))
                    throw new Exception("Please select Currency.");
                if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(HeaderData["BuyerId"])))
                    throw new Exception("Please select Buyer.");
                if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(HeaderData["CustomerId"])))
                    throw new Exception("Please select Customer.");
                if (MaterialData == null)
                    throw new Exception("Please enter at least one material group");


                for (int i = 0; i < MaterialData.Count; i++)
                {
                    if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(MaterialData[i]["MaterialGroupMasterId"])))
                        throw new Exception("Please select material.");
                    if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(MaterialData[i]["Description"])))
                        throw new Exception("Please enter description.");
                    if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(clsStaticInfo.dbl(MaterialData[i]["Quantity"].ToString()))))
                        throw new Exception("Please enter quantity.");
                    if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(clsStaticInfo.dbl(MaterialData[i]["Rate"].ToString()))))
                        throw new Exception("Please enter rate.");
                    if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(MaterialData[i]["DeliveryDate"])))
                        throw new Exception("Please select delivery date.");
                    if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(clsStaticInfo.dbl(MaterialData[i]["Amount"].ToString()))))
                        throw new Exception("Please enter amount.");
                }
                ConnectionManager.DAL.ConManager conPIMaster = new ConnectionManager.DAL.ConManager("1");
                conPIMaster.OpenDataSetThroughAdapter("select * from PIMaster where Id='" + HeaderData["Id"] + "'", out DataSet dsPIMaster, false, "1");
                string _Id = "";
                String _PMVersionId = "";
                string _PMMasterId = "";
                DataSet dsPIVersion = new DataSet();
                ConnectionManager.DAL.ConManager conPIVersion = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                #region data update
                if (dsPIMaster.Tables[0].Rows.Count == 0)
                {
                    //if (string.IsNullOrEmpty(HeaderData["PINo"].ToString()) == dsPIMaster.Tables[0]["PINo"].ToString())
                    //    throw new Exception("Please select Customer.");
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("PIMaster", out _Id);
                    _Id = "PI" + _Id;
                    HeaderData["Id"] = _Id;
                    PIMasterId = HeaderData["Id"].ToString();
                    AddNewRow(dsPIMaster.Tables[0], HeaderData);
                }
                else
                {
                    //PIMasterId = HeaderData["Id"].ToString();
                    EditRow(dsPIMaster.Tables[0].Rows[0], HeaderData);
                }

                conPIVersion = new ConnectionManager.DAL.ConManager("1");
                conPIVersion.OpenDataSetThroughAdapter("select * from PIVersion where Id='" + PIVersionId + "' ", out dsPIVersion, false, "1");
                if (dsPIVersion.Tables[0].Rows.Count == 0)
                {
                    string _IdV = "";
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("PIVersion", out _IdV);
                    PIVersionId = "PV" + _IdV;
                    DataRow drVersion = dsPIVersion.Tables[0].NewRow();

                    drVersion["Id"] = PIVersionId;
                    drVersion["VersionNo"] = 1;
                    drVersion["PIMasterId"] = PIMasterId;

                    drVersion["AddedBy"] = identity.Name;
                    drVersion["AddedDate"] = System.DateTime.Now.ToString();
                    drVersion["AddedFromIP"] = identity.IPAddress;
                    dsPIVersion.Tables[0].Rows.Add(drVersion);

                }
                else
                {
                    DataRow drVersion = dsPIMaster.Tables[0].Rows[0];
                    drVersion.BeginEdit();

                    drVersion["UpdatedBy"] = identity.Name;
                    drVersion["UpdatedDate"] = System.DateTime.Now.ToString();
                    drVersion["UpdatedFromIP"] = identity.IPAddress;
                    drVersion.EndEdit();
                }

                ConnectionManager.DAL.ConManager conPIMaterial = new ConnectionManager.DAL.ConManager("1");
                conPIMaterial.OpenDataSetThroughAdapter("select * from PIMaterial where  PIVersionId='" + PIVersionId + "'", out DataSet dsPIMaterial, false, "1");

                if (MaterialData == null || MaterialData.Count == 0)
                {
                    while (dsPIMaterial.Tables[0].DefaultView.Count > 0)
                    {
                        dsPIMaterial.Tables[0].DefaultView[0].Delete();
                    }
                }

                if (MaterialData != null)
                {
                    //delete
                    for (int i = 0; i < dsPIMaterial.Tables[0].Rows.Count; i++)
                    {
                        var x = MaterialData.Where(ee => clsStaticInfo.nullrecorder(ee["Id"]) == clsStaticInfo.nullrecorder(dsPIMaterial.Tables[0].Rows[i]["Id"].ToString())).FirstOrDefault();

                        if (x == null)
                        {
                            dsPIMaterial.Tables[0].Rows[i].Delete();
                        }
                    }
                    foreach (var item in MaterialData)
                    {

                        dsPIMaterial.Tables[0].DefaultView.RowFilter = "Id='" + clsStaticInfo.nullrecorder(item["Id"]) + "'";

                        DataView dv = new DataView(dsPIMaterial.Tables[0]);
                        dv.RowFilter = "Id='" + clsStaticInfo.nullrecorder(item["Id"]) + "'";
                        if (dv.Count > 0)
                        {
                            //edit
                            // _Id = VersionData[0]["Id"].ToString();
                            DataRow drmo = dv[0].Row;
                            drmo["PIMasterId"] = PIMasterId;
                            drmo["PIVersionId"] = PIVersionId;
                            EditRow(drmo, item);

                        }
                        else
                        {
                            string _materialId = "";
                            //add new
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("PIMaterial", out _materialId);
                            _materialId = "PM" + _materialId;
                            item["Id"] = _materialId;
                            item["PIMasterId"] = PIMasterId;
                            item["PIVersionId"] = PIVersionId;
                            AddNewRow(dsPIMaterial.Tables[0], item);

                        }
                    }

                }

                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsPIMaster, dsPIVersion, dsPIMaterial);
                return Json(new { Error = false, Data = HeaderData, Message = AplosMessage.Insert });

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

        #endregion -- Operations


        [HttpPost, Authorize]
        public ActionResult PIList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress,PM.RevisionNo
,C.Code Currency,B.UserName Buyer,P.UserName Customer,pv.Id PIVersionId,PV.VersionNo AS LastVersion
 FROM PIMaster PM 
LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
LEFT OUTER JOIN PIVersion AS pv ON PM.Id=pv.PIMasterId and PV.Id=(select top 1 Id from PIVersion where PIMasterId=PM.Id ORDER BY VersionNo DESC)";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult PIPackingList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT plm.Id PIPackingListMasterId,plm.[Description],plm.Remarks,FORMAT(plm.AddedDate,'dd-MMM-yyyy') AddedDate,P.Id,p2.Id PIVersionId,p2.VersionNo LastVersion
FROM PIPackingListMaster AS plm
LEFT  JOIN PIMaster AS p ON p.Id=plm.PIMasterId
LEFT JOIN PIVersion AS p2 ON P2.PIMasterId=p.Id AND  P2.Id=(select top 1 Id from PIVersion where PIMasterId=p.Id ORDER BY VersionNo DESC)
) AS TEMP WHERE " + strkey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult PIPackingMaterialList(string PIPackingMaterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT plm.Id PIPackingListMasterId,plm2.Id PIPackingListMaterialId,plm2.PIQuantity,uom.Code PIUoM
,pld.POQuantity
  FROM PIPackingListMaster AS plm
LEFT OUTER JOIN PIPackingListMaterial AS plm2 ON plm2.PIPackingListMasterId=plm.Id
LEFT OUTER JOIN PIPackingListDetail AS pld ON pld.PIPackingListMasterId=plm.Id
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON plm2.PIUoMId=uom.Id
WHERE plm.Id='" + PIPackingMaterId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetPopUp(string PIMaterial, string PIMaterialGroup)
        {

            string sql = @"SELECT convert(bit,CASE WHEN ISNULL(PMD.Id,'')<>'' THEN 1 ELSE 0 END) AS Active,  pmp.Id,PO.DocRefNo PONo,pmp.PODetailId,v.UserName Vendor,FORMAT(pod.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
,MG.UserName MaterialGroup,MM.UserName Material
,mma.StandardName Article
,cv1.UserName SKU1,cv2.UserName SKU2,cv3.UserName SKU3
,pod.TransactionQty POQty,ISNULL(pmd.Quantity,pmp.QuantityAtPIUoM) AS  DistributeQTY,pouom.code POUoM,pod.TransactionRate PORate,pod.TransactionAmount POAmount,C.code POCurrency
,MG.UserName MaterialGroup,piuom.Code PIUoM,piuom.Id PIUoMId
    FROM POMappingWithPI pmp
    LEFT OUTER JOIN PIMaterial p ON pmp.PIMaterialId=p.Id  
    LEFT OUTER JOIN SCS.UnitOfMeasurement AS piuom ON piuom.Id=p.UoMId 
   LEFT OUTER JOIN mst.MaterialGroupMaster MG ON MG.Id=p.MaterialGroupMasterId
   LEFT OUTER JOIN TRN.PurchaseOrderDetail AS pod ON pod.Id=pmp.PODetailId
    LEFT OUTER JOIN SCS.UnitOfMeasurement AS pouom ON pouom.Id=pod.TransactionUoMId 
      LEFT join PIPackingListDetail AS PMD ON pmd.PIMaterialId='" + PIMaterial + @"' AND PMD.PODetailId=pmp.PODetailId
   LEFT OUTER JOIN TRN.PurchaseOrder AS po ON po.Id=pod.InventoryReceiveId
   LEFT OUTER JOIN SCS.Currency AS c ON c.Id=po.CurrencyId
   LEFT OUTER JOIN HKP.Party AS V ON V.Id=po.PartyId
   LEFT OUTER JOIN TRN.InventoryMaterial AS IM ON IM.Id=POD.InventoryMaterialId
   
   
  LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=pod.ArticleId
   LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=mma.MaterialMasterId
   LEFT OUTER JOIN HKP.CharacteristicsValue AS cv1 ON cv1.Id=pod.FirstCharacteristicsValueId
   LEFT OUTER JOIN HKP.CharacteristicsValue AS cv2 ON cv2.Id=pod.SecondCharacteristicsValueId
   LEFT OUTER JOIN HKP.CharacteristicsValue AS cv3 ON cv3.Id=pod.ThirdCharacteristicsValueId
    WHERE p.Id='" + PIMaterial + @"' AND p.MaterialGroupMasterId='" + PIMaterialGroup + @"'";
            var PopUp = _sqlRepository.GetDataCollection(sql, null);

            return Json(new { data = PopUp }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetAllData(string PIMasterId, string VersionId,string PIPackingListMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT Id,Description,Remarks,PImasterId FROM PIPackingListMaster WHERE PImasterId='" + PIMasterId + @"' and Id='"+ PIPackingListMasterId + @"'";
            var PIPackingMasterData = _sqlRepository.GetDataCollection(sql, null);

             sql = @"SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
							,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress,PM.RevisionNo
							,C.Code Currency,B.UserName Buyer,P.UserName Customer
							 FROM PIMaster PM 
							LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
							LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
							LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
						WHERE PM.Id='" + PIMasterId + @"'";

            var PIMasterData = _sqlRepository.GetDataCollection(sql, null);

            sql = @"SELECT p.Id, p.PIMasterId, p.PIVersionId, p.Rate, p.Quantity AllocatedQty, p.Quantity, p.Amount, p.UoMId,uom.Code UoM,NULL AS MaterialGroupUOMList,
							   p.[Description],FORMAT(p.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, p.MaterialGroupMasterId,mgm.UserName AS MaterialGroup       
						  FROM PIMaterial AS p
						  LEFT JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=p.MaterialGroupMasterId
						  LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=p.UoMId
						WHERE p.PIMasterId='" + PIMasterId + @"' AND p.PIVersionId='" + VersionId + @"'";

            var PIMaterial = _sqlRepository.GetDataCollection(sql, null);

            sql = @"SELECT U.MaterialGroupMasterId,UOM.Code,UOM.Id FROM (
					SELECT mgm.Id MaterialGroupMasterId, mgm.BaseUoMId AS UOMId FROM mst.MaterialGroupMaster AS mgm
					UNION ALL
					SELECT m.MaterialGroupMasterId, m.AlternativeUoMId
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
            sql = @"SELECT * FROM PIVersion AS p WHERE p.Id=( select top 1 Id from PIVersion where PIMasterId='" + PIMasterId + @"' ORDER BY VersionNo DESC)";
            var LastVersison = _sqlRepository.GetDataCollection(sql, null);

            

            return Json(new { PIMaster = PIMasterData, VarsionData = LastVersison, ItemData = PIMaterial,PIPackingListMasterData= PIPackingMasterData }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllData2(string PIMasterId, string VersionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT Id,Description,Remarks,PImasterId FROM PIPackingListMaster WHERE PImasterId='" + PIMasterId + @"'";
            var PIPackingMasterData = _sqlRepository.GetDataCollection(sql, null);

            sql = @"SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
							,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress,PM.RevisionNo
							,C.Code Currency,B.UserName Buyer,P.UserName Customer
							 FROM PIMaster PM 
							LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
							LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
							LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
						WHERE PM.Id='" + PIMasterId + @"'";

            var PIMasterData = _sqlRepository.GetDataCollection(sql, null);

            sql = @"SELECT p.Id, p.PIMasterId, p.PIVersionId, p.Rate, p.Quantity AllocatedQty, p.Quantity, p.Amount, p.UoMId,uom.Code UoM,NULL AS MaterialGroupUOMList,
							   p.[Description],FORMAT(p.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, p.MaterialGroupMasterId,mgm.UserName AS MaterialGroup       
						  FROM PIMaterial AS p
						  LEFT JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=p.MaterialGroupMasterId
						  LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=p.UoMId
						WHERE p.PIMasterId='" + PIMasterId + @"' AND p.PIVersionId='" + VersionId + @"'";

            var PIMaterial = _sqlRepository.GetDataCollection(sql, null);

            sql = @"SELECT U.MaterialGroupMasterId,UOM.Code,UOM.Id FROM (
					SELECT mgm.Id MaterialGroupMasterId, mgm.BaseUoMId AS UOMId FROM mst.MaterialGroupMaster AS mgm
					UNION ALL
					SELECT m.MaterialGroupMasterId, m.AlternativeUoMId
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
            sql = @"SELECT * FROM PIVersion AS p WHERE p.Id=( select top 1 Id from PIVersion where PIMasterId='" + PIMasterId + @"' ORDER BY VersionNo DESC)";
            var LastVersison = _sqlRepository.GetDataCollection(sql, null);



            return Json(new { PIMaster = PIMasterData, VarsionData = LastVersison, ItemData = PIMaterial, PIPackingListMasterData = PIPackingMasterData }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllVersionData(string PIMasterId)
        {

            string sql = @"SELECT * FROM PIVersion AS pv WHERE pv.PIMasterId='" + PIMasterId + @"'";
            var VersisonList = _sqlRepository.GetDataCollection(sql, null);

            return Json(new { VarsionData = VersisonList }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetUoMList(string MaterialGroupMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT U.MaterialGroupMasterId,UOM.Code,UOM.Id FROM (
						SELECT mgm.Id MaterialGroupMasterId, mgm.BaseUoMId AS UOMId FROM mst.MaterialGroupMaster AS mgm
						UNION ALL
						SELECT m.MaterialGroupMasterId, m.AlternativeUoMId
						  FROM mst.MaterialGroupAlternativeUoM AS M
						) U
						JOIN scs.UnitOfMeasurement AS uom ON uom.Id=U.UOMId
						WHERE U.MaterialGroupMasterId='" + MaterialGroupMasterId + @"'";

            var _UOMList = _sqlRepository.GetDataCollection(sql, null);

            return Json(new { UOMList = _UOMList }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeletePI(string PIMasterId, string PIVersionId)
        {
            try
            {

                string ret = tg.DeleteProformaInvoice(PIMasterId, PIVersionId);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpPost, Authorize]
        public JsonResult NewVersion(string PIMasterId, string PIVersionId)
        {
            try
            {
                DataRow drVersion, drMaterial;
                String _PMVersionId = "";
                string _PIMaterialId = "";
                ConnectionManager.DAL.ConManager conPIVersion = new ConnectionManager.DAL.ConManager("1");
                conPIVersion.OpenDataSetThroughAdapter("select * from dbo.PIVersion where Id='" + PIVersionId + @"'AND PIMasterId='" + PIMasterId + @"'  ", out DataSet dsPIVersion, false, "1");
                if (dsPIVersion.Tables[0].Rows.Count == 0)
                    throw new Exception("Please select an existing version");

                ConnectionManager.DAL.ConManager conPINewVersion = new ConnectionManager.DAL.ConManager("1");
                conPINewVersion.OpenDataSetThroughAdapter("select * from dbo.PIVersion where 1=2 ", out DataSet dsPINewVersion, false, "1");

                ConnectionManager.DAL.ConManager conPIMaterial = new ConnectionManager.DAL.ConManager("1");
                conPIVersion.OpenDataSetThroughAdapter("select * from dbo.PIMaterial where PIMasterId='" + PIMasterId + @"'AND PIVersionId='" + PIVersionId + @"'  ", out DataSet dsPIMaterial, false, "1");
                ConnectionManager.DAL.ConManager conPINewMaterial = new ConnectionManager.DAL.ConManager("1");
                conPINewVersion.OpenDataSetThroughAdapter("select * from dbo.PIMaterial where 1=2 ", out DataSet dsPINewMaterial, false, "1");

                DataTable dtMaxVersion = _sqlRepository.GetDataTable(@"select MAX(VersionNo) AS VersionNo from PIVersion where  PIMasterId='" + PIMasterId + @"' ");

                string _VersionId = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("dbo.PIVersion", out _VersionId);
                _VersionId = "PV" + _VersionId;
                int count = (int)clsStaticInfo.dbl(dtMaxVersion.Rows[0]["VersionNo"].ToString());
                for (int i = 0; i < dsPIVersion.Tables[0].Rows.Count; i++)
                {
                    drVersion = dsPINewVersion.Tables[0].NewRow();
                    drVersion["Id"] = _VersionId;
                    drVersion["PIMasterId"] = dsPIVersion.Tables[0].Rows[i]["PIMasterId"];
                    drVersion["VersionNo"] = count + 1;
                    drVersion["VersionRefNo"] = dsPIVersion.Tables[0].Rows[i]["VersionRefNo"];
                    drVersion["VersionDate"] = dsPIVersion.Tables[0].Rows[i]["VersionDate"];
                    drVersion["AddedBy"] = identity.Name;
                    drVersion["AddedDate"] = System.DateTime.Now.ToString();
                    drVersion["AddedFromIP"] = identity.IPAddress;

                    drVersion["UpdatedBy"] = identity.Name;
                    drVersion["UpdatedDate"] = System.DateTime.Now.ToString();
                    drVersion["UpdatedFromIP"] = identity.IPAddress;
                    dsPINewVersion.Tables[0].Rows.Add(drVersion);

                }



                genid = new bplib.clsGenID();
                genid.GenID("dbo.PIMaterial", out _PIMaterialId);
                _PIMaterialId = "PM" + _PIMaterialId;
                int Mcount = 0;
                for (int i = 0; i < dsPIMaterial.Tables[0].Rows.Count; i++)
                {
                    drMaterial = dsPINewMaterial.Tables[0].NewRow();
                    Mcount++;
                    drMaterial["Id"] = _PIMaterialId + "-" + Mcount;
                    drMaterial["PIMasterId"] = dsPIMaterial.Tables[0].Rows[i]["PIMasterId"];
                    drMaterial["PIVersionId"] = _VersionId;
                    drMaterial["Rate"] = dsPIMaterial.Tables[0].Rows[i]["Rate"];
                    drMaterial["Quantity"] = dsPIMaterial.Tables[0].Rows[i]["Quantity"];
                    drMaterial["Amount"] = dsPIMaterial.Tables[0].Rows[i]["Amount"];
                    drMaterial["UoMId"] = dsPIMaterial.Tables[0].Rows[i]["UoMId"];
                    drMaterial["Description"] = dsPIMaterial.Tables[0].Rows[i]["Description"];
                    drMaterial["DeliveryDate"] = dsPIMaterial.Tables[0].Rows[i]["DeliveryDate"];
                    drMaterial["MaterialGroupMasterId"] = dsPIMaterial.Tables[0].Rows[i]["MaterialGroupMasterId"];
                    drMaterial["AddedBy"] = identity.Name;
                    drMaterial["AddedDate"] = System.DateTime.Now.ToString();
                    drMaterial["AddedFromIP"] = identity.IPAddress;

                    drMaterial["UpdatedBy"] = identity.Name;
                    drMaterial["UpdatedDate"] = System.DateTime.Now.ToString();
                    drMaterial["UpdatedFromIP"] = identity.IPAddress;
                    dsPINewMaterial.Tables[0].Rows.Add(drMaterial);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsPINewVersion, dsPINewMaterial);

                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetMaterialGroupList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT mgm.Id,mgm.UserName AS MaterialGroup 
                                                        FROM mst.MaterialGroupMaster AS mgm WHERE mgm.[Active]=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

    }
}