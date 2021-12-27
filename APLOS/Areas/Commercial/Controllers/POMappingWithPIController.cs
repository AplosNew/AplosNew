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

        //private readonly IFabricRollMasterService _fabricRollMasterService;
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
        public JsonResult Save(string PIMaterial, List<Dictionary<string, object>> POList)
        {
            try
            {
                

                #region data update
  

                ConnectionManager.DAL.ConManager conPIMaterial = new ConnectionManager.DAL.ConManager("1");
                conPIMaterial.OpenDataSetThroughAdapter("select * from POMappingWithPI", out DataSet dsPIMaterial, false, "1");

                foreach (var item in POList)
                {
                    dsPIMaterial.Tables[0].DefaultView.RowFilter = "Id='" + clsStaticInfo.nullrecorder(item["Id"]) + "'";

                    DataView dv = new DataView(dsPIMaterial.Tables[0]);
                    dv.RowFilter = "Id='" + clsStaticInfo.nullrecorder(item["Id"]) + "'";
                    if (dv.Count > 0)
                    {
                        //edit
                        // _Id = VersionData[0]["Id"].ToString();
                        DataRow drmo = dv[0].Row;

                        drmo["PIMaterialID"] = item["PIMaterialID"];
                        drmo["PODetailId"] = item["PODetailId"];
                        drmo["QuantityAtPIUoM"] = item["QuantityAtPIUoM"];
                        drmo["PIUoMId"] = item["PIUoMId"];
                        drmo["POQuantity"] = item["POQuantity"];
                        drmo["POUoM"] = item["POUoM"];

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
                        AddNewRow(dsPIMaterial.Tables[0], item);

                    }
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
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress,PM.RevisionNo
,C.Code Currency,B.UserName Buyer,P.UserName Customer,pv.Id PIVersionId,PV.VersionNo AS LastVersion
 FROM PIMaster PM 
LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
LEFT OUTER JOIN PIVersion AS pv ON PM.Id=pv.PIMasterId and PV.Id=(select top 1 Id from PIVersion where PIMasterId=PM.Id ORDER BY VersionNo DESC)
--ORDER BY PM.PIDate DESC
) AS TEMP WHERE " + strkey + "ORDER BY TEMP.PIDate DESC";

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

            sql = @"SELECT p.Id, p.PIMasterId, p.PIVersionId, p.Rate, p.Quantity, p.Amount, p.UoMId,NULL AS MaterialGroupUOMList,
							   p.[Description], p.DeliveryDate, p.MaterialGroupMasterId,mgm.UserName AS MaterialGroup       
						  FROM PIMaterial AS p
						  LEFT JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=p.MaterialGroupMasterId
						WHERE p.PIMasterId='" + PIMasterId + @"' AND p.PIVersionId='" + VersionId + @"'";

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

        //public ActionResult DeleteMaterial(string id)
        //{
        //    try
        //    {

        //        string ret = tg.DeletePIMaterial(id);

        //        if (ret == "Success")
        //        {
        //            return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

        //    }


        //}
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
        public ActionResult GetPODetailsData(string MaterialGroupMasterId,string PIMaterialId)
        {

            string sql = @"
                            select convert(bit,case when isnull(POMPI.Id,'')<>'' then 1 else 0 END) AS Saved, POMPI.Id,POD.Id PODetailId
                            ,POD.TransactionRate PORate,POD.TransactionQty POQuantity,POD.TransactionUoMId POUoM

                            from TRN.PurchaseOrderDetail POD
							left join PIMaterial PIM on pim.Id='" + PIMaterialId + @"'
							left join POMappingWithPI POMPI on POMPI.PIMaterialId=PIM.Id and pod.Id=POMPI.PODetailId
                            left join mst.MaterialMasterArticle MMA on mma.id=POd.ArticleId
                            left join MST.MaterialMaster MM on MM.Id=MMA.MaterialMasterId

                            where MM.MaterialGroupMasterId = '"+ MaterialGroupMasterId + @"' and POD.Id not in (select PODetailId from POMappingWithPI where PIMaterialId<>'"+ PIMaterialId + @"')";
            var POList = _sqlRepository.GetDataCollection(sql, null);

            return Json(new { Polist = POList }, JsonRequestBehavior.AllowGet);
        }
    }
}