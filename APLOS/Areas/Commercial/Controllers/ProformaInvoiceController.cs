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
using Library.Service.Logs;
using System.Reflection;
using Library.OrderManagement.ShipmentControl;
using Library.OrderManagement.ProformaInvoice;
#endregion using

namespace Aplos.Areas.Commercial.Controllers
{
    public class ProformaInvoiceController : BaseController
    {
        #region -- Constructor
        private readonly ISqlRepository _sqlRepository;

        public ProformaInvoiceController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion -- Constructor
        TermsAndConditionsService tg = new TermsAndConditionsService();
        ProformaInvoice PI = new ProformaInvoice();
        bplib.clsGenID objGenID = new bplib.clsGenID();
        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        //[HttpGet, Authorize]
        //public JsonResult GetList(GridParameter parameters, string paidHours)
        //{
        //    CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_fabricRollMasterService.Query(parameters, identity.CompanyGroupId, paidHours, identity.PlantId), JsonRequestBehavior.AllowGet);
        //}

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
                    if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(MaterialData[i]["UoMId"])))
                        throw new Exception("Please select UoM.");
                    if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(MaterialData[i]["DeliveryDate"])))
                        throw new Exception("Please select delivery date.");
                    if (Convert.ToDateTime(HeaderData["PIDate"]) < Convert.ToDateTime(MaterialData[i]["DeliveryDate"].ToString()))
                        throw new Exception("Delivery date must less than PI date.");
                    if (string.IsNullOrEmpty(clsStaticInfo.nullrecorder(clsStaticInfo.dbl(MaterialData[i]["Amount"].ToString()))))
                        throw new Exception("Please enter amount.");
                }
                ConnectionManager.DAL.ConManager conPIMaster = new ConnectionManager.DAL.ConManager("1");

                conPIMaster.OpenDataSetThroughAdapter("select * from PIMaster where RefNo='" + HeaderData["RefNo"] + "' AND Id<>'" + HeaderData["Id"] + @"' ", out DataSet dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                    throw new Exception("PI Ref No. already exists.");

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
                    PIVersionId = "PV" + "-" + _IdV;
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
                if (!string.IsNullOrEmpty(HeaderData["TermsAndConditionsId"].ToString()))
                {
                    SaveTermsData(HeaderData["TermsAndConditionsId"].ToString(), HeaderData["Id"].ToString());
                }
                return Json(new { Error = false, Data = HeaderData, Message = AplosMessage.Insert });


            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Error = true, Message = ex.Message });
            }
        }
        public void SaveTermsData(string TermsAndConditionsId, string PIMasterId)
        {
            DataSet dsToSalesOrder;
            DataSet dsToFirstCharacteristics;
            try
            {
                string Id = "";
                DataSet dsSOId;

                string NewSoId = string.Empty;
                DataSet dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("SELECT * FROM TermsAndConditionsPOChild WHERE POId='" + POId + "'", out dsDetail, false, "1");
                //if (dsDetail.Tables[0].Rows.Count > 0)
                //{
                //    if (dsDetail.Tables[0].Rows[0]["TermsAndConditionsMasterId"].ToString() != TitleId)
                //    {

                TnCDeleteDetail(PIMasterId);

                //    }
                //}
                con.OpenDataSetThroughAdapter("SELECT * FROM TermsAndConditionsPIChild WHERE 1=2", out dsToSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TermsAndConditionsPIDetails WHERE 1=2", out dsToFirstCharacteristics, false, "1");

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM  TermsAndConditionsChild WHERE TermsAndConditionsMasterId='" + TermsAndConditionsId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TermsAndConditionsDetails Where TermsAndConditionsChildId IN(Select Id from TermsAndConditionsChild Where TermsAndConditionsMasterId='" + TermsAndConditionsId + "')");

                int SCount = 0;
                objGenID.GenerateIDAuto("dbo.TermsAndConditionsPIChild", out Id);

                for (int m = 0; m < dtFromMaster.Rows.Count; m++)
                {
                    SCount++;
                    DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                    CopyRow(dtFromMaster.Rows[m], ref drSalesOrder);
                    drSalesOrder["Id"] = TermsAndConditionsId + Convert.ToInt32(Id) + SCount;
                    NewSoId = drSalesOrder["Id"].ToString();
                    // drSalesOrder["TermsAndConditionsMasterId"] = TitleId;
                    drSalesOrder["PIMasterId"] = PIMasterId;
                    dsToSalesOrder.Tables[0].Rows.Add(drSalesOrder);

                    dtFromFirstCharacteristics.DefaultView.RowFilter = "TermsAndConditionsChildId='" + dtFromMaster.Rows[m]["Id"].ToString() + "'";
                    for (int i = 0; i < dtFromFirstCharacteristics.DefaultView.Count; i++)
                    {
                        DataRow drFirstCharacteristics = dsToFirstCharacteristics.Tables[0].NewRow();
                        CopyRow(dtFromFirstCharacteristics.DefaultView[i].Row, ref drFirstCharacteristics);
                        drFirstCharacteristics["Id"] = NewSoId + (i + 1);
                        drFirstCharacteristics["TermsAndConditionsPIChildId"] = NewSoId;

                        dsToFirstCharacteristics.Tables[0].Rows.Add(drFirstCharacteristics);
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsToSalesOrder, dsToFirstCharacteristics);
                //return Json(new { Error = false, Message = AplosMessage.Insert });


            }
            catch (Exception ex)
            {
                throw ex;
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
        public void TnCDeleteDetail(string PIMasterId)
        {
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            string strSQLDetail = "DELETE FROM TermsAndConditionsPIDetails Where TermsAndConditionsPIChildId IN(SELECT ID FROM TermsAndConditionsPIChild WHERE PIMasterId='" + PIMasterId + "')";
            string strSQLChild = "DELETE FROM TermsAndConditionsPIChild WHERE PIMasterId='" + PIMasterId + "'";
            con = new ConnectionManager.DAL.ConManager("1");
            con.OpenConnection("1");
            con.BeginTransaction();
            con.ExecuteNonQueryWrapper(strSQLDetail, true, "1");
            con.ExecuteNonQueryWrapper(strSQLChild, true, "1");
            con.CommitTransaction();
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
,P2.Amount,p2.POQuantity,PM.AddedDate
 FROM PIMaster PM 
LEFT OUTER JOIN
(SELECT p.PIMasterId, SUM(p.Amount) Amount,SUM(pmp.POQuantity) POQuantity FROM PIMaterial p
LEFT OUTER JOIN POMappingWithPI pmp ON pmp.PIMaterialID=p.Id
 GROUP BY p.PIMasterId) P2 ON p2.PIMasterId = PM.Id

LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
LEFT OUTER JOIN PIVersion AS pv ON PM.Id=pv.PIMasterId and PV.Id=(select top 1 Id from PIVersion where PIMasterId=PM.Id ORDER BY VersionNo DESC)
--ORDER BY PM.PIDate DESC
) AS TEMP WHERE " + strkey + "ORDER BY TEMP.AddedDate DESC";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllData(string PIMasterId, string VersionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
							,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress,PM.RevisionNo
							,C.Code Currency,B.UserName Buyer,P.UserName Customer,PM.TermsAndConditionsId
							 FROM PIMaster PM 
							LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
							LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
							LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
						WHERE PM.Id='" + PIMasterId + @"'";

            var PIMasterData = _sqlRepository.GetDataCollection(sql, null);

            sql = @"SELECT p.Id, p.PIMasterId, p.PIVersionId,
CAST(ROUND(p.Rate, 4) AS DECIMAL(10,4)) Rate,
CAST(ROUND(p.Quantity, 2) AS DECIMAL(10,2)) Quantity,
ROUND(p.Amount, 2) Amount,
 p.UoMId,NULL AS MaterialGroupUOMList,
							   p.[Description],FORMAT(p.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, p.MaterialGroupMasterId,mgm.UserName AS MaterialGroup
							   ,p.HSNCodeId,h.Code HSNCode
						  FROM PIMaterial AS p
						  LEFT JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=p.MaterialGroupMasterId
						  LEFT JOIN hkp.HSNCode AS h ON h.Id=p.HSNCodeId
						WHERE p.PIMasterId='" + PIMasterId + @"' AND p.PIVersionId='" + VersionId + @"'";

            var PIMaterial = _sqlRepository.GetDataCollection(sql, null);

            sql = @"SELECT U.MaterialGroupMasterId,UOM.Code,UOM.userName,UOM.Id FROM (
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
                var U = UOMList.Where(w => w["MaterialGroupMasterId"].ToString() == PIMaterial[i]["MaterialGroupMasterId"].ToString()).ToList();
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
            string sql = @"SELECT U.MaterialGroupMasterId,UOM.Code,UOM.userName,UOM.Id FROM (
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
        [HttpGet, Authorize]
        public ActionResult GetHSNList(string MaterialGroupMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"						SELECT mgm.Id MaterialGroupMasterId,h.Code HSNCode
						  FROM mst.MaterialGroupMaster AS mgm
						LEFT JOIN hkp.HSNCode AS h ON mgm.HSNCodeId=h.Id
						WHERE mgm.Id='" + MaterialGroupMasterId + @"'";

            var _HSNList = _sqlRepository.GetDataCollection(sql, null);

            return Json(new { HSNList = _HSNList }, JsonRequestBehavior.AllowGet);
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

                return Json(new { Error = false, VersionId = _VersionId, Message = AplosMessage.Insert });
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
        [HttpGet, Authorize]
        public ActionResult GetHSNCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT h.Id,h.Code HSNCode FROM hkp.HSNCode AS h";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult TermsAndConditions()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string _sql = "select Id,Description,UserName TermsAndConditions from HKP.TermsAndConditions where Type='ProformaInvoice' And CompanyId='" + identity.CompanyId + @"'";
                //_sqlRepository.ExecuteSqlCommand(_sql);

                return Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public ActionResult DeleteTitle(string id)
        {
            try
            {

                string ret = PI.DeletePITitle(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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
        string TableName = "hkp.TermsAndConditions";
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }

        [HttpPost, Authorize]
        public ActionResult GetTermsAndConditionsPIList(string TermsAndConditionMasterId, string PIMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select TC.Id TermsAndConditionPIChildId,TC.Id,TC.Title
from TermsAndConditionsPIChild TC
WHERE TC.PIMasterId='" + PIMasterId + @"' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetTermsAndConditionsPIDetailList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select TCD.Id,TC.Id TermsAndConditionPIChildId,TCD.HeaderCaption ,TCD.Description  from TermsAndConditionsPIDetails TCD 
left outer join TermsAndConditionsPIChild TC on TC.Id=TCD.TermsAndConditionsPIChildId ORDER BY TCD.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPopUp(string TermsAndConditionsPIDetailId)
        {
            try
            {
                return Json(PI.GetTermsAndConditionPOPopUp(TermsAndConditionsPIDetailId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public ActionResult DeletePIDetailPOPup(string id)
        {
            try
            {
                string ret = PI.DeletePIDetailPopUp(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false/*, Sequence = GetSequence()*/, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public JsonResult SaveData(Dictionary<string, object> GridData, string TermsAndConditionPIChildId)
        {
            try
            {
                DataSet dsGrid;
                ConnectionManager.DAL.ConManager conBin = new ConnectionManager.DAL.ConManager("1");
                conBin.OpenDataSetThroughAdapter("select top 1 Sequence from dbo.TermsAndConditionsPIDetails where TermsAndConditionsPIChildId='" + TermsAndConditionPIChildId + "' order by AddedDate desc", out DataSet dsGridSeq, false, "1");
                conBin.OpenDataSetThroughAdapter("select * from dbo.TermsAndConditionsPIDetails where TermsAndConditionsPIChildId='" + TermsAndConditionPIChildId + "'", out dsGrid, false, "1");
                string DetailId = "";
                int count = 0;
                DataView dv = new DataView(dsGrid.Tables[0]);
                dv.RowFilter = "Id='" + GridData["Id"] + "'";

                if (dv.Count == 0)
                {
                    if (DetailId == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.TermsAndConditionsPIDetails", out DetailId);
                    }
                    if (dsGridSeq.Tables[0].Rows.Count == 0)
                    {
                        count++;
                    }
                    else
                    {
                        count = (int)clsStaticInfo.dbl(dsGridSeq.Tables[0].Rows[0]["Sequence"].ToString()) + 1;
                    }
                    DataRow dr = dsGrid.Tables[0].NewRow();

                    GridData["Id"] = "TD-" + DetailId;
                    GridData["TermsAndConditionsPIChildId"] = TermsAndConditionPIChildId;
                    GridData["Sequence"] = count;
                    AddNewRow(dsGrid.Tables[0], GridData);
                }
                else
                {
                    DataRow drmo = dv[0].Row;
                    EditRow(drmo, GridData);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsGrid);
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
    }
}