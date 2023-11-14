#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Helpers;
using Library.Service.Payrolls;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class PostSalesInvoiceController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public PostSalesInvoiceController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
      [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT P.[Id]
                          ,P.SalesId
                          ,FORMAT(P.InvoiceDate,'dd-MMM-yyyy') InvoiceDate
                          ,P.BankMasterId
                          ,P.ShipmentModeId
                          ,P.PortOfLoadingId
                          ,P.ExpFormNo
                          ,FORMAT(P.ExpDate,'dd-MMM-yyyy')ExpDate
                          ,P.CargoNetWt
                          ,P.CargoGrossWt
                          ,P.Dimension
                          ,P.ExFactoryDocRef
                          ,FORMAT(P.ExFactoryDate,'dd-MMM-yyyy')ExFactoryDate
                          ,P.TransportAgentId
                          ,P.TransportDocRefNo
                          ,FORMAT(P.TransportDocDate,'dd-MMM-yyyy')TransportDocDate
                          ,P.TransportVehicleNo
                          ,P.TransportDriverName
                          ,P.TransportDriverNo
                          ,P.PreCarriageBy
                          ,P.PlaceOfReceiptByPreCarriage
                          ,P.PreCarriageDocRef
                          ,FORMAT(P.PreCarriageDocDate,'dd-MMM-yyyy')PreCarriageDocDate
                          ,P.CNFAgentId
                          ,P.CNFContainerNo
                          ,P.CNFVesselTrackingNo
                          ,P.CNFVesselName
                          ,P.CNFVesselSalesDetails
                          ,P.CNFBLAWB
                          ,FORMAT(P.CNFBLAWBDate,'dd-MMM-yyyy')CNFBLAWBDate
                          ,P.ETA
                          ,P.FinalDestinationId
                          ,P.PortOfDischargeId
                          ,P.PortOfDelivaryId
                          ,P.BankDocRef
                          ,FORMAT(P.BankDocDate,'dd-MMM-yyyy')BankDocDate
                          ,P.AddedBy
                          ,FORMAT(P.[AddedDate],'dd-MMM-yyyy')AddedDate
                          ,P.[AddedFromIP]
                          ,P.[UpdatedBy]
                          ,FORMAT(P.[UpdatedDate],'dd-MMM-yyyy')[UpdatedDate]
                          ,P.UpdatedFromIP
						  ,C.UserName CNFAgentName
						  ,T.UserName TransportAgentName
                          ,S.InvoiceNo
						  ,PT.UserName [Port]
						  ,SP.UserName ShipMode
                      FROM [dbo].[PostSalesInvoice] P
					  LEFT JOIN TRN.Sales S ON S.Id=P.SalesId
					  LEFT JOIN HKP.Party C ON C.Id=P.CNFAgentId
					  LEFT JOIN HKP.Party T ON T.Id=P.TransportAgentId					  
					  LEFT JOIN MST.[Port] PT ON PT.Id=P.PortOfLoadingId
					  LEFT JOIN [MST].[ShipMode] SP ON SP.Id=P.ShipmentModeId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListBySalesId(string SalesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT P.[Id]
                          ,P.SalesId
                          ,FORMAT(P.InvoiceDate,'dd-MMM-yyyy') InvoiceDate
                          ,P.BankMasterId
                          ,P.ShipmentModeId
                          ,P.PortOfLoadingId
                          ,P.ExpFormNo
                          ,FORMAT(P.ExpDate,'dd-MMM-yyyy')ExpDate
                          ,P.CargoNetWt
                          ,P.CargoGrossWt
                          ,P.Dimension
                          ,P.ExFactoryDocRef
                          ,FORMAT(P.ExFactoryDate,'dd-MMM-yyyy')ExFactoryDate
                          ,P.TransportAgentId
                          ,P.TransportDocRefNo
                          ,FORMAT(P.TransportDocDate,'dd-MMM-yyyy')TransportDocDate
                          ,P.TransportVehicleNo
                          ,P.TransportDriverName
                          ,P.TransportDriverNo
                          ,P.PreCarriageBy
                          ,P.PlaceOfReceiptByPreCarriage
                          ,P.PreCarriageDocRef
                          ,FORMAT(P.PreCarriageDocDate,'dd-MMM-yyyy')PreCarriageDocDate
                          ,P.CNFAgentId
                          ,P.CNFContainerNo
                          ,P.CNFVesselTrackingNo
                          ,P.CNFVesselName
                          ,P.CNFVesselSalesDetails
                          ,P.CNFBLAWB
                          ,FORMAT(P.CNFBLAWBDate,'dd-MMM-yyyy')CNFBLAWBDate
                          ,P.ETA
                          ,P.FinalDestinationId
                          ,P.PortOfDischargeId
                          ,P.PortOfDelivaryId
                          ,P.BankDocRef
                          ,FORMAT(P.BankDocDate,'dd-MMM-yyyy')BankDocDate
                          ,P.AddedBy
                          ,FORMAT(P.[AddedDate],'dd-MMM-yyyy')AddedDate
                          ,P.[AddedFromIP]
                          ,P.[UpdatedBy]
                          ,FORMAT(P.[UpdatedDate],'dd-MMM-yyyy')[UpdatedDate]
                          ,P.UpdatedFromIP
						  ,C.UserName CNFAgentName
						  ,T.UserName TransportAgentName
                          ,S.InvoiceNo
						  ,PT.UserName [Port]
						  ,SP.UserName ShipMode
                          ,P.ExportRefNo,P.TransporterCHAForwarderId,P.DocumentReceiveDate,P.AWBB2B,P.ActualPaymentReceived,P.ShippingBillNo,P.PortCode,P.DocumentSubmissionDate
					      ,P.DocAcceptanceDate,P.FinalShipmentStatus,P.ShippingBillDate,P.ShipmentDate,P.NegotiationType,P.PaymentReceivedDate,P.Remark,S.InvoiceStatus
                      FROM [dbo].[PostSalesInvoice] P
					  LEFT JOIN TRN.Sales S ON S.Id=P.SalesId
					  LEFT JOIN HKP.Party C ON C.Id=P.CNFAgentId
					  LEFT JOIN HKP.Party T ON T.Id=P.TransportAgentId					  
					  LEFT JOIN MST.[Port] PT ON PT.Id=P.PortOfLoadingId
					  LEFT JOIN [MST].[ShipMode] SP ON SP.Id=P.ShipmentModeId
                      Where P.SalesId='" + SalesId+"' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPortByDestinationCbo(string destinationId)
        {
            
            var sql = @"SELECT Id,UserName FROM MST.[Port] WHERE CountryId=(SELECT P.CountryId FROM MST.Destination P Where P.Id='"+ destinationId + "')";
            return Json(_sqlRepository.GetCombo(sql, "Id", "UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount, 0) + ISNULL(SS.Amount, 0) AS Amount,
                                       Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays, S.BaseOnDueDate
									, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.MatureDate, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId,S.BLNumber,S.ItemDescription,S.ComercialInvoiceNo,S.EXPFromNo,S.EXPDate,S.BLDate
                                    , CASE WHEN S.RowState = 'Parked' THEN 1 ELSE 0 END AS IsPark,FORMAT(S.AddedDate, 'dd-MMM-yyyy')AddedDate,s.AddedBy,S.AddedFromIP,FORMAT(S.UpdatedDate, 'dd-MMM-yyyy') UpdatedDate,s.UpdatedBy,S.UpdatedFromIP 
									
									,ContractNo=Stuff((
                    SELECT distinct',' + C.ContractNo
                    FROM  dbo.[Contract] C 
					LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN [TRN].[SalesMaterial] SM ON SM.SalesOrderId=SO.Id
                    WHERE S.Id = SM.SalesId
                    FOR XML PATH('')
                    ), 1, 1, '')
					,LCRef=Stuff((
                    SELECT distinct',' + MLC.LCRef
                    FROM  dbo.[Contract] C 
					LEFT JOIN dbo.[MasterLC] MLC ON MLC.Id=C.MasterLCId
					LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN [TRN].[SalesMaterial] SM ON SM.SalesOrderId=SO.Id
                    WHERE S.Id = SM.SalesId
                    FOR XML PATH('')
                    ), 1, 1, '')
					,BenificiaryBankId=Stuff((
                    SELECT distinct',' + MLC.BenificiaryBankId
                    FROM  dbo.[Contract] C 
					LEFT JOIN dbo.[MasterLC] MLC ON MLC.Id=C.MasterLCId
					LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN [TRN].[SalesMaterial] SM ON SM.SalesOrderId=SO.Id
                    WHERE S.Id = SM.SalesId
                    FOR XML PATH('')
                    ), 1, 1, '')
                                       FROM[TRN].[Sales] AS S
                                    JOIN[HKP].[Party] AS P ON P.Id = S.PartyId
                                    LEFT JOIN[HKP].[PartyPlant] AS PPI ON PPI.Id = S.InvoicingPartyPlantId
                                    LEFT JOIN[MST].[AddressMaster] AS AM ON AM.Id = PPI.AddressMasterId
                                    LEFT JOIN[SCS].[State] AS ST ON ST.Id = AM.StateId
                                    LEFT JOIN[HKP].[PartyPlant] AS PPD ON PPD.Id = S.DeliveryPartyPlantId
                                    LEFT JOIN[MST].[AddressMaster] AS AMD ON AMD.Id = PPD.AddressMasterId
                                    LEFT JOIN[SCS].[State] AS STD ON STD.Id = AMD.StateId
                                    LEFT JOIN[SCS].[Currency] AS C ON C.Id = S.CurrencyId
                                    LEFT JOIN[ORG].[Plant] AS PT ON PT.Id = S.PlantId
                                    LEFT JOIN[MST].[AddressMaster] AS AMP ON AMP.Id = PT.AddressMasterId
                                    LEFT JOIN(SELECT M.SalesId, SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId = S.Id
                                    LEFT JOIN(SELECT M.SalesId, SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId = S.Id

                                    WHERE S.CompanyGroupId = '" + identity.CompanyGroupId + "' AND S.CompanyId = '" + identity.CompanyId + "'";
            JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }



        [HttpPost,Authorize]
        public JsonResult Create(PostSalesInvoice entity)
        {

            try
            {
                SaveData(entity, out string masterId);
                return Json(new { Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult Edit(PostSalesInvoice entity)
        {

            try
            {
                SaveData(entity, out string masterId);
                return Json(new { Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PostSalesInvoice), out sID);
            return sID;
        }


        private void SaveData(PostSalesInvoice data, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            try
            {

                string sql = "SELECT * FROM [dbo].[PostSalesInvoice] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetPK();
                    dr["SalesId"] = data.SalesId;
                    dr["InvoiceDate"] = data.InvoiceDate;
                    dr["BankMasterId"] = data.BankMasterId;
                    dr["BankDocRef"] = data.BankDocRef;

                    if (String.IsNullOrEmpty(data.BankDocDate.ToString()))
                    {
                        dr["BankDocDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["BankDocDate"] = data.BankDocDate;
                    }
                    dr["ExpFormNo"] = data.ExpFormNo;
                    if (String.IsNullOrEmpty(data.ExpDate.ToString()))
                    {
                        dr["ExpDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ExpDate"] = data.ExpDate;
                    }
                    dr["ExFactoryDocRef"] = data.ExFactoryDocRef;

                    if (String.IsNullOrEmpty(data.ExFactoryDate.ToString()))
                    {
                        dr["ExFactoryDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ExFactoryDate"] = data.ExFactoryDate;
                    }
                    dr["CargoNetWt"] = data.CargoNetWt;
                    dr["CargoGrossWt"] = data.CargoGrossWt;
                    dr["ShipmentModeId"] = data.ShipmentModeId;  
                    dr["PortOfLoadingId"] = data.PortOfLoadingId;                    
                    dr["Dimension"] = data.Dimension;
                    dr["FinalDestinationId"] = data.FinalDestinationId;
                    dr["PortOfDischargeId"] = data.PortOfDischargeId;
                    dr["PortOfDelivaryId"] = data.PortOfDelivaryId;
                    if (String.IsNullOrEmpty(data.ETA.ToString()))
                    {
                        dr["ETA"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ETA"] = data.ETA;
                    }

                    dr["TransportAgentId"] = data.TransportAgentId;
                    dr["TransportDocRefNo"] = data.TransportDocRefNo;
                    dr["TransportVehicleNo"] = data.TransportVehicleNo;
                    dr["TransportDriverName"] = data.TransportDriverName;
                    dr["TransportDriverNo"] = data.TransportDriverNo;

                    if (String.IsNullOrEmpty(data.TransportDocDate.ToString()))
                    {
                        dr["TransportDocDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["TransportDocDate"] = data.TransportDocDate;
                    }

                    dr["PreCarriageBy"] = data.PreCarriageBy;
                    dr["PlaceOfReceiptByPreCarriage"] = data.PlaceOfReceiptByPreCarriage;
                    dr["PreCarriageDocRef"] = data.PreCarriageDocRef;

                    if (String.IsNullOrEmpty(data.PreCarriageDocDate.ToString()))
                    {
                        dr["PreCarriageDocDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["PreCarriageDocDate"] = data.PreCarriageDocDate;
                    }                    

                    dr["CNFAgentId"] = data.CNFAgentId;
                    dr["CNFContainerNo"] = data.CNFContainerNo;
                    dr["CNFVesselTrackingNo"] = data.CNFVesselTrackingNo;
                    dr["CNFVesselName"] = data.CNFVesselName;
                    dr["CNFVesselSalesDetails"] = data.CNFVesselSalesDetails;
                    dr["CNFBLAWB"] = data.CNFBLAWB;
                    if (String.IsNullOrEmpty(data.CNFBLAWBDate.ToString()))
                    {
                        dr["CNFBLAWBDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["CNFBLAWBDate"] = data.CNFBLAWBDate;
                    }

                    dr["CNFAgentId"] = data.CNFAgentId;

                    dr["ExportRefNo"] = data.ExportRefNo;
                    dr["TransporterCHAForwarderId"] = data.TransporterCHAForwarderId;

                    if (String.IsNullOrEmpty(data.DocumentReceiveDate.ToString()))
                    {
                        dr["DocumentReceiveDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["DocumentReceiveDate"] = data.DocumentReceiveDate;
                    }

                    dr["AWBB2B"] = data.AWBB2B;
                    dr["ActualPaymentReceived"] = data.ActualPaymentReceived;
                    dr["ShippingBillNo"] = data.ShippingBillNo;
                    dr["PortCode"] = data.PortCode;
               
                    if (String.IsNullOrEmpty(data.DocumentSubmissionDate.ToString()))
                    {
                        dr["DocumentSubmissionDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["DocumentSubmissionDate"] = data.DocumentSubmissionDate;
                    }

                    if (String.IsNullOrEmpty(data.DocAcceptanceDate.ToString()))
                    {
                        dr["DocAcceptanceDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["DocAcceptanceDate"] = data.DocAcceptanceDate;
                    }

                    dr["FinalShipmentStatus"] = data.FinalShipmentStatus;


                    if (String.IsNullOrEmpty(data.ShippingBillDate.ToString()))
                    {
                        dr["ShippingBillDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ShippingBillDate"] = data.ShippingBillDate;
                    }
                    if (String.IsNullOrEmpty(data.ShipmentDate.ToString()))
                    {
                        dr["ShipmentDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ShipmentDate"] = data.ShipmentDate;
                    }
                    
                    dr["NegotiationType"] = data.NegotiationType;
                    if (String.IsNullOrEmpty(data.PaymentReceivedDate.ToString()))
                    {
                        dr["PaymentReceivedDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["PaymentReceivedDate"] = data.PaymentReceivedDate;
                    }
                    dr["Remark"] = data.Remark;

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

                    dr["SalesId"] = data.SalesId;
                    dr["InvoiceDate"] = data.InvoiceDate;
                    dr["BankMasterId"] = data.BankMasterId;
                    dr["BankDocRef"] = data.BankDocRef;

                    if (String.IsNullOrEmpty(data.BankDocDate.ToString()))
                    {
                        dr["BankDocDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["BankDocDate"] = data.BankDocDate;
                    }
                    dr["ExpFormNo"] = data.ExpFormNo;
                    if (String.IsNullOrEmpty(data.ExpDate.ToString()))
                    {
                        dr["ExpDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ExpDate"] = data.ExpDate;
                    }
                    dr["ExFactoryDocRef"] = data.ExFactoryDocRef;

                    if (String.IsNullOrEmpty(data.ExFactoryDate.ToString()))
                    {
                        dr["ExFactoryDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ExFactoryDate"] = data.ExFactoryDate;
                    }
                    dr["CargoNetWt"] = data.CargoNetWt;
                    dr["CargoGrossWt"] = data.CargoGrossWt;
                    dr["ShipmentModeId"] = data.ShipmentModeId;
                    dr["PortOfLoadingId"] = data.PortOfLoadingId;
                    dr["Dimension"] = data.Dimension;
                    dr["FinalDestinationId"] = data.FinalDestinationId;
                    dr["PortOfDischargeId"] = data.PortOfDischargeId;
                    dr["PortOfDelivaryId"] = data.PortOfDelivaryId;
                    if (String.IsNullOrEmpty(data.ETA.ToString()))
                    {
                        dr["ETA"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ETA"] = data.ETA;
                    }

                    dr["TransportAgentId"] = data.TransportAgentId;
                    dr["TransportDocRefNo"] = data.TransportDocRefNo;
                    dr["TransportVehicleNo"] = data.TransportVehicleNo;
                    dr["TransportDriverName"] = data.TransportDriverName;
                    dr["TransportDriverNo"] = data.TransportDriverNo;

                    if (String.IsNullOrEmpty(data.TransportDocDate.ToString()))
                    {
                        dr["TransportDocDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["TransportDocDate"] = data.TransportDocDate;
                    }

                    dr["PreCarriageBy"] = data.PreCarriageBy;
                    dr["PlaceOfReceiptByPreCarriage"] = data.PlaceOfReceiptByPreCarriage;
                    dr["PreCarriageDocRef"] = data.PreCarriageDocRef;

                    if (String.IsNullOrEmpty(data.PreCarriageDocDate.ToString()))
                    {
                        dr["PreCarriageDocDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["PreCarriageDocDate"] = data.PreCarriageDocDate;
                    }

                    dr["CNFAgentId"] = data.CNFAgentId;
                    dr["CNFContainerNo"] = data.CNFContainerNo;
                    dr["CNFVesselTrackingNo"] = data.CNFVesselTrackingNo;
                    dr["CNFVesselName"] = data.CNFVesselName;
                    dr["CNFVesselSalesDetails"] = data.CNFVesselSalesDetails;
                    dr["CNFBLAWB"] = data.CNFBLAWB;
                    if (String.IsNullOrEmpty(data.CNFBLAWBDate.ToString()))
                    {
                        dr["CNFBLAWBDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["CNFBLAWBDate"] = data.CNFBLAWBDate;
                    }

                    dr["ExportRefNo"] = data.ExportRefNo;
                    dr["TransporterCHAForwarderId"] = data.TransporterCHAForwarderId;
               
                    if (String.IsNullOrEmpty(data.DocumentReceiveDate.ToString()))
                    {
                        dr["DocumentReceiveDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["DocumentReceiveDate"] = data.DocumentReceiveDate;
                    }
                    dr["AWBB2B"] = data.AWBB2B;
                    dr["ActualPaymentReceived"] = data.ActualPaymentReceived;
                    dr["ShippingBillNo"] = data.ShippingBillNo;
                    dr["PortCode"] = data.PortCode;
                    if (String.IsNullOrEmpty(data.DocumentSubmissionDate.ToString()))
                    {
                        dr["DocumentSubmissionDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["DocumentSubmissionDate"] = data.DocumentSubmissionDate;
                    }

                    if (String.IsNullOrEmpty(data.DocAcceptanceDate.ToString()))
                    {
                        dr["DocAcceptanceDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["DocAcceptanceDate"] = data.DocAcceptanceDate;
                    }

                    dr["FinalShipmentStatus"] = data.FinalShipmentStatus;


                    if (String.IsNullOrEmpty(data.ShippingBillDate.ToString()))
                    {
                        dr["ShippingBillDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ShippingBillDate"] = data.ShippingBillDate;
                    }
                    if (String.IsNullOrEmpty(data.ShipmentDate.ToString()))
                    {
                        dr["ShipmentDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ShipmentDate"] = data.ShipmentDate;
                    }

                    dr["NegotiationType"] = data.NegotiationType;
                    if (String.IsNullOrEmpty(data.PaymentReceivedDate.ToString()))
                    {
                        dr["PaymentReceivedDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["PaymentReceivedDate"] = data.PaymentReceivedDate;
                    }
                    dr["Remark"] = data.Remark;


                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM PostSalesInvoice WHERE Id = '" + Id + "'";
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
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost]
        public JsonResult CreateInvoiceStatus(Dictionary<string,object> data)
        {
            try
            {
                SaveInvoiceStatusData(data);
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }
        private void SaveInvoiceStatusData(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            try
            {

                string sql = "SELECT * FROM trn.Sales WHERE Id='" + data["Id"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {

                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();

                    dr["InvoiceStatus"] = data["InvoiceStatus"];
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster); 
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion
    }

    public class PostSalesInvoice : BaseModel
    {
        public string Id { get; set; }
        public string SalesId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string BankMasterId { get; set; }
        public string ShipmentModeId { get; set; }
        public string PortOfLoadingId { get; set; }
        public string ExpFormNo { get; set; }
        public DateTime? ExpDate { get; set; }
        public string CargoNetWt { get; set; }
        public string CargoGrossWt { get; set; }
        public string Dimension { get; set; }
        public string ExFactoryDocRef { get; set; }
        public DateTime? ExFactoryDate { get; set; }
        public string TransportAgentId { get; set; }
        public string TransportDocRefNo { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime? TransportDocDate { get; set; }
        public string TransportVehicleNo { get; set; }
        public string TransportDriverName { get; set; }
        public string TransportDriverNo { get; set; }
        public string PreCarriageBy { get; set; }
        public string PlaceOfReceiptByPreCarriage { get; set; }
        public string PreCarriageDocRef { get; set; }
        public DateTime? PreCarriageDocDate { get; set; }
        public string CNFAgentId { get; set; }
        public string CNFContainerNo { get; set; }
        public string CNFVesselTrackingNo { get; set; }
        public string CNFVesselName { get; set; }
        public string CNFVesselSalesDetails { get; set; }
        public string CNFBLAWB { get; set; }
        public DateTime? CNFBLAWBDate { get; set; }
        public string FinalDestinationId { get; set; }
        public string PortOfDischargeId { get; set; }
        public string PortOfDelivaryId { get; set; }
        public string BankDocRef { get; set; }
        public DateTime? BankDocDate { get; set; }

        public string ExportRefNo { get; set; }
        public string TransporterCHAForwarderId { get; set; }
        public DateTime? DocumentReceiveDate { get; set; }
        public string AWBB2B { get; set; }
        public string ActualPaymentReceived { get; set; }
        public string ShippingBillNo { get; set; }
        public string PortCode { get; set; }
        public DateTime? DocumentSubmissionDate { get; set; }
        public DateTime? DocAcceptanceDate { get; set; }
        public string FinalShipmentStatus { get; set; }
        public DateTime? ShippingBillDate { get; set; }
        public DateTime? ShipmentDate { get; set; }
        public string NegotiationType { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public string Remark { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string Flag { get; set; }

    }
}