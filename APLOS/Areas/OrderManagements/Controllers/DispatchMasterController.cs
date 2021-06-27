#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.OrderManagements;
using Library.OrderManagement.Production;
using Library.Security.Core;
using Library.Service.OrderManagements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class DispatchMasterController : BaseController
    {
        #region Constructor
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;
        public DispatchMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

       

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpPost]
        public JsonResult Insert(Dictionary<string, object> data,List<Dictionary<string, object>> selectedSalesOrderList)
        {
            SaveData(data, selectedSalesOrderList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        private void SaveData(Dictionary<string, object> data,List<Dictionary<string, object>> selectedSalesOrderList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {

                DataSet dsMaster, dsDispatchDetail, dsDispatchDetailSO;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[DispatchMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id, _DispatchDetailId = "";
                string masterId = "";
                string dispatchDetailId = "";


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchMaster", out _Id);

                    data["Id"] = "DM" + _Id;
                    data["PlantId"] = identity.PlantId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["PlantId"] = identity.PlantId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                 masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();


                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchDetail WHERE DispatchMasterId ='" + data["Id"] + "'", out dsDispatchDetail, false, "1");


                if (dsDispatchDetail.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchDetail", out _DispatchDetailId);

                    DataRow dr = dsDispatchDetail.Tables[0].NewRow();

                    dr["Id"] = "DD" + _DispatchDetailId;
                    dr["DispatchMasterId"] = masterId;
                   

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsDispatchDetail.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsDispatchDetail.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                dispatchDetailId = dsDispatchDetail.Tables[0].Rows[0]["Id"].ToString();

                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchDetailSO WHERE DispatchDetailId ='" + dispatchDetailId + "'", out dsDispatchDetailSO, false, "1");

                foreach (var item in selectedSalesOrderList)
                {
                    DataView dv = new DataView(dsDispatchDetailSO.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = GetDispatchOrderItemPK();
                        item["DispatchDetailId"] = dispatchDetailId;

                        AddNewRow(dsDispatchDetailSO.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }

                }
                

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDispatchDetail, dsDispatchDetailSO);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private string GetDispatchMaterialPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchMaterial", out sID);
            return sID;
        }
        private string GetDispatchOrderItemPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchDetailSO", out sID);
            return sID;
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

        [HttpGet, Authorize]
        public ActionResult GetSOList(string customerId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string sql = @"Select 0 AS Active,SO.Id SalesOrderId,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, D.UserName Destination,FORMAT(SO.CommitmentDate,'dd-MMM-yyyy') CommitmentDate
                            ,ISNULL(SO.CustomerPOId,CPO.PONumber) PONumber,SM.UserName ShipMode
                            from TRN.SalesOrder SO
                            LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
                            LEFT JOIN TRN.MasterOrder MO ON MO.Id=MOI.MasterOrderId
                            LEFT JOIN MST.Destination D ON D.Id=SO.DestinationId
                            LEFT JOIN MST.ShipMode SM ON SM.Id=SO.ShipmentModeId
                            LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id=SO.CustomerPOId
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=SO.OrderStatusId
                            LEFT JOIN HKP.OrderCategory OC ON OC.Id=SO.OrderCategoryId
                            Where MO.PartyId='"+ customerId + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDispatchDetailSOList(string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                     string sql = @"Select DSO.Id,DSO.DispatchDetailId,SO.Id SalesOrderId,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, D.UserName Destination,FORMAT(SO.CommitmentDate,'dd-MMM-yyyy') CommitmentDate
                    ,ISNULL(SO.CustomerPOId,CPO.PONumber) PONumber,SM.UserName ShipMode
                    from TRN.SalesOrder SO
                    LEFT JOIN [dbo].[DispatchDetailSO] DSO ON DSO.SalesOrderId=SO.Id
                    LEFT JOIN [dbo].[DispatchDetail] DD ON DD.Id=DSO.DispatchDetailId
                    LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
                    LEFT JOIN TRN.MasterOrder MO ON MO.Id=MOI.MasterOrderId
                    LEFT JOIN MST.Destination D ON D.Id=SO.DestinationId
                    LEFT JOIN MST.ShipMode SM ON SM.Id=SO.ShipmentModeId
                    LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id=SO.CustomerPOId
                    LEFT JOIN HKP.OrderStatus OS ON OS.Id=SO.OrderStatusId
                    LEFT JOIN HKP.OrderCategory OC ON OC.Id=SO.OrderCategoryId
                    Where DD.DispatchMasterId='" + masterId + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"Select DM.*,P.UserName PartyName,INP.UserName InvoicingPartyPlant,DLP.UserName DeliveryPartyPlant 
                                from [dbo].[DispatchMaster] DM
                                LEFT JOIN HKP.Party P ON P.Id=DM.PartyId
                                LEFT JOIN HKP.PartyPlant INP ON INP.Id=DM.InvoicingPartyPlantId
                                LEFT JOIN HKP.PartyPlant DLP ON DLP.Id=DM.DeliveryPartyPlantId
                                Where DM.PlantId='" + identity.PlantId + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetAllConfirmedPackingContentData()
        {
            return Json(_productionSummaryData.GetAllConfirmedPackingContentData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPackingChildDataList(string MasterId)
        {
            string sql = @"SELECT 0 AS [Check],P.*, [State]=CASE WHEN P.IsConfirmed=1 THEN 1 ELSE 0 END FROM [dbo].[PackingChild] P WHERE P.PackingContentMasterId='" + MasterId + "' AND P.IsConfirmed=1";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
    //public class DispatchDetail
    //{
    //    public string  Id { get; set; }
    //    public string DispatchMasterId { get; set; }
    //    public string AddedBy { get; set; }
    //    public DateTime AddedDate { get; set; }
    //    public string AddedFromIP { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime UpdatedDate { get; set; }
    //    public string UpdatedFromIP { get; set; }
    //}
}