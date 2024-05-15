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

        public ActionResult InvoiceStatus()
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
                          ,FORMAT(P.NegotiatingDate,'dd-MMM-yyyy')NegotiatingDate
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
                            ,P.ExportRefNo,P.TransporterCHAForwarderId,FORMAT(P.DocumentReceiveDate,'dd-MMM-yyyy')DocumentReceiveDate,P.AWBB2B,P.ActualPaymentReceived,P.ShippingBillNo,P.PortCode,FORMAT(P.DocumentSubmissionDate,'dd-MMM-yyyy')DocumentSubmissionDate
					      ,FORMAT(P.DocAcceptanceDate,'dd-MMM-yyyy')DocAcceptanceDate,P.FinalShipmentStatus,FORMAT(P.ShippingBillDate,'dd-MMM-yyyy')ShippingBillDate,FORMAT(P.ShipmentDate,'dd-MMM-yyyy')ShipmentDate,P.NegotiationType,FORMAT(P.PaymentReceivedDate,'dd-MMM-yyyy')PaymentReceivedDate,P.Remark,S.InvoiceStatus,P.TransporterCHAForwarderId,TF.UserName TransporterCHAForwarder,FORMAT(P.PaymentDueMatureDate,'dd-MMM-yyyy')PaymentDueMatureDate,P.RFIDSealNo,P.LineSealNo,P.PaymentAdviseNo,P.PaymentReceiveConfirmationBy,P.PaymentReceivedConfirmationInBankDate, FORMAT(P.DocDeliveryDate,'dd-MMM-yyyy')DocDeliveryDate
                          ,EI.EmployeeName PaymentReceiveConfirmationByName 
                      FROM [dbo].[PostSalesInvoice] P
					  LEFT JOIN TRN.Sales S ON S.Id=P.SalesId
					  LEFT JOIN HKP.Party C ON C.Id=P.CNFAgentId
					  LEFT JOIN HKP.Party T ON T.Id=P.TransportAgentId					  
					  LEFT JOIN HKP.Party TF ON TF.Id=P.TransporterCHAForwarderId					  
					  LEFT JOIN MST.[Port] PT ON PT.Id=P.PortOfLoadingId
					  LEFT JOIN [MST].[ShipMode] SP ON SP.Id=P.ShipmentModeId
					  LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=P.PaymentReceiveConfirmationBy";

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
                          ,FORMAT(P.NegotiatingDate,'dd-MMM-yyyy')NegotiatingDate
                          ,P.AddedBy
                          ,FORMAT(P.[AddedDate],'dd-MMM-yyyy')AddedDate
                          ,P.[AddedFromIP]
                          ,P.[UpdatedBy]
                          ,P.RFIDSealNo
                          ,P.LineSealNo
                          ,P.PaymentAdviseNo,P.PaymentReceiveConfirmationBy
                          ,EI.EmployeeName PaymentReceiveConfirmationByName
                          ,FORMAT(P.[UpdatedDate],'dd-MMM-yyyy')[UpdatedDate]
                          ,P.UpdatedFromIP
						  ,C.UserName CNFAgentName
						  ,T.UserName TransportAgentName
                          ,S.InvoiceNo
						  ,PT.UserName [Port]
						  ,SP.UserName ShipMode
                           ,P.ExportRefNo,P.TransporterCHAForwarderId,FORMAT(P.DocumentReceiveDate,'dd-MMM-yyyy')DocumentReceiveDate,P.AWBB2B,P.ActualPaymentReceived,P.ShippingBillNo,P.PortCode,FORMAT(P.DocumentSubmissionDate,'dd-MMM-yyyy')DocumentSubmissionDate
					      ,FORMAT(P.DocAcceptanceDate,'dd-MMM-yyyy')DocAcceptanceDate,P.FinalShipmentStatus,FORMAT(P.ShippingBillDate,'dd-MMM-yyyy')ShippingBillDate,FORMAT(P.ShipmentDate,'dd-MMM-yyyy')ShipmentDate,P.NegotiationType,FORMAT(P.PaymentReceivedDate,'dd-MMM-yyyy')PaymentReceivedDate,P.Remark,S.InvoiceStatus,P.TransporterCHAForwarderId,TF.UserName TransporterCHAForwarder,P.FileName,FORMAT(P.PaymentDueMatureDate,'dd-MMM-yyyy')PaymentDueMatureDate,FORMAT(P.PaymentReceivedConfirmationInBankDate,'dd-MMM-yyyy')PaymentReceivedConfirmationInBankDate , FORMAT(P.DocDeliveryDate,'dd-MMM-yyyy')DocDeliveryDate
                      FROM [dbo].[PostSalesInvoice] P
					  LEFT JOIN TRN.Sales S ON S.Id=P.SalesId
					  LEFT JOIN HKP.Party C ON C.Id=P.CNFAgentId
					  LEFT JOIN HKP.Party T ON T.Id=P.TransportAgentId	
                      LEFT JOIN HKP.Party TF ON TF.Id=P.TransporterCHAForwarderId
					  LEFT JOIN MST.[Port] PT ON PT.Id=P.PortOfLoadingId
					  LEFT JOIN [MST].[ShipMode] SP ON SP.Id=P.ShipmentModeId
					  LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=P.PaymentReceiveConfirmationBy
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

                string sql = "SELECT * FROM [dbo].[PostSalesInvoice] WHERE SalesId='" + data.SalesId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                if (!string.IsNullOrEmpty(data.ExportRefNo))
                {
                    objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[PostSalesInvoice] WHERE Id<>'" + data.Id + "' AND ExportRefNo='"+data.ExportRefNo+"'", out DataSet dsERN, false, "1");
                    if (dsERN.Tables[0].Rows.Count>0)
                    {
                        throw new Exception("Export Ref No is unique.");
                    }
                }

                if (!string.IsNullOrEmpty(data.ShippingBillNo))
                {
                    objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[PostSalesInvoice] WHERE Id<>'" + data.Id + "' AND ShippingBillNo='" + data.ShippingBillNo + "'", out DataSet dsERN, false, "1");
                    if (dsERN.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Shipping Bill No is unique.");
                    }
                }

                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = data.SalesId;
                    dr["SalesId"] = data.SalesId;
                    dr["InvoiceDate"] = data.InvoiceDate;
                    dr["BankMasterId"] = data.BankMasterId;
                    dr["BankDocRef"] = data.BankDocRef;

                    if (String.IsNullOrEmpty(data.NegotiatingDate.ToString()))
                    {
                        dr["NegotiatingDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["NegotiatingDate"] = data.NegotiatingDate;
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
                    dr["RFIDSealNo"] = data.RFIDSealNo;
                    dr["LineSealNo"] = data.LineSealNo;

                    dr["PaymentReceiveConfirmationBy"] = data.PaymentReceiveConfirmationBy;
                    dr["PaymentAdviseNo"] = data.PaymentAdviseNo;

                    if (String.IsNullOrEmpty(data.PaymentDueMatureDate.ToString()))
                    {
                        dr["PaymentDueMatureDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["PaymentDueMatureDate"] = data.PaymentDueMatureDate;
                    }


                    if (String.IsNullOrEmpty(data.PaymentReceivedConfirmationInBankDate.ToString()))
                    {
                        dr["PaymentReceivedConfirmationInBankDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["PaymentReceivedConfirmationInBankDate"] = data.PaymentReceivedConfirmationInBankDate;
                    }

                    if(String.IsNullOrEmpty(data.DocDeliveryDate.ToString()))
                    {
                        dr["DocDeliveryDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["DocDeliveryDate"] = data.DocDeliveryDate;
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

                    if (String.IsNullOrEmpty(data.NegotiatingDate.ToString()))
                    {
                        dr["NegotiatingDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["NegotiatingDate"] = data.NegotiatingDate;
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
                    dr["RFIDSealNo"] = data.RFIDSealNo;
                    dr["LineSealNo"] = data.LineSealNo;

                    dr["PaymentReceiveConfirmationBy"] = data.PaymentReceiveConfirmationBy;
                    dr["PaymentAdviseNo"] = data.PaymentAdviseNo;
                    if (String.IsNullOrEmpty(data.PaymentDueMatureDate.ToString()))
                    {
                        dr["PaymentDueMatureDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["PaymentDueMatureDate"] = data.PaymentDueMatureDate;
                    }

                    if (String.IsNullOrEmpty(data.PaymentReceivedConfirmationInBankDate.ToString()))
                    {
                        dr["PaymentReceivedConfirmationInBankDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["PaymentReceivedConfirmationInBankDate"] = data.PaymentReceivedConfirmationInBankDate;
                    }

                    if (String.IsNullOrEmpty(data.DocDeliveryDate.ToString()))
                    {
                        dr["DocDeliveryDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["DocDeliveryDate"] = data.DocDeliveryDate;
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

        #region upload Production Bulletin picture
        [HttpPost, Authorize]
        public ActionResult SavePostSaleFile(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the Post Sales Invoice first");

                foreach (var file in UploadDefault)
                {
                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetPostSalesInvoiceImagePath(), fileName);

                    var directory = ResourcesPathReader.GetPostSalesInvoiceImagePath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetPostSalesInvoiceImagePath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetPostSalesInvoiceImagePath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from dbo.PostSalesInvoice where id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);
                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id)
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("SELECT FileName FROM dbo.PostSalesInvoice WHERE Id ='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion upload product picture

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
        public DateTime? NegotiatingDate { get; set; }
        public DateTime? PaymentDueMatureDate { get; set; }
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
        public DateTime? DocDeliveryDate { get; set; }
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
        public string RFIDSealNo { get; set; }
        public string LineSealNo { get; set; }
        public string PaymentReceiveConfirmationBy { get; set; }
        public string PaymentAdviseNo { get; set; }
        public DateTime? PaymentReceivedConfirmationInBankDate { get; set; }

    }
}