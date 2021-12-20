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

#endregion using

namespace Aplos.Areas.Commercial.Controllers
{
    public class ProformaInvoiceController : BaseController
    {
        #region -- Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;
        private SqlRepository _sqlRepository = new SqlRepository();
        public ProformaInvoiceController(IFabricRollMasterService fabricRollMasterService)
        {
            _fabricRollMasterService = fabricRollMasterService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string paidHours)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fabricRollMasterService.Query(parameters, identity.CompanyGroupId, paidHours, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult create(Dictionary<string, object> HeaderData, Dictionary<string, object> VersionData, List<Dictionary<string, object>> MaterialData)
        {
            try
            {
                //if (string.IsNullOrEmpty(VersionData["Id"].ToString()) && string.IsNullOrEmpty(HeaderData["Id"].ToString()))
                //    throw new Exception("Select version.");

                String _PMVersionId = "";
                string _PMMasterId = "";
                ConnectionManager.DAL.ConManager conPIMaster = new ConnectionManager.DAL.ConManager("1");
                conPIMaster.OpenDataSetThroughAdapter("select * from dbo.PIMaster where Id='" + HeaderData["Id"] + "'", out DataSet dsPIMaster, false, "1");
                string _Id = "";
                DataSet dsPIVersion = new DataSet();
                ConnectionManager.DAL.ConManager conPIVersion = new ConnectionManager.DAL.ConManager("1");
                #region data update
                if (dsPIMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("dbo.PIMaster", out _Id);
                    _Id = "PI" + _Id;
                    HeaderData["Id"] = _Id;
                    _PMMasterId = HeaderData["Id"].ToString();
                    AddNewRow(dsPIMaster.Tables[0], HeaderData);


                    conPIVersion = new ConnectionManager.DAL.ConManager("1");
                    conPIVersion.OpenDataSetThroughAdapter("select * from dbo.PIVersion where PIMasterId='" + HeaderData["Id"] + "' AND Id='" + VersionData["Id"] + "'", out dsPIVersion, false, "1");
                    if (dsPIVersion.Tables[0].Rows.Count == 0)
                    {
                        genid = new bplib.clsGenID();
                        genid.GenID("dbo.PIVersion", out _Id);
                        _Id = "PV" + _Id;
                        VersionData["Id"] = _Id;
                        VersionData["VersionNo"] = 1;
                        VersionData["PIMasterId"] = _PMMasterId;
                        _PMVersionId = VersionData["Id"].ToString();
                        AddNewRow(dsPIVersion.Tables[0], VersionData);
                    }
                }
                else
                {
                    _Id = HeaderData["Id"].ToString();
                    EditRow(dsPIMaster.Tables[0].Rows[0], HeaderData);

                    conPIVersion = new ConnectionManager.DAL.ConManager("1");
                    conPIVersion.OpenDataSetThroughAdapter("select * from dbo.PIVersion where PIMasterId='" + HeaderData["Id"] + "' AND Id='" + VersionData["Id"] + "' ", out dsPIVersion, false, "1");
                    if (dsPIMaster.Tables[0].Rows.Count > 0)
                    {
                        EditRow(dsPIVersion.Tables[0].Rows[0], VersionData);
                    }
                }

                ConnectionManager.DAL.ConManager conPIMaterial = new ConnectionManager.DAL.ConManager("1");
                conPIMaterial.OpenDataSetThroughAdapter("select * from dbo.PIMaterial where PIMasterId='" + HeaderData["Id"] + "' AND PIVersionId='" + VersionData["Id"] + "'", out DataSet dsPIMaterial, false, "1");

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
                            _Id = VersionData["Id"].ToString();
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                        else
                        {
                            //add new
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("dbo.PIMaterial", out _Id);
                            _Id = "PM" + _Id;
                            item["Id"] = _Id;
                            item["PIMasterId"] = _PMMasterId;
                            item["PIVersionId"] = _PMVersionId;
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
,C.Code Currency,B.UserName Buyer,P.UserName Customer,pv.Id PIVersionId
 FROM PIMaster PM 
LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
LEFT OUTER JOIN PIVersion AS pv ON PM.Id=pv.PIMasterId
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
            sql = @" SELECT * FROM PIVersion AS pv WHERE pv.PIMasterId='" + PIMasterId + @"'";
            var VersisonList = _sqlRepository.GetDataCollection(sql, null);

            return Json(new { PIMaster = PIMasterData, VarsionData = VersisonList, ItemData = PIMaterial }, JsonRequestBehavior.AllowGet);
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

        [HttpPost, Authorize]
        public JsonResult NewVersion(string PIMasterId, string PIVersionId)
        {
            try
            {

                DataTable dtVersion, dtMaterial;
                DataRow drVersion, drMaterial;
                String _PMVersionId = "";
                string _PIMaterialId = "";
                ConnectionManager.DAL.ConManager conPIVersion = new ConnectionManager.DAL.ConManager("1");
                conPIVersion.OpenDataSetThroughAdapter("select * from dbo.PIVersion where Id='" + PIVersionId + @"'AND PIMasterId='" + PIMasterId + @"'  ", out DataSet dsPIVersion, false, "1");
                ConnectionManager.DAL.ConManager conPINewVersion = new ConnectionManager.DAL.ConManager("1");
                conPINewVersion.OpenDataSetThroughAdapter("select * from dbo.PIVersion where 1=2 ", out DataSet dsPINewVersion, false, "1");

                ConnectionManager.DAL.ConManager conPIMaterial = new ConnectionManager.DAL.ConManager("1");
                conPIVersion.OpenDataSetThroughAdapter("select * from dbo.PIMaterial where PIMasterId='" + PIMasterId + @"'AND PIVersionId='" + PIVersionId + @"'  ", out DataSet dsPIMaterial, false, "1");
                ConnectionManager.DAL.ConManager conPINewMaterial = new ConnectionManager.DAL.ConManager("1");
                conPINewVersion.OpenDataSetThroughAdapter("select * from dbo.PIMaterial where 1=2 ", out DataSet dsPINewMaterial, false, "1");

                dtMaterial = dsPINewMaterial.Tables[0];
                dtVersion = dsPINewVersion.Tables[0];
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (dsPIVersion.Tables[0].Rows.Count > 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("dbo.PIVersion", out _Id);
                    _Id = "PV" + _Id;
                    int count = 0;
                    for (int i = 0; i < dsPIVersion.Tables[0].Rows.Count; i++)
                    {
                        drVersion = dtVersion.NewRow();
                        count++;
                        drVersion["Id"] = _Id + count;
                        drVersion["PIMasterId"] = dsPIVersion.Tables[0].Rows[i]["PIMasterId"];
                        drVersion["VersionNo"] = dsPIVersion.Tables[0].Rows[i]["VersionNo"];
                        drVersion["VersionRefNo"] = dsPIVersion.Tables[0].Rows[i]["VersionRefNo"];
                        drVersion["VersionDate"] = dsPIVersion.Tables[0].Rows[i]["VersionDate"];
                        drVersion["AddedBy"] = identity.Name;
                        drVersion["AddedDate"] = System.DateTime.Now.ToString();
                        drVersion["AddedFromIP"] = identity.IPAddress;
                        dtVersion.Rows.Add(drVersion);

                    }


                    if (dsPIMaterial.Tables[0].Rows.Count > 0)
                    {
                        genid = new bplib.clsGenID();
                        genid.GenID("dbo.PIMaterial", out _PIMaterialId);
                        _PIMaterialId = "PM" + _Id;
                        int Mcount = 0;
                        for (int i = 0; i < dsPIMaterial.Tables[0].Rows.Count; i++)
                        {
                            drMaterial = dtMaterial.NewRow();
                            Mcount++;
                            drMaterial["Id"] = _Id + Mcount;
                            drMaterial["PIMasterId"] = dsPIMaterial.Tables[0].Rows[i]["PIMasterId"];
                            drMaterial["VersionNo"] = dsPIMaterial.Tables[0].Rows[i]["VersionNo"];
                            drMaterial["VersionRefNo"] = dsPIMaterial.Tables[0].Rows[i]["VersionRefNo"];
                            drMaterial["VersionDate"] = dsPIMaterial.Tables[0].Rows[i]["VersionDate"];
                            drMaterial["AddedBy"] = identity.Name;
                            drMaterial["AddedDate"] = System.DateTime.Now.ToString();
                            drMaterial["AddedFromIP"] = identity.IPAddress;
                            dtMaterial.Rows.Add(drMaterial);
                        }
                        return null;
                    }

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsPINewVersion, dsPINewMaterial);

                    return Json(new { Error = false, Message = AplosMessage.Insert });
                    
                }
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

    }
}